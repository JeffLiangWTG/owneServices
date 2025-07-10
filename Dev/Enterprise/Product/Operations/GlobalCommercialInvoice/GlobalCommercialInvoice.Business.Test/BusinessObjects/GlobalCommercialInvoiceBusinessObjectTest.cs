using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Freight.Integration.IGlobalCommercialInvoiceComplianceProcessor;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(GlobalCommercialInvoiceBusinessObject))]
	public class GlobalCommercialInvoiceBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorThrowArgumentException()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			var exception = AssertExceptionThrown<ArgumentException>(() => new GlobalCommercialInvoiceBusinessObject(dummyBizO));
			AssertContains("The host business entity must implement IGlobalCommercialInvoiceProvider.", exception.Message);
		}

		public void TestCreatingAndRegisteringCollections()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var bizO = new GlobalCommercialInvoiceBusinessObject(shipment);

			AssertNotNull("Should create Header Collection", bizO.Headers);
			AssertNotNull("Should create Line Collection", bizO.Lines);
			AssertEquals("Should register Header Collection", expected: true, actual: shipment.IsRegisteredEditableChildObject(bizO.Headers));
			AssertEquals("Should register Line Collection", expected: true, actual: shipment.IsRegisteredEditableChildObject(bizO.Lines));
		}

		public void TestCreatingAndRegisteringCustomCollections()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var header1 = Factory.CreateInvoiceHeader(shipment);
			var header2 = Factory.CreateInvoiceHeader(shipment);

			Factory.CreateInvoiceLine(header1);
			Factory.CreateInvoiceLine(header2);
			Factory.CreateInvoiceLine(header2);

			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			var lineCollection = new GlobalCommercialInvoiceLineIntegratedCollection(shipment, headerCollection);
			var bizO = new GlobalCommercialInvoiceBusinessObject(shipment, headerCollection, lineCollection);

			AssertEquals("Should use the provided Header Collection", expected: headerCollection, actual: bizO.Headers);
			AssertEquals("Should use the provided Line Collection", expected: lineCollection, actual: bizO.Lines);
			AssertEquals("Should register the provided Header Collection", expected: true, actual: shipment.IsRegisteredEditableChildObject(headerCollection));
			AssertEquals("Should register the provided Line Collection", expected: true, actual: shipment.IsRegisteredEditableChildObject(lineCollection));
		}

		public void TestCommodityUpdatedWhenHeaderOriginChanged()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var shipment = (BusinessObject)Factory.CreateNewShipment();
				var header = Factory.CreateInvoiceHeader(shipment);
				header.GIH_RN_NKCountryOrigin = "US";
				var line = Factory.CreateInvoiceLine(header);
				line.GIL_Tariff1 = "111111";
				var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
				var lineCollection = new GlobalCommercialInvoiceLineIntegratedCollection(shipment, headerCollection);
				var bizO = new GlobalCommercialInvoiceBusinessObject(shipment, headerCollection, lineCollection);
				var provider = bizO as IGlobalCommercialInvoiceComplianceProvider;
				AssertEquals(1, provider.Commodities.Count());
				AssertEquals("US", provider.Commodities.FirstOrDefault().Origin);

				header.GIH_RN_NKCountryOrigin = "AU";
				AssertEquals(1, provider.Commodities.Count());
				AssertEquals("AU", provider.Commodities.FirstOrDefault().Origin);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GlobalCommercialInvoiceBusinessObject((BusinessObject)Factory.CreateNewShipment());
		}
	}
}
