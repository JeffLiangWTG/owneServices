using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class MergeFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestConstructor_ChecksForActiveCollection()
		{
			var activeCollection = new ActiveBusinessObjectCollection<DummyHeader>(Factory);
			var adHocCollection = new ActiveBusinessObjectCollection<DummyHeader>(Factory, new AdhocCollectionRelationship(typeof(DummyHeader)));

			AssertExceptionThrown<ArgumentException>(() => new SimpleDummyMergeFlattenedDataTransferProcessor(activeCollection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory))));
			AssertNoExceptionThrown(() => new SimpleDummyMergeFlattenedDataTransferProcessor(adHocCollection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory))));
		}

		public void TestImport()
		{
			var headerCollection = new DummyHeaderCollection(Factory);
			var flattenedCollection = new DummyFlattenedCollection(Factory);

			var flattened1a = flattenedCollection.AddNew();
			flattened1a.FlatHeaderString = "hello";
			flattened1a.FlatChild1Int = 1;
			flattened1a.FlatChild2Int = 2;
			var flattened1b = flattenedCollection.AddNew();
			flattened1b.FlatHeaderString = "hello";
			flattened1b.FlatChild1Int = 2;
			flattened1b.FlatChild2Int = flattened1a.FlatChild2Int;
			var flattened1c = flattenedCollection.AddNew();
			flattened1c.FlatHeaderString = "hello";
			flattened1c.FlatChild1Int = 9;
			flattened1c.FlatChild2Int = flattened1a.FlatChild2Int;

			var flattened2 = flattenedCollection.AddNew();
			flattened2.FlatHeaderString = "hello";
			flattened2.FlatHeaderDate = new ZDate(2012, 1, 1);
			flattened2.FlatChild1Int = 1;
			flattened2.FlatChild1Date = new ZDate(2011, 11, 11);

			var flattened3 = flattenedCollection.AddNew();
			flattened3.FlatHeaderString = "hello";

			var flattened4a = flattenedCollection.AddNew();
			flattened4a.FlatHeaderString = "Invalid 1";
			flattened4a.FlatChild1Int = 1;
			flattened4a.FlatChild2Int = 2;
			var flattened4b = flattenedCollection.AddNew();
			flattened4b.FlatHeaderString = "Invalid 1";
			flattened4b.FlatChild1Int = flattened4a.FlatChild1Int;
			flattened4b.FlatChild2Int = 6;

			var flattened5a = flattenedCollection.AddNew();
			flattened5a.FlatHeaderString = "Invalid 2";
			flattened5a.FlatChild1Int = 1;
			flattened5a.FlatChild2Int = 1;

			var child1Merger = new DummyChildType1Merger();
			var child2Merger = new DummyChildType2Merger();
			var processor = new DummyMergeFlattenedDataTransferProcessor_ThatExcludesFlatHeaderStringsContainingInvalid(headerCollection, new ImportCollectionInfoForDummyFlattened(flattenedCollection), child1Merger, child2Merger);
			processor.Import();

			AssertEquals(5, processor.HeadersToCreate);
			AssertEquals(3, processor.HeadersCreated);
			AssertEquals("Only 3 headers should be created from the flattened values", 3, headerCollection.Count);

			AssertEquals(2, processor.HeadersExcluded);
			AssertContains("FlattenedRecord [FlatHeaderString: Invalid 1] excluded: HeaderZString can not contain 'Invalid'", processor.Log);
			AssertContains("FlattenedRecord [FlatHeaderString: Invalid 2] excluded: HeaderZString can not contain 'Invalid'", processor.Log);
			AssertEquals("flatetened 4a,4b childrenType1 should be excluded", 2, child1Merger.ChildRecordsExcluded);
			AssertEquals("flatetened 4a,4b childrenType2 should be excluded", 3, child2Merger.ChildRecordsExcluded);

			var header1 = headerCollection[0];
			AssertEquals("Three childrenType1 created from flattened 1a,1b,1c", 3, header1.ChildrenType1.Count);
			AssertEquals(1, header1.ChildrenType1[0].ChildPositiveInt);
			AssertEquals(2, header1.ChildrenType1[1].ChildPositiveInt);
			AssertEquals(9, header1.ChildrenType1[2].ChildPositiveInt);
			AssertEquals("One childType2 created from flattened 1a (1b,1c are duplicates)", 1, header1.ChildrenType2.Count);
			AssertEquals(2, header1.ChildrenType2[0].ChildPositiveInt);

			var header2 = headerCollection[1];
			AssertEquals("One childType1 created from flattened 2", 1, header2.ChildrenType1.Count);
			AssertEquals(1, header2.ChildrenType1[0].ChildPositiveInt);
			AssertEquals(new ZDate(2011, 11, 11), header2.ChildrenType1[0].ChildZDate);

			var header3 = headerCollection[2];
			AssertEquals("No childrenType1 created from flattened3", 0, header3.ChildrenType1.Count);
			AssertEquals("No childrenType2 created from flattened3", 0, header3.ChildrenType2.Count);

			processor.Rollback();
			AssertEquals(0, headerCollection.Count);
		}

		public void TestImport_WithMergersThatModifyRecordsDuringProcessing()
		{
			var headerCollection = new DummyHeaderCollection(Factory);
			var flattenedCollection = new DummyFlattenedCollection(Factory);
			var flattened = flattenedCollection.AddNew();
			flattened.FlatHeaderString = "hello";
			flattened.FlatChild1Int = 5;
			flattened.FlatChild2Int = 99;

			var flattened_duplicate = flattenedCollection.AddNew();
			flattened_duplicate.FlatHeaderString = "hello";
			flattened_duplicate.FlatChild1Int = 5;
			flattened_duplicate.FlatChild2Int = 99;

			var child1Merger = new DummyChildType1Merger_ThatSetsFlatChild1IntToZero();
			var child2Merger = new DummyChildType2Merger_ThatSetsFlatChild2IntToOne();
			var processor = new DummyMergeFlattenedDataTransferProcessor_ThatCapitalisesFlatHeaderString(headerCollection, new ImportCollectionInfoForDummyFlattened(flattenedCollection), child1Merger, child2Merger);
			processor.Import();

			AssertEquals(1, processor.HeadersToCreate);
			AssertEquals(1, processor.HeadersCreated);
			AssertEquals(0, processor.HeadersExcluded);
			AssertEquals("Only 1 header should be created", 1, headerCollection.Count);
			AssertEquals("Header stirng should have been capitalised", "HELLO", headerCollection[0].HeaderString);

			AssertEquals(1, child1Merger.ChildRecordsCreated);
			AssertEquals(1, child1Merger.DuplicateChildRecordsFound);
			AssertEquals(0, child1Merger.ChildRecordsExcluded);
			AssertEquals("Only 1 childrenType1 should be created", 1, headerCollection[0].ChildrenType1.Count);
			AssertEquals("Int should have been set to zero", 0, headerCollection[0].ChildrenType1[0].ChildPositiveInt);

			AssertEquals(1, child2Merger.ChildRecordsCreated);
			AssertEquals(1, child2Merger.DuplicateChildRecordsFound);
			AssertEquals(0, child2Merger.ChildRecordsExcluded);
			AssertEquals("Only 1 childrenType2 should be created", 1, headerCollection[0].ChildrenType2.Count);
			AssertEquals("Int should have been set to one", 1, headerCollection[0].ChildrenType2[0].ChildPositiveInt);
		}

		public void TestImport_WithFlattenedCollectionCountChangedShouldReport()
		{
			var headerCollection = new DummyHeaderCollection(Factory);
			var flattenedCollection = new DummyFlattenedCollection(Factory);
			flattenedCollection.AddNew();
			flattenedCollection.AddNew();

			var processor = new DummyMergeFlattenedDataTransferProcessor_ThatChangesFlattenedCollectionCountDuringImport(headerCollection, new ImportCollectionInfoForDummyFlattened(flattenedCollection), true);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(2, flattenedCollection.Count);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			});

			processor.Import();
			CombineAssertions("collection count change is captured during Import.", () =>
			{
				AssertEquals(1, flattenedCollection.Count);
				AssertEquals("Collection should not be changed. original count = 2, current count = 1", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestImport_WithFlattenedCollectionCountUnchangedShouldNotReport()
		{
			var headerCollection = new DummyHeaderCollection(Factory);
			var flattenedCollection = new DummyFlattenedCollection(Factory);
			flattenedCollection.AddNew();
			flattenedCollection.AddNew();

			var processor = new DummyMergeFlattenedDataTransferProcessor_ThatChangesFlattenedCollectionCountDuringImport(headerCollection, new ImportCollectionInfoForDummyFlattened(flattenedCollection), false);
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(2, flattenedCollection.Count);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			});

			processor.Import();
			CombineAssertions("collection count unchange is not captured during Import.", () =>
			{
				AssertEquals(2, flattenedCollection.Count);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			});
		}

		abstract class DummyMergeFlattenedDataTransferProcessorBase : MergeFlattenedDataTransferProcessor<DummyHeader, DummyFlattened>
		{
			public DummyMergeFlattenedDataTransferProcessorBase(IBusinessObjectCollection headerCollection, ImportCollectionInfoForDummyFlattened importCollectionInfo, params FlattenedToUniqueChildrenMerger<DummyHeader, DummyFlattened>[] uniqueChildrenMergers)
				: base(headerCollection, importCollectionInfo)
			{
				foreach (var merger in uniqueChildrenMergers)
				{
					AddUniqueChildrenMerger(merger);
				}
			}

			protected override IEnumerable<string> HeaderColumnsOnFlattened
			{
				get
				{
					yield return DummyFlattened.Schema.FlatHeaderString;
					yield return DummyFlattened.Schema.FlatHeaderDate;
				}
			}

			protected override string GetProgressChangedStatus(int recordsProcessed)
			{
				return string.Format("Records processed: {0}", recordsProcessed);
			}
		}

		class SimpleDummyMergeFlattenedDataTransferProcessor : DummyMergeFlattenedDataTransferProcessorBase
		{
			public SimpleDummyMergeFlattenedDataTransferProcessor(IBusinessObjectCollection headerCollection, ImportCollectionInfoForDummyFlattened importCollectionInfo)
				: base(headerCollection, importCollectionInfo)
			{
			}

			protected override DummyHeader CreateHeader(IBusinessObjectCollection headerCollection, DummyFlattened flattenedRecord) => null;
		}

		class DummyMergeFlattenedDataTransferProcessor_ThatChangesFlattenedCollectionCountDuringImport : DummyMergeFlattenedDataTransferProcessorBase
		{
			readonly bool removeCollectionItem;

			public DummyMergeFlattenedDataTransferProcessor_ThatChangesFlattenedCollectionCountDuringImport(IBusinessObjectCollection headerCollection, ImportCollectionInfoForDummyFlattened importCollectionInfo, bool removeCollectionItem)
				: base(headerCollection, importCollectionInfo)
			{
				this.removeCollectionItem = removeCollectionItem;
			}

			protected override DummyHeader CreateHeader(IBusinessObjectCollection headerCollection, DummyFlattened flattenedRecord) => null;

			protected override void ImportCore()
			{
				if (removeCollectionItem)
				{
					flattenedCollection.RemoveAt(0);
				}
			}
		}

		class DummyMergeFlattenedDataTransferProcessor_ThatExcludesFlatHeaderStringsContainingInvalid : DummyMergeFlattenedDataTransferProcessorBase
		{
			public DummyMergeFlattenedDataTransferProcessor_ThatExcludesFlatHeaderStringsContainingInvalid(DummyHeaderCollection headerCollection, ImportCollectionInfoForDummyFlattened importCollectionInfo, params FlattenedToUniqueChildrenMerger<DummyHeader, DummyFlattened>[] uniqueChildrenMergers)
				: base(headerCollection, importCollectionInfo, uniqueChildrenMergers)
			{
			}

			protected override DummyHeader CreateHeader(IBusinessObjectCollection headerCollection, DummyFlattened flattenedRecord)
			{
				if (!flattenedRecord.FlatHeaderString.Contains("Invalid"))
				{
					var header = (DummyHeader)headerCollection.AddNew();
					header.HeaderString = flattenedRecord.FlatHeaderString;
					header.HeaderDate = flattenedRecord.FlatHeaderDate;
					return header;
				}

				Log += string.Format("FlattenedRecord [FlatHeaderString: {0}] excluded: HeaderZString can not contain 'Invalid'\r\n", flattenedRecord.FlatHeaderString);
				return null;
			}
		}

		class DummyMergeFlattenedDataTransferProcessor_ThatCapitalisesFlatHeaderString : DummyMergeFlattenedDataTransferProcessorBase
		{
			public DummyMergeFlattenedDataTransferProcessor_ThatCapitalisesFlatHeaderString(DummyHeaderCollection headerCollection, ImportCollectionInfoForDummyFlattened importCollectionInfo, params FlattenedToUniqueChildrenMerger<DummyHeader, DummyFlattened>[] uniqueChildrenMergers)
				: base(headerCollection, importCollectionInfo, uniqueChildrenMergers)
			{
			}

			protected override DummyHeader CreateHeader(IBusinessObjectCollection headerCollection, DummyFlattened flattenedRecord)
			{
				flattenedRecord.FlatHeaderString = flattenedRecord.FlatHeaderString.ToUpper();
				var header = (DummyHeader)headerCollection.AddNew();
				header.HeaderString = flattenedRecord.FlatHeaderString;
				header.HeaderDate = flattenedRecord.FlatHeaderDate;
				return header;
			}
		}
	}
}
