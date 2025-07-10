using System;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImportCustomsDeclarationDocumentWrapper))]
	abstract class ImportCustomsDeclarationDocumentWrapperAbstractTest<TImportCustomsDeclarationDocumentWrapper> : NonPersistentBusinessObjectTestCase
		where TImportCustomsDeclarationDocumentWrapper : ImportCustomsDeclarationDocumentWrapper
	{
		[ExpectNoExceptions]
		public void TestSpecificTaxBaseQuantityFormat()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var fee = entryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			fee.CF_ChargeType = "DTS";
			fee.CF_MethodOfCalculation = "KGM";
			fee.CF_Rate = 1.25M;
			fee.CF_BaseValue = 0.3m;
			var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections[0].Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("0.3").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeclarationNoFormatted()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DeclarationNoFormatted, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "AA  07094AD515";
			entryHeader.CH_Status = "AWO";
			NUnit.Framework.Assert.That(wrapper.DeclarationNoFormatted, NUnit.Framework.Is.EqualTo("AA/  /07/094/AD515").Using(CustomComparers.TypeComparison));
			entryHeader.EntryNumber = "AA";
			NUnit.Framework.Assert.That(wrapper.DeclarationNoFormatted, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declaration.EntryNumber = "AB  07094AD515";
			NUnit.Framework.Assert.That(wrapper.DeclarationNoFormatted, NUnit.Framework.Is.EqualTo("AB/  /07/094/AD515").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalInvoiceAmountCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			entryHeader.CH_DeclarationIncoterm = Core.Constants.IncoTerms.FreeOnBoard;
			NUnit.Framework.Assert.That(wrapper.TotalInvoiceAmountCaption, NUnit.Framework.Is.EqualTo("離  岸  價  格").Using(CustomComparers.TypeComparison));
			entryHeader.CH_DeclarationIncoterm = Core.Constants.IncoTerms.ExWorks;
			NUnit.Framework.Assert.That(wrapper.TotalInvoiceAmountCaption, NUnit.Framework.Is.EqualTo("出  廠  價  格").Using(CustomComparers.TypeComparison));
			entryHeader.CH_DeclarationIncoterm = Core.Constants.IncoTerms.FreeAlongsideShip;
			NUnit.Framework.Assert.That(wrapper.TotalInvoiceAmountCaption, NUnit.Framework.Is.EqualTo("離  岸  價  格").Using(CustomComparers.TypeComparison));
			entryHeader.CH_DeclarationIncoterm = Core.Constants.IncoTerms.CostAndInsurance;
			NUnit.Framework.Assert.That(wrapper.TotalInvoiceAmountCaption, NUnit.Framework.Is.EqualTo("離  岸  價  格").Using(CustomComparers.TypeComparison));
			entryHeader.CH_DeclarationIncoterm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			NUnit.Framework.Assert.That(wrapper.TotalInvoiceAmountCaption, NUnit.Framework.Is.EqualTo("離  岸  價  格").Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return GetDocumentWrapper(entryHeader, Factory);
		}

		[ExpectNoExceptions]
		public void TestNameOfPortOfLoading()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_PortName = "TAIWANG";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			declaration.JE_RL_NKOrigin = "TWXXX";
			NUnit.Framework.Assert.That(wrapper.NameOfPortOfLoading, NUnit.Framework.Is.EqualTo("TAIWANG").Using(CustomComparers.TypeComparison));
			declaration.JE_RL_NKOrigin = "TWZ99";
			declaration.JE_Z99PortOfOrigin = "台湾";
			NUnit.Framework.Assert.That(wrapper.NameOfPortOfLoading, NUnit.Framework.Is.EqualTo("台湾").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeclarationNo()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DeclarationNo, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "AAA";
			entryHeader.CH_Status = "AWO";
			NUnit.Framework.Assert.That(wrapper.DeclarationNo, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
			declaration.EntryNumber = "BBB";
			NUnit.Framework.Assert.That(wrapper.DeclarationNo, NUnit.Framework.Is.EqualTo("BBB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImportCustomsDeclarationDocumentDataFromEntryMessage()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "ABI31100394446", "A10", 14608m, 200m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "ABI31100394446", "B51", 409m, 200m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "ABI31100394446", "B40", 196m, 200m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "ABI31100394446", "B31", 866m, 200m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "ABI31100394446", "C10", 139m, 200m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "ABI31100394446", "B10", 699m, 200m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "ABI31100394446", "B40", 421m, 200m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "ABI32100355835", "D10", 300m);

			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.DeclarationNo, NUnit.Framework.Is.EqualTo("BBAA0812300001").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.A10ImportDuty, NUnit.Framework.Is.EqualTo("14608").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.A19ImportDuty, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.B51ImportDuty, NUnit.Framework.Is.EqualTo("409").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.B59ImportDuty, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee1, NUnit.Framework.Is.EqualTo(699m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee2, NUnit.Framework.Is.EqualTo(196M).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee3, NUnit.Framework.Is.EqualTo(866m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee4, NUnit.Framework.Is.EqualTo(139m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalNonCashAmount, NUnit.Framework.Is.EqualTo(300m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.BusinessTaxBase, NUnit.Framework.Is.EqualTo(200m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalAmount, NUnit.Framework.Is.EqualTo(17338m).Using(CustomComparers.TypeComparison));
			});

			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "BCJ31100394447", "B29", 0m, 500m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "BCJ32100355835", "F10", 600m);

			wrapper = GetDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.TotalNonCashAmount, NUnit.Framework.Is.EqualTo(900m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.BusinessTaxBase, NUnit.Framework.Is.EqualTo(700m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalAmount, NUnit.Framework.Is.EqualTo(17338m).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestCodeOfStorageLocationCode()
		{
			using (TWCustomsDataRegistry.Instance.DefaultPrintingGoodsLocationDescription.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var wrapper = CreateDocumentWrapper("TEST GROUPING");
				NUnit.Framework.Assert.That(wrapper.CodeOfStorageLocation, NUnit.Framework.Is.EqualTo("XXXX0123\r\nXXXXXX").Using(CustomComparers.TypeComparison));
			}
		}

		[TestDate(2019, 06, 25)]
		[ExpectNoExceptions]
		public void TestImportCustomsDeclarationDocumentData()
		{
			var wrapper = CreateDocumentWrapper("TEST GROUPING");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.CategoriesOfTransport, NUnit.Framework.Is.EqualTo("海運").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TypeOfDeclaration, NUnit.Framework.Is.EqualTo("G1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DeclarationNo, NUnit.Framework.Is.EqualTo("BBAA0812300001").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DeclarationNoBarCode, NUnit.Framework.Is.EqualTo("*BBAA0812300001*").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Registration, NUnit.Framework.Is.EqualTo("212").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ManifestSerialNumber, NUnit.Framework.Is.EqualTo("X2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CodeOfVessel, NUnit.Framework.Is.EqualTo("654321").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.MasterBillNo, NUnit.Framework.Is.EqualTo("XXX123456").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ExchangeRate, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CodeOfPortOfLoading, NUnit.Framework.Is.EqualTo("TST").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.NameOfPortOfLoading, NUnit.Framework.Is.EqualTo("TestPortName").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ExportationDate, NUnit.Framework.Is.EqualTo("108年09月01日").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DateOfImportation, NUnit.Framework.Is.EqualTo("108年08月01日").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.FreightCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.FreightFormat, NUnit.Framework.Is.EqualTo("200.00").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CodeOfStorageLocationCode, NUnit.Framework.Is.EqualTo("XXXX0123").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CodeOfStorageLocation, NUnit.Framework.Is.EqualTo("XXXX0123").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CodeOfStorageLocationDescription, NUnit.Framework.Is.EqualTo("XXXXXX").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TransportModeCode, NUnit.Framework.Is.EqualTo("12").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DateOfDeclaration, NUnit.Framework.Is.EqualTo("108年05月15日").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.InsuranceFeeCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.InsuranceFeeFormat, NUnit.Framework.Is.EqualTo("100.00").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.BAN, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ImporterCustomsSupervisionCode, NUnit.Framework.Is.EqualTo("22233").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.RelatedToSeller, NUnit.Framework.Is.EqualTo("135").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ModeOfDutyPayment, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ExpensesToBeAddedCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ExpensesToBeAddedFormat, NUnit.Framework.Is.EqualTo("102.00").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ExpensesToBeDeductedCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ExpensesToBeDeductedFormat, NUnit.Framework.Is.EqualTo("107.00").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ImporterChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ImporterEnglishName, NUnit.Framework.Is.EqualTo("OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ImporterAddressLine, NUnit.Framework.Is.EqualTo(@"TW OTA ADDRESS 1TW OTA ADDRESS 2
ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ImporterAEOCode, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CustomsApprovalNo, NUnit.Framework.Is.EqualTo("A3").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.SellerChineseName, NUnit.Framework.Is.EqualTo("Seller TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.SellerEnglishName, NUnit.Framework.Is.EqualTo("Seller OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.SellerAddressLine, NUnit.Framework.Is.EqualTo(@"Seller TW OTA ADDRESS 1Seller TW OTA ADDRESS 2
SELLER ADDRESS 1 SELLER ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.SellerAEONo, NUnit.Framework.Is.EqualTo("TWAEO-8889999").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.SellerCountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.BusinessAdministrationNo, NUnit.Framework.Is.EqualTo("9696969").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.SellerCustomsSupervisionCode, NUnit.Framework.Is.EqualTo("22233").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalPackageQuantityAndUnit, NUnit.Framework.Is.EqualTo("123 / PK").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DescriptionOfPackage, NUnit.Framework.Is.EqualTo("PKG Descr").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.GrossWeight, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalAmount, NUnit.Framework.Is.EqualTo(1112m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalNonCashAmount, NUnit.Framework.Is.EqualTo(421m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.BusinessTaxBase, NUnit.Framework.Is.EqualTo(2560m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ClearanceType, NUnit.Framework.Is.EqualTo("C1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ClearanceCode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DuplicateNo, NUnit.Framework.Is.EqualTo("B1\r\nB2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.NoOfCopies, NUnit.Framework.Is.EqualTo("1\r\n2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DeclarantAndAEOCode, NUnit.Framework.Is.EqualTo("新加坡商敦豪全球貨運物流股份有限公司台灣 123\r\nTWAEO-363636").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DedicatedStaff, NUnit.Framework.Is.EqualTo("XA1\r\n1234").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.HouseBillNo, NUnit.Framework.Is.EqualTo("ABC85858").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalInvoiceAmount, NUnit.Framework.Is.EqualTo(88m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalInvoiceAmountCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalAdValoremTaxBaseAmount, NUnit.Framework.Is.EqualTo(383m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalAdValoremTaxBaseAmountCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalCIFAmount, NUnit.Framework.Is.EqualTo(383m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalCIFAmountCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.A10ImportDuty, NUnit.Framework.Is.EqualTo("101").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.A19ImportDuty, NUnit.Framework.Is.EqualTo("102").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.B51ImportDuty, NUnit.Framework.Is.EqualTo("800").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.B59ImportDuty, NUnit.Framework.Is.EqualTo("104").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee1Description, NUnit.Framework.Is.EqualTo("貨物稅").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee2Description, NUnit.Framework.Is.EqualTo("營業稅").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee3Description, NUnit.Framework.Is.EqualTo("菸酒稅").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee4Description, NUnit.Framework.Is.EqualTo("健康福利捐").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee1DescriptionEN, NUnit.Framework.Is.EqualTo("Commodity Tax").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee2DescriptionEN, NUnit.Framework.Is.EqualTo("Business tax").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee3DescriptionEN, NUnit.Framework.Is.EqualTo("Tobacco and Alcohol Tax").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee4DescriptionEN, NUnit.Framework.Is.EqualTo("Health and Welfare Surcharge").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee1, NUnit.Framework.Is.EqualTo(105m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee2, NUnit.Framework.Is.EqualTo(108m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee3, NUnit.Framework.Is.EqualTo(106m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.DutyTaxFee4, NUnit.Framework.Is.EqualTo(107m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CommonDecimalPlaces, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo("TPCREGNO").Using(CustomComparers.TypeComparison));
			});

			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.HideIMPImporterTradChineseAddr = true;
			jobDeclarationDocumentAddressConfig.HideIMPImporterEnglishAddr = false;
			jobDeclarationDocumentAddressConfig.HideIMPSellerTradChineseAddr = true;
			wrapper = GetDocumentWrapper(declaration.EntryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.ImporterAddressLine, NUnit.Framework.Is.EqualTo(@"ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.SellerAddressLine, NUnit.Framework.Is.EqualTo(@"SELLER ADDRESS 1 SELLER ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison));
			});

			jobDeclarationDocumentAddressConfig.HideIMPImporterTradChineseAddr = false;
			jobDeclarationDocumentAddressConfig.HideIMPImporterEnglishAddr = true;
			jobDeclarationDocumentAddressConfig.HideIMPSellerTradChineseAddr = false;
			wrapper = GetDocumentWrapper(declaration.EntryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.ImporterAddressLine, NUnit.Framework.Is.EqualTo(@"TW OTA ADDRESS 1TW OTA ADDRESS 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.SellerAddressLine, NUnit.Framework.Is.EqualTo(@"Seller TW OTA ADDRESS 1Seller TW OTA ADDRESS 2
SELLER ADDRESS 1 SELLER ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison));
			});
		}

		[TestDate(2020, 08, 25)]
		[ExpectNoExceptions]
		public void TestDateOfDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AA";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DateOfDeclaration, NUnit.Framework.Is.EqualTo("109年08月25日").Using(CustomComparers.TypeComparison), "DateOfDeclaration");
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 8, 28);
			NUnit.Framework.Assert.That(wrapper.DateOfDeclaration, NUnit.Framework.Is.EqualTo("109年08月28日").Using(CustomComparers.TypeComparison), "DateOfDeclaration");
		}

		[TestDate(2020, 07, 17)]
		[ExpectNoExceptions]
		public void TestOtherDeclarationsForTransportTypeSea()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AA";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_MarksAndNumbers = "TEST MARKS";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "T1";
			importer.OH_FullName = "test org";
			var poaDoc = importer.RequiredDocuments.AddNew();
			poaDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc.EQ_DateReceived = new ZDateTimeOffset(2020, 7, 17);
			poaDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			poaDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			poaDoc.EQ_DocNumber = "111111";
			var poaDocAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaDocAttr.D0_AttribValue = "A";
			declaration.JE_OH_Importer = importer.PK;
			entryInstruction.TW_TradersRemarks += System.Environment.NewLine + "Traders Remarks Note Line 1";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var containers = declaration.CusContainers;
			AddNewContainerForAdditionalInfoTest(containers, invoiceHeader, entryHeader, "UUUU1234568");

			var marksNumbers = new ZStringBuilder();
			for (var i = 1; i <= 9; ++i)
			{
				marksNumbers.AppendLine(i.ToString());
			}

			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			CombineAssertions("Mark&Number、Container、TradersRemarks not exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9

UUUU1234568

其他申報事項：
常年(長期)委任報關核准文號：111111
起：109年07月17日
迄：116年07月17日
Traders Remarks Note Line 1"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.Null.Or.Empty);
			});

			entryInstruction.TW_TradersRemarks += @"
Traders Remarks Note Line 2
Traders Remarks Note Line 3";

			marksNumbers.AppendLine("10");
			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();
			AddNewContainerForAdditionalInfoTest(containers, invoiceHeader, entryHeader, "UUUU1234597");

			wrapper = GetDocumentWrapper(entryHeader, Factory);
			CombineAssertions("TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
10
UUUU1234568
UUUU1234597
其他申報事項：
常年(長期)委任報關核准文號：111111
起：109年07月17日
迄：116年07月17日
Traders Remarks Note Line 1
其他申報事項資料列印於後"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列其他申報事項資料*****)
Traders Remarks Note Line 2
Traders Remarks Note Line 3"));
			});

			marksNumbers.AppendLine("11");
			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();
			wrapper = GetDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number、TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
標記資料列印於後
UUUU1234568
UUUU1234597
其他申報事項：
常年(長期)委任報關核准文號：111111
起：109年07月17日
迄：116年07月17日
Traders Remarks Note Line 1
其他申報事項資料列印於後"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
10
11
(*****續列其他申報事項資料*****)
Traders Remarks Note Line 2
Traders Remarks Note Line 3"));
			});

			AddNewContainerForAdditionalInfoTest(containers, invoiceHeader, entryHeader, "UUUU1234569");
			wrapper = GetDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number、Container、TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
標記資料列印於後
UUUU1234568
貨櫃號碼資料列印於後
其他申報事項：
常年(長期)委任報關核准文號：111111
起：109年07月17日
迄：116年07月17日
Traders Remarks Note Line 1
其他申報事項資料列印於後"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
10
11
(*****續列貨櫃號碼資料*****)
UUUU1234597
UUUU1234569
(*****續列其他申報事項資料*****)
Traders Remarks Note Line 2
Traders Remarks Note Line 3"));
			});

			entryInstruction.TW_OverrideTradersRemarks = false;
			wrapper = GetDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number、Container exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
標記資料列印於後
UUUU1234568
貨櫃號碼資料列印於後
其他申報事項：
常年(長期)委任報關核准文號：111111
起：109年07月17日
迄：116年07月17日"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
10
11
(*****續列貨櫃號碼資料*****)
UUUU1234597
UUUU1234569"));
			});

			invoiceHeader.JZ_MarksAndNumbers = "1";
			wrapper = GetDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Container exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1









UUUU1234568
貨櫃號碼資料列印於後
其他申報事項：
常年(長期)委任報關核准文號：111111
起：109年07月17日
迄：116年07月17日"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列貨櫃號碼資料*****)
UUUU1234597
UUUU1234569"));
			});

			declaration.JE_OH_Importer = ZGuid.Empty;
			entryInstruction.TW_TradersRemarks = @"Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4";
			wrapper = GetDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Container、TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1









UUUU1234568
貨櫃號碼資料列印於後
其他申報事項：
Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
其他申報事項資料列印於後"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列貨櫃號碼資料*****)
UUUU1234597
UUUU1234569
(*****續列其他申報事項資料*****)
Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4"));
			});

			invoiceHeader.JZ_MarksAndNumbers = @"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
Mark & Number Line9
Mark & Number Line10 Mark & Number Line10 Mark & Number Line10 Mark & Number Line10";
			containers.DeleteAll();
			entryInstruction.TW_OverrideTradersRemarks = false;
			wrapper = GetDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark &Number exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark
& Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
標記資料列印於後"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
Mark & Number Line9
Mark & Number Line10 Mark & Number Line10 Mark & Number Line10 Mark & Number Line10"));
			});
		}

		[TestDate(2020, 07, 17)]
		[ExpectNoExceptions]
		public void TestOtherDeclarationsForTransportTypeAir()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AA";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_MarksAndNumbers = "TEST MARKS";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "T1";
			importer.OH_FullName = "test org";
			var poaDoc = importer.RequiredDocuments.AddNew();
			poaDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc.EQ_DateReceived = new ZDateTimeOffset(2020, 7, 17);
			poaDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			poaDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			poaDoc.EQ_DocNumber = "111111";
			var poaDocAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaDocAttr.D0_AttribValue = "A";
			declaration.JE_OH_Importer = importer.PK;
			entryInstruction.TW_TradersRemarks += System.Environment.NewLine + "Traders Remarks Note Line 1";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var marksNumbers = new ZStringBuilder();
			for (var i = 1; i <= 11; ++i)
			{
				marksNumbers.AppendLine(i.ToString());
			}

			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			CombineAssertions("Mark&Number、TradersRemarks not exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
10
11

其他申報事項：
常年(長期)委任報關核准文號：111111
起：109年07月17日
迄：116年07月17日
Traders Remarks Note Line 1"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.Null.Or.Empty);
			});

			declaration.JE_OH_Importer = ZGuid.Empty;
			entryInstruction.TW_TradersRemarks = @"Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4";

			marksNumbers.AppendLine("12");
			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();

			wrapper = GetDocumentWrapper(entryHeader, Factory);
			CombineAssertions("TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
10
11
12
其他申報事項：
Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
其他申報事項資料列印於後"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列其他申報事項資料*****)
Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4"));
			});

			invoiceHeader.JZ_MarksAndNumbers = @"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
Mark & Number Line9
Mark & Number Line10
Mark & Number Line11
Mark & Number Line12
Mark & Number Line13 Mark & Number Line13 Mark & Number Line13 Mark & Number Line13";
			wrapper = GetDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number、TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark
& Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
Mark & Number Line9
Mark & Number Line10
標記資料列印於後
其他申報事項：
Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
其他申報事項資料列印於後"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
Mark & Number Line11
Mark & Number Line12
Mark & Number Line13 Mark & Number Line13 Mark & Number Line13 Mark & Number Line13
(*****續列其他申報事項資料*****)
Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4"));
			});

			entryInstruction.TW_OverrideTradersRemarks = false;
			wrapper = GetDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo(@"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark
& Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
Mark & Number Line9
Mark & Number Line10
標記資料列印於後"));
				NUnit.Framework.Assert.That(GetPart2String(wrapper.Part2Sections), NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
Mark & Number Line11
Mark & Number Line12
Mark & Number Line13 Mark & Number Line13 Mark & Number Line13 Mark & Number Line13"));
			});
		}

		string GetPart1String(ImportCustomsDeclarationDocumentWrapper wrapper)
		{
			return (wrapper.MarksAndNumbersFront1 + "\r\n" +
				wrapper.MarksAndNumbersFront2 + "\r\n" +
				wrapper.MarksAndNumbersFront3 + "\r\n" +
				wrapper.MarksAndNumbersFront4 + "\r\n" +
				wrapper.MarksAndNumbersFront5 + "\r\n" +
				wrapper.MarksAndNumbersFront6 + "\r\n" +
				wrapper.MarksAndNumbersFront7 + "\r\n" +
				wrapper.MarksAndNumbersFront8 + "\r\n" +
				wrapper.MarksAndNumbersFront9 + "\r\n" +
				wrapper.MarksAndNumbersFront10 + "\r\n" +
				wrapper.MarksAndNumbersFront11 + "\r\n" +
				wrapper.MarksAndNumbersFront12 + "\r\n" +
				wrapper.MarksAndNumbersFront13 + "\r\n" +
				wrapper.MarksAndNumbersFront14 + "\r\n" +
				wrapper.MarksAndNumbersFront15 + "\r\n" +
				wrapper.MarksAndNumbersFront16 + "\r\n" +
				wrapper.MarksAndNumbersFront17 + "\r\n" +
				wrapper.MarksAndNumbersFront18).Trim();
		}

		string GetPart2String(BusinessObjectCollectionWrapper<ImportPart2SectionBodyWrapper> part2Sections)
		{
			return ZString.Join("\r\n", part2Sections.Cast<ImportPart2SectionBodyWrapper>().Select(c => c.OtherDeclarationsFront).ToArray());
		}

		[ExpectNoExceptions]
		public void TestMarksAndNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentNotes = shipment.Notes.VisibleNotes;
			var noteD = shipmentNotes.AddNew();
			noteD.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteD.ST_NoteContextModule = nameof(StmNoteContextModule.D);
			noteD.ST_NoteText = "D Marks & Numbers";
			var noteA = shipmentNotes.AddNew();
			noteA.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteA.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			noteA.ST_NoteText = "A Marks & Numbers";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceHeader1.JZ_MarksAndNumbers = "MARKS AND NUMBER 1";
			invoiceHeader2.JZ_MarksAndNumbers = "MARKS AND NUMBER 1";
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo("D Marks & Numbers"));
			shipmentNotes.Remove(noteD);
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo("MARKS AND NUMBER 1"));
			invoiceHeader1.JZ_MarksAndNumbers = ZString.Empty;
			invoiceHeader2.JZ_MarksAndNumbers = ZString.Empty;
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo("A Marks & Numbers"));
			shipmentNotes.Remove(noteA);
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(GetPart1String(wrapper), NUnit.Framework.Is.EqualTo("N/M"));
		}

		CusContainer AddNewContainerForAdditionalInfoTest(ICusContainerCollection<CusContainer> containers, JobComInvoiceHeader invHeader, CusEntryHeader entryHeader, ZString containerNum)
		{
			var container = containers.AddNew();
			container.CO_ContainerNumber = containerNum;
			return container;
		}

		[ExpectNoExceptions]
		public void TestSetSectionBody_DashDisplayWhenAllGroupingsAreSame()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.JI_Group = "";
			invoiceLine2.JI_Group = "";
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(14));
			NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			invoiceLine1.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2  ";
			invoiceLine2.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2  ";
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(14));
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 1").Using(CustomComparers.TypeComparison), "sections[1].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 2").Using(CustomComparers.TypeComparison), "sections[2].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[4].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 1").Using(CustomComparers.TypeComparison), "sections[6].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 2").Using(CustomComparers.TypeComparison), "sections[7].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[9].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				AssertBox35InSectionsIsEmpty(sections, 11, 14);
			});

			invoiceLine1.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2 is intentionally made to be very looooooooooooooooooooooooooooooooong  ";
			invoiceLine2.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2 is intentionally made to be very looooooooooooooooooooooooooooooooong  ";
			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(17));
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 1").Using(CustomComparers.TypeComparison), "sections[1].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 2 is intentionally").Using(CustomComparers.TypeComparison), "sections[2].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("made to be very").Using(CustomComparers.TypeComparison), "sections[3].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("looooooooooooooooooooooooooooooooon").Using(CustomComparers.TypeComparison), "sections[4].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("g").Using(CustomComparers.TypeComparison), "sections[5].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[7].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 1").Using(CustomComparers.TypeComparison), "sections[9].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 2 is intentionally").Using(CustomComparers.TypeComparison), "sections[10].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[11].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("made to be very").Using(CustomComparers.TypeComparison), "sections[11].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[11].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[11].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[11].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[11].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[11].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[11].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("looooooooooooooooooooooooooooooooon").Using(CustomComparers.TypeComparison), "sections[12].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[12].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[12].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[12].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[12].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[13].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("g").Using(CustomComparers.TypeComparison), "sections[13].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[13].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[13].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[13].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[13].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[13].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[13].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[14].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[14].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[14].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[14].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[14].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[14].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[14].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[14].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[15].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[15].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[15].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[15].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[15].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[15].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[15].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[15].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[16].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[16].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[16].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[16].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[16].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[16].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[16].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[16].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
			});

			invoiceLine1.JI_Group = "  This grouping is specially designed so that it spans into exactly three lines  ";
			invoiceLine2.JI_Group = "  This grouping is specially designed so that it spans into exactly three lines  ";
			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(14));
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("This grouping is specially designed").Using(CustomComparers.TypeComparison), "sections[1].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("so that it spans into exactly three").Using(CustomComparers.TypeComparison), "sections[2].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("lines").Using(CustomComparers.TypeComparison), "sections[3].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[5].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("This grouping is specially designed").Using(CustomComparers.TypeComparison), "sections[7].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("so that it spans into exactly three").Using(CustomComparers.TypeComparison), "sections[8].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[8].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("lines").Using(CustomComparers.TypeComparison), "sections[9].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[9].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[10].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[11].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[11].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[11].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[11].Box35DescriptionOfGoods_Line4");
				NUnit.Framework.Assert.That(sections[11].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[11].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[11].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[11].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[12].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[12].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[12].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[12].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				AssertBox35InSectionsIsEmpty(sections, 13, 14);
			});
		}

		[ExpectNoExceptions]
		public void TestSetSectionBody_DashDisplayWhenGroupingsAreDifferent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.JI_Group = "";
			invoiceLine2.JI_Group = "  Grouping  ";
			declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(14));
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[0].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "sections[2].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[4].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				AssertBox35InSectionsIsEmpty(sections, 5, 14);
			});

			entryLine1.CL_Description = "Entry Line 1-1\r\nEntry Line 1-2\r\nEntry Line 1-3";
			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(12));
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1-1").Using(CustomComparers.TypeComparison), "sections[0].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("Entry Line 1-2").Using(CustomComparers.TypeComparison), "sections[0].Box35DescriptionOfGoods_Line3");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("Entry Line 1-3").Using(CustomComparers.TypeComparison), "sections[0].Box35DescriptionOfGoods_Line4");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "sections[2].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[4].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				AssertBox35InSectionsIsEmpty(sections, 5, 12);
			});

			entryLine1.CL_Description = "Entry Line 1";
			invoiceLine1.JI_Group = "  Grouping  ";
			invoiceLine2.JI_Group = "";
			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(14));
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "sections[1].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[3].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[4].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				AssertBox35InSectionsIsEmpty(sections, 5, 14);
			});

			invoiceLine1.JI_Group = "  Grouping  ";
			invoiceLine2.JI_Group = "  Another Grouping  ";
			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(14));
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "sections[1].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[3].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Another Grouping").Using(CustomComparers.TypeComparison), "sections[5].Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[5].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[7].Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
				AssertBox35InSectionsIsEmpty(sections, 8, 14);
			});
		}

		[ExpectNoExceptions]
		void AssertBox35InSectionsIsEmpty(ImportDeclarationSectionBodyWrapper[] sections, int indexFrom, int indexTo)
		{
			for (int i = indexFrom; i < indexTo; i++)
			{
				NUnit.Framework.Assert.That(sections[i].Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "$sections[{i}].Box35DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[i].Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "$sections[{i}].Box35DescriptionOfGoods_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[i].Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "$sections[{i}].Box35DescriptionOfGoods_Line3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[i].Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "$sections[{i}].Box35DescriptionOfGoods_Line4 - should be [null] or [empty]");
			}
		}

		[ExpectNoExceptions]
		public virtual void TestSetSectionBody()
		{
			var veryLongGrouping = new string('A', 146);
			var wrapper = CreateDocumentWrapper(veryLongGrouping);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(14));
			var section1 = sections[0];
			var section2 = sections[1];
			var section3 = sections[2];
			var section4 = sections[3];
			var section5 = sections[4];
			var section6 = sections[5];
			var section7 = sections[6];
			var section8 = sections[7];
			var section9 = sections[8];
			var section10 = sections[9];
			var section11 = sections[10];
			var section12 = sections[11];
			var section13 = sections[12];
			var section14 = sections[13];
			CombineAssertions(() =>
			{
				var oneLineOfAs = new ZString('A', 35);
				NUnit.Framework.Assert.That(section1.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 11 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 12 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 13 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 14 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 11 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 12 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 13 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 14 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 11 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 12 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 13 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 14 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 11 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 12 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 13 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 14 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37Line 11 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37Line 12 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38Line 13 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section1.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38Line 14 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo(oneLineOfAs), "Box35Line 21");
				NUnit.Framework.Assert.That(section2.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 22 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 23 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 24 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 21 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 22 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 23 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 24 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 21 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 22 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 23 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 24 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 21 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 22 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 23 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 24 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 21 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 22 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 23 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section2.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 24 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 3 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo(oneLineOfAs), "Box35Line 31");
				NUnit.Framework.Assert.That(section3.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 32 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 33 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 34 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 31 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 32 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 33 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 34 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 31 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 32 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 33 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 34 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 31 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 32 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 33 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 34 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 31 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 32 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 33 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section3.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 34 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 4 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo(oneLineOfAs), "Box35Line 41");
				NUnit.Framework.Assert.That(section4.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 42 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 43 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 44 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 41 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 42 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 43 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 44 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 41 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 42 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 43 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 44 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 41 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 42 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 43 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 44 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 41 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 42 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 43 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section4.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 44 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 5 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 5 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo(oneLineOfAs), "Box35Line 51");
				NUnit.Framework.Assert.That(section5.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 52 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 53 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 54 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 51 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 52 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 53 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 54 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 51 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 52 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 53 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 54 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 51 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 52 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 53 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 54 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 51 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 52 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 53 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section5.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 54 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 6 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 6 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("AAAAAA").Using(CustomComparers.TypeComparison), "Box35Line 61");
				NUnit.Framework.Assert.That(section6.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 62 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 63 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 64 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 61 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 62 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 63 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 64 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 61 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 62 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 63 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 64 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 61 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 62 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 63 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 64 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 61 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 62 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 63 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section6.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 64 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 7 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 7 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 71 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 72 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 73 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 74 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 71 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 72 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 73 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 74 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 71 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 72 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 73 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 74 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 71 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 72 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 73 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 74 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 71 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 72 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 73 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section7.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 74 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section8.Box34ItemNumber, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "Box34ItemNumber 8");
				NUnit.Framework.Assert.That(section8.CountryCode, NUnit.Framework.Is.EqualTo("Australia-AU").Using(CustomComparers.TypeComparison), "CountryCode 8");
				NUnit.Framework.Assert.That(section8.Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 81 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section8.Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("買方料號:1560-HANSDA-503").Using(CustomComparers.TypeComparison), "Box35Line 82");
				NUnit.Framework.Assert.That(section8.Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("賣方料號:1060039601G").Using(CustomComparers.TypeComparison), "Box35Line 83");
				NUnit.Framework.Assert.That(section8.Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("JI_Description Test").Using(CustomComparers.TypeComparison), "Box35Line 84");
				NUnit.Framework.Assert.That(section8.Box39IncoTermAndCurrency, NUnit.Framework.Is.EqualTo("FOB TWD").Using(CustomComparers.TypeComparison), "Box39Line 81");
				NUnit.Framework.Assert.That(section8.Box39UnitPrice_Line1, NUnit.Framework.Is.EqualTo("44").Using(CustomComparers.TypeComparison), "Box39Line 82");
				NUnit.Framework.Assert.That(section8.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 83 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section8.Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("(RAP TWD 1,255)").Using(CustomComparers.TypeComparison), "Box39Line 84");
				NUnit.Framework.Assert.That(section8.Box43CustomsValue_Line1, NUnit.Framework.Is.EqualTo("383").Using(CustomComparers.TypeComparison), "Box43Line 81");
				NUnit.Framework.Assert.That(section8.Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("1,234").Using(CustomComparers.TypeComparison), "Box43Line 82");
				NUnit.Framework.Assert.That(section8.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 83 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section8.Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("(RAP 2,510)").Using(CustomComparers.TypeComparison), "Box43Line 84");
				NUnit.Framework.Assert.That(section8.Box40NetWeight, NUnit.Framework.Is.EqualTo("3KGM").Using(CustomComparers.TypeComparison), "Box40NetWeight 81");
				NUnit.Framework.Assert.That(section8.Box41QuantityAndUnit, NUnit.Framework.Is.EqualTo("2PKG").Using(CustomComparers.TypeComparison), "Box41QuantityAndUnit 82");
				NUnit.Framework.Assert.That(section8.Box42StatisticsQuantityAndUnit_Line1, NUnit.Framework.Is.EqualTo("(2PKG)").Using(CustomComparers.TypeComparison), "Box42StatisticsQuantityAndUnit_Line1 83");
				NUnit.Framework.Assert.That(section8.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 84 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section8.Box37ImportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("D4CBJ752251115-1").Using(CustomComparers.TypeComparison), "Box37ImportPermitNumberAndItemNumber_Line1 81");
				NUnit.Framework.Assert.That(section8.Box37ImportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("D4CBJ752251126-2").Using(CustomComparers.TypeComparison), "Box37ImportPermitNumberAndItemNumber_Line2 82");
				NUnit.Framework.Assert.That(section8.Box38CCCCode, NUnit.Framework.Is.EqualTo("8703.23.10.00-5").Using(CustomComparers.TypeComparison), "Box38CCCCode 83");
				NUnit.Framework.Assert.That(section8.Box38AssignedNumber, NUnit.Framework.Is.EqualTo("0ZZ").Using(CustomComparers.TypeComparison), "Box38AssignedNumber 84");
				NUnit.Framework.Assert.That(section9.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 9 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 9 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("BRAND: BRAND: HYUMDAI MODEL: STAREX").Using(CustomComparers.TypeComparison), "Box35Line 91");
				NUnit.Framework.Assert.That(section9.Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("商標(牌名):BRAND: HYUMDAI").Using(CustomComparers.TypeComparison), "Box35Line 92");
				NUnit.Framework.Assert.That(section9.Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("型號:STAREX").Using(CustomComparers.TypeComparison), "Box35Line 93");
				NUnit.Framework.Assert.That(section9.Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("規格:(非食品或食品添加物用途)").Using(CustomComparers.TypeComparison), "Box35Line 94");
				NUnit.Framework.Assert.That(section9.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 91 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 92 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 93 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 94 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 91 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 92 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 93 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 94 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 91 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 92 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 93 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 94 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 91 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 92 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 93 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section9.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 94 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 10 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 10 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("原報單號碼/項次:Pre Entry No-12").Using(CustomComparers.TypeComparison), "Box35Line 101");
				NUnit.Framework.Assert.That(section10.Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("輸出入許可文件號碼/項次:").Using(CustomComparers.TypeComparison), "Box35Line 102");
				NUnit.Framework.Assert.That(section10.Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("D4CBJ752251137-3").Using(CustomComparers.TypeComparison), "Box35Line 103");
				NUnit.Framework.Assert.That(section10.Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("D4CBJ752251148-4").Using(CustomComparers.TypeComparison), "Box35Line 104");
				NUnit.Framework.Assert.That(section10.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 101 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 102 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 103 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 104 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 101 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 102 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 103 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 104 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 101 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 102 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 103 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 104 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 101 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 102 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 103 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section10.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 104 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 11 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 11 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("D4CBJ752251159").Using(CustomComparers.TypeComparison), "Box35Line 111");
				NUnit.Framework.Assert.That(section11.Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("產地證明書號碼/項次:").Using(CustomComparers.TypeComparison), "Box35Line 112");
				NUnit.Framework.Assert.That(section11.Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("Certificate Origin Number-3").Using(CustomComparers.TypeComparison), "Box35Line 113");
				NUnit.Framework.Assert.That(section11.Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("主管機關指定代號:").Using(CustomComparers.TypeComparison), "Box35Line 114");
				NUnit.Framework.Assert.That(section11.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 111 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 112 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 113 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 114 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 111 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 112 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 113 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 114 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 111 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 112 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 113 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 114 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 111 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 112 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 113 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section11.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 114 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 12 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 12 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("66").Using(CustomComparers.TypeComparison), "Box35Line 121");
				NUnit.Framework.Assert.That(section12.Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("77").Using(CustomComparers.TypeComparison), "Box35Line 122");
				NUnit.Framework.Assert.That(section12.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 123 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 124 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 121 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 122 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 123 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 124 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 121 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 122 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 123 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 124 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box40NetWeight.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box40NetWeight 121 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 122 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 123 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 124 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 121 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 122 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 123 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section12.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 124 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 13 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 13 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 131 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 132 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 133 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 134 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box39IncoTermAndCurrency, NUnit.Framework.Is.EqualTo("----------").Using(CustomComparers.TypeComparison), "Box39Line 131");
				NUnit.Framework.Assert.That(section13.Box39UnitPrice_Line1, NUnit.Framework.Is.EqualTo("Total:   ").Using(CustomComparers.TypeComparison), "Box39Line 132");
				NUnit.Framework.Assert.That(section13.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 133 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 134 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box43CustomsValue_Line1, NUnit.Framework.Is.EqualTo("--------------").Using(CustomComparers.TypeComparison), "Box43Line 131");
				NUnit.Framework.Assert.That(section13.Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("383").Using(CustomComparers.TypeComparison), "Box43Line 132");
				NUnit.Framework.Assert.That(section13.Box43CustomsValue_Line3, NUnit.Framework.Is.EqualTo("vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison), "Box43Line 133");
				NUnit.Framework.Assert.That(section13.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 134 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box40NetWeight, NUnit.Framework.Is.EqualTo("----------------").Using(CustomComparers.TypeComparison), "Box40NetWeight 131");
				NUnit.Framework.Assert.That(section13.Box41QuantityAndUnit, NUnit.Framework.Is.EqualTo("3KGM").Using(CustomComparers.TypeComparison), "Box41QuantityAndUnit 132");
				NUnit.Framework.Assert.That(section13.Box42StatisticsQuantityAndUnit_Line1, NUnit.Framework.Is.EqualTo("2PKG").Using(CustomComparers.TypeComparison), "Box42StatisticsQuantityAndUnit_Line1 133");
				NUnit.Framework.Assert.That(section13.Box42StatisticsQuantityAndUnit_Line2, NUnit.Framework.Is.EqualTo("(2PKG)").Using(CustomComparers.TypeComparison), "Box42StatisticsQuantityAndUnit_Line2 134");
				NUnit.Framework.Assert.That(section13.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 131 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 132 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 133 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section13.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 134 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box34ItemNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box34ItemNumber 14 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CountryCode 14 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box35DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 141 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box35DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 142 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box35DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 143 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box35DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box35Line 144 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box39IncoTermAndCurrency.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 141 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box39UnitPrice_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 142 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box39UnitPrice_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 143 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box39Line 144 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box43CustomsValue_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 141 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("(RAP 2,510)").Using(CustomComparers.TypeComparison), "Box43Line 142");
				NUnit.Framework.Assert.That(section14.Box43CustomsValue_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 143 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box43Line 144 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box40NetWeight, NUnit.Framework.Is.EqualTo("vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison), "Box40NetWeight 141");
				NUnit.Framework.Assert.That(section14.Box41QuantityAndUnit.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box41QuantityAndUnit 142 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box42StatisticsQuantityAndUnit_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line1 143 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box42StatisticsQuantityAndUnit_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box42StatisticsQuantityAndUnit_Line2 144 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box37ImportPermitNumberAndItemNumber_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line1 141 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box37ImportPermitNumberAndItemNumber_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box37ImportPermitNumberAndItemNumber_Line2 142 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box38CCCCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38CCCCode 143 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(section14.Box38AssignedNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Box38AssignedNumber 144 - should be [null] or [empty]");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestSetSectionBodyAdjustForCell()
		{
			customizeSectionBodyRow = "6";
			var veryLongGrouping = new string('A', 146);
			var wrapper = CreateDocumentWrapper(veryLongGrouping);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(23));
			NUnit.Framework.Assert.That(sections.Select(x => x.Box35DescriptionOfGoods_Line1), NUnit.Framework.Is.EqualTo(new ZString[] {
"",
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
"AAAAAA",
"",
"BRAND: BRAND: HYUMDAI MODEL: STAREX",
"商標(牌名):BRAND: HYUMDAI",
"型號:STAREX",
"規格:(非食品或食品添加物用途)",
"原報單號碼/項次:Pre Entry No-12",
"輸出入許可文件號碼/項次:",
"D4CBJ752251137-3",
"D4CBJ752251148-4",
"D4CBJ752251159",
"產地證明書號碼/項次:",
"Certificate Origin Number-3",
"主管機關指定代號:",
"66",
"77",
"",
"" }));
		}

		[ExpectNoExceptions]
		public void TestSupplierPartNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CustomsSupplierPartNo = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Supplier Part Number in detail", ZString.Empty);

			invoiceLine.JI_CustomsSupplierPartNo = "Test Supplier Part No";
			AssertTextInDetail(entryHeader, "Supplier Part Number in detail", "賣方料號:Test Supplier Part No");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.SupplierPartNumber));
			AssertTextInDetail(entryHeader, "do not have Supplier Part Number in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestOwnerPartNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Owner Part Number in detail", ZString.Empty);

			invoiceLine.JI_CustomsOwnerPartNo = "Test Owner Part No";
			AssertTextInDetail(entryHeader, "Owner Part No in detail", "買方料號:Test Owner Part No");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.OwnerPartNumber));
			AssertTextInDetail(entryHeader, "do not have Owner Part No in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestDeclarationGoodsDescriptionInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_NDescription = ZString.Empty;
			invoiceLine.JI_Description = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Declaration Goods Description in detail", ZString.Empty);

			invoiceLine.JI_NDescription = "測試中文描述";
			invoiceLine.JI_Description = "Test English Description";
			AssertTextInDetail(entryHeader, "Declaration Goods Description in detail", @"測試中文描述
Test English Description");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.GoodsDescription));
			AssertTextInDetail(entryHeader, "do not have Declaration Goods Description in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestBrandInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.JI_BrandName = ZString.Empty;
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Brand in detail", ZString.Empty);

			invoiceLine.JI_BrandName = "Test Brand";
			AssertTextInDetail(entryHeader, "Brand in detail", "商標(牌名):Test Brand");

			invoiceLine.JI_BrandName = "Test long long long Brand";
			AssertTextInDetail(entryHeader, "long Brand in detail", @"商標(牌名):
Test long long long Brand");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.Brand));
			AssertTextInDetail(entryHeader, "do not have Brand in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestModelInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_Model = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Model in detail", ZString.Empty);

			invoiceLine.JI_Model = "Test Model";
			AssertTextInDetail(entryHeader, "Model in detail", "型號:Test Model");

			invoiceLine.JI_Model = "Test long long long long long Model";
			AssertTextInDetail(entryHeader, "long Model in detail", @"型號:
Test long long long long long Model");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.Model));
			AssertTextInDetail(entryHeader, "do not have Model in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestSpecificationInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_Compositions = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Specification in detail", ZString.Empty);

			invoiceLine.JI_Compositions = "Test Specification";
			AssertTextInDetail(entryHeader, "Specification in detail", "規格:Test Specification");

			invoiceLine.JI_Compositions = "Test long long long Specification";
			AssertTextInDetail(entryHeader, "long Specification in detail", @"規格:
Test long long long Specification");//

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.Specification));
			AssertTextInDetail(entryHeader, "do not have Specification in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestPreviousBondedEntryNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
			invoiceLine.PreviousBondedEntryLineNumber = ZInt.Zero;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Previous Bonded Entry Number in detail", ZString.Empty);

			invoiceLine.PreviousBondedEntryNumber = "Test1234567";
			AssertTextInDetail(entryHeader, "do not have Previous Bonded Entry Number in detail", ZString.Empty);

			invoiceLine.PreviousBondedEntryLineNumber = 2;
			AssertTextInDetail(entryHeader, "Previous Bonded Entry Number in detail", "原進倉報單號碼/項次:Test1234567-2");

			invoiceLine.PreviousBondedEntryNumber = "Test1234567890";
			AssertTextInDetail(entryHeader, "long Previous Bonded Entry Number in detail", @"原進倉報單號碼/項次:
Test1234567890-2");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.PreviousBondedEntryNumber));
			AssertTextInDetail(entryHeader, "do not have Previous Bonded Entry Number in detail", "");
		}

		[ExpectNoExceptions]
		public void TestPreviousEntryNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			invoiceLine.JI_PreviousEntryLineNumber = ZShort.Zero;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Previous Entry Number in detail", ZString.Empty);

			invoiceLine.JI_PreviousEntryNumber = "MMMMMMMMMMMMMM";
			invoiceLine.JI_PreviousEntryLineNumber = 2233;
			AssertTextInDetail(entryHeader, "Previous Entry Number in detail", "原報單號碼/項次:MMMMMMMMMMMMMM-2233");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			var previousEntryNumberOption = goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.PreviousEntryNumber);
			previousEntryNumberOption.Caption = "原報單號碼/項次1111111:";
			AssertTextInDetail(entryHeader, "long Previous Entry Number caption in detail", @"原報單號碼/項次1111111:
MMMMMMMMMMMMMM-2233");

			goodsDescriptionConfigs.Remove(previousEntryNumberOption);
			AssertTextInDetail(entryHeader, "do not have Previous Entry Number in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestPermitNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			var permitCusSupportingCollection = invoiceLine.PermitCusSupportingCollection;
			for (short i = 1; i <= 2; i++)
			{
				var addDoc = permitCusSupportingCollection.AddNew();
				addDoc.CSI_ReferenceNumber = "DocA12" + i.ToString();
				addDoc.CSI_LineNo = new ZShort(i);
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have permit number in detail", ZString.Empty);

			var addDoc3 = permitCusSupportingCollection.AddNew();
			addDoc3.CSI_ReferenceNumber = "DocA123";
			addDoc3.CSI_LineNo = new ZShort(3);

			AssertTextInDetail(entryHeader, "single permit number in detail", "輸出入許可文件號碼/項次:DocA123-3");

			addDoc3.CSI_ReferenceNumber = "DocA1231111111";
			AssertTextInDetail(entryHeader, "long permit number in detail", @"輸出入許可文件號碼/項次:
DocA1231111111-3");

			var addDoc4 = permitCusSupportingCollection.AddNew();
			addDoc4.CSI_ReferenceNumber = "DocA124";
			addDoc4.CSI_LineNo = new ZShort(4);

			AssertTextInDetail(entryHeader, "miltiple permit numbers in detail", @"輸出入許可文件號碼/項次:
DocA1231111111-3
DocA124-4");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.Permits));
			AssertTextInDetail(entryHeader, "do not have permit number in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestCertificateOfOriginInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			var permitCusSupportingCollection = invoiceLine.PermitCusSupportingCollection;
			var permit1 = permitCusSupportingCollection.AddNew();
			permit1.CSI_ReferenceNumber = "Doc1";
			permit1.CSI_LineNo = 1;
			AssertTextInDetail(entryHeader, "do not have Certificate Of Origin in detail", ZString.Empty);

			invoiceLine.CertificateOfOriginNumber = "TEST123456789";
			AssertTextInDetail(entryHeader, "do not have Certificate Of Origin in detail", ZString.Empty);

			var permit2 = permitCusSupportingCollection.AddNew();
			permit2.CSI_ReferenceNumber = "Doc2";
			permit2.CSI_LineNo = 2;
			AssertTextInDetail(entryHeader, "Certificate Of Origin in detail", "產地證明書號碼/項次:TEST123456789");

			invoiceLine.CertificateOfOriginNumberItemNumber = 3;
			AssertTextInDetail(entryHeader, "Certificate Of Origin in detail", @"產地證明書號碼/項次:TEST123456789-3");

			invoiceLine.CertificateOfOriginNumber = "TEST1234567890000000000000";
			AssertTextInDetail(entryHeader, "Long Certificate Of Origin in detail", @"產地證明書號碼/項次:
TEST1234567890000000000000-3");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.CertificateOfOrigin));
			AssertTextInDetail(entryHeader, "do not have Certificate Of Origin in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestSHTCImportPermitInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.HighTechLicense = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have SHTC Import Permit in detail", ZString.Empty);

			invoiceLine.HighTechLicense = "T";
			AssertTextInDetail(entryHeader, "SHTC Import Permit in detail", "戰略性高科技貨品國際進口證明號碼:T");

			invoiceLine.HighTechLicense = "Test HighTechLicense";
			AssertTextInDetail(entryHeader, "long SHTC Import Permit in detail", @"戰略性高科技貨品國際進口證明號碼:
Test HighTechLicense");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.SHTCImportPermit));
			AssertTextInDetail(entryHeader, "do not have SHTC Import Permit in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestCITESImportPermitInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.CitesPermit = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Cites Permit in detail", ZString.Empty);

			invoiceLine.CitesPermit = "TT";
			AssertTextInDetail(entryHeader, "Cites Permit in detail", "華盛頓公約進口許可證號碼:TT");

			invoiceLine.CitesPermit = "Test long Cites Permit";
			AssertTextInDetail(entryHeader, "long Cites Permit in detail", @"華盛頓公約進口許可證號碼:
Test long Cites Permit");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.CITESImportPermit));
			AssertTextInDetail(entryHeader, "do not have Cites Permit in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestAssignedNumbersInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			var assignedJobComInvLineRefsCollection = invoiceLine.AssignedJobComInvLineRefsCollection;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();

			AssertTextInDetail(entryHeader, "do not have Assigned Number in detail", ZString.Empty);

			var assignedNumber1 = assignedJobComInvLineRefsCollection.AddNew();
			assignedNumber1.JG_ReferenceNumber = "test number 1";

			AssertTextInDetail(entryHeader, "do not have Assigned Number in detail", ZString.Empty);

			var assignedNumber2 = assignedJobComInvLineRefsCollection.AddNew();
			assignedNumber2.JG_ReferenceNumber = "test number 2";
			AssertTextInDetail(entryHeader, "single Assigned Number in detail", "主管機關指定代號:test number 2");

			assignedNumber2.JG_ReferenceNumber = "test long long long number 2";
			AssertTextInDetail(entryHeader, "long Assigned number in detail", @"主管機關指定代號:
test long long long number 2");

			assignedNumber2.JG_ReferenceNumber = "test number 2";
			var assignedNumber3 = assignedJobComInvLineRefsCollection.AddNew();
			assignedNumber3.JG_ReferenceNumber = "test number 3";

			AssertTextInDetail(entryHeader, "miltiple Assigned Numbers in detail", @"主管機關指定代號:
test number 2
test number 3");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ImportDeclarationDocumentFieldList.Codes.AssignedNumbers));
			AssertTextInDetail(entryHeader, "do not have Assigned Number in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		void AssertTextInDetail(CusEntryHeader entryHeader, ZString message, ZString expectedNumber)
		{
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			var detail = new ZStringBuilder();
			foreach (var section in wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>())
			{
				detail.Append(section.Box35DescriptionOfGoods_Line1);
				detail.Append(section.Box35DescriptionOfGoods_Line2);
				detail.Append(section.Box35DescriptionOfGoods_Line3);
				detail.Append(section.Box35DescriptionOfGoods_Line4);
			}
			NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends().Trim(), NUnit.Framework.Is.EqualTo(expectedNumber).Using(CustomComparers.TypeComparison), message.ToString());
		}

		[ExpectNoExceptions]
		public void TestRAPAndROR()
		{
			var grouping = new string('A', 10);
			var entryHeader = GenerateEntryHeader(grouping);
			var invoiceLine1 = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			invoiceLine1.JI_InvoiceQuantity = 33;

			var invoiceHeader = declaration.Invoices.First();
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_Procedure = "38";
			invoiceLine2.JI_UseOneTenthCV = ZBool.False;
			invoiceLine2.JI_RAPPrice = 510m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			var sectionItems = wrapper.Sections;
			var section = sectionItems.Cast<ImportDeclarationSectionBodyWrapper>().ElementAt(10);
			NUnit.Framework.Assert.That(section.Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("(RAP 2,510)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box43CustomsValue_Line3, NUnit.Framework.Is.EqualTo("(ROR 510)").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRAPPrice()
		{
			var veryLongGrouping = new string('A', 150);
			var entryHeader = GenerateEntryHeader(veryLongGrouping);
			var invoiceLine1 = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			invoiceLine1.JI_InvoiceQuantity = 33;
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			var sectionItems = wrapper.Sections;
			var sections = sectionItems.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			var section6 = sections[7];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section6.Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("(RAP TWD 76.060606)").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(section6.Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("(RAP 2,510)").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[13].Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("(RAP 2,510)").Using(CustomComparers.TypeComparison));
			});
			invoiceLine1.JI_UseOneTenthCV = ZBool.True;
			invoiceLine1.CusEntryLine.CL_CustomsValue = 3300;
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			sectionItems = wrapper.Sections;
			sections = sectionItems.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			section6 = sections[7];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section6.Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("(RAP TWD 10)").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(section6.Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("(RAP 330)").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[13].Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("(RAP 330)").Using(CustomComparers.TypeComparison));
			});
			invoiceLine1.JI_Procedure = "31";
			invoiceLine1.JI_RAPPrice = 2510m;
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				foreach (var item in sections)
				{
					NUnit.Framework.Assert.That(item.Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(item.Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				}
				NUnit.Framework.Assert.That(sections[13].Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo(ZString.Empty));
			});
			invoiceLine1.JI_Procedure = "37";
			invoiceLine1.JI_RAPPrice = 0m;
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(14), "Length");
				NUnit.Framework.Assert.That(sections[0].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 1 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[0].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 1 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 2 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 2 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 3 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 3 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 4 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[3].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 4 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 5 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[4].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 5 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 6 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 6 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 7 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[6].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 7 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[7].Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("(RAP TWD 0)").Using(CustomComparers.TypeComparison), "Item 8 Box39RorRapUnitPrice");
				NUnit.Framework.Assert.That(sections[7].Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("(RAP 0)").Using(CustomComparers.TypeComparison), "Item 8 Box43RorRapCustomsValue");
				NUnit.Framework.Assert.That(sections[8].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 9 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[8].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 9 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 10 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[9].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 10 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 11 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[10].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 11 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[11].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 12 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[11].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 12 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 13 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[12].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 13 Box43RorRapCustomsValue - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[13].Box43CustomsValue_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 13 Box43CustomsValue_Line2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[13].Box39RorRapUnitPrice.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 14 Box39RorRapUnitPrice - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[13].Box43RorRapCustomsValue.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Item 14 Box43RorRapCustomsValue - should be [null] or [empty]");
			});
		}

		[ExpectNoExceptions]
		public void TestRORPrice()
		{
			var veryLongGrouping = new string('A', 150);
			var entryHeader = GenerateEntryHeader(veryLongGrouping);
			var invoiceLine1 = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_UseOneTenthCV = ZBool.False;
			invoiceLine1.JI_RAPPrice = 2510m;
			invoiceLine1.JI_InvoiceQuantity = 33;
			var wrapper = GetDocumentWrapper(entryHeader, Factory);
			var sectionItems = wrapper.Sections;
			var sections = sectionItems.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			var section6 = sections[7];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section6.Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("(ROR TWD 76.060606)").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(section6.Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("(ROR 2,510)").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[13].Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("(ROR 2,510)").Using(CustomComparers.TypeComparison));
			});
			invoiceLine1.JI_UseOneTenthCV = ZBool.True;
			invoiceLine1.CusEntryLine.CL_CustomsValue = 3300;
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			sectionItems = wrapper.Sections;
			sections = sectionItems.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			section6 = sections[7];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section6.Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("(ROR TWD 10)").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(section6.Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("(ROR 330)").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[13].Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo("(ROR 330)").Using(CustomComparers.TypeComparison));
			});
			invoiceLine1.JI_Procedure = "31";
			invoiceLine1.JI_RAPPrice = 2510m;
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				foreach (var item in sections)
				{
					NUnit.Framework.Assert.That(item.Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(item.Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				}
				NUnit.Framework.Assert.That(sections[13].Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo(ZString.Empty));
			});
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_UseOneTenthCV = ZBool.False;
			invoiceLine1.JI_RAPPrice = 0m;
			wrapper = GetDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				foreach (var item in sections)
				{
					NUnit.Framework.Assert.That(item.Box39RorRapUnitPrice, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(item.Box43RorRapCustomsValue, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				}
				NUnit.Framework.Assert.That(sections[13].Box43CustomsValue_Line2, NUnit.Framework.Is.EqualTo(ZString.Empty));
			});
		}

		[ExpectNoExceptions]
		public void TestJobNumber()
		{
			var header = GenerateEntryHeader();
			header.Declaration.JE_DeclarationReference = "R1234567";
			var wrapper = GetDocumentWrapper(header, Factory);
			NUnit.Framework.Assert.That(wrapper.JobNumber, NUnit.Framework.Is.EqualTo("R1234567").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOwnerReference()
		{
			var header = GenerateEntryHeader();
			header.Declaration.JE_OwnerRef = "OwnerRef1234";
			var wrapper = GetDocumentWrapper(header, Factory);
			NUnit.Framework.Assert.That(wrapper.OwnerReference, NUnit.Framework.Is.EqualTo("OwnerRef1234").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEntryReleaseDate()
		{
			var header = GenerateEntryHeader();
			header.CH_EntryReleaseDate = new ZDateTime(2020, 4, 28);
			var wrapper = GetDocumentWrapper(header, Factory);
			NUnit.Framework.Assert.That(wrapper.EntryReleaseDate, NUnit.Framework.Is.EqualTo("2020/04/28").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCreateUser()
		{
			var expectedUser = Factory.NewWithValidTestData<GlbStaff>();
			expectedUser.GS_Code = "AAA";
			expectedUser.GS_FullName = "AAA's Full Name";
			var header = GenerateEntryHeader();
			header.Declaration.JE_SystemCreateUser = "AAA";
			var wrapper = GetDocumentWrapper(header, Factory);
			NUnit.Framework.Assert.That(wrapper.CreateUser, NUnit.Framework.Is.EqualTo("AAA's Full Name").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetBox37Line1()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			invoiceLine.ExemptionOfControllingAgenciesCusSupportings.RemoveAndDeleteAll();
			var cusSupportingCollection = invoiceLine.PermitCusSupportingCollection;
			var cusSupporting = cusSupportingCollection.First();
			cusSupporting.CSI_ReferenceNumber = "D4CBJ752251115";
			cusSupporting.CSI_LineNo = ZShort.Zero;
			invoiceLine.CertificateOfOriginNumber = "ABC123456";
			invoiceLine.CertificateOfOriginNumberItemNumber = 1;
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("D4CBJ752251115").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("D4CBJ752251126-2").Using(CustomComparers.TypeComparison));
			});
			cusSupporting.CSI_LineNo = 9;
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("D4CBJ752251115-9").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("D4CBJ752251126-2").Using(CustomComparers.TypeComparison));
			});
			cusSupportingCollection.RemoveAndDeleteAll();
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("ABC123456-1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			});

			invoiceLine.CertificateOfOriginNumber = "";
			invoiceLine.CertificateOfOriginNumberItemNumber = 0;
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestSetBox37Line2()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			invoiceLine.ExemptionOfControllingAgenciesCusSupportings.RemoveAndDeleteAll();
			var cusSupportingCollection = invoiceLine.PermitCusSupportingCollection;
			cusSupportingCollection.RemoveAndDeleteAll();
			var cusSupporting1 = cusSupportingCollection.AddNew();
			cusSupporting1.CSI_ReferenceNumber = "D4CBJ752251115";
			cusSupporting1.CSI_LineNo = 1;
			var cusSupporting2 = cusSupportingCollection.AddNew();
			cusSupporting2.CSI_ReferenceNumber = "D4CBJ752251116";
			cusSupporting2.CSI_LineNo = 0;
			invoiceLine.CertificateOfOriginNumber = "ABC123456";
			invoiceLine.CertificateOfOriginNumberItemNumber = 1;
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("D4CBJ752251116").Using(CustomComparers.TypeComparison));
			cusSupporting2.CSI_LineNo = 2;
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("D4CBJ752251116-2").Using(CustomComparers.TypeComparison));
			cusSupportingCollection.RemoveAndDelete(cusSupporting2);
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections[3].Box37ImportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("ABC123456-1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPrintOriginAdditionalDocumentOnGoodsDescriptionWhenNotPrintedOnImportPermitNoAndItemNo()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			invoiceLine.ExemptionOfControllingAgenciesCusSupportings.RemoveAndDeleteAll();
			var cusSupportingCollection = invoiceLine.PermitCusSupportingCollection;
			cusSupportingCollection.RemoveAndDeleteAll();
			var cusSupporting1 = cusSupportingCollection.AddNew();
			cusSupporting1.CSI_ReferenceNumber = "D4CBJ752251115";
			cusSupporting1.CSI_LineNo = 1;
			var cusSupporting2 = cusSupportingCollection.AddNew();
			cusSupporting2.CSI_ReferenceNumber = "D4CBJ752251116";
			cusSupporting2.CSI_LineNo = 0;
			invoiceLine.CertificateOfOriginNumber = "ABC123456";
			invoiceLine.CertificateOfOriginNumberItemNumber = 1;
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections[5].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("產地證明書號碼/項次:ABC123456-1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeclDocTypeCodeAndDescription()
		{
			var header = GenerateEntryHeader();
			var declaration = header.Declaration;
			var wrapper = GetDocumentWrapper(header, Factory);
			NUnit.Framework.Assert.That(wrapper.DeclDocTypeCodeAndDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DeclDocTypeCodeAndDescription");
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.ExportCustoms;
			NUnit.Framework.Assert.That(wrapper.DeclDocTypeCodeAndDescription, NUnit.Framework.Is.EqualTo("5-出口證明用\r\n聯").Using(CustomComparers.TypeComparison), "DeclDocTypeCodeAndDescription");
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DeclDocType = ImportDeclDocTypeList.Codes.ImportCustoms;
			NUnit.Framework.Assert.That(wrapper.DeclDocTypeCodeAndDescription, NUnit.Framework.Is.EqualTo("2-進口證明用\r\n聯").Using(CustomComparers.TypeComparison), "DeclDocTypeCodeAndDescription");
		}

		[ExpectNoExceptions]
		public void TestDeclarationTypeDescription()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			NUnit.Framework.Assert.That(wrapper.TypeOfDeclaration, NUnit.Framework.Is.EqualTo("G1").Using(CustomComparers.TypeComparison), "TypeOfDeclaration");
			NUnit.Framework.Assert.That(wrapper.DeclarationTypeDescription, NUnit.Framework.Is.EqualTo("外貨進口").Using(CustomComparers.TypeComparison), "DeclarationTypeDescription");
			entryInstruction.CEI_Style = "D7";
			NUnit.Framework.Assert.That(wrapper.TypeOfDeclaration, NUnit.Framework.Is.EqualTo("D7").Using(CustomComparers.TypeComparison), "TypeOfDeclaration");
			NUnit.Framework.Assert.That(wrapper.DeclarationTypeDescription, NUnit.Framework.Is.EqualTo("保稅倉相互轉儲或運\r\n往保稅廠").Using(CustomComparers.TypeComparison), "DeclarationTypeDescription");
		}

		[TestDate(2020, 6, 2)]
		[ExpectNoExceptions]
		public void TestTariffChapterNameWithCustomsOfficeStartWithC()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			entryHeader.EntryNumber = "CA  0958000143";
			new TestTWCreator(Factory).CreateExchangeRateToUSD();
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			NUnit.Framework.Assert.That(!(wrapper.TotalCIFAmountInUSD > 10000m), NUnit.Framework.Is.True, "less or equal to 10,000 then print 小單");
			invoiceLine.JI_Tariff = "000038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "380038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "710071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "840084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "850100xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "853899xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "853900xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "899999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "900000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "979999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000006";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000000";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000005";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000007";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			entryHeader.MergedLines.Cast<CusEntryLine>().First().CL_CustomsValue = 310000.31;
			entryHeader.ResetIsCustomsValueCalculated();
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.EqualTo(10000.01m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.GreaterThan(10000m).Using(CustomComparers.TypeComparison), "more than 10,000 then print 大單");
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "853000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "854000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "912345xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課三股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "X";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "NoStartWithC";
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858501to858538";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858539to89";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "900097orSpec";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var otherInvoiceLine1 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine1.JI_RAPCurr = "TWD";
			otherInvoiceLine1.JI_InvoiceQuantity = 2400;
			otherInvoiceLine1.JI_EnteredUnitPrice = 50m;
			var otherInvoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine2.JI_RAPCurr = "TWD";
			otherInvoiceLine2.JI_InvoiceQuantity = 2400;
			otherInvoiceLine2.JI_EnteredUnitPrice = 50m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			entryHeader.EntryNumber = "CA  0958000143";
			invoiceLine.JI_Tariff = "010038xxxxx";
			otherInvoiceLine1.JI_Tariff = "390071xxxxx";
			otherInvoiceLine2.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 6, 2)]
		[ExpectNoExceptions]
		public void TestTariffChapterNameWithCustomsOfficeStartWithA()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			entryHeader.EntryNumber = "AA  0958000143";
			new TestTWCreator(Factory).CreateExchangeRateToUSD();
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);

			NUnit.Framework.Assert.That(!(wrapper.TotalCIFAmountInUSD > 10000m), NUnit.Framework.Is.True, "less or equal to 10,000 then print 小單");
			invoiceLine.JI_Tariff = "000038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "219938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "220071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "339971xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "340084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "439984xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "440000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "639999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "640000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "839999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "860000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "879999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課三股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "840000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "849999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "900000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "909999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "850000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "859999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "880000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "899999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課三股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "910000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "979999xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課三股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "98990000000";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000005";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000007";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			entryHeader.MergedLines.Cast<CusEntryLine>().First().CL_CustomsValue = 310000.31;
			entryHeader.ResetIsCustomsValueCalculated();
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.EqualTo(10000.01m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.GreaterThan(10000m).Using(CustomComparers.TypeComparison), "more than 10,000 then print 大單");
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "220071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "340084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "440000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "640000xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "860045xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課三股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "840045xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "900045xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "850045xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "880045xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課三股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "910045xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估三課三股/大單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "X";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "NoStartWithA";
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858501to858538";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858539to89";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "900097orSpec";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var otherInvoiceLine1 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine1.JI_RAPCurr = "TWD";
			otherInvoiceLine1.JI_InvoiceQuantity = 2400;
			otherInvoiceLine1.JI_EnteredUnitPrice = 50m;

			var otherInvoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine2.JI_RAPCurr = "TWD";
			otherInvoiceLine2.JI_InvoiceQuantity = 2400;
			otherInvoiceLine2.JI_EnteredUnitPrice = 50m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			entryHeader.EntryNumber = "AA  0958000143";
			invoiceLine.JI_Tariff = "010038xxxxx";
			otherInvoiceLine1.JI_Tariff = "390071xxxxx";
			otherInvoiceLine2.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 6, 2)]
		[ExpectNoExceptions]
		public void TestTariffChapterNameWithCustomsOfficeStartWithD()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			entryHeader.EntryNumber = "DA  0958000143";
			new TestTWCreator(Factory).CreateExchangeRateToUSD();
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);

			NUnit.Framework.Assert.That(!(wrapper.TotalCIFAmountInUSD > 10000m), NUnit.Framework.Is.True, "less or equal to 10,000 then print 小單");
			invoiceLine.JI_Tariff = "000038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "269938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "680038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "719938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "980038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "989938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "270071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "679971xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "979984xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "99990000000";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "99990000005";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "99990000007";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			entryHeader.MergedLines.Cast<CusEntryLine>().First().CL_CustomsValue = 310000.31;
			entryHeader.ResetIsCustomsValueCalculated();
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.EqualTo(10000.01m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.GreaterThan(10000m).Using(CustomComparers.TypeComparison), "more than 10,000 then print 大單");
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "680038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "980038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "270071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/大單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "X";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "NoStartWithD";
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858501to858538";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858539to89";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "900097orSpec";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var otherInvoiceLine1 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine1.JI_RAPCurr = "TWD";
			otherInvoiceLine1.JI_InvoiceQuantity = 2400;
			otherInvoiceLine1.JI_EnteredUnitPrice = 50m;

			var otherInvoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine2.JI_RAPCurr = "TWD";
			otherInvoiceLine2.JI_InvoiceQuantity = 2400;
			otherInvoiceLine2.JI_EnteredUnitPrice = 50m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			entryHeader.EntryNumber = "DA  0958000143";
			invoiceLine.JI_Tariff = "010038xxxxx";
			otherInvoiceLine1.JI_Tariff = "390071xxxxx";
			otherInvoiceLine2.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 6, 2)]
		[ExpectNoExceptions]
		public void TestTariffChapterNameWithCustomsOfficeStartWithBAorBJ()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			new TestTWCreator(Factory).CreateExchangeRateToUSD();

			AssertTariffChapterNameWithCustomsOfficeStartWithBAorBJ("BA");
			AssertTariffChapterNameWithCustomsOfficeStartWithBAorBJ("BJ");

			void AssertTariffChapterNameWithCustomsOfficeStartWithBAorBJ(ZString customsOffice)
			{
				entryHeader.Declaration.InvoiceLines.DeleteAll();
				var invoiceLine = entryHeader.Declaration.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
				invoiceLine.JI_CustomsQuantity = 532;
				invoiceLine.JI_InvoiceQuantity = 2;
				invoiceLine.JI_InvoiceUQ = "PKG";
				invoiceLine.JI_LinePrice = 88m;
				invoiceLine.JI_VatPymntMthd = "CAS";

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
				entryHeader.EntryNumber = ZString.Format("{0}  0958000143", customsOffice);
				var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);

				NUnit.Framework.Assert.That(!(wrapper.TotalCIFAmountInUSD > 10000m), NUnit.Framework.Is.True, "less or equal to 10,000 then print 小單");
				invoiceLine.JI_Tariff = "000038xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_Tariff = "010038xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_Tariff = "719938xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

				invoiceLine.JI_Tariff = "720038xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_Tariff = "979938xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));

				invoiceLine.JI_Tariff = "98990000000";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_Tariff = "98990000005";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_Tariff = "98990000007";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

				entryHeader.MergedLines.Cast<CusEntryLine>().First().CL_CustomsValue = 310000.31;
				entryHeader.ResetIsCustomsValueCalculated();
				NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.EqualTo(10000.01m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.GreaterThan(10000m).Using(CustomComparers.TypeComparison), "more than 10,000 then print 大單");
				invoiceLine.JI_Tariff = "100038xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
				invoiceLine.JI_Tariff = "720071xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/大單").Using(CustomComparers.TypeComparison));

				invoiceLine.JI_Tariff = "X";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
				entryHeader.EntryNumber = "NoStartWithBABJ";
				invoiceLine.JI_Tariff = "100038xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
				invoiceLine.JI_Tariff = "390071xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
				invoiceLine.JI_Tariff = "720084xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
				invoiceLine.JI_Tariff = "858501to858538";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
				invoiceLine.JI_Tariff = "858539to89";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
				invoiceLine.JI_Tariff = "900097orSpec";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));

				var otherInvoiceLine1 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
				otherInvoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
				otherInvoiceLine1.JI_RAPCurr = "TWD";
				otherInvoiceLine1.JI_InvoiceQuantity = 2400;
				otherInvoiceLine1.JI_EnteredUnitPrice = 50m;

				var otherInvoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
				otherInvoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
				otherInvoiceLine2.JI_RAPCurr = "TWD";
				otherInvoiceLine2.JI_InvoiceQuantity = 2400;
				otherInvoiceLine2.JI_EnteredUnitPrice = 50m;

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
				wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
				entryHeader.EntryNumber = ZString.Format("{0}  0958000143", customsOffice);
				invoiceLine.JI_Tariff = "010038xxxxx";
				otherInvoiceLine1.JI_Tariff = "720071xxxxx";
				otherInvoiceLine2.JI_Tariff = "720071xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

				invoiceLine.JI_Tariff = "720071xxxxx";
				otherInvoiceLine1.JI_Tariff = "010038xxxxx";
				otherInvoiceLine2.JI_Tariff = "010038xxxxx";
				NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			}
		}

		[TestDate(2020, 6, 2)]
		[ExpectNoExceptions]
		public void TestTariffChapterNameWithCustomsOfficeStartWithBC()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			entryHeader.EntryNumber = "BC  0958000143";
			new TestTWCreator(Factory).CreateExchangeRateToUSD();
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);

			NUnit.Framework.Assert.That(!(wrapper.TotalCIFAmountInUSD > 10000m), NUnit.Framework.Is.True, "less or equal to 10,000 then print 小單");
			invoiceLine.JI_Tariff = "000038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "069938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "130038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "169938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "070038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "129938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "170038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "249938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "250038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "409938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "410038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "709938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "710038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "839938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "900038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "979938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "840038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "899938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "98990000000";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000005";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000007";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			entryHeader.MergedLines.Cast<CusEntryLine>().First().CL_CustomsValue = 310000.31;
			entryHeader.ResetIsCustomsValueCalculated();
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.EqualTo(10000.01m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.GreaterThan(10000m).Using(CustomComparers.TypeComparison), "more than 10,000 then print 大單");
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "070071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "250038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "410071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "710038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "840071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/大單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "X";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "NoStartWithBC";
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858501to858538";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858539to89";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "900097orSpec";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var otherInvoiceLine1 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine1.JI_RAPCurr = "TWD";
			otherInvoiceLine1.JI_InvoiceQuantity = 2400;
			otherInvoiceLine1.JI_EnteredUnitPrice = 50m;

			var otherInvoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine2.JI_RAPCurr = "TWD";
			otherInvoiceLine2.JI_InvoiceQuantity = 2400;
			otherInvoiceLine2.JI_EnteredUnitPrice = 50m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			entryHeader.EntryNumber = "BC  0958000143";
			invoiceLine.JI_Tariff = "010038xxxxx";
			otherInvoiceLine1.JI_Tariff = "720071xxxxx";
			otherInvoiceLine2.JI_Tariff = "720071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "720071xxxxx";
			otherInvoiceLine1.JI_Tariff = "010038xxxxx";
			otherInvoiceLine2.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 6, 2)]
		[ExpectNoExceptions]
		public void TestTariffChapterNameWithCustomsOfficeStartWithBD()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			entryHeader.EntryNumber = "BD  0958000143";
			new TestTWCreator(Factory).CreateExchangeRateToUSD();
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);

			NUnit.Framework.Assert.That(!(wrapper.TotalCIFAmountInUSD > 10000m), NUnit.Framework.Is.True, "less or equal to 10,000 then print 小單");
			invoiceLine.JI_Tariff = "000038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "249938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "250038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "409938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "410038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "639938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "640038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "719938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "720038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "839938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "900038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "979938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "840038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "899938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "98990000000";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000005";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000007";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			entryHeader.MergedLines.Cast<CusEntryLine>().First().CL_CustomsValue = 310000.31;
			entryHeader.ResetIsCustomsValueCalculated();
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.EqualTo(10000.01m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.GreaterThan(10000m).Using(CustomComparers.TypeComparison), "more than 10,000 then print 大單");
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "250071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "410038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "640071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "820038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "840071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課二股/大單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "X";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "NoStartWithBD";
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858501to858538";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858539to89";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "900097orSpec";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var otherInvoiceLine1 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine1.JI_RAPCurr = "TWD";
			otherInvoiceLine1.JI_InvoiceQuantity = 2400;
			otherInvoiceLine1.JI_EnteredUnitPrice = 50m;

			var otherInvoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine2.JI_RAPCurr = "TWD";
			otherInvoiceLine2.JI_InvoiceQuantity = 2400;
			otherInvoiceLine2.JI_EnteredUnitPrice = 50m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			entryHeader.EntryNumber = "BD  0958000143";
			invoiceLine.JI_Tariff = "010038xxxxx";
			otherInvoiceLine1.JI_Tariff = "720071xxxxx";
			otherInvoiceLine2.JI_Tariff = "720071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "720071xxxxx";
			otherInvoiceLine1.JI_Tariff = "010038xxxxx";
			otherInvoiceLine2.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二課一股/小單").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 6, 2)]
		[ExpectNoExceptions]
		public void TestTariffChapterNameWithCustomsOfficeStartWithBE()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			entryHeader.EntryNumber = "BE  0958000143";
			new TestTWCreator(Factory).CreateExchangeRateToUSD();
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);

			NUnit.Framework.Assert.That(!(wrapper.TotalCIFAmountInUSD > 10000m), NUnit.Framework.Is.True, "less or equal to 10,000 then print 小單");
			invoiceLine.JI_Tariff = "000038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "229938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "230038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "469938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "470038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "739938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "740038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "979938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "98990000000";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000005";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "98990000007";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			entryHeader.MergedLines.Cast<CusEntryLine>().First().CL_CustomsValue = 310000.31;
			entryHeader.ResetIsCustomsValueCalculated();
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.EqualTo(10000.01m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.GreaterThan(10000m).Using(CustomComparers.TypeComparison), "more than 10,000 then print 大單");
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "230071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課二股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "470038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "740071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課四股/大單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "X";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "NoStartWithBE";
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858501to858538";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858539to89";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "900097orSpec";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var otherInvoiceLine1 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine1.JI_RAPCurr = "TWD";
			otherInvoiceLine1.JI_InvoiceQuantity = 2400;
			otherInvoiceLine1.JI_EnteredUnitPrice = 50m;

			var otherInvoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine2.JI_RAPCurr = "TWD";
			otherInvoiceLine2.JI_InvoiceQuantity = 2400;
			otherInvoiceLine2.JI_EnteredUnitPrice = 50m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			entryHeader.EntryNumber = "BE  0958000143";
			invoiceLine.JI_Tariff = "010038xxxxx";
			otherInvoiceLine1.JI_Tariff = "720071xxxxx";
			otherInvoiceLine2.JI_Tariff = "720071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "720071xxxxx";
			otherInvoiceLine1.JI_Tariff = "010038xxxxx";
			otherInvoiceLine2.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一課三股/小單").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 6, 2)]
		[ExpectNoExceptions]
		public void TestTariffChapterNameWithCustomsOfficeStartWithBF()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var invoiceLine = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			entryHeader.EntryNumber = "BF  0958000143";
			new TestTWCreator(Factory).CreateExchangeRateToUSD();
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);

			NUnit.Framework.Assert.That(!(wrapper.TotalCIFAmountInUSD > 10000m), NUnit.Framework.Is.True, "less or equal to 10,000 then print 小單");
			invoiceLine.JI_Tariff = "000038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "839938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "840038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二股/小單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "989938xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "99990000000";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "99990000005";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "99990000007";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			entryHeader.MergedLines.Cast<CusEntryLine>().First().CL_CustomsValue = 310000.31;
			entryHeader.ResetIsCustomsValueCalculated();
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.EqualTo(10000.01m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.TotalCIFAmountInUSD, NUnit.Framework.Is.GreaterThan(10000m).Using(CustomComparers.TypeComparison), "more than 10,000 then print 大單");
			invoiceLine.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一股/大單").Using(CustomComparers.TypeComparison));
			invoiceLine.JI_Tariff = "840071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二股/大單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "X";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "NoStartWithBF";
			invoiceLine.JI_Tariff = "100038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "390071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "720084xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858501to858538";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "858539to89";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			invoiceLine.JI_Tariff = "900097orSpec";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var otherInvoiceLine1 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine1.JI_RAPCurr = "TWD";
			otherInvoiceLine1.JI_InvoiceQuantity = 2400;
			otherInvoiceLine1.JI_EnteredUnitPrice = 50m;

			var otherInvoiceLine2 = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			otherInvoiceLine2.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			otherInvoiceLine2.JI_RAPCurr = "TWD";
			otherInvoiceLine2.JI_InvoiceQuantity = 2400;
			otherInvoiceLine2.JI_EnteredUnitPrice = 50m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			entryHeader.EntryNumber = "BF  0958000143";
			invoiceLine.JI_Tariff = "010038xxxxx";
			otherInvoiceLine1.JI_Tariff = "840071xxxxx";
			otherInvoiceLine2.JI_Tariff = "840071xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估一股/小單").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "840071xxxxx";
			otherInvoiceLine1.JI_Tariff = "010038xxxxx";
			otherInvoiceLine2.JI_Tariff = "010038xxxxx";
			NUnit.Framework.Assert.That(wrapper.TariffChapterName, NUnit.Framework.Is.EqualTo("分估二股/小單").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNameOfVesselOrTransportId()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var declaration = entryHeader.Declaration;
			declaration.JE_VesselName = "Vessel";
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			NUnit.Framework.Assert.That(wrapper.NameOfVesselOrTransportId, NUnit.Framework.Is.EqualTo("Vessel").Using(CustomComparers.TypeComparison));
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "CA217";
			NUnit.Framework.Assert.That(wrapper.NameOfVesselOrTransportId, NUnit.Framework.Is.EqualTo("CA217").Using(CustomComparers.TypeComparison));
			declaration.JE_TransportMode = "XXX";
			declaration.JE_VesselName = "Vessel";
			declaration.JE_VoyageFlightNo = "CA217";
			NUnit.Framework.Assert.That(wrapper.NameOfVesselOrTransportId, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestBox39Line11Box39Line21Box43Line11Box43Line21()
		{
			var section = new ImportDeclarationSectionBodyWrapper { IsNotPrintEmpty = true };
			section.AddPriceItem("Price1");
			section.AddPriceItem("Price2");
			section.AddPriceItem("Price3");
			section.AddDutyPayingValueItem("FOBValue1");
			section.AddDutyPayingValueItem("FOBValue2");
			NUnit.Framework.Assert.That(section.Box39Line11, NUnit.Framework.Is.EqualTo("Price1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box39Line21, NUnit.Framework.Is.EqualTo("(本欄空白)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box43Line11, NUnit.Framework.Is.EqualTo("(本欄空白)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box43Line21, NUnit.Framework.Is.EqualTo(ZString.Empty));
			section.IsNotPrintEmpty = false;
			NUnit.Framework.Assert.That(section.Box39Line11, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(section.Box39Line21, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(section.Box43Line11, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(section.Box43Line21, NUnit.Framework.Is.EqualTo(ZString.Empty));
			section.IsNotPrintEmpty = false;
			section.IsTotal = true;
			NUnit.Framework.Assert.That(section.Box39Line11, NUnit.Framework.Is.EqualTo("Price1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box39Line21, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(section.Box43Line11, NUnit.Framework.Is.EqualTo("(本欄空白)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section.Box43Line21, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestTrademarkImage()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			var declaration = entryHeader.Declaration;
			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var doc1 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);
			var invoiceLine1 = (JobComInvoiceLine)entryHeader.InvoiceLines.First();
			invoiceLine1.JI_InvoiceQuantity = 33;
			invoiceLine1.TrademarkStorageDocsGuid = doc1.UniqueKey;
			var wrapper = GetDocumentWrapper(entryHeader, entryHeader.Factory);
			var sectionItems = wrapper.Sections;
			var sections = sectionItems.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections[0].TrademarkImage, NUnit.Framework.Is.EqualTo(default(Image)));
			NUnit.Framework.Assert.That(sections[1].TrademarkImage, NUnit.Framework.Is.EqualTo(default(Image)));
			NUnit.Framework.Assert.That(sections[2].TrademarkImage, NUnit.Framework.Is.EqualTo(default(Image)));
			NUnit.Framework.Assert.That(sections[3].TrademarkImage, NUnit.Framework.Is.Not.EqualTo(default(Image)));
			NUnit.Framework.Assert.That(sections[4].TrademarkImage, NUnit.Framework.Is.EqualTo(default(Image)));
			NUnit.Framework.Assert.That(sections[5].TrademarkImage, NUnit.Framework.Is.EqualTo(default(Image)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestDetailsWithTrademarkImage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text";
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text";
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_CL = entryLine2.PK;
			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var doc1 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);
			invoiceLine1.TrademarkStorageDocsGuid = doc1.UniqueKey;
			declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			NUnit.Framework.Assert.That(sections[0].TrademarkImage, NUnit.Framework.Is.Not.EqualTo(default(Image)));
			NUnit.Framework.Assert.That(sections[1].TrademarkImage, NUnit.Framework.Is.EqualTo(default(Image)));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Section 0 Details 1");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line").Using(CustomComparers.TypeComparison), "Section 0 Details 2");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("text Entry Line text Entry").Using(CustomComparers.TypeComparison), "Section 0 Details 3");
				NUnit.Framework.Assert.That(sections[0].Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 0 Details 4");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 1 Details 1");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 1 Details 2");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 1 Details 3");
				NUnit.Framework.Assert.That(sections[1].Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("商標(牌名):FIG").Using(CustomComparers.TypeComparison), "Section 1 Details 4");
			}

			);
			NUnit.Framework.Assert.That(sections[2].TrademarkImage, NUnit.Framework.Is.EqualTo(default(Image)));
			NUnit.Framework.Assert.That(sections[3].TrademarkImage, NUnit.Framework.Is.EqualTo(default(Image)));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Section 2 Details 1");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 2 Details 2");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 2 Details 3");
				NUnit.Framework.Assert.That(sections[2].Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 2 Details 4");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 3 Details 1");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 3 Details 2");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("Entry Line text").Using(CustomComparers.TypeComparison), "Section 3 Details 3");
				NUnit.Framework.Assert.That(sections[3].Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Section 3 Details 4");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestDeclarantAndAEOCode()
		{
			var wrapper = CreateDocumentWrapper("TEST GROUPING");
			NUnit.Framework.Assert.That(wrapper.DeclarantAndAEOCode, NUnit.Framework.Is.EqualTo("新加坡商敦豪全球貨運物流股份有限公司台灣 123\r\nTWAEO-363636").Using(CustomComparers.TypeComparison));
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			NUnit.Framework.Assert.That(wrapper.DeclarantAndAEOCode, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDutyRateSpecific()
		{
			var minSmallDate = ZDateTime.MinSmallDateTimeValue;
			var maxSmallDate = ZDateTime.MaxSmallDateTimeValue;
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroupAllCountry = universalReferenceTestDataHelper.LoadOrCreateTradeGroup("TW", "ALL", minSmallDate, maxSmallDate);
			var comRateType = universalReferenceTestDataHelper.CreateNewOrGetExistingRateType("TW", "COM", "Commodity Taxes");
			var hwsRateCode = universalReferenceTestDataHelper.LoadOrCreateNewCusRateCode(Factory, "HWS", comRateType.PK);
			var tatRateCode = universalReferenceTestDataHelper.LoadOrCreateNewCusRateCode(Factory, "TAT", comRateType.PK);
			var ttTariffType = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType("TW", "TT");
			var atTariffType = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType("TW", "AT");
			var ttCusTariff = universalReferenceTestDataHelper.LoadOrCreateNewTariff("TW", ttTariffType.PK, "123456789", minSmallDate, maxSmallDate, "tt Tariff");
			var atCusTariff = universalReferenceTestDataHelper.LoadOrCreateNewTariff("TW", atTariffType.PK, "987654321", minSmallDate, maxSmallDate, "at Tariff");
			var hwsRateByttCusTariff = universalReferenceTestDataHelper.CreateRate(ttCusTariff, hwsRateCode.PK, minSmallDate, maxSmallDate, "1000 * [KGM]");
			var tatRateByttCusTariff = universalReferenceTestDataHelper.CreateRate(ttCusTariff, tatRateCode.PK, minSmallDate, maxSmallDate, "7 * [LTR] * AlcoholPercentage", rateFormulaDeriveFrom: "7/LTR");
			var tatRateByatCusTariff = universalReferenceTestDataHelper.CreateRate(atCusTariff, tatRateCode.PK, minSmallDate, maxSmallDate, "26 * [LTR] * AlcoholPercentage", rateFormulaDeriveFrom: "26/LTR");
			universalReferenceTestDataHelper.CreateCusApplicability(hwsRateByttCusTariff, tradeGroupAllCountry, minSmallDate.Date, maxSmallDate.Date);
			universalReferenceTestDataHelper.CreateCusApplicability(tatRateByttCusTariff, tradeGroupAllCountry, minSmallDate.Date, maxSmallDate.Date);
			universalReferenceTestDataHelper.CreateCusApplicability(tatRateByatCusTariff, tradeGroupAllCountry, minSmallDate.Date, maxSmallDate.Date);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsUnitQty = "A";
			invoiceLine1.JI_CustomsSecondUnitQty = "B";
			invoiceLine1.JI_CustomsQuantity = 500M;
			invoiceLine1.JI_CustomsSecondQuantity = 100M;
			invoiceLine1.JI_CL = entryLine1.PK;
			var line1Tax = invoiceLine1.Taxes.AddNew();
			line1Tax.JLT_Type = "TT";
			line1Tax.JLT_Tariff = "123456789";
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsUnitQty = "A";
			invoiceLine2.JI_CustomsSecondUnitQty = "B";
			invoiceLine2.JI_CustomsQuantity = 500M;
			invoiceLine2.JI_CustomsSecondQuantity = 100M;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_CountryOfOrigin = "CN";
			var line2Tax = invoiceLine2.Taxes.AddNew();
			line2Tax.JLT_Type = "AT";
			line2Tax.JLT_Tariff = "987654321";
			var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			var section1 = sections[0];
			NUnit.Framework.Assert.That(section1.Box44SpecificDutyRate, NUnit.Framework.Is.EqualTo("7/LTR").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section1.Box44OtherDutyRate, NUnit.Framework.Is.EqualTo("1000/KGM").Using(CustomComparers.TypeComparison));
			var section2 = sections[1];
			NUnit.Framework.Assert.That(section2.Box44SpecificDutyRate, NUnit.Framework.Is.EqualTo("0/LTR").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(section2.Box44OtherDutyRate.ToString(), NUnit.Framework.Is.Null.Or.Empty);

			invoiceLine1.JI_AlcoholPercentage = 1.1m;
			invoiceLine2.JI_AlcoholPercentage = 1.1m;

			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.Sections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			section1 = sections[0];
			NUnit.Framework.Assert.That(section1.Box44SpecificDutyRate, NUnit.Framework.Is.EqualTo("7/LTR").Using(CustomComparers.TypeComparison));
			section2 = sections[1];
			NUnit.Framework.Assert.That(section2.Box44SpecificDutyRate, NUnit.Framework.Is.EqualTo("28.6/LTR").Using(CustomComparers.TypeComparison));
		}

		protected ImportCustomsDeclarationDocumentWrapper CreateDocumentWrapper(ZString grouping) => GetDocumentWrapper(GenerateEntryHeader(grouping), Factory);

		protected CusEntryHeader GenerateEntryHeader(string grouping = "")
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Taiwan, "Taiwan");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(facility.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BB");
			helper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, description: "Trade Promotion Fee", ensureDataGroupingExists: true);
			var vessel = newFactory.New<RefVessel>();
			vessel.RV_Code = "VSCD";
			vessel.RV_LloydsNumber = "123456";
			vessel.RV_RadioCallSign = "654321";
			newFactory.Save();

			var newBroker = Factory.NewWithValidTestData<GlbStaff>();
			newBroker.GS_FullName = "XA1";
			var brkCertificate = newBroker.Certificates.AddNew();
			brkCertificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			brkCertificate.XZ_RN_NKCountryOfIssuance = "TW";
			brkCertificate.XZ_RefNumber = "1234";
			brkCertificate.XZ_ExpiryOrDueDate = DateTime.Today.AddYears(1);
			var agentOrg = Factory.NewWithValidTestData<OrgHeader>();
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AEO", "363636", "TW");
			var translatedAddress = agentOrg.MainAddress.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			translatedAddress.OTA_Address1 = "ota add1";
			translatedAddress.OTA_Address2 = "ota add2";
			translatedAddress.OTA_CompanyName = "新加坡商敦豪全球貨運物流股份有限公司台灣";
			var organization1 = Factory.New<OrgHeader>();
			organization1.Addresses.RemoveAndDeleteAll();
			organization1.OH_Code = "Org1";
			organization1.OH_RL_NKClosestPort = "TW";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AEO", "123465789", "TW");
			var contact = organization1.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			var address1 = organization1.Addresses[0];
			address1.OA_RN_NKCountryCode = "TW";
			address1.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			address1.OA_Language = "EN";
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			address1.OA_Phone = "PHONE";
			address1.OA_Email = "EMAIL";
			var entranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			entranslatedAddress1.OTA_Language = "EN";
			entranslatedAddress1.OTA_Address1 = "EN OTA ADDRESS 1";
			entranslatedAddress1.OTA_Address2 = "EN OTA ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.TPC, "TPCREGNO", "TW");
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "123465789", "TW");
			organization1.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.CBF, "22233", Core.Constants.CountryCodes.Taiwan);
			var vatCusCode = organization1.CustomsCodes.AddNew();
			vatCusCode.OK_RN_NKCodeCountry = "TW";
			vatCusCode.OK_CodeType = "VAT";
			vatCusCode.OK_CustomsRegNo = "123465789";
			var ccpCusCode = address1.CustomsCodes.AddNew();
			ccpCusCode.OK_RN_NKCodeCountry = "TW";
			ccpCusCode.OK_CodeType = "CCP";
			ccpCusCode.OK_CustomsRegNo = "987654321";
			var pasCusCode = organization1.CustomsCodes.AddNew();
			pasCusCode.OK_RN_NKCodeCountry = "TW";
			pasCusCode.OK_CodeType = "PAS";
			pasCusCode.OK_CustomsRegNo = "PASREGNO";
			var pidCusCode = organization1.CustomsCodes.AddNew();
			pidCusCode.OK_RN_NKCodeCountry = "TW";
			pidCusCode.OK_CodeType = "PID";
			pidCusCode.OK_CustomsRegNo = "PIDREGNO";
			var organization2 = Factory.New<OrgHeader>();
			organization2.Addresses.RemoveAndDeleteAll();
			organization2.OH_Code = "Org2";
			organization2.OH_RL_NKClosestPort = "TW";
			contact = organization2.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			var address2 = organization2.Addresses[0];
			address2.OA_RN_NKCountryCode = "TW";
			address2.OA_CompanyNameOverride = "Seller OVERRIDEN COMPANY NAME";
			address2.OA_Language = "EN";
			address2.OA_Address1 = "Seller ADDRESS 1";
			address2.OA_Address2 = "Seller ADDRESS 2";
			address2.OA_Phone = "PHONE";
			address2.OA_Email = "EMAIL";
			var entranslatedAddress2 = address2.TranslatedAddresses.AddNew();
			entranslatedAddress2.OTA_Language = "EN";
			entranslatedAddress2.OTA_Address1 = "Seller EN OTA ADDRESS 1";
			entranslatedAddress2.OTA_Address2 = "Seller EN OTA ADDRESS 2";
			var zhTWtranslatedAddress2 = address2.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress2.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress2.OTA_CompanyName = "Seller TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress2.OTA_Address1 = "Seller TW OTA ADDRESS 1";
			zhTWtranslatedAddress2.OTA_Address2 = "Seller TW OTA ADDRESS 2";
			organization2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AEO", "8889999", "TW");
			organization2.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.CBF, "22233", Core.Constants.CountryCodes.Taiwan);
			vatCusCode = organization2.CustomsCodes.AddNew();
			vatCusCode.OK_RN_NKCodeCountry = "TW";
			vatCusCode.OK_CodeType = "VAT";
			vatCusCode.OK_CustomsRegNo = "9696969";
			declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = ContainerModeList.Codes.BreakBulk;
			declaration.JE_CustomsOffice = "AA";
			declaration.JE_VesselArrivalReg = "212";
			declaration.JE_SLD = "X2";
			declaration.JE_VesselName = "VSCD";
			declaration.JE_MasterBill = "XXX123456";
			declaration.JE_HouseBill = "ABC85858";
			declaration.JE_ExportDate = new ZDateTime(2019, 09, 01);
			declaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._1;
			declaration.JE_DefermentAccountNumber = "A3";
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.JE_GS_NKCusAgent = newBroker.GS_Code;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = organization1.MainAddress.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = organization2.MainAddress.PK;
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_RL_NKPortOfLoading = "TWKEL";
			declaration.JE_OA_DeclarantAddress = agentOrg.MainAddress.PK;
			var testPort = Factory.NewWithValidTestData<RefUNLOCO>();
			testPort.RL_Code = "TST";
			testPort.RL_PortName = "TestPortName";
			declaration.JE_RL_NKOrigin = testPort.RL_Code;
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_GoodsLocation = "XXXX0123";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 05, 15);
			entryInstruction.CEI_PackageDescription = "PKG Descr";
			entryInstruction.TW_TradersRemarks = "Traders Remarks Note";
			entryInstruction.CEI_ExamMode = ExamModeList.Codes.FactoryInspection;
			var declarationDuplicates = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicates.CY_Code = "B1";
			declarationDuplicates.CY_Data = "1";
			declarationDuplicates = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicates.CY_Code = "B2";
			declarationDuplicates.CY_Data = "2";
			entryInstruction.CEI_BoxNumber = "123";
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_TariffNum = "87032310005";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "TWD";
			invoice.JZ_InvoiceCurrExRate = 1m;
			invoice.JZ_RelatedIndicator = "N";
			invoice.JZ_MarksAndNumbers = "TEST MARKS";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 388m;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = TW.Business.Constants.ProcedureCodes._37;
			invoiceLine1.JI_CC = classification.PK;
			invoiceLine1.JI_Model = "STAREX";
			invoiceLine1.JI_EnteredUnitPrice = 44m;
			invoiceLine1.JI_Weight = 1000m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_Group = grouping;
			invoiceLine1.JI_BrandName = "BRAND: HYUMDAI";
			invoiceLine1.JI_CountryOfOrigin = "AU";
			invoiceLine1.JI_CustomsOwnerPartNo = "1560-HANSDA-503";
			invoiceLine1.JI_CustomsSupplierPartNo = "1060039601G";
			invoiceLine1.JI_Compositions = "(非食品或食品添加物用途)";
			invoiceLine1.CertificateOfOriginNumber = "Certificate Origin Number";
			invoiceLine1.CertificateOfOriginNumberItemNumber = 3;
			invoiceLine1.JI_PreviousEntryNumber = "Pre Entry No";
			invoiceLine1.JI_PreviousEntryLineNumber = 12;
			invoiceLine1.JI_NetWeight = 3;
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_CustomsQuantity = 532;
			invoiceLine1.JI_InvoiceQuantity = 2;
			invoiceLine1.JI_InvoiceUQ = "PKG";
			invoiceLine1.JI_RAPCurr = "TWD";
			invoiceLine1.JI_RAPPrice = 2510;
			invoiceLine1.JI_EPTDigit1 = "0";
			invoiceLine1.JI_EPTDigit2 = "Z";
			invoiceLine1.JI_EPTDigit3 = "Z";
			invoiceLine1.JI_LinePrice = 88m;
			invoiceLine1.JI_VatPymntMthd = "CAS";
			invoiceLine1.JI_CustomsSecondQuantity = 2;
			invoiceLine1.JI_CustomsSecondUnitQty = "PKG";
			invoiceLine1.JI_Description = "JI_Description Test";
			var assignedJobComInvLineRef = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			assignedJobComInvLineRef.JG_ReferenceNumber = "66";
			assignedJobComInvLineRef = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			assignedJobComInvLineRef.JG_ReferenceNumber = "77";
			var cusSupporting = invoiceLine1.PermitCusSupportingCollection.AddNew();
			cusSupporting.CSI_ReferenceNumber = "D4CBJ752251115";
			cusSupporting.CSI_LineNo = 1;
			cusSupporting = invoiceLine1.PermitCusSupportingCollection.AddNew();
			cusSupporting.CSI_ReferenceNumber = "D4CBJ752251126";
			cusSupporting.CSI_LineNo = 2;
			cusSupporting = invoiceLine1.PermitCusSupportingCollection.AddNew();
			cusSupporting.CSI_ReferenceNumber = "D4CBJ752251137";
			cusSupporting.CSI_LineNo = 3;
			cusSupporting = invoiceLine1.PermitCusSupportingCollection.AddNew();
			cusSupporting.CSI_ReferenceNumber = "D4CBJ752251148";
			cusSupporting.CSI_LineNo = 4;
			var controllingAgenciesCusSupportings = invoiceLine1.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			controllingAgenciesCusSupportings.CSI_ReferenceNumber = "D4CBJ752251159";
			var charge = invoice.Charges.AddNew();
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 200m;
			charge.J7_RX_NKCurrency = "TWD";
			charge.J7_IsNotIncludedInInvoice = false;
			charge = invoice.Charges.AddNew();
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			charge.J7_ChargeType = "ONS";
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = "TWD";
			charge.J7_IsNotIncludedInInvoice = false;
			charge = invoice.Charges.AddNew();
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			charge.J7_ChargeType = "ADD";
			charge.J7_Amount = 102m;
			charge.J7_RX_NKCurrency = "TWD";
			charge = invoice.Charges.AddNew();
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			charge.J7_ChargeType = "DED";
			charge.J7_IsGSTApplicable = true;
			charge.J7_Amount = 107m;
			charge.J7_RX_NKCurrency = "TWD";
			charge.J7_IsIncludedInITOT = true;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "UUUU1234567";
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Groupage;
			container.CO_Seal = "9695";
			container.CO_SecondSeal = "9487";
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			invoiceLine1.ContainersPivot.AddPivotFor(container);
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.AllocateEntryNumber();
			Factory.Save();
			entryHeader.CH_EntryStatus = "RFM";
			entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
			var entryLine = entryHeader.AllEntryLines.Cast<CusEntryLine>().First();
			var cusEntryHeaderCharge = entryHeader.Charges.AddNew();
			cusEntryHeaderCharge.C1_ChargeType = "TPF";
			cusEntryHeaderCharge.C1_MethodOfPayment = "CAS";
			cusEntryHeaderCharge.C1_ChargeAmount = 800m;
			cusEntryHeaderCharge = entryHeader.Charges.AddNew();
			cusEntryHeaderCharge.C1_ChargeType = "DTA";
			cusEntryHeaderCharge.C1_MethodOfPayment = "CAS";
			cusEntryHeaderCharge.C1_ChargeAmount = 101m;
			cusEntryHeaderCharge = entryHeader.Charges.AddNew();
			cusEntryHeaderCharge.C1_ChargeType = "DTA";
			cusEntryHeaderCharge.C1_MethodOfPayment = "DEF";
			cusEntryHeaderCharge.C1_ChargeAmount = 102m;
			cusEntryHeaderCharge = entryHeader.Charges.AddNew();
			cusEntryHeaderCharge.C1_ChargeType = "TPF";
			cusEntryHeaderCharge.C1_MethodOfPayment = "DEF";
			cusEntryHeaderCharge.C1_ChargeAmount = 104m;
			cusEntryHeaderCharge = entryHeader.Charges.AddNew();
			cusEntryHeaderCharge.C1_ChargeType = "CTA";
			cusEntryHeaderCharge.C1_MethodOfPayment = "CAS";
			cusEntryHeaderCharge.C1_ChargeAmount = 105;
			cusEntryHeaderCharge = entryHeader.Charges.AddNew();
			cusEntryHeaderCharge.C1_ChargeType = "TAT";
			cusEntryHeaderCharge.C1_MethodOfPayment = "CAS";
			cusEntryHeaderCharge.C1_ChargeAmount = 106;
			cusEntryHeaderCharge = entryHeader.Charges.AddNew();
			cusEntryHeaderCharge.C1_ChargeType = "HWS";
			cusEntryHeaderCharge.C1_MethodOfPayment = "DEF";
			cusEntryHeaderCharge.C1_ChargeAmount = 107;
			cusEntryHeaderCharge = entryHeader.Charges.AddNew();
			cusEntryHeaderCharge.C1_ChargeType = "VAT";
			cusEntryHeaderCharge.C1_MethodOfPayment = "DEF";
			cusEntryHeaderCharge.C1_ChargeAmount = 108;
			entryLine.CL_ValueForVAT = 2560.47m;
			var fee = entryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			fee.CF_ChargeType = "CTA";
			fee.CF_MethodOfPayment = "CAS";
			fee.CF_MethodOfCalculation = "KGM";
			fee.CF_Rate = 1.25M;
			fee.CF_BaseValue = 2000;
			fee = entryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			fee.CF_ChargeType = "DTA";
			fee.CF_MethodOfCalculation = "";
			fee.CF_Rate = 1.25M;
			fee = entryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			fee.CF_ChargeType = "DTS";
			fee.CF_MethodOfCalculation = "KGM";
			fee.CF_Rate = 1.25M;
			fee.CF_BaseValue = 1234m;
			fee = entryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			fee.CF_ChargeType = "CTS";
			fee.CF_MethodOfCalculation = "KGM";
			fee.CF_Rate = 1.25M;
			fee = entryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			fee.CF_ChargeType = "SSG";
			fee.CF_MethodOfCalculation = "KGM";
			fee.CF_Rate = 0.1M;
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			jobDeclarationDocumentAddressConfig.CustomizeSectionBodyRow = customizeSectionBodyRow;
			return entryHeader;
		}

		[ExpectNoExceptions]
		public void TestFirstPageGoodsItemListSections()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "ACR";
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "AMH";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.JI_Group = "";
			invoiceLine2.JI_Group = "";
			declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var firstPageSections = wrapper.FirstPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(firstPageSections.Length, NUnit.Framework.Is.EqualTo(13));
				NUnit.Framework.Assert.That(GetAllSectionsDescription(), NUnit.Framework.Is.EqualTo(@"Entry Line 1



Entry Line 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsPermitNo(), NUnit.Framework.Is.EqualTo(@"NIL



NIL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsPrice(), NUnit.Framework.Is.EqualTo(@"----------
Total:").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsQualityAndUnit(), NUnit.Framework.Is.EqualTo(@"0KGM
1ACR


0KGM
2AMH


----------------
0KGM
1ACR
2AMH
vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsDutyValue(), NUnit.Framework.Is.EqualTo(@"--------------
0
vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
				invoiceLine1.JI_Group = "  Grouping Line 11 ";
				invoiceLine2.JI_Group = "  Grouping Line 21 ";
				wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				firstPageSections = wrapper.FirstPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
				NUnit.Framework.Assert.That(firstPageSections.Length, NUnit.Framework.Is.EqualTo(13));
				NUnit.Framework.Assert.That(GetAllSectionsDescription(), NUnit.Framework.Is.EqualTo(@"Grouping Line 11








Entry Line 1






Grouping Line 21








Entry Line 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsPermitNo(), NUnit.Framework.Is.EqualTo(@"NIL















NIL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsPrice(), NUnit.Framework.Is.EqualTo(@"----------
Total:").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsQualityAndUnit(), NUnit.Framework.Is.EqualTo(@"0KGM
1ACR














0KGM
2AMH


----------------
0KGM
1ACR
2AMH").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsDutyValue(), NUnit.Framework.Is.EqualTo(@"--------------
0
vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
				invoiceLine1.JI_Group = "  Grouping Line 11  \r\n  Grouping Line 12 \r\n  Grouping Line 13 ";
				wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				firstPageSections = wrapper.FirstPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
				NUnit.Framework.Assert.That(firstPageSections.Length, NUnit.Framework.Is.EqualTo(13));
				NUnit.Framework.Assert.That(GetAllSectionsDescription(), NUnit.Framework.Is.EqualTo(@"Grouping Line 11



Grouping Line 12



Grouping Line 13








Entry Line 1






Grouping Line 21








Entry Line 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsPermitNo(), NUnit.Framework.Is.EqualTo(@"NIL















NIL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsPrice(), NUnit.Framework.Is.EqualTo(@"----------
Total:").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsQualityAndUnit(), NUnit.Framework.Is.EqualTo(@"0KGM
1ACR














0KGM
2AMH


----------------
0KGM
1ACR
2AMH").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsDutyValue(), NUnit.Framework.Is.EqualTo(@"--------------
0
vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
			}

			);
			ZString GetAllSectionsDescription()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box35DescriptionOfGoods_Line1 + "\r\n" + c.Box35DescriptionOfGoods_Line2 + "\r\n" + c.Box35DescriptionOfGoods_Line3 + "\r\n" + c.Box35DescriptionOfGoods_Line4 + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsPermitNo()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box37ImportPermitNumberAndItemNumber_Line1 + "\r\n" + c.Box37ImportPermitNumberAndItemNumber_Line2 + "\r\n" + c.Box38CCCCode + "\r\n" + c.Box38AssignedNumber + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsPrice()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box39IncoTermAndCurrency + "\r\n" + c.Box39UnitPrice_Line1 + "\r\n" + c.Box39UnitPrice_Line2 + "\r\n" + c.Box39RorRapUnitPrice + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsQualityAndUnit()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box40NetWeight + "\r\n" + c.Box41QuantityAndUnit + "\r\n" + c.Box42StatisticsQuantityAndUnit_Line1 + "\r\n" + c.Box42StatisticsQuantityAndUnit_Line2 + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsDutyValue()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box43CustomsValue_Line1 + "\r\n" + c.Box43CustomsValue_Line2 + "\r\n" + c.Box43CustomsValue_Line3 + "\r\n" + c.Box43RorRapCustomsValue + "\r\n");
				return result.Trim();
			}
		}

		[ExpectNoExceptions]
		public void TestFirstPageGoodsItemListSectionsAdjustForCell()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "ACR";
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "AMH";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.JI_Group = "";
			invoiceLine2.JI_Group = "";

			CombineAssertions(() =>
			{
				declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
				declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.CustomizeSectionBodyRow = "4";
				var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				var firstPageSections = wrapper.FirstPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
				NUnit.Framework.Assert.That(firstPageSections.Length, NUnit.Framework.Is.EqualTo(15));
				NUnit.Framework.Assert.That(firstPageSections.Select(x => x.RowCountToShow), NUnit.Framework.Is.EqualTo(new int[] { 2, 1, 1, 0, 2, 4, 1, 1, 1, 0, 1, 1, 1, 1, 1 }));
				NUnit.Framework.Assert.That(firstPageSections.Select(x => x.ShowDashLine), NUnit.Framework.Is.EqualTo(new bool[] { false, false, false, true, false, false, false, false, false, true, false, false, false, false, false }));
				NUnit.Framework.Assert.That(firstPageSections.Select(x => x.Box35DescriptionOfGoods_Line2), NUnit.Framework.Is.EqualTo(new ZString[] { "Entry Line 1", "", "", "", "Entry Line 2", "", "", "", "", "", "", "", "", "", "" }));
			});

			CombineAssertions(() =>
			{
				declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
				declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.CustomizeSectionBodyRow = "10";
				var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				var firstPageSections = wrapper.FirstPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
				NUnit.Framework.Assert.That(firstPageSections.Length, NUnit.Framework.Is.EqualTo(17));
				NUnit.Framework.Assert.That(firstPageSections.Select(x => x.RowCountToShow), NUnit.Framework.Is.EqualTo(new int[] { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }));
				NUnit.Framework.Assert.That(firstPageSections.Select(x => x.ShowDashLine), NUnit.Framework.Is.EqualTo(new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false }));
				NUnit.Framework.Assert.That(firstPageSections.Select(x => x.Box35DescriptionOfGoods_Line2), NUnit.Framework.Is.EqualTo(new ZString[] { "Entry Line 1", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" }));
			});
		}

		[ExpectNoExceptions]
		public void TestOtherPageGoodsItemListSections()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1 \r\n Entry Line 12";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "ACR";
			invoiceLine1.JI_LinePrice = 100;
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_Procedure = "31";
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2 \r\n Entry Line 22";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "AMH";
			invoiceLine2.JI_LinePrice = 200;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine1.JI_Procedure = "31";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.JI_Group = " Grouping Line 11 ";
			invoiceLine2.JI_Group = "  Grouping Line 21 ";
			var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(!wrapper.OtherPageGoodsItemListSections.Any(), NUnit.Framework.Is.True);
			invoiceLine2.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2  \r\n  Grouping Line 3";
			declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var otherPageSections = wrapper.OtherPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(GetAllSectionsDescription().ToString(), NUnit.Framework.Is.Null.Or.Empty, "1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(GetAllSectionsPermitNo().ToString(), NUnit.Framework.Is.Null.Or.Empty, "2 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(GetAllSectionsPrice(), NUnit.Framework.Is.EqualTo(@"----------
Total:").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsQualityAndUnit(), NUnit.Framework.Is.EqualTo(@"----------------
0KGM
1ACR
2AMH
vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsDutyValue(), NUnit.Framework.Is.EqualTo(@"--------------
0
vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
			}

			);
			entryLine1.CL_Description = CreateDescription(10);
			entryLine2.CL_Description = CreateDescription(52);
			wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			otherPageSections = wrapper.OtherPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(GetAllSectionsDescription(), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26
27
28
29
30
31
32
33
34
35
36
37
38
39
40
41
42
43
44
45
46
47
48
49
50
51
52").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsPermitNo(), NUnit.Framework.Is.EqualTo(@"NIL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsPrice(), NUnit.Framework.Is.EqualTo(@"100






























































----------
Total:").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsQualityAndUnit(), NUnit.Framework.Is.EqualTo(@"0KGM
2AMH






























































----------------
0KGM
1ACR
2AMH
vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsDutyValue(), NUnit.Framework.Is.EqualTo(@"--------------
0
vvvvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
			}

			);
			ZString CreateDescription(int lineCount)
			{
				var description = new ZStringBuilder();
				for (var i = 1; i <= lineCount; ++i)
				{
					description.AppendLine(i.ToString());
				}

				return description.ToString();
			}

			ZString GetAllSectionsDescription()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box35DescriptionOfGoods_Line1 + "\r\n" + c.Box35DescriptionOfGoods_Line2 + "\r\n" + c.Box35DescriptionOfGoods_Line3 + "\r\n" + c.Box35DescriptionOfGoods_Line4 + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsPermitNo()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box37ImportPermitNumberAndItemNumber_Line1 + "\r\n" + c.Box37ImportPermitNumberAndItemNumber_Line2 + "\r\n" + c.Box38CCCCode + "\r\n" + c.Box38AssignedNumber + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsPrice()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box39IncoTermAndCurrency + "\r\n" + c.Box39UnitPrice_Line1 + "\r\n" + c.Box39UnitPrice_Line2 + "\r\n" + c.Box39RorRapUnitPrice + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsQualityAndUnit()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box40NetWeight + "\r\n" + c.Box41QuantityAndUnit + "\r\n" + c.Box42StatisticsQuantityAndUnit_Line1 + "\r\n" + c.Box42StatisticsQuantityAndUnit_Line2 + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsDutyValue()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box43CustomsValue_Line1 + "\r\n" + c.Box43CustomsValue_Line2 + "\r\n" + c.Box43CustomsValue_Line3 + "\r\n" + c.Box43RorRapCustomsValue + "\r\n");
				return result.Trim();
			}
		}

		[ExpectNoExceptions]
		public void TestOtherPageGoodsItemListSectionsAdjustForCell()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1 \r\n Entry Line 12";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "ACR";
			invoiceLine1.JI_LinePrice = 100;
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_Procedure = "31";
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2 \r\n Entry Line 22";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "AMH";
			invoiceLine2.JI_LinePrice = 200;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine1.JI_Procedure = "31";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.JI_Group = " Grouping Line 11 \r\n Grouping Line 22";
			invoiceLine2.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2  \r\n  Grouping Line 3\r\n  Grouping Line 4";

			CombineAssertions("1 item and total", () =>
			{
				declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
				declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.CustomizeSectionBodyRow = "4";
				var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				var otherPageSections = wrapper.OtherPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
				NUnit.Framework.Assert.That(otherPageSections.Length, NUnit.Framework.Is.EqualTo(3));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.RowCountToShow), NUnit.Framework.Is.EqualTo(new int[] { 3, 4, 1 }));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.ShowDashLine), NUnit.Framework.Is.EqualTo(new bool[] { false, false, false }));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.Box35DescriptionOfGoods_Line2), NUnit.Framework.Is.EqualTo(new ZString[] { "Entry Line 2", "", "" }));
			});

			entryLine1.CL_Description = new string('A', 146);
			CombineAssertions(() =>
			{
				declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
				declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.CustomizeSectionBodyRow = "10";
				var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				var otherPageSections = wrapper.OtherPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
				NUnit.Framework.Assert.That(otherPageSections.Length, NUnit.Framework.Is.EqualTo(22));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.RowCountToShow), NUnit.Framework.Is.EqualTo(new int[] { 4, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 3, 4, 1 }));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.ShowDashLine), NUnit.Framework.Is.EqualTo(new bool[] { false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, true, false, false, false }));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.Box35DescriptionOfGoods_Line1), NUnit.Framework.Is.EqualTo(new ZString[] { "", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "AAAAAA", "", "", "", "", "", "", "Grouping Line 1", "Grouping Line 2", "Grouping Line 3", "Grouping Line 4", "", "", "", "", "", "", "", "", "" }));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.Box35DescriptionOfGoods_Line2), NUnit.Framework.Is.EqualTo(new ZString[] { "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "Entry Line 2", "", "" }));
			});
		}

		[ExpectNoExceptions]
		public void TestOtherPageGoodsItemListSectionsAdjustForCellWhenFirstPageGoodsItemRowCountToShowEqaulsGoodsItemRowCountToShow()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			for (var i = 0; i < 4; i++)
			{
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_Description = $"Good{i}";
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "12149000902";
				invoiceLine.JI_InvoiceQuantity = 1;
				invoiceLine.JI_InvoiceUQ = "ACR";
				invoiceLine.JI_CL = entryLine.PK;
			}

			CombineAssertions(() =>
			{
				var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
				jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
				jobDeclarationDocumentAddressConfig.CustomizeSectionBodyRow = "6";
				var wrapper = new ImportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				var otherPageSections = wrapper.OtherPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>().ToArray();
				NUnit.Framework.Assert.That(otherPageSections.Length, NUnit.Framework.Is.EqualTo(4));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.RowCountToShow), NUnit.Framework.Is.EqualTo(new int[] { 3, 4, 1, 1 }));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.ShowDashLine), NUnit.Framework.Is.EqualTo(new bool[] { false, false, false, false }));
				NUnit.Framework.Assert.That(otherPageSections.Select(x => x.Box35DescriptionOfGoods_Line2), NUnit.Framework.Is.EqualTo(new ZString[] { "Good3", "", "", "" }));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestShouldHideRow()
		{
			var section = new ImportDeclarationSectionBodyWrapper();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section.Box35[0] = "test Description 1";
			section.Box35[1] = "test Description 2";
			section.Box35[2] = "test Description 3";
			section.Box35[3] = "test Description 4";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper();
			section.Box37ImportPermitNumberAndItemNumber_Line1 = "test codes 1";
			section.Box37ImportPermitNumberAndItemNumber_Line2 = "test codes 2";
			section.Box38CCCCode = "test codes 3";
			section.Box38AssignedNumber = "test codes 4";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper();
			section.AddPriceItem("test price 1");
			section.AddPriceItem("test price 2");
			section.AddPriceItem("test price 3");
			section.AddPriceItem("test price 4");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper();
			section.AddStatisticsQualityAndUnitItem("test Quality 1");
			section.AddStatisticsQualityAndUnitItem("test Quality 2");
			section.AddStatisticsQualityAndUnitItem("test Quality 3");
			section.AddStatisticsQualityAndUnitItem("test Quality 4");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper();
			section.AddDutyPayingValueItem("test Duty 1");
			section.AddDutyPayingValueItem("test Duty 2");
			section.AddDutyPayingValueItem("test Duty 3");
			section.AddDutyPayingValueItem("test Duty 4");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper();
			section.Box44Ad_ValoremDutyRate = "test DutyRateAdValorem";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper();
			section.Box44SpecificDutyRate = "test DutyRateSpecific";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper();
			section.Box44OtherDutyRate = "test DutyRateAdValorem";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper()
			{ Box34ItemNumber = "test Item No" };
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper()
			{ CountryCode = "TW" };
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper()
			{ ShowEvenRowIsEmpty = true };
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section = new ImportDeclarationSectionBodyWrapper();
			var testImage = Image.FromFile(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"));
			section.TrademarkImage = new Bitmap(testImage, new Size(37, 37));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(!section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section.TrademarkImage = new Bitmap(testImage, new Size(36, 36));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(!section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
			section.TrademarkImage = new Bitmap(testImage, new Size(24, 24));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(section.ShouldHideRow1, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow1");
				NUnit.Framework.Assert.That(!section.ShouldHideRow2, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow2");
				NUnit.Framework.Assert.That(section.ShouldHideRow3, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow3");
				NUnit.Framework.Assert.That(section.ShouldHideRow4, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShouldHideRow4");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestRowCountToShow()
		{
			var section = new ImportDeclarationSectionBodyWrapper();
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(0));
			section.Box35[0] = "test Description 1";
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(1));
			section.Box35[1] = "test Description 2";
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(2));
			section.Box35[2] = "test Description 3";
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(3));
			section.Box35[3] = "test Description 4";
			NUnit.Framework.Assert.That(section.RowCountToShow, NUnit.Framework.Is.EqualTo(4));
		}

		[ExpectNoExceptions]
		public void TestCodeOfVessel()
		{
			var wrapper = CreateDocumentWrapper("TEST GROUPING");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.CodeOfVessel, NUnit.Framework.Is.EqualTo("654321").Using(CustomComparers.TypeComparison), "should get from BorderTransportMeans when SEA");
				declaration.JE_TransportMode = "AIR";
				NUnit.Framework.Assert.That(wrapper.CodeOfVessel, NUnit.Framework.Is.EqualTo(ZString.Empty), "should be empty when other");
			});
		}

		[ExpectNoExceptions]
		public void TestNew()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(GetDocumentWrapper(null, Factory), NUnit.Framework.Is.EqualTo(default(TImportCustomsDeclarationDocumentWrapper)));
				NUnit.Framework.Assert.That(GetDocumentWrapper(entryHeader, Factory), NUnit.Framework.Is.TypeOf(typeof(TImportCustomsDeclarationDocumentWrapper)));
			});
		}

		[ExpectNoExceptions]
		public void TestCusEntryHeader()
		{
			var entryHeader = GenerateEntryHeader("TEST GROUPING");
			NUnit.Framework.Assert.That(GetDocumentWrapper(entryHeader, Factory).CusEntryHeader, NUnit.Framework.Is.EqualTo(entryHeader));
		}

		protected abstract TImportCustomsDeclarationDocumentWrapper GetDocumentWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory);

		string customizeSectionBodyRow;
		protected JobDeclaration declaration;
	}
}
