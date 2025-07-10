using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public class AdditionalCodeBuilder : AdditionalCodeBuilderBase
	{
		public AdditionalCodeBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string FilePrefix => "GB_RefCusCodeList";

		protected override string XMLWriterDataSource => "GB Additional Codes";

		protected override string DataGrouping => Constants.DefaultValues.GBDataGrouping;
	}
}
