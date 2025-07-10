using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.Business
{
	public static class DpsLog
	{
		public static readonly string LineBreak = new string('-', 300);

		static string PermanentClearUserConfirmMessage => Res.GetString("768DB050-15A8-4978-B81C-0C4B3B5BC214", "The user confirmed that they understood the impact of this by entering 'I understand the Impact'.\r\n\r\nThis record will remain clear unless manually changed.");

		static string GetOverrideDetails(string entityName, string status, string credentialsOverrideDetails, string dateTime)
		{
			var universalMessage = Res.GetString("8E54259A-5AE5-4AC8-B137-C886EF410AEE", @"The Screening Status of {0} was set to {1} by user {2} using override credentials {3} on the {4}.", entityName, status, EnvProxy.Instance.CurrentUser.FullName, credentialsOverrideDetails, dateTime);
			return status == ScreeningStatusesList.Descriptions.PermanentClear ? string.Concat(universalMessage, " ", PermanentClearUserConfirmMessage) : universalMessage;
		}

		public static string GetStatus(string screeningStatus)
		{
			switch (screeningStatus)
			{
				case ScreeningStatusesList.Codes.Clear:
					return DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
				case ScreeningStatusesList.Codes.Matched:
					return DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty;
				case ScreeningStatusesList.Codes.Canceled:
					return DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled;
				case ScreeningStatusesList.Codes.PermanentClear:
					return DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear;
				default:
					return DeniedPartyConstants.LogsScreeningStatus.PotentialMatchesFound;
			}
		}

		public static string GetCountrySanctionsLogStatus(string screeningStatus) => screeningStatus == ScreeningStatusesList.Codes.Matched ? DeniedPartyConstants.LogsScreeningStatus.ScreenedMarkAsSanctioned : GetStatus(screeningStatus);

		public static void AddNew(BusinessObjectFactory factory, BusinessObject screenedEntity, string logStatus, string clearedReason, string highConfidenceResults, string mediumConfidenceResults, int lowConfidenceResultsCount, bool forceReScreen, string credentialOverride, List<DpsSourceWithParties> sourceBizOs)
		{
			Argument.NotNull(sourceBizOs, nameof(sourceBizOs));
			if (!sourceBizOs.Any())
			{
				throw new ArgumentException("Source should not be empty");
			}
			var dpsLog = AddNewCore(factory, screenedEntity, logStatus, clearedReason);
			dpsLog.PJ_IsForcedRescreen = forceReScreen;
			dpsLog.PJ_HighConfidenceResults = highConfidenceResults;
			dpsLog.PJ_MediumConfidenceResults = mediumConfidenceResults;
			dpsLog.PJ_LowConfidenceResultsCount = lowConfidenceResultsCount;
			dpsLog.PJ_MatchingData = string.IsNullOrWhiteSpace(credentialOverride) ?
			GetUserAcceptedMatchesMessage(screenedEntity, logStatus, ZDateTime.UtcNow, highConfidenceResults, mediumConfidenceResults)
			: GetUserAcceptedMatchesMessageWithCredentialsOverride(credentialOverride, screenedEntity, logStatus, ZDateTime.UtcNow, highConfidenceResults, mediumConfidenceResults);

			SetComplianceLists(dpsLog);
			SetSourceDetails(dpsLog, sourceBizOs, screenedEntity);
		}

		public static void AddNewWithDefaultValues(BusinessObjectFactory factory, BusinessObject overridenEntity, string logStatus, string clearedReason)
		{
			var dpsLog = AddNewCore(factory, overridenEntity, logStatus, clearedReason);
			dpsLog.PJ_SourceID = overridenEntity.PK;
			dpsLog.PJ_SourceTableCode = overridenEntity.TablePrefix;
		}

		static StmEntityScreeningLog AddNewCore(BusinessObjectFactory factory, BusinessObject screenedEntity, string logStatus, string clearedReason)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(screenedEntity, nameof(screenedEntity));
			Argument.NotNull(clearedReason, nameof(clearedReason));
			Argument.NotNullOrEmpty(logStatus, nameof(logStatus));

			var dpsLog = factory.New<StmEntityScreeningLog>();
			dpsLog.PJ_Status = logStatus;
			dpsLog.PJ_ParentID = screenedEntity.PK;
			dpsLog.PJ_ParentTableCode = screenedEntity.TablePrefix;
			dpsLog.PJ_ClearedReason = clearedReason;

			return dpsLog;
		}

		public static void AddNew(BusinessObjectFactory factory, string logStatus, IDeniedPartyResultItemV4 deniedResult, List<DpsSourceWithParties> sourceBizOs, bool forceReScreen, string credentialOverride)
		{
			AddNew(factory, deniedResult.ScreenedEntity, logStatus, deniedResult.FullClearingReason, deniedResult.HighConfidenceResults, deniedResult.MediumConfidenceResults, deniedResult.LowConfidenceResultsCount, forceReScreen, credentialOverride, sourceBizOs);
			DeleteDuplicateCompanyData(factory, deniedResult.ScreenedEntity.PK);
		}

		public static void SaveWithExceptionHandler(BusinessObjectFactory factory)
		{
			try
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, false, true);
			}
			catch (Exception ex) when (ex is ZSaveConcurrencyException || ex is ZCannotSaveException || ex is ZSaveException)
			{
				if (ex is ZSaveConcurrencyException)
				{
					var rejectConcurrencyMergeException = ex.Data.Values.OfType<CannotSaveAfterCriticalErrorException>().FirstOrDefault();
					if (rejectConcurrencyMergeException != null)
					{
						throw rejectConcurrencyMergeException;
					}
				}

				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		static void DeleteDuplicateCompanyData(BusinessObjectFactory factory, ZGuid headerPk)
		{
			var query = new ZQuery(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(OrgCompanyDataSchema.OB_OH, headerPk);
			query.FetchOnlyFromLocalCache = false;

			var results = factory.Load<OrgCompanyData>(query);

			if (results.Any(x => x.IsInDatabase))
			{
				results.Where(x => !x.IsInDatabase).ForEach(x => x.Delete());
			}
			else
			{
				results.Skip(1).ForEach(x => x.Delete());
			}
		}

		static string GetUserAcceptedMatchesMessageWithCredentialsOverride(string credentialsOverrideDetails, BusinessObject screenedEntity, string logStatus, ZDateTime logDatetime, string highConfidenceResults, string mediumConfidenceResults)
		{
			var message = string.Empty;

			if (logStatus == DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear && screenedEntity is OrgHeader header)
			{
				message = GetOverrideDetails(header.OH_FullName, ScreeningStatusesList.Descriptions.PermanentClear, credentialsOverrideDetails, logDatetime.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.CurrentCulture));
			}
			else
			{
				string screenedEntityFullName;
				if (screenedEntity is OrgHeader org)
				{
					screenedEntityFullName = org.OH_FullName;
				}
				else if (screenedEntity is RefVessel vessel)
				{
					screenedEntityFullName = vessel.RV_Name;
				}
				else
				{
					screenedEntityFullName = screenedEntity.HumanReadableName;
				}

				if (logStatus == DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty)
				{
					var matchedMessage = new StringBuilder();

					matchedMessage.AppendLine(GetOverrideDetails(screenedEntityFullName, ScreeningStatusesList.Descriptions.Matched, credentialsOverrideDetails, logDatetime.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.CurrentCulture)));
					message = MatchedPartiesMessage(highConfidenceResults, mediumConfidenceResults, matchedMessage);
				}
				else if (logStatus == DeniedPartyConstants.LogsScreeningStatus.ScreenedClear)
				{
					message = GetOverrideDetails(screenedEntityFullName, ScreeningStatusesList.Descriptions.Clear, credentialsOverrideDetails, logDatetime.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.CurrentCulture));
				}
			}

			return message;
		}

		static string GetUserAcceptedMatchesMessage(BusinessObject screenedEntity, string logStatus, ZDateTime logDatetime, string highConfidenceResults, string mediumConfidenceResults)
		{
			var message = ZString.Empty;

			if (logStatus == DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear && screenedEntity is OrgHeader header)
			{
				message = string.Concat(Res.GetString("59B62791-4FEE-4456-AE7C-7BA2AB972650", @"The Screening Status of {0} was set to Permanent Clear by user {1} on the {2}.", header.OH_FullName, EnvProxy.Instance.CurrentUser.FullName, logDatetime.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.CurrentCulture)), " ", PermanentClearUserConfirmMessage);
			}
			else if (logStatus == DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty || logStatus == DeniedPartyConstants.LogsScreeningStatus.ScreenedMarkAsSanctioned)
			{
				var matchedMessage = new StringBuilder();
				message = MatchedPartiesMessage(highConfidenceResults, mediumConfidenceResults, matchedMessage);
			}

			return message.ToString();
		}

		static string MatchedPartiesMessage(string highConfidenceResults, string mediumConfidenceResults, StringBuilder matchedMessage)
		{
			if (!string.IsNullOrEmpty(highConfidenceResults))
			{
				matchedMessage.AppendLine(Res.GetString("9366FC83-A842-444C-854A-75C022F720DC", "High Confidence Result:"));
				matchedMessage.AppendLine(highConfidenceResults);
			}

			if (!string.IsNullOrEmpty(mediumConfidenceResults))
			{
				if (!string.IsNullOrEmpty(highConfidenceResults))
				{
					matchedMessage.AppendLine(LineBreak);
				}

				matchedMessage.AppendLine(Res.GetString("838ED0B1-3ECF-4051-B8E6-290C349A5B38", "Medium Confidence Result:"));
				matchedMessage.Append(mediumConfidenceResults);
			}

			return matchedMessage.ToString();
		}

		static void SetSourceDetails(StmEntityScreeningLog dpsLog, List<DpsSourceWithParties> sourceBizOs, BusinessObject screenedEntity)
		{
			var bizO = sourceBizOs[0].SourceBizO;

			if (sourceBizOs.Count == 1 && (bizO.TableName == JobShipmentSchema.Constants.TableName || bizO.TableName == JobDeclarationSchema.Constants.TableName || bizO.TableName == JobConsolSchema.Constants.TableName || bizO.TableName == WhsDocketSchema.Constants.TableName))
			{
				dpsLog.PJ_SourceID = bizO.PK;
				dpsLog.PJ_SourceTableCode = bizO.TablePrefix;
			}
			else
			{
				dpsLog.PJ_SourceID = screenedEntity.PK;
				dpsLog.PJ_SourceTableCode = screenedEntity.TablePrefix;
			}
		}

		static void SetComplianceLists(StmEntityScreeningLog dpsLog)
		{
			var compListItems = DpsComplianceListHelper.GetActiveIncludedAndExcludedComplianceListItems(dpsLog.Factory);
			var activeIncludedItems = compListItems.ActiveIncludedList;
			var activeExcludedItems = compListItems.ActiveExcludedList;
			var (includedLists, excludedLists) = GetComplianceLists(activeIncludedItems.Length, activeIncludedItems, activeExcludedItems.Length, activeExcludedItems);

			dpsLog.PJ_ExcludedLists = excludedLists;
			dpsLog.PJ_IncludedLists = includedLists;
		}

		static (string includedLists, string excludedLists) GetComplianceLists(int includedCount, DpsComplianceListItem[] includedItems, int excludedCount, DpsComplianceListItem[] excludedItems)
		{
			var includedLists = new StringBuilder();
			includedLists.Append(Res.GetString("5d456039-88fe-4c4a-8a9d-9d26771dd446", "{0} INCLUDED", includedCount));

			if (includedCount > 0)
			{
				includedLists = AppendComplianceSourceLists(includedLists, includedItems);
			}

			var excludedLists = new StringBuilder();
			excludedLists.Append(Res.GetString("a4a8e031-3349-4d8e-b8d0-b688fc95ae37", "{0} EXCLUDED", excludedCount));

			if (excludedCount > 0)
			{
				excludedLists = AppendComplianceSourceLists(excludedLists, excludedItems);
			}

			return (includedLists.ToString(), excludedLists.ToString());
		}

		static StringBuilder AppendComplianceSourceLists(StringBuilder sourcedList, DpsComplianceListItem[] complianceItems)
		{
			sourcedList.AppendLine();
			sourcedList.AppendLine(LineBreak);
			complianceItems.ForEach(u => AppendComplianceMessageLists(sourcedList, u));
			return sourcedList;
		}

		static void AppendComplianceMessageLists(StringBuilder sourcedList, DpsComplianceListItem compListItem)
		{
			sourcedList.AppendLine();
			if (compListItem.Name.IsEmpty)
			{
				sourcedList.Append(compListItem.Code);
			}
			else
			{
				sourcedList.Append(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", compListItem.Code, compListItem.Name));
			}
		}
	}
}
