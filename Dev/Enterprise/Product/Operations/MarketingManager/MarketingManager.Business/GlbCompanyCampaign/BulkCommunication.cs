using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MarketingManager.Business
{
	public class BulkCommunication : NonPersistentBusinessObject
	{
		public BulkCommunication(BusinessObjectFactory factory, GlbCompanyCampaign campaign)
			: base(factory)
		{
			Summary = campaign.G0_CampaignNameMultilingual;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			staffCoordinator = GlbStaff.CurrentUser.GS_Code;
			duration = TimeSpan.FromMinutes(30);
		}

		#region Properties

		[List("TypeOfCall_ActiveList")]
		[MaxLength(3)]
		public ZString TypeOfCall
		{
			get { return typeOfCall; }
			set
			{
				SetNonPersistentPropertyValue(TypeOfCallInfo, ref typeOfCall, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTypeOfCall();
				}
			}
		}
		ZString typeOfCall;

		public ZPropertyInfo TypeOfCallInfo
		{
			get { return GetZPropertyInfo(nameof(TypeOfCall)); }
		}

		[List("Category_ActiveList")]
		[MaxLength(3)]
		public ZString Category
		{
			get { return category; }
			set
			{
				SetNonPersistentPropertyValue(CategoryInfo, ref category, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCategory();
				}
			}
		}
		ZString category;

		public ZPropertyInfo CategoryInfo
		{
			get { return GetZPropertyInfo(nameof(Category)); }
		}

		[List("Status_ActiveList")]
		[MaxLength(3)]
		public ZString Status
		{
			get { return status; }
			set
			{
				SetNonPersistentPropertyValue(StatusInfo, ref status, value);
				OnStatusChanged();
				if (!IsValidationSuspended)
				{
					Validation.ValidateStatus();
				}
			}
		}
		ZString status;

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(nameof(Status)); }
		}

		public ZString Summary
		{
			get { return summary; }
			set { summary = value; }
		}
		ZString summary;

		[List("SalesReps")]
		[MaxLength(3)]
		public ZString StaffCoordinator
		{
			get { return staffCoordinator; }
			set
			{
				CheckMaximumLength(StaffCoordinatorInfo, value);
				SetNonPersistentPropertyValue(StaffCoordinatorInfo, ref staffCoordinator, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStaffCoordinator();
				}
			}
		}
		ZString staffCoordinator;

		public ZPropertyInfo StaffCoordinatorInfo
		{
			get { return GetZPropertyInfo(nameof(StaffCoordinator)); }
		}

		public ZDateTime Duration
		{
			get { return duration; }
			set { duration = value; }
		}
		ZDateTime duration;

		public ZDateTime NextCallLocal
		{
			get
			{
				return nextCallLocal.IsValid ? Env.Time.GetLocalTimeFromUtc(nextCallLocal.ToDateTime()) : nextCallLocal;
			}
			set
			{
				nextCallLocal = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateNextCallLocal();
				}
				NextCallLocalInfo.RefreshBinding();
			}
		}
		ZDateTime nextCallLocal;

		public ZPropertyInfo NextCallLocalInfo
		{
			get { return GetZPropertyInfo(nameof(NextCallLocal)); }
		}

		public ZDateTime CallDate
		{
			get
			{
				return callDate.IsValid ? Env.Time.GetLocalTimeFromUtc(callDate.ToDateTime()) : callDate;
			}
			set
			{
				callDate = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCallDate();
				}
				CallDateInfo.RefreshBinding();
			}
		}
		ZDateTime callDate;

		public ZPropertyInfo CallDateInfo
		{
			get { return GetZPropertyInfo(nameof(CallDate)); }
		}

		public ZBool ShouldSendInvitation
		{
			get { return shouldSendInvitation; }
			set { shouldSendInvitation = value; }
		}
		ZBool shouldSendInvitation;

		public ZString OverallDisposition
		{
			get { return IsClosed ? OrgSalesCallOverallDispositionList.Codes.Closed : OrgSalesCallOverallDispositionList.Codes.Open; }
		}

		public ZPropertyInfo OverallDispositionInfo
		{
			get { return GetZPropertyInfo(nameof(OverallDisposition)); }
		}

		[ChildEditable]
		public BulkCommunicationCollection CommunicationCollection
		{
			get
			{
				if (communicationCollection == null)
				{
					communicationCollection = new BulkCommunicationCollection(Factory);
					RegisterEditableChildObject(communicationCollection);
				}
				return communicationCollection;
			}
		}
		BulkCommunicationCollection communicationCollection;

		#region IsClosed

		public ZBool IsClosed
		{
			get { return OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.GetClosedFromCode(Status); }
		}

		#endregion

		#region Property Description

		public ZString TypeOfCallDescription
		{
			get { return TypeOfCall_List.GetDescriptionFromCode(TypeOfCall); }
		}

		public ZString CategoryDescription
		{
			get
			{
				return Category_List.GetDescriptionFromCode(Category);
			}
		}

		public ZString StatusDescription
		{
			get { return Status_List.GetDescriptionFromCode(Status); }
		}

		public ZString OverallDispositionDescription
		{
			get { return OverallDispositionList.GetDescriptionFromCode(OverallDisposition); }
		}

		#endregion

		#endregion

		#region Lookups

		public ICodeDescriptionPairList TypeOfCall_ActiveList
		{
			get { return Factory.GetCachedValue<ICodeDescriptionPairList>("BulkCommunication.OQ_TypeOfCall_ActiveList", () => OrganisationsDataRegistry.Instance.CommunicationType.Value.GetActiveCodeDescriptionPairList()); }
		}

		public ICodeDescriptionPairList Category_ActiveList
		{
			get { return Factory.GetCachedValue<ICodeDescriptionPairList>("BulkCommunication.OQ_Category_ActiveList", () => OrganisationsDataRegistry.Instance.CategoryList.Value.GetActiveCodeDescriptionPairList()); }
		}

		public ICodeDescriptionPairList Status_ActiveList
		{
			get { return Factory.GetCachedValue<ICodeDescriptionPairList>("BulkCommunication.OQ_Status_ActiveList", () => OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.GetActiveCodeDescriptionPairList()); }
		}

		public GlbStaffCollection SalesReps
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public ICodeDescriptionPairList TypeOfCall_List
		{
			get { return OrganisationsDataRegistry.Instance.CommunicationType.Value; }
		}

		public ICodeDescriptionPairList Category_List
		{
			get { return OrganisationsDataRegistry.Instance.CategoryList.Value; }
		}

		public ICodeDescriptionPairList Status_List
		{
			get { return OrganisationsDataRegistry.Instance.CommunicationStatusList.Value; }
		}

		public ICodeDescriptionPairList OverallDispositionList
		{
			get { return new OrgSalesCallOverallDispositionList(); }
		}

		#endregion

		#region Events

		public event EventHandler StatusChanged;

		void OnStatusChanged()
		{
			if (StatusChanged != null)
			{
				StatusChanged(null, EventArgs.Empty);
			}
		}

		#endregion

		#region Create Relationship For NewEntity

		public static void CreateRelationshipForNewEntity(OrgSalesCall newEntity, GlbCompanyCampaignItem masterActivity, ShowError showErrorIfExists)
		{
			if (newEntity != null)
			{
				var shouldShow = true;

				if (!newEntity.RelatedParentActivityPivotCollection.HasParent(masterActivity))
				{
					var inquiry = masterActivity.Recipient as SalesEnquiry;
					if (inquiry != null && inquiry.Header == null)
					{
						newEntity.LinkedInquiry = inquiry;
						newEntity.RelatedParentActivityPivotCollection.DeleteAll();
						newEntity.RelatedParentActivityPivotCollection.AddNewPivot(masterActivity);
					}
					else
					{
						using (newEntity.RelatedParentActivityPivotCollection.TemporarilyIgnoreSuperAndSubActivityRelationships())
						{
							var addParentResult = newEntity.RelatedParentActivityPivotCollection.AddActivity(masterActivity);
							if (!addParentResult.Success)
							{
								shouldShow = false;
								showErrorIfExists(addParentResult.Reason);
							}
						}
					}
				}

				if (shouldShow)
				{
					var deciderFactory = new ImportRelatedActivityNoDecisionFactory();
					((IImportParentRelatedActivityInfoOnNew)newEntity).ImportParentInfo(masterActivity, deciderFactory);
				}
			}
		}

		public delegate void ShowError(string message);

		#endregion

		#region Validation

		public BulkCommunicationValidation Validation
		{
			get { return GetNewValidation(); }
		}

		BulkCommunicationValidation GetNewValidation()
		{
			return new BulkCommunicationValidation(this);
		}

		#endregion

		public void SendCalenderReminder()
		{
			foreach (OrgSalesCall communication in CommunicationCollection)
			{
				if (communication.ShouldSendInvitation && !OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.Value)
				{
					var sendCalendarReminderResult = communication.SendCalendarReminder();
					if (!sendCalendarReminderResult.Sent)
					{
						communication.AddRowError(sendCalendarReminderResult.Message);
					}
				}
			}
		}

		public void UpdateFromBulkCommunication(BulkCommunication bulkComm, IEnumerable<OrgSalesCall> selectedElements)
		{
			if (selectedElements != null && selectedElements.Any())
			{
				foreach (OrgSalesCall salesCall in selectedElements)
				{
					salesCall.OQ_TypeOfCall = bulkComm.TypeOfCall;
					salesCall.OQ_Category = bulkComm.Category;
					salesCall.OQ_CallSummary = bulkComm.Summary;
					salesCall.OQ_Status = bulkComm.Status;
					salesCall.OQ_GS_NKSalesRep = bulkComm.StaffCoordinator;
					salesCall.OQ_NextCallLocal = bulkComm.NextCallLocal;
					salesCall.OQ_CallDateLocal = bulkComm.CallDate;
					salesCall.OQ_Duration = bulkComm.Duration;
					salesCall.ShouldSendInvitation = bulkComm.ShouldSendInvitation;
				}
			}
		}
	}
}
