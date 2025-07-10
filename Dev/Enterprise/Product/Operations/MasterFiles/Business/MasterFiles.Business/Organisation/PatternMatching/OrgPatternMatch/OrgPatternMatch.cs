using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(OrgHeader.Schema.OH_Code), DescriptionProperty(OrgHeader.Schema.OH_FullName)]
	public sealed class OrgPatternMatch : AutoOrgPatternMatch, IOrgPatternMatch
	{
		#region Schema

		public new class Schema : AutoOrgPatternMatch.Schema
		{
			public const string OS_Rank = "OS_Rank";
			public const string OS_Score = "OS_Score";
		}

		#endregion

		#region Match Types & Scores

		public static class MatchScores
		{
			public const int POBox = 5;
			public const int Corporation = 5;
			public const int Port = 10;
			public const int StreetNumber = 10;
			public const int POBoxNumber = 10;
			public const int CitySoundex = 10;
			public const int StateSoundex = 10;
			public const int PostCode = 15;
			public const int EmailName = 15;
			public const int NameWordSoundex = 15;
			public const int FirstNameWordSoundex = 20;
			public const int AddressWordSoundex = 20;
			public const int FirstAddressWordSoundex = 25;
			public const int StreetNumberFirstAddressWord = 30;

			public const int PhoneLastSevenDigits = 80;
			public const int FaxLastSevenDigits = 80;

			public const int AddressExact = 120;
			public const int NameExact = 120;

			public const int BusinessRegNo = 150;
			public const int EmailExact = 150;
		}

		#endregion

		public OrgPatternMatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#region Properties

		#region OS_Rank

		ZInt rank;

		public ZInt OS_Rank
		{
			get { return rank; }
			set { SetNonPersistentPropertyValue(OS_RankInfo, ref rank, value); }
		}

		public ZPropertyInfo OS_RankInfo
		{
			get { return GetZPropertyInfo(nameof(OS_Rank)); }
		}

		#endregion

		#region OS_Score

		ZInt score;

		public ZInt OS_Score
		{
			get { return score; }
			set { SetNonPersistentPropertyValue(OS_ScoreInfo, ref score, value); }
		}

		public ZPropertyInfo OS_ScoreInfo
		{
			get { return GetZPropertyInfo(Schema.OS_Score); }
		}

		#endregion

		#region OS_Email

		[EmailAddress]
		public override ZString OS_Email
		{
			get
			{
				return base.OS_Email;
			}

			set
			{
				base.OS_Email = value;
			}
		}

		#endregion

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Encoding

		/// <summary>
		/// Pattern Matches are Organisation and Address dependent. This creates a pattern match based on the Org/Address combo
		/// </summary>
		/// <param name="Org">Organisation to generate match from</param>
		/// <param name="CompanyName">Name of Company</param>
		/// <param name="Address">Address to generate from</param>
		public void EncodeOrganisation(IMatchingOrganisation org, StringWithLanguage companyName, IMatchingAddress address, string localBusinessNumber)
		{
			new PatternMatchRowBuilder(this).EncodeOrganisation(org, companyName, address, localBusinessNumber);
		}

		public void EncodeName(StringWithLanguage companyName, OrgPatternMatchGenerationHelper patternMatchGenerationHelper, bool hyphensSplitWords)
		{
			new PatternMatchRowBuilder(this).EncodeName(companyName, patternMatchGenerationHelper, hyphensSplitWords);
		}

		#endregion

		#region Match Filter

		internal ZQuery GetFilter()
		{
			return GetFilter(true);
		}

		internal ZQuery GetFilter(bool allowMorePotentialMatches)
		{
			var filter = GetFilterExcludingUnlocoAndBusinessRegNo(allowMorePotentialMatches);

			var refUNLOCOFilter = ExcludeOrgsNotInSameCountry();
			filter.AddToFilter(refUNLOCOFilter, JoinCondition.And);

			var businessRegNoFilter = ExcludeOrgsWithDifferentBusinessRegNo();
			filter.AddToFilter(businessRegNoFilter, JoinCondition.And);

			return filter;
		}

		internal ZQuery GetFilterExcludingUnlocoAndBusinessRegNo(bool allowMorePotentialMatches)
		{
			var filter = new ZQuery();

			var nameFilter = CreateFilter(CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_CompanyName1, OrgPatternMatchSchema.OS_CompanyName2, OrgPatternMatchSchema.OS_CompanyName3, OrgPatternMatchSchema.OS_CompanyName4);
			var addressFilter = CreateFilter(CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_Address1, OrgPatternMatchSchema.OS_Address2, OrgPatternMatchSchema.OS_Address3, OrgPatternMatchSchema.OS_Address4);
			var nameAndAddressFilter = new ZQuery(nameFilter, allowMorePotentialMatches ? JoinCondition.Or : JoinCondition.And, addressFilter);
			var fullCompanyNameFilter = CreateFullCompanyNameFilter(OS_FullCompanyName, allowMorePotentialMatches);
			var contactAndOrgDetailsFilter = CreateFilter(CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_Email);
			var phoneAndFaxFilter = CreatePhoneAndFaxFilter(OrgPatternMatchSchema.OS_Phone, OrgPatternMatchSchema.OS_FaxNum, OS_Phone, OS_FaxNum);
			var orgDetailsFilter = new ZQuery(contactAndOrgDetailsFilter, JoinCondition.Or, phoneAndFaxFilter);
			var nameAndContactFilter = new ZQuery();
			nameAndContactFilter.AddToFilter(nameAndAddressFilter);
			nameAndContactFilter.AddToFilter(fullCompanyNameFilter, JoinCondition.Or);
			nameAndContactFilter.AddToFilter(orgDetailsFilter, JoinCondition.Or);

			filter.AddToFilter(nameAndContactFilter);

			return filter;
		}

		ZQuery CreateFullCompanyNameFilter(ZString fullCompanyName, bool allowMorePotentialMatches)
		{
			var result = new ZQuery();
			if (!fullCompanyName.IsEmpty)
			{
				var joinedQuery = CreateFilterAllowBlanks(CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_FullCompanyName, fullCompanyName);//GetFullCompanyNameFilter(fullCompanyName);
				var nameFilter = CreateFilter(CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_CompanyName1, OrgPatternMatchSchema.OS_CompanyName2, OrgPatternMatchSchema.OS_CompanyName3, OrgPatternMatchSchema.OS_CompanyName4);
				result.AddToFilter(new ZQuery(nameFilter, allowMorePotentialMatches ? JoinCondition.Or : JoinCondition.And, joinedQuery));
			}

			return result;
		}

		ZQuery CreatePhoneAndFaxFilter(SchemaColumn phoneColumn, SchemaColumn faxColumn, ZString phoneValue, ZString faxValue)
		{
			// Compare phone->phone, phone->fax, fax->fax, fax->phone, as users seem to cross enter these fields sometimes.
			var filter = new ZQuery();

			if (!phoneValue.IsEmpty)
			{
				AddToFilterWithCustomParameters(filter, phoneColumn, CustomParameterOperators.Equals, phoneValue);
				AddToFilterWithCustomParameters(filter, faxColumn, CustomParameterOperators.Equals, phoneValue);
			}

			if (!faxValue.IsEmpty && faxValue != phoneValue)
			{
				AddToFilterWithCustomParameters(filter, faxColumn, CustomParameterOperators.Equals, faxValue);
				AddToFilterWithCustomParameters(filter, phoneColumn, CustomParameterOperators.Equals, faxValue);
			}

			return filter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery CreateFilter(CustomParameterOperators operatorForFields, params SchemaColumn[] filterColumns)
		{
			var filter = new ZQuery();

			foreach (var column in filterColumns)
			{
				filter.AddToFilter(CreateFilter(operatorForFields, column, this[column.Name] as IZType), JoinCondition.Or);
			}

			return filter;
		}

		ZQuery CreateFilter(CustomParameterOperators operatorForFields, SchemaColumn filterColumn, IZType value)
		{
			var filter = new ZQuery();

			if (value != null && !value.IsEmpty)
			{
				filter.AddToFilter(CreateFilterAllowBlanks(operatorForFields, filterColumn, value));
			}

			return filter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		internal static ZQuery CreateFilterAllowBlanks(CustomParameterOperators operatorForFields, SchemaColumn filterColumn, IZType value)
		{
			var filter = new ZQuery();

			AddToFilterWithCustomParameters(filter, filterColumn, operatorForFields, value.ToString());

			return filter;
		}

		internal enum CustomParameterOperators
		{
			StartsWith,
			Equals
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		static void AddToFilterWithCustomParameters(ZQuery filter, SchemaColumn column, CustomParameterOperators customOperator, string value)
		{
			if (customOperator == CustomParameterOperators.StartsWith)
			{
				filter.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.StartsWith, value);
			}
			else
			{
				filter.AddToFilter(JoinCondition.Or, column, value);
			}
		}

		ZQuery ExcludeOrgsWithDifferentBusinessRegNo()
		{
			return ExcludeOrgsWithDifferentBusinessRegNo(OS_BusinessRegNo);
		}

		internal static ZQuery ExcludeOrgsWithDifferentBusinessRegNo(ZString businessRegNo)
		{
			var result = new ZQuery();

			if (!businessRegNo.IsEmpty)
			{
				result.AddToFilter(CreateFilterAllowBlanks(CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_BusinessRegNo, businessRegNo));
				result.AddToFilter(CreateFilterAllowBlanks(CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_BusinessRegNo, ZString.Empty), JoinCondition.Or);
			}

			return result;
		}

		ZQuery ExcludeOrgsNotInSameCountry()
		{
			return ExcludeOrgsNotInSameCountry(OS_UNLOCO);
		}

		internal static ZQuery ExcludeOrgsNotInSameCountry(ZString unloco)
		{
			var result = new ZQuery();

			if (!unloco.IsEmpty)
			{
				// Add UNLOCO Filter
				result.AddToFilter(CreateFilterAllowBlanks(CustomParameterOperators.StartsWith, OrgPatternMatchSchema.OS_UNLOCO, unloco));
				result.AddToFilter(CreateFilterAllowBlanks(CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_UNLOCO, ZString.Empty), JoinCondition.Or);

				if (unloco.Length >= 2)
				{
					// Add Country Filter
					result.AddToFilter(CreateFilterAllowBlanks(CustomParameterOperators.StartsWith, OrgPatternMatchSchema.OS_UNLOCO, unloco.Substring(0, 2)), JoinCondition.Or);
				}
			}

			return result;
		}

		#endregion

		#region Match Score

		/// <summary>
		/// Sets the score for this pattern match based on the highest scoring match from all matches passed in.
		/// </summary>
		/// <param name="MatchesForOrg">A collection of pattern matches for an organisation which will be used</param>
		public void SetMatchScoreForOrg(OrgPatternMatchCollection matchesForOrg)
		{
			OS_Score = 0;

			foreach (OrgPatternMatch match in matchesForOrg)
			{
				int tempScore = GetMatchScoreForOrg(match);

				// Do not match on OrgType, for performance improvements.
				if (tempScore > OS_Score)
				{
					OS_Score = tempScore;
				}
			}
		}

		public int GetMatchScoreForOrg(OrgPatternMatch match)
		{
			var result = 0;
			result += GetNameScore(match);
			result += GetAddressScore(match);
			result += GetPortScore(match);
			result += GetEmailScore(match);
			result += GetPhoneAndFaxScore(match);
			result += GetBusinessRegNoScore(match);

			return result;
		}

		bool MatchFound(IZType sourceValue, IZType compareValue)
		{
			return !sourceValue.IsEmpty && sourceValue.Equals(compareValue);
		}

		ZInt EvaluateScore(IZType sourceValue, IZType compareValue, int matchScore)
		{
			return MatchFound(sourceValue, compareValue) ? matchScore : 0;
		}

		ZInt GetNameScore(OrgPatternMatch orgMatch)
		{
			var score = 0;

			if (MatchFound(OS_FullCompanyName, orgMatch.OS_FullCompanyName) && (OS_IsCorporation == orgMatch.OS_IsCorporation))
			{
				score = MatchScores.NameExact;
			}
			else
			{
				score += EvaluateScore(OS_CompanyName1, orgMatch.OS_CompanyName1, MatchScores.FirstNameWordSoundex);
				score += EvaluateScore(OS_CompanyName2, orgMatch.OS_CompanyName2, MatchScores.NameWordSoundex);
				score += EvaluateScore(OS_CompanyName3, orgMatch.OS_CompanyName3, MatchScores.NameWordSoundex);
				score += EvaluateScore(OS_CompanyName4, orgMatch.OS_CompanyName4, MatchScores.NameWordSoundex);

				if (!OS_FullCompanyName.IsEmpty && !orgMatch.OS_FullCompanyName.IsEmpty && OS_IsCorporation && orgMatch.OS_IsCorporation)
				{
					score += MatchScores.Corporation;
				}
			}

			return score;
		}

		ZInt GetAddressScore(OrgPatternMatch orgMatch)
		{
			var score = 0;

			if (AtLeastFourAddressPartsAvailable && AddressesMatchExactly(orgMatch))
			{
				score = MatchScores.AddressExact;
			}
			else
			{
				score += EvaluateScore(OS_Address1, orgMatch.OS_Address1, MatchScores.FirstAddressWordSoundex);
				score += EvaluateScore(OS_Address2, orgMatch.OS_Address2, MatchScores.AddressWordSoundex);
				score += EvaluateScore(OS_Address3, orgMatch.OS_Address3, MatchScores.AddressWordSoundex);
				score += EvaluateScore(OS_Address4, orgMatch.OS_Address4, MatchScores.AddressWordSoundex);

				score += EvaluateScore(OS_StreetNumber.Replace("\\", "/"), orgMatch.OS_StreetNumber.Replace("\\", "/"), MatchScores.StreetNumber);
				score += EvaluateScore(OS_PostCode, orgMatch.OS_PostCode, MatchScores.PostCode);
				score += EvaluateScore(OS_City, orgMatch.OS_City, MatchScores.CitySoundex);
				score += EvaluateScore(OS_State, orgMatch.OS_State, MatchScores.StateSoundex);

				if (MatchFound(OS_Address1, orgMatch.OS_Address1) && MatchFound(OS_StreetNumber.Replace("\\", "/"), orgMatch.OS_StreetNumber.Replace("\\", "/")))
				{
					score += MatchScores.StreetNumberFirstAddressWord;
				}

				if (MatchFound(OS_POBoxNumber, orgMatch.OS_POBoxNumber))
				{
					score += MatchScores.POBoxNumber;
				}
				else if (OS_IsPOBox && orgMatch.OS_IsPOBox)
				{
					score += MatchScores.POBox;
				}
			}
			return score;
		}

		bool AtLeastFourAddressPartsAvailable
		{
			get
			{
				int count = 0;
				if (!string.IsNullOrEmpty(OS_Address1))
				{
					count++;
				}

				if (!string.IsNullOrEmpty(OS_Address2))
				{
					count++;
				}

				if (!string.IsNullOrEmpty(OS_Address3))
				{
					count++;
				}

				if (!string.IsNullOrEmpty(OS_Address4))
				{
					count++;
				}

				if (!string.IsNullOrEmpty(OS_StreetNumber))
				{
					count++;
				}

				if (!string.IsNullOrEmpty(OS_POBoxNumber))
				{
					count++;
				}

				if (!string.IsNullOrEmpty(OS_City))
				{
					count++;
				}

				if (!string.IsNullOrEmpty(OS_State))
				{
					count++;
				}

				if (!string.IsNullOrEmpty(OS_PostCode))
				{
					count++;
				}

				return count >= 4;
			}
		}

		bool AddressesMatchExactly(OrgPatternMatch compareMatch)
		{
			return MatchFound(OS_Address1, compareMatch.OS_Address1)
				&& OS_Address2 == compareMatch.OS_Address2
				&& OS_Address3 == compareMatch.OS_Address3
				&& OS_Address4 == compareMatch.OS_Address4
				&& OS_StreetNumber == compareMatch.OS_StreetNumber
				&& OS_IsPOBox == compareMatch.OS_IsPOBox
				&& OS_POBoxNumber == compareMatch.OS_POBoxNumber
				&& OS_City == compareMatch.OS_City
				&& (OS_State == compareMatch.OS_State || compareMatch.OS_State.IsEmpty)
				&& (OS_PostCode == compareMatch.OS_PostCode || compareMatch.OS_PostCode.IsEmpty);
		}

		ZInt GetPortScore(OrgPatternMatch orgMatch)
		{
			return EvaluateScore(OS_UNLOCO, orgMatch.OS_UNLOCO, MatchScores.Port);
		}

		ZInt GetEmailScore(OrgPatternMatch orgMatch)
		{
			var score = 0;

			if (MatchFound(OS_Email, orgMatch.OS_Email) && MatchFound(OS_Domain, orgMatch.OS_Domain))
			{
				score = MatchScores.EmailExact;
			}
			else
			{
				score += EvaluateScore(OS_Email, orgMatch.OS_Email, MatchScores.EmailName);
			}

			return score;
		}

		ZInt GetPhoneAndFaxScore(OrgPatternMatch orgMatch)
		{
			// Compare phone->phone, phone->fax, fax->fax, fax->phone, as users seem to cross enter these fields sometimes.
			var score = EvaluateScore(OS_Phone, orgMatch.OS_Phone, MatchScores.PhoneLastSevenDigits);
			score += EvaluateScore(OS_Phone, orgMatch.OS_FaxNum, MatchScores.PhoneLastSevenDigits);
			score += EvaluateScore(OS_FaxNum, orgMatch.OS_FaxNum, MatchScores.FaxLastSevenDigits);
			score += EvaluateScore(OS_FaxNum, orgMatch.OS_Phone, MatchScores.FaxLastSevenDigits);
			return score;
		}

		ZInt GetBusinessRegNoScore(OrgPatternMatch orgMatch)
		{
			return EvaluateScore(OS_BusinessRegNo, orgMatch.OS_BusinessRegNo, MatchScores.BusinessRegNo);
		}

		#endregion

		#region OrgHeader property wrapping for display in grid

		public ZString AddressCapabilityCodes
		{
			get
			{
				return OS_OA.IsValid ?
					Address.AddressCapability.GetListOfCodes() :
					ZString.Empty;
			}
		}

		#region OH_Code

		public ZString OH_Code
		{
			get { return OS_OH.IsValid ? Header.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo OH_CodeInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Code)); }
		}

		#endregion

		#region OH_FullName

		public ZString OH_FullName
		{
			get { return OS_OH.IsValid ? Header.OH_FullName : ZString.Empty; }
		}

		public ZPropertyInfo OH_FullNameInfo
		{
			get { return GetZPropertyInfo(nameof(OH_FullName)); }
		}

		#endregion

		#region OH_Calc_Email

		public ZString OH_Calc_Email
		{
			get { return OS_OA.IsValid ? Address.OA_Email : ZString.Empty; }
		}

		public ZPropertyInfo OH_Calc_EmailInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_Email)); }
		}

		#endregion

		#region OH_Calc_Address1

		public ZString OH_Calc_Address1
		{
			get { return OS_OA.IsValid ? Address.OA_Address1 : ZString.Empty; }
		}

		public ZPropertyInfo OH_Calc_Address1Info
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_Address1)); }
		}

		#endregion

		#region OH_Calc_Address2

		public ZString OH_Calc_Address2
		{
			get { return OS_OA.IsValid ? Address.OA_Address2 : ZString.Empty; }
		}

		public ZPropertyInfo OH_Calc_Address2Info
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_Address2)); }
		}

		#endregion

		#region OH_Calc_Phone

		public ZString OH_Calc_Phone
		{
			get { return OS_OA.IsValid ? Address.OA_Phone : ZString.Empty; }
		}

		public ZPropertyInfo OH_Calc_PhoneInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_Phone)); }
		}

		#endregion

		#region OH_Calc_Fax

		public ZString OH_Calc_Fax
		{
			get { return OS_OA.IsValid ? Address.OA_Fax : ZString.Empty; }
		}

		public ZPropertyInfo OH_Calc_FaxInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_Fax)); }
		}

		#endregion

		#region OH_Calc_PostCode

		public ZString OH_Calc_PostCode
		{
			get { return OS_OA.IsValid ? Address.OA_PostCode : ZString.Empty; }
		}

		public ZPropertyInfo OH_Calc_PostCodeInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_PostCode)); }
		}

		#endregion

		#region OH_Calc_City

		public ZString OH_Calc_City
		{
			get { return OS_OA.IsValid ? Address.OA_City : ZString.Empty; }
		}

		public ZPropertyInfo OH_Calc_CityInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_City)); }
		}

		#endregion

		#region OH_Calc_State

		public ZString OH_Calc_State
		{
			get { return OS_OA.IsValid ? Address.OA_State : ZString.Empty; }
		}

		public ZPropertyInfo OH_Calc_StateInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_State)); }
		}

		#endregion

		#region LocalBusinessNumber

		public ZString LocalBusinessNumber
		{
			get { return OS_OH.IsValid ? OS_BusinessRegNo : ZString.Empty; }
		}

		public ZPropertyInfo LocalBusinessNumberInfo
		{
			get { return GetZPropertyInfo(nameof(LocalBusinessNumber)); }
		}

		#endregion

		#region Match Likelihood

		public ZString MatchLikelihood
		{
			get
			{
				string result;

				if (OS_Score < OrgPatternMatchCollection.MediumMatchThreshold)
				{
					result = OrgMatchThresholds.Descriptions.Low;
				}
				else if (OS_Score < OrgPatternMatchCollection.HighMatchThreshold)
				{
					result = OrgMatchThresholds.Descriptions.Medium;
				}
				else if (OS_Score < OrgPatternMatchCollection.ExtremeMatchThreshold)
				{
					result = OrgMatchThresholds.Descriptions.High;
				}
				else
				{
					result = OrgMatchThresholds.Descriptions.Extreme;
				}

				return result;
			}
		}

		public ZPropertyInfo MatchLikelihoodInfo
		{
			get { return GetZPropertyInfo(nameof(MatchLikelihood)); }
		}

		#endregion

		#endregion

		protected override ZString HumanReadableNameCore => Header != null && !Header.IsDeleted
			? Header.HumanReadableName
			: (ZString)Res.GetString("42A70464-AABD-4B4A-867D-2B56BEA48899", "Deleted Organization");

		public override string QuickViewCard
		{
			get
			{
				var seperator = System.Environment.NewLine + " - ";
				string capabilities = null;
				string port = null;
				if (OS_OA.IsValid)
				{
					capabilities = string.Join(seperator, Address.AddressCapability.EnabledCapabilities.Select(pair => pair.Description));
					port = Address.OA_RL_NKRelatedPortCode;
				}

				if (string.IsNullOrEmpty(capabilities))
				{
					capabilities = Res.GetString("D8129E61-6C48-4784-AB43-A5C1BD3A7C43", "None");
				}

				return Res.GetString("E1DBFD06-E4CA-4AD0-8DA1-7BC55B6E7F88",
@"Name: {0}
Port: {1}
Address 1: {2}
Address 2: {3}
City: {4}
Capabilities:{5}", OH_FullName, port, OH_Calc_Address1, OH_Calc_Address2, OH_Calc_City, seperator + capabilities);
			}
		}

		public ZString DbName { get; set; } = Db.DatabaseName;
	}
}
