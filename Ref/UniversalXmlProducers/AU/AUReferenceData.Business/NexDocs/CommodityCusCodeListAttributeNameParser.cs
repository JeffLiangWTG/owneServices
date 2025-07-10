using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CommodityCusCodeListAttributeNameParser : BaseNexDocCodeParser<RefCusCodeListAttributeName>
	{
		public CommodityCusCodeListAttributeNameParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListAttributeNameWriterConfiguration();

		protected override string XMLWriterDataSource => "NexDoc Commodity RefCusCodeListAttributeNames";

		protected override string OutPutFileName => "RefCusCodeTypeZZ_AU_COMMODITY_REFCUSCODELISTATTRIBUTENAME.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendDuplicateDetails(ErrorBuilder, "Commodity Type", keyValues, listCodeSet);

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendErrorDetails(ErrorBuilder, "Commodity Type",keyValues, listCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeDescriptionAndStartDateIsValid(keyValues);

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new BaseKeyValues(listItemCodeSet);

		protected override void AddToRefList(List<RefCusCodeListAttributeName> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			result.Add(new RefCusCodeListAttributeName()
			{
				ZXE_Description = "PRODUCT TYPE APPLICABLE FOR " + keyValues.Description,
				ZXE_IsMandatory = true,
				ZXE_IsValueMandatory = true,
				ZXE_ZZK_NKCodeType = NexDocConstants.CodeTypes.ProductCategoryPrefix + keyValues.Code,
				ZXE_ZZK_NKCodeTypeForValueList = NexDocConstants.CodeTypes.ProductTypePrefix + keyValues.Code
			});
		}
	}
}
