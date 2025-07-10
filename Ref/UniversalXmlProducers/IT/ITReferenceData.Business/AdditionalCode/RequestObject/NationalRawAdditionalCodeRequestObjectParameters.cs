using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public class NationalRawAdditionalCodeRequestObjectParameters : INationalRawAdditionalCodeRequestObjectParameters
	{
		public string UC { get; private set; }

		public string SC { get; private set; }

		public string ST { get; private set; }

		public string Label { get; private set; }

		public string AdditionalCodeSequentialNumber { get; private set; }

		public string AdditionalCodeType { get; private set; }

		public string ValidityStartDate { get; private set; }

		public string SidCad { get; private set; }

		public static INationalRawAdditionalCodeRequestObjectParameters Build(Match regexMatch)
		{
			Argument.NotNull(regexMatch, nameof(regexMatch));

			return new NationalRawAdditionalCodeRequestObjectParameters
			{
				UC = regexMatch.Groups["UC"].Value,
				SC = regexMatch.Groups["SC"].Value,
				ST = regexMatch.Groups["ST"].Value,
				Label = regexMatch.Groups["Label"].Value,
				AdditionalCodeSequentialNumber = regexMatch.Groups["AdditionalCodeSequentialNumber"].Value,
				AdditionalCodeType = regexMatch.Groups["AdditionalCodeType"].Value,
				ValidityStartDate = regexMatch.Groups["ValidityStartDate"].Value,
				SidCad = regexMatch.Groups["SidCad"].Value,
			};
		}
	}
}
