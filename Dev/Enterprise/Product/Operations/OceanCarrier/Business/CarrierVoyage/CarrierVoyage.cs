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
	[CodeProperty("CVO_VoyageId")]
	public sealed class CarrierVoyage : AutoCarrierVoyage, INumberFountainConsumer, IWorkflowProvider, IEDocsProvider
	{
		IProcessHeaderCollection workflows;
		CarrierVoyageProcessTaskCollection workflowItems;

		public CarrierVoyage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public CarrierService CarrierService => Factory.Load<CarrierService>(CVO_CSV_Service);

		[RelatedBusinessObject(nameof(CarrierService))]
		public override ZGuid CVO_CSV_Service
		{
			get => base.CVO_CSV_Service;
			set => base.CVO_CSV_Service = value;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateVoyageIdIfNeeded(this);
		}

		public static void PopulateVoyageIdIfNeeded(CarrierVoyage voyage)
		{
			if (!voyage.CVO_VoyageId.IsEmpty || voyage.IsDeleted || voyage.IsInDatabase || !voyage.ShouldPopulateVoyageId)
			{
				return;
			}

			if (voyage.Factory is IDbConnected)
			{
				voyage.CVO_VoyageId = Env.NumberFountains.CarrierVoyageID.GetNextFormatted(voyage.Factory);
			}
		}

		public bool ShouldPopulateVoyageId { get; set; } = true;

		#region INumberFountainConsumer

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.CarrierVoyageID;

		ZString INumberFountainEntityWithID.ID
		{
			get => CVO_VoyageId;
			set => CVO_VoyageId = value;
		}

		#endregion

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CarrierVoyageProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		public ZString WorkflowType => WorkflowDescriptors.CarrierVoyageWorkflowDescriptorCode;

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CarrierVoyageDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.CarrierVoyage));

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
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, CVO_IsPublished ? "YES" : "NO", ZString.Empty);
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
