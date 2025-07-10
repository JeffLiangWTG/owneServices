using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Matching;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.OrgMatching
{
	/// <summary>
	/// Organisation Object Matching
	/// Org Matching will match OrgHeader entity base on the following steps:
	/// 1. It would match Entity By Organisation Code. 
	/// 2. If it could not match By Code
	///		2.1 It will match By Similarity
	///		2.2 If it could not match By Similarity
	///			2.2.1 It will match By Default Value defined
	/// </summary>
	/// 
	public class OrganisationMatcher
	{
		readonly ISimpleLogger logger;
		readonly BusinessObjectFactory factory;
		#region Constructor

		public OrganisationMatcher(BusinessObjectFactory factory, IfUnmatched unmatchedBehaviour) : this(factory, unmatchedBehaviour, new UniversalDataBuss.Integration.DummyLogger())
		{
		}

		public OrganisationMatcher(BusinessObjectFactory factory, IfUnmatched unmatchedBehaviour, ISimpleLogger logger, DataContextType? dataContextType = null)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.logger = Argument.NotNull(logger, nameof(logger));

			localCodeMatcher = new OrgLocalCodeMatcher(factory);
			similarityMatcher = new OrgSimilarMatcher(factory);

			switch (unmatchedBehaviour)
			{
				case IfUnmatched.ReturnNull:
					defaultValueMatcher = new NullDefaultValueMatcher();
					break;
				case IfUnmatched.ReturnUnmatchedOrganisation:
					defaultValueMatcher = new BaseOrgDefaultValueMatcher(factory);
					break;
				case IfUnmatched.TakeBehaviourFromOverallSetting:
					defaultValueMatcher = new OrgDefaultValueMatcher(factory);
					break;
				case IfUnmatched.TakeBehaviourFromUXMLModuleSepcifiedSetting:
					defaultValueMatcher = new OrgModuleValueMatcher(factory, (DataContextType)dataContextType);
					break;
			}
		}

#if DEBUG
		/// <summary>
		/// Testing only
		/// </summary>
		/// <param name="defaultValueMatcher"></param>
		/// <param name="localCodeMatcher"></param>
		/// <param name="similarityMatcher"></param>
		public OrganisationMatcher(BusinessObjectFactory factory, IOrgLocalCodeMatcher localCodeMatcher, IOrgSimilarMatcher similarityMatcher, IOrgDefaultValueMatcher defaultValueMatcher, ISimpleLogger logger)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.logger = Argument.NotNull(logger, nameof(logger));

			this.defaultValueMatcher = defaultValueMatcher;
			this.localCodeMatcher = localCodeMatcher;
			this.similarityMatcher = similarityMatcher;
		}

#endif
		#endregion

		readonly IOrgLocalCodeMatcher localCodeMatcher;
		readonly IOrgSimilarMatcher similarityMatcher;
		internal readonly IOrgDefaultValueMatcher defaultValueMatcher;

		// this property will be Get only after switching to new match engine
		public bool ShouldUseNewEngine { get; set; } = OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.Value;

		public OrgHeader GetMatchingOrganization(OrganizationAddress organizationAddress)
		{
			return GetMatchingOrganization(new OrganizationAddressFormatted(organizationAddress));
		}

		public OrgHeader GetMatchingOrganization(OrganizationAddressFormatted organizationAddressFormatted)
		{
			var converter = new OrganisationConverter(organizationAddressFormatted);
			var organizationMatchingData = converter.GetMatchingData(factory);
			return GetMatchingOrganization(organizationMatchingData);
		}

		public OrgHeader GetMatchingOrganization(IOrgHeaderForMatching orgMatchingData)
		{
			if (orgMatchingData == null)
			{
				return null;
			}

			var result = GetMatchingOrganizationByLocalCode(orgMatchingData)
				   ?? GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(orgMatchingData)
				   ?? GetMatchingRegistrationNumberIfContainTaxRegistrationCodeType(orgMatchingData)
				   ?? GetOrganizationByMatchEngine(orgMatchingData)
				   ?? GetDefaultOrganizationIfRegistrySaysTo();

			if (result?.OH_IsActive == false)
			{
				LogWhenMatchingOrganizationIsInactive(result);
				result = null;
			}

			return result;
		}

		public OrgAddress GetMatchingAddress(OrganizationAddress organizationAddress, ZString? shortCode, bool onlyMatchByCode)
		{
			return GetMatchingAddress(new OrganizationAddressFormatted(organizationAddress), shortCode, onlyMatchByCode);
		}

		public OrgAddress GetMatchingAddress(OrganizationAddressFormatted organizationAddressFormatted, ZString? shortCode, bool onlyMatchByCode)
		{
			var converter = new OrganisationConverter(organizationAddressFormatted);
			var organizationMatchingData = converter.GetMatchingData(factory);
			return GetMatchingAddress(organizationMatchingData, shortCode, onlyMatchByCode);
		}

		public virtual OrgAddress GetMatchingAddress(IOrgHeaderForMatching orgMatchingData, ZString? shortCode, bool onlyMatchByCode)
		{
			var organisation = GetMatchingOrganizationByLocalCode(orgMatchingData);
			var result = GetAddressFromOrganizationDirectly(organisation, shortCode);

			if (!onlyMatchByCode)
			{
				result = result
				?? GetMostSimilarOrMainAddressFromOrganizationMatchingLocalCode(orgMatchingData, organisation)
				?? GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecifiedForAddress(orgMatchingData, shortCode)
				?? GetMatchingRegistrationNumberIfContainTaxRegistrationCodeTypeForAddress(orgMatchingData)
				?? GetAddressByMatchEngine(orgMatchingData, ZGuid.Empty)
				?? GetDefaultOrganizationMainAddressIfRegistrySaysTo();
			}

			if (result?.Header?.OH_IsActive == false)
			{
				LogWhenMatchingOrganizationIsInactive(result.Header);
				result = null;
			}

			return result;
		}

		protected OrgHeader GetMatchingOrganizationByLocalCode(IOrgHeaderForMatching orgMatchingData)
		{
			return localCodeMatcher.Match(orgMatchingData.OH_Code, orgMatchingData);
		}

		bool CanUseUnmatchedOrgInSimilarityMatcher
		{
			get
			{
				return !((defaultValueMatcher is NullDefaultValueMatcher) ||
					(defaultValueMatcher is OrgModuleValueMatcher matcher && matcher.Match() == null));
			}
		}

		OrgHeader GetDefaultOrganizationIfRegistrySaysTo()
		{
			var result = defaultValueMatcher.Match();
			if (result != null)
			{
				LogWhenAssignedToUnMatchedOrganization();
			}

			return result;
		}

		OrgAddress GetDefaultOrganizationMainAddressIfRegistrySaysTo()
		{
			var result = defaultValueMatcher.Match()?.MainAddress;
			if (result != null)
			{
				LogWhenAssignedToUnMatchedOrganization();
			}

			return result;
		}

		void LogWhenAssignedToUnMatchedOrganization()
		{
			logger.Log(LogType.Information, string.Format("No match found - Assigned to UNMATCHED organization (Code: {0})", OrgHeader.UnmatchedOrganisationCode));
		}

		void LogWhenMatchingOrganizationIsInactive(OrgHeader org)
		{
			logger.Log(LogType.Warning, Res.GetString("3CA4D136-B30F-4BF3-AB0F-D7C395A98F0E", "Unable to use Organization (Code: {0}) as it is marked as inactive.", org.OH_Code));
		}

		protected virtual OrgCusCode GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(IOrgHeaderForMatching orgMatchingData, ZString? shortCode)
		{
			OrgCusCode result = null;
			if (ShouldMatchOnRegistration(orgMatchingData, shortCode))
			{
				var customsCode = orgMatchingData.CustomsCodes[0];
				if (customsCode != null)
				{
					var codeType = customsCode.OK_CodeType;
					var customsRegNo = customsCode.OK_CustomsRegNo;
					var countryCode = customsCode.OK_RN_NKCodeCountry;
					if (!customsRegNo.IsEmpty && !OrgCusCodeValidation.AllowDuplicates(codeType, countryCode))
					{
						var cusCodes = factory
							.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(countryCode, codeType, customsRegNo))
							.Where(c => c.Header?.OH_IsActive ?? false)
							.ToArray();

						if (cusCodes.Length == 1)
						{
							result = cusCodes[0];
						}
						else if (cusCodes.Length > 1)
						{
							logger.Log(LogType.Warning, Res.GetString("08519EBF-A2F8-4683-B091-43C4C8D57EFD", "Unable to determine a match as multiple organizations were matched with registration detail (Country/Region='{0}', Type='{1}', Number='{2}').", countryCode, codeType, customsRegNo));
						}
					}
				}
			}
			return result;
		}

		OrgHeader GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(IOrgHeaderForMatching orgMatchingData)
		{
			var cusCode = GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(orgMatchingData, null);
			var header = cusCode?.Header;
			if (header != null)
			{
				logger.Log(LogType.Information, GetMessageForMatchedByRegistrationDetail(header.OH_Code, cusCode.OK_RN_NKCodeCountry, cusCode.OK_CodeType, cusCode.OK_CustomsRegNo));
			}
			return header;
		}

		OrgAddress GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecifiedForAddress(IOrgHeaderForMatching orgMatchingData, ZString? shortCode)
		{
			var cusCode = GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(orgMatchingData, shortCode);
			var address = cusCode?.PremisesAddress ?? cusCode?.Header?.MainAddress;
			if (address != null)
			{
				logger.Log(LogType.Information, GetMessageForMatchedAddressByRegistrationDetail(address.OA_Code, cusCode.OK_RN_NKCodeCountry, cusCode.OK_CodeType, cusCode.OK_CustomsRegNo, address.Header?.OH_Code ?? ""));
			}
			return address;
		}

		OrgHeader GetMatchingRegistrationNumberIfContainTaxRegistrationCodeType(IOrgHeaderForMatching orgMatchingData)
		{
			var cusCodes = GetOrgCusCodesByTaxRegistrationCodeTypeAndNumber(orgMatchingData);
			OrgHeader header = null;
			if (cusCodes.Any())
			{
				if (cusCodes.Length == 1)
				{
					var cusCode = cusCodes[0];
					header = cusCode.Header;
					logger.Log(LogType.Information, GetMessageForMatchedByRegistrationDetail(header.OH_Code, cusCode.OK_RN_NKCodeCountry, cusCode.OK_CodeType, cusCode.OK_CustomsRegNo));
				}
				AddMultipleOrganisationLogIfNecessary(cusCodes);
			}

			return header;
		}

		OrgAddress GetMatchingRegistrationNumberIfContainTaxRegistrationCodeTypeForAddress(IOrgHeaderForMatching orgMatchingData)
		{
			var cusCodes = GetOrgCusCodesByTaxRegistrationCodeTypeAndNumber(orgMatchingData);

			OrgAddress address = null;
			if (cusCodes.Any())
			{
				if (cusCodes.Length == 1)
				{
					var cusCode = cusCodes[0];
					address = cusCode.PremisesAddress ?? cusCode.Header?.MainAddress;
					if (address != null)
					{
						logger.Log(LogType.Information, GetMessageForMatchedAddressByRegistrationDetail(address.OA_Code, cusCode.OK_RN_NKCodeCountry, cusCode.OK_CodeType, cusCode.OK_CustomsRegNo, address.Header?.OH_Code ?? ""));
					}
				}
				AddMultipleOrganisationLogIfNecessary(cusCodes);
			}
			return address;
		}

		void AddMultipleOrganisationLogIfNecessary(OrgCusCode[] cusCodes)
		{
			if (cusCodes.Length > 1)
			{
				logger.Log(LogType.Warning, Res.GetString("9B19C40E-4FCE-4802-899A-34B08F0371ED", "Multiple records were found with the {0} registration number {1}. Organization could not been matched.", cusCodes[0].OK_CodeType, cusCodes[0].OK_CustomsRegNo));
			}
		}

		OrgCusCode[] GetOrgCusCodesByTaxRegistrationCodeTypeAndNumber(IOrgHeaderForMatching orgMatchingData)
		{
			var result = Array.Empty<OrgCusCode>();
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var countryComplianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(countryCode) as ICountryComplianceInfo;
			var taxRegistrationCodeType = countryComplianceInfo?.GetConsumptionTaxRegistrationCode();
			if (taxRegistrationCodeType != null && ShouldMatchOnTaxRegistrationCodeType(factory, orgMatchingData, countryCode))
			{
				var taxRegistrationOrgCusCode = orgMatchingData.CustomsCodes.FirstOrDefault(x => x.OK_RN_NKCodeCountry == countryCode && x.OK_CodeType == taxRegistrationCodeType);
				if (taxRegistrationOrgCusCode != null)
				{
					result = factory
						.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(countryCode, taxRegistrationCodeType, taxRegistrationOrgCusCode.OK_CustomsRegNo))
						.Where(c => c.Header?.OH_IsActive ?? false)
						.ToArray();
					if (!result.Any())
					{
						logger.Log(LogType.Warning, Res.GetString("7104AEFE-1D62-48B9-8E83-A98D3EBF0B7E", "No organization could be matched with tax registration number (Country='{0}', Organization Name='{1}', Type='{2}', Number='{3}').", countryCode, orgMatchingData.OH_FullName, taxRegistrationCodeType, taxRegistrationOrgCusCode.OK_CustomsRegNo));
					}
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("E130B5E1-0ECA-4EF1-B170-AE2A8C4AAD6F", "No tax registration number found for organization matching (Country='{0}', Organization Name='{1}', Type='{2}').", countryCode, orgMatchingData.OH_FullName, taxRegistrationCodeType));
				}
			}
			return result;
		}

		string GetMessageForMatchedByRegistrationDetail(string orgCode, string countryCode, string codeType, string customsRegNo)
			=> Res.GetString("4D4CF3D2-986F-452C-BD37-0850389E373C", "Matched to '{0}' by registration detail (Country/Region='{1}', Type='{2}', Number='{3}').", orgCode, countryCode, codeType, customsRegNo);

		protected virtual string GetMessageForMatchedAddressByRegistrationDetail(string code, string countryCode, string codeType, string customsRegNo, string orgCode)
			=> Res.GetString("17444179-DDA7-41D4-A820-A627F05C3827", "Matched to address '{0}' on '{1}' by registration detail (Country/Region='{2}', Type='{3}', Number='{4}').", code, orgCode, countryCode, codeType, customsRegNo);

		static bool ShouldMatchOnRegistration(IOrgHeaderForMatching orgMatchingData, ZString? shortCode)
		{
			return !shortCode.HasValue
				&& orgMatchingData.CustomsCodes.Count == 1
				&& orgMatchingData.OH_Code.IsEmpty
				&& orgMatchingData.OH_FullName.IsEmpty
				&& orgMatchingData.OH_RL_NKClosestPort.IsEmpty
				&& !orgMatchingData.Addresses.Any(x => !IsAddressEmpty(x));
		}

		static bool ShouldMatchOnTaxRegistrationCodeType(BusinessObjectFactory factory, IOrgHeaderForMatching orgMatchingData, string countryCode)
		{
			return IsEligibleToMatchOrganisationByVATRegistrationNumber(factory)
				&& orgMatchingData.CustomsCodes.Any(x => x.OK_RN_NKCodeCountry == countryCode);
		}

		static bool IsEligibleToMatchOrganisationByVATRegistrationNumber(BusinessObjectFactory factory)
			=> factory.HasAnyOfContexts(OrganisationMatchingByVATRegistrationNumberContexts.GetContexts(
				AccountingMasterFilesRegistry.Instance.UseVATRegistrationNumberAsOrganizationMatchingCriteria.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
				.GetActiveCodeDescriptionPairList()));

		static bool IsAddressEmpty(IMatchingAddress address)
		{
			return address.OA_Address1.IsEmpty
				&& address.OA_Address2.IsEmpty
				&& address.OA_Code.IsEmpty
				&& address.OA_City.IsEmpty
				&& address.OA_PostCode.IsEmpty
				&& address.OA_RL_NKRelatedPortCode.IsEmpty
				&& address.OA_State.IsEmpty
				&& address.OA_Email.IsEmpty
				&& address.OA_Fax.IsEmpty
				&& address.OA_Mobile.IsEmpty
				&& address.OA_Phone.IsEmpty;
		}

		OrgAddress GetMostSimilarOrMainAddressFromOrganizationMatchingLocalCode(IOrgHeaderForMatching orgMatchingData, OrgHeader organisation)
		{
			OrgAddress result = null;

			if (organisation != null)
			{
				if (ShouldUseNewEngine)
				{
					result = GetAddressByMatchEngine(orgMatchingData, organisation.PK);
				}
				else
				{
					var finder = orgMatchingData.SimilarOrgFinder;
					var matchingFilter = new ZQuery(OrgPatternMatchSchema.OS_OH, organisation.PK);

					finder.FindSimilarOrganisations(matchingFilter, false);

					var matches = orgMatchingData.SimilarOrgMatches;
					matches.MatchThresholdOverride = OrgMatchThresholds.Codes.High;
					try
					{
						if (matches.LikelyMatchesExist)
						{
							var bestMatch = matches.OfType<OrgPatternMatch>().FirstOrDefault(m => m.OS_Rank == 1);
							if (bestMatch != null && !bestMatch.OS_OA.IsEmpty)
							{
								logger.Log(LogType.Information, string.Format("Matched to '{0}' by code, address '{1}' with a score of {2}.", organisation.OH_Code, bestMatch.Address.OA_Code, bestMatch.OS_Score));
								result = bestMatch.Address;
							}
						}
					}
					finally
					{
						matches.MatchThresholdOverride = ZString.Empty;
					}
				}

				if (result == null)
				{
					logger.Log(LogType.Information, string.Format("Matched to '{0}' by code, main address used.", organisation.OH_Code));
					result = organisation.MainAddress;
				}
			}

			return result;
		}

		protected OrgAddress GetAddressFromOrganizationDirectly(OrgHeader organisation, ZString? addressShortCode)
		{
			if (organisation != null)
			{
				if (organisation.Addresses.Count == 1)
				{
					var onlyAddress = organisation.Addresses[0];
					logger.Log(LogType.Information, Res.GetString("8406A3D4-EE53-423E-A5DA-F9EC5EDDE5B7", "Matched to '{0}' by code, address '{1}' (only address).", organisation.OH_Code, onlyAddress.OA_Code));
					return onlyAddress;
				}

				if (!string.IsNullOrWhiteSpace(addressShortCode))
				{
					var matchesOnShortCode = organisation.Addresses.Find(new ZQuery(OrgAddressSchema.OA_Code, addressShortCode));
					if (matchesOnShortCode != null && matchesOnShortCode.Length == 1 && matchesOnShortCode[0] is OrgAddress)
					{
						logger.Log(LogType.Information, Res.GetString("DFE916B3-8743-404A-85E3-BC18B6A13C6A", "Matched to '{0}' by code, address '{1}' by short code.", organisation.OH_Code, addressShortCode));
						return matchesOnShortCode[0] as OrgAddress;
					}
				}
			}

			return null;
		}

		OrgHeader GetOrganizationByMatchEngine(IOrgHeaderForMatching orgMatchingData)
		{
			return ShouldUseNewEngine ? OrganizationMatchEngine.GetMatchingOrgHeader(orgMatchingData, factory) : similarityMatcher.GetMatchingOrganisation(orgMatchingData, CanUseUnmatchedOrgInSimilarityMatcher, logger);
		}

		OrgAddress GetAddressByMatchEngine(IOrgHeaderForMatching orgMatchingData, ZGuid organizationPk)
		{
			return ShouldUseNewEngine ? OrganizationMatchEngine.GetMatchingOrgAddress(orgMatchingData, organizationPk, factory, logger) : similarityMatcher.GetMatchingAddress(orgMatchingData, CanUseUnmatchedOrgInSimilarityMatcher, logger);
		}
	}
}
