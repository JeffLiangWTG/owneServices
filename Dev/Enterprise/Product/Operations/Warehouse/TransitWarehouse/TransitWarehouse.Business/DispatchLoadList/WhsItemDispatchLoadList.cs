using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	/// <summary>
	/// Transit Warehouse is a GLOW development. This class only exists support UXML.
	/// </summary>
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[CodeProperty(WhsItemDispatchLoadList.Schema.WDL_JobID)]
	[DescriptionProperty(WhsItemDispatchLoadList.Schema.WDL_JobID)]
	[UniversalDataContext(DataContextType.TransitDispatchLoadList)]
	public class WhsItemDispatchLoadList : AutoWhsItemDispatchLoadList,
		IWhsItemDispatchLoadList,
		ITransportParentCommon,
		IDocManagerSupport,
		IDocumentSupportable,
		IEDocsProvider,
		IRoutingSupport,
		ITransportParent,
		ITransportChangeNotifier,
		IDocAddresses,
		IJobCostingPlugIn,
		IJobNumber,
		IJobInvoicingPlugIn,
		ITransitJobInvoicingPlugIn,
		IWorkflowProvider,
		IUniversalXMLNoteParent,
		IHaveCusEntryNumReferences
	{
		public WhsItemDispatchLoadList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		#region Delete

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region WDL_IsReadyToStage

		[ResourceStringData("WhsItemDispatchLoadList|WDL_IsReadyToStage", Caption = "Is Ready to Stage")]
		public override ZBool WDL_IsReadyToStage
		{
			get { return base.WDL_IsReadyToStage; }
			set { base.WDL_IsReadyToStage = value; }
		}

		#endregion

		#region Location

		[ResourceStringData("WhsItemDispatchLoadList|WDL_WL_StagingLocation", Caption = "Location")]
		[RelatedBusinessObject("Location")]
		public override ZGuid WDL_WL_StagingLocation
		{
			get { return base.WDL_WL_StagingLocation; }
			set { base.WDL_WL_StagingLocation = value; }
		}

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WDL_WL_StagingLocation); }
		}

		#endregion

		#region WDL_WW_Warehouse

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		[ResourceStringData("WhsItemDispatchLoadList|WDL_WW_Warehouse", Caption = "Warehouse")]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WDL_WW_Warehouse
		{
			get { return base.WDL_WW_Warehouse; }
			set { base.WDL_WW_Warehouse = value; }
		}

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WDL_WW_Warehouse); }
		}

		#endregion

		#region WDL_JobID

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		[ResourceStringData("b96ef959-29f4-4472-9a44-4db0090db478", Caption = "Load List ID")]
		public override ZString WDL_JobID
		{
			get { return base.WDL_JobID; }
			set { base.WDL_JobID = value; }
		}

		#endregion

		#region WDL_ReferenceNumber

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		[ResourceStringData("fd2386a5-6fc5-4fc0-a020-222474446f6a", Caption = "Reference Number")]
		public override ZString WDL_ReferenceNumber
		{
			get { return base.WDL_ReferenceNumber; }
			set { base.WDL_ReferenceNumber = value; }
		}

		#endregion

		#region WDL_SystemCreateTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WDL_SystemCreateTimeUtc
		{
			get { return base.WDL_SystemCreateTimeUtc; }
			set { base.WDL_SystemCreateTimeUtc = value; }
		}

		#endregion

		#region WDL_SystemCreateUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDL_SystemCreateUser
		{
			get { return base.WDL_SystemCreateUser; }
			set { base.WDL_SystemCreateUser = value; }
		}

		#endregion

		#region WDL_SystemLastEditTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WDL_SystemLastEditTimeUtc
		{
			get { return base.WDL_SystemLastEditTimeUtc; }
			set { base.WDL_SystemLastEditTimeUtc = value; }
		}

		#endregion

		#region WDL_SystemLastEditUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDL_SystemLastEditUser
		{
			get { return base.WDL_SystemLastEditUser; }
			set { base.WDL_SystemLastEditUser = value; }
		}

		#endregion

		#region WDL_ParentID

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WDL_ParentID
		{
			get { return base.WDL_ParentID; }
			set { base.WDL_ParentID = value; }
		}

		#endregion

		#region WDL_ParentTableCode

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDL_ParentTableCode
		{
			get { return base.WDL_ParentTableCode; }
			set { base.WDL_ParentTableCode = value; }
		}

		#endregion

		#region DispatchTransportationUnits

		public IReadOnlyCollection<WhsItemDispatchTransportationUnit> DispatchTransportationUnits
		{
			get
			{
				if (dispatchTransportationUnits == null)
				{
					var pivotSubQuery = new ZDBOnlySubQuery(typeof(AutoWhsItemDispatchLoadListDTUPivot), WhsItemDispatchTransportationUnitSchema.PK, WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit);
					pivotSubQuery.AddToFilter(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList, PK);

					var dtuQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
					dtuQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

					dispatchTransportationUnits = Factory.Load<WhsItemDispatchTransportationUnit>(dtuQuery);
				}
				return dispatchTransportationUnits;
			}
		}

		IReadOnlyCollection<WhsItemDispatchTransportationUnit> dispatchTransportationUnits;

		#endregion

		#region LoadListType

		public ZString LoadListType => DispatchTransportationUnits.Count == 0
			? TransportUnitTypes.None
			: DispatchTransportationUnits.All(t => t.WDH_UnitType == TransportUnitTypes.ULD)
				? TransportUnitTypes.ULD
				: DispatchTransportationUnits.All(t => t.WDH_UnitType == TransportUnitTypes.Container)
					? TransportUnitTypes.Container
					: DispatchTransportationUnits.All(t => t.WDH_UnitType == TransportUnitTypes.Vehicle)
						? TransportUnitTypes.Vehicle
						: TransportUnitTypes.Mix;

		#endregion

		#region PackageStates

		public WhsItemPackageStateCollection PackageStates
		{
			get
			{
				return packageStates ?? (packageStates = new WhsItemPackageStateCollection(this));
			}
		}

		WhsItemPackageStateCollection packageStates;

		#endregion

		#region Additional References

		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					additionalReferenceNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					additionalReferenceNumbers.Load();
				}

				return additionalReferenceNumbers;
			}
		}

		CusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		public void RefreshAdditionalReferenceNumbers()
		{
			additionalReferenceNumbers = null;
		}

		#endregion

		#region CusEntryNumReferences

		public ICusEntryNumReferenceCollection CusEntryNumReferences
		{
			get
			{
				if (cusEntryNumReferences == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumReferenceCollectionProvider>();
					cusEntryNumReferences = provider.GetCollection(this);
				}

				return cusEntryNumReferences;
			}
		}

		ICusEntryNumReferenceCollection cusEntryNumReferences;

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IsFinalised

		public ZBool IsFinalised
		{
			get => PackageStates.All(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Finalized);
			set
			{
				if (value)
				{
					var dateTimeOffsetNow = TransitWarehouseHelper.GetNowInCurrentWarehouse(Warehouse);
					var gateOutDTUs = DispatchTransportationUnits.Where(d => !d.WDH_GateOutTime.IsEmpty && d.WDH_FinalisedTime.IsEmpty);
					foreach (var dtu in gateOutDTUs)
					{
						dtu.Finalise(dateTimeOffsetNow, (NoResString)"Finalised (e.g. via workflow)"); // Log Reference.
					}
				}
			}
		}

		#endregion

		#region FormarttedReference

		public ZString FormattedReference => TransitWarehouseHelper.GetFormattedReferenceString(MasterBillNumber, WDL_JobID);

		#endregion

		#region Creditor

		public JobDocAddress Creditor
		{
			get
			{
				if (creditor == null || creditor.IsDeleted)
				{
					creditor = DocAddresses.FindOrCreateWithRequirement(CreditorDocAddressRequirement);
				}
				return creditor;
			}
		}
		JobDocAddress creditor;

		JobDocAddressRequirement CreditorDocAddressRequirement
		{
			get
			{
				return creditorDocAddressRequirement ??
				(creditorDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Creditor, ContactType.LocalClient));
			}
		}
		JobDocAddressRequirement creditorDocAddressRequirement;

		#endregion

		#region ITransportParentCommon

		public ZString TypeCode => Constants.TransportParentTypes.TransitDispatchLoadList;

		#endregion

		#region IDocAddresses Members

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

		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				JobDocAddressDependentCollection docAddresses = null;
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
				}

				return docAddresses;
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return ""; }
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new[] { DocAddressType.Creditor }; }
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.TransitDispatchLoadList);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WDL_JobID.IsEmpty ?
					Res.GetString("37d3998b-782f-4932-878e-41e3d211679e", "Dispatch Load List") :
					Res.GetString("6e318bf0-117e-4870-8c89-f5183fb4b29d", "Dispatch Load List {0}", WDL_JobID);
			}
		}

		#endregion

		#region TransportCollection

		public TransportCollection Transports
		{
			get
			{
				if (transports == null || transports.Any(t => t.IsDeleted))
				{
					transports = new TransportCollection(this);
					transports.Load();
				}
				return transports;
			}
		}
		TransportCollection transports;

		#endregion

		#region DocumentSupporter

		DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ?? (documentSupporter = new WhsItemDispatchLoadListDocumentSupporter(this));
		DocumentSupporter documentSupporter;

		#endregion

		#region IRoutingSupport Members

		RoutingCollection IRoutingSupport.TransportsIncludingRelated => transportsIncludingRelated ?? (transportsIncludingRelated = new RoutingCollection(this));
		RoutingCollection transportsIncludingRelated;

		TransportCollection IRoutingSupport.Transports => Transports;

		ZString IRoutingSupport.TransportMode => WDL_TransportMode;

		string IRoutingSupport.AdditionalETAUpdateMsg => string.Empty;

		string IRoutingSupport.AdditionalETDUpdateMsg => string.Empty;

		#endregion

		#region ITransportParent Members

		public Directions JobDirection => Directions.Unknown;

		TransportSupporter ITransportParent.TransportSupporter => new WhsItemDispatchLoadListTransportSupporter(this);

		TransportCollection ITransportParent.Transports => Transports;

		#endregion

		#region ITransportChangeNotifier Members

		public void NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out var handler))
			{
				handler?.Invoke(transport, previousValue);
			}
		}

		Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler> TransportChangeNotifierDictionary
		{
			get { return transportChangeNotifierDictionary ?? (transportChangeNotifierDictionary = new Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler>()); }
		}

		Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler> transportChangeNotifierDictionary;

		public void AddTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier)
		{
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out var handler))
			{
				handler -= notifier;
				handler += notifier;
			}
			else
			{
				TransportChangeNotifierDictionary.Add(notifyType, notifier);
			}
		}

		public void RemoveTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier)
		{
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out var handler))
			{
				handler -= notifier;
			}
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.TransitDispatchLoadList; }
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

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsItemDispatchLoadListProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_WW, WDL_WW_Warehouse, ZGuid.Empty);
			return result;
		}

		#endregion

		#region JobNumber

		string IJobNumber.JobNumber => WDL_JobID;

		#endregion

		#region MasterBillNumber

		public ZString MasterBillNumber
		{
			get
			{
				var masterBillAdditionalReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.MasterBill);
				return masterBillAdditionalReference != null && masterBillAdditionalReference.CE_EntryNum != ""
					? masterBillAdditionalReference.CE_EntryNum
					: ZString.Empty;
			}
		}

		#endregion

		#region CarrierBookingReference

		public ZString CarrierBookingReference
		{
			get
			{
				if (carrierBookingReference.IsEmpty)
				{
					var reference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference);
					carrierBookingReference = reference != null ? reference.CE_EntryNum : ZString.Empty;
				}
				return carrierBookingReference;
			}
		}
		ZString carrierBookingReference;

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;
				types.Add(TransitWarehouseNoteHelper.GetNoteTypes());
				types.Add(PredefinedNoteTypes.Instance.DispatchInstructionWarning);
				return types;
			}
		}

		#endregion

		#region IJobCostingPlugIn Members

		public ZString JK_UniqueConsignRef => WDL_ReferenceNumber;

		public RefUNLOCO LoadPort => null;

		public RefUNLOCO DischargePort => null;

		public JobProfitLossCollection ProfitLossContainer => null;

		public decimal ConsolExchangeRate => 0m;

		public RefCurrency ConsolCurrency => null;

		public bool IsMasterCollect => false;

		public OrgHeader ReceivingAgent => null;

		public OrgHeader ReceivingAgentAPInvoicingParty => null;

		public OrgHeader ReceivingAgentARInvoicingParty => null;

		public OrgHeader SendingAgent => null;

		public OrgHeader SendingAgentAPInvoicingParty => null;

		public OrgHeader SendingAgentARInvoicingParty => null;

		public ZString TransportMode => PackageStates.FirstOrDefault(p => !string.IsNullOrEmpty(p.DispatchConsignment?.WDC_TransportMode ?? ""))?.DispatchConsignment?.WDC_TransportMode ?? "";

		public ZString ContainerMode => ZString.Empty;

		public ZString Direction => ZString.Empty;

		public ZString ConsolType => ZString.Empty;

		public ZString Module => ApportionmentMethodModules.TransitWarehouse;

		public CodeDescriptionPairList PrepaidCollectList => null;

		public IGenericJobCostSupporter CostSupporter => costSupporter ?? (costSupporter = new WhsItemDispatchLoadListCostSupporter(this));

		IGenericJobCostSupporter costSupporter;

		public decimal ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK) => 0m;

		public void AddNewToLogs(Event @event, ZString reference) => Logs.AddNew(@event, reference);

		public ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob) => ZString.Empty;

		#endregion

		#region InvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn => invoicingPlugIn ?? (invoicingPlugIn = new TransitJobInvoicingPlugIn<WhsItemDispatchLoadList>(this));
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => InvoicingPlugIn.InvoicingSupporter;

		#endregion

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion => true;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			if (WDL_ReferenceNumber.IsEmpty)
			{
				WDL_ReferenceNumber = (ZString)Env.NumberFountains.TransitWarehouseDispatchID.GetNextFormatted(Factory);
			}
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		#endregion

		#region ITransitJobInvoicingPlugIn

		JobInvoicingConsumerType ITransitJobInvoicingPlugIn.ConsumerType => JobInvoicingConsumerTypes.TransitDispatchLoadList;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.AuditSecurity => Env.Security.WhsItemDispatchLoadListAuditBilling;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.JobInvoicingSecurity => Env.Security.WhsItemDispatchLoadListJobInvoicing;

		IJobInvoicingSupporter ITransitJobInvoicingPlugIn.InvoicingSupporter => new TransitJobInvoicingSupporter<WhsItemDispatchLoadList>(this);

		#endregion

		#region IEdocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		#endregion

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var whs = (BusinessObject)Factory.NewWithValidTestData<WhsWarehouse>();
			WDL_WW_Warehouse = whs.PK;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion
	}
}
