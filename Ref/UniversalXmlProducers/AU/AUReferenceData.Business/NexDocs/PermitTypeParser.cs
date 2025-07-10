using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class PermitTypeParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public PermitTypeParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfiguration(NexDocConstants.CodeTypes.PermitType);

		protected override string XMLWriterDataSource => "NexDoc Permit Types";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_PERMIT_TYPE.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendDuplicateDetails(ErrorBuilder, "Permit Type", keyValues, listCodeSet);

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendErrorDetails(ErrorBuilder, "Permit Type", keyValues, listCodeSet);

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new BaseKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeDescriptionAndStartDateIsValid(keyValues);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet) => result.Add(Helper.BaseRefCusCodeList(keyValues, listItemCodeSet));
	}
}
