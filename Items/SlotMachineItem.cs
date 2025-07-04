using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SlotMachineItem.UI;

namespace SlotMachine.Items
{
	public class SlotMachineItem : ModItem
	{
		public override void SetDefaults()
		{
			// set item size, use time, style, value, and rarity
			Item.width = 50;
			Item.height = 56;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.value = Item.buyPrice(0, 10, 0, 0);
			Item.rare = ItemRarityID.Expert;
		}

		public override bool? UseItem(Player player)
		{
			var slotMachinePlayer = player.GetModPlayer<SlotMachinePlayer>();

			if (slotMachinePlayer.slotMachineCooldown > 0)
			{
				return false; // prevent use if cooldown is active
			}

			if (!Main.dedServ && Main.myPlayer == player.whoAmI && Main.netMode != NetmodeID.Server)
			{
				var slotMachineSystem = ModContent.GetInstance<SlotMachineSystem>();

				// Ensure the player always has a UI instance
				if (slotMachinePlayer.slotMachineUI == null)
				{
					slotMachinePlayer.slotMachineUI = new SlotMachineUI();
					slotMachinePlayer.slotMachineUI.Activate();
				}

				// If UI is visible and mouse is over it, handle click
				if (slotMachineSystem._slotMachineInterface.CurrentState == slotMachinePlayer.slotMachineUI && slotMachinePlayer.slotMachineUI.IsMouseOverUI())
				{
					if (slotMachinePlayer.slotMachineUI.IsActive)
					{
						slotMachinePlayer.slotMachineUI.HandleMouseClick();
						slotMachinePlayer.slotMachineCooldown = Item.useTime;
						return false;
					}
					// If UI is not visible, do nothing
				}
				else
				{
					if (slotMachineSystem._slotMachineInterface.CurrentState == slotMachinePlayer.slotMachineUI)
					{
						slotMachinePlayer.slotMachineUI.Hide();
						slotMachinePlayer.slotMachineUI.ResetUI(); // Always reset state when hiding
						slotMachineSystem._slotMachineInterface.SetState(null);
					}
					else
					{
						slotMachinePlayer.slotMachineUI.ResetUI(); // Always reset state before showing
						slotMachinePlayer.slotMachineUI.Show();
						slotMachineSystem._slotMachineInterface.SetState(slotMachinePlayer.slotMachineUI);
					}
					slotMachinePlayer.slotMachineCooldown = Item.useTime;
				}
			}
			return true;
		}

		public override void AddRecipes()
		{
			// add crafting recipe
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Wood, 10);
			recipe.AddIngredient(ItemID.GoldCoin, 20);
			recipe.Register();
		}
	}
}