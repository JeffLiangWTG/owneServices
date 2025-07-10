using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	[TestedType(typeof(SGJobDeclarationToAsycudaManifestShipmentWriter))]
	sealed class SGJobDeclarationToAsycudaManifestShipmentWriterTest : ASYCUDA.Business.UniversalDataTransfer.Testing.JobDeclarationToAsycudaManifestShipmentWriterAbstractTest
	{
		public void TestPopulateFieldsForOVR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "78787878781";
			declaration.JE_HouseBill = "OVR_TEST";

			var importer = CreateOrg("IMPORG");
			var supplier = CreateOrg("SUPORG");
			var notify = CreateOrg("NTFORG");
			AddDeclarationData(declaration, supplier, importer, notify, false);

			var invoiceHeader1 = CreateInvoiceHeader(declaration, 1);
			invoiceHeader1.JZ_InvoiceNumber = "OVR_TEST";
			var invoiceLine = (JobComInvoiceLine)invoiceHeader1.InvoiceLines[0];
			var product = invoiceLine.ProductCodes.AddNew();
			product.BZ_Tariff = "ZBP0BA0QVDG";
			product.BZ_Qty1 = 0m;

			AssertOVRProperties(declaration, null, null);

			declaration.JE_MessageType = "INP";
			declaration.JE_MessageSubType = "APS";
			declaration.SG_US_NKPlaceOfReceipt = "OVR";
			declaration.SG_US_NKPlaceOfCargoRelease = "OVR";
			AssertOVRProperties(declaration, "N", null);

			product.BZ_Tariff = "OVR";
			var cascCode = invoiceLine.CASCCode1s.AddNew();
			AssertOVRProperties(declaration, "Y", null);

			cascCode.CY_Data = "gstn001";
			AssertOVRProperties(declaration, "Y", "gstn001");
		}

		void AssertOVRProperties(JobDeclaration declaration, string expectedGSTPaymentIndicator, string expectedGSTNReferenceNumber)
		{
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, declaration));
			var writer = CreateWriter(writingManager);
			var shipment = writer.GetDataObject(declaration);
			var subShipment = shipment.SubShipmentCollection[0];

			CombineAssertions(() =>
			{
				var packingLine = subShipment.PackingLineCollection[0].PackingLineCollection[0];
				var addinfoCollection = packingLine.AddInfoGroupCollection[0].AddInfoCollection;
				AssertEquals(expectedGSTPaymentIndicator, addinfoCollection?.FirstOrDefault(x => "GSTPaymentIndicator" == (string)x.Key)?.Value);

				AssertEquals(expectedGSTNReferenceNumber, subShipment.AddInfoCollection?.FirstOrDefault(x => "GSTNReferenceNo" == (string)x.Key)?.Value);
			});
		}

		public void TestCreateXmlDuty_NoImporterAndSupplier()
		{
			AssertXmlData(true, true, "Enterprise.Customs.SG.Access.Business.Testing.UniversalDataTransfer.TestFiles.DeclarationToManifestEvent_NoImporterAndSupplier.xml");
		}

		public void TestCreateXmlDuty_NoImporterAndSupplier_Export()
		{
			AssertXmlData(true, true, "Enterprise.Customs.SG.Access.Business.Testing.UniversalDataTransfer.TestFiles.DeclarationToManifestEvent_NoImporterAndSupplier_Export.xml", SetupExportData);
		}

		public void TestCreateXmlDuty_AddExciseValueToDutyAmount_Export()
		{
			AssertXmlData(true, false, "Enterprise.Customs.SG.Access.Business.Testing.UniversalDataTransfer.TestFiles.DeclarationToManifestEvent_Export.xml", SetupExportData);
		}

		public void TestCreateXmlDuty_DoNotAddExciseValueToDutyAmount_Export()
		{
			AssertXmlData(false, false, "Enterprise.Customs.SG.Access.Business.Testing.UniversalDataTransfer.TestFiles.DeclarationToManifestEvent_NoExcise_Export.xml", SetupExportData);
		}

		protected override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

		protected override string CreateXmlDuty_AddExciseValueToDutyAmountResource => "Enterprise.Customs.SG.Access.Business.Testing.UniversalDataTransfer.TestFiles.DeclarationToManifestEvent.xml";

		protected override string CreateXmlDuty_DoNotAddExciseValueToDutyAmountResource => "Enterprise.Customs.SG.Access.Business.Testing.UniversalDataTransfer.TestFiles.DeclarationToManifestEvent_NoExcise.xml";

		protected override void AddDeclarationData(BaseJobDeclaration declaration, OrgHeader supplier, OrgHeader importer, OrgHeader notify, bool excludeImporterAndSupplier)
		{
			base.AddDeclarationData(declaration, supplier, importer, notify, excludeImporterAndSupplier);
			declaration.JE_MessageType = "INP";
			declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.NTP;
			var inwardCarrierAgent = CreateOrganisation("INWARD CARRIER AGENT", "INWDTEST");
			var outwardCarrierAgent = CreateOrganisation("OUTWARD CARRIER AGENT", "OUTWDTEST");
			var sgDeclaration = (JobDeclaration)declaration;
			sgDeclaration.JE_OH_InwardCarrierAgent = inwardCarrierAgent.PK;
			sgDeclaration.OutwardShippingLineForwarderPK = outwardCarrierAgent.PK;
		}

		protected override void AddEntryDetails(BaseJobDeclaration declaration)
		{
			base.AddEntryDetails(declaration);
			var entryHeader = declaration.CustomsEntryHeaders.OfType<Customs.Business.CusEntryHeader>().First();
			entryHeader.EntryNumber = "PMT000123";
		}

		protected override BaseJobComInvoiceHeader CreateInvoiceHeader(BaseJobDeclaration declaration, int num)
		{
			var invoice = base.CreateInvoiceHeader(declaration, num);
			CreateInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 3, 3000);
			return invoice;
		}

		protected override void AddRegistrationNumbers(OrgHeader org)
		{
			base.AddRegistrationNumbers(org);
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, $"SGUEN-{org.OH_Code}-01", Core.Constants.CountryCodes.Singapore);
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.PartyStatusType, SGPartyStatusList.Codes.Y, Core.Constants.CountryCodes.Singapore);
		}

		protected override BaseJobComInvoiceLine CreateInvoiceLine(BaseJobComInvoiceLine invoiceLine, int num, ZDecimal linePrice)
		{
			var sgInvoiceLine = (JobComInvoiceLine)base.CreateInvoiceLine(invoiceLine, num, linePrice);
			TariffView tariff;
			switch (num % 3)
			{
				case 0:
					tariff = tariff1;
					break;
				case 1:
					tariff = tariff2;
					break;
				default:
					tariff = tariff3;
					break;
			}

			sgInvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			sgInvoiceLine.SG_LastSellingPrice = linePrice + 1m;
			sgInvoiceLine.MarksAndNumbers = "MARKS " + num;
			return sgInvoiceLine;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;

		protected override JobDeclarationToAsycudaManifestShipmentWriter CreateWriter(IDataWritingManager writingManager) => new SGJobDeclarationToAsycudaManifestShipmentWriter(writingManager);

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "Goods and Services Tax");
			tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, TariffTypes.HarmonizedSystem);
			preference = helper.CreatePreferenceForCountryAndGrouping(PreferentialIndicatorCodeList.Codes.STD, "STANDARD", Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore);
			Factory.Save();
			tariff1 = CreateTariff("1020304050", false, true, "com1");
			helper.CreateTariffUOM(tariff1, UnitOfMeasureTypes.StatisticalUOMType, Core.Constants.Weight.Kilograms);
			tariff2 = CreateTariff("2030405060", true, false, "com2");
			helper.CreateTariffUOM(tariff2, UnitOfMeasureTypes.StatisticalUOMType, Core.Constants.Weight.Kilograms);
			tariff3 = CreateTariff("3040506070", false, false, "com3");
			helper.CreateTariffUOM(tariff3, UnitOfMeasureTypes.StatisticalUOMType, Core.Constants.Weight.Kilograms);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff3);
			Factory.Save();
		}

		TariffView tariff1;
		TariffView tariff2;
		TariffView tariff3;
		RefCusTariffType tariffType;
		CusRefPreferenceView preference;

		void SetupExportData(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			var sgDeclaration = (JobDeclaration)declaration;
			if (sgDeclaration != null)
			{
				sgDeclaration.SG_OutwardMAWB = "MB12345";
				sgDeclaration.SG_OutwardHAWB = "HB12345";
			}
		}

		TariffView CreateTariff(ZString tariffCode, bool isExportControl, bool isImportControl, ZString commodityCode) => SetTariffRateAndCommodity(preference, tariffType, tariffCode, isExportControl, isImportControl, commodityCode);

		TariffView SetTariffRateAndCommodity(CusRefPreferenceView preference, RefCusTariffType tariffType, ZString tariffCode, bool isExportControl, bool isImportControl, ZString commodityCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff = helper.LoadOrCreateNewTariff(tariffType, tariffCode);
			Factory.Save();
			helper.CreateTariffExciseRate(tariff, 1);
			helper.CreateDutyRate(tariff, preference, 10);
			var commodity = helper.CreateCommodity(tariff, commodityCode);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISEXPORTCONTROL, isExportControl ? "Y" : "N", commodity);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, isImportControl ? "Y" : "N", commodity);
			Factory.Save();
			return tariff;
		}

		OrgHeader CreateOrganisation(ZString name, ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.OH_Code = code;
			org.MainAddress.OA_Address1 = name + " ADDRESS 1";
			return org;
		}
	}
}
