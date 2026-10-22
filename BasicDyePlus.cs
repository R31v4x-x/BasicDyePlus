global using Microsoft.Xna.Framework;
global using Color = Microsoft.Xna.Framework.Color;
global using Matrix = Microsoft.Xna.Framework.Matrix;
global using Rectangle = Microsoft.Xna.Framework.Rectangle;
global using Vector2 = Microsoft.Xna.Framework.Vector2;
global using Vector3 = Microsoft.Xna.Framework.Vector3;
global using Vector4 = Microsoft.Xna.Framework.Vector4;
using BasicDyePlus.Items.Dyes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BasicDyePlus {
	public class BasicDyePlus : Mod {

	}
	public class ModifyNPCShops : GlobalNPC {
		public override void ModifyShop(NPCShop shop) {
			switch (shop.NpcType) {
				case NPCID.DyeTrader: {
					shop.Add(ModContent.ItemType<Wecho_Dye>(), Condition.Hardmode);
					break;
				}
			}
		}
	}
}
