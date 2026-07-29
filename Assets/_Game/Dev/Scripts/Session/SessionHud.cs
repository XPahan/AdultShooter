using SexShot.Dev.Combat;
using SexShot.Dev.Player;
using SexShot.Dev.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SexShot.Dev.Session
{
    public class SessionHud : MonoBehaviour
    {
        [SerializeField] private GameSession _session;
        [SerializeField] private Texture2D _healthIcon;
        [SerializeField] private Texture2D _ammoIcon;
        [SerializeField] private bool _showCrosshair = true;
        [SerializeField] private float _crosshairSize = 6f;
        [SerializeField] private float _crosshairGap = 4f;
        [SerializeField] private float _crosshairThickness = 2f;
        [SerializeField] private float _sideButtonWidth = 110f;
        [SerializeField] private float _sideButtonHeight = 36f;
        [SerializeField] private float _sideButtonMargin = 16f;
        [SerializeField] private float _sideButtonSpacing = 8f;
        [SerializeField] private float _statusMargin = 24f;
        [SerializeField] private float _statusIconSize = 56f;
        [SerializeField] private float _statusRowSpacing = 10f;
        [SerializeField] private float _statusValueWidth = 90f;

        private bool _menuOpen;
        private bool _pausedByMenu;
        private GUIStyle _statusValueStyle;
        private GUIStyle _weaponHintStyle;

        private void Update()
        {
            if (_session == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            ToggleMenu();
        }

        private void OnGUI()
        {
            if (_session == null)
            {
                return;
            }

            EnsureStyles();

            if (_menuOpen)
            {
                DrawSideButtons();
            }

            if (_session.Player == null)
            {
                return;
            }

            if (_showCrosshair && _session.IsSessionActive && !_menuOpen)
            {
                DrawCrosshair();
            }

            var player = _session.Player;
            var health = player.Health;
            var weapons = player.Weapons;
            var ammo = weapons != null ? weapons.AmmoInventory : null;
            var weapon = weapons != null ? weapons.ActiveWeapon : null;

            DrawWeaponHint(weapon);
            DrawStatusPanel(health, ammo, weapon);

            if (!_session.IsSessionActive)
            {
                var w = 320f;
                var h = 60f;
                GUI.Box(new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h), "YOU DIED — Session Interrupted");
            }
        }

        private void EnsureStyles()
        {
            if (_statusValueStyle == null)
            {
                _statusValueStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 28,
                    fontStyle = FontStyle.Bold
                };
                _statusValueStyle.normal.textColor = Color.white;
            }

            if (_weaponHintStyle == null)
            {
                _weaponHintStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = 14
                };
                _weaponHintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
            }
        }

        private void DrawWeaponHint(WeaponDefinition weapon)
        {
            var weaponName = weapon != null ? weapon.DisplayName : "-";
            GUI.Label(
                new Rect(16f, 16f, 280f, 40f),
                $"{weaponName}\n[1] Pistol  [2] Shotgun  [3] Rifle",
                _weaponHintStyle);
        }

        private void DrawStatusPanel(
            Health health,
            AmmoInventory ammo,
            WeaponDefinition weapon)
        {
            var healthValue = health != null
                ? Mathf.CeilToInt(health.CurrentHealth).ToString()
                : "-";
            var ammoValue = ammo != null && weapon != null
                ? ammo.GetAmmo(weapon.WeaponId).ToString()
                : "-";

            var rowHeight = _statusIconSize;
            var panelWidth = _statusIconSize + 12f + _statusValueWidth;
            var panelHeight = rowHeight * 2f + _statusRowSpacing;
            var panelX = Screen.width - panelWidth - _statusMargin;
            var panelY = Screen.height - panelHeight - _statusMargin;

            DrawStatusRow(panelX, panelY, _healthIcon, healthValue);
            DrawStatusRow(panelX, panelY + rowHeight + _statusRowSpacing, _ammoIcon, ammoValue);
        }

        private void DrawStatusRow(float x, float y, Texture2D icon, string value)
        {
            if (icon != null)
            {
                GUI.DrawTexture(new Rect(x, y, _statusIconSize, _statusIconSize), icon, ScaleMode.ScaleToFit);
            }

            GUI.Label(
                new Rect(x + _statusIconSize + 12f, y, _statusValueWidth, _statusIconSize),
                value,
                _statusValueStyle);
        }

        private void ToggleMenu()
        {
            _menuOpen = !_menuOpen;
            if (_menuOpen)
            {
                OpenMenu();
            }
            else
            {
                CloseMenu();
            }
        }

        private void OpenMenu()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (CanResumeGameplay())
            {
                _session.Player.SetGameplayInputEnabled(false);
                if (Time.timeScale > 0f)
                {
                    Time.timeScale = 0f;
                    _pausedByMenu = true;
                }
            }
        }

        private void CloseMenu()
        {
            if (_pausedByMenu)
            {
                Time.timeScale = 1f;
                _pausedByMenu = false;
            }

            if (CanResumeGameplay())
            {
                _session.Player.SetGameplayInputEnabled(true);
            }
        }

        private bool CanResumeGameplay()
        {
            return _session.IsSessionActive
                && _session.Player != null
                && _session.Player.IsAlive;
        }

        private void DrawCrosshair()
        {
            var centerX = Screen.width * 0.5f;
            var centerY = Screen.height * 0.5f;
            var previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.9f);

            DrawCrosshairLine(centerX - _crosshairThickness * 0.5f, centerY - _crosshairGap - _crosshairSize, _crosshairThickness, _crosshairSize);
            DrawCrosshairLine(centerX - _crosshairThickness * 0.5f, centerY + _crosshairGap, _crosshairThickness, _crosshairSize);
            DrawCrosshairLine(centerX - _crosshairGap - _crosshairSize, centerY - _crosshairThickness * 0.5f, _crosshairSize, _crosshairThickness);
            DrawCrosshairLine(centerX + _crosshairGap, centerY - _crosshairThickness * 0.5f, _crosshairSize, _crosshairThickness);

            GUI.color = previousColor;
        }

        private static void DrawCrosshairLine(float x, float y, float width, float height)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
        }

        private void DrawSideButtons()
        {
            var totalHeight = _sideButtonHeight * 2f + _sideButtonSpacing;
            var x = Screen.width - _sideButtonWidth - _sideButtonMargin;
            var y = (Screen.height - totalHeight) * 0.5f;

            if (GUI.Button(new Rect(x, y, _sideButtonWidth, _sideButtonHeight), "Перезапуск"))
            {
                _session.RestartSession();
            }

            if (GUI.Button(new Rect(x, y + _sideButtonHeight + _sideButtonSpacing, _sideButtonWidth, _sideButtonHeight), "Выход"))
            {
                _session.QuitGame();
            }
        }
    }
}
