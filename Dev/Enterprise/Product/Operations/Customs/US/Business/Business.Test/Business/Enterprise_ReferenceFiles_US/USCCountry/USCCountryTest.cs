using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCCountry))]
	sealed class USCCountryTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2012, 06, 01)]
		public void TestIsRestrictedCountry()
		{
			var aCountry = Factory.New<USCCountry>();
			aCountry.UC_RestrictionIndicator = true;
			aCountry.UC_RestrictionIndicatorBeginDate = new ZDateTime(2012, 05, 01);
			aCountry.UC_RestrictionIndicatorEndDate = new ZDateTime(2013, 05, 01);
			Assert(aCountry.IsRestrictedCountry(new ZDateTime(2012, 06, 20)));
			Assert(aCountry.IsRestrictedCountry());
			Assert(!aCountry.IsRestrictedCountry(new ZDateTime(2014, 01, 01)));
		}

		public void TestIsLeastDevelopedCountry()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = Core.Constants.CountryCodes.Zambia;
			AssertEquals(false, country.IsLeastDevelopedCountry(ZDateTime.Today));

			country.UC_RateColumnIndicator = "3";
			AssertEquals(true, country.IsLeastDevelopedCountry(ZDateTime.Today));

			country.UC_RateColumnEndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, country.IsLeastDevelopedCountry(ZDateTime.Today));
		}

		public void TestGetRateIndicator()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_RateColumnIndicator = "1";
			country.UC_RateColumnEndDate = ZDateTime.Today.AddDays(-1);

			AssertEquals("RateColumn", ZString.Empty, country.GetRateIndicator(ZDate.Today));
			AssertEquals("RateColumn", "1", country.GetRateIndicator(ZDate.Today.AddDays(-1)));
		}

		public void TestIsNAFTACountry()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "CA";
			AssertEquals(true, country.IsNAFTACountry);

			country.UC_Code = "XO";
			AssertEquals(true, country.IsNAFTACountry);

			country.UC_Code = "AU";
			AssertEquals(false, country.IsNAFTACountry);

			country.UC_Code = "MX";
			AssertEquals(true, country.IsNAFTACountry);
		}

		public void TestIsEligibleForCBI()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_SPICode = "E";
			country.UC_SPIEndDate = ZDateTime.Today.AddDays(-1);

			AssertEquals(false, country.IsEligibleForCBI(ZDateTime.Today));

			AssertEquals(true, country.IsEligibleForCBI(ZDateTime.Today.AddDays(-1)));

			country.UC_SPICode = "J";
			AssertEquals(false, country.IsEligibleForCBI(ZDateTime.Today.AddDays(-1)));
		}

		public void TestIsEligibleForATPA()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_SPICode = "J";
			country.UC_SPIEndDate = ZDateTime.Today.AddDays(-1);

			AssertEquals(false, country.IsEligibleForATPA(ZDateTime.Today));

			AssertEquals(true, country.IsEligibleForATPA(ZDateTime.Today.AddDays(-1)));

			country.UC_SPICode = "E";
			AssertEquals(false, country.IsEligibleForATPA(ZDateTime.Today.AddDays(-1)));
		}

		public void TestIsEligibleForCAFTA()
		{
			USCCountry country = Factory.New<USCCountry>();
			AssertEquals(false, country.IsEligibleForCAFTA(ZDateTime.BrettsBirthday));

			country.UC_MiscellaneousSPIIndicator = "P";
			AssertEquals(true, country.IsEligibleForCAFTA(ZDateTime.BrettsBirthday));

			country.UC_MiscellaneousSPIBeginDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(false, country.IsEligibleForCAFTA(ZDateTime.BrettsBirthday));

			country.UC_MiscellaneousSPIBeginDate = ZDateTime.Empty;
			country.UC_MiscellaneousSPIEndDate = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertEquals(false, country.IsEligibleForCAFTA(ZDateTime.BrettsBirthday));
		}

		public void TestCountryIssuingStandardVisa()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = Core.Constants.CountryCodes.Bahrain;
			AssertEquals(true, country.IssuesStandardisedVisas);

			country.UC_Code = Core.Constants.CountryCodes.Afghanistan;
			AssertEquals(false, country.IssuesStandardisedVisas);

			country.UC_Code = Core.Constants.CountryCodes.Bangladesh;
			AssertEquals(true, country.IssuesStandardisedVisas);

			country.UC_Code = Core.Constants.CountryCodes.Australia;
			AssertEquals(false, country.IssuesStandardisedVisas);
		}

		public void TestIsValidForSPI()
		{
			USCCountry country = Factory.New<USCCountry>();
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.C, ZDateTime.Empty));

			country.UC_SpecialTradeProgramsIndicator = PrimarySpecProgramIndicatorList.Codes.D;
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.R, ZDateTime.Empty));

			country.UC_SpecialTradeProgramsIndicator = PrimarySpecProgramIndicatorList.Codes.D;
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.D, ZDateTime.Empty));

			country.UC_SpecialTradeProgramsBeginDate = ZDateTime.BrettsBirthday;
			country.UC_SpecialTradeProgramsEndDate = ZDateTime.Today;
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.D, ZDateTime.Today.AddDays(1)));
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.D, ZDateTime.Today));
		}

		public void TestIsValidSPIForSPICountry()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "AU";
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.AU, ZDateTime.Empty));

			country.UC_Code = "BH";
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.AU, ZDateTime.Empty));

			country.UC_Code = Core.Constants.CountryCodes.Oman;
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.OM, ZDateTime.Empty));

			country.UC_Code = Core.Constants.CountryCodes.Peru;
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.PE, ZDateTime.Empty));

			country.UC_Code = Core.Constants.CountryCodes.Colombia;
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.CO, ZDateTime.Empty));

			country.UC_Code = Core.Constants.CountryCodes.Panama;
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.PA, ZDateTime.Empty));

			country.UC_Code = Core.Constants.CountryCodes.Nepal;
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.NP, ZDateTime.Empty));

			country.UC_Code = Core.Constants.CountryCodes.Japan;
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.JP, ZDateTime.Empty));

			country.UC_Code = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.KR, new ZDateTime(2012, 2, 29)));
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.KR, new ZDateTime(2012, 3, 15)));

			country.UC_SPICode = "K";
			country.UC_SPIBeginDate = new ZDateTime(2012, 3, 15);
			country.UC_SPIEndDate = new ZDateTime(2099, 12, 31);
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.KR, new ZDateTime(2012, 2, 29)));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.KR, new ZDateTime(2012, 3, 15)));
		}

		public void TestIsValidSPIForCA()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "CA";
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.CA, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.S, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.SPlus, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.B, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.BSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.CSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.KSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.LSharp, ZDateTime.Empty));

			country.UC_Code = "XO";
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.CA, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.S, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.SPlus, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.B, ZDateTime.Empty));
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.MX, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.BSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.CSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.KSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.LSharp, ZDateTime.Empty));
		}

		public void TestIsValidSPIForMX()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "MX";

			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.MX, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.S, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.SPlus, ZDateTime.Empty));
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.BSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.CSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.KSharp, ZDateTime.Empty));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.LSharp, ZDateTime.Empty));

			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.AU, ZDateTime.Empty));
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.B, ZDateTime.Empty));
		}

		public void TestIsValidSPIForA()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "~~";
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.A, ZDateTime.Empty));

			country.UC_GSPIndicator = true;
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.A, ZDateTime.Empty));
		}

		public void TestIsValidSPIForCAFTA()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "~~";
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.P, ZDateTime.Today));
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.PPlus, ZDateTime.Today));

			country.UC_MiscellaneousSPIIndicator = "P";
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.P, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.PPlus, ZDateTime.Today));

			country.UC_MiscellaneousSPIEndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.P, ZDateTime.Today));
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.PPlus, ZDateTime.Today));
		}

		public void TestIsValidSPIForAGOA()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "~~";
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.D, ZDateTime.Today));

			country.UC_SpecialTradeProgramsIndicator = "D";
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.D, ZDateTime.Today));

			country.UC_SpecialTradeProgramsEndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.D, ZDateTime.Today));
		}

		public void TestIsValidSPIForCBI()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "~~";
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.E, ZDateTime.Today));

			country.UC_SPICode = "E";
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.E, ZDateTime.Today));

			country.UC_SPIEndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.E, ZDateTime.Today));
		}

		public void TestIsValidSPIForATPA()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "~~";
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.J, ZDateTime.Today));

			country.UC_SPICode = "J";
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.J, ZDateTime.Today));

			country.UC_SPIEndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.J, ZDateTime.Today));
		}

		public void TestIsValidSPIForATPDEA()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "~~";
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.JPlus, ZDateTime.Today));

			country.UC_SpecialTradeProgramsIndicator = "J";
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.JPlus, ZDateTime.Today));

			country.UC_SpecialTradeProgramsEndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, country.IsValidForSPI(SpecialProgramList.Codes.JPlus, ZDateTime.Today));
		}

		public void TestIsValidSPIForCBTPA()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "~~";
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.R, ZDateTime.Today));

			country.UC_SpecialTradeProgramsIndicator = "R";
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.R, ZDateTime.Today));

			country.UC_SpecialTradeProgramsEndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.R, ZDateTime.Today));
		}

		public void TestIsValidSPIForYAndZ()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "~~";
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.Y, ZDateTime.Today));
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.Z, ZDateTime.Today));

			country.UC_SPICode = PrimarySpecProgramIndicatorList.Codes.Y;
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.Y, ZDateTime.Today));
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.Z, ZDateTime.Today));

			country.UC_SPICode = PrimarySpecProgramIndicatorList.Codes.Z;
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.Y, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.Z, ZDateTime.Today));

			country.UC_SPIEndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.Z, ZDateTime.Today));
		}

		public void TestIsEligibleForSpecialTradeProgram()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_SpecialTradeProgramsIndicator = "D";
			AssertEquals(true, country.IsEligibleForAGOA(ZDateTime.BrettsBirthday));

			country.UC_SpecialTradeProgramsBeginDate = ZDateTime.BrettsBirthday.AddDays(1);
			country.UC_SpecialTradeProgramsEndDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(false, country.IsEligibleForAGOA(ZDateTime.BrettsBirthday));

			country.UC_SpecialTradeProgramsIndicator = "R";
			country.UC_SpecialTradeProgramsBeginDate = ZDateTime.Empty;
			country.UC_SpecialTradeProgramsEndDate = ZDateTime.Empty;
			AssertEquals(true, country.IsEligibleForCBTPA(ZDateTime.BrettsBirthday));

			country.UC_SpecialTradeProgramsBeginDate = ZDateTime.BrettsBirthday.AddDays(1);
			country.UC_SpecialTradeProgramsEndDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(false, country.IsEligibleForCBTPA(ZDateTime.BrettsBirthday));

			country.UC_SpecialTradeProgramsIndicator = "J";
			country.UC_SpecialTradeProgramsBeginDate = ZDateTime.Empty;
			country.UC_SpecialTradeProgramsEndDate = ZDateTime.Empty;
			AssertEquals(true, country.IsEligibleForATPDEA(ZDateTime.BrettsBirthday));

			country.UC_SpecialTradeProgramsBeginDate = ZDateTime.BrettsBirthday.AddDays(1);
			country.UC_SpecialTradeProgramsEndDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(false, country.IsEligibleForATPDEA(ZDateTime.BrettsBirthday));
		}

		public void TestIsValidForGSP()
		{
			USCCountry country = Factory.New<USCCountry>();
			AssertEquals(false, country.IsValidForGSP(ZDateTime.BrettsBirthday));

			country.UC_GSPIndicator = ZBool.True;
			AssertEquals(true, country.IsValidForGSP(ZDateTime.BrettsBirthday));

			country.UC_GSPBeginDate = ZDateTime.BrettsBirthday.AddDays(-1);
			country.UC_GSPEndDate = ZDateTime.BrettsBirthday;
			AssertEquals(true, country.IsValidForGSP(ZDateTime.BrettsBirthday.AddHours(12)));
		}

		public void TestIsFolkloreAgreementsCountry()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = Core.Constants.CountryCodes.Bangladesh;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Colombia;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.India;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Japan;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Malaysia;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Mexico;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Pakistan;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Peru;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Philippines;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Taiwan;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Thailand;
			AssertEquals(true, country.IsFolkloreAgreementsCountry);

			country.UC_Code = Core.Constants.CountryCodes.Sweden;
			AssertEquals(false, country.IsFolkloreAgreementsCountry);
		}

		public void TestIsInsularPossessionsCountry()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_SPICode = "Y";
			country.UC_SPIEndDate = ZDateTime.Today.AddDays(-1);

			Assert(!country.IsInsularPossessionsCountry(ZDateTime.Today));

			Assert(country.IsInsularPossessionsCountry(ZDateTime.Today.AddDays(-1)));

			country.UC_SPICode = "J";
			Assert(!country.IsInsularPossessionsCountry(ZDateTime.Today.AddDays(-1)));
		}

		public void TestIsValidForSPIWhenIsCanadianRegion()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = CanadaProvinceTerritoryCodes.Codes.XD;
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.S, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.SPlus, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.CA, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.BSharp, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.B, ZDateTime.Today));

			country.UC_Code = CanadaProvinceTerritoryCodes.Codes.XB;
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.S, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.SPlus, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.CA, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(SpecialProgramList.Codes.BSharp, ZDateTime.Today));
			AssertEquals(true, country.IsValidForSPI(PrimarySpecProgramIndicatorList.Codes.B, ZDateTime.Today));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			USCCountry result = factory.New<USCCountry>();
			result.UC_RateColumnIndicator = "1";
			result.UC_SPICode = "1";
			result.UC_Code = "#@";
			return result;
		}
	}
}
