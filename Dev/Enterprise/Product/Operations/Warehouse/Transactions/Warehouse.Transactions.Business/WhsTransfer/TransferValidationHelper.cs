using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	// Tested in WhsTransferLineValidation & WhsVASOrder
	static class TransferValidationHelper
	{
		#region GetMessageForLocationCapacity

		public static MessageWithSeverity GetMessageForLocationCapacity(IEnumerable<WhsTransferLine> productsToTransferIn, LocationWithCapacity destLocationInfo, bool processLinesWithEmptyDestLocation)
		{
			EnsureTransferLinesAreValid(productsToTransferIn);
			var result = new MessageWithSeverity(MessageWithSeverity.MessageTypes.None, "");
			ZString message = "";
			var destLocation = destLocationInfo?.Location;

			if (destLocation != null)
			{
				bool isWeightDefined = destLocation.WLV_MaxWeight != 0 && destLocationInfo.Weight.HasValue;
				bool isVolumeDefined = destLocation.WLV_MaxCubic != 0 && destLocationInfo.Volume.HasValue;
				bool isQuantityDefined = destLocation.WLV_MaxQuantity != 0 && destLocationInfo.Quantity.HasValue;

				if (isWeightDefined || isVolumeDefined || isQuantityDefined)
				{
					// remove if it move to same location
					if (productsToTransferIn.Any())
					{
						ZDecimal totalTransferredAmount = 0m;
						if (isQuantityDefined && CheckLocationForMaxQuantity(productsToTransferIn, destLocationInfo, processLinesWithEmptyDestLocation, out totalTransferredAmount))
						{
							if (WhsEnvironment.IsRF)
							{
								message = Res.GetString("11CBF9C3-B50F-4FA1-B960-FA3CED4F9F43", "The Maximum Qty({0}) for the destination location will be exceeded. Please select another location.", destLocation.WLV_MaxQuantity.ToZInt());
							}
							else
							{
								message = Res.GetString("eaa7be3d-81cc-40cb-bf65-17ba5158e05c",
									"Total transferring Quantity ({0}) exceeds the maximum available Quantity ({1}) for this location.",
									totalTransferredAmount,
									destLocationInfo.Quantity.Value.ToString(3));
							}
							result = new MessageWithSeverity(MessageWithSeverity.MessageTypes.Error, message);
						}
						else if (isWeightDefined && CheckLocationForMaxWeight(productsToTransferIn, destLocationInfo, processLinesWithEmptyDestLocation, out totalTransferredAmount))
						{
							message = Res.GetString("29f2725e-53ed-4afa-8926-cdb6ec1f0339",
								"Total transferring Weight ({0} {2}) exceeds the maximum available Weight ({1} {2}) for this location.",
								totalTransferredAmount.ToString(2), // 2 Decimal Places
								destLocationInfo.Weight.Value.ToString(2), // 2 Decimal Places
								destLocation.WLV_MaxWeightUnit);
							result = new MessageWithSeverity(MessageWithSeverity.MessageTypes.Warning, message);
						}
						else if (isVolumeDefined && CheckLocationForMaxVolume(productsToTransferIn, destLocationInfo, processLinesWithEmptyDestLocation, out totalTransferredAmount))
						{
							message = Res.GetString("f3004c9d-9f97-4b21-a80e-c185fbb06e48",
								"Total transferring Volume ({0} {2}) exceeds the maximum available Volume ({1} {2}) for this location.",
								totalTransferredAmount.ToString(3), // 3 Decimal Places
								destLocationInfo.Volume.Value.ToString(3), // 3 Decimal Places
								destLocation.WLV_MaxCubicUnit);
							result = new MessageWithSeverity(MessageWithSeverity.MessageTypes.Warning, message);
						}
					}
				}
			}

			return result;
		}

		static void EnsureTransferLinesAreValid(IEnumerable<WhsTransferLine> productsToTransferIn)
		{
			var firstTransferLine = productsToTransferIn.FirstOrDefault();
			if (firstTransferLine != null && productsToTransferIn.Any(l => l.WE_WD != firstTransferLine.WE_WD))
			{
				throw new ArgumentException("Transfer Lines passed into GetWarningForLocationCapacity() must all be from the same transfer.");
			}
		}

		static bool CheckLocationForMaxWeight(IEnumerable<WhsTransferLine> productsToTransferIn, LocationWithCapacity destLocation, bool processLinesWithEmptyDestLocation, out ZDecimal totalStockStorage)
		{
			return CheckLocationForMaxStorage(productsToTransferIn, processLinesWithEmptyDestLocation, destLocation.Location.PK, destLocation.Weight.Value, destLocation.Location.WLV_MaxWeightUnit, OrgSupplierPartSchema.OP_Weight, OrgSupplierPartSchema.OP_WeightUQ, out totalStockStorage);
		}

		static bool CheckLocationForMaxVolume(IEnumerable<WhsTransferLine> productsToTransferIn, LocationWithCapacity destLocation, bool processLinesWithEmptyDestLocation, out ZDecimal totalStockStorage)
		{
			return CheckLocationForMaxStorage(productsToTransferIn, processLinesWithEmptyDestLocation, destLocation.Location.PK, destLocation.Volume.Value, destLocation.Location.WLV_MaxCubicUnit, OrgSupplierPartSchema.OP_Cubic, OrgSupplierPartSchema.OP_CubicUQ, out totalStockStorage);
		}

		static bool CheckLocationForMaxStorage(IEnumerable<WhsTransferLine> productsToTransferIn, bool processLinesWithEmptyDestLocation, ZGuid destLocationPK, ZDecimal destLocationAvailableCapacity, ZString destLocationMaxUnit, SchemaDecimalColumn productWgtOrVolColumn, SchemaStringColumn productWgtOrVolUnitColumn, out ZDecimal totalStockStorage)
		{
			bool showWarning = false;
			totalStockStorage = 0m;

			if (destLocationAvailableCapacity >= 0m)
			{
				totalStockStorage += GetTotalTransactionsStorage(productsToTransferIn, processLinesWithEmptyDestLocation, destLocationPK, destLocationMaxUnit, productWgtOrVolColumn, productWgtOrVolUnitColumn);
				if (destLocationAvailableCapacity < totalStockStorage)
				{
					showWarning = true;
				}
			}

			return showWarning;
		}

		static bool CheckLocationForMaxQuantity(IEnumerable<WhsTransferLine> productsToTransferIn, LocationWithCapacity destLocation, bool processLinesWithEmptyDestLocation, out ZDecimal totalStockQuantity)
		{
			bool showWarning = false;
			totalStockQuantity = 0m;

			if (destLocation.Quantity.HasValue)
			{
				foreach (var transferLine in productsToTransferIn)
				{
					totalStockQuantity += GetTransferLineAdditionalQuantity(transferLine, processLinesWithEmptyDestLocation, destLocation.Location.PK);
				}

				if (destLocation.Quantity < totalStockQuantity)
				{
					showWarning = true;
				}
			}
			return showWarning;
		}

		#region GetTotalTransactionsStorage

		static ZDecimal GetTotalTransactionsStorage(IEnumerable<WhsTransferLine> productsToTransferIn, bool processLinesWithEmptyDestLocation, ZGuid destLocationPK, ZString destLocationMaxUnit, SchemaDecimalColumn productWgtOrVolColumn, SchemaStringColumn productWgtOrVolUnitColumn)
		{
			ZDecimal totalTransactionsStorage = 0m;

			foreach (var transferLine in productsToTransferIn)
			{
				var product = transferLine.SupplierPart;
				if (product != null)
				{
					var productWgtOrVol = (ZDecimal)product[productWgtOrVolColumn];
					if (productWgtOrVol > 0m)
					{
						var additionalQuantity = GetTransferLineAdditionalQuantity(transferLine, processLinesWithEmptyDestLocation, destLocationPK);
						if (additionalQuantity != 0m)
						{
							var productWgtOrVolUnit = (ZString)product[productWgtOrVolUnitColumn];
							var conversionFactorToMatchLocationUnit = product.UnitConverter.ConversionFactor(productWgtOrVolUnit, destLocationMaxUnit);
							totalTransactionsStorage += additionalQuantity * productWgtOrVol * conversionFactorToMatchLocationUnit;
						}
					}
				}
			}

			return totalTransactionsStorage;
		}

		static ZDecimal GetTransferLineAdditionalQuantity(WhsTransferLine transferLine, bool processLinesWithEmptyDestLocation, ZGuid destLocationPK)
		{
			var sourceLocation = transferLine.TransferFromLocation;
			var destLocation = transferLine.Location;

			ZDecimal result = 0m;

			// if transferring to the same location, minus the amount.
			if (sourceLocation != null && sourceLocation.PK == destLocationPK)
			{
				result = -transferLine.QtyToMoveIncludingMatchingLines;
			}
			// otherwise if we are transferring to the destination location we are checking, add the amount.
			else if (destLocation != null)
			{
				result = destLocation.PK == destLocationPK ? transferLine.QtyToMoveIncludingMatchingLines : ZDecimal.Zero;
			}
			// otherwise for VAS Order transfers where the destination location is not yet set, add the amount.
			else if (processLinesWithEmptyDestLocation)
			{
				result = transferLine.QtyToMoveIncludingMatchingLines;
			}

			return result;
		}

		#endregion		

		#endregion
	}
}
