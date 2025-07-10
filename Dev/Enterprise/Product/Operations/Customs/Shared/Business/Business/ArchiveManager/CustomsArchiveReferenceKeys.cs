using Enterprise.ArchiveManager.Integration;

namespace Enterprise.Customs.Business.ArchiveManager
{
	public static class CustomsArchiveReferenceKeys
	{
		public readonly static ReferenceKeyType CustomsReferenceNumber = new ReferenceKeyType("CUS", ResString.GetMultilingualString("8B1D1DC3-3B93-4ada-BBD9-F1E10A6FA609", "Customs Ref #"));
		public readonly static ReferenceKeyType EDIInterchangeNumber = new ReferenceKeyType("INT", ResString.GetMultilingualString("A14E7B21-0FBD-4bf6-9ADE-0237CEB2285D", "EDI Interchange #"));
		public readonly static ReferenceKeyType EDIMessage = new ReferenceKeyType("MSG", ResString.GetMultilingualString("E0737363-115C-4b37-96AC-199A729EEF84", "EDI Message"));
		public readonly static ReferenceKeyType EDIMessageReceiveTransmit = new ReferenceKeyType("MRT", ResString.GetMultilingualString("A8E43295-3B8B-4848-985B-561DD4FE3F90", "EDI Message RCV|TRX"));
		public readonly static ReferenceKeyType EDIMessageApplicationReference = new ReferenceKeyType("MAR", ResString.GetMultilingualString("B00F40F1-BD0D-46b5-A60E-6E9EF81D00CC", "EDI Message App. Ref"));
		public readonly static ReferenceKeyType EDIMessageNum = new ReferenceKeyType("MNO", ResString.GetMultilingualString("02F06DF9-0BF2-4868-A496-3BA8D8655FCB", "EDI Message #"));
		public readonly static ReferenceKeyType EDIMessageSystemCreateUser = new ReferenceKeyType("MUS", ResString.GetMultilingualString("C4574D00-5C6B-418e-9A7B-6696800D45C4", "EDI Message Create User"));
	}
}
