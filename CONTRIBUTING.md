# Contributing to 2D Pixel Sandbox

Thank you for your interest in contributing! This document provides guidelines and information for collaborators.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Project Structure](#project-structure)
3. [Development Workflow](#development-workflow)
4. [Code Style Guide](#code-style-guide)
5. [Testing Guidelines](#testing-guidelines)
6. [Documentation](#documentation)
7. [Pull Request Process](#pull-request-process)
8. [Communication](#communication)

---

## Getting Started

### Prerequisites

- Unity 2022.3 LTS or newer
- Git
- Code editor (VS Code, Rider, or Visual Studio)
- Basic understanding of:
  - C# programming
  - Unity game engine
  - Compute shaders (for advanced work)

### Initial Setup

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   cd 2D-Sandbox
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Add project from disk
   - Open with Unity 2022.3 LTS or newer

3. **Verify setup**
   - Open scene: `Assets/Scenes/Prototype-01.unity`
   - Press Play ▶️
   - Test controls (WASD, Space, E)

4. **Set up audio** (optional, 5 minutes)
   - Follow `AUDIO_SETUP_GUIDE.md`
   - Tools > Audio > Generate Placeholder WAVs
   - Create AudioManager GameObject

### Essential Reading

**Before contributing, please read:**

1. **README.md** - Project overview and features
2. **Docs/API_REFERENCE.md** - Complete API documentation
3. **Docs/DEVELOPMENT_GUIDE.md** - Development patterns
4. **Docs/PROJECT_STRUCTURE.md** - File organization
5. **Docs/WORLD_SYSTEMS.md** - Core architecture

---

## Project Structure

```
2D-Sandbox/
├── Assets/
│   ├── Scripts/
│   │   ├── PixelWorld/      ← Core systems (main development)
│   │   ├── Items/            ← Gameplay objects
│   │   ├── UI/               ← User interface
│   │   ├── Audio/            ← Audio management
│   │   └── Editor/           ← Unity Editor tools
│   ├── Shaders/
│   │   ├── Compute/          ← GPU physics simulation
│   │   └── Materials/        ← Rendering shaders
│   ├── Scenes/               ← Unity scenes
│   ├── Prefabs/              ← Reusable objects
│   └── Settings/             ← URP configuration
├── Docs/                     ← Technical documentation
├── ProjectSettings/          ← Unity project settings
└── Packages/                 ← Unity packages

DO NOT COMMIT: Library/, Temp/, Logs/, UserSettings/
```

### Key Directories

**Assets/Scripts/PixelWorld/** - Core game systems
- Add new world features here
- Follow existing patterns
- Keep systems modular

**Assets/Scripts/Items/** - Gameplay objects (bombs, power-ups, etc.)
- One file per item type
- Inherit from MonoBehaviour
- Use AudioEventTriggers for sounds

**Assets/Shaders/** - GPU code
- Modify carefully (expertise required)
- Always test on multiple hardware
- Document shader parameters

---

## Development Workflow

### Branch Strategy

```
main           - Stable, production-ready code
develop        - Integration branch for features
feature/NAME   - New features
fix/NAME       - Bug fixes
docs/NAME      - Documentation updates
```

### Creating a Feature

1. **Create feature branch**
   ```bash
   git checkout develop
   git pull
   git checkout -b feature/my-feature
   ```

2. **Develop and test**
   - Write code following style guide
   - Test thoroughly
   - Add documentation

3. **Commit changes**
   ```bash
   git add .
   git commit -m "feat(system): add feature description"
   ```

4. **Push and create PR**
   ```bash
   git push origin feature/my-feature
   ```
   Then create Pull Request on GitHub

### Commit Message Format

Use conventional commits:

```
type(scope): subject

[optional body]

[optional footer]
```

**Types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation only
- `style` - Formatting, missing semicolons, etc.
- `refactor` - Code restructuring
- `perf` - Performance improvements
- `test` - Adding tests
- `chore` - Maintenance tasks

**Examples:**
```
feat(player): add double jump ability
fix(collision): resolve wall-climbing bug
docs(api): add AudioManager documentation
refactor(world): extract cave generation
perf(shader): optimize sand physics loop
```

---

## Code Style Guide

### C# Style

#### Naming Conventions

```csharp
// Classes: PascalCase
public class PixelWorldManager { }

// Public properties/methods: PascalCase
public int Width { get; }
public void ModifyWorld() { }

// Private fields: _camelCase with underscore
private int _worldWidth;
private bool _isInitialized;

// Serialized fields: camelCase (no underscore)
[SerializeField] private float moveSpeed = 5f;

// Constants: PascalCase or UPPER_SNAKE_CASE
private const int MaxPlayers = 4;
private const float GRAVITY = 9.81f;

// Local variables: camelCase
int playerCount = 0;
float deltaTime = Time.deltaTime;
```

#### File Organization

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace PixelWorld
{
    /// <summary>
    /// Brief description of class purpose.
    /// Detailed explanation if needed.
    /// </summary>
    public class ExampleClass : MonoBehaviour
    {
        // 1. Serialized Fields (Inspector)
        [Header("Settings")]
        [SerializeField] private float speed = 5f;
        
        // 2. Public Properties
        public bool IsActive { get; private set; }
        
        // 3. Private Fields
        private int _currentState;
        private Transform _cachedTransform;
        
        // 4. Unity Messages (in order)
        private void Awake() { }
        private void Start() { }
        private void Update() { }
        private void FixedUpdate() { }
        private void OnDestroy() { }
        
        // 5. Public Methods
        public void DoSomething() { }
        
        // 6. Private Methods
        private void InternalLogic() { }
        
        // 7. Context Menu / Editor Methods
        [ContextMenu("Test")]
        private void TestMethod() { }
    }
}
```

#### Best Practices

```csharp
// ✅ GOOD: Cache component references
private Rigidbody2D _rb;
void Start() {
    _rb = GetComponent<Rigidbody2D>();
}

// ❌ BAD: GetComponent every frame
void Update() {
    GetComponent<Rigidbody2D>().velocity = ...;
}

// ✅ GOOD: Use SerializeField for inspector exposure
[SerializeField] private float speed = 5f;

// ❌ BAD: Public fields
public float speed = 5f;

// ✅ GOOD: Null checks for singletons
if (AudioManager.Instance != null) {
    AudioManager.Instance.PlaySound();
}

// ❌ BAD: Assume instance exists
AudioManager.Instance.PlaySound(); // Might throw!

// ✅ GOOD: Use headers for organization
[Header("Movement")]
[SerializeField] private float moveSpeed;

[Header("Combat")]
[SerializeField] private float attackDamage;

// ✅ GOOD: Add tooltips for complex parameters
[Tooltip("Higher values = more hollow caves")]
[Range(0.2f, 0.6f)]
[SerializeField] private float caveThreshold;
```

### HLSL Shader Style

```hlsl
// Variables: _PascalCase (Unity convention)
float4 _MainColor;
float _GlitterIntensity;

// Macros: UPPER_SNAKE_CASE
#define MAT_EMPTY 0
#define MAX_ITERATIONS 100

// Functions: PascalCase or camelCase (consistent)
float CalculateNoise(float2 uv) { }
float hash(float2 p) { }

// Thread groups: Standard Unity format
[numthreads(8,8,1)]
void CSMain(uint3 id : SV_DispatchThreadID) { }
```

---

## Testing Guidelines

### Manual Testing Checklist

Before submitting PR, test:

- [ ] **Build:** Project builds without errors/warnings
- [ ] **Play Mode:** No console errors during gameplay
- [ ] **Controls:** All inputs work (keyboard + gamepad)
- [ ] **Performance:** Maintains 60 FPS on mid-range hardware
- [ ] **Collision:** Player movement works correctly
- [ ] **Audio:** Sound effects trigger properly
- [ ] **Edge Cases:** Test boundary conditions

### Testing New Features

1. **Test in isolation** - Does feature work alone?
2. **Test integration** - Does it break existing features?
3. **Test edge cases** - World boundaries, null inputs, etc.
4. **Test performance** - No frame drops or stutters?
5. **Test multiple sessions** - Works after restart?

### Debug Tools

Use built-in tools for testing:

```csharp
// PixelWorldManager context menu
[ContextMenu("Regenerate World")]

// WorldSystemValidator
[ContextMenu("🔍 Validate All Systems")]

// AudioManager
[ContextMenu("Test All Sounds")]
```

### Performance Testing

```csharp
// Use Unity Profiler (Window > Analysis > Profiler)
// Check:
// - CPU time per frame
// - GPU time per frame
// - Memory allocations (should be minimal)
// - GC allocations (should be zero in gameplay)
```

---

## Documentation

### Code Documentation

#### XML Documentation for Public APIs

```csharp
/// <summary>
/// Modifies the pixel world at the specified position.
/// </summary>
/// <param name="worldPos">Position in world space (Unity units)</param>
/// <param name="radius">Radius of effect in pixels</param>
/// <param name="matID">Material ID (0=Empty, 1=Rock, 2=Dirt, 3=Sand, 4=Water)</param>
/// <example>
/// <code>
/// PixelWorldManager.Instance.ModifyWorld(transform.position, 10f, 0);
/// </code>
/// </example>
public void ModifyWorld(Vector2 worldPos, float radius, int matID)
```

#### Inline Comments for Complex Logic

```csharp
// Pull-based cellular automata: Each cell looks ABOVE for falling materials
// This prevents race conditions in parallel GPU execution
if (up == MAT_SAND) result = MAT_SAND;
```

### Documentation Files

When adding features, update:

1. **Docs/API_REFERENCE.md** - If adding public API
2. **Docs/DEVELOPMENT_GUIDE.md** - If adding patterns
3. **README.md** - If adding major features
4. **CHANGELOG.md** - Always update with changes

### Documentation Style

```markdown
# Section Title

Brief overview paragraph.

## Subsection

### Code Example

\```csharp
// Clear, runnable example
AudioEventTriggers.OnPlayerJump();
\```

### Notes

- ✅ Use checkmarks for completed items
- ❌ Use X for incorrect patterns
- 💡 Use lightbulb for tips
- ⚠️ Use warning for important notes
```

---

## Pull Request Process

### Before Submitting PR

1. **Test thoroughly** - Follow testing checklist
2. **Update documentation** - API, guides, changelog
3. **Check code style** - Follow style guide
4. **No linter errors** - Zero errors/warnings
5. **Commit messages** - Follow format

### PR Template

```markdown
## Description
Brief description of changes.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Code refactoring

## Testing Done
- [ ] Manual testing in Play mode
- [ ] Tested edge cases
- [ ] Performance profiling
- [ ] No console errors

## Checklist
- [ ] Code follows style guide
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] Zero linter errors
- [ ] Builds successfully

## Screenshots (if applicable)
[Add screenshots/GIFs of new features]

## Related Issues
Fixes #123
Related to #456
```

### Review Process

1. **Submit PR** - Against `develop` branch
2. **Automated checks** - Wait for CI/CD (if configured)
3. **Code review** - Address reviewer comments
4. **Approval** - At least 1 approval required
5. **Merge** - Squash and merge to develop

---

## Communication

### Asking Questions

**Good Questions:**
- Include context and what you've tried
- Reference specific files/line numbers
- Provide error messages/screenshots
- Show relevant code snippets

**Example:**
```
I'm trying to add a new material type (lava) in PixelWorld.compute.
I've added MAT_LAVA = 5 and physics logic in CSMain, but it's not
rendering correctly. The shader shows black pixels instead of red.

Relevant code:
[code snippet]

Error message:
[error]
```

### Reporting Bugs

Use GitHub Issues with this template:

```markdown
**Bug Description:**
Clear description of the bug.

**Steps to Reproduce:**
1. Open scene X
2. Press button Y
3. Observe Z

**Expected Behavior:**
What should happen.

**Actual Behavior:**
What actually happens.

**Environment:**
- Unity Version: 2022.3.15f1
- OS: Windows 10
- Hardware: RTX 3060

**Console Output:**
[Paste error messages]

**Screenshots:**
[If applicable]
```

---

## Coding Patterns

### Singleton Pattern

```csharp
public class MyManager : MonoBehaviour
{
    public static MyManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // If needed
    }
}
```

### Audio Triggers

```csharp
// Always use AudioEventTriggers (null-safe)
AudioEventTriggers.OnPlayerJump();
```

### Collision Checks

```csharp
// Always check HasData
if (PixelCollisionSystem.Instance != null && 
    PixelCollisionSystem.Instance.HasData)
{
    bool solid = PixelCollisionSystem.Instance.IsSolid(pos);
}
```

### Context Menus

```csharp
[ContextMenu("Debug Info")]
private void LogDebugInfo()
{
    Debug.Log($"Status: {_isActive}");
}
```

---

## Common Tasks

### Adding a New Material

See `Docs/DEVELOPMENT_GUIDE.md` - "Adding New Features" section.

Summary:
1. Define material ID in compute shader
2. Add physics logic in CSMain
3. Add rendering in pixel shader
4. Update collision system
5. Test thoroughly

### Adding a New Sound Effect

1. Add AudioClip field to AudioManager
2. Create playback method
3. Add trigger to AudioEventTriggers
4. Call from gameplay code
5. Add placeholder WAV file

### Adding a New Debug Tool

1. Create script in `Assets/Scripts/PixelWorld/`
2. Add `[ContextMenu]` methods
3. Use Gizmos for visual debugging
4. Document in API reference

---

## Resources

### Unity Documentation
- [Compute Shaders](https://docs.unity3d.com/Manual/class-ComputeShader.html)
- [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)

### Project Documentation
- `Docs/API_REFERENCE.md` - Complete API
- `Docs/DEVELOPMENT_GUIDE.md` - Development patterns
- `Docs/WORLD_SYSTEMS.md` - Core architecture
- `Docs/AUDIO_SYSTEM.md` - Audio implementation

### Learning Resources
- Noita GDC Talk - Falling sand simulation
- Cellular Automata tutorials
- GPU programming basics

---

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

## Questions?

- **Technical questions:** Open a GitHub Issue
- **Feature ideas:** Open a GitHub Discussion
- **Bug reports:** Open a GitHub Issue with bug template
- **Security issues:** Contact maintainer directly

---

## Thank You!

Your contributions help make this project better for everyone. We appreciate your time and effort! 🎉

---

**Last Updated:** November 29, 2025  
**Maintainer:** johnny@z13labs.com

