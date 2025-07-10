using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryLineFeeLookups))]
	sealed class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestEntryLineFee()
		{
			var parent = Factory.New<CusEntryLineFee>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Lookups.EntryLineFee));
		}

		[ExpectNoExceptions]
		public void TestMethodOfPaymentList()
		{
			var parent = Factory.New<CusEntryLineFee>();
			var actualMethodOfPaymentList = parent.Lookups.MethodOfPaymentList;
			NUnit.Framework.Assert.That(actualMethodOfPaymentList.CodesAsString, NUnit.Framework.Is.EqualTo("CAS, DEF"));
		}

		[ExpectNoExceptions]
		public void TestMethodOfCalculationList()
		{
			var fee = Factory.New<CusEntryLineFee>();
			var lookups = new CusEntryLineFeeLookups(fee);
			NUnit.Framework.Assert.That(lookups.MethodOfCalculationList, NUnit.Framework.Is.EqualTo(TWRefCusCodeListTypes.GetMethodOfCalculationList(Factory)));
		}

		[ExpectNoExceptions]
		public void TestRateOverrideReasonList()
		{
			var parent = Factory.New<CusEntryLineFee>();
			var actualList = parent.Lookups.RateOverrideReasonList;
			NUnit.Framework.Assert.That(actualList.CodesAsString, NUnit.Framework.Is.EqualTo("ADD, OVR"));
			NUnit.Framework.Assert.That(actualList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<TWRateOverrideReasonList>()));
		}

		[ExpectNoExceptions]
		public void TestChargeTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM");
			helper.LoadOrCreateNewCusRateCode(Factory, "CTA", rateType.PK, true, false, "Commodity Tax (Ad-Valorem)1");
			var ctaRateCodeLan = helper.LoadOrCreateNewCusRateCodeLanguage(Factory, "CTA", "ZHT");
			ctaRateCodeLan.ZXC_Description = "貨物稅(從價)1";

			helper.LoadOrCreateNewCusRateCode(Factory, "HWS", rateType.PK, true, false, "Health and Welfare Surcharge1");
			helper.LoadOrCreateNewCusRateCode(Factory, "TAT", rateType.PK, true, false, "Tobacco and Alcohol Tax");
			helper.LoadOrCreateNewCusRateCode(Factory, "CTS", rateType.PK, true, false, "Commodity Tax (Specific)");
			helper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK, true, false, "Import Duty (Ad-Valorem)");
			helper.LoadOrCreateNewCusRateCode(Factory, "DTS", rateType.PK, true, false, "Import Duty (Specific)");
			helper.LoadOrCreateNewCusRateCode(Factory, "ADD", rateType.PK, true, false, "Anti-Dumping Duty");
			helper.LoadOrCreateNewCusRateCode(Factory, "CVD", rateType.PK, true, false, "Countervailing Duty");
			helper.LoadOrCreateNewCusRateCode(Factory, "RTD", rateType.PK, true, false, "Retaliatory Duty");
			helper.LoadOrCreateNewCusRateCode(Factory, "ADT", rateType.PK, true, false, "Additional Duty");
			helper.LoadOrCreateNewCusRateCode(Factory, "SSG", rateType.PK, true, false, "Specifically Selected Goods and Services Tax");

			helper.CreateRefCusTaxOrFeeType("VAT", "Value Added Tax");
			helper.CreateRefCusTaxOrFeeType("OTH", "Other");
			var vatTaxOrFee = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, 0, 0, "VAT");
			vatTaxOrFee.ZZF_Description = "Business tax1";

			var tpfTaxOrFee = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, 0, 0, "OTH");
			tpfTaxOrFee.ZZF_Description = "Trade Promotion Fee1";
			helper.CreateTaxOrFeeLanguage(tpfTaxOrFee, "ZHT", "推廣貿易服務費1");

			var ddfTaxOrFee = helper.CreateTaxOrFee("DDF", 0.0004m, Core.Constants.CountryCodes.Taiwan, 0, 0, "OTH");
			ddfTaxOrFee.ZZF_Description = "Late Declaration Fee";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_PartNo = "XXX";
			invoiceLine.JI_Tariff = "A";
			declaration.DoMerge();
			var entyLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var fee = entyLine.Fees.AddNew();
			var actualList = fee.Lookups.ChargeTypeList;
			NUnit.Framework.Assert.That(actualList.GetAllCodesZString(), NUnit.Framework.Is.EquivalentTo(new ZString[] { "CTA", "CTS", "HWS", "TAT", "DTA", "DTS", "SSG", "ADD", "RTD", "CVD", "ADT", "TPF", "VAT" }));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("CTA"), NUnit.Framework.Is.EqualTo("Commodity Tax (Ad-Valorem)1"));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("HWS"), NUnit.Framework.Is.EqualTo("Health and Welfare Surcharge1"));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("VAT"), NUnit.Framework.Is.EqualTo("Business tax1"));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("TPF"), NUnit.Framework.Is.EqualTo("Trade Promotion Fee1"));

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			actualList = fee.Lookups.ChargeTypeList;
			NUnit.Framework.Assert.That(actualList.CodesAsString, NUnit.Framework.Is.EqualTo("TPF"));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("TPF"), NUnit.Framework.Is.EqualTo("Trade Promotion Fee1"));

			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseTraditional;
			Factory.Save();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			actualList = fee.Lookups.ChargeTypeList;
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("CTA"), NUnit.Framework.Is.EqualTo("貨物稅(從價)1"));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("HWS"), NUnit.Framework.Is.EqualTo("Health and Welfare Surcharge1"));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("VAT"), NUnit.Framework.Is.EqualTo("Business tax1"));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("TPF"), NUnit.Framework.Is.EqualTo("推廣貿易服務費1"));

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			actualList = fee.Lookups.ChargeTypeList;
			NUnit.Framework.Assert.That(actualList.CodesAsString, NUnit.Framework.Is.EqualTo("TPF"));
			NUnit.Framework.Assert.That(actualList.GetDescriptionFromCode("TPF"), NUnit.Framework.Is.EqualTo("推廣貿易服務費1"));
		}
	}
}
