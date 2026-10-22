using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace BasicDyePlus.Items.Dyes {
	public class Wecho_Dye : ModItem {
		public float Transparency { get; set => MathHelper.Clamp(field, 0f, 1f); } = 0.5f;
		public override void SetStaticDefaults() {
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(ModContent.Request<Effect>("BasicDyePlus/Effects/WechoDye"), "WechoDye"))
				.UseOpacity(Transparency);
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
		}
		public override bool CanStack(Item source) => true;
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			Transparency = 0.5f;
		}
		public override void UpdateInventory(Player player) {
			Transparency = 0.5f;
		}
	}
}
