using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(GlobalCommercialInvoiceLine))]
	public class GlobalCommercialInvoiceLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.CreateInvoiceLine(
			Factory.CreateInvoiceHeader((BusinessObject)Factory.CreateNewShipment()));

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		public override void TestFetchForLoad()
		{
			var businessObjectForFetchForLoad = GetBusinessObjectForFetchForLoad();
			businessObjectForFetchForLoad.Factory.Save();
			var businessObjectFactory = NewFactory();
			GetBusinessObjectInNewFactory(businessObjectForFetchForLoad, businessObjectFactory);
			var actual = new ZStringBuilder(businessObjectFactory.GetAllFetchHintedTableNames()).ToStringWithDelimiterBetweenAppends(",");
			AssertEquals("** DEVELOPER-ONLY TEST **\r\nFetch hints that are located in FetchForLoad should always be consumed prior to the completed construction of the business object. \r\nAny fetch hint not following this pattern should be moved to either FetchForValidate or FetchForView or even a manual fetch hint in a particular business scenario.", string.Empty, actual);
		}

		public void TestDefaultValues()
		{
			var weightUnit = Env.Registry.FreightWeightUnit;
			var volumeUnit = Env.Registry.FreightWeightUnit;
			Env.Registry.FreightWeightUnit = Constants.Weight.Pounds;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicYards;

			var invoiceLine = Factory.New<GlobalCommercialInvoiceLine>();
			AssertEquals("Unit of Gross Weight", Constants.Weight.Pounds, invoiceLine.GIL_GrossWeightUQ);
			AssertEquals("Unit of Net Weight", Constants.Weight.Pounds, invoiceLine.GIL_NetWeightUQ);
			AssertEquals("Unit of Volume", Constants.Volume.CubicYards, invoiceLine.GIL_VolumeUQ);

			Env.Registry.FreightWeightUnit = weightUnit;
			Env.Registry.FreightWeightUnit = volumeUnit;
		}

		/// <summary>
		/// Tariffs need a separate test because they format the user input.
		/// </summary>
		/// <param name="info"></param>
		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name != nameof(GlobalCommercialInvoiceLine.GIL_Tariff1) && info.Name != nameof(GlobalCommercialInvoiceLine.GIL_Tariff2))
			{
				base.TestBizObjectField(info);
			}
		}

		public void TestGetParentLineHeaderWithCalculatedProperties()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var (headerCollection, lineCollection) = Factory.CreateNewHeaderAndLineCollection(shipment);
			var header = headerCollection.AddNew();
			header.GIH_RX_NKInvoiceCurrency = "AUD";

			var line = lineCollection.AddNew();

			AssertEquals(header.GIH_RX_NKInvoiceCurrency, line.LinePriceCurrency);
		}

		public void TestInvoiceLineInvoiceNumberOnlyGetsSetToRelatedInvoiceHeaders()
		{
			var (headerCollection, lineCollection) = CreateNewHeaderAndLineCollection();
			var header = headerCollection.AddNew();
			header.GIH_InvoiceNumber = "1234";

			var line = lineCollection.AddNew();
			Factory.Save();
			AssertEquals(header.PK, line.GIL_GIH_Header);

			line.GIL_GIH_Header = ZGuid.NewZGuid();
			Factory.Save();
			AssertEquals(header.PK, line.GIL_GIH_Header);
		}

		public void TestGetReadOnlySecurityCheckpointForEdit()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var (headerCollection, lineCollection) = Factory.CreateNewHeaderAndLineCollection(shipment);
			var header = headerCollection.AddNew();
			header.GIH_InvoiceNumber = "INV001";
			header.GIH_RX_NKInvoiceCurrency = "AUD";

			var line = lineCollection.AddNew();

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
					AssertEquals(readonlyInfo, line.GIL_DescriptionInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_GrossWeightInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_GrossWeightUQInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_InvoiceQuantityInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_InvoiceUQInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_LineNoInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_LinePriceInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_NetWeightInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_NetWeightUQInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_ProductInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_Tariff1Info.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_Tariff2Info.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_VolumeInfo.ReadOnly);
					AssertEquals(readonlyInfo, line.GIL_VolumeUQInfo.ReadOnly);
				}
			}
		}

		public void TestTariffFieldsFormatting()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var (_, lineCollection) = Factory.CreateNewHeaderAndLineCollection(shipment);
			var line = lineCollection.AddNew();

			line.GIL_Tariff1 = "1234567890";
			AssertEquals("1234.56.78 90", line.GIL_Tariff1);

			line.GIL_Tariff2 = "12345678901234567890";
			AssertEquals("1234.56.78 901234567890", line.GIL_Tariff2);

			line.GIL_Tariff1 = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
			AssertEquals(string.Empty, line.GIL_Tariff1);

			line.GIL_Tariff2 = "12345//67&89?012*3(4)56@78#90";
			AssertEquals("1234.56.78 901234567890", line.GIL_Tariff2);

			line.GIL_Tariff1 = "12345678901234567890123456789012345678901234567890";
			AssertEquals("1234.56.78 901234567890123456789012", line.GIL_Tariff1);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var (headerCollection, lineCollection) = CreateNewHeaderAndLineCollection();
			var header = headerCollection.AddNew();
			header.GIH_InvoiceNumber = "INV001";
			header.GIH_RX_NKInvoiceCurrency = "AUD";

			var line = lineCollection.AddNew();
			line.Headers = headerCollection;
			line.GIL_GIH_Header = header.PK;
			Factory.Save();

			lineCollection.Delete(line);
			Factory.Save();
			Assert(lineCollection.Count == 0);
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			var (headerCollection, lineCollection) = CreateNewHeaderAndLineCollection();
			var header = headerCollection.AddNew();
			header.GIH_InvoiceNumber = "INV001";
			header.GIH_RX_NKInvoiceCurrency = "AUD";

			var line = lineCollection.AddNew();
			line.Headers = headerCollection;
			line.GIL_GIH_Header = header.PK;
			Factory.Save();

			base.TestSettingValueCallsRefreshBindingCore(line);
		}

		(GlobalCommercialInvoiceHeaderCollection HeaderCollection, GlobalCommercialInvoiceLineIntegratedCollection LineCollection) CreateNewHeaderAndLineCollection()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			var lineCollection = new GlobalCommercialInvoiceLineIntegratedCollection(shipment, headerCollection);

			return (headerCollection, lineCollection);
		}
	}
}
