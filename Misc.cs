using PegasusLib;
using System.Diagnostics.Contracts;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace BasicDyePlus {
	public static class BasicDyePlusExtension {
		public static void WriteVector3(this BinaryWriter bb, Vector3 v) {
			bb.Write(v.X);
			bb.Write(v.Y);
			bb.Write(v.Z);
		}
		public static Vector3 ReadVector3(this BinaryReader br) => new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		public static LocalizedText UILocalization(object suffix, UIElement element = null) {
			string key = "UI.";
			if (element is not null) key += element.GetType().Name + ".";
			key += suffix.ToString();
			return BasicDyePlus.instance.GetLocalization(key);
		}
		public static LocalizedText UILocalization(this UIElement element, object suffix) => UILocalization(suffix, element);
		/// <summary>
		/// not really pure, but it's important that this is always observed by a <see langword="using"/>, and this provides a warning if it's not observed at all
		/// </summary>
		[Pure]
		public static ScopedOverride<T> ScopedOverride<T>(ref this T variable, T value) where T : struct => new(ref variable, value);
	}
	public static class ContentExtensions {
		/// <inheritdoc cref="Lang.GetItemName(int)"/>
		public static LocalizedText GetItemName(this Item item) => GetItemNameFromID(item.type);
		/// <inheritdoc cref="Lang.GetItemName(int)"/>
		public static LocalizedText GetItemNameFromID(int type) {
			return ModContent.GetModItem(type)?.DisplayName ?? Lang.GetItemName(type);
		}
		/// <inheritdoc cref="StyleDimension.Set(float, float)"/>
		public static UIElement SetRectangle(this UIElement uiElement, float left, float top, float width = -1, float height = -1) {
			uiElement.SetLeft(left).SetTop(top);
			if (width >= 0) uiElement.SetWidth(width);
			if (height >= 0) uiElement.SetHeight(height);
			return uiElement;
		}
		/// <inheritdoc cref="StyleDimension.Set(float, float)"/>
		public static UIElement SetLeft(this UIElement uiElement, float leftPx, float leftPer = 0) {
			uiElement.Left.Set(leftPx * Main.inventoryScale, leftPer);
			return uiElement;
		}
		/// <inheritdoc cref="StyleDimension.Set(float, float)"/>
		public static UIElement SetTop(this UIElement uiElement, float topPx, float rightPer = 0) {
			uiElement.Top.Set(topPx * Main.inventoryScale, rightPer);
			return uiElement;
		}
		/// <inheritdoc cref="StyleDimension.Set(float, float)"/>
		public static UIElement SetWidth(this UIElement uiElement, float widthPx, float widthPer = 0) {
			uiElement.Width.Set(widthPx * Main.inventoryScale, widthPer);
			return uiElement;
		}
		/// <inheritdoc cref="StyleDimension.Set(float, float)"/>
		public static UIElement SetHeight(this UIElement uiElement, float heightPx, float heightPer = 0) {
			uiElement.Height.Set(heightPx * Main.inventoryScale, heightPer);
			return uiElement;
		}
	}
}
