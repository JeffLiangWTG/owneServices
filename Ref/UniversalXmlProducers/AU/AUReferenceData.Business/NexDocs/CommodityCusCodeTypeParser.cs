using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CommodityCusCodeTypeParser : BaseNexDocCodeParser<RefCusCodeType>
	{
		public CommodityCusCodeTypeParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeTypeWriterConfiguration();

		protected override string XMLWriterDataSource => "NexDoc Commodity RefCusCodeTypes";

		protected override string OutPutFileName => "RefCusCodeTypeZZ_AU_COMMODITY_REFCUSCODETYPE.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendDuplicateDetails(ErrorBuilder, "Commodity Type", keyValues, listCodeSet);

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendErrorDetails(ErrorBuilder, "Commodity Type", keyValues, listCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeDescriptionAndStartDateIsValid(keyValues);

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new BaseKeyValues(listItemCodeSet);

		protected override void AddToRefList(List<RefCusCodeType> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			result.Add(new RefCusCodeType()
			{
				ZZK_CodeType = NexDocConstants.CodeTypes.ProductTypePrefix + keyValues.Code,
				ZZK_Description = "NEXDOCS PRODUCT TYPE " + keyValues.Description
			});
			result.Add(new RefCusCodeType()
			{
				ZZK_CodeType = NexDocConstants.CodeTypes.ProductCategoryPrefix + keyValues.Code,
				ZZK_Description = "NEXDOCS PRODUCT CATEGORY " + keyValues.Description
			});
		}
	}
}
