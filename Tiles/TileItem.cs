using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ID.ContentSamples.CreativeHelper;

namespace BasicDyePlus.Tiles {
	[Autoload(false)]
	public class TileItem(ModTile tile, string textureOverride = null, string nameOverride = null) : ModItem() {
		private static readonly Dictionary<ModTile, TileItem> itemsByTile = [];
		[field: CloneByReference]
		public ModTile Tile { get; } = tile;
		public override string Name => nameOverride ?? Tile.Name + "_Item";
		public override string Texture => textureOverride ?? Tile.Texture + "_Item";
		public override LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), Tile.PrettyPrintName);
		[field: CloneByReference] public event Action<Item> ExtraStaticDefaults;
		[field: CloneByReference] public event Action<Item> ExtraDefaults;
		[field: CloneByReference] public event Action<Item> OnAddRecipes;
		[field: CloneByReference] public event ModifySorting OnModifyResearchSorting;
		protected override bool CloneNewInstances => true;
		public sealed override void Load() {
			itemsByTile.Add(Tile, this);
		}

		public override void SetStaticDefaults() {
			if (ExtraStaticDefaults is not null) {
				ExtraStaticDefaults(Item);
				ExtraStaticDefaults = null;
			}
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(Tile.Type);
			Item.width = 14;
			Item.height = 28;
			Item.value = 150;
			ExtraDefaults?.Invoke(Item);
		}
		public override void AddRecipes() {
			if (OnAddRecipes is not null) {
				OnAddRecipes(Item);
				OnAddRecipes = null;
			}
		}
		public delegate void ModifySorting(Item item, ref ItemGroup itemGroup);
		public override void ModifyResearchSorting(ref ItemGroup itemGroup) {
			OnModifyResearchSorting?.Invoke(Item, ref itemGroup);
		}
		public TileItem WithExtraStaticDefaults(Action<Item> extra) {
			ExtraStaticDefaults += extra;
			return this;
		}
		public TileItem WithSet(Func<bool[]> set) => WithSet(set, true);
		public TileItem WithSet<T>(Func<T[]> set, T value) => WithExtraStaticDefaults(item => set()[item.type] = value);
		public TileItem WithExtraDefaults(Action<Item> extra) {
			ExtraDefaults += extra;
			return this;
		}
		public TileItem WithOnAddRecipes(Action<Item> recipes) {
			OnAddRecipes += recipes;
			return this;
		}
		public TileItem WithResearchSorting(ItemGroup itemGroup) => WithModifyResearchSorting((item, ref group) => group = itemGroup);
		public TileItem WithModifyResearchSorting(ModifySorting sorting) {
			OnModifyResearchSorting += sorting;
			return this;
		}
		public TileItem RegisterItem() {
			Tile.Mod.AddContent(this);
			return this;
		}

		public static TileItem Get(ModTile tile) {
			return itemsByTile.TryGetValue(tile, out TileItem item1) ? item1 : null;
		}
		public static TileItem Get<TTile>() where TTile : ModTile => Get(ModContent.GetInstance<TTile>());
		public static int ItemType<TTile>() where TTile : ModTile => Get<TTile>().Type;
	}
}
