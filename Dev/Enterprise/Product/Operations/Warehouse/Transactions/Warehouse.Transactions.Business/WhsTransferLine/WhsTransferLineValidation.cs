using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferLineValidation : WhsDocketLineValidation
	{
		public WhsTransferLineValidation(AutoWhsDocketLine parent)
			: base(parent)
		{
		}

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			// this property is not visible on transfer form so do not call base
		}

		#endregion

		#region Locations

		#region Transfer From Location

		#region TransferFromLocationString

		public void ValidateTransferFromLocationString()
		{
			ValidateCalculatedProperty(Parent.TransferFromLocationStringInfo);
		}

		protected void CheckTransferFromLocationString()
		{
			CheckLocationIsValid(Parent.TransferFromLocationStringInfo, Parent.WE_WL_TransferFrom);

			var transfer = Transfer;
			MandatoryValidation.CheckEntered(Parent.TransferFromLocationStringInfo);
			CheckLocationForInternalTransfer(Parent.TransferFromLocationStringInfo, transfer);

			var transferFromLocation = Parent.TransferFromLocation;
			CheckLocationForInterWarehouseTransfer(Parent.TransferFromLocationStringInfo, transfer, transferFromLocation);
			CheckTransferFromLocationForDockDoorLocation(Parent.TransferFromLocationStringInfo, transfer, transferFromLocation);

			if (!transfer?.IsTransferringForOrder ?? true)
			{
				CheckLocationIsNotInwardProcessing(Parent.TransferFromLocationStringInfo, transferFromLocation);
			}
		}

		#endregion

		#region TransferFromWarehousePK

		public void ValidateTransferFromWarehousePK()
		{
			ValidateCalculatedProperty(Parent.TransferFromWarehousePKInfo);
		}

		protected void CheckTransferFromWarehousePK()
		{
			var transfer = Transfer;
			if (transfer != null && transfer.WD_DocketSubType == TransferType.Codes.InterWhsDest)
			{
				CheckSourceAndDestinationWarehouseAreDifferent(Parent.TransferFromWarehousePKInfo);
			}
		}

		#endregion

		#endregion

		#region Location

		#region CheckLocationString

		protected override void CheckLocationString()
		{
			base.CheckLocationString();

			var transfer = Parent.Docket;
			if (Parent.IsFinalising || (transfer != null && transfer.IsMasterTransfer && transfer.WD_DocketSubType == TransferType.Codes.InterWhsSource))
			{
				MandatoryValidation.CheckEntered(Parent.LocationStringInfo);
				if (!Parent.LocationString.IsEmpty && !Parent.WE_WL.IsValid)
				{
					Parent.LocationStringInfo.AddError(Res.GetString("7032b001-ea47-4d7d-b5c5-39ea1682118e", "Location '{0}' is not valid location.", Parent.LocationString));
				}
			}

			var location = Parent.Location;
			if (location != null)
			{
				CheckLocationIsNotVoid(location, ErrorTransferIntoVoidLocation);

				var client = transfer?.Client;
				var part = Parent.SupplierPart;
				if (client != null && part != null)
				{
					// For Dock Door Transfers, we are not actually creating stock on hand in the location, but may be transferring components to a pickface where the combination will be valid
					if (transfer == null || (!transfer.IsTransferringForOrder && !IsTransferReplenishingPickAndLocationInCorrectDynamicArea()))
					{
						WhsValidationHelper.CheckProductAssignedToCorrectFixedOrDynamicLocation(location, client, part, Parent.LocationStringInfo);
						CheckLocationIsNotInwardProcessing(Parent.LocationStringInfo, location);
					}
				}

				if (!location.IsDockDoorLocation)
				{
					WhsValidationHelper.CheckLocationStockOnHand(Parent.LocationStringInfo, location, Parent);
				}
			}

			CheckLocationForInternalTransfer(Parent.LocationStringInfo, transfer);
			CheckLocationString_ForInternalTransferIsFromCorrectWarehouse(transfer);
			CheckLocationForInterWarehouseTransfer(Parent.LocationStringInfo, transfer, location);

			// Transfers on a Pick are allowed to transfer to Dock Door and Packing Station Locations.
			if (transfer == null || !transfer.IsTransferringForOrder)
			{
				var crossDockLocations = location != null && Parent.IsCrossDockPutaway
					? Parent.ReservedPickLines
							.Select(reservedLine => ((WhsPickableDocketLine)reservedLine.DocketLine).PickableDocket.WD_WL_CrossDock)
							.Distinct()
							.ToArray()
					: Array.Empty<ZGuid>();

				if (crossDockLocations.Any(location => !location.IsEmpty))
				{
					CheckLocationForCrossDockPutaway(location, crossDockLocations);
				}
				else
				{
					WhsValidationHelper.CheckLocationIsNotDockDoorLocation(Parent.LocationStringInfo, Parent, Res.GetString("96ca9677-aaa3-4bcc-a41a-08915c155c56", "You cannot putaway to Dock Door locations."));
				}
				WhsValidationHelper.CheckLocationIsNotPackingStationLocation(Parent.LocationStringInfo, location, Res.GetString("b1601541-ef76-48db-9d28-8aa969c49fc4", "You cannot putaway to Packing Station locations."));
				WhsValidationHelper.CheckLocationIsNotPackingConsolidationLocation(Parent.LocationStringInfo, location, Res.GetString("d72086de-9c89-4497-b675-eab8f1c2d3c5", "You cannot putaway to Packing Consolidation locations."));
			}

			CheckMatchingLineLocations();
			CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(Parent.LocationStringInfo, transfer, location, Parent.WE_PalletID);

			bool IsTransferReplenishingPickAndLocationInCorrectDynamicArea()
			{
				// Dynamic Locations can store non-dynamic products for replenishment but only in the dynamic area stipulated on the Pick.
				return transfer.IsTransferLinkedToPickForReplenishment
					&& location.WLV_WA_PickingArea == (transfer.PickBeingReplenished?.WP_WA_DynamicPickAreaOverride ?? ZGuid.Empty);
			}
		}

		void CheckLocationIsNotInwardProcessing(ZPropertyInfo locationStringInfo, WhsLocation location)
		{
			if (location?.IsInInwardProcessingArea ?? false)
			{
				locationStringInfo.AddError(ErrorFromToInwardProcessingArea);
			}
		}

		#region CheckMatchingLineLocations

		void CheckMatchingLineLocations()
		{
			if (!Parent.LocationStringInfo.HasErrors() && Parent.IsMainTransactionLine())
			{
				var invalidMatchingLineLocationsMessage = Res.GetString("f98abe25-3721-4132-9e3a-bfc8ff24f184", "Something went wrong when saving this Transfer line. Please delete the line and re-enter the details.");
				var parentLocationPK = Parent.WE_WL;
				if (Parent.MatchingLines.Any(ml => !ml.WE_WL.Equals(parentLocationPK)))
				{
					Parent.AddRowError(invalidMatchingLineLocationsMessage);
				}
				else
				{
					Parent.RemoveRowError(invalidMatchingLineLocationsMessage);
				}
			}
		}

		#endregion

		#region CheckLocationString_ForInternalTransferIsFromCorrectWarehouse

		void CheckLocationString_ForInternalTransferIsFromCorrectWarehouse(WhsTransfer transfer)
		{
			if (transfer != null && !transfer.IsInterWarehouseTransfer)
			{
				var sourceLocation = Parent.TransferFromLocation;
				var destLocation = Parent.Location;
				if (sourceLocation != null && destLocation != null && sourceLocation.WLV_WW_Whs != destLocation.WLV_WW_Whs)
				{
					Parent.LocationStringInfo.AddError(ErrorOriginAndDestLocationShouldBeFromSameWarehouse);
				}
			}
		}

		#endregion

		#region CheckLocationForCrossDockPutaway

		void CheckLocationForCrossDockPutaway(WhsLocation location, ZGuid[] crossDockLocations)
		{
			if (Parent.ReservedPickLines.Sum(line => line.ReservedQuantity) != Parent.WE_TransactionQuantity)
			{
				Parent.LocationStringInfo.AddError(Res.GetString("fdc9c223-c72f-42e7-9dd7-ac24547a0560", "Invalid cross dock putaway."));
			}
			else
			{
				if (crossDockLocations.Length > 1)
				{
					Parent.LocationStringInfo.AddError(Res.GetString("15f913de-d996-4726-bcf1-60cf6d6654fb", "Cross docked inventory cannot go to multiple cross dock locations."));
				}
				else if (crossDockLocations.Single() != location.PK)
				{
					Parent.LocationStringInfo.AddError(Res.GetString("344bd91a-7589-48ba-9d57-b6df972c4f75", "Putaway location is not the same as the Order Cross Dock Location."));
				}
			}
		}

		#endregion

		void CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(ZPropertyInfo info, WhsTransfer transfer, WhsLocation location, ZString palletID)
			=> WhsValidationHelper.CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(
				info,
				location,
				palletID,
				addNotificationAsError: !(transfer != null && transfer.IsReturnStockTransfer) || Parent.IsFinalising);

		#endregion

		#region ValidateDestinationWarehousePK

		public void ValidateDestinationWarehousePK()
		{
			ValidateCalculatedProperty(Parent.DestinationWarehousePKInfo);
		}

		protected virtual void CheckDestinationWarehousePK()
		{
			var transfer = Transfer;
			if (transfer != null && transfer.WD_DocketSubType == TransferType.Codes.InterWhsSource)
			{
				CheckSourceAndDestinationWarehouseAreDifferent(Parent.DestinationWarehousePKInfo);
			}
		}

		#endregion

		#endregion

		#region CheckLocationForInternalTransfer

		void CheckLocationForInternalTransfer(ZPropertyInfo info, WhsTransfer transfer)
		{
			if (transfer != null && !transfer.IsInterWarehouseTransfer)
			{
				var destLocation = Parent.Location;
				if (destLocation != null)
				{
					var sourceLocation = Parent.TransferFromLocation;
					if (sourceLocation != null && !sourceLocation.CanTransferToOrFromLocation(destLocation) && !DifferentAreaTypeIsValid(transfer, sourceLocation, destLocation))
					{
						var areaTypes = new AreaTypes();
						info.AddError(
							Res.GetString("af1d6614-9c78-44fd-89fc-4962f09c2825", "You cannot transfer stock from a {0} area to a {1} area.",
							areaTypes.GetDescriptionFromCode(sourceLocation.WLV_PickingAreaType),
							areaTypes.GetDescriptionFromCode(destLocation.WLV_PickingAreaType)));
					}
					else
					{
						CheckBothLocationsAreDifferent(info);
					}
				}
			}
		}

		static bool DifferentAreaTypeIsValid(WhsTransfer transfer, WhsLocation sourceLocation, WhsLocation destLocation)
		{
			// Putaway or Return Stock Transfers can Transfer from Dock Door Locations, so it is valid to have the area type different as long as the source area type is a Dock Door
			return ((transfer.WD_IsPutawayTransfer || transfer.IsReturnStockTransfer) && sourceLocation.WLV_PickingAreaType == AreaTypes.Codes.DockDoor)
				// Transfers on a Pick are allowed to transfer to a Dock Door Location.
				|| (transfer.IsTransferringForOrder && destLocation.WLV_PickingAreaType == AreaTypes.Codes.DockDoor);
		}

		void CheckBothLocationsAreDifferent(ZPropertyInfo info)
		{
			if (!Parent.WE_WL.IsEmpty && !Parent.WE_WL_TransferFrom.IsEmpty)
			{
				if (Parent.WE_WL == Parent.WE_WL_TransferFrom)
				{
					info.AddWarning(WarningLocationsAreSame);
				}
			}
		}

		void CheckTransferFromLocationForDockDoorLocation(ZPropertyInfo info, WhsTransfer transfer, WhsLocation sourceLocation)
		{
			if (transfer != null && sourceLocation != null)
			{
				if (transfer.WD_IsPutawayTransfer ^ sourceLocation.IsDockDoorLocation && !transfer.IsReturnStockTransfer && !transfer.IsTransferringForOrder)
				{
					info.AddError(Res.GetString("c352d5ec-39f6-4597-81d9-e8242be1a829", "Only Putaway Transfers and Cross Dock Orders can transfer out from Dock Door Locations."));
				}
			}
		}

		#endregion

		#region CheckLocationForInterWarehouseTransfer

		void CheckLocationForInterWarehouseTransfer(ZPropertyInfo info, WhsTransfer transfer, WhsLocation location)
		{
			if (transfer != null && transfer.IsInterWarehouseTransfer && location != null)
			{
				if (location.WLV_PickingAreaType == AreaTypes.Codes.Bonded)
				{
					info.AddError(ErrorFromToBondArea);
				}
				if (location.WLV_PickingAreaType == AreaTypes.Codes.Excise)
				{
					info.AddError(ErrorFromToExciseArea);
				}
			}
		}

		#endregion

		#region CheckSourceAndDestinationWarehouseAreDifferent

		void CheckSourceAndDestinationWarehouseAreDifferent(ZPropertyInfo info)
		{
			MandatoryValidation.CheckEntered(info);
			TypeValidation.CheckValidGuid(info);
			if (Parent.TransferFromWarehousePK == Parent.DestinationWarehousePK)
			{
				info.AddError(ErrorOriginAndDestWarehousesMustDiffer);
			}
		}

		#endregion

		#endregion

		#region CheckWE_PalletID

		protected override void CheckWE_PalletID()
		{
			base.CheckWE_PalletID();

			var transfer = Transfer;
			if (transfer != null)
			{
				if (Parent.IsFinalising || Parent.IsFinalised)
				{
					CheckDestinationPalletIDIsNotTransferringToMultipleLocations();
					CheckPalletIdNotAlreadyInAnotherLocation();
					CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred();
				}
				CheckPalletIdIsEmptyIfDestinationLocationIsPickFace();

				CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(Parent.WE_PalletIDInfo, transfer, Parent.Location, Parent.WE_PalletID);
			}
		}

		#region CheckDestinationPalletIDIsNotTransferringToMultipleLocations

		protected void CheckDestinationPalletIDIsNotTransferringToMultipleLocations()
		{
			if (!Parent.WE_PalletIDInfo.HasErrors() && !Parent.WE_PalletID.IsEmpty)
			{
				var isTransferringPalletIDToMultipleLocations = Parent.Docket.Lines.Any(l =>
					(!l.IsFinalised || l.WE_StockOnHand > 0)
					&& l.WE_PalletID.Equals(Parent.WE_PalletID)
					&& !l.WE_WL.Equals(Parent.WE_WL));

				if (isTransferringPalletIDToMultipleLocations)
				{
					Parent.WE_PalletIDInfo.AddError(TransferringPalletIdToMultipleLocationsError);
				}
			}
		}

		#endregion

		#region CheckDestinationPalletIdNotAlreadyInAnotherLocation

		protected void CheckPalletIdNotAlreadyInAnotherLocation()
		{
			if (!Parent.WE_PalletIDInfo.HasErrors() && !Parent.WE_PalletID.IsEmpty)
			{
				var destWarehouse = Parent.Warehouse;
				if (destWarehouse != null)
				{
					var locations = PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Parent.Factory, destWarehouse.PK, Parent.WE_PalletID);
					var otherLocationWithID = locations.FirstOrDefault(l => !l.PK.Equals(Parent.WE_WL));
					var palletAlreadyExistsInLocationThatIsNotSourceLocation =
							(otherLocationWithID != null &&
							(Parent.WE_TransferFromPalletId != Parent.WE_PalletID ||
							Parent.WE_TransferFromPalletId == Parent.WE_PalletID && otherLocationWithID.PK != Parent.WE_WL_TransferFrom));
					if (palletAlreadyExistsInLocationThatIsNotSourceLocation)
					{
						Parent.WE_PalletIDInfo.AddError(WhsValidationHelper.GetDuplicatePalletIdMessage(otherLocationWithID));
					}
				}
			}
		}

		#endregion

		#region CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred

		protected void CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred()
		{
			var transfer = Transfer;
			if (!transfer.IsInterWarehouseTransfer) // no need to check for full/partial transfer if transferring to another warehouse
			{
				if (!Parent.WE_PalletIDInfo.HasErrors() && !Parent.WE_PalletID.IsEmpty && Parent.WE_TransferFromPalletId == Parent.WE_PalletID && Parent.WE_WL_TransferFrom != Parent.WE_WL)
				{
					var inventoryQuery = new ZQuery(WhsInventoryViewSchema.WI_WL, Parent.WE_WL_TransferFrom);
					inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
					inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
					inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_PalletID, Parent.WE_TransferFromPalletId);
					var matchingsInventoryLines = new ActiveBusinessObjectCollection<WhsInventoryView>(Factory, inventoryQuery);

					var matchingsLines = (transfer.IsFinalising || transfer.IsFinalisingLines)
						? transfer.LinesToValidateWhenFinalising.Where(l => l.WE_TransferFromPalletId == Parent.WE_TransferFromPalletId)
						: new WhsTransferLine[] { Parent };

					var totalUnitsInInventory = matchingsInventoryLines.Sum(i => i.WI_TotalUnits);
					// we do not want to check stock that is picked for the pallet ID since that stock is already gone from the Location
					var totalUnitsToTransfer = matchingsLines.Where(l => !l.IsPicked).Sum(l => l.QtyToMoveIncludingMatchingLines);

					if (totalUnitsInInventory > totalUnitsToTransfer)
					{
						Parent.WE_PalletIDInfo.AddError(Res.GetString("37948729-e724-4c16-a97a-461920dd1220",
							"This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once."));
					}
				}
			}
		}

		#endregion

		#region CheckPalletIdIsEmptyIfDestinationLocationIsPickFace

		void CheckPalletIdIsEmptyIfDestinationLocationIsPickFace()
		{
			if (!Parent.WE_PalletIDInfo.HasErrors()
				&& !Parent.WE_WL.IsEmpty && !Parent.WE_PalletID.IsEmpty
				&& Parent.Product != null)
			{
				var pickFace = Parent.Product.PickFaces.FindByLocation(Parent.Docket.WD_OH_Client, Parent.WE_WL);
				if (pickFace != null && !pickFace.Location.LocationType.WLT_RetainPalletIDsInFixedPickFaces)
				{
					Parent.WE_PalletIDInfo.AddError(Res.GetString("1b94086f-16ea-43d0-9af1-873f06ff3616", "A Destination Pallet ID cannot be entered as the Destination Location is a Pick Face that does not Retain Pallet IDs."));
				}
			}
		}

		#endregion

		#endregion

		#region CheckWE_CurrentInventoryStatus

		protected override void CheckWE_CurrentInventoryStatus()
		{
			base.CheckWE_CurrentInventoryStatus();

			var parent = Parent;
			MandatoryValidation.CheckEntered(parent.WE_CurrentInventoryStatusInfo);
			ListValidation.ErrorIfInvalidCode(parent.WE_CurrentInventoryStatusInfo);
		}

		#endregion

		#region CheckHeldCode

		protected override void CheckHeldCode(string status, ZPropertyInfo propertyInfo)
		{
			if (status == InventoryStatus.Codes.Held)
			{
				MandatoryValidation.CheckEntered(propertyInfo);
			}
			else if (!(Parent.Docket?.WD_IsPutawayTransfer ?? false) && status != InventoryStatus.Codes.InTransit)
			{
				MandatoryValidation.CheckNotEntered(propertyInfo);
			}
		}

		#endregion

		#region CheckPickedBy

		public void ValidatePickedBy()
		{
			ValidateCalculatedProperty(Parent.GS_NKPickedByInfo);
		}

		protected void CheckGS_NKPickedBy()
		{
			ListValidation.ErrorIfInvalidCode(Parent.GS_NKPickedByInfo);
			CheckPickedLinesHaveAPickerAssigned();
			CheckChangePickerValidation();
		}

		void CheckPickedLinesHaveAPickerAssigned()
		{
			if (Parent.IsPicked && Parent.GS_NKPickedBy.IsEmpty)
			{
				Parent.GS_NKPickedByInfo.AddError(Res.GetString("564673fd-5a4b-47c4-bf1c-8dbeaa5b6f44", "Lines that have been picked must have a Picker."));
			}
		}

		void CheckChangePickerValidation()
		{
			var parent = Parent;
			if (!parent.GS_NKPickedByInfo.HasErrors() && parent.PickLines.Any(pl => pl.WZ_IsPicking && pl.WZ_GS_NKAssignedToInfo.HasChanges))
			{
				parent.GS_NKPickedByInfo.AddError(Res.GetString("abc788a1-986f-499a-bf43-f1f981dc8ba8", "Cannot be reassigned, Picking has already commenced."));
			}
		}

		#endregion

		#region ValidatePickedTime

		public void Validate_PickedTime() // prevent grid cell change calling validation by putting the underscore
		{
			ValidateCalculatedProperty(Parent.PickedTimeInfo);
		}

		protected void CheckPickedTime()
		{
			if (!Parent.IsPicked)
			{
				var reasonUserCannotPick = Parent.GetReasonUserCannotPick();
				if (!reasonUserCannotPick.IsEmpty)
				{
					Parent.PickedTimeInfo.AddWarning(reasonUserCannotPick);
				}
			}
		}

		#endregion

		#region CheckWE_GS_NKPutawayBy

		protected override void CheckWE_GS_NKPutawayBy()
		{
			base.CheckWE_GS_NKPutawayBy();

			CheckOnlyOnePersonCanPutawayAPalletIfFullPalletIsMoved();
			CheckPutawayLinesHavePutawayBySet();
		}

		void CheckOnlyOnePersonCanPutawayAPalletIfFullPalletIsMoved()
		{
			if (!Parent.WE_GS_NKPutawayBy.IsEmpty && Parent.WE_WL_TransferFrom != Parent.WE_WL && !Parent.WE_TransferFromPalletId.IsEmpty && Parent.WE_TransferFromPalletId == Parent.WE_PalletID)
			{
				var transfer = Transfer;
				if (transfer != null && transfer.WD_DocketSubType == TransferType.Codes.Internal)
				{
					var transferLineAssignedToAnotherUser = transfer.Lines.FirstOrDefault(l => l.WE_PalletID == Parent.WE_PalletID && !l.WE_GS_NKPutawayBy.IsEmpty && l.WE_GS_NKPutawayBy != Parent.WE_GS_NKPutawayBy);
					if (transferLineAssignedToAnotherUser != null)
					{
						Parent.WE_GS_NKPutawayByInfo.AddError(Res.GetString("9ac6ba1e-5800-4de0-9721-af034907f823", "Only one user can putaway a Pallet that is fully transferred. This Pallet is already assigned to user '{0}'.", transferLineAssignedToAnotherUser.WE_GS_NKPutawayBy));
					}
				}
			}
		}

		void CheckPutawayLinesHavePutawayBySet()
		{
			if (Parent.IsPutaway && Parent.WE_GS_NKPutawayBy.IsEmpty)
			{
				Parent.WE_GS_NKPutawayByInfo.AddError(Res.GetString("ac06b212-03ce-4e2e-b1d9-9488f0d9525e", "Enter the user that put this product away."));
			}
		}

		#endregion

		#region CheckWE_PutawayTime

		protected override void CheckWE_PutawayTime()
		{
			base.CheckWE_PutawayTime();
			CheckPutawayLinesArePicked();
			CheckPutawayLinesHaveADestinationLocation();
		}

		void CheckPutawayLinesArePicked()
		{
			if (Parent.IsPutaway && !Parent.IsPicked)
			{
				Parent.WE_PutawayTimeInfo.AddError(Res.GetString("9360bdf0-58f7-4cdc-9c73-44338291d6fe", "This product cannot be put away because it is not yet picked."));
			}
		}

		void CheckPutawayLinesHaveADestinationLocation()
		{
			if (Parent.IsPutaway && Parent.LocationString.IsEmpty)
			{
				Parent.WE_PutawayTimeInfo.AddError(Res.GetString("d294d073-0e82-411a-a159-70e42d67b5c1", "This product cannot be put away because it does not have a Destination Location."));
			}
		}

		#endregion

		#region CheckWE_OP

		protected override void CheckWE_OP()
		{
			base.CheckWE_OP();

			var part = Parent.SupplierPart;
			if (part != null)
			{
				WhsValidationHelper.CheckProductWithoutPalletConversionInLocationUsingPalletSpaces(Parent.WE_OPInfo, Parent.Location, part.OP_StockKeepingUnitPerPallet);
			}
		}

		#endregion

		#region ValidStatuses

		protected override IEnumerable<ZString> ValidStatuses => validStatuses.Value;

		readonly Lazy<IEnumerable<ZString>> validStatuses = new Lazy<IEnumerable<ZString>>(() => new ZString[] { DocketLineStatus.Codes.Entered, DocketLineStatus.Codes.HeldForTransfer, DocketLineStatus.Codes.Finalised });

		#endregion

		#region CheckArrivalDateForBinding

		public void ValidateArrivalDateForBinding()
		{
			ValidateCalculatedProperty(Parent.ArrivalDateForBindingInfo);
		}

		protected void CheckArrivalDateForBinding()
		{
			TypeValidation.CheckValidZDateTimeOffsetWithoutRange(Parent.ArrivalDateForBindingInfo);
			TypeValidation.CheckValidZDateTimeOffsetRange(Parent.ArrivalDateForBindingInfo, new TypeValidationLimits { PastYearsBeforeError = DateRangeValidation.MaximumPastYears });
		}

		#endregion

		#region ValidateQtyToMoveIncludingMatchingLines

		public void ValidateQtyToMoveIncludingMatchingLines()
		{
			if (Parent.IsMainTransactionLine())
			{
				ValidateCalculatedProperty(Parent.QtyToMoveIncludingMatchingLinesInfo);
			}
		}

		protected virtual void CheckQtyToMoveIncludingMatchingLines()
		{
			MandatoryValidation.CheckNotNegative(Parent.QtyToMoveIncludingMatchingLinesInfo);
			MandatoryValidation.CheckNotZero(Parent.QtyToMoveIncludingMatchingLinesInfo);

			if (!Parent.IsChildTransferLine)
			{
				Parent.CheckEnoughInventoryExistsToCommit(Parent.QtyToMoveIncludingMatchingLinesInfo);
			}

			if (IsAttributeValidationRequired && !Parent.QtyToMoveIncludingMatchingLinesInfo.HasErrors()
				 && !WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				PartAttributeValidation.CheckQtyForSerialNumber(Parent.Product, Parent.Docket.Client, Parent.QtyToMoveIncludingMatchingLinesInfo, Parent.WE_SerialNumberInfo);
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTransferFromLocationString();
			ValidateTransferFromWarehousePK();
			ValidateDestinationWarehousePK();
			ValidatePickedBy();
			Validate_PickedTime();
			ValidateQtyToMoveIncludingMatchingLines();
		}

		#endregion

		#region Implementation

		public override bool IsAttributeValidationRequired =>
			!Parent.IsFinalised
			&& (!IsInPreSaveValidation || !IsPickByBOMKit)
			&& Parent.SupplierPart != null && Transfer != null && Transfer.Client != null;

		bool IsInPreSaveValidation => ((IBusinessObjectInternals)Parent).IsInPreSaveValidation;

		bool IsPickByBOMKit => Parent.Factory.GetCachedValue(nameof(IsPickByBOMKit) + Parent.PK.ToString(), GetIsPickByBOMKit, CacheStalenessPolicy.StaleOnFactorySave);

		bool GetIsPickByBOMKit() => Factory.Load<WhsDocketLine>(Parent.WE_WE_OriginalDocketLineForRating)?.IsCreatedFromPickByBOM ?? false;

		protected new WhsTransferLine Parent => (WhsTransferLine)base.Parent;

		WhsTransfer Transfer => Parent.Docket;

		public static string WarningLocationsAreSame => Res.GetString("53f08ad9-9831-4466-8e97-a7138ef5db03", "The Source and Destination Locations are the same.");
		public static string ErrorOriginAndDestWarehousesMustDiffer => Res.GetString("29d09c3f-2e29-4425-9a61-158662eda04c", "The Source and Destination Warehouse cannot be the same on an Inter-Warehouse Transfer.");
		public static string ErrorOriginAndDestLocationShouldBeFromSameWarehouse => Res.GetString("9140a344-b402-4e43-a508-f4ced2e87eff", "The Source and Destination Locations cannot be from different warehouses for an Intra-Warehouse Transfer.");
		public static string ErrorFromToBondArea => Res.GetString("2754bbfa-974c-4c93-93b1-f4271afefb75", "You cannot transfer stock from or to a Bond area on an Inter-Warehouse Transfer.");
		public static string ErrorFromToExciseArea => Res.GetString("76ad35d7-0806-4eea-8f7f-64aee50aa1b3", "You cannot transfer stock from or to an Excise area on an Inter-Warehouse Transfer.");
		public static string ErrorFromToInwardProcessingArea => Res.GetString("67e81d05-5555-4514-84a7-f22a4c74a3ba", "You cannot transfer stock from or to an Inward Processing area.");
		public static string ErrorTransferIntoVoidLocation => Res.GetString("ceaab591-6861-4d9e-a88c-255095fc458d", "You cannot transfer stock into a Void Location.");
		public static string TransferringPalletIdToMultipleLocationsError => Res.GetString("62fc77e6-b327-4518-a225-99873875ffb9", "This transfer would cause the same Pallet ID to exist in multiple locations.");

		#endregion
	}
}
