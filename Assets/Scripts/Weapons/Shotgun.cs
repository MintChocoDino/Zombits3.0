using UnityEngine;

public class Shotgun : Weapon
{
    [Header("Shotgun")]
    public int pelletsPerShot = 6;
    public float spreadAngle = 25f;

    protected override void Fire(Vector2 aimDirection)
    {
        float baseAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        float halfSpread = spreadAngle * 0.5f;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            float t = pelletsPerShot == 1 ? 0.5f : (float)i / (pelletsPerShot - 1);
            float angle = baseAngle - halfSpread + spreadAngle * t;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            SpawnBullet(dir);
        }
    }
}