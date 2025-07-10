using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public static class IWhsUNDGLimitValidationHelperExtensions
	{
		#region GetValidationMessage

		public static ZString GetWhsUNDGLimitValidationMessage(this IWhsUNDGLimitValidationHelper helper, IWhsUNDGLimit undgLimit)
		{
			var undgLimitBO = (WhsUNDGLimit)undgLimit;
			Argument.NotNull(undgLimitBO, nameof(undgLimitBO));

			var warehouse = undgLimitBO.Warehouse;
			var errorMessageBuilder = new ZStringBuilder();
			if (!undgLimitBO.HasErrors && warehouse.WW_IsDangerousGoodsManagementEnabled)
			{
				var info = helper.GetUNDGLimitValidationInfo(undgLimit);
				if (info != null && (info.TotalWeight > 0 || info.TotalVolume > 0))
				{
					var dgThresholdPercentage = warehouse.WW_DGThresholdPercentage;
					if (undgLimitBO.IsDGAllowed)
					{
						var weightResult = IsWeightOverLimit(info.TotalWeight, undgLimitBO.WWD_TotalWeightLimit, dgThresholdPercentage);
						var volumeResult = IsVolumeOverLimit(info.TotalVolume, undgLimitBO.WWD_TotalVolumeLimit, dgThresholdPercentage);

						if (weightResult.IsOverLimit)
						{
							errorMessageBuilder.Append(Res.GetString("9597124E-4DA5-4156-ACBC-82484185A05E", "{0} '{1}' is at {2}% weight capacity.",
								info.Title,
								info.Code,
								weightResult.Percentage));
						}

						if (volumeResult.IsOverLimit)
						{
							errorMessageBuilder.Append(Res.GetString("3BD6AB6C-122E-45E9-AA60-1799B2F4FF50", "{0} '{1}' is at {2}% volume capacity.",
								info.Title,
								info.Code,
								volumeResult.Percentage));
						}
					}
					else
					{
						errorMessageBuilder.Append(Res.GetString("8EC3E717-0E6A-4FAC-8586-D4F9BCBB8BF3", "This Warehouse cannot store {0} '{1}', but there are already {0} '{1}' {2} in warehouse.",
							info.Title,
							info.Code,
							helper.UNDGStorageUnit));
					}
				}
			}

			return errorMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region CheckOverLimit

		static (bool IsOverLimit, decimal Percentage) IsWeightOverLimit(ZDecimal totalWeight, ZDecimal totalWeightLimit, ZByte dgThresholdPercentage)
		{
			var totalWeightPercentage = totalWeightLimit > 0 ? Utilities.Round(totalWeight / totalWeightLimit * 100, 0) : 0;
			var isThresholdReached = totalWeightPercentage > 0 && dgThresholdPercentage > 0 && totalWeightPercentage > dgThresholdPercentage;
			var isOverLimit = totalWeightLimit > 0 && (totalWeight > totalWeightLimit || isThresholdReached);

			return (isOverLimit, totalWeightPercentage);
		}

		static (bool IsOverLimit, decimal Percentage) IsVolumeOverLimit(ZDecimal totalVolume, ZDecimal totalVolumeLimit, ZByte dgThresholdPercentage)
		{
			var totalVolumePercentage = totalVolumeLimit > 0 ? Utilities.Round(totalVolume / totalVolumeLimit * 100, 0) : 0;
			var isThresholdReached = totalVolumePercentage > 0 && dgThresholdPercentage > 0 && totalVolumePercentage > dgThresholdPercentage;
			var isOverLimit = totalVolumeLimit > 0 && (totalVolume > totalVolumeLimit || isThresholdReached);

			return (isOverLimit, totalVolumePercentage);
		}

		#endregion
	}
}
