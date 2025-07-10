using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVBookingHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBookedBys()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var lookups = bookingHeader.Lookups;
			AssertEquals(0, lookups.BookedBys.Count);

			var org1 = Factory.New<OrgHeader>();
			org1.Contacts.AddNew().OC_ContactName = "Glen";
			org1.Contacts.AddNew().OC_ContactName = "Bill";

			var org2 = Factory.New<OrgHeader>();
			org2.Contacts.AddNew().OC_ContactName = "Steve";
			org2.Contacts.AddNew().OC_ContactName = "Barry Wong";

			bookingHeader.HVH_OA_BillToParty = org1.MainAddress.PK;
			AssertContainsExactElementsInAnyOrder(new[] { "Glen", "Bill" }, lookups.BookedBys.Cast<OrgContact>().Select(x => x.OC_ContactName));

			bookingHeader.HVH_OA_BillToParty = org2.MainAddress.PK;
			AssertContainsExactElementsInAnyOrder(new[] { "Steve", "Barry Wong" }, lookups.BookedBys.Cast<OrgContact>().Select(x => x.OC_ContactName));
		}

		public void TestOrganisationCollectionTypes()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			CombineAssertions("Organisation Lookup lists should have correct types for filter defaults", () =>
			{
				AssertType("Dispatch Address", typeof(OrganisationsFindBoxCollection), header.Lookups.OrgList);
				AssertType("Bill To Party Lookup", typeof(DebtorCollection), header.Lookups.DebtorOrgList);
				AssertType("Origin Depot Lookup", typeof(PackDepotCollection), header.Lookups.PackDepotOrgList);
			});
		}

		public void TestDeniedPartyScreeningStatusList()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var list = bookingHeader.Lookups.DeniedPartyScreeningStatusList;
			AssertContainsExactElementsInAnyOrder(new[] { "BLK", "CLR", "JCL", "MAT", "NDS", "NOT", "CLP", "REL", "REQ", "UNK", "CAN", "JCE", "JBE" }, list.GetAllCodes());
		}
	}
}
