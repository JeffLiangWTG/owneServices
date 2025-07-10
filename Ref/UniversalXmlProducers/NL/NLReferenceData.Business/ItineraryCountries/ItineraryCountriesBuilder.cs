using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class ItineraryCountriesBuilder : RefCusCodeListBuilder<ItineraryCountriesData>
	{
		public ItineraryCountriesBuilder(StringBuilder errorCollector) : base(errorCollector)
		{
		}

		protected override string XMLWriterDataSource => "Itinerary Countries";

		protected override IEnumerable<RefCusCodeList> ConvertToRefModels(IEnumerable<ItineraryCountriesData> data)
		{
			var results = new List<RefCusCodeList>();

			foreach (var ad in data)
			{
				var newRefCode = new RefCusCodeList
				{
					ZZD_Code = ad.Code,
					ZZD_Description = ad.Description,
				};

				results.Add(newRefCode);
			}
			return results;
		}

		protected override StringBuilder IsValidCore(RefCusCodeList refModel) => new StringBuilder();

		protected override XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusCodeList>(true);

			entityConfig.IncludeColumn(x => x.ZZD_Code, true);
			entityConfig.IncludeColumn(x => x.ZZD_Description);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.ItineraryCountriesDefaults.CodeType);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DefaultValues.NLDataGrouping);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.DefaultValues.MinimumDateTime);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaximumDateTime);
			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			return writerConfig;
		}

		protected override IEnumerable<ItineraryCountriesData> OrderList(IList<ItineraryCountriesData> data)
		{
			return data.OrderBy(x => x.Code);
		}
	}
}
