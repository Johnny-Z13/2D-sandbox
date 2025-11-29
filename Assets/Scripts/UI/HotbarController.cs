using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace PixelWorld
{
    public class HotbarController : MonoBehaviour
    {
        public static HotbarController Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color normalColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private int _selectedIndex = 0;
        private Image[] _slots;
        private Color[] _baseColors;

        public int SelectedIndex => _selectedIndex;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Initialize slots
            if (slotContainer != null)
            {
                // Get all child images that act as slot backgrounds
                List<Image> slotsList = new List<Image>();
                foreach (Transform child in slotContainer)
                {
                    Image img = child.GetComponent<Image>();
                    if (img != null)
                    {
                        slotsList.Add(img);
                    }
                }
                _slots = slotsList.ToArray();
                
                // Store base colors
                _baseColors = new Color[_slots.Length];
                for (int i = 0; i < _slots.Length; i++)
                {
                    _baseColors[i] = _slots[i].color;
                }

                UpdateUI();
            }
        }

        private void Update()
        {
            // Keyboard input for hotbar selection using New Input System directly
            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
                if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
                if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
                if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);
            }

        }

        public void SelectSlot(int index)
        {
            if (index >= 0 && index < 4)
            {
                _selectedIndex = index;
                UpdateUI();
                AudioEventTriggers.OnHotbarSwitch();
            }
        }

        public void CycleSlot(int direction)
        {
            int newIndex = _selectedIndex + direction;
            if (newIndex < 0) newIndex = 3;
            if (newIndex > 3) newIndex = 0;
            SelectSlot(newIndex);
        }

        private void UpdateUI()
        {
            if (_slots == null) return;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null)
                {
                    // Use base color, but dim it if not selected
                    Color baseCol = _baseColors[i];
                    if (i == _selectedIndex)
                    {
                        // Selected: Full brightness (or slightly boosted/tinted if you want)
                        _slots[i].color = baseCol;
                        _slots[i].transform.localScale = Vector3.one * 1.2f; // Slightly bigger pop
                    }
                    else
                    {
                        // Not selected: Dimmed (50% opacity or darker)
                        _slots[i].color = new Color(baseCol.r * 0.6f, baseCol.g * 0.6f, baseCol.b * 0.6f, baseCol.a);
                        _slots[i].transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }
}
