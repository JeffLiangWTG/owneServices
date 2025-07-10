using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class AWBAddressFormatter
	{
		public AWBAddressFormatter(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		readonly BusinessObjectFactory factory;

		public ZString IssuingAgentName { get; private set; }
		public ZString IssuingAgentAddress1 { get; private set; }
		public ZString IssuingAgentAddress2 { get; private set; }

		ZString address1;
		ZString address2;
		ZString city;
		ZString state;
		ZString postCode;
		ZString countryName;

		const string commaAndSpace = ", ";

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Format(BillIssuedBy issuedBy, bool transformState = false)
		{
			Argument.NotNull(issuedBy, "issuedBy");

			SetupAddress(issuedBy, transformState);

			IssuingAgentName = issuedBy.Name.ToUpper();

			var country = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Desc, countryName));

			var formattingRule = country != null
				? country.RN_AddressFormattingRule
				: ZString.Empty;

			switch (formattingRule)
			{
				case CountryAddressFormattingRuleList.Codes.PostcodeBeforeCityStateInBrackets:
					FormatPostCodeBeforeCityWithStateInBackets();
					break;

				case CountryAddressFormattingRuleList.Codes.NoStateCityInCapitalsPostcodeAtEnd:
				case CountryAddressFormattingRuleList.Codes.AddressCityInCapitalsAndPostCodeThenCountry:
				case CountryAddressFormattingRuleList.Codes.CityPostcodeCountry:
					FormatNoStateCityPostcodeAtEnd();
					break;

				case CountryAddressFormattingRuleList.Codes.CityFirstThenAddressPostCodeLast:
					FormatCityFirstThenAddressPostCodeLast();
					break;

				case CountryAddressFormattingRuleList.Codes.AddressSubdivisionAndCountry:
					FormatAddressSubdivisionAndCountry();
					break;

				case CountryAddressFormattingRuleList.Codes.Singapore: // special case where we modif the coutry name from SINGAPORE to "REP. OF SINGAPORE"
					FormatSingapore();
					break;

				case CountryAddressFormattingRuleList.Codes.AddressSuburbInCaptialsOptionalCityPostcode:
					FormatAddressOptionalCityPostcodeAtEnd();
					break;

				case CountryAddressFormattingRuleList.Codes.AddressPostCodeAndCityInCapitalsThenCountry:
				case CountryAddressFormattingRuleList.Codes.AddressThenSecondAddressAndCityInCapitalsFinallyPostCodeAndCountryTogether:
					FormatAddressNoStatePostCodeAndCityAtEnd();
					break;

				case CountryAddressFormattingRuleList.Codes.PostcodeCityStateCountry:
				case CountryAddressFormattingRuleList.Codes.PostcodeCityCommaState:
				case CountryAddressFormattingRuleList.Codes.PostcodeBeforeCityAndState:
					FormatPostcodeCityStateCountry();
					break;

				case CountryAddressFormattingRuleList.Codes.CityCountryNoPostcodeNoState:
					FormatAddressCityCountryNoPostcodeNoState();
					break;

				default:
					FormatCityStatePostcodeCountry();
					break;
			}
		}

		void SetupAddress(BillIssuedBy issuedBy, bool transformState = false)
		{
			address1 = issuedBy.Address1.ToUpper();
			address2 = issuedBy.Address2.ToUpper();
			city = issuedBy.City.ToUpper();

			state = transformState
				? issuedBy.State.TransformState(factory, issuedBy.CountryCode)
				: issuedBy.State;
			state = state.ToUpper();

			postCode = issuedBy.PostCode.ToUpper();
			countryName = issuedBy.CountryName.ToUpper();
		}

		void FormatPostCodeBeforeCityWithStateInBackets()
		{
			IssuingAgentAddress1 = address1;

			var stateWithBrackets = (state.Length > 0) ? "(" + state + ")" : string.Empty;
			IssuingAgentAddress2 = FormatAddress(address2, postCode, city, stateWithBrackets, countryName);
		}

		void FormatNoStateCityPostcodeAtEnd()
		{
			IssuingAgentAddress1 = address1;
			IssuingAgentAddress2 = FormatAddress(address2, city, postCode, countryName);
		}

		void FormatCityFirstThenAddressPostCodeLast()
		{
			IssuingAgentAddress1 = FormatAddress(city, address1);
			IssuingAgentAddress2 = FormatAddress(address2, postCode, countryName);
		}

		void FormatAddressSubdivisionAndCountry()
		{
			IssuingAgentAddress1 = address1;
			IssuingAgentAddress2 = FormatAddress(address2, state, countryName);
		}

		void FormatSingapore()
		{
			var formattedCountryName = this.countryName == "SINGAPORE" ? (NoResString)"REP. OF SINGAPORE" : ""; // Singapore Official Country Name

			IssuingAgentAddress1 = address1;
			IssuingAgentAddress2 = FormatAddress(address2, city, postCode, formattedCountryName);
		}

		void FormatAddressOptionalCityPostcodeAtEnd()
		{
			IssuingAgentAddress1 = address1;
			IssuingAgentAddress2 = FormatAddress(address2, postCode, countryName);
		}

		void FormatAddressNoStatePostCodeAndCityAtEnd()
		{
			IssuingAgentAddress1 = address1;
			IssuingAgentAddress2 = FormatAddress(address2, postCode, city, countryName);
		}

		void FormatPostcodeCityStateCountry()
		{
			IssuingAgentAddress1 = address1;
			IssuingAgentAddress2 = FormatAddress(address2, postCode, city, state, countryName);
		}

		void FormatCityStatePostcodeCountry()
		{
			IssuingAgentAddress1 = address1;
			IssuingAgentAddress2 = FormatAddress(address2, city, state, postCode, countryName);
		}

		void FormatAddressCityCountryNoPostcodeNoState()
		{
			IssuingAgentAddress1 = address1;
			IssuingAgentAddress2 = FormatAddress(address2, city, countryName);
		}

		string FormatAddress(params string[] addressElements)
		{
			return string.Join(commaAndSpace, addressElements.Where(elem => !string.IsNullOrWhiteSpace(elem)));
		}
	}
}
