using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(GlobalCommercialInvoiceHeader))]
	public class GlobalCommercialInvoiceHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.CreateInvoiceHeader((BusinessObject)Factory.CreateNewShipment());

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		public void TestSetDefaultValues()
		{
			var invoiceHeader = Factory.New<GlobalCommercialInvoiceHeader>();
			AssertEquals(ZDate.Today, invoiceHeader.GIH_InvoiceDate);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, invoiceHeader.GIH_RX_NKInvoiceCurrency);
		}

		public void TestGetReadOnlySecurityCheckpointForEdit()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			var header = headerCollection.AddNew();
			header.GIH_InvoiceNumber = "INV001";
			header.GIH_RX_NKInvoiceCurrency = "AUD";

			var securityCore = Factory.CreateSecurityCore();
			securityCore.ShipmentsCommercialInvoice.IsAllowed = true;
			securityCore.ShipmentsCommercialInvoiceEdit.IsAllowed = true;

			AssertReadonlyFields(securityCore, readonlyInfo: false);

			securityCore.ShipmentsCommercialInvoiceEdit.IsAllowed = false;
			AssertReadonlyFields(securityCore, readonlyInfo: true);

			void AssertReadonlyFields(SecurityCore security, bool readonlyInfo)
			{
				using (Env.SetTemporarySecurityInstanceForTest(security))
				{
					AssertEquals(readonlyInfo, header.GIH_InvoiceAmountInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_InvoiceDateInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_InvoiceNumberInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_OA_ImporterAddressInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_OA_SupplierAddressInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_OH_ImporterInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_OH_SupplierInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_RN_NKCountryExportInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_RN_NKCountryImportInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_RN_NKCountryOriginInfo.ReadOnly);
					AssertEquals(readonlyInfo, header.GIH_RX_NKInvoiceCurrencyInfo.ReadOnly);
				}
			}
		}
	}
}
