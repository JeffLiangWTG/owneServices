using System.Collections.Immutable;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryViewValidation : AutoWhsInventoryViewValidation
	{
		public WhsInventoryViewValidation(AutoWhsInventoryView parent)
			: base(parent)
		{
		}

		#region  Related Entities

		protected new WhsInventoryView Parent
		{
			get { return (WhsInventoryView)base.Parent; }
		}

		#endregion

		#region Validation

		#region CheckWI_SplitQuantity

		protected virtual void CheckWI_SplitQuantity()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				WhsValidationHelper.CheckSplitQuantity(Parent.WI_SplitQuantityInfo, Parent.WI_InDocketLineUnits);
			}
		}

		#endregion

		#region CheckWI_InDocketLineUnits

		protected override void CheckWI_InDocketLineUnits()
		{
			base.CheckWI_InDocketLineUnits();

			if (WhsEnvironment.IsWebTracker)
			{
				WhsValidationHelper.CheckInDocketLineUnits(Parent.WI_InDocketLineUnitsInfo);

				var receive = Parent.Docket as WhsReceive;
				if (receive != null)
				{
					WhsValidationHelper.CheckCrossDockedUnits(Parent.WI_InDocketLineUnitsInfo, Parent.WI_ExpectedReceiptQuantity, Parent.WI_CrossDockQuantity, Parent.InDocketLine != null && Parent.InDocketLine.IsInDatabase);
					if (!WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
					{
						WhsValidationHelper.CheckQtyForSerialNumber(PartAttributeValidation, Parent.WI_InDocketLineUnitsInfo, Parent.WI_SerialNumberInfo, receive, Parent.Product);
					}
				}
			}
		}

		#endregion

		#region CheckPartAttributes

		#region CheckWI_ExpiryDate

		protected override void CheckWI_ExpiryDate()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var receive = Parent.Docket as WhsReceive;
				if (receive != null)
				{
					if (receive.IsFinalising && Parent.WI_TotalUnits > 0m)
					{
						PartAttributeValidation.CheckExpiryDate(Parent.Client, Parent.SupplierPart, Parent.WI_ExpiryDateInfo);
					}

					if (!receive.IsFinalised && !receive.IsFinalising)
					{
						PartAttributeValidation.ValidateExpiryDateAgainstExpiryNotificationPeriod(Parent.Client, Parent.Warehouse, Parent.Product, Parent.WI_ExpiryDate, Parent.WI_ExpiryDateInfo);
					}
				}
			}
		}

		protected override void CheckWI_ExpiryDateIsValidZDate()
		{
			TypeValidation.CheckValidZDateWithoutRange(Parent.WI_ExpiryDateInfo);
		}

		protected override void CheckWI_ExpiryDateIsValidZDateRange()
		{
			var limits = new TypeValidationLimits { FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears };
			if (!ShouldCheckForOldDates)
			{
				limits.PastYearsBeforeError = DateRangeValidation.MaximumPastYears;
			}
			TypeValidation.CheckValidZDateRange(Parent.WI_ExpiryDateInfo, limits);
		}

		#endregion

		#region CheckWI_PackingDate

		protected override void CheckWI_PackingDate()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var receive = Parent.Docket as WhsReceive;
				if (receive != null && receive.IsFinalising && Parent.WI_TotalUnits > 0m)
				{
					PartAttributeValidation.CheckPackingDate(Parent.Client, Parent.SupplierPart, Parent.WI_PackingDateInfo);
				}
			}
		}

		protected override void CheckWI_PackingDateIsValidZDate()
		{
			TypeValidation.CheckValidZDateWithoutRange(Parent.WI_PackingDateInfo);
		}

		protected override void CheckWI_PackingDateIsValidZDateRange()
		{
			if (ShouldCheckForOldDates)
			{
				base.CheckWI_PackingDateIsValidZDateRange();
			}
			else
			{
				TypeValidation.CheckValidZDateRange(Parent.WI_PackingDateInfo, new TypeValidationLimits { PastYearsBeforeError = DateRangeValidation.MaximumPastYears });
			}
		}

		#endregion

		#region CheckWI_PartAttrib1

		protected override void CheckWI_PartAttrib1()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				AddApplicationDefinedAttributeWarnings(Parent.WI_PartAttrib1Info, 1);
				CheckPartAttribute(Parent.WI_PartAttrib1Info, 1);
			}
		}

		#endregion

		#region CheckWI_PartAttrib2

		protected override void CheckWI_PartAttrib2()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				AddApplicationDefinedAttributeWarnings(Parent.WI_PartAttrib2Info, 2);
				CheckPartAttribute(Parent.WI_PartAttrib2Info, 2);
			}
		}

		#endregion

		#region CheckWI_PartAttrib3

		protected override void CheckWI_PartAttrib3()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				AddApplicationDefinedAttributeWarnings(Parent.WI_PartAttrib3Info, 3);
				CheckPartAttribute(Parent.WI_PartAttrib3Info, 3);
			}
		}

		#endregion

		#region CheckWI_SerialNumber

		protected override void CheckWI_SerialNumber()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				if (!Parent.ValidationSerialNumberWarningMessage.IsEmpty)
				{
					Parent.WI_SerialNumberInfo.AddWarning(Parent.ValidationSerialNumberWarningMessage);
				}

				CheckSerialNumber(Parent.WI_SerialNumberInfo);
			}
		}

		#endregion

		#region AddApplicationDefinedAttributeWarnings

		void AddApplicationDefinedAttributeWarnings(ZPropertyInfo info, int attributeNumber)
		{
			if (!Parent.ValidationPartAttribWarningMessage[attributeNumber].IsEmpty)
			{
				info.AddWarning(Parent.ValidationPartAttribWarningMessage[attributeNumber]);
			}
		}

		#endregion

		#region CheckPartAttribute

		void CheckPartAttribute(ZPropertyInfo info, int attributeNumber)
		{
			var client = Parent.Client;
			var product = Parent.Product;
			var receive = Parent.Docket as WhsReceive;
			if (receive != null && product != null && !product.IsPartAttribReleaseCaptured(client, attributeNumber))
			{
				if (receive.IsFinalising && Parent.WI_TotalUnits > 0m)
				{
					PartAttributeValidation.CheckAttribute(client, product.Parent, info, attributeNumber);
					PartAttributeValidation.CheckJulianBatchNumberAttributeFormat(client, product, info, attributeNumber);
				}
			}

			WhsValidationHelper.CheckIfTrimIsNeeded(info);
			PartAttributeValidation.ValidatePartAttribIsNotReleaseCapturedWithValue(product, client, info, attributeNumber);
		}

		bool ShouldCheckSerialNumberIsUnique(bool receiveStartedReceiving)
		{
			return Parent.WI_InDocketLineUnits > 0
				|| (!receiveStartedReceiving && Parent.WI_ExpectedReceiptQuantity > 0);
		}

		#endregion

		#region CheckSerialNumber

		void CheckSerialNumber(ZPropertyInfo info)
		{
			var client = Parent.Client;
			var product = Parent.Product;
			var receive = Parent.Docket as WhsReceive;
			if (receive != null && product != null && !product.IsSerialNumberReleaseCaptured(client))
			{
				if (receive.IsFinalising && Parent.WI_TotalUnits > 0m)
				{
					if (!product.IsSerialNumberUsed(client))
					{
						PartAttributeValidation.CheckSerialNumber(client, product.Parent, info);
					}
					else
					{
						MandatoryValidation.CheckEntered(info);
						CheckSerialNumberIsUnique(receive, client, info, checkInDB: true);
					}
				}
				else if (!receive.IsFinalised)
				{
					CheckSerialNumberIsUnique(receive, client, info, checkInDB: false);
				}
			}

			WhsValidationHelper.CheckIfTrimIsNeeded(info);
			PartAttributeValidation.ValidateSerialIsNotReleaseCapturedWithValue(product, client, info);
		}

		void CheckSerialNumberIsUnique(WhsReceive receive, OrgHeader client, ZPropertyInfo info, bool checkInDB)
		{
			if (ShouldCheckSerialNumberIsUnique(receive.StartedReceiving))
			{
				var isSerialNumberUnique = receive.ReceiveValidationStrategy.CheckSerialNumberIsUnique(client, Parent, checkInDB);
				if (!isSerialNumberUnique)
				{
					info.AddError(Res.GetString("99dfd59b-5022-42bb-a0be-2b465c9e172e", "Serial # already used."));
				}
			}
		}

		#endregion

		#region ShouldCheckForOldDates

		bool ShouldCheckForOldDates
		{
			get
			{
				var result = true;

				var docketLineType = Parent.WI_InDocketLineType;
				if (docketLineType == DocketType.Codes.Transfer || docketLineType == DocketType.Codes.Adjustment)
				{
					result = false;
				}
				else if (docketLineType == DocketType.Codes.Receive)
				{
					result = !((WhsReceiveLine)Parent.InDocketLine)?.ShouldIgnoreOutOfRangeDates ?? true;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region CheckWI_F3_NKPackType

		protected override void CheckWI_F3_NKPackType()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				MandatoryValidation.CheckEntered(Parent.WI_F3_NKPackTypeInfo);
				if (!Parent.WI_F3_NKPackTypeInfo.HasErrors() && Parent.SupplierPart != null && !Parent.Product.IsPackTypeUsedByProduct(Parent.WI_F3_NKPackType))
				{
					Parent.WI_F3_NKPackTypeInfo.AddWarning(Res.GetString("fd54c4e5-f474-4f28-87c1-981bf704c8bf", "The Pack Type {0} has not been defined for this Product.\r\nPack Types are defined on the Maintain -> Warehouse -> Products -> Details -> Unit Conversions Tab.\r\nBecause this Pack Type is not defined, a conversion to Units is not possible.", Parent.WI_F3_NKPackType));
				}
			}
		}

		#endregion

		#region CheckWI_OP

		protected override void CheckWI_OP()
		{
			base.CheckWI_OP();

			if (WhsEnvironment.IsWebTracker)
			{
				var part = Parent.SupplierPart;
				WhsValidationHelper.CheckProductIsValid(part, Parent.WI_OH_Client, Parent.WI_OPInfo);
				WhsValidationHelper.CheckForProductWarningMessage(Parent, Parent.WI_OPInfo);
				WhsValidationHelper.CheckForTempProduct(Parent, Parent.WI_OPInfo);

				if (part != null)
				{
					PartAttributeValidation.ValidateCanCalculateExpiryDateIfJulianBatchNumberIsUsed(Parent.Client, Parent.Warehouse, Parent.Product, Parent.WI_OPInfo);
					if (Parent.InDocketLine != null && (ZGuid)Parent.InDocketLine.WE_OPInfo.OriginalValue != Parent.WI_OP)
					{
						WhsValidationHelper.CheckProductShouldNotBeChangedIfInventoryIsReserved(Parent.InDocketLine.IsInDatabase, Parent.WI_OPInfo, Parent);
					}
					WhsValidationHelper.CheckProductHasWeightDefinition(part, Parent.WI_OPInfo);
					WhsValidationHelper.CheckProductHasCubicDefinition(part, Parent.WI_OPInfo);
					WhsValidationHelper.CheckProductHasPalletDefinition(part, Parent.WI_OPInfo);
				}
			}
		}

		#region CheckWI_OPIsValidZGuid

		protected override void CheckWI_OPIsValidZGuid()
		{
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return !FKsToNotValidateForCancelledRecords.Contains(info.Name)
				&& (!(info.Name == WhsInventoryViewSchema.Constants.WI_OP
				&& Parent.Docket is WhsReceive receive
				&& Parent.InDocketLine is WhsReceiveLine receiveLine
				&& receive.IsFinalising
				&& receiveLine.WE_TransactionQuantity == 0))
				&& base.ShouldValidateFKToCancelledRecord(info);
		}

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(
				WhsInventoryViewSchema.Constants.WI_WL,
				WhsInventoryViewSchema.Constants.WI_WE_InDocketLine,
				WhsInventoryViewSchema.Constants.WI_WE_OriginalInDocketLineForRating,
				WhsInventoryViewSchema.Constants.WI_F3_NKPackType);

		#endregion

		#endregion

		#region CheckWI_ArrivalDateIsValidZDateTimeRange

		protected override void CheckWI_ArrivalDateIsValidZDateTimeOffsetRange()
		{
			// Arrival date validation is already handled by whsdocketline
		}

		#endregion

		// calculated

		#region ValidateWI_SplitQuantity

		public void ValidateWI_SplitQuantity()
		{
			ValidateCalculatedProperty(Parent.WI_SplitQuantityInfo);
		}

		#endregion

		#region PartAttributeValidation

		protected virtual PartAttributeValidation PartAttributeValidation
		{
			get { return new PartAttributeValidation(Parent); }
		}

		#endregion

		#region ValidateWI_ExpectedReceiptQuantity

		public void ValidateWI_ExpectedReceiptQuantity()
		{
			ValidateCalculatedProperty(Parent.WI_ExpectedReceiptQuantityInfo);
		}

		protected void CheckWI_ExpectedReceiptQuantity()
		{
			if (WhsEnvironment.IsWebTracker)
			{
				var receive = Parent.Docket as WhsReceive;
				if (receive != null)
				{
					WhsValidationHelper.CheckCrossDockedUnits(Parent.WI_ExpectedReceiptQuantityInfo, Parent.WI_InDocketLineUnits, Parent.WI_CrossDockQuantity, Parent.InDocketLine != null && Parent.InDocketLine.IsInDatabase);
					WhsValidationHelper.CheckQtyForSerialNumber(PartAttributeValidation, Parent.WI_ExpectedReceiptQuantityInfo, Parent.WI_SerialNumberInfo, receive, Parent.Product);
				}
			}
		}

		#endregion

		// Unneccessary validation from base

		#region Unneccessary validation from base

		// Since WhsInventoryView is a view, these non nullable fields do not have defaults so the generator unneccessarily adds validation
		// These are handled by validation or defaults in the data layer on WhsDocketLine
		protected override void CheckWI_BondedEntryKeyIsNotEmpty() { }
		protected override void CheckWI_F3_NKPackTypeIsNotEmpty() { }
		protected override void CheckWI_InDocketLineTypeIsNotEmpty() { }
		protected override void CheckWI_InDocketLineUnitsIsNotEmpty() { }
		protected override void CheckWI_InventoryStatusIsNotEmpty() { }
		protected override void CheckWI_IsOriginalReceiptLineIsNotEmpty() { }
		protected override void CheckWI_PalletIDIsNotEmpty() { }
		protected override void CheckWI_PartAttrib1IsNotEmpty() { }
		protected override void CheckWI_PartAttrib2IsNotEmpty() { }
		protected override void CheckWI_PartAttrib3IsNotEmpty() { }
		protected override void CheckWI_SerialNumberIsNotEmpty() { }
		protected override void CheckWI_AllocationKeyIsNotEmpty() { }
		protected override void CheckWI_TotalUnitsIsNotEmpty() { }
		protected override void CheckWI_HeldCodeIsNotEmpty() { }

		#endregion

		#endregion
	}
}
