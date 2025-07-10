using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsLocationViewValidation : AutoWhsLocationViewValidation
	{
		#region constructor

		public WhsLocationViewValidation(AutoWhsLocationView parent)
			: base(parent)
		{
		}

		#endregion

		#region Parent

		public new WhsLocation Parent => (WhsLocation)base.Parent;

		#endregion

		#region constants

		static readonly ImmutableList<string> CYDOnlyLocationClasses = ImmutableList.Create(
			LocationClasses.Codes.WSA,
			LocationClasses.Codes.WCL,
			LocationClasses.Codes.RPR,
			LocationClasses.Codes.INS,
			LocationClasses.Codes.SUR
		);

		#endregion

		#region CheckWLV_WLT_LocationType

		protected override void CheckWLV_WLT_LocationType()
		{
			MandatoryValidation.CheckEntered(Parent.WLV_WLT_LocationTypeInfo);

			if (!Parent.IsInDatabase || Parent.WLV_WLT_LocationTypeInfo.HasChanges)
			{
				if (!Parent.WLV_WLT_LocationTypeInfo.HasErrors() && !IsLocationTypeChangeAllowed)
				{
					Parent.WLV_WLT_LocationTypeInfo.AddError(Res.GetString("11045ea6-1986-4c30-9a8a-66577a8fd221", "Cannot change Location Type as there is Existing or Pending Inventory."));
				}
				else if (!Parent.IsDockDoorLocation && IsLocationUsedAsDefaultInboundDockDoor)
				{
					Parent.WLV_WLT_LocationTypeInfo.AddError(ErrorChangingTypeFromDockDoorIfUsedByWarehouseAsDefaultDockDoor(Inbound));
				}
				else if (!Parent.IsDockDoorLocation && IsLocationUsedAsDefaultOutboundDockDoor)
				{
					Parent.WLV_WLT_LocationTypeInfo.AddError(ErrorChangingTypeFromDockDoorIfUsedByWarehouseAsDefaultDockDoor(Outbound));
				}

				var locationType = Parent.LocationType;
				var row = Parent.Row;
				CheckWL_LocationType_SupportedForThisWarehouseType(locationType);
				CheckWL_LocationType_MaxCapacity();
				CheckWL_LocationType_IsNotUsedByPick(row);
				CheckWL_LocationType_MustHaveClassFIXForPickFace(row);
				CheckWL_LocationType_DynamicPickFaceArea(locationType);
				CheckWL_LocationClass_CannotUseCYDOnlyLocationClassesIfWarehouseIsNotCYD();
				CheckWL_LocationClass_CannotChangeCYDWaitingBayLocationTypeWhileInUse();
			}
		}

		bool IsLocationUsedAsDefaultInboundDockDoor => Parent?.Row?.Warehouse?.WW_DefaultInboundDockDoor == Parent.PK;
		bool IsLocationUsedAsDefaultOutboundDockDoor => Parent?.Row?.Warehouse?.WW_DefaultOutboundDockDoor == Parent.PK;

		bool IsLocationTypeChangeAllowed
		{
			get
			{
				var result = true;

				if (Parent.WLV_WLT_LocationTypeInfo.HasChanges)
				{
					var originalClass = Parent.Factory.Load<WhsLocationType>((ZGuid)Parent.WLV_WLT_LocationTypeInfo.OriginalValue)?.WLT_LocationClass ?? "";
					var newClass = Parent.LocationType.WLT_LocationClass;
					if ((newClass == LocationClasses.Codes.FIX || originalClass != newClass) && !Parent.IsEmpty)
					{
						var validFromClasses = new ZString[] { LocationClasses.Codes.NOR, LocationClasses.Codes.HPL, LocationClasses.Codes.FIX };
						var validToClasses = new ZString[] { LocationClasses.Codes.NOR, LocationClasses.Codes.HPL };
						result = validFromClasses.Contains(originalClass) && validToClasses.Contains(newClass);
					}
				}
				return result;
			}
		}

		public static string Inbound => Res.GetString("95cd2fea-5b0e-40e5-a44e-ebf234a99545", "inbound");
		public static string Outbound => Res.GetString("a9cb931f-74ac-43ad-a78f-f028a2628695", "outbound");

		public static string ErrorChangingTypeFromDockDoorIfUsedByWarehouseAsDefaultDockDoor(string dockDoorType)
		{
			return Res.GetString("C8598C0E-FB8C-4AAB-A250-3D44B920A45F", "Cannot change location type as this location is a default {0} dock door location for this warehouse.", dockDoorType);
		}

		void CheckWL_LocationType_SupportedForThisWarehouseType(WhsLocationType locationType)
		{
			if (!Parent.WLV_WLT_LocationTypeInfo.HasErrors() && locationType != null)
			{
				var warehouse = Parent.Warehouse;
				if (warehouse != null && !locationType.IsLocationTypeSupportedByThisWarehouseType(warehouse.WW_WarehouseType))
				{
					Parent.WLV_WLT_LocationTypeInfo.AddError(Res.GetString("39babf1e-4640-493f-99df-7b026d7845c2", "This Location Type is not supported for this type of Warehouse."));
				}
			}
		}

		public static string ErrorChangingTypeFromDockDoorIfUsedByPick => Res.GetString("97B75A07-9765-4F4B-80F4-25C3977B3673", "Cannot change Type of location that is used as Dock Door Location by pick(s).");
		public static string ErrorChangingTypeFromPackingStationIfUsedByPick => Res.GetString("bb7d0fdb-4ea0-469b-bd1c-9e47e05c27ef", "Cannot change Type of location that is used as Packing Station Location by pick(s).");

		void CheckWL_LocationType_IsNotUsedByPick(WhsRow row)
		{
			if (!Parent.WLV_WLT_LocationTypeInfo.HasErrors())
			{
				if (!Parent.IsDockDoorLocation && (row?.LocationHasDockDoorPick(Parent) ?? false))
				{
					Parent.WLV_WLT_LocationTypeInfo.AddError(ErrorChangingTypeFromDockDoorIfUsedByPick);
				}
				else if (!Parent.IsPackingStationLocation && (row?.LocationHasPackingStationPick(Parent) ?? false))
				{
					Parent.WLV_WLT_LocationTypeInfo.AddError(ErrorChangingTypeFromPackingStationIfUsedByPick);
				}
			}
		}

		void CheckWL_LocationType_MustHaveClassFIXForPickFace(WhsRow row)
		{
			if (!Parent.WLV_WLT_LocationTypeInfo.HasErrors() && !Parent.IsFixedLocation && (row?.LocationHasPickFace(Parent) ?? false))
			{
				Parent.WLV_WLT_LocationTypeInfo.AddError(Res.GetString("EED032C3-62C7-4897-ADF3-FE971D7AFAF4", "A Location used in a Pick Face cannot change its Location Type, unless the new Location Type is also of class FIX"));
			}
		}

		void CheckWL_LocationType_DynamicPickFaceArea(WhsLocationType locationType)
		{
			var locationTypeClass = locationType?.WLT_LocationClass ?? ZString.Empty;
			if (Parent.WLV_PickingAreaType == AreaTypes.Codes.DynamicPickFace && locationTypeClass != LocationClasses.Codes.DPF)
			{
				Parent.WLV_WLT_LocationTypeInfo.AddError(Res.GetString("E900F91B-9AF4-4A8A-8425-605290FF521C", "Dynamic Pick Face Areas must have Dynamic Pick Face Locations only."));
			}
			else if (Parent.WLV_PickingAreaType != AreaTypes.Codes.DynamicPickFace && locationTypeClass == LocationClasses.Codes.DPF)
			{
				Parent.WLV_WLT_LocationTypeInfo.AddError(Res.GetString("2633F7D0-DBC4-4CAF-96C1-F462FA2CD2CF", "Dynamic Pick Face Locations can only be set in Dynamic Pick Face Areas."));
			}
		}

		void CheckWL_LocationClass_CannotUseCYDOnlyLocationClassesIfWarehouseIsNotCYD()
		{
			if (!Parent.WLV_WLT_LocationTypeInfo.HasErrors() && Parent.WLV_WLT_LocationTypeInfo.HasChanges &&
				CYDOnlyLocationClasses.Contains(Parent.WLV_LocationClass))
			{
				if (Parent.WLV_WarehouseType != WarehouseTypes.Codes.ContainerYard)
				{
					Parent.WLV_WLT_LocationTypeInfo.AddError(Res.GetString("14711D8A-3B9D-43DB-A908-97645BA2C569", "{0} Location Class cannot be used if the warehouse type is not CYD.", Parent.WLV_LocationClass));
				}
			}
		}

		void CheckWL_LocationClass_CannotChangeCYDWaitingBayLocationTypeWhileInUse()
		{
			if (!Parent.WLV_WLT_LocationTypeInfo.HasErrors() && Parent.WLV_WLT_LocationTypeInfo.HasChanges && Parent.WLV_WarehouseType == WarehouseTypes.Codes.ContainerYard)
			{
				var originalClass = Parent.Factory.Load<WhsLocationType>((ZGuid)Parent.WLV_WLT_LocationTypeInfo.OriginalValue)?.WLT_LocationClass ?? "";
				var newClass = Parent.LocationType.WLT_LocationClass;

				if (originalClass == LocationClasses.Codes.WSA && newClass != LocationClasses.Codes.WSA)
				{
					var queryOfWaitingBayTPUs = new ZQuery(new ZQuery(CYDTransportationUnitSchema.YTU_GateInTime, SQLComparisonOperator.NotEqual, null), JoinCondition.And, new ZQuery(CYDTransportationUnitSchema.YTU_GateOutTime, SQLComparisonOperator.Equal, null));
					var queryOfWaitingBayTPUsOnThisLocation = new ZQuery(queryOfWaitingBayTPUs, JoinCondition.And, new ZQuery(CYDTransportationUnitSchema.YTU_WL_WaitingBayLocation, Parent.PK));
					var transportationReferences = Parent.Factory.Load<ICYDTransportationUnit>(queryOfWaitingBayTPUsOnThisLocation).Select(unit => unit.YTU_TransportationReference).Distinct().ToList();

					if (transportationReferences.Count > 0)
					{
						transportationReferences.Sort();
						Parent.WLV_WLT_LocationTypeInfo.AddError(Res.GetString("B71E72A4-DFE2-4D08-BE49-877646255378", "The selected location is currently being used as a waiting bay for Transportation Units ({0}). Please select another location to continue.", string.Join(", ", transportationReferences)));
					}
				}
			}
		}

		void CheckWL_LocationType_MaxCapacity()
		{
			if ((Parent.IsFixedLocation || Parent.IsDynamicPickFaceLocation)
				&& (Parent.WLV_MaxWeight > 0m || Parent.WLV_MaxCubic > 0m || Parent.WLV_MaxQuantity > 0m))
			{
				Parent.WLV_WLT_LocationTypeInfo.AddError(MaxCapacityForPickFaceLocationErrorMessage);
			}
		}

		#endregion

		#region CheckWLV_RS_NKTransitServiceLevelIsNotEmpty

		protected override void CheckWLV_RS_NKTransitServiceLevelIsNotEmpty()
		{
			// Suppress mandatory validation for the view
		}

		#endregion

		#region CheckWLV_RS_NKTransitServiceLevel

		protected override void CheckWLV_RS_NKTransitServiceLevel()
		{
			base.CheckWLV_RS_NKTransitServiceLevel();
			if (!Parent.WLV_RS_NKTransitServiceLevelInfo.HasErrors())
			{
				CheckDischargeAndServiceLevelBothSetOrNot(Parent.WLV_RS_NKTransitServiceLevelInfo);
			}
			if (!Parent.WLV_RS_NKTransitServiceLevelInfo.HasErrors())
			{
				CheckDuplicationOfServiceLevelAndDischargeLRC(Parent.WLV_RS_NKTransitServiceLevelInfo);
			}
			if (!Parent.WLV_RS_NKTransitServiceLevelInfo.HasErrors())
			{
				CheckDischargeLRCOverlapping(Parent.WLV_RS_NKTransitServiceLevelInfo);
			}
		}

		#endregion

		#region CheckWLV_RS_NKTransitServiceLevelIsNotEmpty

		protected override void CheckWLV_TransitDischargeLRCIsNotEmpty()
		{
			// Suppress mandatory validation for the view
		}

		#endregion

		#region CheckWLV_TransitDischargeLRC

		protected override void CheckWLV_TransitDischargeLRC()
		{
			base.CheckWLV_TransitDischargeLRC();
			if (!Parent.WLV_TransitDischargeLRCInfo.HasErrors())
			{
				CheckDischargeAndServiceLevelBothSetOrNot(Parent.WLV_TransitDischargeLRCInfo);
			}
			if (!Parent.WLV_TransitDischargeLRCInfo.HasErrors())
			{
				CheckDuplicationOfServiceLevelAndDischargeLRC(Parent.WLV_TransitDischargeLRCInfo);
			}
			if (!Parent.WLV_TransitDischargeLRCInfo.HasErrors())
			{
				CheckDischargeLRCOverlapping(Parent.WLV_TransitDischargeLRCInfo);
			}
		}

		#endregion

		#region CheckDischargeAndServiceLevelBothSetOrNot

		void CheckDischargeAndServiceLevelBothSetOrNot(ZPropertyInfo propertyInfo)
		{
			ListValidation.ErrorIfInvalidCode(propertyInfo);

			if (Parent.WLV_RS_NKTransitServiceLevel.IsEmpty ^ Parent.WLV_TransitDischargeLRC.IsEmpty)
			{
				var errorMsg = Res.GetString("B42D0B9D-BEEA-488B-AC5B-33AF8268D794", "Discharge and Service Level should be both set or both empty.");
				propertyInfo.AddError(errorMsg);
			}
		}

		#endregion

		#region CheckDuplicationOfServiceLevelAndDischargeLRC

		void CheckDuplicationOfServiceLevelAndDischargeLRC(ZPropertyInfo propertyInfo)
		{
			if (!Parent.WLV_RS_NKTransitServiceLevel.IsEmpty && !Parent.WLV_TransitDischargeLRC.IsEmpty && Parent.WLV_WW_Whs.IsValid)
			{
				var query = new ZQuery();
				query.AddToFilter(WhsLocationViewSchema.WLV_RS_NKTransitServiceLevel, Parent.WLV_RS_NKTransitServiceLevel);
				query.AddToFilter(WhsLocationViewSchema.WLV_TransitDischargeLRC, Parent.WLV_TransitDischargeLRC);

				query.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, Parent.WLV_WW_Whs);
				query.AddToFilter(WhsLocationViewSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsLocation>(query) != null)
				{
					var errorMsg = Res.GetString("a03ca3b3-abf3-469c-9e9d-569a60bf3857",
						"There is already a location for Discharge '{0}' and Service Level '{1}'. Transit automation will therefore not know which location to use when cross-docking packages.", Parent.WLV_TransitDischargeLRC, Parent.WLV_RS_NKTransitServiceLevel);
					propertyInfo.AddError(errorMsg);
				}
			}
		}

		#endregion

		#region CheckDischargeLRCOverlapping

		void CheckDischargeLRCOverlapping(ZPropertyInfo propertyInfo)
		{
			var serviceLevel = Parent.WLV_RS_NKTransitServiceLevel;
			var dischargeLRC = Parent.WLV_TransitDischargeLRC;

			if (!dischargeLRC.IsEmpty && !serviceLevel.IsEmpty)
			{
				var country = Parent.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, dischargeLRC);
				if (country != null)
				{
					var locationQuery = new ZQuery();
					locationQuery.AddToFilter(WhsLocationViewSchema.WLV_RS_NKTransitServiceLevel, serviceLevel);
					locationQuery.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, Parent.WLV_WW_Whs);

					var zonesInCountry = GetZonesInCountry(country.PK);
					if (zonesInCountry.Length > 0)
					{
						locationQuery.AddToFilter(WhsLocationViewSchema.WLV_TransitDischargeLRC, zonesInCountry);

						var locationsWithOverlappingZone = Parent.Factory.Load<WhsLocation>(locationQuery);
						if (locationsWithOverlappingZone.Length > 0)
						{
							var locationStrings = string.Join(", ", locationsWithOverlappingZone.Select(l => l.WLV_LocationString));
							var warningMsg = Res.GetString("062e3f91-a867-443d-8c2b-c45e6717ddcc", "This location will never be matched because the following locations [{0}] have a Discharge Zone that contains {1} and would be matched instead.", locationStrings, country.RN_Code);
							propertyInfo.AddWarning(warningMsg);
						}
					}
				}
			}
		}

		ZString[] GetZonesInCountry(ZGuid countryPK)
		{
			var zoneQuery = new ZDBOnlyQuery(typeof(RefZoneHeader));

			var zonePivotToCountryQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), RefZonePivotSchema.F2_FZ);
			zonePivotToCountryQuery.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefCountrySchema.Constants.Prefix);
			zonePivotToCountryQuery.AddToFilter(RefZonePivotSchema.F2_ParentID, countryPK);

			zoneQuery.AddSubQuery(zonePivotToCountryQuery, JoinCondition.And);
			var zonesInCountry = Parent.Factory.Load<RefZoneHeader>(zoneQuery);
			return zonesInCountry.Select(z => z.FZ_Code).ToArray();
		}

		#endregion

		#region CheckWLV_LocationStatus

		protected override void CheckWLV_LocationStatus()
		{
			MandatoryValidation.CheckEntered(Parent.WLV_LocationStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WLV_LocationStatusInfo, new LocationStatus());

			if (!Parent.IsInDatabase || Parent.WLV_LocationStatusInfo.HasChanges)
			{
				CheckWLV_LocationStatus_CannotChangeToVoidIfHasStock();
				CheckWLV_LocationStatus_CannotChangeFromNormalIfUsedByPick();
				CheckWLV_LocationStatus_CannotChangeFromNormalIfUsedByWarehouseAsDefaultDockDoor();
				CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasOpenRTU();
				CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasPackage();
				CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasLoadListForThisLocation();
				CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasUnfinalisedTransferToThisLocation();
				CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasPendingCycleCountLocation();
				CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasPendingCycleCountLocationVariance();
			}
		}

		void CheckWLV_LocationStatus_CannotChangeToVoidIfHasStock()
		{
			var location = Parent;

			if (!location.WLV_LocationStatusInfo.HasErrors()
				&& location.IsInDatabase
				&& location.WLV_LocationStatus == LocationStatus.Codes.Void
				&& location.WLV_LocationStatusInfo.HasChanges
				&& !location.IsEmpty)
			{
				location.WLV_LocationStatusInfo.AddError(Res.GetString("0e7d5af5-f16a-462f-b506-9d36644a83e9", "Cannot change Location Status to {0} as stock exists in this Location.", location.WLV_LocationStatus));
			}
		}

		void CheckWLV_LocationStatus_CannotChangeFromNormalIfUsedByPick()
		{
			if (!Parent.WLV_LocationStatusInfo.HasErrors()
				&& Parent.IsInDatabase
				&& Parent.WLV_LocationStatus != LocationStatus.Codes.Normal
				&& Parent.WLV_LocationStatusInfo.HasChanges
				&& (Parent.Row?.LocationHasUnfinalisedPick(Parent) ?? false))
			{
				Parent.WLV_LocationStatusInfo.AddError(Res.GetString("38229db7-bcfe-4564-89f8-07b56fa3d9d0", "Cannot change Location Status from {0} as an un-finalized Pick exists that references this Location.", LocationStatus.Codes.Normal));
			}
		}

		void CheckWLV_LocationStatus_CannotChangeFromNormalIfUsedByWarehouseAsDefaultDockDoor()
		{
			if (!Parent.WLV_LocationStatusInfo.HasErrors()
				&& Parent.IsInDatabase
				&& Parent.WLV_LocationStatus != LocationStatus.Codes.Normal
				&& Parent.WLV_LocationStatusInfo.HasChanges)
			{
				if (IsLocationUsedAsDefaultInboundDockDoor)
				{
					Parent.WLV_LocationStatusInfo.AddError(CannotChangeLocationStatusBecauseUsedAsDefaultDockDoorLocation(Inbound));
				}
				if (IsLocationUsedAsDefaultOutboundDockDoor)
				{
					Parent.WLV_LocationStatusInfo.AddError(CannotChangeLocationStatusBecauseUsedAsDefaultDockDoorLocation(Outbound));
				}
			}
		}

		#region Transit Warehouse

		void CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasOpenRTU()
		{
			var location = Parent;

			if (!location.WLV_LocationStatusInfo.HasErrors()
				&& location.IsInDatabase
				&& location.Warehouse.WW_WarehouseType == WarehouseTypes.Codes.Transit
				&& location.WLV_LocationStatus == LocationStatus.Codes.Void
				&& location.WLV_LocationStatusInfo.HasChanges
				&& HasOpenReceiveTransportationUnitInLocation)
			{
				location.WLV_LocationStatusInfo.AddError(Res.GetString("8533fd6c-11c8-4e62-a800-9a934ea0814c", "Cannot change Location Status to {0} as there are Receive Transportation Units planned for this location or currently being unloaded.", LocationStatus.Descriptions.Void));
			}
		}

		bool HasOpenReceiveTransportationUnitInLocation
		{
			get
			{
				var query = new ZQuery();
				query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation, SQLComparisonOperator.Equal, Parent.PK);
				query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime, SQLComparisonOperator.Equal, null);

				return Parent.Factory.LoadTop1<IWhsItemReceiveTransportationUnit>(query) != null;
			}
		}

		void CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasPackage()
		{
			var location = Parent;

			if (!location.WLV_LocationStatusInfo.HasErrors()
				&& location.IsInDatabase
				&& location.Warehouse.WW_WarehouseType == WarehouseTypes.Codes.Transit
				&& location.WLV_LocationStatus == LocationStatus.Codes.Void
				&& location.WLV_LocationStatusInfo.HasChanges
				&& HasPackageInLocation)
			{
				location.WLV_LocationStatusInfo.AddError(Res.GetString("42a7daa9-55cf-451b-a4e5-ae109238373b", "Cannot change Location Status to {0} as there are Packages in this location.", LocationStatus.Descriptions.Void));
			}
		}

		bool HasPackageInLocation
		{
			get
			{
				var query = new ZQuery(WhsItemPackageStateSchema.WPS_WL_LastLocation, Parent.PK);
				query.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, new string[3] { "FIN", "DEP", "ADJ" });
				return Parent.Factory.LoadTop1<IWhsItemPackageState>(query) != null;
			}
		}

		void CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasUnfinalisedTransferToThisLocation()
		{
			var location = Parent;

			if (!location.WLV_LocationStatusInfo.HasErrors()
				&& location.IsInDatabase
				&& location.Warehouse.WW_WarehouseType == WarehouseTypes.Codes.Transit
				&& location.WLV_LocationStatus == LocationStatus.Codes.Void
				&& location.WLV_LocationStatusInfo.HasChanges
				&& HasUnfinalisedTransferToThisLocation)
			{
				location.WLV_LocationStatusInfo.AddError(Res.GetString("9a2a6b3f-a5bb-480d-9095-96f1d6660bab", "Cannot change Location Status to {0} as there are un-finalized Transfers to this location.", LocationStatus.Descriptions.Void));
			}
		}

		bool HasUnfinalisedTransferToThisLocation
		{
			get
			{
				var transferQuery = new ZDBOnlyQuery(typeof(IWhsItemTransferHeader));
				transferQuery.AddToFilter(WhsItemTransferHeaderSchema.WTH_IsFinalised, SQLComparisonOperator.Equal, false);

				var transferLineQuery = new ZDBOnlySubQuery(typeof(IWhsItemTransferLine), WhsItemTransferLineSchema.WTF_WTH_TransitTransferHeader);
				transferLineQuery.AddToFilter(WhsItemTransferLineSchema.WTF_WL_To, SQLComparisonOperator.Equal, Parent.PK);

				transferQuery.AddSubQuery(transferLineQuery, JoinCondition.And);

				return Parent.Factory.LoadTop1<IWhsItemTransferHeader>(transferQuery) != null;
			}
		}

		void CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasPendingCycleCountLocation()
		{
			var location = Parent;

			if (!location.WLV_LocationStatusInfo.HasErrors()
				&& location.IsInDatabase
				&& location.Warehouse.WW_WarehouseType == WarehouseTypes.Codes.Transit
				&& location.WLV_LocationStatus == LocationStatus.Codes.Void
				&& location.WLV_LocationStatusInfo.HasChanges
				&& HasPendingCycleCountLocationForThisLocation)
			{
				location.WLV_LocationStatusInfo.AddError(Res.GetString("dd089540-7468-4925-b754-3ec659c14843", "Cannot change Location Status to {0} as there are pending cycle counting jobs for this location.", LocationStatus.Descriptions.Void));
			}
		}

		bool HasPendingCycleCountLocationForThisLocation
		{
			get
			{
				var query = new ZQuery(WhsItemCycleCountLocationSchema.WIC_WL_Location, Parent.PK);
				query.AddToFilter(new ZQuery(WhsItemCycleCountLocationSchema.WIC_Status, SQLComparisonOperator.Equal, "NST"));
				query.AddToFilter(new ZQuery(WhsItemCycleCountLocationSchema.WIC_Status, SQLComparisonOperator.Equal, "INP"), JoinCondition.Or);
				query.AddToFilter(new ZQuery(WhsItemCycleCountLocationSchema.WIC_Status, SQLComparisonOperator.Equal, "PCV"), JoinCondition.Or);
				query.AddToFilter(new ZQuery(WhsItemCycleCountLocationSchema.WIC_Status, SQLComparisonOperator.Equal, "ERR"), JoinCondition.Or);
				return Parent.Factory.LoadTop1<IWhsItemCycleCountLocation>(query) != null;
			}
		}

		void CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasPendingCycleCountLocationVariance()
		{
			var location = Parent;

			if (!location.WLV_LocationStatusInfo.HasErrors()
				&& location.IsInDatabase
				&& location.Warehouse.WW_WarehouseType == WarehouseTypes.Codes.Transit
				&& location.WLV_LocationStatus == LocationStatus.Codes.Void
				&& location.WLV_LocationStatusInfo.HasChanges
				&& HasPendingCycleCountLocationVarianceForThisLocation)
			{
				location.WLV_LocationStatusInfo.AddError(Res.GetString("9002cb30-ab0b-425b-a374-3b9d3eb2b748", "Cannot change Location Status to {0} as there are open cycle counting variance for this location.", LocationStatus.Descriptions.Void));
			}
		}

		bool HasPendingCycleCountLocationVarianceForThisLocation
		{
			get
			{
				var openVarianceQuery = new ZDBOnlyQuery(typeof(IWhsItemCycleCountLocationVariance));
				openVarianceQuery.AddToFilter(WhsItemCycleCountLocationVarianceSchema.WIV_Status, SQLComparisonOperator.Equal, "OPN");

				var innerQuery = new ZDBOnlyQuery(typeof(IWhsItemCycleCountLocationVariance));
				innerQuery.AddToFilter(WhsItemCycleCountLocationVarianceSchema.WIV_WL_ExpectedStockLocation, Parent.PK);

				var cycleCountSubQuery = new ZDBOnlySubQuery(typeof(IWhsItemCycleCountLocation), WhsItemCycleCountLocationSchema.PK);
				cycleCountSubQuery.AddToFilter(WhsItemCycleCountLocationSchema.WIC_WL_Location, Parent.PK);
				innerQuery.AddSubQuery(WhsItemCycleCountLocationVarianceSchema.WIV_WIC_CycleCountLocation, cycleCountSubQuery, JoinCondition.Or);

				openVarianceQuery.AddToFilter(innerQuery, JoinCondition.And);

				return Parent.Factory.LoadTop1<IWhsItemCycleCountLocationVariance>(openVarianceQuery) != null;
			}
		}

		void CheckTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasLoadListForThisLocation()
		{
			var location = Parent;

			if (!location.WLV_LocationStatusInfo.HasErrors()
				&& location.IsInDatabase
				&& location.Warehouse.WW_WarehouseType == WarehouseTypes.Codes.Transit
				&& location.WLV_LocationStatus == LocationStatus.Codes.Void
				&& location.WLV_LocationStatusInfo.HasChanges
				&& HasLoadListForThisLocation)
			{
				location.WLV_LocationStatusInfo.AddError(Res.GetString("f830c6cb-d888-49db-ad67-52340d0843ba", "Cannot change Location Status to {0} as there are Loads planned for this location.", LocationStatus.Descriptions.Void));
			}
		}

		bool HasLoadListForThisLocation
		{
			get
			{
				var query = new ZQuery(WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation, Parent.PK);
				query.AddToFilter(new ZQuery(WhsItemDispatchLoadListSchema.WDL_CompleteTime, null));
				return Parent.Factory.LoadTop1<IWhsItemDispatchLoadList>(query) != null;
			}
		}

		#endregion

		public static string CannotChangeLocationStatusBecauseUsedAsDefaultDockDoorLocation(string dockDoorType)
		{
			return Res.GetString("d12c69e9-a8d4-4414-b7c0-6b00fb5be510", "Cannot change Location Status from {0} as this location is the default {1} dock door location for this warehouse.", LocationStatus.Codes.Normal, dockDoorType);
		}

		#endregion

		#region CheckWLV_PickMethod

		protected override void CheckWLV_PickMethod()
		{
			MandatoryValidation.CheckEntered(Parent.WLV_PickMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WLV_PickMethodInfo, WarehouseDataRegistry.Instance.PickMethod.Value);
		}

		#endregion

		#region CheckWLV_PickPathSequence

		protected override void CheckWLV_PickPathSequence()
		{
			base.CheckWLV_PickPathSequence();
			MandatoryValidation.CheckNotNegative(Parent.WLV_PickPathSequenceInfo);
		}

		#endregion

		#region CheckWLV_PutawayPathSequence

		protected override void CheckWLV_PutawayPathSequence()
		{
			base.CheckWLV_PutawayPathSequence();
			MandatoryValidation.CheckNotNegative(Parent.WLV_PutawayPathSequenceInfo);
			MandatoryValidation.CheckNotZero(Parent.WLV_PutawayPathSequenceInfo);
		}

		#endregion

		#region CheckWLV_WA_PickingArea

		protected override void CheckWLV_WA_PickingArea()
		{
			base.CheckWLV_WA_PickingArea();
			ListValidation.ErrorIfInvalidPK(Parent.WLV_WA_PickingAreaInfo);
			if (!Parent.IsInDatabase || Parent.WLV_WA_PickingAreaInfo.HasChanges)
			{
				CheckWLV_WA_PickingArea_DynamicPickFaceArea();
				CheckWLV_WA_PickingArea_BondedArea();
				CheckInwardProcessingArea(Parent.WLV_WA_PickingAreaInfo, Parent.WLV_PickingAreaType, Parent.WLV_PutawayAreaType);
			}
		}

		void CheckWLV_WA_PickingArea_DynamicPickFaceArea()
		{
			var locationTypeClass = Parent.LocationType?.WLT_LocationClass ?? ZString.Empty;
			if (Parent.WLV_PickingAreaType == AreaTypes.Codes.DynamicPickFace && locationTypeClass != LocationClasses.Codes.DPF)
			{
				Parent.WLV_WA_PickingAreaInfo.AddError(Res.GetString("69B74E32-1A64-4AAB-8430-2C44BA5597D7", "Dynamic Pick Face Areas must only have Dynamic Pick Face Locations."));
			}
			else if ((ZGuid)Parent.WLV_WA_PickingAreaInfo.OriginalValue != Parent.WLV_WA_PickingArea)
			{
				var previousArea = Parent.Factory.Load<WhsArea>((ZGuid)Parent.WLV_WA_PickingAreaInfo.OriginalValue);
				if (previousArea.WA_AreaType == AreaTypes.Codes.DynamicPickFace
					&& previousArea.PickLocations.Count == 0
					&& Parent.Factory.LoadTop1<IWhsProductParamsByWhsAndClient>(new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea, previousArea.PK)) != null)
				{
					Parent.WLV_WA_PickingAreaInfo.AddError(Res.GetString("D6D67300-250A-4B58-903E-492B9FA3C3BA", "Cannot change pick area from {0} to {1}, as previous dynamic pick area {0} would contain no locations.", previousArea.WA_NameMultilingual, Parent.PickingArea.WA_NameMultilingual));
				}
			}
		}

		void CheckWLV_WA_PickingArea_BondedArea()
		{
			var previousArea = Parent.Factory.Load<WhsArea>((ZGuid)Parent.WLV_WA_PickingAreaInfo.OriginalValue);
			if (previousArea != null
					&& previousArea.PK != Parent.WLV_WA_PickingArea
					&& previousArea.WA_AreaType == AreaTypes.Codes.Bonded)
			{
				if (ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Parent.Factory, Parent.PK))
				{
					Parent.WLV_WA_PickingAreaInfo.AddError(Res.GetString("22CA0E75-852E-4698-8E8E-1FCC918F1767", "Locations containing Customs inventory in a Bonded Picking Area cannot change Picking Area."));
				}
				if (ValidationHelper.CheckIfLocationsHaveTransitPackage(Parent.Factory, Parent.PK))
				{
					Parent.WLV_WA_PickingAreaInfo.AddError(Res.GetString("EA10E93D-16F0-4C9E-BC11-AF7703091C0D", "Locations containing packages in a Bonded Picking Area cannot change Picking Area."));
				}
			}
		}

		void CheckInwardProcessingArea(ZPropertyInfo areaInfo, string areaType, string otherAreaType)
		{
			if (areaType == AreaTypes.Codes.InwardProcessing && otherAreaType != AreaTypes.Codes.InwardProcessing)
			{
				areaInfo.AddError(Res.GetString("5d9837a2-92a2-4d05-9be0-9376d0c41d88", "When using Inward Processing Areas both the Picking Area and Putaway Area must be marked as Inward Processing Areas."));
			}

			if (!areaInfo.HasErrors())
			{
				var previousAreaType = (string)Parent.Factory.Load<WhsArea>((ZGuid)areaInfo.OriginalValue)?.WA_AreaType;

				if ((areaType == AreaTypes.Codes.InwardProcessing || previousAreaType == AreaTypes.Codes.InwardProcessing) &&
					areaType != previousAreaType && ValidationHelper.CheckIfLocationsHaveStockOnHand(Parent.Factory, Parent.PK))
				{
					areaInfo.AddError(Res.GetString("55f3c7a4-91cc-4f24-821f-3faa71a1dc30", "Locations containing inventory cannot have their Area changed to or from an Inward Processing Area."));
				}
			}
		}

		#endregion

		#region CheckWLV_WA_PutawayArea

		protected override void CheckWLV_WA_PutawayArea()
		{
			base.CheckWLV_WA_PutawayArea();
			ListValidation.ErrorIfInvalidPK(Parent.WLV_WA_PutawayAreaInfo);
			if (!Parent.IsInDatabase || Parent.WLV_WA_PutawayAreaInfo.HasChanges)
			{
				CheckWLV_WA_PutawayArea_BondedArea();
				CheckInwardProcessingArea(Parent.WLV_WA_PutawayAreaInfo, Parent.WLV_PutawayAreaType, Parent.WLV_PickingAreaType);
			}
		}

		void CheckWLV_WA_PutawayArea_BondedArea()
		{
			var previousArea = Parent.Factory.Load<WhsArea>((ZGuid)Parent.WLV_WA_PutawayAreaInfo.OriginalValue);
			if (previousArea != null
					&& previousArea.PK != Parent.WLV_WA_PutawayArea
					&& previousArea.WA_AreaType == AreaTypes.Codes.Bonded)
			{
				if (ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Parent.Factory, Parent.PK))
				{
					Parent.WLV_WA_PutawayAreaInfo.AddError(Res.GetString("0B2D8EA5-1BDB-480D-82C5-D0FF821023F0", "Locations containing Customs inventory in a Bonded Putaway Area cannot change Putaway Area."));
				}
				if (ValidationHelper.CheckIfLocationsHaveTransitPackage(Parent.Factory, Parent.PK))
				{
					Parent.WLV_WA_PutawayAreaInfo.AddError(Res.GetString("82D35E4D-E139-422B-B4CC-84B46A4FD552", "Locations containing packages in a Bonded Putaway Area cannot change Putaway Area."));
				}
			}
		}

		#endregion

		#region CheckCapacity

		#region CheckWLV_MaxWeight

		protected override void CheckWLV_MaxWeight()
		{
			if (Parent.WLV_MaxWeight < 0)
			{
				Parent.WLV_MaxWeightInfo.AddError(Res.GetString("1915ec08-4154-48c1-afb9-9abe23c33517", "Please do not enter a negative max weight."));
			}
			else
			{
				CheckMaxCapacityForPickFace(Parent.WLV_MaxWeightInfo);
			}
		}

		#endregion

		#region CheckWLV_MaxCubic

		protected override void CheckWLV_MaxCubic()
		{
			if (Parent.WLV_MaxCubic < 0)
			{
				Parent.WLV_MaxCubicInfo.AddError
					(Res.GetString("8bd0b2a2-2a79-4aff-8588-a3d86e2e1c96", "Please do not enter a negative max volume."));
			}
			else
			{
				CheckMaxCapacityForPickFace(Parent.WLV_MaxCubicInfo);
			}
		}

		#endregion

		#region CheckWLV_MaxHeight

		protected override void CheckWLV_MaxHeight()
		{
			CompareValidation.CheckNumberNotNegative(Parent.WLV_MaxHeightInfo);
			CheckMaxDimensionsAllOrNothing(Parent.WLV_MaxHeightInfo);
		}

		#endregion

		#region CheckWLV_MaxDepth

		protected override void CheckWLV_MaxDepth()
		{
			CompareValidation.CheckNumberNotNegative(Parent.WLV_MaxDepthInfo);
			CheckMaxDimensionsAllOrNothing(Parent.WLV_MaxDepthInfo);
		}

		#endregion

		#region CheckWLV_MaxWidth

		protected override void CheckWLV_MaxWidth()
		{
			CompareValidation.CheckNumberNotNegative(Parent.WLV_MaxWidthInfo);
			CheckMaxDimensionsAllOrNothing(Parent.WLV_MaxWidthInfo);
		}

		#endregion

		#region CheckMaxDimensionsAllOrNothing

		void CheckMaxDimensionsAllOrNothing(ZPropertyInfo propertyInfo)
		{
			var noDimensionsEntered = Parent.WLV_MaxDepth.IsEmpty && Parent.WLV_MaxHeight.IsEmpty && Parent.WLV_MaxWidth.IsEmpty;

			if (!noDimensionsEntered)
			{
				var allDimensionsEntered = !Parent.WLV_MaxDepth.IsEmpty && !Parent.WLV_MaxHeight.IsEmpty && !Parent.WLV_MaxWidth.IsEmpty;

				if (!allDimensionsEntered)
				{
					propertyInfo.AddError(Res.GetString("0afa1763-3613-4720-ade1-7c908d6cfb49", "Either all or no Max Dimensions must be entered."));
				}
			}
		}

		#endregion

		#region CheckWLV_MaxQuantity

		protected override void CheckWLV_MaxQuantity()
		{
			if (Parent.Warehouse?.WW_WarehouseType.ToString() == WarehouseTypes.Codes.ContainerYard && Parent.WLV_MaxQuantity < 1)
			{
				Parent.WLV_MaxQuantityInfo.AddError(Res.GetString("EAB6BB21-869B-43E3-98E8-839FF2CBEAA6", "Please enter a number greater than or equal to 1 for max quantity."));
			}
			else if (Parent.WLV_MaxQuantity < 0)
			{
				Parent.WLV_MaxQuantityInfo.AddError(Res.GetString("5D134744-C9A9-4E05-96D4-E8C91C163318", "Please do not enter a negative max quantity."));
			}
			else if (IsLocationUnitsCapacityReduced())
			{
				var consumedCapacity = GetConsumedCapacityForLocation(Parent.Factory, Parent.PK);
				if (consumedCapacity > Parent.WLV_MaxQuantity)
				{
					Parent.WLV_MaxQuantityInfo.AddError(Res.GetString("E26F770C-E388-4172-9458-66B587E3A498", "There are currently {0} units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.",
							consumedCapacity.ToStringTrimZeros()));
				}
			}

			if (!Parent.WLV_MaxQuantityInfo.HasErrors())
			{
				CheckMaxCapacityForPickFace(Parent.WLV_MaxQuantityInfo);
			}
		}

		static ZDecimal GetConsumedCapacityForLocation(BusinessObjectFactory factory, ZGuid locationPK)
		{
			var dynamicBizO = new DynamicBusinessObjectCollection(factory);
			dynamicBizO.Load("SELECT ConsumedCapacityUnits FROM dbo.WhsLocationUnitsConsumed(@LocationPK)", // direct SQL
				new[] { ZSqlParameter.New("@LocationPK", locationPK, WhsLocationViewSchema.PK) });
			return dynamicBizO.Count > 0 ? (ZDecimal)dynamicBizO[0]["ConsumedCapacityUnits"] : 0;
		}

		bool IsLocationUnitsCapacityReduced()
		{
			return Parent.WLV_MaxQuantity > 0
				&& ((ZDecimal)Parent.WLV_MaxQuantityInfo.OriginalValue > Parent.WLV_MaxQuantity
					|| (ZDecimal)Parent.WLV_MaxQuantityInfo.OriginalValue == 0);
		}

		#endregion

		void CheckMaxCapacityForPickFace(ZPropertyInfo propertyInfo)
		{
			if (Parent.IsFixedLocation || Parent.IsDynamicPickFaceLocation)
			{
				if ((ZDecimal)propertyInfo.Value != 0m)
				{
					propertyInfo.AddError(MaxCapacityForPickFaceLocationErrorMessage);
				}
			}
		}

		ZString MaxCapacityForPickFaceLocationErrorMessage => Res.GetString("b2be953e-0b5e-460e-89b1-019c04e8df34", "The max capacity of Fixed or Dynamic Pick Face Location must be 0.");

		#region CheckWLV_MaxWeightUnit

		protected override void CheckWLV_MaxWeightUnit()
		{
			base.CheckWLV_MaxWeightUnit();
			ListValidation.ErrorIfInvalidCode(Parent.WLV_MaxWeightUnitInfo);

			if (Parent.WLV_MaxWeight > 0)
			{
				MandatoryValidation.CheckEntered(Parent.WLV_MaxWeightUnitInfo);
			}
		}

		#endregion

		#region CheckWLV_MaxCubicUnit

		protected override void CheckWLV_MaxCubicUnit()
		{
			base.CheckWLV_MaxCubicUnit();
			ListValidation.ErrorIfInvalidCode(Parent.WLV_MaxCubicUnitInfo);

			if (Parent.WLV_MaxCubic > 0)
			{
				MandatoryValidation.CheckEntered(Parent.WLV_MaxCubicUnitInfo);
			}
		}

		#endregion

		#region CheckWLV_MaxDimensionUnit

		protected override void CheckWLV_MaxDimensionUnit()
		{
			base.CheckWLV_MaxDimensionUnit();
			ListValidation.ErrorIfInvalidCode(Parent.WLV_MaxDimensionUnitInfo);

			if (Parent.WLV_MaxHeight > 0 || Parent.WLV_MaxDepth > 0 || Parent.WLV_MaxWidth > 0)
			{
				MandatoryValidation.CheckEntered(Parent.WLV_MaxDimensionUnitInfo);
			}
		}

		#endregion

		#endregion

		#region CheckWLV_MaximumPickCountBeforeAutomatedStocktake

		protected override void CheckWLV_MaximumPickCountBeforeAutomatedStocktake()
		{
			var location = Parent;

			if (location.WLV_MaximumPickCountBeforeAutomatedStocktake > 0 && location.WLV_MaximumPickCountBeforeAutomatedStocktake < location.WLV_FinalisedPickCount)
			{
				location.WLV_MaximumPickCountBeforeAutomatedStocktakeInfo.AddWarning(ResString.GetMultilingualString("b175c95a-dd83-4578-a418-5a4d08b50103",
					"Since the Current Touch Count is greater than the Maximum Touch Count," +
					" once a pick is finalized for this location, Stocktake will be generated and the Current Touch Count will be set back to zero."));
			}
			else
			{
				CompareValidation.CheckNumberNotNegative(location.WLV_MaximumPickCountBeforeAutomatedStocktakeInfo);
			}
		}

		#endregion

		#region CheckWLV_CheckDigit

		protected override void CheckWLV_CheckDigit()
		{
			if (Parent.WLV_CheckDigit != WhsLocation.EmptyCheckDigit && Parent.WLV_WW_Whs.IsValid)
			{
				if (!Parent.IsEligibleForCheckDigits)
				{
					Parent.WLV_CheckDigitInfo.AddError(Res.GetString("ca88b5e6-8514-4b65-98ff-45040d3aa879", "Check digits cannot be used with dock door, consolidation, or packing station locations."));
				}

				var row = Parent.Row;
				if (row?.LocationHasNonUniqueCheckDigit(Parent) ?? false)
				{
					var warningMsg = Res.GetString("cdb677fd-7537-4017-9790-5c4479a8787b",
						"There is another location with the check digit '{0}'.", Parent.WLV_CheckDigit);
					Parent.WLV_CheckDigitInfo.AddWarning(warningMsg);
				}
			}
		}

		#endregion

		// The following Checks are overriden because the Architecture cannot determine the Default Value of Columns in a View
		// and so it generates the IsNotEmpty() checks for fields that are allowed to be Empty. We override to allow empty values.

		#region CheckWLV_CheckDigitIsNotEmpty

		protected override void CheckWLV_CheckDigitIsNotEmpty() { }

		#endregion

		#region CheckWLV_FinalisedPickCountIsNotEmpty

		protected override void CheckWLV_FinalisedPickCountIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxCubicIsNotEmpty

		protected override void CheckWLV_MaxCubicIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxCubicUnitIsNotEmpty

		protected override void CheckWLV_MaxCubicUnitIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaximumPickCountBeforeAutomatedStocktakeIsNotEmpty

		protected override void CheckWLV_MaximumPickCountBeforeAutomatedStocktakeIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxQuantityIsNotEmpty

		protected override void CheckWLV_MaxQuantityIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxQuantityUnitIsNotEmpty

		protected override void CheckWLV_MaxQuantityUnitIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxWeightIsNotEmpty

		protected override void CheckWLV_MaxWeightIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxWeightUnitIsNotEmpty

		protected override void CheckWLV_MaxWeightUnitIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxHeightIsNotEmpty

		protected override void CheckWLV_MaxHeightIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxDepthIsNotEmpty

		protected override void CheckWLV_MaxDepthIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxWidthIsNotEmpty

		protected override void CheckWLV_MaxWidthIsNotEmpty() { }

		#endregion

		#region CheckWLV_MaxDimensionUnitIsNotEmpty

		protected override void CheckWLV_MaxDimensionUnitIsNotEmpty() { }

		#endregion

		#region CheckWLV_PalletFloorSpaces

		protected override void CheckWLV_PalletFloorSpaces()
		{
			base.CheckWLV_PalletFloorSpaces();

			if (Parent.WLV_PalletFloorSpaces == 0 && Parent.WLV_PalletStackHeight > 0)
			{
				Parent.WLV_PalletFloorSpacesInfo.AddError(Res.GetString("5e4b527d-9b72-4882-937a-69ad396fac9e", "Pallet Floor Spaces must be set when a Pallet Stack Height is specified."));
			}

			if (!Parent.WLV_PalletFloorSpacesInfo.HasErrors())
			{
				CheckPalletSpaces(Parent.WLV_PalletFloorSpacesInfo);
			}
		}

		#endregion

		#region CheckWLV_PalletStackHeight

		protected override void CheckWLV_PalletStackHeight()
		{
			base.CheckWLV_PalletStackHeight();

			if (Parent.WLV_PalletStackHeight == 0 && Parent.WLV_PalletFloorSpaces > 0)
			{
				Parent.WLV_PalletStackHeightInfo.AddError(Res.GetString("95ca4425-9252-4be4-a103-36b19da8e3bd", "Pallet Stack Height must be set when Pallet Floor Spaces are specified."));
			}

			if (!Parent.WLV_PalletStackHeightInfo.HasErrors())
			{
				CheckPalletSpaces(Parent.WLV_PalletStackHeightInfo);
			}
		}

		#endregion

		#region CheckWLV_PalletFloorSpacesIsNotEmpty

		protected override void CheckWLV_PalletFloorSpacesIsNotEmpty() { }

		#endregion

		#region CheckWLV_PalletStackHeightIsNotEmpty

		protected override void CheckWLV_PalletStackHeightIsNotEmpty() { }

		#endregion

		#region CheckPalletSpaces

		void CheckPalletSpaces(ZPropertyInfo propertyInfo)
		{
			var warehouseType = Parent.Warehouse?.WW_WarehouseType ?? ZString.Empty;
			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				if (Parent.WLV_PalletFloorSpaces > 0 || Parent.WLV_PalletStackHeight > 0)
				{
					if (WhsLocationType.OutboundLocationClasses.Contains(Parent.LocationType.WLT_LocationClass))
					{
						propertyInfo.AddError(Res.GetString("4dba1859-3992-41f2-b5fc-05975fa8a073", "Cannot Setup Pallet Spaces for a Location with Location Class {0}.", Parent.LocationType.WLT_LocationClass));
					}
				}

				if (Parent.WLV_PalletFloorSpaces > 0 && Parent.WLV_PalletStackHeight > 0 && !propertyInfo.HasErrors())
				{
					var row = Parent.Row;

					if (row?.LocationHasNonPalletStock(Parent) ?? false)
					{
						propertyInfo.AddError(Res.GetString("d14a7643-ec11-43c1-96f2-b35b559f5747", "Cannot Setup Pallet Spaces for a Location currently containing non-palletized stock."));
					}

					if (!propertyInfo.HasErrors())
					{
						if (row?.GetLocationProductCount(Parent) > 1)
						{
							propertyInfo.AddError(Res.GetString("68dd09cb-81b1-4110-a343-b139b28969ff", "Cannot Setup Pallet Spaces for a Location containing multiple products."));
						}
					}

					if (!propertyInfo.HasErrors())
					{
						var palletCapacity = Parent.WLV_PalletFloorSpaces * Parent.WLV_PalletStackHeight;
						var palletCount = row?.GetLocationPalletCount(Parent);
						if (palletCapacity < palletCount)
						{
							propertyInfo.AddError(Res.GetString("50c14007-8dc9-468d-a2e0-6fd986f00165", "Pallet Spaces ({0}) cannot be lower than existing Pallet Count ({1}) for this location.", palletCapacity, palletCount));
						}
					}
				}
			}
		}

		#endregion

		#region CheckWLV_CycleCountPathSequence

		protected override void CheckWLV_CycleCountPathSequenceIsNotEmpty() { }

		protected override void CheckWLV_CycleCountPathSequence()
		{
			if (Parent.WLV_CycleCountPathSequence < 0)
			{
				Parent.WLV_CycleCountPathSequenceInfo.AddError(Res.GetString("8945AF77-0368-4432-9BA3-21BBC8C307AE", "Please do not enter a negative cycle count path sequence."));
			}
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(
				WhsLocationViewSchema.Constants.WLV_WA_PickingArea,
				WhsLocationViewSchema.Constants.WLV_WA_PutawayArea,
				WhsLocationViewSchema.Constants.WLV_WR,
				WhsLocationViewSchema.Constants.WLV_SQ_DefaultPrintQueue);

		#endregion
	}
}
