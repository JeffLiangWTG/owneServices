using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveLineValidation : WhsDocketLineValidation
	{
		public WhsReceiveLineValidation(WhsReceiveLine parent)
			: base(parent)
		{
		}

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			base.CheckWE_TransactionQuantity();

			var parent = Parent;
			var info = parent.WE_TransactionQuantityInfo;
			if (parent.WE_TransactionQuantity == 0)
			{
				info.AddWarning(Res.GetString("4469d12b-d140-4fdb-9ff7-bb9c320531e1", "Quantity is equal to zero. Please remove the line unless recording an under."));
			}

			WhsValidationHelper.CheckInDocketLineUnits(info);
			var receive = parent.Docket;
			if (receive != null)
			{
				WhsValidationHelper.CheckCrossDockedUnits(info, parent.WE_ClientOrderedUnits, parent.ReservedQuantity, parent.IsInDatabase);
				var product = parent.Product;
				if (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
				{
					if (!info.HasErrors() &&
						(receive.IsFinalising || ((IBusinessObjectInternals)parent).IsInPreSaveValidation) &&
						IsSerialNumberUsed(product, receive.Client) &&
						parent.WE_TransactionQuantity != parent.SerialNumbers.Count)
					{
						info.AddError(Res.GetString("7153F394-F753-4090-9A6D-AFF681F87BC5", "The number of entered serial numbers {0} does not match the required quantity {1}. Please ensure the serial numbers match the specified quantity.", parent.SerialNumbers.Count, parent.WE_TransactionQuantity));
					}
				}
				else
				{
					WhsValidationHelper.CheckQtyForSerialNumber(PartAttributeValidation, info, parent.WE_SerialNumberInfo, receive, product);
				}

				if (((IBusinessObjectInternals)Parent).IsInPreSaveValidation
					&& receive.IsReturnReceive
					&& (!Parent.IsInDatabase || !(Parent.WE_TransactionQuantityInfo.ReadOnly && Parent.WE_ClientOrderedUnitsInfo.ReadOnly))
					&& !receive.IsReturnReceiveLineQtyValid(Parent))
				{
					Parent.WE_TransactionQuantityInfo.AddError(Res.GetString("a9809e92-909e-4373-8e26-d5129e878c7a", "Quantity is more than the available quantity to return for the released inventory from the returned order."));
				}
			}
		}

		#endregion

		#region CheckWE_WHC_NKOriginalInventoryHeldCode

		protected override void CheckWE_WHC_NKOriginalInventoryHeldCode()
		{
			base.CheckWE_WHC_NKOriginalInventoryHeldCode();

			var parent = Parent;
			var docket = parent.Docket;
			if (!parent.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty && docket != null && !docket.IsFinalised)
			{
				var heldCode = Parent.OriginalHeldCode;
				if (parent.IsInDatabase && heldCode != null && heldCode.IsDamaged && parent.ReservedPickLines.Count > 0)
				{
					parent.WE_WHC_NKOriginalInventoryHeldCodeInfo.AddError(Res.GetString("b3470370-fe1f-4a7d-a79a-35c3a268cf3e", "This Inventory is Cross Docked, you cannot set the Hold Code to Damaged."));
				}
			}
		}

		protected override void CheckHeldCode(string status, ZPropertyInfo propertyInfo)
		{
			if (status.Equals(InventoryStatus.Codes.Held))
			{
				MandatoryValidation.CheckEntered(propertyInfo);
			}
			else if (status.Equals(InventoryStatus.Codes.Available))
			{
				MandatoryValidation.CheckNotEntered(propertyInfo);
			}
		}

		#endregion

		#region CheckWE_CurrentHoldReason

		protected override void CheckWE_CurrentHoldReason()
		{
			base.CheckWE_CurrentHoldReason();

			if (!Parent.WE_CurrentHoldReason.IsEmpty && Parent.WE_WHC_NKCurrentInventoryHeldCode.IsEmpty)
			{
				Parent.WE_CurrentHoldReasonInfo.AddError(Res.GetString("f71ceed2-9252-4fbb-b58e-67838a7100ce", "Hold Reason cannot be entered if the stock is not on Hold. Enter in a Hold Code or remove the Reason."));
			}
		}

		#endregion

		#region Product

		#region CheckWE_OP

		protected override void CheckWE_OP()
		{
			base.CheckWE_OP();

			var part = Parent.SupplierPart;
			WhsValidationHelper.CheckForProductWarningMessage(Parent.Inventory.Cast<WhsInventoryView>().FirstOrDefault(), Parent.WE_OPInfo);
			WhsValidationHelper.CheckForTempProduct(Parent.Inventory.Cast<WhsInventoryView>().FirstOrDefault(), Parent.WE_OPInfo);

			var receive = Parent.Docket;
			if (part != null)
			{
				if (receive != null
					&& !receive.IsFinalised
					&& receive.IsFinalising
					&& part.ShouldPreventReceiveOfPartWithNoWeightOrDims(receive.Client))
				{
					Parent.WE_OPInfo.AddError(Res.GetString("90adbddd-cacf-42ab-bbea-de102dd1661f", "Product cannot be received as the Product Master is missing weight or dimensions."));
				}
				else
				{
					WhsValidationHelper.CheckProductHasWeightDefinition(part, Parent.WE_OPInfo);
					WhsValidationHelper.CheckProductHasCubicDefinition(part, Parent.WE_OPInfo);
					WhsValidationHelper.CheckProductHasPalletDefinition(part, Parent.WE_OPInfo);
				}
				if (Parent.WE_OPInfo.HasChanges)
				{
					WhsValidationHelper.CheckProductShouldNotBeChangedIfInventoryIsReserved(Parent.IsInDatabase, Parent.WE_OPInfo, Parent.Inventory.Cast<WhsInventoryView>().FirstOrDefault());
				}

				WhsValidationHelper.CheckProductWithoutPalletConversionInLocationUsingPalletSpaces(Parent.WE_OPInfo, Parent.Location, part.OP_StockKeepingUnitPerPallet);

				if (receive != null
					&& !IsValidReceiveLineProduct(receive))
				{
					Parent.WE_OPInfo.AddError(Res.GetString("0fb4fb02-24ec-40ba-8dd7-69a3df085342", "Product cannot be found in the associated order to return."));
				}
			}
		}

		bool IsValidReceiveLineProduct(WhsReceive receive)
		{
			return !receive.IsReturnReceive
				|| (Parent.IsInDatabase && Parent.WE_OPInfo.ReadOnly)
				|| receive.IsValidProductForReturnReceive(Parent.WE_OP);
		}

		protected override void CheckWE_OPIsValidZGuid()
		{
			// allows temp products
		}

		protected override bool AllowInactiveProducts()
		{
			var receive = Parent.Docket;
			return base.AllowInactiveProducts() || (receive != null && receive.IsFinalising && Parent.WE_TransactionQuantity == 0);
		}

		#endregion

		#region CheckProductDesc

		protected override void CheckProductDesc()
		{
			base.CheckProductDesc();

			if (Parent.IsTemporaryProduct)
			{
				if (Parent.ProductDesc.IsEmpty)
				{
					Parent.ProductDescInfo.AddWarning(WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning);
				}
				WhsValidationHelper.CheckJobHasRecordWithSameProductCodeButDifferentPropertyValue(Parent.Docket, Parent.ProductDescInfo); // Check for Empty since other Inventories can be filled
			}
		}

		#endregion

		#region CheckCommodityCode

		protected override void CheckCommodityCode()
		{
			WhsValidationHelper.CheckCommodityCode(Parent.Docket, Parent.CommodityCodeInfo);
		}

		#endregion

		#region CheckProductUQ

		protected override void CheckProductUQ()
		{
			WhsValidationHelper.CheckStockKeepingUnits(Parent.Docket, Parent.ProductUQInfo);
		}

		#endregion

		#endregion

		#region CheckWE_WL

		protected override void CheckWE_WL()
		{
			base.CheckWE_WL();

			var receive = Parent.Docket;
			if (receive != null && !receive.IsFinalised)
			{
				var location = Parent.Location;
				WhsValidationHelper.CheckLocationIsRequiredWhenFinalising(receive, Parent.WE_TransactionQuantity, Parent.WE_WLInfo);
				WhsValidationHelper.CheckLocationIsInCorrectWarehouse(receive, Parent.WE_WLInfo, location);
				WhsValidationHelper.CheckLocationIsNotVoid(Parent.WE_WLInfo, location);
				CheckLocationIsOriginalInventoryStatusReceived();

				if (!receive.IsCreatedFromPickByBOM)
				{
					WhsValidationHelper.CheckLocationIsNotPackingStationLocation(Parent.WE_WLInfo, location, Res.GetString("2f7a6466-02af-4edd-afb8-7d72817c6b55", "Please enter a valid location, the Location you entered is a Packing Station Location."));
					WhsValidationHelper.CheckLocationIsNotPackingConsolidationLocation(Parent.WE_WLInfo, location, Res.GetString("4fe87ee2-6312-402c-bfcb-cb64871d061a", "Please enter a valid location, the Location you entered is a Packing Consolidation Location."));
					WhsValidationHelper.CheckLocationForReceiptEntry(Parent.WE_WLInfo, location, Parent.Warehouse, Parent);

					var client = receive.Client;
					var part = Parent.SupplierPart;
					if (location != null && client != null && part != null)
					{
						WhsValidationHelper.CheckProductAssignedToCorrectFixedOrDynamicLocation(location, client, part, Parent.WE_WLInfo);
					}
				}

				CheckIfReceivedReceiveLinesArePutaway(location);
				LocationInBondAreaValidation(receive, location);
				LocationInwardProcessingAreaValidation(receive, location);

				WhsValidationHelper.CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(Parent.WE_WLInfo, location, Parent.WE_PalletID);
			}
		}

		void LocationInBondAreaValidation(WhsReceive receive, WhsLocation location)
		{
			if (location != null)
			{
				if (receive.WD_DocketSubType == ReceiveType.Codes.Customs && !receive.WD_IsInwardsProcessingJob && location.WLV_PutawayAreaType != AreaTypes.Codes.Bonded && location.WLV_PutawayAreaType != AreaTypes.Codes.Excise)
				{
					Parent.WE_WLInfo.AddError(Res.GetString("b43f9b18-6f0c-4828-b56a-5ba85fc33940", "A Customs Receipt can only receive into locations that are in a Bonded or Excise Area."));
				}
				else if (receive.WD_DocketSubType != ReceiveType.Codes.Customs && (location.WLV_PutawayAreaType == AreaTypes.Codes.Bonded || location.WLV_PutawayAreaType == AreaTypes.Codes.Excise))
				{
					Parent.WE_WLInfo.AddError(Res.GetString("9a9de929-b43b-4586-a6ac-13e665e1f03b", "A Goods Receipt cannot receive into locations in a Bonded or Excise Area."));
				}
			}
		}

		void CheckIfReceivedReceiveLinesArePutaway(WhsLocation location)
		{
			if (location != null
				&& Parent.WE_StockOnHand > 0
				&& Parent.WE_CurrentInventoryStatus == InventoryStatus.Codes.Received
				&& Parent.Docket.IsFinalising)
			{
				var receiveLineLocationTypeClass = location.WLV_LocationClass;
				if (receiveLineLocationTypeClass == LocationClasses.Codes.DDL && Parent.PutawayTransferLine == null)
				{
					Parent.WE_WLInfo.AddError(Res.GetString("39e4740a-53da-4080-a7eb-84a71e547db3", "This 'Received To Dock Door' receive line is still in a dock door location.\r\n Please either change Location to a non-dock door location or utilize putaway transfer to move the stock."));
				}
			}
		}

		void LocationInwardProcessingAreaValidation(WhsReceive receive, WhsLocation location)
		{
			if (receive.WD_IsInwardsProcessingJob)
			{
				if (!(location?.IsInInwardProcessingArea ?? true))
				{
					Parent.WE_WLInfo.AddError(Res.GetString("2e5e1255-559c-4a69-8b15-705c07195b5b", "Please enter a valid location, the Location you have entered is not in an Inward Processing area."));
				}
			}
			else
			{
				WhsValidationHelper.CheckLocationIsNotInAnInwardProcessingArea(Parent.WE_WLInfo, location);
			}
		}

		void CheckLocationIsOriginalInventoryStatusReceived()
		{
			if (!Parent.WE_WLInfo.HasErrors()
				&& Parent.IsInDatabase
				&& (ZGuid)Parent.WE_WLInfo.OriginalValue != Parent.WE_WL
				&& (ZString)Parent.WE_OriginalInventoryStatusInfo.OriginalValue == InventoryStatus.Codes.Received)
			{
				Parent.WE_WLInfo.AddError(Res.GetString("BAA386F7-9083-437C-B1AD-BB7EC99131A5", "Location cannot be updated if the original value of the inventory status is Received."));
			}
		}

		#endregion

		#region CheckWE_DocketLineStatus

		protected override void CheckWE_DocketLineStatus()
		{
			base.CheckWE_DocketLineStatus();
			CheckUNDG();
		}

		bool IsUNDGValidationRequired
		{
			get
			{
				var location = Parent.Location;
				return location != null && !location.IsDockDoorLocation && Parent.WE_CurrentInventoryStatus == InventoryStatus.Codes.Putaway;
			}
		}

		void CheckUNDG()
		{
			var propertyInfo = Parent.WE_DocketLineStatusInfo;
			var productPK = Parent.WE_OP;
			if (productPK.IsValid && Parent.WE_TransactionQuantity > 0 && !propertyInfo.HasErrors() && IsUNDGValidationRequired)
			{
				var receive = Parent.Docket;
				var docketUNDGValidationCache = receive.DocketUNDGValidationCache;
				var productUNDGInfosCache = docketUNDGValidationCache.ProductUNDGInfos;
				if (productUNDGInfosCache != null && productUNDGInfosCache.TryGetValue(productPK, out var productUNDGInfos))
				{
					var validationResult = DocketUNDGValidationHelper.CheckUNDGTotalsForProductsInWarehouse(docketUNDGValidationCache, productUNDGInfos);
					if (!validationResult.OverLimitMessage.IsEmpty)
					{
						propertyInfo.AddError(receive.Validation.BuildUNDGLimitExceededErrorWhenSaving(validationResult.OverLimitMessage));
					}
					else if (!validationResult.OverThresholdMessage.IsEmpty)
					{
						propertyInfo.AddWarning(receive.Validation.BuildUNDGWarningPercentageExceededErrorWhenSaving(receive.Warehouse.WW_DGThresholdPercentage, validationResult.OverThresholdMessage));
					}
				}
			}
		}

		#endregion

		#region CheckLocationString

		protected override void CheckLocationString()
		{
			// location string is not visible on form therefore should not be validated.
		}

		#endregion

		#region ValidateSplitQuantity

		public void ValidateSplitQuantity()
		{
			ValidateCalculatedProperty(Parent.SplitQuantityInfo);
		}

		protected virtual void CheckSplitQuantity()
		{
			WhsValidationHelper.CheckSplitQuantity(Parent.SplitQuantityInfo, Parent.WE_TransactionQuantity);
		}

		#endregion

		#region ValidateReservedQuantity

		public void ValidateReservedQuantity()
		{
			ValidateCalculatedProperty(Parent.ReservedQuantityInfo);
		}

		protected virtual void CheckReservedQuantity()
		{
			var receive = Parent.Docket;
			if (receive != null && !receive.IsFinalised && receive.IsFinalising)
			{
				if (Parent.ReservedQuantity > Parent.WE_TransactionQuantity)
				{
					Parent.ReservedQuantityInfo.AddError(Res.GetString("7e1fde04-c038-4f04-a544-74730cb79135", "This Receive Line has {0} units reserved, you cannot finalize while the received quantity is less than this.", Parent.ReservedQuantity));
				}
			}
		}

		#endregion

		#region PartAttributes

		#region CheckWE_ExpiryDate

		protected override void CheckWE_ExpiryDate()
		{
			base.CheckWE_ExpiryDate();

			var receive = Parent.Docket;
			if (receive != null && !receive.IsFinalised && !receive.IsFinalising)
			{
				PartAttributeValidation.ValidateExpiryDateAgainstExpiryNotificationPeriod(receive.Client, receive.Warehouse, Parent.Product, Parent.WE_ExpiryDate, Parent.WE_ExpiryDateInfo);
			}
		}

		#endregion

		#region IsAttributeValidationRequired

		public override bool IsAttributeValidationRequired
		{
			get
			{
				var receive = Parent.Docket;
				return receive != null
					&& (receive.IsFinalising || (Parent.WE_CurrentInventoryStatus == InventoryStatus.Codes.Received))
					&& (Parent.WE_StockOnHand > 0m && !Parent.IsCreatedFromPickByBOM);
			}
		}

		#endregion

		#region IsJulianBatchNumberFormatValidationRequired

		protected override bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo, int attributeNumber)
		{
			return IsAttributeValidationRequired;
		}

		#endregion

		#region IsReleaseCapturedValidationRequired

		protected override bool IsReleaseCapturedValidationRequired => true;

		#endregion

		#region CheckWE_PartAttrib1

		protected override void CheckWE_PartAttrib1()
		{
			base.CheckWE_PartAttrib1();
			AddApplicationDefinedAttributeWarnings(Parent.WE_PartAttrib1Info, 1);
		}

		#endregion

		#region CheckWE_PartAttrib2

		protected override void CheckWE_PartAttrib2()
		{
			base.CheckWE_PartAttrib2();
			AddApplicationDefinedAttributeWarnings(Parent.WE_PartAttrib2Info, 2);
		}

		#endregion

		#region CheckWE_PartAttrib3

		protected override void CheckWE_PartAttrib3()
		{
			base.CheckWE_PartAttrib3();
			AddApplicationDefinedAttributeWarnings(Parent.WE_PartAttrib3Info, 3);
		}

		#endregion

		#region CheckWE_SerialNumber

		protected override void CheckWE_SerialNumber()
		{
			if (!WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				base.CheckWE_SerialNumber();
				if (Parent.Inventory.Count > 0 && !Parent.Inventory[0].ValidationSerialNumberWarningMessage.IsEmpty)
				{
					Parent.WE_SerialNumberInfo.AddWarning(Parent.Inventory[0].ValidationSerialNumberWarningMessage);
				}
				CheckSerialNumberIsUnique();
			}
		}

		#endregion

		#region AddApplicationDefinedAttributeWarnings

		void AddApplicationDefinedAttributeWarnings(ZPropertyInfo info, int attributeNumber)
		{
			if (Parent.Inventory.Count > 0 && !Parent.Inventory[0].ValidationPartAttribWarningMessage[attributeNumber].IsEmpty)
			{
				info.AddWarning(Parent.Inventory[0].ValidationPartAttribWarningMessage[attributeNumber]);
			}
		}

		#endregion

		#region CheckSerialNumberIsUnique

		void CheckSerialNumberIsUnique()
		{
			var parent = Parent;
			var receive = parent.Docket;
			if (receive != null)
			{
				var product = parent.Product;
				var client = receive.Client;
				if (IsSerialNumberUsed(product, client))
				{
					if (IsAttributeValidationRequired)
					{
						MandatoryValidation.CheckEntered(parent.WE_SerialNumberInfo);
						CheckSerialNumberIsUniqueCore(receive, client, checkInDB: true);
					}
					else if (!receive.IsFinalised)
					{
						CheckSerialNumberIsUniqueCore(receive, client, checkInDB: false);
					}
				}
			}
		}

		bool IsSerialNumberUsed(WhsProduct product, OrgHeader client) =>
			product != null && client != null && !product.IsSerialNumberReleaseCaptured(client) && product.IsSerialNumberUsed(client);

		void CheckSerialNumberIsUniqueCore(WhsReceive receive, OrgHeader client, bool checkInDB)
		{
			if (Parent.Inventory.Count > 0 && ShouldCheckSerialNumberIsUnique(receive.StartedReceiving))
			{
				var isSerialNumberUnique = receive.ReceiveValidationStrategy.CheckSerialNumberIsUnique(client, Parent.Inventory[0], checkInDB);
				if (!isSerialNumberUnique)
				{
					Parent.WE_SerialNumberInfo.AddError(Res.GetString("99dfd59b-5022-42bb-a0be-2b465c9e172e", "Serial # already used."));
				}
			}
		}

		bool ShouldCheckSerialNumberIsUnique(bool receiveStartedReceiving)
		{
			var parent = Parent;
			return (parent.WE_StockOnHand > 0 || (!receiveStartedReceiving && parent.WE_ClientOrderedUnits > 0));
		}

		#endregion

		#endregion

		#region CheckWE_PalletID

		protected override void CheckWE_PalletID()
		{
			base.CheckWE_PalletID();

			CheckIsPalletIdAssignedOnDockDoorLocation();
			CheckPalletIdIsEmptyIfLocationIsPickFace();
			CheckPalletIdIsNotEmptyIfPalletIDEnforced();

			var parentInventory = Parent.Inventory;
			if (!Parent.WE_PalletIDInfo.HasErrors() && parentInventory.Count > 0 && Parent.WE_StockOnHand > 0)
			{
				WhsValidationHelper.CheckPalletID(Parent.WE_PalletIDInfo, Parent.WE_WL, Parent.Docket, Parent, parentInventory[0].ValidationPalletIDWarningMessage);
			}

			Parent.ReceiveLineValidationStrategy.CheckPalletIDAssignedToPutawayTransfer(Parent);
			WhsValidationHelper.CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(Parent.WE_PalletIDInfo, Parent.Location, Parent.WE_PalletID);
		}

		void CheckIsPalletIdAssignedOnDockDoorLocation()
		{
			if (!Parent.WE_PalletIDInfo.HasErrors()
				&& Parent.WE_PalletID.IsEmpty
				&& !Parent.IsPutaway
				&& (Parent.Location?.IsDockDoorLocation ?? false))
			{
				Parent.WE_PalletIDInfo.AddError(Res.GetString("dae797df-2057-4b26-92cf-e8c4e4ce76ed", "You must have a Pallet ID entered when using dock door locations."));
			}
		}

		void CheckPalletIdIsEmptyIfLocationIsPickFace()
		{
			if (!Parent.WE_PalletIDInfo.HasErrors()
					&& !Parent.WE_WL.IsEmpty
					&& !Parent.WE_PalletID.IsEmpty
					&& Parent.Product != null)
			{
				var pickFace = Parent.Product.PickFaces.FindByLocation(Parent.Docket.WD_OH_Client, Parent.WE_WL);
				if (pickFace != null && !pickFace.Location.LocationType.WLT_RetainPalletIDsInFixedPickFaces)
				{
					Parent.WE_PalletIDInfo.AddError(Res.GetString("66c4e660-178d-4d42-8d6e-a0e605de92ac", "A Pallet ID cannot be entered as the Location is a Pick Face that does not Retain Pallet IDs."));
				}
			}
		}

		void CheckPalletIdIsNotEmptyIfPalletIDEnforced()
		{
			var parent = Parent;
			var docket = parent.Docket;

			if (docket != null && docket.IsFinalising && parent.WE_PalletID.IsEmpty && parent.WE_TransactionQuantity > 0)
			{
				var client = docket.Client;

				if (client != null)
				{
					var clientParams = WhsClientParams.GetClientParams(client);
					var whsClientParameterByWarehouseCollection = clientParams.ClientParametersByWarehouse;
					var whsClientParameterByWarehouse = whsClientParameterByWarehouseCollection.FindWithEmptyFallback(client.PK, docket.WD_WW_Whs, docket.WD_ReceiveCategory);

					if (whsClientParameterByWarehouse != null && whsClientParameterByWarehouse.WY_EnforcePalletIDEntry && parent.Product.PickFaces.FindByLocation(docket.WD_OH_Client, parent.WE_WL) == null)
					{
						Parent.WE_PalletIDInfo.AddError(Res.GetString("cda24199-215d-4c47-8849-f6395557b900", "You must have a Pallet ID upon finalizing when pallet ID entries are enforced."));
					}
				}
			}
		}

		#endregion

		#region CheckWE_ClientOrderedUnits

		protected override void CheckWE_ClientOrderedUnits()
		{
			base.CheckWE_ClientOrderedUnits();

			var receive = Parent.Docket;
			if (receive != null)
			{
				WhsValidationHelper.CheckCrossDockedUnits(Parent.WE_ClientOrderedUnitsInfo, Parent.WE_TransactionQuantity, Parent.ReservedQuantity, Parent.IsInDatabase);
				if (!WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
				{
					WhsValidationHelper.CheckQtyForSerialNumber(PartAttributeValidation, Parent.WE_ClientOrderedUnitsInfo, Parent.WE_SerialNumberInfo, receive, Parent.Product);
				}
			}
		}

		#endregion

		#region ValidateConsigneeNameOrPK

		public void ValidateConsigneeNameOrPK()
		{
			// we don't want ConsigneeNameOrPKInfo being hit if not necessary for performance reasons.
			if (Parent.ShouldValidateConsignee)
			{
				ValidateCalculatedProperty(Parent.ConsigneeNameOrPKInfo);
			}
		}

		protected void CheckConsigneeNameOrPK()
		{
			WhsValidationHelper.CheckConsigneeNameOrPK(Parent, (ZWrappedPropertyInfo)Parent.ConsigneeNameOrPKInfo);
		}

		#endregion

		#region ValidStatuses

		protected override IEnumerable<ZString> ValidStatuses => validStatuses.Value;

		readonly Lazy<IEnumerable<ZString>> validStatuses = new Lazy<IEnumerable<ZString>>(() => new ZString[] { string.Empty, DocketLineStatus.Codes.PickedForUnload, DocketLineStatus.Codes.Finalised, DocketLineStatus.Codes.Cancelled });

		#endregion

		#region Implementation

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateConsigneeNameOrPK();
			ValidateSplitQuantity();
			ValidateReservedQuantity();
		}

		protected new WhsReceiveLine Parent
		{
			get { return (WhsReceiveLine)base.Parent; }
		}

		#endregion
	}
}

