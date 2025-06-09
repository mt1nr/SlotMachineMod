using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using System;
using Microsoft.Xna.Framework;
using SlotMachineItem.UI;

namespace SlotMachine
{
	public class SlotMachineSystem : ModSystem
	{
		public static Texture2D slotMachineTexture;
		public UserInterface _slotMachineInterface;
		private SlotMachineUI _slotMachineUI;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				// load slot machine texture
				slotMachineTexture = ModContent.Request<Texture2D>("SlotMachine/Textures/SlotMachineTexture").Value;

				// set up UI and interface
				_slotMachineInterface = new UserInterface();
				_slotMachineUI = new SlotMachineUI();
				_slotMachineUI.Activate();
				_slotMachineInterface.SetState(_slotMachineUI);
			}
		}

		public override void Unload()
		{
			// clean up static and UI references
			slotMachineTexture = null;
			_slotMachineInterface = null;
			_slotMachineUI = null;
		}

		public override void UpdateUI(GameTime gameTime)
		{
			// update slot machine UI if active
			if (_slotMachineInterface?.CurrentState != null)
			{
				_slotMachineInterface.Update(gameTime);
			}
		}

		public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
		{
			// insert slot machine UI before mouse text
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"SlotMachine: SlotMachineUI",
					delegate
					{
						// draw slot machine UI if active
						if (_slotMachineInterface?.CurrentState != null)
						{
							_slotMachineInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}