using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsProductWhsUNDGLimitValidationHelper : IWhsUNDGLimitValidationHelper
	{
		#region GetWhsWarehouseUNDGTotals

		public WhsUNDGLimitValidationInfo GetUNDGLimitValidationInfo(IWhsUNDGLimit undgLimit)
		{
			var undgLimitBO = (WhsUNDGLimit)undgLimit;
			Argument.NotNull(undgLimitBO, nameof(undgLimitBO));

			var limitType = UNDGLimitType.DG;
			var code = string.Empty;
			var totalWeight = 0m;
			var totalVolume = 0m;

				if (!undgLimitBO.WWD_DG.IsEmpty)
				{
					code = undgLimitBO.UNDGSubstance.DG_Code;
					(totalWeight, totalVolume) = GetWhsWarehouseUNDGTotalsBySubstance(undgLimitBO.Factory, undgLimitBO.WWD_WW_Warehouse, undgLimitBO.WWD_DG, undgLimitBO.WWD_TotalWeightLimitUQ, undgLimitBO.WWD_TotalVolumeLimitUQ);
				}
				else if (!undgLimitBO.WWD_DCR_UNDGCountryReference.IsEmpty)
				{
					limitType = UNDGLimitType.CountryReference;
					code = undgLimitBO.UNDGCountryReference.DCR_Code;
					(totalWeight, totalVolume) = GetWhsWarehouseUNDGTotalsByCountryReference(undgLimitBO.Factory, undgLimitBO.WWD_WW_Warehouse, undgLimitBO.WWD_DCR_UNDGCountryReference, undgLimitBO.WWD_TotalWeightLimitUQ, undgLimitBO.WWD_TotalVolumeLimitUQ);
				}
				else if (!undgLimitBO.WWD_UNDGClass.IsEmpty)
				{
					limitType = UNDGLimitType.UNDGClass;
					code = undgLimitBO.WWD_UNDGClass;
					(totalWeight, totalVolume) = GetWhsWarehouseUNDGTotalsByClass(undgLimitBO.Factory, undgLimitBO.WWD_WW_Warehouse, undgLimitBO.WWD_UNDGClass, undgLimitBO.WWD_TotalWeightLimitUQ, undgLimitBO.WWD_TotalVolumeLimitUQ);
				}

			return new WhsUNDGLimitValidationInfo(limitType, code, totalWeight, totalVolume);
		}

		(decimal TotalWeight, decimal TotalVolume) GetWhsWarehouseUNDGTotalsBySubstance(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid undgSubstance, ZString warehouseWeightLimitUQ, ZString warehouseVolumeLimitUQ)
			=> CalculateUNDGTotalWeightAndVolume(WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(factory, warehousePK, undgSubstance), warehouseWeightLimitUQ, warehouseVolumeLimitUQ);

		(decimal TotalWeight, decimal TotalVolume) GetWhsWarehouseUNDGTotalsByCountryReference(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid undgCountryReference, ZString warehouseWeightLimitUQ, ZString warehouseVolumeLimitUQ)
			=> CalculateUNDGTotalWeightAndVolume(WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsByCountryReference(factory, warehousePK, undgCountryReference), warehouseWeightLimitUQ, warehouseVolumeLimitUQ);

		(decimal TotalWeight, decimal TotalVolume) GetWhsWarehouseUNDGTotalsByClass(BusinessObjectFactory factory, ZGuid warehousePK, ZString undgClass, ZString warehouseWeightLimitUQ, ZString warehouseVolumeLimitUQ)
			=> CalculateUNDGTotalWeightAndVolume(WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsByClass(factory, warehousePK, undgClass), warehouseWeightLimitUQ, warehouseVolumeLimitUQ);

		(decimal TotalWeight, decimal TotalVolume) CalculateUNDGTotalWeightAndVolume(IReadOnlyCollection<WhsWarehouseUNDGWeightAndVolume> undgWeightAndVolumes, ZString warehouseWeightLimitUQ, ZString warehouseVolumeLimitUQ)
		{
			var weights = new Dictionary<string, decimal>();
			var volumes = new Dictionary<string, decimal>();

			foreach (var undg in undgWeightAndVolumes)
			{
				if (weights.TryGetValue(undg.TotalWeightUQ, out var weight))
				{
					weights[undg.TotalWeightUQ] = weight + undg.TotalWeight;
				}
				else
				{
					weights[undg.TotalWeightUQ] = undg.TotalWeight;
				}

				if (volumes.TryGetValue(undg.TotalVolumeUQ, out var volume))
				{
					volumes[undg.TotalVolumeUQ] = volume + undg.TotalVolume;
				}
				else
				{
					volumes[undg.TotalVolumeUQ] = undg.TotalVolume;
				}
			}

			var totalWeight = weights.Sum(w => Constants.Weight.Convert(w.Value, w.Key, warehouseWeightLimitUQ));
			var totalVolume = volumes.Sum(v => Constants.Volume.Convert(v.Value, v.Key, warehouseVolumeLimitUQ));

			return (totalWeight, totalVolume);
		}

		#endregion

		#region UNDGStorageUnit

		public ZString UNDGStorageUnit => Res.GetString("9d9e6f5e-6c71-461a-a5c9-cc48814dab20", "products");

		#endregion
	}
}
