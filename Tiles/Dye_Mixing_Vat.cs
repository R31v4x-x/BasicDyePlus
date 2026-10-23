using BasicDyePlus.UI;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace BasicDyePlus.Tiles {
	public class Dye_Mixing_Vat : ModTile {
		public override string Texture => $"Terraria/Images/Tiles_{TileID.DyeVat}";
		public override void Load() {
			new TileItem(this, $"Terraria/Images/Item_{ItemID.DyeVat}")
			.WithExtraStaticDefaults(item => item.ResearchUnlockCount = 1)
			.WithExtraDefaults(item => item.value = Item.sellPrice(silver: 10))
			.WithOnAddRecipes(item => Recipe.Create(item.type)
				.AddIngredient(ItemID.DyeVat)
				.AddIngredient(ItemID.Paintbrush)
				.AddTile(TileID.WorkBenches)
				.Register()
			).RegisterItem();
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			//TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.DyeVat, 0));
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(144, 148, 144), TileItem.Get<Dye_Mixing_Vat>().DisplayName);
			DustType = DustID.WoodFurniture;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override bool RightClick(int i, int j) {
			Dye_Mixing_Vat_UI_System sys = GetInstance<Dye_Mixing_Vat_UI_System>();
			Main.playerInventory = true;
			if (sys.dyeMixingInterface.CurrentState is Dye_Mixing_UI) {
				sys.HideMixingUI();
				SoundEngine.PlaySound(SoundID.MenuClose);
			} else {
				sys.ShowMixingUI();
				SoundEngine.PlaySound(SoundID.MenuOpen);
			}
			return true;
		}
	}
}
