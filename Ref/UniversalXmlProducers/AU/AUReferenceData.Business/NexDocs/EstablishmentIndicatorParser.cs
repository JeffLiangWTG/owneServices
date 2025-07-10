using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class EstablishmentIndicatorParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public EstablishmentIndicatorParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfiguration(NexDocConstants.CodeTypes.EstablishmentIndicator);

		protected override string XMLWriterDataSource => "NEXDOC Establishment Indicator";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_ESTABLISHMENT_INDICATOR.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendDuplicateDetails(ErrorBuilder, "Establishment Indicator", keyValues, listCodeSet);

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendErrorDetails(ErrorBuilder, "Establishment Indicator", keyValues, listCodeSet);

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new BaseKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeDescriptionAndStartDateIsValid(keyValues);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet) => result.Add(Helper.BaseRefCusCodeList(keyValues, listItemCodeSet));
	}
}
