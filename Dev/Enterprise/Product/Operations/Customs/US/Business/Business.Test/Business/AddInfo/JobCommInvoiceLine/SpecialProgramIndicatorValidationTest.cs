using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SpecialProgramIndicatorValidationTest : TestCaseWithFactory
	{
		public void TestGetErrorTextMessage()
		{
			string errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText("AU", "AU", SpecialProgramList.Codes.BSharp, ZDateTime.Today, null);
			AssertEquals("Automotive Product Trade Act is only supported when importing goods of an origin of Canada.", errorText);
			string errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText("", "", SpecialProgramList.Codes.AU, ZDateTime.Today, null);
			AssertEquals("Australia Free Trade Agreement requires that goods are exported and originated directly from Australia.", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Bahrain, Core.Constants.CountryCodes.Iran, SpecialProgramList.Codes.IL, ZDateTime.Today, null);
			AssertEquals("Israel Free Trade Agreement requires that goods are exported and originated directly from Israel.", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Bahrain, Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.BH, ZDateTime.Today, null);
			AssertEquals("Bahrain Free Trade Agreement requires that goods are exported and originated directly from Bahrain.", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Zaire, Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.JO, ZDateTime.Today, null);
			AssertEquals("Jordan Free Trade Agreement requires that goods are exported and originated directly from Jordan.", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Zaire, Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.MA, ZDateTime.Today, null);
			AssertEquals("Morocco Free Trade Agreement requires that goods are exported and originated directly from Morocco.", errorText1);
			USCTariff importTariff = new USCTariff.Loader(Factory).LoadBestMatch("7209170030", ZDateTime.Today);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Zaire, Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.CL, ZDateTime.Today, importTariff);
			AssertEquals("Chile Free Trade Agreement requires that goods are exported directly from Chile, with the exception of Tariff Number '991199'", errorText1);
			importTariff = new USCTariff.Loader(Factory).LoadBestMatch("99119920", ZDateTime.Today);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText("", Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.CL, ZDateTime.Today, importTariff);
			AssertNotContains("Chile Free Trade Agreement requires that goods are exported directly from Chile, with the exception of Tariff Number '991199'", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Zaire, Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.OM, ZDateTime.Today, null);
			AssertEquals("OMAN Free Trade Agreement requires that goods are exported and originated directly from Oman.", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Zaire, Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.PE, ZDateTime.Today, null);
			AssertEquals("PERU Free Trade Agreement requires that goods are exported and originated directly from Peru.", errorText1);
			importTariff = new USCTariff.Loader(Factory).LoadBestMatch("99990084", ZDateTime.Today);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Zaire, Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.SG, ZDateTime.Today, importTariff);
			AssertEquals(SpecialProgramIndicatorValidation.SingaporeFTAError, errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Zaire, Core.Constants.CountryCodes.WesternSamoa, SpecialProgramList.Codes.SG, ZDateTime.Today, null);
			AssertNotContains(SpecialProgramIndicatorValidation.SingaporeFTAError, errorText1);
			var errorTextMexican = "Mexican Special Rate under NAFTA requires that goods are originated from Mexico and exported from a NAFTA country.";
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Mexico, Core.Constants.CountryCodes.Mexico, SpecialProgramList.Codes.MX, ZDateTime.Today, null);
			AssertEquals("", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Mexico, Core.Constants.CountryCodes.Canada, SpecialProgramList.Codes.MX, ZDateTime.Today, null);
			AssertEquals("", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText("", Core.Constants.CountryCodes.Mexico, SpecialProgramList.Codes.MX, ZDateTime.Today, null);
			AssertEquals(errorTextMexican, errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Mexico, "", SpecialProgramList.Codes.MX, ZDateTime.Today, null);
			AssertEquals(errorTextMexican, errorText1);
			var errorTextCanadian = "Canadian Special Rate under NAFTA requires that goods are originated from Canada and exported from a NAFTA country.";
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Canada, SpecialProgramList.Codes.CA, ZDateTime.Today, null);
			AssertEquals("", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Mexico, SpecialProgramList.Codes.CA, ZDateTime.Today, null);
			AssertEquals("", errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText("", Core.Constants.CountryCodes.Canada, SpecialProgramList.Codes.CA, ZDateTime.Today, null);
			AssertEquals(errorTextCanadian, errorText1);
			errorText1 = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Canada, "", SpecialProgramList.Codes.CA, ZDateTime.Today, null);
			AssertEquals(errorTextCanadian, errorText1);
		}

		public void TestMXOriginAndExport()
		{
			var description = new SpecialProgramList().GetDescriptionFromCode(SpecialProgramList.Codes.MX);
			string errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Mexico, Core.Constants.CountryCodes.Afghanistan, SpecialProgramList.Codes.MX, ZDateTime.Today, null);
			AssertEquals(description + " requires that goods are originated from Mexico and exported from a NAFTA country.", errorText);
			errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Mexico, Core.Constants.CountryCodes.Mexico, SpecialProgramList.Codes.MX, ZDateTime.Today, null);
			AssertEquals("", errorText);
		}

		public void TestCAOriginAndExport()
		{
			var description = new SpecialProgramList().GetDescriptionFromCode(SpecialProgramList.Codes.CA);
			string errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Afghanistan, SpecialProgramList.Codes.CA, ZDateTime.Today, null);
			AssertEquals(description + " requires that goods are originated from Canada and exported from a NAFTA country.", errorText);
			errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(CanadaProvinceTerritoryCodes.Codes.XE, Core.Constants.CountryCodes.Canada, SpecialProgramList.Codes.CA, ZDateTime.Today, null);
			AssertEquals("", errorText);
			errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Canada, SpecialProgramList.Codes.CA, ZDateTime.Today, null);
			AssertEquals("", errorText);
		}

		public void TestOMANOriginAndExport()
		{
			string errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Oman, Core.Constants.CountryCodes.Australia, SpecialProgramList.Codes.OM, ZDateTime.Today, null);
			AssertEquals("OMAN Free Trade Agreement requires that goods are exported and originated directly from Oman.", errorText);
			errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Oman, Core.Constants.CountryCodes.Oman, SpecialProgramList.Codes.OM, ZDateTime.Today, null);
			AssertEquals("", errorText);
		}

		public void TestPERUOriginAndExport()
		{
			string errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Peru, Core.Constants.CountryCodes.Australia, SpecialProgramList.Codes.PE, ZDateTime.Today, null);
			AssertEquals("PERU Free Trade Agreement requires that goods are exported and originated directly from Peru.", errorText);
			errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Peru, Core.Constants.CountryCodes.Peru, SpecialProgramList.Codes.PE, ZDateTime.Today, null);
			AssertEquals("", errorText);
		}

		public void TestSingaporeOriginAustraliaExportFails()
		{
			USCTariff importTariff = new USCTariff.Loader(Factory).LoadBestMatch("99990084", ZDateTime.Today);
			string errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText("SG", "AU", SpecialProgramList.Codes.SG, ZDateTime.Today, importTariff);
			AssertEquals(SpecialProgramIndicatorValidation.SingaporeFTAError, errorText);
		}

		public void TestJapanOriginAndExport()
		{
			string errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Japan, Core.Constants.CountryCodes.Australia, SpecialProgramList.Codes.JP, ZDateTime.Today, null);
			AssertEquals("Japan Trade Agreement requires that goods are exported and originated directly from Japan.", errorText);
			errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText(Core.Constants.CountryCodes.Japan, Core.Constants.CountryCodes.Japan, SpecialProgramList.Codes.JP, ZDateTime.Today, null);
			AssertEquals("", errorText);
		}

		public void TestInvalidSpecialProgramIndicators()
		{
			CheckSPI("AU", SpecialProgramList.Codes.AU);
			CheckSPI("CA", SpecialProgramList.Codes.BSharp);
			CheckSPI("CA", SpecialProgramList.Codes.CA);
			CheckSPI("CL", SpecialProgramList.Codes.CL);
			CheckSPI("CA", SpecialProgramList.Codes.CSharp);
			CheckSPI("MX", SpecialProgramList.Codes.CSharp);
			CheckSPI("IL", SpecialProgramList.Codes.IL);
			CheckSPI("JO", SpecialProgramList.Codes.JO);
			CheckSPI("CA", SpecialProgramList.Codes.KSharp);
			CheckSPI("MX", SpecialProgramList.Codes.KSharp);
			CheckSPI("CA", SpecialProgramList.Codes.LSharp);
			CheckSPI("MX", SpecialProgramList.Codes.LSharp);
			CheckSPI("MX", SpecialProgramList.Codes.MX);
			CheckSPI("SG", SpecialProgramList.Codes.SG);
			CheckSPI(Core.Constants.CountryCodes.KoreaSouth, SpecialProgramList.Codes.KR);
			CheckSPI(Core.Constants.CountryCodes.Colombia, SpecialProgramList.Codes.CO);
			CheckSPI(Core.Constants.CountryCodes.Panama, SpecialProgramList.Codes.PA);
			CheckSPI(Core.Constants.CountryCodes.Nepal, SpecialProgramList.Codes.NP);
			CheckSPI("US", SpecialProgramList.Codes.S);
			CheckSPI("MX", SpecialProgramList.Codes.S);
			CheckSPI("CA", SpecialProgramList.Codes.S);
			CheckSPI("US", SpecialProgramList.Codes.SPlus);
			CheckSPI("MX", SpecialProgramList.Codes.SPlus);
			CheckSPI("CA", SpecialProgramList.Codes.SPlus);
		}

		public void TestAndeanDruggies()
		{
			CheckSPI("BO", SpecialProgramList.Codes.JPlus);
			CheckSPI("CO", SpecialProgramList.Codes.JPlus);
			CheckSPI("EC", SpecialProgramList.Codes.JPlus);
			CheckSPI("PE", SpecialProgramList.Codes.JPlus);
			string errorText = new SpecialProgramIndicatorValidation(Factory).GetErrorText("AU", "AU", SpecialProgramList.Codes.JPlus, ZDateTime.Today, null);
			AssertEquals("Andean Trade Promotion and Drug Eradication Act (ATPDEA) is only supported when importing goods of an origin of Colombia, Bolivia, Ecuador or Peru.", errorText);
		}

		public void TestGetErrorTextForSpiSAndSPlus()
		{
			var validation = new SpecialProgramIndicatorValidation(Factory);
			var errorText = "USMCA (Originating) requires that goods are originated from Canada or Mexico and exported from a NAFTA country.";
			var us = Core.Constants.CountryCodes.UnitedStates;
			var ca = Core.Constants.CountryCodes.Canada;
			var mx = Core.Constants.CountryCodes.Mexico;
			var au = Core.Constants.CountryCodes.Australia;
			var xb = CanadaProvinceTerritoryCodes.Codes.XB;
			var xd = CanadaProvinceTerritoryCodes.Codes.XD;
			var s = SpecialProgramList.Codes.S;
			var sPlus = SpecialProgramList.Codes.SPlus;
			var today = ZDateTime.Today;
			AssertEquals(string.Empty, validation.GetErrorText(us, ca, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(ca, us, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(us, us, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(ca, ca, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(ca, mx, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(mx, mx, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(mx, ca, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(xb, ca, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(xb, mx, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(xd, ca, s, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(xd, mx, s, today, null));
			AssertEquals(errorText, validation.GetErrorText(au, ca, s, today, null));
			AssertEquals(errorText, validation.GetErrorText(au, mx, s, today, null));
			AssertEquals(errorText, validation.GetErrorText(au, au, s, today, null));
			AssertEquals(errorText, validation.GetErrorText(ca, au, s, today, null));
			AssertEquals(errorText, validation.GetErrorText(mx, au, s, today, null));
			errorText = "USMCA (Qualifying) requires that goods are originated from Canada or Mexico and exported from a NAFTA country.";
			AssertEquals(string.Empty, validation.GetErrorText(us, ca, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(ca, us, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(us, us, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(ca, ca, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(ca, mx, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(mx, mx, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(mx, ca, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(xb, ca, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(xb, mx, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(xd, ca, sPlus, today, null));
			AssertEquals(string.Empty, validation.GetErrorText(xd, mx, sPlus, today, null));
			AssertEquals(errorText, validation.GetErrorText(au, ca, sPlus, today, null));
			AssertEquals(errorText, validation.GetErrorText(au, mx, sPlus, today, null));
			AssertEquals(errorText, validation.GetErrorText(au, au, sPlus, today, null));
			AssertEquals(errorText, validation.GetErrorText(ca, au, sPlus, today, null));
			AssertEquals(errorText, validation.GetErrorText(mx, au, sPlus, today, null));
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffCode9802004040 = "9802004040";
			var tariffView9802004040 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, tariffCode9802004040, new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, UniversalReferenceConstants.TariffAttributeTypes.Values.USMCA_REPAIR, tariffView9802004040);
			var tariff9802004040 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffCode9802004040)).LastOrDefault();
			if (tariff9802004040 == null)
			{
				tariff9802004040 = Factory.New<USCTariff>();
				tariff9802004040.UE_Tariff = tariffCode9802004040;
			}

			tariff9802004040.UE_SPICode = "B C P S S+AUBHCLCOILJOKRMAOMPAPESG";
			errorText = "USMCA (Originating) requires that goods are exported from a USMCA country.";
			AssertEquals(string.Empty, validation.GetErrorText(au, ca, s, today, tariff9802004040));
			AssertEquals(string.Empty, validation.GetErrorText(au, us, s, today, tariff9802004040));
			AssertEquals(string.Empty, validation.GetErrorText(au, mx, s, today, tariff9802004040));
			AssertEquals(errorText, validation.GetErrorText(us, au, s, today, tariff9802004040));
			errorText = "USMCA (Qualifying) requires that goods are exported from a USMCA country.";
			AssertEquals(string.Empty, validation.GetErrorText(au, ca, sPlus, today, tariff9802004040));
			AssertEquals(string.Empty, validation.GetErrorText(au, us, sPlus, today, tariff9802004040));
			AssertEquals(string.Empty, validation.GetErrorText(au, mx, sPlus, today, tariff9802004040));
			AssertEquals(errorText, validation.GetErrorText(us, au, sPlus, today, tariff9802004040));
		}

		void CheckSPI(ZString acceptableCountry, ZString spiCode)
		{
			Assert(new SpecialProgramIndicatorValidation(Factory).GetErrorText("BR", "BR", spiCode, ZDateTime.Today, null).Length > 0);
			Assert(new SpecialProgramIndicatorValidation(Factory).GetErrorText(acceptableCountry, acceptableCountry, spiCode, ZDateTime.Today, null).Length == 0);
		}
	}
}
