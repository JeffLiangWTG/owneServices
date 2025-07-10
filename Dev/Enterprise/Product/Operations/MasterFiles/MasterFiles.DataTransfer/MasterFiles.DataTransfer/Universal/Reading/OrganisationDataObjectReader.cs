using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;
using static Enterprise.Registry.Business.OrganisationsDataRegistry;
using RegistrationNumber = Enterprise.MasterFiles.Business.RegistrationNumber;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class OrganisationDataObjectReader : DataObjectReader<OrganizationAddressFormatted>
	{
		public OrganisationDataObjectReader(OrganizationAddress addressData, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(new OrganizationAddressFormatted(addressData), logger, factory)
		{
			converter = new OrganisationConverter(dataObject);
		}

		readonly OrganisationConverter converter;

		#region GetMatchedOrNewForTesting()
#if DEBUG

		public OrgAddress GetMatchedOrNewForTesting()
			=> GetMatched()
				?? OrganisationCreator.GetNewOrgHeaderForTest(dataObject, factory, logger).MainAddress;

#endif
		#endregion

		/// <summary>
		/// Tries to match an OrgAddress and logs the result.
		/// <param name="canUseUnmatchedOrgNote">Determines if an 'Unmatched' note can be added to the OrgAddress</param>
		/// </summary>
		/// <returns></returns>
		public OrgAddress GetMatched(bool canUseUnmatchedOrgNote = false)
		{
			var matchingLogger = new InlineLogger(Res.GetString("D5D56771-0898-4A45-8674-08908FC91AF4", "Matching '{0}':-", dataObject.AddressType));
			var organizationMatchingData = converter.GetMatchingData(factory.BOFactory);
			var matcher = GetMatcher(canUseUnmatchedOrgNote, matchingLogger);
			var matchedAddress = matcher.GetMatchingAddress(organizationMatchingData, dataObject.AddressShortCode, logger.OrgMatchingDisabled);
			var matchingResult = matchingLogger.Result;
			logger.Log(matchingResult.Type, matchingResult.Message);

			var xmlSessionTracker = logger as IXmlSessionTracker;
			xmlSessionTracker?.RecordUsedAddressTypeInCurrentUnknownAddressTypeWarningInfoKeeper(dataObject.AddressType);

			return matchedAddress;
		}

		/// <summary>
		/// Used when there IS NEED for an 'Unmatched' note on an OrganizationAddress stored in a field referencing an OrgAddress or OrgHeader.
		/// </summary>
		/// <returns></returns>
		public OrgAddress GetMatched(BusinessObject notesParent, OrganisationTypes orgCategory, string orgType = null)
		{
			var matchedOrgAddress = GetMatched(true);

			AddUnmatchedNoteIfAppropriate(notesParent, orgCategory, orgType, matchedOrgAddress);

			return matchedOrgAddress;
		}

		public static void MatchedOrNew(IDocAddresses addressParent, OrganizationAddress addressData, IXmlImportLogger logger, UniversalObjectFactory factory, Dictionary<string, ValueSetter> delaySetters, DocAddressType? docAddressTypeOverride = null)
		{
			if (delaySetters == null)
			{
				new OrganisationDataObjectReader(addressData, logger, factory).GetMatchedOrNew(addressParent, docAddressTypeOverride);
			}
			else
			{
				delaySetters.Add(new OrganizationDataObjectValueSetter(addressParent, addressData, logger, factory, docAddressTypeOverride));
			}
		}

		public JobDocAddress GetMatchedOrNew(IDocAddresses addressParent, OrgAddress orgAddress, DocAddressType? docAddressTypeOverride)
		{
			return AddOrUpdateJobDocAddress(addressParent, () => orgAddress, docAddressTypeOverride);
		}

		/// <summary>
		/// Tries to match an OrgAddress for an OrganizationAddress stored in a JobDocAddress.
		/// <param name="canUseUnmatchedOrgNote">Determines if an 'Unmatched' note can be added to the OrgAddress</param>
		/// </summary>
		/// <returns></returns>
		public JobDocAddress GetMatchedOrNew(IDocAddresses addressParent, DocAddressType? docAddressTypeOverride = null, bool canUseUnmatchedOrgNote = false)
		{
			return AddOrUpdateJobDocAddress(addressParent, () => GetMatched(canUseUnmatchedOrgNote), docAddressTypeOverride);
		}

		/// <summary>
		/// Used when there IS NEED for an 'Unmatched' note for an OrganizationAddress stored in a JobDocAddress.
		/// </summary>
		/// <returns></returns>
		public JobDocAddress GetMatchedOrNew(BusinessObject noteAndAddressParent, OrganisationTypes orgCategory, string orgType = null, DocAddressType? docAddressTypeOverride = null)
		{
			var addressParent = noteAndAddressParent as IDocAddresses
				?? throw new ArgumentException("noteAndAddressParent must implement IDocAddresses");

			OrgAddress matchedOrgAddress;
			var jobDocAddressBO = AddOrUpdateJobDocAddress(addressParent, () => GetMatched(true), out matchedOrgAddress, docAddressTypeOverride);
			if (jobDocAddressBO != null)
			{
				AddUnmatchedNoteIfAppropriate(noteAndAddressParent, orgCategory, orgType, matchedOrgAddress);
			}

			return jobDocAddressBO;
		}

		public void AddAdditionalUnmatchedNoteWhenUnmatched(BusinessObject sourceObject, OrganisationTypes organisationType, ZGuid organisationPK)
		{
			if (organisationPK == OrgHeader.UnmatchedOrganisationPK)
			{
				new UnmatchedNoteCreator(factory.BOFactory, dataObject).AddUnmatchedNote(sourceObject, organisationType);
			}
		}

		void AddUnmatchedNoteIfAppropriate(BusinessObject noteAndAddressParent, OrganisationTypes orgCategory, string orgType, OrgAddress matchedOrgAddress)
		{
			if (matchedOrgAddress != null)
			{
				bool registryEnabled;
				if (noteAndAddressParent is IOrder)
				{
					registryEnabled = Instance.UnmatchedOrganisationConfiguration.Value.GetBoolFromCode(JobTypeCodes.Order);
				}
				else
				{
					registryEnabled = Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled;
				}

				if (registryEnabled && matchedOrgAddress.OA_OH == Instance.UseUnmatchedOrganisationForMatching.Value.Organisation)
				{
					new UnmatchedNoteCreator(factory.BOFactory, dataObject).AddUnmatchedNote(noteAndAddressParent, orgCategory, orgType);
				}
			}
		}

		protected virtual OrganisationMatcher GetMatcher(bool canUseUnmatchedOrgNote, ISimpleLogger matchingLogger)
		{
			var behaviourAndType = GetUnmatchedBehaviourAndDataContextType(canUseUnmatchedOrgNote);
			var behaviour = behaviourAndType.IfUnmatchedBehaviour;
			var type = behaviourAndType.DataContextType;

			return new OrganisationMatcher(factory.BOFactory, behaviour, matchingLogger, type);
		}

		protected (IfUnmatched IfUnmatchedBehaviour, DataContextType? DataContextType) GetUnmatchedBehaviourAndDataContextType(bool canUseUnmatchedOrgNote)
		{
			var bizOType = logger?.TopLevelDataContext?.DataTargetCollection?.FirstOrDefault()?.Type;
			var isSpecifiedModule = Enum.TryParse<DataContextType>(bizOType, out var parsedContextType) && IsModulLevelContextType(parsedContextType);
			var ifUnmatchedBehaviour = canUseUnmatchedOrgNote ?
				(isSpecifiedModule ? IfUnmatched.TakeBehaviourFromUXMLModuleSepcifiedSetting : IfUnmatched.TakeBehaviourFromOverallSetting) : IfUnmatched.ReturnNull;

			return (ifUnmatchedBehaviour, parsedContextType);
		}

		bool IsModulLevelContextType(DataContextType dataContextType)
		{
			return dataContextType == DataContextType.OrderManagerOrder;
		}

		public static void LogSuccessfulMatch(OrgHeader matchedOrganisation, IXmlImportLogger logger)
		{
			logger.Log(LogType.Information, Res.GetString("637cb248-eb10-482a-9006-9de9f07b5754", "Successfully matched organization with code '{0}'.", matchedOrganisation.OH_Code));
		}

		JobDocAddress FindOrAddJobDocAddress(IDocAddresses addressParent, DocAddressType docAddressType)
		{
			JobDocAddress jobDocAddress;
			if (TryFindJobDocAddress(addressParent, docAddressType, out jobDocAddress))
			{
				return jobDocAddress;
			}

			var jobDocAddressBO = addressParent.DocAddresses.AddNew();
			SetValue(jobDocAddressBO, JobDocAddressSchema.E2_AddressType, GetDocAddressTypeCode(docAddressType));
			return jobDocAddressBO;
		}

		bool TryFindJobDocAddress(IDocAddresses addressParent, DocAddressType docAddressType, out JobDocAddress jobDocAddress)
		{
			jobDocAddress = null;
			var addressTypeCode = GetDocAddressTypeCode(docAddressType);
			foreach (JobDocAddress docAddress in addressParent.DocAddresses)
			{
				if (docAddress.E2_AddressType == addressTypeCode)
				{
					jobDocAddress = docAddress;
					return true;
				}
			}

			return false;
		}

		ZString GetDocAddressTypeCode(DocAddressType docAddressType)
		{
			return DocAddressTypes.GetCode(factory.BOFactory, docAddressType);
		}

		JobDocAddress AddOrUpdateJobDocAddress(IDocAddresses addressParent, Func<OrgAddress> matchOrgAddress, DocAddressType? docAddressTypeOverride)
		{
			OrgAddress matchedOrgAddress;
			return AddOrUpdateJobDocAddress(addressParent, matchOrgAddress, out matchedOrgAddress, docAddressTypeOverride);
		}

		JobDocAddress AddOrUpdateJobDocAddress(IDocAddresses addressParent, Func<OrgAddress> matchOrgAddress, out OrgAddress matchedOrgAddress, DocAddressType? docAddressTypeOverride)
		{
			matchedOrgAddress = null;

			if (IsNotDocAddressType(GetDataObjectAddressType()))
			{
				return null;
			}

			if (RemoveJobDocAddressFromParent(addressParent, docAddressTypeOverride))
			{
				return null;
			}

			matchedOrgAddress = matchOrgAddress();

			DocAddressType docAddressType;
			if (!TryFindDocAddressType(addressParent, docAddressTypeOverride, out docAddressType))
			{
				return null;
			}

			var jobDocAddress = FindOrAddJobDocAddress(addressParent, docAddressType);
			var canOverride = addressParent.GetCanOverrideCheckpoint(jobDocAddress)?.IsAllowed ?? true;
			PopulateJobDocAddress(matchedOrgAddress, jobDocAddress, canOverride);

			if (jobDocAddress != null)
			{
				if (
					jobDocAddress.IsEmpty
					|| jobDocAddress.IsOverridenButEmpty
					|| ((matchedOrgAddress == null || jobDocAddress.E2_AddressOverride) && dataObject.HasTypeAndCodeOnly())
					)
				{
					jobDocAddress.Delete();
					return null;
				}

				var additionalAddressInfo = dataObject.AdditionalAddressInformation.GetValueOrDefault();
				if (!additionalAddressInfo.IsEmpty && !jobDocAddress.E2_AddressOverride && jobDocAddress.HasRealAddress)
				{
					var orgAddress = jobDocAddress.Address;
					if (!OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.Value)
					{
						SetValue(jobDocAddress, JobDocAddressSchema.E2_AdditionalAddressInformation, ZString.Empty);
					}
					else if ((orgAddress?.AdditionalInfos.All(info => !info.OAI_AdditionalInfo.EqualsIgnoringCase(additionalAddressInfo)) ?? false) &&
						(!orgAddress?.Header?.OH_OverrideAdditionalAddressInformation ?? false))
					{
						SetValue(jobDocAddress, JobDocAddressSchema.E2_AdditionalAddressInformation, ZString.Empty);
					}
				}
			}
			return jobDocAddress;
		}

		bool IsNotDocAddressType(string dataObjectAdressType)
		{
			return dataObjectAdressType == AddressTypes.Recipient;
		}

		bool RemoveJobDocAddressFromParent(IDocAddresses addressParent, DocAddressType? docAddressTypeOverride)
		{
			var isToRemove = IsToRemoveAddress();

			DocAddressType docAddressType;
			JobDocAddress jobDocAddress;
			if (isToRemove && TryFindDocAddressType(addressParent, docAddressTypeOverride, out docAddressType) && TryFindJobDocAddress(addressParent, docAddressType, out jobDocAddress))
			{
				DeleteJobDocAddress(jobDocAddress);
			}

			return isToRemove;
		}

		bool IsToRemoveAddress()
		{
			return dataObject.HasTypeOnly();
		}

		bool TryFindDocAddressType(IDocAddresses addressParent, DocAddressType? docAddressTypeOverride, out DocAddressType docAddressType)
		{
			docAddressType = docAddressTypeOverride ?? GetDocAddressType(GetDataObjectAddressType());
			var isFound = docAddressType != DocAddressType.None && addressParent.SupportedAddressTypes.Contains(docAddressType);
			if (!isFound)
			{
				logger.Log(LogType.Warning, Res.GetString("4488f78b-1923-47fa-b99d-48d6300d751f", "Unknown Address Type [{0}] found. Job Document Address not imported.", GetDataObjectAddressType()));
				var xmlSessionTracker = logger as IXmlSessionTracker;
				if (xmlSessionTracker != null)
				{
					xmlSessionTracker.RecordAnUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(GetDataObjectAddressType());
				}
			}
			return isFound;
		}

		ZString GetDataObjectAddressType()
		{
			return dataObject.AddressType.GetValueOrDefault();
		}

		void DeleteJobDocAddress(JobDocAddress jobDocAddress)
		{
			jobDocAddress.Delete();
			logger.Log(LogType.Information, Res.GetString("c0287a4f-e283-4559-a36a-f07a21f99bb4", "Set '{0}' to Empty.", GetDataObjectAddressType()));
		}

		public static DocAddressType GetDocAddressType(ZString addressType)
		{
			if (!addressType.IsEmpty)
			{
				foreach (DocAddressType docAddressType in Enum.GetValues(typeof(DocAddressType)))
				{
					if (addressType == docAddressType.ToString())
					{
						return docAddressType;
					}
				}
			}

			return DocAddressType.None;
		}

		public bool PopulateIsResidential { get; set; }

		public bool IsJobDocAddressMatchingOrgAddress(IOrgAddress addressBO, JobDocAddress jobDocAddress)
		{
			bool result = true;

			if ((dataObject.AddressOverride == null || !dataObject.AddressOverride.Value) && addressBO != null)
			{
				result = !jobDocAddress.E2_AddressOverride && jobDocAddress.E2_OA_Address == addressBO.PK;
			}
			else
			{
				result = jobDocAddress.E2_AddressOverride;
				result = result && jobDocAddress.E2_Address1 == (dataObject.Address1 ?? ZString.Empty);
				result = result && jobDocAddress.E2_Address2 == (dataObject.Address2 ?? ZString.Empty);
				result = result && jobDocAddress.E2_City == (dataObject.City ?? ZString.Empty);
				result = result && jobDocAddress.E2_CompanyName == (dataObject.CompanyName ?? ZString.Empty);
				result = result && jobDocAddress.E2_Email == (dataObject.Email ?? ZString.Empty);
				result = result && jobDocAddress.E2_Fax == (dataObject.Fax ?? ZString.Empty);
				result = result && jobDocAddress.E2_GovRegNum == (dataObject.GovRegNum ?? ZString.Empty);
				result = result && jobDocAddress.E2_Mobile == (dataObject.Mobile ?? ZString.Empty);
				result = result && jobDocAddress.E2_Phone == (dataObject.Phone ?? ZString.Empty);
				result = result && jobDocAddress.E2_Postcode == (dataObject.Postcode ?? ZString.Empty);
				result = result && jobDocAddress.E2_State == (dataObject.State ?? ZString.Empty);
				result = result && jobDocAddress.E2_RN_NKCountryCode == GetNKCountryCode(jobDocAddress, dataObject.Country, dataObject.Port);

				result = result && jobDocAddress.E2_Contact == (dataObject.Contact ?? ZString.Empty);
			}
			result = result && jobDocAddress.E2_AdditionalAddressInformation == (dataObject.AdditionalAddressInformation ?? ZString.Empty);

			return result;
		}

		public void PopulateJobDocAddress(IOrgAddress addressBO, JobDocAddress jobDocAddress, bool canOverride = true)
		{
			if (canOverride && (addressBO == null || dataObject.AddressOverride.GetValueOrDefault()))
			{
				SetValue(jobDocAddress, JobDocAddressSchema.E2_AddressOverride, true);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Address1, dataObject.Address1);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Address2, dataObject.Address2);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_City, dataObject.City);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_CompanyName, dataObject.CompanyName);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Email, dataObject.Email);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Fax, dataObject.Fax);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_GovRegNum, dataObject.GovRegNum);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_GovRegNumType, dataObject.GovRegNumType);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Mobile, dataObject.Mobile);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Phone, dataObject.Phone);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Postcode, dataObject.Postcode);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_RN_NKCountryCode, dataObject.Country, dataObject.Port);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_State, dataObject.State);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_SuppressAddressValidationError, dataObject.SuppressAddressValidationError);
				SetDocAddressNumberIfRequired(jobDocAddress);

				if (PopulateIsResidential)
				{
					SetValue(jobDocAddress, JobDocAddressSchema.E2_IsResidential, dataObject.IsResidential);
				}
			}
			else if (addressBO != null)
			{
				SetValue(jobDocAddress, JobDocAddressSchema.E2_AddressOverride, false);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_OA_Address, addressBO.PK);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_SuppressAddressValidationError, false);
				SetGovernmentRegistrationNumberAndType(jobDocAddress);
			}

			SetValue(jobDocAddress, JobDocAddressSchema.E2_Contact, dataObject.Contact);
			SetValue(jobDocAddress, JobDocAddressSchema.E2_AdditionalAddressInformation, dataObject.AdditionalAddressInformation);
		}

		void SetDocAddressNumberIfRequired(JobDocAddress docAddress)
		{
			if (docAddress.SupportsDocAddressNumbers)
			{
				var docAddressNumbers = docAddress.DocAddressNumbers;
				var effectiveDocAddressNumbersToBeAddedOrUpdated = GetEffectiveDocAddressNumbersToBeAddedOrUpdated(docAddress);
				var docAddressNumbersToBeRemoved = docAddressNumbers.Cast<JobDocAddressNumber>().Where(a => !effectiveDocAddressNumbersToBeAddedOrUpdated.Any(b => b.numberType == a.E2N_NumberType && b.countryCode == a.E2N_RN_NKCountryCode)).ToList();
				if (docAddressNumbersToBeRemoved.Any())
				{
					docAddressNumbersToBeRemoved.ForEach(x => docAddressNumbers.RemoveAndDelete(x));
				}

				effectiveDocAddressNumbersToBeAddedOrUpdated.ForEach(x =>
				{
					var docAddressNumber = docAddressNumbers.FindOrCreate(x.numberType, x.countryCode);
					SetValue(docAddressNumber, JobDocAddressNumberSchema.E2N_Number, x.number);
				});
			}
		}

		List<(ZString numberType, ZString number, ZString countryCode)> GetEffectiveDocAddressNumbersToBeAddedOrUpdated(JobDocAddress docAddress)
		{
			List<(ZString numberType, ZString number, ZString countryCode)> result = new List<(ZString numberType, ZString number, ZString countryCode)>();
			var govRegNum = dataObject.GovRegNum.GetValueOrDefault();
			var countryCode = dataObject.Country?.Code ?? ZString.Empty;
			var numberType = dataObject.GovRegNumType?.Code.GetValueOrDefault() ?? ZString.Empty;
			if (!numberType.IsEmpty && !countryCode.IsEmpty && !govRegNum.IsEmpty)
			{
				result.Add((numberType, govRegNum, countryCode));
			}

			var registrationNumberCollection = dataObject.RegistrationNumberCollection;
			if (registrationNumberCollection != null)
			{
				var docAddressNumberFromRegistrationNumberCollection = registrationNumberCollection.Where(x => x.Value.HasValue && x.Type != null && x.Type.Code.HasValue && x.CountryOfIssue != null && x.CountryOfIssue.Code.HasValue).Select(x => (x.Type.Code.Value, x.Value.Value, x.CountryOfIssue.Code.Value));
				result.AddRange(docAddressNumberFromRegistrationNumberCollection);
			}

			return result;
		}

		void SetValue(JobDocAddress jobDocAddressBO, SchemaColumn column, Country country, UNLOCO port)
		{
			jobDocAddressBO[column] = GetNKCountryCode(jobDocAddressBO, country, port);
		}

		ZString GetNKCountryCode(JobDocAddress jobDocAddressBO, Country country, UNLOCO port)
		{
			var result = ZString.Empty;

			var portCode = port.GetUNLOCOAsUpperCase(jobDocAddressBO.Factory);
			var countryCode = country.GetCodeAsUpperCase();

			if (!countryCode.IsEmpty)
			{
				result = countryCode;
			}
			else if (portCode.Length == 5)
			{
				result = portCode.SubstringSafe(0, 2);
			}

			return result;
		}

		void SetGovernmentRegistrationNumberAndType(JobDocAddress jobDocAddress)
		{
			if (dataObject.GovRegNumType != null && dataObject.GovRegNumType.Code.GetValueOrDefault() != ZString.Empty && dataObject.GovRegNum.GetValueOrDefault() != ZString.Empty && jobDocAddress.Requirement != null)
			{
				jobDocAddress.Requirement.GetRegistrationNumberResult = (JobDocAddress docAddress) => { return GetRegistrationNumber(docAddress); };
			}
		}

		RegistrationNumberResult GetRegistrationNumber(JobDocAddress jobDocAddress)
		{
			var registrationNumber = new RegistrationNumber() { Number = dataObject.GovRegNum.Value, NumberType = dataObject.GovRegNumType.GetCodeAsUpperCase() };
			return new RegistrationNumberResult(jobDocAddress.Factory, true, delegate
			{ return registrationNumber; });
		}
	}
}
