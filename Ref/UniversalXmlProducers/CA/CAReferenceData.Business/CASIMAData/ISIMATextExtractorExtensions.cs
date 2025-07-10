using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData
{
	public static class ISIMATextExtractorExtensions
	{
		public static string ExtractDumpingCase(this string referenceNumberText, ISIMATextExtractor extractor)
		{
			Argument.NotNullOrEmpty(referenceNumberText, nameof(referenceNumberText));
			Argument.NotNull(extractor, nameof(extractor));

			return extractor.ExtractDumpingCase(referenceNumberText);
		}

		public static string[] ExtractClassificationNumbers(this string classificationText, ISIMATextExtractor extractor)
		{
			Argument.NotNullOrEmpty(classificationText, nameof(classificationText));
			Argument.NotNull(extractor, nameof(extractor));

			return extractor.ExtractClassificationNumbers(classificationText);
		}

		public static IEnumerable<string> ExtractDutyTypes(this string dutyText, ISIMATextExtractor extractor)
		{
			Argument.NotNullOrEmpty(dutyText, nameof(dutyText));
			Argument.NotNull(extractor, nameof(extractor));

			return extractor.ExtractDutyTypes(dutyText);
		}

		public static string ExtractEffectiveDate(this string dutyText, ISIMATextExtractor extractor)
		{
			Argument.NotNullOrEmpty(dutyText, nameof(dutyText));
			Argument.NotNull(extractor, nameof(extractor));

			return extractor.ExtractEffectiveDate(dutyText);
		}

		public static string ExtractDeterminationDate(this string dutyText, ISIMATextExtractor extractor)
		{
			Argument.NotNullOrEmpty(dutyText, nameof(dutyText));
			Argument.NotNull(extractor, nameof(extractor));

			return extractor.ExtractDeterminationDate(dutyText);
		}

		public static Tuple<string, string> ExtractDutyRate(this string dutyText, ISIMATextExtractor extractor)
		{
			Argument.NotNullOrEmpty(dutyText, nameof(dutyText));
			Argument.NotNull(extractor, nameof(extractor));

			return extractor.ExtractDutyRate(dutyText);
		}

		public static string[] ExtractCountryOfOriginOrExport(this string[] countryNames, ISIMATextExtractor extractor)
		{
			Argument.NotNull(countryNames, nameof(countryNames));
			Argument.NotNull(extractor, nameof(extractor));

			return extractor.ExtractCountryOfOriginOrExport(countryNames);
		}
	}
}
