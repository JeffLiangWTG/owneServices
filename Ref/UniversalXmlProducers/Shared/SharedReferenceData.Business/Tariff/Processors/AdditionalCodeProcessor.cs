using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public class AdditionalCodeProcessor : ProcessorBase<AdditionalCode>
	{
		public AdditionalCodeProcessor(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, builders, new AdditionalCodeLoader())
		{
		}

		public override bool IsChapterSpecific => false;

		protected override void UpdateModelsCore(string chapterFilter, List<ITariffModel> referenceData, StringBuilder errorCollector)
		{
			var models = Models.Cast<AdditionalCode>().ToList();

			Parallel.ForEach(models, m =>
			{
				m.Description = CommonHelper.CleanHtmlTags(m.Description, false);
			});

			Models = models.Cast<ITariffModel>().ToList();
		}
	}
}
