using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Pigeon.Movement;

[HarmonyPatch]
public static class DropPodPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DropPod), "StartCountdown_ClientRpc")]
    public static void StartCountdown_ClientRpc_Postfix()
    {
        if (!SkinRandomizerPlugin.enabled.Value)
            return;

        RandomizeFavoriteSkins();
    }

    private static void RandomizeFavoriteSkins()
    {
        if (Player.LocalPlayer == null) return;

        var gears = new List<IUpgradable>
        {
            Player.LocalPlayer.Character,
        };

        if (DropPod.Instance != null)
        {
            gears.Add(DropPod.Instance.Prefab ?? (IUpgradable)DropPod.Instance);
        }

        // Equipped weapons only (slots 0 and 1)
        TryAddWeaponGear(gears, PlayerData.Instance.weapon1ID);
        TryAddWeaponGear(gears, PlayerData.Instance.weapon2ID);

        foreach (var gear in gears)
        {
            RandomizeSkinsForGear(gear);
        }
    }

    private static void TryAddWeaponGear(List<IUpgradable> gears, int weaponId)
    {
        if (weaponId == 0)
            return;

        var gear = PlayerData.GetGearFromID(weaponId);
        if (gear != null)
            gears.Add(gear);
    }

    private static void RandomizeSkinsForGear(IUpgradable gear)
    {
        if (gear == null) return;

        var gearData = PlayerData.GetGearData(gear);
        if (gearData == null) return;

        var equippedSkinsField = typeof(PlayerData.GearData).GetField("equippedSkins", BindingFlags.NonPublic | BindingFlags.Instance);
        var equippedSkins = equippedSkinsField?.GetValue(gearData) as List<PlayerData.UpgradeEquipData>;
        if (equippedSkins != null)
        {
            equippedSkins.Clear();
        }

        var mainSkins = new List<UpgradeInstance>();
        var gunCrabs = new List<UpgradeInstance>();
        var constellations = new List<UpgradeInstance>();

        var skinEnumerator = new PlayerData.SkinEnumerator(gear);
        while (skinEnumerator.MoveNext())
        {
            var upgrade = skinEnumerator.Upgrade;
            if (!upgrade.Favorite) continue;

            var skinUpgrade = upgrade.Upgrade as SkinUpgrade;
            if (skinUpgrade == null) continue;

            SkinUpgradeProperty_VFXCrab vfxCrabProp;
            bool hasVFXCrab = skinUpgrade.HasProperty(upgrade.Seed, out vfxCrabProp, out _);

            SkinUpgradeProperty_GunCrab gunCrabProp;
            bool hasGunCrab = skinUpgrade.HasProperty(upgrade.Seed, out gunCrabProp, out _);

            if (hasVFXCrab)
            {
                constellations.Add(upgrade);
            }
            else if (hasGunCrab)
            {
                gunCrabs.Add(upgrade);
            }
            else
            {
                mainSkins.Add(upgrade);
            }
        }

        bool anyEquipped = false;
        anyEquipped |= SelectAndEquipRandom(mainSkins, gearData);
        anyEquipped |= SelectAndEquipRandom(gunCrabs, gearData);
        anyEquipped |= SelectAndEquipRandom(constellations, gearData);

        if (anyEquipped)
        {
            ApplySkinsToGear(gear);
        }
    }

    private static bool SelectAndEquipRandom(List<UpgradeInstance> skins, PlayerData.GearData gearData)
    {
        if (skins.Count == 0) return false;

        var randomIndex = UnityEngine.Random.Range(0, skins.Count);
        var selectedSkin = skins[randomIndex];
        gearData.EquipUpgrade(selectedSkin, 0, 0, 0);
        return true;
    }

    private static void ApplySkinsToGear(IUpgradable gearPrefab)
    {
        if (gearPrefab == Player.LocalPlayer.Character)
        {
            Player.LocalPlayer.ApplySkins();
            return;
        }

        if (DropPod.Instance != null &&
            (gearPrefab == DropPod.Instance || gearPrefab == DropPod.Instance.Prefab || gearPrefab is DropPod))
        {
            DropPod.Instance.ApplyGearSkins();
            return;
        }

        // Live weapon instances: re-apply upgrades so materials, crabs, and constellations refresh
        var playerGear = Player.LocalPlayer.Gear;
        if (playerGear != null)
        {
            for (int i = 0; i < playerGear.Length; i++)
            {
                var liveGear = playerGear[i];
                if (liveGear == null)
                    continue;

                if (liveGear.Prefab == gearPrefab ||
                    liveGear == gearPrefab ||
                    (liveGear.Info != null && gearPrefab.Info != null && liveGear.Info.ID == gearPrefab.Info.ID))
                {
                    liveGear.ApplyUpgrades();
                    return;
                }
            }
        }

        // Fallback if no live instance is available yet — data is still saved for next equip/spawn
        var equippedSkins = new PlayerData.EquippedSkinEnumerator(gearPrefab);
        while (equippedSkins.MoveNext())
        {
            var upgrade = equippedSkins.Upgrade;
            if (upgrade?.Upgrade is SkinUpgrade skinUpgrade)
            {
                skinUpgrade.Apply(gearPrefab, upgrade.Seed, Player.LocalPlayer);
            }
        }

        gearPrefab.OnUpgradesChanged(null, null);
        Player.LocalPlayer.ApplySkins();
    }
}
