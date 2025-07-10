using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = CargoWise.EventReference.Constants;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;
using EventParameters = Enterprise.Core.Constants.EventReferenceParameterTypes;

namespace Enterprise.Freight.Business
{
	[UniversalCopyWithExtendedEntities]
	[IDontMindLoadingASubclassInstead]
	public class JobDocsAndCartage : AutoJobDocsAndCartage,
		IJobDocsAndCartage,
		IHaveServices,
		IHaveRequiredDocuments,
		IEventDatePropertyChecker
	{
		#region Schema

		public new class Schema : AutoJobDocsAndCartage.Schema
		{
			public const string JP_Calc_JobID = "JP_Calc_JobID";
		}

		#endregion

		public JobDocsAndCartage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ParentType = GetTypeFromPrefix(JP_ParentTableCode);
			this.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(ResetParentScreeningStatus);
		}

		#region TypeDecider

		public class JobDocsAndCartageTypeDecider : TypeDecider
		{
			public override Type GetTypeForNew()
			{
				if (!IsBeingCalledInternally)
				{
					throw new NotSupportedException("JobDocsAndCartage BizOs can only be instantiated through JobDocsAndCartage.New(IDocsAndCartageParent Parent).");
				}

				return GetTypeForBinding();
			}

			public override Type GetTypeForBinding()
			{
				return typeof(JobDocsAndCartage);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return GetTypeForBinding();
			}
		}

		public static readonly TypeDecider TypeDecider = new JobDocsAndCartageTypeDecider();

		[ThreadStatic]
		static bool IsBeingCalledInternally;

		#endregion

		#region Static Instantiation Members

		#region New

		public static JobDocsAndCartage New(IDocsAndCartageParent parent)
		{
			EnsureParentIsBizO(parent);
			BusinessObject bizO = (BusinessObject)parent;

			JobDocsAndCartage result = null;
			try
			{
				IsBeingCalledInternally = true;
				result = (JobDocsAndCartage)bizO.Factory.New(GetTypeForFactoryUse(parent.DocsAndCartageType));
			}
			finally
			{
				IsBeingCalledInternally = false;
			}
			result.ParentType = parent.DocsAndCartageParentType;
			result.InitialiseForeignKey(bizO);
			result.SetupEventsAndRegisterChild(parent);

			return result;
		}

		#endregion

		#region Load

		public static JobDocsAndCartage Load(IShipmentWithDocsAndCartage parent)
		{
			return Load(parent, false);
		}

		public static JobDocsAndCartage Load(IShipmentWithDocsAndCartage parent, bool fetchOnlyFromLocalCache)
		{
			EnsureParentIsBizO(parent);
			return LoadCore(parent, fetchOnlyFromLocalCache);
		}

		static JobDocsAndCartage LoadCore(IShipmentWithDocsAndCartage parent, bool fetchOnlyFromLocalCache)
		{
			BusinessObject bizO = (BusinessObject)parent;

			ZQuery filter = new ZQuery(JobDocsAndCartageSchema.JP_ParentID, parent.PK);
			filter.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;

			JobDocsAndCartage result = (JobDocsAndCartage)bizO.Factory.LoadTop1(GetTypeForFactoryUse(parent.DocsAndCartageType), filter);

			if (result != null)
			{
				result.ParentType = parent.DocsAndCartageParentType;
				result.SetupEventsAndRegisterChild(parent);
			}

			return result;
		}

		#endregion

		#region GetOrCreateDocsAndCartageFromParent

		public static JobDocsAndCartage GetOrCreateDocsAndCartageFromParent(CommonShipment shipment)
		{
			return GetOrCreateDocsAndCartageFromParent(shipment as BusinessObject);
		}

		public static JobDocsAndCartage GetOrCreateDocsAndCartageFromParent(IShipmentWithDocsAndCartage parent)
		{
			EnsureParentIsBizO(parent);

			return parent.Shipment != null ? parent.Shipment.DocsAndCartage : GetOrCreateDocsAndCartageFromParent((BusinessObject)parent);
		}

		static JobDocsAndCartage GetOrCreateDocsAndCartageFromParent(BusinessObject parent)
		{
			return Load((IShipmentWithDocsAndCartage)parent, !parent.IsInDatabase) ?? New((IDocsAndCartageParent)parent);
		}

		#endregion

		#region SwitchDocsAndCartageAndRemoveOriginal

		public static JobDocsAndCartage SwitchDocsAndCartageAndRemoveOriginal(IShipmentWithDocsAndCartage parent)
		{
			EnsureParentIsBizO(parent);

			JobDocsAndCartage result = Load(parent);
			if (parent.Shipment != null)
			{
				if (result != null)
				{
					((BusinessObject)parent).UnRegisterEditableChildObject(result);
					result.Delete();
				}
				result = parent.Shipment.DocsAndCartage;
			}

			return result;
		}

		#endregion

		#region Static Implementation

		static Type GetTypeForFactoryUse(Type specifiedType)
		{
			Type defaultType = typeof(JobDocsAndCartage);
			return (defaultType.IsAssignableFrom(specifiedType)) ? specifiedType : defaultType;
		}

		static void EnsureParentIsBizO(IDocsAndCartageParent parent)
		{
			if (!(parent is BusinessObject))
			{
				throw new NotSupportedException("Caller must be a valid BusinessObject.");
			}
		}

		#endregion

		#endregion

		#region Parent and ForeignKey Details

		public IShipmentWithDocsAndCartage Parent
		{
			get { return (JP_ParentID.IsEmpty) ? null : (IShipmentWithDocsAndCartage)Factory.Load(ParentType, JP_ParentID); }
		}

		protected Type ParentType
		{
			get; set;
		}

		void InitialiseForeignKey(BusinessObject bizO)
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				JP_ParentID = bizO.PK;
				JP_ParentTableCode = bizO.TablePrefix;
			}
		}

		[BusinessObjectTestExclude]
		public override ZGuid JP_ParentID
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JP_ParentID; }
			set
			{
				base.JP_ParentID = value;
				RequiredDocuments.MarkAsNeedingValidation();
			}
		}

		[List("Lookups.JP_ExportStatementList")]
		public override ZString JP_ExportStatement
		{
			get { return base.JP_ExportStatement; }
			set { base.JP_ExportStatement = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString JP_ParentTableCode
		{
			get { return base.JP_ParentTableCode; }
			set { base.JP_ParentTableCode = value; }
		}

		public CommonShipment ShipmentInParent
		{
			get { return Parent == null ? null : Parent.Shipment; }
		}

		Type GetTypeFromPrefix(string prefix)
		{
			var actualPrefix = string.IsNullOrEmpty(prefix) ? JobShipmentSchema.Constants.Prefix : prefix;

			if (PrefixToTypeHash == null)
			{
				PrefixToTypeHash = new Dictionary<string, Type>();
				PrefixToTypeHash[JobShipmentSchema.Constants.Prefix] = typeof(CommonShipment);
				PrefixToTypeHash[JobDeclarationSchema.Constants.Prefix] = ObjectFactory.GetType<IBaseJobDeclaration>();
			}

			Type result;
			PrefixToTypeHash.TryGetValue(actualPrefix, out result);

			return result;
		}

		protected Dictionary<string, Type> PrefixToTypeHash;

		void ResetParentScreeningStatus(object sender, HasChangesChangedEventArgs e)
		{
			if (!IsDeleted && HasChanges)
			{
				IShouldUpdateScreeningStatus parentProvider = null;

				try
				{
					parentProvider = Parent as IShouldUpdateScreeningStatus;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					//ignore errors
				}

				if (parentProvider != null && (!(parentProvider is CommonShipment shipment) || shipment.JS_ScreeningStatus != ScreeningStatusesList.Codes.JobCleared))
				{
					parentProvider.ShouldUpdateScreeningStatus = true;
				}
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobDocsAndCartageFetchStrategy(this);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			Services.RemoveAndDeleteAll();
			RequiredDocuments.RemoveAndDeleteAll();
			OrderItems.RemoveAndDeleteAll();
			DeleteCallStack = System.Environment.StackTrace; // WI00183202
			base.Delete();
		}

		public String DeleteCallStack;

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();

			if (JP_ParentID.IsEmpty || JP_ParentID == Guid.Empty)
			{
				var message = string.Format(CultureInfo.InvariantCulture, JobDocsAndCartageSchema.JP_ParentID + (NoResString)" is empty or Invalid while saving JobDocsAndCartage, call stack: {0}", System.Environment.StackTrace);
				ErrorReporter.ReportOnce("Parent ID is empty or Invalid.", message);
			}

			if (JP_FCLAvailableInfo.HasChanges || JP_LCLAvailableInfo.HasChanges)
			{
				var availableDate = AvailableDate.ToOffset(); // Assuming AvailableDate is local to current branch
				var parent = Parent;
				var parentShipment = parent.Shipment;
				if (parentShipment != null)
				{
					parentShipment.Logs.CreateRecreateOrUpdateEventLog(
						Events.CargoAvailable,
						EstimateActual.Actual,
						availableDate,
						ZString.Empty,
						parentShipment.GetParametersForEvent(Events.CargoAvailable).ToArray());
				}

				if (parent is IBaseJobDeclaration declaration && declaration is IStmALogParent declarationLogParent)
				{
					declarationLogParent.Logs.CreateRecreateOrUpdateEventLog(
						Events.CargoAvailable,
						EstimateActual.Actual,
						availableDate);
				}
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (fOrderItems != null)
			{
				OrderItems.ApplyCurrentSortOrder();
			}
		}

		public override bool IsSavedByFactory
		{
			get { return fIsPersistent && base.IsSavedByFactory; }
		}

		bool fIsPersistent = true;

		public void MakeNonPersistent()
		{
			fIsPersistent = false;
		}

		#endregion

		#region Clone

		public void CopyPersistentValuesFrom(JobDocsAndCartage sourceObject)
		{
			using (SuspendSettingHasChanges())
			{
				base.CopyPersistentValuesFrom(sourceObject);
				JP_OrderItemsAsString = sourceObject.JP_OrderItemsAsString;
			}
		}

		/// <summary>
		/// To prevent BusinessObject.Clone being used.
		/// </summary>
		public new int Clone()
		{
			return 0;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());

			result.Add(JobDocsAndCartageSchema.Constants.JP_DeliveryCartageCompleted);
			result.Add(JobDocsAndCartageSchema.Constants.JP_PickupCartageCompleted);
			result.Add(JobDocsAndCartageSchema.Constants.JP_ParentID);

			if (Parent is IBaseJobDeclaration)
			{
				result.Add(JobDocsAndCartageSchema.Constants.JP_ExportStatement);
			}

			return result;
		}

		public JobDocsAndCartage Clone(IDocsAndCartageParent newParent)
		{
			JobDocsAndCartage result = New(newParent);
			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.CopyPersistentValuesFrom(this);
				result.InitialiseForeignKey((BusinessObject)newParent);
			}
			return result;
		}

		#endregion

		#region Validation

		protected override JobDocsAndCartageValidation GetNewValidation()
		{
			JobDocsAndCartageValidation result = base.GetNewValidation();
			if (Parent != null)
			{
				JobDocsAndCartageValidation piggyBackValidation = Parent.PiggyBackedValidation;
				if (piggyBackValidation != null)
				{
					result.Add(piggyBackValidation);
				}
			}
			return result;
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JP_PrintOptionForPackagesOnAWB = Env.Registry.Freight.AirWaybill.HAWBDimensionsDefault;
		}

		#endregion

		#region Properties

		[List("Lookups.PickupEquipmentNeededList")]
		public override ZString JP_FCLPickupEquipmentNeeded
		{
			get { return base.JP_FCLPickupEquipmentNeeded; }
			set
			{
				ZString oldValue = base.JP_FCLPickupEquipmentNeeded;
				base.JP_FCLPickupEquipmentNeeded = value;

				if (oldValue != JP_FCLPickupEquipmentNeeded && ShipmentInParent != null)
				{
					DefaultDropModeOnConfirms(ShipmentInParent.PickupConfirms, oldValue, JP_FCLPickupEquipmentNeeded);
				}
			}
		}

		[List("Lookups.DeliveryEquipmentNeededList")]
		public override ZString JP_FCLDeliveryEquipmentNeeded
		{
			get { return base.JP_FCLDeliveryEquipmentNeeded; }
			set
			{
				ZString oldValue = base.JP_FCLDeliveryEquipmentNeeded;
				base.JP_FCLDeliveryEquipmentNeeded = value;

				if (oldValue != JP_FCLDeliveryEquipmentNeeded && ShipmentInParent != null)
				{
					DefaultDropModeOnConfirms(ShipmentInParent.DeliveryConfirms, oldValue, JP_FCLDeliveryEquipmentNeeded);
				}
			}
		}

		void DefaultDropModeOnConfirms(CommonPickupDeliveryConfirmCollection confirms, ZString originalDropMode, ZString newDropMode)
		{
			foreach (CommonPickupDeliveryConfirm confirm in confirms)
			{
				if (confirm.EU_DropMode.IsEmpty || confirm.EU_DropMode == originalDropMode)
				{
					confirm.EU_DropMode = newDropMode;
				}
			}
		}

		#region JP_EstimatedPickup

		[EventDateProperty(AutoEvents.PickupCartageCompleteFinalisedCode, EstimateActual.Estimate)]
		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_EstimatedPickup
		{
			get { return new ZDateTime(base.JP_EstimatedPickup, DateTimeKind.Unspecified); }
			set
			{
				if (!CompareDatesToNearestMinute(base.JP_EstimatedPickup, value))
				{
					var originalValue = base.JP_EstimatedPickup;
					base.JP_EstimatedPickup = value;

					if (ShouldDefaultContainersDepartureFromParent)
					{
						DefaultContainersPickupOrDeliveryDateFromParent(CommonContainer.Schema.JC_DepartureEstimatedPickup, originalValue, value);
					}

					var shipment = Parent as CommonShipment;
					if (shipment != null)
					{
						ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned, originalValue, value, shipment.AutoCreateLooseConfirmations);
					}

					var parent = Parent as IStmALogParent;
					if (parent != null)
					{
						var eventTime = GetZDateTimeOffsetBasedOnDocAddressTypeFromShipmentInParent(value, DocAddressType.ConsignorPickupDeliveryAddress);
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.PickupCartageCompleteFinalised, EstimateActual.Estimate, eventTime);
					}
				}
			}
		}

		#endregion

		#region Default Containers Pickup or Delivery Date From Parent

		void DefaultContainersPickupOrDeliveryDateFromParent(string containerPropertyToUpdate, ZDateTime originalValue, ZDateTime newValue)
		{
			if (!newValue.IsValid)
			{
				return;
			}

			var shipment = Parent as CommonShipment;
			if (shipment != null)
			{
				var dateInfosWithEmptyValue = from containerDateInfo in
												  from CommonContainer container in shipment.Containers.ToArray()
												  where container.GetParentShipments().Take(2).Count() == 1
												  select container.FindPropertyInfo(containerPropertyToUpdate)
											  where containerDateInfo.Value.IsEmpty
											  select containerDateInfo;

				foreach (var dateInfo in dateInfosWithEmptyValue)
				{
					dateInfo.Value = newValue;
				}
			}
			else if (Parent is IBaseJobDeclaration)
			{
				var dateInfosWithEmptyOrOriginalValue = from containerDateInfo in
															from container in GetContainersFromDeclaration((BusinessObject)Parent)
															select container.FindPropertyInfo(containerPropertyToUpdate)
														where containerDateInfo.Value.IsEmpty || (ZDateTime)containerDateInfo.Value == originalValue
														select containerDateInfo;

				foreach (var dateInfo in dateInfosWithEmptyOrOriginalValue)
				{
					dateInfo.Value = newValue;
				}
			}
		}

		ZBool ShouldDefaultContainersDepartureFromParent
		{
			get
			{
				var shipment = Parent as CommonShipment;
				if (shipment?.DepartureConsol != null)
				{
					return !(ContainerModes.IsLCLType(shipment.JS_PackingMode) && ContainerModes.IsFCLType(shipment.DepartureConsol.JK_ConsolMode));
				}
				return true;
			}
		}

		ZBool ShouldDefaultContainersArrivalFromParent
		{
			get
			{
				var shipment = Parent as CommonShipment;
				if (shipment?.ArrivalConsol != null)
				{
					return !(ContainerModes.IsLCLType(shipment.JS_PackingMode) && ContainerModes.IsFCLType(shipment.ArrivalConsol.JK_ConsolMode));
				}
				return true;
			}
		}

		#endregion

		#region JP_EstimatedDelivery

		protected bool JP_EstimatedDelivery_ReadOnly => !Env.Security.MaintainShipmentEstimatedDeliveryDateOverride.IsAllowed;

		[EventDateProperty(AutoEvents.DeliveryCartageCompleteFinalisedCode, EstimateActual.Estimate)]
		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_EstimatedDelivery
		{
			get { return new ZDateTime(base.JP_EstimatedDelivery, DateTimeKind.Unspecified); }
			set
			{
				if (!CompareDatesToNearestMinute(base.JP_EstimatedDelivery, value))
				{
					var originalValue = base.JP_EstimatedDelivery;
					base.JP_EstimatedDelivery = value;

					if (ShouldDefaultContainersArrivalFromParent)
					{
						DefaultContainersPickupOrDeliveryDateFromParent(CommonContainer.Schema.JC_ArrivalEstimatedDelivery, originalValue, value);
					}

					var shipment = Parent as CommonShipment;
					if (shipment != null)
					{
						ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned, originalValue, value, shipment.AutoCreateLooseConfirmations);
					}

					var parent = Parent as IStmALogParent;
					if (parent != null)
					{
						var eventTime = GetZDateTimeOffsetBasedOnDocAddressTypeFromShipmentInParent(value, DocAddressType.ConsigneePickupDeliveryAddress);
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.DeliveryCartageCompleteFinalised, EstimateActual.Estimate, eventTime);
					}
				}
			}
		}

		#endregion

		#region SuspendSettingShipmentDates

		#region SuspendSettingContainerDates

		public IDisposable SuspendSettingShipmentDates()
		{
			return new SetShipmentDatesSuspender(this);
		}

		sealed class SetShipmentDatesSuspender : IDisposable
		{
			public SetShipmentDatesSuspender(JobDocsAndCartage shipment)
			{
				if (shipment == null)
				{
					throw new ArgumentNullException(nameof(shipment));
				}

				this.shipment = shipment;
				shipment.fSetShipmentDatesSuspensionLevel++;
			}

			#region IDisposable Members

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					shipment.fSetShipmentDatesSuspensionLevel--;
				}
			}

			bool disposed;

			#endregion

			readonly JobDocsAndCartage shipment;
		}

		internal int SetShipmentDatesSuspensionLevel
		{
			get { return fSetShipmentDatesSuspensionLevel; }
		}
		int fSetShipmentDatesSuspensionLevel;

		#endregion

		#endregion

		#region JP_PickupCartageCompleted

		[EventDateProperty(AutoEvents.PickupCartageCompleteFinalisedCode, EstimateActual.Actual)]
		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_PickupCartageCompleted
		{
			get
			{
				return new ZDateTime(base.JP_PickupCartageCompleted, DateTimeKind.Unspecified);
			}
			set
			{
				if (base.JP_PickupCartageCompleted != value)
				{
					var originalValue = base.JP_PickupCartageCompleted;
					base.JP_PickupCartageCompleted = value;

					if (ShouldDefaultContainersDepartureFromParent)
					{
						DefaultContainersPickupOrDeliveryDateFromParent(CommonContainer.Schema.JC_DepartureCartageComplete, originalValue, value);
					}

					var shipment = Parent as CommonShipment;
					if (shipment != null)
					{
						ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual, originalValue, value, true);
					}

					var parent = Parent as IStmALogParent;
					if (parent != null)
					{
						var eventTime = GetZDateTimeOffsetBasedOnDocAddressTypeFromShipmentInParent(value, DocAddressType.ConsignorPickupDeliveryAddress);
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.PickupCartageCompleteFinalised, EstimateActual.Actual, eventTime);
					}
				}
			}
		}

		#endregion

		#region JP_DeliveryCartageCompleted

		[EventDateProperty(AutoEvents.DeliveryCartageCompleteFinalisedCode, EstimateActual.Actual)]
		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_DeliveryCartageCompleted
		{
			get
			{
				return new ZDateTime(base.JP_DeliveryCartageCompleted, DateTimeKind.Unspecified);
			}
			set
			{
				if (base.JP_DeliveryCartageCompleted != value)
				{
					var originalValue = base.JP_DeliveryCartageCompleted;
					base.JP_DeliveryCartageCompleted = value;

					if (ShouldDefaultContainersArrivalFromParent)
					{
						DefaultContainersPickupOrDeliveryDateFromParent(CommonContainer.Schema.JC_ArrivalCartageComplete, originalValue, value);
					}

					var shipment = Parent as CommonShipment;
					if (shipment != null)
					{
						ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual, originalValue, value, true);
					}
					var parent = Parent as IStmALogParent;
					if (parent != null)
					{
						var eventTime = GetZDateTimeOffsetBasedOnDocAddressTypeFromShipmentInParent(value, DocAddressType.ConsigneePickupDeliveryAddress);
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.DeliveryCartageCompleteFinalised, EstimateActual.Actual, eventTime);
					}
				}
			}
		}

		#endregion

		#region JP_PickupCartageAdvised

		[EventDateProperty(Events.PickupCartageAdvisedCode, EstimateActual.Actual)]
		[EventDateProperty(Events.BookingRequestedCode, EstimateActual.Actual)]
		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_PickupCartageAdvised
		{
			get { return new ZDateTime(base.JP_PickupCartageAdvised, DateTimeKind.Unspecified); }
			set
			{
				if (base.JP_PickupCartageAdvised != value)
				{
					var originalValue = base.JP_PickupCartageAdvised;
					base.JP_PickupCartageAdvised = value;

					if (ShouldDefaultContainersDepartureFromParent)
					{
						DefaultContainersPickupOrDeliveryDateFromParent(CommonContainer.Schema.JC_DepartureCartageAdvised, originalValue, value);
					}

					if (Parent is IStmALogParent parent)
					{
						var valueWithOffset = value.ToOffset(); // Assuming the current branch is correct
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.PickupCartageAdvised, EstimateActual.Actual, valueWithOffset);
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.BookingRequested, EstimateActual.Actual, valueWithOffset, transportBookingEventReference ?? string.Empty,
							new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, EventParameters.PickupTransport));
					}
				}
			}
		}

		string transportBookingEventReference;

		public IDisposable TemporarilySetTransportBookingEventReference(string referenceNumber)
		{
			transportBookingEventReference = referenceNumber;
			return new DisposableAction(() => transportBookingEventReference = null);
		}

		#endregion

		#region JP_DeliveryCartageAdvised

		[EventDateProperty(Events.DeliveryCartageAdvisedCode, EstimateActual.Actual)]
		[EventDateProperty(Events.BookingRequestedCode, EstimateActual.Actual)]
		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_DeliveryCartageAdvised
		{
			get { return new ZDateTime(base.JP_DeliveryCartageAdvised, DateTimeKind.Unspecified); }
			set
			{
				if (base.JP_DeliveryCartageAdvised != value)
				{
					var originalValue = base.JP_DeliveryCartageAdvised;
					base.JP_DeliveryCartageAdvised = value;

					if (ShouldDefaultContainersArrivalFromParent)
					{
						DefaultContainersPickupOrDeliveryDateFromParent(CommonContainer.Schema.JC_ArrivalCartageAdvised, originalValue, value);
					}

					if (Parent is IStmALogParent parent)
					{
						var valueWithOffset = value.ToOffset(); // Assuming current branch is correct
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.DeliveryCartageAdvised, EstimateActual.Actual, valueWithOffset);
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.BookingRequested, EstimateActual.Actual, valueWithOffset, transportBookingEventReference ?? string.Empty,
							new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, EventParameters.DeliveryTransport));
					}
				}
			}
		}

		#endregion

		#region JP_PickupRequiredBy

		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_PickupRequiredBy
		{
			get { return new ZDateTime(base.JP_PickupRequiredBy, DateTimeKind.Unspecified); }
			set
			{
				if (base.JP_PickupRequiredBy != value)
				{
					ZDateTime originalValue = base.JP_PickupRequiredBy;
					base.JP_PickupRequiredBy = value;

					var shipment = Parent as CommonShipment;
					if (shipment != null)
					{
						ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy, originalValue, value, shipment.AutoCreateLooseConfirmations);
					}
				}
			}
		}

		#endregion

		#region JP_DeliveryRequiredBy

		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_DeliveryRequiredBy
		{
			get { return new ZDateTime(base.JP_DeliveryRequiredBy, DateTimeKind.Unspecified); }
			set
			{
				if (base.JP_DeliveryRequiredBy != value)
				{
					ZDateTime originalValue = base.JP_DeliveryRequiredBy;
					base.JP_DeliveryRequiredBy = value;

					var shipment = Parent as CommonShipment;
					if (shipment != null)
					{
						ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy, originalValue, value, shipment.AutoCreateLooseConfirmations);
					}
				}
			}
		}

		#endregion

		#region JP_PickupLabourTime

		[ZDateTimeDurationValueExclude1900]
		public override ZDateTime JP_PickupLabourTime
		{
			get { return new ZDateTime(base.JP_PickupLabourTime, DateTimeKind.Unspecified); }
			set { base.JP_PickupLabourTime = value.ConvertToDurationBasedDate(JP_PickupLabourTimeInfo); }
		}

		#endregion

		#region JP_DeliveryLabourTime

		[ZDateTimeDurationValue]
		public override ZDateTime JP_DeliveryLabourTime
		{
			get { return new ZDateTime(base.JP_DeliveryLabourTime, DateTimeKind.Unspecified); }
			set { base.JP_DeliveryLabourTime = value.ConvertToDurationBasedDate(JP_DeliveryLabourTimeInfo); }
		}

		#endregion

		#region JP_PickupTruckWaitTime

		[ZDateTimeDurationValueExclude1900]
		public override ZDateTime JP_PickupTruckWaitTime
		{
			get { return new ZDateTime(base.JP_PickupTruckWaitTime, DateTimeKind.Unspecified); }
			set { base.JP_PickupTruckWaitTime = value.ConvertToDurationBasedDate(JP_PickupTruckWaitTimeInfo); }
		}

		#endregion

		#region JP_DeliveryTruckWaitTime

		[ZDateTimeDurationValueExclude1900]
		public override ZDateTime JP_DeliveryTruckWaitTime
		{
			get { return new ZDateTime(base.JP_DeliveryTruckWaitTime, DateTimeKind.Unspecified); }
			set { base.JP_DeliveryTruckWaitTime = value.ConvertToDurationBasedDate(JP_DeliveryTruckWaitTimeInfo); }
		}

		#endregion

		#region JP_OrderItemsAsString

		[BusinessObjectTestExclude]
		[MaxLength(1000000)]
		public ZString JP_OrderItemsAsString
		{
			get { return orderItemsAsString ?? (orderItemsAsString = OrderItems.AsString); }
			set
			{
				if (orderItemsAsString != value)
				{
					isSettingOrderItemAsString = true;
					try
					{
						CheckMaximumLength(JP_OrderItemsAsStringInfo, value);
						orderItemsAsString = value;
						OrderItems.AsString = value;
						Validation.ValidateJP_OrderItemsAsString();
						if (!JP_OrderItemsAsStringInfo.HasErrors())
						{
							orderItemsAsString = OrderItems.AsString;
						}
						JP_OrderItemsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isSettingOrderItemAsString = false;
					}
				}
			}
		}

		string orderItemsAsString;
		bool isSettingOrderItemAsString;

		public ZPropertyInfo JP_OrderItemsAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(JP_OrderItemsAsString)); }
		}

		#endregion

		#region JP_StorageTimeUnits

		public ZString JP_StorageTimeUnits
		{
			get
			{
				if (Parent == null)
				{
					return ZString.Empty;
				}
				return Parent.TransportMode != Core.Constants.TransportModes.Air ? Res.GetString("Shipment|JP_StorageTimeUnits|Days", "Days") : Res.GetString("Shipment|JP_StorageTimeUnits|Hours", "Hours");
			}
		}

		public ZPropertyInfo JP_StorageTimeUnitsInfo
		{
			get { return GetZPropertyInfo(nameof(JP_StorageTimeUnits)); }
		}

		#endregion

		#region JP_FCLDeliveryDetentionCharge

		public override ZDecimal JP_FCLDeliveryDetentionCharge
		{
			get
			{
				return GetValueFromShipmentPenalty(
					container => container?.DeliveryPenalties?.FindOrCreateArrivalCarrierDetentionPenalty(false)?.CPY_PerUnitCost ?? ZDecimal.Zero,
					base.JP_FCLDeliveryDetentionCharge,
					Core.Constants.ContainerDetentionDirection.Import);
			}
			set
			{
				if (!UpdateShipmentPenalties(container =>
					{
						var arrivalCarrierDetentionPenalty = container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(true, false);

						if (arrivalCarrierDetentionPenalty != null)
						{
							arrivalCarrierDetentionPenalty.CPY_PerUnitCost = value;
						}
					}, Core.Constants.ContainerDetentionDirection.Import))
				{
					base.JP_FCLDeliveryDetentionCharge = value;
				}
			}
		}

		#endregion

		#region JP_FCLDeliveryDetentionDays

		public override ZByte JP_FCLDeliveryDetentionDays
		{
			get
			{
				return GetValueFromShipmentPenalty(
					container => container?.DeliveryPenalties?.FindOrCreateArrivalCarrierDetentionPenalty(false)?.DurationAsDays ?? ZByte.Zero,
					base.JP_FCLDeliveryDetentionDays,
					Core.Constants.ContainerDetentionDirection.Import);
			}
			set
			{
				if (!UpdateShipmentPenalties(container =>
					{
						var arrivalCarrierDetentionPenalty = container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(true, false);

						if (arrivalCarrierDetentionPenalty != null)
						{
							arrivalCarrierDetentionPenalty.DurationAsDays = value;
						}
					}, Core.Constants.ContainerDetentionDirection.Import))
				{
					base.JP_FCLDeliveryDetentionDays = value;
				}
			}
		}

		#endregion

		#region JP_FCLDeliveryDetentionFreeDays

		public override ZByte JP_FCLDeliveryDetentionFreeDays
		{
			get
			{
				return GetValueFromShipmentPenalty(
					container => container?.DeliveryPenalties?.FindOrCreateArrivalCarrierDetentionPenalty(false)?.FreeTimeAsDays ?? ZByte.Zero,
					base.JP_FCLDeliveryDetentionFreeDays,
					Core.Constants.ContainerDetentionDirection.Import);
			}
			set
			{
				if (!UpdateShipmentPenalties(container =>
					{
						var arrivalCarrierDetentionPenalty = container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(true, false);

						if (arrivalCarrierDetentionPenalty != null)
						{
							arrivalCarrierDetentionPenalty.FreeTimeAsDays = value;
						}
					}, Core.Constants.ContainerDetentionDirection.Import))
				{
					base.JP_FCLDeliveryDetentionFreeDays = value;
				}
			}
		}

		#endregion

		#region JP_FCLPickupDetentionCharge

		public override ZDecimal JP_FCLPickupDetentionCharge
		{
			get
			{
				return GetValueFromShipmentPenalty(
					container => container?.PickupPenalties?.FindOrCreateDepartureCarrierDetentionPenalty(false)?.CPY_PerUnitCost ?? ZDecimal.Zero,
					base.JP_FCLPickupDetentionCharge,
					Core.Constants.ContainerDetentionDirection.Export);
			}
			set
			{
				if (!UpdateShipmentPenalties(container =>
					{
						var departureCarrierStoragePenalty = container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(true, false);

						if (departureCarrierStoragePenalty != null)
						{
							departureCarrierStoragePenalty.CPY_PerUnitCost = value;
						}
					}, Core.Constants.ContainerDetentionDirection.Export))
				{
					base.JP_FCLPickupDetentionCharge = value;
				}
			}
		}

		#endregion

		#region JP_FCLPickupDetentionDays

		public override ZByte JP_FCLPickupDetentionDays
		{
			get
			{
				return GetValueFromShipmentPenalty(
					container => container?.PickupPenalties?.FindOrCreateDepartureCarrierDetentionPenalty(false)?.DurationAsDays ?? ZByte.Zero,
					base.JP_FCLPickupDetentionDays,
					Core.Constants.ContainerDetentionDirection.Export);
			}
			set
			{
				if (!UpdateShipmentPenalties(container =>
					{
						var departureCarrierStoragePenalty = container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(true, false);

						if (departureCarrierStoragePenalty != null)
						{
							departureCarrierStoragePenalty.DurationAsDays = value;
						}
					}, Core.Constants.ContainerDetentionDirection.Export))
				{
					base.JP_FCLPickupDetentionDays = value;
				}
			}
		}

		#endregion

		#region JP_FCLPickupDetentionFreeDays

		public override ZByte JP_FCLPickupDetentionFreeDays
		{
			get
			{
				return GetValueFromShipmentPenalty(
					container => container?.PickupPenalties?.FindOrCreateDepartureCarrierDetentionPenalty(false)?.FreeTimeAsDays ?? ZByte.Zero,
					base.JP_FCLPickupDetentionFreeDays,
					Core.Constants.ContainerDetentionDirection.Export);
			}
			set
			{
				if (!UpdateShipmentPenalties(container =>
					{
						var departureCarrierStoragePenalty = container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(true, false);

						if (departureCarrierStoragePenalty != null)
						{
							departureCarrierStoragePenalty.FreeTimeAsDays = value;
						}
					}, Core.Constants.ContainerDetentionDirection.Export))
				{
					base.JP_FCLPickupDetentionFreeDays = value;
				}
			}
		}

		#endregion

		#region JP_LCLAirStorageCharge

		public override ZDecimal JP_LCLAirStorageCharge
		{
			get
			{
				return GetValueFromShipmentPenalty(
					container => container?.DeliveryPenalties?.FindOrCreateArrivalCarrierStoragePenalty(false)?.CPY_PerUnitCost ?? ZDecimal.Zero,
					base.JP_LCLAirStorageCharge,
					Core.Constants.ContainerDetentionDirection.Import);
			}
			set
			{
				if (!UpdateShipmentPenalties(container =>
					{
						var arrivalCarrierStoragePenalty = container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(true, false);

						if (arrivalCarrierStoragePenalty != null)
						{
							arrivalCarrierStoragePenalty.CPY_PerUnitCost = value;
						}
					}, Core.Constants.ContainerDetentionDirection.Import))
				{
					base.JP_LCLAirStorageCharge = value;
				}
			}
		}

		#endregion

		#region JP_LCLAirStorageDaysOrHours

		public override ZByte JP_LCLAirStorageDaysOrHours
		{
			get
			{
				return GetValueFromShipmentPenalty(
					container => container?.DeliveryPenalties?.FindOrCreateArrivalCarrierStoragePenalty(false)?.DurationAsDays ?? ZByte.Zero,
					base.JP_LCLAirStorageDaysOrHours,
					Core.Constants.ContainerDetentionDirection.Import);
			}
			set
			{
				if (!UpdateShipmentPenalties(container =>
					{
						var arrivalCarrierStoragePenalty = container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(true, false);

						if (arrivalCarrierStoragePenalty != null)
						{
							arrivalCarrierStoragePenalty.DurationAsDays = value;
						}
					}, Core.Constants.ContainerDetentionDirection.Import))
				{
					base.JP_LCLAirStorageDaysOrHours = value;
				}
			}
		}

		#endregion

		#region Container Penalties

		bool UpdateShipmentPenalties(Action<CommonContainer> penaltyAction, string direction)
		{
			if (IsShipmentPenalty(direction))
			{
				foreach (var container in ShipmentInParent.Containers)
				{
					penaltyAction(container);
				}

				return true;
			}

			return false;
		}

		T GetValueFromShipmentPenalty<T>(Func<CommonContainer, T> penaltyFunc, T cartageValue, string direction)
		{
			if (IsShipmentPenalty(direction))
			{
				return penaltyFunc(ShipmentInParent.Containers.FirstOrDefault());
			}

			return cartageValue;
		}

		bool IsShipmentPenalty(string direction)
		{
			return ShipmentInParent != null
			   && (ShipmentInParent.JS_PackingMode == ContainerModes.FCL || direction == Core.Constants.ContainerDetentionDirection.Import && ShipmentInParent.JS_PackingMode == ContainerModes.BuyersConsol && ShipmentInParent.JS_ShipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead)
			   && (ShipmentInParent.DepartureConsol != null && ShipmentInParent.DepartureConsol.JK_TransportMode == Core.Constants.TransportModes.Sea && direction == Core.Constants.ContainerDetentionDirection.Export || ShipmentInParent.ArrivalConsol != null && ShipmentInParent.ArrivalConsol.JK_TransportMode == Core.Constants.TransportModes.Sea && direction == Core.Constants.ContainerDetentionDirection.Import);
		}

		#endregion

		#region Calculated HXD properties

		public ZString JP_Calc_HXDNumber
		{
			get
			{
				JobRequiredDocument hXDDoc = RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.HeXiaoDan);
				return hXDDoc != null ? hXDDoc.EQ_DocNumber : ZString.Empty;
			}
		}

		public ZPropertyInfo JP_Calc_HXDNumberInfo
		{
			get { return GetZPropertyInfo(nameof(JP_Calc_HXDNumber)); }
		}

		public ZDateTime JP_Calc_HXDRcvdFromShipper
		{
			get
			{
				JobRequiredDocument hXDDoc = RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.HeXiaoDan);
				return hXDDoc != null ? hXDDoc.EQ_DateReceived.ToZDateTime() : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo JP_Calc_HXDRcvdFromShipperInfo
		{
			get { return GetZPropertyInfo(nameof(JP_Calc_HXDRcvdFromShipper)); }
		}

		public ZDateTime JP_Calc_HXDSentToBroker
		{
			get
			{
				JobRequiredDocument hXDDoc = RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.HeXiaoDan);
				return hXDDoc != null ? hXDDoc.EQ_SntToCustomsBroker : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo JP_Calc_HXDSentToBrokerInfo
		{
			get { return GetZPropertyInfo(nameof(JP_Calc_HXDSentToBroker)); }
		}

		public ZDateTime JP_Calc_HXDRcvdFromBroker
		{
			get
			{
				JobRequiredDocument hXDDoc = RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.HeXiaoDan);
				return hXDDoc != null ? hXDDoc.EQ_RcvFromCustomsBroker : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo JP_Calc_HXDRcvdFromBrokerInfo
		{
			get { return GetZPropertyInfo(nameof(JP_Calc_HXDRcvdFromBroker)); }
		}

		public ZDateTime JP_Calc_HXDReturnedToShipper
		{
			get
			{
				JobRequiredDocument hXDDoc = RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.HeXiaoDan);
				return hXDDoc != null ? hXDDoc.EQ_ReturnToShipper : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo JP_Calc_HXDReturnedToShipperInfo
		{
			get { return GetZPropertyInfo(nameof(JP_Calc_HXDReturnedToShipper)); }
		}

		#endregion

		#region Addresses

		public OrgHeader PickupCartageCo
		{
			get { return PickupCartageCoAddr != null ? PickupCartageCoAddr.Header : null; }
		}

		public OrgHeader DeliveryCartageCo
		{
			get { return DeliveryCartageCoAddr != null ? DeliveryCartageCoAddr.Header : null; }
		}

		[List("Lookups.LocalTransport_ListForPickup")]
		public ZGuid PickupCartageCoPK
		{
			get { return JP_OA_PickupCartageCoAddr_ZAddress.OrgPK; }
			set
			{
				JP_OA_PickupCartageCoAddr_ZAddress.OrgPK = value;
				PickupCartageCoPKInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidatePickupCartageCoPK();
					Validation.ValidateJP_OA_PickupCartageCoAddr();
				}
			}
		}

		public ZPropertyInfo PickupCartageCoPKInfo
		{
			get { return GetZPropertyInfo(nameof(PickupCartageCoPK)); }
		}

		[List("Lookups.LocalTransport_ListForDelivery")]
		public ZGuid DeliveryCartageCoPK
		{
			get { return JP_OA_DeliveryCartageCoAddr_ZAddress.OrgPK; }
			set
			{
				JP_OA_DeliveryCartageCoAddr_ZAddress.OrgPK = value;
				DeliveryCartageCoPKInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateDeliveryCartageCoPK();
					Validation.ValidateJP_OA_DeliveryCartageCoAddr();
				}
			}
		}

		public ZPropertyInfo DeliveryCartageCoPKInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryCartageCoPK)); }
		}

		public override ZGuid JP_OA_DeliveryCartageCoAddr
		{
			get { return base.JP_OA_DeliveryCartageCoAddr; }
			set
			{
				base.JP_OA_DeliveryCartageCoAddr = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDeliveryCartageCoPK();
					Validation.ValidateJP_OA_DeliveryCartageCoAddr();
				}
			}
		}

		protected override ZAddress GetNewJP_OA_DeliveryCartageCoAddr_ZAddress()
		{
			ZAddress result = base.GetNewJP_OA_DeliveryCartageCoAddr_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		public override ZGuid JP_OA_PickupCartageCoAddr
		{
			get { return base.JP_OA_PickupCartageCoAddr; }
			set
			{
				base.JP_OA_PickupCartageCoAddr = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePickupCartageCoPK();
					Validation.ValidateJP_OA_PickupCartageCoAddr();
				}
			}
		}

		protected override ZAddress GetNewJP_OA_PickupCartageCoAddr_ZAddress()
		{
			ZAddress result = base.GetNewJP_OA_PickupCartageCoAddr_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		ZDateTimeOffset GetZDateTimeOffsetBasedOnDocAddressTypeFromShipmentInParent(ZDateTime originalValue, DocAddressType docAddressType)
		{
			var result = originalValue.ToOffset();
			if (ShipmentInParent != null && !originalValue.IsEmpty && originalValue.IsValid)
			{
				var address = JobDocAddress.Load(ShipmentInParent, docAddressType);
				RefUNLOCO fallbackUnloco = null;
				if (docAddressType == DocAddressType.ConsignorPickupDeliveryAddress)
				{
					fallbackUnloco = ShipmentInParent.Origin;
				}
				else if (docAddressType == DocAddressType.ConsigneePickupDeliveryAddress)
				{
					fallbackUnloco = ShipmentInParent.Destination;
				}
				var unloco = fallbackUnloco;
				if (address is ILocation location && location?.UNLOCO != null)
				{
					unloco = location?.UNLOCO;
				}
				if (unloco != null)
				{
					var utcOffsetOfAddress = unloco.TimeZoneSet?.GetCalculationTimeZone().GetUtcOffsetBasedOnLocal(originalValue.ToDateTime());
					if (utcOffsetOfAddress.HasValue)
					{
						result = new ZDateTimeOffset(new ZDateTime(originalValue, DateTimeKind.Unspecified), utcOffsetOfAddress.Value);
					}
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Services

		[ChildEditable(true)]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (fServices == null)
				{
					fServices = GetNewServiceCollection();
					fServices.Load(new ZQuery(JobServiceSchema.ES_ParentID, PK));
					RegisterEditableChildObject(fServices);
				}
				return fServices;
			}
		}
		JobServiceDependentCollection fServices;

		protected virtual JobServiceDependentCollection GetNewServiceCollection()
		{
			return new JobServiceDependentCollection(this, Factory);
		}

		#endregion

		#region Required Documents

		[ChildEditable(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = GetNewRequiredDocumentCollection();
					fRequiredDocuments.Load();
					RegisterEditableChildObject(fRequiredDocuments);
				}
				return fRequiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection fRequiredDocuments;

		protected virtual JobRequiredDocumentDependentCollection GetNewRequiredDocumentCollection()
		{
			return new JobRequiredDocumentDependentCollection(this, Factory);
		}

		#endregion

		#region Order Items

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(JobOrderItemSchema.Constants.TableName, JobOrderItemSchema.Constants.JT_JP)]
		public OrderItemCollection OrderItems
		{
			get
			{
				if (fOrderItems == null)
				{
					fOrderItems = new OrderItemCollection(this, Factory);
					fOrderItems.Load();
					RegisterEditableChildObject(fOrderItems);

					fOrderItems.CountChanged += new CollectionCountChangedEventHandler(OnOrderItems_CountChanged);

					fOrderItems.OrderReferenceChanged -= new EventHandler(OnOrderReferenceChanged);
					fOrderItems.OrderReferenceChanged += new EventHandler(OnOrderReferenceChanged);
				}
				return fOrderItems;
			}
		}
		OrderItemCollection fOrderItems;

		void OnOrderReferenceChanged(object sender, EventArgs e)
		{
			if (!isSettingOrderItemAsString)
			{
				orderItemsAsString = null;
			}

			JP_OrderItemsAsStringInfo.RefreshBinding();
		}

		void OnOrderItems_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			JP_OrderItemsAsStringInfo.RefreshBinding();

			if (Parent != null && Parent.ConsigneeDocumentaryAddress != null && !Parent.ConsigneeDocumentaryAddress.IsValidationSuspended)
			{
				Parent.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			}
		}

		#endregion

		#endregion

		#region Available / Storage

		public ZDateTime AvailableDate
		{
			get
			{
				ZDateTime date;

				if (ShipmentInParent != null)
				{
					date = ShipmentInParent.IsArrivalContainerModeFCLorULD ? JP_FCLAvailable : JP_LCLAvailable;
				}
				else
				{
					date = !JP_LCLAvailable.IsEmpty ? JP_LCLAvailable : JP_FCLAvailable;
				}

				return date;
			}
		}

		#region UpdateAvailabilityAndStorageDatesForContainers

		bool updateAvailabilityAndStorageDatesForContainersPending;

		internal void UpdateAvailabilityAndStorageDatesForContainers()
		{
			updateAvailabilityAndStorageDatesForContainersPending = true;
		}

		void UpdateAvailabilityAndStorageDatesForContainersIfPending()
		{
			if (updateAvailabilityAndStorageDatesForContainersPending)
			{
				updateAvailabilityAndStorageDatesForContainersPending = false;
				UpdateAvailabilityAndStorageDatesForContainersNow();
			}
		}

		internal void UpdateAvailabilityAndStorageDatesForContainersNow()
		{
			base.JP_FCLAvailable = GetMinimumOfDatesFromContainers(CommonContainer.Schema.JC_OverriddenFCLAvailable);
			base.JP_FCLStorageCommences = GetMinimumOfDatesFromContainers(CommonContainer.Schema.JC_OverriddenFCLStorage);
			base.JP_LCLAvailable = JP_LCLDatesOverrideConsol ? base.JP_LCLAvailable : GetMinimumOfDatesFromContainers(CommonContainer.Schema.JC_OverriddenLCLAvailable);
			base.JP_LCLStorageCommences = JP_LCLDatesOverrideConsol ? base.JP_LCLStorageCommences : GetMinimumOfDatesFromContainers(CommonContainer.Schema.JC_OverriddenLCLStorage);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			UpdateAvailabilityAndStorageDatesForContainersIfPending();
		}

		#endregion

		#region Updating Available / Storage Dates FromContainer

		internal void UpdateFCLAvailableDateFromContainer(ZDateTime newContainerDate)
		{
			base.JP_FCLAvailable = UpdateMinimumOfDatesFromContainerDateChange(CommonContainer.Schema.JC_OverriddenFCLAvailable, newContainerDate, base.JP_FCLAvailable);
		}

		internal void UpdateFCLStorageDateFromContainer(ZDateTime newContainerDate)
		{
			base.JP_FCLStorageCommences = UpdateMinimumOfDatesFromContainerDateChange(CommonContainer.Schema.JC_OverriddenFCLStorage, newContainerDate, base.JP_FCLStorageCommences);
		}

		internal void UpdateLCLAvailableDateFromContainer(ZDateTime newContainerDate)
		{
			if (!JP_LCLDatesOverrideConsol)
			{
				base.JP_LCLAvailable = UpdateMinimumOfDatesFromContainerDateChange(CommonContainer.Schema.JC_OverriddenLCLAvailable, newContainerDate, base.JP_LCLAvailable);
			}
		}

		internal void UpdateLCLStorageDateFromContainer(ZDateTime newContainerDate)
		{
			if (!JP_LCLDatesOverrideConsol)
			{
				base.JP_LCLStorageCommences = UpdateMinimumOfDatesFromContainerDateChange(CommonContainer.Schema.JC_OverriddenLCLStorage, newContainerDate, base.JP_LCLStorageCommences);
			}
		}

		internal ZBool IsSkipUpdateAvailableDateFromContainer(CommonConsol consol)
		{
			return consol != null && Parent is CommonShipment && ShipmentInParent.ArrivalConsol?.PK != consol.PK;
		}

		ZDateTime UpdateMinimumOfDatesFromContainerDateChange(string containerDateOverridenValuePropertyName, ZDateTime newContainerDate, ZDateTime currentMinimumContainerAvailableOrStorageDate)
		{
			ZDateTime result = currentMinimumContainerAvailableOrStorageDate;
			if (newContainerDate.IsValid || newContainerDate.IsEmpty)
			{
				if (currentMinimumContainerAvailableOrStorageDate.IsEmpty || newContainerDate < currentMinimumContainerAvailableOrStorageDate)
				{
					result = newContainerDate;
				}
				else if (newContainerDate.IsEmpty || newContainerDate > currentMinimumContainerAvailableOrStorageDate)
				{
					result = GetMinimumOfDatesFromContainers(containerDateOverridenValuePropertyName);
				}
			}
			return result;
		}

		ZDateTime GetMinimumOfDatesFromContainers(string containerDateOverridenValuePropertyName)
		{
			ZDateTime result = ZDateTime.Empty;
			foreach (CommonContainer container in Containers)
			{
				ZDateTime containerDate = (ZDateTime)container[containerDateOverridenValuePropertyName];
				if (result.IsEmpty || (!containerDate.IsEmpty && containerDate < result))
				{
					result = containerDate;
				}
			}
			return result;
		}

		IEnumerable<CommonContainer> Containers
		{
			get
			{
				IEnumerable<CommonContainer> result = Array.Empty<CommonContainer>();
				if (ShipmentInParent != null)
				{
					result = GetContainersFromShipment(ShipmentInParent);
				}
				else if (ObjectFactory.GetType<IBaseJobDeclaration>().IsInstanceOfType(Parent))
				{
					BusinessObject declaration = Parent as BusinessObject;
					result = GetContainersFromDeclaration(declaration);
				}
				return result;
			}
		}

		IEnumerable<CommonContainer> GetContainersFromShipment(CommonShipment shipment)
		{
			if (shipment.ArrivalConsol != null &&
				shipment.OuterPackLines.Count == 0 &&
				shipment.ArrivalConsol.Containers.Count == 1)
			{
				foreach (CommonContainer container in shipment.ArrivalConsol.Containers)
				{
					yield return container;
				}
			}
			else
			{
				foreach (CommonContainer container in shipment.Containers)
				{
					yield return container;
				}
			}
		}

		IEnumerable<CommonContainer> GetContainersFromDeclaration(BusinessObject declaration)
		{
			BusinessObjectCollection cusContainers = (BusinessObjectCollection)declaration["CusContainers"];
			foreach (BusinessObject cusContainer in cusContainers)
			{
				CommonContainer container = (CommonContainer)cusContainer["JobContainer"];
				if (container != null)
				{
					yield return container;
				}
			}
		}

		#endregion

		#region JP_FCLAvailable

		/// <summary>
		/// The persistent field in the database stores the minimum FCL available date for all
		/// containers packed for this shipment.
		/// 
		/// If you set this value manually it may be overwritten.
		/// </summary>
		[ReadOnly(true)]
		public override ZDateTime JP_FCLAvailable
		{
			get
			{
				UpdateAvailabilityAndStorageDatesForContainersIfPending();
				return new ZDateTime(GetAvailableOrStorageDate(base.JP_FCLAvailable, false, JobConsolTransportSchema.JW_TerminalAvailabilityDate.Name, JobContainerSchema.JC_OverrideFCLAvailableStorage.Name), DateTimeKind.Unspecified);
			}
		}

		#endregion

		#region JP_FCLStorageCommences

		/// <summary>
		/// The persistent field in the database stores the minimum FCL storage date for all
		/// containers packed for this shipment.
		/// 
		/// If you set this value manually it may be overwritten.
		/// </summary>
		[ReadOnly(true)]
		public override ZDateTime JP_FCLStorageCommences
		{
			get
			{
				UpdateAvailabilityAndStorageDatesForContainersIfPending();
				return new ZDateTime(GetAvailableOrStorageDate(base.JP_FCLStorageCommences, false, JobConsolTransportSchema.JW_TerminalStorageDate.Name, JobContainerSchema.JC_OverrideFCLAvailableStorage.Name), DateTimeKind.Unspecified);
			}
		}

		#endregion

		#region JP_LCLAvailable

		/// <summary>
		/// The persistent field in the database stores the minimum LCL available date for all
		/// containers packed for this shipment, unless the value is overridden in which
		/// case the overridden value is stored.
		/// </summary>
		public override ZDateTime JP_LCLAvailable
		{
			get
			{
				UpdateAvailabilityAndStorageDatesForContainersIfPending();
				return new ZDateTime(GetAvailableOrStorageDate(base.JP_LCLAvailable, JP_LCLDatesOverrideConsol, JobConsolTransportSchema.JW_DepotAvailabilityDate.Name, JobContainerSchema.JC_OverrideLCLAvailableStorage.Name), DateTimeKind.Unspecified);
			}
			set
			{
				if (value.IsValid && !JP_LCLDatesOverrideConsol)
				{
					JP_LCLDatesOverrideConsol = true;
				}
				base.JP_LCLAvailable = value;
			}
		}

		protected bool JP_LCLAvailable_ReadOnly
		{
			get { return !JP_LCLDatesOverrideConsol; }
		}

		#endregion

		#region JP_LCLStorageCommences

		/// <summary>
		/// The persistent field in the database stores the minimum LCL storage date for all
		/// containers packed for this shipment, unless the value is overridden in which
		/// case the overridden value is stored.
		/// </summary>
		public override ZDateTime JP_LCLStorageCommences
		{
			get
			{
				UpdateAvailabilityAndStorageDatesForContainersIfPending();
				return new ZDateTime(GetAvailableOrStorageDate(base.JP_LCLStorageCommences, JP_LCLDatesOverrideConsol, JobConsolTransportSchema.JW_DepotStorageDate.Name, JobContainerSchema.JC_OverrideLCLAvailableStorage.Name), DateTimeKind.Unspecified);
			}
			set
			{
				if (value.IsValid && !JP_LCLDatesOverrideConsol)
				{
					JP_LCLDatesOverrideConsol = true;
				}
				base.JP_LCLStorageCommences = value;
			}
		}

		protected bool JP_LCLStorageCommences_ReadOnly
		{
			get { return !JP_LCLDatesOverrideConsol; }
		}

		#endregion

		public override ZBool JP_LCLDatesOverrideConsol
		{
			get { return base.JP_LCLDatesOverrideConsol; }
			set
			{
				if (JP_LCLDatesOverrideConsol != value)
				{
					ZDateTime newLCLAvailable = JP_LCLAvailable;
					ZDateTime newLCLStorage = JP_LCLStorageCommences;
					base.JP_LCLDatesOverrideConsol = value;
					if (value)
					{
						base.JP_LCLAvailable = newLCLAvailable;
						base.JP_LCLStorageCommences = newLCLStorage;
					}
					else
					{
						base.JP_LCLAvailable = GetMinimumOfDatesFromContainers(CommonContainer.Schema.JC_OverriddenLCLAvailable);
						base.JP_LCLStorageCommences = GetMinimumOfDatesFromContainers(CommonContainer.Schema.JC_OverriddenLCLStorage);
					}
				}
			}
		}

		ZDateTime GetAvailableOrStorageDate(ZDateTime overriddenValueOrMinimumOfDatesOnContainer, bool isOverride, string transportDatePropertyName, string containerDateOverridePropertyName)
		{
			ZDateTime result = overriddenValueOrMinimumOfDatesOnContainer;
			if (!isOverride && ArrivalTransport != null)
			{
				var transportDate = (ZDateTime)ArrivalTransport[transportDatePropertyName];

				if (result.IsEmpty)
				{
					result = transportDate;
				}

				else if (!transportDate.IsEmpty && transportDate < result)
				{
					var containerDatesNotOverriden = Containers.Any(container => !(ZBool)container[containerDateOverridePropertyName]);

					if (containerDatesNotOverriden || !Containers.Any())
					{
						result = transportDate;
					}
				}
			}
			return result;
		}

		Transport ArrivalTransport
		{
			get
			{
				ITransportParent parent;

				if (ObjectFactory.GetType<IBaseJobDeclaration>().IsInstanceOfType(Parent))
				{
					parent = (ITransportParent)Parent;
				}
				else
				{
					parent = ShipmentInParent?.ArrivalConsol;
				}

				Transport lastLeg = null;
				if (parent != null)
				{
					lastLeg = new TransportOrderHelper(parent.Transports).LastLeg;
				}

				return lastLeg;
			}
		}

		#endregion

		#region IHaveServices Members

		ZString IHaveServices.TableCode
		{
			get { return JobDocsAndCartageSchema.Constants.Prefix; }
		}

		ZString IHaveServices.TransportMode
		{
			get { return Parent.TransportMode; }
		}

		ZString IHaveServices.ContainerMode
		{
			get { return Parent.ContainerMode; }
		}

		public virtual IHaveServices[] DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		BusinessObject IHaveServices.ServiceParent
		{
			get { return (BusinessObject)Parent; }
		}

		bool IHaveServices.NeedsServiceEvents
		{
			get { return ((Parent as IServicesParent)?.NeedsServiceEvents ?? false) || ShipmentInParent != null; }
		}

		IBranch IHaveServices.ServiceBranch => Env.CurrentBranch;

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		#endregion

		#region IHaveRequiredDocuments Members

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return JobDocsAndCartageSchema.Constants.Prefix; }
		}

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return Parent != null ? new ZString(Parent.UniqueConsignRef) : ZString.Empty; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return Parent != null ? new ZString(Parent.MasterBillNumber) : ZString.Empty; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return Parent != null ? new ZString(Parent.HouseBillNumber) : ZString.Empty; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return Parent != null ? Parent.ExportBroker : null; }
		}

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return (BusinessObject)Parent; }
		}

		public IReadOnlyList<ZString> AdditionalRefTypes
		{
			get { return Array.Empty<ZString>(); }
		}

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
			if (!IsDeleted)
			{
				var parentToThis = Parent as IParentToJobDocsAndCartage;
				if (parentToThis != null)
				{
					parentToThis.PreLogAllDocumentsReceivedEvents();
				}
			}
		}

		#endregion

		#region IEventDatePropertyChecker Members

		bool IEventDatePropertyChecker.CanUpdateProperty(IStmALog log, ZPropertyInfo property)
		{
			var canUpdate = true;

			if (log.SL_SE_NKEvent == Events.BookingRequested.Code)
			{
				if (log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Type, out var type))
				{
					canUpdate = type == EventParameters.PickupTransport && property == JP_PickupCartageAdvisedInfo
						|| type == EventParameters.DeliveryTransport && property == JP_DeliveryCartageAdvisedInfo;
				}
				else
				{
					canUpdate = false;
				}
			}

			return canUpdate;
		}

		#endregion

		#region Parent Events

		void SetupEventsAndRegisterChild(IDocsAndCartageParent parent)
		{
			if (parent != null && !parent.GetType().IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				((BusinessObject)parent).RegisterEditableChildObject(this);
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("151745ab-9387-4b33-90f5-9bc9b08001be", "Cartage Data");
			}
		}

		bool CompareDatesToNearestMinute(ZDateTime firstTime, ZDateTime secondTime)
		{
			if (firstTime == secondTime)
			{
				return true;
			}

			firstTime = firstTime.IsValid ? new ZDateTime(firstTime.Year, firstTime.Month, firstTime.Day, firstTime.Hour, firstTime.Minute, 0) : firstTime;
			secondTime = secondTime.IsValid ? new ZDateTime(secondTime.Year, secondTime.Month, secondTime.Day, secondTime.Hour, secondTime.Minute, 0) : secondTime;

			return firstTime == secondTime;
		}
	}
}
