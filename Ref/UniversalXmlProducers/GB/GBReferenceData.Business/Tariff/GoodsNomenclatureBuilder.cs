using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public class GoodsNomenclatureBuilder : GoodsNomenclatureBuilderBase
	{
		public GoodsNomenclatureBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string FilePrefix => "GB_RefCusNomenclatureGroup";
		protected override string XMLWriterDataSource => "GB Goods Nomenclature";
		protected override string DataGrouping => Constants.DefaultValues.GBDataGrouping;
	}
}
