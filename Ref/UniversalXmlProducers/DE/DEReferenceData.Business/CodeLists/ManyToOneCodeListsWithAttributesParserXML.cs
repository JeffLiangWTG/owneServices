using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public abstract class ManyToOneCodeListsWithAttributesParserXML : ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>
	{
		protected ManyToOneCodeListsWithAttributesParserXML(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected abstract IEnumerable<string> InputAttributes { get; }

		protected virtual bool AttributeDatesEnabled => false;

		protected static List<RefCusCodeList> CombineListsWithLevelAttributes(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults, string baseListCodeType, IEnumerable<(string codeType, string attributeToUpdate)> orderedCodeTypesWithAttributesToUpdate) =>
			CombineListsWithAttributes(parseResults, baseListCodeType, orderedCodeTypesWithAttributesToUpdate, CodeListsHelper.CreateOrUpdateRefCusCodeListWithLevelAttribute);

		protected static List<RefCusCodeList> CombineListsWithAttributes(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults, string baseListCodeType, IEnumerable<(string codeType, string attributeToUpdate)> orderedCodeTypesWithAttributesToUpdate,
			Action<List<RefCusCodeList>, List<IKeyValuesWithAttributes>, string> createOrUpdateRefCusCodeListWithAttributeAction)
		{
			var result = new List<RefCusCodeList>();
			if (parseResults.TryGetValue(baseListCodeType, out var baseListKeyValues))
			{
				result.AddRange(baseListKeyValues.List.Select(x => CodeListsHelper.CreateRefListWithAttributes(x)));
				foreach (var (codeType, attributeToUpdate) in orderedCodeTypesWithAttributesToUpdate)
				{
					if (parseResults.TryGetValue(codeType, out var keyValuesToProcess))
					{
						createOrUpdateRefCusCodeListWithAttributeAction(result, keyValuesToProcess.List, attributeToUpdate);
					}
				}
			}
			return result;
		}

		protected static void AddAttributeLevels(List<RefCusCodeList> result, Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults,  ICollection<string> attributesToAddLevel, IDictionary<string, string> codeTypesByLevel, DateTime currentDate)
		{
			foreach (var refCusCodeList in result)
			{
				var attributes = refCusCodeList.RefCusCodeListAttributes.ToList();
				var levelAttributes = attributes.Where(a => a.ZZE_ZXE_NKName == CodeListsConstants.XMLEntryElementNames.LEVEL);
				var levelsToAdd = levelAttributes.Select(a => a.ZZE_Value).Distinct().ToArray();

				foreach (var attributeToAddLevel in attributesToAddLevel)
				{
					attributes.RemoveAll(a => a.ZZE_ZXE_NKName == attributeToAddLevel);
					var attributesToReplace = new List<RefCusCodeListAttribute>();
					foreach (var levelToAdd in levelsToAdd)
					{
						var codeType = codeTypesByLevel[levelToAdd];
						var parseResult = parseResults[codeType];

						var keyValuesWithAttribute = parseResult.List.SingleOrDefault(v => v.Code == refCusCodeList.ZZD_Code);
						var currentLevelAttributes = keyValuesWithAttribute?.Attributes.Where(a => a.Key == attributeToAddLevel);
						if (currentLevelAttributes != null)
						{
							foreach (var currentLevelAttribute in currentLevelAttributes)
							{
								attributesToReplace.Add(new RefCusCodeListAttribute
								{
									ZZE_ZXE_NKName = $"{attributeToAddLevel};{levelToAdd}",
									ZZE_Value = currentLevelAttribute.Value,
									ZZE_StartDate = currentLevelAttribute.StartDate,
									ZZE_EndDate = currentLevelAttribute.EndDate,
								});
							}
						}
					}

					foreach (var grouping in attributesToReplace.GroupBy(a => (a.ZZE_StartDate, a.ZZE_EndDate)).Where(a => !a.Key.ZZE_EndDate.HasValue || a.Key.ZZE_EndDate.Value > currentDate))
					{
						if (grouping.Select(a => a.ZZE_Value).Distinct().Count() > 1)
						{
							attributes.AddRange(grouping);
						}
						else
						{
							var highestAttribute = grouping.Last();
							attributes.Add(new RefCusCodeListAttribute
							{
								ZZE_ZXE_NKName = highestAttribute.ZZE_ZXE_NKName.EndsWith(codeTypesByLevel.Keys.Last(), StringComparison.InvariantCulture) ? attributeToAddLevel : highestAttribute.ZZE_ZXE_NKName,
								ZZE_Value = highestAttribute.ZZE_Value,
								ZZE_StartDate = highestAttribute.ZZE_StartDate,
								ZZE_EndDate = highestAttribute.ZZE_EndDate,
							});
						}
					}
				}

				refCusCodeList.RefCusCodeListAttributes = attributes.ToArray();
			}
		}

		protected override void AppendInvalidDataErrorDetails(string customsCodeListIdentifier, IKeyValuesWithAttributes keyValues, XElement entry)
		{
			base.AppendInvalidDataErrorDetails(customsCodeListIdentifier, keyValues, entry);
			foreach (var attribute in InputAttributes)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture,  $"{attribute}: {entry.Element(attribute)?.Value}");
			}
		}

		protected override void AddOrUpdateCodesToImport(Dictionary<string, IKeyValuesWithAttributes> codesToImport, IKeyValuesWithAttributes newKeyValues)
		{
			if (AttributeDatesEnabled)
			{
				CombineDuplicatedAttributesWithDifferentDates(() => base.AddOrUpdateCodesToImport(codesToImport, newKeyValues));
			}
			else
			{
				base.AddOrUpdateCodesToImport(codesToImport, newKeyValues);
			}

			void CombineDuplicatedAttributesWithDifferentDates(Action baseFunctionality)
			{
				var codeIsDuplicate = codesToImport.TryGetValue(newKeyValues.UniqueCode, out var alreadyAddedRecord);
				List<KeyValueAttribute> combinedAttributes = null;
				if (codeIsDuplicate)
				{
					combinedAttributes = alreadyAddedRecord.Attributes.ToList();
					foreach (var newAttribute in newKeyValues.Attributes)
					{
						if (!combinedAttributes.Any(a => a.Key == newAttribute.Key && a.StartDate == newAttribute.StartDate && a.EndDate == newAttribute.EndDate))
						{
							combinedAttributes.Add(newAttribute);
						}
					}
				}

				baseFunctionality();

				if (combinedAttributes != null)
				{
					codesToImport[newKeyValues.UniqueCode].Attributes.Clear();
					foreach (var originalAttribute in combinedAttributes)
					{
						codesToImport[newKeyValues.UniqueCode].Attributes.Add(originalAttribute);
					}
				}
			}
		}
	}
}
