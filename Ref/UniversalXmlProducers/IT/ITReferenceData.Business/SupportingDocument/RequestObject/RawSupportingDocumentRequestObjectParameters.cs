using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class RawSupportingDocumentRequestObjectParameters : IRawSupportingDocumentRequestObjectParameters
	{
		RawSupportingDocumentRequestObjectParameters()
		{
		}

		public string UC { get; private set; }

		public string SC { get; private set; }

		public string ST { get; private set; }

		public string Label { get; private set; }

		public string Suffix { get; private set; }

		public string ProgressiveNumber { get; private set; }

		public string DescriptionValidityStartDate { get; private set; }

		public static IRawSupportingDocumentRequestObjectParameters Build(Match regexMatch)
		{
			Argument.NotNull(regexMatch, nameof(regexMatch));

			return new RawSupportingDocumentRequestObjectParameters
			{
				UC = regexMatch.Groups["UC"].Value,
				SC = regexMatch.Groups["SC"].Value,
				ST = regexMatch.Groups["ST"].Value,
				Label = regexMatch.Groups["Label"].Value,
				Suffix = regexMatch.Groups["Suffix"].Value,
				ProgressiveNumber = regexMatch.Groups["ProgressiveNumber"].Value,
				DescriptionValidityStartDate = regexMatch.Groups["DescriptionValidityStartDate"].Value
			};
		}
	}
}
