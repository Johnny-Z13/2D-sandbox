# World Systems Guide

Complete technical guide to the pixel world simulation, collision detection, and physics systems.

---

## Overview

The game uses a **GPU-accelerated cellular automata** approach for simulating a destructible 2D pixel world. Key systems:

1. **PixelWorldManager** - World state & simulation orchestration
2. **PixelWorld.compute** - GPU physics simulation (sand, water, etc.)
3. **PixelCollisionSystem** - CPU collision detection via GPU readback
4. **PixelWorldRender.shader** - Visual rendering

---

## Architecture

### Data Flow
```
CPU (Unity C#)                           GPU (Compute Shader)
┌──────────────────┐                    ┌──────────────────┐
│ PixelWorldManager│◄──────Init────────►│ PixelWorld.compute│
│                  │                    │    (CSInit)      │
│ - World State    │                    │                  │
│ - Parameters     │                    │ Generate Terrain │
│ - User Input     │                    └──────────────────┘
└──────────────────┘                             │
         │                                       │
         │ Mouse/Dig Input                      │
         └───────────►┌──────────────────┐◄─────┘
                      │ RenderTexture    │
                      │ (RInt format)    │
                      │ 4096×1536 pixels │
                      └──────────────────┘
                             │         ▲
         Async GPU Readback  │         │ Each Frame
                      ┌──────▼─────────┴─────┐
                      │ PixelWorld.compute   │
                      │    (CSMain)          │
                      │                      │
                      │ Sand Physics         │
                      │ Water Physics        │
                      │ User Modifications   │
                      │ Stability Check      │
                      └───────────┬──────────┘
                                  │
                      ┌───────────▼──────────┐
                      │ PixelWorldRender     │
                      │   .shader            │
                      │                      │
                      │ Visual Effects       │
                      │ Material Colors      │
                      └──────────────────────┘
                                  │
                      ┌───────────▼──────────┐
                      │   Screen Display     │
                      └──────────────────────┘
```

---

## PixelWorldManager

### Responsibilities
- Initialize world simulation
- Pass parameters to GPU
- Handle user input (mouse painting, digging)
- Dispatch compute shader each frame
- Provide public API for world modification

### Key Methods

#### `InitializeWorld()`
Sets up RenderTextures and dispatches initial terrain generation.

```csharp
// Called once at startup
void InitializeWorld() {
    // Create double-buffered RenderTextures
    _worldA = CreateWorldTexture();
    _worldB = CreateWorldTexture();
    
    // Find compute kernels
    _kernelInit = pixelSimShader.FindKernel("CSInit");
    _kernelMain = pixelSimShader.FindKernel("CSMain");
    
    // Set parameters
    pixelSimShader.SetInt("_Width", width);
    pixelSimShader.SetInt("_Height", height);
    pixelSimShader.SetInt("_Seed", seed);
    // ... cave generation params ...
    
    // Generate initial terrain
    DispatchKernel(_kernelInit);
}
```

#### `ModifyWorld(Vector2 worldPos, float radius, int matID)`
Public API for modifying terrain (digging, painting, etc.).

```csharp
// Called by player controller, mouse input, bombs, etc.
public void ModifyWorld(Vector2 worldPos, float radius, int matID) {
    Vector2 pixelPos = WorldToPixel(worldPos);
    _externalInput = new Vector4(pixelPos.x, pixelPos.y, radius, matID);
}
```

#### `SimulateFrame()`
Runs physics simulation each frame (double-buffered).

```csharp
void SimulateFrame() {
    var source = _useAAsSource ? _worldA : _worldB;
    var dest = _useAAsSource ? _worldB : _worldA;
    
    // Pass textures to compute shader
    pixelSimShader.SetTexture(_kernelMain, "WorldIn", source);
    pixelSimShader.SetTexture(_kernelMain, "WorldOut", dest);
    pixelSimShader.SetInt("_StabilityThreshold", stabilityThreshold);
    
    // Dispatch simulation
    DispatchKernel(_kernelMain);
    
    // Swap buffers
    _useAAsSource = !_useAAsSource;
}
```

### Configuration

```csharp
[Header("World Dimensions")]
public int width = 4096;
public int height = 1536;

[Header("Cave Generation")]
public float caveThreshold = 0.35f;       // Hollowness
public float caveFrequencyX = 8.0f;       // Horizontal stretch
public float caveFrequencyY = 16.0f;      // Vertical stretch
public float caveLayerBlend = 0.5f;       // Complexity

[Header("Water Generation")]
public float waterPoolChance = 0.3f;      // Pool frequency
public float waterDepthThreshold = 0.25f; // Depth limit
public float waterNoiseThreshold = 0.55f; // Rarity

[Header("Material Variety")]
public float sandFrequency = 20.0f;       // Sand clustering
public float sandThresholdShallow = 0.6f; // Sand in dirt layer
public float sandThresholdDeep = 0.7f;    // Deep sand pockets

[Header("Physics Settings")]
public int stabilityThreshold = 2;        // Floating pixel cleanup
```

---

## Compute Shader (PixelWorld.compute)

### Material IDs
```hlsl
#define MAT_EMPTY 0  // Air
#define MAT_ROCK  1  // Stone
#define MAT_DIRT  2  // Soil
#define MAT_SAND  3  // Falling sand
#define MAT_WATER 4  // Liquid water
```

### CSInit - Terrain Generation

Uses **Fractal Brownian Motion (FBM)** and **Perlin Noise** for procedural generation.

```hlsl
[numthreads(8,8,1)]
void CSInit (uint3 id : SV_DispatchThreadID) {
    // 1. Multi-layer cave noise
    float caveNoise1 = fbm((uv + offset) * _CaveFrequency);
    float caveNoise2 = fbm((uv + offset + offset2) * _CaveFrequency * 0.7);
    float caveNoise = lerp(caveNoise1, (caveNoise1 + caveNoise2) * 0.5, _CaveLayerBlend);
    
    // 2. Surface terrain
    float surfaceHeight = 0.95 + 0.02 * noise(uv.x * 15.0);
    
    // 3. Material variation noise
    float mineralNoise = noise(uv * _SandParams.x);
    float waterNoise = noise(uv * 12.0);
    
    // 4. Determine material
    if (uv.y > surfaceHeight) {
        mat = MAT_EMPTY; // Sky
    }
    else if (caveNoise > dynamicCaveThreshold) {
        // Cave hollow - might have water or sand
        mat = MAT_EMPTY;
        if (canSpawnWater) mat = MAT_WATER;
        if (canSpawnSand) mat = MAT_SAND;
    }
    else {
        // Solid ground
        if (depth < 0.05) mat = MAT_DIRT;  // Surface layer
        else mat = MAT_ROCK;                // Deep rock
        
        // Add sand variations
        if (mineralNoise > threshold) mat = MAT_SAND;
    }
    
    // 5. Enforce boundaries
    if (id.x < 5u || id.x > _Width - 5u || id.y < 5u) {
        mat = MAT_ROCK; // Unbreakable walls
    }
    
    WorldOut[id.xy] = mat;
}
```

### CSMain - Physics Simulation

**Pull-based cellular automata** for efficient GPU parallel execution.

```hlsl
[numthreads(8,8,1)]
void CSMain (uint3 id : SV_DispatchThreadID) {
    int self = WorldIn[id.xy];
    int up = WorldIn[uint2(id.x, id.y + 1u)];
    int down = WorldIn[uint2(id.x, id.y - 1u)];
    int left = WorldIn[uint2(id.x - 1u, id.y)];
    int right = WorldIn[uint2(id.x + 1u, id.y)];
    
    // Each cell looks ABOVE to see what should fall into it
    
    if (self == MAT_SAND) {
        // Sand falls down or diagonally
        if (down == MAT_EMPTY || down == MAT_WATER) {
            result = (down == MAT_WATER) ? MAT_WATER : MAT_EMPTY;
        }
        else if (downLeft == MAT_EMPTY || downRight == MAT_EMPTY) {
            result = MAT_EMPTY; // Slide diagonally
        }
    }
    else if (self == MAT_WATER) {
        // Water falls down or spreads horizontally
        if (up == MAT_SAND) result = MAT_SAND; // Swap with falling sand
        else if (down == MAT_EMPTY) result = MAT_EMPTY;
        else if (left == MAT_EMPTY || right == MAT_EMPTY) result = MAT_EMPTY;
    }
    else if (self == MAT_ROCK || self == MAT_DIRT) {
        // Stability Check: Floating pixels crumble to sand
        if (!downSolid) {
            int supportCount = (upSolid?1:0) + (leftSolid?1:0) + (rightSolid?1:0);
            if (supportCount < _StabilityThreshold) {
                result = MAT_SAND;
            }
        }
    }
    else if (self == MAT_EMPTY) {
        // Pull falling materials from above
        if (up == MAT_SAND) result = MAT_SAND;
        else if (up == MAT_WATER) result = MAT_WATER;
        // ... check diagonals for sand ...
        // ... horizontal water flow ...
    }
    
    WorldOut[id.xy] = result;
}
```

**Why Pull-Based?**
- Avoids race conditions in parallel GPU execution
- Each thread reads neighbors, writes only to self
- Deterministic results regardless of thread execution order

---

## PixelCollisionSystem

### Purpose
Read GPU pixel data back to CPU for collision detection (player, projectiles, etc.).

### Challenges
- GPU→CPU transfer is expensive
- Need data every frame for real-time collision
- RenderTexture is on GPU, collision happens on CPU

### Solution: AsyncGPUReadback

```csharp
public class PixelCollisionSystem : MonoBehaviour {
    private NativeArray<int> _worldData;
    private bool _hasData = false;
    
    void Update() {
        // Request readback periodically
        if (timer >= interval) RequestReadback();
    }
    
    void OnReadbackComplete(AsyncGPUReadbackRequest request) {
        if (!request.hasError) {
            var data = request.GetData<int>();
            NativeArray<int>.Copy(data, _worldData, data.Length);
            _hasData = true;
        }
    }
    
    public bool IsSolid(Vector2 worldPos) {
        // ... checks single pixel ...
    }
    
    public bool IsSolidDown(Vector2 worldPos, int threshold) {
        // Checks a 3x2 area below the position
        // Returns true only if solid pixel count > threshold
        // Used for player falling to ignore small dust particles
    }
}
```

### Performance Notes
- **Latency:** 1-2 frames behind GPU state (acceptable for gameplay)
- **Bandwidth:** ~16MB/frame for 4096×1536×4 bytes (manageable)
- **Optimization:** Could sample every N frames if needed

### Coordinate Conversion

World space (Unity units) ↔ Pixel space (array indices):

```csharp
Vector2 WorldToPixel(Vector2 worldPos) {
    float halfWorldWidth = (_width * cellSize) / 2f;  // e.g., 40.96
    float halfWorldHeight = (_height * cellSize) / 2f; // e.g., 15.36
    
    float pixelX = (worldPos.x + halfWorldWidth) / cellSize;
    float pixelY = (worldPos.y + halfWorldHeight) / cellSize;
    
    return new Vector2(pixelX, pixelY);
}
```

---

## PlayerController2D

### Collision Detection

Uses raycasting approach with `PixelCollisionSystem`:

```csharp
void Move() {
    Vector2 pos = transform.position;
    Vector2 delta = _velocity * Time.deltaTime;
    
    // Horizontal collision
    Vector2 sideCheckPos = pos + new Vector2(dirX * playerWidth * 0.5f, 0);
    if (_collision.IsSolid(sideCheckPos + new Vector2(delta.x, 0))) {
        _velocity.x = 0;
        delta.x = 0;
    }
    pos.x += delta.x;
    
    // Vertical collision (ground check)
    _isGrounded = false;
    if (delta.y < 0) { // Falling
        float feetY = pos.y - playerHeight * 0.5f;
        
        // Cast multiple rays across player width
        for (int i = 0; i < verticalRays; i++) {
            float t = (float)i / (verticalRays - 1);
            float rayX = Lerp(pos.x - playerWidth * 0.4f, 
                              pos.x + playerWidth * 0.4f, t);
            
            // Use IsSolidDown with threshold to ignore dust
            if (_collision.IsSolidDown(new Vector2(rayX, feetY + delta.y), groundCollisionThreshold)) {
                _isGrounded = true;
                _velocity.y = 0;
                break;
            }
        }
        
        if (!_isGrounded) pos.y += delta.y;
    }
    
    transform.position = pos;
}
```

**Why Multiple Rays?**
- Single raycast can miss corners
- Multiple rays ensure robust ground detection
- Typical: 3-5 rays across player width

---

## Material Properties

### Material Behavior

| Material | Gravity | Flows Down | Flows Horizontal | Density |
|----------|---------|------------|------------------|---------|
| Empty    | -       | -          | -                | 0       |
| Rock     | No      | No         | No               | ∞       |
| Dirt     | No      | No         | No               | ∞       |
| Sand     | Yes     | Yes        | Diagonal only    | 2       |
| Water    | Yes     | Yes        | Yes              | 1       |

### Interaction Rules

```
Sand + Empty → Sand falls
Sand + Water → Sand sinks, water rises
Water + Empty → Water falls
Water + Rock/Dirt → Water flows horizontally
Water + Water → Water spreads to equalize
Rock/Dirt (Unsupported) → Crumbles to Sand (Stability Threshold)
```

---

## Rendering

### PixelWorldRender.shader

Maps material IDs to colors with procedural effects:

```hlsl
fixed4 frag(v2f i) : SV_Target {
    int mat = tex2D(_WorldTex, i.uv).r;
    
    if (mat == MAT_SAND) {
        // Multi-layer glitter effect
        float3 baseColor = _SandBaseColor;
        float glitter = CalculateGlitter(i.worldPos, _Time);
        float shimmer = CalculateShimmer(i.worldPos, _Time);
        
        return float4(baseColor + glitter + shimmer, 1.0);
    }
    else if (mat == MAT_WATER) {
        // Caustics and depth shading
        float3 waterColor = _WaterColor;
        float caustic = CalculateCaustics(i.worldPos, _Time);
        
        return float4(waterColor + caustic, _WaterAlpha);
    }
    // ... other materials ...
}
```

See `VISUAL_UPGRADE_README.md` for complete rendering details.

---

## Performance Optimization

### Current Performance
- **Target:** 60 FPS
- **World Size:** 4096×1536 = 6.3 million pixels
- **Simulation:** Every frame on GPU
- **Rendering:** Every frame
- **Collision:** 1-2 frame delay (async readback)

### Optimization Techniques

#### 1. Double Buffering
Avoids read/write conflicts on GPU:
```
Frame N: Read from A, Write to B
Frame N+1: Read from B, Write to A
```

#### 2. Compute Shader Threads
```hlsl
[numthreads(8,8,1)]  // 64 threads per group
```
- Total thread groups: (4096/8) × (1536/8) = 512 × 192 = 98,304 groups
- Total threads: 98,304 × 64 = ~6.3 million threads
- GPU executes in parallel warps/wavefronts

#### 3. Pull-Based Updates
- Each thread only writes to one pixel
- No race conditions
- No atomic operations needed
- Scales perfectly to any GPU

#### 4. Async GPU Readback
- Non-blocking CPU-GPU transfer
- Collision data arrives 1-2 frames later (acceptable)
- Avoids frame stalls

### Potential Optimizations

If performance becomes an issue:

1. **Reduce World Size** - 2048×1024 = 4× faster
2. **Skip Frames** - Simulate every 2nd frame
3. **Throttle Readback** - Collision every N frames
4. **LOD System** - Lower resolution far from player
5. **Culling** - Don't simulate off-screen areas

---

## Debugging

### WorldSystemValidator

Comprehensive diagnostics tool:

```csharp
[ContextMenu("🔍 Validate All Systems")]
public void ValidateAllSystems() {
    CheckReferences();        // Are all components assigned?
    CheckWorldDimensions();   // Are dimensions sensible?
    CheckRendererScale();     // Does renderer match world size?
    CheckCollisionSystem();   // Is collision working?
    CheckPlayerSpawn();       // Is player positioned correctly?
}
```

### WorldGenerationDebugger

Analyzes generated terrain:

```csharp
[ContextMenu("🔍 Check World Generation")]
public void CheckWorldGeneration() {
    // Reads back world data from GPU
    // Counts material types
    // Reports % empty, rock, dirt, sand, water
    // Warns if world is all empty or all solid
}
```

### WorldBoundsDebugger

Visualizes world boundaries in Scene view:

```csharp
void OnDrawGizmos() {
    // Draws world bounds box
    // Draws 5-pixel boundary walls
    // Shows dimensions as text
    // Optional: grid overlay
}
```

---

## Common Issues & Solutions

### Issue: Player Falls Through Ground

**Causes:**
1. Collision system hasn't received data yet
2. Surface height in CSInit too low
3. Player spawned below surface

**Solution:**
```csharp
// In PixelWorldManager CSInit:
float surfaceHeight = 0.95; // Should be 95% of world height

// In PlayerController2D:
if (!_collision.HasData) {
    _velocity = Vector2.zero;
    return; // Wait for collision data
}
```

### Issue: No Ground Rendering

**Causes:**
1. Compute shader didn't compile
2. Parameters not set before dispatch
3. RenderTexture creation failed

**Solution:**
```csharp
// Add validation in InitializeWorld():
if (_kernelInit < 0) {
    Debug.LogError("CSInit kernel not found!");
    return;
}

// Log parameters for debugging:
Debug.Log($"Cave threshold: {caveThreshold}");
```

### Issue: Physics Looks Wrong

**Causes:**
1. Double buffering swapped incorrectly
2. Pull-based logic has bugs
3. Boundary conditions wrong

**Solution:**
- Check that WorldIn/WorldOut swap each frame
- Verify neighbor reads use correct offsets
- Test with simple patterns (single sand grain)

---

## Advanced Topics

### Adding New Materials

1. Define new material ID in compute shader:
```hlsl
#define MAT_LAVA 5
```

2. Add physics logic in CSMain:
```hlsl
else if (self == MAT_LAVA) {
    // Lava spreads like water but destroys other materials
    if (down == MAT_EMPTY) result = MAT_EMPTY;
    else if (down == MAT_WATER) result = MAT_EMPTY; // Evaporate water
    // ... etc ...
}
```

3. Add rendering in shader:
```hlsl
else if (mat == MAT_LAVA) {
    return float4(1, 0.5, 0, 1); // Orange glow
}
```

4. Update collision system:
```csharp
public bool IsSolid(Vector2 worldPos) {
    // ...
    return mat == MAT_ROCK || mat == MAT_DIRT || mat == MAT_SAND || mat == MAT_LAVA;
}
```

### Chunk-Based Worlds

For truly massive worlds (e.g., 100,000× 100,000 pixels):
- Split world into chunks (e.g., 1024×1024 each)
- Only simulate chunks near player
- Stream chunks in/out as player moves
- Save inactive chunks to disk

### Network Multiplayer

Challenges:
- Synchronizing 6M pixels over network = impossible
- Need deterministic simulation

Solutions:
- Only sync player actions (input, dig commands)
- Each client runs identical simulation
- Use same seed for world generation
- Occasional hash checks for desyncs

---

## Further Reading

- `CAVE_GENERATION_GUIDE.md` - Procedural generation details
- `VISUAL_UPGRADE_README.md` - Rendering & effects
- `Assets/2D-sandbox-PRD` - Design goals & features
- `Assets/Noita_Destructible_Environment_Tech.md` - Inspiration

---

**This guide covers the core technical systems. For gameplay features, see DEVELOPMENT_GUIDE.md.**
