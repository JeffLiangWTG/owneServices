using System;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public abstract class CommonParser
	{
		protected CommonParser(IDateTimeProvider dateTimeProvider)
		{
			DateTimeProvider = dateTimeProvider;
		}

		protected IDateTimeProvider DateTimeProvider { get; }

		protected StringBuilder ErrorBuilder => errorBuilder ??= new StringBuilder();
		StringBuilder errorBuilder;

		protected StringBuilder LogBuilder => logBuilder ??= new StringBuilder();
		StringBuilder logBuilder;

		protected abstract XmlWriterConfiguration XMLWriterConfiguration { get; }

		protected virtual Uri GetQueryUrlForTariffCode(string refDbServiceURI, string tariffCode, bool isESTariff, bool checkExactTariff) => TariffHelper.GetQueryUrlForTariffCode(refDbServiceURI, tariffCode, DateTimeProvider.CurrentLocalDate, isESTariff, checkExactTariff);

		protected virtual string GetDataGroupingWhenESTariff(string refDbServiceURI, string tariffCode) => TariffHelper.GetDataGroupingWhenESTariff(GetQueryUrlForTariffCode, refDbServiceURI, tariffCode, ErrorBuilder);
	}
}
