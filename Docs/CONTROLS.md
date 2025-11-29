# Controls & Input Reference

Complete control scheme for keyboard, mouse, and gamepad.

---

## 🎮 Default Controls

### Keyboard & Mouse

| Action | Key/Button | Notes |
|--------|------------|-------|
| **Movement** |
| Move Left | A or ← | |
| Move Right | D or → | |
| Move Up | W or ↑ | *Ladder/water only* |
| Move Down | S or ↓ | *Ladder/water only* |
| **Actions** |
| Jump | Space | |
| Crouch | C | Can't move while crouching |
| Dig/Interact | E | Hold to dig continuously |
| Attack | Enter | |
| **Hotbar / Tools** |
| Cycle Next | 2 | Switch to next tool |
| Cycle Previous | 1 | Switch to previous tool |
| **World Interaction** |
| Paint Sand | Left Mouse | Hold to paint |
| Paint Water | Right Mouse | Hold to paint |
| Erase | Middle Mouse | Hold to erase |
| **Camera** |
| Zoom In | Mouse Wheel Up | |
| Zoom Out | Mouse Wheel Down | |
| **UI/Debug** |
| Toggle Debug Info | F12 | *If VisualDebugger enabled* |
| **Graphics Presets** *(If RenderingPresetController enabled)* |
| Preset 1 (Default) | F1 | Shows notification for 3 seconds |
| Preset 2 (Desert Gold) | F2 | Shows notification for 3 seconds |
| Preset 3 (Subtle) | F3 | Shows notification for 3 seconds |
| Preset 4 (Extreme) | F4 | Shows notification for 3 seconds |
| Preset 5 (Screenshot) | F5 | Shows notification for 3 seconds |
| Preset 6 (Performance) | F6 | Shows notification for 3 seconds |

### Gamepad (Xbox/PlayStation)

| Action | Button | Notes |
|--------|--------|-------|
| **Movement** |
| Move | Left Stick | Analog movement |
| Look | Right Stick | *Not currently used* |
| **Actions** |
| Jump | A / Cross | |
| Crouch | B / Circle | |
| Dig/Interact | X / Square | |
| Attack | Y / Triangle | |
| **Weapon/Items** |
| Previous Tool | D-Pad Left | |
| Next Tool | D-Pad Right | |
| **Camera** |
| Zoom | Left Stick Click | *Not currently implemented* |

---

## 🛠️ Customizing Controls

### Method 1: Unity Input System (Recommended)

1. Open `Assets/InputSystem_Actions.inputactions` in Unity
2. Double-click to open Input Actions editor
3. Select action (e.g., "Jump")
4. Modify bindings or add new ones
5. Save and reimport

### Method 2: Code (PlayerController2D.cs)

```csharp
// Input callbacks are automatically called by Input System
public void OnMove(InputValue value) {
    _moveInput = value.Get<Vector2>();
}

public void OnJump(InputValue value) {
    if (value.isPressed && _isGrounded) {
        _velocity.y = jumpForce;
    }
}
```

---

## 🎯 Input Actions Breakdown

### Player Action Map

Defined in `Assets/InputSystem_Actions.inputactions`:

```
Action Map: "Player"
├── Move (Vector2)
│   ├── Keyboard: WASD, Arrow Keys (2D Vector Composite)
│   ├── Gamepad: Left Stick
│   └── Joystick: Stick
├── Jump (Button)
│   ├── Keyboard: Space
│   └── Gamepad: A/Cross (buttonSouth)
├── Crouch (Button)
│   ├── Keyboard: C
│   └── Gamepad: B/Circle (buttonEast)
├── Interact (Button)
│   ├── Keyboard: E
│   └── Gamepad: X/Square (buttonWest)
├── Attack (Button)
│   ├── Keyboard: Enter
│   ├── Mouse: *REMOVED* (was causing conflict)
│   └── Gamepad: Y/Triangle (buttonNorth)
├── Previous (Button)
│   ├── Keyboard: 1
│   └── Gamepad: D-Pad Left
├── Next (Button)
│   ├── Keyboard: 2
│   └── Gamepad: D-Pad Right
└── Look (Vector2)
    ├── Mouse: Delta (pointer delta)
    └── Gamepad: Right Stick
```

### Mouse Painting (Direct Input)

Handled directly in `PixelWorldManager.cs`:

```csharp
if (Mouse.current.leftButton.isPressed) {
    // Paint Sand (material ID 3)
    ModifyWorld(mouseWorldPos, 10, 3);
}
if (Mouse.current.rightButton.isPressed) {
    // Paint Water (material ID 4)
    ModifyWorld(mouseWorldPos, 10, 4);
}
if (Mouse.current.middleButton.isPressed) {
    // Erase (material ID 0 = empty)
    ModifyWorld(mouseWorldPos, 10, 0);
}
```

---

## 🎮 Control Schemes

Unity Input System automatically switches between control schemes based on last input:

### Keyboard & Mouse Scheme
- Requires both keyboard and mouse connected
- Mouse buttons: Paint materials
- Keyboard: Movement and actions
- Mouse wheel: Zoom camera

### Gamepad Scheme
- Requires gamepad connected
- All actions mapped to buttons
- Analog stick for smooth movement
- D-pad for weapon/item switching

---

## 🔧 Advanced: Input Polling vs Message System

### Message System (Current)
Player controller uses **SendMessages** notification behavior:

```csharp
// Unity automatically calls these methods when input occurs
public void OnMove(InputValue value) { ... }
public void OnJump(InputValue value) { ... }
public void OnCrouch(InputValue value) { ... }
```

**Pros:**
- Clean, event-driven
- No need to poll every frame
- Unity handles all binding logic

**Cons:**
- Slightly less responsive than polling
- Requires exact method names

### Polling (Alternative)
If you need maximum responsiveness:

```csharp
private PlayerInput _playerInput;

void Update() {
    if (_playerInput.actions["Jump"].WasPressedThisFrame()) {
        // Handle jump
    }
    
    Vector2 move = _playerInput.actions["Move"].ReadValue<Vector2>();
    // Use move input
}
```

---

## 📋 Control Testing Checklist

When adding new inputs:

### Keyboard & Mouse
- [ ] Works with WASD
- [ ] Works with arrow keys
- [ ] Mouse buttons don't conflict
- [ ] Mouse wheel zooms camera
- [ ] Key bindings don't conflict with Unity editor shortcuts

### Gamepad
- [ ] Works with Xbox controller
- [ ] Works with PlayStation controller
- [ ] Analog stick has proper dead zone
- [ ] Face buttons respond correctly
- [ ] D-pad navigation works

### Both
- [ ] Input System switches schemes automatically
- [ ] No input lag or delayed response
- [ ] Rebinding works (if implemented)
- [ ] Tutorial text updates based on active scheme

---

## 🐛 Common Input Issues

### Issue: WASD Not Working

**Cause:** Malformed binding groups in Input Actions.

**Solution:**
```json
// In InputSystem_Actions.inputactions
// Ensure binding groups don't have leading semicolons:
"groups": "Keyboard&Mouse"  // ✅ Correct
"groups": ";Keyboard&Mouse" // ❌ Wrong (causes input to be ignored)
```

### Issue: Mouse Clicks Paint AND Attack

**Cause:** Left mouse button bound to both "Attack" action and `PixelWorldManager` painting.

**Solution:**
- Remove mouse binding from "Attack" action
- Let `PixelWorldManager` handle mouse painting directly
- Use Enter key or gamepad button for attacking

### Issue: Digging Won't Stop

**Cause:** "Interact" action has "Hold" interaction requirement.

**Solution:**
```json
// Remove "Hold" interaction from Interact action
"interactions": ""  // ✅ Responds immediately
"interactions": "Hold"  // ❌ Requires holding
```

### Issue: Gamepad Not Detected

**Causes:**
1. Gamepad not connected before starting game
2. Driver issues
3. Unity Input System package not installed

**Solutions:**
1. Connect gamepad before starting Unity
2. Check Windows/Mac gamepad settings
3. Verify Input System package in Package Manager

---

## 📚 Related Documentation

- `InputSystem_Actions.inputactions` - Unity Input Actions asset
- [Unity Input System Package Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)

---

**Having issues with controls?** Check the troubleshooting section above or review the Input Actions asset in Unity.
