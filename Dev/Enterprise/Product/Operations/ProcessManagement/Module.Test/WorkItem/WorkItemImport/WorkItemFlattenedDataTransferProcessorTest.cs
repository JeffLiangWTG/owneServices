using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module.Test
{
	public class WorkItemFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		#region Correct Property Values

		public void TestImport_WithCorrectValues()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "2AA";
			record0.WKI_ActivityType = "3AA";
			record0.WKI_ActivitySubtype = "4AA";
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = "Work Item 0";
			record0.WKI_Details = "Work Item 0 Description";

			var record1 = flattenedCollection.AddNew();
			record1.WKI_WorkItemType = "1BB";
			record1.WKI_WorkItemArea = "2CC";
			record1.WKI_ActivityType = "3S1";
			record1.WKI_ActivitySubtype = "4S1";
			record1.WKI_Priority = "MED";
			record1.WKI_PortOrCountry = "XZALV";
			record1.WKI_Summary = "Work Item 1";
			record1.WKI_Details = "Work Item 1 Description";

			var record2 = flattenedCollection.AddNew();
			record2.WKI_WorkItemType = "1AA";
			record2.WKI_WorkItemArea = "2DD";
			record2.WKI_ActivityType = "3S1";
			record2.WKI_ActivitySubtype = "4S2";
			record2.WKI_Priority = "LOW";
			record2.WKI_PortOrCountry = "XZANT";
			record2.WKI_Summary = "Work Item 2";
			record2.WKI_Details = "Work Item 2 Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 3, processor.NewCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});
			Assert("Logs", !processor.Logs.Any());

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals("Each record should have been imported as a new work item", 3, newWorkItems.Length);

			var workItem0 = newWorkItems.SingleOrDefault(w => w.WKI_Summary == "Work Item 0");
			AssertNotNull(workItem0);
			AssertEquals(workItem0.WKI_WorkItemType, "1AA");
			AssertEquals(workItem0.WKI_WorkItemArea, "2AA");
			AssertEquals(workItem0.WKI_ActivityType, "3AA");
			AssertEquals(workItem0.WKI_ActivitySubtype, "4AA");
			AssertEquals(workItem0.WKI_Priority, "LOW");
			AssertEquals(workItem0.WKI_PortOrCountry, "XZAAD");
			AssertEquals(workItem0.WKI_Details.ToUTF8(), "Work Item 0 Description");

			var workItem1 = newWorkItems.SingleOrDefault(w => w.WKI_Summary == "Work Item 1");
			AssertNotNull(workItem1);
			AssertEquals(workItem1.WKI_WorkItemType, "1BB");
			AssertEquals(workItem1.WKI_WorkItemArea, "2CC");
			AssertEquals(workItem1.WKI_ActivityType, "3S1");
			AssertEquals(workItem1.WKI_ActivitySubtype, "4S1");
			AssertEquals(workItem1.WKI_Priority, "MED");
			AssertEquals(workItem1.WKI_PortOrCountry, "XZALV");
			AssertEquals(workItem1.WKI_Details.ToUTF8(), "Work Item 1 Description");

			var workItem2 = newWorkItems.SingleOrDefault(w => w.WKI_Summary == "Work Item 2");
			AssertNotNull(workItem2);
			AssertEquals(workItem2.WKI_WorkItemType, "1AA");
			AssertEquals(workItem2.WKI_WorkItemArea, "2DD");
			AssertEquals(workItem2.WKI_ActivityType, "3S1");
			AssertEquals(workItem2.WKI_ActivitySubtype, "4S2");
			AssertEquals(workItem2.WKI_Priority, "LOW");
			AssertEquals(workItem2.WKI_PortOrCountry, "XZANT");
			AssertEquals(workItem2.WKI_Details.ToUTF8(), "Work Item 2 Description");
		}

		public void TestImport_WithLeadingAndTrailingSpaces()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "2AA";
			record0.WKI_ActivityType = "3AA";
			record0.WKI_ActivitySubtype = "4AA";
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = " Work Item 0 ";
			record0.WKI_Details = " Work Item 0 Description ";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 1, processor.NewCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});
			Assert("Logs", !processor.Logs.Any());

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals("Each record should have been imported as a new work item", 1, newWorkItems.Length);

			var workItem0 = newWorkItems.SingleOrDefault(w => w.WKI_Summary == "Work Item 0");
			AssertNotNull(workItem0);
			AssertEquals(workItem0.WKI_WorkItemType, "1AA");
			AssertEquals(workItem0.WKI_WorkItemArea, "2AA");
			AssertEquals(workItem0.WKI_ActivityType, "3AA");
			AssertEquals(workItem0.WKI_ActivitySubtype, "4AA");
			AssertEquals(workItem0.WKI_Priority, "LOW");
			AssertEquals(workItem0.WKI_PortOrCountry, "XZAAD");
			AssertEquals(workItem0.WKI_Details.ToUTF8(), "Work Item 0 Description");
		}

		#endregion

		#region Invalid Property Values

		public void TestImport_WithInvalidWorkItemType()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "ZZZ"; // incorrect type
			record0.WKI_WorkItemArea = "2DD";
			record0.WKI_ActivityType = "3S1";
			record0.WKI_ActivitySubtype = "4S2";
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = "Work Item Summary";
			record0.WKI_Details = "Work Item Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid work item type code 'ZZZ'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals(0, newWorkItems.Length);
		}

		public void TestImport_WithInvalidWorkItemArea()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "1BB"; // incorrect area: is applicable to type 1BB only
			record0.WKI_ActivityType = "3S1";
			record0.WKI_ActivitySubtype = "4S2";
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = "Work Item Summary";
			record0.WKI_Details = "Work Item Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid work item area code '1BB'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals(0, newWorkItems.Length);
		}

		public void TestImport_WithInvalidWorkItemActivityType()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "2AA";
			record0.WKI_ActivityType = "3BB"; // incorrect activity type: is applicable to type 1BB and area 2B1 only
			record0.WKI_ActivitySubtype = "ANY";
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = "Work Item Summary";
			record0.WKI_Details = "Work Item Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid work item activity type code '3BB'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals(0, newWorkItems.Length);
		}

		public void TestImport_WithInvalidWorkItemActivitySubType()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "2AA";
			record0.WKI_ActivityType = "3AA";
			record0.WKI_ActivitySubtype = "ZZZ"; // incorrect activity subtype
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = "Work Item Summary";
			record0.WKI_Details = "Work Item Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid work item activity subtype code 'ZZZ'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals(0, newWorkItems.Length);
		}

		public void TestImport_WithInvalidWorkItemPriority()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "2AA";
			record0.WKI_ActivityType = "3AA";
			record0.WKI_ActivitySubtype = "4AA";
			record0.WKI_Priority = "ZZZ"; // incorrect priority
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = "Work Item Summary";
			record0.WKI_Details = "Work Item Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid work item priority 'ZZZ'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals(0, newWorkItems.Length);
		}

		public void TestImport_WithInvalidWorkItemPortOrCountry()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "2AA";
			record0.WKI_ActivityType = "3AA";
			record0.WKI_ActivitySubtype = "4AA";
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "ZZZ"; // incorrect port
			record0.WKI_Summary = "Work Item Summary";
			record0.WKI_Details = "Work Item Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid work item port or country/region 'ZZZ'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals(0, newWorkItems.Length);
		}

		#endregion

		#region Missing Property Values

		public void TestImport_WithPropertiesThatCanBeLegallyEmpty()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "";
			record0.WKI_WorkItemArea = "";
			record0.WKI_ActivityType = "";
			record0.WKI_ActivitySubtype = "";
			record0.WKI_Priority = "";
			record0.WKI_PortOrCountry = "";
			record0.WKI_Summary = "Work Item Summary";
			record0.WKI_Details = "";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 1, processor.NewCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});
			Assert("Logs", !processor.Logs.Any());

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals("Each record should have been imported as a new work item", 1, newWorkItems.Length);

			var workItem0 = newWorkItems.SingleOrDefault(w => w.WKI_Summary == "Work Item Summary");
			AssertNotNull(workItem0);
			AssertEquals(workItem0.WKI_WorkItemType, "");
			AssertEquals(workItem0.WKI_WorkItemArea, "");
			AssertEquals(workItem0.WKI_ActivityType, "");
			AssertEquals(workItem0.WKI_ActivitySubtype, "");
			AssertEquals(workItem0.WKI_Priority, "");
			AssertEquals(workItem0.WKI_PortOrCountry, "");
			AssertEquals(workItem0.WKI_Details.ToUTF8(), "");
		}

		public void TestImport_WithNoSummary()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "2AA";
			record0.WKI_ActivityType = "3AA";
			record0.WKI_ActivitySubtype = "4AA";
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = ""; // missing summary
			record0.WKI_Details = "Work Item Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Missing work item summary",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals(0, newWorkItems.Length);
		}

		#endregion

		#region Rollback

		public void TestRollback()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKI_WorkItemType = "1AA";
			record0.WKI_WorkItemArea = "2AA";
			record0.WKI_ActivityType = "3AA";
			record0.WKI_ActivitySubtype = "4AA";
			record0.WKI_Priority = "LOW";
			record0.WKI_PortOrCountry = "XZAAD";
			record0.WKI_Summary = "Work Item 0";
			record0.WKI_Details = "Work Item 0 Description";

			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingWorkItems = Factory.Load<WorkItem>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 1, processor.NewCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});
			Assert("Logs", !processor.Logs.Any());

			var newWorkItems = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals("Each record should have been imported as a new work item", 1, newWorkItems.Length);

			processor.Rollback();

			var itemsAfterRollingBack = Factory.Load<WorkItem>(new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, existingWorkItems.Select(x => x.PK)));
			AssertEquals("Should have no work items", 0, itemsAfterRollingBack.Length);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
		}

		#endregion
	}
}
