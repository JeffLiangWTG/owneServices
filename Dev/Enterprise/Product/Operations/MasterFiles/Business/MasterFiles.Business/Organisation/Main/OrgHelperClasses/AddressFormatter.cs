using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AddressFormatter
	{
		public AddressFormatter(BusinessObjectFactory factory, OrgHeader recipientOrganisation, GlbCompany senderCompany, bool includeCountryEvenIfSame)
		{
			Argument.NotNull(factory, "factory");
			this.Factory = factory;

			if (recipientOrganisation == null || !recipientOrganisation.IsDeleted || !recipientOrganisation.IsDeleting || !recipientOrganisation.IsBeingDeleted)
			{
				this.RecipientOrganisation = recipientOrganisation;
			}
			this.SenderCompany = senderCompany;
			this.IncludeCountryEvenIfSame = includeCountryEvenIfSame;
			this.Language = ZString.Empty;
		}

		public AddressFormatter(BusinessObjectFactory factory, JobDocAddress docAddress, GlbCompany senderCompany, bool includeCountryEvenIfSame)
			: this(factory, (OrgHeader)null, senderCompany, includeCountryEvenIfSame)
		{
			SetupAddressFrom(docAddress);
			RetrieveCompanyDetails();
			RequireExtractFromOrgDetails = false;
		}

		public AddressFormatter(BusinessObjectFactory factory, OrgAddress address, GlbCompany senderCompany, bool includeCountryEvenIfSame)
			: this(factory, (OrgHeader)null, senderCompany, includeCountryEvenIfSame)
		{
			SetupAddressFrom(address);
			RetrieveCompanyDetails();
			RequireExtractFromOrgDetails = false;
		}

		public AddressFormatter(BusinessObjectFactory factory, OrgAddress address, GlbCompany senderCompany, bool includeCountryEvenIfSame, bool excludeAdditionalAddressInfo)
			: this(factory, (OrgHeader)null, senderCompany, includeCountryEvenIfSame)
		{
			SetupAddressFrom(address, excludeAdditionalAddressInfo);
			RetrieveCompanyDetails();
			RequireExtractFromOrgDetails = false;
		}

		public AddressFormatter(BusinessObjectFactory factory, OrgAddress address)
		: this(factory, address, true)
		{
		}

		public AddressFormatter(BusinessObjectFactory factory, OrgAddress address, bool shouldSetupCountryFromRelatedCountry)
			: this(factory, (OrgHeader)null, null, false)
		{
			RequireExtractFromOrgDetails = false;
			ShouldSetupCountryFromRelatedCountry = shouldSetupCountryFromRelatedCountry;
			SetupAddressFrom(address);
		}

		public AddressFormatter(BusinessObjectFactory factory, string name, string address1, string address2, string city, string state, string postCode, MultilingualString countryName, bool includeCountryEvenIfSame)
			: this(factory, name, string.Empty, address1, address2, city, state, postCode, countryName, Constants.Languages.English, includeCountryEvenIfSame)
		{
		}

		public AddressFormatter(BusinessObjectFactory factory, string name, string additionalAddressInformation, string address1, string address2, string city, string state, string postCode, MultilingualString countryName, bool includeCountryEvenIfSame)
			: this(factory, name, additionalAddressInformation, address1, address2, city, state, postCode, countryName, Constants.Languages.English, includeCountryEvenIfSame)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AddressFormatter(BusinessObjectFactory factory, string name, string additionalAddressInformation, string address1, string address2, string city, string state, string postCode, MultilingualString countryName, string language, bool includeCountryEvenIfSame, string countryCode = "")
		{
			Argument.NotNull(factory, "factory");
			this.Factory = factory;

			SetupAddressFrom(name, additionalAddressInformation, address1, address2, city, state, postCode, countryName, language);
			this.IncludeCountryEvenIfSame = includeCountryEvenIfSame;

			RequireExtractFromOrgDetails = false;
			if (!countryCode.IsNullOrEmpty())
			{
				this.CountryCode = countryCode;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AddressFormatter(BusinessObjectFactory factory, ISupportWebAddressValidation address, MultilingualString countryName, string countryCode, bool includeCountryEvenIfSame) : this(factory, address, countryName, countryCode, includeCountryEvenIfSame, null)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AddressFormatter(BusinessObjectFactory factory, ISupportWebAddressValidation address, MultilingualString countryName, string countryCode, bool includeCountryEvenIfSame, string overrideAdditionalInfo)
		{
			Argument.NotNull(factory, "factory");
			this.Factory = factory;

			SetupAddressFrom(address.CompanyName, overrideAdditionalInfo ?? string.Empty, address.Address1, address.Address2, address.City, address.StateCode, address.Postcode, countryName, address.Language);
			this.IncludeCountryEvenIfSame = includeCountryEvenIfSame;
			this.CountryCode = countryCode;

			RequireExtractFromOrgDetails = false;
		}

		public string CompanyName()
		{
			ExtractAndBuildAddress();
			return RemoveMultipleSpacesAndTrim(Name);
		}

		public string PostalAddress()
		{
			ExtractAndBuildAddress();
			return FormattedAddress;
		}

		public string PostalAddressAsASingleLine()
		{
			string result = PostalAddress();
			return result.Replace(newLine, space);
		}

		public string PostalAddressWithoutCompanyName()
		{
			var addressOnlyTemp = this.addressOnly;
			try
			{
				this.addressOnly = true;
				ExtractAndBuildAddress();
			}
			finally
			{
				this.addressOnly = addressOnlyTemp;
			}

			return FormattedAddress;
		}

		public string PostalAddressAsASingleLineWithoutCompanyName()
		{
			string result = PostalAddressWithoutCompanyName();
			return result.Replace(newLine, space);
		}

		public ZString Language
		{
			get
			{
				if (language.IsEmpty)
				{
					if (RecipientOrganisation != null)
					{
						language = RecipientOrganisation.MainAddress.OA_Language;
					}
					else
					{
						language = Constants.Languages.English;
					}
				}
				return language;
			}
			protected set { language = value; }
		}
		ZString language;

		protected virtual void RetrieveOrganisationDetails()
		{
			if (RecipientOrganisation != null)
			{
				Name = RecipientOrganisation.OH_FullName;
				AdditionalAddressInformation = RecipientOrganisation.MainAddress.PrimaryOrgAddressAdditionalInfoDetail;
				Address1 = RecipientOrganisation.MainAddress.OA_Address1;
				Address2 = RecipientOrganisation.MainAddress.OA_Address2;
				City = RecipientOrganisation.MainAddress.OA_City;
				State = RecipientOrganisation.MainAddress.OA_State;
				PostCode = RecipientOrganisation.MainAddress.OA_PostCode;
				if (RecipientOrganisation.UNLOCO != null && RecipientOrganisation.UNLOCO.Country != null)
				{
					CountryName = RecipientOrganisation.UNLOCO.Country.RN_DescMultilingual;
					CountryCode = RecipientOrganisation.UNLOCO.Country.RN_Code;
				}
				else
				{
					CountryCode = "";
					CountryName = (NoResString)"";
				}
				Language = RecipientOrganisation.MainAddress.OA_Language;
			}
			else
			{
				Name = AdditionalAddressInformation = Address1 = Address2 = City = State = StateName = PostCode = CountryCode = string.Empty;
				CountryName = (NoResString)"";
				Language = string.Empty;
			}
		}

		protected virtual void RetrieveCompanyDetails()
		{
			SenderCountry = (SenderCompany != null && SenderCompany.Country != null) ? SenderCompany.Country.RN_DescMultilingual : ZString.Empty;
		}

		void ExtractAndBuildAddress()
		{
			if (RequireExtractFromOrgDetails)
			{
				RetrieveOrganisationDetails();
				RetrieveCompanyDetails();
			}

			BuildAddress();
		}

		protected virtual void BuildAddress()
		{
			FormattedAddress = RemoveMultipleSpacesAndTrim(Format());

			if (!Env.Registry.OrgAllowMixedCase)
			{
				FormattedAddress = FormattedAddress.ToUpper();
			}
		}

		protected string RemoveMultipleSpacesAndTrim(string value)
		{
			if (value != null)
			{
				value = Regex.Replace(value, @"(\s)\1+", "$1"); // Multiple consecutive whitespace characters
				value = Regex.Replace(value, @"\s+$", "", RegexOptions.Multiline); // Trailing whitespace
				value = Regex.Replace(value, @"^\s+", "", RegexOptions.Multiline); // Leading whitespace
			}
			return value;
		}

		protected string Format()
		{
			SetStateNameIfNotSet();

			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCode);
			string result = "";

			if (country != null)
			{
				// More specific rules are available in the Universal Postal Union's addressing guidelines, downloadable from
				// http://www.upu.int/post_code/en/countries/
				switch (country.RN_AddressFormattingRule)
				{
					case CountryAddressFormattingRuleList.Codes.PostcodeBeforeCityAndState:
						result = FormatPostCodeCityStateAllOnOneLine();
						break;

					case CountryAddressFormattingRuleList.Codes.PostcodeBeforeCityStateInBrackets:
						result = FormatPostCodeBeforeCityWithStateInBackets();
						break;

					case CountryAddressFormattingRuleList.Codes.NoStateCityInCapitalsPostcodeAtEnd:
						result = FormatNoStateCityInCapitalsPostcodeAtEnd();
						break;

					case CountryAddressFormattingRuleList.Codes.CityFirstThenAddressPostCodeLast:
						result = FormatCityFirstThenAddressPostCodeLast();
						break;

					case CountryAddressFormattingRuleList.Codes.AddressThenSecondAddressAndCityInCapitalsFinallyPostCodeAndCountryTogether:
						result = FormatAddressThenSecondAddressAndCityInCapitalsFinallyPostCodeAndCountryTogether();
						break;

					case CountryAddressFormattingRuleList.Codes.AddressCityInCapitalsAndPostCodeThenCountry:
						result = FormatAddressCityInCapitalsAndPostCodeThenCountry();
						break;

					case CountryAddressFormattingRuleList.Codes.AddressSubdivisionAndCountry:
						result = FormatAddressSubdivisionAndCountry();
						break;

					case CountryAddressFormattingRuleList.Codes.PostcodeCityCommaState:
						result = FormatPostcodeCityCommaState();
						break;

					case CountryAddressFormattingRuleList.Codes.Singapore:  // special case where we modif the coutry name from SINGAPORE to "REP. OF SINGAPORE"
						result = FormatSingapore();
						break;

					case CountryAddressFormattingRuleList.Codes.AddressSuburbInCaptialsOptionalCityPostcode:
						result = FormatAddressSuburbInCaptialsOptionalCityPostcode();
						break;

					case CountryAddressFormattingRuleList.Codes.AddressPostCodeAndCityInCapitalsThenCountry:
						result = FormatAddressPostCodeAndCityInCapitalsThenCountry();
						break;

					case CountryAddressFormattingRuleList.Codes.PostcodeCityStateCountry:
						result = FormatPostcodeCityStateCountry();
						break;

					case CountryAddressFormattingRuleList.Codes.CityPostcodeCountry:
						result = FormatCityPostcodeCountry();
						break;

					case CountryAddressFormattingRuleList.Codes.CityStatePostcodeCountry:
						result = FormatCityStatePostcodeCountry(true);
						break;

					case CountryAddressFormattingRuleList.Codes.CityStatePostcodeNoCountry:
						result = FormatCityStatePostcodeCountry(false);
						break;

					case CountryAddressFormattingRuleList.Codes.CityStateAsOneLinePostcodeCountry:
						result = FormatCityStateAsOneLinePostcodeCountry();
						break;

					case CountryAddressFormattingRuleList.Codes.CityCountryNoPostcodeNoState:
						result = FormatAddressCityCountryNoPostcodeNoState();
						break;

					case CountryAddressFormattingRuleList.Codes.PostcodeStateCityAddressCountry:
						result = FormatPostcodeStateCityAddressCountry();
						break;

					default:
						result = FormatCityStatePostcodeAllOnOneLine();
						break;
				}
			}
			else
			{
				result = FormatCityStatePostcodeAllOnOneLine();
			}

			return result;
		}

		protected string FormatPostcodeCityStateCountry()
		{
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ PostCode + space + City + newLine
				+ (!string.IsNullOrEmpty(StateName) ? StateName : State) + newLine
				+ DisplayCountryName;
		}

		protected string FormatCityPostcodeCountry()
		{
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ City + newLine
				+ PostCode + newLine
				+ DisplayCountryName;
		}

		protected string FormatCityStatePostcodeCountry(bool includeCountry)
		{
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			string address = NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ City + newLine
				+ (!string.IsNullOrEmpty(StateName) ? StateName : State) + newLine
				+ PostCode;

			return includeCountry ? address + newLine + DisplayCountryName : address;
		}

		protected string FormatCityStateAsOneLinePostcodeCountry()
		{
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ City + commaAndSpace + (!string.IsNullOrEmpty(StateName) ? StateName : State) + newLine
				+ PostCode + newLine
				+ DisplayCountryName;
		}

		protected string FormatAddressSuburbInCaptialsOptionalCityPostcode()
		{
			// e.g. South Africa
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2.ToUpper() + newLine
				+ (string.IsNullOrEmpty(City) ? "" : City + newLine)        // city is oiptional
				+ PostCode + newLine +
				DisplayCountryName;
		}

		protected string FormatSingapore()
		{
			// Singapore only
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ City.ToUpper() + space + PostCode + newLine
				+ (DisplayCountryName.ToUpper() == "SINGAPORE" ? (NoResString)"REP. OF SINGAPORE" : "");
		}

		protected string FormatPostcodeCityCommaState()
		{
			// e.g. Malaysia
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ PostCode + space + City.ToUpper() + (string.IsNullOrEmpty(State) ? "" : commaAndSpace + State.ToUpper()) + newLine
				+ DisplayCountryName;
		}

		protected string FormatAddressSubdivisionAndCountry()
		{
			// e.g. Gambia
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ (string.IsNullOrEmpty(State) ? "" : State + newLine)
				+ DisplayCountryName;
		}

		protected string FormatAddressCityCountryNoPostcodeNoState()
		{
			// e.g. Hong Kong
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ City + newLine
				+ DisplayCountryName;
		}

		protected string FormatAddressPostCodeAndCityInCapitalsThenCountry()
		{
			// e.g. Germany, Island
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ PostCode + space + City.ToUpper() + newLine
				+ DisplayCountryName;
		}

		protected string FormatAddressCityInCapitalsAndPostCodeThenCountry()
		{
			// e.g. New Zealand
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ City.ToUpper() + space + PostCode + newLine
				+ DisplayCountryName;
		}

		protected string FormatAddressThenSecondAddressAndCityInCapitalsFinallyPostCodeAndCountryTogether()
		{
			// e.g. Japan
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ (!string.IsNullOrEmpty(Address2) ? (Address2 + commaAndSpace) : "") + City.ToUpper() + newLine
				+ PostCode + space + DisplayCountryName;
		}

		protected string FormatCityFirstThenAddressPostCodeLast()
		{   // e.g. Hungary
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ City + newLine
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ PostCode + newLine
				+ DisplayCountryName;
		}

		protected string FormatNoStateCityInCapitalsPostcodeAtEnd()
		{   // e.g. United Kingdom
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ City.ToUpper() + newLine
				+ PostCode + newLine
				+ DisplayCountryName;
		}

		protected string FormatPostCodeCityStateAllOnOneLine()
		{
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ PostCode + space + City + space + State + newLine
				+ DisplayCountryName;
		}

		protected string FormatPostCodeBeforeCityWithStateInBackets()
		{
			string displayState = "(" + State.Trim() + ")";
			if (displayState == "()")
			{
				displayState = "";
			}

			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ PostCode + space + City + space + displayState + newLine
				+ DisplayCountryName;
		}

		protected string FormatCityStatePostcodeAllOnOneLine()
		{   // e.g. USA, Canada, Australia
			var additionalInfo = string.Empty;
			if (!string.IsNullOrEmpty(AdditionalAddressInformation))
			{
				additionalInfo = AdditionalAddressInformation + newLine;
			}

			return NameFormatted
				+ additionalInfo
				+ Address1 + newLine
				+ Address2 + newLine
				+ City + space + State + space + PostCode + newLine
				+ DisplayCountryName;
		}

		protected string FormatPostcodeStateCityAddressCountry()
		{
			return NameFormatted
				+ PostCode + newLine
				+ (!string.IsNullOrEmpty(StateName) ? StateName : State) + space + City + space + Address1 + space + Address2 + newLine
				+ AdditionalAddressInformation + newLine
				+ DisplayCountryName;
		}

		protected string DisplayCountryName
		{
			get
			{
				if (CountryName != SenderCountry || IncludeCountryEvenIfSame)
				{
					using (language.IsEmpty ? null : Res.TemporarilySwitchLanguage(language))
					{
						return CountryName;
					}
				}
				else
				{
					return "";
				}
			}
		}

		void SetupAddressFrom(JobDocAddress docAddress)
		{
			if (docAddress != null)
			{
				OrgAddress address = docAddress.Address;
				if (docAddress.E2_AddressOverride || address == null)
				{
					SetupAddressFrom(docAddress.E2_CompanyName, string.Empty, docAddress.E2_Address1, docAddress.E2_Address2, docAddress.E2_City, docAddress.E2_State, docAddress.E2_Postcode, (NoResString)"", "");
					SetupCountryFrom(docAddress.Country);
				}
				else
				{
					SetupAddressFrom(address);
				}
			}
		}

		void SetupAddressFrom(OrgAddress address, bool excludeAdditionalAddressInfo = false)
		{
			if (address != null)
			{
				var additionalAddressInfo = excludeAdditionalAddressInfo ? ZString.Empty : address.PrimaryOrgAddressAdditionalInfoDetail;
				SetupAddressFrom(address.EffectiveCompanyNameTruncated, additionalAddressInfo, address.OA_Address1, address.OA_Address2, address.OA_City, address.OA_State, address.OA_PostCode, (NoResString)"", address.OA_Language);
				var relatedPort = address.EffectiveRelatedPortCode;
				if (ShouldSetupCountryFromRelatedCountry && address.RelatedCountry != null)
				{
					SetupCountryFrom(address.RelatedCountry);
				}
				else if (relatedPort != null)
				{
					SetupCountryFrom(relatedPort.Country);
				}
			}
		}

		void SetupAddressFrom(string companyName, string additionalAddressInfo, string address1, string address2, string city, string state, string postCode, MultilingualString countryName, string lang)
		{
			this.Name = companyName;
			this.AdditionalAddressInformation = additionalAddressInfo;
			this.Address1 = address1;
			this.Address2 = address2;
			this.City = city;
			this.State = state;
			this.PostCode = postCode;
			this.CountryName = countryName;
			this.Language = lang;

			SetStateNameIfNotSet();
		}

		void SetStateNameIfNotSet()
		{
			if (string.IsNullOrEmpty(StateName) && !string.IsNullOrEmpty(State) && (!(string.IsNullOrEmpty(CountryCode)) || !(string.IsNullOrEmpty(CountryName))))
			{
				ZQuery stateQuery;
				if (!string.IsNullOrEmpty(CountryCode))
				{
					stateQuery = new ZQuery();
					stateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, CountryCode);
				}
				else
				{
					stateQuery = new ZDBOnlyQuery(typeof(RefCountryStates));
					ZDBOnlySubQuery countrySubQuery = new ZDBOnlySubQuery(typeof(RefCountry), RefCountryStatesSchema.RW_RN_NKCountryCode, RefCountrySchema.RN_Code);
					countrySubQuery.AddToFilter(RefCountrySchema.RN_Desc, CountryName.GetUnresolvedString());
					(stateQuery as ZDBOnlyQuery).AddSubQuery(countrySubQuery, JoinCondition.And);
				}
				stateQuery.AddToFilter(RefCountryStatesSchema.RW_Code, State);
				RefCountryStates state = Factory.LoadTop1<RefCountryStates>(stateQuery);
				if (state != null)
				{
					StateName = state.RW_DescriptionMultilingual;
				}
			}
		}

		void SetupCountryFrom(RefCountry country)
		{
			if (country != null)
			{
				CountryCode = country.Code;
				CountryName = country.RN_DescMultilingual;
				SetStateNameIfNotSet();
			}
		}

		string NameFormatted
		{
			get { return (!string.IsNullOrEmpty(Name) && !addressOnly ? Name + newLine : ""); }
		}

		bool addressOnly;

		readonly protected BusinessObjectFactory Factory;

		protected OrgHeader recipientOrganisation;
		public OrgHeader RecipientOrganisation
		{
			get
			{
				if (recipientOrganisation != null &&
					(!recipientOrganisation.IsDeleted ||
					!recipientOrganisation.IsDeleting ||
					!recipientOrganisation.IsBeingDeleted))
				{
					return recipientOrganisation;
				}
				else
				{
					return null;
				}
			}
			set
			{
				recipientOrganisation = value;
			}
		}

		readonly protected GlbCompany SenderCompany;
		protected string Name
		{
			get { return name; }
			set
			{
				name = Env.Registry.OrgAllowMixedCase ? value : value?.ToUpperInvariant();
			}
		}
		string name;
		protected string AdditionalAddressInformation;
		protected string Address1;
		protected string Address2;
		protected string City;
		protected string State;
		protected string StateName;
		protected string PostCode;

		protected MultilingualString CountryName;
		protected bool ShouldSetupCountryFromRelatedCountry;
		protected string CountryCode;
		protected string SenderCountry;
		protected string FormattedAddress;
		readonly bool IncludeCountryEvenIfSame;
		readonly bool RequireExtractFromOrgDetails = true;
		protected const string commaAndSpace = ", ";
		protected const string newLine = "\n";
		protected const string space = " ";
	}
}
