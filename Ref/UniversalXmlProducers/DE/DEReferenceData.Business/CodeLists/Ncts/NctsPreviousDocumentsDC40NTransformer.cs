using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts
{
	public class NctsPreviousDocumentsDC40NTransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public NctsPreviousDocumentsDC40NTransformer(string[] downLoadLinks)
			: this(downLoadLinks, DateTime.Today)
		{
		}

		public NctsPreviousDocumentsDC40NTransformer(string[] downLoadLinks, DateTime currentDate)
			: base(downLoadLinks)
		{
			this.currentDate = currentDate;
		}

		readonly DateTime currentDate;

		protected override string[] CustomsCodeListIdentifiers => new[] {CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0933_PREVIOUS_DOCUMENTS, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0935_PREVIOUS_DOCUMENTS, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0936_PREVIOUS_DOCUMENTS };

		protected override string CodeType => CodeListsConstants.Ncts.CodeTypes.NCTS_DC40N_PREVIOUS_DOCUMENTS;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_DC40N_PREVIOUS_DOCUMENTS);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(true, CodeType, enableAttributeDates: true);

		protected override bool AttributeDatesEnabled => true;

		protected override IEnumerable<string> InputAttributes => new[]
		{
			CodeListsConstants.XMLEntryElementNames.REFERENCE,
			CodeListsConstants.XMLEntryElementNames.ITEM_NUMBER,
			CodeListsConstants.XMLEntryElementNames.COMPLEMENT,
			CodeListsConstants.XMLEntryElementNames.DETAIL,
			CodeListsConstants.XMLEntryElementNames.AUTHORITY,
			CodeListsConstants.XMLEntryElementNames.ISSUING_DATE,
			CodeListsConstants.XMLEntryElementNames.VALIDITY_DATE,
			CodeListsConstants.XMLEntryElementNames.MEASUREMENT_UNIT,
			CodeListsConstants.XMLEntryElementNames.COMPLEMENTARY_UNIT,
			CodeListsConstants.XMLEntryElementNames.VALUE
		};

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults)
		{
			(string codeType, string attributeToUpdate)[] orderedCodeTypeWithAttributeToUpdate =
			{
				(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0935_PREVIOUS_DOCUMENTS, CodeListsConstants.AttributeValues.House),
				(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0933_PREVIOUS_DOCUMENTS, CodeListsConstants.AttributeValues.Header)
			};

			var result = CombineListsWithLevelAttributes(parseResults, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0936_PREVIOUS_DOCUMENTS, orderedCodeTypeWithAttributeToUpdate);

			var codeTypesByLevel = new Dictionary<string, string>
			{
				{ CodeListsConstants.AttributeValues.Item, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0936_PREVIOUS_DOCUMENTS },
				{ CodeListsConstants.AttributeValues.House, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0935_PREVIOUS_DOCUMENTS },
				{ CodeListsConstants.AttributeValues.Header, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0933_PREVIOUS_DOCUMENTS },
			};
			string[] attributesToAddLevel = { CodeListsConstants.XMLEntryElementNames.REFERENCE, CodeListsConstants.XMLEntryElementNames.COMPLEMENT };
			AddAttributeLevels(result, parseResults, attributesToAddLevel, codeTypesByLevel, currentDate);

			return result;
		}

		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var attributes = new List<KeyValueAttribute>();

			var startDateParsed = Helper.GetDateTime(entry.Element(CodeListsConstants.XMLEntryElementNames.START_DATE)?.Value, CodeListsConstants.SourceDateFormatXML);
			var startDate = startDateParsed.SuccessfullyParsed ? startDateParsed.DateTime : Constants.MinimumDateTime;
			var endDateParsed = Helper.GetDateTime(entry.Element(CodeListsConstants.XMLEntryElementNames.END_DATE)?.Value, CodeListsConstants.SourceDateFormatXML);
			var endDate = endDateParsed.SuccessfullyParsed ? endDateParsed.DateTime : Constants.MaximumDateTime;


			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.LEVEL_ITEM, CodeListsConstants.AttributeValues.Item, startDate, endDate);
			foreach (var allowedAttribute in InputAttributes)
			{
				CodeListsHelper.AddAllowedToAttributesAsBool(attributes, allowedAttribute, entry.Element(allowedAttribute)?.Value, startDate, endDate);
			}

			return new BaseKeyValuesWithAttributesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);
		}
	}
}
