using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BasicDyePlus.Items.Dyes {
	public class Unmarked_Dye : ModItem {
		public override string Texture => $"Terraria/Images/Item_{ItemID.SilverDye}";
		public Vector3 PrimaryColor = Color.Black.ToVector3();
		public Vector3 SecondaryColor = Color.White.ToVector3();
		public float Saturation = 1f;
		public ModifiedType Modified = ModifiedType.None;
		public enum ModifiedType {
			None = 0,
			Mixed = 1,
			Combo = 2
		}
		public override LocalizedText DisplayName => this.GetLocalization($"{Modified.ToString()}DisplayName".Replace(nameof(ModifiedType.None), string.Empty));
		public override LocalizedText Tooltip => Modified == ModifiedType.None ? base.Tooltip : LocalizedText.Empty;
		public override void SetStaticDefaults() {
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(Main.PixelShaderRef, "ArmorColored"))
				.UseColor(PrimaryColor)
				.UseSecondaryColor(SecondaryColor)
				.UseSaturation(Saturation);
		}
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
			if (Saturation <= 0) Item.value = 0;
		}
		public override bool CanStack(Item source) {
			Unmarked_Dye dye = source.ModItem as Unmarked_Dye;
			return dye.PrimaryColor == PrimaryColor && dye.SecondaryColor == SecondaryColor && dye.Saturation == Saturation;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new(Mod, "colors", $"Primary: {PrimaryColor}, Secondary: {SecondaryColor}, Saturation: {Saturation}"));
		}
	}
}
