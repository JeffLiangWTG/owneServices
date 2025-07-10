using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class AWBAddressFormatterTest : TestCaseWithFactory
	{
		public void TestFormatNoStateCityPostcodeAtEnd()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.NoStateCityInCapitalsPostcodeAtEnd;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street", "Station Approach", "Totternhoe", "Beds", "2010", "United Kingdom"));
			Factory.Save();
			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("STATION APPROACH, TOTTERNHOE, 2010, UNITED KINGDOM", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatCityStatePostcodeCountry()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityStatePostcodeAllOnOneLine;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street Street", "Gardeners Road Kings Road Road Road", "Sydney", "New South Wales", "2010", "Australia"));
			Factory.Save();
			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS ROAD ROAD ROAD, SYDNEY, NEW SOUTH WALES, 2010, AUSTRALIA", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatPostcodeCityStateCountry()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Germany));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.PostcodeCityStateCountry;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			Factory.Save();

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street Street", "Gardeners Road Kings", "Berlin", "AnyState", "2010", "Germany"));
			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS, 2010, BERLIN, ANYSTATE, GERMANY", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatPostcodeCityStateCountry_TransformJPState()
		{
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(
				new ZQuery(RefCountryStatesSchema.RW_Code, "13"), JoinCondition.And,
				new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, Constants.CountryCodes.Japan)));
			state.Country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.PostcodeCityStateCountry;

			var translateJapanese = Factory.New<RefLanguageText>();
			translateJapanese.RLT_ColumnName = "RW_Description";
			translateJapanese.RLT_Language = SharedConstants.Languages.Japanese;
			translateJapanese.RLT_ParentId = state.PK;
			translateJapanese.RLT_ParentTableCode = "RW";
			translateJapanese.RLT_Text = "東京都";

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			Factory.Save();

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street", "Gardeners Road Kings", "Tokyo City", "13", "100-0012", "Japan"));
				var addressFormatter = new AWBAddressFormatter(Factory);
				addressFormatter.Format(issuedBy, transformState: true);

				AssertEquals("GARDENERS ROAD KINGS, 100-0012, TOKYO CITY, TOKYO, JAPAN", addressFormatter.IssuingAgentAddress2);
			}
		}

		public void TestFormatAddressCityCountryNoPostcodeNoState()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.HongKong));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityCountryNoPostcodeNoState;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			Factory.Save();

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street Street", "Gardeners Road", "Hong Kong City", "N/A", "N/A", "Hong Kong"));
			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD, HONG KONG CITY, HONG KONG", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatCityFirstThenAddressPostCodeLast()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Hungary));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityFirstThenAddressPostCodeLast;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street Street", "Gardeners Road Kings Road", "Valami", "N/A", "2010", "Hungary"));
			Factory.Save();
			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("VALAMI, 82 FAKE STREET STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS ROAD, 2010, HUNGARY", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatAddressNoStatePostCodeAndCityAtEnd()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Japan));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressThenSecondAddressAndCityInCapitalsFinallyPostCodeAndCountryTogether;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street Street", "Gardeners Road Kings Road", "Tokyo", "N/A", "2010", "Japan"));
			Factory.Save();

			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS ROAD, 2010, TOKYO, JAPAN", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatAddressNoStatePostCodeAndCityAtEndWithAdreessMerging()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Malaysia));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.PostcodeCityCommaState;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street Street", "Gardeners Road Kings Road", "Kuala Lumpur", "Panang", "2010", "Malaysia"));
			Factory.Save();
			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS ROAD, 2010, KUALA LUMPUR, PANANG, MALAYSIA", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatPostCodeBeforeCityWithStateInBackets()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.India));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.PostcodeBeforeCityStateInBrackets;
			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street", "Gardeners Road Kings", "Chandigarh", "Punjab", "160098", "India"));
			Factory.Save();

			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS, 160098, CHANDIGARH, (PUNJAB), INDIA", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatAddressSubdivisionAndCountry()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Poland));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressSubdivisionAndCountry;
			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street", "Gardeners Road Kings", "Warszawa", "Chodecz", "160098", "Poland"));
			Factory.Save();

			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS, CHODECZ, POLAND", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatSingapore()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Singapore));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.Singapore;
			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street", "Gardeners Road Kings", "Mall square", "Singapore", "160098", "Singapore"));
			Factory.Save();

			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS, MALL SQUARE, 160098, REP. OF SINGAPORE", addressFormatter.IssuingAgentAddress2);
		}

		public void TestFormatAddressOptionalCityPostcodeAtEnd()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.SouthAfrica));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressSuburbInCaptialsOptionalCityPostcode;
			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("Wiggles", "82 Fake Street Street Street Street", "Gardeners Road Kings Road", "Smallville", "Joburg", "160098", "South Africa"));
			Factory.Save();

			var addressFormatter = new AWBAddressFormatter(Factory);
			addressFormatter.Format(issuedBy);

			AssertEquals("WIGGLES", addressFormatter.IssuingAgentName);
			AssertEquals("82 FAKE STREET STREET STREET STREET", addressFormatter.IssuingAgentAddress1);
			AssertEquals("GARDENERS ROAD KINGS ROAD, 160098, SOUTH AFRICA", addressFormatter.IssuingAgentAddress2);
		}

		#region Implementation

		RefAirline GetBillIssuedByAirLine(ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString country)
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "NHK";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_RM_Airline = airline.PK;

			airline.RM_AirlineName1 = name;
			airline.RM_AddressLine1 = address1;
			airline.RM_AddressLine2 = address2;
			airline.RM_AirlineCity = city;
			airline.RM_AirlineState = state;
			airline.RM_AirlinePostalCode = postCode;
			airline.RM_AirlineCountry = country;

			return airline;
		}

		#endregion
	}
}
