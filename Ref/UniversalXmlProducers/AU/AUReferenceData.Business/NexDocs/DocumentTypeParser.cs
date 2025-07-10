using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class DocumentTypeParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public DocumentTypeParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfiguration(NexDocConstants.CodeTypes.DocumentType);

		protected override string XMLWriterDataSource => "NexDoc Document Types";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_DOCUMENT_TYPE.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendDuplicateDetails(ErrorBuilder, "Document Type", keyValues, listCodeSet);

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendErrorDetails(ErrorBuilder, "Document Type", keyValues, listCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeDescriptionAndStartDateIsValid(keyValues);

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new BaseKeyValues(listItemCodeSet);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet) => result.Add(Helper.BaseRefCusCodeList(keyValues, listItemCodeSet));
	}
}
