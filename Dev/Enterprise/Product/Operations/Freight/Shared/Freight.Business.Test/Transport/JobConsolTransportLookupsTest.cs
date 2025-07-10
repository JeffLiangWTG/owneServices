using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobConsolTransportLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCarriers()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			AssertType<ShippingProviderCollection>(transport.Lookups.Carriers);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertType<AirShippingProviderCollection>(transport.Lookups.Carriers);

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertType<SeaShippingProviderCollection>(transport.Lookups.Carriers);

			transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			AssertType<SeaShippingProviderCollection>(transport.Lookups.Carriers);

			transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			AssertType<TransportScheduleRailShippingProviderCollection>(transport.Lookups.Carriers);

			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			AssertType<TransportScheduleLineHaulShippingProviderCollection>(transport.Lookups.Carriers);
		}

		public void TestCreditors()
		{
			var nonCreditor = Factory.NewWithValidTestData<OrgHeader>();
			var creditorInCurrentCompany = Factory.NewWithValidTestData<OrgHeader>();
			var creditorInOtherCompany = Factory.NewWithValidTestData<OrgHeader>();

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var currentOrgCompanyData = creditorInCurrentCompany.CompanyDataCollection.AddNew();
			currentOrgCompanyData.OB_IsCreditor = true;
			currentOrgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			var otherOrgCompanyData = creditorInOtherCompany.CompanyDataCollection.AddNew();
			otherOrgCompanyData.OB_IsCreditor = true;
			otherOrgCompanyData.OB_GC = otherCompany.PK;

			Factory.Save();

			AssertNotEquals("Precondition: must have a current company", ZGuid.Empty, GlbCompany.CurrentCompany.PK);

			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			var creditorList = transport.Lookups.CreditorList;
			creditorList.Load();
			Assert("collection has creditor in current company", creditorList.Contains(creditorInCurrentCompany.PK));
			Assert("collection has creditor in other company", creditorList.Contains(creditorInOtherCompany.PK));
			Assert("collection does not have non-creditor", !creditorList.Contains(nonCreditor.PK));
		}
	}
}
