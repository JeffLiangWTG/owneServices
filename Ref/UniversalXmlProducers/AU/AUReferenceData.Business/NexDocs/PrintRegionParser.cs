using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class PrintRegionParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public PrintRegionParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfigurationWithAttributes(NexDocConstants.CodeTypes.PrintRegion, true);

		protected override string XMLWriterDataSource => "NexDoc Print Regions";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_PRINT_REGION.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Duplicate Print Region exists in downloaded List.");
			AppendPrintRegionErrorDetails(keyValues, listCodeSet);
		}

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Unable to import Print Region due to empty Code or Description.");
			AppendPrintRegionErrorDetails(keyValues, listCodeSet);
		}

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new PrintRegionKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeAndDescriptionIsValid(keyValues);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			var code = keyValues.Code;
			var codeList = result.FirstOrDefault(c => c.ZZD_Code.Equals(code, StringComparison.OrdinalIgnoreCase));

			if (codeList == null)
			{
				codeList = new RefCusCodeList()
				{
					ZZD_Code = code,
					ZZD_Description = keyValues.Description
				};

				result.Add(codeList);
			}

			var refCusCodeListAttributes = codeList.RefCusCodeListAttributes?.ToList() ?? new List<RefCusCodeListAttribute>();
			AddAttribute(refCusCodeListAttributes, NexDocConstants.AttributeNames.CommodityTypeCode, listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CommodityTypeCode), (value) => !string.IsNullOrWhiteSpace(value), true);

			codeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
		}

		void AppendPrintRegionErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			var listItemCodeSet = listCodeSet.Items;
			ErrorBuilder.AppendLine("Print Region Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {keyValues.Code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {keyValues.Description}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Commodity Type: {listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CommodityTypeCode)}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Updated Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.UpdatedDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
		}

		sealed class PrintRegionKeyValues : IKeyValues
		{
			public PrintRegionKeyValues(IItemCodeSet[] listItemCodeSet)
			{
				this.listItemCodeSet = listItemCodeSet;
			}

			readonly IItemCodeSet[] listItemCodeSet;

			string IKeyValues.Code => code ?? (code = listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code)?.ToUpperInvariant() ?? string.Empty);
			string code;

			string IKeyValues.Description => description ?? (description = listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Description)?.ToUpperInvariant() ?? string.Empty);
			string description;

			string IKeyValues.SecondaryCode => string.Empty;

			bool IKeyValues.IsStartDateSuccesfullyParsed => false;

			DateTime IKeyValues.StartDate => DateTime.MinValue;

			string IKeyValues.UniqueCode
			{
				get
				{
					if (string.IsNullOrWhiteSpace(uniqueCode))
					{
						var codes = new[]
						{
							listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code)?.ToUpperInvariant() ?? string.Empty,
							listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CommodityTypeCode)?.ToUpperInvariant() ?? string.Empty,
						};

						uniqueCode = string.Join(Separator, codes);
					}

					return uniqueCode;
				}
			}
			string uniqueCode;
		}

		const string Separator = "|";
	}
}
