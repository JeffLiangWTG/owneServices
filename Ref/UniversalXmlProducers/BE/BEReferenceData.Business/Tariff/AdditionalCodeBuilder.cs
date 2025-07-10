using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class AdditionalCodeBuilder : AdditionalCodeBuilderBase
	{
		public AdditionalCodeBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string FilePrefix => "BE_RefCusCodeList";

		protected override string XMLWriterDataSource => Constants.DataSources.BeAdditionalCodes;

		protected override string DataGrouping => Constants.Common.LocalCountryCode;

		protected override bool SupportsMultipleLanguages => true;
	}
}
