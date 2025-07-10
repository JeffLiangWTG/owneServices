using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module
{
	[TestedType(typeof(LoadListModeFilter))]
	sealed class LoadListModeFilterTest : ModuleFilterTestCase<LoadListModeFilter>
	{
		#region Implementation

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override LoadListModeFilter GetNewModuleFilter()
		{
			CodeDescriptionPairList list1 = FreightCodePairLists.LinkableTransportModeList();
			CodeDescriptionPairList list2 = new CodeDescriptionPairList(OLookUpEditType.FreightContainerMode);
			return new LoadListModeFilter("moo", DummyBizoSchema.Z0_Description, DummyBizoSchema.Z0_Description, list1, list2);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.ModesAndTypes; }
		}

		#endregion
	}
}
