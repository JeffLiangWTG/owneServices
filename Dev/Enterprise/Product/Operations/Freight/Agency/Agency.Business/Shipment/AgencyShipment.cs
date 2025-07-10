using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business
{
	[TestExcludeWorkflowProviderHasTestCase]  // Tested in BillOfLading and AgencyBooking
	[SupportExRateSource(ExRateSourceType.Voyage)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.AgencyShipment)]
	[CanHaveTriggersDespiteNotImplementingIWorkflowProvider] // Actually I do implement IWorkflowProvider. But my WorkflowDescriptor isn't included in EnterpriseApplicationConfiguration.xml as I don't implement a GUI.
	public partial class AgencyShipment : CommonShipment,
		Integration.Agency.IAgencyShipment,
		IWorkflowProvider,
		IRatingSupporter,
		ISendEmailSource,
		IBillGenerationSupport,
		ISailingParentFindBox,
		IJobInvoicingExRateSourceProvider,
		IVoyageFinderParent,
		IProcessHandlingInfoProvider,
		IDeniedPartyProvider,
		IComplianceJobDirectionProvider,
		ICompliancePartyRiskStatusProvider,
		IComplianceCommodityRiskStatusProvider,
		IComplianceLocationRiskStatusProvider
	{
		#region Schema

		public new class Schema : CommonShipment.Schema
		{
			public const string JS_NKDischargePort = "JS_NKDischargePort";
			public const string JS_NKLoadPort = "JS_NKLoadPort";
			public const string BookingPartyNameOrPK = "BookingPartyNameOrPK";

			public const string TopLevelPacksTotalVolumeInShipmentVolumeUnit = "TopLevelPacksTotalVolumeInShipmentVolumeUnit";
			public const string TopLevelPacksTotalWeightInShipmentWeightUnit = "TopLevelPacksTotalWeightInShipmentWeightUnit";

			public const string BookedContainers = "BookedContainers";
			public const string RealContainers = "RealContainers";
			public const string Vehicles = "Vehicles";
			public const string TopLevelPacks = "TopLevelPacks";
		}

		#endregion

		public AgencyShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JS_ShipmentStatus), ConcurrencyPolicy.Strict);
			SetupBuyerSupplierLinkHelper();
		}

		#region Validation

		protected override JobShipmentValidation GetNewValidation()
		{
			return new AgencyShipmentValidation(this);
		}

		public new AgencyShipmentValidation Validation
		{
			get { return (AgencyShipmentValidation)base.Validation; }
		}

		#endregion

		#region Confirm

		public bool IsBillOfLadingStage
		{
			get { return ShipmentStatusHelperMethods.IsBillOfLadingStage(JS_ShipmentStatus); }
		}

		public void Confirm()
		{
			if (!IsBillOfLadingStage)
			{
				bool suppressPackLinesUpdateValue = SuppressPackLinesUpdate;
				try
				{
					JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
					IsConfirmed = true;

					SuppressPackLinesUpdate = true;
					ConvertWeightsAndVolumes();

					if (!IsFCL)
					{
						ConvertCargoFromBookedToReal();
						ConvertCargoMeasures(RealContainers);
						SplitCargoIfNecessary(RealContainers);
					}
					else
					{
						SplitContainers();
					}

					ConfirmCore();
					Logs.AddNew(Events.BookingConfirmed);

					BookedContainers.ResetChangeTracking();
					RealContainers.ResetChangeTracking();
					OuterPackLines.ResetChangeTracking();

					ResetShippingContainerViews();
				}
				finally
				{
					SuppressPackLinesUpdate = suppressPackLinesUpdateValue;
				}
			}
		}

		protected virtual void ConfirmCore()
		{
		}

		protected virtual bool IsConfirmed { get; set; }

		internal bool GetIsConfirmed()
		{
			return IsConfirmed;
		}

		#endregion

		#region Lookups

		public new AgencyShipmentLookups Lookups
		{
			get { return (AgencyShipmentLookups)base.Lookups; }
		}

		protected override JobShipmentLookups GetNewLookups()
		{
			return new AgencyShipmentLookups(this);
		}

		#endregion

		public void DefaultWeightAndVolumeUnits()
		{
			if (JS_PackingMode == Constants.ContainerModes.FCL)
			{
				DefaultWeightAndVolumeUnitsFromPacklines();
			}
		}

		void DefaultWeightAndVolumeUnitsFromPacklines()
		{
			ZString packWeightUnit = CalculateUnitFromPacks(packLine => packLine.JL_ActualWeightUQ);

			if (packWeightUnit.IsEmpty)
			{
				packWeightUnit = RegistryWeightUnit;
			}

			ZString packVolumeUnit = CalculateUnitFromPacks(packLine => packLine.JL_ActualVolumeUQ);

			if (packVolumeUnit.IsEmpty)
			{
				packVolumeUnit = RegistryVolumeUnit;
			}

			IsUpdatingShipmentFromPackLines = true;
			try
			{
				JS_UnitOfWeight = packWeightUnit;
				JS_UnitOfVolume = packVolumeUnit;
			}
			finally
			{
				IsUpdatingShipmentFromPackLines = false;
			}
		}

		ZString CalculateUnitFromPacks(Func<PackLine, ZString> getUnit)
		{
			ZString result = ZString.Empty;

			foreach (PackLine packLine in OuterPackLines)
			{
				ZString unit = getUnit(packLine);

				if (unit.IsEmpty)
				{
					continue;
				}

				if (result.IsEmpty)
				{
					result = unit;
				}
				else if (result != unit)
				{
					result = ZString.Empty;
					break;
				}
			}

			return result;
		}

		public ZString RegistryWeightUnit
		{
			get
			{
				return IsBillOfLadingStage
					? AgencyRegistry.Instance.DefaultBillWeightUnit.Value
					: AgencyRegistry.Instance.DefaultBookingWeightUnit.Value;
			}
		}

		public ZString RegistryVolumeUnit
		{
			get
			{
				return IsBillOfLadingStage
					? AgencyRegistry.Instance.DefaultBillVolumeUnit.Value
					: AgencyRegistry.Instance.DefaultBookingVolumeUnit.Value;
			}
		}

		#region LastSavedValues

		public ZGuid JS_JX_LastSavedSailing
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ZGuid)JS_JXInfo.OriginalValue; }
		}

		public AllocationUsage LastSavedUsage
		{
			get
			{
				if (lastSavedUsage == null)
				{
					if (IsInDatabase)
					{
						BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();
						AgencyShipment shipment = readOnlyFactory.Load<AgencyShipment>(PK);
						lastSavedUsage = (ShipmentStatusHelperMethods.CountsTowardsAllocations(shipment.JS_ShipmentStatus)
							? AllocationUsage.LoadFromShipment(shipment) : new AllocationUsage());
					}
					else
					{
						lastSavedUsage = new AllocationUsage();
					}
				}

				return lastSavedUsage;
			}
		}
		AllocationUsage lastSavedUsage;

		#endregion

		#region LoadAllocationUsageSet

		public AllocationUsageSet LoadAllocationUsageSet()
		{
			AllocationUsageSet result;

			if (Sailing == null)
			{
				result = null;
			}
			else
			{
				VoyageCountry country = Sailing.Origin.VoyageCountry;
				if (country.J0_AllocationsByPrincipal)
				{
					result = AllocationUsage.LoadRelevantToSailing(Sailing, JS_OH_DeliveryAgent, this);
				}
				else
				{
					result = AllocationUsage.LoadRelevantToSailing(Sailing, ZGuid.Empty, this);
				}
			}

			return result;
		}

		#endregion

		#region BusinessObject Overrides

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Calling getter")]
		protected override void RunPreSaveValidationCore()
		{
			if (JS_ShipmentStatus != ShipmentStatusList.Codes.WebBooking)
			{
				object loaded = Allocation;
			}
			RefCountryRulesHelper.AddRulesToNotes(Origin?.Country, Destination?.Country, Notes, TransportMode, IsInDatabase, true);
			base.RunPreSaveValidationCore();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JS_A_BKD = ZDateTime.Today;
			JS_RL_NKDestination = ZString.Empty;
			JS_RX_NKFrtRateCurrency = Constants.CurrencyCodes.UnitedStates;
			JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			JS_TransportMode = Constants.TransportModes.Sea;
			JS_PackingMode = Constants.ContainerModes.FCL;
			JS_IsShipping = true;
			JS_IsBooking = false;
			JS_IsForwardRegistered = false;
		}

		protected override ZString DefaultWeightUnit
		{
			get { return AgencyRegistry.Instance.DefaultBillWeightUnit.Value; }
		}

		protected override ZString DefaultVolumeUnit
		{
			get { return AgencyRegistry.Instance.DefaultBillVolumeUnit.Value; }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				switch (JS_ShipmentStatus)
				{
					case ShipmentStatusList.Codes.WebFwdInstruction:
						return Res.GetString("988d550b-5441-4b96-aaa4-1822a9ba6f21", "Shipping Forwarding Instruction {0}", JS_UniqueConsignRef);

					case ShipmentStatusList.Codes.Confirmed:
						return Res.GetString("3764dd51-c91d-4ca1-a792-c0aa15a8a924", "Shipping Bill of Lading {0}", JS_UniqueConsignRef);

					case ShipmentStatusList.Codes.Booked:
					case ShipmentStatusList.Codes.WaitListed:
					case ShipmentStatusList.Codes.WebBooking:
						return Res.GetString("bb383754-b08f-4a3d-88b3-ef4f4dc870e0", "Shipping Booking {0}", JS_UniqueConsignRef);

					default:
						return Res.GetString("3fe12fa7-5a77-4e0c-86de-125f4ec640ed", "Shipping Shipment {0}", JS_UniqueConsignRef);
				}
			}
		}

		protected override INumberFountainProxy NumberFountainForUniqueConsignRef
		{
			get { return Env.NumberFountains.JobShipmentNumberAgency; }
		}

		protected override DocAddressType[] SupportedAddressTypes
		{
			get
			{
				List<DocAddressType> addressTypes = new List<DocAddressType>(base.SupportedAddressTypes);
				addressTypes.Add(DocAddressType.BookingPartyDocumentaryAddress);
				addressTypes.Add(DocAddressType.SendingForwarderAddress);
				addressTypes.Add(DocAddressType.ReceivingForwarderAddress);

				return addressTypes.ToArray();
			}
		}

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.BookingPartyDocumentaryAddress:
					return BookingPartyDocAddressRequirement;
				case DocAddressType.SendingForwarderAddress:
					return SendingForwarderAddressRequirement;
				case DocAddressType.ReceivingForwarderAddress:
					return ReceivingForwarderAddressRequirement;
				default:
					return base.GetDocAddressRequirement(addressType);
			}
		}

		public override IBusiness TemplateCopy()
		{
			AgencyShipment shipment = (AgencyShipment)Clone();
			using (shipment.SuspendSettingHasChanges())
			using (shipment.GetValidationSuspender())
			{
				foreach (ZPropertyInfo propertyInfo in shipment.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						propertyInfo.Value = (propertyInfo.Name == Schema.JS_A_BKD) ? ZDateTime.Today : ZDateTime.Empty;
					}
				}

				shipment.JS_NKLoadPort = this.JS_NKLoadPort;
				shipment.JS_NKDischargePort = this.JS_NKDischargePort;
				shipment.JS_JX = this.JS_JX;
				shipment.JS_UniqueConsignRef = ZString.Empty;
				shipment.JS_HouseBill = ZString.Empty;
				shipment.JS_TotalPackageCount = 0;
				shipment.JS_ActualWeight = 0;
				shipment.JS_ActualVolume = 0;
				shipment.JS_BookingReference = "";
				shipment.JS_CFSReference = "";
				shipment.OuterPackLines.RemoveAndDeleteAll();
				shipment.Notes.RemoveAndDeleteAll();
				shipment.CusEntryNumbers.RemoveAndDeleteAll();
			}
			return shipment;
		}

		protected override NumberGenerator.FountainGetterDelegate GetGeneratorFountain
		{
			get { return Env.NumberFountains.GetAgencyGeneratorFountain; }
		}

		protected override NumberGeneratorTarget NewShipmentNumberGeneratorTargetCore()
		{
			return new OceanShipmentNumberGeneratorTarget();
		}

		protected override NumberGeneratorTarget NewBillOfLadingNumberGeneratorTargetCore()
		{
			return new OceanBillOfLadingNumberGeneratorTarget();
		}

		protected override bool GetSuppressBillNumberGenerationCore()
		{
			return false;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (JS_IsCancelled)
			{
				SetReadOnlyIncludingChildren(true);
				OuterPackLines.SetReadOnlyIncludingChildren(true);
				BookedContainers.SetReadOnlyIncludingChildren(true);
				RealContainers.SetReadOnlyIncludingChildren(true);
			}
		}

		protected override void ConsignorDocumentaryAddressChanged()
		{
			base.ConsignorDocumentaryAddressChanged();
			Lookups.ReSetConsignorContacts_List();
			RaiseConsignorDocumentaryAddressChanged();
		}

		protected override void ConsigneeDocumentaryAddressChanged()
		{
			base.ConsigneeDocumentaryAddressChanged();
			Lookups.ReSetConsigneeContacts_List();
			RaiseConsigneeDocumentaryAddressChanged();
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				BookedContainers.RemoveAndDeleteAll();
				RealContainers.RemoveAndDeleteAll();
				WorkflowItems.RemoveAndDeleteAll();
				new GenCustomAddOnRuleAckCollection(this).DeleteAll();
				ObjectFactory.Get<IChildrenDeletionHelper>().DeleteChildren(this);
			}
			base.Delete();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				lastSavedUsage = null;
			}

			JS_NKLoadPortInfo.RefreshBinding();
			JS_NKDischargePortInfo.RefreshBinding();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			LogSailingChange();
			LogOriginDestinationChange();

			if (ShouldLogStatusChangesOnSaving())
			{
				LogStatusChanges();
			}

			RefCountryRulesHelper.AddRulesToNotes(Origin?.Country, Destination?.Country, Notes, TransportMode, IsInDatabase, false);
		}

		protected ZBool ShouldLogStatusChangesOnSaving()
		{
			return !AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value;
		}

		void LogOriginDestinationChange()
		{
			ZString oldOrigin = (ZString)JS_RL_NKOriginInfo.OriginalValue;
			ZString oldDestination = (ZString)JS_RL_NKDestinationInfo.OriginalValue;
			var empty = (ZString)(NoResString)"*empty*"; // Used for Logging

			oldOrigin = oldOrigin.IsEmpty ? empty : oldOrigin;
			oldDestination = oldDestination.IsEmpty ? empty : oldDestination;
			var newOrigin = JS_RL_NKOrigin.IsEmpty ? empty : JS_RL_NKOrigin;
			var newDestination = JS_RL_NKDestination.IsEmpty ? empty : JS_RL_NKDestination;

			bool originChanged = oldOrigin != newOrigin;
			bool destinationChanged = oldDestination != newDestination;

			if (originChanged || destinationChanged)
			{
				if (oldOrigin == empty && oldDestination == empty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "ORIGIN/DESTINATION SET TO {0}/{1}", newOrigin, newDestination));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else
				{
					if (originChanged && destinationChanged)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "ORIGIN/DESTINATION CHANGED FROM {0}/{1} TO {2}/{3}", oldOrigin, oldDestination, newOrigin, newDestination));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
					else if (originChanged)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "ORIGIN CHANGED FROM {0} TO {1}", oldOrigin, newOrigin));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
					else
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "DESTINATION CHANGED FROM {0} TO {1}", oldDestination, newDestination));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (WorkflowDescriptors.Instance.TryGetValueSafe(GetWorkflowType()) != null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}

			if (!IsDeleted && IsInDatabase)
			{
				LogContainerChanges();
				LogDangerousGoodsChanges();
			}
		}

		protected void LogStatusChangedEvent(KeyValuePair<string, string>[] parameters)
		{
			Logs.GetAllLogs();
			var existingLogsNotInDB = Logs.LogsNotInDB.OfType<StmALog>()
				.Where(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && !x.SL_IsCancelled && x.Parameters.TryGetValue(Params.Type, out var type) && type == Core.Constants.EventReferenceMessageTypes.ShipmentStatus);

			var newValue = parameters.FirstOrDefault(parameter => parameter.Key == EventConstants.EventReferenceParameters.Codes.New).Value;
			var reference = new[] { ShipmentStatusList.Codes.ElectronicBooking, ShipmentStatusList.Codes.ElectronicShippingInstruction, ShipmentStatusList.Codes.EBookingCancellationRequest }.Contains(newValue)
				? PurposeDescription
				: ZString.Empty;

			if (existingLogsNotInDB.Any())
			{
				var log = existingLogsNotInDB.FirstOrDefault();

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					if (!reference.IsEmpty)
					{
						log.SL_Reference = reference;
					}

					foreach (var param in parameters)
					{
						log.Parameters[param.Key] = param.Value;
					}

					log.SL_EventTimeOffset = ZDateTimeOffset.Now;
				}
			}
			else
			{
				Logs.AddNew(Events.StatusUpdated, reference, ZDateTimeOffset.Now, parameters);
			}
		}

		void LogStatusChanges()
		{
			if (!IsInDatabase || JS_ShipmentStatusInfo.OriginalValue.ToString() != JS_ShipmentStatus)
			{
				LogStatusChangedEvent(GetParametersForEvent(Events.StatusUpdated).ToArray());
			}
		}

		void LogContainerChanges()
		{
			bool containersAreLoaded = IsBillOfLadingStage ? realContainers != null : bookedContainers != null;
			if (!containersAreLoaded)
			{
				return;
			}

			foreach (var container in ShippingContainers.AddedElements)
			{
				Logs.CreateOrRecreateEventLog(Events.ContainerJobAdded, EstimateActual.Actual, ZDateTimeOffset.Now, GetContainerNumberSafe(container));
			}

			foreach (var container in ShippingContainers.RemovedElements)
			{
				Logs.CreateOrRecreateEventLog(Events.ContainerJobRemoved, EstimateActual.Actual, ZDateTimeOffset.Now, GetContainerNumberSafe(container));
			}
		}

		ZString GetContainerNumberSafe(AgencyShipmentContainer container)
		{
			IBusinessObjectInternals containerInternals = container;
			var rowVersion = container.IsDeleted ? DataRowVersion.Original : DataRowVersion.Default;

			var containerNumber = new ZString(containerInternals.GetValueFromRowSafely(JobContainerSchema.JC_ContainerNum, rowVersion));

			if (containerNumber.IsEmpty)
			{
				var containerTypePK = new ZGuid(containerInternals.GetValueFromRowSafely(JobContainerSchema.JC_RC, rowVersion));

				var containerType = containerTypePK.IsValid ? Factory.Load<RefContainer>(containerTypePK) : null;
				var containerCount = new ZShort(containerInternals.GetValueFromRowSafely(JobContainerSchema.JC_ContainerCount, rowVersion));

				containerNumber = ZString.Format("{0} ({1})", containerType != null ? containerType.RC_Code : ZString.Empty, containerCount);
			}

			return containerNumber;
		}

		void LogDangerousGoodsChanges()
		{
			bool haveDangerousGoodsChanged = false;

			bool containersAreLoaded = IsBillOfLadingStage ? realContainers != null : bookedContainers != null;
			if (containersAreLoaded)
			{
				haveDangerousGoodsChanged = ((ICollectionChangeTrackable<UNDGDataItem>)ShippingContainers).ChangedElements.Any();
			}

			bool packLinesAreLoaded = outerPackLines != null;
			if (!haveDangerousGoodsChanged && packLinesAreLoaded)
			{
				haveDangerousGoodsChanged = ((ICollectionChangeTrackable<UNDGDataItem>)OuterPackLines).ChangedElements.Any();
			}

			if (haveDangerousGoodsChanged)
			{
				Logs.CreateOrRecreateEventLog(Events.DangerousGoodsChanged, EstimateActual.Actual, ZDateTimeOffset.Now);
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new AgencyShipmentFetchStrategy(this);
		}

		protected override ZString GetDefaultChargesDisplay()
		{
			return AgencyRegistry.Instance.AgencyOBLChargesDefaultDisplay.Value;
		}

		#endregion

		#region Related Business Objects

		#region OuterPackLines

		[ChildEditable(false)]
		public new AgencyShipmentPackLineCollection OuterPackLines
		{
			get { return (AgencyShipmentPackLineCollection)base.OuterPackLines; }
		}
		protected override OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			return new AgencyShipmentPackLineCollection(this);
		}

		#endregion

		#region Containers

		[ChildEditableTestExclude]
		public AgencyShipmentContainerDependentCollection ShippingContainers
		{
			get { return IsBillOfLadingStage ? RealContainers : BookedContainers; }
		}

		[ChildEditable(true)]
		public AgencyShipmentContainerDependentCollection BookedContainers
		{
			get
			{
				if (bookedContainers == null)
				{
					bookedContainers = NewContainersCollection(ContainerBookedStatus.Codes.Booked);
					bookedContainers.Load();
					RegisterEditableChildObject(bookedContainers);
				}
				return bookedContainers;
			}
		}
		AgencyShipmentContainerDependentCollection bookedContainers;

		[ChildEditable(true)]
		public AgencyShipmentContainerDependentCollection RealContainers
		{
			get
			{
				if (realContainers == null)
				{
					realContainers = NewContainersCollection(ContainerBookedStatus.Codes.Real);
					realContainers.Load();
					RegisterEditableChildObject(realContainers);
				}
				return realContainers;
			}
		}
		AgencyShipmentContainerDependentCollection realContainers;

		protected virtual AgencyShipmentContainerDependentCollection NewContainersCollection(ZString purpose)
		{
			return new AgencyShipmentContainerDependentCollection(this, purpose);
		}

		public AgencyShipmentContainersView FCLContainers
		{
			get { return fclContainers ?? (fclContainers = GetNewAgencyShipmentContainersView(Core.Constants.ContainerModes.FCL, RealContainers)); }
		}

		AgencyShipmentContainersView fclContainers;

		public AgencyShipmentContainersView FCLBookedContainers
		{
			get { return fclBookedContainers ?? (fclBookedContainers = GetNewAgencyShipmentContainersView(Core.Constants.ContainerModes.FCL, BookedContainers)); }
		}
		AgencyShipmentContainersView fclBookedContainers;

		public AgencyShipmentContainersView Vehicles
		{
			get { return vehicles ?? (vehicles = GetNewAgencyShipmentContainersView(Core.Constants.ContainerModes.RollOnRollOff, ShippingContainers)); }
		}
		AgencyShipmentContainersView vehicles;

		public AgencyShipmentContainersView TopLevelPacks
		{
			get { return topLevelPacks ?? (topLevelPacks = GetNewAgencyShipmentContainersView(AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes, ShippingContainers)); }
		}
		AgencyShipmentContainersView topLevelPacks;

		AgencyShipmentContainersView GetNewAgencyShipmentContainersView(string cargoType, AgencyShipmentContainerDependentCollection containerCollection)
		{
			var containerModes = AgencyShipmentContainerModeList.GetContainerModes(cargoType);
			return new AgencyShipmentContainersView(containerCollection, CreateViewFilter(containerModes));
		}

		AgencyShipmentContainersView GetNewAgencyShipmentContainersView(IEnumerable<ZString> cargoTypes, AgencyShipmentContainerDependentCollection containerCollection)
		{
			var containerModes = cargoTypes.SelectMany(x => AgencyShipmentContainerModeList.GetContainerModes(x)).Distinct();
			return new AgencyShipmentContainersView(containerCollection, CreateViewFilter(containerModes));
		}

		Func<AgencyShipmentContainer, bool> CreateViewFilter(IEnumerable<string> containerModesForCargoType)
		{
			return (container) => containerModesForCargoType.Any(mode => mode == container.JC_ContainerMode);
		}

		[ChildEditable]
		public BusinessObjectCollection Cargo
		{
			get
			{
				if (IsTopLevelPacksMode)
				{
					return topLevelPackLineCollectionAdapter ?? (topLevelPackLineCollectionAdapter = new AgencyShipmentPackLineCollectionAdapter(TopLevelPacks));
				}

				return OuterPackLines;
			}
		}
		AgencyShipmentPackLineCollectionAdapter topLevelPackLineCollectionAdapter;

		#endregion

		#region Principal

		public virtual OrgHeader Principal
		{
			get { return base.DeliveryAgent; }
		}

		#endregion

		#region BookingPartyDocumentaryAddress

		public OrgHeader BookingParty
		{
			get { return BookingPartyDocumentaryAddress.Organisation; }
		}

		public JobDocAddress BookingPartyDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(bookingPartyDocumentaryAddress))
				{
					bookingPartyDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(BookingPartyDocAddressRequirement);
				}
				return bookingPartyDocumentaryAddress;
			}
		}
		JobDocAddress bookingPartyDocumentaryAddress;

		JobDocAddressRequirement BookingPartyDocAddressRequirement
		{
			get
			{
				if (bookingPartyDocAddressRequirement == null)
				{
					bookingPartyDocAddressRequirement = GetBookingPartyDocAddressRequirement();
				}

				return bookingPartyDocAddressRequirement;
			}
		}
		JobDocAddressRequirement bookingPartyDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetBookingPartyDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress, ContactType.Consignor);
		}

		#endregion

		#region ReceivingForwarderAddress

		public OrgHeader ReceivingForwarder
		{
			get { return ReceivingForwarderAddress.Organisation; }
		}

		public JobDocAddress ReceivingForwarderAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(receivingForwarderAddress))
				{
					this.receivingForwarderAddress = DocAddresses.FindOrCreateWithRequirement(ReceivingForwarderAddressRequirement);
				}

				return this.receivingForwarderAddress;
			}
		}
		JobDocAddress receivingForwarderAddress;

		JobDocAddressRequirement ReceivingForwarderAddressRequirement
		{
			get
			{
				if (this.receivingForwarderAddressRequirement == null)
				{
					this.receivingForwarderAddressRequirement = GetReceivingForwarderAddressRequirement();
				}

				return this.receivingForwarderAddressRequirement;
			}
		}
		JobDocAddressRequirement receivingForwarderAddressRequirement;

		protected virtual JobDocAddressRequirement GetReceivingForwarderAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.ReceivingForwarderAddress, ContactType.FreightAgent);

			return requirement;
		}

		#endregion

		#region SendingForwarderAddress

		public OrgHeader SendingForwarder
		{
			get { return SendingForwarderAddress.Organisation; }
		}

		public JobDocAddress SendingForwarderAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(sendingForwarderAddress))
				{
					this.sendingForwarderAddress = DocAddresses.FindOrCreateWithRequirement(SendingForwarderAddressRequirement);
				}

				return this.sendingForwarderAddress;
			}
		}
		JobDocAddress sendingForwarderAddress;

		JobDocAddressRequirement SendingForwarderAddressRequirement
		{
			get
			{
				if (this.sendingForwarderAddressRequirement == null)
				{
					this.sendingForwarderAddressRequirement = GetSendingForwarderAddressRequirement();
				}

				return this.sendingForwarderAddressRequirement;
			}
		}
		JobDocAddressRequirement sendingForwarderAddressRequirement;

		protected virtual JobDocAddressRequirement GetSendingForwarderAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.SendingForwarderAddress, ContactType.FreightAgent);

			return requirement;
		}

		#endregion

		protected override JobDocAddress GetNewConsignorPickupAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(ConsignorDocAddressRequirement);
		}

		protected override JobDocAddressRequirement GetConsignorDocAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, AddressType.OFC, ContactType.Consignor);
			requirement.ValidateOrganisationPK = Validation.ValidateConsignorPK;

			return requirement;
		}

		protected override JobDocAddress GetNewConsigneeDeliveryAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(ConsigneeDocAddressRequirement);
		}

		protected override JobDocAddressRequirement GetConsigneeDocAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.ConsigneeDocumentaryAddress, AddressType.OFC, ContactType.Consignee);
			requirement.ValidateOrganisationPK = Validation.ValidateConsigneePK;

			return RelaxOverriddenAddressValidation(requirement);
		}

		protected override JobDocAddressRequirement GetNotifyPartyDocAddressRequirement()
		{
			return RelaxOverriddenAddressValidation(base.GetNotifyPartyDocAddressRequirement());
		}

		protected override JobDocAddressRequirement GetNotifyParty2DocAddressRequirement()
		{
			return RelaxOverriddenAddressValidation(base.GetNotifyParty2DocAddressRequirement());
		}

		protected override JobDocAddressRequirement GetNotifyParty3DocAddressRequirement()
		{
			return RelaxOverriddenAddressValidation(base.GetNotifyParty3DocAddressRequirement());
		}

		#region Allocation

		public AllocationCalcWrapper Allocation
		{
			get
			{
				if (allocation == null)
				{
					allocation = new AllocationCalcWrapper(this);
					RegisterEditableChildObject(allocation);
				}
				return allocation;
			}
		}
		AllocationCalcWrapper allocation;

		#endregion

		#region Sailings

		public virtual JobSailingCollection Sailings
		{
			get
			{
				if (sailings == null)
				{
					sailings = new JobSailingCollection(Factory);
					if (!JS_JX.IsEmpty)
					{
						sailings.AddFromDatabase(JS_JX);
						UpdateLoadDischargeFromSailing();
						RefreshVoyageVesselForBinding();
					}
				}
				return sailings;
			}
		}
		JobSailingCollection sailings;

		#endregion

		#region Sailing

		public override JobSailing Sailing
		{
			get
			{
				JobSailing result = null;
				if (Sailings.Count == 1)
				{
					result = Sailings[0];
				}
				else
				{
					result = Factory.Load<JobSailing>(JS_JX);
				}
				return result;
			}
		}

		#endregion

		#region VoyageVesselForBinding

		public ZString VoyageVesselForBinding => Sailing?.JX_JV_NKVessel ?? ZString.Empty;

		public ZPropertyInfo VoyageVesselForBindingInfo => GetZPropertyInfo(nameof(VoyageVesselForBinding));

		#endregion

		void RefreshVoyageVesselForBinding()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateVoyageVesselForBinding();
			}

			VoyageVesselForBindingInfo.RefreshBinding();
		}

		#endregion

		#region Disabled Relationships

		[Browsable(false)]
		[Bindable(false)]
		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		[ActionFieldFollow(false)]
		public override OrgHeader DeliveryAgent
		{
			get { return null; }
		}

		[Browsable(false)]
		[Bindable(false)]
		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		[Obsolete("This collection is invalid for agency, there is no replacement as agency does not use consols", true)]
		[ActionFieldFollow(false)]
		public new ConsolCollection Consols
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Consols; }
		}

		[Browsable(false)]
		[Bindable(false)]
		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		[Obsolete("This property is invalid for agency, there is no replacement as agency does not use consols", true)]
		[ActionFieldFollow(false)]
		public new CommonConsol ArrivalConsol
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ArrivalConsol; }
		}

		[Browsable(false)]
		[Bindable(false)]
		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		[Obsolete("This property is invalid for agency, there is no replacement as agency does not use consols", true)]
		[ActionFieldFollow(false)]
		public new CommonConsol DepartureConsol
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.DepartureConsol; }
		}

		[Browsable(false)]
		[Bindable(false)]
		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		[Obsolete("This collection is invalid for agency, there is no replacement as agency does not use inner packlines", true)]
		[ActionFieldFollow(false)]
		public new InnerPackLineCollection InnerPackLines
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.InnerPackLines; }
		}

		[Browsable(false)]
		[Bindable(false)]
		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		[Obsolete("This property is invalid for agency, there is no replacement as agency does not coload masters", true)]
		[ActionFieldFollow(false)]
		public new CommonShipment CoLoadMasterShipment
		{
			get { return null; }
		}

		#endregion

		#region Un-Bound Properties

		public bool ShouldEnforceAllocations
		{
			get
			{
				return Sailing != null
					&& Sailing.IsInDatabase
					&& (ImportExportHelper.IsBranchCountry(Sailing.Origin.JA_RL_NKPortOfLoading) || FreightConfigurationRegistry.Instance.UseGlobalAllocations.Value)
					&& Sailing.Origin.VoyageCountry.J0_AllocationMethod != AllocationMethodList.Codes.Ignore
					&& ShipmentStatusHelperMethods.CountsTowardsAllocations(JS_ShipmentStatus);
			}
		}

		public bool HasHazardous
		{
			get { return HasHazardousContainers() || HasHazardousPackLines(); }
		}

		bool HasHazardousContainers()
		{
			return ShippingContainers.Cast<AgencyShipmentContainer>().Any(container => container.HasHazardous);
		}

		bool HasHazardousPackLines()
		{
			return OuterPackLines.Cast<PackLine>().Any(packLine => packLine.HasHazardous);
		}

		public ZString HazardousStatus
		{
			get
			{
				if (hazardousStatus == null)
				{
					hazardousStatus = new CachedProperty<ZString>(Factory, CalculateHazardousStatus);
				}
				return hazardousStatus.Value;
			}
		}
		CachedProperty<ZString> hazardousStatus;

		ZString CalculateHazardousStatus()
		{
			bool hasHazardous = false;
			bool hasNonHazardous = false;

			foreach (AgencyShipmentContainer container in ShippingContainers)
			{
				if (container.HasHazardous)
				{
					if (hasNonHazardous)
					{
						return HazardousStatusValues.Mixed;
					}
					else
					{
						hasHazardous = true;
					}
				}
				else
				{
					if (hasHazardous)
					{
						return HazardousStatusValues.Mixed;
					}
					else
					{
						hasNonHazardous = true;
					}
				}
			}

			foreach (PackLine packLine in OuterPackLines)
			{
				if (packLine.HasHazardous)
				{
					if (hasNonHazardous)
					{
						return HazardousStatusValues.Mixed;
					}
					else
					{
						hasHazardous = true;
					}
				}
				else
				{
					if (hasHazardous)
					{
						return HazardousStatusValues.Mixed;
					}
					else
					{
						hasNonHazardous = true;
					}
				}
			}

			return hasHazardous ? HazardousStatusValues.Hazardous : HazardousStatusValues.NonHazardous;
		}

		public override ZBool HasETDPassed
		{
			get { return (Sailing != null && !Sailing.JX_JA_E_DEP.IsEmpty && ZDateTime.Now > Sailing.JX_JA_E_DEP); }
		}

		public ZBool IsCompanyRegisteredForGST
		{
			get { return GlbCompany.CurrentCompany.GC_IsGSTRegistered; }
		}

		#endregion

		#region HouseBill Defaulting overrides

		protected override bool NeedsHouseBill
		{
			get
			{
				var registryValue = ShipmentStatusHelperMethods.IsBillOfLadingStage(JS_ShipmentStatus)
						? AgencyRegistry.Instance.AlwaysGenerateBillOfLadingNumbers.Value
						: AgencyRegistry.Instance.AlwaysGenerateBookingNumbers.Value;

				return registryValue || base.NeedsHouseBill;
			}
		}

		#endregion

		#region Bound Properties

		public override ZDateTime JS_E_DEP
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_E_DEP; }
			set
			{
				if (base.JS_E_DEP != value)
				{
					base.JS_E_DEP = value;
				}
				if (!ignoreGetNextSailingOrClearSailings && !InAddDateEvent)
				{
					GetNextSailingOrClearSailings();
				}
			}
		}

		public override ZDateTime JS_E_ARV
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_E_ARV; }
			set
			{
				if (base.JS_E_ARV != value)
				{
					base.JS_E_ARV = value;
				}
			}
		}

		[ReadOnly(true)]
		public override ZDateTime JS_A_BKD
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_A_BKD; }
			set
			{
				base.JS_A_BKD = value;
				GetNextSailingOrClearSailings();
			}
		}

		public override ZDateTime JS_Calc_LastETA
		{
			get
			{
				JobSailing sailing = this.Sailing;
				return (sailing == null) ? ZDateTime.Empty : sailing.JX_JB_E_ARV;
			}
		}

		public override ZDecimal JS_Calc_TEUCount
		{
			get
			{
				ZDecimal result = 0;
				AgencyShipmentContainerDependentCollection containers = IsBillOfLadingStage ? RealContainers : BookedContainers;
				foreach (AgencyShipmentContainer container in containers)
				{
					result += container.JC_Calc_TEUCount;
				}
				return result;
			}
		}

		public override ZInt JS_Calc_ContainerCount
		{
			get
			{
				ZInt result = 0;
				AgencyShipmentContainerDependentCollection containers = IsBillOfLadingStage ? RealContainers : BookedContainers;
				foreach (AgencyShipmentContainer container in containers)
				{
					result += container.JC_Calc_ContainerCount;
				}
				return result;
			}
		}

		public override ZInt JS_Calc_20GPCount
		{
			get
			{
				ZInt result = 0;
				AgencyShipmentContainerDependentCollection containers = IsBillOfLadingStage ? RealContainers : BookedContainers;
				foreach (AgencyShipmentContainer container in containers)
				{
					if (container.JC_Is20GP)
					{
						result += container.JC_Calc_ContainerCount;
					}
				}
				return result;
			}
		}

		public override ZInt JS_Calc_40GPCount
		{
			get
			{
				ZInt result = 0;
				AgencyShipmentContainerDependentCollection containers = IsBillOfLadingStage ? RealContainers : BookedContainers;
				foreach (AgencyShipmentContainer container in containers)
				{
					if (container.JC_Is40GP)
					{
						result += container.JC_Calc_ContainerCount;
					}
				}
				return result;
			}
		}

		public override ZInt JS_Calc_20RECount
		{
			get
			{
				ZInt result = 0;
				AgencyShipmentContainerDependentCollection containers = IsBillOfLadingStage ? RealContainers : BookedContainers;
				foreach (AgencyShipmentContainer container in containers)
				{
					if (container.JC_Is20RE)
					{
						result += container.JC_Calc_ContainerCount;
					}
				}
				return result;
			}
		}

		public override ZInt JS_Calc_40RECount
		{
			get
			{
				ZInt result = 0;
				AgencyShipmentContainerDependentCollection containers = IsBillOfLadingStage ? RealContainers : BookedContainers;
				foreach (AgencyShipmentContainer container in containers)
				{
					if (container.JC_Is40RE)
					{
						result += container.JC_Calc_ContainerCount;
					}
				}
				return result;
			}
		}

		public override ZInt JS_Calc_OtherContainerCount
		{
			get
			{
				ZInt result = 0;
				AgencyShipmentContainerDependentCollection containers = IsBillOfLadingStage ? RealContainers : BookedContainers;
				foreach (AgencyShipmentContainer container in containers)
				{
					if (container.JC_IsOtherContainerType)
					{
						result += container.JC_Calc_ContainerCount;
					}
				}
				return result;
			}
		}

		#region JS_PackingMode

		public bool IsFCL
		{
			get { return JS_PackingMode == Constants.ContainerModes.FCL || JS_PackingMode.IsEmpty; }
		}

		public bool IsRollOnRollOff
		{
			get { return JS_PackingMode == Constants.ContainerModes.RollOnRollOff; }
		}

		public bool IsTopLevelPacksMode
		{
			get { return AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes.Contains(JS_PackingMode); }
		}

		public override ZString JS_PackingMode
		{
			get { return base.JS_PackingMode; }
			set
			{
				ZString previousPackingMode = base.JS_PackingMode;
				if (previousPackingMode != value)
				{
					var cleanupActions = new List<Action>();
					if (CanContinueChangingPackingMode(previousPackingMode, cleanupActions))
					{
						cleanupActions.ForEach(action => action());
						OuterPackLines.MarkAsNeedingValidation();
						base.JS_PackingMode = value;
					}
				}
			}
		}

		string GetCargoDescriptionFromPackingMode(ZString cargoType, bool booked)
		{
			switch (cargoType)
			{
				case Constants.ContainerModes.RollOnRollOff:
					return Res.GetString("2c95b678-980b-4fe2-8a37-4b27b68772d7", "Vehicles");

				case Constants.ContainerModes.BreakBulk:
				case Constants.ContainerModes.Bulk:
				case Constants.ContainerModes.Liquid:
					return Res.GetString("35c8ffdb-c886-4ca8-9d1a-c216e453ab31", "Packs");

				default:
					return booked
						? Res.GetString("846e1159-7aa8-48de-ab02-805f99f900c8", "Booked Containers")
						: Res.GetString("47753edd-5b53-4e4c-80d4-c9df538835c1", "Containers");
			}
		}

		public event EventHandler<CancelEventArgsWithMessage> PackingModeChanging;

		bool CanContinueChangingPackingMode(ZString oldPackingMode, List<Action> cleanupActions)
		{
			bool result = true;

			ZStringBuilder builder = new ZStringBuilder();

			if (BookedContainers.Any())
			{
				builder.AppendLine(GetCargoDescriptionFromPackingMode(oldPackingMode, true));
				cleanupActions.Add(() => BookedContainers.RemoveAndDeleteAll());
			}

			if (RealContainers.Any())
			{
				builder.AppendLine(GetCargoDescriptionFromPackingMode(oldPackingMode, false));
				cleanupActions.Add(() => RealContainers.RemoveAndDeleteAll());
			}

			if (OuterPackLines.Any())
			{
				builder.AppendLine(Res.GetString("35c8ffdb-c886-4ca8-9d1a-c216e453ab31", "Packs"));
				cleanupActions.Add(() => OuterPackLines.RemoveAndDeleteAll());
			}

			OuterPackLines.MarkAsNeedingValidation();

			var packingModeChangeHandler = PackingModeChanging;
			if (packingModeChangeHandler != null && !builder.IsEmpty)
			{
				builder.Prepend(Res.GetString("e1e5c99d-cc8c-4ae2-b6ae-5c1c6c7b7c8f", "Changing cargo type will remove all data from:") + System.Environment.NewLine);
				builder.AppendLine();
				builder.AppendLine(Res.GetString("b82f7d3a-8d74-4e11-840b-b7e20dd01b23", "Are you sure you want to continue?"));

				var args = new CancelEventArgsWithMessage(Res.GetString("eb42f678-9af8-489d-ab08-12fca08af88c", "Confirm Cargo Type Change"), builder.ToString());
				packingModeChangeHandler(this, args);
				result = !args.Cancel;
			}

			return result;
		}

		#endregion

		protected override IEnumerable<IPackLineParentChangeNotifiable> PackLinesNotifiablesCore
		{
			get
			{
				if (IsTopLevelPacksMode)
				{
					yield return TopLevelPacks;
				}
				else
				{
					yield return OuterPackLines;
				}
			}
		}

		public override ZInt JS_OuterPacks
		{
			get { return base.JS_OuterPacks; }
			set
			{
				ZInt previousValue = base.JS_OuterPacks;
				if (previousValue != value)
				{
					base.JS_OuterPacks = value;

					if (UpdatePackLines && IsTopLevelPacksMode)
					{
						TopLevelPacks.NotifyPackageCountChanged(previousValue, value);
					}
				}
			}
		}

		public override ZString JS_F3_NKPackType
		{
			get { return base.JS_F3_NKPackType; }
			set
			{
				ZString previousValue = base.JS_F3_NKPackType;
				if (previousValue != value)
				{
					base.JS_F3_NKPackType = value;

					if (UpdatePackLines && IsTopLevelPacksMode)
					{
						TopLevelPacks.NotifyPackageTypeChanged(previousValue, value);
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsInDatabase))]
		[RelatedBusinessObject("CalcLoadPort")]
		[List("Lookups.RefUNLOCO_List")]
		[MaxLength(CommonShipment.Schema.JS_RL_NKOriginMaxLength)]
		public virtual ZString JS_NKLoadPort
		{
			get
			{
				if (!loadAndDischargeUpdatedFromSailing)
				{
					UpdateLoadDischargeFromSailing();
				}

				return (!loadPort.IsEmpty) ? loadPort : JS_Calc_CurrentLoadPort;
			}
			set
			{
				CheckMaximumLength(JS_NKLoadPortInfo, value);
				loadPort = value;
				if (!fIsImportingData)
				{
					GetNextSailingOrClearSailings();
					DefaultOriginFromLoadPort();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_NKLoadPort();
				}
				JS_NKLoadPortInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo JS_NKLoadPortInfo
		{
			get { return GetZPropertyInfo(Schema.JS_NKLoadPort); }
		}
		ZString loadPort;

		public RefUNLOCO CalcLoadPort
		{
			get { return !JS_NKLoadPort.IsEmpty ? Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JS_NKLoadPort) : null; }
		}

		[ReadOnlyMember(nameof(IsInDatabase))]
		[RelatedBusinessObject("CalcDischargePort")]
		[List("Lookups.RefUNLOCO_List")]
		[MaxLength(CommonShipment.Schema.JS_RL_NKDestinationMaxLength)]
		public ZString JS_NKDischargePort
		{
			get
			{
				if (!loadAndDischargeUpdatedFromSailing)
				{
					UpdateLoadDischargeFromSailing();
				}

				return (!dischargePort.IsEmpty) ? dischargePort : JS_Calc_CurrentDischargePort;
			}
			set
			{
				CheckMaximumLength(JS_NKDischargePortInfo, value);
				dischargePort = value;

				if (!fIsImportingData)
				{
					GetNextSailingOrClearSailings();
					DefaultDestinationFromDischargePort();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_NKDischargePort();
				}
				JS_NKDischargePortInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JS_NKDischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.JS_NKDischargePort); }
		}
		ZString dischargePort;

		public RefUNLOCO CalcDischargePort
		{
			get { return !JS_NKLoadPort.IsEmpty ? Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JS_NKDischargePort) : null; }
		}

		[ReadOnlyMember(nameof(HasPostedCharges))]
		[List("Lookups.Principal_List")]
		public override ZGuid JS_OH_DeliveryAgent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_OH_DeliveryAgent; }
			set
			{
				if (base.JS_OH_DeliveryAgent != value)
				{
					base.JS_OH_DeliveryAgent = value;

					SendingAgentAddressPKInfo.RefreshBinding();
					ReceivingAgentAddressPKInfo.RefreshBinding();
				}
			}
		}

		public override ZPropertyInfo JS_OH_DeliveryAgentInfo
		{
			get
			{
				ZPropertyInfo info = base.JS_OH_DeliveryAgentInfo;
				info.HumanReadableName = Res.GetString("7d27a77c-d5d7-4951-97c6-0597758400ca", "Principal");
				return info;
			}
		}
		public bool HasPostedCharges
		{
			get
			{
				if (Job != null)
				{
					JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, Job.PK));
					foreach (JobCharge charge in charges)
					{
						if (charge.IsCostPosted || charge.IsRevenuePosted)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		protected bool JS_OA_BookedShippingLineAddress_ReadOnly
		{
			get { return !JS_JX.IsEmpty; }
		}

		public override ZGuid JS_JX
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_JX; }
			set
			{
				IsSettingSailing = true;

				if (!Transports.IsLoaded)
				{
					Transports.Load();
				}

				if (Sailings.Contains(JS_JX))
				{
					Sailings.Remove(JS_JX);
				}
				base.JS_JX = value;

				JobSailing sailing = Factory.Load<JobSailing>(JS_JX);
				if (sailing != null)
				{
					Sailings.Add(sailing);
					UpdateLoadDischargeFromSailing();
					DefaultDestinationFromDischargePort();
					DefaultOriginFromLoadPort();
					DefaultEstimatedDatesFromSailing(sailing);
				}

				DefaultFromSailing();

				RefreshContainerMovements();

				IsSettingSailing = false;
			}
		}

		bool IsSettingSailing { get; set; }

		void LogSailingChange()
		{
			var oldSailing = Factory.Load<JobSailing>(JS_JX_LastSavedSailing);
			var newSailing = Factory.Load<JobSailing>(JS_JX);

			if (oldSailing != newSailing)
			{
				if (oldSailing != null && newSailing != null)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "SAILING {0} REPLACED WITH {1}", SailingDescription(oldSailing), SailingDescription(newSailing)));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else if (oldSailing == null && newSailing != null)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "SAILING {0} ADDED", SailingDescription(newSailing)));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else if (oldSailing != null)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "SAILING {0} REMOVED", SailingDescription(oldSailing)));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		string SailingDescription(JobSailing sailing)
		{
			return string.Format(CultureInfo.InvariantCulture, @"""{0} - {1}""", sailing.JX_JV_NKVessel, sailing.JX_JV_VoyageFlight);
		}

		public void RefreshContainerMovements()
		{
			if (realContainers != null)
			{
				realContainers.RefreshContainerMovements();
			}

			if (bookedContainers != null)
			{
				bookedContainers.RefreshContainerMovements();
			}
		}

		public override ZString JS_HouseBill
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_HouseBill; }
			set
			{
				base.JS_HouseBill = value;

				if (JS_CFSReference.IsEmpty && NeedsHouseBill)
				{
					JS_CFSReference = JS_HouseBill;
				}
			}
		}

		[ReadOnly(true)]
		public override ZString JS_TransportMode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_TransportMode; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.JS_TransportMode = value; }
		}

		[List("Lookups.JS_ShipmentStatus_List")]
		public override ZString JS_ShipmentStatus
		{
			get { return base.JS_ShipmentStatus; }
			set
			{
				base.JS_ShipmentStatus = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_OH_DeliveryAgent();
				}
			}
		}

		[MaxLength(3)]
		public ZString BookingPartyFieldType
		{
			get
			{
				return BookingPartyDocumentaryAddress.E2_AddressOverride ?

					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}
		public ZPropertyInfo BookingPartyFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(BookingPartyFieldType)); }
		}

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("BookingParty")]
		[List("Lookups.OrgHeader_List")]
		public ZString BookingPartyNameOrPK
		{
			get
			{
				ZString result = "";

				if (BookingPartyDocumentaryAddress.E2_AddressOverride)
				{
					result = BookingPartyDocumentaryAddress.E2_CompanyNameTruncated;
				}
				else if (BookingPartyDocumentaryAddress.Organisation != null)
				{
					result = BookingPartyDocumentaryAddress.Organisation.PK.ToString();
				}
				else
				{
					result = ZGuid.Empty.ToString();
				}

				return result;
			}
			set
			{
				if (BookingPartyDocumentaryAddress.E2_AddressOverride)
				{
					BookingPartyDocumentaryAddress.E2_CompanyName = value;
				}
				else
				{
					try
					{
						BookingPartyDocumentaryAddress.OrganisationPK = new Guid(value);
					}
					catch (FormatException)
					{
					}
				}
				BookingPartyNameOrPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BookingPartyNameOrPKInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.BookingPartyNameOrPK);
			}
		}

		public int BookingPartyNameOrPK_MaxLength
		{
			get { return BookingPartyDocumentaryAddress.E2_AddressOverride ? JobDocAddressSchema.E2_CompanyName.MaxLength : 36; } // where 36 = AnyGuid.ToString().Length
		}

		protected override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			return new AgencyShipmentCustomsEntryNumber(this);
		}

		public override ZString JS_RL_NKOrigin
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_RL_NKOrigin; }
			set
			{
				if (base.JS_RL_NKOrigin != value)
				{
					base.JS_RL_NKOrigin = value;
					SendingAgentAddressPKInfo.RefreshBinding();
				}
			}
		}

		public override ZString JS_RL_NKDestination
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JS_RL_NKDestination; }
			set
			{
				if (base.JS_RL_NKDestination != value)
				{
					base.JS_RL_NKDestination = value;
					ReceivingAgentAddressPKInfo.RefreshBinding();
				}
			}
		}

		#region Sending / Receiving Agents

		#region SendingAgentAddress

		[ReadOnly(true)]
		[RelatedBusinessObject("SendingAgentAddress")]
		[List("BindToLists.OrgAddress_List")]
		[ResourceStringData("JobShipment|SendingAgentAddressPK", Caption = "Sending Agent", FullDescription = "Sending Agent based on Principal and Origin port of the Shipment.")]
		public ZGuid SendingAgentAddressPK
		{
			get
			{
				if (sendingAgentAddressPK == null)
				{
					sendingAgentAddressPK = new CachedProperty<ZGuid>(Factory, delegate
					{
						ZGuid result = GetAgentAddress(Principal, JS_RL_NKOrigin);

						if (result.IsEmpty)
						{
							SendingAgentAddressPK_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
						}

						return result;
					});
				}
				return sendingAgentAddressPK.Value;
			}
		}
		CachedProperty<ZGuid> sendingAgentAddressPK;

		public ZPropertyInfo SendingAgentAddressPKInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(SendingAgentAddressPK)); }
		}

		public OrgAddress SendingAgentAddress
		{
			get { return Factory.Load<OrgAddress>(SendingAgentAddressPK); }
		}

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress SendingAgentAddressPK_ZAddress
		{
			get { return sendingAgentAddressPK_ZAddress ?? (sendingAgentAddressPK_ZAddress = new ZAddress(SendingAgentAddressPKInfo)); }
		}
		ZAddress sendingAgentAddressPK_ZAddress;

		#endregion

		#region ReceivingAgentAddress

		[ReadOnly(true)]
		[RelatedBusinessObject("ReceivingAgentAddress")]
		[List("BindToLists.OrgAddress_List")]
		[ResourceStringData("JobShipment|ReceivingAgentAddressPK", Caption = "Receiving Agent", FullDescription = "Receiving Agent based on Principal and Destination port of the Shipment.")]
		public ZGuid ReceivingAgentAddressPK
		{
			get
			{
				if (receivingAgentAddressPK == null)
				{
					receivingAgentAddressPK = new CachedProperty<ZGuid>(Factory, delegate
					{
						ZGuid result = GetAgentAddress(Principal, JS_RL_NKDestination);

						if (result.IsEmpty)
						{
							ReceivingAgentAddressPK_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
						}

						return result;
					});
				}

				return receivingAgentAddressPK.Value;
			}
		}
		CachedProperty<ZGuid> receivingAgentAddressPK;

		public ZPropertyInfo ReceivingAgentAddressPKInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ReceivingAgentAddressPK)); }
		}

		public OrgAddress ReceivingAgentAddress
		{
			get { return Factory.Load<OrgAddress>(ReceivingAgentAddressPK); }
		}

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress ReceivingAgentAddressPK_ZAddress
		{
			get { return receivingAgentAddressPK_ZAddress ?? (receivingAgentAddressPK_ZAddress = new ZAddress(ReceivingAgentAddressPKInfo)); }
		}
		ZAddress receivingAgentAddressPK_ZAddress;

		#endregion

		ZGuid GetAgentAddress(OrgHeader header, ZString port)
		{
			OrgAddress address = null;
			if (header != null && !port.IsEmpty)
			{
				address = header.CarrierAppointedAgentPorts_Agency.FindAddress(port);
			}

			return address != null ? address.PK : ZGuid.Empty;
		}

		#endregion

		#endregion

		#region Top Level Packs Support

		protected override bool ShipmentTotalsDiffer
		{
			get { return IsTopLevelPacksMode ? IsTopLevelPackTotalsDifferent() : base.ShipmentTotalsDiffer; }
		}

		protected override void UpdateShipmentTotals()
		{
			if (IsTopLevelPacksMode)
			{
				UpdateTotalsFromTopLevelPacks();
			}
			else
			{
				base.UpdateShipmentTotals();
			}
		}

		bool IsTopLevelPackTotalsDifferent()
		{
			return JS_OuterPacks != TopLevelPacks.TotalContainers
				|| Utilities.Round(TopLevelPacks.TotalWeightInShipmentWeightUnit, 3) != Utilities.Round(JS_ActualWeightReadOnly, 3)
				|| Utilities.Round(TopLevelPacks.TotalVolumeInShipmentVolumeUnit, 3) != Utilities.Round(JS_ActualVolumeReadOnly, 3);
		}

		void UpdateTotalsFromTopLevelPacks()
		{
			JS_OuterPacks = TopLevelPacks.TotalContainers;
			JS_F3_NKPackType = TopLevelPacks.TotalPackagesUnit;

			ZDecimal weight = TopLevelPacks.TotalWeightInShipmentWeightUnit;
			ZString weightUnit = FreightUtilities.IsValidWeightUnit(JS_UnitOfWeight) ? JS_UnitOfWeight : RegistryWeightUnit;

			ZDecimal volume = TopLevelPacks.TotalVolumeInShipmentVolumeUnit;
			ZString volumeUnit = FreightUtilities.IsValidVolumeUnit(JS_UnitOfVolume) ? JS_UnitOfVolume : RegistryVolumeUnit;

			new WeightConversionStrategy().ReScale(ref weight, ref weightUnit, 9, 3);
			new VolumeConversionStrategy().ReScale(ref volume, ref volumeUnit, 9, 3);

			JS_ActualWeight = weight;
			JS_UnitOfWeight = weightUnit;
			JS_ActualVolume = volume;
			JS_UnitOfVolume = volumeUnit;
		}

		#region TopLevelPacks Totals

		public ZInt TopLevelPacksTotalPacks
		{
			get { return TopLevelPacks.TotalContainers; }
		}

		public ZPropertyInfo TopLevelPacksTotalPacksInfo
		{
			get { return GetZPropertyInfo(nameof(TopLevelPacksTotalPacks)); }
		}

		public ZDecimal TopLevelPacksTotalVolumeInShipmentVolumeUnit
		{
			get { return this.GetRoundedValue(TopLevelPacksTotalVolumeInShipmentVolumeUnitInfo, TopLevelPacks.TotalVolumeInShipmentVolumeUnit); }
		}

		public ZPropertyInfo TopLevelPacksTotalVolumeInShipmentVolumeUnitInfo
		{
			get { return GetZPropertyInfo(nameof(TopLevelPacksTotalVolumeInShipmentVolumeUnit)); }
		}

		public ZDecimal TopLevelPacksTotalWeightInShipmentWeightUnit
		{
			get { return this.GetRoundedValue(TopLevelPacksTotalWeightInShipmentWeightUnitInfo, TopLevelPacks.TotalWeightInShipmentWeightUnit); }
		}

		public ZPropertyInfo TopLevelPacksTotalWeightInShipmentWeightUnitInfo
		{
			get { return GetZPropertyInfo(nameof(TopLevelPacksTotalWeightInShipmentWeightUnit)); }
		}

		public ZString TopLevelPacksPackagesUnit
		{
			get { return TopLevelPacks.TotalPackagesUnit; }
		}

		#endregion

		#endregion

		#region ReleaseType Defaulting Overrides

		protected override ZString DefaultReleaseType
		{
			get { return AgencyRegistry.Instance.ReleaseTypeDefault.Value; }
		}

		protected override void DefaultNumberOfBillsByReleaseType(ZString releaseType)
		{
			if (this.BuyerSupplierLinksHelper != null)
			{
				this.BuyerSupplierLinksHelper.RestoreNumberOfBills();
			}
			else
			{
				DefaultNumberOfBillsWithFallback(releaseType);
			}
		}

		protected override void DefaultNumberOfBillsWithFallback(ZString releaseType)
		{
			if (!IsCopying && !fIsImportingData)
			{
				if (!IsSettingDefaultValues
					&& releaseType != Core.Constants.ShipmentReleaseTypes.ExpressBofL
					&& Consignee != null && Consignee.MiscServ != null
					&& (Consignee.MiscServ.OM_IMOriginalSeaBills > 0 || Consignee.MiscServ.OM_IMCopySeaBills > 0))
				{
					JS_NoOriginalBills = Consignee.MiscServ.OM_IMOriginalSeaBills;
					JS_NoCopyBills = Consignee.MiscServ.OM_IMCopySeaBills;
				}
				else
				{
					JS_NoOriginalBills = (byte)AgencyRegistry.Instance.ReleaseTypes.Value.OriginalsNumberByType(releaseType);
					JS_NoCopyBills = (byte)AgencyRegistry.Instance.ReleaseTypes.Value.CopiesNumberByType(releaseType);
				}
			}
		}

		#endregion

		#region Service Level Defaulting overrides

		public override RefServiceLevel RegistryServiceLevel
		{
			get { return registryServiceLevel ?? (registryServiceLevel = Factory.Load<RefServiceLevel>(AgencyRegistry.Instance.ServiceLevelDefault.Value)); }
		}
		RefServiceLevel registryServiceLevel;

		#endregion

		#region IHaveInternalCartage Members

		protected override ZGuid DeliveryDepotAddress
		{
			get { return ZGuid.Empty; }
		}

		protected override ZPropertyInfo DeliveryDepotAddressInfo
		{
			get { return null; }
		}

		protected override ZGuid DeliveryCTOAddress
		{
			get { return ZGuid.Empty; }
		}

		protected override ZPropertyInfo DeliveryCTOAddressInfo
		{
			get { return null; }
		}

		protected override ZGuid DeliveryContainerYardAddress
		{
			get { return ZGuid.Empty; }
		}

		protected override ZPropertyInfo DeliveryContainerYardAddressInfo
		{
			get { return null; }
		}

		#endregion

		#region IDocManagerSupport

		protected override DocManagerInfo NewDocManager()
		{
			return new AgencyShipmentDocManagerInfo(this);
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return GetWorkflowType(); }
		}

		protected virtual ZString GetWorkflowType()
		{
			return WorkflowDescriptors.AgencyShipmentWorkflowDescriptorCode;
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
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(NewWorkflowItemsCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		AgencyShipmentProcessTaskCollection workflowItems;

		protected virtual AgencyShipmentProcessTaskCollection NewWorkflowItemsCollection()
		{
			return new AgencyShipmentProcessTaskCollection(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var job = new JobHeader.Loader(this).Load(true, false);

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JS_PackingMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GB, job != null ? job.JH_GB : GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, job != null ? job.JH_GE : GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, GetPortsInTemplateSelectionOrder(JS_NKLoadPort, JS_RL_NKOrigin));
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, GetPortsInTemplateSelectionOrder(JS_NKDischargePort, JS_RL_NKDestination));

			return result;
		}

		internal IZType[] GetClientsInTemplateSelectionOrder()
		{
			List<IZType> result = new List<IZType>();
			if (this.IsImport())
			{
				if (ConsigneePK.IsValid)
				{
					result.Add(ConsigneePK);
				}

				if (ConsignorPK.IsValid)
				{
					result.Add(ConsignorPK);
				}
			}
			else
			{
				if (ConsignorPK.IsValid)
				{
					result.Add(ConsignorPK);
				}

				if (ConsigneePK.IsValid)
				{
					result.Add(ConsigneePK);
				}
			}

			JobHeader job = new JobHeader.Loader(this).Load();
			if (job != null)
			{
				result.Add(job.LocalChargesPK);
			}
			result.Add(ZGuid.Empty);
			return result.ToArray();
		}

		IZType[] GetPortsInTemplateSelectionOrder(ZString sailingPort, ZString jobPort)
		{
			List<IZType> result = new List<IZType>();
			if (!sailingPort.IsEmpty)
			{
				result.Add(sailingPort);
				result.Add(sailingPort.Left(2));
			}

			if (jobPort != sailingPort && !jobPort.IsEmpty)
			{
				result.Add(jobPort);
				result.Add(jobPort.Left(2));
			}

			result.Add(ZString.Empty);
			return result.ToArray();
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			return GetAddressBookSelection();
		}

		protected virtual AddressBookSelection GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();

			result.AddRecipient(Consignor);
			result.AddRecipient(Consignee);
			result.AddRecipient(Principal);
			result.AddRecipient(BookedShippingLine);
			result.AddRecipient(BookingParty);

			return result;
		}

		protected virtual string GetTemplateCategory()
		{
			return MailTemplateCategoryList.Codes.None;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return GetEmailSubject(); }
		}

		protected virtual string GetEmailSubject()
		{
			return Res.GetString("7cb877f2-3a7f-4f6f-81a6-5ce8d92dfaf5", "Agency Shipment - {0}", JS_UniqueConsignRef);
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return GetTemplateCategory(); }
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
			get { return GetDocWrapperType(); }
		}

		protected virtual Type GetDocWrapperType()
		{
			return null;
		}

		#endregion

		#region IBillGenerationSupport Members

		OrgHeader IBillGenerationSupport.CarrierPrincipal
		{
			get { return Factory.Load<OrgHeader>(JS_OH_DeliveryAgent); }
		}

		ZString IBillGenerationSupport.TranshipmentIndicator
		{
			get
			{
				ContainerTranshipmentIndicatorCollection collection = AgencyRegistry.Instance.ContainerTranshipmentIndicator.Value;
				ContainerTranshipmentIndicator indicator;

				if (IsContainerTranshipmentIndicatorBreakBulk)
				{
					indicator = collection.BreakBulk;
				}
				else if (IsContainerTranshipmentIndicatorEmpty)
				{
					indicator = collection.Empty;
				}
				else
				{
					indicator = collection.Laden;
				}

				if (this.IsDomestic())
				{
					return indicator.Domestic;
				}
				else if (IsContainerTranshipmentIndicatorTranship)
				{
					return indicator.Tranship;
				}
				else
				{
					return indicator.Direct;
				}
			}
		}

		bool IsContainerTranshipmentIndicatorBreakBulk
		{
			get { return (this.JS_PackingMode == Core.Constants.ContainerModes.BreakBulk || this.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff); }
		}

		bool IsContainerTranshipmentIndicatorEmpty
		{
			get { return ShippingContainers.Any() && ShippingContainers.Cast<AgencyShipmentContainer>().All(c => c.JC_IsEmptyContainer); }
		}

		bool IsContainerTranshipmentIndicatorTranship
		{
			get
			{
				bool result = false;
				if (!this.IsDomestic() && Destination != null && !JS_NKDischargePort.IsEmpty)
				{
					result = Destination.Code != JS_NKDischargePort;
				}
				return result;
			}
		}

		protected override RefUNLOCO LoadCore
		{
			get { return CalcLoadPort; }
		}

		protected override RefUNLOCO DischargeCore
		{
			get { return CalcDischargePort; }
		}

		#endregion

		#region ISailingParentFindBox Members

		ZString ISailingParentFindBox.Destination
		{
			get { return JS_RL_NKDestination; }
		}

		ZString ISailingParentFindBox.DischargePort
		{
			get { return JS_NKDischargePort; }
		}

		ZString ISailingParentFindBox.LoadPort
		{
			get { return JS_NKLoadPort; }
		}

		ZString ISailingParentFindBox.Origin
		{
			get { return JS_RL_NKOrigin; }
		}

		ZGuid ISailingParentFindBox.SailingPK
		{
			get { return JS_JX; }
			set { JS_JX = value; }
		}

		ZString ISailingParentFindBox.TransportMode
		{
			get { return JS_TransportMode; }
		}

		#endregion

		#region IHaveInternalCartage Members

		protected override ZGuid PickupContainerYardAddress
		{
			get { return ZGuid.Empty; }
		}

		protected override ZPropertyInfo PickupContainerYardAddressInfo
		{
			get { return null; }
		}

		protected override ZGuid PickupDepotAddress
		{
			get { return ZGuid.Empty; }
		}

		protected override ZPropertyInfo PickupDepotAddressInfo
		{
			get { return null; }
		}

		protected override ZGuid PickupCTOAddress
		{
			get { return ZGuid.Empty; }
		}

		protected override ZPropertyInfo PickupCTOAddressInfo
		{
			get { return null; }
		}

		#endregion

		#region IVoyageFinderParent Members

		ZString IVoyageFinderParent.TransportMode
		{
			get { return JS_TransportMode; }
		}

		ZString IVoyageFinderParent.LoadPort
		{
			get { return JS_NKLoadPort != ZString.Empty ? JS_NKLoadPort : JS_RL_NKOrigin; }
		}

		ZString IVoyageFinderParent.DischargePort
		{
			get { return JS_NKDischargePort != ZString.Empty ? JS_NKDischargePort : JS_RL_NKDestination; }
		}

		ZGuid IVoyageFinderParent.CarrierPK
		{
			get { return BookedShippingLinePK; }
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return PrincipalSecurityAllowed;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return NotAuthorizedForThisPrincipal;
			}
		}

		public static MultilingualString NotAuthorizedForThisPrincipal
		{
			get { return ResString.GetMultilingualString("3b03e9da-ad90-4a0f-ba69-66a14214a671", "You are not authorized to access shipments for this principal"); }
		}

		public bool PrincipalSecurityAllowed
		{
			get
			{
				return Principal == null || ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(Principal);
			}
		}

		#endregion

		#region Invoicing/Rating

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new AgencyShipmentRatingAdaptersProvider(this); }
		}

		protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new AgencyShipmentInvoicingSupporter(this);
		}

		#endregion

		#region BusinessObjectsWithRelatedEvents

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				result.AddRange(RelatedTransportBookingEvents);
				result.AddRange(ShippingContainers);

				return result.ToArray();
			}
		}

		#endregion

		#region Implementation

		JobDocAddressRequirement RelaxOverriddenAddressValidation(JobDocAddressRequirement requirement)
		{
			JobDocAddressRequirement.ValidationDelegate noValidation = delegate
			{ };
			requirement.ValidateAddress1 = noValidation;
			requirement.ValidateCity = noValidation;
			return requirement;
		}

		public override OrgAddress ImportReleaseDepot
		{
			get
			{
				var transport = TransportsIncludingRelated.ArrivalTransport;
				return transport == null ? null : transport.ArrivalLocation;
			}
		}

		public override OrgAddress ExportReceivingDepot
		{
			get
			{
				var transport = TransportsIncludingRelated.DepartureTransport;
				return transport == null ? null : transport.DepartureLocation;
			}
		}

		void ConvertCargoMeasures(AgencyShipmentContainerDependentCollection cargo)
		{
			if (AgencyRegistry.Instance.ConvertToDefaultUnitsOnConfirmation.Value)
			{
				string targetWeightUnit = AgencyRegistry.Instance.DefaultBillWeightUnit.Value;
				string targetVolumeUnit = AgencyRegistry.Instance.DefaultBillVolumeUnit.Value;

				foreach (AgencyShipmentContainer container in cargo)
				{
					container.JC_GrossWeight = Constants.Weight.Convert(container.JC_GrossWeight, container.JC_GrossWeightUQ, targetWeightUnit);
					container.JC_GrossWeightUQ = targetWeightUnit;

					container.JC_GrossVolume = Constants.Volume.Convert(container.JC_GrossVolume, container.JC_GrossVolumeUQ, targetVolumeUnit);
					container.JC_GrossVolumeUQ = targetVolumeUnit;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_ActualWeight();
					Validation.ValidateJS_ActualVolume();
				}
			}
		}

		void SplitCargoIfNecessary(AgencyShipmentContainerDependentCollection cargo)
		{
			if (IsRollOnRollOff)
			{
				foreach (AgencyShipmentContainer container in cargo.ToArray())
				{
					int count = container.JC_ContainerCount;
					if (count > 1)
					{
						decimal weight = container.JC_GrossWeight / count;
						decimal volume = container.JC_GrossVolume / count;

						container.JC_ContainerCount = 1;
						container.JC_GrossWeight = weight;
						container.JC_GrossVolume = volume;

						for (int i = 1; i < count; i++)
						{
							AgencyShipmentContainer clone = (AgencyShipmentContainer)container.Clone();
							cargo.Add(clone);
						}
					}
				}
			}
		}

		void ConvertCargoFromBookedToReal()
		{
			foreach (AgencyShipmentContainer container in BookedContainers.ToArray())
			{
				BookedContainers.Remove(container);

				container.JC_Purpose = ContainerBookedStatus.Codes.Real;
				container.SetReadOnlyIncludingChildren(false);

				RealContainers.Add(container);
			}
		}

		void ResetShippingContainerViews()
		{
			TopLevelPacks.SwapCollectionToFilter(ShippingContainers);
			Vehicles.SwapCollectionToFilter(ShippingContainers);
		}

		void ConvertWeightsAndVolumes()
		{
			if (AgencyRegistry.Instance.ConvertToDefaultUnitsOnConfirmation.Value)
			{
				string targetWeightUnit = AgencyRegistry.Instance.DefaultBillWeightUnit.Value;
				string targetVolumeUnit = AgencyRegistry.Instance.DefaultBillVolumeUnit.Value;

				Dictionary<ZPropertyInfo, ZDecimal> newValues = new Dictionary<ZPropertyInfo, ZDecimal>();
				HashSet<ZGuid> existingPacklines = new HashSet<ZGuid>();

				newValues[JS_ActualWeightInfo] = Utilities.Round(Constants.Weight.Convert(JS_ActualWeight, JS_UnitOfWeight, targetWeightUnit), 3);
				newValues[JS_ActualVolumeInfo] = Utilities.Round(Constants.Volume.Convert(JS_ActualVolume, JS_UnitOfVolume, targetVolumeUnit), 3);

				foreach (PackLine pack in OuterPackLines)
				{
					existingPacklines.Add(pack.PK);
					newValues[pack.JL_ActualWeightInfo] = Utilities.Round(Constants.Weight.Convert(pack.JL_ActualWeight, pack.JL_ActualWeightUQ, targetWeightUnit), 3);
					newValues[pack.JL_ActualVolumeInfo] = Utilities.Round(Constants.Volume.Convert(pack.JL_ActualVolume, pack.JL_ActualVolumeUQ, targetVolumeUnit), 3);
				}

				foreach (CommonContainer container in BookedContainers)
				{
					newValues[container.JC_Calc_NetWeightInfo] = container.JC_Calc_NetWeight;
					newValues[container.JC_TareWeightInfo] = container.JC_TareWeight;
				}

				JS_UnitOfWeight = targetWeightUnit;
				JS_UnitOfVolume = targetVolumeUnit;

				JS_ActualWeight = newValues[JS_ActualWeightInfo];
				JS_ActualVolume = newValues[JS_ActualVolumeInfo];

				foreach (PackLine pack in OuterPackLines.ToArray())
				{
					if (existingPacklines.Contains(pack.PK))
					{
						pack.JL_ActualWeightUQ = targetWeightUnit;
						pack.JL_ActualVolumeUQ = targetVolumeUnit;

						pack.JL_ActualWeight = newValues[pack.JL_ActualWeightInfo];
						pack.JL_ActualVolume = newValues[pack.JL_ActualVolumeInfo];
					}
					else
					{
						pack.Delete();
					}
				}

				foreach (CommonContainer container in BookedContainers)
				{
					container.JC_Calc_NetWeight = newValues[container.JC_Calc_NetWeightInfo];
					container.JC_TareWeight = newValues[container.JC_TareWeightInfo];
				}
			}
		}

		void SplitContainers()
		{
			IDictionary<ZGuid, ZGuid[]> realBookedContainerMap = ShippingContainerMapper.GetBookedRealContainerMap(this);

			BusinessObjectCloneArgs cloneArgs = new BusinessObjectCloneArgs();
			cloneArgs.AddExcludedColumns(new[]
			{
				JobContainerSchema.Constants.JC_ContainerCount,
				JobContainerSchema.Constants.JC_TareWeight,
				JobContainerSchema.Constants.JC_GrossWeight,
				JobContainerSchema.Constants.JC_GrossWeightUQ,
				JobContainerSchema.Constants.JC_ReleaseNum,
				JobContainerSchema.Constants.JC_Purpose,
				JobContainerSchema.Constants.JC_AdditionalSealNum,
				JobContainerSchema.Constants.JC_Additional2SealNum,
				CommonContainer.Schema.JC_Calc_NetWeight,
			});

			BusinessObjectCloneArgs copyArgs = new BusinessObjectCloneArgs();
			copyArgs.AddExcludedColumns(new[]
			{
				JobContainerSchema.Constants.JC_ContainerCount,
				JobContainerSchema.Constants.JC_TareWeight,
				JobContainerSchema.Constants.JC_GrossWeight,
				JobContainerSchema.Constants.JC_GrossWeightUQ,
				JobContainerSchema.Constants.JC_ReleaseNum,
				JobContainerSchema.Constants.JC_Purpose,
				JobContainerSchema.Constants.JC_SealNum,
				JobContainerSchema.Constants.JC_AdditionalSealNum,
				JobContainerSchema.Constants.JC_Additional2SealNum,
				CommonContainer.Schema.JC_Calc_NetWeight,

				JobContainerSchema.Constants.JC_GrossWeightVerificationType,
				JobContainerSchema.Constants.JC_GrossWeightVerificationDateTime,

				JobContainerSchema.Constants.JC_ContainerNum,
				JobContainerSchema.Constants.JC_RC,
				JobContainerSchema.Constants.JC_IsShipperOwned,
			});

			var docAddressCloneArgs = new BusinessObjectCloneArgs();

			docAddressCloneArgs.AddExcludedColumns(new[]
			{
				JobDocAddressSchema.Constants.E2_ParentID,
				JobDocAddressSchema.Constants.E2_ParentTableCode
			});

			Dictionary<ZGuid, ZGuid> containerPacklineMap = new Dictionary<ZGuid, ZGuid>();

			foreach (AgencyShipmentContainer container in BookedContainers.ToArray())
			{
				int count = container.JC_ContainerCount;

				string targetWeightUnit;
				decimal adjustedNetWeight;
				decimal adjustedTareWeight;

				if (AgencyRegistry.Instance.ConvertToDefaultUnitsOnConfirmation.Value)
				{
					targetWeightUnit = AgencyRegistry.Instance.DefaultBillWeightUnit.Value;
					adjustedNetWeight = Constants.Weight.Convert(container.JC_Calc_NetWeight, container.JC_GrossWeightUQ, targetWeightUnit);
					adjustedTareWeight = Constants.Weight.Convert(container.JC_TareWeight, container.JC_GrossWeightUQ, targetWeightUnit);
				}
				else
				{
					targetWeightUnit = container.JC_GrossWeightUQ;
					adjustedNetWeight = container.JC_Calc_NetWeight;
					adjustedTareWeight = container.JC_TareWeight;
				}

				AgencyShipmentContainer lastRealContainer = null;
				var mapRealContainers = (AgencyShipmentContainer[])RealContainers.Find(new ZQuery(JobContainerSchema.PK, realBookedContainerMap[container.PK]));

				for (int index = 0; index < mapRealContainers.Length; index++)
				{
					lastRealContainer = mapRealContainers[index];

					decimal netWeight = Constants.Weight.Convert(lastRealContainer.JC_Calc_NetWeight, lastRealContainer.JC_GrossWeightUQ, targetWeightUnit);
					decimal tareWeight = Constants.Weight.Convert(lastRealContainer.JC_TareWeight, lastRealContainer.JC_GrossWeightUQ, targetWeightUnit);

					adjustedNetWeight -= netWeight;
					adjustedTareWeight -= tareWeight;

					lastRealContainer.CopyPersistentValuesFrom(container, copyArgs);

					lastRealContainer.JC_GrossWeightUQ = targetWeightUnit;
					lastRealContainer.JC_Calc_NetWeight = netWeight;
					lastRealContainer.JC_TareWeight = tareWeight;
				}

				if (count > mapRealContainers.Length)
				{
					decimal unitNetWeight = Math.Max(adjustedNetWeight, 0) / (count - mapRealContainers.Length);
					decimal unitTareWeight = Math.Max(adjustedTareWeight, 0) / (count - mapRealContainers.Length);

					for (int index = mapRealContainers.Length; index < count; index++)
					{
						lastRealContainer = (AgencyShipmentContainer)container.Clone(cloneArgs);

						using (lastRealContainer.GetValidationSuspender())
						{
							lastRealContainer.JC_ContainerCount = 1;
							lastRealContainer.JC_GrossWeightUQ = targetWeightUnit;
							lastRealContainer.JC_Calc_NetWeight = unitNetWeight;
							lastRealContainer.JC_TareWeight = unitTareWeight;

							lastRealContainer.GrossWeightVerifiedByAddress.CopyPersistentValuesFrom(container.GrossWeightVerifiedByAddress, docAddressCloneArgs);
						}

						RealContainers.Add(lastRealContainer);
					}
				}

				if (count == 1 && lastRealContainer != null)
				{
					containerPacklineMap.Add(container.PK, lastRealContainer.PK);
				}
			}

			foreach (PackLine packline in OuterPackLines)
			{
				ZGuid newPK;

				using (packline.GetValidationSuspender())
				{
					CommonContainer oldContainer = (CommonContainer)BookedContainers.FindByPK(packline.JL_JC);
					decimal oldNet = oldContainer == null ? ZDecimal.Zero : oldContainer.JC_Calc_NetWeight;

					if (containerPacklineMap.TryGetValue(packline.JL_JC, out newPK))
					{
						CommonContainer newContainer = (CommonContainer)RealContainers.FindByPK(newPK);
						decimal newNet = newContainer == null ? ZDecimal.Zero : newContainer.JC_Calc_NetWeight;

						packline.JL_JC = newPK;

						if (newContainer != null)
						{
							newContainer.JC_Calc_NetWeight = newNet;
						}
					}
					else
					{
						packline.JL_JC = ZGuid.Empty;
					}

					if (oldContainer != null)
					{
						oldContainer.JC_Calc_NetWeight = oldNet;
					}
				}
			}
		}

		void DefaultFromSailing()
		{
			JobSailing sailing;
			VoyageOrigin origin;
			JobVoyage voyage;

			if ((sailing = Sailing) != null && (origin = sailing.Origin) != null && sailing.Destination != null && (voyage = origin.Voyage) != null)
			{
				if (!suspendDefaultingCarrierFromSailing)
				{
					JS_OA_BookedShippingLineAddress = Sailing.Voyage.Line?.MainAddress.PK ?? ZGuid.Empty;
				}

				if (JS_OH_DeliveryAgent.IsEmpty)
				{
					if (voyage.TradeLanes.Count == 1 && IsValidPrincipal(voyage.TradeLanes[0].NB_OH))
					{
						JS_OH_DeliveryAgent = voyage.TradeLanes[0].NB_OH;
					}
					else if (voyage.Line != null && voyage.Line.CompanyData.OB_CRIsShipsAgencyPrincipal && IsValidPrincipal(voyage.JV_OH_Line))
					{
						JS_OH_DeliveryAgent = voyage.JV_OH_Line;
					}
				}

				if (!origin.JA_A_DEP.IsEmpty && AgencyRegistry.Instance.UpdateShipmentDatesFromSailing.Value)
				{
					if (JS_ShippedOnBoardDate.IsEmpty)
					{
						JS_ShippedOnBoardDate = origin.JA_A_DEP;
					}

					if (JS_HouseBillIssueDate.IsEmpty)
					{
						JS_HouseBillIssueDate = origin.JA_A_DEP;
					}
				}
			}
		}

		public IDisposable SuspendDefaultingCarrierFromSailing()
		{
			suspendDefaultingCarrierFromSailing = true;
			return new DisposableAction(() => suspendDefaultingCarrierFromSailing = false);
		}

		bool suspendDefaultingCarrierFromSailing;

		bool IsValidPrincipal(ZGuid principalPK)
		{
			var principalList = Lookups.Principal_List;
			principalList.Load();

			var result = principalList.Contains(principalPK);
			return result;
		}

		void DefaultDestinationFromDischargePort()
		{
			if (JS_RL_NKDestination.IsEmpty && !JS_NKDischargePort.IsEmpty)
			{
				JS_RL_NKDestination = JS_NKDischargePort;
			}
		}

		void GetNextSailingOrClearSailings()
		{
			if (!IsInDatabase && !Factory.IsInTransaction)
			{
				if (loadPort.IsEmpty || dischargePort.IsEmpty)
				{
					JS_JX = ZGuid.Empty;
					Sailings.RemoveAll();
				}
				else if (!IsSailingValid(Sailing))
				{
					GetNextSailing();
				}
			}
		}

		bool IsSailingValid(JobSailing sailing)
		{
			if (sailing == null || sailing.JX_JA_RL_NKPortOfLoading != loadPort || sailing.JX_JB_RL_NKPortOfDischarge != dischargePort)
			{
				return false;
			}
			else
			{
				ZDateTime referenceDate = JS_E_DEP.IsValidSmallDateTime ? JS_E_DEP : JS_A_BKD;
				ZDateTime sailingCutOff = sailing.JX_JA_CTOCutOff.IsValidSmallDateTime ? sailing.JX_JA_CTOCutOff : sailing.JX_JA_E_DEP;
				return referenceDate.IsEmpty || sailingCutOff.IsEmpty || sailingCutOff > referenceDate;
			}
		}

		void DefaultOriginFromLoadPort()
		{
			if (JS_RL_NKOrigin.IsEmpty && !JS_NKLoadPort.IsEmpty)
			{
				JS_RL_NKOrigin = JS_NKLoadPort;
			}
		}

		void DefaultEstimatedDatesFromSailing(JobSailing sailing)
		{
			if (Sailing != null)
			{
				ignoreGetNextSailingOrClearSailings = true;
				try
				{
					JS_E_DEP = sailing.JX_JA_E_DEP;
					JS_E_ARV = sailing.JX_JB_E_ARV;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_E_DEP();
						Validation.ValidateJS_E_ARV();
					}
				}
				finally
				{
					ignoreGetNextSailingOrClearSailings = false;
				}
			}
		}
		bool ignoreGetNextSailingOrClearSailings;

		void GetNextSailing()
		{
			JobSailingCollection currentSailing = RetrieveRelatedSailings();

			if (!JS_JX.IsValid || !currentSailing.Contains(JS_JX))
			{
				if (currentSailing != null && currentSailing.Count > 0)
				{
					JobSailing nextSailing = currentSailing[0];
					JS_JX = nextSailing.PK;
				}
				else
				{
					JS_JX = ZGuid.Empty;
					Sailings.RemoveAll();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_NKDischargePort();
				}
			}
		}

		void UpdateLoadDischargeFromSailing()
		{
			loadAndDischargeUpdatedFromSailing = true;

			if (Sailing != null)
			{
				loadPort = Sailing.JX_JA_RL_NKPortOfLoading;
				dischargePort = Sailing.JX_JB_RL_NKPortOfDischarge;
				JS_NKDischargePortInfo.RefreshBinding();
				JS_NKLoadPortInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateJS_NKLoadPort();
					Validation.ValidateJS_NKDischargePort();
				}
			}
		}

		bool loadAndDischargeUpdatedFromSailing;

		public JobSailingCollection RetrieveRelatedSailings()
		{
			JobSailingCollection sailing_List = null;

			ZDateTime referenceDate = JS_E_DEP.IsValidSmallDateTime ? JS_E_DEP : JS_A_BKD;

			if (!JS_NKLoadPort.IsEmpty && !JS_NKDischargePort.IsEmpty && referenceDate.IsValidSmallDateTime)
			{
				SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
				builder.TransportMode = JS_TransportMode;
				builder.LoadPort = JS_NKLoadPort;
				builder.DischargePort = JS_NKDischargePort;
				builder.SetDateRange(SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasDateInRange, referenceDate.Date, ZDateTime.Empty);

				ZDBOnlySubQuery originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				originSubQuery.AddToFilter(JoinCondition.And, JobVoyOriginSchema.JA_CutOff, SQLComparisonOperator.GreaterThanOrEqualTo, referenceDate);
				originSubQuery.AddToFilter(JoinCondition.Or, JobVoyOriginSchema.JA_CutOff, SQLComparisonOperator.Equal, null);

				ZDBOnlyQuery sailingQuery = new ZDBOnlyQuery(typeof(JobSailing));
				sailingQuery.AddSubQuery(originSubQuery, JoinCondition.And);

				ZQuery filter = builder.ToSailingFilter();
				filter.AddToFilter(sailingQuery, JoinCondition.And);
				filter.AddToFilter(JobSailingSchema.JX_IsPublished, ZBool.True);

				sailing_List = new JobSailingCollection(Factory, filter);
				sailing_List.Load();
				sailing_List.Sort(JobSailing.Schema.JX_JA_E_DEP, ListSortDirection.Ascending);
			}

			return sailing_List;
		}

		#endregion

		#region IJobInvoicingExRateSourceProvider Members

		IExchangeRateSource IJobInvoicingExRateSourceProvider.GetExRateSource(ExRateSourceType sourceType)
		{
			switch (sourceType)
			{
				case ExRateSourceType.Voyage:
					{
						var sailing = Sailing;
						return sailing == null ? null : new AgencyShipmentExRateSource(this, sailing.Voyage);
					}
			}
			return null;
		}

		#endregion

		#region IDefaultNumberOfDecimalPlacesSupporter

		protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		protected override ZString GetUnitOfMeasureCore(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.TopLevelPacksTotalVolumeInShipmentVolumeUnit:
					unitOfMeasure = GetShipmentVolumeUnit();
					break;

				case Schema.TopLevelPacksTotalWeightInShipmentWeightUnit:
					unitOfMeasure = GetShipmentWeightUnit();
					break;

				default:
					unitOfMeasure = base.GetUnitOfMeasureCore(property);
					break;
			}

			return unitOfMeasure;
		}

		ZString GetShipmentVolumeUnit()
		{
			return FreightUtilities.IsValidVolumeUnit(JS_UnitOfVolume) ? JS_UnitOfVolume : RegistryVolumeUnit;
		}

		ZString GetShipmentWeightUnit()
		{
			return FreightUtilities.IsValidWeightUnit(JS_UnitOfWeight) ? JS_UnitOfWeight : RegistryWeightUnit;
		}

		protected override ZDecimal GetRoundedValueCore(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForShipping.GetRoundedValue(this, column, property, value);
		}

		protected override void RoundMeasurePropertiesOnTransportModeChangedCore()
		{
			// not required as transport mode is always SEA
		}

		#endregion

		#region IProcessHandlingInfoProvider

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new AgencyShipmentProcessHandlingInfo(this); }
		}

		#endregion

		#region Events Management

		public override IReadOnlyDictionary<string, string> GetParametersForEvent(Event eventType)
		{
			var parameters = base.GetParametersForEvent(eventType).ToDictionary(p => p.Key, p => p.Value);

			if (eventType == Events.FreightLoaded || eventType == Events.CargoReceivedAtDepot)
			{
				var firstSeaLeg = TransportsIncludingRelated.FirstLegMatching(leg => leg.IsSea);

				parameters[EventConstants.EventReferenceParameters.Codes.Location] = firstSeaLeg != null ? firstSeaLeg.JW_RL_NKLoadPort.ToString() : null;
			}
			else if (eventType == Events.StatusUpdated)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Type] = Constants.EventReferenceMessageTypes.ShipmentStatus;
				parameters[EventConstants.EventReferenceParameters.Codes.Old] = IsInDatabase ? JS_ShipmentStatusInfo.OriginalValue.ToString() : null;
				parameters[EventConstants.EventReferenceParameters.Codes.New] = JS_ShipmentStatus;

				if (JS_ShipmentStatus == ShipmentStatusList.Codes.ElectronicBooking)
				{
					parameters[EventConstants.EventReferenceParameters.Codes.Reason] = Constants.EventReferenceParameterReasons.ElectronicBookingReceived;
				}
				else if (JS_ShipmentStatus == ShipmentStatusList.Codes.ElectronicShippingInstruction)
				{
					parameters[EventConstants.EventReferenceParameters.Codes.Reason] = Constants.EventReferenceParameterReasons.ElectronicShippingInstructionReceived;
				}
				else
				{
					parameters[EventConstants.EventReferenceParameters.Codes.Reason] = null;
				}
			}

			return parameters;
		}

		protected override bool IsEventFacilityMatched(IStmALog log)
		{
			if (log.SL_SE_NKEvent == Events.FreightLoadedCode)
			{
				var parameters = GetParametersForEvent(Events.All[log.SL_SE_NKEvent]);

				string shipmentFacility;
				string facilityInEvent;

				parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out shipmentFacility);
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out facilityInEvent);

				return string.IsNullOrEmpty(facilityInEvent) || facilityInEvent == shipmentFacility;
			}

			return base.IsEventFacilityMatched(log);
		}

		protected override void ProcessLogCore(IStmALog log)
		{
			base.ProcessLogCore(log);

			switch (log.SL_SE_NKEvent)
			{
				case Events.ArrivalCode:
				case Events.DepartureCode:
					FreightEventsHelper.PropagateArivalDepartureEventToMatchedTransportLeg(log, this.Transports);
					break;
			}
		}

		protected override IDictionary<string, string> GetDateEventParameters(Event eventType)
		{
			var parameters = base.GetDateEventParameters(eventType);

			if (eventType == Events.Arrival && !JS_TransportMode.IsEmpty)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Mode] = JS_TransportMode;
			}
			else if (eventType == Events.Departure && !JS_TransportMode.IsEmpty)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Mode] = JS_TransportMode;
			}

			return parameters;
		}

		#endregion

		#region Default PickupRoadOrRailLeg DepartureLocation / DeliveryRoadOrRailLeg ArrivalLocation

		protected override void ConsignorDocumentaryOrgHeaderChanged()
		{
			base.ConsignorDocumentaryOrgHeaderChanged();

			DefaultPickupRoadOrRailLegDepartureLocation();
		}

		protected override void ConsigneeDocumentaryOrgHeaderChanged()
		{
			base.ConsigneeDocumentaryOrgHeaderChanged();

			DefaultDeliveryRoadOrRailLegArrivalLocation();
		}

		public void DefaultPickupRoadOrRailLegDepartureLocation()
		{
			var pickupRoadOrRailLeg = PickupRoadOrRailLeg;
			if (pickupRoadOrRailLeg != null && pickupRoadOrRailLeg.JW_OA_DepartureLocation.IsEmpty && Consignor != null && !JS_RL_NKOrigin.IsEmpty)
			{
				var address = Consignor.Addresses.BestAddressForPort(JS_RL_NKOrigin, nameof(AddressType.PIC)) ?? Consignor.GetAddressWithFallback(AddressType.PIC);

				if (address != null)
				{
					pickupRoadOrRailLeg.JW_OA_DepartureLocation = address.PK;
				}
			}
		}

		public void DefaultDeliveryRoadOrRailLegArrivalLocation()
		{
			var deliveryRoadOrRailLeg = DeliveryRoadOrRailLeg;
			if (deliveryRoadOrRailLeg != null && deliveryRoadOrRailLeg.JW_OA_ArrivalLocation.IsEmpty && Consignee != null && !JS_RL_NKDestination.IsEmpty)
			{
				var address = Consignee.Addresses.BestAddressForPort(JS_RL_NKDestination, nameof(AddressType.DLV)) ?? Consignee.GetAddressWithFallback(AddressType.DLV);

				if (address != null)
				{
					deliveryRoadOrRailLeg.JW_OA_ArrivalLocation = address.PK;
				}
			}
		}

		public Transport PickupRoadOrRailLeg
		{
			get
			{
				return TransportsIncludingRelated.FirstLegMatching(t =>
					(t.IsRoad || t.IsRail)
					&& t.JW_RL_NKLoadPort == JS_RL_NKOrigin
					&& t.JW_TransportType != Constants.TransportPlanningType.MainVessel);
			}
		}

		public Transport DeliveryRoadOrRailLeg
		{
			get
			{
				return TransportsIncludingRelated.LastLegMatching(t =>
					(t.IsRoad || t.IsRail)
					&& t.JW_RL_NKDiscPort == JS_RL_NKDestination
					&& t.JW_TransportType != Constants.TransportPlanningType.MainVessel);
			}
		}

		#region IDeniedPartyProvider

		ZString IDeniedPartyProvider.ReferenceId => JS_UniqueConsignRef;

		#endregion

		#region IComplianceRiskStatusProvider

		IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties => GetParties();

		protected virtual List<ScreeningParty> GetParties()
		{
			var parties = new List<ScreeningParty>();

			foreach (JobDocAddress docAddress in DocAddresses)
			{
				parties.Add(new ScreeningParty(this, docAddress.AddressCaption, docAddress));
			}

			parties.Add(new ScreeningParty(this, Res.GetString("7ec70b33-24ca-47bf-95e8-24988293bda4", "Planned Carrier"), BookedShippingLine));
			parties.Add(new ScreeningParty(this, Res.GetString("bada19a0-db37-4e3e-b20c-0586d92a0868", "Principal"), ((IBillGenerationSupport)this).CarrierPrincipal));

			if (SendingAgentAddress != null)
			{
				parties.Add(new ScreeningParty(this, Res.GetString("2a8d2204-e230-4e33-9272-4d379d2fdcc9", "Sending Agent"), SendingAgentAddress.Header));
			}
			if (ReceivingAgentAddress != null)
			{
				parties.Add(new ScreeningParty(this, Res.GetString("9a7912da-0722-4650-9d6c-b42439dc27e1", "Receiving Agent"), ReceivingAgentAddress.Header));
			}

			foreach (Transport transport in TransportsIncludingRelated)
			{
				parties.Add(new ScreeningParty(this, Res.GetString("ac5ce260-beed-48aa-9fec-94b7f7cf6378", "Carrier"), transport.Carrier));
				parties.Add(new ScreeningParty(this, Res.GetString("52a23e79-d073-4e5c-a74e-507f62f203eb", "Creditor"), transport.Creditor));
				var departureOrg = transport.DepartureLocation?.Header;
				parties.Add(new ScreeningParty(this, Res.GetString("a78b63b2-e7b8-42d3-bfc4-3917b32f6c61", "Depart From"), departureOrg));
				var arrivalOrg = transport.ArrivalLocation?.Header;
				parties.Add(new ScreeningParty(this, Res.GetString("938d4843-6cb5-4b43-be13-d0a6dd049d8d", "Arrival At"), arrivalOrg));
			}

			foreach (AgencyShipmentContainer container in BookedContainers)
			{
				var departureYard = container.DepartureContainerYardAddress?.Header;
				if (departureYard != null)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("5b245231-503c-403c-9fb9-df9452fb024a", "Departure Container Yard"), departureYard));
				}
			}

			foreach (AgencyShipmentContainer container in RealContainers)
			{
				parties.Add(new ScreeningParty(this, Res.GetString("b16996fb-1b1a-4695-96c2-424c992ceb6f", "Verified By"), container.GrossWeightVerifiedByAddress));
			}

			if (Job != null)
			{
				parties.Add(new ScreeningParty(this, Res.GetString("18a60180-5fbf-4b15-8a5f-6c005c80d283", "Local Client"), Job.LocalCharges));
				var charges = (IBusinessObjectCollection)Job["Charges"];
				foreach (JobCharge charge in charges)
				{
					parties.Add(new ScreeningParty(this, Res.GetString("69a3bfbb-997b-4387-b583-3ffa6a8b4d2a", "Creditor"), charge.CostAccount));
					parties.Add(new ScreeningParty(this, Res.GetString("594c7fcc-39cf-47fb-8385-248c3a851c5f", "Debtor"), charge.SellAccount));
				}
			}

			return parties;
		}

		IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations
		{
			get
			{
				var countries = new List<ScreeningParty>();
				AddCountryToList(this, (NoResString)"Origin Country", Origin?.Country);
				AddCountryToList(this, (NoResString)"Destination Country", Destination?.Country);
				AddCountryToList(this, (NoResString)"Place of Receipt", PlaceOfReceipt?.Country);
				AddCountryToList(this, (NoResString)"Place of Delivery", PlaceOfDischarge?.Country);

				foreach (Transport transport in TransportsIncludingRelated)
				{
					AddCountryToList(this, (NoResString)"Routing Load Country", transport.LoadPort?.Country);
					AddCountryToList(this, (NoResString)"Routing Discharge Country", transport.DiscPort?.Country);
				}

				foreach (ScreeningParty party in ((ICompliancePartyRiskStatusProvider)this).Parties)
				{
					RefCountry country = null;

					if (party.DocAddress?.Address != null)
					{
						if (party.DocAddress.E2_AddressOverride)
						{
							country = party.DocAddress.Country;
						}
						else
						{
							country = party.DocAddress.Address.Country;
						}
					}
					else if (party.Header != null)
					{
						country = party.Header.Country;
					}
					else if (!string.IsNullOrEmpty(party.OrgCode))
					{
						var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, party.OrgCode));
						country = org?.Country;
					}

					AddCountryToList(party.Parent, party.Description, country);
				}

				return countries;

				void AddCountryToList(BusinessObject parent, string description, RefCountry country)
				{
					if (country != null)
					{
						countries.Add(new ScreeningParty(parent, description, country));
					}
				}
			}
		}

		IEnumerable<IComplianceCommodity> IComplianceCommodityRiskStatusProvider.Commodities
		{
			get
			{
				var commodities = new List<ComplianceCommodity>();

				if (JS_PackingMode == Constants.ContainerModes.RollOnRollOff)
				{
					var vehicles = Vehicles.OfType<AgencyShipmentContainer>();

					foreach (var vehicle in vehicles)
					{
						if (!vehicle.JC_HarmonisedCode.IsEmpty)
						{
							commodities.Add(new ComplianceCommodity(vehicle.JC_HarmonisedCode, WorldCustomsOrganisationWCO,
								JS_UniqueConsignRef, ((IComplianceItemRiskStatusProvider)this).ParentID, ZString.Empty, GetCommoditySource(), vehicle.JC_Description
							));
						}
					}

					return commodities;
				}
				if (JS_PackingMode == Constants.ContainerModes.BreakBulk || JS_PackingMode == Constants.ContainerModes.Bulk || JS_PackingMode == Constants.ContainerModes.Liquid)
				{
					var packlines = TopLevelPacks.OfType<AgencyShipmentContainer>();

					foreach (var packline in packlines)
					{
						if (!packline.JC_HarmonisedCode.IsEmpty)
						{
							commodities.Add(new ComplianceCommodity(packline.JC_HarmonisedCode, WorldCustomsOrganisationWCO, JS_UniqueConsignRef, ((IComplianceItemRiskStatusProvider)this).ParentID, ZString.Empty, GetCommoditySource(), packline.JC_Description));
						}
					}
					return commodities;
				}

				var lines = OuterPackLines.OfType<AgencyShipmentPackLine>();
				foreach (var line in lines)
				{
					if (!line.JL_HarmonisedCode.IsEmpty
						&& line.Shipment != null && line.Shipment.JS_UniqueConsignRef == JS_UniqueConsignRef)
					{
						commodities.Add(new ComplianceCommodity(line.JL_HarmonisedCode, WorldCustomsOrganisationWCO, JS_UniqueConsignRef, ((IComplianceItemRiskStatusProvider)this).ParentID, line.JL_RN_NKOrigin, GetCommoditySource(), line.JL_Description));
					}
				}

				return commodities;
			}
		}

		protected virtual ZString GetCommoditySource()
		{
			switch (JS_PackingMode)
			{
				case Constants.ContainerModes.RollOnRollOff:
					return Res.GetString("9996a00f-9177-495c-b7c7-31281e208cce", "Details > Vehicles");
				default:
					return Res.GetString("e22144ae-41a5-454c-ac36-d0e2f9d0092b", "Details > Packs");
			}
		}

		ZGuid IComplianceItemRiskStatusProvider.ParentID => PK;

		ZString IComplianceItemRiskStatusProvider.ParentTableCode => TablePrefix;

		ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate
		{
			get
			{
				var result = JS_E_DEP;

				if (!result.IsValid)
				{
					result = JS_SystemCreateTimeUtc.ToLocalBranchTime();
				}

				if (!result.IsValid)
				{
					result = ZDateTime.Today.ToDateTime();
				}

				return result;
			}
		}

		ZBool IComplianceCommodityRiskStatusProvider.IsEditingCommoditySupported => ZBool.True;

		Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport => ComplianceRiskSupport.SupportInitialization;

		(ZBool IsCurrent, ZDateTime JobEndDate) IComplianceItemRiskStatusProvider.JobTime => ComplianceRiskHelper.GetJobEndDateAndIsCurrent(Job?.JH_Status, JS_E_DEP,
			JS_E_ARV, this, Transports.Select(u => new ComplianceRouting { ETD = u.JW_ETD, ETA = u.JW_ETA, ATD = u.JW_ETD, ATA = u.JW_ATA }));

		ComplianceAssessmentPointPairInfo IComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo
		{
			get
			{
				var pointPairs = new[] {
					new ComplianceCheckRequestPointPair
					{
						OriginPoint = new ComplianceCheckRequestPointPairLocation
						{
							Country = Origin?.RL_RN_NKCountryCode,
							UNLOCO = Origin?.RL_Code,
							MovementDescription = (NoResString)"Origin"
						},
						DestinationPoint = new ComplianceCheckRequestPointPairLocation
						{
							Country = Destination?.RL_RN_NKCountryCode,
							UNLOCO = Destination?.RL_Code,
							MovementDescription = (NoResString)"Destination"
						},
						EstimatedTimeOfArrival = JS_E_ARV,
						EstimatedTimeOfDeparture = JS_E_DEP,
						Mode = TransportMode
					}
				};

				return new ComplianceAssessmentPointPairInfo(pointPairs);
			}
		}

		CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => Env.Security.AgencyBookingsComplianceAllowOverrideOverallRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => Env.Security.AgencyBookingsComplianceAllowResynchronizeRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => new DeniedSecurityCheckpoint();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => Env.Security.AgencyBookingsComplianceEditHarmonizedCode;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => Env.Security.AgencyBookingsComplianceEditComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => Env.Security.AgencyBookingsComplianceAllowComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => Env.Security.AgencyBookingsComplianceDeclineComplianceAssessment;

		#endregion

		#endregion

		#region Compliance Risk

		ZBool IComplianceJobDirectionProvider.IsInternational => (this.IsCrossTrade() || this.IsExport() || this.IsImport());

		ZBool IComplianceItemRiskStatusProvider.IsEnabledComplianceWise => ComplianceRiskHelper.IsLinerAgencyEnabledComplianceWise;

		#endregion

		#region ICreditControlledDocumentDelivery Members

		protected override bool IsDPSFreightMovementRestrictedCore()
		{
			if (ComplianceRiskHelper.IsLinerAgencyEnabledComplianceWise)
			{
				return ObjectFactory.Get<IComplianceRiskStatusSupporter>().IsDPSFreightMovementRestricted(ScreeningStatusesList.Codes.Release, this, this);
			}

			return false;
		}

		#endregion

		#region GetStrategies

		IBusinessObjectStrategy[] strategies;
		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			if (strategies == null)
			{
				var baseStrategies = base.GetStrategies();
				var strategyList = new List<IBusinessObjectStrategy>(baseStrategies);
				strategyList.Add(new CommissionSourceMonitorStrategy(new[] { JS_PackingModeInfo, JS_RL_NKOriginInfo, JS_RL_NKDestinationInfo }, this));
				strategies = strategyList.ToArray();
			}

			return strategies;
		}

		#endregion

		#region DefaultSailing

		public void DefaultSailing()
		{
			if (!IsSettingSailing && !((ISupportDataImporting)this).IsImportingData)
			{
				var mainLegs = Transports.Cast<Transport>()
					.Where(t => t.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);

				var legCount = mainLegs.Count();
				if (legCount == 1 && JS_JX != mainLegs.First().JW_JX)
				{
					JS_JX = mainLegs.First().JW_JX;
				}
				else if (legCount == 0 && !JS_JX.IsEmpty)
				{
					JS_JX = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region OnShipmentStatusUpdate

		public event EventHandler<ShipmentStatusEventArgs> OnShipmentStatusUpdate;

		protected ZBool IsOnShipmentStatusUpdateRunnable()
		{
			return OnShipmentStatusUpdate != null;
		}

		protected ZString GetShipmentStatusUpdateReason(string shipmentStatusValue)
		{
			var eventArgs = new ShipmentStatusEventArgs(shipmentStatusValue);
			OnShipmentStatusUpdate?.Invoke(this, eventArgs);

			return eventArgs.StatusUpdatedReason;
		}

		#endregion

		#region IsReceivedElectronicBooking

		public ZBool IsReceivedElectronicBooking()
		{
			return Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.IsInDatabase
				&& x.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.New, out var newShipmentStatus) && newShipmentStatus == ShipmentStatusList.Codes.ElectronicBooking
				&& x.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Type, out var type) && type == Constants.EventReferenceMessageTypes.ShipmentStatus);
		}

		#endregion

		#region IsReceivedElectronicShippingInstruction

		public ZBool IsReceivedElectronicShippingInstruction()
		{
			return Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.IsInDatabase
				&& x.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.New, out var newShipmentStatus) && newShipmentStatus == ShipmentStatusList.Codes.ElectronicShippingInstruction
				&& x.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Type, out var type) && type == Constants.EventReferenceMessageTypes.ShipmentStatus);
		}

		#endregion

		#region GetShipmentStatusBeforeLastestEBookingCancellationRequest

		public ZString GetShipmentStatusBeforeLastestEBookingCancellationRequest()
		{
			var latestLog = Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc)
				.FirstOrDefault(log => !log.SL_IsCancelled && log.IsInDatabase && log.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode
					&& log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Type, out var type) && type == Constants.EventReferenceMessageTypes.ShipmentStatus
					&& log.Parameters.TryGetValue(Params.New, out var newShipmentStatus) && newShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest
					&& log.Parameters.TryGetValue(Params.Old, out var oldShipmentStatus) && !oldShipmentStatus.IsNullOrEmpty() && oldShipmentStatus != ShipmentStatusList.Codes.EBookingCancellationRequest);

			if (latestLog != null && latestLog.Parameters.TryGetValue(Params.Old, out var previousShipmentStatus))
			{
				return previousShipmentStatus;
			}

			return ZString.Empty;
		}

		#endregion

		#region PurposeDescription

		public ZString PurposeDescription { get; set; }

		#endregion

		#region Events

		public event EventHandler<ShowMessageOnGUIEventArgs> OnShowMessageOnGUI;

		public void ShowMessageOnGUI(ZString title, ZString message)
		{
			if (OnShowMessageOnGUI != null)
			{
				OnShowMessageOnGUI(this, new ShowMessageOnGUIEventArgs(title, message));
			}
		}

		#endregion
	}
}
