using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CustomsEntryLineDataObjectReader : Customs.DataTransfer.Universal.CustomsEntryLineDataObjectReader
	{
		public CustomsEntryLineDataObjectReader(EntryLine entryLineDataObject, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, CusEntryHeader entryHeader) : base(entryLineDataObject, logger, helper, entryHeader)
		{
		}

		protected override ZGuid GetEntryNumberParentID() => entryHeader.Declaration.PK;
	}
}
