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

            if (!Main.dedServ)
            {
                if (Main.myPlayer == player.whoAmI && Main.netMode != NetmodeID.Server)
                {
                    var slotMachineSystem = ModContent.GetInstance<SlotMachineSystem>();

                    // handle click if mouse is over UI
                    if (slotMachineSystem._slotMachineInterface.CurrentState is SlotMachineUI slotMachineUI && slotMachineUI.IsMouseOverUI())
                    {
                        slotMachineUI.HandleMouseClick();
                        slotMachinePlayer.slotMachineCooldown = Item.useTime;
                        return false;
                    }

                    // toggle UI visibility
                    if (slotMachineSystem._slotMachineInterface.CurrentState == slotMachinePlayer.slotMachineUI)
                    {
                        slotMachineSystem._slotMachineInterface.SetState(null); // hide UI
                    }
                    else
                    {
                        // show UI if not active
                        if (slotMachinePlayer.slotMachineUI == null || !slotMachinePlayer.slotMachineUI.IsActive)
                        {
                            slotMachinePlayer.slotMachineUI = new SlotMachineUI();
                            slotMachinePlayer.slotMachineUI.Activate();
                        }

                        slotMachinePlayer.slotMachineUI.Show();
                        slotMachineSystem._slotMachineInterface.SetState(slotMachinePlayer.slotMachineUI);
                    }

                    slotMachinePlayer.slotMachineCooldown = Item.useTime; // set cooldown
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