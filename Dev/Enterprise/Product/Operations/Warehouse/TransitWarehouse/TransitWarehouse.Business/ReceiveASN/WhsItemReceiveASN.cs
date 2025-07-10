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
using Enterprise.BufferManagement.Integration;
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
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	/// <summary>
	/// Transit Warehouse is a GLOW development. This class only exists to support UXML, Workflow and Documents.
	/// </summary>
	[UniversalDataContext(DataContextType.TransitReceiveASN)]
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[CodeProperty(WhsItemReceiveASN.Schema.WRP_ReferenceNumber)]
	[DescriptionProperty(WhsItemReceiveASN.Schema.WRP_VehicleReference)]
	public class WhsItemReceiveASN : AutoWhsItemReceiveASN,
		IDocumentSupportable,
		IDocManagerSupport,
		IDocAddresses,
		ITransportParentCommon,
		IWorkflowProvider,
		IUniversalXMLNoteParent,
		IJobNumber,
		IRoutingSupport,
		ITransportParent,
		ITransportChangeNotifier,
		IHaveCusEntryNumReferences,
		IWhsItemReceiveASN
	{
		public WhsItemReceiveASN(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region WRP_VehicleReference

		[ResourceStringData("51c799b7-56d1-462e-b076-0b32ea05f112", Caption = "Vehicle Reference")]
		public override ZString WRP_VehicleReference
		{
			get { return base.WRP_VehicleReference; }
			set { base.WRP_VehicleReference = value; }
		}

		#endregion

		#region WRP_ReferenceNumber

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		[ResourceStringData("1ed01b7e-b775-4376-9181-15cd61a07cfe", Caption = "Reference Number")]
		public override ZString WRP_ReferenceNumber
		{
			get { return base.WRP_ReferenceNumber; }
			set { base.WRP_ReferenceNumber = value; }
		}

		#endregion

		#region WRP_ETA

		[ResourceStringData("d3e73cd1-da85-4a2a-9a1a-ea5b2a4e7ae7", Caption = "ETA")]
		public override ZDateTime WRP_ETA
		{
			get { return base.WRP_ETA; }
			set { base.WRP_ETA = value; }
		}

		#endregion

		#region WRP_WW_IntendedWarehouse

		[RelatedBusinessObject("IntendedWarehouse")]
		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WRP_WW_IntendedWarehouse
		{
			get { return base.WRP_WW_IntendedWarehouse; }
			set { base.WRP_WW_IntendedWarehouse = value; }
		}

		public WhsWarehouse IntendedWarehouse => Factory.Load<WhsWarehouse>(WRP_WW_IntendedWarehouse);

		#endregion

		#region WRP_SystemCreateTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WRP_SystemCreateTimeUtc
		{
			get { return base.WRP_SystemCreateTimeUtc; }
			set { base.WRP_SystemCreateTimeUtc = value; }
		}

		#endregion

		#region WRP_SystemCreateUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRP_SystemCreateUser
		{
			get { return base.WRP_SystemCreateUser; }
			set { base.WRP_SystemCreateUser = value; }
		}

		#endregion

		#region WRP_SystemLastEditTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WRP_SystemLastEditTimeUtc
		{
			get { return base.WRP_SystemLastEditTimeUtc; }
			set { base.WRP_SystemLastEditTimeUtc = value; }
		}

		#endregion

		#region WRP_SystemLastEditUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRP_SystemLastEditUser
		{
			get { return base.WRP_SystemLastEditUser; }
			set { base.WRP_SystemLastEditUser = value; }
		}

		#endregion

		#region WRP_ParentID

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WRP_ParentID
		{
			get { return base.WRP_ParentID; }
			set { base.WRP_ParentID = value; }
		}

		#endregion

		#region WRP_ParentTableCode

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRP_ParentTableCode
		{
			get { return base.WRP_ParentTableCode; }
			set { base.WRP_ParentTableCode = value; }
		}

		#endregion

		#region WRP_CompleteTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WRP_CompleteTime
		{
			get { return base.WRP_CompleteTime; }
			set { base.WRP_CompleteTime = value; }
		}

		#endregion

		#region WRP_K0_ExpectedContainer

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WRP_K0_ExpectedContainer
		{
			get { return base.WRP_K0_ExpectedContainer; }
			set { base.WRP_K0_ExpectedContainer = value; }
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WRP_ReferenceNumber.IsEmpty ?
					Res.GetString("51CAE6BA-EC6B-43EE-9082-E2B105F840E0", "Receive ASN") :
					Res.GetString("FDB9CFF1-41A7-4F79-BE5E-93E77454322D", "Receive ASN {0}", WRP_ReferenceNumber);
			}
		}

		#endregion

		#region Additional References

		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumAdditionalReferenceCollectionProvider>();
					additionalReferenceNumbers = provider.GetCollection(this);

					var additionalReferenceNumbersBusinessObjectCollection = additionalReferenceNumbers as BusinessObjectCollection;
					if (additionalReferenceNumbersBusinessObjectCollection != null)
					{
						additionalReferenceNumbersBusinessObjectCollection.Load();
					}
				}

				return additionalReferenceNumbers;
			}
		}

		ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

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

		#endregion

		#region Delete

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Related Objects

		#region BookingPartyDocAddress

		public JobDocAddress BookingPartyDocAddress
		{
			get
			{
				if (bookingPartyDocAddress == null || bookingPartyDocAddress.IsDeleted)
				{
					bookingPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(BookingPartyDocAddressRequirement);
				}
				return bookingPartyDocAddress;
			}
		}
		JobDocAddress bookingPartyDocAddress;

		JobDocAddressRequirement BookingPartyDocAddressRequirement
		{
			get { return bookingPartyDocAddressRequirement ?? (bookingPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress, ContactType.LocalClient)); }
		}

		JobDocAddressRequirement bookingPartyDocAddressRequirement;

		#endregion

		#region PackageStates

		public WhsItemPackageStateCollection PackageStates
		{
			get { return packageStates ?? (packageStates = new WhsItemPackageStateCollection(this)); }
		}

		WhsItemPackageStateCollection packageStates;

		#endregion

		public IEnumerable<WhsItemReceiveConsignment> ReceiveConsignments
		{
			get { return PackageStates.Select(p => p.ReceiveConsignment).Distinct(); }
		}

		public IEnumerable<WhsItemReceiveTransportationUnit> ReceiveTransportationUnits
		{
			get
			{
				var result = new List<WhsItemReceiveTransportationUnit>();
				var rtus = PackageStates.Where(p => p.WPS_WRH_TransitReceiveHeader.IsValid).Select(p => p.ReceiveTransportationUnit).Distinct();
				result.AddRange(rtus);
				result.AddRange(PlannedReceiveTransportationUnits);
				return result.Distinct().ToArray();
			}
		}

		public IEnumerable<WhsItemReceiveTransportationUnit> PlannedReceiveTransportationUnits
		{
			get
			{
				var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, PK));
				var rtusFromPivots = pivots.Select(p => p.ReceiveTransportationUnit).Distinct();
				return rtusFromPivots;
			}
		}

		#region ASNType

		public ZString ASNType => !ReceiveTransportationUnits.Any()
			? TransportUnitTypes.None
			: ReceiveTransportationUnits.All(t => t.WRH_UnitType == TransportUnitTypes.ULD)
				? TransportUnitTypes.ULD
				: ReceiveTransportationUnits.All(t => t.WRH_UnitType == TransportUnitTypes.Container)
					? TransportUnitTypes.Container
					: ReceiveTransportationUnits.All(t => t.WRH_UnitType == TransportUnitTypes.Vehicle)
						? TransportUnitTypes.Vehicle
						: TransportUnitTypes.Mix;

		#endregion

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsItemReceiveASNDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

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
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
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
			get { return new[] { DocAddressType.TransportCompanyDocumentaryAddress }; }
		}

		#endregion

		#region IJobNumber Memebers

		public string JobNumber => WRP_ReferenceNumber;

		#endregion

		#region Save

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			PopulateUniqueIDIfNeeded();
		}

		void PopulateUniqueIDIfNeeded()
		{
			if (!IsInDatabase && !IsDeleted && WRP_ReferenceNumber.IsEmpty)
			{
				WRP_ReferenceNumber = (ZString)Env.NumberFountains.TransitWarehouseReceiveExpectedPackingID.GetNextFormatted(Factory);
			}
		}

		#endregion

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				OnSaveFailed();
			}
		}

		void OnSaveFailed()
		{
			if (!IsInDatabase)
			{
				WRP_ReferenceNumber = ZString.Empty;
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
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.TransitReceiveASN);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.TransitReceiveASN; }
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsItemReceiveASNProcessTaskCollection(this));
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
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, BookingPartyDocAddress.OrganisationPK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_WW, WRP_WW_IntendedWarehouse, ZGuid.Empty);
			return result;
		}

		#endregion

		#region ITransportParentCommon

		public ZString TypeCode => Constants.TransportParentTypes.TransitReceiveASN;

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

		#region VesselLloydsNumber

		public ZString VesselLloydsNumber
		{
			get
			{
				if (vesselLloydsNumber.IsEmpty)
				{
					var reference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
					vesselLloydsNumber = reference != null ? reference.CE_EntryNum : ZString.Empty;
				}
				return vesselLloydsNumber;
			}
		}
		ZString vesselLloydsNumber;

		#endregion

		#region VoyageNumber

		public ZString VoyageNumber
		{
			get
			{
				if (voyageNumber.IsEmpty)
				{
					var reference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
					voyageNumber = reference != null ? reference.CE_EntryNum : ZString.Empty;
				}
				return voyageNumber;
			}
		}
		ZString voyageNumber;

		#endregion

		#region VesselName

		public ZString VesselName
		{
			get
			{
				if (vesselName.IsEmpty)
				{
					var reference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.Vessel);
					vesselName = reference != null ? reference.CE_EntryNum : ZString.Empty;
				}
				return vesselName;
			}
		}
		ZString vesselName;

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

		#region TransportCompany

		public JobDocAddress TransportCompany
		{
			get
			{
				if (transportCompany == null || transportCompany.IsDeleted)
				{
					transportCompany = DocAddresses.FindOrCreateWithRequirement(TransportCompanyDocAddressDocAddressRequirement);
				}
				return transportCompany;
			}
		}
		JobDocAddress transportCompany;

		JobDocAddressRequirement TransportCompanyDocAddressDocAddressRequirement
		{
			get { return transportCompanyDocAddressDocAddressRequirement ?? (transportCompanyDocAddressDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress, ContactType.LocalTransport)); }
		}

		JobDocAddressRequirement transportCompanyDocAddressDocAddressRequirement;

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

		#region FormarttedReference

		public ZString FormattedReference => TransitWarehouseHelper.GetFormattedReferenceString(MasterBillNumber, WRP_ReferenceNumber);

		#endregion

		#region IRoutingSupport Members

		RoutingCollection IRoutingSupport.TransportsIncludingRelated => transportsIncludingRelated ?? (transportsIncludingRelated = new RoutingCollection(this));
		RoutingCollection transportsIncludingRelated;

		TransportCollection IRoutingSupport.Transports => Transports;

		ZString IRoutingSupport.TransportMode => WRP_TransportMode;

		string IRoutingSupport.AdditionalETAUpdateMsg => string.Empty;

		string IRoutingSupport.AdditionalETDUpdateMsg => string.Empty;

		#endregion

		#region ITransportParent Members

		public Directions JobDirection => Directions.Unknown;

		TransportSupporter ITransportParent.TransportSupporter => new WhsItemReceiveASNTransportSupporter(this);

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

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;
				types.Add(TransitWarehouseNoteHelper.GetNoteTypes());
				return types;
			}
		}

		#endregion

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var warehouse = (BusinessObject)Factory.NewWithValidTestData<WhsWarehouse>();
			WRP_WW_IntendedWarehouse = warehouse.PK;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion
	}
}
