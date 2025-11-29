# Water Physics Improvements

## Overview
Enhanced the water simulation in `PixelWorld.compute` to create realistic fluid behavior that prevents blocky, unrealistic water formations and ensures proper leveling and flow.

## Key Improvements

### 1. **Gravity-Based Downward Flow**
Water now prioritizes falling straight down when there's empty space below.
```hlsl
else if (down == MAT_EMPTY) 
{
    result = MAT_EMPTY;
}
```

### 2. **Diagonal Flow Around Obstacles**
Water flows diagonally downward around obstacles when direct downward movement is blocked:
```hlsl
if (downLeft == MAT_EMPTY && left == MAT_EMPTY) result = MAT_EMPTY;
else if (downRight == MAT_EMPTY && right == MAT_EMPTY) result = MAT_EMPTY;
```
This prevents water from getting "stuck" against solid columns and creates natural waterfalls.

### 3. **Lateral Equalization (Smooth Water Surfaces)**
Water now calculates column depths and equalizes between neighboring columns:
- Measures water depth at current position and neighbors
- Flows sideways when one column is significantly deeper than adjacent columns
- Creates smooth, level water surfaces instead of stair-stepped terrain

**Pressure-Based Spreading**: Deeper water spreads more aggressively
```hlsl
int depthDiffThreshold = max(1, currentDepth / 4);
if (currentDepth > leftDepth + depthDiffThreshold) {
    // Flow to the lower column
}
```

### 4. **Depth-Dependent Flow Distance**
Water can flow farther horizontally when the source column is deeper:
```hlsl
int maxFlowDistance = min(WATER_SCAN_RADIUS, 2 + waterDepth / 3);
```
This ensures:
- Shallow puddles stay local
- Deep water bodies spread to fill connected spaces
- Tall vertical water columns naturally drain into lower areas

### 5. **Surface Tension (Anti-Spike Mechanism)**
Prevents isolated single-pixel water spikes from forming:
```hlsl
// Check if this would create an isolated spike
bool hasWaterNeighbor = (left == MAT_WATER || right == MAT_WATER);
if (!hasWaterNeighbor && below == MAT_EMPTY) {
    result = MAT_EMPTY; // Don't create isolated falling pixels
}
```

### 6. **Increased Scan Radius**
Expanded water scanning range from 5 to 8 pixels, allowing water to detect and flow toward distant empty spaces more effectively.

## Expected Behaviors

### ✅ Connected Water Bodies Level Out
All parts of a connected water body should settle to approximately the same height, creating a single horizontal surface line with natural waves.

### ✅ No Vertical Water Pillars
Tall columns of water drain downward into the main pool until they match the surrounding water level.

### ✅ No Blocky Water "Glued" to Obstacles
Water flows down the sides of solid columns like a waterfall, then spreads into the pool below, rather than forming rectangular blocks stuck to obstacles.

### ✅ Rising Water Level When Adding Water
When the player adds water while underwater, the overall water level rises proportionally as the simulation equalizes the new volume across the connected body.

## Technical Details

### Constants
- `WATER_SCAN_RADIUS`: 8 (horizontal flow detection range)
- `WATER_MAX_COLUMN_CHECK`: 20 (maximum depth calculation)

### Algorithm Flow (Water Cells)
1. **Check if sand is falling into water** → Displace water
2. **Try moving down** → Gravity
3. **Try diagonal down-left/right** → Flow around obstacles
4. **Calculate column depths** → Equalization pressure
5. **Flow to lower neighboring columns** → Leveling
6. **Use parity for variety** → Natural flow patterns

### Algorithm Flow (Empty Cells)
1. **Check for falling materials above**
2. **Scan horizontally for settled water**
3. **Calculate source water depth**
4. **Flow based on depth-dependent distance**
5. **Apply surface tension filter** → Prevent spikes

## Testing Recommendations

### Test Case 1: Vertical Water Column
1. Dig a tall, narrow shaft
2. Fill with water
3. **Expected**: Water drains to the bottom and spreads laterally

### Test Case 2: Multiple Connected Chambers
1. Create interconnected caves at different elevations
2. Add water
3. **Expected**: Water finds lowest point and levels across all connected spaces

### Test Case 3: Water Around Obstacles
1. Place water next to a tall, thin solid column
2. **Expected**: Water flows down the sides, not forming rectangular blocks stuck to the column

### Test Case 4: Adding Water Underwater
1. Create a water pool
2. Submerge the player
3. Add more water (press E in water mode)
4. **Expected**: Water level rises across the entire connected body

## Performance Considerations
- Depth calculation is limited to 20 cells to balance accuracy with performance
- Flow distance scales with depth but is capped at WATER_SCAN_RADIUS
- Parity-based decisions create variety without extra computation

## Related Files
- `Assets/Shaders/Compute/PixelWorld.compute` - Main simulation logic
- `Assets/Scripts/PixelWorld/PixelWorldManager.cs` - World management and input handling
