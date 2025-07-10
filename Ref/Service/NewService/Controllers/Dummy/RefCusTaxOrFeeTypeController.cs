using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefCusTaxOrFeeTypeController
{
	protected override IEnumerable<Models.RefCusTaxOrFeeType> GetData(DataBlockKey<Models.RefCusTaxOrFeeType> key, string version)
	{
		var taxOrFeeTypes = base.GetData(key, version);
		if (taxOrFeeTypes.Any(type => type.RefCusTaxOrFees.Length > 0))
		{
			// Intentionally yield the dummy record first.
			// The client is expected to retrieve this dummy record and immediately throw an exception if rounding issue exists.
			yield return DummyRefCusTaxOrFeeType();
		}

		foreach (var taxOrFeeType in taxOrFeeTypes)
		{
			yield return taxOrFeeType;
		}
	}

	static Models.RefCusTaxOrFeeType DummyRefCusTaxOrFeeType()
	{
		return new Models.RefCusTaxOrFeeType
		{
			ZX0_TaxOrFeeType = "XXX",
			ZX0_Description = RoundingIssueConstants.DummyDescription,
			RefCusTaxOrFees = [ DummyRefCusTaxOrFee() ],
			Deleted = false,
			Checkpoint = null
		};
	}

	static Models.RefCusTaxOrFee DummyRefCusTaxOrFee()
	{
		return new Models.RefCusTaxOrFee
		{
			ZZF_Code = "XXX",
			ZZF_Description = "Dummy RefCusTaxOrFee",
			ZZF_Value = RoundingIssueConstants.Value,
			ZZF_StartDate = DateTime.UtcNow,
			ZZF_EndDate = DateTime.UtcNow.AddMonths(1),
			ZZF_ZZZ_NKDataGrouping = RoundingIssueConstants.Flag,
			ZZF_ZX0_NKTaxOrFeeType = "XXX"
		};
	}
}
