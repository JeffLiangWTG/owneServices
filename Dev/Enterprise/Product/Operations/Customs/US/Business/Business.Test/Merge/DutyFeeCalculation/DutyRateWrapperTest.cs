using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DutyRateWrapperTest : TestCaseWithFactory
	{
		public void TestGeneralWrapperRateDependsOnISOCountry()
		{
			var uscCountry = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, Core.Constants.CountryCodes.Russia);
			if (uscCountry == null)
			{
				uscCountry = Factory.New<USCCountry>();
				uscCountry.UC_Code = Core.Constants.CountryCodes.Russia;
			}
			uscCountry.UC_RateColumnIndicator = "2";
			uscCountry.UC_RateColumnBeginDate = ZDateTime.Empty;
			uscCountry.UC_RateColumnEndDate = ZDateTime.Empty;

			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "0000000000";
			uscTariff.UE_DateFrom = ZDateTime.Now.AddMonths(-1);
			uscTariff.UE_DateTo = ZDateTime.Now.AddMonths(1);
			uscTariff.UE_Column1RateAdValorem = 1m;
			uscTariff.UE_Column2RateAdValorem = 2m;
			uscTariff.UE_Column1RateOther = 3m;
			uscTariff.UE_Column2RateOther = 4m;
			uscTariff.UE_Column1RateSpecific = 5m;
			uscTariff.UE_Column2RateSpecific = 6m;

			Factory.Save();

			var generalWrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, uscTariff, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Russia);
			AssertEquals(2m, generalWrapper.Advalorem);
			AssertEquals(4m, generalWrapper.Other);
			AssertEquals(6m, generalWrapper.Specific);

			generalWrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, uscTariff, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals(1m, generalWrapper.Advalorem);
			AssertEquals(3m, generalWrapper.Other);
			AssertEquals(5m, generalWrapper.Specific);

			uscCountry.UC_RateColumnIndicator = "3";
			generalWrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, uscTariff, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Russia);
			AssertEquals(1m, generalWrapper.Advalorem);
			AssertEquals(3m, generalWrapper.Other);
			AssertEquals(5m, generalWrapper.Specific);

			uscCountry.UC_RateColumnBeginDate = ZDateTime.Now.AddMonths(1);
			uscCountry.UC_RateColumnEndDate = ZDateTime.Now.AddMonths(2);
			generalWrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, uscTariff, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Russia);
			AssertEquals(1m, generalWrapper.Advalorem);
			AssertEquals(3m, generalWrapper.Other);
			AssertEquals(5m, generalWrapper.Specific);

			uscCountry.UC_RateColumnIndicator = "2";
			generalWrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, uscTariff, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Russia);
			AssertEquals(1m, generalWrapper.Advalorem);
			AssertEquals(3m, generalWrapper.Other);
			AssertEquals(5m, generalWrapper.Specific);

			uscCountry.UC_RateColumnBeginDate = ZDateTime.Now.AddMonths(-1);
			uscCountry.UC_RateColumnEndDate = ZDateTime.Now.AddMonths(2);
			generalWrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, uscTariff, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Russia);
			AssertEquals(2m, generalWrapper.Advalorem);
			AssertEquals(4m, generalWrapper.Other);
			AssertEquals(6m, generalWrapper.Specific);

			uscCountry.UC_RateColumnIndicator = "3";
			generalWrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, uscTariff, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Russia);
			AssertEquals(1m, generalWrapper.Advalorem);
			AssertEquals(3m, generalWrapper.Other);
			AssertEquals(5m, generalWrapper.Specific);
		}

		public void TestNullTariff()
		{
			AssertNotNull(DutyRateWrapper.GetWrapper(ZDateTime.Now, null, "", "", ""));
		}

		[TestDate(2011, 1, 2)]
		public void TestGSPExpireIn2011()
		{
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, "Expired Special Program Indicator");
			universalReferenceTestHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, PrimarySpecProgramIndicatorList.Codes.A, "A", new ZDateTime(2011, 1, 1), new ZDateTime(2011, 11, 5), Universal.RefCusCodeListAttributeTypes.Codes.USSPIException, PrimarySpecProgramIndicatorList.Codes.D);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "8483304080";

			invoiceLine.US_UC_NKCountryOfExport = "ID";
			invoiceLine.US_UC_NKCountryOfOrigin = "ID";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertEquals("PreCondition", ZString.Empty, UniversalReferenceDataHelper.GetEffectiveSPIForDutyCalculation(Factory, invoiceLine.US_SPI, invoiceLine.EffectiveDateForDutyRate, invoiceLine.CountryOfOrigin_US));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("With expired SPI", 225m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);

			invoiceLine.US_UC_NKCountryOfExport = "ZA";//'D' is eligible
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";//'D' is eligible
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition", PrimarySpecProgramIndicatorList.Codes.A, UniversalReferenceDataHelper.GetEffectiveSPIForDutyCalculation(Factory, invoiceLine.US_SPI, invoiceLine.EffectiveDateForDutyRate, invoiceLine.CountryOfOrigin_US));
			AssertEquals("With AGOG country", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
		}

		public void TestSugarWithFreeDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2011, 10, 10);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "1701990500";
			AssertNotNull(invoiceLine.ImportTariff);
			invoiceLine.JI_CustomsQuantity = 41106m;
			invoiceLine.JI_CustomsSecondQuantity = 9906m;

			Assert("DutyFree", invoiceLine.ImportTariff.BecomesDutyFreeDueTo(PrimarySpecProgramIndicatorList.Codes.A));

			invoiceLine.US_UC_NKCountryOfExport = "MU";
			invoiceLine.US_UC_NKCountryOfOrigin = "MU";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("It should be duty free", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
		}

		[TestDate(2011, 10, 16)]
		public void TestGSPReactivatedIn2011()
		{
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, "Expired Special Program Indicator");
			universalReferenceTestHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, PrimarySpecProgramIndicatorList.Codes.A, "A", new ZDateTime(2011, 1, 1), new ZDateTime(2011, 10, 15), Universal.RefCusCodeListAttributeTypes.Codes.USSPIException, PrimarySpecProgramIndicatorList.Codes.D);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2011, 10, 10);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "8483304080";

			invoiceLine.US_UC_NKCountryOfExport = "ID";
			invoiceLine.US_UC_NKCountryOfOrigin = "ID";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertEquals("PreCondition", PrimarySpecProgramIndicatorList.Codes.A, UniversalReferenceDataHelper.GetEffectiveSPIForDutyCalculation(Factory, invoiceLine.US_SPI, invoiceLine.EffectiveDateForDutyRate, invoiceLine.CountryOfOrigin_US));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("SPI re-activated", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);

			invoiceLine.US_UC_NKCountryOfExport = "ZA";//'D' is eligible
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";//'D' is eligible
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition", PrimarySpecProgramIndicatorList.Codes.A, UniversalReferenceDataHelper.GetEffectiveSPIForDutyCalculation(Factory, invoiceLine.US_SPI, invoiceLine.EffectiveDateForDutyRate, invoiceLine.CountryOfOrigin_US));
			AssertEquals("With AGOG country", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
		}

		public void TestGeneral()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Column1RateAdValorem = 123m;
			var wrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, tariff, "", "", "");
			AssertEquals(123m, wrapper.Advalorem);
		}

		public void TestSpecial()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "AU";
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_ISOCountryCode = Core.Constants.CountryCodes.Australia;
			dutyRate.UD_AdValoremSpecialRate = 123m;
			var wrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, tariff, "AU", "", Core.Constants.CountryCodes.Australia);
			AssertEquals(123m, wrapper.Advalorem);
		}

		public void TestSpecialPrimary()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "AU";
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_ISOCountryCode = Core.Constants.CountryCodes.Australia;
			dutyRate.UD_AdValoremSpecialRate = 123m;
			var wrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, tariff, "", PrimarySpecProgramIndicatorList.Codes.C, Core.Constants.CountryCodes.Australia);
			AssertEquals(0m, wrapper.Advalorem);
		}

		public void TestWDutyFree()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "AU";
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_ISOCountryCode = Core.Constants.CountryCodes.Australia;
			dutyRate.UD_AdValoremSpecialRate = 123m;
			var wrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, tariff, "", PrimarySpecProgramIndicatorList.Codes.W, Core.Constants.CountryCodes.Australia);
			AssertEquals(0m, wrapper.Advalorem);
		}

		public void TestUnfriendly()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Column2RateAdValorem = 123m;
			var wrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, tariff, "", "", Core.Constants.CountryCodes.KoreaNorth);
			AssertEquals(123m, wrapper.Advalorem);
		}

		public void TestZeroDutyWrapperIsReturnedOnTranslatedSPI()
		{
			var tariff = USCTariffTest.CreateNewTariffIfNotExist(Factory, "6105202010", "7", 0.32m, "DOZ");
			tariff.UE_SPICode = "IL";
			var wrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, tariff, "", "N", Core.Constants.CountryCodes.Egypt);
			AssertEquals(typeof(DutyRateWrapper.ZeroDutyWrapper), wrapper.GetType());

			tariff = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");
			tariff.UE_SPICode = ZString.Empty;
			wrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, tariff, "", "N", Core.Constants.CountryCodes.Egypt);
			AssertEquals(0.2m, wrapper.Advalorem);
		}

		public void TestZeroDutyWrapperIsReturnedOnZeroDutySPI()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_SPICode = "L";
			var wrapper = DutyRateWrapper.GetWrapper(ZDateTime.Now, tariff, "", PrimarySpecProgramIndicatorList.Codes.L, Core.Constants.CountryCodes.Egypt);
			AssertEquals(typeof(DutyRateWrapper.ZeroDutyWrapper), wrapper.GetType());
		}

		public void TestGeneralWrapperIsReturnedFor99038501()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_SPICode = "CA";
			tariff1.UE_Tariff = "8483304080";
			tariff1.UE_Column1RateAdValorem = 0.1m;
			tariff1.UE_DutyComputationCode = "7";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_SPICode = "CA";
			tariff2.UE_Tariff = "99038501";
			tariff2.UE_Column1RateAdValorem = 0.2m;
			tariff2.UE_DutyComputationCode = "7";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2011, 10, 10);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Tariff = "8483304080";
			invoiceLine.US_SupTariff = "99038501";
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			invoiceLine.US_SPI = "CA";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 2000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1000m, invoiceLine.US_SupDuty);
		}

		[TestDate(2021, 08, 31)]
		public void TestFactoryCachedValueUsedInRateWrapper()
		{
			#region Setup Tariffs

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "3603009020";
			tariff.UE_Unit1 = ABIUnitOfMeasureList.Codes.Number;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.002;
			tariff.UE_DateFrom = ZDateTime.Today.AddMonths(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_LinePrice = 100m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine = declaration.FormalEntry.AllEntryLines[0];
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedEntryLine = newFactory.Load<CusEntryLine>(entryLine.PK);
			USCTariff tariffCached;
			Assert("Cached Value Not Found In Factory", !newFactory.TryGetValueFromCacheOnly("360300902031-Aug-21", out tariffCached));
			AssertNull(tariffCached);

			var wrapperForEntryLine = DutyRateWrapper.GetWrapper(loadedEntryLine);
			Assert("Cached Value Found In Factory", newFactory.TryGetValueFromCacheOnly("360300902031-Aug-21", out tariffCached));
			AssertNotNull(tariffCached);
			AssertEquals(0.002m, wrapperForEntryLine.Advalorem);
		}
	}
}
