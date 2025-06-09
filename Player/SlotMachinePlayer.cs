using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using SlotMachineItem.UI;

namespace SlotMachine
{
	public class SlotMachinePlayer : ModPlayer
	{
		public SlotMachineUI slotMachineUI;
		public int slotMachineCooldown;

		public override void ResetEffects()
		{
			if (slotMachineCooldown > 0)
			{
				slotMachineCooldown--;
			}
		}

		public void CloseSlotMachine()
		{
			slotMachineUI.Hide();
		}
	}
}