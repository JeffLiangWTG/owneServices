using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgPartRelationLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestCartonGroups

		public void TestCartonGroups()
		{
			AssertType(ObjectFactory.GetType<IWhsCartonGroupCollection>(), PartRelation.Lookups.CartonGroups);
		}

		#endregion

		#region RFAttributeConfirmList

		public void TestRFAttributeConfirmList_SchemaRedesignEnabled()
		{
			var expectedList = new RFAttributeConfirmCode();

			foreach (CodeDescriptionPair expectedItem in expectedList)
			{
				AssertCollectionContains(expectedItem, PartRelation.Lookups.RFAttributeConfirmList);
			}

			foreach (CodeDescriptionPair item in PartRelation.Lookups.RFAttributeConfirmList)
			{
				AssertCollectionContains(item, expectedList);
			}
		}

		#endregion

		#region TestPickModeList

		public void TestPickModeList()
		{
			AssertEquals(2, PartRelation.Lookups.PickModeList.Count);
			AssertEquals(true, PartRelation.Lookups.PickModeList.ContainsCode(WhsPickMode.Codes.AttributeSpecified));
			AssertEquals(true, PartRelation.Lookups.PickModeList.ContainsCode(WhsPickMode.Codes.AttributeNeutral));
		}

		#endregion

		#region TestJulianBatchNumberFormatsList

		public void TestJulianBatchNumberFormatsList()
		{
			AssertEquals(4, PartRelation.Lookups.JulianBatchNumberFormatsList.Count);
			AssertEquals(true, PartRelation.Lookups.JulianBatchNumberFormatsList.ContainsCode("#JJJJ"));
			AssertEquals(true, PartRelation.Lookups.JulianBatchNumberFormatsList.ContainsCode("#JJJJJ"));
			AssertEquals(true, PartRelation.Lookups.JulianBatchNumberFormatsList.ContainsCode("JJJJ#"));
			AssertEquals(true, PartRelation.Lookups.JulianBatchNumberFormatsList.ContainsCode("JJJJJ#"));
		}

		#endregion

		#region TestHoldCodes

		public void TestHoldCodes()
		{
			AssertType(ObjectFactory.GetType<IWhsInventoryHeldCodeCollection>(), PartRelation.Lookups.HoldCodes);
		}

		#endregion

		#region implementation

		OrgPartRelation fpartRelation;

		OrgPartRelation PartRelation
		{
			get { return fpartRelation ?? (fpartRelation = Factory.New<OrgPartRelation>()); }
		}

		#endregion
	}
}
