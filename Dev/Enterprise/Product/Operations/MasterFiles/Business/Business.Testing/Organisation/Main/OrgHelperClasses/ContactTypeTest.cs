using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ContactTypeTest : TestCase
	{
		public void TestContactType()
		{
			AssertEquals("Administration", ContactType.Administration, ContactType.Find("ADM"));
			Assert("Miscellaneous", ContactType.Miscellaneous == "MSC");
		}

		public void TestImplicitOpStringDoesNotThrowException()
		{
			AssertNull((string)((ContactType)null));
		}

		public void TestFindAggregateTypes()
		{
			AssertEquals("Air Wholesaler Type Count", 1, ContactType.FindRelatedAggregateTypes(ContactType.AirWholesaler).Length);
			AssertEquals("Export Forwarder Aggregate Type Count", 7, ContactType.FindRelatedAggregateTypes(ContactType.ExportFreightAgent).Length);
			AssertEquals("Export Forwarder Aggregate Type Count", 7, ContactType.FindRelatedAggregateTypes(ContactType.ExportAirFreightAgent).Length);
			AssertEquals("Depot Aggregate Type Count", 7, ContactType.FindRelatedAggregateTypes(ContactType.Depot).Length);
		}

		public void TestBrandingType()
		{
			AssertEquals("Contact type All - no Brand type", ContactBrandingType.Client, ContactType.All.BrandingType);
			AssertEquals("Contact type A/R - Brand type Client", ContactBrandingType.Client, ContactType.Receivables.BrandingType);
			AssertEquals("Contact type FWD - Brand type Agent", ContactBrandingType.Agent, ContactType.FreightAgent.BrandingType);
			AssertEquals("Contact Type ExportFreightAgent - FWE", ContactBrandingType.Agent, ContactType.ExportFreightAgent.BrandingType);
			AssertEquals("Contact Type ImportFreightAgent - FWI", ContactBrandingType.Agent, ContactType.ImportFreightAgent.BrandingType);
			AssertEquals("Contact Type NotifyParty - NOT", ContactBrandingType.Client, ContactType.NotifyParty.BrandingType);
			AssertEquals("Contact Type Miscellaneous - MSC", ContactBrandingType.Client, ContactType.Miscellaneous.BrandingType);
			AssertEquals("Contact Type NoContactType - NCT", ContactBrandingType.Client, ContactType.NoContactType.BrandingType);
			AssertEquals("Contact Type NoContactType - LOC", ContactBrandingType.Client, ContactType.LocalClient.BrandingType);
			AssertEquals("Contact Type ExportBroker - BRE", ContactBrandingType.Agent, ContactType.ExportBroker.BrandingType);
			AssertEquals("Contact Type ImportBroker - BRI", ContactBrandingType.Agent, ContactType.ImportBroker.BrandingType);
			AssertEquals("Contact Type CommissionAgreementRecipient - CAR", ContactBrandingType.Client, ContactType.CommissionAgreementRecipient.BrandingType);
			AssertEquals("Contact Type TransitWarehouse - TWH", ContactBrandingType.Agent, ContactType.TransitWarehouse.BrandingType);
			AssertEquals("Contact Type TransitWarehouse - VGM", ContactBrandingType.Agent, ContactType.VerifiedGrossWeightContact.BrandingType);
			AssertEquals("Contact Type Principal - PRI", ContactBrandingType.Client, ContactType.Principal.BrandingType);
		}

		public void TestMultlingual()
		{
			AssertEquals("The Accounts Payable Manager", ContactType.Receivables.DefaultName);
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(delegate(string key)
				{ return new ResourceStringData(key, ":)"); });
				AssertEquals(":)", ContactType.Receivables.DefaultName);
			}
		}
	}
}
