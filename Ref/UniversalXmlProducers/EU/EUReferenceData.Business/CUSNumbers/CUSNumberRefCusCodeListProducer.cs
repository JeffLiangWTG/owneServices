using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public sealed class CUSNumberRefCusCodeListProducer
	{
		public CUSNumberRefCusCodeListProducer(ICusCodeAttributeProvider cusCodeAttributeProvider)
		{
			this.cusCodeAttributeProvider = Argument.NotNull(cusCodeAttributeProvider, nameof(cusCodeAttributeProvider));
		}

		public IEnumerable<RefCusCodeList> GenerateRefCusCodeListItems(IEnumerable<CUSNumber> cusNumbers)
		{
			var result = new List<RefCusCodeList>();
			foreach (var cusNumber in cusNumbers)
			{
				var attributes = new List<RefCusCodeListAttribute>();
				AddAttribute(attributes, cusNumber.CNCode, Constants.CUSNumbers.CNCodeAttributeValue);
				AddAttribute(attributes, cusNumber.CasRn, Constants.CUSNumbers.CASrnAttributeValue);
				AddAttribute(attributes, cusNumber.EcNumber, Constants.CUSNumbers.ECNUMBERAttributeValue);
				AddAttribute(attributes, cusNumber.UnNumber, Constants.CUSNumbers.UNnumberAttributeValue);

				var cusCodeAttribute = cusCodeAttributeProvider.GetCusCodeAttribute(cusNumber.Code);
				if (cusCodeAttribute != null)
				{
					attributes.Add(cusCodeAttribute);
				}

				var translations = cusNumber.Translations.Select(t => new RefCusCodeListLanguage
				{
					ZXA_ZX6_NKLanguage = t.Code,
					ZXA_Description = t.Description,
				}).ToArray();

				result.Add(new RefCusCodeList
				{
					ZZD_Code = cusNumber.Code,
					ZZD_ZZZ_NKDataGrouping = "EUN",
					ZZD_Description = cusNumber.Description,
					ZZD_StartDate = Constants.MinimumDateTime,
					RefCusCodeListAttributes = attributes.ToArray(),
					RefCusCodeListLanguages = translations
				});
			}
			return result;
		}

		static void AddAttribute(List<RefCusCodeListAttribute> attributes, string value, string name)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				attributes.Add(new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = name,
					ZZE_Value = value
				});
			}
		}

		readonly ICusCodeAttributeProvider cusCodeAttributeProvider;
	}
}
