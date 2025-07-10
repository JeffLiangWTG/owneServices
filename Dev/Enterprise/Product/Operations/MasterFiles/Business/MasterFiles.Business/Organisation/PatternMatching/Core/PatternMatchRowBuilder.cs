using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	class PatternMatchRowBuilder
	{
		internal PatternMatchRowBuilder(IOrgPatternMatch patternMatchRow)
		{
			this.patternMatchRow = Argument.NotNull(patternMatchRow, "IOrgPatternMatch patternMatchRow");
		}

		readonly IOrgPatternMatch patternMatchRow;

		internal void EncodeOrganisation(IMatchingOrganisation org, StringWithLanguage companyName, IMatchingAddress address, string localBusinessNumber)
		{
			string portName = address.PortName;
			var countryName = address.CountryName;

			var companyPatternMatchGenerationHelper = OrgPatternMatchGenerationHelper.Get(companyName.LanguageCode, portName, countryName);

			EncodeName(companyName, companyPatternMatchGenerationHelper, true);
			SetIsCorporation(companyName, companyPatternMatchGenerationHelper);

			var addressPatternMatchGenerationHelper = OrgPatternMatchGenerationHelper.Get(address.OA_Language, portName, countryName);

			SetPOBoxDetails(address, addressPatternMatchGenerationHelper);
			SetAddress(address, addressPatternMatchGenerationHelper);
			SetPort(org, address, addressPatternMatchGenerationHelper);
			SetBusinessRegNo(localBusinessNumber);
			SetPostCode(address, addressPatternMatchGenerationHelper);
			SetCity(address, addressPatternMatchGenerationHelper);
			SetState(address, addressPatternMatchGenerationHelper);
			SetPhoneAndFaxNumbers(address);
			SetEmailAndDomain(address, addressPatternMatchGenerationHelper);

			if (patternMatchRow.OS_OH != org.PK)
			{
				patternMatchRow.OS_OH = org.PK;
			}
			if (patternMatchRow.OS_OA != address.PK)
			{
				patternMatchRow.OS_OA = address.PK;
			}
		}

		/// <summary>
		/// Used for Org Name Matching
		/// </summary>
		/// <param name="hyphensSplitWords">Indicates whether words containing hyphens will be split and treated as two words,
		/// or have the hyphen removed and be collapsed to one word.</param>
		internal void EncodeName(StringWithLanguage companyName, OrgPatternMatchGenerationHelper patternMatchGenerationHelper, bool hyphensSplitWords)
		{
			GetCompanyName(companyName, patternMatchGenerationHelper, hyphensSplitWords);
		}

		void SetBusinessRegNo(string localBusinessNumber)
		{
			patternMatchRow.OS_BusinessRegNo = String.IsNullOrEmpty(localBusinessNumber) ? String.Empty : OrgPatternMatchGenerationHelper.SanitisedBusinessRegistrationNumber(localBusinessNumber, OrgPatternMatchSchema.OS_BusinessRegNo.MaxLength);
		}

		void SetPostCode(IMatchingAddress address, OrgPatternMatchGenerationHelper patternMatchGenerationHelper)
		{
			patternMatchRow.OS_PostCode = address.OA_PostCode.IsEmpty ? String.Empty : patternMatchGenerationHelper.PostCode(address.OA_PostCode, OrgPatternMatchSchema.OS_PostCode.MaxLength);
		}

		void SetCity(IMatchingAddress address, OrgPatternMatchGenerationHelper patternMatchGenerationHelper)
		{
			if (address.OA_City.IsEmpty)
			{
				patternMatchRow.OS_City = String.Empty;
			}
			else
			{
				ZString soundexResult = patternMatchGenerationHelper.CitySoundex(address.OA_City);
				if (soundexResult.Length > OrgPatternMatchSchema.OS_City.MaxLength)
				{
					ErrorReporter.ReportOnce("Core-MF-Soundex-City",
							string.Format("Soundex result was too long to be put into OS_City. MaxLength is {0} but result was {1}. Original property value from which soundex came was: {2} in language {3}. Need a new soundex for this language? Bug in this language's soundex?",
							OrgPatternMatchSchema.OS_City.MaxLength, soundexResult, address.OA_City, address.OA_Language));
					patternMatchRow.OS_City = soundexResult.Left(OrgPatternMatch.Schema.OS_CityMaxLength);
				}
				else
				{
					patternMatchRow.OS_City = soundexResult;
				}
			}
		}

		void SetState(IMatchingAddress address, OrgPatternMatchGenerationHelper patternMatchGenerationHelper)
		{
			if (address.OA_State.IsEmpty)
			{
				patternMatchRow.OS_State = String.Empty;
			}
			else
			{
				ZString soundexResult = patternMatchGenerationHelper.StateSoundex(address.OA_State);
				if (soundexResult.Length > OrgPatternMatchSchema.OS_State.MaxLength)
				{
					ErrorReporter.ReportOnce("Core-MF-Soundex-State",
							string.Format("Soundex result was too long to be put into OS_State. MaxLength is {0} but result was {1}. Original property value from which soundex came was: {2} in language {3}. Need a new soundex for this language? Bug in this language's soundex?",
							OrgPatternMatchSchema.OS_State.MaxLength, soundexResult, address.OA_State, address.OA_Language));
					patternMatchRow.OS_State = soundexResult.Left(OrgPatternMatch.Schema.OS_StateMaxLength);
				}
				else
				{
					patternMatchRow.OS_State = soundexResult;
				}
			}
		}

		void SetPort(IMatchingOrganisation org, IMatchingAddress address, OrgPatternMatchGenerationHelper patternMatchGenerationHelper)
		{
			patternMatchRow.OS_UNLOCO = patternMatchGenerationHelper.UNLOCO(org.OH_RL_NKClosestPort, address.OA_RL_NKRelatedPortCode);
		}

		void SetPhoneAndFaxNumbers(IMatchingAddress address)
		{
			patternMatchRow.OS_Phone = address.OA_Phone.IsEmpty ? String.Empty : OrgPatternMatchGenerationHelper.SanitisedPhoneNumber(address.OA_Phone, OrgPatternMatchSchema.OS_Phone.MaxLength);
			patternMatchRow.OS_FaxNum = address.OA_Fax.IsEmpty ? String.Empty : OrgPatternMatchGenerationHelper.SanitisedPhoneNumber(address.OA_Fax, OrgPatternMatchSchema.OS_FaxNum.MaxLength);
		}

		void GetCompanyName(StringWithLanguage companyName, OrgPatternMatchGenerationHelper patternMatchGenerationHelper, bool hyphensSplitWords)
		{
			var companyNameStr = patternMatchGenerationHelper.SuccinctCompanyName(companyName.Value, hyphensSplitWords);
			patternMatchRow.OS_FullCompanyName = ((ZString)companyNameStr).SubstringSafe(0, OrgPatternMatchSchema.OS_FullCompanyName.MaxLength);

			string[] companyNames = patternMatchGenerationHelper.CompanyNameSoundexWords(patternMatchRow.OS_FullCompanyName);
			patternMatchRow.OS_CompanyName1 = companyNames[0];
			patternMatchRow.OS_CompanyName2 = companyNames[1];
			patternMatchRow.OS_CompanyName3 = companyNames[2];
			patternMatchRow.OS_CompanyName4 = companyNames[3];
		}

		void SetIsCorporation(StringWithLanguage companyName, OrgPatternMatchGenerationHelper patternMatchGenerationHelper)
		{
			patternMatchRow.OS_IsCorporation = patternMatchGenerationHelper.IsOrganisationACorporation(companyName.Value);
		}

		void SetAddress(IMatchingAddress address, OrgPatternMatchGenerationHelper patternMatchGenerationHelper)
		{
			patternMatchRow.OS_StreetNumber = patternMatchGenerationHelper.StreetNumber(address.OA_Address1, OrgPatternMatchSchema.OS_StreetNumber.MaxLength);
			string postOfficeBox = patternMatchGenerationHelper.POBoxNumber(address.OA_Address1, address.OA_Address2, OrgPatternMatchSchema.OS_POBoxNumber.MaxLength);

			string[] addressWords = patternMatchGenerationHelper.AddressSoundexWords(address.OA_Address1, address.OA_Address2, patternMatchRow.OS_StreetNumber, postOfficeBox);
			patternMatchRow.OS_Address1 = addressWords[0];
			patternMatchRow.OS_Address2 = addressWords[1];
			patternMatchRow.OS_Address3 = addressWords[2];
			patternMatchRow.OS_Address4 = addressWords[3];
		}

		void SetPOBoxDetails(IMatchingAddress address, OrgPatternMatchGenerationHelper patternMatchGenerationHelper)
		{
			patternMatchRow.OS_IsPOBox = patternMatchGenerationHelper.IsPOBoxAddress(address.OA_Address1, address.OA_Address2);
			patternMatchRow.OS_POBoxNumber = patternMatchGenerationHelper.POBoxNumber(address.OA_Address1, address.OA_Address2, OrgPatternMatchSchema.OS_POBoxNumber.MaxLength);
		}

		void SetEmailAndDomain(IMatchingAddress address, OrgPatternMatchGenerationHelper patternMatchGenerationHelper)
		{
			patternMatchRow.OS_Email = patternMatchGenerationHelper.Email(address.OA_Email, OrgPatternMatchSchema.OS_Email.MaxLength);
			patternMatchRow.OS_Domain = patternMatchGenerationHelper.Domain(address.OA_Email, OrgPatternMatchSchema.OS_Domain.MaxLength);
		}
	}
}
