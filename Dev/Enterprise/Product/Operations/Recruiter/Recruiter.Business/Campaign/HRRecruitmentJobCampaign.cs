using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[ModuleID(ModuleId.HRJobOpenings)]
	[CodeProperty(Schema.CampaignID), DescriptionProperty(AutoHRRecruitmentJobCampaign.Schema.HV_AdTitle)]
	[UserDefinedValues]
	public class HRRecruitmentJobCampaign
		: AutoHRRecruitmentJobCampaign
		, IDocManagerSupport
		, Enterprise.Integration.Recruiter.IHRRecruitmentJobCampaign
		, ICustomFieldProvider
		, IWorkflowProvider
	{
		public HRRecruitmentJobCampaign(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoHRRecruitmentJobCampaign.Schema
		{
			public const string CampaignID = "CampaignID";
			public const string HV_CampaignStartDateLocal = "HV_CampaignStartDateLocal";
			public const string HV_CampaignEndDateLocal = "HV_CampaignEndDateLocal";
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList objects = new ArrayList();

				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.AddRange(Applications);

				foreach (HRJobApplication application in Applications)
				{
					objects.AddRange(application.BusinessObjectsWithRelatedEvents);
				}

				objects.AddRange(AdPlacements);
				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				ArrayList objects = new ArrayList();
				objects.AddRange(base.BusinessObjectsWithRelatedNotes);
				objects.AddRange(Applications);

				foreach (HRJobApplication application in Applications)
				{
					objects.AddRange(application.BusinessObjectsWithRelatedEvents);
				}

				objects.AddRange(AdPlacements);
				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		public override void OnLoaded()
		{
			base.OnLoaded();
			hv_CampaignStartDateLocal = HV_CampaignStartDate.IsValid ? Env.Time.GetUnlocoTimeFromUtc(CampaignDateUNLOCO, HV_CampaignStartDate.ToDateTime()) : HV_CampaignStartDate;
			hv_CampaignEndDateLocal = HV_CampaignEndDate.IsValid ? Env.Time.GetUnlocoTimeFromUtc(CampaignDateUNLOCO, HV_CampaignEndDate.ToDateTime()) : HV_CampaignEndDate;
		}

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (kind == TestBusinessObjectKind.PopulateDependentCollections && Applications.Count == 0)
			{
				Applications.AddNew();
			}

			if (Applications.Count != 0)
			{
				var applicant = Factory.NewWithValidTestData<HRJobApplicant>(TestBusinessObjectKind.MinimumRequiredToSave);

				foreach (HRJobApplication application in Applications)
				{
					application.HP_HA = applicant.PK;
				}
			}
		}

#endif
		#endregion

		#region Delete

		public override void Delete()
		{
			Applications.DeleteAll();
			AdPlacements.RemoveAndDeleteAll();
			WorkflowItems.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Properties

		#region CampaignID

		public ZString CampaignID
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				if (HV_CampaignStartDate.IsValid)
				{
					builder.Append(HV_CampaignStartDate.ToString("yyMMdd"));
					builder.Append("_");
				}
				if (!CampaignUNLOCO.IsEmpty)
				{
					builder.Append(CampaignUNLOCO);
					builder.Append("_");
				}
				builder.Append(HV_AdTitle);
				return builder.ToString();
			}
		}

		public ZPropertyInfo CampaignIDInfo
		{
			get { return GetZPropertyInfo(Schema.CampaignID); }
		}

		public static void GetInfoFromCampaignID(ZString campaignID, out ZString adTitle, out ZDateTime campaignStartDate, out ZString campaignUNLOCO)
		{
			adTitle = ZString.Empty;
			campaignStartDate = ZDate.Empty;
			campaignUNLOCO = ZString.Empty;

			string pattern = (NoResString)@"^((?<start_date>\d{6})_)?((?<unloco>\w+)_)?(?<title>.+)$"; // Regular expression pattern
			Match match = Regex.Match(campaignID, pattern, RegexOptions.Singleline);

			if (match != null)
			{
				if (match.Groups["start_date"] != null)
				{
					ZDateTime.TryParseExact(match.Groups["start_date"].Value, out campaignStartDate, "yyMMdd");
				}

				if (match.Groups["unloco"] != null)
				{
					campaignUNLOCO = match.Groups["unloco"].Value;
				}

				if (match.Groups["title"] != null)
				{
					adTitle = match.Groups["title"].Value;
				}
			}
		}

		#endregion

		#region HV_OH_ClientAccount

		public override ZGuid HV_OH_ClientAccount
		{
			get { return base.HV_OH_ClientAccount; }
			set
			{
				base.HV_OH_ClientAccount = value;

				if (ClientAccount != null)
				{
					HV_OA_ClientAddress = ClientAccount.MainAddress.PK;
					SetCampaignAndPlacementDates();
					HV_OC_ClientContactInfo.RefreshBinding();
				}
				else
				{
					HV_OA_ClientAddress = ZGuid.Empty;
					HV_OC_ClientContact = ZGuid.Empty;
				}
			}
		}
		#endregion

		#region HV_HJ_JobRole

		[RelatedBusinessObject("JobRole")]
		public override ZGuid HV_HJ_JobRole
		{
			get { return base.HV_HJ_JobRole; }
			set
			{
				if (base.HV_HJ_JobRole != value)
				{
					base.HV_HJ_JobRole = value;
				}
			}
		}

		#endregion

		public override ZGuid HV_OA_ClientAddress
		{
			get
			{
				return base.HV_OA_ClientAddress;
			}
			set
			{
				base.HV_OA_ClientAddress = value;
				SetCampaignAndPlacementDates();
			}
		}

		protected bool HV_OC_ClientContact_ReadOnly
		{
			get { return InvalidClientAccount; }
		}

		protected bool HV_OA_ClientAddress_ReadOnly
		{
			get { return InvalidClientAccount; }
		}

		bool InvalidClientAccount
		{
			get { return !HV_OH_ClientAccount.IsValid || ClientAccount == null; }
		}

		public ZString CampaignLocation
		{
			get { return ClientAddress != null ? ClientAddress.PortName : ZString.Empty; }
		}

		public ZString CampaignUNLOCO
		{
			get { return ClientAddress != null ? ClientAddress.OA_RL_NKRelatedPortCode : ZString.Empty; }
		}

		public ZString SalaryRangeAsText
		{
			get
			{
				ZString result;
				if (HV_WageLow.IsEmpty && HV_WageHigh.IsEmpty)
				{
					result = Res.GetString("5be89626-a4f9-490b-b483-c2a07af56731", "Negotiable on application");
				}
				else
				{
					result = (HV_WageLow != HV_WageHigh)
						? string.Format("{0} {1:N0} - {2:N0}", HV_RX_NKWageRangeCurrency, HV_WageLow, HV_WageHigh) // string.Format
						: string.Format("{0} {1:N0}", HV_RX_NKWageRangeCurrency, HV_WageHigh); // string.Format
				}
				return result;
			}
		}

		public ZString EffectiveEndDateAsText
		{
			get
			{
				ZString result;
				if (HV_CampaignEndDate.IsEmpty)
				{
					result = Res.GetString("8b759c03-513f-48ea-aa7b-69941b797cd0", "Ongoing");
				}
				else
				{
					result = HV_CampaignEndDate.ToShortDateString();
				}
				return result;
			}
		}

		public ZPropertyInfo EffectiveEndDateAsTextInfo
		{
			get { return GetZPropertyInfo(AutoHRRecruitmentJobCampaign.Schema.HV_CampaignEndDate); }
		}

		#region HV_CampaignStartDateLocal

		public ZDateTime HV_CampaignStartDateLocal
		{
			get { return hv_CampaignStartDateLocal; }
			set
			{
				hv_CampaignStartDateLocal = value.Date;
				HV_CampaignStartDate = value.IsValid ? (ZDateTime)Env.Time.GetUtcFromUnlocoTime(CampaignDateUNLOCO, value.Date.ToDateTime()) : value.Date;
				if (!IsValidationSuspended)
				{
					Validation.ValidateHV_CampaignEndDate();
				}
			}
		}
		ZDateTime hv_CampaignStartDateLocal;

		public ZPropertyInfo HV_CampaignStartDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.HV_CampaignStartDateLocal, x => HV_CampaignStartDateInfo); }
		}

		#endregion

		#region HV_CampaignEndDateLocal

		public ZDateTime HV_CampaignEndDateLocal
		{
			get { return hv_CampaignEndDateLocal; }
			set
			{
				hv_CampaignEndDateLocal = value.Date;
				HV_CampaignEndDate = value.IsValid ? (ZDateTime)Env.Time.GetUtcFromUnlocoTime(CampaignDateUNLOCO, value.Date.ToDateTime()) : value.Date;
				if (!IsValidationSuspended)
				{
					Validation.ValidateHV_CampaignStartDate();
				}
			}
		}
		ZDateTime hv_CampaignEndDateLocal;

		public ZPropertyInfo HV_CampaignEndDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.HV_CampaignEndDateLocal, x => HV_CampaignEndDateInfo); }
		}

		#endregion

		#region Campaign Stard/End Date TimeZone

		public ZString CampaignStartDateTimeZone
		{
			get { return HV_CampaignStartDate.IsValid ? GetTimeZoneDescription(HV_CampaignStartDate.ToDateTime()) : ZString.Empty; }
		}

		public ZString CampaignEndDateTimeZone
		{
			get { return HV_CampaignEndDate.IsValid ? GetTimeZoneDescription(HV_CampaignEndDate.ToDateTime()) : ZString.Empty; }
		}

		#endregion

		public ZString CampaignDateUNLOCO
		{
			get
			{
				ZString result = CampaignUNLOCO;
				if (result.IsEmpty)
				{
					result = ClientAccount != null ? ClientAccount.MainAddress.OA_RL_NKRelatedPortCode : ZString.Empty;
				}
				if (result.IsEmpty)
				{
					result = GlbBranch.CurrentBranch != null ? GlbBranch.CurrentBranch.HomePort.RL_Code : ZString.Empty;
				}
				return result;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new HRRecruitmentJobCampaignDocManagerInfo(this, Core.Constants.DocManagerCodes.JobCampaign);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region Related Business Objects

		#region Applications

		[ChildEditable(true)]
		public HRJobApplicationDependentCollection Applications
		{
			get
			{
				if (fApplications == null)
				{
					fApplications = new HRJobApplicationDependentCollection(this);
					RegisterEditableChildObject(fApplications);
				}

				return fApplications;
			}
		}

		HRJobApplicationDependentCollection fApplications;

		#endregion

		#region AdPlacements

		[ChildEditable(true)]
		public HRJobAdPlacementDependentCollection AdPlacements
		{
			get
			{
				if (fAdPlacements == null)
				{
					fAdPlacements = new HRJobAdPlacementDependentCollection(this);
					fAdPlacements.Load();
					RegisterEditableChildObject(fAdPlacements);
				}

				return fAdPlacements;
			}
		}

		HRJobAdPlacementDependentCollection fAdPlacements;

		#endregion

		#region Job Role

		public HRJobRole JobRole
		{
			get { return Factory.Load<HRJobRole>(HV_HJ_JobRole); }
		}

		#endregion

		#endregion

		#region Sending Email To Applicants

		protected override void OnFactorySaving()
		{
			if (fApplications != null)
			{
				newlyAddedApplications = Applications.Cast<HRJobApplication>().Where(a => !a.IsInDatabase && a.Applicant != null).ToArray();
			}

			base.OnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && newlyAddedApplications != null && newlyAddedApplications.Any())
			{
				if (NewApplicationsAdded != null)
				{
					NewApplicationsAdded(this, new NewApplicationsAddedEventArgs(newlyAddedApplications));
				}
				newlyAddedApplications = null;
			}
		}

		IEnumerable<HRJobApplication> newlyAddedApplications;
		public event EventHandler<NewApplicationsAddedEventArgs> NewApplicationsAdded;

		public class NewApplicationsAddedEventArgs : EventArgs
		{
			public NewApplicationsAddedEventArgs(IEnumerable<HRJobApplication> applications)
			{
				this.applications = applications;
			}
			public readonly IEnumerable<HRJobApplication> applications;
		}

		#endregion

		#region Campaign Date/TimeZone Calculation

		ZString GetTimeZoneDescription(DateTime utcDateTime)
		{
			ZString result = ZString.Empty;

			RefUNLOCO unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, CampaignDateUNLOCO);
			ITimeZone unlocoTimeZone = unloco.TimeZoneSet.GetCalculationTimeZone();
			ZString civilianCode = unlocoTimeZone.IsDaylightSavingBasedOnUtc(utcDateTime) ?
				unloco.TimeZoneSet.DaylightSavingZone.R2_CivilianTimeZoneCode : unloco.TimeZoneSet.StandardZone.R2_CivilianTimeZoneCode;

			result = string.Concat(unloco.RL_RN_NKCountryCode, " ", civilianCode);
			return result;
		}

		void SetCampaignAndPlacementDates()
		{
			ZString campaignUNLOCO = CampaignDateUNLOCO;
			HV_CampaignStartDate = HV_CampaignStartDateLocal.IsValid ? (ZDateTime)Env.Time.GetUtcFromUnlocoTime(campaignUNLOCO, HV_CampaignStartDateLocal.Date.ToDateTime()) : HV_CampaignStartDate.Date;
			HV_CampaignEndDate = HV_CampaignEndDateLocal.IsValid ? (ZDateTime)Env.Time.GetUtcFromUnlocoTime(campaignUNLOCO, HV_CampaignEndDateLocal.Date.ToDateTime()) : HV_CampaignEndDate.Date;

			foreach (HRJobAdPlacement placement in AdPlacements)
			{
				placement.HQ_EffectiveStartDate = placement.HQ_EffectiveStartDateLocal.IsValid ? (ZDateTime)Env.Time.GetUtcFromUnlocoTime(campaignUNLOCO, placement.HQ_EffectiveStartDateLocal.Date.ToDateTime()) : placement.HQ_EffectiveStartDate.Date;
				placement.HQ_EffectiveEndDate = placement.HQ_EffectiveEndDateLocal.IsValid ? (ZDateTime)Env.Time.GetUtcFromUnlocoTime(campaignUNLOCO, placement.HQ_EffectiveEndDateLocal.Date.ToDateTime()) : placement.HQ_EffectiveEndDate.Date;
			}
		}

		#endregion

		#region Workflow/Custom Fields

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
			=> new CustomBusinessObject(Factory, this, new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this));

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
			=> null;

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
			=> WorkflowItems;

		[ChildEditable(true)]
		public HRRecruitmentJobCampaignProcessTaskCollection WorkflowItems
		{
			get
			{
				if (fTasks == null)
				{
					fTasks = this.GetOrCreateProcessTaskCollection(() => new HRRecruitmentJobCampaignProcessTaskCollection(this));
					RegisterEditableChildObject(fTasks);
				}
				return fTasks;
			}
		}
		HRRecruitmentJobCampaignProcessTaskCollection fTasks;

		public IColumnValueRanker GetTemplateSelectionCriteria()
			=> new ColumnValueRanker();

		public ZString WorkflowType
			=> HRRecruitmentJobCampaignWorkflowDescriptor.WorkflowTypeCode;

		#endregion
	}
}
