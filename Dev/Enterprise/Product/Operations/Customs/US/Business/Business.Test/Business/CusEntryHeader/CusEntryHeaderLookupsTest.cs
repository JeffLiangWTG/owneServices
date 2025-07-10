using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeader()
		{
			CusEntryHeader parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Lookups.EntryHeader, parent);
		}

		public void TestMessageStatusList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals(typeof(ImportMessageStatusList), entry.Lookups.MessageStatusList.GetType());
			entry.CH_Status = ImportMessageStatusList.Codes.ErrorDepartureWithdraw;
			AssertEquals(ImportMessageStatusList.Descriptions.ErrorDepartureWithdraw, entry.MessageStatusDescription);
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(typeof(ImportMessageStatusList), entry.Lookups.MessageStatusList.GetType());
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals(typeof(AESDirectCustomsEntryStatus), entry.Lookups.MessageStatusList.GetType());
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			entry = reconDeclaration.ReconEntry.GetEntry();
			AssertEquals(typeof(ReconMessageStatusList), entry.Lookups.MessageStatusList.GetType());
		}
	}
}
