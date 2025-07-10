using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message for ErrorReporter")]
	sealed class CommonContainerUniqueContainerNumIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public CommonContainerUniqueContainerNumIndexFailureHandler(CommonContainer container)
		{
			this.container = container;
		}

		readonly CommonContainer container;

		IEnumerable<string> IUniqueIndexFailureHandler.HandledUniqueIndexNames
		{
			get { yield return JobContainerSchema.Constants.Indexes.NR_UX__JC_JK_JC_ContainerNum; }
		}

		void IUniqueIndexFailureHandler.NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			ReportSelfConcurrencyIfApplicable();
			var errorMsg = Res.GetString("13173ced-4e3d-4f91-b7e0-fd8c71933252", "While you have been working, another user has added a container with the same container number. Please close and re-open the form to get the latest changes.");
			var caption = Res.GetString("8afec2f3-77ae-44f8-91d7-3ae49c76632c", "Error Saving Container {0}", container.JC_ContainerNum);
			notifier.ReportError(errorMsg, caption);
		}

		void ReportSelfConcurrencyIfApplicable()
		{
			if (container == null)
			{
				return;
			}

			var containerNumber = container.JC_ContainerNum;
			var jc_JK = container.JC_JK;

			var query = new ZQuery();
			query.AddToFilter(JobContainerSchema.JC_JK, jc_JK);
			query.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
			query.FetchOnlyFromLocalCache = true;

			var factory = container.Factory;
			var containersWithDuplicatedNumbers = factory.Load<CommonContainer>(query);

			var doWeHaveTwoOrMoreContainersPendingSave = containersWithDuplicatedNumbers
				.Where(c => !c.IsInDatabase)
				.Take(2)
				.Count() == 2;

			if (doWeHaveTwoOrMoreContainersPendingSave)
			{
				SendErrorReport(containersWithDuplicatedNumbers,
					containerNumber,
					(NoResString)"There's 2 or more containers with the same number pending save");
				return;
			}

			ReportCustomsSelfConcurrencyIfApplicable(containersWithDuplicatedNumbers, factory, containerNumber, jc_JK);
		}

		void ReportCustomsSelfConcurrencyIfApplicable(CommonContainer[] containersWithDuplicatedNumbers, BusinessObjectFactory factory,
			ZString containerNumber, ZGuid jc_JK)
		{
			if (!((IUser)GlbStaff.CurrentUser).IsBatchProcessor)
			{
				return;
			}

			var anyOfTheContainersWasCreatedByCustoms = containersWithDuplicatedNumbers
				.Any(c => c.CreatedFromCusContainer);
			if (!anyOfTheContainersWasCreatedByCustoms)
			{
				return;
			}

			var changeSet = factory.GetChanges();
			var objectChanges = changeSet.GetChangedObjects();

			var extraMessage = new ZStringBuilder();
			var anyOfTheContainersWasDeleted = false;

			foreach (var objectChange in objectChanges)
			{
				var sessionInstance = objectChange.SessionInstance;
				if (sessionInstance is CommonContainer jobContainer)
				{
					extraMessage.AppendLine($"The container changed in current Factory: {objectChange.DisplayName}|IsExistsInDatabase: {objectChange.IsExistsInDatabase}|IsModifiedInDatabase: {objectChange.IsModifiedInDatabase}|Deleted: {sessionInstance.IsDeleted}|LastModified - {objectChange.LastModified}");
					if (sessionInstance.IsDeleted && objectChange.IsExistsInDatabase)
					{
						var containerInDb = objectChange.DatabaseInstance as CommonContainer;
						if (containerInDb != null && containerInDb.JC_ContainerNum == containerNumber)
						{
							anyOfTheContainersWasDeleted = true;
							extraMessage.AppendLine($"The container in DB is deleting. PK: {containerInDb.PK}|JC_ContainerNum: {containerInDb.JC_ContainerNum}|JC_ContainerJobID: {containerInDb.JC_ContainerJobID}|Create User: {containerInDb.JC_SystemCreateUser}|Creation Time (UTC): {containerInDb.JC_SystemCreateTimeUtc.ToISO8601String()}|Last Edit User: {containerInDb.JC_SystemLastEditUser}|Last Edit Time (UTC): {containerInDb.JC_SystemLastEditTimeUtc.ToISO8601String()}");
							if (jobContainer.DeletionLoggingDetails != null)
							{
								extraMessage.AppendLine("    Please consider, can we delete a JobContainer when there're CusContainers linked to it? If we can, then we have an issue on deleting an old JobContainer and creating a new one.");
								extraMessage.AppendLine("    Please see unit test TestClearHasChangesShouldNotHappenWhenContainerIsDeleted_CS00135077 and comment on https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/27964?path=%2FEnterprise%2FProduct%2FOperations%2FCustoms%2FShared%2FBusiness%2FSynchroniser%2FPackingSynchroniser.cs&discussionId=141450&iteration=9&_a=files");
								extraMessage.AppendLine($"    Deletion Logging Details: {jobContainer.DeletionLoggingDetails}");
							}
						}
					}
					extraMessage.AppendLine();
				}
				if (sessionInstance is Enterprise.Integration.Customs.Shared.IBaseCusContainer cusContainer && cusContainer.CO_ContainerNumber == containerNumber && objectChange.DatabaseInstance is Enterprise.Integration.Customs.Shared.IBaseCusContainer cusContainerInDb)
				{
					extraMessage.AppendLine($"The CusContainer changed in current Factory. PK: {cusContainer.PK}|CO_ContainerNumber: {cusContainer.CO_ContainerNumber}|CO_JC: {cusContainer.CO_JC}|Original CO_JC: {cusContainerInDb.CO_JC}|IsExistsInDatabase: {objectChange.IsExistsInDatabase}|IsModifiedInDatabase: {objectChange.IsModifiedInDatabase}|Deleted: {sessionInstance.IsDeleted}|LastModified - {objectChange.LastModified}");
					extraMessage.AppendLine();
				}
			}

			if (anyOfTheContainersWasDeleted || containersWithDuplicatedNumbers.Length > 1)
			{
				var newFactory = new BusinessObjectFactory();
				var dbOnlyQuery = new ZDBOnlyQuery(typeof(CommonContainer));
				dbOnlyQuery.AddToFilter(JobContainerSchema.JC_JK, jc_JK);
				dbOnlyQuery.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
				var containerInDbNew = newFactory.LoadTop1<CommonContainer>(dbOnlyQuery);
				if (containerInDbNew != null)
				{
					extraMessage.AppendLine($"The container in DB (loaded by new factory). PK: {containerInDbNew.PK}|JC_ContainerNum: {containerInDbNew.JC_ContainerNum}|JC_ContainerJobID: {containerInDbNew.JC_ContainerJobID}|Create User: {containerInDbNew.JC_SystemCreateUser}|Creation Time (UTC): {containerInDbNew.JC_SystemCreateTimeUtc.ToISO8601String()}|Last Edit User: {containerInDbNew.JC_SystemLastEditUser}|Last Edit Time (UTC): {containerInDbNew.JC_SystemLastEditTimeUtc.ToISO8601String()}");
				}

				var subErrorReportKey = anyOfTheContainersWasDeleted ? "Deleted Container(s) - V3" : default;
				SendErrorReport(containersWithDuplicatedNumbers,
					containerNumber,
					"There's 2 or more containers with the same number and at least one of them was created by Customs",
					extraMessage.ToString(),
					subErrorReportKey);
			}
		}

		void SendErrorReport(CommonContainer[] containersWithDuplicatedNumbers,
			string containerNumber,
			string reasonForSendingErrorReport,
			string extraMessage = "",
			string subKey = default)
		{
			var containerInfos = containersWithDuplicatedNumbers
				.Select(c => $"PK: {c.PK}|JC_ContainerNum: {containerNumber}|IsInDatabase: {c.IsInDatabase}|Deleted: {c.IsDeleted}|CreatedFromCusContainer: {c.CreatedFromCusContainer}|Creation Time (UTC): {(c.IsInDatabase ? c.JC_SystemCreateTimeUtc.ToISO8601String() : c.CreationDateTimeUtc.ToISO8601String())}|Creation Stack Trace:{c.CreationStackTrace}");

			var message = $"{reasonForSendingErrorReport}, which caused possible self concurrency of NR_UX__JC_JK_JC_ContainerNum\r\n{string.Join("\r\n", containerInfos)}\r\n{extraMessage}";
			var reportKey = "NR_UX__JC_JK_JC_ContainerNum Self Concurrency";
			if (!string.IsNullOrEmpty(subKey))
			{
				reportKey += ": " + subKey;
			}
			ErrorReporter.ReportOnce(reportKey, message);
		}
	}
}
