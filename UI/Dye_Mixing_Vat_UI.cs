using BasicDyePlus.Tiles;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace BasicDyePlus.UI {
	public class Dye_Mixing_Vat_UI_System : ModSystem {
		public UserInterface dyeMixingInterface;
		public Dye_Mixing_UI dyeMixingUI;

		public void ShowMixingUI() {
			dyeMixingInterface?.SetState(dyeMixingUI = new Dye_Mixing_UI());
		}
		public void HideMixingUI() {
			foreach (Item item in dyeMixingUI.items) {
				if (!item.IsAir) Main.LocalPlayer.QuickSpawnItem(item.GetSource_DropAsItem(), item, item.stack);
			}
			dyeMixingInterface?.SetState(null);
		}
		public override void PostSetupContent() {
			// Create custom interface which can swap between different UIStates
			dyeMixingInterface = new UserInterface();

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
		public DyeSlot[] DyeSlot = new DyeSlot[3];
		public Item[] items = new Item[3];
		public override void OnInitialize() {
			for (int i = 0; i < items.Length; i++) {
				items[i] ??= new();
			}
			// Here we define our container UIElement. In DraggableUIPanel.cs, you can see that DraggableUIPanel is a UIPanel with a couple added features.
			UIPanel panel = new DyePanel();
			panel.SetPadding(8);
			// We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`.
			// This means that this class, ExampleCoinsUI, will be our Parent. Since ExampleCoinsUI is a UIState, the Left and Top are relative to the top left of the screen.
			// SetRectangle method help us to set the position and size of UIElement
			SetRectangle(panel, 715.60614f, 269.3318f, 209, 105);
			panel.BackgroundColor = new Color(73, 94, 171);

			LocalizedText text = TileItem.Get<Dye_Mixing_Vat>().DisplayName;
			float padding = ChatManager.GetStringSize(FontAssets.MouseText.Value, text.Value, Vector2.One * Main.inventoryScale).Y + 4;
			UIText label = new(text, 1.2f * Main.inventoryScale);
			SetRectangle(label, 0, 0);
			label.IgnoresMouseInteraction = true;
			panel.Append(label);

			// UIMoneyDisplay is a fairly complicated custom UIElement. UIMoneyDisplay handles drawing some text and coin textures.
			// Organization is key to managing UI design. Making a contained UIElement like UIMoneyDisplay will make many things easier.
			DyeSlot[0] = new DyeSlot(items) { Index = 0 };
			SetRectangle(DyeSlot[0], 0, padding);
			panel.Append(DyeSlot[0]);
			DyeSlot[1] = new DyeSlot(items) { Index = 1 };
			SetRectangle(DyeSlot[1], 52 + 16, padding);
			panel.Append(DyeSlot[1]);
			DyeSlot[2] = new DyeSlot(items) { Index = 2 };
			SetRectangle(DyeSlot[2], 52 * 2 + 32, padding);
			panel.Append(DyeSlot[2]);

			Append(panel);
			// As a recap, ExampleCoinsUI is a UIState, meaning it covers the whole screen. We attach CoinCounterPanel to ExampleCoinsUI some distance from the top left corner.
			// We then place playButton, closeButton, and MoneyDisplay onto CoinCounterPanel so we can easily place these UIElements relative to CoinCounterPanel.
			// Since CoinCounterPanel will move, this proper organization will move playButton, closeButton, and MoneyDisplay properly when CoinCounterPanel moves.
		}
		public override void Update(GameTime gameTime) {
			if (!Main.playerInventory) {
				ModContent.GetInstance<Dye_Mixing_Vat_UI_System>().HideMixingUI();
				return;
			}

			base.Update(gameTime);
		}
		private static void SetRectangle(UIElement uiElement, float left, float top, float width = -1, float height = -1) {
			uiElement.Left.Set(left * Main.inventoryScale, 0f);
			uiElement.Top.Set(top * Main.inventoryScale, 0f);
			if (width >= 0) uiElement.Width.Set(width * Main.inventoryScale, 0f);
			if (height >= 0) uiElement.Height.Set(height * Main.inventoryScale, 0f);
		}

	}
	public class DyePanel : UIPanel {
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
				Main.isMouseLeftConsumedByUI = true;
			}
		}
	}
	public class DyeSlot(Item[] items) : UIElement {
		public int Index;
		public override void OnInitialize() {
			Height.Set(52 * Main.inventoryScale, 0);
			Width.Set(52 * Main.inventoryScale, 0);
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
				Main.isMouseLeftConsumedByUI = true;
			}
		}
		public void HandleItemSlot() {
			ItemSlot.MouseHover(items, ItemSlot.Context.EquipDye, Index);
			if (Main.mouseItem.IsAir && items[Index].IsAir) Main.hoverItemName = Language.GetTextValue($"Mods.BasicDyePlus.UI.DyeSlots.{Index}");
			if ((Index != 2 && Main.mouseItem.dye > 0) || Main.mouseItem.IsAir) {
				ItemSlot.LeftClick(items, ItemSlot.Context.InventoryItem, Index);
				ItemSlot.RightClick(items, ItemSlot.Context.InventoryItem, Index);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle dim = GetInnerDimensions();
			ItemSlot.Draw(spriteBatch, items, ItemSlot.Context.InventoryCoin, Index, dim.Position());
			if (ContainsPoint(Main.MouseScreen)) {
				HandleItemSlot();
			}
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
