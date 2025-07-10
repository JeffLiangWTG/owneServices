using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(CusEntryNumCollection))]
	public class CusEntryNumCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryNumCollection(Factory.New<USExportAsycudaBill>(), string.Empty, string.Empty);
		}

		public void TestRefreshCollection()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var aesITNNumbers = new CusEntryNumCollection(bill, CusEntryNumberTypes.UnitedStates.ITN, string.Empty);
			var entryNumber1 = aesITNNumbers.AddNew();
			entryNumber1.CE_EntryNum = "001";
			var entryNumber2 = aesITNNumbers.AddNew();
			entryNumber2.CE_EntryNum = "002";
			aesITNNumbers.RefreshCollection("001,111,222");
			AssertEquals(3, aesITNNumbers.Count);
			AssertEquals("001,111,222", aesITNNumbers.GetCodesAsCommaSeparatedString());
		}

		public void TestGetCodesAsCommaSeparatedString()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.Parent = bill;
			entryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
			entryNumber1.CE_EntryNum = "001";
			var entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.Parent = bill;
			entryNumber2.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
			entryNumber2.CE_EntryNum = "002";
			var entryNumber3 = Factory.New<CusEntryNumber>();
			entryNumber3.Parent = bill;
			entryNumber3.CE_EntryType = CusEntryNumberTypes.UnitedStates.InBond;
			entryNumber3.CE_EntryNum = "003";
			Factory.Save();

			var aesITNNumbers = new CusEntryNumCollection(bill, CusEntryNumberTypes.UnitedStates.ITN, string.Empty);
			aesITNNumbers.Load();
			AssertEquals("001,002",aesITNNumbers.GetCodesAsCommaSeparatedString());

			var inBondNumbers = new CusEntryNumCollection(bill, CusEntryNumberTypes.UnitedStates.InBond, string.Empty);
			inBondNumbers.Load();
			AssertEquals("003", inBondNumbers.GetCodesAsCommaSeparatedString());
		}
	}
}
