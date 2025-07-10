using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class AdditionalTextFormatParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public AdditionalTextFormatParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfiguration(NexDocConstants.CodeTypes.AdditionalTextFormat);

		protected override string XMLWriterDataSource => "NexDoc Additional Text Formats";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_ADD_TEXT_FORMAT.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Duplicate Additional Text Format exists in downloaded List.");
			AppendAdditionalTextFormatErrorDetails(keyValues, listCodeSet);
		}

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Unable to import Additional Text Format due to empty Code or Description.");
			AppendAdditionalTextFormatErrorDetails(keyValues, listCodeSet);
		}

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new BaseKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeAndDescriptionIsValid(keyValues);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			result.Add(new RefCusCodeList()
			{
				ZZD_Code = keyValues.Code,
				ZZD_Description = keyValues.Description
			});
		}

		void AppendAdditionalTextFormatErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			var listItemCodeSet = listCodeSet.Items;
			ErrorBuilder.AppendLine($"Additional Text Format Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {keyValues.Code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {keyValues.Description}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Updated Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.UpdatedDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
		}
	}
}
