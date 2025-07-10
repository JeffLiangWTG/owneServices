using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class ProductCategoryParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public ProductCategoryParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfigurationWithAttributes(true);

		protected override string XMLWriterDataSource => "NexDoc Product Categories";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_PRODUCT_CATEGORY.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Duplicate Product Category exists in downloaded List.");
			AppendProductCategoryErrorDetails(keyValues, listCodeSet);
		}

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Unable to import Product Category due to empty Category Code, Type or Category Description.");
			AppendProductCategoryErrorDetails(keyValues, listCodeSet);
		}

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new ProductCategoryKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeAndDescriptionIsValid(keyValues) && !string.IsNullOrWhiteSpace(keyValues.SecondaryCode);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			var codeType = NexDocConstants.CodeTypes.ProductCategoryPrefix + keyValues.SecondaryCode;
			var code = keyValues.Code;
			var refcusCodeListAlreadyInList = result.Where(x => x.ZZD_ZZK_NKCodeType == codeType && x.ZZD_Code == code).SingleOrDefault();

			if (refcusCodeListAlreadyInList == null)
			{
				var refcusCodeList = new RefCusCodeList()
				{
					ZZD_ZZK_NKCodeType = NexDocConstants.CodeTypes.ProductCategoryPrefix + keyValues.SecondaryCode,
					ZZD_Code = keyValues.Code,
					ZZD_Description = keyValues.Description,
				};
				var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
				AddAttribute(refCusCodeListAttributes, NexDocConstants.AttributeNames.ProductType, listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code), (value) => !string.IsNullOrWhiteSpace(value), true);
				refcusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
				result.Add(refcusCodeList);
			}
			else
			{
				var productTypeToAdd = listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code);
				if (!string.IsNullOrWhiteSpace(productTypeToAdd))
				{
					var attributes = refcusCodeListAlreadyInList.RefCusCodeListAttributes;
					refcusCodeListAlreadyInList.RefCusCodeListAttributes = attributes.Concat(new[] { new RefCusCodeListAttribute() { ZZE_ZXE_NKName = NexDocConstants.AttributeNames.ProductType, ZZE_Value = productTypeToAdd } }).ToArray();
				}
			}
		}

		void AppendProductCategoryErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			var listItemCodeSet = listCodeSet.Items;
			ErrorBuilder.AppendLine("Product Category Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code)}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Type: {keyValues.SecondaryCode}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Description)}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Category Code: {keyValues.Code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Category Description: {keyValues.Description}");
		}

		class ProductCategoryKeyValues : IKeyValues
		{
			public ProductCategoryKeyValues(IItemCodeSet[] listItemCodeSet)
			{
				ListItemCodeSet = listItemCodeSet;
			}
			readonly IItemCodeSet[] ListItemCodeSet;

			public string Code => code ?? (code = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CategoryCode));
			string code;

			public string SecondaryCode => secondaryCode ?? (secondaryCode = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.ProductType));
			string secondaryCode;

			public string Description => description ?? (description = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CategoryDescription).ToUpperInvariant());
			string description;

			public bool IsStartDateSuccesfullyParsed => false;

			public DateTime StartDate => DateTime.MinValue;

			public string UniqueCode => Code + SecondaryCode + ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code);
		}
	}
}
