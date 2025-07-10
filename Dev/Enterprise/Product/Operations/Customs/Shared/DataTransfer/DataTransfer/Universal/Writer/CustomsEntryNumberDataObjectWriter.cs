using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryNumberDataObjectWriter : DataObjectWriter<CusEntryNumber, UniversalCustoms.EntryNumber>
	{
		public CustomsEntryNumberDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "UniversalDataObjectWriterHelper helper");
		}

		protected readonly UniversalDataObjectWriterHelper helper;

		protected override UniversalCustoms.EntryNumber PopulateDataObject(CusEntryNumber entryNumberBO)
		{
			return new UniversalCustoms.EntryNumber()
			{
				EntryIsSystemGenerated = entryNumberBO.CE_EntryIsSystemGenerated,
				//EntryLineReference = entryNumberRow.CE_EntryLineReference;// speak to Ben before adding this back
				Number = entryNumberBO.CE_EntryNum,
				Type = ListHelper.GetWithDescription<EntryType>(entryNumberBO.CE_EntryType, entryNumberBO.Lookups.AdditionalReferenceNumberTypes),
			};
		}
	}
}
