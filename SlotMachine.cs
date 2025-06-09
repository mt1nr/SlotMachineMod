using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace SlotMachine
{
    public class SlotMachine : Mod
    {
        public static SoundStyle stopSound;

        public override void Load()
        {
            stopSound = new SoundStyle("SlotMachine/Sounds/StopSound");
        }

        public override void Unload()
        {
        }
    }
}