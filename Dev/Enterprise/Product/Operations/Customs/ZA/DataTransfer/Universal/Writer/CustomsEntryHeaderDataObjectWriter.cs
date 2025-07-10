using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter
	{
		internal CustomsEntryHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryLineDataObjectWriter GetNewCustomsEntryLineDataObjectWriter()
		{
			return new CustomsEntryLineDataObjectWriter(writeManager, helper);
		}
	}
}
