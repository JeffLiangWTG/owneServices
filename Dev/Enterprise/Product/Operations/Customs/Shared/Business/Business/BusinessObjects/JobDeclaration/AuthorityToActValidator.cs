using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class AuthorityToActValidator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Country Specific")]
		public AuthorityToActValidator()
			: this("Power of Attorney", string.Empty)
		{
		}

		public AuthorityToActValidator(string countrySpecificNameForPOA, string noPOADocumentForImporterOverrideString)
			: this(countrySpecificNameForPOA, noPOADocumentForImporterOverrideString, string.Empty)
		{
		}

		public AuthorityToActValidator(string countrySpecificNameForPOA, string noPOADocumentForImporterOverrideString, string pOANotValidForConditionsOverrideString)
		{
			this.CountrySpecificNameForPOA = countrySpecificNameForPOA;
			this.NoPOADocumentForImporterOverrideString = noPOADocumentForImporterOverrideString;
			this.pOANotValidForConditionsOverrideString = pOANotValidForConditionsOverrideString;
		}

		public readonly string CountrySpecificNameForPOA;
		public readonly string NoPOADocumentForImporterOverrideString;
		readonly string pOANotValidForConditionsOverrideString;

		public void Validate(BaseJobDeclaration declaration, OrgHeader organisationBeingValidated, ZPropertyInfo propertyBeingValidated, string powerOfAttorneyCode, string powerOfAttorneyCustoms)
		{
			Validate(declaration, organisationBeingValidated, propertyBeingValidated, powerOfAttorneyCode, powerOfAttorneyCustoms, "");
		}

		public void Validate(BaseJobDeclaration declaration, OrgHeader organisationBeingValidated, ZPropertyInfo propertyBeingValidated, string powerOfAttorneyCode, string powerOfAttorneyCustoms, string powerOfAttorneyForwarding)
		{
			Validate(declaration, organisationBeingValidated, propertyBeingValidated, powerOfAttorneyCode, powerOfAttorneyCustoms, powerOfAttorneyForwarding, new List<(Predicate<JobRequiredDocument>, string)>()
			{
				HasNoCountryCodeOrHasMatchedCountryCode(declaration.CountryCode),
				HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(declaration.Company.GC_Code),
				HasNoDirectionAttributeOrHasMatchedDirectionAttribute(declaration.JE_MessageType)
			}, CurrentCountryOrDirectionCondition(declaration.CountryCode));
		}

		protected List<JobRequiredDocument> GetAllPOAMatchingExtraCriteria(IAuthorityToActDeclarationProvider declaration, OrgHeader organisationBeingValidated, string[] powerOfAttorneyCodes, Predicate<JobRequiredDocument> extraMatch)
		{
			var poas = new List<JobRequiredDocument>();
			if (organisationBeingValidated != null)
			{
				JobRequiredDocument poa = null;

				foreach (var code in powerOfAttorneyCodes)
				{
					poa = GetPowerOfAttorney(declaration, organisationBeingValidated.RequiredDocuments, extraMatch, code, declaration.CountryCode);
					if (poa != null)
					{
						poas.Add(poa);
					}
				}
			}
			return poas;
		}

		protected List<JobRequiredDocument> GetAllPOAFromCartageAndDocs(IAuthorityToActDeclarationProvider declaration, string[] powerOfAttorneyCodes)
		{
			var poas = new List<JobRequiredDocument>();
			if(declaration.DocsAndCartage != null)
			{
				foreach (var code in powerOfAttorneyCodes)
				{
					var poa = GetPowerOfAttorney(declaration, declaration.DocsAndCartage.RequiredDocuments, null, code, declaration.CountryCode);
					if (poa != null)
					{
						poas.Add(poa);
					}
				}
			}
			return poas;
		}

		void ValidateMandatory(IAuthorityToActDeclarationProvider declaration, OrgHeader organisationBeingValidated, ZPropertyInfo propertyBeingValidated, string[] powerOfAttorneyCodes, List<JobRequiredDocument> poasMatchingExtraCriteria, List<JobRequiredDocument> poasFromCartageAndDocs, List<(Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText)> extraMatchingConditionsList, string extraMatchingConditionsOverrideString)
		{
			if (organisationBeingValidated != null)
			{
				if (poasFromCartageAndDocs.Count == 0 && poasMatchingExtraCriteria.Count == 0)
				{
					var notificationType = CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType(declaration.RegistryCompanyPK);
					var powerOfAttorneys = GetAllPowerOfAttorneysMatchExtraCriteria(organisationBeingValidated.RequiredDocuments, powerOfAttorneyCodes);

					if (powerOfAttorneys.Any())
					{
						var pOANotValidForConditionsNotification = string.Empty;

						if (!string.IsNullOrEmpty(pOANotValidForConditionsOverrideString))
						{
							pOANotValidForConditionsNotification = pOANotValidForConditionsOverrideString;
						}
						else if (!string.IsNullOrEmpty(extraMatchingConditionsOverrideString))
						{
							pOANotValidForConditionsNotification = GetPOANotValidForConditions(CountrySpecificNameForPOA, OrganizationString, extraMatchingConditionsOverrideString);
						}
						else
						{
							var matchingConditionsList = CountryAndCompanyMatchingConditions(declaration.CountryCode, declaration.Company.GC_Code);
							if (extraMatchingConditionsList != null)
							{
								matchingConditionsList.AddRange(extraMatchingConditionsList);
							}

							if (matchingConditionsList != null && matchingConditionsList.Count > 0)
							{
								var matchingConditionTextList = new List<string>();

								foreach (var powerOfAttorney in powerOfAttorneys)
								{
									foreach (var matchingCondition in matchingConditionsList)
									{
										if (!matchingCondition.MatchingCondition(powerOfAttorney))
										{
											if (!string.IsNullOrEmpty(matchingCondition.MatchingConditionText) && !matchingConditionTextList.Contains(matchingCondition.MatchingConditionText))
											{
												matchingConditionTextList.Add(matchingCondition.MatchingConditionText);
											}
										}
									}
								}

								if (matchingConditionsList.Any())
								{
									var matchingCondtionString = string.Join(", ", matchingConditionTextList);
									var lastComma = matchingCondtionString.LastIndexOf(',');
									if (lastComma != -1)
									{
										matchingCondtionString = matchingCondtionString.Remove(lastComma, 1).Insert(lastComma, (NoResString)" and");
									}

									pOANotValidForConditionsNotification = GetPOANotValidForConditions(CountrySpecificNameForPOA, OrganizationString, matchingCondtionString);
								}
							}
						}

						if (!string.IsNullOrEmpty(pOANotValidForConditionsNotification))
						{
							propertyBeingValidated.AddNotification(notificationType, pOANotValidForConditionsNotification);
						}
					}
					else
					{
						propertyBeingValidated.AddNotification(notificationType, string.IsNullOrEmpty(NoPOADocumentForImporterOverrideString) ? GetNoPOADocumentForImporterString(CountrySpecificNameForPOA) : NoPOADocumentForImporterOverrideString);
					}
				}
			}
		}

		protected void ValidatePOADates(ZPropertyInfo propertyBeingValidated, List<JobRequiredDocument> poasMatchExtraCriteria, List<JobRequiredDocument> poasFromCartageAndDocs)
		{
			foreach (var poa in poasMatchExtraCriteria)
			{
				List<KeyValuePair<INotificationType, ZString>> notifications;
				var validPowerOfAttorney = ValidatePowerOfAttorneyDocumentDatesCore(poa, OrganizationString, out notifications);
				AddNotifications(propertyBeingValidated, notifications);
				if (validPowerOfAttorney)
				{
					return;
				}
			}
			foreach (var poa in poasFromCartageAndDocs)
			{
				ValidatePowerOfAttorneyDocumentDates(propertyBeingValidated, poa, DeclarationString);
			}
		}

		public void Validate(IAuthorityToActDeclarationProvider declaration, OrgHeader organisationBeingValidated, ZPropertyInfo propertyBeingValidated, string powerOfAttorneyCode, string powerOfAttorneyCustoms, string powerOfAttorneyForwarding, List<(Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText)> extraMatchingConditionsList, string extraMatchingConditionsOverrideString = "")
		{
			if (organisationBeingValidated != null)
			{
				var powerOfAttorneyCodes = new string[] { powerOfAttorneyCode, powerOfAttorneyCustoms, powerOfAttorneyForwarding };
				var poasMatchingExtraCriteria = GetAllPOAMatchingExtraCriteria(declaration, organisationBeingValidated, powerOfAttorneyCodes, doc => extraMatchingConditionsList.All(x => x.MatchingCondition(doc)));
				var poasFromCartageAndDocs = GetAllPOAFromCartageAndDocs(declaration, powerOfAttorneyCodes);

				ValidateMandatory(declaration, organisationBeingValidated, propertyBeingValidated, powerOfAttorneyCodes, poasMatchingExtraCriteria, poasFromCartageAndDocs, extraMatchingConditionsList, extraMatchingConditionsOverrideString);
				ValidatePOADates(propertyBeingValidated, poasMatchingExtraCriteria, poasFromCartageAndDocs);
			}
		}

		public bool HasValidPOA(BaseJobDeclaration declaration, OrgHeader organisationBeingValidated, string[] powerOfAttorneyCodes, Predicate<JobRequiredDocument> extraMatch)
		{
			var poasMatchingExtra = GetAllPOAMatchingExtraCriteria(declaration, organisationBeingValidated, powerOfAttorneyCodes, extraMatch);
			foreach (var poa in poasMatchingExtra)
			{
				if (IsValidPowerOfAttorneyDocumentDates(poa))
				{ return true; }
			}

			var poasFromCartageAndDocs = GetAllPOAFromCartageAndDocs(declaration, powerOfAttorneyCodes);
			foreach (var poa in poasFromCartageAndDocs)
			{
				if (IsValidPowerOfAttorneyDocumentDates(poa))
				{ return true; }
			}
			return false;
		}

		protected string OrganizationString
		{
			get { return Res.GetString("CC9B7FD7-3CA4-4c85-A332-187011C6E53E", "organization (eDocs > Document Tracking)"); }
		}

		protected string DeclarationString
		{
			get { return Res.GetString("8A1B9BA1-3504-4568-8B78-E45AAB0EAF1F", "declaration (eDocs > Document Tracking)"); }
		}

		public bool HasPowerOfAttorney(JobRequiredDocumentDependentCollection requiredDocuments, string[] powerOfAttorneyCodes, Predicate<JobRequiredDocument> extraMatch)
		{
			return GetAllPowerOfAttorneysMatchExtraCriteria(requiredDocuments, powerOfAttorneyCodes, extraMatch).Any();
		}

		protected IEnumerable<JobRequiredDocument> GetAllPowerOfAttorneysMatchExtraCriteria(JobRequiredDocumentDependentCollection requiredDocuments, string[] powerOfAttorneyCodes, Predicate<JobRequiredDocument> extraMatch)
		{
			foreach (var code in powerOfAttorneyCodes)
			{
				if (requiredDocuments.GetDocByType(code) != null)
				{
					foreach (JobRequiredDocument doc in requiredDocuments)
					{
						if (doc.EQ_DocType == code && extraMatch(doc))
						{
							yield return doc;
						}
					}
				}
			}
		}

		public bool HasAnyPowerOfAttorney(JobRequiredDocumentDependentCollection requiredDocuments, string[] powerOfAttorneyCodes)
		{
			return GetAllPowerOfAttorneysMatchExtraCriteria(requiredDocuments, powerOfAttorneyCodes).Any();
		}

		protected IEnumerable<JobRequiredDocument> GetAllPowerOfAttorneysMatchExtraCriteria(JobRequiredDocumentDependentCollection requiredDocuments, string[] powerOfAttorneyCodes)
		{
			return GetAllPowerOfAttorneysMatchExtraCriteria(requiredDocuments, powerOfAttorneyCodes, x => true);
		}

		protected JobRequiredDocument GetPowerOfAttorney(IAuthorityToActDeclarationProvider declaration, JobRequiredDocumentDependentCollection requiredDocuments, Predicate<JobRequiredDocument> extraMatch, string code, ZString countryCode)
		{
			var poa = requiredDocuments.GetDocByType(code, countryCode, x => (x.EQ_ValidToDate >= ZDateTime.Today || x.EQ_ValidToDate.IsEmpty) && HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(x, declaration));
			if (poa == null)
			{
				poa = requiredDocuments.GetDocByType(code, countryCode, x => HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(x, declaration, extraMatch));
				if (poa == null)
				{
					poa = requiredDocuments.GetDocByType(code, countryCode, x => HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(x, declaration));
				}
			}
			return poa;
		}

		ZBool HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(JobRequiredDocument jobReqDoc, IAuthorityToActDeclarationProvider declaration)
		{
			return HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(jobReqDoc, declaration, x => !x.Attributes.Any(a => !a.IsCompanyCode));
		}

		ZBool HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(JobRequiredDocument document, IAuthorityToActDeclarationProvider declaration, Predicate<JobRequiredDocument> attributePredicate)
		{
			var result = true;
			if (attributePredicate != null)
			{
				result = attributePredicate(document);
			}
			if (result && declaration?.Company is GlbCompany company)
			{
				result = HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(company.GC_Code).MatchingCondition.Invoke(document);
			}

			return result;
		}

		protected List<(Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText)> CountryAndCompanyMatchingConditions(string countryCode, string companyCode)
		{
			return new List<(Predicate<JobRequiredDocument>, string)>()
			{
				HasNoCountryCodeOrHasMatchedCountryCode(countryCode),
				HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(companyCode)
			};
		}

		public static (Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText) HasNoCountryCodeOrHasMatchedCountryCode(string countryCode)
		{
			return (doc => doc.EQ_RN_NKRelatedCountry.IsEmpty || doc.EQ_RN_NKRelatedCountry == countryCode, $"'{countryCode}' Country");
		}

		public static (Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText) HasNoCompanyCodeAttributeOrHasMatchedCompanyCodeAttribute(string companyCode)
		{
			return HasNoAttributeFoundByNameOrAttributeMatches(JobRequiredDocAttribTypeList.Codes.CompanyCode, companyCode, (NoResString)"Company");
		}

		public static (Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText) HasNoDirectionAttributeOrHasMatchedDirectionAttribute(string direction)
		{
			return HasNoAttributeFoundByNameOrAttributeMatches(JobRequiredDocAttribTypeList.Codes.Direction, direction, (NoResString)"Direction");
		}

		public static (Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText) HasNoPortOfEntryAttributeOrHasMatchedPortOfEntryAttribute(string portOfEntry)
		{
			return HasNoAttributeFoundByNameOrAttributeMatches(JobRequiredDocAttribTypeList.Codes.PortOfEntry, portOfEntry, (NoResString)"Port of Entry");
		}

		public static (Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText) HasNoAttributeFoundByNameOrAttributeMatches(string attributeName, string attributeValue, string description)
		{
			return (doc => !doc.Attributes.HasAttributeType(attributeName) || doc.Attributes[attributeName, attributeValue] != null, $"'{attributeValue}' {description}");
		}

		public bool ValidatePowerOfAttorneyDocumentDates(ZPropertyInfo propertyInfo, JobRequiredDocument document, ZString documentOwner)
		{
			List<KeyValuePair<INotificationType, ZString>> notifications;
			var result = ValidatePowerOfAttorneyDocumentDatesCore(document, documentOwner, out notifications);
			AddNotifications(propertyInfo, notifications);
			return result;
		}

		protected void AddNotifications(ZPropertyInfo propertyInfo, List<KeyValuePair<INotificationType, ZString>> notifications)
		{
			foreach (var pair in notifications)
			{
				propertyInfo.AddNotification(pair.Key, pair.Value);
			}
		}

		protected bool IsValidPowerOfAttorneyDocumentDates(JobRequiredDocument document)
		{
			return document != null &&
				(
					!document.IsPeriodic || !document.EQ_ValidToDate.IsEmpty && document.EQ_ValidToDate >= ZDateTime.Today
				);
		}

		enum POADocumentDatesStatus { NoDocumentExists, NoExpiryDate, Expired, WillExpireWithinAMonth, NoReceivedDate }

		IEnumerable<POADocumentDatesStatus> GetPowerOfAttorneyDocumentDatesStatuses(JobRequiredDocument document)
		{
			if (document != null)
			{
				if (document.IsPeriodic)
				{
					if (document.EQ_ValidToDate.IsEmpty)
					{
						yield return POADocumentDatesStatus.NoExpiryDate;
					}
					else if (document.EQ_ValidToDate < ZDateTime.Today)
					{
						yield return POADocumentDatesStatus.Expired;
					}
					else if ((document.EQ_ValidToDate - ZDateTime.Today) < new TimeSpan(30, 0, 0, 0))
					{
						yield return POADocumentDatesStatus.WillExpireWithinAMonth;
					}
				}

				if (document.EQ_DateReceived.IsEmpty)
				{
					yield return POADocumentDatesStatus.NoReceivedDate;
				}
			}
			else
			{
				yield return POADocumentDatesStatus.NoDocumentExists;
			}
		}

		bool IsValidPowerOfAttorneyDocumentDatesFromStatus(IEnumerable<POADocumentDatesStatus> statuses)
		{
			return !statuses.Contains(POADocumentDatesStatus.NoDocumentExists) && !statuses.Contains(POADocumentDatesStatus.Expired);
		}

		protected bool ValidatePowerOfAttorneyDocumentDatesCore(JobRequiredDocument document, ZString documentOwner, out List<KeyValuePair<INotificationType, ZString>> notifications)
		{
			var notificationType = CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType();
			notifications = new List<KeyValuePair<INotificationType, ZString>>();
			var statuses = GetPowerOfAttorneyDocumentDatesStatuses(document);
			foreach (var status in statuses)
			{
				switch (status)
				{
					case POADocumentDatesStatus.NoExpiryDate:
						notifications.Add(new KeyValuePair<INotificationType, ZString>(notificationType, GetExpiryDateRequiredForPeriodicDocumentString(CountrySpecificNameForPOA, documentOwner)));
						break;
					case POADocumentDatesStatus.NoReceivedDate:
						notifications.Add(new KeyValuePair<INotificationType, ZString>(notificationType, GetReceivedDateRequiredString(CountrySpecificNameForPOA, documentOwner)));
						break;
					case POADocumentDatesStatus.Expired:
						notifications.Add(new KeyValuePair<INotificationType, ZString>(notificationType, GetPOAExpiredString(document.EQ_ValidToDate.Date.ToString(), documentOwner, CountrySpecificNameForPOA)));
						break;
					case POADocumentDatesStatus.WillExpireWithinAMonth:
						notifications.Add(new KeyValuePair<INotificationType, ZString>(CargoWise.EntityFramework.NotificationType.Warning, GetPOAWillExpireSoonString(document.EQ_ValidToDate.Date.ToString(), documentOwner, CountrySpecificNameForPOA)));
						break;
				}
			}
			return IsValidPowerOfAttorneyDocumentDatesFromStatus(statuses);
		}

		public static string GetPOANotValidForConditions(string countrySpecificNameForPoa, string objectName, string conditions)
		{
			return Res.GetString("6CDC8A67-9550-4448-9F0D-9081939E9987", "There is a {0} Document on the {1:G}, but it is not valid for {2}.", countrySpecificNameForPoa, objectName, conditions);
		}

		public static string GetNoPOADocumentForImporterString(string countrySpecificNameForPoa)
		{
			return Res.GetString("8fdf41a5-1659-42ad-9348-0799002d08c6", "There is no {0:G} against this organization. Please place your cursor on the organization and press F3 to edit it. The document can be added to the eDocs tab.", countrySpecificNameForPoa);
		}

		public static string GetPOAWillExpireSoonString(string expireDate, string objectName, string countrySpecificNameForPoa)
		{
			return Res.GetString("47eb756a-c33f-4e60-a1d7-47cf1f570582", "The {2} Document on the {0:G} will expire on {1:G}.", objectName, expireDate, countrySpecificNameForPoa);
		}

		public static string GetPOAExpiredString(string expireDate, string objectName, string countrySpecificNameForPoa)
		{
			return Res.GetString("bffa50e9-851b-4393-9a95-fa9e616d7ecf", "The {2} Document on the {0:G} expired on {1:G}.", objectName, expireDate, countrySpecificNameForPoa);
		}

		public static string GetReceivedDateRequiredString(string countrySpecificNameForPoa, string objectName)
		{
			return Res.GetString("a6a14ddc-63e3-4c9c-9b12-0cc144adf86d", "A received date is required for the {1} Document on the {0:G}.", objectName, countrySpecificNameForPoa);
		}

		public static string GetExpiryDateRequiredForPeriodicDocumentString(string countrySpecificNameForPoa, string objectName)
		{
			return Res.GetString("ff7f15fd-139e-4202-8785-88f736cbf27d", "An expiry date is required for the periodic {1} Document on the {0:G}.", objectName, countrySpecificNameForPoa);
		}

		public static string CurrentCountryOrDirectionCondition(string countryCode)
		{
			return Res.GetString("{11111111-34BD-4e95-AFBE-2F5844E3E734}", "{0} and/or this direction", countryCode);
		}
	}
}
