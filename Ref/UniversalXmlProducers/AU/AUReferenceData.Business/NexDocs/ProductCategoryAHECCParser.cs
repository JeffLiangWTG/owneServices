using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class ProductCategoryAHECCParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public ProductCategoryAHECCParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfigurationWithAttributes(true);

		protected override string XMLWriterDataSource => "NexDoc Product Category AHECC";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_PRODUCT_CATEGORY_AHECC.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Duplicate Product Category AHECC exists in downloaded List.");
			AppendProductCategoryErrorDetails(keyValues, listCodeSet);
		}

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Unable to import Product Category due to empty Category Code or AHECC Code.");
			AppendProductCategoryErrorDetails(keyValues, listCodeSet);
		}

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new ProductCategoryAHECCKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => !string.IsNullOrWhiteSpace(keyValues.Code) && !string.IsNullOrWhiteSpace(keyValues.SecondaryCode);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			var code = keyValues.Code;
			var refcusCodeList = new RefCusCodeList()
			{
				ZZD_ZZK_NKCodeType = NexDocConstants.CodeTypes.ProductCategoryAHECC,
				ZZD_Code = code,
				ZZD_Description = code,
				ZZD_StartDate = keyValues.StartDate
			};

			var (endDateSuccessfullyParsed, endDateTime) = listItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.EndDate);
			if (endDateSuccessfullyParsed)
			{
				refcusCodeList.ZZD_EndDate = endDateTime;
			}

			var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
			AddAttribute(refCusCodeListAttributes, NexDocConstants.AttributeNames.AHECCC, listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.AHECCCode), (value) => !string.IsNullOrWhiteSpace(value), true);
			refcusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
			result.Add(refcusCodeList);
		}

		protected override IEnumerable<IListCodeSet> GetFilteredCodeSetList()
		{
			var codeSetList = base.GetFilteredCodeSetList();

			var result = codeSetList.GroupBy(x => x.Items.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CategoryCodeFull))
				.Select(x =>
				{
					var listCodeSet = x.FirstOrDefault(y => string.IsNullOrEmpty(y.Items.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.EndDate)));
					if (listCodeSet == null)
					{
						listCodeSet = x.OrderByDescending(y => y.Items.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.EndDate)).FirstOrDefault();
					}
					return listCodeSet;
				}).ToArray();

			return result;
		}

		void AppendProductCategoryErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			var listItemCodeSet = listCodeSet.Items;
			ErrorBuilder.AppendLine("Product Category AHECC Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Category Code: {listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CategoryCodeFull)}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"AHECC Code: {keyValues.SecondaryCode}");
		}

		class ProductCategoryAHECCKeyValues : IKeyValues
		{
			public ProductCategoryAHECCKeyValues(IItemCodeSet[] listItemCodeSet)
			{
				ListItemCodeSet = listItemCodeSet;
				StartDateDetails = ListItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.StartDate);
			}
			readonly IItemCodeSet[] ListItemCodeSet;
			readonly (bool SuccessfullyParsed, DateTime DateTime) StartDateDetails;

			public string Code => code ?? (code = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CategoryCodeFull));
			string code;

			public string SecondaryCode => secondaryCode ?? (secondaryCode = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.AHECCCode));
			string secondaryCode;

			public string Description => Code;

			public bool IsStartDateSuccesfullyParsed => StartDateDetails.SuccessfullyParsed;

			public DateTime StartDate => StartDateDetails.DateTime;

			public string UniqueCode => Code;
		}
	}
}
