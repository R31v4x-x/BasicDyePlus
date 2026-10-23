using BasicDyePlus.Tiles;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace BasicDyePlus.UI {
	public class Dye_Mixing_Vat_UI_System : ModSystem {
		public UserInterface dyeMixingInterface;
		public Dye_Mixing_UI dyeMixingUI;

		public void ShowMixingUI() {
			dyeMixingInterface?.SetState(new Dye_Mixing_UI() { Left = DyePanel.BaseLeft, Top = DyePanel.BaseTop });
		}
		public void HideMixingUI() {
			dyeMixingInterface?.SetState(null);
		}
		public override void PostSetupContent() {
			// Create custom interface which can swap between different UIStates
			dyeMixingInterface = new UserInterface();
			// Creating custom UIState
			dyeMixingUI = new Dye_Mixing_UI();

			// Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
			dyeMixingUI.Activate();
		}
		public override void UpdateUI(GameTime gameTime) {
			// Here we call .Update on our custom UI and propagate it to its state and underlying elements
			if (dyeMixingInterface?.CurrentState != null) {
				dyeMixingInterface?.Update(gameTime);
			}
		}
		// Adding a custom layer to the vanilla layer list that will call .Draw on your interface if it has a state
		// Setting the InterfaceScaleType to UI for appropriate UI scaling
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"BasicDyePlus: Dye Mixing Vat",
					delegate {
						if (dyeMixingInterface?.CurrentState != null) {
							dyeMixingInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
	public class Dye_Mixing_UI : UIState {
		public DyePanel DyePanel;
		public DyeSlot[] DyeSlot = new DyeSlot[3];
		public Item[] items = new Item[3];
		public override void OnInitialize() {
			for (int i = 0; i < items.Length; i++) {
				items[i] ??= new();
			}
			// Here we define our container UIElement. In DraggableUIPanel.cs, you can see that DraggableUIPanel is a UIPanel with a couple added features.
			DyePanel = new DyePanel();
			DyePanel.SetPadding(0);
			// We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`.
			// This means that this class, ExampleCoinsUI, will be our Parent. Since ExampleCoinsUI is a UIState, the Left and Top are relative to the top left of the screen.
			// SetRectangle method help us to set the position and size of UIElement
			SetRectangle(DyePanel, (Main.screenWidth * 0.5f) * Main.inventoryScale, (Main.screenHeight * 0.5f - 180) * Main.inventoryScale, 170f, 90f);
			DyePanel.BackgroundColor = new Color(73, 94, 171);

			UIElement label = new UIText(TileItem.Get<Dye_Mixing_Vat>().DisplayName);
			SetRectangle(label, 8, 8, label.Width.Pixels, label.Height.Pixels);
			label.IgnoresMouseInteraction = true;
			DyePanel.Append(label);

			// UIMoneyDisplay is a fairly complicated custom UIElement. UIMoneyDisplay handles drawing some text and coin textures.
			// Organization is key to managing UI design. Making a contained UIElement like UIMoneyDisplay will make many things easier.
			DyeSlot[0] = new DyeSlot(items) { Index = 0 };
			SetRectangle(DyeSlot[0], 8, 34, 52, 52);
			DyePanel.Append(DyeSlot[0]);
			DyeSlot[1] = new DyeSlot(items) { Index = 1 };
			SetRectangle(DyeSlot[1], 8 + 52, 34, 52, 52);
			DyePanel.Append(DyeSlot[1]);
			DyeSlot[2] = new DyeSlot(items) { Index = 2 };
			SetRectangle(DyeSlot[2], 8 + (52 * 2), 34, 52, 52);
			DyePanel.Append(DyeSlot[2]);

			Append(DyePanel);
			// As a recap, ExampleCoinsUI is a UIState, meaning it covers the whole screen. We attach CoinCounterPanel to ExampleCoinsUI some distance from the top left corner.
			// We then place playButton, closeButton, and MoneyDisplay onto CoinCounterPanel so we can easily place these UIElements relative to CoinCounterPanel.
			// Since CoinCounterPanel will move, this proper organization will move playButton, closeButton, and MoneyDisplay properly when CoinCounterPanel moves.
		}
		public override void Update(GameTime gameTime) {
			if (!Main.playerInventory) {
				foreach (Item item in items) {
					if (!item.IsAir) Main.LocalPlayer.QuickSpawnItem(item.GetSource_DropAsItem(), item, item.stack);
				}
				ModContent.GetInstance<Dye_Mixing_Vat_UI_System>().HideMixingUI();
				return;
			}
			DyePanel.Update(gameTime);
			for (int i = 0; i < DyeSlot.Length; i++) {
				DyeSlot[i].Update(gameTime);
			}
		}
		private static void SetRectangle(UIElement uiElement, float left, float top, float width, float height) {
			uiElement.Left.Set(left, 0f);
			uiElement.Top.Set(top, 0f);
			uiElement.Width.Set(width, 0f);
			uiElement.Height.Set(height, 0f);
		}

	}
	public class DyePanel : UIPanel {
		// Stores the offset from the top left of the UIPanel while dragging
		private Vector2 offset;
		// A flag that checks if the panel is currently being dragged
		private bool dragging;
		public static StyleDimension BaseLeft;
		public static StyleDimension BaseTop;

		public override void LeftMouseDown(UIMouseEvent evt) {
			// When you override UIElement methods, don't forget call the base method
			// This helps to keep the basic behavior of the UIElement
			base.LeftMouseDown(evt);
			// When the mouse button is down on this element, then we start dragging
			if (evt.Target == this) {
				DragStart(evt);
			}
		}
		public override void LeftMouseUp(UIMouseEvent evt) {
			base.LeftMouseUp(evt);
			// When the mouse button is up, then we stop dragging
			if (evt.Target == this) {
				DragEnd(evt);
			}
		}
		private void DragStart(UIMouseEvent evt) {
			// The offset variable helps to remember the position of the panel relative to the mouse position
			// So no matter where you start dragging the panel, it will move smoothly
			offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
			dragging = true;
		}
		private void DragEnd(UIMouseEvent evt) {
			Vector2 endMousePosition = evt.MousePosition;
			dragging = false;

			Left.Set(endMousePosition.X - offset.X, 0f);
			Top.Set(endMousePosition.Y - offset.Y, 0f);
			BaseLeft = Left;
			BaseTop = Top;

			Recalculate();
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			// Checking ContainsPoint and then setting mouseInterface to true is very common
			// This causes clicks on this UIElement to not cause the player to use current items
			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
				Main.isMouseLeftConsumedByUI = true;
			}

			if (dragging) {
				Left.Set(Main.mouseX - offset.X, 0f); // Main.MouseScreen.X and Main.mouseX are the same
				Top.Set(Main.mouseY - offset.Y, 0f);
				BaseLeft = Left;
				BaseTop = Top;
				Recalculate();
			}

			// Here we check if the DraggableUIPanel is outside the Parent UIElement rectangle
			// (In our example, the parent would be ExampleCoinsUI, a UIState. This means that we are checking that the DraggableUIPanel is outside the whole screen)
			// By doing this and some simple math, we can snap the panel back on screen if the user resizes his window or otherwise changes resolution
			Rectangle parentSpace = Parent.GetDimensions().ToRectangle();
			if (!GetDimensions().ToRectangle().Intersects(parentSpace)) {
				Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
				Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
				BaseLeft = Left;
				BaseTop = Top;
				// Recalculate forces the UI system to do the positioning math again.
				Recalculate();
			}
		}
	}
	public class DyeSlot(Item[] items) : UIElement {
		public int Index;
		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);/*
			if (ContainsPoint(Main.MouseScreen)) {
				if (Index == 2 && !Main.mouseItem.IsAir) return;
				ItemSlot.LeftClick(items, ItemSlot.Context.InventoryCoin, Index);
			}*/
		}
		public override void Update(GameTime gameTime) {
			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
				Main.isMouseLeftConsumedByUI = true;
				if (Main.mouseItem.IsAir) {
					if (items[Index].IsAir) {
						Main.mouseText = true;
						Main.hoverItemName = "Hewo";
						Main.HoverItem = Main.LocalPlayer.HeldItem;
					}
				}
				//if (Index == 2 && !Main.mouseItem.IsAir) return;
				ItemSlot.LeftClick(items, ItemSlot.Context.EquipDye, Index);
			}
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle dim = GetInnerDimensions();
			ItemSlot.Draw(spriteBatch, items, ItemSlot.Context.InventoryCoin, Index, dim.Position());
			if (!items[Index].IsAir) return;
			Texture2D icon = TextureAssets.Extra[ExtrasID.EquipIcons].Value;
			Rectangle frame = icon.Frame(3, 6, 1);
			spriteBatch.Draw(
				icon,
				dim.Position() + TextureAssets.InventoryBack.Value.Size() / 2 * Main.inventoryScale,
				frame,
				Color.White * 0.35f,
				0,
				frame.Size() * 0.5f,
				Main.inventoryScale,
				SpriteEffects.None,
				0
			);
		}
	}
}
