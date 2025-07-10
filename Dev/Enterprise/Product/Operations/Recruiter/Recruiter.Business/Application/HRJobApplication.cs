using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty(AutoHRJobApplication.Schema.HP_ApplicationNumber), DescriptionProperty(Schema.JobCampaignTitle)]
	[DependentBusinessObject(typeof(HRJobApplicant), "Applications")]
	public class HRJobApplication : AutoHRJobApplication,
		IHRJobApplication,
		IDocManagerSupport,
		IWorkflowProvider,
		IHaveRequiredDocuments
	{
		public HRJobApplication(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			HP_SourceType = ReferringSourcesTypes.Codes.NoReferrerRegistered;
			HP_ApplicationOverallRating = "-1";
		}

		void SetDefaultCountryForApplicant()
		{
			if (Applicant == null || JobOpening == null || !Applicant.HA_RN_NKCountry.IsEmpty)
			{
				return;
			}

			var country = JobOpening.ClientAddress?.OA_RN_NKCountryCode ?? JobOpening.ClientAccount?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
			if (!country.IsEmpty)
			{
				Applicant.HA_RN_NKCountry = country;
			}
		}

		#endregion

		#region Schema

		public new class Schema : AutoHRJobApplication.Schema
		{
			public const string JobCampaignTitle = "JobCampaignTitle";
			public const string SubmissionTimeLocal = "SubmissionTimeLocal";
			public const string ApplicationOverallRatingDescription = "ApplicationOverallRatingDescription";
			public const string Applicant_HA_EmailAddress = "Applicant_HA_EmailAddress";
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted) // Potential for double-delete from the active bizo collections
			{
				Interviews.RemoveAndDeleteAll();
				Documents.DeleteAll();
				RequiredDocuments.DeleteAll();
				WorkflowItems.RemoveAndDeleteAll();
				ParsingQueue.DeleteAll();
			}

			base.Delete();
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString commentText = Res.GetString("a2b1cdb1-e490-4467-8554-040da15e408f", "Application") + " " + (Applicant != null ? Res.GetString("0c9b2a20-ed7d-475c-9206-70094fbf848c", "from {0}", Applicant.HA_FullName) : "");
				commentText = commentText + (JobOpening != null ? " " + Res.GetString("12301dbe-a4df-4e28-b118-9cef8a907a85", "for Campaign {0}", JobOpening.HV_AdTitle) : "");
				if (AssignedTo != null && (HP_GS_NKAssignedToInfo.HasChanges || !IsInDatabase))
				{
					commentText = Res.GetString("f3bfda4e-cb8d-4bb6-8a94-337d4b58f1db", "{0} Assigned To {1}", commentText, AssignedTo.GS_FullName);
				}

				return commentText;
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList objects = new ArrayList();

				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.AddRange(Interviews);
				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var objects = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);
				if (Applicant != null)
				{
					objects.Add(Applicant);

					var relatedApplications = Applicant.Applications.ToArray().Where(application => application.PK != PK);
					objects.AddRange(relatedApplications);
				}
				return objects.ToArray();
			}
		}

		#endregion

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (HP_SubmissionTimeUtc.IsEmpty)
			{
				HP_SubmissionTimeUtc = ZDateTime.UtcNow;
			}

			if (HasChanges && IsInDatabase)
			{
				var previousRating = HP_ApplicationOverallRatingInfo?.OriginalValue?.ToString();
				if (previousRating != "3" && HP_ApplicationOverallRating == "3")
				{
					RejectionHandler.QueueRejectionEmail(this);
				}
				else if (previousRating == "3" && HP_ApplicationOverallRating != "3")
				{
					RejectionHandler.CancelRejectionEmail(this);
				}
			}

			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			SetApplicationNumberIfRequired();

			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					HP_ApplicationNumber = "";
				}
			}
			base.OnSaved(saveSucceeded);
		}

		static IAutomatedEmailRejectionHandler RejectionHandler => ObjectFactory.Get<IAutomatedEmailRejectionHandler>();

		public void SetApplicationNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(HP_ApplicationNumberInfo, ApplicationNumberFountain);
			}
		}

		INumberFountainProxy ApplicationNumberFountain
		{
			get { return Env.NumberFountains.JobApplicationNo; }
		}

		#endregion

		#region Properties

		#region HP_HA

		[RelatedBusinessObject("Applicant")]
		public override ZGuid HP_HA
		{
			get { return base.HP_HA; }
			set
			{
				if (base.HP_HA != value)
				{
					if (Applicant != null)
					{
						ApplicantCollection.Remove(Applicant);
					}

					base.HP_HA = value;
					SetDefaultCountryForApplicant();

					if (Applicant != null)
					{
						ApplicantCollection.Add(Applicant);
					}
				}
			}
		}

		#endregion

		#region HP_HV

		[RelatedBusinessObject("JobOpening")]
		[List("Lookups.Campaigns")]
		public override ZGuid HP_HV
		{
			get { return base.HP_HV; }
			set
			{
				if (base.HP_HV != value)
				{
					base.HP_HV = value;
					SetDefaultCountryForApplicant();
				}
			}
		}

		#endregion

		#region ApplicationOverallRating

		[List("Lookups.OverallRatings")]
		public override ZString HP_ApplicationOverallRating { get => base.HP_ApplicationOverallRating; set => base.HP_ApplicationOverallRating = value; }

		[MaxLength(10)]
		[List("Lookups.OverallRatings")]
		public ZString ApplicationOverallRatingDescription
		{
			get => applicationOverallRatingDescription ?? Lookups.OverallRatings.GetDescriptionFromCode(HP_ApplicationOverallRating);

			set
			{
				CheckMaximumLength(ApplicationOverallRatingDescriptionInfo, value);

				//only save invalid value for validation.
				applicationOverallRatingDescription = null;
				if (value.IsEmpty || Lookups.OverallRatings.ContainsCode(value))
				{
					HP_ApplicationOverallRating = value;
				}
				else
				{
					var codeFromDescription = Lookups.OverallRatings.GetCodeFromDescription(value);
					if (codeFromDescription != null)
					{
						HP_ApplicationOverallRating = codeFromDescription;
					}
					else
					{
						HP_ApplicationOverallRating = ZString.Empty;
						applicationOverallRatingDescription = value;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateApplicationOverallRatingDescription();
				}

				ApplicationOverallRatingDescriptionInfo.RefreshBinding();
			}
		}

		string applicationOverallRatingDescription;

		public ZPropertyInfo ApplicationOverallRatingDescriptionInfo => GetZPropertyInfo(Schema.ApplicationOverallRatingDescription);

		#endregion

		#region HP_CurrentStatus

		[List("Lookups.ApplicationStatuses")]
		public override ZString HP_CurrentStatus { get => base.HP_CurrentStatus; set => base.HP_CurrentStatus = value; }

		#endregion

		#region Code & Description attribute property (for eDocs in Applicant)

		public ZString JobCampaignTitle
		{
			get { return JobOpening != null ? JobOpening.HV_AdTitle : ZString.Empty; }
		}

		public ZPropertyInfo JobCampaignTitleInfo
		{
			get { return GetZPropertyInfo(Schema.JobCampaignTitle); }
		}

		#endregion

		#region Applied Date

		public ZDateTime AppliedDate
		{
			get
			{
				var createTime = Logs.CreatedDateUtc;
				if (!createTime.IsValid)
				{
					return createTime;
				}
				else if (JobOpening?.CampaignUNLOCO.IsEmpty ?? true)
				{
					return Env.Time.GetLocalTimeFromUtc(createTime.ToDateTime());
				}
				else
				{
					return Env.Time.GetUnlocoTimeFromUtc(JobOpening.CampaignUNLOCO, createTime.ToDateTime());
				}
			}
		}

		#endregion

		#region Parsed Documents

		public ZBool ParsedDocuments => Documents.Any();

		#endregion

		#region Submission Time Local

		[BusinessObjectTestExclude]
		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public ZDateTime SubmissionTimeLocal
		{
			get
			{
				ZDateTime result = HP_SubmissionTimeUtc;

				if (result.IsEmpty)
				{
					return Env.Time.GetLocalTimeFromUtc(ZDateTime.UtcNow.ToDateTime());
				}
				return Env.Time.GetLocalTimeFromUtc(result.ToDateTime());
			}
			set
			{
				DateTime result = DateTime.UtcNow;
				if (value.IsValid && !value.IsEmpty)
				{
					result = value.ToDateTime();
				}
				HP_SubmissionTimeUtc = Env.Time.GetUtcFromLocalTime(result);

				SubmissionTimeLocalInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SubmissionTimeLocalInfo
		{
			get { return GetZPropertyInfo(Schema.SubmissionTimeLocal); }
		}

		#endregion

		#region Referring Party

		[List("Lookups.SourceTypes")]
		public override ZString HP_SourceType
		{
			get => base.HP_SourceType;
			set
			{
				base.HP_SourceType = value;
				if (!IsReferringOrganisationApplicable)
				{
					HP_OH_ReferringOrganisation = ZGuid.Empty;
				}

				if (!IsReferringPersonApplicable)
				{
					HP_PER_ReferringPerson = ZGuid.Empty;
				}

				if (!IsReferringStaffApplicable)
				{
					referringStaffCode = ZString.Empty;
				}
			}
		}

		public override ZGuid HP_OH_ReferringOrganisation
		{
			get => base.HP_OH_ReferringOrganisation;
			set
			{
				base.HP_OH_ReferringOrganisation = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateHP_PER_ReferringPerson();
				}
			}
		}

		public bool HP_OH_ReferringOrganisation_ReadOnly => !IsReferringOrganisationApplicable;

		#region ReferringStaffCode

		[MaxLength(3)]
		[RelatedBusinessObject("ReferringStaff")]
		[List("Lookups.ReferringStaffs")]
		[BusinessObjectTestExclude]
		public ZString ReferringStaffCode
		{
			get
			{
				var result = ZString.Empty;
				if (IsReferringStaffApplicable)
				{
					result = !referringStaffCode.IsEmpty ? referringStaffCode : (ReferringPerson?.StaffCollection.FirstOrDefault()?.GS_Code ?? ZString.Empty);
				}
				return result;
			}
			set
			{
				CheckMaximumLength(ReferringStaffCodeInfo, value);
				HP_PER_ReferringPerson = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, value)?.Person?.PK ?? ZGuid.Empty;

				//only save invalid value for validation.
				SetNonPersistentPropertyValue(HP_PER_ReferringPersonInfo, ref referringStaffCode, HP_PER_ReferringPerson.IsValid ? ZString.Empty : value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateReferringStaffCode();
				}

				ReferringStaffCodeInfo.RefreshBinding();
			}
		}

		ZString referringStaffCode;

		public bool ReferringStaffCode_ReadOnly => !IsReferringStaffApplicable;

		public virtual GlbStaff ReferringStaff => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ReferringStaffCode);

		public virtual ZPropertyInfo ReferringStaffCodeInfo => GetZPropertyInfo(nameof(ReferringStaffCode));

		public bool IsReferringOrganisationApplicable => !HP_SourceType.EqualsIgnoringCase(ReferringSourcesTypes.Codes.Website) && !HP_SourceType.EqualsIgnoringCase(ReferringSourcesTypes.Codes.StaffReferral);

		public bool IsReferringOrganisationDropDown => RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.Value.Cast<ReferringPartyConfiguration>()
					.Any(x => x.DefaultReferringSource.EqualsIgnoringCase(HP_SourceType) && x.ReferringParty.EqualsIgnoringCase(OrgHeaderSchema.Constants.Prefix));

		public bool IsReferringPersonApplicable => !HP_SourceType.EqualsIgnoringCase(ReferringSourcesTypes.Codes.StaffReferral) && SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.Value;

		public bool IsReferringStaffApplicable => HP_SourceType.EqualsIgnoringCase(ReferringSourcesTypes.Codes.StaffReferral);

		public bool IsReferringOrganisationMandatory => IsReferringOrganisationApplicable &&
			(HP_SourceType.EqualsIgnoringCase(ReferringSourcesTypes.Codes.RecruitmentAgent) ||
				RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.Value.Cast<ReferringPartyConfiguration>()
					.Any(x => x.DefaultReferringSource.EqualsIgnoringCase(HP_SourceType) && x.ReferringParty.EqualsIgnoringCase(OrgHeaderSchema.Constants.Prefix)));

		#endregion

		#region ReferringPerson

		public bool HP_PER_ReferringPerson_ReadOnly => !IsReferringPersonApplicable;

		#endregion

		public void SetupReferringSource(ZString senderEmailAddress)
		{
			if (!senderEmailAddress.IsEmpty)
			{
				ZGuid groupPK = RecruiterDataRegistry.Instance.ReferringStaffMembersToIgnoreGroup.Value;
				if (!groupPK.IsEmpty)
				{
					var group = Factory.Load<GlbGroup>(groupPK);
					if (group != null)
					{
						if (group.Staff.Cast<GlbStaff>().Any(x => x.GS_EmailAddress.EqualsIgnoringCase(senderEmailAddress)))
						{
							return;
						}
					}
				}

				var configurations = RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.Value.Cast<ReferringPartyConfiguration>();
				var firstMatch = configurations.FirstOrDefault(x => x.Domain.EqualsIgnoringCase(senderEmailAddress))
					?? configurations.FirstOrDefault(x => senderEmailAddress.EndsWith(x.Domain, StringComparison.OrdinalIgnoreCase));
				{
					firstMatch = configurations.FirstOrDefault(x => senderEmailAddress.EndsWith(x.Domain, StringComparison.OrdinalIgnoreCase));
				}

				if (firstMatch != null)
				{
					HP_SourceType = firstMatch.DefaultReferringSource;

					if (firstMatch.ReferringParty == OrgHeaderSchema.Constants.Prefix)
					{
						if (!firstMatch.OrganizationPK.IsEmpty)
						{
							HP_OH_ReferringOrganisation = firstMatch.OrganizationPK;
						}
					}
					else
					{
						var query = new ZQuery(GlbStaffSchema.GS_EmailAddress, senderEmailAddress);
						query.AddToFilter(GlbStaffSchema.GS_IsActive, ZBool.True);
						query.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
						query.AddToFilter(GlbStaffSchema.GS_IsDevice, false);
						query.OrderBy = GlbStaffSchema.Constants.GS_Code;

						var staff = Factory.LoadTop1<GlbStaff>(query);
						if (staff != null)
						{
							HP_PER_ReferringPerson = staff.GS_PER;
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region IWorkflowProvider Members

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, JobOpening?.ClientAccount?.PK ?? ZGuid.Empty, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, Applicant?.HA_RN_NKCountry ?? string.Empty, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, JobOpening?.HV_GS_NKControlledBy ?? string.Empty, ZString.Empty);
			return result;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new HRJobApplicationWorkflowDescriptor().Code; }
		}

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
		{
			get { return WorkflowItems; }
		}

		[ChildEditable]
		public HRJobApplicationProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					if (IsNull || !PK.IsValid)
					{
						workflowItems = this.GetOrCreateProcessTaskCollection(() => new HRJobApplicationProcessTaskCollection(this, ZQuery.NoResultQuery));
					}
					else
					{
						workflowItems = this.GetOrCreateProcessTaskCollection(() => new HRJobApplicationProcessTaskCollection(this));
					}

					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		HRJobApplicationProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			HP_HV = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>().PK;
			HP_ApplicationNumber = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, AutoHRJobApplication.Schema.HP_ApplicationNumberMaxLength);
		}

#endif
		#endregion

		#region Related Business Objects

		HRJobApplicantCollection applicantCollection;
		public HRJobApplicantCollection ApplicantCollection
		{
			get
			{
				if (applicantCollection == null)
				{
					applicantCollection = new HRJobApplicantCollection(Factory);
					if (Applicant != null)
					{
						applicantCollection.Add(Applicant);
					}
				}
				return applicantCollection;
			}
		}

		public HRJobApplicant Applicant
		{
			get { return (HRJobApplicant)Factory.Load(TypeOfJobApplicantToLoad, HP_HA); }
		}

		protected virtual Type TypeOfJobApplicantToLoad
		{
			get { return typeof(HRJobApplicant); }
		}

		public HRRecruitmentJobCampaign JobOpening
		{
			get { return Factory.Load<HRRecruitmentJobCampaign>(HP_HV); }
		}

		public ProcessTask CurrentTask => WorkflowItems.GetCurrentTask();

		#region Interviews

		[ChildEditable(true)]
		public HRJobApplicationInterviewDependentCollection Interviews
		{
			get
			{
				if (fInterviews == null)
				{
					fInterviews = new HRJobApplicationInterviewDependentCollection(this);
					fInterviews.Load();
					RegisterEditableChildObject(fInterviews);
				}

				return fInterviews;
			}
		}

		HRJobApplicationInterviewDependentCollection fInterviews;

		#endregion

		#region Documents

		[ChildEditable(true)]
		public HRJobApplicationDocumentCollection Documents
		{
			get
			{
				if (documents == null)
				{
					documents = new HRJobApplicationDocumentCollection(this);
					RegisterEditableChildObject(documents);
				}
				return documents;
			}
		}
		HRJobApplicationDocumentCollection documents;

		#endregion

		#endregion

		#region Parsing Queue

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		HRJobApplicationParsingQueue[] ParsingQueue => Factory.Load<HRJobApplicationParsingQueue>(new ZQuery(HRJobApplicationParsingQueueSchema.HPQ_HP, PK));

		#endregion

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var title = Applicant?.Name;
				if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(JobCampaignTitle))
				{
					title += " - ";
				}
				title += JobCampaignTitle;
				return title;
			}
		}

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new HRJobApplicationDocManagerInfo(this, Core.Constants.DocManagerCodes.JobApplication);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IHaveRequiredDocuments members

		public ZString UniqueConsignRef => null;

		public ZString HouseBill => null;

		public ZString MasterBill => null;

		public OrgHeader ExportBroker => null;

		public ZString TableCode => HRJobApplicationSchema.Constants.Prefix;

		[ChildEditable]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					requiredDocuments.Load();
					RegisterEditableChildObject(requiredDocuments);
				}
				return requiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection requiredDocuments;

		public BusinessObject UltimateDocumentParent => this;

		public IReadOnlyList<ZString> AdditionalRefTypes => Array.Empty<ZString>();

		public void PreLogAllDocumentsReceivedEvents()
		{
		}

		#endregion

		public override string ToString()
			=> FormattableString.Invariant($"Candidate({Applicant?.HA_FullName})");

		public StmALog LogRCEEvent(HRJobApplicationEvent recEvent, string info)
		{
			var logParameters = new KeyValuePair<string, string>[]
			{
				new((NoResString)"EVT", recEvent.ToString()),
				new((NoResString)"DES", info),
			};

			return Logs.AddNew(AutoEvents.RecruitmentCandidateEvent, logParameters);
		}

		public override Notes Notes
			=> notes ?? (notes = new HRCandidateNotes(this));
		Notes notes;

		public GroupedEConversation EConversation
			=> IsNull ? null : (mergedConversation ?? (mergedConversation = new GroupedEConversation(this, (NoResString)"Candidate created")));
		GroupedEConversation mergedConversation;
	}
}
