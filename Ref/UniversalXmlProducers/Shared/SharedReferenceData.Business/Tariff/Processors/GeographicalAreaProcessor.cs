using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public class GeographicalAreaProcessor : ProcessorBase<GeographicalArea>
	{
		public GeographicalAreaProcessor(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, builders, new GeographicalAreaLoader())
		{
		}

		public override bool IsChapterSpecific => false;

		protected override void UpdateModelsCore(string chapterFilter, List<ITariffModel> referenceData, StringBuilder errorCollector)
		{
			var models = Models.Cast<GeographicalArea>();

			foreach (var m in models.Where(x => x.Countries?.Any() ?? false))
			{
				foreach (var c in m.Countries)
				{
					var lookup = models.FirstOrDefault(x => x.HJID == c.GeographicalAreaHjid);
					c.CountryCode = lookup?.GeographicalAreaId ?? string.Empty;
					c.Description = lookup?.Description ?? string.Empty;
				}
			}

			Models = models.Cast<ITariffModel>().ToList();
		}
	}
}
