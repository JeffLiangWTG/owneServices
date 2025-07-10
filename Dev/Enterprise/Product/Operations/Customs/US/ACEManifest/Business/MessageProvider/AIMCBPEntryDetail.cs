using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	class AIMCBPEntryDetail : IAIMCBPEntryDetail
	{
		public AIMCBPEntryDetail(AsycudaBill bill)
		{
			entryType = bill.CustomsEntryNumberType;
			entryNumber = bill.CustomsEntryNumber;
		}

		readonly string entryType;
		readonly string entryNumber;

		ZString IAIMCBPEntryDetail.EntryType => entryType;

		ZString IAIMCBPEntryDetail.EntryNumber => entryNumber;
	}
}
