using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public static class CSVProcessorFactory
{
	public static ICSVProcessorParser GetCSVParser(string listType, IDateTimeProvider dateTimeProvider, string dateFormat) =>
		listType switch
		{
			Constants.RefCusCodeListTypes.AI44E => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.DC40A => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.DC40E => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.DC40N => new CSVProcessorParser<CSVProcessorAttributesItem, CSVProcessorAttributesItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.DC40T => new CSVProcessorParser<CSVProcessorNationalAttributesItem, CSVProcessorNationalAttributesItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.DC40W => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.DC40X => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.DC44H => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.EXSEC => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.TD44E => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			Constants.RefCusCodeListTypes.TD44G => new CSVProcessorParser<CSVProcessorItem, CSVProcessorItemMap>(dateTimeProvider, dateFormat, listType),
			_ => throw new InvalidOperationException($"List type '{listType}' is not supported")
		};
}
