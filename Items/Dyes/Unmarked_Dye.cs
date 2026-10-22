using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace BasicDyePlus.Items.Dyes {
	public class Unmarked_Dye : ModItem {
		public override string Texture => $"Terraria/Images/Item_{ItemID.SilverDye}";
		public Vector3 PrimaryColor = Color.Black.ToVector3();
		public Vector3 SecondaryColor = Color.White.ToVector3();
		public float Saturation = 1f;
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
	}
}
