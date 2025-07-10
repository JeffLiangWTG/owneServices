using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs
{
	public class EmcsProductCodesEPCTransformer : CodeListsParserTSV<RefCusCodeList, IKeyValuesWithAttributes>
	{
		public EmcsProductCodesEPCTransformer(Dictionary<string, string> downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0036_PRODUCT_CODES;

		protected override string CodeType => CodeListsConstants.Emcs.CodeTypes.EMCS_EPC_PRODUCT_CODES;

		protected override string HtmlElementIdentifier => "Codeliste 36 - Excise Product";

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EPC_PRODUCT_CODES);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(false, CodeType);

		protected override void AppendInvalidDataErrorDetails(string[] record)
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.");
			CodeListsHelper.GetBaseErrorDetailsTSV(ErrorBuilder, record);
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"ExciseProductsCategoryCode: {record[EmcsCL0036TSVColumns.EXCISE_PRODUCTS_CATEGORY_CODE.Key]}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"UnitOfMeasureCode: {record[EmcsCL0036TSVColumns.UNIT_OF_MEASURE_CODE.Key]}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"AlcoholicStrengthApplicabilityFlag: {record[EmcsCL0036TSVColumns.ALCOHOLIC_STRENGTH_APPLICABILITY_FLAG.Key]}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"DegreePlatoApplicabilityFlag: {record[EmcsCL0036TSVColumns.DEGREE_PLATO_APPLICABILITY_FLAG.Key]}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"DensityApplicabilityFlag: {record[EmcsCL0036TSVColumns.DENSITY_APPLICABILITY_FLAG.Key]}");
		}

		protected override RefCusCodeList CreateRefList(IKeyValuesWithAttributes keyValues) => CodeListsHelper.CreateRefListWithAttributes(keyValues);

		protected override IKeyValuesWithAttributes GetKeyValues(string[] record)
		{
			var exciseProductCategory = EmcsCL0036TSVColumns.EXCISE_PRODUCTS_CATEGORY_CODE;
			var unitOfMeasure = EmcsCL0036TSVColumns.UNIT_OF_MEASURE_CODE;
			var alcoholicStrength = EmcsCL0036TSVColumns.ALCOHOLIC_STRENGTH_APPLICABILITY_FLAG;
			var degreePlato = EmcsCL0036TSVColumns.DEGREE_PLATO_APPLICABILITY_FLAG;
			var density = EmcsCL0036TSVColumns.DENSITY_APPLICABILITY_FLAG;

			var attributes = new List<KeyValueAttribute>();
			CodeListsHelper.AddToAttributes(attributes, exciseProductCategory.Value, record[exciseProductCategory.Key]);
			CodeListsHelper.AddToAttributes(attributes, unitOfMeasure.Value, record[unitOfMeasure.Key]);
			CodeListsHelper.AddToAttributes(attributes, alcoholicStrength.Value, record[alcoholicStrength.Key] == "1" ? CodeListsConstants.AttributeValues.Yes : CodeListsConstants.AttributeValues.No);
			CodeListsHelper.AddToAttributes(attributes, degreePlato.Value, record[degreePlato.Key] == "1" ? CodeListsConstants.AttributeValues.Yes : CodeListsConstants.AttributeValues.No);
			CodeListsHelper.AddToAttributes(attributes, density.Value, record[density.Key] == "1" ? CodeListsConstants.AttributeValues.Yes : CodeListsConstants.AttributeValues.No);

			return new BaseKeyValuesWithAttributesTSV(record, attributes);
		}

		protected override string[] GetCodeList(HttpClient client, string downloadUrl) => DownloadEMCSAndExtractTSVCodeList(client, downloadUrl, CustomsCodeListIdentifier);
	}

	class EmcsCL0036TSVColumns
	{
		public static KeyValuePair<int, string> EXCISE_PRODUCTS_CATEGORY_CODE => new KeyValuePair<int, string>(5, "ExciseProductCategory");

		public static KeyValuePair<int, string> UNIT_OF_MEASURE_CODE => new KeyValuePair<int, string>(6, "UnitMeasure");

		public static KeyValuePair<int, string> ALCOHOLIC_STRENGTH_APPLICABILITY_FLAG => new KeyValuePair<int, string>(7, "AlcoholicStrength");

		public static KeyValuePair<int, string> DEGREE_PLATO_APPLICABILITY_FLAG => new KeyValuePair<int, string>(8, "DegreePlato");

		public static KeyValuePair<int, string> DENSITY_APPLICABILITY_FLAG => new KeyValuePair<int, string>(9, "Density");
	}
}
