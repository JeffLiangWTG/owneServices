using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class FlattenedToChildRecordsMergerTest : TestCaseWithFactory
	{
		public void TestCreateChildRecordIfHaventPreviously()
		{
			var header1 = new DummyHeader(Factory);
			var merger = new DummyChildType1Merger();
			var importCollectionInfo = new ImportCollectionInfoForDummyFlattened(null);
			AssertEquals("Precondition:", 0, header1.ChildrenType1.Count);

			var flattened1 = new DummyFlattened(Factory);
			flattened1.FlatChild1Int = 99;
			flattened1.FlatChild1Date = new ZDate(2010, 10, 10);
			merger.CreateChildRecordIfUnique(header1, flattened1, new ReadonlyImportValuesDictionary(flattened1, importCollectionInfo));

			AssertEquals("Child should be created", 1, merger.ChildRecordsFound);
			AssertEquals("Child should be created", 1, merger.ChildRecordsCreated);
			AssertEquals("Child should be created", 0, merger.DuplicateChildRecordsFound);
			AssertEquals("Child should be created", 0, merger.ChildRecordsExcluded);

			AssertEquals(1, header1.ChildrenType1.Count);
			AssertEquals(99, header1.ChildrenType1[0].ChildPositiveInt);
			AssertEquals(new ZDate(2010, 10, 10), header1.ChildrenType1[0].ChildZDate);

			var flattened2 = new DummyFlattened(Factory);
			flattened2.FlatChild1Int = 44;
			flattened2.FlatChild1Date = new ZDate(2010, 10, 10);
			merger.CreateChildRecordIfUnique(header1, flattened2, new ReadonlyImportValuesDictionary(flattened2, importCollectionInfo));

			AssertEquals("Another child should be created", 2, merger.ChildRecordsFound);
			AssertEquals("Another child should be created", 2, merger.ChildRecordsCreated);
			AssertEquals("Another child should be created", 0, merger.DuplicateChildRecordsFound);
			AssertEquals("Another child should be created", 0, merger.ChildRecordsExcluded);

			AssertEquals(2, header1.ChildrenType1.Count);
			AssertEquals(44, header1.ChildrenType1[1].ChildPositiveInt);
			AssertEquals(new ZDate(2010, 10, 10), header1.ChildrenType1[1].ChildZDate);

			var flattened3 = new DummyFlattened(Factory);
			flattened3.FlatChild1Int = -1;
			flattened3.FlatChild1Date = new ZDate(2010, 10, 10);
			merger.CreateChildRecordIfUnique(header1, flattened3, new ReadonlyImportValuesDictionary(flattened3, importCollectionInfo));

			AssertEquals("Should exclude child because FlatChild1Int is not positive", 3, merger.ChildRecordsFound);
			AssertEquals("Should exclude child because FlatChild1Int is not positive", 2, merger.ChildRecordsCreated);
			AssertEquals("Should exclude child because FlatChild1Int is not positive", 0, merger.DuplicateChildRecordsFound);
			AssertEquals("Should exclude child because FlatChild1Int is not positive", 1, merger.ChildRecordsExcluded);

			var flattened4 = new DummyFlattened(Factory);
			flattened4.FlatChild1Int = 99;
			flattened4.FlatChild1Date = new ZDate(2010, 10, 10);
			merger.CreateChildRecordIfUnique(header1, flattened4, new ReadonlyImportValuesDictionary(flattened4, importCollectionInfo));

			AssertEquals("Should not create child because already added one with same values for header1", 4, merger.ChildRecordsFound);
			AssertEquals("Should not create child because already added one with same values for header1", 2, merger.ChildRecordsCreated);
			AssertEquals("Should not create child because already added one with same values for header1", 1, merger.DuplicateChildRecordsFound);
			AssertEquals("Should not create child because already added one with same values for header1", 1, merger.ChildRecordsExcluded);

			var header2 = new DummyHeader(Factory);
			merger.NotifiyParentChanged();
			merger.CreateChildRecordIfUnique(header2, flattened4, new ReadonlyImportValuesDictionary(flattened4, importCollectionInfo));
			AssertEquals("Should create child because creating on new header2", 5, merger.ChildRecordsFound);
			AssertEquals("Should create child because creating on new header2", 3, merger.ChildRecordsCreated);
			AssertEquals("Should create child because creating on new header2", 1, merger.DuplicateChildRecordsFound);
			AssertEquals("Should create child because creating on new header2", 1, merger.ChildRecordsExcluded);
		}
	}
}
