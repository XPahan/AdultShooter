using SexShot.Dev.Player;
using UnityEngine;

namespace SexShot.Dev.Session
{
    public class SessionHud : MonoBehaviour
    {
        [SerializeField] private GameSession _session;
        [SerializeField] private bool _showCrosshair = true;
        [SerializeField] private float _crosshairSize = 6f;
        [SerializeField] private float _crosshairGap = 4f;
        [SerializeField] private float _crosshairThickness = 2f;

        private void OnGUI()
        {
            if (_session == null || _session.Player == null)
            {
                return;
            }

            if (_showCrosshair && _session.IsSessionActive)
            {
                DrawCrosshair();
            }

            var player = _session.Player;
            var health = player.Health;
            var weapons = player.Weapons;
            var ammo = weapons != null ? weapons.AmmoInventory : null;
            var weapon = weapons != null ? weapons.ActiveWeapon : null;

            var healthText = health != null
                ? $"HP: {Mathf.CeilToInt(health.CurrentHealth)}/{Mathf.CeilToInt(health.MaxHealth)}"
                : "HP: -";
            var weaponText = weapon != null ? weapon.DisplayName : "-";
            var ammoText = ammo != null && weapon != null
                ? $"Ammo: {ammo.GetAmmo(weapon.WeaponId)}"
                : "Ammo: -";

            GUI.Box(new Rect(16f, 16f, 260f, 78f), $"{healthText}\nWeapon: {weaponText}\n{ammoText}\n[1]Pistol [2]Shotgun [3]Rifle");

            if (!_session.IsSessionActive)
            {
                var w = 320f;
                var h = 60f;
                GUI.Box(new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h), "YOU DIED — Session Interrupted");
            }
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
    }
}
