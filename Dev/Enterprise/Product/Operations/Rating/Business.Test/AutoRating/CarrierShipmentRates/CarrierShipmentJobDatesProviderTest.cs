using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test.AutoRating.CarrierShipmentRates;

public class CarrierShipmentJobDatesProviderTest
{
	[Test]
	public void GetAutoratingDateOverrideCore_ReturnsReadyDateFromParent()
	{
		var date = new DateTime(2025, 01, 09);
		var parent = new CarrierShipmentRateQueryBusinessObject(
			new CarrierShipmentRateQueryDto()
			{
				Shipment = new CarrierShipmentRateShipmentDto()
				{
					ReadyDate = date
				}
			},
			null,
			null);

		var jobDatesProvider = new CarrierShipmentJobDatesProvider(parent);
		var result = jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride);

		Assert.That(result.ToDateTime(), Is.EqualTo(date));
	}
}
