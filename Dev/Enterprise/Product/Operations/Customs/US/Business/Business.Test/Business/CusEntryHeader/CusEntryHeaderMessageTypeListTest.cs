using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryHeaderMessageTypeListTest : TestCaseWithFactory
	{
		public void TestGetMessagesTypesRightFor()
		{
			AssertEquals(ImportMessageStatusList.MessageType.BorderCargoRelease, CusEntryHeaderMessageTypeList.GetMessagesTypesRightFor(CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease));
			AssertEquals(ImportMessageStatusList.MessageType.Export, CusEntryHeaderMessageTypeList.GetMessagesTypesRightFor(CusEntryHeaderMessageTypeList.Codes.Export));
			AssertEquals(ImportMessageStatusList.MessageType.CargoRelease, CusEntryHeaderMessageTypeList.GetMessagesTypesRightFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease));
			AssertEquals(ImportMessageStatusList.MessageType.EntrySummary, CusEntryHeaderMessageTypeList.GetMessagesTypesRightFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			AssertEquals(ImportMessageStatusList.MessageType.InBondDeparture, CusEntryHeaderMessageTypeList.GetMessagesTypesRightFor(CusEntryHeaderMessageTypeList.Codes.InBond));
			AssertEquals(ImportMessageStatusList.MessageType.ReconEntry, CusEntryHeaderMessageTypeList.GetMessagesTypesRightFor(CusEntryHeaderMessageTypeList.Codes.ReconEntry));
			AssertEquals(ImportMessageStatusList.MessageType.ACECargoRelease, CusEntryHeaderMessageTypeList.GetMessagesTypesRightFor(CusEntryHeaderMessageTypeList.Codes.ACECargoRelease));
		}
	}
}
