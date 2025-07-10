using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.FRReferenceData.Services.UniversalDataHelper;
using AttributeNames = CargoWise.RefDbRepo.FRReferenceData.Services.TAXCodeListAttributeNameGenerator.AttributeNames;
namespace CargoWise.RefDbRepo.FRReferenceData.Services;

public class TAXCodeListGenerator : XmlDrivenCodeListDataFileGenerator
{
	protected override string ValidityTag => string.Empty;

	protected override string CodeTag => "CHAMP1";

	protected override string StartDateTag => "CHAMP15";

	protected override string EndDateTag => "CHAMP16";

	protected override string DescriptionTag => "CHAMP2";

	protected override string AdditionalDescriptionTag => string.Empty;

	protected override string CodeType => "TAX";

	protected override bool HasAttributes => true;

	protected override BaseCodeListAttributeNameGenerator[] AttributeNameGenerators => [
		new TAXCodeListAttributeNameGenerator()
	];

	protected override IEnumerable<string> InputFileNames
	{
		get
		{
			yield return ApplicationConfig.Instance.RateTypeFileName;
		}
	}

	protected override RefCusCodeTypeGenerator CodeTypeGenerator => new TAXCodeTypeGenerator();

	protected override bool GetStartDateFromInputFile => true;

	protected override bool GetEndDateFromInputFile => true;

	protected override RefCusCodeListAttribute[] GetCusCodeListAttributesFromNode(XmlNode node)
	{
		var PrecalculatedApplicabilityDict = new Dictionary<string, string>
		{
			{ "0", "Allowed" },
			{ "1", "Mandatory" },
			{ "2", "Prohibited" }
		};
		var CalculationTypeDict = new Dictionary<string, string>
		{
			{ "0", "ADVAL" },
			{ "1", "SPEC" },
			{ "2", "AUT" },
		};

		
		var result = new List<RefCusCodeListAttribute>();
		for (var i = 3; i <= 14; i++)
		{
			var tagNodeName = $"CHAMP{i}";
			var attributeInfo = TAXCodeListAttributeNameGenerator.AttributeInfos[i - 3];
			var attributeName = attributeInfo.Name;
			var tagValue = attributeInfo.DataType == Constants.AttributeDataTypes.Boolean
				? GetTagBooleanValue(node, tagNodeName)
				: GetTagValue(node, tagNodeName);

			if (!string.IsNullOrEmpty(tagValue))
			{
				if (attributeName == AttributeNames.ApplicationTerritory)
				{
					var refinedTagValue = new string(tagValue).Where(x => char.IsLetter(x) || char.IsNumber(x)).ToArray();
					var territories = refinedTagValue.Chunk(5).Select(x => new string(x));
					result.AddRange(territories.Select(x => CreateRefCusCodeListAttribute(AttributeNames.ApplicationTerritory, x)));
				}
				else
				{
					result.Add(attributeName switch
					{
						AttributeNames.Precalculable => CreateRefCusCodeListAttribute(attributeName, PrecalculatedApplicabilityDict[tagValue]),
						AttributeNames.CalculationType => CreateRefCusCodeListAttribute(attributeName, CalculationTypeDict[tagValue]),
						_ => CreateRefCusCodeListAttribute(attributeName, tagValue)
					});
				}
			}
		}
		return result.ToArray();
	}
}
