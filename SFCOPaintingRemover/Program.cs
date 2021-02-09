using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Threading.Tasks;
using Noggog;

namespace SFCOPaintingRemover
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            return SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "SFCOPaintingRemover.esp",
                        TargetRelease = GameRelease.SkyrimSE,
                        BlockAutomaticExit = true,
                    }
                });
        }

        private static ModKey SFCO = ModKey.FromNameAndExtension("Snazzy Furniture and Clutter Overhaul.esp");

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (state.LoadOrder.TryGetValue(SFCO, out var listing)
                && listing.Mod != null)
            {
                var cache = listing.Mod.ToImmutableLinkCache();
                foreach (var staticObject in cache.PriorityOrder.WinningOverrides<IStaticGetter>())
                {
                    if (staticObject.EditorID != null && staticObject.EditorID.ContainsInsensitive("painting"))
                    {
                        var staticObjectCopy = staticObject.DeepCopy();
                        if (staticObjectCopy.Model != null)
                        {
                            staticObjectCopy.Model.Clear();
                            state.PatchMod.Statics.Set(staticObjectCopy);
                        }
                    }
                }
            }
            else
                throw new Exception("ERROR: Snazzy's Furniture and Clutter Overhaul not detected in load order. You need to install SFCO prior to running this patcher!");
        }
    }
}
