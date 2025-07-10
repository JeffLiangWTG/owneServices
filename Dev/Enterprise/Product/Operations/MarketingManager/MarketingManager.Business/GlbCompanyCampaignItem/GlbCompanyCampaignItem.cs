using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	#region GlbCompanyCampaignTypeDecider

	public class GlbCompanyCampaignItemTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(GlbCompanyCampaignItem);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var result = typeof(GlbCompanyCampaignItem);

			var recipientTableCode = row[GlbCompanyCampaignItemSchema.Constants.G8_RecipientTableCode].ToString();
			if (recipientTableCode == HRJobApplicantSchema.Constants.Prefix)
			{
				var campaignPK = new ZGuid(row[GlbCompanyCampaignItemSchema.Constants.G8_G0]);
				var campaign = factory.Load<GlbCompanyCampaign>(campaignPK);
				if (ObjectFactory.GetType<ILearningCentreCampaign>().IsInstanceOfType(campaign))
				{
					result = ObjectFactory.GetType<ILearningCentreCampaignItem>();
				}
			}

			return result;
		}

		public override Type GetTypeForNew()
		{
			return typeof(GlbCompanyCampaignItem);
		}
	}

	#endregion

	[CodeProperty(GlbCompanyCampaignItem.Schema.Code), DescriptionProperty(GlbCompanyCampaignItem.Schema.Description)]
	public class GlbCompanyCampaignItem : AutoGlbCompanyCampaignItem,
		IGlbCompanyCampaignItem,
		IDocManagerSupport,
		ISalesRelationActivity,
		ISubRelatableActivity,
		IImportChildRelatedActivityInfoOnAttach,
		IImportChildRelatedActivityInfoOnDetach,
		IScheduleItemsProvider
	{
		public static readonly GlbCompanyCampaignItemTypeDecider TypeDecider = new GlbCompanyCampaignItemTypeDecider();

		public GlbCompanyCampaignItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(G8_LastFailedTransitionUtc), ConcurrencyPolicy.Ignore);
			itemSent = false;
		}

		#region Schema

		public new class Schema : AutoGlbCompanyCampaignItem.Schema
		{
			public const string OrgPK = "OrgPK";
			public const string CampaignID = "CampaignID";
			public const string CampaignName = "CampaignName";
			public const string Code = "Code";
			public const string ContactName = "ContactName";
			public const string WorkPhone = "WorkPhone";
			public const string EmailAddress = "EmailAddress";
			public const string Description = "Description";
			public const string TrackingStatusDescription = "TrackingStatusDescription";
			public const string FollowedUpStaffName = "FollowedUpStaffName";
			public const string SenderStaffName = "SenderStaffName";
			public const string LastSentTime = "LastSentTime";
			public const string LastCommunicationDate = "LastCommunicationDate";
			public const string LastCommunicationStaffCode = "LastCommunicationStaffCode";
			public const string ScheduleTime = "ScheduleTime";
			public const string ScheduleTimeUtc = "ScheduleTimeUtc";
			public const string ScheduleTimeRecipientTime = "ScheduleTimeRecipientTime";
		}

		#endregion

		#region Follow up

		public void FollowUp()
		{
			G8_GS_NKFollowedUpBy = GlbStaff.CurrentUser.GS_Code;
			G8_FollowedUp = ZDateTime.Now;
		}

		#endregion

		#region Create Task

		/// <summary>
		/// Populate a Stand Alone Task with details from the specific campaign item.
		/// Ie contact, organiastion and campaign information is copied into the task.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded tasks description: probably need to convert to resource string later.")]
		public GlbCompanyCampaignItemProcessTask GetPopulatedTask()
		{
			var task = Factory.New<GlbCompanyCampaignItemProcessTask>();
			IGlbCompanyCampaignItemRecipient recipient = Recipient;
			if (recipient != null && recipient is OrgContact)
			{
				task.OrganisationPK = (recipient.Organisation != null) ? recipient.Organisation.PK : ZGuid.Empty;
				task.P9_OA = (task.Organisation != null && task.Organisation.Addresses.Count > 0) ? task.Organisation.Addresses[0].PK : ZGuid.Invalid;
				task.P9_OC = recipient.PK;
			}
			task.P9_ParentTableCode = GlbCompanyCampaignItemSchema.Constants.Prefix;
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Description = ZString.Format("Campaign Task - {0}", CampaignID).SubstringSafe(0, ProcessTasksSchema.P9_Description.MaxLength);
			return task;
		}

		#endregion

		#region Mark as Verified

		public void MarkAsVerified()
		{
			G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
		}

		#endregion

		#region Drip Marketing

		void ReactivateBatchSendRecurrenceTaskIfRequired()
		{
			if (G8_TrackingStatus == TrackingStatusCodes.Codes.QUE
				&& G8_ScheduleTimeUtc.IsEmpty)
			{
				var campaign = CompanyCampaign;
				if (campaign != null)
				{
					var sendSettings = campaign.SendSettings;
					if (sendSettings != null)
					{
						var scheduleTask = sendSettings.ScheduleTask;
						if (scheduleTask != null)
						{
							scheduleTask.S5_IsActive = true;
						}
					}
				}
			}
		}

		#endregion

		#region Code

		public ZString Code
		{
			get
			{
				var result = ZString.Empty;
				var recipient = Recipient;
				if (recipient != null)
				{
					result = recipient.RelatedDocName;
				}
				return result;
			}
		}

		#endregion

		#region SystemCreateUser

		[RelatedBusinessObject("SystemCreateUser")]
		[ReadOnly(true)]
		public override ZString G8_SystemCreateUser
		{
			get { return base.G8_SystemCreateUser; }
			set { base.G8_SystemCreateUser = value; }
		}

		public virtual GlbStaff SystemCreateUser
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, G8_SystemCreateUser); }
		}

		#endregion

		#region SystemLastEditUser

		[RelatedBusinessObject("SystemLastEditUser")]
		[ReadOnly(true)]
		public override ZString G8_SystemLastEditUser
		{
			get { return base.G8_SystemLastEditUser; }
			set { base.G8_SystemLastEditUser = value; }
		}

		public virtual GlbStaff SystemLastEditUser
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, G8_SystemLastEditUser); }
		}

		#endregion

		#region Company Campaign

		[RelatedBusinessObject("CompanyCampaign")]
		[List("Lookups.Campaigns")]
		public override ZGuid G8_G0
		{
			get { return base.G8_G0; }
			set { base.G8_G0 = value; }
		}

		public virtual GlbCompanyCampaign CompanyCampaign
		{
			get { return Factory.Load<GlbCompanyCampaign>(G8_G0); }
		}

		#endregion

		#region Read-only Properties

		[ReadOnly(true)]
		public override ZGuid G8_RecipientID
		{
			get { return base.G8_RecipientID; }
			set { base.G8_RecipientID = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime G8_SystemCreateTimeUtc
		{
			get { return base.G8_SystemCreateTimeUtc; }
			set { base.G8_SystemCreateTimeUtc = value; }
		}

		[ReadOnly(true)]
		public override ZString G8_DeliveryMethod
		{
			get { return base.G8_DeliveryMethod; }
			set { base.G8_DeliveryMethod = value; }
		}

		[ReadOnly(true)]
		public override ZString G8_Stage
		{
			get { return base.G8_Stage; }
			set { base.G8_Stage = value; }
		}

		#endregion

		#region OrgPK

		[RelatedBusinessObject("ClientOrg")]
		[List("Lookups.ClientOrganisations")]
		public ZGuid OrgPK
		{
			get
			{
				IGlbCompanyCampaignItemRecipient recipient = Recipient;
				return (recipient != null && recipient.Organisation != null) ? recipient.Organisation.PK : ZGuid.Empty;
			}
		}

		public ZPropertyInfo OrgPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrgPK); }
		}

		public OrgHeader ClientOrg
		{
			get
			{
				IGlbCompanyCampaignItemRecipient recipient = Recipient;
				return (recipient != null) ? recipient.Organisation : null;
			}
		}

		#endregion

		#region Campaign ID

		public ZString CampaignID
		{
			get { return CompanyCampaign != null ? CompanyCampaign.CampaignID : ZString.Empty; }
		}

		public ZPropertyInfo CampaignIDInfo
		{
			get { return GetZPropertyInfo(Schema.CampaignID); }
		}

		#endregion

		#region Campaign Name

		public ZString CampaignName
		{
			get { return CompanyCampaign != null ? CompanyCampaign.G0_CampaignNameMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo CampaignNameInfo
		{
			get { return GetZPropertyInfo(Schema.CampaignName); }
		}

		#endregion

		#region Contact Name

		public ZString ContactName
		{
			get
			{
				ZString result = ZString.Empty;
				IGlbCompanyCampaignItemRecipient recipient = Recipient;
				if (recipient != null)
				{
					if (recipient is OrgContact)
					{
						result = ((OrgContact)recipient).OC_ContactName;
					}
					else
					{
						result = recipient.Name;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ContactNameInfo
		{
			get { return GetZPropertyInfo(Schema.ContactName); }
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Baseline")]
		public virtual string GetMyAccountUserAgreementUrl(string agreementType, bool sendAgreementCopy)
		{
			return "MACRO-ONLY-AVAILABLE-ON-EDI";
		}

		public VoteExamSurveySubmittedAnswerCollection SubmittedAnswers
		{
			get
			{
				var fSubmittedAnswers = GetNewSubmittedAnswers();
				fSubmittedAnswers.Load();
				return fSubmittedAnswers;
			}
		}

		protected virtual VoteExamSurveySubmittedAnswerCollection GetNewSubmittedAnswers()
		{
			return new VoteExamSurveySubmittedAnswerCollection(this);
		}

		public ZString VoteExamSurveyCampaignURL
		{
			get
			{
				return VoteExamSurveyUrlHelper.GetCampaignUrl(this);
			}
		}

		public VoteExamSurveyAnswerCollection PersistedAnswers
		{
			get
			{
				if (persistedAnswers == null)
				{
					persistedAnswers = new VoteExamSurveyAnswerCollection(this);
					persistedAnswers.Load();
				}
				return persistedAnswers;
			}
		}

		[BusinessObjectTestExclude] //reference from VoteExamSurveyAnswerSet
		public VoteExamSurveyAnswerWrapperCollection AnswerWrappers { get; set; }

		public virtual ZString SkillTestName
		{
			get
			{
				return ZString.Empty;
			}
		}

		public override ZString G8_TrackingStatus
		{
			get { return base.G8_TrackingStatus; }
			set
			{
				base.G8_TrackingStatus = value;
				UpdateTrackingStatus(base.G8_TrackingStatus);
			}
		}

		void UpdateTrackingStatus(ZString status)
		{
			var recipient = Recipient;
			if (recipient != null)
			{
				if (!string.IsNullOrEmpty(recipient.Email))
				{
					if (status == TrackingStatusCodes.Codes.VER)
					{
						var emailAddress = GlbEmailAddress.LoadOrNew(Factory, recipient.Email);
						emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;
					}
					else if (status == TrackingStatusCodes.Codes.NDR)
					{
						var emailAddress = GlbEmailAddress.LoadOrNew(Factory, recipient.Email);
						emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
					}
				}

				if (status == TrackingStatusCodes.Codes.NDR && !G8_RecipientID.IsEmpty && CompanyCampaign.IsTouchCampaign)
				{
					var transitions = CampaignSummaryStats.LoadTransitions(this);
					var pks = transitions.Where(t => t.HorizontalId > CompanyCampaign.G0_HorizontalId).Select(t => t.CampaignItemId);

					var query = new ZQuery(GlbCompanyCampaignItemSchema.PK, pks);
					var items = Factory.Load<GlbCompanyCampaignItem>(query);
					foreach (var campaignItem in items)
					{
						if (campaignItem.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE)
						{
							campaignItem.Delete();
						}
					}
				}
			}
		}

		#region Organisation Full Name

		public ZString OrganisationFullName
		{
			get
			{
				var result = ZString.Empty;
				IGlbCompanyCampaignItemRecipient recipient = Recipient;
				if (recipient != null && !(recipient is GlbStaff || recipient is IHRJobApplicant))
				{
					if (recipient is OrgContact)
					{
						result = ((OrgContact)recipient).Header.OH_FullName;
					}
					else if (recipient is SalesEnquiry)
					{
						result = ((SalesEnquiry)recipient).O1_CompanyName;
					}
					else
					{
						result = recipient.Organisation.OH_FullName;
					}
				}
				return result;
			}
		}

		#endregion

		#region Organisation Closest Port/Country

		public ZString OrganisationClosestPortOrCountry
		{
			get
			{
				var result = ZString.Empty;
				IGlbCompanyCampaignItemRecipient recipient = Recipient;
				if (recipient != null)
				{
					if (recipient is OrgContact)
					{
						if (((OrgContact)recipient).Header.ClosestPort != null)
						{
							result = ((OrgContact)recipient).Header.ClosestPort.RL_RN_NKCountryCode;
						}
					}
					else if (recipient is SalesEnquiry)
					{
						var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, ((SalesEnquiry)recipient).O1_PortOrCountry);
						if (unloco != null)
						{
							result = unloco.Country.RN_Code;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region Email Address

		public ZString EmailAddress
		{
			get
			{
				IGlbCompanyCampaignItemRecipient recipient = Recipient;
				return recipient != null ? recipient.Email : "";
			}
		}

		public ZPropertyInfo EmailAddressInfo
		{
			get { return GetZPropertyInfo(Schema.EmailAddress); }
		}

		#endregion

		#region Protected Properties

		#region View Denied Message
		internal ZString ViewDeniedMessage
		{
			get { return Res.GetString("48B80C5A-7740-4A73-BB02-65EC7EE78E9F", "** View Denied due to Security Access **"); }
		}
		#endregion

		bool ViewAllowed
		{
			get
			{
				if (G8_RecipientTableCode == "GS")
				{ return Env.Security.StaffViewOtherStaffDetails.IsAllowed; }
				if (G8_RecipientTableCode == "HA")
				{ return Env.Security.HRJobApplicantView.IsAllowed; }
				return true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		bool ViewNotAllowed
		{
			get { return !ViewAllowed; }
		}

		#region Work Phone
		[ReadOnlyMember(nameof(ViewNotAllowed))]
		public ZString WorkPhone
		{
			get
			{
				IGlbCompanyCampaignItemRecipient recipient = Recipient;
				if (ViewAllowed)
				{
					return recipient != null ? recipient.Phone : "";
				}
				else
				{
					return ViewDeniedMessage;
				}
			}
		}

		public ZPropertyInfo WorkPhoneInfo
		{
			get { return GetZPropertyInfo(Schema.WorkPhone); }
		}

		#endregion

		#endregion

		#region Sender Staff Name

		public ZString SenderStaffName
		{
			get { return SystemCreateUser != null ? SystemCreateUser.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo SenderStaffNameInfo
		{
			get { return GetZPropertyInfo(nameof(SenderStaffName)); }
		}

		#endregion

		#region Follow-up Staff Name

		public ZString FollowedUpStaffName
		{
			get { return FollowedUpBy != null ? FollowedUpBy.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo FollowedUpStaffNameInfo
		{
			get { return GetZPropertyInfo(nameof(FollowedUpStaffName)); }
		}

		#endregion

		#region BounceBack

		public ZString BounceBackEmail
		{
			get
			{
				return LastBounceBackEmailNote != null && this.G8_TrackingStatus == TrackingStatusCodes.Codes.NDR ? LastBounceBackEmailNote.ST_NoteDataAsText : ZString.Empty;
			}
		}

		public ZPropertyInfo BounceBackEmailInfo
		{
			get { return GetZPropertyInfo(nameof(BounceBackEmail)); }
		}

		public ZString TrackingStatusDescription
		{
			get
			{
				if (CompanyCampaign.IsTargetList)
				{
					return Res.GetString("a41e4473-40ee-4080-840b-3e30bfbb2f63", "Not Applicable");
				}
				else if (!this.G8_TrackingStatus.IsEmpty)
				{
					return new TrackingStatusCodes().GetDescriptionFromCode(this.G8_TrackingStatus);
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo TrackingStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TrackingStatusDescription)); }
		}

		StmNote LastBounceBackEmailNote
		{
			get
			{
				if (!lastBounceBackEmailNoteLoaded && this.G8_TrackingStatus == TrackingStatusCodes.Codes.NDR)
				{
					lastBounceBackEmailNote = BounceBackEmailProcessor.GetLastBounceBackEmailNote(this);
					lastBounceBackEmailNoteLoaded = true;
				}
				return lastBounceBackEmailNote;
			}
		}
		StmNote lastBounceBackEmailNote;
		bool lastBounceBackEmailNoteLoaded;

		public void ReloadNotesFromDB()
		{
			lastBounceBackEmailNoteLoaded = false;
			BounceBackEmailInfo.RefreshBinding();
		}

		#endregion

		#region RecipientType

		public ZString RecipientType
		{
			get
			{
				var result = G8_RecipientTableCode;
				var list = new CampaignContactTypeCodeList();
				if (!result.IsEmpty)
				{
					result = list.GetDescriptionFromCode(result);
				}
				return result;
			}
		}

		#endregion

		#region Last Communication

		public ZDateTime LastCommunicationDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				var lastCommunication = LastCommunication;
				if (lastCommunication != null)
				{
					result = lastCommunication.OQ_CallDateLocal.IsValid ? lastCommunication.OQ_CallDateLocal : lastCommunication.OQ_NextCallLocal;
				}

				return result;
			}
		}

		public ZPropertyInfo LastActualCommunicationInfo
		{
			get { return GetZPropertyInfo(nameof(LastCommunicationDate)); }
		}

		public ZString LastCommunicationStaffCode
		{
			get
			{
				ZString result = "";
				var lastCommunication = LastCommunication;
				if (lastCommunication != null)
				{
					result = lastCommunication.SalesRep.GS_Code;
				}

				return result;
			}
		}

		public ZPropertyInfo LastCommunicationStaffCodeInfo
		{
			get { return GetZPropertyInfo(nameof(LastCommunicationStaffCode)); }
		}

		OrgSalesCall LastCommunication
		{
			get { return RelatedChildActivityPivotCollection.Activities.OfType<OrgSalesCall>().OrderByDescending(activity => activity.OQ_CallDate).FirstOrDefault(); }
		}

		#endregion

		#region Last Sent Time

		public ZDateTime LastSentTime
		{
			get { return G8_LastSentTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region Schedule Time

		public ZDateTime ScheduleTime
		{
			get { return G8_ScheduleTimeUtc.ToLocalBranchTime(); }
		}

		public ZDateTime ScheduleTimeUtc
		{
			get { return G8_ScheduleTimeUtc; }
		}

		public ZDateTime ScheduleTimeRecipientTime
		{
			get
			{
				if (G8_ScheduleTimeUtc.IsEmpty || !G8_ScheduleTimeUtc.IsValid)
				{
					return G8_ScheduleTimeUtc;
				}

				var recipient = RecipientFromView;

				var unloco = recipient == null || recipient.VCC_RelatedPortCode.IsEmpty
					? Env.CurrentBranch.NKUNLOCO
					: (string)recipient.VCC_RelatedPortCode;

				return Env.Time.GetUnlocoTimeFromUtc(unloco, G8_ScheduleTimeUtc.ToDateTime());
			}
		}

		#endregion

		public IGlbCompanyCampaignItemRecipient Recipient
		{
			get
			{
				IGlbCompanyCampaignItemRecipient result;

				switch (G8_RecipientTableCode)
				{
					case OrgContactSchema.Constants.Prefix:
						result = Factory.Load<OrgContact>(G8_RecipientID);
						break;

					case OrgColdCallRegisterSchema.Constants.Prefix:
						result = Factory.Load<SalesEnquiry>(G8_RecipientID);
						break;

					case HRJobApplicantSchema.Constants.Prefix:
						result = (IGlbCompanyCampaignItemRecipient)Factory.Load<IHRJobApplicant>(G8_RecipientID);
						break;

					case GlbStaffSchema.Constants.Prefix:
						result = Factory.Load<GlbStaff>(G8_RecipientID);
						break;

					default:
						result = Factory.Load<OrgContact>(G8_RecipientID);
						break;
				}

				if (result == null)
				{
					if (CompanyCampaign is IHRGlbCompanyCampaign)
					{
						result = Factory.GetNull<GlbStaff>();
					}
					else
					{
						result = Factory.GetNull<OrgContact>();
					}
				}

				return result;
			}
		}

		public CampaignContact RecipientFromView
		{
			get
			{
				return Factory.Load<CampaignContact>(G8_RecipientID);
			}
		}

		public OrgContact RecipientAsOrgContact
		{
			get { return Recipient as OrgContact; }
		}

		public IExamUrlRecipient RecipientAsExamUrlRecipient
		{
			get { return Recipient as IExamUrlRecipient; }
		}

		public OrgColdCallRegister RecipientAsSalesEnquiry
		{
			get
			{
				return Recipient as OrgColdCallRegister;
			}
		}

		public GlbStaff RecipientAsGlbStaff => Recipient as GlbStaff;

		public IHRJobApplicant RecipientAsHRJobApplicant => Recipient as IHRJobApplicant;

		public virtual ZString ResendConfirmationMessage
		{
			get { return Res.GetString("bf257c9a-9302-43a5-a85d-6d1fb6fb978f", "You are about to send the campaign to the selected contacts. If already sent, the campaign will be re-sent to these contacts. Would you like to continue?"); }
		}

		public virtual ZString RequeueConfirmationMessage
		{
			get { return Res.GetString("5f21c70b-6576-4e47-8304-35c1b9478b7f", "You are about to delete the 'Scheduled delivery' for the selected contact(s). Would you like to continue?"); }
		}

		public ZString Description
		{
			get
			{
				if (!IsDeleted && !ContactName.IsEmpty)
				{
					return Res.GetString("368bd8ec-700f-4882-be27-aa084f468446", "Campaign Sent to {0}", ContactName);
				}
				else
				{
					return Res.GetString("3a96a71b-0500-452b-b631-6e7fae36c107", "Campaign Sent");
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Description; }
		}

		#region StatModel

		public CampaignItemClickStatModel StatModel
		{
			get
			{
				if (statModel == null)
				{
					statModel = new CampaignItemClickStatModel(this);
					statModel.ReportBy = ReportByList.Codes.Context;
				}
				return statModel;
			}
		}
		CampaignItemClickStatModel statModel;

		#endregion

		#region Calculated Properties

		public ZString SenderEmailAddress
		{
			get
			{
				if (CompanyCampaign.G0_UseLastEmailSenderAddress && !G8_SenderEmailAddress.IsEmpty)
				{
					return G8_SenderEmailAddress;
				}

				if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.ORG && !CompanyCampaign.G0_EmailSenderRole.IsEmpty)
				{
					return GetAssignedStaff(CompanyCampaign.G0_EmailSenderRole)?.GS_EmailAddress ?? CompanyCampaign.CampaignCoordinator.GS_EmailAddress;
				}

				if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.SPS)
				{
					return Sender?.GS_EmailAddress ?? CompanyCampaign.CampaignCoordinator.GS_EmailAddress;
				}

				return CompanyCampaign.SenderEmailAddress;
			}
		}

		public ZString SenderName
		{
			get
			{
				if (CompanyCampaign.G0_UseLastEmailSenderAddress && !G8_EmailSenderName.IsEmpty)
				{
					return G8_EmailSenderName;
				}

				if (CompanyCampaign.G0_UseLastEmailSenderAddress)
				{
					return Sender?.GS_FullName ?? CompanyCampaign.SenderName;
				}

				if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.ORG && !CompanyCampaign.G0_EmailSenderRole.IsEmpty)
				{
					return GetAssignedStaff(CompanyCampaign.G0_EmailSenderRole)?.GS_FullName ?? CompanyCampaign.CampaignCoordinator.GS_FullName;
				}

				if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.SPS)
				{
					return Sender?.GS_FullName ?? CompanyCampaign.CampaignCoordinator.GS_FullName;
				}

				return CompanyCampaign.SenderName;
			}
		}

		public ZString SenderUNLOCO
		{
			get
			{
				if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML)
				{
					return CompanyCampaign.G0_RL_NKEmailSenderUNLOCO;
				}

				var branch = GetEmailSenderStaff()?.HomeBranch;
				return branch != null
					? branch.GB_RL_NKHomePort
					: ZString.Empty;
			}
		}

		public ZString SenderTitle => GetEmailSenderStaff()?.GS_Title ?? ZString.Empty;

		public ZString SenderWorkPhone => GetEmailSenderStaff()?.GS_WorkPhone ?? ZString.Empty;

		public ZString SenderMobilePhone => GetEmailSenderStaff()?.GS_MobilePhone ?? ZString.Empty;

		public ZString ReplyToEmailAddress
		{
			get
			{
				if (CompanyCampaign.UseEmailSenderAddressAsReplyTo)
				{
					if (CompanyCampaign.G0_UseLastEmailSenderAddress && !G8_SenderEmailAddress.IsEmpty)
					{
						return G8_SenderEmailAddress;
					}
					if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML)
					{
						return CompanyCampaign.G0_SenderEmail;
					}
					return SenderEmailAddress;
				}
				return CompanyCampaign.G0_ReplyToEmail;
			}
		}

		public ZString RecipientOrgFullName
		{
			get
			{
				ZString result = "";
				if (RecipientAsOrgContact != null)
				{
					result = Recipient.Organisation.OH_FullName;
				}
				else if (RecipientAsSalesEnquiry != null)
				{
					result = ((SalesEnquiry)Recipient).O1_CompanyName;
				}
				return result;
			}
		}

		public ZString RecipientCountryOrPort
		{
			get
			{
				ZString result = "";
				if (RecipientAsOrgContact != null)
				{
					if (RecipientAsOrgContact.BranchAddress != null && RecipientAsOrgContact.BranchAddress.Country != null)
					{
						result = RecipientAsOrgContact.BranchAddress.Country.RN_Code;
					}
					else if (Recipient.Organisation != null && Recipient.Organisation.ClosestPort != null && Recipient.Organisation.ClosestPort.Country != null)
					{
						result = Recipient.Organisation.ClosestPort.Country.RN_Code;
					}
				}
				else
				{
					var salesEnquiry = Recipient as SalesEnquiry;
					if (salesEnquiry != null)
					{
						result = salesEnquiry.O1_PortOrCountry.SubstringSafe(0, RefUNLOCO.Schema.RL_RN_NKCountryCodeMaxLength);
					}
					else if (RecipientAsGlbStaff != null)
					{
						result = RecipientAsGlbStaff.GS_RN_NKCountryCode;
					}
					else if (RecipientAsHRJobApplicant != null)
					{
						result = RecipientAsHRJobApplicant.HA_RN_NKCountry;
					}
				}

				return result;
			}
		}

		public ZBool IsUnsubscribed => GetIsUnsubscribed();

		public ZBool CanTransition
		{
			get
			{
				return
					G8_TrackingStatus != TrackingStatusCodes.Codes.QUE
					&& G8_TrackingStatus != TrackingStatusCodes.Codes.NDR
					&& !G8_IsSuspended
					&& !G8_IsBlocked;
			}
		}

		bool GetIsUnsubscribed()
		{
			if (EmailAddress.IsEmpty)
			{
				return false;
			}
			IEnumerable<GlbCompanyCampaignSubscription> contactSubs = CompanyCampaign?.UnsubscribedCollection?.Where(subscription => subscription.GCS_Email.EqualsIgnoringCase(EmailAddress));
			int matchingLevel = 0;
			bool bSubscribed = true;
			foreach (GlbCompanyCampaignSubscription contactSub in contactSubs)
			{
				if (contactSub.GCS_MediaCategory.Equals(CompanyCampaign?.G0_Category)
					&& contactSub.GCS_MediaType.Equals(CompanyCampaign?.G0_Type))
				{
					matchingLevel = 4;
					bSubscribed = contactSub.GCS_IsSubscribed;
					break;
				}
				else if (contactSub.GCS_MediaCategoryWithAll.Equals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode)
					&& contactSub.GCS_MediaType.Equals(CompanyCampaign?.G0_Type))
				{
					matchingLevel = 3;
					bSubscribed = contactSub.GCS_IsSubscribed;
				}
				else if (contactSub.GCS_MediaCategory.Equals(CompanyCampaign?.G0_Category)
					&& contactSub.GCS_MediaTypeWithAll.Equals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode))
				{
					if (matchingLevel < 2)
					{
						matchingLevel = 2;
						bSubscribed = contactSub.GCS_IsSubscribed;
					}
				}
				else if (contactSub.GCS_MediaCategoryWithAll.Equals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode)
					&& contactSub.GCS_MediaTypeWithAll.Equals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode))
				{
					if (matchingLevel < 1)
					{
						matchingLevel = 1;
						bSubscribed = contactSub.GCS_IsSubscribed;
					}
				}
			}
			return !bSubscribed;
		}

		protected bool IsUnsubscribed_ReadOnly => true;

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GlbCompanyCampaignItemFetchStrategy(this);
		}

		#endregion

		public GlbCompanyCampaignItem GetPreviousTransitionCampaignItem()
		{
			var transitions = CampaignSummaryStats.LoadTransitions(this);
			var prevTransitionPk = transitions.OrderByDescending(results => results.HorizontalId).FirstOrDefault(results => results.CampaignItemId != PK)?.CampaignItemId ?? ZGuid.Empty;

			return Factory.Load<GlbCompanyCampaignItem>(prevTransitionPk);
		}

		public GlbStaff GetEmailSenderStaff()
		{
			if (CompanyCampaign.G0_UseLastEmailSenderAddress && Sender != null)
			{
				return Sender;
			}

			if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.ORG && !CompanyCampaign.G0_EmailSenderRole.IsEmpty)
			{
				return GetAssignedStaff(CompanyCampaign.G0_EmailSenderRole) ?? CompanyCampaign.CampaignCoordinator;
			}

			if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML)
			{
				return null;
			}

			if (CompanyCampaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.SPS)
			{
				return Sender ?? CompanyCampaign.CampaignCoordinator;
			}

			return CompanyCampaign.CampaignCoordinator;
		}

		public GlbStaff GetAssignedStaff(string senderRole)
		{
			if (ClientOrg != null)
			{
				ZQuery query = new ZQuery(OrgStaffAssignmentsSchema.O8_OH, ClientOrg.PK);
				query.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, senderRole);
				IEnumerable<OrgStaffAssignments> orgStaffAssignment = Factory.Load<OrgStaffAssignments>(query);

				var staff = orgStaffAssignment.FirstOrDefault(s => s.Company == CompanyCampaign.Company);
				if (staff != null && !staff.PersonResponsible.GS_EmailAddress.IsEmpty)
				{
					return staff.PersonResponsible;
				}

				staff = orgStaffAssignment.FirstOrDefault(s => s.Company != null && s.Company != CompanyCampaign.Company && s.Company.Country == ClientOrg.Country);
				if (staff != null && !staff.PersonResponsible.GS_EmailAddress.IsEmpty)
				{
					return staff.PersonResponsible;
				}

				staff = orgStaffAssignment.FirstOrDefault(s => s.Company == null);
				if (staff != null && !staff.PersonResponsible.GS_EmailAddress.IsEmpty)
				{
					return staff.PersonResponsible;
				}
			}

			return null;
		}

		public void InvalidateAnswersSets()
		{
			persistedAnswers = null;
		}

		VoteExamSurveyAnswerCollection persistedAnswers;

		public void MergeAnswers()
		{
			PersistedAnswers.Load();

			Dictionary<string, VoteExamSurveyAnswer> answersInDb = new Dictionary<string, VoteExamSurveyAnswer>(PersistedAnswers.Count);
			List<VoteExamSurveyAnswer> answersInMemory = new List<VoteExamSurveyAnswer>(PersistedAnswers.Count);

			foreach (VoteExamSurveyAnswer answer in PersistedAnswers)
			{
				if (answer.IsInDatabase)
				{
					answersInDb[answer.Question.HY_QuestionOrder + "_" + answer.Question.HY_SubQuestionOrder] = answer;
				}
				else
				{
					answersInMemory.Add(answer);
				}
			}

			foreach (VoteExamSurveyAnswer answerInMemory in answersInMemory)
			{
				string key = answerInMemory.Question.HY_QuestionOrder + "_" + answerInMemory.Question.HY_SubQuestionOrder;
				if (answersInDb.ContainsKey(key))
				{
					var answerInDb = answersInDb[key];
					answerInDb.HZ_Answer = answerInMemory.HZ_Answer;
					answerInDb.HZ_AnswerComment = answerInMemory.HZ_AnswerComment;
					PersistedAnswers.RemoveAndDelete(answerInMemory);
				}
			}
		}

		public int GetSeedForQuestionRandomiser()
		{
			ZDateTime arbitraryDate = new ZDateTime(2008, 1, 1);
			return (int)((ReferenceDateForQuestionRandomiserSeed - arbitraryDate).TotalMilliseconds % int.MaxValue);
		}

		protected virtual ZDateTime ReferenceDateForQuestionRandomiserSeed
		{
			get { return ZDateTime.Now; }
		}

		#region Save

		public bool IsPendingTransition
		{
			get
			{
				return (dripEventTriggered || itemSent)
					&& CompanyCampaign != null
					&& (CompanyCampaign.IsMasterCampaign || CompanyCampaign.IsTouchCampaign);
			}
		}
		bool dripEventTriggered;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsInDatabase || HasChanges)
			{
				ReactivateBatchSendRecurrenceTaskIfRequired();
			}
		}

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			base.OnSaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (IsPendingTransition)
			{
				dripEventTriggered = false;

				if (!itemSent && saveSucceeded && CompanyCampaign != null)
				{
					CompanyCampaign.TransitionAndSchedule(new[] { PK });
				}

				itemSent = false;
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			ViewRelatedActivityPivot.MoveAllPivotsToSuperActivityIfAvailableOtherwiseDelete(this);
			PersistedAnswers.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		public virtual void ResetTrackingInfo(bool isScheduled)
		{
			if (isScheduled)
			{
				G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			}
			else
			{
				G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
				itemSent = true;
			}
		}
		bool itemSent;

		public virtual bool ResetVoteSurveyExam(bool forceReset)
		{
			return true;
		}

		public ZString CurrentQuestionsCountryCode { get; set; }

		public virtual bool HasPreviousSessionEnded
		{
			get { return G8_ClosedDateUtc.IsValid; }
		}

		public virtual string HasPreviousSessionEndedMessage
		{
			get { return Res.GetString("67b44a18-c24b-4ce5-952f-7ac4d830f6cd", "You have previously submitted your answers. Only one submission is allowed per participant."); }
		}

		public virtual bool CanAutoStartVoteExamSurvey
		{
			get { return IsContinuingPreviousAttempt; }
		}

		public virtual bool IsContinuingPreviousAttempt
		{
			get { return false; }
		}

		public virtual bool CanResetVoteSurveyExam
		{
			get { return false; }
		}
		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, DocManagerCode);
				}
				return docManagerInfo;
			}
		}

		protected virtual string DocManagerCode
		{
			get { return Core.Constants.DocManagerCodes.CompanyCampaignItem; }
		}

		DocManagerInfo docManagerInfo;

		public IStorageMain StorageMain => storageMain ?? (storageMain = DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(this, Core.Constants.DocManagerCodes.CompanyCampaignItem));
		IStorageMain storageMain;

		#endregion

		#region IGlbCompanyCampaignItem Members

		IGlbCompanyCampaign IGlbCompanyCampaignItem.Campaign
		{
			get { return CompanyCampaign; }
		}

		#endregion

		#region RecipientInfo

		public class RecipientInfo
		{
			public RecipientInfo(ZGuid recipientPK, string recipientTableCode)
			{
				this.recipientPK = recipientPK;
				this.recipientTableCode = recipientTableCode;
			}

			public readonly ZGuid recipientPK;
			public readonly string recipientTableCode;
		}

		#endregion

		#region IRelatableActivity

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ISuperRelatableActivity ISubRelatableActivity.SuperActivity
		{
			get { return CompanyCampaign; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.CampaignManagement; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get
			{
				var recipient = Recipient;
				return recipient != null ? recipient.Organisation : null;
			}
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return G8_RecipientIDInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get
			{
				if (G8_RecipientTableCode == OrgColdCallRegisterSchema.Constants.Prefix)
				{
					var inquiry = RecipientAsSalesEnquiry;
					return inquiry != null ? inquiry.LinkedContact : null;
				}
				else
				{
					return RecipientAsOrgContact;
				}
			}
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return G8_RecipientIDInfo.HasChanges; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { Description }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
			dripEventTriggered =
				!(relatedActivity is GlbCompanyCampaign)
				&& !(relatedActivity is GlbCompanyCampaignItem);
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityCollection == null)
				{
					relatedChildActivityCollection = new SubActivityRelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityCollection == null)
				{
					relatedParentActivityCollection = new SubActivityRelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityCollection;

		#endregion

		#region ISalesRelationActivity

		public ISalesRelationModel SalesRelationModel
		{
			get
			{
				if (salesRelationModel == null)
				{
					salesRelationModel = new SalesRelationModel(this);
					RegisterEditableChildObject(salesRelationModel);
				}

				return salesRelationModel;
			}
		}
		SalesRelationModel salesRelationModel;

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IImportChildRelatedActivityInfoOnAttach

		bool IImportChildRelatedActivityInfoOnAttach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (childActivity is OrgOpportunity childOpportunity)
			{
				childOpportunity.P8_G0 = G8_G0;

				var parentInquiry = RelatedParentActivityPivotCollection.Activities.OfType<SalesEnquiry>().FirstOrDefault();
				if (parentInquiry != null)
				{
					return ((IImportChildRelatedActivityInfoOnAttach)parentInquiry).ImportChildInfo(childActivity, deciderFactory);
				}
			}

			return true;
		}

		#endregion

		#region IImportChildRelatedActivityInfoOnDetach

		bool IImportChildRelatedActivityInfoOnDetach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (childActivity is OrgOpportunity childOpportunity)
			{
				childOpportunity.P8_G0 = ZGuid.Empty;
			}

			return true;
		}

		#endregion

		#region IScheduleCampaignItems

		public ZString ScheduleStatus => G8_TrackingStatus.IsEmpty || G8_TrackingStatus == TrackingStatusCodes.Codes.QUE || G8_TrackingStatus == TrackingStatusCodes.Codes.OPQ
					? TrackingStatusCodes.Codes.QUE
					: ScheduleItemDataLoader.Schema.SentStatus;

		public ZString TableCode
		{
			get { return GlbCompanyCampaignItemSchema.Constants.Prefix; }
		}

		#endregion

		#region Suspenders

		public FunctionalitySuspender SavingAnswersOnTimeOutSuspender
		{
			get { return savingAnswersOnTimeOutSuspender ?? (savingAnswersOnTimeOutSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender savingAnswersOnTimeOutSuspender;

		#endregion
	}
}
