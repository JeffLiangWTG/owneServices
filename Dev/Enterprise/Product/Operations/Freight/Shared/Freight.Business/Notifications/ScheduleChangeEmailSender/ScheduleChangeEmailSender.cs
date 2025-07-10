using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Business
{
	[Serializable]
	abstract partial class ScheduleChangeEmailSender
	{
		public static ScheduleChangeEmailSender New(string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return new AirScheduleChangeEmailSender();
				case Core.Constants.TransportModes.Road:
					return new RoadScheduleChangeEmailSender();
				case Core.Constants.TransportModes.Rail:
					return new RailScheduleChangeEmailSender();
				default:
					return new SeaScheduleChangeEmailSender();
			}
		}

		#region Transport Mode Specific

		protected abstract string TransportMode { get; }
		protected abstract string TransportModeDescription { get; }
		protected abstract GuidRegistryItem NotificationGroup { get; }
		protected abstract string VoyageLabel { get; }
		protected virtual string VesselNameLabel { get { return null; } }
		protected virtual string CarrierLabel { get { return Res.GetString("3243a78e-917e-4f02-906f-f124ec3de1e4", "Carrier"); } }

		#endregion

		#region Email

		public virtual JobScheduleChange[] SendEmailIfRequired(BusinessObjectFactory factory, ZDateTime fromDate, ZDateTime toDate, INotifications notifications, int batchSize = 0)
		{
			JobScheduleChange[] scheduleChanges = LoadScheduleDateChanges(factory, fromDate, toDate, batchSize);
			if (scheduleChanges.Length > 0)
			{
				EmailDef email = GenerateEmail(scheduleChanges, fromDate, toDate);
				try
				{
					Env.OutgoingMailManager.Create(factory, email, NotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(NotificationGroup));
					notifications.Notify(new InfoNotification(Res.GetString("d2cd6620-f354-4d00-8a1e-38eddb2d2423", "Sent {0} schedule change notification email.", TransportModeDescription)));
				}
				catch (EmailSendFailedException) { } // configuration problem
			}

			return scheduleChanges;
		}

		EmailDef GenerateEmail(JobScheduleChange[] scheduleChanges, ZDateTime fromDate, ZDateTime toDate)
		{
			StringWriter body = new StringWriter();
			WriteEmailBody(body, scheduleChanges, fromDate, toDate);

			EmailDef email = new EmailDef();
			email.ContentType = EmailContentTypes.HTML;
			email.Body = body.GetStringBuilder().ToString();
			email.Subject = Res.GetString("9f9bd67a-37b7-438d-9263-2a23ae6e65fb", "{0} Schedules have changed", TransportModeDescription);

			AddImageAttachment(email, "Banner.jpg", SystemDataRegistry.Instance.HtmlEmailBannerImage.Value ?? new Bitmap(1, 1));
			AddImageAttachment(email, "Footer.jpg", SystemDataRegistry.Instance.HtmlEmailFooterImage.Value ?? new Bitmap(1, 1));
			email.Attachments.Add(new AttachmentDef("HowToUnsubscribe.txt", System.Text.Encoding.ASCII.GetBytes(HowToUnsubscribeText)));

			return email;
		}

		static string HowToUnsubscribeText
		{
			get
			{
				return
Res.GetString("6f0534a5-0176-4d76-908b-a8c467a654bf", @"Unless otherwise configured, this email is delivered to all staff members.
You can configure this email to be delivered to a predefined group of users in the {2} registry.

Go to {0}->{1}->Registry to change the registry item at:", ModuleTreeLoaderConstant.Category.Admin.DisplayText, ModuleTreeLoaderConstant.Section.System.DisplayText, Core.Constants.ProductName) + @"
" + FreightDataRegistry.Instance.AirScheduleChangeNotificationGroup.Category.Replace("/", "->");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "HTML Placeholder")]
		void WriteEmailBody(TextWriter writer, JobScheduleChange[] scheduleChanges, ZDateTime fromDate, ZDateTime toDate)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			string leadingContent = GetEmailTemplateFragment(null, (NoResString)"<!--StartSection ChangeDetails-->");
			leadingContent = leadingContent.Replace("(*HtmlStyleSheet*)", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			leadingContent = leadingContent.Replace("(*TransportModeDescription*)", TransportModeDescription);
			leadingContent = leadingContent.Replace("(*ChangesFrom*)", FormatUtc(fromDate) ?? "(first time run)");
			leadingContent = leadingContent.Replace("(*ChangesTo*)", FormatUtc(toDate));
			leadingContent = leadingContent.Replace("(*AllDelayAlertsDeliveryStatus*)", GetDelayAlertDeliveryStatusForAllJobs(scheduleChanges, fromDate));
			string trailingContent = GetEmailTemplateFragment((NoResString)"<!--EndSection ChangeDetails-->", null);

			writer.Write(leadingContent);
			WriteVoyageChangeDetails(writer, factory, scheduleChanges, fromDate);
			writer.Write(trailingContent);
		}

		void AddImageAttachment(EmailDef email, string displayName, Image image)
		{
			MemoryStream imageStream = new MemoryStream();
			image.Save(imageStream, ImageFormat.Jpeg);

			AttachmentDef attachment = new AttachmentDef(displayName, imageStream.ToArray());
			email.Attachments.Add(attachment);
		}

		#endregion

		#region Schedule Changes

		JobScheduleChange[] LoadScheduleDateChanges(BusinessObjectFactory factory, ZDateTime fromDate, ZDateTime toDate, int batchSize)
		{
			ZQuery query = GetScheduleDateChangeQuery(fromDate, toDate, batchSize);
			JobScheduleChange[] result = factory.Load<JobScheduleChange>(query);
			result = Array.FindAll(result, delegate(JobScheduleChange scheduleChange)
			{
				return !(scheduleChange.Voyage == null) && scheduleChange.Voyage.JV_AirSeaRoad == TransportMode;
			});

			return result;
		}

		ZQuery GetScheduleDateChangeQuery(ZDateTime fromDate, ZDateTime toDate, int batchSize)
		{
			ZQuery query = new ZQuery();
			if (fromDate.IsValid)
			{
				query.AddToFilter(JobScheduleChangeSchema.E7_ChangedAt, SQLComparisonOperator.GreaterThanOrEqualTo, fromDate);
			}

			if (toDate.IsValid)
			{
				query.AddToFilter(JobScheduleChangeSchema.E7_ChangedAt, SQLComparisonOperator.LessThan, toDate);
			}

			query.OrderBy = JobScheduleChangeSchema.E7_GS_NKChangedBy.Name + ", " +
				JobScheduleChangeSchema.E7_ChangedAt.Name + ", " +
				JobScheduleChangeSchema.E7_UpdatedValue.Name;

			if (batchSize > 0)
			{
				query.MaximumRows = batchSize;
			}

			return query;
		}

		#endregion

		#region Voyage Details

		void WriteVoyageChangeDetails(TextWriter writer, BusinessObjectFactory factory, JobScheduleChange[] scheduleChanges, ZDateTime fromDate)
		{
			var groupedScheduleChanges = GroupByChangedByAndVesselVoyageCarrier(factory, scheduleChanges);
			foreach (var vesselVoyageCarrierGroup in groupedScheduleChanges)
			{
				var changedByAndVesselVoyageCarrier = vesselVoyageCarrierGroup.Key;
				var scheduleChangesForCurrentGroup = vesselVoyageCarrierGroup.Value.ToArray();

				WriteChangedByAndVesselVoyageContent(writer, changedByAndVesselVoyageCarrier);
				WriteDatesAffected(writer, scheduleChangesForCurrentGroup, changedByAndVesselVoyageCarrier.HasDataProvider);
				WriteJobsAffected(writer, factory, scheduleChangesForCurrentGroup, fromDate);
			}
		}

		void WriteChangedByAndVesselVoyageContent(TextWriter writer, ChangedByAndVesselVoyageCarrier scheduleChange)
		{
			string content = GetEmailTemplateFragment((NoResString)"<!--StartSection UserVesselVoyageCarrier-->", (NoResString)"<!--StartSection VesselName-->");
			if (VesselNameLabel != null)
			{
				content += GetEmailTemplateFragment((NoResString)"<!--StartSection VesselName-->", (NoResString)"<!--EndSection VesselName-->");
			}

			content += GetEmailTemplateFragment((NoResString)"<!--StartSection Voyage-->", (NoResString)"<!--EndSection Voyage-->");
			if (!scheduleChange.Carrier.IsEmpty)
			{
				content += GetEmailTemplateFragment((NoResString)"<!--StartSection Carrier-->", (NoResString)"<!--EndSection Carrier-->");
			}

			string changedBy = (scheduleChange.HasDataProvider)
				? Res.GetString("76e524e5-9ed8-43ac-a631-14fd4364605a", "Automated {0} Schedule System", TransportModeDescription)
				: (string)scheduleChange.ChangedBy.GS_FullName;

			content = content.Replace("(*ChangedBy*)", changedBy);
			content = content.Replace("(*VesselNameLabel*)", VesselNameLabel);
			content = content.Replace("(*VoyageLabel*)", VoyageLabel);
			content = content.Replace("(*CarrierLabel*)", CarrierLabel);
			content = content.Replace("(*VesselName*)", scheduleChange.Vessel);
			content = content.Replace("(*Voyage*)", scheduleChange.Voyage);
			content = content.Replace("(*Carrier*)", scheduleChange.Carrier);
			writer.Write(content);
		}

		void WriteDatesAffected(TextWriter writer, JobScheduleChange[] scheduleChanges, bool hasDataProviderDetails)
		{
			string portPairChangeDetailsHeader = hasDataProviderDetails
				? GetEmailTemplateFragment((NoResString)"<!--EndSection UserVesselVoyageCarrier-->", (NoResString)"<!--StartSection DateChangeDetails-->")
				: GetEmailTemplateFragment((NoResString)"<!--EndSection UserVesselVoyageCarrier-->", (NoResString)"<!--StartSection DataProviderHeading-->")
					+ GetEmailTemplateFragment((NoResString)"<!--EndSection DataProviderHeading-->", (NoResString)"<!--StartSection DateChangeDetails-->");

			string portPairChangeDetailsTrailer = GetEmailTemplateFragment((NoResString)"<!--EndSection DateChangeDetails-->", (NoResString)"<!--StartSection JobsAffected-->");

			writer.Write(portPairChangeDetailsHeader);
			foreach (JobScheduleChange scheduleChange in scheduleChanges)
			{
				WriteDateAffected(writer, scheduleChange, hasDataProviderDetails);
			}
			writer.Write(portPairChangeDetailsTrailer);
		}

		void WriteDateAffected(TextWriter writer, JobScheduleChange scheduleChange, bool hasDataProviderDetails)
		{
			IScheduleChangeParent parent = scheduleChange.Parent;

			if (parent != null)
			{
				string portPairHeader = hasDataProviderDetails
					? GetEmailTemplateFragment((NoResString)"<!--StartSection DateChangeDetails-->", (NoResString)"<!--EndSection DateChangeDetails-->")
					: GetEmailTemplateFragment((NoResString)"<!--StartSection DateChangeDetails-->", (NoResString)"<!--StartSection DataProviderDetails-->")
						+ GetEmailTemplateFragment((NoResString)"<!--EndSection DataProviderDetails-->", (NoResString)"<!--EndSection DateChangeDetails-->");

				portPairHeader = portPairHeader.Replace("(*LoadPort*)", parent.OriginPort.PadRight(5));
				portPairHeader = portPairHeader.Replace("(*DischargePort*)", parent.DestinationPort.PadRight(5));
				portPairHeader = portPairHeader.Replace("(*FieldName*)", scheduleChange.DateTypeDescription.PadRight(18, ' '));
				portPairHeader = portPairHeader.Replace("(*OldDate*)", FormatDateTime(scheduleChange.E7_PreviousValue).PadRight(15, ' '));
				portPairHeader = portPairHeader.Replace("(*UpdatedDate*)", FormatDateTime(scheduleChange.E7_UpdatedValue));

				if (hasDataProviderDetails)
				{
					portPairHeader = portPairHeader.Replace("(*DataProvider*)", DataProviderList.GetDescriptionFromCode(scheduleChange.E7_DataProvider));
				}

				writer.Write(portPairHeader);
			}
		}

		public CodeDescriptionPairList DataProviderList
		{
			get { return dataProviderList ?? (dataProviderList = SailingScheduleHelper.GetDataSourceList()); }
		}

		[NonSerialized]
		CodeDescriptionPairList dataProviderList;

		string FormatDateTime(ZDateTime dateValue)
		{
			return dateValue.IsValid ? dateValue.ToLongTimeString() : (NoResString)"(empty)";
		}

		#endregion

		#region Affected Jobs Details

		void WriteJobsAffected(TextWriter writer, BusinessObjectFactory factory, JobScheduleChange[] scheduleChanges, ZDateTime fromDate)
		{
			Dictionary<ZGuid, DelayAlertTypes> jobsThatRequireDelayAlert = new Dictionary<ZGuid, DelayAlertTypes>();
			AddJobsThatRequireDelayAlerts(jobsThatRequireDelayAlert, DelayAlertDocumentDelivery.LoadJobsThatRequireImportDelayAlertDelivery(scheduleChanges), DelayAlertTypes.Import);
			AddJobsThatRequireDelayAlerts(jobsThatRequireDelayAlert, DelayAlertDocumentDelivery.LoadJobsThatRequireExportDelayAlertDelivery(scheduleChanges), DelayAlertTypes.Export);

			foreach (KeyValuePair<string, JobSailingRelatedJob[]> jobType in LoadJobsAffectedGroupedByJobType(scheduleChanges))
			{
				string jobTypeDescription = jobType.Key;
				JobSailingRelatedJob[] jobs = jobType.Value;
				WriteJobsAffectedForJobType(writer, jobTypeDescription, jobs, jobsThatRequireDelayAlert, fromDate);
			}

			writer.Write(GetEmailTemplateFragment((NoResString)"<!--EndSection JobsAffected-->", (NoResString)"<!--EndSection ChangeDetails-->"));
		}

		void AddJobsThatRequireDelayAlerts(Dictionary<ZGuid, DelayAlertTypes> lookup, JobSailingRelatedJob[] jobs, DelayAlertTypes types)
		{
			for (int i = 0; i < jobs.Length; i++)
			{
				DelayAlertTypes existing;
				ZGuid pk = jobs[i].Job.PK;

				if (!lookup.TryGetValue(pk, out existing))
				{
					lookup.Add(pk, types);
				}
				else if ((types & ~existing) != DelayAlertTypes.None)
				{
					lookup[pk] = existing | types;
				}
			}
		}

		void WriteJobsAffectedForJobType(TextWriter writer, string jobTypeDescription, JobSailingRelatedJob[] jobs, Dictionary<ZGuid, DelayAlertTypes> jobsThatRequireDelayAlert, ZDateTime fromDate)
		{
			string jobTypeGroupHeaderContent = JobTypeGroupHeaderTemplate.Replace("(*AffectedJobType*)", jobTypeDescription);
			writer.Write(jobTypeGroupHeaderContent);

			foreach (JobSailingRelatedJob job in jobs)
			{
				WriteJobNumberContent(writer, job, jobsThatRequireDelayAlert, fromDate);
			}
		}

		void WriteJobNumberContent(TextWriter writer, JobSailingRelatedJob job, Dictionary<ZGuid, DelayAlertTypes> jobsThatRequireDelayAlert, ZDateTime fromDate)
		{
			string jobSailingRelatedJobContent = JobSailingRelatedJobTemplate;
			string jobNumber = job.JobNumber;
			string jobNumberIndent = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>().IsInstanceOfType(job.Job) && ((CommonShipment)job.Job).JS_IsForwardRegistered ? "&nbsp;&nbsp;&#3" : "";
			jobSailingRelatedJobContent = jobSailingRelatedJobContent.Replace("(*JobNumberIndent*)", jobNumberIndent);
			jobSailingRelatedJobContent = jobSailingRelatedJobContent.Replace("(*JobNumber*)", job.JobNumber);
			jobSailingRelatedJobContent = jobSailingRelatedJobContent.Replace("(*JobUrl*)", job.JobUrl);

			ZString delayAlertDeliveryStatus = GetDelayAlertDeliveryStatus(job.Job, jobsThatRequireDelayAlert, fromDate);
			if (!delayAlertDeliveryStatus.IsEmpty)
			{
				jobSailingRelatedJobContent = jobSailingRelatedJobContent.Replace("(*DelayAlertDeliveryStatus*)", "- " + delayAlertDeliveryStatus);
			}
			else
			{
				jobSailingRelatedJobContent = jobSailingRelatedJobContent.Replace("(*DelayAlertDeliveryStatus*)", "");
			}
			writer.Write(jobSailingRelatedJobContent);
		}

		KeyValuePair<string, JobSailingRelatedJob[]>[] LoadJobsAffectedGroupedByJobType(JobScheduleChange[] scheduleChanges)
		{
			List<KeyValuePair<string, JobSailingRelatedJob[]>> result = new List<KeyValuePair<string, JobSailingRelatedJob[]>>();
			AddToListIfNotEmpty(Res.GetString("b92e2f71-7755-4f21-af98-6c332e808b71", "Consolidation (and related shipment)"), RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(scheduleChanges), result);
			AddToListIfNotEmpty(Res.GetString("a077826e-f52a-47f3-97ac-94ba3fbaf36b", "Customs Declaration"), RelatedSailingJobs.LoadRelatedDeclarationJobs(scheduleChanges), result);
			AddToListIfNotEmpty(Res.GetString("780e8922-1ae2-46ab-8bcf-4ef5a9ecee60", "CFS Load List"), RelatedSailingJobs.LoadRelatedLoadListJobs(scheduleChanges), result);
			AddToListIfNotEmpty(Res.GetString("f9ccad34-1b21-4110-8b91-afe7ac98ade9", "Booking"), RelatedSailingJobs.LoadRelatedExportBookingJobs(scheduleChanges), result);
			AddToListIfNotEmpty(Res.GetString("933f5fb4-6d6a-41a6-857f-df8806efb6ec", "Shipping Booking"), RelatedSailingJobs.LoadRelatedAgencyBookingJobs(scheduleChanges), result);
			AddToListIfNotEmpty(Res.GetString("982e53ae-e898-4197-9525-c33f9992e482", "Shipping Bill of Lading"), RelatedSailingJobs.LoadRelatedAgencyDocumentationJobs(scheduleChanges), result);
			AddToListIfNotEmpty(Res.GetString("da6826eb-a0d8-4e5c-9c77-e9b3cc72dab8", "Port Transport"), RelatedSailingJobs.LoadRelatedLocalCartageJobs(scheduleChanges), result);
			AddToListIfNotEmpty(Res.GetString("476bc670-d978-4235-ad44-8da16b729db0", "Order"), RelatedSailingJobs.LoadRelatedOrderJobs(scheduleChanges), result);

			return result.ToArray();
		}

		void AddToListIfNotEmpty(string jobTypeDescription, JobSailingRelatedJob[] jobsAffected, List<KeyValuePair<string, JobSailingRelatedJob[]>> list)
		{
			if (jobsAffected.Length > 0)
			{
				list.Add(new KeyValuePair<string, JobSailingRelatedJob[]>(jobTypeDescription, jobsAffected));
			}
		}

		Dictionary<ChangedByAndVesselVoyageCarrier, List<JobScheduleChange>> GroupByChangedByAndVesselVoyageCarrier(BusinessObjectFactory factory, JobScheduleChange[] scheduleChanges)
		{
			var result = new Dictionary<ChangedByAndVesselVoyageCarrier, List<JobScheduleChange>>();
			foreach (JobScheduleChange scheduleChange in scheduleChanges)
			{
				if (scheduleChange.ChangedBy != null)
				{
					var carrierOrg = factory.Load<OrgHeader>(scheduleChange.Voyage.JV_OH_Line);
					var carrierOrgName = (carrierOrg != null) ? carrierOrg.OH_FullNameTruncated : ZString.Empty;
					var hasDataProvider = !scheduleChange.E7_DataProvider.IsEmpty;
					var key = new ChangedByAndVesselVoyageCarrier(scheduleChange.ChangedBy, scheduleChange.Voyage.JV_RV_NKVessel, scheduleChange.Voyage.JV_VoyageFlight, carrierOrgName, hasDataProvider);

					List<JobScheduleChange> list = null;
					result.TryGetValue(key, out list);

					if (list == null)
					{
						list = new List<JobScheduleChange>();
						result[key] = list;
					}

					list.Add(scheduleChange);
				}
			}

			return result;
		}

		RelatedSailingJobs RelatedSailingJobs
		{
			get { return relatedSailingJobs ?? (relatedSailingJobs = new RelatedSailingJobs()); }
		}

		[NonSerialized]
		RelatedSailingJobs relatedSailingJobs;

		class ChangedByAndVesselVoyageCarrier
		{
			public ChangedByAndVesselVoyageCarrier(GlbStaff changedBy, ZString vessel, ZString voyage, ZString carrier, ZBool hasDataProvider)
			{
				this.ChangedBy = changedBy;
				this.Vessel = vessel;
				this.Voyage = voyage;
				this.Carrier = carrier;
				this.HasDataProvider = hasDataProvider;
			}

			public readonly GlbStaff ChangedBy;
			public readonly ZString Vessel;
			public readonly ZString Voyage;
			public readonly ZString Carrier;
			public readonly ZBool HasDataProvider;

			public override bool Equals(object obj)
			{
				var rhs = obj as ChangedByAndVesselVoyageCarrier;
				return rhs != null
					&& ChangedBy.GS_Code == rhs.ChangedBy.GS_Code
					&& Vessel == rhs.Vessel
					&& Voyage == rhs.Voyage
					&& Carrier == rhs.Carrier
					&& HasDataProvider == rhs.HasDataProvider;
			}

			public override int GetHashCode()
			{
				return ChangedBy.GS_Code.GetHashCode() ^ Vessel.GetHashCode() ^ Voyage.GetHashCode() ^ Carrier.GetHashCode() ^ HasDataProvider.GetHashCode();
			}
		}

		#endregion

		#region Delay Alert Document

		DelayAlertDocumentDelivery DelayAlertDocumentDelivery
		{
			get { return delayAlertDocumentDelivery ?? (delayAlertDocumentDelivery = new DelayAlertDocumentDelivery()); }
		}

		[NonSerialized]
		DelayAlertDocumentDelivery delayAlertDocumentDelivery;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html markup")]
		ZString GetDelayAlertDeliveryStatus(BusinessObject job, Dictionary<ZGuid, DelayAlertTypes> jobsThatRequireDelayAlert, ZDateTime fromDate)
		{
			ZString result = ZString.Empty;
			DelayAlertTypes alertTypes;

			if (IsDelayAlertCancelledByUser(job, fromDate))
			{
				result = Res.GetString("100f8a2c-b27f-4ae7-9931-e027aac0ed05", "delay alert delivery was canceled at the user's request");
			}
			else if (jobsThatRequireDelayAlert.TryGetValue(job.PK, out alertTypes) && alertTypes != DelayAlertTypes.None)
			{
				if (!IsDelayAlertDelivered(job, fromDate))
				{
					const string HtmlPrefix = "<strong><span style=\"color: #FF0000\">";
					const string HtmlSuffix = "</span></strong>";

					ZString failureMessage = GetDelayAlertDeliveryFailureMessage(job, fromDate);
					failureMessage = failureMessage.IsEmpty ? "" : (" (" + failureMessage + ")");

					string core = Res.GetString("6b3828da-4488-41c1-bd90-3acc1911c2d4", "delay alert failed to be delivered") + failureMessage;

					result = HtmlPrefix + core + HtmlSuffix;
				}
				else if (job.Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(job.PK) != null)
				{
					switch (alertTypes)
					{
						case DelayAlertTypes.Export:
							result = Res.GetString("7174979e-32c3-4f76-92cd-70837334b86f", "delay alert delivered to the exporter");
							break;

						case DelayAlertTypes.Import:
							result = Res.GetString("e6424a38-d28a-4efb-8c73-db0f6fa360a5", "delay alert delivered to the importer");
							break;

						case DelayAlertTypes.Import | DelayAlertTypes.Export:
							result = Res.GetString("94df0c11-7cfc-4cac-9720-ef231f15a635", "delay alert delivered to the exporter & importer");
							break;
					}
				}
				else
				{
					switch (alertTypes)
					{
						case DelayAlertTypes.Export:
							result = Res.GetString("decde08e-093b-4621-937c-28f5b13aa9f6", "delay alert delivered to the consignor");
							break;

						case DelayAlertTypes.Import:
							result = Res.GetString("b2afc687-8b08-4b4d-acb6-b1da5fb1039c", "delay alert delivered to the consignee");
							break;

						case DelayAlertTypes.Import | DelayAlertTypes.Export:
							result = Res.GetString("a5eadae5-01b9-4bfd-b0ed-879de52bc3ed", "delay alert delivered to the consignor & consignee");
							break;
					}
				}
			}

			return result;
		}

		bool IsDelayAlertDelivered(BusinessObject job, ZDateTime fromDate)
		{
			StmALog documentDeliveredLog = LoadDocumentDeliveredLog(job, Events.DocumentSent, fromDate);
			return documentDeliveredLog != null;
		}

		bool IsDelayAlertCancelledByUser(BusinessObject job, ZDateTime fromDate)
		{
			StmALog documentDeliveredLog = LoadDocumentDeliveredLog(job, Events.DocumentNotDelivered, Constants.DocumentNotDeliveredReasons.Codes.Cancelled, fromDate);
			return documentDeliveredLog != null;
		}

		ZString GetDelayAlertDeliveryFailureMessage(BusinessObject job, ZDateTime fromDate)
		{
			StmALog documentDeliveryFailureLog = LoadDocumentDeliveredLog(job, Events.DocumentNotDelivered, Constants.DocumentNotDeliveredReasons.Codes.Failed, fromDate);
			return documentDeliveryFailureLog == null ? ZString.Empty : documentDeliveryFailureLog.ReferenceFreeText;
		}

		static StmALog LoadDocumentDeliveredLog(BusinessObject job, Event eventType, ZDateTime fromDate)
		{
			return LoadDocumentDeliveredLog(job, eventType, "", fromDate);
		}

		static StmALog LoadDocumentDeliveredLog(BusinessObject job, Event eventType, string reasonParameter, ZDateTime fromDate)
		{
			var result = job.Factory.LoadTop1<StmALog>(DocumentDeliveredLogQuery(job, eventType, reasonParameter));

			// this code is intentionally done in memory, and NOT in the db, so that the old statistics on SL_PostedTimeUTC do not make it use the incorrect index.
			// could be done with a hint, but this is easier
			if (fromDate.IsValid && result != null && result.SL_PostedTimeUtc < fromDate)
			{
				result = null;
			}
			return result;
		}

		static ZDBOnlyQuery DocumentDeliveredLogQuery(BusinessObject job, Event eventType, string reasonParameter)
		{
			var query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddToFilter(StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, job.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, eventType.Code);
			if (!string.IsNullOrEmpty(reasonParameter))
			{
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, StmALog.GenerateEventReference("", new[] { new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, reasonParameter) }));
			}

			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Delay Alert");

			query.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";
			return query;
		}

		public static bool DocumentDeliveredLogExists(BusinessObject job, ZDateTime fromDate)
		{
			return LoadDocumentDeliveredLog(job, Events.DocumentDelivered, fromDate) != null;
		}

		string GetDelayAlertDeliveryStatusForAllJobs(JobScheduleChange[] scheduleChanges, ZDateTime fromDate)
		{
			bool isAtLeast1Delivered = false;
			bool isAtLeast1Failed = false;

			foreach (JobSailingRelatedJob job in DelayAlertDocumentDelivery.LoadJobsThatRequireImportDelayAlertDelivery(scheduleChanges))
			{
				if (!IsDelayAlertCancelledByUser(job.Job, fromDate))
				{
					isAtLeast1Delivered = true;

					if (!IsDelayAlertDelivered(job.Job, fromDate))
					{
						isAtLeast1Failed = true;
						break;
					}
				}
			}

			if (!isAtLeast1Failed)
			{
				foreach (JobSailingRelatedJob job in DelayAlertDocumentDelivery.LoadJobsThatRequireExportDelayAlertDelivery(scheduleChanges))
				{
					if (!IsDelayAlertCancelledByUser(job.Job, fromDate))
					{
						isAtLeast1Delivered = true;

						if (!IsDelayAlertDelivered(job.Job, fromDate))
						{
							isAtLeast1Failed = true;
							break;
						}
					}
				}
			}

			return GetDelayAlertDeliveryStatusForAllJobs(isAtLeast1Delivered, isAtLeast1Failed);
		}

		string GetDelayAlertDeliveryStatusForAllJobs(bool isAtLeast1Delivered, bool isAtLeast1Failed)
		{
			string result = "";
			if (isAtLeast1Failed)
			{
				result = (NoResString)"<strong><span style=\"color: #FF0000\">" + Res.GetString("be3fd56f-59f9-4fd2-a8e9-931166b15d3d", "!! Delay alert document failed to be delivered to one or more clients. !!") + (NoResString)"</span></strong><br>\r\n";
				result += Res.GetString("0e2148ed-7037-4139-a143-211cdec3a6cf", "Check the event log of each job, or try to deliver the document manually.<br><br>");
			}
			else if (isAtLeast1Delivered)
			{
				result = (NoResString)"<strong><span style=\"color: #FF0000\">" + Res.GetString("f4f7b114-e135-4c3f-af82-bf06fbff298e", "Delay alert documents have been delivered to one or more clients.") + (NoResString)"</span></strong><br><br>";
			}

			return result;
		}

		[Flags]
		enum DelayAlertTypes
		{
			None = 0x00,
			Import = 0x01,
			Export = 0x02,
		}

		#endregion

		#region Html Templates

		string JobTypeGroupHeaderTemplate
		{
			get
			{
				if (jobTypeGroupHeaderTemplate == null)
				{
					jobTypeGroupHeaderTemplate = GetEmailTemplateFragment((NoResString)"<!--StartSection JobsAffected-->", (NoResString)"<!--StartSection JobAffected-->");
				}
				return jobTypeGroupHeaderTemplate;
			}
		}
		string jobTypeGroupHeaderTemplate;

		string JobSailingRelatedJobTemplate
		{
			get
			{
				if (jobSailingRelatedJobTemplate == null)
				{
					jobSailingRelatedJobTemplate = GetEmailTemplateFragment((NoResString)"<!--StartSection JobAffected-->", (NoResString)"<!--EndSection JobAffected-->");
				}
				return jobSailingRelatedJobTemplate;
			}
		}
		string jobSailingRelatedJobTemplate;

		string GetEmailTemplateFragment(string startComment, string endComment)
		{
			int startIndex = 0;
			if (startComment != null)
			{
				string startCommentAndNewline = startComment + "\r\n";
				startIndex = IndexOfOrThrow(EmailTemplateHtml, startCommentAndNewline, 0);
				startIndex += startCommentAndNewline.Length;
			}

			int endIndex = EmailTemplateHtml.Length;
			if (endComment != null)
			{
				endIndex = IndexOfOrThrow(EmailTemplateHtml, endComment, startIndex);
			}
			return EmailTemplateHtml.Substring(startIndex, endIndex - startIndex);
		}

		int IndexOfOrThrow(string text, string toFind, int startIndex)
		{
			int result = text.IndexOf(toFind, startIndex);
			if (result == -1)
			{
				throw new ArgumentException("Could not find text '" + toFind + "'");
			}
			return result;
		}

		static string EmailTemplateHtml
		{
			get
			{
				if (emailTemplateHtml == null)
				{
					Stream stream = typeof(ScheduleChangeEmailSender).Assembly.GetManifestResourceStream("Enterprise.Freight.Business.Notifications.ScheduleChangeEmailSender.ScheduleChangeEmailTemplate.htm");
					emailTemplateHtml = new StreamReader(stream).ReadToEnd();
				}
				return emailTemplateHtml;
			}
		}

		[ThreadStatic]
		static string emailTemplateHtml;

		#endregion

		#region Implementation

		static string FormatUtc(ZDateTime utcDateTime)
		{
			if (!utcDateTime.IsValid || utcDateTime.IsEmpty)
			{
				return null;
			}
			return utcDateTime.ToLongTimeString() + " UTC";
		}

		#endregion
	}
}
