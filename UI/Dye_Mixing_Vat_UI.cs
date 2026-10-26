using BasicDyePlus.Items.Dyes;
using BasicDyePlus.Tiles;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;
using static BasicDyePlus.BasicDyePlusExtension;

namespace BasicDyePlus.UI {
	public class Dye_Mixing_Vat_UI_System : ModSystem {
		public UserInterface dyeMixingInterface;
		public Dye_Mixing_UI dyeMixingUI;

		public void ShowMixingUI() {
			dyeMixingInterface?.SetState(dyeMixingUI = new Dye_Mixing_UI());
			dyeMixingUI.Activate();
		}
		public void HideMixingUI() {
			foreach (Item item in dyeMixingUI.items) {
				if (!item.IsAir) Main.LocalPlayer.QuickSpawnItem(item.GetSource_DropAsItem(), item, item.stack);
			}
			dyeMixingInterface?.SetState(null);
		}
		public override void PostSetupContent() {
			dyeMixingInterface = new UserInterface();
		}
		public override void UpdateUI(GameTime gameTime) {
			if (dyeMixingInterface?.CurrentState != null) {
				dyeMixingInterface?.Update(gameTime);
			}
		}
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
	public enum CombineMode {
		Mixing = (byte)0,
		Combo = (byte)1,
	}
	public class Dye_Mixing_UI : UIState {
		public DyeSlot[] DyeSlot = new DyeSlot[3];
		public Item[] items = new Item[3];
		public CombineMode Mode { get; set; }
		public override void OnInitialize() {
			#region
			for (int i = 0; i < items.Length; i++) {
				items[i] ??= new();
			}

			UIPanel panel = new DyePanel();
			panel.SetPadding(8);
			panel.SetRectangle(715.60614f, 269.3318f, (52 * 2) + 48, 173);

			LocalizedText text = TileItem.Get<Dye_Mixing_Vat>().DisplayName;
			Vector2 padding = ChatManager.GetStringSize(FontAssets.MouseText.Value, text.Value, new Vector2(1.2f, 1) * Main.inventoryScale) + new Vector2(0, 4);
			UIText label = new(text, 1.2f * Main.inventoryScale);
			label.SetLeft(0).SetTop(0);
			label.IgnoresMouseInteraction = true;
			panel.Append(label);

			//panel.SetWidth(/*Math.Max(ChatManager.GetStringSize(FontAssets.MouseText.Value, text.Value, new Vector2(1.2f, 0)).X + 8, */(52 * 2) + 48);

			for (int i = 0; i < DyeSlot.Length; i++) {
				DyeSlot[i] = new DyeSlot(items) { Index = i };
				DyeSlot[i].Initialize();
			}
			DyeSlot[0].SetLeft(-28, 0.5f).SetTop(padding.Y);
			DyeSlot[1].SetLeft(28, 0.5f).SetTop(padding.Y);
			DyeSlot[2].SetLeft(-28, 0.5f).SetTop(padding.Y + 52 + 12);
			panel.Append(DyeSlot[0]);
			panel.Append(DyeSlot[1]);
			panel.Append(DyeSlot[2]);

			DyeCombine combine = new(items, Mode);
			combine.SetLeft(28, 0.5f).SetTop(padding.Y + 52 + 12).SetWidth(52).SetHeight(26);
			panel.Append(combine);
			#endregion
			DyeSwitch @switch = new(this);
			@switch.SetLeft(28, 0.5f).SetTop(padding.Y + 52 + 12 + 26).SetWidth(52).SetHeight(26);
			panel.Append(@switch);

			Append(panel);
		}
		public override void Update(GameTime gameTime) {
			if (!Main.playerInventory) {
				ModContent.GetInstance<Dye_Mixing_Vat_UI_System>().HideMixingUI();
				return;
			}

			base.Update(gameTime);
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
			this.SetWidth(52).SetHeight(52);
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
			if (Main.mouseItem.IsAir && items[Index].IsAir) Main.hoverItemName = this.UILocalization(Index).Value;
			if ((Index != 2 && Main.mouseItem.dye > 0) || Main.mouseItem.IsAir) {
				ItemSlot.LeftClick(items, ItemSlot.Context.InventoryItem, Index);
				ItemSlot.RightClick(items, ItemSlot.Context.InventoryItem, Index);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle dim = GetInnerDimensions();
			ItemSlot.Draw(spriteBatch, items, Index == 2 ? ItemSlot.Context.ChestItem : ItemSlot.Context.InventoryCoin, Index, dim.Position());
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
	public class DyeCombine(Item[] items, CombineMode mode) : UIButton<string>(string.Empty, 1.2f * Main.inventoryScale) {
		public override void OnInitialize() {
			HoverSound = SoundID.MenuTick;
			ClickSound = SoundID.ResearchComplete;
			HoverText = this.UILocalization($"{mode}.Tooltip").Format(UILocalization("DyeSlot.0"), UILocalization("DyeSlot.1"));
			TooltipText = true;
			SetText(this.UILocalization($"{mode}.DisplayName").Value);
		}
		public override void LeftClick(UIMouseEvent evt) {
			bool fail = false;
			if (ContainsPoint(Main.MouseScreen)) {
				if (items[2].IsAir && !items[0].IsAir && !items[1].IsAir) {
					(int, int) GetItem(Item item) {
						return (item.type, item.stack);
					}
					items[2].SetDefaults(ModContent.ItemType<Unmarked_Dye>());
					items[2].newAndShiny = true;
					items[2].value = (ContentSamples.ItemsByType[items[0].type].value * items[0].stack) + (ContentSamples.ItemsByType[items[1].type].value * items[1].stack);
					Unmarked_Dye dye = items[2].ModItem as Unmarked_Dye;
					dye.PrimaryDye = GetItem(items[0]);
					dye.SecondaryDye = GetItem(items[1]);
					dye.PrimaryColor = Color.Red.ToVector3();
					dye.SecondaryColor = Color.Blue.ToVector3();
					dye.Modified = (ModifiedType)mode;
					items[0].SetDefaults();
					items[1].SetDefaults();
				} else fail = true;
			}

			if (fail) ClickSound = SoundID.MenuTick;
			else ClickSound = SoundID.ResearchComplete;
			base.LeftClick(evt);
		}
	}
	public class DyeSwitch(Dye_Mixing_UI ui) : UIButton<string>(string.Empty, 1.2f * Main.inventoryScale) {
		public override void OnInitialize() {
			HoverSound = SoundID.MenuTick;
			ClickSound = SoundID.MenuTick;
			HoverText = this.UILocalization("Tooltip").Value;
			TooltipText = true;
			SetText(this.UILocalization("DisplayName").Value);
		}
		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);

			if (ContainsPoint(Main.MouseScreen)) {
				ui.Mode ^= CombineMode.Combo;
			}
		}
	}
}
