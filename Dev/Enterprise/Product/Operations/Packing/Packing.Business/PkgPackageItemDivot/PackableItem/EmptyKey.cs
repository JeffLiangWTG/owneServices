using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Packing.Business
{
	[Immutable]
	sealed class EmptyKey : GroupingKey
	{
		EmptyKey()
		{
		}

		public override int GetHashCode() => -1;

		protected override bool Equals(GroupingKey other) => other is EmptyKey;

		public static readonly GroupingKey Instance = new EmptyKey();

		protected override bool IsSimilarItem_DoNotUseCore(GroupingKey other) => other is EmptyKey;
	}
}
