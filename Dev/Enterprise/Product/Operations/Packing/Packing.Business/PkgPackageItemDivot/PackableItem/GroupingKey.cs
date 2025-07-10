using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Packing.Business
{
	[Immutable]
	public abstract class GroupingKey
	{
		protected GroupingKey()
		{
		}

		public sealed override bool Equals(object obj) => this == obj as GroupingKey;

		public static bool operator ==(GroupingKey x, GroupingKey y) => object.ReferenceEquals(x, y) || (x?.Equals(y) ?? false);
		public static bool operator !=(GroupingKey x, GroupingKey y) => !(x == y);

		protected abstract bool Equals(GroupingKey other);
		public abstract override int GetHashCode();

		/// <summary>
		/// This is temporary dodgy work around. It should be removed in WI00128061 
		/// We need it because Release lines in Whs are not grouping all similar packable items properly (cross order line). 
		/// Do *NOT* use this property in the mean time for any new functionality.
		/// </summary>
		public bool IsSimilarItem_DoNotUse(GroupingKey otherKey)
		{
			return IsSimilarItem_DoNotUseCore(otherKey);
		}

		protected abstract bool IsSimilarItem_DoNotUseCore(GroupingKey otherKey);

		public GroupingKey KeyForAutoPack => KeyForAutoPackCore;

		protected virtual GroupingKey KeyForAutoPackCore => this;
	}
}
