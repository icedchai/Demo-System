using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Patches
{
    public class Patcher
    {
        private Harmony harmony;

        public void Patch()
        {
            harmony = new Harmony("me.icedchai.demo");
            harmony.PatchAll();
        }

        public void UnpatchAll()
        {
            harmony.UnpatchAll("me.icedchai.demo");
        }
    }
}
