using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Shaders;

namespace BasicDyePlus.Core {
	public class ExpandableSet<T>(Func<ArmorShaderData, T> initializer) {
		public List<T> Contents = [];
		static readonly FastFieldInfo<ArmorShaderDataSet, List<ArmorShaderData>> _shaderData = "_shaderData";
		public List<ArmorShaderData> armorShaders = _shaderData.GetValue(GameShaders.Armor);

		private void EnsureCapacity(int size, bool initLast = true) {
			if (size > armorShaders.Count) throw new IndexOutOfRangeException();
			while (Contents.Count < size) {
				Contents.Add(initLast || Contents.Count + 1 < size ? initializer(armorShaders[Contents.Count]) : default);
			}
		}
		public T this[int index] {
			get {
				EnsureCapacity(index + 1);
				return Contents[index];
			}
			set {
				EnsureCapacity(index + 1, false);
				Contents[index] = value;
			}
		}
	}
}
