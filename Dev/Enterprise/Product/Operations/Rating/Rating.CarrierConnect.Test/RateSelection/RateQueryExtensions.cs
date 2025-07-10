using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;

namespace Enterprise.Rating.CarrierConnect.Test;

public static class RateQueryExtensions
{
	public static RateQueryDto AddContainer(
		this RateQueryDto query,
		string containerType,
		string commodityCode = "GEN",
		int count = 1,
		decimal weight = 0m,
		decimal volume = 0m,
		decimal chargeableOverride = 0m,
		string chargeableUnit = "")
	{
		query.JobInfo.Containers = query.JobInfo.Containers.Append(new ()
		{
			Commodity = commodityCode,
			Count = count,
			Number = null,
			ContainerType = containerType,
			PackLines =
			[
				new JobPackLineDto
				{
					Commodity = "GEN",
					Count = 1,
					Volume = volume,
					Weight = weight,
					VolumeUnit = "M3",
					WeightUnit = "KG",
					ChargeableOverride = chargeableOverride,
					ChargeableUnit = chargeableUnit,
				}
			]
		});

		return query;
	}

	public static RateQueryDto AddNonContainerizedCargo(this RateQueryDto query, string commodityCode = "GEN", decimal weight = 0m, decimal volume = 0m)
		=> query.AddContainer("LCL", commodityCode, 1, weight, volume);

	internal static List<RateChargeDtoAssertion> AddCharge(this List<RateChargeDtoAssertion> collection, string chargeCode, decimal localAmount, string containerType, string commodityCode)
	{
		collection.Add(new RateChargeDtoAssertion
		{
			ChargeCode = new() { ChargeCode = chargeCode },
			LocalAmount = localAmount,
			ContainerType = containerType,
			Commodity = new()
			{
				RefCommodityCode = commodityCode,
			}
		});
		return collection;
	}
}
