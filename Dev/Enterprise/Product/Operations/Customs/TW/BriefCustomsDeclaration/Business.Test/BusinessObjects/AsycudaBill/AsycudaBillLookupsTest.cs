using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackageTypeList()
		{
			var factory = Factory;
			var bill = factory.New<AsycudaManifestHeader>().Bills.AddNew();
			AssertEquals(factory.GetCachedValue<RefCusPackListProvider>().GetCIPCustomsPackList(factory, string.Empty).CodesAsString, bill.Lookups.PackageTypeList.CodesAsString);
		}

		public void TestLocations()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "AA";
			header.AMA_TransportMode = "TT";
			var bill = header.Bills.AddNew();
			var lookups = new AsycudaBillLookups(bill);
			var list = lookups.Locations;
			AssertType<ZZRefCusCodeListCombinedCollection>("Type", list);

			var collection = list as ZZRefCusCodeListCombinedCollection;
			CombineAssertions(() =>
			{
				AssertEquals("Attribute Name:Property", "CUSTOMSOFFICE", collection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
				AssertEquals("Attribute Value:Property", "AA", collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
				AssertEquals("List Type:Property", "FAC", collection.FilterBusinessObjectDefaults["List Type:Property"].Value);
				AssertEquals("Effective Date:Property1", ZDateTime.Today, collection.FilterBusinessObjectDefaults["Effective Date:Property1"].Value);
				AssertContainsExactElementsInExactOrder("Country/Region or Grouping", new[] { "TW" }, collection.DataGroupingCodes);
			});
		}

		public void TestProcedures()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertSame(Factory.GetCachedValue<ProcedureList>(), bill.Lookups.Procedures);
		}

		public void TestIncotermList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertSame(Factory.GetCachedValue<BriefCustomsDeclarationIncotermList>(), bill.Lookups.IncotermList);
		}

		public void TestShipperBondedIDTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.ShipperBondedIDTypeList;
			AssertContainsExactElementsInAnyOrder(OrgHeaderHelper.BCDExporterBondedIDCodeTypes, list.GetAllCodes());
			AssertSame("cached", list, bill.Lookups.ShipperBondedIDTypeList);
		}
	}
}
