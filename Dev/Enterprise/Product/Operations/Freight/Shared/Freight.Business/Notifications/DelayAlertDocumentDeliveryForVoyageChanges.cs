using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Business
{
	public sealed class DelayAlertDocumentDelivery
	{
		#region DeliverForVoyageChanges

		public void DeliverForVoyageChanges(JobVoyage voyage)
		{
			JobScheduleChange[] scheduleChanges = new JobScheduleChangeLogger().LogVoyageDateChanges(voyage);
			if (FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.Value.IsEnabledFor(voyage.JV_AirSeaRoad) && scheduleChanges.Length > 0)
			{
				JobSailingRelatedJob[] jobsThatRequireImportDelayAlertDelivery = LoadJobsThatRequireImportDelayAlertDelivery(scheduleChanges);
				JobSailingRelatedJob[] jobsThatRequireExportDelayAlertDelivery = LoadJobsThatRequireExportDelayAlertDelivery(scheduleChanges);

				bool hasJobsThatRequireImportDelayAlertDelivery = jobsThatRequireImportDelayAlertDelivery.Length > 0;
				bool hasJobsThatRequireExportDelayAlertDelivery = jobsThatRequireExportDelayAlertDelivery.Length > 0;
				if (hasJobsThatRequireImportDelayAlertDelivery || hasJobsThatRequireExportDelayAlertDelivery)
				{
					var provider = ScheduleUpdateQueryProviderFactory.Get(voyage.Factory);

					if (provider.ShouldSendDelayAlerts(hasJobsThatRequireImportDelayAlertDelivery, hasJobsThatRequireExportDelayAlertDelivery))
					{
						Deliver(jobsThatRequireImportDelayAlertDelivery, false);
						Deliver(jobsThatRequireExportDelayAlertDelivery, true);
					}
					else
					{
						CreateDelayAlertDeliveryPreventedByUserLog(voyage);
						CreateDelayAlertDeliveryPreventedByUserLogs(jobsThatRequireImportDelayAlertDelivery);
						CreateDelayAlertDeliveryPreventedByUserLogs(jobsThatRequireExportDelayAlertDelivery);
					}
				}
			}
		}

		void Deliver(JobSailingRelatedJob[] jobs, bool isExportDA)
		{
			foreach (JobSailingRelatedJob job in jobs)
			{
				NotificationBuffer deliveryNotifications = new NotificationBuffer();
				DelayAlertDocumentDeliveryJob deliveryJob = new DelayAlertDocumentDeliveryJob((IDocumentSupportable)job.Job, isExportDA);

				ZDateTime deliveryDate = ZDateTime.UtcNow;
				deliveryJob.Deliver(deliveryNotifications);

				if (!deliveryNotifications.HasErrors && !ScheduleChangeEmailSender.DocumentDeliveredLogExists(job.Job, deliveryDate))
				{
					job.Job.GetLogs().AddNew(Events.DocumentNotDelivered,
						"Delay Alert delivery attempted but failed",
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, DelayAlertDocumentName),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, Constants.DocumentNotDeliveredReasons.Codes.Failed));
				}

				if (deliveryNotifications.HasErrors)
				{
					job.Job.GetLogs().AddNew(Events.DocumentNotDelivered,
						"Insufficient delay alert delivery information: " + deliveryNotifications.Events[0].Message,
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, DelayAlertDocumentName),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, Constants.DocumentNotDeliveredReasons.Codes.Failed));
				}
				else
				{
					CheckExcessiveDelayAlert(deliveryJob);
				}
			}
		}

		void CreateDelayAlertDeliveryPreventedByUserLogs(JobSailingRelatedJob[] jobsThatRequireDelayAlertDelivery)
		{
			foreach (JobSailingRelatedJob job in jobsThatRequireDelayAlertDelivery)
			{
				CreateDelayAlertDeliveryPreventedByUserLog(job.Job);
			}
		}

		void CreateDelayAlertDeliveryPreventedByUserLog(BusinessObject businessObject)
		{
			businessObject.GetLogs().AddNew(Events.DocumentNotDelivered,
				"Auto-delivery of Delay Alert document was prevented by the user",
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, DelayAlertDocumentName),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, Constants.DocumentNotDeliveredReasons.Codes.Cancelled));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
		const string DelayAlertDocumentName = "Delay Alert";

		#endregion

		#region LoadJobsThatRequireDelayAlertDelivery

		public JobSailingRelatedJob[] LoadJobsThatRequireImportDelayAlertDelivery(JobScheduleChange[] scheduleChanges)
		{
			const string Import = DelayAlertDeliveryDirections.Codes.Import;

			JobSailingRelatedJob[] result = null;
			JobScheduleChange[] scheduleETAChanges = GetETAChanges(scheduleChanges);

			if (scheduleETAChanges.Length > 0 && FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.Value.IsEnabledForAny())
			{
				DelayAlertDeliveryRuleCollection collection = FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.Value;
				List<JobSailingRelatedJob> list = new List<JobSailingRelatedJob>();

				foreach (KeyValuePair<string, List<JobScheduleChange>> pair in GroupByTransportMode(scheduleETAChanges))
				{
					if (collection.ShouldDeliverDelayAlert(pair.Key, DelayAlertDeliveryModules.Codes.Forwarding, Import))
					{
						list.AddRange(RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(scheduleETAChanges));
					}

					if (collection.ShouldDeliverDelayAlert(pair.Key, DelayAlertDeliveryModules.Codes.Customs, Import))
					{
						list.AddRange(RelatedSailingJobs.LoadRelatedDeclarationJobs(scheduleETAChanges));
					}

					if (collection.ShouldDeliverDelayAlert(pair.Key, DelayAlertDeliveryModules.Codes.ShippingManager, Import))
					{
						list.AddRange(RelatedSailingJobs.LoadRelatedAgencyDocumentationJobs(scheduleETAChanges));
					}
				}

				result = Array.FindAll(list.ToArray(), ShouldDeliverImportDelayAlertDocumentForETAChange);
			}

			return result ?? Array.Empty<JobSailingRelatedJob>();
		}

		public JobSailingRelatedJob[] LoadJobsThatRequireExportDelayAlertDelivery(JobScheduleChange[] scheduleChanges)
		{
			const string Export = DelayAlertDeliveryDirections.Codes.Export;

			JobSailingRelatedJob[] result = Array.Empty<JobSailingRelatedJob>();
			JobScheduleChange[] scheduleETDChanges = GetETDChanges(scheduleChanges);

			if (scheduleETDChanges.Length > 0 && FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.Value.IsEnabledForAny())
			{
				DelayAlertDeliveryRuleCollection collection = FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.Value;
				List<JobSailingRelatedJob> list = new List<JobSailingRelatedJob>();

				foreach (KeyValuePair<string, List<JobScheduleChange>> pair in GroupByTransportMode(scheduleETDChanges))
				{
					if (collection.ShouldDeliverDelayAlert(pair.Key, DelayAlertDeliveryModules.Codes.Forwarding, Export))
					{
						list.AddRange(RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(scheduleETDChanges));
					}

					if (collection.ShouldDeliverDelayAlert(pair.Key, DelayAlertDeliveryModules.Codes.Customs, Export))
					{
						list.AddRange(RelatedSailingJobs.LoadRelatedDeclarationJobs(scheduleETDChanges));
					}

					if (collection.ShouldDeliverDelayAlert(pair.Key, DelayAlertDeliveryModules.Codes.ShippingManager, Export))
					{
						list.AddRange(RelatedSailingJobs.LoadRelatedAgencyDocumentationJobs(scheduleETDChanges));
						list.AddRange(RelatedSailingJobs.LoadRelatedAgencyBookingJobs(scheduleETDChanges));
					}
				}

				result = Array.FindAll(list.ToArray(), ShouldDeliverExportDelayAlertDocumentForETDChange);
			}

			return result;
		}

		bool ShouldDeliverImportDelayAlertDocumentForETAChange(JobSailingRelatedJob job)
		{
			bool isJobDelayAlertable =
				ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>().IsInstanceOfType(job.Job) ||
				(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>().IsInstanceOfType(job.Job) && ((ZGuid)job.Job[JobDeclarationSchema.JE_JS]).IsEmpty) ||
				ObjectFactory.GetType<Integration.Agency.IBillOfLading>().IsInstanceOfType(job.Job);

			return
				isJobDelayAlertable &&
				!((IImportExport)job.Job).IsExport() &&
				new DelayAlertDocumentDeliveryJob((IDocumentSupportable)job.Job, false).HasRecipients;
		}

		bool ShouldDeliverExportDelayAlertDocumentForETDChange(JobSailingRelatedJob job)
		{
			bool isJobDelayAlertable =
				ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>().IsInstanceOfType(job.Job) ||
				(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>().IsInstanceOfType(job.Job) && ((ZGuid)job.Job[JobDeclarationSchema.JE_JS]).IsEmpty) ||
				ObjectFactory.GetType<Integration.Agency.IBillOfLading>().IsInstanceOfType(job.Job) ||
				ObjectFactory.GetType<Integration.Agency.IAgencyBooking>().IsInstanceOfType(job.Job);

			return
				isJobDelayAlertable &&
				!((IImportExport)job.Job).IsImport() &&
				new DelayAlertDocumentDeliveryJob((IDocumentSupportable)job.Job, true).HasRecipients;
		}

		JobScheduleChange[] GetETAChanges(JobScheduleChange[] scheduleChanges)
		{
			List<JobScheduleChange> result = new List<JobScheduleChange>();
			foreach (JobScheduleChange scheduleChange in scheduleChanges)
			{
				if (scheduleChange.E7_DateType == ScheduleDateTypes.Codes.ETA &&
					scheduleChange.E7_PreviousValue.IsValid &&
					scheduleChange.E7_UpdatedValue.IsValid &&
					scheduleChange.E7_PreviousValue < scheduleChange.E7_UpdatedValue)
				{
					result.Add(scheduleChange);
				}
			}

			return result.ToArray();
		}

		JobScheduleChange[] GetETDChanges(JobScheduleChange[] scheduleChanges)
		{
			List<JobScheduleChange> result = new List<JobScheduleChange>();

			foreach (JobScheduleChange scheduleChange in scheduleChanges)
			{
				if (scheduleChange.E7_DateType == ScheduleDateTypes.Codes.ETD &&
					scheduleChange.E7_PreviousValue.IsValid &&
					scheduleChange.E7_UpdatedValue.IsValid &&
					scheduleChange.E7_PreviousValue < scheduleChange.E7_UpdatedValue)
				{
					result.Add(scheduleChange);
				}
			}

			return result.ToArray();
		}

		IDictionary<string, List<JobScheduleChange>> GroupByTransportMode(JobScheduleChange[] changes)
		{
			IDictionary<string, List<JobScheduleChange>> result = new SortedDictionary<string, List<JobScheduleChange>>();

			foreach (JobScheduleChange change in changes)
			{
				IScheduleChangeParent parent;
				JobVoyage voyage;
				string mode;

				if ((parent = change.Parent) == null || (voyage = parent.Voyage) == null)
				{
					mode = "";
				}
				else
				{
					mode = voyage.JV_AirSeaRoad;
				}

				List<JobScheduleChange> list;

				if (!result.TryGetValue(mode, out list))
				{
					list = new List<JobScheduleChange>();
					result.Add(mode, list);
				}

				list.Add(change);
			}

			return result;
		}

		RelatedSailingJobs RelatedSailingJobs
		{
			get
			{
				if (relatedSailingJobs == null)
				{
					relatedSailingJobs = new RelatedSailingJobs();
				}
				return relatedSailingJobs;
			}
		}
		RelatedSailingJobs relatedSailingJobs;

		#endregion

		#region CS00985785 - Excessive DelayAlertDocumentDeliveryJob

		const int ExcessiveDelayAlertTimeRangeInMinute = 5;
		int ExcessiveDelayAlertInTimeRangeThreshold => Globals.IsTest ? 3 : 100;

		void CheckExcessiveDelayAlert(DelayAlertDocumentDeliveryJob job)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddMinutes(-ExcessiveDelayAlertTimeRangeInMinute));
			query.AddToFilter(StmPrintJobSchema.SP_JobType, SQLComparisonOperator.Equal, PrintJobType.EML);
			query.AddToFilter(StmPrintJobSchema.SP_ParentGuid, SQLComparisonOperator.Equal, job.BusinessObject.PK);

			var relatedPrintJobs = job.Factory.Load<StmPrintJob>(query);
			var emailSubject = relatedPrintJobs.LastOrDefault()?.SP_EmailSubjectLine ?? ZString.Empty;
			if (relatedPrintJobs.Length >= ExcessiveDelayAlertInTimeRangeThreshold && relatedPrintJobs.All(x => x.SP_EmailSubjectLine.Equals(emailSubject)))
			{
				ErrorReporter.ReportOnce("CS00985785 - Excessive DelayAlertDocumentDeliveryJob",
					$"Excessive DelayAlertDocumentDeliveryJob related to BizObj: {job.BusinessObject.TableName} - {job.BusinessObject.PK}.\nStackTrace: {new System.Diagnostics.StackTrace(true)}");
			}
		}

		#endregion
	}
}
