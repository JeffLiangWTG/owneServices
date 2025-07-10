using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CargoReportWorkflow : Integration.Customs.ICargoReportWorkflow, ICustomsServiceTaskProcess, IDisposable
	{
		public CargoReportWorkflow(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		protected readonly BusinessObjectFactory factory;

		/// <summary>
		/// Called by the service task as part of workflow processing
		/// </summary>
		/// <param name="shipment"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "This isn't the time to refactor here.")]
		public void ProcessOneImportShipment(ForwardingShipment shipment)
		{
			ProcessTask[] cargoReportAcceptedMilestones = ForwardingShipmentProcessTask.CargoReportAcceptedMilestones(shipment);
			var scheduledCargoReportDate = new ZDateTimeOffset(ScheduledCargoReportDate(shipment));
			Logger ??= new LoggingInformation();
			if (cargoReportAcceptedMilestones.Length == 0)
			{
				Logger.Log(string.Format(CultureInfo.CurrentCulture, "No cargoReportAcceptedMilestones, Will create Cargo Report Accepted Milestone for {0}", shipment.JobNumber), Integration.LogType.Debug);
				try
				{
					ProcessTask newMileStone = shipment.WorkflowItems.Milestones.AddNew();
					newMileStone.P9_Description = Res.GetString("1996d80a-4675-485e-939b-b15ec41e5c1c", "Cargo Report Accepted");
					newMileStone.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
					newMileStone.SetScheduledDate(scheduledCargoReportDate);
					newMileStone.P9_IsPublished = CustomsDataRegistry.Instance.PublishLateCargoMilestone.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					ZGuid groupPK = shipment.IsSea ? CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.Value :
						CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.Value;
					if (factory.Load<GlbGroup>(groupPK) != null)
					{
						newMileStone.P9_GG_AssignedGroup = groupPK;
					}
					Logger.Log(string.Format(CultureInfo.CurrentCulture, "Creating Cargo Report Accepted Milestone {0} for {1} with scheduled date {2}", newMileStone.PK, shipment.JobNumber, scheduledCargoReportDate), Integration.LogType.Debug);
				}
				catch (Exception ex)
				{
					Logger.LogError(ex.Message);
				}
			}
			else
			{
				Logger.Log(string.Format(CultureInfo.CurrentCulture, "Found {0} cargoReportAcceptedMilestones for shipment {1}", cargoReportAcceptedMilestones.Length, shipment.JobNumber), Integration.LogType.Debug);
				foreach (var mileStone in cargoReportAcceptedMilestones)
				{
					Logger.Log(string.Format(CultureInfo.CurrentCulture, "Cargo Report Accepted Milestone {0} for {1} with scheduled date {2}", mileStone.PK, shipment.JobNumber, mileStone.TriggerProperties.ScheduledDate), Integration.LogType.Debug);
				}

				if (!scheduledCargoReportDate.IsEmpty)
				{
					foreach (ProcessTask mileStone in cargoReportAcceptedMilestones)
					{
						if (!mileStone.IsClosed && mileStone.TriggerProperties.ScheduledDate != scheduledCargoReportDate)
						{
							mileStone.SetScheduledDate(scheduledCargoReportDate);
						}
					}
				}
			}
		}

		ZString emailText;
		ZString emailTextSortKey;
		ForwardingShipment shipment;
		Dictionary<ZGuid, KeyValuePair<List<ZString>, bool>> exceptionReportTextLists;
		Dictionary<ZGuid, GuidRegistryItem> registryKeys;
		string tableBody;
		public LoggingInformation Logger { get; set; }

		public void Dispose()
		{
		}

		public int retryCount { get; private set; }

#if DEBUG

		public void ExecuteBatch()
		{
			ExecuteBatch(CancellationToken.None);
		}

#endif

		public void ExecuteBatch(CancellationToken token)
		{
			Logger.Log(string.Format(CultureInfo.CurrentCulture, "Starting Late and Pending Cargo Report for {0}", GlbCompany.CurrentCompany.GC_Name));
			retryCount = 0;
			ZString result = ExecuteBatchCore(token);
			if (!result.IsEmpty)
			{
				retryCount++;
				Logger.LogWarning("Late and Pending Cargo Report save error on first try: " + result);
				result = ExecuteBatchCore(token);
				if (!result.IsEmpty)
				{
					Logger.LogError("Late and Pending Cargo Report save error on second try, run aborted: " + result);
				}
			}
			Logger.Log(string.Format(CultureInfo.CurrentCulture, "Finished Late and Pending Cargo Report for {0}", GlbCompany.CurrentCompany.GC_Name));
		}

		ZQuery MilestoneQuery(bool includeEmptyScheduledDate, bool includeClosed)
		{
			var processTaskQuery = new ZDBOnlyQuery(typeof(ProcessTasks));
			processTaskQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, Events.CargoReportAccepted.Code);
			processTaskQuery.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);

			var scheduledDateFiler = new ZQuery();
			scheduledDateFiler.AddToFilter(ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddMonths(-1));
			if (includeEmptyScheduledDate)
			{
				scheduledDateFiler.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_ScheduledDateUtc, ZDateTime.Empty);
			}
			processTaskQuery.AddToFilter(scheduledDateFiler);

			if (!includeClosed)
			{
				processTaskQuery.AddToFilter(ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTask.IsClosedCodes);
			}

			var jobQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			jobQuery.AddToFilter(JobShipmentSchema.JS_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.UtcNow.AddYears(-1));
			processTaskQuery.AddSubQuery(ProcessTasksSchema.P9_ParentID, jobQuery, JoinCondition.And);

			return processTaskQuery;
		}

		Dictionary<ZGuid, ForwardingShipment> LoadCandidateShipments(BusinessObjectFactory factory, object shipmentPKs)
		{
			var shipmentQuery = new ZQuery(JobShipmentSchema.PK, shipmentPKs)
				.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, SQLComparisonOperator.StartsWith, GlbBranch.CurrentBranch.Country.Code);

			return factory.Load<ForwardingShipment>(shipmentQuery).ToDictionary(x => x.PK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "This isn't the time to refactor here.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "This isn't the time to refactor here.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This isn't the time to refactor here.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode", Justification = "This isn't the time to refactor here.")]
		ZString ExecuteBatchCore(CancellationToken token)
		{
			var reportFactory = new BusinessObjectFactory();
			exceptionReportTextLists = new Dictionary<ZGuid, KeyValuePair<List<ZString>, bool>>();
			registryKeys = new Dictionary<ZGuid, GuidRegistryItem>();

			var collection = new DynamicBusinessObjectCollection(reportFactory);
			var query2 = MilestoneQuery(true, includeClosed: false).ParameterisedText;
			collection.Load(string.Format(CultureInfo.CurrentCulture, "select {0}, {1} from {2} where ", ProcessTasksSchema.Constants.PK, ProcessTasksSchema.Constants.P9_ParentID, ProcessTasksSchema.Constants.TableName) + query2.ParameterisedQueryText, query2.Parameters);
			var guids = collection.Cast<DynamicBusinessObject>().Select(x => new { PK = x[ProcessTasksSchema.Constants.PK], Parent = x[ProcessTasksSchema.Constants.P9_ParentID] }).ToList();
			var batchSize = CustomsDataRegistry.Instance.LateAndPendingCargoReportBatchSize.Value;

			while (guids.Count > 0)
			{
				token.ThrowIfCancellationRequested();
				int min = Math.Min(batchSize, guids.Count);
				var toProcess = guids.Take(min).ToArray();
				guids.RemoveRange(0, toProcess.Length);
				var newFactory = new BusinessObjectFactory();
				GC.Collect();

				// Seed load forwarding shipments
				var shipments = LoadCandidateShipments(newFactory, toProcess.Select(x => x.Parent));

				newFactory.Load<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JS, shipments.Keys));
				newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, shipments.Keys));

				foreach (ProcessTask mileStone in newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, toProcess.Select(x => x.PK))))
				{
					token.ThrowIfCancellationRequested();
					if (shipments.TryGetValue(mileStone.P9_ParentID, out shipment))
					{
						var scheduledCargoReportDate = new ZDateTimeOffset(ScheduledCargoReportDate(shipment));
						if (!scheduledCargoReportDate.IsEmpty && scheduledCargoReportDate != mileStone.TriggerProperties.ScheduledDate)
						{
							mileStone.SetScheduledDate(scheduledCargoReportDate);
						}
					}
				}
				try
				{
					newFactory.Save();
				}
				catch (ZSaveConcurrencyException ex)
				{
					return ex.Message;
				}
			}

			string tableHeading = GetEmailTemplateFragment((NoResString)"<!--StartSection ReportShipmentsTable-->", (NoResString)"<!--StartSection OneShipmentsTable-->");
			tableBody = GetEmailTemplateFragment((NoResString)"<!--StartSection OneShipmentsTable-->", (NoResString)"<!--EndSection OneShipmentsTable-->");
			string tableFooter = GetEmailTemplateFragment((NoResString)"<!--EndSection OneShipmentsTable-->", (NoResString)"<!--EndSection ReportShipmentsTable-->");

			collection = new DynamicBusinessObjectCollection(reportFactory);
			query2 = MilestoneQuery(false, includeClosed: true).ParameterisedText;
			collection.Load(string.Format(CultureInfo.CurrentCulture, "select {0}, {1} from {2} where ", ProcessTasksSchema.Constants.PK, ProcessTasksSchema.Constants.P9_ParentID, ProcessTasksSchema.Constants.TableName) + query2.ParameterisedQueryText, query2.Parameters);
			guids = collection.Cast<DynamicBusinessObject>().Select(x => new { PK = x[ProcessTasksSchema.Constants.PK], Parent = x[ProcessTasksSchema.Constants.P9_ParentID] }).ToList();
			while (guids.Count > 0)
			{
				token.ThrowIfCancellationRequested();
				int min = Math.Min(batchSize, guids.Count);
				var toProcess = guids.Take(min).ToArray();
				guids.RemoveRange(0, toProcess.Length);
				var newFactory = new BusinessObjectFactory();
				GC.Collect();

				// Seed load forwarding shipments
				var shipments = LoadCandidateShipments(newFactory, toProcess.Select(x => x.Parent));

				newFactory.Load<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JS, shipments.Keys));
				newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, shipments.Keys));

				var milestoneExceptionQuery = new ZQuery(ProcessTasksSchema.P9_ParentID, shipments.Keys)
					.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, JobShipmentSchema.Constants.Prefix)
					.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, Events.CargoReportAccepted.Code)
					.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.ExceptionType);

				newFactory.Load<ProcessTask>(milestoneExceptionQuery);

				foreach (ProcessTask mileStone in newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, toProcess.Select(x => x.PK))))
				{
					token.ThrowIfCancellationRequested();
					if (shipments.TryGetValue(mileStone.P9_ParentID, out shipment) && !ImportExportHelper.IsDomestic(shipment.JS_RL_NKOrigin, shipment.JS_RL_NKDestination))
					{
						emailText = ZString.Empty;
						emailTextSortKey = ZString.Empty;
						if (mileStone.IsClosed)
						{
							ProcessClosedMilestone(mileStone);
						}
						else
						{
							ProcessOpenMilestone(mileStone);
						}
						if (!emailTextSortKey.IsEmpty)
						{
							var groupFromRegistry = false;
							ZGuid emailRecipient = mileStone.P9_GG_AssignedGroup;
							if (emailRecipient.IsEmpty)
							{
								emailRecipient = shipment.IsSea ? CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.Value :
								CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.Value;
								groupFromRegistry = true;
							}
							if (!emailRecipient.IsEmpty)
							{
								string emailTableBody = tableBody;
								emailTableBody = emailTableBody.Replace("(*Shipment*)", shipment.JS_UniqueConsignRef);
#if DEBUG
								if (!Globals.IsTest)
#endif
								{
									emailTableBody = emailTableBody.Replace("(*ShipmentUrl*)", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.JobShipment, shipment.PK.ToGuid()));
								}
								emailTableBody = emailTableBody.Replace("(*Voyage*)", VoyageFlightDetails);
								emailTableBody = emailTableBody.Replace("(*FirstPort*)", shipment.FallbackPortOfFirstArrival);
								emailTableBody = emailTableBody.Replace("(*FirstPortETA*)", shipment.FallbackPortOfFirstArrivalETA.ToString());
								emailTableBody = emailTableBody.Replace("(*RequiredBy*)", emailTextSortKey == "1" ? "" : mileStone.P9_ScheduledDate.AddHours(shipment.CargoReportAcceptedEventSafetyMargin).ToZDateTime().ToString());
								emailTableBody = emailTableBody.Replace("(*Comments*)", emailText);
								if (!exceptionReportTextLists.ContainsKey(emailRecipient))
								{
									exceptionReportTextLists.Add(emailRecipient, new KeyValuePair<List<ZString>, bool>(new List<ZString>(), groupFromRegistry));
									registryKeys.Add(emailRecipient, shipment.IsSea ?
											CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo :
											CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo);
								}
								exceptionReportTextLists[emailRecipient].Key.Add(emailTextSortKey + emailTableBody);
							}
						}
					}
				}
			}

			foreach (ZGuid emailRecipient in exceptionReportTextLists.Keys)
			{
				token.ThrowIfCancellationRequested();
				string leadingContent = GetEmailTemplateFragment(null, (NoResString)"<!--StartSection CargoReportExceptionDetails-->");
				leadingContent = leadingContent.Replace("(*HtmlStyleSheet*)", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
				leadingContent = leadingContent.Replace("(*CurrentTime*)", ZDateTime.Now.ToLongTimeString());
				string trailingContent = GetEmailTemplateFragment((NoResString)"<!--EndSection CargoReportExceptionDetails-->", null);

				StringBuilder emailTextBuilder = new StringBuilder(leadingContent);
				List<ZString> oneRecipientReportTextList = exceptionReportTextLists[emailRecipient].Key;
				oneRecipientReportTextList.Sort();
				ZString lastSortKey = ZString.Empty;
				foreach (ZString oneTextLine in oneRecipientReportTextList)
				{
					token.ThrowIfCancellationRequested();
					if (lastSortKey != oneTextLine.Substring(0, 1))
					{
						if (!lastSortKey.IsEmpty)
						{
							emailTextBuilder.Append(tableFooter);
						}
						lastSortKey = oneTextLine.Substring(0, 1);
						if (lastSortKey == "1")
						{
							emailTextBuilder.Append(GetEmailTemplateFragment((NoResString)"<!--StartSection NoArrival-->", (NoResString)"<!--EndSection NoArrival-->"));
						}
						else if (lastSortKey == "2")
						{
							emailTextBuilder.Append(GetEmailTemplateFragment((NoResString)"<!--StartSection LateShipments-->", (NoResString)"<!--EndSection LateShipments-->"));
						}
						else if (lastSortKey == "3")
						{
							emailTextBuilder.Append(GetEmailTemplateFragment((NoResString)"<!--StartSection ApproachingShipments-->", (NoResString)"<!--EndSection ApproachingShipments-->"));
						}
						else if (lastSortKey == "4")
						{
							emailTextBuilder.Append(GetEmailTemplateFragment((NoResString)"<!--StartSection ManuallyActioned-->", (NoResString)"<!--EndSection ManuallyActioned-->"));
						}
						else if (lastSortKey == "5")
						{
							emailTextBuilder.Append(GetEmailTemplateFragment((NoResString)"<!--StartSection IsNowLate-->", (NoResString)"<!--EndSection IsNowLate-->"));
						}
						else if (lastSortKey == "6")
						{
							emailTextBuilder.Append(GetEmailTemplateFragment((NoResString)"<!--StartSection IsNowNotLate-->", (NoResString)"<!--EndSection IsNowNotLate-->"));
						}
						emailTextBuilder.Append(tableHeading);
					}
					emailTextBuilder.Append(oneTextLine.Substring(1));
				}
				emailTextBuilder.Append(tableFooter);
				emailTextBuilder.Append(trailingContent);

				var cargoReportExceptionsEmail = new EmailDef();
				cargoReportExceptionsEmail.ContentType = EmailContentTypes.HTML;
				cargoReportExceptionsEmail.Subject = Res.GetString("949d3a95-7f54-46a7-aa23-58cfab60bfd6", "Late and Pending Shipment Cargo Report Notifications");
				cargoReportExceptionsEmail.Body = emailTextBuilder.ToString();
				AddImageAttachment(cargoReportExceptionsEmail, "Banner.jpg", SystemDataRegistry.Instance.HtmlEmailBannerImage.Value ?? new Bitmap(1, 1));
				AddImageAttachment(cargoReportExceptionsEmail, "Footer.jpg", SystemDataRegistry.Instance.HtmlEmailFooterImage.Value ?? new Bitmap(1, 1));
				var emailGroup = reportFactory.Load<GlbGroup>(emailRecipient);
				var emailGroupCode = emailGroup != null ? emailGroup.GG_Code : ZString.Empty;
				var groupLocation = exceptionReportTextLists[emailRecipient].Value ? (NoResString)RegistryKeyDescription(emailRecipient) : (NoResString)string.Format(CultureInfo.CurrentCulture, (NoResString)"Milestone -> '{0}'", emailGroupCode).TrimEnd();
				cargoReportExceptionsEmail.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(emailRecipient.ToGuid(), groupLocation);
				try
				{
					if (Logger != null)
					{
						Logger.Log("Sending Cargo Report Notifications eMail to group " + emailGroupCode);
					}

					Env.OutgoingCustomsMailManager.CreateAndSave(cargoReportExceptionsEmail);
					if (Logger != null)
					{
						Logger.Log("Sent Cargo Report Notifications eMail to group " + emailGroupCode);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (Logger != null)
					{
						Logger.LogError("Sending Cargo Report Notifications eMail to group " + emailGroupCode + " failed: " + ex.Message);
					}
				}
#if DEBUG
				cargoReportExceptionsEmailForTesting = cargoReportExceptionsEmail;
#endif
			}
			try
			{
				reportFactory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				return ex.Message;
			}
			return ZString.Empty;
		}

#if DEBUG
		public EmailDef cargoReportExceptionsEmailForTesting;
#endif

		void ProcessClosedMilestone(ProcessTask mileStone)
		{
			ZDateTimeOffset scheduledCargoReportDate = new ZDateTimeOffset(ScheduledCargoReportDate(shipment));
			ProcessTask cachedMilestoneException = mileStone.MilestoneException;
			if (!scheduledCargoReportDate.IsEmpty && scheduledCargoReportDate != mileStone.TriggerProperties.ScheduledDate && mileStone.P9_ActualDate.IsValid)
			{
				if ((cachedMilestoneException == null || (cachedMilestoneException != null && cachedMilestoneException.P9_Notes.IsEmpty))
					&& scheduledCargoReportDate.AddHours(shipment.CargoReportAcceptedEventSafetyMargin) < mileStone.TriggerProperties.ActualDate)
				{
					emailTextSortKey = "5";
					emailText = Res.GetString("1a487508-2bb0-4816-858e-1af405254e3d", "Cargo Report was late.");
				}
				else if (cachedMilestoneException != null && !cachedMilestoneException.P9_Notes.IsEmpty
					&& scheduledCargoReportDate.AddHours(shipment.CargoReportAcceptedEventSafetyMargin) >= mileStone.TriggerProperties.ActualDate)
				{
					emailTextSortKey = "6";
					emailText = Res.GetString("7dccbc68-4f21-4418-a8ff-e0f80cd36010", "Cargo Report was not late.");
				}
			}
		}

		void ProcessOpenMilestone(ProcessTask mileStone)
		{
			ProcessTask cachedMilestoneException = mileStone.MilestoneException;
			if (cachedMilestoneException != null)
			{
				if (!shipment.IsDomestic() || CustomsDataRegistry.Instance.CustomsIsRequiredForDomesticShipments.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					if (ReportNoCustomsSuppliedImpendingArrivalDate(shipment))
					{
						emailTextSortKey = "1";
						emailText = CommentForNoCustomsSuppliedImpendingArrivalDate(shipment);
					}
					else if (cachedMilestoneException.IsExceptionActioned)
					{
						var customsStatusProvider = ForwardingShipmentCustomsStatusProvider.New(shipment);

						if (customsStatusProvider.CustomsMessageStatus() == CMRBaseStatuses.Codes.AwaitingResponseToOriginal)
						{
							if (shipment.IsCargoReportOverdue(mileStone))
							{
								emailTextSortKey = "2";
								emailText = Res.GetString("8e4acbc2-e5c4-4242-8df7-f00c4960da96", "!!! LATE, Waiting for response from Customs.");
							}
							else
							{
								emailTextSortKey = "3";
								emailText = Res.GetString("0020a954-fe96-4a72-8b98-a6ee8dc7752c", "DUE DATE APPROACHING, Waiting for response from Customs.");
							}
						}
						else if (customsStatusProvider.CustomsMessageStatus() == CMRBaseStatuses.Codes.OriginalRejected)
						{
							if (shipment.IsCargoReportOverdue(mileStone))
							{
								emailTextSortKey = "2";
								emailText = Res.GetString("855522c8-6708-45f1-befc-cf828c291eae", "!!! LATE, Original message rejected.");
							}
							else
							{
								emailTextSortKey = "3";
								emailText = Res.GetString("24cca2f9-4966-4da6-b5ea-1a2c88561ca8", "DUE DATE APPROACHING, Original message rejected.");
							}
						}
						else
						{
							emailText = Res.GetString("da5a54eb-3297-4fc3-acab-19ef484bac8e", "Exception manually actioned.");
							emailTextSortKey = "4";
						}
					}
					else if (shipment.IsCargoReportOverdue(mileStone))
					{
						emailTextSortKey = "2";
						emailText = Res.GetString("5d288666-b389-4aa1-8c9f-b98c0e3d47fe", "!!! LATE.");
					}
					else
					{
						emailTextSortKey = "3";
						emailText = Res.GetString("72209523-ff4b-4852-9cff-3a2b2e179654", "DUE DATE APPROACHING.");
					}
				}
			}
		}

		protected virtual ZDateTime ScheduledCargoReportDate(ForwardingShipment shipment)
		{
			ZDateTime result = shipment.FallbackPortOfFirstArrivalETA;
			if (!result.IsEmpty)
			{
				result = result.AddHours(-shipment.CargoReportAcceptedEventSafetyMargin);
			}
			return result;
		}

		protected virtual bool ReportNoCustomsSuppliedImpendingArrivalDate(ForwardingShipment shipment)
		{
			return false;
		}

		protected virtual string CommentForNoCustomsSuppliedImpendingArrivalDate(ForwardingShipment shipment)
		{
			return Res.GetString("7d91cb77-bfce-4737-82de-bb6a4e7a633f", "No Customs Impending Arrival Found.");
		}

		ZString VoyageFlightDetails
		{
			get
			{
				ZString result = "";
				if (shipment.ArrivalConsol != null)
				{
					if (shipment.IsSea && shipment.ArrivalConsol.Vessel != null)
					{
						result = shipment.ArrivalConsol.Vessel.RV_Code + "/";
					}
					result += shipment.ArrivalConsol.JK_JX_JV_VoyageFlight;
				}
				return result;
			}
		}

		void AddImageAttachment(EmailDef email, string displayName, Image image)
		{
			MemoryStream imageStream = new MemoryStream();
			image.Save(imageStream, ImageFormat.Jpeg);
			AttachmentDef attachment = new AttachmentDef(displayName, imageStream.ToArray());
			email.Attachments.Add(attachment);
		}

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

		string EmailTemplateHtml
		{
			get
			{
				if (emailTemplateHtml == null)
				{
					Stream stream = typeof(CargoReportWorkflow).Assembly.GetManifestResourceStream("Enterprise.Customs.Business.CargoReportWorkflow.LateCargoReportEmailTemplate.htm");
					emailTemplateHtml = new StreamReader(stream).ReadToEnd();
				}
				return emailTemplateHtml;
			}
		}
		string emailTemplateHtml;

		string RegistryKeyDescription(ZGuid emailRecipient)
		{
			GuidRegistryItem registryItem;
			return registryKeys.TryGetValue(emailRecipient, out registryItem) ? @"Registry->" + registryItem.Category.Replace("/", "->") + "->" + registryItem.Name : "";
		}
	}
}
