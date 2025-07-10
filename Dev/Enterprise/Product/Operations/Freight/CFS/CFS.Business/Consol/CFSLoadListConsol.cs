using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business.Rating;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	[UniversalDataContext(DataContextType.CFSLoadListConsol)]
	public class CFSLoadListConsol : CommonConsol,
		Integration.CFS.ICFSLoadListConsol,
		IJobInvoicingPlugIn,
		IHaveInternalCartage,
		ITransportParent,
		ICartageParent,
		IWorkflowProvider,
		ISendEmailSource,
		IRatingSupporter
	{
		#region Schema

		public new class Schema : CommonConsol.Schema
		{
			public const string JK_OH_Forwarder = "JK_OH_Forwarder";
			public const string JK_OA_CTOAddress = "JK_OA_CTOAddress";
			public const string JK_OA_EmptyContainerYard = "JK_OA_EmptyContainerYard";
			public const string JK_OA_CartageCoAddress = "JK_OA_CartageCoAddress";
			public const string JK_OA_DepotAddress = "JK_OA_DepotAddress";

			public const string CartageCoPK = "CartageCoPK";
			public const string DepotPK = "DepotPK";

			public const string CanadaCCNNumber = "CanadaCCNNumber";
			public const string CanadaPCNNumber = "CanadaPCNNumber";
		}

		#endregion

		public CFSLoadListConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			factory.SetFreightDomainContext(FreightDomainContext.CFS);
			AutomaticallyUpdatePackLineContainers = false;
		}

		#region Business Object Overrides

		#region Validation

		protected override JobConsolValidation GetNewValidation()
		{
			return new CFSLoadListConsolValidation(this);
		}

		public new CFSLoadListConsolValidation Validation
		{
			get { return (CFSLoadListConsolValidation)base.Validation; }
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			fSettingDefaultValues = true;
			try
			{
				JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				JK_TransportMode = Core.Constants.TransportModes.Sea;
				JK_IsCFS = true;
				JK_IsForwarding = false;
			}
			finally
			{
				fSettingDefaultValues = false;
			}
		}

		#endregion

		#region RunPreSaveValidation

		protected override void RunPreSaveValidationCore()
		{
			Shipments.SetValuesFromParent();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase || JK_MasterBillNumInfo.HasChanges)
			{
				UpdateTransportFlightSubscriptionEvent();
			}
		}

		void UpdateTransportFlightSubscriptionEvent()
		{
			foreach (var transport in Transports.OfType<Transport>().Where(t => t.IsAir))
			{
				FlightMonitoringSystemManager.UpdateFlightSubscriptionEvent(transport);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			Factory.RemoveContext(BusinessContext.EnableJobHeaderNumberChange);
		}

		#endregion

		#region OnFactorySaving

		protected override void OnFactorySaving()
		{
			foreach (CFSContainer container in Containers)
			{
				if (!container.IsDeleted)
				{
					container.JC_OH_ShippingLine = ShippingLinePK;

					if (container.JC_OH_CFSClient.IsEmpty)
					{
						container.JC_OH_CFSClient = JK_OH_Forwarder;
					}
				}
			}

			base.OnFactorySaving();
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (!JK_IsForwarding)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		#endregion

		#region DeleteContainersWhenConsolIsDeleted

		protected override void DeleteContainersWhenConsolIsDeleted()
		{
			foreach (CFSContainer container in Containers)
			{
				container.JC_JK = ZGuid.Empty;
			}
			// Do nothing; we do not delete containers from consols in CFS.
		}

		#endregion

		#region CheckAllShipmentsForDuplicateLoadingAndDischarge

		protected override void CheckAllShipmentsForDuplicateLoadingAndDischarge()
		{
		}

		#endregion

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("0043406d-4121-4e84-9bfe-a55f4a1c9171", "Load List"); }
		}

		#endregion

		#region BusinessObjectsWithRelatedEvents

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				result.AddRange(Factory.Load<CommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, PK)));

				return result.ToArray();
			}
		}

		#endregion

		protected override bool DeferFiringWorkflowCore
		{
			get { return JK_IsForwarding; }
		}

		#endregion

		#region Related Business Objects

		#region Containers

		public new CFSContainerCollection Containers
		{
			get
			{
				CFSContainerCollection result = (CFSContainerCollection)base.Containers;
				result.UpdatePackLineContainerOnAdd = AutomaticallyUpdatePackLineContainers;
				return result;
			}
		}

		public event EventHandler<CancelEventArgs> OnRemovingContainerWithPackLines
		{
			add { this.Containers.OnRemovingContainerWithPackLines += value; }
			remove { this.Containers.OnRemovingContainerWithPackLines -= value; }
		}

		protected override CommonContainerCollection GetNewContainerCollection()
		{
			CFSContainerCollection result = new CFSContainerCollection(this, Factory);
			result.CountChanged += new CollectionCountChangedEventHandler(Containers_CountChanged);
			return result;
		}

		void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CFSContainer container = e.BizObject as CFSContainer;
			if (container != null)
			{
				if (e.ItemRemoved)
				{
					for (int i = container.PackLines.Count - 1; i >= 0; i--)
					{
						PackLine packLine = container.PackLines[0];
						if (Shipments.Contains(packLine.JL_JS))
						{
							container.PackLines.Remove(packLine);
						}
					}
				}
				else if (e.ItemAdded)
				{
				}
			}
		}

		#endregion

		#region Shipments

		[ChildEditable(true)]
		public new CFSShipmentCollection Shipments
		{
			get { return (CFSShipmentCollection)base.Shipments; }
		}

		protected override ConsolShipmentCollection GetNewConsolShipmentCollection()
		{
			return new CFSShipmentCollection(this);
		}

		protected override void AfterConsolShipmentCollectionCreated()
		{
			base.AfterConsolShipmentCollectionCreated();
			if (this.IsExport())
			{
				Shipments.Sort(CFSShipment.Schema.JS_InterimReceipt, ListSortDirection.Ascending);
			}
			else
			{
				Shipments.Sort(CFSShipment.Schema.JS_UniqueConsignRef, ListSortDirection.Ascending);
			}
		}

		#endregion

		#region UnallocatedPackLines

		protected override UnAllocatedPackLinesView CreateUnAllocatedPackLines()
		{
			return new CFSUnallocatedPackLinesView(this, RelatedPackLines);
		}

		#endregion

		#endregion

		#region Property overrides

		#region JK_OA_ContainerYardEmptyPickupAddress

		public override ZGuid JK_OA_ContainerYardEmptyPickupAddress
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JK_OA_ContainerYardEmptyPickupAddress; }
			set
			{
				base.JK_OA_ContainerYardEmptyPickupAddress = value;
				Containers.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region JK_OA_ContainerYardEmptyReturnAddress

		public override ZGuid JK_OA_ContainerYardEmptyReturnAddress
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JK_OA_ContainerYardEmptyReturnAddress; }
			set
			{
				base.JK_OA_ContainerYardEmptyReturnAddress = value;
				Containers.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region JK_IsForwarding

		public override ZBool JK_IsForwarding
		{
			get { return base.JK_IsForwarding; }
			set
			{
				if (value != JK_IsForwarding)
				{
					base.JK_IsForwarding = value;

					if (value)
					{
						((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
					}

					if (!value && !IsInDatabase)
					{
						DeleteAllUnsavedForwardingProcessTasks();
					}
				}
			}
		}

		void DeleteAllUnsavedForwardingProcessTasks()
		{
			if (!JK_IsForwarding)
			{
				var query = new ZQuery(ProcessTasksSchema.P9_ParentID, this.PK)
				{
					FetchOnlyFromLocalCache = true
				};

				var unsavedForwardingTasks = Factory.Load<Enterprise.Integration.Forwarding.IForwardingConsolProcessTask>(query);
				foreach (var unsavedForwardingTask in unsavedForwardingTasks.Cast<BusinessObject>().Where(task => !task.IsInDatabase))
				{
					unsavedForwardingTask.Delete();
				}
			}
		}

		#endregion

		#region JK_TransportMode

		[List("JK_TransportMode_List")]
		public override ZString JK_TransportMode
		{
			get { return base.JK_TransportMode; }
			set
			{
				base.JK_TransportMode = value;
				Containers.SetTransportModeFromLoadList();
			}
		}

		#endregion

		#region JK_TotalShipmentActVolumeCheck

		public override ZDecimal JK_TotalShipmentActVolumeCheck
		{
			get { return base.JK_TotalShipmentActVolumeCheck; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_TotalShipmentActVolumeCheck, JK_TotalShipmentActVolumeCheckInfo, value);

				if (base.JK_TotalShipmentActVolumeCheck != roundedValue)
				{
					base.JK_TotalShipmentActVolumeCheck = roundedValue;
				}
			}
		}

		#endregion

		#region JK_TotalShipmentActWeightCheck

		public override ZDecimal JK_TotalShipmentActWeightCheck
		{
			get { return base.JK_TotalShipmentActWeightCheck; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_TotalShipmentActWeightCheck, JK_TotalShipmentActWeightCheckInfo, value);

				if (base.JK_TotalShipmentActWeightCheck != roundedValue)
				{
					base.JK_TotalShipmentActWeightCheck = roundedValue;
				}
			}
		}

		#endregion

		#region JK_TotalShipmentActWeightCheck

		public override ZDecimal JK_TotalShipmentChargableCheck
		{
			get { return base.JK_TotalShipmentChargableCheck; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_TotalShipmentChargableCheck, JK_TotalShipmentChargableCheckInfo, value);

				if (base.JK_TotalShipmentChargableCheck != roundedValue)
				{
					base.JK_TotalShipmentChargableCheck = roundedValue;
				}
			}
		}

		#endregion

		#region JK_TotalShipmentActOtherUnit

		public override ZString JK_TotalShipmentActOtherUnit
		{
			get { return base.JK_TotalShipmentActOtherUnit; }
			set
			{
				base.JK_TotalShipmentActOtherUnit = value;

				if (IsAir)
				{
					this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActVolumeCheck, JK_TotalShipmentActVolumeCheckInfo);
				}
				else
				{
					this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActWeightCheck, JK_TotalShipmentActWeightCheckInfo);
				}
			}
		}

		#endregion

		#region JK_TotalShipmentChargeableUnit

		public override ZString JK_TotalShipmentChargeableUnit
		{
			get { return base.JK_TotalShipmentChargeableUnit; }
			set
			{
				base.JK_TotalShipmentChargeableUnit = value;
				this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentChargableCheck, JK_TotalShipmentChargableCheckInfo);

				if (IsAir)
				{
					this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActWeightCheck, JK_TotalShipmentActWeightCheckInfo);
				}
				else
				{
					this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActVolumeCheck, JK_TotalShipmentActVolumeCheckInfo);
				}
			}
		}

		#endregion

		[List("JK_ConsolMode_List")]
		public override ZString JK_ConsolMode
		{
			get { return base.JK_ConsolMode; }
			set { base.JK_ConsolMode = value; }
		}

		#region JK_AgentReference

		public override ZString JK_AgentsReference
		{
			get
			{
				return base.JK_AgentsReference;
			}
			set
			{
				base.JK_AgentsReference = value;
				if (!JK_AgentsReference.IsEmpty)
				{
					foreach (CommonShipment shipment in Shipments)
					{
						if (shipment.JS_ConsolReference.IsEmpty)
						{
							shipment.JS_ConsolReference = value.SubstringSafe(0, JobShipmentSchema.JS_ConsolReference.MaxLength);
						}
					}
				}
			}
		}

		#endregion

		#region JK_OA_SendingForwarderAddress

		public override ZGuid JK_OA_SendingForwarderAddress
		{
			get { return base.JK_OA_SendingForwarderAddress; }
			set
			{
				if (JK_OA_SendingForwarderAddress != value && (IsDefaultedFromForwarder || IsImportingData))
				{
					base.JK_OA_SendingForwarderAddress = value;
				}
				else
				{
					JK_OA_SendingForwarderAddressInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region JK_OA_ReceivingForwarderAddress

		public override ZGuid JK_OA_ReceivingForwarderAddress
		{
			get { return base.JK_OA_ReceivingForwarderAddress; }
			set
			{
				if (JK_OA_ReceivingForwarderAddress != value && (IsDefaultedFromForwarder || IsImportingData))
				{
					base.JK_OA_ReceivingForwarderAddress = value;
				}
				else
				{
					JK_OA_ReceivingForwarderAddressInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region JK_RL_NKLoadPort

		public override ZString JK_RL_NKLoadPort
		{
			get { return base.JK_RL_NKLoadPort; }
			set
			{
				bool wasPackLoadList = IsPackLoadList;

				base.JK_RL_NKLoadPort = value;

				if (IsPackLoadList != wasPackLoadList)
				{
					IsDefaultedFromPorts = true;
					try
					{
						JK_OH_Forwarder = wasPackLoadList ? SendingForwarderPK : ReceivingForwarderPK;
					}
					finally
					{
						IsDefaultedFromPorts = false;
					}

					SetupBasedOnImportExport();
				}
			}
		}

		#endregion

		#region JK_RL_NKDischargePort

		public override ZString JK_RL_NKDischargePort
		{
			get
			{
				return base.JK_RL_NKDischargePort;
			}
			set
			{
				bool wasPackLoadList = IsPackLoadList;

				base.JK_RL_NKDischargePort = value;

				if (IsPackLoadList != wasPackLoadList)
				{
					SetupBasedOnImportExport();
				}
			}
		}

		#endregion

		bool IHaveInternalCartage.IsForPickupCartage
		{
			get { return IsPackLoadList; }
		}

		#endregion

		#region New Bound Properties

		#region New Properties and Methods

		#region MessageCaption

		ZString fMessageCaption;
		public ZString MessageCaption
		{
			get { return fMessageCaption; }
			set { fMessageCaption = value; }
		}

		#endregion

		#region WarningMessage

		ZString fWarningMessage;
		[MaxLength(300)]
		public ZString WarningMessage
		{
			get { return fWarningMessage; }
			set
			{
				CheckMaximumLength(WarningMessageInfo, value);
				fWarningMessage = value;
				WarningMessageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WarningMessageInfo
		{
			get { return GetZPropertyInfo(nameof(WarningMessage)); }
		}

		#endregion

		#region ErrorMessage

		ZString fErrorMessage;
		[MaxLength(250)]
		public ZString ErrorMessage
		{
			get { return fErrorMessage; }
			set
			{
				CheckMaximumLength(ErrorMessageInfo, value);
				fErrorMessage = value;
				ErrorMessageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ErrorMessageInfo
		{
			get { return GetZPropertyInfo(nameof(ErrorMessage)); }
		}

		#endregion

		#region ForwarderIsGoingToChange

		ZBool fForwarderIsGoingToChange;
		public ZBool ForwarderIsGoingToChange
		{
			get { return fForwarderIsGoingToChange; }
			set
			{
				fForwarderIsGoingToChange = value;
				ForwarderIsGoingToChangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ForwarderIsGoingToChangeInfo
		{
			get { return GetZPropertyInfo(nameof(ForwarderIsGoingToChange)); }
		}

		#endregion

		#region SailingIsGoingToChange

		ZBool fSailingIsGoingToChange;
		public ZBool SailingIsGoingToChange
		{
			get { return fSailingIsGoingToChange; }
			set
			{
				fSailingIsGoingToChange = value;
				SailingIsGoingToChangeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SailingIsGoingToChangeInfo
		{
			get { return GetZPropertyInfo(nameof(SailingIsGoingToChange)); }
		}

		#endregion

		#region IsPackLoadList

		public ZBool IsPackLoadList
		{
			get { return (this.IsExport() || (Transports.DepartureTransport != null && Transports.DepartureTransport.JW_RL_NKLoadPort == GlbBranch.CurrentBranch.GB_RL_NKHomePort)); }
		}

		#endregion

		#region MainTransport

		public Transport MainTransport
		{
			get { return Transports.MostInterestingTransport; }
		}

		#endregion

		#endregion

		#region JK_OH_Forwarder

		[RelatedBusinessObject("Forwarder")]
		[List("Forwarder_List")]
		public virtual ZGuid JK_OH_Forwarder
		{
			get { return (IsPackLoadList) ? SendingForwarderPK : ReceivingForwarderPK; }
			set
			{
				if (JK_OH_Forwarder != value)
				{
					IsDefaultedFromForwarder = true;
					if (IsPackLoadList)
					{
						OldForwarder = SendingForwarderPK;
						SetDefaultSendingForwarderAddress(value);
						SetDefaultReceivingForwarderAddress(ZGuid.Empty);
					}
					else
					{
						OldForwarder = ReceivingForwarderPK;
						SetDefaultReceivingForwarderAddress(value);
						SetDefaultSendingForwarderAddress(ZGuid.Empty);
					}
					IsDefaultedFromForwarder = false;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_OH_Forwarder();
					}
					SetForwardingFlag();

					Shipments.SetValuesFromParent();
				}
				JK_OH_ForwarderInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JK_OH_ForwarderInfo
		{
			get { return GetZPropertyInfo(Schema.JK_OH_Forwarder); }
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		[RelatedBusinessObject("Forwarder")]
		public ZGuid JK_OH_ForwarderForBinding
		{
			get => JK_OH_Forwarder;
			set
			{
				if (JK_OH_ForwarderForBinding != value)
				{
					if (Containers.Count > 0 && !IsDefaultedFromPorts)
					{
						ContinueWithChanging = true;
						MessageCaption = Res.GetString("e703e1dd-dcd6-4366-a900-a5b099eb3ed0", "Change Client");
						WarningMessage = Res.GetString("90b7ccc5-8f31-4140-85ea-4cbb8d73d659", "The current client of all the attached containers will be overridden. Do you want to continue?");
						WarningMessage = "";
						if (!ContinueWithChanging)
						{
							ContinueWithChanging = false;
							return;
						}
						ContinueWithChanging = false;
					}
					JK_OH_Forwarder = value;
				}
			}
		}

		ZGuid fOldForwarder;
		public ZGuid OldForwarder
		{
			get { return fOldForwarder; }
			set { fOldForwarder = value; }
		}

		public OrgHeader Forwarder
		{
			get { return Factory.Load<OrgHeader>(JK_OH_Forwarder); }
		}

		protected void SetForwardingFlag()
		{
			if (!IsInDatabase || !JK_IsForwarding)
			{
				ZBool isForward = ZBool.False;
				if (Forwarder != null)
				{
					isForward = Forwarder.IsProxyOrgOfAnyCompany();
				}
				JK_IsForwarding = isForward;
				Shipments.SetValuesFromParent();
				foreach (CFSShipment shipment in Shipments)
				{
					if (shipment.JS_OH_HandledOnBehalfOfForwarder == JK_OH_Forwarder)
					{
						using (shipment.SuspendCheckReset())
						{
							shipment.JS_IsForwardRegistered = isForward;
						}
					}
				}
			}
		}

		protected ZGuid FindFirstPackUnpackAddress(OrgAddressDependentCollection addresses, ZString addressType)
		{
			ZGuid result = ZGuid.Empty;

			if (addresses.Count > 0)
			{
				for (int count = 0; count < addresses.Count && result.IsEmpty; count++)
				{
					OrgAddress address = addresses[count];
					if (addressType == OrgConstants.AddressType.Delivery)
					{
						if ((address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery)
							|| address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Delivery))
							&& address.IsInDatabase)
						{
							result = address.PK;
						}
					}
					else if (addressType == OrgConstants.AddressType.Pickup)
					{
						if ((address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery)
							|| address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Pickup))
							&& address.IsInDatabase)
						{
							result = address.PK;
						}
					}
				}
				if (result.IsEmpty)
				{
					if (addresses[0].IsInDatabase)
					{
						result = addresses[0].PK;
					}
				}
			}
			return result;
		}

		protected override void SetTypeFlagsCore()
		{
			// we want to set the forwarding and cfs flags differently here
		}

		#endregion

		#region SetArrivalDepot and SetArrivalDepot

		protected override void SetArrivalDepot()
		{
			SetConsolDepot();
		}

		protected override void SetDepartureDepot()
		{
			SetConsolDepot();
		}

		void SetConsolDepot()
		{
			if (Forwarder != null && Forwarder.IsProxyOrgOfAnyCompany())
			{
				if (IsPackLoadList)
				{
					JK_OA_PackDepotAddress = FindFirstPackUnpackAddress(Forwarder.Addresses, OrgConstants.AddressType.Delivery);
					JK_OA_UnpackDepotAddress = ZGuid.Empty;
				}
				else
				{
					JK_OA_UnpackDepotAddress = FindFirstPackUnpackAddress(Forwarder.Addresses, OrgConstants.AddressType.Pickup);
					JK_OA_PackDepotAddress = ZGuid.Empty;
				}
			}
			else
			{
				var ownOrg = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy)
					?? Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

				if (ownOrg != null && ownOrg.IsInDatabase)
				{
					if (IsPackLoadList)
					{
						JK_OA_PackDepotAddress = FindFirstPackUnpackAddress(ownOrg.Addresses, OrgConstants.AddressType.Delivery);
						JK_OA_UnpackDepotAddress = ZGuid.Empty;
					}
					else
					{
						JK_OA_UnpackDepotAddress = FindFirstPackUnpackAddress(ownOrg.Addresses, OrgConstants.AddressType.Pickup);
						JK_OA_PackDepotAddress = ZGuid.Empty;
					}
				}
			}
		}

		#endregion

		#region JK_OA_CTOAddress

		public ZGuid JK_OA_CTOAddress
		{
			get { return (IsPackLoadList) ? JK_OA_DepartureCTOAddress : JK_OA_ArrivalCTOAddress; }
			set
			{
				if (IsPackLoadList)
				{
					JK_OA_DepartureCTOAddress = value;
					JK_OA_ArrivalCTOAddress = ZGuid.Empty;
				}
				else
				{
					JK_OA_ArrivalCTOAddress = value;
					JK_OA_DepartureCTOAddress = ZGuid.Empty;
				}
				JK_OA_CTOAddressInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_OA_CTOAddress();
				}
			}
		}

		public OrgAddress CTOAddress
		{
			get { return IsPackLoadList ? DepartureCTOAddress : ArrivalCTOAddress; }
		}

		public ZPropertyInfo JK_OA_CTOAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JK_OA_CTOAddress); }
		}

		#region ZAddress

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress JK_OA_CTOAddress_ZAddress
		{
			get
			{
				if (fJK_OA_CTOAddress_ZAddress == null)
				{
					fJK_OA_CTOAddress_ZAddress = new ZAddress(JK_OA_CTOAddressInfo);
					fJK_OA_CTOAddress_ZAddress.DefaultAddressType = IsPackLoadList
						? AddressType.DLV
						: AddressType.PIC;
				}
				return fJK_OA_CTOAddress_ZAddress;
			}
		}
		ZAddress fJK_OA_CTOAddress_ZAddress;

		#endregion

		#endregion

		#region JK_OA_EmptyContainerYard

		public ZGuid JK_OA_EmptyContainerYard
		{
			get { return IsPackLoadList ? JK_OA_ContainerYardEmptyPickupAddress : JK_OA_ContainerYardEmptyReturnAddress; }
			set
			{
				if (IsPackLoadList)
				{
					JK_OA_ContainerYardEmptyPickupAddress = value;
					JK_OA_ContainerYardEmptyReturnAddress = ZGuid.Empty;
				}
				else
				{
					JK_OA_ContainerYardEmptyPickupAddress = ZGuid.Empty;
					JK_OA_ContainerYardEmptyReturnAddress = value;
				}
				JK_OA_EmptyContainerYardInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_OA_EmptyContainerYard();
				}
			}
		}

		public OrgAddress EmptyContainerYardAddress
		{
			get { return IsPackLoadList ? ContainerYardEmptyPickupAddress : ContainerYardEmptyReturnAddress; }
		}

		public ZPropertyInfo JK_OA_EmptyContainerYardInfo
		{
			get { return GetZPropertyInfo(Schema.JK_OA_EmptyContainerYard); }
		}

		#region ZAddress

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress JK_OA_EmptyContainerYard_ZAddress
		{
			get
			{
				if (fJK_OA_EmptyContainerYard_ZAddress == null)
				{
					fJK_OA_EmptyContainerYard_ZAddress = new ZAddress(JK_OA_EmptyContainerYardInfo);
					fJK_OA_EmptyContainerYard_ZAddress.DefaultAddressType = IsPackLoadList
						? AddressType.DLV
						: AddressType.PIC;
				}

				return fJK_OA_EmptyContainerYard_ZAddress;
			}
		}

		ZAddress fJK_OA_EmptyContainerYard_ZAddress;

		#endregion

		#endregion

		#region JK_OA_CartageCoAddress

		[RelatedBusinessObject("CartageCoAddress")]
		public ZGuid JK_OA_CartageCoAddress
		{
			get { return (IsPackLoadList) ? JK_OA_DeparturePackCFSTransportAddress : JK_OA_ArrivalUnpackCFSTransportAddress; }
			set
			{
				if (JK_OA_CartageCoAddress != value)
				{
					if (IsPackLoadList)
					{
						JK_OA_DeparturePackCFSTransportAddress = value;
					}
					else
					{
						JK_OA_ArrivalUnpackCFSTransportAddress = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_OA_CartageCoAddress();
					}
				}
				JK_OA_CartageCoAddressInfo.RefreshBinding();
			}
		}

		public virtual OrgAddress CartageCoAddress
		{
			get { return Factory.Load<OrgAddress>(JK_OA_CartageCoAddress); }
			set { JK_OA_CartageCoAddress = value != null ? value.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo JK_OA_CartageCoAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JK_OA_CartageCoAddress); }
		}

		[RelatedBusinessObject("CartageCo")]
		[List("LocalTransport_List")]
		public ZGuid CartageCoPK
		{
			get { return CartageCo != null ? CartageCo.PK : ZGuid.Empty; }
			set
			{
				var org = Factory.Load<OrgHeader>(value);
				JK_OA_CartageCoAddress = org != null ? org.MainAddress.PK : ZGuid.Empty;
				Validation.ValidateCartageCoPK();
				CartageCoPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CartageCoPKInfo
		{
			get { return GetZPropertyInfo(Schema.CartageCoPK); }
		}

		public OrgHeader CartageCo
		{
			get { return CartageCoAddress != null ? Factory.Load<OrgHeader>(CartageCoAddress.OA_OH) : null; }
		}

		#region ZAddress

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress JK_OA_CartageCoAddress_ZAddress
		{
			get { return fJK_OA_CartageCoAddress_ZAddress ?? (fJK_OA_CartageCoAddress_ZAddress = GetNewJK_OA_CartageCoAddress_ZAddress()); }
		}
		ZAddress fJK_OA_CartageCoAddress_ZAddress;

		protected virtual ZAddress GetNewJK_OA_CartageCoAddress_ZAddress()
		{
			return new ZAddress(JK_OA_CartageCoAddressInfo);
		}

		#endregion

		#endregion

		#region JK_OA_PackDepotAddress

		public override ZGuid JK_OA_PackDepotAddress
		{
			get { return base.JK_OA_PackDepotAddress; }
			set
			{
				base.JK_OA_PackDepotAddress = value;
				JK_OA_DepotAddressInfo.RefreshBinding();
			}
		}

		#endregion

		#region JK_OA_UnpackDepotAddress

		public override ZGuid JK_OA_UnpackDepotAddress
		{
			get { return base.JK_OA_UnpackDepotAddress; }
			set
			{
				base.JK_OA_UnpackDepotAddress = value;
				JK_OA_DepotAddressInfo.RefreshBinding();
			}
		}

		#endregion

		#region JK_OA_DepotAddress

		[RelatedBusinessObject("DepotAddress")]
		public ZGuid JK_OA_DepotAddress
		{
			get
			{
				return IsPackLoadList
					? JK_OA_PackDepotAddress
					: JK_OA_UnpackDepotAddress;
			}
			set
			{
				if (value != JK_OA_DepotAddress)
				{
					if (IsPackLoadList)
					{
						JK_OA_PackDepotAddress = value;
						JK_OA_UnpackDepotAddress = ZGuid.Empty;
					}
					else
					{
						JK_OA_UnpackDepotAddress = value;
						JK_OA_PackDepotAddress = ZGuid.Empty;
					}

					Validation.ValidateJK_OA_DepotAddress();
					JK_OA_DepotAddressInfo.RefreshBinding();
				}
			}
		}

		public virtual OrgAddress DepotAddress
		{
			get { return Factory.Load<OrgAddress>(JK_OA_DepotAddress); }
		}

		public ZPropertyInfo JK_OA_DepotAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JK_OA_DepotAddress); }
		}

		[RelatedBusinessObject("Depot")]
		[List("Depot_List")]
		public ZGuid DepotPK
		{
			get
			{
				OrgAddress depotAddress = Factory.Load<OrgAddress>(JK_OA_DepotAddress);
				return depotAddress == null ? ZGuid.Empty : depotAddress.OA_OH;
			}
			set
			{
				if (value != DepotPK)
				{
					var depot = Factory.Load<OrgHeader>(value);
					var addressType = IsPackLoadList ? OrgConstants.AddressType.Delivery : OrgConstants.AddressType.Pickup;
					JK_OA_DepotAddress = depot != null ? FindFirstPackUnpackAddress(depot.Addresses, addressType) : ZGuid.Empty;

					Validation.ValidateDepotPK();
					DepotPKInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DepotPKInfo
		{
			get { return GetZPropertyInfo(Schema.DepotPK); }
		}

		public OrgHeader Depot
		{
			get { return DepotAddress != null ? Factory.Load<OrgHeader>(DepotAddress.OA_OH) : null; }
		}

		protected bool JK_OA_DepotAddress_ReadOnly
		{
			get { return Forwarder == null || (LoadPort == null && DischargePort == null); }
		}

		protected bool DepotPK_ReadOnly
		{
			get { return JK_OA_DepotAddress_ReadOnly; }
		}

		#region ZAddress

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress JK_OA_DepotAddress_ZAddress
		{
			get { return fJK_OA_DepotAddress_ZAddress ?? (fJK_OA_DepotAddress_ZAddress = GetNewJK_OA_DepotAddress_ZAddress()); }
		}
		ZAddress fJK_OA_DepotAddress_ZAddress;

		protected virtual ZAddress GetNewJK_OA_DepotAddress_ZAddress()
		{
			return new ZAddress(JK_OA_DepotAddressInfo);
		}

		#endregion

		#endregion

		#region Canada Specific

		public ZString CanadaCCNNumber
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					var cnnNumber = Numbers.Find(num => num.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN).FirstOrDefault();
					return (cnnNumber != null) ? cnnNumber.CE_EntryNum : ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString CanadaPCNNumber
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					var pcnNumber = Numbers.Find(num => num.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.PCN).FirstOrDefault();
					return (pcnNumber != null) ? pcnNumber.CE_EntryNum : ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#endregion

		#region Validation

		#region ValidatePackLinesRelatingToConsol

		public void ValidatePackLinesRelatingToConsol(BusinessObject[] packs)
		{
			foreach (BusinessObject pack in packs)
			{
				var packLine = pack as CFSPackLine;
				if (packLine != null)
				{
					CFSShipment shipment = ShipmentReceivalFromPack(packLine);

					if (shipment != null && !packLine.HasWarnings)
					{
						shipment.Validation.ValidateJS_ActualVolume();
						shipment.Validation.ValidateJS_ActualWeight();
						shipment.Validation.ValidateJS_OuterPacks();

						if (shipment.JS_ActualVolumeInfo.HasWarnings() ||
							shipment.JS_ActualWeightInfo.HasWarnings() ||
							shipment.JS_OuterPacksInfo.HasWarnings())
						{
							packLine.AddRowWarning(Res.GetString("37e74aca-3196-4b95-be01-3dba05057534", "There are warnings on the Shipment relating to the packline volume, weight or count"));
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region Lists

		#region LocalTransport_List

		protected LocalTransportCollection fLocalTransport_List;
		public LocalTransportCollection LocalTransport_List
		{
			get
			{
				if (fLocalTransport_List == null)
				{
					fLocalTransport_List = new LocalTransportCollection(Factory);
				}
				return fLocalTransport_List;
			}
		}

		#endregion

		#region Depot_List

		public CFSLoadListConsolDepotCollection Depot_List
		{
			get
			{
				return Factory.GetCachedValue("CFSLoadListConsol.DepotCollection", () => new CFSLoadListConsolDepotCollection(Factory));
			}
		}

		#endregion

		#region ContainerYard_List

		protected ContainerYardCollection fContainerYard_List;
		public ContainerYardCollection ContainerYard_List
		{
			get
			{
				if (fContainerYard_List == null)
				{
					fContainerYard_List = new ContainerYardCollection(Factory);
				}
				return fContainerYard_List;
			}
		}

		#endregion

		#region OrgDebtor_List

		public OrganisationsFindBoxCollection OrgDebtor_List
		{
			get { return new DebtorCollection(Factory); }
		}

		#endregion

		#region Forwarder_List

		public OrganisationsFindBoxCollection Forwarder_List
		{
			get
			{
				return IsPackLoadList
					? SendingForwarderList
					: ReceivingForwarderList;
			}
		}

		#endregion

		#region CTOAddress_List

		public OrgHeaderCollection CTOAddress_List
		{
			get
			{
				return IsPackLoadList
					? OrgDepartureCtoList
					: OrgArrivalCtoList;
			}
		}

		public OrgHeaderCollection EmptyContainerYard_List
		{
			get
			{
				return IsPackLoadList
					? OrgDepartureContainerYardList
					: OrgArrivalContainerYardList;
			}
		}

		#endregion

		#region Shipments_List

		protected override ModuleShipmentCollection GetModuleShipmentCollection()
		{
			return new CFSModuleShipmentCollection(Factory);
		}

		protected override void SetFiltersOnModuleShipmentCollection(ModuleShipmentCollection collection)
		{
			CFSShipmentDefaultFilterProvider provider = new CFSShipmentDefaultFilterProvider();
			provider.TransportMode = JK_TransportMode;
			provider.LoadPort = JK_RL_NKLoadPort;
			provider.DischargePort = JK_RL_NKDischargePort;
			provider.Client = JK_OH_Forwarder;
			provider.SetDefaultFilters(collection);
		}

		#endregion

		#region Containers_List

		CFSContainerRegistrationList fContainers_List;
		public CFSContainerRegistrationList Containers_List
		{
			get
			{
				if (fContainers_List == null)
				{
					fContainers_List = GetNewContainers_List();
				}

				#region Set Default Filters

				fContainers_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Purpose Type", "Property", (ZString)ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS));
				fContainers_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Container Mode", "Property", JK_ConsolMode));

				if (JK_OH_Forwarder.IsValid)
				{
					fContainers_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ConstantsAndReusables.OrgFilterTypes.Client, "Property", JK_OH_Forwarder));
				}

				if (!JK_JX_JA_RL_NKPortOfLoading.IsEmpty || !JK_JX_JB_RL_NKPortOfDischarge.IsEmpty)
				{
					fContainers_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ConstantsAndReusables.PortFilterTypes.LoadDischarge, "Property1", JK_JX_JA_RL_NKPortOfLoading));
					fContainers_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ConstantsAndReusables.PortFilterTypes.LoadDischarge, "Property2", JK_JX_JB_RL_NKPortOfDischarge));
				}

				fContainers_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Voyage / Flight / Vessel", "Vessel", JK_JX_JV_NKVessel));
				fContainers_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Voyage / Flight / Vessel", "VoyageFlightNo", JK_JX_JV_VoyageFlight));

				#endregion

				return fContainers_List;
			}
		}

		protected virtual CFSContainerRegistrationList GetNewContainers_List()
		{
			CFSContainerRegistrationList result = new CFSContainerRegistrationList(Factory);
			return result;
		}

		#endregion

		public OrganisationsFindBoxCollection OrgHeaderListForGuidFindBox
		{
			get
			{
				if (fOrgHeaders == null)
				{
					fOrgHeaders = new OrganisationsFindBoxCollection(Factory);
				}
				return fOrgHeaders;
			}
		}
		OrganisationsFindBoxCollection fOrgHeaders;

		#endregion

		#region IHaveInternalCartage Members

		ZGuid IHaveInternalCartage.BranchPK
		{
			get { return ZGuid.Empty; }
		}

		ZString IHaveInternalCartage.OwnerRef
		{
			get { return ZString.Empty; }
		}

		bool IHaveInternalCartage.IsAllowedToUpdateAdviseDates
		{
			get { return false; }
		}

		ZString IHaveInternalCartage.TransportMode
		{
			get { return JK_TransportMode; }
		}

		ZString IHaveInternalCartage.ServiceLevel
		{
			get { return ""; }
		}

		ZString IHaveInternalCartage.CartageTypeOverride
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IHaveInternalCartage.DeliveryCartageAdvisedInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.PickupCartageAdvisedInfo
		{
			get { return null; }
		}

		bool IHaveInternalCartage.TypeSpecificPreCreationCheck()
		{
			return true;
		}

		public ZBool DeliveryAndPickupCartageApplicable
		{
			get { return ZBool.True; }
		}

		public ZBool PackedAtDepot
		{
			get { return ZBool.True; }
		}

		public Freight.Business.IContainer[] GetContainers()
		{
			return (Freight.Business.IContainer[])Containers.ToArray(typeof(Freight.Business.IContainer));
		}

		public IPackLineInfo[] GetPackLines()
		{
			return (IPackLineInfo[])RelatedPackLines.ToArray(typeof(IPackLineInfo));
		}

		public OrgHeader PickupCartageOrg
		{
			get { return DeparturePackCFSTransport; }
		}

		public OrgHeader DeliveryCartageOrg
		{
			get { return ArrivalUnpackCFSTransport; }
		}

		ZString IHaveInternalCartage.ContainerMode
		{
			get { return Core.Constants.ContainerModes.Containerised; }
		}

		public ZBool InternalCartageEnabled
		{
			get { return ZBool.True; }
		}

		string ICartageExportSupport.JobNumber
		{
			get { return JK_UniqueConsignRef; }
		}

		JobDocAddressDependentCollection IHaveInternalCartage.DocAddresses
		{
			get { return null; }
		}

		#region Addresses

		#region CTO Addresses

		ZGuid IHaveInternalCartage.CartagePickupCTOAddress
		{
			get { return (ZGuid)JK_OA_DepartureCTOAddressInfo.Value; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupCTOAddressInfo
		{
			get { return JK_OA_DepartureCTOAddressInfo; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryCTOAddress
		{
			get { return (ZGuid)JK_OA_ArrivalCTOAddressInfo.Value; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryCTOAddressInfo
		{
			get { return JK_OA_ArrivalCTOAddressInfo; }
		}

		#endregion

		#region Depot (CFS) Addresses

		ZGuid IHaveInternalCartage.CartagePickupDepotAddress
		{
			get { return (ZGuid)JK_OA_PackDepotAddressInfo.Value; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupDepotAddressInfo
		{
			get { return JK_OA_PackDepotAddressInfo; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryDepotAddress
		{
			get { return (ZGuid)JK_OA_UnpackDepotAddressInfo.Value; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryDepotAddressInfo
		{
			get { return JK_OA_UnpackDepotAddressInfo; }
		}

		#endregion

		#region ContainerYard Addresses

		ZGuid IHaveInternalCartage.CartagePickupContainerYardAddress
		{
			get { return (ZGuid)JK_OA_ContainerYardEmptyPickupAddressInfo.Value; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupContainerYardAddressInfo
		{
			get { return JK_OA_ContainerYardEmptyPickupAddressInfo; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryContainerYardAddress
		{
			get { return (ZGuid)JK_OA_ContainerYardEmptyReturnAddressInfo.Value; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryContainerYardAddressInfo
		{
			get { return JK_OA_ContainerYardEmptyReturnAddressInfo; }
		}

		#endregion

		#region Importer/Exporter Addresses

		JobDocAddress IHaveInternalCartage.CartageImporterDocAddress
		{
			get { return null; }
		}

		JobDocAddress IHaveInternalCartage.CartageExporterDocAddress
		{
			get { return null; }
		}

		#endregion

		#endregion

		#endregion

		#region Generation of Job ID

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				var handlers = new List<IUniqueIndexFailureHandler>();
				if (ParentCartage != null)
				{
					handlers.Add(new ConsolWithParentCartageUniqueIndexFailureHandler(this));
				}
				else
				{
					handlers.AddRange(base.UniqueIndexFailureHandlers);
				}

				return handlers;
			}
		}

		class ConsolWithParentCartageUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public ConsolWithParentCartageUniqueIndexFailureHandler(CFSLoadListConsol parent)
			{
				if (parent == null)
				{
					throw new ArgumentNullException(nameof(parent));
				}

				this.parent = parent;
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(
					Res.GetString("23d863af-c9d4-4170-b16c-10175443cf54", "The system has made changes while you have been working on this Load List.\r\nPlease try and save again."), Res.GetString("f20f57c9-5675-45aa-8fca-30cee582ce0e", "Load List changes"));

				parent.Factory.ClearQueryCache();
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return JobConsolSchema.Constants.Indexes.NR_UX__JK_UniqueConsignRef; }
			}

			readonly CFSLoadListConsol parent;
		}

		public override void PopulateJK_UniqueConsignRefIfNeeded()
		{
			if (ParentCartage != null)
			{
				JK_UniqueConsignRef = GetParentCartageUniqueConsignRef();
				if (!Factory.HasContext(BusinessContext.EnableJobHeaderNumberChange))
				{
					Factory.SetContext(BusinessContext.EnableJobHeaderNumberChange);
				}
			}
			else if (OnSaveWillChangeFromCFSJobNumberToForwardingJobNumber)
			{
				MakeExistingJK_UniqueConsignRefForwarding();
			}
			else
			{
				ConsignRefHandler = null;
				PopulateFormattedNumberPropertyIfRequired(JK_UniqueConsignRefInfo, NumberFountainForUniqueConsignRef);
			}
		}

		string GetParentCartageUniqueConsignRef()
		{
			string cartageNumberSuffix = ParentCartage.JJ_ConsignmentID + "/L";

			ZQuery uniqueIndexQuery = new ZQuery(JobConsolSchema.JK_UniqueConsignRef, SQLComparisonOperator.StartsWith, cartageNumberSuffix);
			CFSLoadListConsol[] loadLists = Factory.Load<CFSLoadListConsol>(uniqueIndexQuery);
			int max = 0;
			foreach (CFSLoadListConsol loadList in loadLists)
			{
				string numberAsString = loadList.JK_UniqueConsignRef.SubstringSafe(cartageNumberSuffix.Length);
				int number;
				if (int.TryParse(numberAsString, out number) && number > max)
				{
					max = number;
				}
			}

			return cartageNumberSuffix + new ZInt(max + 1);
		}

		void MakeExistingJK_UniqueConsignRefForwarding()
		{
			string oldID = JK_UniqueConsignRef;
			JK_UniqueConsignRef = base.GetNewJK_UniqueConsignRef(Factory);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(AutoEvents.EditedARecord, ZString.Format("Changed job number from {0} to {1}", oldID, JK_UniqueConsignRef));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		protected bool IsNewCFSJob
		{
			get { return JK_UniqueConsignRef.IsEmpty && JK_IsCFS && !JK_IsForwarding; }
		}

		protected bool ShouldHaveForwardingJobNumber
		{
			get
			{
				ZQuery jobHeaderFilter = new ZQuery(JobHeaderSchema.JH_ParentID, PK);
				bool attachedJobHeader = Factory.LoadTop1(typeof(JobHeader), jobHeaderFilter) != null;
				return JK_IsCFS && JK_IsForwarding && !JK_UniqueConsignRef.IsEmpty && !attachedJobHeader;
			}
		}

		protected bool HasCFSJobNumber
		{
			get { return JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainCFSPrefix, StringComparison.Ordinal); }
		}

		public bool OnSaveWillChangeFromCFSJobNumberToForwardingJobNumber
		{
			get { return HasCFSJobNumber && ShouldHaveForwardingJobNumber; }
		}

		protected override INumberFountainProxy NumberFountainForUniqueConsignRef
		{
			get
			{
				if (IsNewCFSJob)
				{
					return Env.NumberFountains.JobConsolNumberCFS;
				}
				else
				{
					return base.NumberFountainForUniqueConsignRef;
				}
			}
		}

		#endregion

		#region Parent Cartage

		protected override void PopulateFromCartageCore(LocalCartage.Integration.ICommonCartage cartage_OtherFactory, CommonContainer[] selectedContainers_OtherFactory)
		{
			CommonCartage cartage = (CommonCartage)cartage_OtherFactory;

			JK_OH_Forwarder = cartage.LocalClientPK;
			JK_RL_NKLoadPort = cartage.PortOfLoading;
			JK_RL_NKDischargePort = cartage.PortOfDischarge;
			if (!cartage.JJ_JX_Sailing.IsEmpty)
			{
				Transports[0].JW_JX = cartage.JJ_JX_Sailing;
			}

			foreach (CommonContainer container in selectedContainers_OtherFactory)
			{
				CommonContainer container_NewFactory = (CommonContainer)Factory.Load(Containers.TypeOfElements, container.PK);
				container_NewFactory.JC_OH_CFSClient = cartage.LocalClientPK;
				container_NewFactory.JC_IsCFSRegistered = true;
				if (!cartage.JJ_JX_Sailing.IsEmpty)
				{
					container_NewFactory.JC_JX = cartage.JJ_JX_Sailing;
				}

				Containers.Add(container_NewFactory);
			}

			JobHeader loadList_JobHeader = new JobHeader.Loader(this).TryLoadOrCreate();
			loadList_JobHeader.JH_OA_LocalChargesAddr = cartage.Job.JH_OA_LocalChargesAddr;
			loadList_JobHeader.JH_JH_ParentJob = cartage.Job.PK;
		}

		CommonCartage ParentCartage
		{
			get
			{
				CommonCartage result = null;
				if (Job != null && !Job.IsDeleted && Job.ParentJob != null && !Job.ParentJob.JH_ParentID.IsEmpty && Job.ParentJob.JH_ParentTableCode == JobCartageSchema.Constants.Prefix)
				{
					result = Factory.Load<CommonCartage>(Job.ParentJob.JH_ParentID);
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		protected override void DefaultSendingForwarder()
		{
			if (JK_OH_Forwarder.IsEmpty)
			{
				base.DefaultSendingForwarder();
			}
		}

		protected override void DefaultReceivingForwarder()
		{
			if (JK_OH_Forwarder.IsEmpty)
			{
				base.DefaultReceivingForwarder();
			}
		}

		public void SetWarnNotErrorOnLocationTotalsOnPackLines(bool warnInsteadOfError)
		{
			foreach (CFSShipment shipment in Shipments)
			{
				shipment.SetWarnNotErrorOnLocationTotalsOnPackLines(warnInsteadOfError);
			}
		}

		protected CFSShipment ShipmentReceivalFromPack(PackLine pack)
		{
			return Factory.Load<CFSShipment>(pack.JL_JS);
		}

		#region ThereExistsPackShipmentTotalsImbalance

		public bool ThereExistsPackShipmentTotalsImbalance(BusinessObject[] packs)
		{
			bool thereAreNotifications = false;

			foreach (BusinessObject pack in packs)
			{
				CFSPackLine packLine = pack as CFSPackLine;
				if (packLine != null)
				{
					CFSShipment shipment = packLine.Shipment;

					if (shipment != null)
					{
						shipment.Validation.ValidateJS_ActualVolume();
						shipment.Validation.ValidateJS_ActualWeight();
						shipment.Validation.ValidateJS_OuterPacks();

						if (shipment.JS_ActualVolumeInfo.HasNotifications() ||
							shipment.JS_ActualWeightInfo.HasNotifications() ||
							shipment.JS_OuterPacksInfo.HasNotifications())
						{
							thereAreNotifications = true;
						}
					}
				}
			}

			return thereAreNotifications;
		}

		#endregion

		#region SetupBasedOnImportExport

		protected void SetupBasedOnImportExport()
		{
			if (this.unAllocatedPackLines != null)
			{
				UnAllocatedPackLines.ShowOnlyReceived = IsPackLoadList;
			}
		}

		#endregion

		public bool ContinueWithChanging;
		bool IsDefaultedFromPorts;
		protected bool IsDefaultedFromForwarder;

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			CartageHelper.AttachCartageJobsToParentJob(job, PK);
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

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		CFSLoadListConsolInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = GetNewInvoicingSupporter()); }
		}

		protected virtual CFSLoadListConsolInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new CFSLoadListConsolInvoicingSupporter(this);
		}

		protected override JobInvoicingConsumerType GetConsumerTypeCore()
		{
			return InvoicingSupporter.ConsumerType;
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new CFSLoadListConsolRatingAdapter(new CFSLoadListConsolRatingRoute(this));
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new CFSLoadListConsolRatingAdaptersProvider<CFSLoadListConsol>(this); }
		}

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();

			result.AddRecipient(Forwarder);

			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return base.GetEmailSubject(); }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.LoadLists; }
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return ObjectFactory.GetType<DocumentWrappers.IDocLoadListConsol>(); }
		}

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new CFSLoadListConsolDocManagerInfo(this, "LOA");
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return new CFSLoadListConsolDocumentSupporter(this); }
		}

		#endregion

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new CFSLoadListConsolTransportSupporter(this); }
		}

		void ITransportParentCommon.MarkAsNeedingValidation()
		{
			MarkAsNeedingValidation();
			Containers.MarkAsNeedingValidation();
		}

		#endregion

		#region ICartageParent Members

		event EventHandler ICartageParent.CartageTypesChanged
		{
			add
			{
				JK_RL_NKLoadPortInfo.ValueChanged += value;
				JK_RL_NKDischargePortInfo.ValueChanged += value;
			}
			remove
			{
				JK_RL_NKLoadPortInfo.ValueChanged -= value;
				JK_RL_NKDischargePortInfo.ValueChanged -= value;
			}
		}

		IReadOnlyCollection<CartageType> ICartageParent.CartageTypes
		{
			get { return new[] { ((ICartageParent)this).GetLocalCartageType }; }
		}

		CartageType ICartageParent.GetLocalCartageType
		{
			get { return IsPackLoadList ? new LoadListPickupCartageType(this) : new LoadListDeliveryCartageType(this); }
		}

		ZString ICartageParent.UniqueConsignmentID
		{
			get { return JK_UniqueConsignRef; }
		}

		ZGuid ICartageParent.CartageParentID
		{
			get { return PK; }
		}

		ZString ICartageParent.CartageParentTableCode
		{
			get { return JobConsolSchema.Constants.Prefix; }
		}

		ZGuid ICartageParent.BranchPK
		{
			get { return ZGuid.Empty; }
		}

		ZString ICartageParent.OrderReferenceNumber
		{
			get { return JK_AgentsReference; }
		}

		ZString ICartageParent.ServiceLevel
		{
			get { return ""; }
		}

		ControllerID ICartageParent.ControllerID
		{
			get { return ControllerIDs.LoadListConsol; }
		}

		ZString ICartageParent.WayBillNumber
		{
			get { return JK_MasterBillNum; }
		}

		ZString ICartageParent.GoodsDescription
		{
			get { return ""; }
		}

		ZGuid ICartageParent.JobHeaderPK
		{
			get { return Job != null ? Job.PK : ZGuid.Empty; }
		}

		ZGuid ICartageParent.LocalClientAddressPK
		{
			get { return Job != null ? Job.JH_OA_LocalChargesAddr : ZGuid.Empty; }
		}

		ZInt ICartageParent.TotalPackages
		{
			get { return 0; }
		}

		ZString ICartageParent.TotalPackType
		{
			get { return ""; }
		}

		ZDecimal ICartageParent.TotalWeight
		{
			get { return 0; }
		}

		ZString ICartageParent.TotalWeightUnit
		{
			get { return ""; }
		}

		ZDecimal ICartageParent.TotalVolume
		{
			get { return 0; }
		}

		ZString ICartageParent.TotalVolumeUnit
		{
			get { return ""; }
		}

		bool ICartageParent.RebuildLocalCartageMenuOnClick
		{
			get { return false; }
		}

		bool ICartageParent.UseJobTotals
		{
			get { return false; }
		}

		void ICartageParent.CartageCreatedAndSaved()
		{
		}

		IStmALogParent ICartageParent.BusinessObjectForRelatedEvents
		{
			get { return this; }
		}

		IDocManagerSupport ICartageParent.BusinessObjectForRelatedEDocs
		{
			get { return this; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return JK_UniqueConsignRef; }
		}

		public void SetJobNumberFieldOnSaving()
		{
			if (!IsInDatabase && JK_UniqueConsignRef.IsEmpty)
			{
				JK_UniqueConsignRef = NumberFountainForUniqueConsignRef.GetNextFormatted(Factory);
			}
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter

		protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		protected override ZString GetUnitOfMeasureCore(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.JK_TotalShipmentActVolumeCheck:
					unitOfMeasure = (IsAir) ? JK_TotalShipmentActOtherUnit : JK_TotalShipmentChargeableUnit;
					break;

				case Schema.JK_TotalShipmentActWeightCheck:
					unitOfMeasure = (IsAir) ? JK_TotalShipmentChargeableUnit : JK_TotalShipmentActOtherUnit;
					break;

				case Schema.JK_TotalShipmentChargableCheck:
					unitOfMeasure = JK_TotalShipmentChargeableUnit;
					break;

				default:
					unitOfMeasure = base.GetUnitOfMeasureCore(property);
					break;
			}

			return unitOfMeasure;
		}

		protected override void RoundMeasurePropertiesOnTransportModeChangedCore()
		{
			base.RoundMeasurePropertiesOnTransportModeChangedCore();

			this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActVolumeCheck, JK_TotalShipmentActVolumeCheckInfo);
			this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentActWeightCheck, JK_TotalShipmentActWeightCheckInfo);
			this.SetRoundedValue(JobConsolSchema.JK_TotalShipmentChargableCheck, JK_TotalShipmentChargableCheckInfo);
		}

		#endregion

		#region IWorkflowProvider

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.CFSLoadList.Code; }
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

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CFSLoadListConsolProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}

		CFSLoadListConsolProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JK_TransportMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, JK_RL_NKLoadPort, JK_RL_NKLoadPort.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, JK_RL_NKDischargePort, JK_RL_NKDischargePort.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, JK_OH_Forwarder, ZGuid.Empty);

			return result;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CFSLoadListConsolFetchStrategy(this);
		}
	}

	public class CFSLoadListConsolInvoicingSupporter : JobInvoicingSupporter
	{
		public CFSLoadListConsolInvoicingSupporter(CFSLoadListConsol parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly CFSLoadListConsol Parent;

		public override ZDateTime ATA
		{
			get { return Parent.JK_JX_JB_A_ARV; }
		}

		public override ZDateTime ATD
		{
			get { return Parent.JK_JX_JA_A_DEP; }
		}

		public override ZDateTime ETA
		{
			get { return Parent.JK_JX_JB_E_ARV; }
		}

		public override ZDateTime ETD
		{
			get { return Parent.JK_JX_JA_E_DEP; }
		}

		public override ZDateTime ArrivalAtLoadPort
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				var transport = Parent.Transports.MostInterestingTransport;
				if (transport != null && transport.JW_IsLinked && transport.Sailing != null)
				{
					if (transport.Sailing.Origin.JA_A_ARV.IsValid)
					{
						result = transport.Sailing.Origin.JA_A_ARV;
					}
					else if (transport.Sailing.Origin.JA_E_ARV.IsValid)
					{
						result = transport.Sailing.Origin.JA_E_ARV;
					}
				}

				return result;
			}
		}

		public override ZDateTime EstimatedArrivalAtLoadPort
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				var transport = Parent.Transports.MostInterestingTransport;
				if (transport != null && transport.JW_IsLinked && transport.Sailing != null && transport.Sailing.Origin.JA_E_ARV.IsValid)
				{
					result = transport.Sailing.Origin.JA_E_ARV;
				}

				return result;
			}
		}

		public override ZString TransportMode
		{
			get { return Parent.JK_TransportMode; }
		}

		public override ZString ContainerMode
		{
			get { return Parent.JK_ConsolMode; }
		}

		public override RefUNLOCO Origin
		{
			get
			{
				if (Parent.Schedule != null && Parent.Schedule.Origin != null && Parent.Schedule.Origin.JA_RL_NKPortOfLoading.IsValid)
				{
					return Parent.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Parent.Schedule.Origin.JA_RL_NKPortOfLoading);
				}

				return null;
			}
		}

		public override RefUNLOCO Destination
		{
			get
			{
				if (Parent.Schedule != null && Parent.Schedule.Destination != null && Parent.Schedule.Destination.JB_RL_NKPortOfDischarge.IsValid)
				{
					return Parent.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Parent.Schedule.Destination.JB_RL_NKPortOfDischarge);
				}

				return null;
			}
		}

		public override ZDecimal ActualChargeable
		{
			get { return Parent.JK_ConsolChargeable; }
		}

		public override OrgHeader SendingAgent
		{
			get { return Parent.Forwarder; }
		}

		public override OrgHeader ReceivingAgent
		{
			get { return Parent.Forwarder; }
		}

		public override OrgHeader Consignor
		{
			get { return Parent.Forwarder; }
		}

		public override OrgHeader Consignee
		{
			get { return Parent.Forwarder; }
		}

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override bool IsImport
		{
			get { return Parent.IsImport(); }
		}

		public override bool IsExport
		{
			get { return Parent.IsExport(); }
		}

		public override bool IsDomestic
		{
			get { return Parent.IsDomestic(); }
		}

		public override bool IsCrossTrade
		{
			get { return Parent.IsCrossTrade(); }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.CFSLoadList; }
		}

		public override ZDecimal ActualWeight
		{
			get { return Parent.JK_TotalShipmentWeight; }
		}

		public override ZString ActualWeightUnit
		{
			get { return Parent.JK_TotalShipmentWeightUnit; }
		}

		public override ZDecimal ActualVolume
		{
			get { return Parent.JK_TotalShipmentVolume; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return Parent.JK_TotalShipmentVolumeUnit; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Parent.IsInDatabaseIncludingChildren; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.CFSLoadListAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.CFSLoadListJobInvoicing;
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}

		public override int ContainerCount
		{
			get { return Parent.Containers.Count; }
		}

		public override ZDecimal TEUCount
		{
			get { return Parent.Containers.TEUCount; }
		}

		public override ZString MasterBillNumber
		{
			get { return Parent.JK_MasterBillNum; }
		}

		public override ZString VoyageVesselOrFlightDate
		{
			get
			{
				var result = ZString.Empty;
				if (Parent != null)
				{
					var departureDate = !Parent.JK_JX_JA_A_DEP.IsEmpty ? Parent.JK_JX_JA_A_DEP : Parent.JK_JX_JA_E_DEP;
					result = GetVoyageVesselOrFlightDatesCore(Parent.JK_TransportMode, departureDate, Parent.JK_JX_JV_NKVessel, Parent.JK_JX_JV_VoyageFlight);
				}
				return result;
			}
		}
	}
}
