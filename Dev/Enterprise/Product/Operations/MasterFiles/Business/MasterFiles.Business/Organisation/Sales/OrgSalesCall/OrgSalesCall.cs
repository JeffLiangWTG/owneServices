using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.MasterFiles.Business
{
	[UserDefinedValues]
	[UniversalDataContext(DataContextType.Communication)]
	[UniversalCopyWithExtendedEntities]
	[CodeProperty(OrgSalesCallSchema.Constants.OQ_CommunicationID)]
	[DescriptionProperty(OrgSalesCallSchema.Constants.OQ_CallSummary)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgSalesCall : AutoOrgSalesCall,
		IWorkflowProvider,
		IDocManagerSupport,
		ICustomFieldProvider,
		ISalesRelationActivity,
		ISalesValueAssociatedEntity,
		IImportParentRelatedActivityInfoOnNew,
		IDocumentSupportable,
		IJobNumber
	{
		public OrgSalesCall(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoOrgSalesCall.Schema
		{
			public const string OQ_CallDateLocal = "OQ_CallDateLocal";
			public const string OQ_NextCallLocal = "OQ_NextCallLocal";
			public const string OrgName = "OrgName";
			public const string ContactName = "ContactName";
			public const string ContactWorkPhone = "ContactWorkPhone";
			public const string ContactMobile = "ContactMobile";
			public const string ContactFax = "ContactFax";
			public const string ContactEmail = "ContactEmail";
			public const string ClientVisibleNote = "ClientVisibleNote";
		}

		#endregion

		#region Default Values and Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OQ_GS_NKSalesRep = GlbStaff.CurrentUser.GS_Code;
			OQ_Duration = TimeSpan.FromMinutes(30);
		}

		#endregion

		#region SavedReminderRecipients

		IEnumerable<CommunicationReminderRecipient> SavedReminderRecipients
		{
			get
			{
				EnsureSavedReminderRecipientsInitialized();
				return savedReminderRecipients;
			}
			set
			{
				savedReminderRecipients = value;
			}
		}
		IEnumerable<CommunicationReminderRecipient> savedReminderRecipients;

		void EnsureSavedReminderRecipientsInitialized()
		{
			if (savedReminderRecipients == null)
			{
				savedReminderRecipients = GetAllReminderRecipients();
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get { return (OQ_CallDate.IsValid) ? Res.GetString("8fc112bd-d734-416a-9498-3b51c4483b55", "Call Date") + " " + OQ_CallDate.ToString() : ""; }
		}

		#endregion

		#region Saving

		bool thisFactorySaveHasChanges;

		protected override void RunPreSaveValidationCore()
		{
			RefreshLinkedInquiryIfInitialized();
			base.RunPreSaveValidationCore();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (HasChanges || (Header != null && Header.HasChanges))
			{
				SynchroniseAssociatedTradeLanesPivots();
			}

			SalesRepInfoInfoHasChangesOnSaving = OQ_GS_NKSalesRepInfo.HasChanges;
			PropertyInfosThatTriggerAutoSendHasChangesOnSaving = PropertyInfosThatTriggerAutoSend.Any(info => info.HasChanges);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			thisFactorySaveHasChanges = HasChanges;
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded && thisFactorySaveHasChanges)
			{
				Dictionary<string, List<CommunicationReminderRecipient>> recipientsToCancelCalendarReminderGroupedByEmail = null;

				if (ShouldSendInvitation)
				{
					var recipientsToCancel = SalesRepInfoInfoHasChangesOnSaving ? GetSentRecipients() : GetSentRecipientsThatHaveBeenRemoved();
					recipientsToCancelCalendarReminderGroupedByEmail = CreateMutableLookup(recipientsToCancel, x => x.Email, StringComparer.OrdinalIgnoreCase);
				}

				SavedReminderRecipients = GetAllReminderRecipients();

				if (ShouldSendInvitation)
				{
					if (OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.Value)
					{
						Action<IEnumerable<CommunicationReminderRecipient>> presendAction = (sendRecipients) =>
							{
								foreach (var recipient in sendRecipients)
								{
									recipientsToCancelCalendarReminderGroupedByEmail.Remove(recipient.Email);
								}

								if (recipientsToCancelCalendarReminderGroupedByEmail.Count > 0)
								{
									CancelCalendarReminder(recipientsToCancelCalendarReminderGroupedByEmail.SelectMany(x => x.Value));
									recipientsToCancelCalendarReminderGroupedByEmail.Clear();
								}
							};

						SendCalendarReminder(true, presendAction);
					}

					if (recipientsToCancelCalendarReminderGroupedByEmail.Count > 0)
					{
						CancelCalendarReminder(recipientsToCancelCalendarReminderGroupedByEmail.SelectMany(x => x.Value));
					}
				}
			}

			SalesRepInfoInfoHasChangesOnSaving = false;
			PropertyInfosThatTriggerAutoSendHasChangesOnSaving = false;
		}

		static Dictionary<TKey, List<TElement>> CreateMutableLookup<TKey, TElement>(IEnumerable<TElement> elements, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			var result = new Dictionary<TKey, List<TElement>>(comparer);
			foreach (var element in elements)
			{
				TKey key = keySelector(element);

				List<TElement> grouping;
				if (!result.TryGetValue(key, out grouping))
				{
					grouping = new List<TElement>();
					result.Add(key, grouping);
				}

				grouping.Add(element);
			}

			return result;
		}

		public override void OnSaving()
		{
			UpdateHeaderSalesCalls();
			PopulateIdOnSaving();
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			base.OnSaving();
		}

		void UpdateHeaderSalesCalls()
		{
			if (!IsInDatabase || OQ_OHInfo.HasChanges || OQ_CallDateInfo.HasChanges || OQ_NextCallInfo.HasChanges)
			{
				if (Header != null)
				{
					Header.MiscServ.UpdateLastCallDateFromSalesCalls();
				}
				if (OQ_OHInfo.HasChanges)
				{
					var previousHeader = Factory.Load<OrgHeader>((ZGuid)OQ_OHInfo.OriginalValue);
					if (previousHeader != null)
					{
						previousHeader.MiscServ.UpdateLastCallDateFromSalesCalls();
					}
				}
			}
		}

		void PopulateIdOnSaving()
		{
			if (!IsInDatabase && OQ_CommunicationID.IsEmpty)
			{
				OQ_CommunicationID = Env.NumberFountains.CommunicationID.GetNextFormatted(Factory);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				OQ_CommunicationID = ZString.Empty;
			}

			base.OnSaved(saveSucceeded);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			var header = Header;

			WorkflowItems.RemoveAndDeleteAll();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			AssociatedTradeLanesPivots.DeleteAll();
			AdditionalAttendeesStaff.RemoveAndDeleteAll();
			AdditionalAttendeesContact.RemoveAndDeleteAll();
			AdditionalAttendeesOther.RemoveAndDeleteAll();
			SalesValueAssociatedEntity.DeleteAllSalesValuePivots(this);
			base.Delete();

			header?.MiscServ?.UpdateLastCallDateFromSalesCalls();
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber => OQ_CommunicationID;

		#endregion

		#region Testing Data
#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			OQ_CommunicationID = ZString.Empty;
			OQ_CallDate = ZDateTime.UtcNow.AddMonths(3).AddDays(2);
			OQ_CallSummary = "talked for a while - Random Number: " + new Random(DateTime.Now.Millisecond).Next(0, 100);
			OQ_FollowupNotes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf(ZString.Replicate('A', 79)));
			OQ_GS_NKSalesRep = GlbStaff.CurrentUser.GS_Code;
			OQ_NextCall = ZDateTime.UtcNow.AddMonths(4).AddMinutes(9);
			OQ_SalesCallNotes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf(ZString.Replicate('B', 79)));
			OQ_Status = Core.Constants.Sales.Status.HotProspect;
			OQ_TypeOfCall = Core.Constants.Sales.CommunicationType.FirstCall;
			foreach (OrgSalesCallAdditionalAttendee attendee in AdditionalAttendeesOther)
			{
				attendee.O6_AttendeeName = "other";
			}
			foreach (OrgSalesCallAdditionalAttendee attendee in AdditionalAttendeesContact)
			{
				attendee.O6_AttendeeName = "contact";
			}
			foreach (OrgSalesCallAdditionalAttendee attendee in AdditionalAttendeesStaff)
			{
				attendee.O6_AttendeeName = "staff";
			}
		}

#endif
		#endregion

		#region Trade Lane Integration

		#region Associated Trade Lanes Pivots

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgSalesCallTradeLanePivotCollection AssociatedTradeLanesPivots
		{
			get
			{
				if (fAssociatedTradeLanesPivots == null)
				{
					fAssociatedTradeLanesPivots = new OrgSalesCallTradeLanePivotCollection(this);
					RegisterEditableChildObject(fAssociatedTradeLanesPivots);
				}
				return fAssociatedTradeLanesPivots;
			}
		}
		OrgSalesCallTradeLanePivotCollection fAssociatedTradeLanesPivots;

		#endregion

		#region Binding

		void SynchroniseAssociatedTradeLanesPivots()
		{
			if (TradeProfileDescriptionList == null || Header == null)
			{
				return;
			}

			AssociatedTradeLanesPivots.DeleteObsoletePivots(Header.SalesCollection.ToArray<OrgSales>());
			foreach (ZBoolDescriptionPair pair in TradeProfileDescriptionList)
			{
				OrgSales tradeLane = (OrgSales)Header.SalesCollection.FindByPK(pair.PK);
				if (tradeLane == null)
				{
					continue;
				}

				if (pair.Value)
				{
					if (!AssociatedTradeLanesPivots.Contains(tradeLane))
					{
						AssociatedTradeLanesPivots.AddPivotFor(tradeLane);
					}
				}
				else
				{
					if (AssociatedTradeLanesPivots.Contains(tradeLane))
					{
						AssociatedTradeLanesPivots.DeletePivotFor(tradeLane);
					}
				}
			}
		}

		public ZBoolDescriptionPairList TradeProfileDescriptionList
		{
			get
			{
				if (fTradeProfileDescriptionList == null)
				{
					UpdateTradeProfileDescriptionList();
				}
				return fTradeProfileDescriptionList;
			}
		}
		ZBoolDescriptionPairList fTradeProfileDescriptionList;

		public void UpdateTradeProfileDescriptionList()
		{
			if (fTradeProfileDescriptionList != null)
			{
				SynchroniseAssociatedTradeLanesPivots();
			}
			else
			{
				fTradeProfileDescriptionList = new ZBoolDescriptionPairList();
				fTradeProfileDescriptionList.OnPairChanged += delegate
				{ HasChanges = true; };
			}
			fTradeProfileDescriptionList.Clear();

			if (Header != null)
			{
				foreach (OrgSales tradeLane in Header.SalesCollection.Cast<OrgSales>().Where(x => !x.IsDeleted && !x.IsActual).OrderBy(x => x.TradeLaneDetailedDescription))
				{
					fTradeProfileDescriptionList.AddNew(tradeLane.PK,
						tradeLane.TradeLaneDetailedDescription,
						AssociatedTradeLanesPivots.Contains(tradeLane));
				}
			}
		}

		#endregion

		#endregion

		#region Properties

		public bool ShouldIncludeOnSalesCallDocument { get; set; }

		[ReadOnly(true)]
		public override ZString OQ_CommunicationID
		{
			get { return base.OQ_CommunicationID; }
			set { base.OQ_CommunicationID = value; }
		}

		[List("Lookups.SalesReps")]
		public override ZString OQ_GS_NKSalesRep
		{
			get { return base.OQ_GS_NKSalesRep; }
			set
			{
				if (base.OQ_GS_NKSalesRep != value)
				{
					EnsureSavedReminderRecipientsInitialized();
					base.OQ_GS_NKSalesRep = value;
				}
			}
		}

		protected bool OQ_GS_NKSalesRep_ReadOnly
		{
			get { return IsInDatabase && !Env.Security.CommunicationManagerEditModifyStaffAssignment.IsAllowed; }
		}

		public override ZDateTime OQ_CallDate
		{
			get { return base.OQ_CallDate; }
			set
			{
				base.OQ_CallDate = value;
				Validation.ValidateOQ_NextCall();
			}
		}

		public ZDateTime OQ_CallDateLocal
		{
			get
			{
				return OQ_CallDate.IsValid ? Env.Time.GetLocalTimeFromUtc(OQ_CallDate.ToDateTime()) : OQ_CallDate;
			}
			set
			{
				OQ_CallDate = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
			}
		}

		public ZWrappedPropertyInfo OQ_CallDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OQ_CallDateLocal, x => OQ_CallDateInfo); }
		}

		[ZDateTimeDurationValue]
		public override ZDateTime OQ_Duration
		{
			get => base.OQ_Duration;
			set => base.OQ_Duration = value.ConvertToDurationBasedDate(OQ_DurationInfo);
		}

		public override ZDateTime OQ_NextCall
		{
			get { return base.OQ_NextCall; }
			set
			{
				base.OQ_NextCall = value;
				Validation.ValidateOQ_CallDate();
			}
		}

		public ZDateTime OQ_NextCallLocal
		{
			get
			{
				return OQ_NextCall.IsValid ? Env.Time.GetLocalTimeFromUtc(OQ_NextCall.ToDateTime()) : OQ_NextCall;
			}
			set
			{
				OQ_NextCall = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
			}
		}

		public ZWrappedPropertyInfo OQ_NextCallLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OQ_NextCallLocal, x => OQ_NextCallInfo); }
		}

		public ZDateTime DateLocal
		{
			get { return OQ_CallDateLocal.IsValid ? OQ_CallDateLocal : OQ_NextCallLocal; }
		}

		public ZDateTime OQ_SystemCreateTimeLocal
		{
			get { return OQ_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		public ZDateTime OQ_SystemLastEditTimeLocal
		{
			get { return OQ_SystemLastEditTimeUtc.ToLocalBranchTime(); }
		}

		[List("Lookups.OQ_TypeOfCall_ActiveList")]
		public override ZString OQ_TypeOfCall
		{
			get { return base.OQ_TypeOfCall; }
			set { base.OQ_TypeOfCall = value; }
		}

		public ZString OQ_TypeOfCallDescription
		{
			get { return Lookups.OQ_TypeOfCall_List.GetDescriptionFromCode(OQ_TypeOfCall); }
		}

		[List("Lookups.OQ_Status_ActiveList")]
		public override ZString OQ_Status
		{
			get { return base.OQ_Status; }
			set { base.OQ_Status = value; }
		}

		[List("Lookups.OQ_Category_ActiveList")]
		public override ZString OQ_Category
		{
			get { return base.OQ_Category; }
			set { base.OQ_Category = value; }
		}

		#region OQ_SalesCallNotes_HTML

		public ZBlob OQ_SalesCallNotes_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(OQ_SalesCallNotes);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.OQ_SalesCallNotes = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		#endregion

		public ZString OQ_CategoryDescription
		{
			get
			{
				return Lookups.OQ_Category_List.GetDescriptionFromCode(OQ_Category);
			}
		}

		public ZPropertyInfo OQ_CategoryDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(OQ_CategoryDescription)); }
		}

		public ZString OQ_StatusDescription
		{
			get { return Lookups.OQ_Status_List.GetDescriptionFromCode(OQ_Status); }
		}

		#region Extra Category

		public ZString ExtraCategoryDescription
		{
			get { return OrganisationsDataRegistry.Instance.CategoryListLabel.Value; }
		}

		#endregion

		#region IsClosed

		public ZBool IsClosed
		{
			get { return OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.GetClosedFromCode(OQ_Status); }
		}

		#endregion

		#region OverallDisposition

		public ZString OverallDisposition
		{
			get { return IsClosed ? OrgSalesCallOverallDispositionList.Codes.Closed : OrgSalesCallOverallDispositionList.Codes.Open; }
		}

		public ZString OverallDispositionDescription
		{
			get { return Lookups.OverallDispositionList.GetDescriptionFromCode(OverallDisposition); }
		}

		#endregion

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		[List("Lookups.Headers")]
		public override ZGuid OQ_OH
		{
			get { return base.OQ_OH; }
			set
			{
				if (value != base.OQ_OH)
				{
					base.OQ_OH = value;

					if (value.IsValid)
					{
						LinkedInquiry = null;
					}

					OQ_OC = ZGuid.Empty;
					OQ_OA_LocationAddress = ZGuid.Empty;
					UpdateTradeProfileDescriptionList();
				}
			}
		}

		public ZString OrgName
		{
			get
			{
				if (IsLinkedToInquiry)
				{
					return LinkedInquiry.O1_CompanyName;
				}
				else
				{
					var header = Header;
					if (header != null)
					{
						return header.OH_FullName;
					}
				}

				return ZString.Empty;
			}
		}

		public bool OQ_OH_ReadOnly
		{
			get { return !Env.Security.CommunicationManagerEditClientDetails.IsAllowed; }
		}

		[List("Lookups.OrgActiveContactsPlusExistingContact")]
		public override ZGuid OQ_OC
		{
			get { return base.OQ_OC; }
			set
			{
				var oldContact = Contact;
				base.OQ_OC = value;
				var newContact = Contact;

				if (newContact != oldContact)
				{
					RemovePrimaryContactFromAttendees(oldContact);
					AddPrimaryContactToAttendees(newContact);
				}
			}
		}

		public bool OQ_OC_ReadOnly
		{
			get { return !Env.Security.CommunicationManagerEditClientDetails.IsAllowed; }
		}

		void RemovePrimaryContactFromAttendees(OrgContact primaryContact)
		{
			if (primaryContact != null)
			{
				foreach (var attendee in AdditionalAttendeesContact.Cast<OrgSalesCallAdditionalAttendee>().Where(attendee => attendee.O6_AttendeeID == primaryContact.PK).ToArray())
				{
					if (!attendee.O6_ReceiverReminder)
					{
						AdditionalAttendeesContact.RemoveAndDelete(attendee);
					}
				}
			}
		}

		void AddPrimaryContactToAttendees(OrgContact primaryContact)
		{
			if (primaryContact != null)
			{
				if (!AdditionalAttendeesContact.Cast<OrgSalesCallAdditionalAttendee>().Any(attendee => attendee.O6_AttendeeID == primaryContact.PK))
				{
					var contactAttendee = AdditionalAttendeesContact.AddNew();
					contactAttendee.O6_AttendeeID = primaryContact.PK;
				}
			}
		}

		#region Contact

		public ZString ContactName
		{
			get
			{
				if (IsLinkedToInquiry)
				{
					return LinkedInquiry.O1_ContactName;
				}
				else
				{
					var contact = Contact;
					if (contact != null)
					{
						return contact.OC_ContactName;
					}
				}

				return ZString.Empty;
			}
		}

		public ZString ContactWorkPhone
		{
			get
			{
				if (IsLinkedToInquiry)
				{
					return LinkedInquiry.O1_Phone;
				}
				else
				{
					var contact = Contact;
					if (contact != null)
					{
						if (!contact.OC_Phone.IsEmpty)
						{
							return contact.OC_Phone;
						}

						var org = Header;
						if (org != null)
						{
							return org.MainAddress.OA_Phone;
						}
					}
				}

				return ZString.Empty;
			}
		}

		public ZString ContactMobile
		{
			get
			{
				if (IsLinkedToInquiry)
				{
					return LinkedInquiry.O1_Mobile;
				}
				else
				{
					var contact = Contact;
					if (contact != null)
					{
						return contact.OC_Mobile;
					}
				}

				return ZString.Empty;
			}
		}

		public ZString ContactFax
		{
			get
			{
				if (IsLinkedToInquiry)
				{
					return LinkedInquiry.O1_Fax;
				}
				else
				{
					var contact = Contact;
					if (contact != null)
					{
						return contact.OC_Fax;
					}
				}

				return ZString.Empty;
			}
		}

		public ZString ContactEmail
		{
			get
			{
				if (IsLinkedToInquiry)
				{
					return LinkedInquiry.O1_Email;
				}
				else
				{
					var contact = Contact;
					if (contact != null)
					{
						return contact.OC_Email;
					}
				}

				return ZString.Empty;
			}
		}

		#endregion

		[List("Lookups.ActiveLocationList")]
		[MaxLength(Schema.OQ_LocationTextMaxLength)]
		public ZString Location
		{
			get
			{
				ZString result;
				var map = Lookups.LocationPkCodeMap;
				if (LocationAddress != null && map.ContainsKey(LocationAddress.PK))
				{
					result = map[LocationAddress.PK];
				}
				else if (LocationResource != null && map.ContainsKey(LocationResource.PK))
				{
					result = map[LocationResource.PK];
				}
				else
				{
					result = OQ_LocationText;
				}
				return result;
			}
			set
			{
				if (value != Location)
				{
					OQ_OA_LocationAddress = ZGuid.Empty;
					OQ_GS_NKLocationResource = ZString.Empty;
					OQ_LocationText = ZString.Empty;

					var map = Lookups.LocationCodePkMap;
					ZGuid locationPk;
					if (map.TryGetValue(value, out locationPk))
					{
						var orgAddress = Factory.Load<OrgAddress>(locationPk);
						if (orgAddress != null)
						{
							OQ_OA_LocationAddress = orgAddress.PK;
						}
						else
						{
							var locationResource = Factory.Load<GlbStaff>(locationPk);
							if (locationResource != null)
							{
								OQ_GS_NKLocationResource = locationResource.GS_Code;
							}
						}
					}
					else
					{
						OQ_LocationText = value.SubstringSafe(0, OQ_LocationTextInfo.MaxLength);
					}
				}

				LocationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LocationInfo
		{
			get { return GetZPropertyInfo(nameof(Location)); }
		}

		public bool IsInvitationSent
		{
			get { return LastSentReminderLog != null; }
		}

		public ZBool ShouldSendInvitation
		{
			get { return shouldSendInvitation; }
			set
			{
				SetNonPersistentPropertyValue(ShouldSendInvitationInfo, ref shouldSendInvitation, value);
				Validation.ValidateShouldSendInvitation();
			}
		}
		ZBool shouldSendInvitation = ZBool.True;

		public ZPropertyInfo ShouldSendInvitationInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldSendInvitation)); }
		}

		public ZString LastInvitationActionDescription
		{
			get
			{
				int totalSentReminders = 0;
				int totalCancelledReminders = 0;
				ZDateTime latestSent = ZDateTime.MinSmallDateTimeValue;
				ZDateTime latestCancelled = ZDateTime.MinSmallDateTimeValue;

				if (SavedReminderRecipients.Any())
				{
					foreach (var recipient in SavedReminderRecipients)
					{
						var reminderLog = GetLastReminderLog(recipient);
						if (reminderLog != null)
						{
							if (CommunicationReminder.IsReminderSentLog(reminderLog))
							{
								totalSentReminders++;
								latestSent = (latestSent < reminderLog.PostedLocalBranchTime) ? reminderLog.PostedLocalBranchTime : latestSent;
							}
							else
							{
								totalCancelledReminders++;
								latestCancelled = (latestCancelled < reminderLog.PostedLocalBranchTime) ? reminderLog.PostedLocalBranchTime : latestCancelled;
							}
						}
					}
				}
				else
				{
					var lastCancelledLogQuery = new ZQuery();
					lastCancelledLogQuery.AddToFilter(StmALogSchema.SL_Parent, PK);
					lastCancelledLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Reminder.Constants.EventCode);
					lastCancelledLogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Reminder.Constants.ReferencePrefix + CommunicationReminder.Constants.ReminderIdentifier);
					lastCancelledLogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Type:" + CommunicationReminder.Constants.TypeCancelCode);
					lastCancelledLogQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
					var lastCancelledLog = Factory.LoadTop1<StmALog>(lastCancelledLogQuery);
					if (lastCancelledLog != null)
					{
						latestCancelled = lastCancelledLog.PostedLocalBranchTime;
						totalCancelledReminders++;
					}
				}

				if (totalSentReminders > 0)
				{
					return Res.GetString("afc6a2cc-37d8-4265-bc85-4314b2d04a3f", "Invitation sent:") + " " + latestSent.ToLongTimeString();
				}
				else if (totalCancelledReminders > 0)
				{
					return Res.GetString("35bd5a50-92b6-4133-987a-c8808faff71f", "Invitation canceled:") + " " + latestCancelled.ToLongTimeString();
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo LastInvitationActionDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(LastInvitationActionDescription)); }
		}

		public ZBlob OQ_FollowupNotes_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(OQ_FollowupNotes);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.OQ_FollowupNotes = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public bool OQ_SalesCallNotes_ReadOnly
		{
			get { return !Env.Security.CommunicationManagerEditNotes.IsAllowed; }
		}

		public bool OQ_FollowupNotes_ReadOnly
		{
			get { return !Env.Security.CommunicationManagerEditFollowUpNotes.IsAllowed; }
		}

		#region Client Visible Note

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString ClientVisibleNote
		{
			get
			{
				if (!clientVisibleNote.HasValue)
				{
					var query = new ZQuery(StmNoteSchema.ST_ParentID, PK);
					query.AddToFilter(StmNoteSchema.ST_Table, TableName);
					query.AddToFilter(StmNoteSchema.ST_Description, Schema.ClientVisibleNote);
					query.FetchOnlyFromLocalCache = !IsInDatabase; // Avoid unnecessary hit to DB if new
					clientVisibleNoteStmNote = Factory.LoadTop1<HiddenStmNote>(query);

					clientVisibleNote = clientVisibleNoteStmNote != null ? clientVisibleNoteStmNote.ST_NoteText : ZString.Empty;
				}

				return clientVisibleNote.Value;
			}
			set
			{
				if (ClientVisibleNote != value)
				{
					clientVisibleNote = value;
					HasChanges = true;

					if (!clientVisibleNote.Value.IsEmpty && clientVisibleNoteStmNote == null)
					{
						clientVisibleNoteStmNote = Factory.New<HiddenStmNote>();
						clientVisibleNoteStmNote.ST_ParentID = PK;
						clientVisibleNoteStmNote.ST_Table = TableName;
						clientVisibleNoteStmNote.ST_Description = Schema.ClientVisibleNote;
					}

					clientVisibleNoteStmNote.ST_NoteText = clientVisibleNote.Value;

					clientVisibleNoteStmNote.RefreshBinding();
					ClientVisibleNoteInfo.RefreshBinding();
				}
			}
		}
		ZString? clientVisibleNote;
		HiddenStmNote clientVisibleNoteStmNote;

		public ZPropertyInfo ClientVisibleNoteInfo
		{
			get { return GetZPropertyInfo(Schema.ClientVisibleNote); }
		}

		#endregion

		#region Workflow

		public ProcessTask CurrentTask
		{
			get
			{
				var processTasks = WorkflowItems.Tasks.Cast<ProcessTask>();
				var currentTaskCandidates = processTasks.Where(t => t.IsCurrent);

				if (!currentTaskCandidates.Any())
				{
					currentTaskCandidates = processTasks.Where(t => t.P9_Status == ProcessTaskStatusCodeList.Codes.Open);
				}

				return currentTaskCandidates.OrderBy(t => t.P9_Sequence).FirstOrDefault();
			}
		}

		public ZString TaskStatus
		{
			get
			{
				var taskStatus = "";

				if (WorkflowItems.Count > 0)
				{
					if (CurrentTask != null)
					{
						taskStatus = CurrentTask.P9_Status;
					}
					else
					{
						var processTasks = WorkflowItems.Tasks.Cast<ProcessTask>();
						if (processTasks.Any(t => t.P9_Status == ProcessTaskStatusCodeList.Codes.Closed))
						{
							taskStatus = ProcessTaskStatusCodeList.Codes.Closed;
						}
						else
						{
							taskStatus = ProcessTaskStatusCodeList.Codes.Cancelled;
						}
					}
				}

				return taskStatus;
			}
		}

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = ResString.GetMultilingualString("e477da50-78ea-4a36-94e0-aec8d029206d", "Communication");
				if (!IsDeleted && !OQ_CommunicationID.IsEmpty)
				{
					result += " (" + OQ_CommunicationID + ")";
				}

				return result;
			}
		}

		#endregion

		[ChildEditable(true)]
		public RelatedActivityLinkCollection RelatedActivityLinkCollection
		{
			get
			{
				if (relatedActivityLinkCollection == null)
				{
					relatedActivityLinkCollection = new RelatedActivityLinkCollection(this);
					if (IsLinkedToInquiry)
					{
						relatedActivityLinkCollection.SetReadOnlyIncludingChildren(true);
					}
					else
					{
						RegisterEditableChildObject(relatedActivityLinkCollection);
					}
				}

				return relatedActivityLinkCollection;
			}
		}
		RelatedActivityLinkCollection relatedActivityLinkCollection;

		#endregion

		#region LinkedInquiry

		public bool IsLinkedToInquiry
		{
			get { return LinkedInquiry != null; }
		}

		public SalesEnquiry LinkedInquiry
		{
			get
			{
				if (!linkedInquiryInitialized)
				{
					RefreshLinkedInquiry();
				}
				return linkedInquiry;
			}
			set
			{
				linkedInquiryInitialized = true;

				if (linkedInquiry != value)
				{
					var wasLinked = IsLinkedToInquiry;

					if (value != null)
					{
						DeleteAllRelatedActivitiesExcludingLinkedInquiry(value);

						OQ_OH = ZGuid.Empty;
						OQ_OC = ZGuid.Empty;
					}

					linkedInquiry = value;

					if (wasLinked != IsLinkedToInquiry)
					{
						UpdateLinkedInquiryDependantCollections();
					}
					OnLinkedInquiryChanged();
				}
			}
		}
		SalesEnquiry linkedInquiry;
		bool linkedInquiryInitialized;

		void DeleteAllRelatedActivitiesExcludingLinkedInquiry(SalesEnquiry linkedInquiry)
		{
			bool hasRelatedActivityPivotForInquiry = false;
			foreach (var parentActivityPivot in RelatedParentActivityPivotCollection.ToArray())
			{
				if (parentActivityPivot.ParentActivity.PK == linkedInquiry.PK)
				{
					hasRelatedActivityPivotForInquiry = true;
				}
				else if (parentActivityPivot.ParentActivity.ActivityType == RelatableActivityTypeList.Codes.CampaignManagement)
				{
					hasRelatedActivityPivotForInquiry = true;
				}
				else
				{
					RelatedParentActivityPivotCollection.Delete(parentActivityPivot);
				}
			}

			if (!hasRelatedActivityPivotForInquiry)
			{
				RelatedParentActivityPivotCollection.AddNewPivot(linkedInquiry);
			}

			RelatedChildActivityPivotCollection.DeleteAll();
		}

		void UpdateLinkedInquiryDependantCollections()
		{
			if (relatedActivityLinkCollection != null)
			{
				relatedActivityLinkCollection.SetReadOnlyIncludingChildren(IsLinkedToInquiry);
				if (IsLinkedToInquiry)
				{
					RegisterEditableChildObject(relatedActivityLinkCollection);
				}
				else
				{
					UnRegisterEditableChildObject(relatedActivityLinkCollection);
				}
			}

			if (relatedChildActivityPivotCollection != null)
			{
				relatedChildActivityPivotCollection.SetReadOnlyIncludingChildren(IsLinkedToInquiry);
			}

			if (relatedParentActivityPivotCollection != null)
			{
				relatedParentActivityPivotCollection.SetReadOnlyIncludingChildren(IsLinkedToInquiry);
			}
		}

		void OnLinkedInquiryChanged()
		{
			if (LinkedInquiryChanged != null)
			{
				LinkedInquiryChanged(this, EventArgs.Empty);
			}
		}
		public event EventHandler LinkedInquiryChanged;

		void RefreshLinkedInquiry()
		{
			LinkedInquiry = CalculateLinkedInquiry();
		}

		void RefreshLinkedInquiryIfInitialized()
		{
			if (linkedInquiryInitialized)
			{
				RefreshLinkedInquiry();
			}
		}

		SalesEnquiry CalculateLinkedInquiry()
		{
			if (OQ_OH.IsValid)
			{
				return null;
			}

			if (RelatedChildActivityPivotCollection.Any(pivot => pivot.IsInDatabase))
			{
				return null;
			}

			SalesEnquiry inquiry = null;
			foreach (var pivot in RelatedParentActivityPivotCollection)
			{
				var pivotActivityAsCampaignItem = pivot.ParentActivity as IGlbCompanyCampaignItem;

				var pivotActivityAsInquiry = pivot.ParentActivity as SalesEnquiry;
				if (pivotActivityAsInquiry != null)
				{
					inquiry = pivotActivityAsInquiry;
				}
				else if (pivotActivityAsCampaignItem != null)
				{
					return Factory.Load<SalesEnquiry>(pivotActivityAsCampaignItem.G8_RecipientID);
				}
				else if (pivot.IsInDatabase)
				{
					return null;
				}
			}

			if (inquiry == null || inquiry.O1_OH_ConvertedToQualifiedLeadInfo.OriginalValue.IsValid)
			{
				return null;
			}

			return inquiry;
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			RefreshLinkedInquiryIfInitialized();
		}

		#endregion

		#region Additional Attendees

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgSalesCallAdditionalAttendeeStaffCollection AdditionalAttendeesStaff
		{
			get
			{
				if (fAdditionalAttendeesStaff == null)
				{
					fAdditionalAttendeesStaff = new OrgSalesCallAdditionalAttendeeStaffCollection(this);
					fAdditionalAttendeesStaff.Load();
					RegisterEditableChildObject(fAdditionalAttendeesStaff);
					EnsureSavedReminderRecipientsInitialized();
				}
				return fAdditionalAttendeesStaff;
			}
		}
		OrgSalesCallAdditionalAttendeeStaffCollection fAdditionalAttendeesStaff;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgSalesCallAdditionalAttendeeContactCollection AdditionalAttendeesContact
		{
			get
			{
				if (fAdditionalAttendeesContact == null)
				{
					fAdditionalAttendeesContact = new OrgSalesCallAdditionalAttendeeContactCollection(this);
					fAdditionalAttendeesContact.Load();
					fAdditionalAttendeesContact.ReceiveReminderHasValueChanged += AdditionalAttendeesContact_ReceiveReminderValueChanged;
					RegisterEditableChildObject(fAdditionalAttendeesContact);
					EnsureSavedReminderRecipientsInitialized();
				}
				return fAdditionalAttendeesContact;
			}
		}
		OrgSalesCallAdditionalAttendeeContactCollection fAdditionalAttendeesContact;

		void AdditionalAttendeesContact_ReceiveReminderValueChanged(object sender, AdditionalAttendeeEventArgs e)
		{
			if (e.ShouldReceiveReminder)
			{
				OQ_IsReminderClientFacing = true;
			}
		}

		public bool HasRiskOfSendingInternalNotesToAttendees
		{
			get
			{
				var result = false;
				var hasUnsavedShouldReceiveReminderAttendee = AdditionalAttendeesContact.Cast<OrgSalesCallAdditionalAttendee>().Any(c => c.O6_ReceiverReminder || (!c.O6_ReceiverReminder && c.O6_ReceiverReminderInfo.HasChanges));
				if (!OQ_IsReminderClientFacing && hasUnsavedShouldReceiveReminderAttendee)
				{
					result = true;
				}
				return result;
			}
		}

		public void FixRiskOfSendingInternalNotesToAttendees()
		{
			OQ_IsReminderClientFacing = true;
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgSalesCallAdditionalAttendeeOtherCollection AdditionalAttendeesOther
		{
			get
			{
				if (fAdditionalAttendeesOther == null)
				{
					fAdditionalAttendeesOther = new OrgSalesCallAdditionalAttendeeOtherCollection(this);
					fAdditionalAttendeesOther.Load();
					RegisterEditableChildObject(fAdditionalAttendeesOther);
				}
				return fAdditionalAttendeesOther;
			}
		}
		OrgSalesCallAdditionalAttendeeOtherCollection fAdditionalAttendeesOther;

		#endregion

		#region Calendar Reminders

		bool SalesRepInfoInfoHasChangesOnSaving;
		bool PropertyInfosThatTriggerAutoSendHasChangesOnSaving;

		internal IEnumerable<ZPropertyInfo> PropertyInfosThatTriggerAutoSend
		{
			get
			{
				yield return OQ_OHInfo;
				yield return OQ_OCInfo;
				yield return OQ_NextCallInfo;
				yield return OQ_DurationInfo;
				yield return OQ_OA_LocationAddressInfo;
				yield return OQ_GS_NKLocationResourceInfo;
				yield return OQ_LocationTextInfo;
				yield return OQ_GS_NKSalesRepInfo;
			}
		}

		#region GetReminderRecipients

		IEnumerable<CommunicationReminderRecipient> GetAllReminderRecipients()
		{
			return GetReminderRecipients(salesRep => true, attendee => true);
		}

		IEnumerable<CommunicationReminderRecipient> GetCurrentReminderRecipients()
		{
			return GetReminderRecipients(salesRep => true, attendee => attendee.O6_ReceiverReminder);
		}

		IEnumerable<CommunicationReminderRecipient> GetReminderRecipients(Func<GlbStaff, bool> includeSalesRepPredicate, Func<OrgSalesCallAdditionalAttendee, bool> includeAdditionalAttendeePredicate)
		{
			var reminderRecipients = new List<CommunicationReminderRecipient>();
			var reminderRecipientEmails = new HashSet<string>();

			if (SalesRep != null)
			{
				var salesRepEmail = SalesRep.GS_EmailAddress;
				if (!salesRepEmail.IsEmpty && !reminderRecipientEmails.Contains(salesRepEmail) && includeSalesRepPredicate(SalesRep))
				{
					reminderRecipients.Add(new CommunicationReminderRecipient(SalesRep.PK, SalesRep.GS_FullName, salesRepEmail));
					reminderRecipientEmails.Add(salesRepEmail);
				}
			}

			foreach (OrgSalesCallAdditionalAttendee contact in AdditionalAttendeesContact)
			{
				var contactEmail = contact.Email;
				if (contact.O6_ReceiverReminder && !contactEmail.IsEmpty && !reminderRecipientEmails.Contains(contactEmail) && includeAdditionalAttendeePredicate(contact))
				{
					reminderRecipients.Add(new CommunicationReminderRecipient(contact.PK, contact.Name, contactEmail));
					reminderRecipientEmails.Add(contactEmail);
				}
			}

			foreach (OrgSalesCallAdditionalAttendee staff in AdditionalAttendeesStaff)
			{
				var staffEmail = staff.Email;
				if (staff.O6_ReceiverReminder && !staffEmail.IsEmpty && !reminderRecipientEmails.Contains(staffEmail) && includeAdditionalAttendeePredicate(staff))
				{
					reminderRecipients.Add(new CommunicationReminderRecipient(staff.PK, staff.Name, staffEmail));
					reminderRecipientEmails.Add(staffEmail);
				}
			}

			return reminderRecipients;
		}

		#endregion

		#region LastSentReminder

		bool lastSentReminderLogInitialised;
		StmALog lastSentReminderLog;
		StmALog LastSentReminderLog
		{
			get
			{
				if (!lastSentReminderLogInitialised)
				{
					lastSentReminderLogInitialised = true;
					lastSentReminderLog = GetLastSentReminderLog();
				}
				return lastSentReminderLog;
			}
			set
			{
				lastSentReminderLogInitialised = true;
				lastSentReminderLog = value;
			}
		}

		StmALog GetLastSentReminderLog()
		{
			StmALog lastSentReminderLog = null;
			foreach (var recipient in SavedReminderRecipients)
			{
				var recipientReminderLog = GetLastReminderLog(recipient);
				if (recipientReminderLog != null && CommunicationReminder.IsReminderSentLog(recipientReminderLog))
				{
					if (lastSentReminderLog == null || lastSentReminderLog.SL_PostedTimeUtc < recipientReminderLog.SL_PostedTimeUtc)
					{
						lastSentReminderLog = recipientReminderLog;
					}
				}
			}

			return lastSentReminderLog;
		}

		#endregion

		public SendCalendarReminderResult SendCalendarReminder()
		{
			return SendCalendarReminder(false, null);
		}

		SendCalendarReminderResult SendCalendarReminder(bool isAutoSend, Action<IEnumerable<CommunicationReminderRecipient>> presendAction)
		{
			if (!Env.Security.CommunicationManagerAllowSendInvitation.IsAllowed)
			{
				return SendCalendarReminderResult.Failure(Env.Security.CommunicationManagerAllowSendInvitation.ErrorMessageForNotAllowed);
			}

			if (!isAutoSend && HasChanges)
			{
				return SendCalendarReminderResult.Failure(Res.GetString("7c94edb2-14c7-4b08-8351-4c7cd3160c49", "Please save changes before sending invitation."));
			}

			if (OQ_NextCall.IsEmpty)
			{
				return SendCalendarReminderResult.Failure(Res.GetString("bb93f924-a454-4ca3-a3d2-ba2868e6adc0", "Cannot send invitation because scheduled date is blank."));
			}

			if (!OQ_CallDate.IsEmpty)
			{
				return SendCalendarReminderResult.Failure(Res.GetString("8f0c8191-7dee-4850-9820-f413480abc51", "Cannot send invitation because there is already an actual date."));
			}

			var reminderRecipients = GetCurrentReminderRecipients();
			if (!reminderRecipients.Any())
			{
				return SendCalendarReminderResult.Failure(Res.GetString("3ba4b605-af04-4765-84aa-dfba943f8411", "Cannot send invitation because there is no recipient or all recipients have no email addresses."));
			}

			var shouldSendToEveryone = !IsInvitationSent || (isAutoSend ? PropertyInfosThatTriggerAutoSendHasChangesOnSaving : OQ_SystemLastEditTimeUtc > LastSentReminderLog.SL_PostedTimeUtc);
			if (!shouldSendToEveryone)
			{
				reminderRecipients = reminderRecipients.Where(recipient => !IsReminderSentFor(recipient)).ToList();
				if (!reminderRecipients.Any())
				{
					return SendCalendarReminderResult.Failure(Res.GetString("316c9572-e8a2-4bba-b614-877f42d8658e", "Cannot send invitation because communication details have not changed."));
				}
			}

			if (presendAction != null)
			{
				presendAction(reminderRecipients);
			}

			var reminder = SendCalendarReminder(reminderRecipients);
			return SendCalendarReminderResult.Success(reminder);
		}

		Reminder SendCalendarReminder(IEnumerable<CommunicationReminderRecipient> recipients)
		{
			var originalReminderDate = LastSentReminderLog != null ? CommunicationReminder.GetReminderDateFromLog(LastSentReminderLog) : ZDateTime.Empty;
			var reminder = CommunicationReminder.New(originalReminderDate, OQ_NextCall, this, recipients, OQ_IsReminderClientFacing);
			reminder.CreateAppointment();

			OnReminderSent(reminder);
			return reminder;
		}

		public SendCalendarReminderResult CancelCalendarReminder()
		{
			if (!Env.Security.CommunicationManagerAllowSendInvitation.IsAllowed)
			{
				return SendCalendarReminderResult.Failure(Env.Security.CommunicationManagerAllowSendInvitation.ErrorMessageForNotAllowed);
			}

			if (HasChanges)
			{
				return SendCalendarReminderResult.Failure(Res.GetString("cb6c088a-9ef9-4bad-b0e4-af0f384a9df7", "Please save changes before canceling invitation."));
			}

			if (LastSentReminderLog == null)
			{
				return SendCalendarReminderResult.Failure(Res.GetString("c1ce6871-02d0-42a8-9efb-67597c4d3dd8", "Cannot cancel invitation because it has not been sent yet."));
			}

			var reminder = CancelCalendarReminder(GetCurrentReminderRecipients());
			return SendCalendarReminderResult.Success(reminder);
		}

		Reminder CancelCalendarReminder(IEnumerable<CommunicationReminderRecipient> recipients)
		{
			var originalReminderDate = LastSentReminderLog != null ? CommunicationReminder.GetReminderDateFromLog(LastSentReminderLog) : ZDateTime.Empty;
			var cancelReminder = CommunicationReminder.New(originalReminderDate, ZDateTime.Empty, this, recipients, OQ_IsReminderClientFacing);
			cancelReminder.CreateAppointment();

			OnReminderSent(cancelReminder);
			return cancelReminder;
		}

		IEnumerable<CommunicationReminderRecipient> GetSentRecipients()
		{
			return SavedReminderRecipients.Where(x => IsReminderSentFor(x));
		}

		IEnumerable<CommunicationReminderRecipient> GetSentRecipientsThatHaveBeenRemoved()
		{
			var currentReminderRecipients = GetCurrentReminderRecipients();
			var currentReminderRecipientEmails = new HashSet<string>(currentReminderRecipients.Select(x => x.Email), StringComparer.OrdinalIgnoreCase);
			return SavedReminderRecipients.Where(recipient => !currentReminderRecipientEmails.Contains(recipient.Email) && IsReminderSentFor(recipient));
		}

		StmALog GetLastReminderLog(CommunicationReminderRecipient recipient)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Reminder.Constants.EventCode);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Reminder.Constants.ReferencePrefix + CommunicationReminder.Constants.ReminderIdentifier + " Recipient:" + recipient.ParentPk.ToString());
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			return Factory.LoadTop1<StmALog>(query);
		}

		bool IsReminderSentFor(CommunicationReminderRecipient recipient)
		{
			var lastReminderLog = GetLastReminderLog(recipient);
			return (lastReminderLog != null) && CommunicationReminder.IsReminderSentLog(lastReminderLog);
		}

		protected virtual void OnReminderSent(CommunicationReminder reminder)
		{
			LastSentReminderLog = GetLastSentReminderLog();
			LastInvitationActionDescriptionInfo.RefreshBinding();
		}

		public class SendCalendarReminderResult
		{
			SendCalendarReminderResult(ZBool sent, ZString message, Reminder reminder)
			{
				Sent = sent;
				Message = message;
				Reminder = reminder;
			}

			public static SendCalendarReminderResult Success(Reminder reminder)
			{
				return new SendCalendarReminderResult(true, ZString.Empty, reminder);
			}

			public static SendCalendarReminderResult Failure(ZString message)
			{
				return new SendCalendarReminderResult(false, message, null);
			}

			public readonly ZBool Sent;
			public readonly ZString Message;
			public readonly Reminder Reminder;
		}

		#endregion

		#region ReadOnly Security

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgSalesCallFetchStrategy(this);
		}

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (tasks == null)
				{
					tasks = this.GetOrCreateProcessTaskCollection(() => new OrgSalesCallProcessTaskCollection(this));
					RegisterEditableChildObject(tasks);
				}
				return tasks;
			}
		}
		OrgSalesCallProcessTaskCollection tasks;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return OrgSalesCallWorkflowDescriptor.WorkflowTypeCode; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			// Must match OrgSalesCallWorkflowDescriptor.SubTypeInformation
			// and OrgSalesCallFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, OQ_TypeOfCall, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, OQ_Status, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, OQ_Category, ZString.Empty);
			return result;
		}

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CommunicationManager);
				}
				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupporter

		public DocumentSupporter DocumentSupporter
		{
			get { return new OrgSalesCallDocumentSupporter(this); }
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region IRelatableActivity

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.Communication; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return Header; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return OQ_OHInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return Contact; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return OQ_OCInfo.HasChanges; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { OQ_TypeOfCall, ContactName, OQ_CallSummary, OQ_Status, OverallDispositionDescription, (Header != null ? Header.OH_Code : ZString.Empty) }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new OrgSalesCallRelatedChildActivityPivotCollection(this);
					if (IsLinkedToInquiry)
					{
						relatedChildActivityPivotCollection.SetReadOnlyIncludingChildren(true);
					}
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new OrgSalesCallRelatedParentActivityPivotCollection(this);
					if (IsLinkedToInquiry)
					{
						relatedParentActivityPivotCollection.SetReadOnlyIncludingChildren(true);
					}
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region ISalesRelationActivity

		public ISalesRelationModel SalesRelationModel
		{
			get { return salesRelationModel ?? (salesRelationModel = new SalesRelationModel(this)); }
		}
		SalesRelationModel salesRelationModel;

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return OrgSalesCallSchema.Constants.OQ_SalesCallNotes; }
		}

		#endregion

		#region ISalesAssociatedEntity

		ZString ISalesValueAssociatedEntity.ID => OQ_CommunicationID;
		ZString ISalesValueAssociatedEntity.EntityType => ((IRelatableActivity)this).ActivityType;
		ControllerID ISalesValueAssociatedEntity.ControllerID => ControllerIDs.Communication;
		ZGuid? ISalesValueAssociatedEntity.CompanyPk => null;
		ZString ISalesValueAssociatedEntity.CompanyCode => ZString.Empty;
		ZPropertyInfo ISalesValueAssociatedEntity.OrgPkInfo => OQ_OHInfo;
		ZString ISalesValueAssociatedEntity.Summary => ((IRelatableActivity)this).Summary;
		ZString ISalesValueAssociatedEntity.ValueCurrency => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		ZDateTime ISalesValueAssociatedEntity.DateForExchangeRate => ZDateTime.Now;

		ZDateTime ISalesValueAssociatedEntity.GetDateAssociatedToSalesValue(ISalesValue salesValue)
		{
			return OQ_SystemCreateTimeLocal;
		}

		ZString ISalesValueAssociatedEntity.GetUserThatAssociatedToSalesValue(ISalesValue salesValue)
		{
			return OQ_SystemCreateUser;
		}

		public ISalesHeaderCollection ActualAndProspectiveSalesHeaderCollection
		{
			get
			{
				if (salesHeaderCollection == null)
				{
					salesHeaderCollection = ObjectFactory.Get<ISalesHeaderCollectionBuilder>().New(this, true);
				}
				return salesHeaderCollection;
			}
		}
		ISalesHeaderCollection salesHeaderCollection;

		public ISalesHeaderCollection ProspectiveSalesHeaderCollection
		{
			get
			{
				if (prospectiveSalesHeaderCollection == null)
				{
					prospectiveSalesHeaderCollection = ObjectFactory.Get<ISalesHeaderCollectionBuilder>().New(this, false);
				}
				return prospectiveSalesHeaderCollection;
			}
		}
		ISalesHeaderCollection prospectiveSalesHeaderCollection;

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			ImportParentClientAndContact(parentActivity);

			var parentInquiry = parentActivity as SalesEnquiry;
			if (parentInquiry == null)
			{
				var parentCampaignItem = parentActivity as IGlbCompanyCampaignItem;
				if (parentCampaignItem != null)
				{
					parentInquiry = Factory.Load<SalesEnquiry>(parentCampaignItem.G8_RecipientID);
				}
			}
			if (parentInquiry != null && !parentInquiry.O1_OH_ConvertedToQualifiedLead.IsValid && !OQ_OH.IsValid)
			{
				ImportLinkedInquiry(parentInquiry);
			}

			return true;
		}

		void ImportParentClientAndContact(IRelatableActivity parentActivity)
		{
			if (parentActivity.Client != null)
			{
				OQ_OH = parentActivity.Client.PK;
			}

			if (parentActivity.Contact != null)
			{
				OQ_OC = parentActivity.Contact.PK;
			}

			if (!parentActivity.Summary.IsEmpty && parentActivity.ActivityType == RelatableActivityTypeList.Codes.CampaignManagement)
			{
				OQ_CallSummary = parentActivity.Summary.SubstringSafe(0, OrgSalesCallSchema.OQ_CallSummary.MaxLength);
			}
		}

		void ImportLinkedInquiry(SalesEnquiry parentInquiry)
		{
			if (!OQ_OH.IsValid
				&& !RelatedChildActivityPivotCollection.Any()
				&& !RelatedParentActivityPivotCollection.Activities
					.Any(parent => parent == null || parent.PK != parentInquiry.PK && parent.ActivityType != RelatableActivityTypeList.Codes.CampaignManagement))
			{
				LinkedInquiry = parentInquiry;
			}
		}

		#endregion

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new CommunicationNumberFountainUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;
	}

	class CommunicationNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
	{
		public CommunicationNumberFountainUniqueIndexFailureHandler(OrgSalesCall orgSalesCall)
			: base(OrgSalesCallSchema.Constants.Indexes.NR_UX__OQ_CommunicationID, orgSalesCall)
		{
		}

		protected override INumberFountainProxy NumberFountainToFix => Env.NumberFountains.CommunicationID;
	}
}
