using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketLineValidation : AutoWhsDocketLineValidation
	{
		public WhsDocketLineValidation(AutoWhsDocketLine parent)
			: base(parent)
		{
		}

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			if (IsAttributeValidationRequired && !Parent.WE_TransactionQuantityInfo.HasErrors()
				&& !WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				PartAttributeValidation.CheckQtyForSerialNumber(Parent.Product, Parent.Docket.Client, (ZPropertyInfoDecimal)Parent.WE_TransactionQuantityInfo, Parent.WE_SerialNumberInfo);
			}
		}

		#endregion

		#region CheckWE_F3_NKPackType

		protected override void CheckWE_F3_NKPackType()
		{
			MandatoryValidation.CheckEntered(Parent.WE_F3_NKPackTypeInfo);
			if (!Parent.WE_F3_NKPackTypeInfo.HasErrors() && Parent.SupplierPart != null)
			{
				if (!Parent.Product.IsPackTypeUsedByProduct(Parent.WE_F3_NKPackType))
				{
					Parent.WE_F3_NKPackTypeInfo.AddNotification(WarehouseDataRegistry.Instance.PackTypesValidation.Value ? CargoWise.ComponentModel.NotificationType.Error : CargoWise.ComponentModel.NotificationType.Warning,
						Res.GetString("c401a427-742d-4be1-84cb-fc9b03e5de53", "The Pack Type {0} has not been defined for this Product.\r\nPack Types are defined on the Maintain -> Warehouse -> Products -> Details -> Unit Conversions Tab.\r\nBecause this Pack Type is not defined, a conversion to Units is not possible.", Parent.WE_F3_NKPackType));
				}
			}
		}

		#endregion

		#region Product

		protected override void CheckWE_OP()
		{
			base.CheckWE_OP();
			ValidatePartsForWE_OP();
		}

		void ValidatePartsForWE_OP()
		{
			if (!Parent.ReadOnly)
			{
				var part = Parent.SupplierPart;
				var docket = Parent.Docket;
				var client = docket != null ? docket.WD_OH_Client : ZGuid.Empty;
				WhsValidationHelper.CheckProductIsValid(part, client, Parent.WE_OPInfo, AllowInactiveProducts());
				CheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed();
			}
		}

		protected virtual bool AllowInactiveProducts()
		{
			return false;
		}

		protected virtual void CheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed()
		{
			var docket = Parent.Docket;
			if (docket != null)
			{
				PartAttributeValidation.ValidateCanCalculateExpiryDateIfJulianBatchNumberIsUsed(docket.Client, docket.Warehouse, Parent.Product, Parent.WE_OPInfo);
			}
		}

		#region ValidateProductDesc

		public void ValidateProductDesc()
		{
			ValidateCalculatedProperty(Parent.ProductDescInfo);
		}

		protected virtual void CheckProductDesc()
		{
		}

		#endregion

		#region ValidateProductUQ

		public void ValidateProductUQ()
		{
			ValidateCalculatedProperty(Parent.ProductUQInfo);
		}

		protected virtual void CheckProductUQ()
		{
		}

		#endregion

		#region ValidateCommodityCode

		public void ValidateCommodityCode()
		{
			ValidateCalculatedProperty(Parent.CommodityCodeInfo);
		}

		protected virtual void CheckCommodityCode()
		{
		}

		#endregion

		#endregion

		#region CheckWE_AdjustmentArrivalDateIsValidZDateTimeRange

		protected override void CheckWE_AdjustmentArrivalDateIsValidZDateTimeOffsetRange()
		{
			var limits = Parent.GetExpectedPackingDateValidationRange();
			new DateRangeValidation(limits).ValidateDateValueHasChanged(Parent.WE_AdjustmentArrivalDateInfo);
		}

		#endregion

		#region CheckWE_RequiredByDateIsValidZDateTimeRange

		protected override void CheckWE_RequiredByDateIsValidZDateTimeOffsetRange()
		{
			var limits = Parent.GetExpectedPackingDateValidationRange();
			new DateRangeValidation(limits).ValidateDateValueHasChanged(Parent.WE_RequiredByDateInfo);
		}

		#endregion

		#region Attributes

		#region CheckWE_ExpiryDate

		protected override void CheckWE_ExpiryDate()
		{
			if (IsAttributeValidationRequired)
			{
				PartAttributeValidation.CheckExpiryDate(Parent.Docket.Client, Parent.SupplierPart, Parent.WE_ExpiryDateInfo);
			}
		}

		protected override void CheckWE_ExpiryDateIsValidZDate()
		{
			TypeValidation.CheckValidZDateWithoutRange(Parent.WE_ExpiryDateInfo);
		}

		protected override void CheckWE_ExpiryDateIsValidZDateRange()
		{
			var limits = Parent.GetExpectedExpiryDateValidationRange();
			new DateRangeValidation(limits).ValidateDateValueHasChanged(Parent.WE_ExpiryDateInfo);
		}

		#endregion

		#region CheckWE_PackingDate

		protected override void CheckWE_PackingDate()
		{
			if (IsAttributeValidationRequired)
			{
				PartAttributeValidation.CheckPackingDate(Parent.Docket.Client, Parent.SupplierPart, Parent.WE_PackingDateInfo);
			}
		}

		protected override void CheckWE_PackingDateIsValidZDate()
		{
			TypeValidation.CheckValidZDateWithoutRange(Parent.WE_PackingDateInfo);
		}

		protected override void CheckWE_PackingDateIsValidZDateRange()
		{
			var limits = Parent.GetExpectedPackingDateValidationRange();
			new DateRangeValidation(limits).ValidateDateValueHasChanged(Parent.WE_PackingDateInfo);
		}

		#endregion

		#region CheckWE_PartAttrib1

		protected override void CheckWE_PartAttrib1()
		{
			base.CheckWE_PartAttrib1();
			CheckPartAttribute(Parent.WE_PartAttrib1Info, 1);
		}

		#endregion

		#region CheckWE_PartAttrib2

		protected override void CheckWE_PartAttrib2()
		{
			base.CheckWE_PartAttrib2();
			CheckPartAttribute(Parent.WE_PartAttrib2Info, 2);
		}

		#endregion

		#region CheckWE_PartAttrib3

		protected override void CheckWE_PartAttrib3()
		{
			base.CheckWE_PartAttrib3();
			CheckPartAttribute(Parent.WE_PartAttrib3Info, 3);
		}

		#endregion

		#region CheckWE_SerialNumber

		protected override void CheckWE_SerialNumber()
		{
			base.CheckWE_SerialNumber();
			CheckSerialNumber(Parent.WE_SerialNumberInfo);
		}

		void CheckSerialNumber(ZPropertyInfo info)
		{
			var docket = Parent.Docket;
			if (docket != null)
			{
				if (!info.HasErrors() && IsAttributeValidationRequired)
				{
					var product = Parent.Product;
					if (product != null)
					{
						var client = docket.Client;
						if (!product.IsSerialNumberReleaseCaptured(client))
						{
							PartAttributeValidation.CheckSerialNumber(client, product.Parent, info);
						}
					}
				}

				if (!info.HasErrors() && IsReleaseCapturedValidationRequired)
				{
					PartAttributeValidation.ValidateSerialIsNotReleaseCapturedWithValue(Parent.Product, docket.Client, info);
				}
			}

			WhsValidationHelper.CheckIfTrimIsNeeded(info);
		}

		#endregion

		#region CheckWE_CustomAttrib1

		protected override void CheckWE_CustomAttrib1()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomAttrib1Info);
		}

		#endregion

		#region CheckWE_CustomAttrib2

		protected override void CheckWE_CustomAttrib2()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomAttrib2Info);
		}

		#endregion

		#region CheckWE_CustomAttrib3

		protected override void CheckWE_CustomAttrib3()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomAttrib3Info);
		}

		#endregion

		#region CheckWE_CustomAttrib4

		protected override void CheckWE_CustomAttrib4()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomAttrib4Info);
		}

		#endregion

		#region CheckWE_CustomAttrib5

		protected override void CheckWE_CustomAttrib5()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomAttrib5Info);
		}

		#endregion

		#region CheckWE_CustomAttrib6

		protected override void CheckWE_CustomAttrib6()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomAttrib6Info);
		}

		#endregion

		#region CheckWE_CustomDecimal1

		protected override void CheckWE_CustomDecimal1()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDecimal1Info);
		}

		#endregion

		#region CheckWE_CustomDecimal2

		protected override void CheckWE_CustomDecimal2()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDecimal2Info);
		}

		#endregion

		#region CheckWE_CustomDecimal3

		protected override void CheckWE_CustomDecimal3()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDecimal3Info);
		}

		#endregion

		#region CheckWE_CustomDecimal4

		protected override void CheckWE_CustomDecimal4()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDecimal4Info);
		}

		#endregion

		#region CheckWE_CustomDecimal5

		protected override void CheckWE_CustomDecimal5()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDecimal5Info);
		}

		#endregion

		#region CheckWE_CustomDate1

		protected override void CheckWE_CustomDate1()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDate1Info);
		}

		#endregion

		#region CheckWE_CustomDate2

		protected override void CheckWE_CustomDate2()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDate2Info);
		}

		#endregion

		#region CheckWE_CustomDate3

		protected override void CheckWE_CustomDate3()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDate3Info);
		}

		#endregion

		#region CheckWE_CustomDate4

		protected override void CheckWE_CustomDate4()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDate4Info);
		}

		#endregion

		#region CheckWE_CustomDate5

		protected override void CheckWE_CustomDate5()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomDate5Info);
		}

		#endregion

		#region CheckWE_CustomFlag1

		protected override void CheckWE_CustomFlag1()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomFlag1Info);
		}

		#endregion

		#region CheckWE_CustomFlag2

		protected override void CheckWE_CustomFlag2()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomFlag2Info);
		}

		#endregion

		#region CheckWE_CustomFlag3

		protected override void CheckWE_CustomFlag3()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomFlag3Info);
		}

		#endregion

		#region CheckWE_CustomFlag4

		protected override void CheckWE_CustomFlag4()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomFlag4Info);
		}

		#endregion

		#region CheckWE_CustomFlag5

		protected override void CheckWE_CustomFlag5()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomFlag5Info);
		}

		#endregion

		#region CheckWE_CustomTextBlob1

		protected override void CheckWE_CustomTextBlob1()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.WE_CustomTextBlob1Info);
		}

		#endregion

		#region CustomLabelsProvider

		WhsDocketLine.CustomLabelsProvider CustomLabelsProvider
		{
			get { return customLabelsProvider ?? (customLabelsProvider = new WhsDocketLine.CustomLabelsProvider(Parent.Docket)); }
		}

		WhsDocketLine.CustomLabelsProvider customLabelsProvider;

		#endregion

		#region CustomLabelPropertyValidation

		CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}

		CustomLabelPropertyValidation customLabelPropertyValidation;

		#endregion

		void CheckPartAttribute(ZPropertyInfo info, int attributeNumber)
		{
			var docket = Parent.Docket;
			if (docket != null)
			{
				if (!info.HasErrors() && IsAttributeValidationRequired)
				{
					var product = Parent.Product;
					if (product != null)
					{
						var client = docket.Client;
						if (!product.IsPartAttribReleaseCaptured(client, attributeNumber))
						{
							PartAttributeValidation.CheckAttribute(client, product.Parent, info, attributeNumber);
						}
					}
				}

				if (!info.HasErrors() && IsReleaseCapturedValidationRequired)
				{
					PartAttributeValidation.ValidatePartAttribIsNotReleaseCapturedWithValue(Parent.Product, docket.Client, info, attributeNumber);
				}

				if (!info.HasErrors() && IsJulianBatchNumberFormatValidationRequired(info, attributeNumber))
				{
					PartAttributeValidation.CheckJulianBatchNumberAttributeFormat(docket.Client, Parent.Product, info, attributeNumber);
				}
			}

			WhsValidationHelper.CheckIfTrimIsNeeded(info);
		}

		public virtual bool IsAttributeValidationRequired => false;

		protected virtual bool IsReleaseCapturedValidationRequired => IsAttributeValidationRequired;

		protected virtual bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo, int attributeNumber)
		{
			return IsAttributeValidationRequired;
		}

		#endregion

		#region CheckWE_WL

		protected override void CheckWE_WL()
		{
			base.CheckWE_WL();

			CheckLocationHasEnoughCapacity(Parent.WE_WLInfo);
		}

		#endregion

		#region Location

		protected virtual void CheckLocationString()
		{
			CheckLocationIsValidWarehouseSet();
			CheckLocationIsValid(Parent.LocationStringInfo, Parent.WE_WL);
			CheckLocationIsInDocketWarehouse();
			CheckLocationHasEnoughCapacity(Parent.LocationStringInfo);
		}

		#region CheckLocationIsValidWarehouseSet

		void CheckLocationIsValidWarehouseSet()
		{
			if (!Parent.LocationStringInfo.HasErrors() && Parent.WarehousePK.IsEmpty)
			{
				Parent.LocationStringInfo.AddError(Res.GetString("e5917de7-1a08-4d66-a3e5-713f63936038", "A valid warehouse has not been selected for this {0}.", Parent.HumanReadableName));
			}
		}

		#endregion

		#region CheckLocationIsValid

		protected static void CheckLocationIsValid(ZPropertyInfo info, ZGuid locationPK)
		{
			if (!info.HasErrors() && locationPK == ZGuid.Invalid)
			{
				info.AddError(Res.GetString("86631c1e-7c6b-4758-ad26-ad07dfd24522", "Please enter a valid Location."));
			}
		}

		#endregion

		#region CheckLocationIsInDocketWarehouse

		void CheckLocationIsInDocketWarehouse()
		{
			if (!Parent.LocationStringInfo.HasErrors())
			{
				var docket = Parent.Docket;
				if (docket != null && docket.WD_DocketSubType != TransferType.Codes.InterWhsSource)
				{
					var location = Parent.Location;
					if (location != null)
					{
						var row = location.Row;
						if (row != null && docket.WD_WW_Whs != row.WR_WW_Whs)
						{
							Parent.LocationStringInfo.AddError(Res.GetString("3ccb2f57-b394-4397-a49f-e0623518e4fe",
								"This Location does not belong to the {0}'s Warehouse.", docket.Description));
						}
					}
				}
			}
		}

		#endregion

		#region CheckLocationIsNotVoid

		protected void CheckLocationIsNotVoid(WhsLocation location, string errorMessage)
		{
			if (location != null && location.WLV_LocationStatus == LocationStatus.Codes.Void)
			{
				Parent.LocationStringInfo.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckLocationHasEnoughCapacity

		void CheckLocationHasEnoughCapacity(ZPropertyInfo propInfo)
		{
			var parent = Parent;
			var docket = parent.Docket;
			if (docket != null && !parent.IsDeleted && parent.WE_WL.IsValid && parent.WE_TransactionQuantity > 0 && !propInfo.HasErrors() && !parent.IsFinalised)
			{
				var location = parent.Location;
				if (location != null && location.HasCapacityLimitation)
				{
					var availableCapacityInfo = docket.LocationCapacityValidationManager.GetLocationAvailableCapacity(location);
					var requiredCapacityInfo = docket.LocationCapacityValidationManager.GetLocationRequiredCapacity(location);

					docket.LocationCapacityValidationManager.ValidateForLocationCapacity(propInfo, location, availableCapacityInfo, requiredCapacityInfo);
				}

				var validationCache = docket.GetProductPalletValidationCache();
				if (validationCache != null && location != null && location.WLV_PalletFloorSpaces * location.WLV_PalletStackHeight > 0)
				{
					var requiredPalletCount = validationCache.GetRequiredLocationPalletCount(location);

					if (requiredPalletCount != 0)
					{
						CheckLocationHasEnoughPalletCapacity(propInfo, location, requiredPalletCount, validationCache.GetPalletCountExcludingThisJob(location));
						CheckLocationHasEnoughProductCapacity(propInfo, validationCache.GetLocationUsesMoreThanOneProduct(location));
					}
				}
			}
		}

		#endregion

		#region CheckLocationHasEnoughPalletCapacity

		void CheckLocationHasEnoughPalletCapacity(ZPropertyInfo propInfo, WhsLocation location, ZInt requiredPallets, ZInt locationPalletCountExcludingThisJob)
		{
			var availablePallets = location.WLV_PalletFloorSpaces * location.WLV_PalletStackHeight - locationPalletCountExcludingThisJob;

			if (requiredPallets > availablePallets)
			{
				var errorMessage = Res.GetString("944C7D6C-ECA0-4A50-A8C9-070627288EF7",
					"Total required Pallets ({0}) exceeds the maximum available Pallets ({1}) for this location.",
					requiredPallets.ToString(),
					availablePallets.ToString());
				propInfo.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckLocationHasEnoughProductCapacity

		void CheckLocationHasEnoughProductCapacity(ZPropertyInfo propInfo, ZBool locationUsesMoreThanOneProduct)
		{
			if (locationUsesMoreThanOneProduct)
			{
				var errorMessage = Res.GetString("447A7ACD-933B-4F3D-89FE-31B6CE3DD6F3",
					"Only a single product can be used in locations using Pallet Space capacities.");
				propInfo.AddError(errorMessage);
			}
		}

		#endregion

		#region ValidateLocationString

		public void ValidateLocationString()
		{
			ValidateCalculatedProperty(Parent.LocationStringInfo);
		}

		#endregion

		#endregion

		#region PalletID

		protected override void CheckWE_PalletID()
		{
			base.CheckWE_PalletID();

			WhsValidationHelper.CheckIfTrimIsNeeded(Parent.WE_PalletIDInfo);
		}

		protected override void CheckWE_TransferFromPalletId()
		{
			base.CheckWE_TransferFromPalletId();

			WhsValidationHelper.CheckIfTrimIsNeeded(Parent.WE_TransferFromPalletIdInfo);
		}

		#endregion

		#region CheckWE_GS_NKPutawayBy

		protected override void CheckWE_GS_NKPutawayBy()
		{
			base.CheckWE_GS_NKPutawayBy();

			ListValidation.ErrorIfInvalidCode(Parent.WE_GS_NKPutawayByInfo);
		}

		#endregion

		#region CheckWE_WHC_NKOriginalInventoryHeldCode

		protected override void CheckWE_WHC_NKOriginalInventoryHeldCode()
		{
			base.CheckWE_WHC_NKOriginalInventoryHeldCode();

			CheckHeldCode(Parent.WE_OriginalInventoryStatus, Parent.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			if (!string.IsNullOrEmpty(Parent.WE_WHC_NKOriginalInventoryHeldCode))
			{
				ListValidation.ErrorIfInvalidCode(Parent.WE_WHC_NKOriginalInventoryHeldCodeInfo, Parent.Lookups.InventoryHeldCodeCollection);
			}
		}

		protected virtual void CheckHeldCode(string status, ZPropertyInfo propertyInfo)
		{
			if (status.Equals(InventoryStatus.Codes.Held))
			{
				if (Parent.WE_TransactionQuantity >= 0) // Special case: Allow adjustment outs created from stocktake to not specify a held code
				{
					MandatoryValidation.CheckEntered(propertyInfo);
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(propertyInfo);
			}
		}

		#endregion

		#region CheckWE_WHC_NKCurrentInventoryHeldCode

		protected override void CheckWE_WHC_NKCurrentInventoryHeldCode()
		{
			base.CheckWE_WHC_NKCurrentInventoryHeldCode();

			CheckHeldCode(Parent.WE_CurrentInventoryStatus, Parent.WE_WHC_NKCurrentInventoryHeldCodeInfo);

			if (!string.IsNullOrEmpty(Parent.WE_WHC_NKCurrentInventoryHeldCode))
			{
				ListValidation.ErrorIfInvalidCode(Parent.WE_WHC_NKCurrentInventoryHeldCodeInfo, Parent.Lookups.InventoryHeldCodeCollection);
			}
		}

		#endregion

		#region CheckWE_WHC_NKOrderedHeldCode

		protected override void CheckWE_WHC_NKOrderedHeldCode()
		{
			base.CheckWE_WHC_NKOrderedHeldCode();

			MandatoryValidation.CheckNotEntered(Parent.WE_WHC_NKOrderedHeldCodeInfo);
		}

		#endregion

		#region CheckWE_StockOnHand

		protected override void CheckWE_StockOnHand()
		{
			base.CheckWE_StockOnHand();
			MandatoryValidation.CheckNotNegative(Parent.WE_StockOnHandInfo);
		}

		#endregion

		#region CheckWE_ClientOrderedUnits

		protected override void CheckWE_ClientOrderedUnits()
		{
			base.CheckWE_ClientOrderedUnits();
			MandatoryValidation.CheckNotNegative(Parent.WE_ClientOrderedUnitsInfo);
		}

		#endregion

		#region CheckWE_DockLineType

		protected override void CheckWE_DocketLineType()
		{
			base.CheckWE_DocketLineType();
			CheckDocketLineTypeWithStatus();
		}

		void CheckDocketLineTypeWithStatus()
		{
			if (!IsDocketLineStatusCorrect)
			{
				var errorMessage = Res.GetString("67DA0BC9-9246-486C-A2BD-D1B125CE8417", "An expected error has occurred while saving the docket lines: Docket line type can't be '{0}' with status '{1}'. Please reload the form to proceed your changes.", Parent.WE_DocketLineType, Parent.WE_DocketLineStatus);
				Parent.WE_DocketLineTypeInfo.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckWE_DocketLineStatus

		protected override void CheckWE_DocketLineStatus()
		{
			base.CheckWE_DocketLineStatus();
			CheckDocketLineStatusWithType();
		}

		void CheckDocketLineStatusWithType()
		{
			if (!IsDocketLineStatusCorrect)
			{
				var errorMessage = Res.GetString("D5669830-98EC-4AF1-B37E-0AD7B324270B", "An expected error has occurred while saving the docket lines: Docket line status can't be '{0}' with type '{1}'. Please reload the form to proceed your changes.", Parent.WE_DocketLineStatus, Parent.WE_DocketLineType);
				Parent.WE_DocketLineStatusInfo.AddError(errorMessage);
			}
		}

		bool IsDocketLineStatusCorrect => ValidStatuses.Contains(Parent.WE_DocketLineStatus);

		#endregion

		#region ValidStatuses

		protected virtual IEnumerable<ZString> ValidStatuses => validStatuses.Value;

		readonly Lazy<IEnumerable<ZString>> validStatuses = new Lazy<IEnumerable<ZString>>(() => new ZString[] { string.Empty, DocketLineStatus.Codes.Finalised, DocketLineStatus.Codes.Cancelled });

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsDocketLineSchema.Constants.WE_WL,
				WhsDocketLineSchema.Constants.WE_WL_TransferFrom,
				WhsDocketLineSchema.Constants.WE_WE_MatchingLine,
				WhsDocketLineSchema.Constants.WE_WE_OriginalDocketLineForRating,
				WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine,
				WhsDocketLineSchema.Constants.WE_WPL_PutawayLine,
				WhsDocketLineSchema.Constants.WE_F3_NKPackType,
				WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode,
				WhsDocketLineSchema.Constants.WE_WHC_NKCurrentInventoryHeldCode,
				WhsDocketLineSchema.Constants.WE_P9_Task);

		#endregion

		// Calculated Properties

		#region HeldCodeChangeQuantity

	#region ValidateHeldCodeChangeQuantity

		public void ValidateHeldCodeChangeQuantity()
		{
			ValidateCalculatedProperty(Parent.HeldCodeChangeQuantityInfo);
		}

		#endregion

		#region CheckHeldCodeChangeQuantity

		protected void CheckHeldCodeChangeQuantity()
		{
			if (Parent.IsFinalised)
			{
				if (Parent.HeldCodeChangeQuantity < 0m)
				{
					Parent.HeldCodeChangeQuantityInfo.AddError(Res.GetString("f42c3521-f8f3-47f1-a419-fb8a482f788a", "Quantity cannot be negative"));
				}
				else if (Parent.HeldCodeChangeQuantity > 0 && Parent.HeldCodeChangeQuantity > GetAvailableToTransferQuantity(Parent))
				{
					Parent.HeldCodeChangeQuantityInfo.AddError(Res.GetString("45bd9aa2-b2c3-4613-b0e1-97b27cea7796", "Quantity cannot be greater than Available To Transfer Quantity"));
				}
			}

			CheckHeldCodeChangeQuantityCore();
		}

		ZDecimal GetAvailableToTransferQuantity(WhsDocketLine parent)
		{
			var inventory = (WhsInventoryView)parent.Inventory.FirstOrDefault();
			return inventory != null ? inventory.WI_AvailableToTransferQuantity : 0;
		}

		protected virtual void CheckHeldCodeChangeQuantityCore()
		{
		}

		#endregion

		#endregion

		#region HeldCodeToChangeTo

		#region ValidateHeldCodeToChangeTo

		public void ValidateHeldCodeToChangeTo()
		{
			ValidateCalculatedProperty(Parent.HeldCodeToChangeToInfo);
		}

		#endregion

		#region CheckHeldCodeToChangeTo

		protected void CheckHeldCodeToChangeTo()
		{
			if (Parent.HeldCodeChangeQuantity > 0m)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HeldCodeToChangeToInfo, Parent.Lookups.InventoryHeldCodeCollection);

				var heldCode = Parent.WE_WHC_NKCurrentInventoryHeldCode;
				if (Parent.HeldCodeToChangeTo.EqualsIgnoringCase(heldCode))
				{
					Parent.HeldCodeToChangeToInfo.AddError(Res.GetString("c3458075-816a-4c27-997d-ac6823273e7d", "The current Hold Code of this record is '{0}'. This field is used to change the current Hold Code, so it is not valid to select the current Hold Code.", heldCode));
				}
			}

			CheckHoldCodeToChangeToCore();
		}

		protected virtual void CheckHoldCodeToChangeToCore()
		{
		}

		#endregion

		#endregion

		#region ValidateHoldReasonToChangeTo

		public void ValidateHoldReasonToChangeTo()
		{
			ValidateCalculatedProperty(Parent.HoldReasonToChangeToInfo);
		}

		#endregion

		#region ValidateWE_PackQuantity

		public void ValidateWE_PackQuantity()
		{
			ValidateCalculatedProperty(Parent.WE_PackQuantityInfo);
		}

		protected virtual void CheckWE_PackQuantity()
		{
			TypeValidation.CheckValidDecimal(Parent.WE_PackQuantityInfo, 18, 3);
		}

		#endregion

		#region Messages

		public static ZString ThisFieldIsRequiredToAutoCreateAProductWarning => Res.GetString("5de84f12-39eb-477b-b562-69f08b12ce74", "This field is required to auto-create a Product");

		public static ZString ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode => Res.GetString("f79bbb6e-a04a-4695-bc4a-16fe89c1d8eb", "This field should be the same for all auto-created products with same Product Code.");

		public static string ErrorForHoldReasonWithoutHoldCode => Res.GetString("4f1f9a83-143c-480a-bb46-6279c4fecff9", "Hold Reason cannot be entered if the stock is not on Hold. Enter in a Hold Code or remove the Reason.");

		#endregion

		#region Implementation

		protected new WhsDocketLine Parent
		{
			get { return (WhsDocketLine)base.Parent; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLocationString();
			ValidateHeldCodeChangeQuantity();
			ValidateHeldCodeToChangeTo();
			ValidateHoldReasonToChangeTo();
			ValidateProductDesc();
			ValidateProductUQ();
			ValidateCommodityCode();
		}

		protected virtual PartAttributeValidation PartAttributeValidation
		{
			get { return new PartAttributeValidation(Parent); }
		}

		#endregion
	}
}
