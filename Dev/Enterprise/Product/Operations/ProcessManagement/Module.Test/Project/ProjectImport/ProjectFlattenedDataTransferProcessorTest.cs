using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module.Test
{
	public class ProjectFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		#region Correct Property Values

		public void TestImport_WithCorrectValues()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "1AA";
			record0.WKP_SubType = "2AA";
			record0.WKP_Module = "3AA";
			record0.WKP_Priority = "4AA";
			record0.WKP_Summary = "Project 0";
			record0.WKP_Details = "Project 0 Description";

			var record1 = flattenedCollection.AddNew();
			record1.WKP_Type = "1BB";
			record1.WKP_SubType = "2CC";
			record1.WKP_Module = "3S1";
			record1.WKP_Priority = "4S1";
			record1.WKP_Summary = "Project 1";
			record1.WKP_Details = "Project 1 Description";

			var record2 = flattenedCollection.AddNew();
			record2.WKP_Type = "1AA";
			record2.WKP_SubType = "2DD";
			record2.WKP_Module = "3S1";
			record2.WKP_Priority = "4S2";
			record2.WKP_Summary = "Project 2";
			record2.WKP_Details = "Project 2 Description";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 3, processor.NewCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});
			Assert("Logs", !processor.Logs.Any());

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals("Each record should have been imported as a new project", 3, newProjects.Length);

			var project0 = newProjects.SingleOrDefault(p => p.WKP_Summary == "Project 0");
			AssertNotNull(project0);
			AssertEquals(project0.WKP_Type, "1AA");
			AssertEquals(project0.WKP_SubType, "2AA");
			AssertEquals(project0.WKP_Module, "3AA");
			AssertEquals(project0.WKP_Priority, "4AA");
			AssertEquals(project0.WKP_Details.ToUTF8(), "Project 0 Description");

			var project1 = newProjects.SingleOrDefault(p => p.WKP_Summary == "Project 1");
			AssertNotNull(project1);
			AssertEquals(project1.WKP_Type, "1BB");
			AssertEquals(project1.WKP_SubType, "2CC");
			AssertEquals(project1.WKP_Module, "3S1");
			AssertEquals(project1.WKP_Priority, "4S1");
			AssertEquals(project1.WKP_Details.ToUTF8(), "Project 1 Description");

			var project2 = newProjects.SingleOrDefault(p => p.WKP_Summary == "Project 2");
			AssertNotNull(project2);
			AssertEquals(project2.WKP_Type, "1AA");
			AssertEquals(project2.WKP_SubType, "2DD");
			AssertEquals(project2.WKP_Module, "3S1");
			AssertEquals(project2.WKP_Priority, "4S2");
			AssertEquals(project2.WKP_Details.ToUTF8(), "Project 2 Description");
		}

		public void TestImport_WithLeadingAndTrailingSpaces()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "1AA";
			record0.WKP_SubType = "2AA";
			record0.WKP_Module = "3AA";
			record0.WKP_Priority = "4AA";
			record0.WKP_Summary = " Project 0 ";
			record0.WKP_Details = " Project 0 Description ";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 1, processor.NewCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});
			Assert("Logs", !processor.Logs.Any());

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals("Each record should have been imported as a new project", 1, newProjects.Length);

			var project0 = newProjects.SingleOrDefault(p => p.WKP_Summary == "Project 0");
			AssertNotNull(project0);
			AssertEquals(project0.WKP_Type, "1AA");
			AssertEquals(project0.WKP_SubType, "2AA");
			AssertEquals(project0.WKP_Module, "3AA");
			AssertEquals(project0.WKP_Priority, "4AA");
			AssertEquals(project0.WKP_Details.ToUTF8(), "Project 0 Description");
		}

		#endregion

		#region Invalid Property Values

		public void TestImport_WithInvalidProjectType()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "ZZZ"; // incorrect type
			record0.WKP_SubType = "2DD";
			record0.WKP_Module = "3S1";
			record0.WKP_Priority = "4S2";
			record0.WKP_Summary = "Project Summary";
			record0.WKP_Details = "Project Description";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid project type code 'ZZZ'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals(0, newProjects.Length);
		}

		public void TestImport_WithInvalidProjectSubType()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "1AA";
			record0.WKP_SubType = "1BB"; // incorrect subtype: is applicable to type 1BB only
			record0.WKP_Module = "3S1";
			record0.WKP_Priority = "4S2";
			record0.WKP_Summary = "Project Summary";
			record0.WKP_Details = "Project Description";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid project subtype code '1BB'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals(0, newProjects.Length);
		}

		public void TestImport_WithInvalidProjectModule()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "1AA";
			record0.WKP_SubType = "2AA";
			record0.WKP_Module = "3BB"; // incorrect module: is applicable to type 1BB and subtype 2B1 only
			record0.WKP_Priority = "ANY";
			record0.WKP_Summary = "Project Summary";
			record0.WKP_Details = "Project Description";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid project module code '3BB'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals(0, newProjects.Length);
		}

		public void TestImport_WithInvalidProjectPriority()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "1AA";
			record0.WKP_SubType = "2AA";
			record0.WKP_Module = "3AA";
			record0.WKP_Priority = "ZZZ"; // incorrect activity subtype
			record0.WKP_Summary = "Project Summary";
			record0.WKP_Details = "Project Description";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid project priority 'ZZZ'",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals(0, newProjects.Length);
		}

		#endregion

		#region Missing Property Values

		public void TestImport_WithPropertiesThatCanBeLegallyEmpty()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "";
			record0.WKP_SubType = "";
			record0.WKP_Module = "";
			record0.WKP_Priority = "";
			record0.WKP_Summary = "Project Summary";
			record0.WKP_Details = "";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 1, processor.NewCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});
			Assert("Logs", !processor.Logs.Any());

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals("Each record should have been imported as a new project", 1, newProjects.Length);

			var project0 = newProjects.SingleOrDefault(p => p.WKP_Summary == "Project Summary");
			AssertNotNull(project0);
			AssertEquals(project0.WKP_Type, "");
			AssertEquals(project0.WKP_SubType, "");
			AssertEquals(project0.WKP_Module, "");
			AssertEquals(project0.WKP_Priority, "");
			AssertEquals(project0.WKP_Details.ToUTF8(), "");
		}

		public void TestImport_WithNoSummary()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "1AA";
			record0.WKP_SubType = "2AA";
			record0.WKP_Module = "3AA";
			record0.WKP_Priority = "4AA";
			record0.WKP_Summary = ""; // missing summary
			record0.WKP_Details = "Project Description";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
			});
			AssertMultilineASCIIEquals("Logs",
@"Line 1: Missing project summary",
				string.Join(System.Environment.NewLine, processor.Logs));

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals(0, newProjects.Length);
		}

		#endregion

		#region Rollback

		public void TestRollback()
		{
			var flattenedCollection = new ProjectFlattenedCollection(Factory);

			var record0 = flattenedCollection.AddNew();
			record0.WKP_Type = "1AA";
			record0.WKP_SubType = "2AA";
			record0.WKP_Module = "3AA";
			record0.WKP_Priority = "4AA";
			record0.WKP_Summary = "Project 0";
			record0.WKP_Details = "Project 0 Description";

			var flattenedCollectionInfo = new ProjectFlattenedCollectionInfo(flattenedCollection);
			var processor = new ProjectFlattenedDataTransferProcessor(flattenedCollectionInfo);

			var existingProjects = Factory.Load<Project>(new ZQuery());
			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 1, processor.NewCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});
			Assert("Logs", !processor.Logs.Any());

			var newProjects = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals("Each record should have been imported as a new project", 1, newProjects.Length);

			processor.Rollback();

			var itemsAfterRollingBack = Factory.Load<Project>(new ZQuery(WorkProjectSchema.PK, SQLComparisonOperator.NotEqual, existingProjects.Select(p => p.PK)));
			AssertEquals("Should have no projects", 0, itemsAfterRollingBack.Length);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
		}

		#endregion
	}
}
