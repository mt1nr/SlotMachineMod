using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;

namespace SlotMachineItem.UI
{
    public class SlotMachineDraggableUIPanel : UIPanel
    {
        private Vector2 offset;
        private bool dragging;

        public override void LeftMouseDown(UIMouseEvent evt) {
            base.LeftMouseDown(evt);
            if (evt.Target == this) DragStart(evt);
        }

        public override void LeftMouseUp(UIMouseEvent evt) {
            base.LeftMouseUp(evt);
            if (evt.Target == this) DragEnd(evt);
        }

        private void DragStart(UIMouseEvent evt) {
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt) {
            dragging = false;
            Left.Set(evt.MousePosition.X - offset.X, 0f);
            Top.Set(evt.MousePosition.Y - offset.Y, 0f);
            Recalculate();
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
            if (dragging) {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }
            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace)) {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }
        }
    }
}