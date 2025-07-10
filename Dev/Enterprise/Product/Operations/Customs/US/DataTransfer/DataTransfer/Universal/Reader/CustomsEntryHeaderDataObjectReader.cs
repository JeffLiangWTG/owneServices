using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectReader : Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectReader
	{
		public CustomsEntryHeaderDataObjectReader(EntryHeader entryHeaderDataObject, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, BaseJobDeclaration declaration, ZGuid primeEntryPK, List<ZString> matchingKeys = null)
			: base(entryHeaderDataObject, logger, helper, declaration, primeEntryPK, matchingKeys)
		{
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryLineDataObjectReader CreateNewCustomsEntryLineDataObjectReader(EntryLine entryLineDataObject, CusEntryHeader entryHeader)
		{
			return new CustomsEntryLineDataObjectReader(entryLineDataObject, logger, helper, entryHeader);
		}
	}
}
