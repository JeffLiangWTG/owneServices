using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefCusQuotaController
{
	protected override IEnumerable<Models.RefCusQuota> GetData(DataBlockKey<Models.RefCusQuota> key, string version)
	{
		var quotas = base.GetData(key, version);
		if (quotas.Any())
		{
			// Intentionally yield the dummy record first.
			// The client is expected to retrieve this dummy record and immediately throw an exception if rounding issue exists.
			yield return DummyRefCusQuota();
		}

		foreach (var quota in quotas)
		{
			yield return quota;
		}
	}

	static Models.RefCusQuota DummyRefCusQuota()
	{
		return new Models.RefCusQuota
		{
			ZXQ_OrderNumber = "XXXXXX",
			ZXQ_InitialAmount = 0.0m,
			ZXQ_UnitOfMeasure = "KGM",
			ZXQ_Balance = RoundingIssueConstants.Value,
			ZXQ_StartDate = DateTime.UtcNow,
			ZXQ_EndDate = DateTime.UtcNow.AddMonths(1),
			ZXQ_ZZZ_NKDataGrouping = RoundingIssueConstants.Flag,
		};
	}
}
