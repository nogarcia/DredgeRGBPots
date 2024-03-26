using HarmonyLib;
using UnityEngine;

namespace DredgeRGBPots
{
    [HarmonyPatch(typeof(PlacedHarvestPOI))]
    internal static class PlacedHarvestPOIPatch
    {
        const string BuoyModelsBase = "CrabPotBuoy/BuoyMesh/";
        const string AberrationObjName = "CrabPotBuoy_Aberration";
        const string AberrationObjPath = BuoyModelsBase + "CrabPotBuoy_Aberration";

        static Texture2D abberationTexture = null;

        const int textureSize = 32;

        public static void Init()
        {
            BuildTextures();
        }

        static void BuildTextures()
        {
            if (abberationTexture == null)
            {
                abberationTexture = new Texture2D(textureSize, textureSize);
            }

            Color[] colors = new Color[textureSize * textureSize];

            // #876699: This appears much brighter in-game
            Color color = new Color(0.53f, 0.40f, 0.60f);

            for (int y = 0; y <= 20; y++)
            {
                for (int x = 21; x < textureSize; x++)
                {
                    colors[textureSize * y + x] = color;
                }
            }
            abberationTexture.SetPixels(colors, 0);
            abberationTexture.Apply(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlacedHarvestPOI.UpdateVisuals))]
        public static void VisualsPostfix(PlacedHarvestPOI __instance)
        {
            if (__instance.harvestable is SerializedCrabPotPOIData)
            {
                SerializedCrabPotPOIData crabPot = __instance.harvestable as SerializedCrabPotPOIData;

                if (crabPot.durability <= 0)
                {
                    // The broken state takes priority!
                    __instance.transform.Find(AberrationObjPath).gameObject.SetActive(false);
                    return;
                }

                bool hasAberration = false;
                foreach (SpatialItemInstance item in crabPot.grid.spatialItems) {
                    FishItemData fish = item.GetItemData<FishItemData>();

                    if (fish == null) continue;

                    hasAberration |= fish.IsAberration;
                }

                if (hasAberration)
                {
                    // Enable the lantern for our aberration state, and disable all the others.
                    __instance.transform.Find(AberrationObjPath).gameObject.SetActive(true);
                    __instance.readyObj.SetActive(false);
                    __instance.idleObj.SetActive(false);
                    __instance.brokenObj.SetActive(false);
                } else
                {
                    // By now, the base game has already updated the state of the buoy appropriately.
                    // All we have to do is clean up after ourselves:
                    __instance.transform.Find(AberrationObjPath).gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlacedHarvestPOI.Awake))]
        public static void AwakePostfix(PlacedHarvestPOI __instance)
        {
            GameObject aberrationObj = Object.Instantiate(__instance.readyObj, __instance.readyObj.transform.parent);
            aberrationObj.name = AberrationObjName;
            aberrationObj.SetActive(false);

            // Relevant textures:
            // Texture2D_9aa7ba2263944b48bbf43c218dc48459: Albedo (haven't checked dimensions)
            // Texture2D_c7b8c5c57d6443a5a9f86b68269754f3: Emission (32x32)
            // Texture2D_d75ba12263b343d3ad393c05a6dda1b7: LightFlickerGradient (32x1)

            // Relevant keywords:
            // BOOLEAN_1C655FB145BE4DCDA50D5BE036D8DE1E: LightsTurnOffAtDay
            // BOOLEAN_831B22E2CB064148ADEAE08DFC09DFD0: RecieveShadows
            // BOOLEAN_0965F30455D645A4AD7F01AF266AE935: Emissive (TRUE for lit buoys)
            // BOOLEAN_3E4CD0FFFCDB4460AA7D73A352CD2455: LightsFlicker (TRUE for buoys with items)

            if (abberationTexture == null)
            {
                BuildTextures();
            }

            aberrationObj.GetComponent<Renderer>().material.SetTexture("Texture2D_c7b8c5c57d6443a5a9f86b68269754f3", abberationTexture);
        }
    }
}
