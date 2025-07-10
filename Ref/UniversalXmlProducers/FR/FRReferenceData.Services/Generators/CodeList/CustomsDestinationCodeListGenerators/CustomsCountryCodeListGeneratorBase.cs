using System.Collections.Generic;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public abstract class CustomsCountryCodeListGeneratorBase : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.CustomsDestinationCodesFileName;
			}
		}

		protected override RefCusCodeList ProcessEachXmlNode(XmlNode node)
		{
			RefCusCodeList result = null;

			var startDate = string.IsNullOrEmpty(StartDateTag) ? UniversalDataHelper.MinimumDateTime : UniversalDataHelper.GetStartDateFromTag(node, StartDateTag);
			var endDate = string.IsNullOrEmpty(EndDateTag) ? UniversalDataHelper.MaximumDateTime : UniversalDataHelper.GetEndDateFromTag(node, EndDateTag);

			if (UniversalDataHelper.CheckDatesAreValid(startDate, endDate))
			{
				var code = UniversalDataHelper.GetTagValue(node, CodeTag);
				var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(node, DescriptionTag));
				var additionalDescription = !string.IsNullOrEmpty(AdditionalDescriptionTag) ? HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(node, AdditionalDescriptionTag)) : string.Empty;
				if (!string.IsNullOrEmpty(additionalDescription))
				{
					description += " - " + additionalDescription;
				}

				var valueForRequirement = UniversalDataHelper.GetTagValue(node, ValueForRequirementTag);
				if (IsMatchingRequirements(valueForRequirement, code))
				{
					result = new RefCusCodeList
					{
						ZZD_Code = code,
						ZZD_Description = description,
						ZZD_StartDate = startDate,
						ZZD_EndDate = endDate,
					};
				}
			}

			return result;
		}


		protected override bool GetStartDateFromInputFile => true;

		protected override bool GetEndDateFromInputFile => true;

		protected override string ValidityTag => string.Empty;

		protected override string CodeTag => "CHAMP1";

		protected override string StartDateTag => "CHAMP5";

		protected override string EndDateTag => "CHAMP6";

		protected override string DescriptionTag => "CHAMP2";

		protected override string AdditionalDescriptionTag => string.Empty;

		static string ValueForRequirementTag => "CHAMP3";

		protected static string[] EftaCountries => new string[] { "IS", "NO", "CH", "LI" };

		protected static string[] OtherEuLikeCountries => new string[] { "MK", "XS", "TR", "UA", "GB" };

		protected abstract bool IsMatchingRequirements(string valueForRequirement, string code);

		protected abstract bool IsValidForRequirement(string valueForRequirement);
	}
}
