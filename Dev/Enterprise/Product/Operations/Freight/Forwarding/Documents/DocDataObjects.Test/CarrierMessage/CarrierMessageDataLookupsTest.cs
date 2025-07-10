using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class CarrierMessageDataLookupsTest : TestCaseWithFactory
	{
		public void TestBOLDocumentationProviderList()
		{
			var list = Lookups.EBLProviderList;

			AssertNotNull("Lookup is not null", list);
			AssertEquals("Lookup is cached", list, Lookups.EBLProviderList);

			var expectedList = new List<string> { EBLProviderConstants.Codes.NotListed, "TS1", "TS3" };

			AssertEquals(3, Lookups.EBLProviderList.Count);
			AssertContainsExactElementsInAnyOrder(expectedList, list.ToArray().Select(x => x.Code));
		}

		public void TestBRWoodenPackageProcessTypes()
		{
			var list = Lookups.BRWoodenPackageProcessTypes;

			AssertNotNull("Lookup is not null", list);
			AssertEquals("Lookup is cached", list, Lookups.BRWoodenPackageProcessTypes);

			var expectedList = new List<string>
			{
				DocDataConstants.WoodenPackageProcessTypes.NotApplicable,
				DocDataConstants.WoodenPackageProcessTypes.NotTreatedAndNotCertified,
				DocDataConstants.WoodenPackageProcessTypes.Processed,
				DocDataConstants.WoodenPackageProcessTypes.TreatedAndCertified
			};

			AssertEquals(4, Lookups.BRWoodenPackageProcessTypes.Count);
			AssertContainsExactElementsInAnyOrder(expectedList, list.ToArray().Select(x => x.Code));
		}

		#region Lookups

		CarrierMessageDataLookups Lookups => lookups ?? (lookups = new CarrierMessageDataLookups(Org));
		CarrierMessageDataLookups lookups;

		OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					var shippingLine = Factory.New<RefShippingLine>();

					var eblProvider1 = shippingLine.ShippingLineEBLProviders.AddNew();
					eblProvider1.RSE_IsAvailable = true;
					eblProvider1.RSE_Name = "TS1";

					var eblProvider2 = shippingLine.ShippingLineEBLProviders.AddNew();
					eblProvider2.RSE_IsAvailable = false;
					eblProvider2.RSE_Name = "TS2";

					var eblProvider3 = shippingLine.ShippingLineEBLProviders.AddNew();
					eblProvider3.RSE_IsAvailable = true;
					eblProvider3.RSE_Name = "TS3";

					org = Factory.New<OrgHeader>();
					org.OH_RSL_ShippingLine = shippingLine.PK;
				}

				return org;
			}
		}
		OrgHeader org;

		#endregion
	}
}
