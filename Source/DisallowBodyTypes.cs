using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DisallowBodyTypes
{
    public class DisallowBodyTypes : Mod
    {
        public class Settings : ModSettings
        {
            public bool _disable_thin = false;
            public bool _disable_fat = false;
            public bool _disable_hulk = false;

            public override void ExposeData()
            {
                Scribe_Values.Look(ref _disable_thin, "disable_thin");
                Scribe_Values.Look(ref _disable_fat, "disable_fat");
                Scribe_Values.Look(ref _disable_hulk, "disable_hulk");
                base.ExposeData();
            }
        }

        public static Settings Config { get; private set; }

        public DisallowBodyTypes(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("disallow.body.types");
            harmony.PatchAll();
            Config = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("Disable thin body type", ref Config._disable_thin);
            listingStandard.CheckboxLabeled("Disable fat body type", ref Config._disable_fat);
            listingStandard.CheckboxLabeled("Disable hulk body type", ref Config._disable_hulk);
            listingStandard.End();
            base.DoSettingsWindowContents(rect);
        }

        public override string SettingsCategory()
        {
            return "DisallowBodyTypes";
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
    public class PawnGenerator__GenerateBodyType
    {
        private static BodyTypeDef SelectNewBodyType(Pawn pawn)
        {
            if (!DisallowBodyTypes.Config._disable_thin && Rand.Value < 0.5f) return BodyTypeDefOf.Thin;
            return (pawn.gender == Gender.Female) ? BodyTypeDefOf.Female : BodyTypeDefOf.Male;
        }
 
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn pawn)
        {
            bool change_body_type =
                (pawn.story.bodyType == BodyTypeDefOf.Thin && DisallowBodyTypes.Config._disable_thin) ||
                (pawn.story.bodyType == BodyTypeDefOf.Fat && DisallowBodyTypes.Config._disable_fat) ||
                (pawn.story.bodyType == BodyTypeDefOf.Hulk && DisallowBodyTypes.Config._disable_hulk);

            if (change_body_type)
            {
                Log.Warning($"Replacing body type {pawn.story.bodyType}");
                pawn.story.bodyType = SelectNewBodyType(pawn);
            }
        }
    }


}