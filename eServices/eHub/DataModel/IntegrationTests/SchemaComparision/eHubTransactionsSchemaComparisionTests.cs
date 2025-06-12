using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using CargoWise.eHub.DataModel.Comparer;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eServices.TestHelpers.Database.Common;
using NUnit.Framework;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubTransactionsSchemaComparisionTests : eHubTransactionsTestBase
	{
		[Test]
		public void TestForeignKeysAndInversePropertySynced()
		{
			using (var context = ContextFactory())
			{
				var contextEntitySets = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace).Single().EntitySets;

				Assert.Multiple(() =>
				{
					foreach (var currentType in Assembly.GetAssembly(context.GetType()).GetTypes())
					{
						var entitySet = contextEntitySets.SingleOrDefault(x => x.ElementType.Name == currentType.Name);

						foreach (var property in currentType.GetProperties())
						{
							foreach (var attribute in property.GetCustomAttributes())
							{
								var entity = entitySet.ElementType.DeclaredMembers.Single(x => x.Name == property.Name);

								if (attribute is ForeignKeyAttribute fkAttribute)
								{
									if (Assembly.GetAssembly(context.GetType()).GetType(property.PropertyType.FullName).GetProperties().
											Where(p => p.GetCustomAttribute(typeof(InversePropertyAttribute)) is InversePropertyAttribute inverseProperty && 
													inverseProperty.Property == property.Name && 
													((p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && p.PropertyType.GetGenericArguments().Single() == currentType) ||
														p.PropertyType == currentType)).Count() == 0)
									{
										Assert.Fail($"{currentType.FullName} has {property.Name} property linked to {property.PropertyType.FullName} with missing InverseProperty attribute.");
									}

									if (((EntityType)entity.DeclaringType).DeclaredNavigationProperties.FirstOrDefault(x => x.GetDependentProperties().Where(p => p.Name == fkAttribute.Name).Any()) == null)
									{
										Assert.Fail($"{currentType.FullName} has {property.Name} property with ForeignKey not properly linked in EF.");
									}
								}

								if (attribute is InversePropertyAttribute ipAttribute && !((AssociationType)((NavigationProperty)entity).RelationshipType).IsForeignKey)
								{
									Assert.Fail($"{currentType.FullName} has {property.Name} property with InverseProperty not properly linked in EF.");
								}
							}
						}
					}
				});
			}
		}

		[Test]
		public void TestDatabaseAndDataModelSynced()
		{
			/****** Need to fix database or data model on foreign key and data type ******/
			Dictionary<string, List<string>> tablesWithMismatchedColumns = new Dictionary<string, List<string>>()
			{
				{ "[dbo].[eHubInboxXmlContent]", new List<string>() { "EX_EI_Inbox" } },
				{ "[dbo].[eHubInboxMessage]", new List<string>() { "EI_InsertUTC" } },
				{ "[dbo].[eHubOutboxMessage]", new List<string>() { "OI_EI_InboxPK" } },
				{ "[dbo].[eHubError]", new List<string>() { "EE_EI_Inbox", "EE_OI_Outbox" } },
				{ "[dbo].[eHubSubscriptionAutoSubscribe]", new List<string>() { "SA_CC_Recipient" } },
				{ "[dbo].[eHubClient]", new List<string>() { "CC_DistributionZone" } },
				{ "[dbo].[eHubInboxMessageArchive]", new List<string>() { "EI_CC_Sender", "EI_CC_Recipient", "EI_InsertUTC" } },
				{ "[dbo].[eHubOutboxMessageArchive]", new List<string>() { "OI_CC_Sender", "OI_CC_Recipient", "OI_EI_InboxPK", "OI_DT_Target", "OI_CompressedLength", "OI_SN" } },
				{ "[dbo].[eHubInterfaceCounter]", new List<string>() { "CT_TS_PK" } },
				{ "[dbo].[eHubServiceProvider]", new List<string>() { "SP_RR" } }
			};

			/****** Need to fix data model to add missing columns ******/
			List<string> tablesWithMissingColumns = new List<string> { "eHubClient", "eHubTransformationSet", "eHubMessageType", "eHubOutboxMessage", "eHubInboxMessageArchive",
				"eHubOutboxMessageArchive", "eHubTransformationType" };

			List<string> tablesToExclude = new List<string> { "Temp_eHubCodeMapKey", "Temp_eHubCodeMapValue", "sysdiagrams", "eHubInboxMessage2", "eHubUSCustomsRegistry", "eHubAirConnection",
				"eHubAirConnectionPerBranch", "eHubAirDefaultServiceProvider", "eHubAirReferenceNumber", "eHubAirServiceProviderMapping", "eHubAlert", "eHubAlertSubscriber", "eHubArchiveQueue",
				"eHubClientAccessRestriction", "eHubClientBatching", "eHubClientCode", "eHubClientDialogue", "eHubCounter", "eHubMessageCopyRegistry", "eHubMessageReferenceRegistry",
				"eHubOutboxMessage2", "eHubReferenceFileCache", "eHubReferenceFileQuery", "eHubRegistrationLog", "eHubSequenceNumber", "eHubClientArchive", "eHubMonitorClient",
				"eHubInboxMessageDisplay", "eHubOutboxMessageDisplay", "eHubMessageEvent" };

			List<string> viewsToExclude = new List<string> { "ediProdClient" };
			using (var context = ContextFactory())
			{
				List<string> databaseTableNames = new List<string>();
				List<string> contextTableNames = new List<string>();

				using (var sqlConnection = new SqlConnection(((DbContext)context).Database.Connection.ConnectionString))
				{
					sqlConnection.Open();


					using (SqlCommand sqlCommand = new SqlCommand(@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", sqlConnection))
					{
						using (SqlDataReader reader = sqlCommand.ExecuteReader())
						{
							while (reader.Read())
							{
								databaseTableNames.Add(reader["TABLE_NAME"].ToString());
							}
						}
					}
				}

				var contextEntitySets = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace).Single().EntitySets;

				foreach (var entitySet in contextEntitySets)
				{
					contextTableNames.Add(entitySet.ElementType.Name);
				}

				var missingTables = databaseTableNames.Where(d => !contextTableNames.Any(c => c == d)).Union(contextTableNames.Where(c => !databaseTableNames.Any(d => d == c))).Except(tablesToExclude).Except(viewsToExclude);

				Assert.Multiple(() =>
				{
					foreach (var tableName in missingTables)
					{
						Assert.Fail($"{tableName} is missing from {(databaseTableNames.Contains(tableName) ? "DataModel" : "database")}");
					}
				});

				var comparer = new EFAndSQLDBComparer(context);

				var results = comparer.CompareEFWithTable(databaseTableNames.Except(tablesWithMissingColumns).ToArray(), false, tablesWithMismatchedColumns);

				results.AddRange(comparer.CompareEFWithTable(tablesWithMissingColumns.ToArray(), true, tablesWithMismatchedColumns));

				Assert.Multiple(() =>
				{
					foreach (var result in results)
					{
						var resultDifferences = result.Differences;
						Assert.AreEqual(0, resultDifferences.Count, "Differences: {0}", string.Join("\r\n", resultDifferences));
					}
				});
			}
		}

		[Test]
		public void TestCompare_TableNotExistInEFContext_Failed()
		{
			using (var context = ContextFactory())
			{
				var comparer = new EFAndSQLDBComparer(context);

				var results = comparer.CompareEFWithTable("DummyTable", false).Differences;

				Assert.AreEqual("EF DB Context does not contain following tables: DummyTable", results.Single());
			}
		}

		[Test]
		public void TestCompare_TableNotExistInSQLDB_Failed()
		{
			using (var context = new eHubTransactionsContextTest(SqlServerHelper.GetAdminConnectionString("eHubTransactions")))
			{
				var comparer = new EFAndSQLDBComparer(context);

				var results = comparer.CompareEFWithTable("DummyTable", false).Differences;

				Assert.AreEqual("Missing Table: The SQL database does not contain a table called [dbo].[DummyTable]. Needed by EF class DummyTable.", results.Single());
			}
		}
	}

	partial class eHubTransactionsContextTest : eHubTransactionsContext
	{
		public eHubTransactionsContextTest(string connectionString)
			: base(connectionString)
		{
		}

		public virtual DbSet<DummyTable> DummyTable { get; set; }
	}

	public class DummyTable
	{
		[Key]
		public string DummyColumn { get; set; }
	}
}
