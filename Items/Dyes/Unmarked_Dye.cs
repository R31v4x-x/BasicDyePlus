using Humanizer;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BasicDyePlus.Items.Dyes {
	public enum ModifiedType : byte {
		Mixed = 0,
		Combo = 1,
		None = 2,
	}
	public class Unmarked_Dye : ModItem {
		public override string Texture => $"Terraria/Images/Item_{ItemID.SilverDye}";
		public (int type, int Count) PrimaryDye = (ItemID.None, 0);
		public (int type, int Count) SecondaryDye = (ItemID.None, 0);
		public Vector3 PrimaryColor = Color.Black.ToVector3();
		public Vector3 SecondaryColor = Color.White.ToVector3();
		public float Saturation = 1f;
		public ModifiedType Modified = ModifiedType.None;
		public override LocalizedText DisplayName => this.GetLocalization($"{Modified}DisplayName".Replace(nameof(ModifiedType.None), string.Empty));
		public override LocalizedText Tooltip => Modified == ModifiedType.None ? base.Tooltip : LocalizedText.Empty;
		public override void SetStaticDefaults() {
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(Main.PixelShaderRef, "ArmorColored"))
				.UseColor(PrimaryColor)
				.UseSecondaryColor(SecondaryColor)
				.UseSaturation(Saturation);
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.RedandBlackDye);
			Item.dye = dye;
			if (Modified == ModifiedType.None) Item.value = 0;
			else Item.value = (ContentSamples.ItemsByType[PrimaryDye.type].value * PrimaryDye.Count) + (ContentSamples.ItemsByType[SecondaryDye.type].value * SecondaryDye.Count);

		}
		public override bool CanStack(Item source) {
			Unmarked_Dye dye = source.ModItem as Unmarked_Dye;
			return dye.Modified == Modified && dye.PrimaryDye == PrimaryDye && dye.SecondaryDye == SecondaryDye && dye.PrimaryColor == PrimaryColor && dye.SecondaryColor == SecondaryColor && dye.Saturation == Saturation;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (Modified != ModifiedType.None) {
				string GetName((int type, int count) dye) {
					string name = ContentExtensions.GetItemNameFromID(dye.type).Value;
					name = name.Replace(Lang.inter[57].Value, "", StringComparison.OrdinalIgnoreCase).TrimEnd();
					if (dye.count > 1) name += $" ({dye.count})";
					return name;
				}

				tooltips[0].Text = tooltips[0].Text.Replace(
					this.GetLocalizedValue("DisplayName"),
					this.GetLocalizedValue($"{Modified}DisplayName").FormatWith(GetName(PrimaryDye), GetName(SecondaryDye))
				);
				int index = tooltips.FindIndex(tl => tl.FullName == "Terraria/Tooltip0");
				tooltips.RemoveAt(index);
			}
			tooltips.Add(new(Mod, "colors", $"Primary: {PrimaryColor}, Secondary: {SecondaryColor}, Saturation: {Saturation}, Modified: {Modified}"));
		}
		public override void SaveData(TagCompound tag) {
			if (Modified != ModifiedType.None) {
				tag.Set(nameof(Modified), (byte)Modified, true);
				tag.Set(nameof(PrimaryColor), PrimaryColor, true);
				tag.Set(nameof(PrimaryDye) + nameof(PrimaryDye.type), PrimaryDye.type, true);
				tag.Set(nameof(PrimaryDye) + nameof(PrimaryDye.Count), PrimaryDye.Count, true);
				tag.Set(nameof(SecondaryColor), SecondaryColor, true);
				tag.Set(nameof(SecondaryDye) + nameof(SecondaryDye.type), SecondaryDye.type, true);
				tag.Set(nameof(SecondaryDye) + nameof(SecondaryDye.Count), SecondaryDye.Count, true);
				tag.Set(nameof(Saturation), Saturation, true);
			}
		}
		public override void LoadData(TagCompound tag) {
			if (tag.TryGet(nameof(Modified), out byte type)) Modified = (ModifiedType)type;
			else return;
			PrimaryColor = tag.SafeGet(nameof(PrimaryColor), Color.Black.ToVector3());
			PrimaryDye = (tag.SafeGet(nameof(PrimaryDye) + nameof(PrimaryDye.type), (int)ItemID.None), tag.SafeGet(nameof(PrimaryDye) + nameof(PrimaryDye.Count), 0));
			SecondaryColor = tag.SafeGet(nameof(SecondaryColor), Color.White.ToVector3());
			SecondaryDye = (tag.SafeGet(nameof(SecondaryDye) + nameof(SecondaryDye.type), (int)ItemID.None), tag.SafeGet(nameof(SecondaryDye) + nameof(SecondaryDye.Count), 0));
			Saturation = tag.SafeGet(nameof(Saturation), 1f);
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)Modified);
			writer.WriteVector3(PrimaryColor);
			writer.Write(PrimaryDye.type);
			writer.Write(PrimaryDye.Count);
			writer.WriteVector3(SecondaryColor);
			writer.Write(SecondaryDye.type);
			writer.Write(SecondaryDye.Count);
			writer.Write(Saturation);
		}
		public override void NetReceive(BinaryReader reader) {
			Modified = (ModifiedType)reader.Read();
			PrimaryColor = reader.ReadVector3();
			PrimaryDye = (reader.Read(), reader.Read());
			SecondaryColor = reader.ReadVector3();
			SecondaryDye = (reader.Read(), reader.Read());
			Saturation = reader.Read();
		}
	}
}
