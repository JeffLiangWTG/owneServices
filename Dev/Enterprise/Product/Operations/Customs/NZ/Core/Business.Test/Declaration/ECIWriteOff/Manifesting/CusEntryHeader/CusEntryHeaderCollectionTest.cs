using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing
{
	[TestedType(typeof(CusEntryHeaderCollection))]
	public class CusEntryHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryHeaderCollection(Factory);
		}

		public void TestAdditionalFilter()
		{
			CusEntryHeader manifestEntryHeader = Factory.New<CusEntryHeader>();
			ECIWriteOff.CusEntryHeader nonManifestEntryHeader = Factory.New<ECIWriteOff.CusEntryHeader>();
			CusEntryHeaderCollection entryHeaders = new CusEntryHeaderCollection(Factory);
			entryHeaders.Load();
			AssertEquals("EntryHeaders.Count", 1, entryHeaders.Count);
			AssertEquals("EntryHeaders[0].PK", manifestEntryHeader.PK, entryHeaders[0].PK);
		}
	}
}
