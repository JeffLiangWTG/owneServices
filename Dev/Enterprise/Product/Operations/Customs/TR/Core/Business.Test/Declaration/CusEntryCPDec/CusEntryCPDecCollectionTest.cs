using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryCPDecCollection))]
	class CusEntryCPDecCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			return entryLine.CPDecCollection;
		}
	}

	[TestedType(typeof(CusEntryCPDecCollection))]
	class CusEntryCPDecCollectionForHeaderTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			return entryHeader.CPDecCollection;
		}
	}
}
