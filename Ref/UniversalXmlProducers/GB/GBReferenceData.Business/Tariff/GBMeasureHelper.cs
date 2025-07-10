using System.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public class GBMeasureHelper : MeasureHelper
	{
		protected override void PreprocessMeasure(Measure m)
		{
			var asvConditions = m.Conditions.Where(x => x.MeasurementUnit == "ASV" && string.IsNullOrEmpty(x.MeasurementUnitQualifier)).ToArray();
			if (asvConditions.Length > 0 && asvConditions.All(x => x.DutyAmount.HasValue && x.DutyAmount.Value <= 1m))
			{
				foreach (var mc in asvConditions)
				{
					mc.DutyAmount *= 100;
				}
				foreach (var component in m.Components.Concat(m.Conditions.SelectMany(x => x.Components)))
				{
					if (component.DutyAmount.HasValue && component.MeasurementUnit == "ASV" && component.MeasurementUnitQualifier == "X")
					{
						component.DutyAmount /= 100;
					}
				}
			}
		}
	}
}
