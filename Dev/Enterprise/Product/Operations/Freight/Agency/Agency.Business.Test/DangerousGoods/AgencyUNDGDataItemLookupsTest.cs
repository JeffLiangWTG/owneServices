using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyUNDGDataItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeOfUNDGSubstanceCollection()
		{
			var lookups = new AgencyUNDGDataItemLookups(Factory.New<AgencyUNDGDataItem>());
			var collection = lookups.UNDGSubstances;

			AssertType<AgencyUNDGSubstanceCollection>(collection);
		}

		public void TestAgencyUNDGDataItem_DefaultFilterShouldBeCFR_WhenOriginIsUS()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";

			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);
			var agencyUNDGDataItem = collection.AddNew();

			AssertEquals(1, agencyUNDGDataItem.Lookups.UNDGSubstances.FilterBusinessObjectDefaults.Count);
			var defaultFilter = agencyUNDGDataItem.Lookups.UNDGSubstances.FilterBusinessObjectDefaults["Standard:Property"];
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR, defaultFilter.Value);
		}

		public void TestAgencyUNDGDataItem_DefaultFilterShouldBeCFR_WhenTransportIsFromCanadaToUS()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "CAVAC";
			shipment.JS_RL_NKDestination = "USLAX";

			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);
			var agencyUNDGDataItem = collection.AddNew();

			AssertEquals(1, agencyUNDGDataItem.Lookups.UNDGSubstances.FilterBusinessObjectDefaults.Count);
			var defaultFilter = agencyUNDGDataItem.Lookups.UNDGSubstances.FilterBusinessObjectDefaults["Standard:Property"];
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR, defaultFilter.Value);
		}

		public void TestAgencyUNDGDataItem_DefaultFilterShouldBeIMO_WhenOriginIsNotUSorCanada()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "DEHAM";

			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);
			var agencyUNDGDataItem = collection.AddNew();

			AssertEquals(1, agencyUNDGDataItem.Lookups.UNDGSubstances.FilterBusinessObjectDefaults.Count);
			var defaultFilter = agencyUNDGDataItem.Lookups.UNDGSubstances.FilterBusinessObjectDefaults["Standard:Property"];
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, defaultFilter.Value);
		}

		public void TestAgencyUNDGDataItemUNDGSubstances_AdditionalFilter_ShouldContainsIMOAndCFRSubstances()
		{
			var shipment = Factory.New<AgencyShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);
			var agencyUNDGDataItem = collection.AddNew();

			var cfrQuery = new ZQuery(UNDGSubstanceSchema.DG_Standard, SQLComparisonOperator.Equal, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR);
			var filterQuery = new ZQuery(UNDGSubstanceSchema.DG_Standard, SQLComparisonOperator.Equal, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			filterQuery.AddToFilter(cfrQuery, JoinCondition.Or);

			AssertEquals(filterQuery, agencyUNDGDataItem.Lookups.UNDGSubstances.AdditionalFilter);
		}
	}
}
