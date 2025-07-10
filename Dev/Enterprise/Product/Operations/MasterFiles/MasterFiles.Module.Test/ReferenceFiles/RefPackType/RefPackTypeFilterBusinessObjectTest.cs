using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPackTypeFilterBusinessObject))]
	sealed class RefPackTypeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestUOMType

		public void TestUOMType()
		{
			var packType1 = CreateRefPackType("PT1", UOMPackTypesList.Codes.Case);
			var packType2 = CreateRefPackType("PT2", UOMPackTypesList.Codes.Pallet);
			var packType3 = CreateRefPackType("PT3", UOMPackTypesList.Codes.Pallet);
			var packType4 = CreateRefPackType("PT4", UOMPackTypesList.Codes.Pallet);
			var packType5 = CreateRefPackType("PT5", UOMPackTypesList.Codes.SplitCase);
			var packType6 = CreateRefPackType("PT6", string.Empty);
			Factory.Save();

			var uomFilter = (ModuleTextFilter)PackTypeFilter[RefPackTypeFilterBusinessObject.Descriptions.UOMType];
			var comparators = uomFilter.ComparisonOperator_List.GetAllCodesZString();

			Asserter.AddToScope(packType1, packType2, packType3, packType4, packType5, packType6);
			AssertContainsExactElementsInAnyOrder("Only select comparison operators are available for filter", comparators, new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.IsBlank, ModuleTextFilter.ComparisonConstants.IsNotBlank });

			uomFilter.IsActive = true;
			uomFilter.Property = UOMPackTypesList.Codes.Case;
			uomFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Asserter.AssertMatches("Filter returns matching UOM Types.", uomFilter, packType1);

			uomFilter.Property = "PLT";
			Asserter.AssertMatches("Filter returns matching UOM Types.", uomFilter, packType2, packType3, packType4);

			uomFilter.Property = "SPC";
			Asserter.AssertMatches("Filter returns matching UOM Types.", uomFilter, packType5);

			uomFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("Filter returns blank UOM Types.", uomFilter, packType6);

			uomFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("Filter returns blank UOM Types.", uomFilter, packType1, packType2, packType3, packType4, packType5);
		}

		#endregion

		#region Implementation

		RefPackType CreateRefPackType(string code, string uomType)
		{
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = code;
			packType.F3_UOMType = uomType;
			return packType;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefPackTypeFilterBusinessObject();
		}

		FilterStripAsserter<RefPackType> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<RefPackType>(Factory, pt => pt.F3_Code)); }
		}
		FilterStripAsserter<RefPackType> asserter;

		RefPackTypeFilterBusinessObject PackTypeFilter
		{
			get { return packTypeFilter ?? (packTypeFilter = new RefPackTypeFilterBusinessObject()); }
		}
		RefPackTypeFilterBusinessObject packTypeFilter;

		#endregion
	}
}
