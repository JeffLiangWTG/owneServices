using System;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator;

static class CheckRoundingScriptFactory
{
	public static ICheckRoundingScript Create(Type storageType)
	{
		return storageType switch
		{
			not null when storageType == typeof(IRefExchangeRateZZ) => new RefExchangeRateZZCheckRoundingScript(),
			not null when storageType == typeof(IRefCusQuota) => new RefCusQuotaCheckRoundingScript(),
			not null when storageType == typeof(IRefCusTaxOrFeeType) => new RefCusTaxOrFeeTypeCheckRoundingScript(),
			_ => new DefaultCheckRoundingScript()
		};
	} 
}
