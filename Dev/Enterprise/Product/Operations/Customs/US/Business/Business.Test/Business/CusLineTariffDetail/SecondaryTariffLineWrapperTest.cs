using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SecondaryTariffLineWrapperTest : TestCaseWithFactory
	{
		public void TestIsTIB9813SecondaryTariffLine()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9813Tariff.UE_Tariff;
			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			IDutyData dutyData = new SecondaryTariffLineWrapper(testHelper.ParentLine.CusEntryLine);
			Assert(!dutyData.IsCombineSecondaryTariffLine);
			dutyData = new SecondaryTariffLineWrapper(testHelper.ChildLine.CusEntryLine);
			Assert(dutyData.IsCombineSecondaryTariffLine);
		}

		public void TestAdditionalIDutyData()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData dutyData = new SecondaryTariffLineWrapper(invoiceLine2.CusEntryLine);
			AssertEquals(EntryTypeList.Codes.ConsumptionFreeDutiable, dutyData.EntryType);
			AssertEquals(false, dutyData.IsAMSFeeExempt);
			AssertEquals(false, dutyData.IsCottonFeeExemptIndicated);
			invoiceLine2.US_CottonCertificateNo = "ORGANICXX";
			AssertEquals(true, dutyData.IsAMSFeeExempt);
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals(true, dutyData.IsSetVLine);
			AssertEquals(invoiceLine.CusEntryLine, ((IDutyData)invoiceLine2.CusEntryLine).ParentTariffLine);
			invoiceLine2.US_SecondarySPI = "";
			AssertEquals(true, ((IDutyData)invoiceLine2.CusEntryLine).IsSecondaryTariffLine);
		}

		public void TestUseEffectiveSPIForSecondaryTariffLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfOrigin = "CA";
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 600m;
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			JobComInvoiceLine secondaryInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryInvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			SecondaryTariffLineWrapper wrapper = new SecondaryTariffLineWrapper(secondaryInvoiceLine.CusEntryLine);
			AssertEquals("Primary SPI", ZString.Empty, wrapper.SpecialProgramsIndicatorPrimary);
			AssertEquals("SPI Country", SpecialProgramList.Codes.CA, wrapper.SpecialProgramsIndicatorCountry);
			AssertEquals("Secondary SPI", SecondarySpecProgIndicatorList.Codes.F, wrapper.SpecialProgramsIndicatorSecondary);
		}

		[TestDate(2007, 05, 30)]
		public void TestFDA()
		{
			IOGA oga = new SecondaryTariffLineWrapper(entryLine);
			AssertEquals(0, oga.FDA.Count);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs.AddNew();
			AssertEquals(1, oga.FDA.Count);
		}

		[TestDate(2007, 05, 30)]
		public void TestDOT()
		{
			IOGA oga = new SecondaryTariffLineWrapper(entryLine);
			AssertEquals(false, oga.DOT.Any());
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.DOTs.AddNew();
			invoiceLine.DOTs.AddNew();
			AssertEquals(2, oga.DOT.Count());
		}

		public void TestGrossWeightInKilograms()
		{
			invoiceLine.JI_Weight = 1.6m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			SecondaryTariffLineWrapper secondaryTariffLineWrapper = new SecondaryTariffLineWrapper(entryLine);
			AssertEquals(2m, secondaryTariffLineWrapper.GrossWeightInKilograms);
		}

		public void TestIPGAMembers()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var secondaryTariffLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryTariffLine.LaceyActLines.AddNew();
			secondaryTariffLine.LaceyActLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var wrapper = new SecondaryTariffLineWrapper(secondaryTariffLine.CusEntryLine);
			var ipgaWrapper = (IGovernmentAgencies)wrapper;
			foreach (ILaceyActCommon data in ipgaWrapper.LaceyActData)
			{
				AssertNotNull(data);
			}
		}

		public void TestIDutyDataQuantities()
		{
			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.ProofGallon;
			secondaryLine.JI_CustomsQuantity = 500.59m;
			secondaryLine.JI_CustomsSecondUnitQty = "KG";
			secondaryLine.JI_CustomsSecondQuantity = 100.51m;
			secondaryLine.JI_CustomsThirdUnitQty = "LT";
			secondaryLine.JI_CustomsThirdQuantity = 50.49m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ISecondaryTariffLine tariffLine = ((ICusEntryLine)invoiceLine.CusEntryLine).SecondaryTariffLines.ElementAt(0);
			AssertEquals(501m, tariffLine.Quantity1);
			AssertEquals(101m, tariffLine.Quantity2);
			AssertEquals(50m, tariffLine.Quantity3);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 1.44m;
			secondaryLine.JI_Tariff = tariff.UE_Tariff;
			secondaryLine.CusEntryLine.CL_AdValoremTariff = tariff.UE_Tariff;
			secondaryLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.ProofLiter;
			secondaryLine.JI_CustomsSecondUnitQty = ABIUnitOfMeasureList.Codes.ProofLiter;
			secondaryLine.JI_CustomsThirdUnitQty = ABIUnitOfMeasureList.Codes.ProofLiter;
			secondaryLine.JI_CustomsQuantity = 500.59m;
			secondaryLine.JI_CustomsSecondQuantity = 100.51m;
			secondaryLine.JI_CustomsThirdQuantity = 50.49m;
			AssertEquals(500.59m, tariffLine.Quantity1);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
			tariff.UE_Column2RateSpecific = 1.44m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CU";
			AssertEquals(100.51m, tariffLine.Quantity2);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			dutyRate.UD_TaxFeeAdvalorem = 1.4m;
			dutyRate.UD_TaxFeeSpecificRate = 1.6m;
			AssertEquals(50.49m, tariffLine.Quantity3);
		}

		public void TestPGALineNumbersOnSecondaryLine()
		{
			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var cusEntryLine = invoiceLine.CusEntryLine;
			((IPGALineNumbers)cusEntryLine).EPAStartLineNumber = 1;
			((IPGALineNumbers)cusEntryLine).FSISStartLineNumber = 2;
			((IPGALineNumbers)cusEntryLine).NMFSStartLineNumber = 3;
			((IPGALineNumbers)cusEntryLine).FDAStartLineNumber = 4;
			((IPGALineNumbers)cusEntryLine).TTBStartLineNumber = 5;
			((IPGALineNumbers)cusEntryLine).NHTSAStartLineNumber = 6;
			((IPGALineNumbers)cusEntryLine).AMSStartLineNumber = 7;
			((IPGALineNumbers)cusEntryLine).APHStartLineNumber = 8;
			((IPGALineNumbers)cusEntryLine).FWSStartLineNumber = 9;
			((IPGALineNumbers)cusEntryLine).ATFStartLineNumber = 10;
			((IPGALineNumbers)cusEntryLine).CPSCStartLineNumber = 11;
			((IPGALineNumbers)cusEntryLine).OMCStartLineNumber = 12;
			((IPGALineNumbers)cusEntryLine).DEAStartLineNumber = 13;
			var secondaryEntryLine = new SecondaryTariffLineWrapper(secondaryLine.CusEntryLine);
			AssertEquals(1, ((IPGALineNumbers)secondaryEntryLine).EPAStartLineNumber);
			AssertEquals(2, ((IPGALineNumbers)secondaryEntryLine).FSISStartLineNumber);
			AssertEquals(3, ((IPGALineNumbers)secondaryEntryLine).NMFSStartLineNumber);
			AssertEquals(4, ((IPGALineNumbers)secondaryEntryLine).FDAStartLineNumber);
			AssertEquals(5, ((IPGALineNumbers)secondaryEntryLine).TTBStartLineNumber);
			AssertEquals(6, ((IPGALineNumbers)secondaryEntryLine).NHTSAStartLineNumber);
			AssertEquals(7, ((IPGALineNumbers)secondaryEntryLine).AMSStartLineNumber);
			AssertEquals(8, ((IPGALineNumbers)secondaryEntryLine).APHStartLineNumber);
			AssertEquals(9, ((IPGALineNumbers)secondaryEntryLine).FWSStartLineNumber);
			AssertEquals(10, ((IPGALineNumbers)secondaryEntryLine).ATFStartLineNumber);
			AssertEquals(11, ((IPGALineNumbers)secondaryEntryLine).CPSCStartLineNumber);
			AssertEquals(12, ((IPGALineNumbers)secondaryEntryLine).OMCStartLineNumber);
			AssertEquals(13, ((IPGALineNumbers)secondaryEntryLine).DEAStartLineNumber);
		}

		public void TestNMFSCOA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, TariffConditionValueTypes.Codes.PGA);
			Factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, zzTariff.PK, "test", true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();

			var tariff = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0000000001")).LastOrDefault();
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "0000000001";
			}
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;

			var nmfsCOALine = invoiceLine1.NMFSLines.AddNew();
			nmfsCOALine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var wrapper = new SecondaryTariffLineWrapper(invoiceLine.CusEntryLine);
			AssertEquals("NMFSCOAIndicator", "D", ((IGovernmentAgenciesIndicators)wrapper).NMFSCOAIndicator);
			AssertEquals("NMFSCOALines Count", 0, ((IGovernmentAgencies)wrapper).NMFSCOALines.Count());

			var wrapper1 = new SecondaryTariffLineWrapper(invoiceLine1.CusEntryLine);
			AssertEquals("NMFSCOAIndicator", ZString.Empty, ((IGovernmentAgenciesIndicators)wrapper1).NMFSCOAIndicator);
			AssertEquals("NMFSCOALines Count", 1, ((IGovernmentAgencies)wrapper1).NMFSCOALines.Count());
		}

		public void TestHFCRelated()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-30);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "EH2";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.USHFCHeaders.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var wrapper = new SecondaryTariffLineWrapper(invoiceLine.CusEntryLine);
			AssertEquals(OGAIndicatorList.Codes.Declared, ((IGovernmentAgenciesIndicators)wrapper).HFCIndicator);
			AssertEquals(1, (((IGovernmentAgencies)wrapper).EPA_HFCHeaders).Count());

			invoiceLine.USHFCHeaders.RemoveAndDeleteAll();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;
			wrapper = new SecondaryTariffLineWrapper(invoiceLine.CusEntryLine);
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, ((IGovernmentAgenciesIndicators)wrapper).HFCIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, ((IGovernmentAgenciesIndicators)wrapper).HFCDisclaimReason);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
		}

		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		JobDeclaration declaration;
	}
}
