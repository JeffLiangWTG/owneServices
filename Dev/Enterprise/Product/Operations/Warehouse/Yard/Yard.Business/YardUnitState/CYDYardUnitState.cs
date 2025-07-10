using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New BusinessObject for Container Yard project")]
	[CodeProperty(Schema.YUS_UnitID)]
	[UniversalDataContext(DataContextType.CYDYardUnitState)]
	public class CYDYardUnitState : AutoCYDYardUnitState,
		IDocAddresses,
		ICYDYardUnitState,
		IWorkflowProvider,
		IDocumentSupportable,
		IEDocsProvider,
		IStmNoteParent
	{
		public CYDYardUnitState(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("CurrentYard")]
		public override ZGuid YUS_WW_CurrentYard
		{
			get => base.YUS_WW_CurrentYard;
			set => base.YUS_WW_CurrentYard = value;
		}

		public WhsWarehouse CurrentYard
		{
			get => Factory.Load<WhsWarehouse>(YUS_WW_CurrentYard);
		}

		[RelatedBusinessObject("CurrentYardLocation")]
		public override ZGuid YUS_WL_CurrentYardLocation
		{
			get => base.YUS_WL_CurrentYardLocation;
			set => base.YUS_WL_CurrentYardLocation = value;
		}

		public WhsLocation CurrentYardLocation
		{
			get => Factory.Load<WhsLocation>(YUS_WL_CurrentYardLocation);
		}

		[RelatedBusinessObject("ReceiveAdviceLine")]
		public override ZGuid YUS_YRL_ReceiveLine
		{
			get => base.YUS_YRL_ReceiveLine;
			set => base.YUS_YRL_ReceiveLine = value;
		}

		public CYDReceiveAdviceLine ReceiveAdviceLine
		{
			get => Factory.Load<CYDReceiveAdviceLine>(YUS_YRL_ReceiveLine);
		}

		[RelatedBusinessObject("ReleaseAdviceLine")]
		public override ZGuid YUS_YEL_ReleaseLine
		{
			get => base.YUS_YEL_ReleaseLine;
			set => base.YUS_YEL_ReleaseLine = value;
		}

		public CYDReleaseAdviceLine ReleaseAdviceLine
		{
			get => Factory.Load<CYDReleaseAdviceLine>(YUS_YEL_ReleaseLine);
		}

		[RelatedBusinessObject("ReceiveTransportationUnit")]
		public override ZGuid YUS_YTU_ReceiveTransportationUnit
		{
			get => base.YUS_YTU_ReceiveTransportationUnit;
			set => base.YUS_YTU_ReceiveTransportationUnit = value;
		}

		public CYDTransportationUnit ReceiveTransportationUnit
		{
			get => Factory.Load<CYDTransportationUnit>(YUS_YTU_ReceiveTransportationUnit);
		}

		[RelatedBusinessObject("DispatchTransportationUnit")]
		public override ZGuid YUS_YTU_DispatchTransportationUnit
		{
			get => base.YUS_YTU_DispatchTransportationUnit;
			set => base.YUS_YTU_DispatchTransportationUnit = value;
		}

		public CYDTransportationUnit DispatchTransportationUnit
		{
			get => Factory.Load<CYDTransportationUnit>(YUS_YTU_DispatchTransportationUnit);
		}

		[RelatedBusinessObject("Delivery")]
		public override ZGuid YUS_YDL_Delivery
		{
			get => base.YUS_YDL_Delivery;
			set => base.YUS_YDL_Delivery = value;
		}

		public CYDDelivery Delivery
		{
			get => Factory.Load<CYDDelivery>(YUS_YDL_Delivery);
		}

		[RelatedBusinessObject("Pickup")]
		public override ZGuid YUS_YPL_Pickup
		{
			get => base.YUS_YPL_Pickup;
			set => base.YUS_YPL_Pickup = value;
		}

		public CYDPickup Pickup
		{
			get => Factory.Load<CYDPickup>(YUS_YPL_Pickup);
		}

		CYDReceiveAdvice receiveAdvice;

		public CYDReceiveAdvice ReceiveAdvice
		{
			get
			{
				if (receiveAdvice == null && ReceiveAdviceLine != null)
				{
					receiveAdvice = Factory.Load<CYDReceiveAdvice>(ReceiveAdviceLine.YRL_YRA_ReceiveAdvice);
				}

				return receiveAdvice;
			}
		}

		CYDReleaseAdvice releaseAdvice;

		public CYDReleaseAdvice ReleaseAdvice
		{
			get
			{
				if (releaseAdvice == null && ReleaseAdviceLine != null)
				{
					releaseAdvice = Factory.Load<CYDReleaseAdvice>(ReleaseAdviceLine.YEL_YRE_ReleaseAdvice);
				}

				return releaseAdvice;
			}
		}

		OrgAddress clientAddress;

		public OrgAddress ClientAddress
		{
			get
			{
				if (clientAddress == null)
				{
					var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));

					var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_OA_Address);
					jobDocAddressSubQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_ParentID, ReceiveAdvice.PK);
					jobDocAddressSubQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, "BKD");

					orgAddressQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

					clientAddress = Factory.LoadTop1<OrgAddress>(orgAddressQuery);
				}

				return clientAddress;
			}
		}

		RefContainer container;

		public RefContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Factory.Load<RefContainer>(Delivery.UnitLineItem.YLI_RC_ContainerType);
				}
				return container;
			}
		}

		CYDUnitLineItem unitLineItem;

		public CYDUnitLineItem UnitLineItem
		{
			get
			{
				if (unitLineItem == null)
				{
					unitLineItem = Factory.Load<CYDUnitLineItem>(YUS_YLI_UnitLineItem);
				}
				return unitLineItem;
			}
		}

		ZString typeSize;

		public ZString TypeSize
		{
			get
			{
				if (typeSize.IsEmpty)
				{
					if (UnitLineItem != null && UnitLineItem.ContainerType != null)
					{
						typeSize = UnitLineItem.ContainerType.RC_Code;
					}
					else if (YUS_YDL_Delivery != Guid.Empty && Delivery.UnitLineItem != null && Delivery.UnitLineItem.ContainerType != null)
					{
						typeSize = Delivery.UnitLineItem.ContainerType.RC_Code;
					}
					else
					{
						typeSize = ReceiveAdviceLine?.UnitLineItem?.ContainerType?.RC_Code ?? ZString.Empty;
					}
				}
				return typeSize;
			}
		}

		public bool HasBeenRejected
		{
			get
			{
				return Delivery is not null && Delivery.YDL_IsReject;
			}
		}

		public bool HasBeenGatedIn
		{
			get
			{
				return ReceiveTransportationUnit is not null && ReceiveTransportationUnit.YTU_GateInTime.IsValid;
			}
		}

		public bool HasBeenGatedOut
		{
			get
			{
				return HasBeenGatedIn && DispatchTransportationUnit is not null && DispatchTransportationUnit.YTU_GateOutTime.IsValid;
			}
		}

		public int FreeStorageDays
		{
			get
			{
				var findStrategy = new FindFreeStorageDaysStrategy();
				var freeDaysCollection = new CYDYardStorageFreeDaysCollection(ClientAddress.Header)
						.Find((item) => findStrategy.Filter(item, this))
						.OrderByDescending(findStrategy.GetPriority);
				return freeDaysCollection.Any() ? freeDaysCollection.First().YFD_FreeDays : 0;
			}
		}

		public ZString Status
		{
			get
			{
				var linkedMNRSurveysQuery = new ZDBOnlyQuery(typeof(MNRSurvey));
				linkedMNRSurveysQuery.AddToFilter(MNRSurveySchema.MRS_ParentID, PK);
				var linkedMNRSurveys = Factory.Load<MNRSurvey>(linkedMNRSurveysQuery);

				if (linkedMNRSurveys == null || linkedMNRSurveys.Length == 0 || !linkedMNRSurveys.All(s => s.MRS_IsCompleted))
				{
					return Res.GetString("53a3d89c-6c80-4199-8b23-bad94bc21f75", "TBS - To Be Surveyed");
				}
				if ((bool)(Delivery?.UnitLineItem?.YLI_IsDamaged))
				{
					return Res.GetString("c4228abf-28ce-4b29-8d73-45f7f2d22d20", "DMG - Damaged");
				}
				return Res.GetString("93cfcd7d-0515-4af4-9dea-606a71922e77", "AV - Available");
			}
		}

		#endregion

		#region RelatedCollections

		[ChildEditable]
		public CYDYardUnitStorageLinesCollection StorageLines
		{
			get
			{
				if (storageLines == null)
				{
					storageLines = new CYDYardUnitStorageLinesCollection(this);
					RegisterEditableChildObject(storageLines);
				}

				return storageLines;
			}
		}
		CYDYardUnitStorageLinesCollection storageLines;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return YUS_UnitID.IsEmpty ?
					Res.GetString("d825d6e3-d3f3-4d7f-a36f-b4c3f8c16e4c", "Yard Unit") :
					Res.GetString("a480c600-186e-4f41-ade5-583cd5e42d87", "Yard Unit {0}", YUS_UnitID);
			}
		}

		#endregion

		#region IWorkflowProvider Members

		[ChildEditable(true)]
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
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CYDYardUnitStateProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.CYDYardUnitStateWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_WW, YUS_WW_CurrentYard, null);
			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CYDYardUnitStateDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CYDYardUnitState);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEdocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IStmNoteParent

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var notes = base.NoteTypesCore;
				notes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
				notes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);

				return notes;
			}
		}

		#endregion

		#region Implementation

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			StorageLines.DeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}

			base.OnFactorySavingBeforeTransactionCore();
		}

		#endregion

		#region IDocAddress

		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new[]
				{
					DocAddressType.InsuredByDocumentaryAddress,
					DocAddressType.SurveyReportPartyDocumentaryAddress
				};
			}
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return ""; }
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return true;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		#endregion

		#region Insurer

		public JobDocAddress Insurer
		{
			get
			{
				if (insurer == null || insurer.IsDeleted)
				{
					insurer = DocAddresses.FindOrCreateWithRequirement(InsurerAddressRequirement);
				}
				return insurer;
			}
		}

		JobDocAddress insurer;

		JobDocAddressRequirement InsurerAddressRequirement
		{
			get { return insurerAddressRequirement ??= new JobDocAddressRequirement(DocAddressType.InsuredByDocumentaryAddress, ContactType.LocalClient); }
		}

		JobDocAddressRequirement insurerAddressRequirement;

		#endregion

		#region ThirdParty

		public JobDocAddress ThirdParty
		{
			get
			{
				if (thirdParty == null || thirdParty.IsDeleted)
				{
					thirdParty = DocAddresses.FindOrCreateWithRequirement(ThirdPartyAddressRequirement);
				}
				return thirdParty;
			}
		}

		JobDocAddress thirdParty;

		JobDocAddressRequirement ThirdPartyAddressRequirement
		{
			get { return thirdPartyAddressRequirement ??= new JobDocAddressRequirement(DocAddressType.SurveyReportPartyDocumentaryAddress, ContactType.LocalClient); }
		}

		JobDocAddressRequirement thirdPartyAddressRequirement;

		#endregion
	}
}
