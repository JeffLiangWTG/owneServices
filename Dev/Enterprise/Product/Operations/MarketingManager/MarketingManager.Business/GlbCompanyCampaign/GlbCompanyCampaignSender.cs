using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSender
	{
		public GlbCompanyCampaignSender(GlbCompanyCampaign campaign)
		{
			this.campaign = campaign;
		}

		public GlbCompanyCampaignSender(GlbCompanyCampaign campaign, IEnumerable<GlbCompanyCampaignItem> campaignItemsToSend)
		{
			this.campaign = campaign;
			this.campaignItemsToSend = campaignItemsToSend.ToArray();
		}

		public GlbCompanyCampaignSender(GlbCompanyCampaign campaign, IEnumerable<CampaignContact> contactsToSend, bool isScheduled = false)
		{
			this.campaign = campaign;
			this.contactsToSend = contactsToSend.ToArray();
			this.IsScheduled = isScheduled;
		}

		readonly GlbCompanyCampaign campaign;
		readonly CampaignContact[] contactsToSend;
		readonly GlbCompanyCampaignItem[] campaignItemsToSend;
		bool IsScheduled { get; }

		readonly List<string> sendErrors = new List<string>();
		public IEnumerable<string> SendErrors { get { return sendErrors; } }

		#region GUI Events

		public delegate void ContactsNotSentCampaignEventHandler(int numContactsSent, ReadOnlyCollection<CampaignContact> contactsNotSentTo);
		public delegate void ContactsNotScheduledCampaignEventHandler<TEventArgs>(object sender, TEventArgs e);
		public delegate bool CheckContinueWithSendingHandler(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignToResend);
		public delegate void CampaignSendingMessageEventHandler(object sender, MessageOnCampaignSendingEventArgs e);
		public delegate void CampaignSuccessfullySentEventHandler(int contactsDeliveredCount);
		public delegate void ContactsToSendToEventHandler<TEventArgs>(object sender, TEventArgs e);

		public event CampaignSendingMessageEventHandler MessageOnCampaignSending;
		public event CheckContinueWithSendingHandler ShouldContinueWithSending;
		public event ContactsNotSentCampaignEventHandler ContactsNotSentCampaign;
		public event CampaignSuccessfullySentEventHandler CampaignSuccessfullySent;
		public event ContactsToSendToEventHandler<ContactsToSendToEventArgs> SendScheduleEvent;
		public event ContactsNotScheduledCampaignEventHandler<ContactsNotScheduledCampaignEventArgs> ContactsNotScheduledCampaign;

		#endregion

		#region EventArgs

		public class ContactsNotScheduledCampaignEventArgs : EventArgs
		{
			readonly Collection<IScheduleItemsProvider> contactsScheduled;
			readonly ReadOnlyCollection<CampaignContact> contactsNotSentTo;

			public ContactsNotScheduledCampaignEventArgs(Collection<IScheduleItemsProvider> contactsScheduled, ReadOnlyCollection<CampaignContact> contactsNotSentTo)
			{
				this.contactsScheduled = contactsScheduled;
				this.contactsNotSentTo = contactsNotSentTo;
			}

			public Collection<IScheduleItemsProvider> ContactsScheduled
			{
				get { return contactsScheduled; }
			}

			public ReadOnlyCollection<CampaignContact> ContactsNotSentTo
			{
				get { return contactsNotSentTo; }
			}
		}

		public class ContactsToSendToEventArgs : EventArgs
		{
			readonly Collection<IScheduleItemsProvider> contactsToSendTo;

			public ContactsToSendToEventArgs(Collection<IScheduleItemsProvider> contactsToSendTo)
			{
				this.contactsToSendTo = contactsToSendTo;
			}

			public Collection<IScheduleItemsProvider> ContactsToSendTo
			{
				get { return contactsToSendTo; }
			}
		}

		#endregion

		#region Send Campaigns

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Mutex ID description")]
		static readonly MutexID SendCampaignMutexId = new MutexID("SendCampaign", "Ensures when two or more users send a campaign at the same time, each receiptient only gets sent once.");

		#region Requeue / Resend

		public void Requeue(GlbCompanyCampaignItem[] sentCampaigns)
		{
			campaign.Validation.ValidateAll();

			if (campaign.CampaignHasChanges)
			{
				DisplayMessageOnCampaignSending(NotificationConstants.SaveCampaignMessage, NotificationConstants.CannotRequeueSummary, true);
			}
			else if (campaign.HasErrors)
			{
				DisplayMessageOnCampaignSending(GetErrorMessagesToDisplayOnCampaignSending(NotificationConstants.CorrectAllErrorsOnRequeuingMessage), NotificationConstants.CannotRequeueSummary, true);
			}
			else
			{
				int numContactsToSendTo = (campaignItemsToSend == null) ? contactsToSend.Length : campaignItemsToSend.Length;
				if (OnShouldSendEmails(numContactsToSendTo, campaignItemsToSend))
				{
					int count = 0;
					OnItemDeleteBegin();

					sentCampaigns.ToList().ForEach(campaignItem =>
					{
						if (!fCancelSending)
						{
							count++;
							campaign.CampaignItemSchedule.SelectedScheduleItems.Remove(campaignItem);
							campaignItem.Delete();

							OnItemSent(count, sentCampaigns.Length);
						}
					});
					campaign.Factory.Save();

					OnCampaignSendEnd();
				}
			}
		}

		#endregion

		public bool CheckAndSendCampaigns(bool shouldCreateCampaignItemOnSchedule = false)
		{
			var isSent = false;
			campaign.Validation.ValidateAll();

			if (campaign.CampaignHasChanges)
			{
				DisplayMessageOnCampaignSending(NotificationConstants.SaveCampaignMessage, NotificationConstants.CannotSendCampaignsSummary, true);
			}
			else if (campaign.HasErrors || campaign.EmailContentIsNotSetButRequired)
			{
				DisplayMessageOnCampaignSending(GetErrorMessagesToDisplayOnCampaignSending(), NotificationConstants.CannotSendCampaignsSummary, true);
			}
			else
			{
				var numContactsToSendTo = campaignItemsToSend?.Length ?? contactsToSend.Length;
				if (numContactsToSendTo == 0)
				{
					DisplayMessageOnCampaignSending(NotificationConstants.NoSelectedRecipientsMessage, NotificationConstants.CannotSendCampaignsSummary, true);
				}
				else
				{
					if (shouldCreateCampaignItemOnSchedule)
					{
						if (OnShouldSendEmails(numContactsToSendTo, campaignItemsToSend))
						{
							SendCampaigns(campaignItemsToSend, campaign, true);
							isSent = true;
						}
					}
					else if (IsScheduled || campaign.IsOpportunityCreationCampaign)
					{
						SendCampaigns(campaignItemsToSend, campaign);
						isSent = true;
					}
					else
					{
						if (OnShouldSendEmails(numContactsToSendTo, campaignItemsToSend))
						{
							SendCampaigns(campaignItemsToSend, campaign);
							isSent = true;
						}
					}
				}
			}

			return isSent;
		}

		public string GetErrorMessagesToDisplayOnCampaignSending()
		{
			return GetErrorMessagesToDisplayOnCampaignSending(NotificationConstants.CorrectAllErrorsMessage);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not needed in logging")]
		public string GetErrorMessagesToDisplayOnCampaignSending(string correctAllMessage)
		{
			var stringBuilder = new ZStringBuilder(correctAllMessage);
			if (campaign.EmailContentIsNotSetButRequired)
			{
				stringBuilder.Append("- " + NotificationConstants.EmailContentCannotBeEmpty);
			}
			if (campaign.RequiresCampaignURL && campaign.Validation.IsCampaignURLFieldMissing(out var errorMessageIsDocumentAttached))
			{
				stringBuilder.Append("- " + errorMessageIsDocumentAttached);
			}

			var campaignErrors = new ZNotificationCollector(campaign, true, false, ZNotificationCollector.PropertyDescriptionType.None)
				.Where(x => x.Type == NotificationType.Error)
				.Select(x => x.Message)
				.Distinct();

			foreach (string error in campaignErrors)
			{
				stringBuilder.Append("- " + error);
			}

			if (campaign.IsTouchCampaign)
			{
				stringBuilder.Append(string.Format("This touch campaign can be accessed through its master campaign with ID {0}.", campaign.MasterCampaign.G0_CampaignID));
			}

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		internal void SendCampaigns(GlbCompanyCampaignItem[] campaignItemToResendOverride, GlbCompanyCampaign currentCampaign, bool isCreateCampaignItemOnSchedule = false)
		{
			try
			{
				ReadOnlyCollection<CampaignContact> contactsNotSentTo;

				if (IsScheduled && !isCreateCampaignItemOnSchedule)
				{
					contactsNotSentTo = SendScheduleCampaigns();
				}
				else if (currentCampaign.IsOpportunityCreationCampaign)
				{
					if (campaignItemToResendOverride != null && campaignItemToResendOverride.Any())
					{
						RequeueOpportunitiesCreation(campaignItemToResendOverride);
						DisplayMessageOnCampaignSending(string.Format(CultureInfo.InvariantCulture, NotificationConstants.OpportunitiesCreatedMessage, ContactsDeliveredCount), NotificationConstants.OpportunitiesCreatedSummary, false);

						return;
					}

					contactsNotSentTo = SendValidCampaigns();
				}
				else if (campaignItemToResendOverride == null || isCreateCampaignItemOnSchedule)
				{
					contactsNotSentTo = SendValidCampaigns();
				}
				else
				{
					contactsNotSentTo = ResendValidCampaign(campaignItemToResendOverride);
				}

				if (contactsNotSentTo.Count == 0)
				{
					if (currentCampaign.IsTargetList)
					{
						DisplayMessageOnCampaignSending(string.Format(CultureInfo.InvariantCulture, NotificationConstants.CampaignsTargetedListMessage, ContactsDeliveredCount), NotificationConstants.CampaignsTargetedSummary, false);
					}
					else if (currentCampaign.IsMasterCampaign)
					{
						DisplayMessageOnCampaignSending(string.Format(CultureInfo.InvariantCulture, NotificationConstants.CampaignsMasteredListMessage, ContactsDeliveredCount), NotificationConstants.CampaignsMasteredSummary, false);
					}
					else if (!IsScheduled)
					{
						DisplayMessageOnCampaignSending(string.Format(CultureInfo.InvariantCulture, NotificationConstants.CampaignsSentMessage, ContactsDeliveredCount), NotificationConstants.CampaignsSentSummary, false);
					}

					if (IsScheduled)
					{
						OnSendScheduleCampaign(ContactsMarkedForSchedule);
					}
					else
					{
						ShowCampaignSuccessfullySent(ContactsDeliveredCount);
					}
				}
				else
				{
					if (IsScheduled)
					{
						ShowContactsNotSentTo(ContactsMarkedForSchedule, contactsNotSentTo);
					}
					else
					{
						ShowContactsNotSentTo((ContactsDeliveredCount < 0 ? 0 : ContactsDeliveredCount), contactsNotSentTo);
					}
				}
			}
			catch (DocumentParsingFailedException e)
			{
				DisplayMessageOnCampaignSending(string.Format(CultureInfo.InvariantCulture, NotificationConstants.DocumentMergedFailedMessage, e.Message), NotificationConstants.CouldNotMergeSummary, true);
			}
			catch (UriFormatException e)
			{
				DisplayMessageOnCampaignSending(e.Message, NotificationConstants.CannotSendCampaignsSummary, true);
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}

		void RequeueOpportunitiesCreation(GlbCompanyCampaignItem[] previouslySentCampaign)
		{
			ContactsDeliveredCount = 0;

			foreach (var campaignItem in previouslySentCampaign)
			{
				if (campaignItem.G8_TrackingStatus == TrackingStatusCodes.Codes.OPQ)
				{
					campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
					ContactsDeliveredCount++;
				}
			}

			if (ContactsDeliveredCount > 0)
			{
				campaign.Factory.Save();
			}
		}

		Collection<IScheduleItemsProvider> ContactsMarkedForSchedule;

		ReadOnlyCollection<CampaignContact> SendScheduleCampaigns()
		{
			List<CampaignContact> contactsNotSentTo = new List<CampaignContact>();
			ContactsMarkedForSchedule = new Collection<IScheduleItemsProvider>();

			ContactsDeliveredCount = 0;
			fCancelSending = false;

			OnCampaignSendBegin();

			var mutex = new ZGlobalMutex(SendCampaignMutexId, campaign.PK.ToString());

			try
			{
				if (mutex.Lock())
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
					query.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, campaign.PK);
					var existingCampaignItems = campaign.Factory.Load<GlbCompanyCampaignItem>(query).ToDictionary(x => x.G8_RecipientID);

					foreach (CampaignContact contactToSend in contactsToSend)
					{
						if (contactToSend.IsNDR || contactToSend.VCC_Email.IsEmpty || !EmailAddressValidation.IsEmailAddressValid(contactToSend.VCC_Email))
						{
							contactsNotSentTo.Add(contactToSend);
						}
						else if (!fCancelSending)
						{
							bool isItemExisting = existingCampaignItems.ContainsKey(contactToSend.PK);

							if (!isItemExisting)
							{
								ContactsMarkedForSchedule.Add(contactToSend);
							}
						}

						ContactsDeliveredCount++;
						OnItemSent(ContactsDeliveredCount, contactsToSend.Length);
					}
				}
			}
			finally
			{
				if (mutex != null && mutex.HasLock)
				{
					mutex.Unlock();
					mutex = null;
				}

				OnCampaignSendEnd();
			}

			return contactsNotSentTo.AsReadOnly();
		}

		ReadOnlyCollection<CampaignContact> ResendValidCampaign(GlbCompanyCampaignItem[] previouslySentCampaign)
		{
			List<CampaignContact> contactsNotSentTo = new List<CampaignContact>();

			try
			{
				ContactsDeliveredCount = 0;

				OnCampaignSendBegin();

				var senderPoolData = PrepareSenderPool(GetExistingCampaignItems());

				foreach (GlbCompanyCampaignItem campaignItem in previouslySentCampaign)
				{
					SetItemSenderFromPool(senderPoolData, campaignItem, true);

					if (!ShouldBeSentViaEmail(campaignItem))
					{
						contactsNotSentTo.Add(campaign.Factory.Load<CampaignContact>(campaignItem.Recipient.PK));
						SetCampaignItemSenderForPrint(campaignItem);
					}
					else
					{
						SetCampaignItemSenderForEmail(campaignItem);
						SendEmailToContact(campaignItem);
					}

					ContactsDeliveredCount++;
					campaignItem.ResetTrackingInfo(IsScheduled);
					campaignItem.G8_LastSentTimeUtc = ZDateTime.UtcNow;
					campaignItem.G8_SystemLastEditTimeUtc = ZDateTime.UtcNow;

					OnItemSent(ContactsDeliveredCount, previouslySentCampaign.Length);
				}
			}
			catch (DocumentParsingFailedException)
			{
				campaign.Factory.Save();
				throw;
			}
			BeginTracking(campaign);
			campaign.Factory.Save();

			OnCampaignSendEnd();

			return contactsNotSentTo.AsReadOnly();
		}

		void CreateOpportunityQueuedItem(CampaignContact contact)
		{
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = contact.ScheduleData?.ScheduleTimeUtc ?? ZDateTime.Now;
			campaignItem.G8_RecipientTableCode = contact.VCC_TableCode;
			campaignItem.G8_IsCheckTransitionRequired = false;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_G0 = campaign.PK;

			ContactsDeliveredCount++;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "Complicated process would only get more complicated by splitting it up.")]
		internal ReadOnlyCollection<CampaignContact> SendValidCampaigns()
		{
			campaign.CampaignItemsSentNotSaved = new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
			var contactsNotSentTo = new List<CampaignContact>();

			ContactsDeliveredCount = 0;
			fCancelSending = false;

			OnCampaignSendBegin();

			var mutex = new ZGlobalMutex(SendCampaignMutexId, campaign.PK.ToString());

			try
			{
				if (mutex.Lock())
				{
					CreateRelationshipWithSourceCampaignIfExists();

					var existingCampaignItems = GetExistingCampaignItems();
					var existingCampaignItemsKeys = new HashSet<ZGuid>(existingCampaignItems.Select(x => x.G8_RecipientID));
					var senderPoolData = PrepareSenderPool(existingCampaignItems);
					campaign.Factory.Load<SalesEnquiry>(new ZQuery(OrgColdCallRegisterSchema.PK, contactsToSend.Where(s => s.VCC_TableCode == OrgColdCallRegisterSchema.Constants.Prefix).Select(s => s.PK))); //Pre-fetch all contacts in one hit. All other calls to Factory.Load<SalesEnquiry> should use the in-memory cache.

					foreach (var contactToSend in contactsToSend)
					{
						if (campaign.IsOpportunityCreationCampaign)
						{
							CreateOpportunityQueuedItem(contactToSend);
							continue;
						}

						if (contactToSend.IsNDR || !EmailAddressValidation.IsEmailAddressValidAndNotEmpty(contactToSend.VCC_Email))
						{
							contactsNotSentTo.Add(contactToSend);
							continue;
						}

						if (fCancelSending)
						{
							break;
						}

						if (existingCampaignItemsKeys.Contains(contactToSend.PK))
						{
							continue;
						}

						var sentCampaign = campaign.CampaignItemsSentNotSaved.AddNew();
						sentCampaign.G8_RecipientID = contactToSend.PK;
						sentCampaign.G8_RecipientTableCode = contactToSend.VCC_TableCode;
						SetItemSenderFromPool(senderPoolData, sentCampaign, false);

						if (sentCampaign.SenderUNLOCO.IsEmpty)
						{
							string sender = sentCampaign.GetEmailSenderStaff()?.GS_Code;
							string message = ResString.GetMultilingualString("6CEA64E9-520A-441B-A043-02AC806E5113",
								"Missing home branch for some of the senders. Campaign: {0}. ", campaign.G0_CampaignNameMultilingual);

							if (!string.IsNullOrEmpty(sender))
							{
								message += ResString.GetMultilingualString("94448802-7315-4595-89A9-AE4AA0FBF085", "Sender: {0}.", sender);
							}

							if (!sendErrors.Contains(message))
							{
								sendErrors.Add(message);
							}

							contactsNotSentTo.Add(contactToSend);
							sentCampaign.Delete();
							continue;
						}

						ReplaceCampaignSalesRelationLinksToToCampaignItem(campaign, sentCampaign);

						var isLinkedToSourceCampaignItem = CreateRelationshipWithSourceCampaignItemIfExists(sentCampaign);

						if (!isLinkedToSourceCampaignItem && contactToSend.VCC_TableCode == OrgColdCallRegisterSchema.Constants.Prefix)
						{
							if (sentCampaign is IRelatableActivity newRelatableActivity)
							{
								var inquiryContact = campaign.Factory.Load<SalesEnquiry>(contactToSend.PK);
								((RelatedParentActivityPivotCollection)newRelatableActivity.RelatedParentActivityPivotCollection).AddActivity(inquiryContact, true);
							}
						}

						try
						{
							DoActualSend(sentCampaign, contactsNotSentTo, contactToSend);
						}
						catch (DocumentParsingFailedException)
						{
							campaign.CampaignItemsSentNotSaved.RemoveAndDeleteAll();
							campaign.Factory.Save();
							throw;
						}
						catch (UriFormatException)
						{
							sentCampaign.Delete();
							throw;
						}

						ContactsDeliveredCount++;
						OnItemSent(ContactsDeliveredCount, contactsToSend.Length);
					}
				}

				BeginTracking(campaign);
				campaign.Factory.Save();
				campaign.CampaignsItemsSent.Load();
				campaign.CampaignItemsSentNotSaved = null;
			}
			finally
			{
				if (mutex != null && mutex.HasLock)
				{
					mutex.Unlock();
					mutex = null;
				}

				OnCampaignSendEnd();
			}

			return contactsNotSentTo.AsReadOnly();
		}

		void DoActualSend(GlbCompanyCampaignItem sentCampaign, List<CampaignContact> contactsNotSentTo, CampaignContact contactToSend)
		{
			if (campaign.IsTargetList || campaign.IsMasterCampaign)
			{
				SetCampaignItemSenderForTargetList(sentCampaign);
			}
			else if (!IsScheduled)
			{
				if (ShouldBeSentViaEmail(sentCampaign))
				{
					SetCampaignItemSenderForEmail(sentCampaign);
					SendEmailToContact(sentCampaign);
				}
				else
				{
					contactsNotSentTo.Add(contactToSend);
					SetCampaignItemSenderForPrint(sentCampaign);
				}
			}
			else
			{
				SetCampaignItemSenderForEmail(sentCampaign);
			}

			if (IsScheduled)
			{
				sentCampaign.G8_ScheduleTimeUtc = contactToSend.ScheduleData.ScheduleTimeUtc;
				sentCampaign.G8_BatchNumber = contactToSend.ScheduleData.BatchNumber;
			}

			sentCampaign.ResetTrackingInfo(IsScheduled);
		}

		GlbCompanyCampaignItem[] GetExistingCampaignItems()
		{
			var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, campaign.PK);
			return campaign.Factory.Load<GlbCompanyCampaignItem>(query);
		}

		#region Sender updates

		void SetCampaignItemSenderForEmail(GlbCompanyCampaignItem sentCampaign)
		{
			if (campaign.G0_UseLastEmailSenderAddress)
			{
				GlbCompanyCampaignItem prevCampaignItem;
				if (campaign.IsTouchCampaign)
				{
					var transitions = CampaignSummaryStats.LoadTransitions(sentCampaign);
					var prevTransitionPk = transitions.OrderByDescending(results => results.HorizontalId).FirstOrDefault(results => results.CampaignId != sentCampaign.PK)?.CampaignItemId ?? ZGuid.Empty;
					prevCampaignItem = sentCampaign.Factory.Load<GlbCompanyCampaignItem>(prevTransitionPk);
				}
				else
				{
					prevCampaignItem = sentCampaign.RelatedParentActivityPivotCollection.Activities.OfType<GlbCompanyCampaignItem>().FirstOrDefault();
				}

				if (SenderIsStillActive(prevCampaignItem))
				{
					SetCampaignItemSender(sentCampaign, GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode, prevCampaignItem.G8_GS_NKSender, prevCampaignItem.G8_SenderEmailAddress, prevCampaignItem.G8_EmailSenderName);
					return;
				}
			}

			SetCampaignItemSender(sentCampaign, GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode,
				(campaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML ? null : sentCampaign.GetEmailSenderStaff())?.GS_Code ?? ZString.Empty,
				sentCampaign.SenderEmailAddress,
				sentCampaign.SenderName);
		}

		static bool SenderIsStillActive(GlbCompanyCampaignItem prevCampaignItem)
		{
			if (prevCampaignItem == null)
			{
				return false;
			}

			if (prevCampaignItem.G8_GS_NKSender.IsEmpty)
			{
				return true; // For missing sender in case of EML we don't poke DB
			}

			var query = new ZQuery(GlbStaffSchema.GS_Code, prevCampaignItem.G8_GS_NKSender) { ReLoadExistingRows = true };
			var sender = prevCampaignItem.Factory.Load<GlbStaff>(query).FirstOrDefault();
			return sender != null && sender.GS_IsActive;
		}

		static void SetCampaignItemSenderForPrint(GlbCompanyCampaignItem sentCampaign) => SetCampaignItemSender(sentCampaign, GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.PrintCode);

		static void SetCampaignItemSenderForTargetList(GlbCompanyCampaignItem sentCampaign) => SetCampaignItemSender(sentCampaign, GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.TargetListCode);

		static void SetCampaignItemSender(GlbCompanyCampaignItem sentCampaign, string deliveryMethod, string senderCode = null, string senderEmailAddress = null, string senderName = null)
		{
			sentCampaign.G8_DeliveryMethod = deliveryMethod;
			sentCampaign.G8_GS_NKSender = senderCode ?? ZString.Empty;
			sentCampaign.G8_SenderEmailAddress = senderEmailAddress ?? ZString.Empty;
			sentCampaign.G8_EmailSenderName = senderName ?? ZString.Empty;
		}

		#region Sender Pool Implementation

		public void SetCampaignItemsSender(List<GlbCompanyCampaignItem> items)
		{
			if (campaign != null && campaign.IsOpportunityCreationCampaign)
			{
				return;
			}

			if (items.Any())
			{
				switch (campaign?.G0_EmailSenderOption.ToString())
				{
					case EmailSenderOptionCodeDescriptionList.Codes.SPS:
						{
							SetCampaignItemsSenderPool(items);
							break;
						}
					case EmailSenderOptionCodeDescriptionList.Codes.EML:
						{
							foreach (var item in items)
							{
								item.G8_EmailSenderName = campaign.SenderName;
								item.G8_SenderEmailAddress = campaign.SenderEmailAddress;
							}
							break;
						}
					default:
						{
							foreach (var item in items)
							{
								item.G8_GS_NKSender = item.GetEmailSenderStaff()?.GS_Code ?? ZString.Empty;
							}
							break;
						}
				}
			}
		}

		void SetCampaignItemsSenderPool(List<GlbCompanyCampaignItem> campaignItems)
		{
			if (campaignItems.Any())
			{
				var senderPoolData = PrepareSenderPool(Array.Empty<GlbCompanyCampaignItem>());
				if (senderPoolData != null)
				{
					foreach (var item in campaignItems)
					{
						item.G8_GS_NKSender = ZString.Empty;
						SetItemSenderFromPool(senderPoolData, item, true);
					}
				}
			}
		}

		SenderPoolData PrepareSenderPool(GlbCompanyCampaignItem[] existingCompanyCampaignItems)
		{
			if (campaign.G0_EmailSenderOption != EmailSenderOptionCodeDescriptionList.Codes.SPS)
			{
				return null;
			}
			if (existingCompanyCampaignItems == null)
			{
				throw new ArgumentNullException(nameof(existingCompanyCampaignItems));
			}

			var senderPoolCurrentRatioDictionary = existingCompanyCampaignItems.GroupBy(item => item.G8_GS_NKSender).Select(items => new { items.Key, SendRatio = items.Count() }).ToDictionary(arg => arg.Key, arg => arg.SendRatio);

			var totalSenderPoolSendRatio = (double)campaign.SenderPool.Sum(item => item.GCP_SendRatio);
			var senderPool = campaign.SenderPool.Select(item => new { item.GCP_GS_NKSender, SendRatio = item.GCP_SendRatio / totalSenderPoolSendRatio }).ToDictionary(arg => arg.GCP_GS_NKSender, arg => arg.SendRatio);

			return new SenderPoolData(senderPoolCurrentRatioDictionary, senderPool);
		}

		void SetItemSenderFromPool(SenderPoolData senderPoolData, GlbCompanyCampaignItem glbCompanyCampaignItem, bool isResend)
		{
			if (senderPoolData == null)
			{
				return;
			}
			if (glbCompanyCampaignItem == null)
			{
				throw new ArgumentNullException(nameof(glbCompanyCampaignItem));
			}
			if (!isResend && !glbCompanyCampaignItem.G8_GS_NKSender.IsEmpty
				|| !glbCompanyCampaignItem.G8_GS_NKSender.IsEmpty && senderPoolData.NormalizedSendRatio.ContainsKey(glbCompanyCampaignItem.G8_GS_NKSender))
			{
				return;
			}

			ZString senderCode;
			var totalEmailCount = senderPoolData.CurrentPoolRatio.Sum(pair => pair.Value);
			if (totalEmailCount == 0)
			{
				senderCode = senderPoolData.NormalizedSendRatio.MaxBy(pair => pair.Value).Key;
			}
			else
			{
				senderCode = senderPoolData.NormalizedSendRatio.Select(item =>
					new
					{
						item.Key,
						SendRatioDelta = item.Value - (senderPoolData.CurrentPoolRatio.ContainsKey(item.Key) ? senderPoolData.CurrentPoolRatio[item.Key] : 0.0) / totalEmailCount
					})
					.MaxBy(arg => arg.SendRatioDelta).Key;
			}

			glbCompanyCampaignItem.G8_GS_NKSender = senderCode;
			if (senderPoolData.CurrentPoolRatio.ContainsKey(senderCode))
			{
				senderPoolData.CurrentPoolRatio[senderCode]++;
			}
			else
			{
				senderPoolData.CurrentPoolRatio[senderCode] = 1;
			}
		}

		class SenderPoolData
		{
			public SenderPoolData(IDictionary<ZString, int> currentPoolRatio, IReadOnlyDictionary<ZString, double> normalizedSendRatio)
			{
				CurrentPoolRatio = currentPoolRatio;
				NormalizedSendRatio = normalizedSendRatio;
			}

			public IDictionary<ZString, int> CurrentPoolRatio { get; }
			public IReadOnlyDictionary<ZString, double> NormalizedSendRatio { get; }
		}

		#endregion

		#endregion

		void CreateRelationshipWithSourceCampaignIfExists()
		{
			if (campaign.SourceCampaignPK.IsValid)
			{
				var sourceCampaign = campaign.Factory.Load<GlbCompanyCampaign>(campaign.SourceCampaignPK);
				if (sourceCampaign != null && sourceCampaign is IRelatableActivity newRelatableActivity)
				{
					((RelatedParentActivityPivotCollection)campaign.RelatedParentActivityPivotCollection).AddActivity(newRelatableActivity, true);
				}
			}
		}

		bool CreateRelationshipWithSourceCampaignItemIfExists(GlbCompanyCampaignItem sentCampaign)
		{
			bool success = false;
			if (campaign.SourceCampaignPK.IsValid)
			{
				ZQuery query = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaign.SourceCampaignPK);
				query.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientID, sentCampaign.G8_RecipientID);
				var previousCampaignItem = campaign.Factory.LoadTop1<GlbCompanyCampaignItem>(query);
				if (previousCampaignItem != null)
				{
					sentCampaign.RelatedParentActivityPivotCollection.AddNewPivot(previousCampaignItem);
					success = true;
				}
			}
			return success;
		}

		internal int ContactsDeliveredCount;

		static void BeginTracking(GlbCompanyCampaign campaign)
		{
			foreach (var link in campaign.TrackedLinks)
			{
				if (!link.GCL_IsTracked)
				{
					link.GCL_IsTracked = true;
				}
			}
		}

		static void ReplaceCampaignSalesRelationLinksToToCampaignItem(GlbCompanyCampaign campaign, GlbCompanyCampaignItem campaignItem)
		{
			var factory = campaignItem.Factory;
			ReplaceCampaignParentSalesRelationLinksToToCampaignItem(factory, campaign, campaignItem);
			ReplaceCampaignChildSalesRelationLinksToToCampaignItem(factory, campaign, campaignItem);
		}

		static void ReplaceCampaignParentSalesRelationLinksToToCampaignItem(BusinessObjectFactory factory, GlbCompanyCampaign campaign, GlbCompanyCampaignItem campaignItem)
		{
			var campaignParentMatchScorePairs = new Dictionary<IRelatableActivity, int>();
			foreach (var pivot in campaign.RelatedParentActivityPivotCollection)
			{
				var parentActivity = pivot.ParentActivity;
				if (parentActivity != null)
				{
					var matchScore = GlbCompanyCampaign.GetMatchScore(campaignItem, parentActivity);
					if (matchScore > 0)
					{
						campaignParentMatchScorePairs.Add(parentActivity, matchScore);
					}
				}
			}

			foreach (var campaignParentMatchScorePair in campaignParentMatchScorePairs.OrderBy(pair => pair.Value).ThenBy(pair => pair.Key.SystemCreateTimeUtc))
			{
				var parent = campaignParentMatchScorePair.Key;
				if (campaignItem.RelatedParentActivityPivotCollection.AddActivity(parent).Success)
				{
					parent.RelatedChildActivityPivotCollection.RemoveActivity(campaign);
				}
			}
		}

		static void ReplaceCampaignChildSalesRelationLinksToToCampaignItem(BusinessObjectFactory factory, GlbCompanyCampaign campaign, GlbCompanyCampaignItem campaignItem)
		{
			var pivotsWithCampaignAsParent = campaign.RelatedChildActivityPivotCollection;
			foreach (var pivot in pivotsWithCampaignAsParent.ToArray())
			{
				var childActivity = pivot.ChildActivity;
				if (childActivity != null && GlbCompanyCampaign.GetMatchScore(campaignItem, childActivity) > 0)
				{
					pivot.ParentActivity = campaignItem;
				}
			}
		}

		public bool SendScheduledEmailToContact(GlbCompanyCampaignItem item)
		{
			if (!ShouldBeSentViaEmail(item))
			{
				return false;
			}

			var staff = item.CompanyCampaign.G0_EmailSenderOption != EmailSenderOptionCodeDescriptionList.Codes.EML
				? item.GetEmailSenderStaff()
				: null;

			if (staff != null && !staff.GS_IsActive)
			{
				item.G8_GS_NKSender = item.CompanyCampaign.G0_GS_NKCampaignCoordinator;
				item.G8_SenderEmailAddress = item.CompanyCampaign.CampaignCoordinator.GS_EmailAddress;
				item.G8_EmailSenderName = item.CompanyCampaign.CampaignCoordinator.GS_FullName;
			}

			SendEmailToContact(item);
			return true;
		}

		internal bool ShouldBeSentViaEmail(GlbCompanyCampaignItem sentCampaign) => EmailAddressValidation.IsEmailAddressValidAndNotEmpty(sentCampaign.Recipient.Email);

		internal void SendEmailToContact(GlbCompanyCampaignItem sentCampaign)
		{
			try
			{
				CampaignEmailTemplateEditor.AddTrackingImageLinkIfNotExists(sentCampaign.CompanyCampaign);
				var embedInHtmlImages = new Dictionary<string, byte[]>();

				var senderStaffPk = sentCampaign.GetEmailSenderStaff()?.PK;
				var email = senderStaffPk.HasValue ? new EmailDef(senderStaffPk.Value.ToGuid()) : new EmailDef();

				email.ContentType = EmailContentTypes.HTML;
				email.ReplyTo = sentCampaign.ReplyToEmailAddress;
				email.FromAddress = sentCampaign.SenderEmailAddress;
				email.Subject = EmailParser.Parse(CampaignDocumentParser.ParseType.PlainText, sentCampaign, campaign.G0_EmailSubject);
				email.FromDisplayName = FormatDisplayName(sentCampaign.SenderName);
				email.SetupBusinessEntityInfo(sentCampaign.PK, GlbCompanyCampaignItemSchema.Constants.Prefix, string.Empty);
				email.Body = EmailParser.Parse(CampaignDocumentParser.ParseType.HtmlEmail, sentCampaign, campaign.HtmlTextToSend, embedInHtmlImages);
				email.AddRecipientForUserCommunication(sentCampaign.Recipient.Email);
				email.QueueWithLowPriority = true;

				if (sentCampaign.CompanyCampaign.IsCampaignURLSettingsValid)
				{
					var unsubscribeUrl = EmailParser.Parse(CampaignDocumentParser.ParseType.PlainText, sentCampaign, "(*UnsubscribeFromMediaCategoryAndTypeUrl*)");
					if (!string.IsNullOrEmpty(unsubscribeUrl))
					{
						email.ListUnsubscribe = FormattableString.Invariant($"<{unsubscribeUrl}>");
					}
				}

				foreach (GlbCompanyCampaignAttachmentItem attachmentItem in campaign.CampaignAttachments)
				{
					if (attachmentItem.Selected)
					{
						AddAttachment(email, attachmentItem, campaign.DocManagerInfo.Files);
						AddAttachment(email, attachmentItem, campaign.DocManagerInfo.Documents);
					}
				}

				foreach (var image in embedInHtmlImages)
				{
					if (image.Value != null && image.Value.Length > 0)
					{
						email.Attachments.Add(new AttachmentDef(image.Key, image.Value));
					}
				}

				if (campaign.G0_StoreEmailInEDocs)
				{
					var eDocsFileName = sentCampaign.ClientOrg?.OH_Code + " " + sentCampaign.ContactName + ".eml";
					var refDocType = campaign.Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.PK, OrganisationsDataRegistry.Instance.PermanentlySavedCampaignEmailDocType.Value));

					Env.OutgoingMailManager.CreateAndAttachToEDocs(campaign.Factory, email, sentCampaign, eDocsFileName, refDocType?.RT_DocType, refDocType?.RT_DescMultilingual);
				}
				else
				{
					Env.OutgoingMailManager.Create(campaign.Factory, email); // we don't care about the return value - this only specifies if it was sent using direct sending
				}

				if (!IsScheduled)
				{
					sentCampaign.G8_LastSentTimeUtc = ZDateTime.UtcNow;
				}
			}
			catch (DocumentParsingFailedException e)
			{
				throw new DocumentParsingFailedException(e.Message, e);
			}
			catch (EmailSendFailedException)
			{
				// if direct sending fails the email will be sent by email batchprocessor later
			}
		}

		ZString FormatDisplayName(ZString displayName)
		{
			if (displayName.Contains(','))
			{
				displayName = "\"" + displayName + "\"";
			}

			return displayName;
		}

		void AddAttachment(EmailDef email, GlbCompanyCampaignAttachmentItem attachmentItem, IStorageDocsBaseCollection collection)
		{
			foreach (IeDoc file in ((BusinessObjectCollection)collection))
			{
				if (file.UniqueKey == attachmentItem.LinkedDoc.UniqueKey)
				{
					email.Attachments.Add(new AttachmentDef(file.FileName, file.ImageData));
				}
			}
		}

		#region Should Continue Sending Emails

		public bool OnShouldSendEmails(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignsToResend)
		{
			bool result = false;

			if (ShouldContinueWithSending != null)
			{
				result = ShouldContinueWithSending(numCampaignsToSend, campaignsToResend);
			}

			return result;
		}

		#endregion

		#region Show Campaigns Not Sent

		void ShowContactsNotSentTo(int numContactsSent, ReadOnlyCollection<CampaignContact> contactsNotSentTo)
		{
			if (ContactsNotSentCampaign != null)
			{
				ContactsNotSentCampaign(numContactsSent, contactsNotSentTo);
			}
		}

		void ShowContactsNotSentTo(Collection<IScheduleItemsProvider> contactsScheduled, ReadOnlyCollection<CampaignContact> contactsNotSentTo)
		{
			if (ContactsNotScheduledCampaign != null)
			{
				ContactsNotScheduledCampaign(null, new ContactsNotScheduledCampaignEventArgs(contactsScheduled, contactsNotSentTo));
			}
		}

		#endregion

		#region Send Schedule

		void OnSendScheduleCampaign(Collection<IScheduleItemsProvider> contactsToSchedule)
		{
			if (SendScheduleEvent != null)
			{
				SendScheduleEvent(null, new ContactsToSendToEventArgs(contactsToSchedule));
			}
		}

		#endregion

		#region Show Campaign Successfully Sent

		void ShowCampaignSuccessfullySent(int contactsDeliveredCount)
		{
			if (CampaignSuccessfullySent != null)
			{
				CampaignSuccessfullySent(contactsDeliveredCount);
			}
		}

		#endregion

		#region Campaign Sending Event

		void DisplayMessageOnCampaignSending(ZString message, ZString summary, bool isError)
		{
			if (MessageOnCampaignSending != null)
			{
				MessageOnCampaignSending(this, new MessageOnCampaignSendingEventArgs(message, summary, isError));
			}
		}

		public class MessageOnCampaignSendingEventArgs : EventArgs
		{
			public MessageOnCampaignSendingEventArgs(ZString message, ZString summary, bool isError)
			{
				this.Message = message;
				this.Summary = summary;
				this.IsError = isError;
			}

			public ZString Summary;
			public ZString Message;
			public bool IsError;
		}

		public event EventHandler CampaignSendBegin;
		void OnCampaignSendBegin()
		{
			if (CampaignSendBegin != null)
			{
				CampaignSendBegin(this, EventArgs.Empty);
			}
		}

		public event EventHandler CampaignSendEnd;
		void OnCampaignSendEnd()
		{
			if (CampaignSendEnd != null)
			{
				CampaignSendEnd(this, EventArgs.Empty);
			}
		}

		void OnItemSent(int sent, int total)
		{
			ItemSentEventArgs args = new ItemSentEventArgs(sent, total);
			if (ItemSent != null)
			{
				ItemSent(this, args);
			}
		}

		public void CancelSendingContacts()
		{
			fCancelSending = true;
		}
		bool fCancelSending;

		public event ItemSentEventHandler ItemSent;

		public event EventHandler ItemDeleteBegin;
		void OnItemDeleteBegin()
		{
			if (ItemDeleteBegin != null)
			{
				ItemDeleteBegin(this, EventArgs.Empty);
			}
		}

		public delegate void ItemSentEventHandler(object sender, ItemSentEventArgs e);

		public class ItemSentEventArgs : EventArgs
		{
			public ItemSentEventArgs(int sent, int total)
				: base()
			{
				this.Sent = sent;
				this.Total = total;
			}

			public int Sent;
			public int Total;
		}

		#endregion

		#region Campaign Email Parser

		CampaignDocumentParser EmailParser
		{
			get
			{
				if (fEmailParser == null)
				{
					fEmailParser = new CampaignDocumentParser(campaign.Factory);
				}

				return fEmailParser;
			}
		}

		public GlbCompanyCampaign Campaign
		{
			get { return campaign; }
		}

		CampaignDocumentParser fEmailParser;

		#endregion

		#endregion
		#region Notification Constants

		public static class NotificationConstants
		{
			public static string CorrectAllErrorsMessage
			{
				get { return Res.GetString("97b19f48-7da1-4914-90f7-b6ed3f9f728f", "All errors on this campaign must be corrected before campaigns can be sent."); }
			}
			public static string SaveCampaignMessage
			{
				get { return Res.GetString("02d4307b-b575-4e5b-acf9-aa40cfd20ec5", "Please save this campaign before sending it to contacts."); }
			}
			public static string EmailContentCannotBeEmpty
			{
				get { return Res.GetString("5209a8f4-e105-42e7-afa9-f9313dd10bcd", "Please enter an Email content."); }
			}
			public static string NoSelectedRecipientsMessage
			{
				get
				{
					return Res.GetString("4AC5A0ED-23BD-4F08-969A-2F9A8D36A0C3", @"You have not selected any contacts to send to.
Please press CTRL+A to select all contacts or CTRL + click each contact in the left-hand gutter to selectively highlight your recipients, followed by Send to Selected.");
				}
			}
			public static string HasBeenSentPreviouslyMessage
			{
				get { return Res.GetString("432c31b3-0f6b-43c0-b98c-cd1233ab24b9", "The campaign has been previously sent to this contact."); }
			}
			public static string CampaignsSentMessage
			{
				get { return Res.GetString("f25b7b66-e0d7-4627-bce9-12b017564ce8", "{0:G} campaigns were successfully sent"); }
			}

			public static string CampaignsTargetedListMessage => Res.GetString("e13e5ab7-e0bc-45f9-ae14-f5c51ed4f917", "{0:G} contacts were successfully added to the Target List.");

			public static string OpportunitiesCreatedMessage => Res.GetString("95F72842-E917-41B3-8CCF-69AF1FD5520A", "{0:G} opportunities were successfully set to be created now.");

			public static string CampaignsMasteredListMessage => Res.GetString("4B777670-2572-4F8E-B4F2-2FA8F9867C89", "{0:G} contacts were successfully added to the Master List.");

			public static string TooManyResultsErrorMessage
			{
				get { return Res.GetString("2943d32f-84d5-47e7-82c2-42f2b5b09445", "Too many records to display ({0:G}). Please fill in more of the search screen and then click 'Find'."); }
			}
			public static string SaveOrEmailFailedMessage
			{
				get { return Res.GetString("0227edc0-3e8a-4db3-acfa-5565b1fa8e3b", "Creation or emailing of the mail merge file for contacts without email addresses failed."); }
			}

			public static string CorrectAllErrorsOnRequeuingMessage
			{
				get { return Res.GetString("c0a638ec-0d4e-461a-95bf-b70bd9285af4", "All errors on this campaign must be corrected before this contact can be re-queued for printing."); }
			}
			public static string SaveCampaignOnRequeuingMessage
			{
				get { return Res.GetString("daa02946-7d7d-45c5-b85b-dbde2d9d7d9e", "Please save this campaign before re-queuing this contact for printing."); }
			}
			public static string ValidEmailAddressFoundOnRequeuingMessage
			{
				get { return Res.GetString("4133b12e-195d-4bcd-909c-55dcebef326e", "The selected contact(s) has a valid email address and cannot be re-queued for printing."); }
			}
			public static string ValidEmailAddressOnCampaignItemMessage
			{
				get { return Res.GetString("f5e72e9a-9200-4661-b245-4685a796162c", "This contact has a valid email address and cannot be re-queued for printing."); }
			}

			public static string CannotSendCampaignsSummary
			{
				get { return Res.GetString("9b541244-bd5c-45d9-9e02-c7d98fa11a28", "Cannot Send Campaigns"); }
			}
			public static string CampaignsSentSummary
			{
				get { return Res.GetString("74f8fc7f-b5ac-4d7b-a52e-8ac5755ac190", "Campaigns Sent"); }
			}
			public static string CampaignsTargetedSummary => Res.GetString("8e641f79-90aa-4b32-8dc7-4282ac8f37bc", "Contacts Targeted");

			public static string OpportunitiesCreatedSummary => Res.GetString("4CA52788-F561-4931-8296-53B2EAF71A52", "Opportunities Created");

			public static string CampaignsMasteredSummary => Res.GetString("F15151DC-EDBE-4DB1-A7E7-3E49E7517463", "Contacts Mastered");

			public static string CouldNotMergeSummary
			{
				get { return Res.GetString("0902feb4-2228-4473-b85f-a88dc1cac6a3", "Could not merge contact information"); }
			}
			public static string CannotRequeueSummary
			{
				get { return Res.GetString("2f40fb84-b823-45e7-85a1-9a34fe154ff1", "Cannot re-queue for printing"); }
			}

			public static string DocumentMergedFailedMessage
			{
				get
				{
					return Res.GetString("1af656d7-deef-4183-9a8e-19577f3b9d40", @"The document attached to this campaign could not be merged with contact information.
Please check that all fields used in the document are valid.

{0:G}

Campaign emails were not sent.");
				}
			}

			public static string SendingCampaignTrackedLinkValidationQuestion
			{
				get
				{
					return Res.GetString("bc8fbad9-b90f-4b3f-83a7-4e31af7d994a", "You are about to send this campaign to {0:G} contact(s). However no hyperlinks/images have been set for monitoring.\r\nIn order to capture a read receipt, destination URL's and/or images must be set for tracking. Would you like to do so?");
				}
			}

			public static string SelectElementsMessage
			{
				get { return Res.GetString("fbad192c-8986-49d9-a8c0-ce4ed1df42f6", "Please select an item from the grid."); }
			}
			public static string SelectElementSummary
			{
				get { return Res.GetString("36a6038b-5c8c-40ef-b702-aa822b75a65a", "Select item(s)"); }
			}
		}

		#endregion
	}
}
