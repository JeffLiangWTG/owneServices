using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusConfigurationVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefCusConfiguration
				{
					ZZJ_PK = Guid.NewGuid(),
					ZZJ_IsGenericCountry = true,
					ZZJ_AllowRiskManagement = true,
					ZZJ_EndDate = DateTime.Now.AddDays(1),
					ZZJ_StartDate = DateTime.Now.AddDays(-1),
					ZZJ_IsTransitDeclarationCounty = true,
					ZZJ_RN_NKCustomsCountry = "Z1",
					ZZJ_TariffDataSource = "OWN",
					ZZJ_TurnOnASYCUDACustoms = true,
					ZZJ_TurnOnASYDCUDAManifest = true,
					ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping = "ZA",
					ZZJ_ZZZ_NKDefaultDataGrouping = "ZA"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefCusConfiguration)data).ZZJ_IsTransitDeclarationCounty = false;
			return true;
		}
	}
}
