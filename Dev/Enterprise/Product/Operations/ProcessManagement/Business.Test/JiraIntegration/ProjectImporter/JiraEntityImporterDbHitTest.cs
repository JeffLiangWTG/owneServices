using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class JiraEntityImporterDbHitTest : TestCaseWithFactory
	{
		public void TestJiraImport_DbHits()
		{
			var factoria = new BusinessObjectFactory();
			AssertEquals(0, factoria.Load<WorkItem>(new ZQuery()).Length);
			AssertEquals(0, factoria.Load<Project>(new ZQuery()).Length);

			var dummyImporter = new DummyJiraEntityImporter(Factory);

			var credentials = new JiraCredentials("FranceFancyPants", "EggParty");
			var hits = new Dictionary<string, int>
			{
				{ ExternalEntityLinkSchema.Constants.TableName, 2 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgRelatedPartySchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ RefDocTypeSchema.Constants.TableName, 2 },
				{ StmEventSchema.Constants.TableName, 2 },
				{ StorageMainSchema.Constants.TableName, 1 },
				{ EDIMessageSchema.Constants.TableName, 1 }
			};

			using (AssertMaxDbHitsForAllFactories(hits, includeFactoryPredicate: f => f.NameForDebugging.Contains("Jira")))
			{
				dummyImporter.Import(credentials);
			}

			AssertEquals("We have 1 batch, and one project", 2, dummyImporter.NumberOfSaves);

			var factorio = new BusinessObjectFactory();
			AssertEquals(1, factorio.Load<WorkItem>(new ZQuery()).Length);
			AssertEquals(1, factorio.Load<Project>(new ZQuery()).Length);
		}

		#region 1 Project with Comments

		public void TestImport1ProjectWithComments_DbHits_1Batch_1Issue()
		{
			AssertImport1ProjectWithCommentsDbHits(1);
		}

		public void TestImport1ProjectWithComments_DbHits_3Batches_101Issues()
		{
			AssertImport1ProjectWithCommentsDbHits(3);
		}

		public void TestImport1ProjectWithComments_DbHits_4Batches_151Issues()
		{
			AssertImport1ProjectWithCommentsDbHits(4);
		}

		void AssertImport1ProjectWithCommentsDbHits(int numBatches)
		{
			var factoria = new BusinessObjectFactory();
			AssertEquals(0, factoria.Load<WorkItem>(new ZQuery()).Length);
			AssertEquals(0, factoria.Load<Project>(new ZQuery()).Length);

			const int batchSize = 5;
			var numIssues = (batchSize * (numBatches - 1)) + 1; // a number of issues so that the last batch only has 1 issue

			var jiraIssueImportBatchSizeRegistry = ProcessManagementRegistry.Instance.JiraIssueImportBatchSize;

			using (jiraIssueImportBatchSizeRegistry.DataType.SuspendValidation())
			{
				jiraIssueImportBatchSizeRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
			}

			var allIssuesJson = JiraIntegrationTestHelper.GetJsonWithSpecifiedNumberOfIssues(numIssues, true);
			var viewModel = new ProjectsToImportViewModel(Factory)
			{
				ShouldImportIssueAttachments = true,
				ShouldImportAllProjects = true,
			};

			JiraDescriptionDecoder DescriptionDecoderForTest(IDocumentFactory docFactory)
			{
				return new JiraDescriptionDecoderForTest(docFactory)
				{
					ReturnBadDocData = true
				};
			}

			byte[] IssueAttachmentProvider(JiraAttachment attachment)
			{
				switch (attachment.ID)
				{
					case "10009":
						return new byte[] { 1, 1, 1 };
					case "10008":
						return new byte[] { 2, 2, 2 };
					case "10001":
						return new byte[] { 3, 3, 3 };
					case "10006":
						return new byte[] { 4, 4, 4 };
					case "10007":
						return new byte[] { 5, 5, 5 };

					default:
						return Array.Empty<byte>();
				}
			}

			var dummyImporter = new DummyJiraEntityImporter(viewModel)
			{
				ManyIssuesJSONString = allIssuesJson,
				DescriptionDecoderToUse = DescriptionDecoderForTest,
				IssueAttachmentContentGetter = IssueAttachmentProvider
			};

			var credentials = new JiraCredentials("Small", "NonSmall");

			var hits = new Dictionary<string, int>
			{
				{ ExternalEntityLinkSchema.Constants.TableName, 2 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 4 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgRelatedPartySchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ RefDocTypeSchema.Constants.TableName, 2 },
				{ StmEventSchema.Constants.TableName, 2 },
				{ StorageMainSchema.Constants.TableName, numBatches },
				{ WorkItemSchema.Constants.TableName, 0 },
			};

			RowFactory.ResetCacheAfterDbUpgrade();

			using (AssertMaxDbHitsForAllFactories(hits, includeFactoryPredicate: f => f.NameForDebugging.Contains("Jira")))
			{
				dummyImporter.Import(credentials);
			}

			AssertEquals("We have " + numBatches + " batches, each with eConversations, and 1 project", (2 * numBatches) + 1, dummyImporter.NumberOfSaves);

			var factorio = new BusinessObjectFactory();
			AssertEquals(numIssues, factorio.Load<WorkItem>(new ZQuery()).Length);
			AssertEquals(1, factorio.Load<Project>(new ZQuery()).Length);
		}

		#endregion

		#region Many Projects with Comments

		public void TestImport2ProjectsWithComments_DbHits()
		{
			var extraProjectsTupleArray = new[] { new Tuple<string, string>("BP", "10008") };

			AssertImportManyProjectsWithCommentsDbHits(JiraIntegrationTestHelper.JSONForTwoProjects, extraProjectsTupleArray);
		}

		public void TestImport4ProjectsWithComments_DbHits()
		{
			var extraProjectsTupleArray = new[]
			{
				new Tuple<string, string>("BP", "10008"),
				new Tuple<string, string>("YEET", "10009"),
				new Tuple<string, string>("ByeDave:(", "10004"),
			};

			AssertImportManyProjectsWithCommentsDbHits(JiraIntegrationTestHelper.JSONForFourProjects, extraProjectsTupleArray);
		}

		public void AssertImportManyProjectsWithCommentsDbHits(string allProjectsOverrideString, Tuple<string, string>[] extraProjectsTupleArray)
		{
			var factoria = new BusinessObjectFactory();
			AssertEquals(0, factoria.Load<WorkItem>(new ZQuery()).Length);
			AssertEquals(0, factoria.Load<Project>(new ZQuery()).Length);

			const int batchSize = 5;
			var issuesPerProject = 6;
			var numProjects = 1 + extraProjectsTupleArray.Length; // account for default project

			var totalIssues = issuesPerProject * numProjects;
			var totalBatches = 2 * numProjects; // 2 batches per project
			var totalTrackedBatches = totalBatches - 1; // this is due to a bug in the SaveAndReclaim method, or something

			var jiraIssueImportBatchSizeRegistry = ProcessManagementRegistry.Instance.JiraIssueImportBatchSize;

			using (jiraIssueImportBatchSizeRegistry.DataType.SuspendValidation())
			{
				jiraIssueImportBatchSizeRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
			}

			var allIssuesJson = JiraIntegrationTestHelper.GetJsonWithSpecifiedNumberOfIssues(issuesPerProject, includeAttachments: true, extraProjectsTupleArray);
			var viewModel = new ProjectsToImportViewModel(Factory)
			{
				ShouldImportIssueAttachments = true,
				ShouldImportAllProjects = true,
			};

			JiraDescriptionDecoder DescriptionDecoderForTest(IDocumentFactory docFactory)
			{
				return new JiraDescriptionDecoderForTest(docFactory)
				{
					ReturnBadDocData = true
				};
			}

			byte[] IssueAttachmentProvider(JiraAttachment attachment)
			{
				switch (attachment.ID)
				{
					case "10009":
						return new byte[] { 1, 1, 1 };
					case "10008":
						return new byte[] { 2, 2, 2 };
					case "10001":
						return new byte[] { 3, 3, 3 };
					case "10004":
						return new byte[] { 4, 4, 4 };
					case "10007":
						return new byte[] { 5, 5, 5 };

					default:
						return new byte[] { 6, 6, 6 };
				}
			}

			var dummyImporter = new DummyJiraEntityImporter(viewModel)
			{
				AllProjectsJSONString = allProjectsOverrideString,
				ManyIssuesJSONString = allIssuesJson,
				DescriptionDecoderToUse = DescriptionDecoderForTest,
				IssueAttachmentContentGetter = IssueAttachmentProvider
			};

			var credentials = new JiraCredentials("Small", "NonSmall");

			var hits = new Dictionary<string, int>
			{
				{ ExternalEntityLinkSchema.Constants.TableName, numProjects + 1 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 4 },
				{ OrgAddressSchema.Constants.TableName, numProjects },
				{ OrgContactSchema.Constants.TableName, numProjects },
				{ OrgHeaderSchema.Constants.TableName, numProjects },
				{ OrgRelatedPartySchema.Constants.TableName, numProjects },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, numProjects + 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, numProjects + 1 },
				{ RefDocTypeSchema.Constants.TableName, 2 },
				{ StmEventSchema.Constants.TableName, 2 },
				{ StorageMainSchema.Constants.TableName, numProjects + 1 },
				{ WorkItemSchema.Constants.TableName, 0 },
			};

			RowFactory.ResetCacheAfterDbUpgrade();

			using (AssertMaxDbHitsForAllFactories(hits, includeFactoryPredicate: f => f.NameForDebugging.Contains("Jira")))
			{
				AssertNoExceptionThrown("Including this because sometimes the AssertDbHits using hides exceptions :c", () => dummyImporter.Import(credentials));
			}

			var factorio = new BusinessObjectFactory();
			AssertEquals(issuesPerProject * numProjects, factorio.Load<WorkItem>(new ZQuery()).Length);
			AssertEquals(numProjects, factorio.Load<Project>(new ZQuery()).Length);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = false; // disable service tasks runs for setup, to avoid unnecessarily-recorded hits
			base.SetUp();

			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem();
			JiraIntegrationTestHelper.AddDefaultOrgProxyClient();
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
		}

		protected override void TearDown()
		{
			base.TearDown();
			SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = true;
		}

		#endregion
	}
}
