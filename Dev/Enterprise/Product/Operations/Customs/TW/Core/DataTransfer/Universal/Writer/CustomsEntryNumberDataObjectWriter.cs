using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class CustomsEntryNumberDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectWriter
	{
		public CustomsEntryNumberDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override UniversalCustoms.EntryNumber PopulateDataObject(CusEntryNumber entryNumberBO)
		{
			var entryNumber = base.PopulateDataObject(entryNumberBO);
			entryNumber.EntryStatus = new EntryStatus { Code = entryNumberBO.CE_EntryStatus, Description = entryNumberBO.EntryStatusDescription };
			entryNumber.IssueDate = entryNumberBO.CE_IssueDate;
			return entryNumber;
		}
	}
}
