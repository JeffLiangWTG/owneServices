using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(ABLEntryNum))]
	class ABLEntryNumTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableNameCore()
		{
			var entryNum = (ABLEntryNum)GetNewBusinessObject();
			AssertEquals("Local Reference No / LRN", entryNum.CE_EntryNumInfo.HumanReadableName);
			AssertEquals("LRN Type", entryNum.CE_EntryTypeInfo.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryNum = "LRN1234";
			return entryNum;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();
	}
}
