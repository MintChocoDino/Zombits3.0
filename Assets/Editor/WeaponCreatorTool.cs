using UnityEditor;
using UnityEngine;

public class WeaponCreatorWindow : EditorWindow
{
    private const string WeaponsFolder = "Assets/Prefabs/Weapons";

    private enum WeaponType { Pistol, Rifle, Shotgun }

    // Form state
    private WeaponType weaponType = WeaponType.Pistol;
    private string weaponName = "NewWeapon";

    private float bulletDamage = 15f;
    private float bulletSpeed = 22f;
    private float fireRate = 4f;
    private int magazineSize = 12;
    private int reserveAmmoMax = 72;
    private float reloadTime = 1.2f;

    // Shotgun-only
    private int pelletsPerShot = 6;
    private float spreadAngle = 25f;

    // Optional visuals
    private Sprite weaponSprite;
    private Sprite iconSprite;
    private Bullet bulletPrefab;

    // Mystery box integration
    private bool addToMysteryBox = false;
    private MysteryBoxInteractable mysteryBoxPrefab;

    private Vector2 scroll;

    [MenuItem("Tools/Weapon Creator")]
    public static void ShowWindow()
    {
        GetWindow<WeaponCreatorWindow>("Weapon Creator");
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Weapon Creator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Fill out the form and click Create Weapon. The prefab will be saved to " + WeaponsFolder + ".",
            MessageType.Info);
        EditorGUILayout.Space();

        // Basic info
        EditorGUILayout.LabelField("Basic Info", EditorStyles.boldLabel);
        weaponName = EditorGUILayout.TextField("Weapon Name", weaponName);
        weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);
        EditorGUILayout.Space();

        // Stats
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
        bulletDamage = EditorGUILayout.FloatField("Bullet Damage", bulletDamage);
        bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", bulletSpeed);
        fireRate = EditorGUILayout.FloatField("Fire Rate (shots/sec)", fireRate);
        magazineSize = EditorGUILayout.IntField("Magazine Size", magazineSize);
        reserveAmmoMax = EditorGUILayout.IntField("Reserve Ammo Max", reserveAmmoMax);
        reloadTime = EditorGUILayout.FloatField("Reload Time (sec)", reloadTime);
        EditorGUILayout.Space();

        // Shotgun-only fields
        if (weaponType == WeaponType.Shotgun)
        {
            EditorGUILayout.LabelField("Shotgun Settings", EditorStyles.boldLabel);
            pelletsPerShot = EditorGUILayout.IntField("Pellets Per Shot", pelletsPerShot);
            spreadAngle = EditorGUILayout.FloatField("Spread Angle (deg)", spreadAngle);
            EditorGUILayout.Space();
        }

        // Visuals (optional)
        EditorGUILayout.LabelField("Visuals (optional)", EditorStyles.boldLabel);
        weaponSprite = (Sprite)EditorGUILayout.ObjectField("Weapon Sprite", weaponSprite, typeof(Sprite), false);
        iconSprite = (Sprite)EditorGUILayout.ObjectField("HUD Icon", iconSprite, typeof(Sprite), false);
        bulletPrefab = (Bullet)EditorGUILayout.ObjectField("Bullet Prefab", bulletPrefab, typeof(Bullet), false);
        EditorGUILayout.Space();

        // Mystery box integration
        EditorGUILayout.LabelField("Mystery Box", EditorStyles.boldLabel);
        addToMysteryBox = EditorGUILayout.Toggle("Add to Loot Pool", addToMysteryBox);
        if (addToMysteryBox)
        {
            mysteryBoxPrefab = (MysteryBoxInteractable)EditorGUILayout.ObjectField(
                "Mystery Box Prefab", mysteryBoxPrefab, typeof(MysteryBoxInteractable), false);
        }
        EditorGUILayout.Space();

        GUILayout.FlexibleSpace();

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(weaponName)))
        {
            if (GUILayout.Button("Create Weapon", GUILayout.Height(32)))
                CreateWeapon();
        }

        EditorGUILayout.EndScrollView();
    }

        private void CreateWeapon()
    {
        EnsureFolderExists(WeaponsFolder);

        // Build the GameObject: root with sprite + script, plus a Muzzle child.
        GameObject root = new GameObject(weaponName);
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        if (weaponSprite != null) sr.sprite = weaponSprite;

        // Add the correct weapon script for the chosen type.
        Weapon weapon;
        switch (weaponType)
        {
            case WeaponType.Pistol:
                weapon = root.AddComponent<Pistol>();
                break;
            case WeaponType.Rifle:
                weapon = root.AddComponent<Rifle>();
                break;
            case WeaponType.Shotgun:
                Shotgun shotgun = root.AddComponent<Shotgun>();
                shotgun.pelletsPerShot = pelletsPerShot;
                shotgun.spreadAngle = spreadAngle;
                weapon = shotgun;
                break;
            default:
                weapon = root.AddComponent<Pistol>();
                break;
        }

        // Fill in stats.
        weapon.bulletDamage = bulletDamage;
        weapon.bulletSpeed = bulletSpeed;
        weapon.fireRate = fireRate;
        weapon.magazineSize = magazineSize;
        weapon.reserveAmmoMax = reserveAmmoMax;
        weapon.reloadTime = reloadTime;
        weapon.icon = iconSprite;
        weapon.bulletPrefab = bulletPrefab;

        // Create the muzzle child and wire it to the script.
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(root.transform);
        muzzle.transform.localPosition = new Vector3(0.3f, 0f, 0f);
        weapon.muzzle = muzzle.transform;

        // Save as prefab, then clean up the scene instance.
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{WeaponsFolder}/{weaponName}.prefab");
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, assetPath);
        DestroyImmediate(root);

        // Optionally register with the mystery box.
        if (addToMysteryBox && mysteryBoxPrefab != null && savedPrefab != null)
            AddToMysteryBoxLoot(savedPrefab);

        // Ping the new prefab so the user sees it in the Project window.
        EditorGUIUtility.PingObject(savedPrefab);
        Selection.activeObject = savedPrefab;

        EditorUtility.DisplayDialog(
            "Weapon Created",
            $"'{weaponName}' saved to {assetPath}." + (addToMysteryBox ? "\n\nAdded to mystery box loot pool." : ""),
            "OK");
    }

    private void AddToMysteryBoxLoot(GameObject weaponPrefab)
    {
        string mbPath = AssetDatabase.GetAssetPath(mysteryBoxPrefab);
        GameObject mbRoot = PrefabUtility.LoadPrefabContents(mbPath);
        MysteryBoxInteractable mb = mbRoot.GetComponent<MysteryBoxInteractable>();
        Weapon newWeapon = weaponPrefab.GetComponent<Weapon>();

        if (mb != null && newWeapon != null)
        {
            Weapon[] oldPool = mb.possibleWeapons ?? new Weapon[0];
            Weapon[] newPool = new Weapon[oldPool.Length + 1];
            for (int i = 0; i < oldPool.Length; i++) newPool[i] = oldPool[i];
            newPool[oldPool.Length] = newWeapon;
            mb.possibleWeapons = newPool;

            PrefabUtility.SaveAsPrefabAsset(mbRoot, mbPath);
        }
        PrefabUtility.UnloadPrefabContents(mbRoot);
    }

    private void EnsureFolderExists(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}