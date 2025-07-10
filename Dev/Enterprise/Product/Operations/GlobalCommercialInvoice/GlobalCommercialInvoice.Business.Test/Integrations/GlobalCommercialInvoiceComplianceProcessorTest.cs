using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Freight.Integration.IGlobalCommercialInvoiceComplianceProcessor;
using static Enterprise.GlobalCommercialInvoice.Integration.Constants;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(GlobalCommercialInvoiceComplianceProcessor))]
	public class GlobalCommercialInvoiceComplianceProcessorTest : TestCaseWithFactory
	{
		public void TestInterfaceImplementation()
		{
			Assert("Class <GlobalCommercialInvoiceBusinessObject> implements <IGlobalCommercialInvoiceComplianceProvider> interface", typeof(IGlobalCommercialInvoiceComplianceProvider).IsAssignableFrom(typeof(GlobalCommercialInvoiceBusinessObject)));
		}

		public void TestComplianceGetParties()
		{
			var orgBuyer = Factory.NewWithValidTestData<OrgHeader>();
			orgBuyer.OH_Code = "Importer";

			var orgSupplier = Factory.NewWithValidTestData<OrgHeader>();
			orgSupplier.OH_Code = "Supplier";

			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var invoice = Factory.CreateInvoiceHeader(shipment);
			invoice.GIH_OH_Importer = orgBuyer.PK;
			invoice.GIH_OH_Supplier = orgSupplier.PK;

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var parties = ((IGlobalCommercialInvoiceProvider)shipment).DataProvider.Parties;
				AssertEquals(2, parties.Count());
				Assert(parties.Any(p => p.Description == Compliance.Parties.ImporterDescription && p.Party == orgBuyer));
				Assert(parties.Any(p => p.Description == Compliance.Parties.SupplierDescription && p.Party == orgSupplier));
			}

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(false)))
			{
				var parties = ((IGlobalCommercialInvoiceProvider)shipment).DataProvider.Parties;
				AssertEquals(0, parties.Count());
			}
		}

		public void TestComplianceGetLocations()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var invoice = Factory.CreateInvoiceHeader(shipment);
			invoice.GIH_RN_NKCountryExport = "US";
			invoice.GIH_RN_NKCountryImport = "AU";
			invoice.GIH_RN_NKCountryOrigin = "PH";

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var locations = ((IGlobalCommercialInvoiceProvider)shipment).DataProvider.Locations;
				AssertEquals(3, locations.Count());
				Assert(locations.Any(l => l.ParentsDescription == Compliance.Locations.CountryOfExportDescription && l.Code == "US"));
				Assert(locations.Any(l => l.ParentsDescription == Compliance.Locations.CountryOfImportDescription && l.Code == "AU"));
				Assert(locations.Any(l => l.ParentsDescription == Compliance.Locations.GoodsCountryOfOriginDescription && l.Code == "PH"));
			}

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(false)))
			{
				var locations = ((IGlobalCommercialInvoiceProvider)shipment).DataProvider.Locations;
				AssertEquals(0, locations.Count());
			}
		}

		public void TestComplianceGetCommodities()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var invoiceHeader = Factory.CreateInvoiceHeader(shipment);
			invoiceHeader.GIH_RN_NKCountryOrigin = "US";

			var invoiceLine = Factory.CreateInvoiceLine(invoiceHeader);
			invoiceLine.GIL_Tariff1 = "012373";
			invoiceLine.GIL_Description = "Export/Import Tariff";
			invoiceLine.GIL_Tariff2 = "011275";

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var commodities = ((IGlobalCommercialInvoiceProvider)shipment).DataProvider.Commodities;
				AssertEquals(2, commodities.Count());
				Assert(commodities.Any(c => c.CommoditySource == Compliance.Commodities.CommercialInvoiceDescription && c.GoodsDescription == "Export/Import Tariff"
						&& c.GroupingOrCountry == "WCO" && c.HarmonizedCode == "0123.73" && c.Origin == "US"));
				Assert(commodities.Any(c => c.CommoditySource == Compliance.Commodities.CommercialInvoiceDescription && c.GoodsDescription == "Export/Import Tariff"
						&& c.GroupingOrCountry == "WCO" && c.HarmonizedCode == "0112.75" && c.Origin == "US"));
			}

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(false)))
			{
				var commodities = ((IGlobalCommercialInvoiceProvider)shipment).DataProvider.Commodities;
				AssertEquals(0, commodities.Count());
			}
		}

		public void TestComplianceDescription()
		{
			CombineAssertions("Parties Description", () =>
			{
				AssertEquals("Commercial Invoice Importer", Compliance.Parties.ImporterDescription);
				AssertEquals("Commercial Invoice Supplier", Compliance.Parties.SupplierDescription);
			});

			CombineAssertions("Locations Description", () =>
			{
				AssertEquals("Commercial Invoice Import Country", Compliance.Locations.CountryOfImportDescription);
				AssertEquals("Commercial Invoice Export Country", Compliance.Locations.CountryOfExportDescription);
				AssertEquals("Commercial Invoice Goods Origin", Compliance.Locations.GoodsCountryOfOriginDescription);
			});

			AssertEquals("Commodities Description", "Commercial Invoice", Compliance.Commodities.CommercialInvoiceDescription);
		}
	}
}
