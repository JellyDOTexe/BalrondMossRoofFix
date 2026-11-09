using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace BalrondMossRoofFix
{
    // Balrond Constructions' moss/bush roof leaves (and other *_bal pieces with foliage) reference a bundled copy of
    // the game's "Custom/Vegetation" shader. It has the same name but is a different shader object without a working
    // alpha cutout here, so every leaf card renders as a solid square. Balrond's own shader swap only replaces shaders
    // named "Balrond/...", so this copy is never replaced. After Balrond finishes in ZNetScene.Awake, this plugin
    // points those materials at the game's own Custom/Vegetation shader.
    [BepInPlugin(Guid, "Balrond Moss Roof Fix", "1.0.0")]
    [BepInDependency("balrond.astafaraios.BalrondConstructions", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "mossrooffix.balrondconstructions";
        private const string Veg = "Custom/Vegetation";
        private static Plugin I;
        private static ConfigEntry<bool> Enabled;
        private Harmony harmony;

        private void Awake()
        {
            I = this;
            Enabled = Config.Bind("General", "Enabled", true, "Swap Balrond foliage materials to the game's vegetation shader (takes effect on the next world load).");
            harmony = new Harmony(Guid);
            var post = new HarmonyMethod(typeof(Plugin), nameof(AfterSceneAwake)) { priority = Priority.Last, after = new[] { "balrond.astafaraios.BalrondConstructions" } };
            harmony.Patch(AccessTools.Method(typeof(ZNetScene), "Awake"), postfix: post);
        }

        private void OnDestroy() => harmony?.UnpatchSelf();

        private static void AfterSceneAwake(ZNetScene __instance)
        {
            if (!Enabled.Value || __instance == null || __instance.m_prefabs == null) return;
            try
            {
                Shader game = FindGameShader(__instance);
                if (game == null) { I.Logger.LogWarning("Game vegetation shader not found; nothing changed."); return; }
                var done = new HashSet<Material>();
                foreach (var prefab in __instance.m_prefabs)
                {
                    if (prefab == null || !prefab.name.EndsWith("_bal", StringComparison.Ordinal)) continue;
                    foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
                        foreach (var m in r.sharedMaterials) Swap(m, game, done);
                    foreach (var ir in prefab.GetComponentsInChildren<InstanceRenderer>(true))
                        Swap(ir.m_material, game, done);
                }
                I.Logger.LogInfo("Moved " + done.Count + " Balrond material(s) to the game's vegetation shader.");
            }
            catch (Exception e) { I.Logger.LogError("Fix failed, nothing further changed: " + e); }
        }

        private static Shader FindGameShader(ZNetScene scene)
        {
            foreach (var n in new[] { "Bush02_en", "Bush01", "Beech1" })
            {
                var p = scene.GetPrefab(n);
                if (p == null) continue;
                foreach (var r in p.GetComponentsInChildren<Renderer>(true))
                    foreach (var m in r.sharedMaterials)
                        if (m != null && m.shader != null && m.shader.name == Veg) return m.shader;
            }
            return null;
        }

        private static void Swap(Material m, Shader game, HashSet<Material> done)
        {
            if (m == null || m.shader == null || m.shader == game || m.shader.name != Veg) return;
            m.shader = game;
            m.renderQueue = -1;
            done.Add(m);
        }
    }
}
