using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using CargoWise.Integration;
	using CargoWise.Types;

	public class AvailableEDocListTest : TestCaseWithFactory
	{
		public void TestGetFileNameFromPK()
		{
			var eDoc1 = new eDocForTesting("", "File1.PDF", "Landed Fish", new ZDateTime(2008, 12, 13));
			var eDoc2 = new eDocForTesting("C", "File2.PDF", "Chocolate I", new ZDateTime(2008, 12, 15));
			var eDoc3 = new eDocForTesting("DDD", "File3.PDF", "Crab Sticks", new ZDateTime(2008, 12, 16));
			var eDocs = new eDocsCollectionForTesting(eDoc1, eDoc2, eDoc3);

			var list = new AvailableEDocList(eDocs);

			AssertEquals("Precondition: list.Count", 3, list.Count);
			AssertEquals("list.GetFileNameFromPK(eDoc1.UniqueKey)", "File1.PDF", list.GetFileNameFromPK(eDoc1.UniqueKey));
			AssertEquals("list.GetFileNameFromPK(eDoc2.UniqueKey)", "File2.PDF", list.GetFileNameFromPK(eDoc2.UniqueKey));
			AssertEquals("list.GetFileNameFromPK(eDoc1.UniqueKey)", "File1.PDF", list.GetFileNameFromPK(eDoc1.UniqueKey));
			AssertEquals("list.GetFileNameFromPK(ZGuid.NewZGuid())", "", list.GetFileNameFromPK(ZGuid.NewZGuid()));
			AssertEquals("list.GetFileNameFromPK(ZGuid.Empty)", "", list.GetFileNameFromPK(ZGuid.Empty));
		}

		public void TestList()
		{
			var eDoc1 = new eDocForTesting("AAA", "File1.PDF", "Landed Fish", new ZDateTime(2008, 12, 13));
			var eDoc2 = new eDocForTesting("BBB", "File2.XLS", "Sucking Pig", new ZDateTime(2008, 12, 14));
			var eDoc3 = new eDocForTesting("CCC", "File3.PDF", "Chocolate I", new ZDateTime(2008, 12, 15)) { IsDeleted = true };
			var eDoc4 = new eDocForTesting("DDD", "File4.PDF", "Crab Sticks", new ZDateTime(2008, 12, 16));
			var eDoc5 = new eDocForTesting("EEE", "File5.TIF", "TIF eDoc", new ZDateTime(2008, 12, 17));
			var eDoc6 = new eDocForTesting("FFF", "File6.BMP", "BMP eDoc", new ZDateTime(2008, 12, 18));
			var eDoc7 = new eDocForTesting("GGG", "File7.PNG", "PNG eDoc", new ZDateTime(2008, 12, 19));
			var eDoc8 = new eDocForTesting("HHH", "File8.JPG", "JPG eDoc", new ZDateTime(2008, 12, 20));
			var eDoc9 = new eDocForTesting("III", @"InvalidFileName<>.PNG", "PNG eDoc", new ZDateTime(2008, 12, 21));
			var eDocs = new eDocsCollectionForTesting(eDoc1, eDoc2, eDoc3, eDoc4, eDoc5, eDoc6, eDoc7, eDoc8, eDoc9);

			var list = new AvailableEDocList(eDocs);

			var completeList = new ZStringBuilder();
			foreach (ICodeDescription data in list)
			{
				completeList.Append(data.Code + " | " + data.Description);
				AssertNotEquals("element.PK", ZGuid.Empty, data.PK);
			}

			const string expectedCompleteList = @"
AAA-File1.PDF | Added: 13-Dec-08 - Landed Fish
DDD-File4.PDF | Added: 16-Dec-08 - Crab Sticks
EEE-File5.TIF | Added: 17-Dec-08 - TIF eDoc
FFF-File6.BMP | Added: 18-Dec-08 - BMP eDoc
GGG-File7.PNG | Added: 19-Dec-08 - PNG eDoc
HHH-File8.JPG | Added: 20-Dec-08 - JPG eDoc
";
			AssertMultilineASCIIEquals("Complete List", expectedCompleteList.Trim(), completeList.ToStringWithNewLineBetweenAppends());
		}
	}
}
