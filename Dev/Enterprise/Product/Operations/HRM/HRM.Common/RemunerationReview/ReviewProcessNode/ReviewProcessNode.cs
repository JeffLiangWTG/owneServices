using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.HRM.Common
{
	[CodeProperty(nameof(UniqueName)), DescriptionProperty(nameof(UniqueName))]
	public class ReviewProcessNode : AutoReviewProcessNode, IWorkflowProvider, IConversationProvider, IConversationAdditionalParticipantProvider, IDocManagerSupport
	{
		public ReviewProcessNode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			var relationship = new DependentRelationship(this, typeof(ReviewProposal), new ZQuery(), ReviewProposalSchema.RRP_RRN_ReviewNode);
			Proposals = new ActiveBusinessObjectCollection<ReviewProposal>(Factory, relationship);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

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

		public IActiveBusinessObjectCollection<ReviewProposal> Proposals { get; }

		[ReadOnly(true)]
		[List("Lookups.ReviewProcessNodes")]
		[ResourceStringData("17490bf5-35e8-4621-9ac0-a2a9e295e658", Caption = "Parent Review")]
		public override ZGuid RRN_RRN_Parent { get => base.RRN_RRN_Parent; set => base.RRN_RRN_Parent = value; }

		[ReadOnly(true)]
		[ResourceStringData("43b8c82a-25e1-4352-ba5e-3820a06aaca0", Caption = "Reviewer")]
		public override ZGuid RRN_GS_Reviewer { get => base.RRN_GS_Reviewer; set => base.RRN_GS_Reviewer = value; }

		[ReadOnly(true)]
		[ResourceStringData("587753f9-8e4d-4efa-9cbd-9cee5ebe1c44", Caption = "Review")]
		public override ZGuid RRN_RPR_ReviewProcess { get => base.RRN_RPR_ReviewProcess; set => base.RRN_RPR_ReviewProcess = value; }

		[ReadOnly(true)]
		public override ZString RRN_Status { get => base.RRN_Status; set => base.RRN_Status = value; }

		[ChildEditable]
		public ReviewProcessNodeProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ReviewProcessNodeProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ReviewProcessNodeProcessTaskCollection workflowItems;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		public ZString WorkflowType => WorkflowDescriptors.ReviewProcessNodeWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
			=> new ColumnValueRanker();

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
			=> null;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			WorkflowItems.Reload(true);
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		[MaxLength(100)]
		public ZString UniqueName => $"{ReviewProcess?.RPR_Name} - {Reviewer?.GS_Code}";

		public JobConversation eConversation
		{
			get
			{
				if (!IsInDatabase)
				{
					return null;
				}
				return conversation ?? (conversation = GetOrCreateConversation());
			}
		}
		JobConversation conversation;
		JobConversation GetOrCreateConversation()
		{
			var result = JobConversation.GetOrCreate(this);
			RegisterEditableChildObject(result);
			return result;
		}

		public ModuleIdentifier ParentModule => ModuleIDs.ReviewProcessNode;

		public ControllerID ParentController => ControllerIDs.ReviewProcessNode;

		public IEnumerable<EConversation.Business.RelatedParty> AdditionalParticipants => Enumerable.Empty<EConversation.Business.RelatedParty>();

		public bool SendEmailNotificationsOnSave => false;

		public string EmailSubjectContentOverride => default;

		public string FromAddressOverride => default;

		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ReviewProcessNode);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		public void RunConversationUpdateActionBeforeSaving()
		{
		}

		public IEnumerable<IConversationParticipant> GetAdditionalParticipants(IReadOnlyCollection<IConversationParticipant> subscribedParticipants, JobConversationParticipant sender)
		{
			yield return Reviewer;
		}

		public override string ToString() => UniqueName;

		#region HumanReadableName

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Name Core")]
		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (RRN_GS_Reviewer.IsEmpty && RRN_RPR_ReviewProcess.IsEmpty)
				{
					return "Review";
				}

				if (RRN_GS_Reviewer.IsEmpty)
				{
					return ReviewProcess.RPR_Name;
				}

				if (RRN_RPR_ReviewProcess.IsEmpty)
				{
					return $"Review - {Reviewer.GS_Code}";
				}

				return $"{ReviewProcess.RPR_Name} - {Reviewer.GS_Code}";
			}
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RRN_Status = "ASN";

			if (!RRN_GS_Reviewer.IsValid)
			{
				RRN_GS_Reviewer = Factory.NewWithValidTestData<GlbStaff>().PK;
			}

			if (!RRN_RPR_ReviewProcess.IsValid)
			{
				RRN_RPR_ReviewProcess = Factory.NewWithValidTestData<ReviewProcess>().PK;
			}
		}
#endif
	}
}
