using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbRoutePlanner : NonPersistentBusinessObject
	{
		public DtbRoutePlanner(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Related Entities

		#region AddressPoints

		public DtbAddressPointCollection AddressPoints
		{
			get
			{
				if (addressPoints == null)
				{
					addressPoints = new DtbAddressPointCollection(Factory);
					RegisterEditableChildObject(addressPoints);
				}

				return addressPoints;
			}
		}

		DtbAddressPointCollection addressPoints;

		#endregion

		#region Carriers

		public ActiveBusinessObjectCollection<OrgHeader> Carriers
		{
			get
			{
				if (carriers == null)
				{
					var filterQuery = new ZQuery(OrgHeaderSchema.OH_IsActive, true);
					filterQuery.AddToFilter(new ZQuery(OrgHeaderSchema.OH_IsLocalTransport, true));
					filterQuery.AddToFilter(new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, true));
					carriers = new ActiveBusinessObjectCollection<OrgHeader>(Factory, filterQuery);
				}

				return carriers;
			}
		}

		ActiveBusinessObjectCollection<OrgHeader> carriers;

		#endregion

		#region Drivers

		public GlbStaffCollection Drivers
		{
			get
			{
				if (drivers == null)
				{
					drivers = new GlbStaffCollection(Factory, new AdhocCollectionRelationship(typeof(GlbStaff)));

					if (DriversGroup != null)
					{
						drivers.AddRange(DriversGroup.Staff.Cast<GlbStaff>().Where(s => s.GS_IsActive));
					}
				}

				return drivers;
			}
		}

		GlbStaffCollection drivers;

		GlbGroup DriversGroup
		{
			get
			{
				if (driversGroup == null)
				{
					var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
					driversGroup = Factory.Load<GlbGroup>(((GuidRegistryItem)transportRegistry.TransportDriversGroup).Value);
				}
				return driversGroup;
			}
		}

		GlbGroup driversGroup;

		#endregion

		#region RunSheets

		public DtbConsignmentRunSheetAdHocCollection RunSheetsFilteredForBinding // binding
		{
			get
			{
				if (runSheetsFilteredForBinding == null)
				{
					runSheetsFilteredForBinding = new DtbConsignmentRunSheetAdHocCollection(Factory);
					RunSheetsCacheFilteredByDate.CountChanged += (s, e) => FilterRunSheetsForBindingByCarrierDriverVehicleChildFilters();
					FilterRunSheetsForBindingByCarrierDriverVehicleChildFilters();
				}
				return runSheetsFilteredForBinding;
			}
		}

		DtbConsignmentRunSheetCollection RunSheetsCacheFilteredByDate // global cache
		{
			get
			{
				if (runSheets == null)
				{
					runSheets = new DtbConsignmentRunSheetCollection(Factory);
					FilterRunSheetsWithoutEntityFilters();
				}
				return runSheets;
			}
		}

		void FilterRunSheetsWithoutEntityFilters()
		{
			if (runSheets != null)
			{
				var query = GetRunSheetFilter_WithoutEntityFilter();
				runSheets.AdditionalFilter = query;
			}
		}

		void FilterRunSheetsForBindingByCarrierDriverVehicleChildFilters()
		{
			if (runSheetsFilteredForBinding != null)
			{
				RunSheetsFilteredForBinding.AdditionalFilter.Clear();
				RunSheetsFilteredForBinding.Relationship.Clear();
				RunSheetsFilteredForBinding.AddRange(RunSheetsCacheFilteredByDate);

				if (RunSheetsCarrierDriverOrVehicleQuery != null)
				{
					RunSheetsFilteredForBinding.AdditionalFilter = new ZQuery(RunSheetsCarrierDriverOrVehicleQuery);
				}
			}
		}

		ZQuery RunSheetsCarrierDriverOrVehicleQuery;
		ZQuery ChildFilterQuery;

		DtbConsignmentRunSheetCollection runSheets;
		DtbConsignmentRunSheetAdHocCollection runSheetsFilteredForBinding;

		ZQuery GetRunSheetFilter_WithoutEntityFilter()
		{
			var query = new ZQuery();

			var date = CurrentDate;
			query.AddToFilter(DtbConsignmentRunSheetSchema.KG_StartTime, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
			query.AddToFilter(DtbConsignmentRunSheetSchema.KG_StartTime, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);

			if (ChildFilterQuery != null)
			{
				query.AddToFilter(ChildFilterQuery);
			}

			return query;
		}

		#endregion

		#region Vehicles

		public RefEquipmentCollection Vehicles
		{
			get { return vehicles ?? (vehicles = new RefEquipmentCollection(Factory, new ZQuery(RefEquipmentSchema.RQ_IsVehicle, true))); }
		}

		RefEquipmentCollection vehicles;

		#endregion

		#endregion

		#region SaveWithRecovery

		public void SaveWithRecovery(IEnumerable<DtbConsignmentConfirmation> confirmations, INotifications notifier)
		{
			if (confirmations.Any()) // will eventually handle confirmations that can't be saved in Part 4
			{
				Exception exception = null;
				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					exception = ex;
				}
				catch (ZCannotSaveException ex)
				{
					exception = ex;
				}

				if (exception != null)
				{
					// save errors
					// 1. Run Sheet Number
					// 2. Allocating same Consignment - Confirmation FK strict
					// 3. Allocating to same RunSheet (depot creation) - RunSheet Date Field strict
					// 4. RunSheet Driver/Truck/TransCo DateRange violation

					var msg = exception.Message.Contains(DtbConsignmentRunSheet.RunSheetAlreadyExistsTriggerErrorMessage)
						? RunSheetAlreadyExistsTriggerErrorMessage
						: AllocationErrorMessage;

					notifier.Notify(new Notification(CargoWise.EntityFramework.NotificationType.Error, msg));
				}
			}
		}

		#region ErrorMessages

		static string RunSheetAlreadyExistsTriggerErrorMessage
		{
			get
			{
				return Res.GetString("DtbRoutePlanner|RunSheetAlreadyExistsTriggerErrorMessage",
@"An error occurred saving this allocation.

Another user has already created a Run Sheet for this Driver, Truck or Transport Company and Date Range.

Click 'OK' to Refresh.");
			}
		}

		static string AllocationErrorMessage
		{
			get
			{
				return Res.GetString("DtbRoutePlanner|AllocationErrorMessage",
@"An error occurred saving this allocation.

The Consignment may have already been assigned,
or the Run Sheet being allocated to may have been edited by another user.

Click 'OK' to Refresh.");
			}
		}

		#endregion

		#endregion

		#region AssignConfirmationsToRunSheet

		public void AssignConfirmationsToRunSheetInstruction(DtbConsignmentRunSheet runSheet, params DtbConsignmentConfirmation[] confirmations)
		{
			Argument.NotNull(runSheet, "runSheet");
			runSheet.AddNewRunSheetInstructions(confirmations);
		}

		public void AssignConfirmationsToRunSheet(OrgHeader carrier, DtbConsignmentRunSheet runSheet, params DtbConsignmentConfirmation[] confirmations)
		{
			var entity = new DtbRoutePlannerAssignConsignmentsInfo(carrier);
			AssignConfirmationsToRunSheetCore(entity, runSheet, confirmations);
		}

		public void AssignConfirmationsToRunSheet(GlbStaff driver, DtbConsignmentRunSheet runSheet, params DtbConsignmentConfirmation[] confirmations)
		{
			var entity = new DtbRoutePlannerAssignConsignmentsInfo(driver);
			AssignConfirmationsToRunSheetCore(entity, runSheet, confirmations);
		}

		public void AssignConfirmationsToRunSheet(RefEquipment vehicle, DtbConsignmentRunSheet runSheet, params DtbConsignmentConfirmation[] confirmations)
		{
			var entity = new DtbRoutePlannerAssignConsignmentsInfo(vehicle);
			AssignConfirmationsToRunSheetCore(entity, runSheet, confirmations);
		}

		void AssignConfirmationsToRunSheetCore(DtbRoutePlannerAssignConsignmentsInfo entity, DtbConsignmentRunSheet runSheet, DtbConsignmentConfirmation[] confirmations)
		{
			Argument.NotNull(confirmations, "confirmations");
			if (confirmations.Length == 0)
			{
				throw new ArgumentException("Provide at least one confirmation.");
			}

			runSheet = runSheet ?? GetNewRunSheetInDBOrCreateNew(entity);
			runSheet.AddNewRunSheetInstructions(confirmations);
		}

		public void AssignConfirmationsToRunSheet(DtbConsignmentRunSheet runSheet, params DtbAddressPoint[] addressPointsToAssign)
		{
			Argument.NotNull(runSheet, "runSheet");
			runSheet.AddNewRunSheetInstructions(addressPointsToAssign);
		}

		public void AssignConfirmationsToRunSheet(OrgHeader carrier, DtbConsignmentRunSheet runSheet, params DtbAddressPoint[] addressPointsToAssign)
		{
			var entity = new DtbRoutePlannerAssignConsignmentsInfo(carrier);
			AssignConfirmationsToRunSheetCore(entity, runSheet, addressPointsToAssign);
		}

		public void AssignConfirmationsToRunSheet(GlbStaff driver, DtbConsignmentRunSheet runSheet, params DtbAddressPoint[] addressPointsToAssign)
		{
			var entity = new DtbRoutePlannerAssignConsignmentsInfo(driver);
			AssignConfirmationsToRunSheetCore(entity, runSheet, addressPointsToAssign);
		}

		public void AssignConfirmationsToRunSheet(RefEquipment vehicle, DtbConsignmentRunSheet runSheet, params DtbAddressPoint[] addressPointsToAssign)
		{
			var entity = new DtbRoutePlannerAssignConsignmentsInfo(vehicle);
			AssignConfirmationsToRunSheetCore(entity, runSheet, addressPointsToAssign);
		}

		void AssignConfirmationsToRunSheetCore(DtbRoutePlannerAssignConsignmentsInfo entity, DtbConsignmentRunSheet runSheet, params DtbAddressPoint[] addressPointsToAssign)
		{
			Argument.NotNull(addressPointsToAssign, "addressPointsToAssign");
			if (addressPointsToAssign.Length == 0)
			{
				throw new ArgumentException("Provide at least one Address Point.");
			}

			runSheet = runSheet ?? GetNewRunSheetInDBOrCreateNew(entity);
			runSheet.AddNewRunSheetInstructions(addressPointsToAssign);
		}

		#region GetNewRunSheetInDBOrCreateNew

		DtbConsignmentRunSheet GetNewRunSheetInDBOrCreateNew(DtbRoutePlannerAssignConsignmentsInfo entity)
		{
			var newRunSheetsInDB = GetExistingRunSheets(entity, fetchOnlyFromLocalCache: false); // run sheets created by another user
			var runSheet = newRunSheetsInDB.FirstOrDefault(); // table trigger prevents multiple

			if (runSheet == null)
			{
				runSheet = CreateNewRunSheet(entity);
			}
			else
			{
				runSheet.ClearOriginalInstructionsCache(); // run sheet never existed on last refresh, therefor instruction cache should be empty from this instance
			}

			return runSheet;
		}

		#endregion

		#region GetExistingRunSheets

		DtbConsignmentRunSheet[] GetExistingRunSheets(DtbRoutePlannerAssignConsignmentsInfo entity, bool fetchOnlyFromLocalCache)
		{
			var currentDayAsZDateTime = CurrentDate.ToZDateTime();
			var startTime = currentDayAsZDateTime;
			var endTime = currentDayAsZDateTime.EndOfDay();

			var startTimeSubQuery = new ZQuery();
			startTimeSubQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_StartTime, SQLComparisonOperator.GreaterThanOrEqualTo, startTime);
			startTimeSubQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_StartTime, SQLComparisonOperator.LessThanOrEqualTo, endTime);

			var endTimeSubQuery = new ZQuery();
			endTimeSubQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_EndTime, SQLComparisonOperator.GreaterThan, startTime);
			endTimeSubQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_EndTime, SQLComparisonOperator.LessThanOrEqualTo, endTime);

			var outerRunSheetsSubQuery = new ZQuery();
			outerRunSheetsSubQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_StartTime, SQLComparisonOperator.LessThanOrEqualTo, startTime);
			outerRunSheetsSubQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, endTime);

			var timeQuery = new ZQuery();
			timeQuery.AddToFilter(startTimeSubQuery, JoinCondition.Or);
			timeQuery.AddToFilter(endTimeSubQuery, JoinCondition.Or);
			timeQuery.AddToFilter(outerRunSheetsSubQuery, JoinCondition.Or);

			var query = GetBaseRunSheetFilter(entity);
			query.AddToFilter(timeQuery);

			query.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;

			return Factory.Load<DtbConsignmentRunSheet>(query);
		}

		ZQuery GetBaseRunSheetFilter(DtbRoutePlannerAssignConsignmentsInfo entity)
		{
			var filter = new ZQuery();
			switch (entity.AssigningType)
			{
				case RunSheetView.Vehicles:
					filter.AddToFilter(DtbConsignmentRunSheetSchema.KG_RQ_Truck, entity.Vehicle.PK);
					break;
				case RunSheetView.Drivers:
					filter.AddToFilter(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, entity.Driver.GS_Code);
					break;
				case RunSheetView.Carriers:
					filter.AddToFilter(DtbConsignmentRunSheetSchema.KG_OH_TransportCo, entity.Carrier.PK);
					break;
			}
			return filter;
		}

		#endregion

		#region CreateNewRunSheet

		DtbConsignmentRunSheet CreateNewRunSheet(DtbRoutePlannerAssignConsignmentsInfo entity)
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			switch (entity.AssigningType)
			{
				case RunSheetView.Carriers:
					runSheet.KG_OH_TransportCo = entity.Carrier.PK;
					break;
				case RunSheetView.Drivers:
					var driverPreferredTruckQuery = new ZQuery(RefEquipmentSchema.RQ_IsVehicle, true);
					driverPreferredTruckQuery.AddToFilter(RefEquipmentSchema.RQ_GS_NKPreferredDriver, entity.Driver.GS_Code);
					driverPreferredTruckQuery.OrderBy = RefEquipmentSchema.RQ_ShortCode.Name;
					var driverPreferredTruck = Factory.LoadTop1<RefEquipment>(driverPreferredTruckQuery);

					runSheet.KG_GS_NKTruckDriver = entity.Driver.GS_Code;
					runSheet.KG_RQ_Truck = driverPreferredTruck != null ? driverPreferredTruck.PK : ZGuid.Empty;
					break;
				case RunSheetView.Vehicles:
					var preferredDriver = entity.Vehicle.RQ_GS_NKPreferredDriver;
					var preferredDriverInDriversGroup = DriversGroup != null && DriversGroup.Staff.Cast<GlbStaff>().Select(d => d.GS_Code).Contains(preferredDriver);

					runSheet.KG_GS_NKTruckDriver = preferredDriverInDriversGroup ? preferredDriver : ZString.Empty;
					runSheet.KG_RQ_Truck = entity.Vehicle.PK;
					break;
			}

			var currentDayAsZDateTimeOffset = new ZDateTimeOffset(CurrentDate.ToZDateTime(), DateTimeKind.Local);

			using (runSheet.GetValidationSuspender())
			{
				runSheet.KG_StartTime = currentDayAsZDateTimeOffset;
				runSheet.KG_EndTime = currentDayAsZDateTimeOffset.EndOfDay();
			}

			runSheet.KG_GB_Branch = GlbBranch.CurrentBranch.PK;

			return runSheet;
		}

		#endregion

		#endregion

		#region CurrentDay

		public RunSheetDay CurrentDay
		{
			get { return currentDay; }
			set
			{
				if (value != currentDay)
				{
					currentDay = value;
					FilterRunSheetsWithoutEntityFilters();
				}
			}
		}

		RunSheetDay currentDay;

		#endregion

		#region CurrentDate

		ZDate CurrentDate
		{
			get { return CurrentDay.IsCustomDate() ? CustomDateOption : CurrentDay.GetDate(); }
		}

		#endregion

		#region CustomDateOption

		public ZDate CustomDateOption
		{
			get { return customDateOption; }
			set
			{
				if (customDateOption != value)
				{
					customDateOption = value;
					FilterRunSheetsWithoutEntityFilters();
				}
			}
		}

		ZDate customDateOption;

		#endregion

		#region Filtering the RunSheets by Carrier/Driver/Vehicle and Child Filters

		public void FilterRunSheetsByChildFilters(ZQuery childFilterQuery)
		{
			ChildFilterQuery = childFilterQuery;
			FilterRunSheetsWithoutEntityFilters();
		}

		public void FilterRunSheetsByCarrier(OrgHeader carrier)
		{
			RunSheetsCarrierDriverOrVehicleQuery = (carrier != null)
				? new ZQuery(DtbConsignmentRunSheetSchema.KG_OH_TransportCo, carrier.PK)
				: ZQuery.NoResultQuery;

			FilterRunSheetsForBindingByCarrierDriverVehicleChildFilters();
		}

		public void FilterRunSheetsByDriver(GlbStaff driver)
		{
			RunSheetsCarrierDriverOrVehicleQuery = (driver != null)
				? new ZQuery(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, driver.GS_Code)
				: ZQuery.NoResultQuery;

			FilterRunSheetsForBindingByCarrierDriverVehicleChildFilters();
		}

		public void FilterRunSheetsByVehicle(RefEquipment vehicle)
		{
			RunSheetsCarrierDriverOrVehicleQuery = (vehicle != null)
				? new ZQuery(DtbConsignmentRunSheetSchema.KG_RQ_Truck, vehicle.PK)
				: ZQuery.NoResultQuery;

			FilterRunSheetsForBindingByCarrierDriverVehicleChildFilters();
		}

		public void ClearRunSheetsCarrierDriverAndVehicleFilter()
		{
			RunSheetsCarrierDriverOrVehicleQuery = null;
			FilterRunSheetsForBindingByCarrierDriverVehicleChildFilters();
		}

		#endregion

		#region LastSelectedEntities

		public void SetLastSelectedEntities(ZGuid runSheetPK, ZGuid carrierPK, ZGuid driverPK, ZGuid vehiclePK)
		{
			LastSelectedEntities = new DtbRoutePlannerSelectedEntities(runSheetPK, carrierPK, driverPK, vehiclePK);
		}

		public DtbRoutePlannerSelectedEntities LastSelectedEntities
		{
			get { return lastSelectedEntities ?? (lastSelectedEntities = new DtbRoutePlannerSelectedEntities()); }
			private set { lastSelectedEntities = value; }
		}

		DtbRoutePlannerSelectedEntities lastSelectedEntities;

		#endregion

		#region CopyTransientProperties

		public override void CopyTransientProperties(BusinessObject copy)
		{
			var newRoutePlanner = (DtbRoutePlanner)copy;
			newRoutePlanner.customDateOption = CustomDateOption;
			newRoutePlanner.currentDay = CurrentDay;

			if (lastSelectedEntities != null)
			{
				newRoutePlanner.LastSelectedEntities = LastSelectedEntities;
			}
			newRoutePlanner.RunSheetsCarrierDriverOrVehicleQuery = RunSheetsCarrierDriverOrVehicleQuery;

			CopySort(AddressPoints, newRoutePlanner.AddressPoints);
			CopySort(Carriers, newRoutePlanner.Carriers);
			CopySort(Drivers, newRoutePlanner.Drivers);
			CopySort(Vehicles, newRoutePlanner.Vehicles);

			if (runSheetsFilteredForBinding != null)
			{
				CopySort(RunSheetsFilteredForBinding, newRoutePlanner.RunSheetsFilteredForBinding);
			}
		}

		void CopySort(IBindingList oldList, IBindingList newList)
		{
			if (oldList.SortProperty != null)
			{
				newList.ApplySort(oldList.SortProperty, oldList.SortDirection);
			}
		}

		#endregion

		#region RunSheetCount

		public ZInt GetRunSheetCountForEntity(OrgHeader carrier)
		{
			return GetRunSheetCountForEntityCore(rs => rs.KG_OH_TransportCo == carrier.PK);
		}

		public ZInt GetRunSheetCountForEntity(GlbStaff driver)
		{
			return GetRunSheetCountForEntityCore(rs => rs.KG_GS_NKTruckDriver == driver.GS_Code);
		}

		public ZInt GetRunSheetCountForEntity(RefEquipment truck)
		{
			return GetRunSheetCountForEntityCore(rs => rs.KG_RQ_Truck == truck.PK);
		}

		ZInt GetRunSheetCountForEntityCore(Func<DtbConsignmentRunSheet, bool> runSheetIsForEntity)
		{
			return RunSheetsCacheFilteredByDate.Count(runSheetIsForEntity);
		}

		#endregion
	}
}
