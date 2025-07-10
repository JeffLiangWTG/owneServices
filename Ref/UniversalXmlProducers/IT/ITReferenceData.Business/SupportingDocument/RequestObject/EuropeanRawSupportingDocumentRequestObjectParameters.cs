using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class EuropeanRawSupportingDocumentRequestObjectParameters : IEuropeanRawSupportingDocumentRequestObjectParameters
	{
		EuropeanRawSupportingDocumentRequestObjectParameters()
		{
		}

		public string RegGrpCountryCode { get; private set; }

		public string UC { get; private set; }

		public string SC { get; private set; }

		public string ST { get; private set; }

		public string Label { get; private set; }

		public string Suffix { get; private set; }

		public string ProgressiveNumber { get; private set; }

		public string DescriptionValidityStartDate { get; private set; }

		public static IEuropeanRawSupportingDocumentRequestObjectParameters Build(Match regexMatch)
		{
			Argument.NotNull(regexMatch, nameof(regexMatch));

			return new EuropeanRawSupportingDocumentRequestObjectParameters
			{
				UC = regexMatch.Groups["UC"].Value,
				SC = regexMatch.Groups["SC"].Value,
				ST = regexMatch.Groups["ST"].Value,
				Label = regexMatch.Groups["Label"].Value,
				Suffix = regexMatch.Groups["DatiGeneraliTipoCertificato"].Value,
				ProgressiveNumber = regexMatch.Groups["DatiGeneraliNumeroCertificato"].Value,
				DescriptionValidityStartDate = regexMatch.Groups["DatiGeneraliDataIniValDesCertificato"].Value,
				RegGrpCountryCode = regexMatch.Groups["CodPaeseRegGrp"].Value,
			};
		}
	}
}
