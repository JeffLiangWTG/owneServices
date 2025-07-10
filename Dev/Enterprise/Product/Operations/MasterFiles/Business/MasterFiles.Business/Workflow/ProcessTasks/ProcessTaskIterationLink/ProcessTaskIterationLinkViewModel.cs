using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskIterationLinkViewModel : NonPersistentBusinessObject
	{
		public ProcessTaskIterationLinkViewModel(IProcessTaskIterationLink processTaskIterationLink)
			: base(Argument.NotNull(processTaskIterationLink, nameof(processTaskIterationLink)).Factory)
		{
			this.ProcessTaskIterationLink = processTaskIterationLink;
			this.ResourceUnderReviewNK = processTaskIterationLink.P9I_GS_NKResourceUnderReview;
			this.IterationReason = processTaskIterationLink.P9I_IterationReason;
		}

		public IProcessTaskIterationLink ProcessTaskIterationLink { get; private set; }
		ZString resourceUnderReviewNK;
		ZString iterationReason;

		#region Fields

		#region Settable

		[ResourceStringData("ProcessTaskIterationLink.P9I_Outcome", Caption = "Outcome", FullDescription = "The outcome of this containment barrier at the time of assessment.")]
		public ZString P9I_Outcome => ProcessTaskIterationLink.P9I_Outcome;
		public ZPropertyInfo P9I_OutcomeInfo => GetZPropertyInfo(nameof(P9I_Outcome));

		[ResourceStringData("ProcessTaskIterationLink.OutcomeDescription", Caption = "Outcome Description", FullDescription = "The outcome of this containment barrier at the time of assessment.")]
		public ZString OutcomeDescription
		{
			get
			{
				var outcomeDescription = string.Empty;
				switch (P9I_Outcome)
				{
					case IterationLinkOutcomeList.Codes.Passed:
						outcomeDescription = Res.GetString("1F9D6ADE-E84A-41E3-AC36-37C7B4EE7B32", "Passed");
						break;
					case IterationLinkOutcomeList.Codes.IterationRequired:
						outcomeDescription = Res.GetString("458C26FF-E7CE-4C1B-8261-D2F3FEA2FA3A", "Iteration Created");
						break;
					case IterationLinkOutcomeList.Codes.Deferred:
						outcomeDescription = Res.GetString("32AFD0A9-AE58-4509-9AC1-8505F83CDB33", "Deferred to another user");
						break;
				}

				return outcomeDescription;
			}
		}
		public ZPropertyInfo OutcomeDescriptionInfo => GetZPropertyInfo(nameof(OutcomeDescription));

		[MaxLength(nameof(ResourceUnderReviewMaxLength))]
		[List("Lookups.ResourceUnderReviewList")]
		[ResourceStringData("ProcessTaskIterationLink.ResourceUnderReviewNK", Caption = "User Under Review", FullDescription = "The user whose work was under review.")]
		public ZString ResourceUnderReviewNK
		{
			get
			{
				return resourceUnderReviewNK;
			}
			set
			{
				CheckMaximumLength(ResourceUnderReviewNKInfo, value);
				SetNonPersistentPropertyValue(ResourceUnderReviewNKInfo, ref resourceUnderReviewNK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateResourceUnderReviewNK();
				}

				if (!ResourceUnderReviewNKInfo.HasErrors())
				{
					ProcessTaskIterationLink.P9I_GS_NKResourceUnderReview = resourceUnderReviewNK;
					resourceUnderReview = null;
				}
			}
		}

		public ZPropertyInfo ResourceUnderReviewNKInfo => GetZPropertyInfo(nameof(ResourceUnderReviewNK));

		#endregion

		[ResourceStringData("ProcessTaskIterationLink.ResourceUnderReviewFullName", Caption = "User Full Name", FullDescription = "The full name of the user whose work was under review.")]
		public ZString ResourceUnderReviewFullName => ResourceUnderReview?.GS_FullName ?? ZString.Empty;
		public ZPropertyInfo ResourceUnderReviewFullNameInfo => GetZPropertyInfo(nameof(ResourceUnderReviewFullName));

		[MaxLength(nameof(IterationReasonMaxLength))]
		[List("Lookups.IterationReasons")]
		[ReadOnlyMember(nameof(IterationReason_ReadOnly))]
		[ResourceStringData("ProcessTaskIterationLinkIterationReason", Caption = "Iteration Reason", FullDescription = "The reason given for the iteration being created.")]
		public ZString IterationReason
		{
			get { return iterationReason; }
			set
			{
				CheckMaximumLength(IterationReasonInfo, value);
				SetNonPersistentPropertyValue(IterationReasonInfo, ref iterationReason, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateIterationReason();
				}

				if (!IterationReasonInfo.HasErrors())
				{
					ProcessTaskIterationLink.P9I_IterationReason = iterationReason;
					workflowIterationReason = null;
				}
			}
		}
		public ZPropertyInfo IterationReasonInfo => GetZPropertyInfo(nameof(IterationReason));

		protected bool IterationReason_ReadOnly => P9I_Outcome != IterationLinkOutcomeList.Codes.IterationRequired;

		[ResourceStringData("ProcessTaskIterationLink.IterationReasonDescription", Caption = "Reason Description", FullDescription = "The reason given for the iteration being created.")]
		public ZString IterationReasonDescription => WorkflowIterationReason?.Description ?? ZString.Empty;
		public ZPropertyInfo IterationReasonDescriptionInfo => GetZPropertyInfo(nameof(IterationReasonDescription));

		[ResourceStringData("ProcessTaskIterationLink.P9I_SystemCreateTimeUtc", Caption = "Create Time (UTC)", FullDescription = "The time at which this containment barrier outcome was assessed.")]
		public ZDateTime P9I_SystemCreateTimeUtc => ProcessTaskIterationLink.P9I_SystemCreateTimeUtc;
		public ZPropertyInfo P9I_SystemCreateTimeUtcInfo => GetZPropertyInfo(nameof(P9I_SystemCreateTimeUtc));

		[ResourceStringData("ProcessTaskIterationLink.P9I_SystemCreateUser", Caption = "Create User", FullDescription = "The user who assessed this containment barrier.")]
		public ZString P9I_SystemCreateUser => ProcessTaskIterationLink.P9I_SystemCreateUser;
		public ZPropertyInfo P9I_SystemCreateUserInfo => GetZPropertyInfo(nameof(P9I_SystemCreateUser));

		[ResourceStringData("ProcessTaskIterationLink.P9I_SystemLastEditTimeUtc", Caption = "Last Edit Time (UTC)", FullDescription = "The time at which this containment barrier outcome was last edited.")]
		public ZDateTime P9I_SystemLastEditTimeUtc => ProcessTaskIterationLink.P9I_SystemLastEditTimeUtc;
		public ZPropertyInfo P9I_SystemLastEditTimeUtcInfo => GetZPropertyInfo(nameof(P9I_SystemLastEditTimeUtc));

		[ResourceStringData("ProcessTaskIterationLink.P9I_SystemLastEditUser", Caption = "Last Edit User", FullDescription = "The user who last edited this containment barrier outcome.")]
		public ZString P9I_SystemLastEditUser => ProcessTaskIterationLink.P9I_SystemLastEditUser;
		public ZPropertyInfo P9I_SystemLastEditUserInfo => GetZPropertyInfo(nameof(P9I_SystemLastEditUser));

		[ResourceStringData("ProcessTaskIterationLinkSystemCreateTime", Caption = "Create Time", FullDescription = "The reason given for the iteration being created.")]
		public ZDateTime SystemCreateTime => Env.Time.GetLocalTimeFromUtc(ProcessTaskIterationLink.P9I_SystemCreateTimeUtc.ToDateTime());
		public ZPropertyInfo SystemCreateTimeInfo => GetZPropertyInfo(nameof(SystemCreateTime));

		[ResourceStringData("ProcessTaskIterationLinkSystemLastEditTimeUtc", Caption = "Last Edit Time", FullDescription = "The time at which this containment barrier outcome was last edited.")]
		public ZDateTime SystemLastEditTime => Env.Time.GetLocalTimeFromUtc(ProcessTaskIterationLink.P9I_SystemLastEditTimeUtc.ToDateTime());
		public ZPropertyInfo SystemLastEditTimeInfo => GetZPropertyInfo(nameof(SystemLastEditTime));

		#endregion

		#region Lookups

		public ProcessTaskIterationLinkViewModelLookups Lookups => lookups ?? (lookups = new ProcessTaskIterationLinkViewModelLookups(this));
		ProcessTaskIterationLinkViewModelLookups lookups;

		#endregion

		#region Implementation

		WorkflowIterationReason WorkflowIterationReason => workflowIterationReason ?? (workflowIterationReason = WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReason(WorkflowType, IterationReason));
		WorkflowIterationReason workflowIterationReason;

		public ZString WorkflowType => workflowType ?? (workflowType = ((ProcessTask)ProcessTaskIterationLink.ContainmentBarrierTask)?.Parent?.WorkflowType) ?? ZString.Empty;
		ZString? workflowType;

		GlbStaff ResourceUnderReview => resourceUnderReview ?? (resourceUnderReview = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ResourceUnderReviewNK));
		GlbStaff resourceUnderReview;

		public ProcessTaskIterationLinkViewModelValidation Validation => GetNewValidation();

		public ProcessTaskIterationLinkViewModelValidation GetNewValidation()
		{
			return new ProcessTaskIterationLinkViewModelValidation(this);
		}

		int IterationReasonMaxLength => ProcessTaskIterationLinkSchema.P9I_IterationReason.MaxLength;
		int ResourceUnderReviewMaxLength => ProcessTaskIterationLinkSchema.P9I_GS_NKResourceUnderReview.MaxLength;

		#endregion
	}
}
