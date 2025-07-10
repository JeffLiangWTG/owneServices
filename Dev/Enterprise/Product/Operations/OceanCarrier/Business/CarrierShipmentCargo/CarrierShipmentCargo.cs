using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.OceanCarrier.Business
{
	[CodeProperty("CSC_CargoID")]
	public sealed class CarrierShipmentCargo : AutoCarrierShipmentCargo, INumberFountainConsumer, IEDocsProvider, IWorkflowProvider, IWorkflowTriggerEventSource
	{
		IProcessHeaderCollection workflows;
		CarrierShipmentCargoProcessTaskCollection workflowItems;

		public CarrierShipmentCargo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CarrierShipmentHeader CarrierShipmentHeader => Factory.Load<CarrierShipmentHeader>(CSC_CSH_CarrierShipment);

		[RelatedBusinessObject(nameof(CarrierShipmentHeader))]
		public override ZGuid CSC_CSH_CarrierShipment
		{
			get => base.CSC_CSH_CarrierShipment;
			set => base.CSC_CSH_CarrierShipment = value;
		}

		public string JobNumber => CSC_CargoID.ToString();

		public ZDecimal GetTotalCargoGrossWeightMeasure()
		{
			var totalCargoGrossWeightMeasure = (CSC_DunnageWeight / CSC_PieceCount) + CSC_CargoWeight + CSC_EquipmentTareWeight;
			return decimal.Round(totalCargoGrossWeightMeasure, 3);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateCargoIdIfNeeded(this);
		}

		public static void PopulateCargoIdIfNeeded(CarrierShipmentCargo cargo)
		{
			if (!cargo.CSC_CargoID.IsEmpty || cargo.IsDeleted || cargo.IsInDatabase || !cargo.ShouldPopulateCargoId)
			{
				return;
			}

			if (cargo.Factory is IDbConnected)
			{
				cargo.CSC_CargoID = Env.NumberFountains.CarrierShipmentCargoID.GetNextFormatted(cargo.Factory);
			}
		}

		#region Workflow

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

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_GB, GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, CSC_CargoType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, GetCargoSubType2() ?? ZString.Empty, ZString.Empty);

			return result;
		}

		string GetCargoSubType2()
		{
			switch (CSC_CargoType)
			{
				case Core.Constants.OceanCarrierCargoTypes.Codes.RoRo:
					return CSC_RoRoType;
				case Core.Constants.OceanCarrierCargoTypes.Codes.BreakBulk:
					return CSC_F3_NKPackType;
				default:
					return ChargeableEquipmentType?.RC_ContainerType;
			}
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CarrierShipmentCargoProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		public ZString WorkflowType => WorkflowDescriptors.CarrierShipmentCargoWorkflowDescriptorCode;

		#endregion

		#region IEDocsProvider
		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CarrierShipmentCargoDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.CarrierShipmentCargo));

		DocManagerInfo docManagerInfo;

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		public bool ShouldPopulateCargoId { get; set; } = true;

		#region INumberFountainConsumer

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.CarrierShipmentCargoID;

		ZString INumberFountainEntityWithID.ID
		{
			get => CSC_CargoID;
			set => CSC_CargoID = value;
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany => CarrierShipmentHeader?.JobHeader?.Company ?? GlbCompany.GetCurrentCompany(Factory);

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders =>
			CarrierShipmentHeader == null ? Array.Empty<IWorkflowProviderCore>() : new IWorkflowProviderCore[] { CarrierShipmentHeader, };

		#endregion

		#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			CSC_CargoMovementTypeOrigin = "FCL";
			CSC_CargoMovementTypeDestination = "FCL";
			CSC_ReceiptDrayage = "ANY";
			CSC_DeliveryDrayage = "ANY";
		}

		#endif
	}
}
