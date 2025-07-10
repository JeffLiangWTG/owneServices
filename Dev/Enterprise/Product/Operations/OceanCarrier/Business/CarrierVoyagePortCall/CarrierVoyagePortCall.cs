using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.OceanCarrier.Business
{
	[CodeProperty("CPO_PortCallId")]
	public sealed class CarrierVoyagePortCall : AutoCarrierVoyagePortCall, INumberFountainConsumer, IWorkflowProvider, IEDocsProvider
	{
		IProcessHeaderCollection workflows;
		CarrierVoyagePortCallProcessTaskCollection workflowItems;

		public CarrierVoyagePortCall(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

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

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CarrierVoyagePortCallProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateVoyagePortCallIdIfNeeded(this);
		}

		public static void PopulateVoyagePortCallIdIfNeeded(CarrierVoyagePortCall portCall)
		{
			if (!portCall.CPO_PortCallId.IsEmpty || portCall.IsDeleted || portCall.IsInDatabase || !portCall.ShouldPopulateVoyagePortCallId)
			{
				return;
			}

			if (portCall.Factory is IDbConnected)
			{
				portCall.CPO_PortCallId = Env.NumberFountains.CarrierVoyagePortCallID.GetNextFormatted(portCall.Factory);
			}
		}

		public bool ShouldPopulateVoyagePortCallId { get; set; } = true;

		#region INumberFountainConsumer

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.CarrierVoyagePortCallID;

		ZString INumberFountainEntityWithID.ID
		{
			get => CPO_PortCallId;
			set => CPO_PortCallId = value;
		}

		#endregion

		public ZString WorkflowType => WorkflowDescriptors.CarrierVoyagePortCallWorkflowDescriptorCode;

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CarrierVoyagePortCallDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.CarrierVoyagePortCall));

		DocManagerInfo docManagerInfo;

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_GB, GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
			return result;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;
	}
}
