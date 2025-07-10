using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class ProductTypeParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public ProductTypeParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfigurationWithAttributes(false);

		protected override string XMLWriterDataSource => "NexDoc Product Types";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_PRODUCT_TYPE.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Duplicate Product Type exists in downloaded List.");
			AppendProductTypeErrorDetails(keyValues, listCodeSet);
		}

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Unable to import Product Type due to empty Code, Type, Description or an invalid Start Date.");
			AppendProductTypeErrorDetails(keyValues, listCodeSet);
		}

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new ProductTypeKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeDescriptionAndStartDateIsValid(keyValues) && !string.IsNullOrWhiteSpace(keyValues.SecondaryCode);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			var refcusCodeList = new RefCusCodeList()
			{
				ZZD_ZZK_NKCodeType = NexDocConstants.CodeTypes.ProductTypePrefix + keyValues.Code,
				ZZD_Code = keyValues.SecondaryCode,
				ZZD_Description = keyValues.Description,
				ZZD_StartDate = keyValues.StartDate
			};
			var (endDateSuccessfullyParsed, endDateTime) = listItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.EndDate);
			if (endDateSuccessfullyParsed)
			{
				refcusCodeList.ZZD_EndDate = endDateTime;
			}
			var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
			AddAttribute(refCusCodeListAttributes, NexDocConstants.AttributeNames.SpeciesGroup, listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.SpeciesGroup), (value) => !string.IsNullOrWhiteSpace(value), true);
			AddAttribute(refCusCodeListAttributes, NexDocConstants.AttributeNames.EPN, listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.EPN), (value) => value == "Y", false);
			AddAttribute(refCusCodeListAttributes, NexDocConstants.AttributeNames.ScientificName, listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.ScientificName), (value) => !string.IsNullOrWhiteSpace(value), true);
			AddAttribute(refCusCodeListAttributes, NexDocConstants.AttributeNames.HalalReq, listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.HalalReq), (value) => value == "Y", false);
			refcusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
			result.Add(refcusCodeList);
		}

		void AppendProductTypeErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			var listItemCodeSet = listCodeSet.Items;
			ErrorBuilder.AppendLine("Product Type Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {keyValues.Code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Type: {keyValues.SecondaryCode}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {keyValues.Description}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.StartDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.EndDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Updated Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.UpdatedDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
		}

		class ProductTypeKeyValues : IKeyValues
		{
			public ProductTypeKeyValues(IItemCodeSet[] listItemCodeSet)
			{
				ListItemCodeSet = listItemCodeSet;
				StartDateDetails = ListItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.StartDate);
			}
			readonly IItemCodeSet[] ListItemCodeSet;
			readonly (bool SuccessfullyParsed, DateTime DateTime) StartDateDetails;

			public string Code => code ?? (code = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code));
			string code;

			public string SecondaryCode => secondaryCode ?? (secondaryCode = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Type));
			string secondaryCode;

			public string Description => description ?? (description = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Description).ToUpperInvariant());
			string description;

			public bool IsStartDateSuccesfullyParsed => StartDateDetails.SuccessfullyParsed;

			public DateTime StartDate => StartDateDetails.DateTime;

			public string UniqueCode => Code + SecondaryCode;
		}
	}
}
