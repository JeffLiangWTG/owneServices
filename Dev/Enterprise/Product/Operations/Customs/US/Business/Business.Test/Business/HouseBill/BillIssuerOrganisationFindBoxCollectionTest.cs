using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(BillIssuerOrganisationFindBoxCollection))]
	sealed class BillIssuerOrganisationFindBoxCollectionTest : OrganisationsFindBoxCollectionTest
	{
		public void TestFilterBusinessObjectDefault()
		{
			var collection = new BillIssuerOrganisationFindBoxCollection(Factory);
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property4"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property6"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "OrJoinCondition"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "AndJoinCondition"));
		}

		public void TestAdditionalFilterForBillIssuer()
		{
			var airLine = Factory.New<OrgHeader>();
			airLine.OH_IsAirLine = true;
			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_IsForwarder = true;
			var localTransporter = Factory.New<OrgHeader>();
			localTransporter.OH_IsLocalTransport = true;
			var railProvider = Factory.New<OrgHeader>();
			railProvider.OH_IsRailProvider = true;
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;
			var shippingProvider = Factory.New<OrgHeader>();
			shippingProvider.OH_IsShippingProvider = true;
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = true;
			var collection = new BillIssuerOrganisationFindBoxCollection(Factory);
			collection.Load();
			AssertEquals("collection has AirLine", true, collection.Contains(airLine));
			AssertEquals("collection has Forwarder", true, collection.Contains(forwarder));
			AssertEquals("collection has LocalTransport", true, collection.Contains(localTransporter));
			AssertEquals("collection has RailProvider", true, collection.Contains(railProvider));
			AssertEquals("collection has ShippingLine", true, collection.Contains(shippingLine));
			AssertEquals("collection has ShippingProvider", true, collection.Contains(shippingProvider));
			AssertEquals("collection does not have Consignee", false, collection.Contains(consignee));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new BillIssuerOrganisationFindBoxCollection(Factory);
	}
}
