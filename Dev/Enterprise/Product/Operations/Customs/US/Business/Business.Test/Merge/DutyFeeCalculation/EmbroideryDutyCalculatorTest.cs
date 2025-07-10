using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EmbroideryDutyCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2020, 03, 03)]
		public void TestCalculateEmbroideryDutyWithoutSettingParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "5810929080";
			invoiceLineOne.JI_LinePrice = 5000;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CH";
			invoiceLineOne.US_UC_NKCountryOfExport = "CH";
			invoiceLineOne.JI_CustomsQuantity = 220m;
			invoiceLineOne.JI_CustomsUnitQty = "KG";

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_Tariff = "5407532060";
			invoiceLineTwo.JI_LinePrice = 5000;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "CH";
			invoiceLineTwo.US_UC_NKCountryOfExport = "CH";
			invoiceLineTwo.JI_CustomsQuantity = 579m;
			invoiceLineTwo.JI_CustomsUnitQty = "M2";
			invoiceLineTwo.JI_CustomsSecondQuantity = 220m;
			invoiceLineTwo.JI_CustomsSecondUnitQty = "KG";

			var invoiceLineThree = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineThree.JI_Tariff = "5810929080";
			invoiceLineThree.US_SupTariff = "99038824";
			invoiceLineThree.JI_LinePrice = 5000;
			invoiceLineThree.US_UC_NKCountryOfOrigin = "CH";
			invoiceLineThree.US_UC_NKCountryOfExport = "CH";
			invoiceLineThree.JI_CustomsQuantity = 220m;
			invoiceLineThree.JI_CustomsUnitQty = "KG";

			var invoiceLineFour = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineFour.JI_Tariff = "5407532060";
			invoiceLineFour.US_SupTariff = "99038803";
			invoiceLineFour.JI_LinePrice = 5000;
			invoiceLineFour.US_UC_NKCountryOfOrigin = "CH";
			invoiceLineFour.US_UC_NKCountryOfExport = "CH";
			invoiceLineFour.JI_CustomsQuantity = 579m;
			invoiceLineFour.JI_CustomsUnitQty = "M2";
			invoiceLineFour.JI_CustomsSecondQuantity = 220m;
			invoiceLineFour.JI_CustomsSecondUnitQty = "KG";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Normal duty on 1st invoice line = 5000 * 0.074", 370m, invoiceLineOne.US_Duty);
			AssertEquals("Prov duty on 1st invoice line should be zero", 0m, invoiceLineOne.US_SupDuty);
			AssertEquals("Normal duty on 2nd invoice line = 5000 * 0.12", 600m, invoiceLineTwo.US_Duty);
			AssertEquals("Prov duty on 2nd invoice line should be zero", 0m, invoiceLineTwo.US_SupDuty);
			AssertEquals("Normal duty on 3rd invoice line = 5000 * 0.074", 370m, invoiceLineThree.US_Duty);
			AssertEquals("Prov duty on 3rd invoice line = 5000 * 0.15", 750m, invoiceLineThree.US_SupDuty);
			AssertEquals("Normal duty on 4th invoice line = 5000 * 0.12", 600m, invoiceLineFour.US_Duty);
			AssertEquals("Prov duty on 4th invoice line = 5000 * 0.25", 1250m, invoiceLineFour.US_SupDuty);
		}

		[TestDate(2020, 03, 03)]
		public void TestCaclculateEmbroideryForParentChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "5810929080";
			invoiceLineOne.JI_LinePrice = 5000;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";
			invoiceLineOne.JI_CustomsQuantity = 220m;
			invoiceLineOne.JI_CustomsUnitQty = "KG";

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.JI_Tariff = "5407532060";
			invoiceLineTwo.JI_LinePrice = 0;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineTwo.US_UC_NKCountryOfExport = "CN";
			invoiceLineTwo.JI_CustomsQuantity = 579m;
			invoiceLineTwo.JI_CustomsUnitQty = "M2";
			invoiceLineTwo.JI_CustomsSecondQuantity = 220m;
			invoiceLineTwo.JI_CustomsSecondUnitQty = "KG";

			var invoiceLineThree = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineThree.JI_Tariff = "5810929080";
			invoiceLineThree.US_SupTariff = "99038824";
			invoiceLineThree.JI_LinePrice = 5000;
			invoiceLineThree.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineThree.US_UC_NKCountryOfExport = "CN";
			invoiceLineThree.JI_CustomsQuantity = 220m;
			invoiceLineThree.JI_CustomsUnitQty = "KG";

			var invoiceLineFour = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineFour.JI_ParentID = invoiceLineThree.PK;
			invoiceLineFour.JI_Tariff = "5407532060";
			invoiceLineFour.US_SupTariff = "99038803";
			invoiceLineFour.JI_LinePrice = 0;
			invoiceLineFour.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineFour.US_UC_NKCountryOfExport = "CN";
			invoiceLineFour.JI_CustomsQuantity = 579m;
			invoiceLineFour.JI_CustomsUnitQty = "M2";
			invoiceLineFour.JI_CustomsSecondQuantity = 220m;
			invoiceLineFour.JI_CustomsSecondUnitQty = "KG";

			var invoiceLineFive = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineFive.JI_Tariff = "5810929080";
			invoiceLineFive.JI_LinePrice = 5000;
			invoiceLineFive.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineFive.US_UC_NKCountryOfExport = "CN";
			invoiceLineFive.JI_CustomsQuantity = 220m;
			invoiceLineFive.JI_CustomsUnitQty = "KG";

			var invoiceLineSix = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineSix.JI_ParentID = invoiceLineFive.PK;
			invoiceLineSix.JI_Tariff = "5407532060";
			invoiceLineSix.US_SupTariff = "99038803";
			invoiceLineSix.JI_LinePrice = 0;
			invoiceLineSix.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineSix.US_UC_NKCountryOfExport = "CN";
			invoiceLineSix.JI_CustomsQuantity = 579m;
			invoiceLineSix.JI_CustomsUnitQty = "M2";
			invoiceLineSix.JI_CustomsSecondQuantity = 220m;
			invoiceLineSix.JI_CustomsSecondUnitQty = "KG";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Normal duty on 1st invoice line = 5000 * 0.12", 600m, invoiceLineOne.US_Duty);
			AssertEquals("Prov duty on 1st invoice line should be zero", 0m, invoiceLineOne.US_SupDuty);
			AssertEquals("Normal duty on 2nd invoice line should be zero", 0m, invoiceLineTwo.US_Duty);
			AssertEquals("Prov duty on 2nd invoice line should be zero", 0m, invoiceLineTwo.US_SupDuty);
			AssertEquals("Normal duty on 3rd invoice line = 5000 * 0.12", 600m, invoiceLineThree.US_Duty);
			AssertEquals("Prov duty on 3rd invoice line = 5000 * 0.15", 750m, invoiceLineThree.US_SupDuty);
			AssertEquals("Normal duty on 4th invoice line should be zero", 0m, invoiceLineFour.US_Duty);
			AssertEquals("Prov duty on 4th invoice line should be zero", 0m, invoiceLineFour.US_SupDuty);
			AssertEquals("Normal duty on 5th invoice line = 5000 * 0.12", 600m, invoiceLineFive.US_Duty);
			AssertEquals("Prov duty on 5th invoice line should be zero.", 0m, invoiceLineFive.US_SupDuty);
			AssertEquals("Normal duty on 6th invoice line should be zero.", 0m, invoiceLineSix.US_Duty);
			AssertEquals("Prov duty on 6th invoice line = 5000 * 0.25", 1250m, invoiceLineSix.US_SupDuty);
		}

		public void TestCalculateDutyOnEmbroideryLineWithoutParentChild()
		{
			#region Setup Tariffs
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038803", "7", 0.25m, ZString.Empty);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99038803 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038803", new ZDateTime(2025, 01, 01), new ZDateTime(2079, 06, 06), "TEST 99038803", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038803);
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2025, 03, 04), new ZDateTime(2079, 06, 06), "TEST 99030124", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLine.JI_FormattedTariff = "5810.92.9080";
			invoiceLine.SupTariffFormatted = "9903.01.24";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.03";
			invoiceLine.JI_LinePrice = 5000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_Duty = 5000 * 0.074", 370m, invoiceLine.US_Duty);
			AssertEquals("US_SupDuty = 5000 * 0.2", 1000m, invoiceLine.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty = 5000 * 0.25", 1250m, invoiceLine.US_SupAdditionalTariff1Duty);
		}

		public void TestCalculateDutyOnEmbroideryLineWithParentChildAndAdditionalSupTariffs()
		{
			#region Setup Tariffs
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "5408323000", "7", 0.069m, "M2");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038803", "7", 0.25m, ZString.Empty);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99038803 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038803", new ZDateTime(2025, 01, 01), new ZDateTime(2079, 06, 06), "TEST 99038803", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99038803);
			var tariffView99030124 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030124", new ZDateTime(2025, 03, 04), new ZDateTime(2079, 06, 06), "TEST 99030124", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030124);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineOne = invoice.InvoiceLines.AddNew();
			invoiceLineOne.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLineOne.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLineOne.JI_FormattedTariff = "5810.92.9080";
			invoiceLineOne.SupTariffFormatted = "9903.01.24";
			invoiceLineOne.SupFormattedAdditionalTariff1 = "9903.88.03";
			invoiceLineOne.JI_LinePrice = 5000m;
			var invoiceLineTwo = invoice.InvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLineTwo.JI_FormattedTariff = "5407.53.2060";
			invoiceLineTwo.SupTariffFormatted = "9903.01.24";
			invoiceLineTwo.SupFormattedAdditionalTariff1 = "9903.88.03";
			var invoiceLineThree = invoice.InvoiceLines.AddNew();
			invoiceLineThree.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLineThree.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLineThree.JI_FormattedTariff = "5810.92.9080";
			invoiceLineThree.SupTariffFormatted = "9903.01.24";
			invoiceLineThree.SupFormattedAdditionalTariff1 = "9903.88.03";
			invoiceLineThree.JI_LinePrice = 5000m;
			var invoiceLineFour = invoice.InvoiceLines.AddNew();
			invoiceLineFour.JI_ParentID = invoiceLineThree.PK;
			invoiceLineFour.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLineFour.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLineFour.JI_FormattedTariff = "5408.32.3000";
			invoiceLineFour.SupTariffFormatted = "9903.01.24";
			invoiceLineFour.SupFormattedAdditionalTariff1 = "9903.88.03";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("US_Duty = 5000 * 0.12, the classification duty rate on 2nd invoice line (12%) is higher than 1st invoice line (7.4%)", 600m, invoiceLineOne.US_Duty);
			AssertEquals("US_SupDuty = 5000 * 0.2", 1000m, invoiceLineOne.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty = 5000 * 0.25", 1250m, invoiceLineOne.US_SupAdditionalTariff1Duty);
			AssertEquals("US_Duty should be zero on child line", 0m, invoiceLineTwo.US_Duty);
			AssertEquals("US_SupDuty should be zero on child line", 0m, invoiceLineTwo.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty should be zero on child line", 0m, invoiceLineTwo.US_SupAdditionalTariff1Duty);
			AssertEquals("US_Duty = 5000 * 0.074, the classification duty rate on 3rd invoice line (7.4%) is higher then 4th invoice line (6.9%)", 370m, invoiceLineThree.US_Duty);
			AssertEquals("US_SupDuty = 5000 * 0.2", 1000m, invoiceLineThree.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty = 5000 * 0.25", 1250m, invoiceLineThree.US_SupAdditionalTariff1Duty);
			AssertEquals("US_Duty should be zero on child line", 0m, invoiceLineFour.US_Duty);
			AssertEquals("US_SupDuty should be zero on child line", 0m, invoiceLineFour.US_SupDuty);
			AssertEquals("US_SupAdditionalTariff1Duty should be zero on child line", 0m, invoiceLineFour.US_SupAdditionalTariff1Duty);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5810929080", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute("RULE", "EMB", zzTariff);

			var tariff5810929080 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5810929080")).LastOrDefault();
			if (tariff5810929080 == null)
			{
				tariff5810929080 = Factory.New<USCTariff>();
				tariff5810929080.UE_Tariff = "5810929080";
				tariff5810929080.UE_DutyComputationCode = "7";
				tariff5810929080.UE_Column1RateAdValorem = 0.074m;
			}
			tariff5810929080.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff5810929080.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff5407532060 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5407532060")).LastOrDefault();
			if (tariff5407532060 == null)
			{
				tariff5407532060 = Factory.New<USCTariff>();
				tariff5407532060.UE_Tariff = "5407532060";
				tariff5407532060.UE_DutyComputationCode = "7";
				tariff5407532060.UE_Column1RateAdValorem = 0.12m;
			}
			tariff5407532060.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff5407532060.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff99038803 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038803")).LastOrDefault();
			if (tariff99038803 == null)
			{
				tariff99038803 = Factory.New<USCTariff>();
				tariff99038803.UE_Tariff = "99038803";
				tariff99038803.UE_DutyComputationCode = "7";
				tariff99038803.UE_Column1RateAdValorem = 0.25m;
			}
			tariff99038803.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038803.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff99038824 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038824")).LastOrDefault();
			if (tariff99038824 == null)
			{
				tariff99038824 = Factory.New<USCTariff>();
				tariff99038824.UE_Tariff = "99038824";
				tariff99038824.UE_DutyComputationCode = "7";
				tariff99038824.UE_Column1RateAdValorem = 0.15m;
			}
			tariff99038824.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038824.UE_DateTo = new ZDateTime(2021, 01, 01);

			Factory.Save();

			base.SetUp();
		}
	}
}
