using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class UpgradeScriptProviderFixture
	{
		[Test]
		public void TestLatestVersion()
		{
			Assert.That(provider.LatestVersion == UpgradeScriptProvider.Upgrades.Max(x => x.UpgradeVersion));
		}

		[Test]
		public void TestRequiredVersion()
		{
			Assert.That(provider.LatestVersion >= provider.RequiredVersion);
		}

		[Test]
		public void TestLatestTestVersionHasCorrespondingTest()
		{
			Assert.That(provider.LatestVersion == LatestTestUpgradeScriptVersion, @"
If this test fails, there is high probablity that one didn't add corresponding fixutre for new schema change.
Please add AssertVersionXXXModifications test in the TestUpgradeScript() below, and update LatestTestUpgradeScriptVersion.

Please note:
1. One ZZ version doesn't have to map to one version(can be multiple) in UpgradeScriptProvider.cs, each version in UpgradeScriptProvider is recommended as small and non-coupling with other versions as possible
2. When adding test in TestUpgradeScript(), please make sure you have
// Restore and Pre-condition check
for each #region AssertVersionXXXModifications

This part is to make sure the database looks exactly like it should before upgrade
e.g. If Version #XXX is to add a new index or new column, should assert that index or column doesn't exist before running Version #XXX script
If index or columns exists (because such change also take place in SharedScriptManager.cs), need to drop first before assertion");
		}

		int LatestTestUpgradeScriptVersion = 579;

		[Test]
		public void TestColumnTypeShouldBeUpperCaseInCreateTableScript()
		{
			var type = typeof(ITableScript);
			var types = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name == "CargoWise.RefDbRepo.RemoteDbManager").SelectMany(x => x.GetTypes())
				.Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
				.ToList();

			var createSeparators = new[] { "CREATE" };
			var lineSeparators = new[] { "\r\n", "," };
			var spaceSeparators = new[] { " " };
			var tablesExistLowerColumnType = new List<string>();
			foreach (var tableType in types)
			{
				var createTableScript = ((ITableScript)Activator.CreateInstance(tableType)).CreateTableScript;
				var createColumnAndConstraintScript = createTableScript.Split(createSeparators, StringSplitOptions.RemoveEmptyEntries)[0];
				var createColumnAndConstraintScripWithoutComments = Regex.Replace(createColumnAndConstraintScript, "--(.*?)\\r\\n", string.Empty, RegexOptions.CultureInvariant);
				var lineStrings = createColumnAndConstraintScripWithoutComments.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);
				foreach (var lineString in lineStrings)
				{
					var unitStrings = lineString.Replace('\t', ' ').Trim().Split(spaceSeparators, StringSplitOptions.RemoveEmptyEntries);
					if (unitStrings.Length >= 2
						&& !unitStrings[0].StartsWith("TABLE", StringComparison.OrdinalIgnoreCase)
						&& !unitStrings[0].StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase)
						&& !unitStrings[0].StartsWith("INDEX", StringComparison.OrdinalIgnoreCase))
					{
						var columnType = unitStrings[1];
						var isUpper = columnType.All(c => char.IsUpper(c) || !char.IsLetter(c));
						if (!isUpper)
						{
							tablesExistLowerColumnType.Add(tableType.ToString());
							break;
						}
					}
				}
			}

			var existLowerColumnType = tablesExistLowerColumnType.Any();
			var tablesExistLowerColumnTypeAsString = existLowerColumnType
				? "The table scripts existed lower case column type: " + string.Join(",", tablesExistLowerColumnType)
				: string.Empty;
			Assert.False(existLowerColumnType, $"Should no column type in lower case. {tablesExistLowerColumnTypeAsString}");
		}

		[Test]
		public void AllUpgradesHaveDescription()
		{
			Assert.That(UpgradeScriptProvider.Upgrades.All(x => !string.IsNullOrEmpty(x.Description.Trim())));
		}

		[Test]
		public void TestGetUpgradeWrapperByVersion()
		{
			var allVersions = UpgradeScriptProvider.Upgrades.Select(x => x.UpgradeVersion);
			var upgradeScripts = UpgradeScriptProvider.Upgrades.ToDictionary(x => x.UpgradeVersion, x => new { x.UpgradeScript, x.Description });
			foreach (var version in allVersions)
			{
				var upgradeWrapper = provider.GetUpgradeWrapperByVersion(version);
				Assert.NotNull(upgradeWrapper);
				if (upgradeScripts.TryGetValue(version, out var scriptInfo))
				{
					Assert.AreEqual(upgradeWrapper.UpgradeScript, scriptInfo.UpgradeScript);
					Assert.AreEqual(upgradeWrapper.Description, scriptInfo.Description);
				}
			}
		}

		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.BlankDbTestCases))]
		public void TestUpgradeScript_Overall(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				foreach (var version in provider.GetAvailableVersionsAfterVersion(1).Where(x => x < 300))
				{
					var t = provider.GetUpgradeWrapperByVersion(version).UpgradeScript;
					TestDBHelper.ExecuteNonQuery(conn, t);
				}
				for (var version = 300; version <= provider.LatestVersion; version++)
				{
					var wrapper = provider.GetUpgradeWrapperByVersion(version);
					Assert.NotNull(wrapper, $"Cannot find upgradeWrapper {version}.");
					Assert.False(string.IsNullOrEmpty(wrapper.UpgradeScript.Trim()), $"Upgrade {version} has no actual script.");
					Assert.DoesNotThrow(() => TestDBHelper.ExecuteNonQuery(conn, wrapper.UpgradeScript), $"Error happens when upgrading to version {version}.");
				}

				var expectedDiff = string.Empty;
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.RemoteDbManager.Test.Resources.SchemaDiff.sql"))
				using (var reader = new StreamReader(stream))
				{
					expectedDiff = reader.ReadToEnd();
				}

				var schemaUpgrade = new SchemaUpgrade("RemoteDb.dacpac", Common.Infrastructure.Test.TestConnectionString.DataSource, null, null);
				var actualDiff = schemaUpgrade.GetDiffSql(dbName);
				Assert.AreEqual(expectedDiff, Regex.Replace(actualDiff, @"\\MSSQL\d*\.MSSQLSERVER\d*", "").Replace("CollationCS", ""), "Upgrade script should not bring new schema difference, comparing with RemoteDb.dacpac.");
			}
		}

#pragma warning disable CA1505
#pragma warning disable CA1506
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.BlankDbTestCases))]
		public void TestUpgradeScript(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				foreach (var upgradeWrapper in UpgradeScriptProvider.Upgrades)
				{
					Assert.False(string.IsNullOrEmpty(upgradeWrapper.UpgradeScript.Trim()));
				}

				foreach (var version in provider.GetAvailableVersionsAfterVersion(1).Where(x => x < 210))
				{
					var t = provider.GetUpgradeWrapperByVersion(version).UpgradeScript;
					TestDBHelper.ExecuteNonQuery(conn, t);
				}
				Assert.That(true);

				#region AssertVersion210Modifications

				// Restore and Pre-condition Check
				var uNDGSubstanceCFRTableName = "UNDGSubstanceCFR";
				var restoreSb = new StringBuilder();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(uNDGSubstanceCFRTableName, "CFR_IsPAXAirRailForbidden", "BIT NOT NULL CONSTRAINT DF_UNDGSubstanceCFR_CFR_IsPAXAirRailForbidden DEFAULT 0"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(uNDGSubstanceCFRTableName, "CFR_IsCargoAirRailForbidden", "BIT NOT NULL CONSTRAINT DF_UNDGSubstanceCFR_CFR_IsCargoAirRailForbidden DEFAULT 0"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(uNDGSubstanceCFRTableName, "CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(uNDGSubstanceCFRTableName, "CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(uNDGSubstanceCFRTableName, "CFR_PAXAirRailLimitType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(uNDGSubstanceCFRTableName, "CFR_CargoAirRailLimitType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(uNDGSubstanceCFRTableName, "CFR_LQMaxAmt"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(uNDGSubstanceCFRTableName, "CFR_LQMaxAmtUQ"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(uNDGSubstanceCFRTableName, "CFR_PAXAirRailLimitType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(uNDGSubstanceCFRTableName, "CFR_CargoAirRailLimitType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(uNDGSubstanceCFRTableName, "CFR_LQMaxAmt"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(uNDGSubstanceCFRTableName, "CFR_LQMaxAmtUQ"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_PAXAirRailLimitType"));
				Assert.False(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_CargoAirRailLimitType"));
				Assert.False(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_LQMaxAmt"));
				Assert.False(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_LQMaxAmtUQ"));
				Assert.True(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_IsPAXAirRailForbidden"));
				Assert.True(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_IsCargoAirRailForbidden"));
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, uNDGSubstanceCFRTableName, "CFR_IsPAXAirRailForbidden"));
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, uNDGSubstanceCFRTableName, "CFR_IsCargoAirRailForbidden"));

				//Verify Version 210 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(210).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_PAXAirRailLimitType"));
				Assert.True(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_CargoAirRailLimitType"));
				Assert.True(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_LQMaxAmt"));
				Assert.True(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_LQMaxAmtUQ"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, uNDGSubstanceCFRTableName, "CFR_IsPAXAirRailForbidden"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, uNDGSubstanceCFRTableName, "CFR_IsCargoAirRailForbidden"));
				#endregion

				#region AssertVersion211Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				Assert.True(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_IsPAXAirRailForbidden"));
				Assert.True(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_IsCargoAirRailForbidden"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, uNDGSubstanceCFRTableName, "CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, uNDGSubstanceCFRTableName, "CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType"));

				//Verify Version 211 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(211).UpgradeScript);

				Assert.False(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_IsPAXAirRailForbidden"));
				Assert.False(TestDBHelper.ColumnExists(conn, uNDGSubstanceCFRTableName, "CFR_IsCargoAirRailForbidden"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, uNDGSubstanceCFRTableName, "CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, uNDGSubstanceCFRTableName, "CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType"));
				#endregion

				#region AssertVersion212Modifications

				// Restore and Pre-condition Check
				var refCusTaxOrFee = new RefCusTaxOrFee();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusTaxOrFee.TableName, "CK_RefCusTaxOrFee_ZZF_StartDate_ZZF_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusTaxOrFee.TableName, "ZZF_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusTaxOrFee.TableName, "ZZF_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTaxOrFee.TableName, "ZZF_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTaxOrFee.TableName, "ZZF_EndDate"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTaxOrFee.TableName, "ZZF_StartDate", "SMALLDATETIME NOT NULL default GetUtcDate()"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTaxOrFee.TableName, "ZZF_EndDate", "SMALLDATETIME NOT NULL default '2079-06-06 23:59'"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTaxOrFee.TableName, "CK_RefCusTaxOrFee_ZZF_StartDate_ZZF_EndDate", "(ZZF_StartDate<=ZZF_EndDate)"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate ON RefCusTaxOrFee(ZZF_ZZZ_NKDataGrouping ASC, ZZF_Code ASC, ZZF_StartDate)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate ON RefCusTaxOrFee (ZZF_ZZZ_NKDataGrouping ASC, ZZF_Code ASC, ZZF_EndDate ASC)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.ColumnTypeMatchingLowercase(conn, refCusTaxOrFee.TableName, "ZZF_StartDate", "smalldatetime"));
				Assert.True(TestDBHelper.ColumnTypeMatchingLowercase(conn, refCusTaxOrFee.TableName, "ZZF_EndDate", "smalldatetime"));

				Assert.True(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate"));

				//Verify Version 212 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(212).UpgradeScript);

				Assert.True(TestDBHelper.ColumnTypeMatchingLowercase(conn, refCusTaxOrFee.TableName, "ZZF_StartDate", "datetime"));
				Assert.True(TestDBHelper.ColumnTypeMatchingLowercase(conn, refCusTaxOrFee.TableName, "ZZF_EndDate", "datetime"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate"));
				#endregion

				#region AssertVersion213Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				Assert.False(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refCusTaxOrFee.TableName, "ZZF_StartDate"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refCusTaxOrFee.TableName, "ZZF_EndDate"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refCusTaxOrFee.TableName, "CK_RefCusTaxOrFee_ZZF_StartDate_ZZF_EndDate"));
				//Verify Version 213 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(213).UpgradeScript);

				Assert.True(TestDBHelper.ColumnTypeMatchingLowercase(conn, refCusTaxOrFee.TableName, "ZZF_StartDate", "datetime"));
				Assert.True(TestDBHelper.ColumnTypeMatchingLowercase(conn, refCusTaxOrFee.TableName, "ZZF_EndDate", "datetime"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refCusTaxOrFee.TableName, "CK_RefCusTaxOrFee_ZZF_StartDate_ZZF_EndDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate"));
				#endregion

				#region AssertVersion214Modifications

				// Restore and Pre-condition Check
				var refAirlineUNDGRule = new RefAirlineUNDGRule();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refAirlineUNDGRule.TableName));
				//Verify Version 214 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(214).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refAirlineUNDGRule.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate"));
				#endregion

				#region AssertVersion215Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				var refCusCodeType = new RefCusCodeType();
				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeList_INS_UPD"));
				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttributeName_INS_UPD"));
				//Verify Version 215 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(215).UpgradeScript);

				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeList_INS_UPD"));
				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttributeName_INS_UPD"));
				#endregion

				#region AssertVersion216Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				var codeTypeId = Guid.NewGuid();
				TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (newid(), 'CH', 'XXX')
INSERT INTO RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(newid(), 'CH', 'CH');
INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES ('{codeTypeId}', 'PKG', 'Package', 1, 0, '');
INSERT INTO RefCusCodeTypeLanguage (ZXI_PK, ZXI_ZX6_NKLanguage, ZXI_ZZK_CodeType, ZXI_Description)
VALUES(newid(), 'CH', '{codeTypeId}', N'Additonal Information in Chinese');");

				//Verify Version 216 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(216).UpgradeScript);

				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusCodeType WHERE ZZK_ZZZ_NKDataGrouping = ''"));
				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, $"SELECT COUNT(*) FROM RefCusCodeTypeLanguage WHERE ZXI_ZZK_CodeType = '{codeTypeId}'"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refCusCodeType.TableName, "ZZK_ZZZ_NKDataGrouping"));
				#endregion

				#region AssertVersion217Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refCusCodeType.TableName, "CK_RefCusCodeType_ZZK_ZZZ_NKDataGrouping"));
				//Verify Version 217 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(217).UpgradeScript);

				Assert.True(TestDBHelper.CheckConstraintExists(conn, refCusCodeType.TableName, "CK_RefCusCodeType_ZZK_ZZZ_NKDataGrouping"));
				#endregion

				#region AssertVersion218Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeList_INS_UPD"));
				//Verify Version 218 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(218).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeList_INS_UPD"));
				#endregion

				#region AssertVersion219Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttributeName_INS_UPD"));
				//Verify Version 219 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(219).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttributeName_INS_UPD"));
				#endregion

				#region AssertVersion300Modifications

				// Restore and Pre-condition Check
				// N/A as to remove obsolete sql objects

				//Verify Version 300 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(300).UpgradeScript);

				Assert.False(TestDBHelper.ObjectExists(conn, "IF", "GetRatesBySingleCriteriaSet"));
				Assert.False(TestDBHelper.ObjectExists(conn, "P", "GetApplicableConditions"));
				Assert.False(TestDBHelper.ObjectExists(conn, "P", "GetApplicableConditionsBySingleAdditionalCode"));
				Assert.False(TestDBHelper.ObjectExists(conn, "P", "GetApplicableRates"));
				Assert.False(TestDBHelper.ObjectExists(conn, "P", "GetApplicableRatesWithoutDataGrouping"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RateView"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffAdditionalCodeView"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffAttributeView"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffRelationshipView"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffView"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "VATApplicabilityView"));
				#endregion

				#region AssertVersion301Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffView_V1"));
				//Verify Version 301 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(301).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffView_V1"));
				#endregion

				#region AssertVersion302Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RateView_V1"));
				//Verify Version 302 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(302).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RateView_V1"));
				#endregion

				#region AssertVersion303Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffAttributeView_V1"));
				//Verify Version 303 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(303).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffAttributeView_V1"));
				#endregion

				#region AssertVersion304Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V1"));
				//Verify Version 304 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(304).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V1"));
				#endregion

				#region AssertVersion305Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffRelationshipView_V1"));
				//Verify Version 305 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(305).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffRelationshipView_V1"));
				#endregion

				#region AssertVersion306Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "VATApplicabilityView_V1"));
				//Verify Version 306 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(306).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "VATApplicabilityView_V1"));
				#endregion

				#region AssertVersion307Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffAdditionalCodeView_V1"));
				//Verify Version 307 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(307).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffAdditionalCodeView_V1"));
				#endregion

				#region AssertVersion313Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				var type = typeof(ITableScript);
				var types = type.Assembly.GetTypes()
					.Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).Except(UpgradeScriptProvider.TableExclusionListForVersion313Upgrader)
					.ToList();
				foreach (var table in types)
				{
					Assert.False(TestDBHelper.ObjectExists(conn, "V", FormattableString.Invariant($"{table.Name}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
				}

				//Verify Version 313 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(313).UpgradeScript);

				foreach (var table in types)
				{
					Assert.True(TestDBHelper.ObjectExists(conn, "V", FormattableString.Invariant($"{table.Name}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
				}
				#endregion

				#region AssertVersion314Modifications

				// Restore and Pre-condition Check
				var refCusConditionLanguage = new RefCusConditionLanguage();
				var refLanguageType = new RefLanguageType();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refCusConditionLanguage.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionLanguageTableView_V1"));

				//Verify Version 314 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(314).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refCusConditionLanguage.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refCusConditionLanguage.TableName, "IX_RefCusConditionLanguage_ZXJ_ZX6_NKLanguage"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusConditionLanguage.TableName, "IX_RefCusConditionLanguage_ZXJ_ZX1_Condition_ZXJ_ZX6_NKLanguage"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionLanguageTableView_V1"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refLanguageType.TableName, "Constraint_ZX6_Language"));
				#endregion

				#region AssertVersion315Modifications

				// Restore and Pre-condition Check
				var refSysConfig = new RefSysConfig();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refSysConfig.TableName, "IX_RefSysConfig_ZRC_ZRT_NKConfigCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refSysConfig.TableName, "FK_RefSysConfig_RefSysConfigType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refSysConfig.TableName, "ZRC_ZRT_ConfigCode", "UNIQUEIDENTIFIER NOT NULL"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refSysConfig.TableName, "FK_RefSysConfig_RefSysConfigType", "ZRC_ZRT_ConfigCode", "RefSysConfigType (ZRT_PK)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refSysConfig.TableName, "IX_RefSysConfig_ZRC_ZRT_ConfigCode", "CREATE UNIQUE NONCLUSTERED INDEX [IX_RefSysConfig_ZRC_ZRT_ConfigCode] ON [RefSysConfig] ([ZRC_ZRT_ConfigCode] ASC)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refSysConfig.TableName, "ZRC_ZRT_NKConfigCode"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 315 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(315).UpgradeScript);

				Assert.False(TestDBHelper.IndexExists(conn, refSysConfig.TableName, "IX_RefSysConfig_ZRC_ZRT_ConfigCode"));
				Assert.False(TestDBHelper.ColumnExists(conn, refSysConfig.TableName, "ZRC_ZRT_ConfigCode"));
				Assert.True(TestDBHelper.ColumnExists(conn, refSysConfig.TableName, "ZRC_ZRT_NKConfigCode"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefSysConfigTableView_V1").Contains("[ZRC_ZRT_NKConfigCode]"));
				Assert.True(TestDBHelper.IndexExists(conn, refSysConfig.TableName, "IX_RefSysConfig_ZRC_ZRT_NKConfigCode"));
				#endregion

				#region AssertVersion316Modifications

				// Restore and Pre-condition Check

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refSysConfig.TableName, "Constraint_ZRC_SingleFieldValue"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refSysConfig.TableName, "ZRC_BinaryValue"));
				var constraint = @"([ZRC_StringValue]<>'' AND [ZRC_BitValue]=(0) AND [ZRC_DecimalValue]=(0) OR [ZRC_BitValue]<>(0) AND [ZRC_StringValue]='' AND [ZRC_DecimalValue]=(0) OR [ZRC_DecimalValue]<>(0) AND [ZRC_StringValue]='' AND [ZRC_BitValue]=(0))";
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refSysConfig.TableName, "Constraint_ZRC_SingleFieldValue", constraint));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refSysConfig.TableName, "Constraint_ZRC_SingleFieldValue"));
				Assert.False(TestDBHelper.ColumnExists(conn, refSysConfig.TableName, "ZRC_BinaryValue"));

				//Verify Version 316 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(316).UpgradeScript);

				Assert.False(TestDBHelper.CheckConstraintExists(conn, refSysConfig.TableName, "Constraint_ZRC_SingleFieldValue"));
				Assert.True(TestDBHelper.ColumnExists(conn, refSysConfig.TableName, "ZRC_BinaryValue"));
				#endregion

				#region AssertVersion317Modifications

				// Restore and Pre-condition Check
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refSysConfig.TableName, "Constraint_ZRC_SingleFieldValue"));

				//Verify Version 317 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(317).UpgradeScript);

				Assert.True(TestDBHelper.CheckConstraintExists(conn, refSysConfig.TableName, "Constraint_ZRC_SingleFieldValue"));
				#endregion

				#region AssertVersion318Modifications

				// Restore and Pre-condition Check
				var refAirlineCommodityCode = new RefAirlineCommodityCode();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refAirlineCommodityCode.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefAirlineCommodityCodeTableView_V1"));

				//Verify Version 318 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(318).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refAirlineCommodityCode.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refAirlineCommodityCode.TableName, "IX_RefAirlineCommodityCode_RAC_AirlineID_RAC_Code"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefAirlineCommodityCodeTableView_V1"));
				#endregion

				#region AssertVersion319Modifications

				// Restore and Pre-condition Check
				var refStlScript = new RefStlScript();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refStlScript.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefStlScriptTableView_V1"));
				//Verify Version 319 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(319).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refStlScript.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefStlScriptTableView_V1"));
				#endregion

				#region AssertVersion320Modifications

				// Restore and Pre-condition Check
				var uNDGAttributeZZ = new UNDGAttributeZZ();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGAttributeZZTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(uNDGAttributeZZ.TableName, "DAZ_IsSystem"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(uNDGAttributeZZ.TableName, "DAZ_IsSystem", "BIT NOT NULL DEFAULT 0"));

				var typeV = "V";
				var viewName = "UNDGAttributeZZTableView_V1";
				var createSQL = @"CREATE VIEW UNDGAttributeZZTableView_V1 AS
SELECT [DAZ_PK],
[DAZ_IsSystem],
[DAZ_Language],
[DAZ_Type],
[DAZ_Index],
[DAZ_Descriptor],
[DAZ_ParentCode],
[DAZ_ParentPK]
FROM UNDGAttributeZZ";
				var sql = $@"
IF NOT EXISTS(
SELECT * FROM sys.objects o
WHERE o.type = '{typeV}' AND o.name = '{viewName}'
)
EXEC dbo.sp_executesql @statement = N'{createSQL}'";

				restoreSb.AppendLine(sql);
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.ColumnExists(conn, uNDGAttributeZZ.TableName, "DAZ_IsSystem"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGAttributeZZTableView_V1").Contains("[DAZ_IsSystem]"));
				//Verify Version 320 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(320).UpgradeScript);

				Assert.False(TestDBHelper.ColumnExists(conn, uNDGAttributeZZ.TableName, "DAZ_IsSystem"));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "UNDGAttributeZZTableView_V1").Contains("[DAZ_IsSystem]"));
				#endregion

				#region AssertVersion321Modifications

				// Restore and Pre-condition Check
				var refCusTariffTypeLanguage = new RefCusTariffTypeLanguage();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refCusTariffTypeLanguage.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffTypeLanguageTableView_V1"));
				//Verify Version 321 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(321).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refCusTariffTypeLanguage.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffTypeLanguage.TableName, "IX_RefCusTariffTypeLanguage_ZXK_ZZI_TariffType_ZXK_ZX6_NKLanguage"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffTypeLanguage.TableName, "IX_RefCusTariffTypeLanguage_ZXK_ZZI_TariffType"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffTypeLanguage.TableName, "IX_RefCusTariffTypeLanguage_ZXK_ZX6_NKLanguage"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffTypeLanguageTableView_V1"));
				#endregion

				#region AssertVersion322Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				var sql_RefAccTaxRate = @"alter table RefAccTaxRate add ZAT_ThisIsTest varchar(10) not null default '';";
				TestDBHelper.ExecuteNonQuery(conn, sql_RefAccTaxRate);
				Assert.True(TestDBHelper.ColumnExists(conn, nameof(RefAccTaxRate), "ZAT_ThisIsTest"));
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, nameof(RefAccTaxRate), "ZAT_ThisIsTest"));
				Assert.False(TestDBHelper.ObjectExists(conn, "D", "DF_RefAccTaxRate_ZAT_ThisIsTest"));

				//Verify Version 322 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(322).UpgradeScript);
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, nameof(RefAccTaxRate), "ZAT_ThisIsTest"));
				Assert.True(TestDBHelper.ObjectExists(conn, "D", "DF_RefAccTaxRate_ZAT_ThisIsTest"));

				sql_RefAccTaxRate = @"alter table RefAccTaxRate drop constraint DF_RefAccTaxRate_ZAT_ThisIsTest;
alter table RefAccTaxRate drop column ZAT_ThisIsTest;";
				TestDBHelper.ExecuteNonQuery(conn, sql_RefAccTaxRate);
				#endregion

				#region AssertVersion323Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, "RefCusTariffBRCharacteristic"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffBRCharacteristicTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusTariffBRCharacteristicValue"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffBRCharacteristicValueTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusTariffBRCharacteristicAttribute"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffBRCharacteristicAttributeTableView_V1"));

				//Verify Version 323 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(323).UpgradeScript);

				AssertExistRefCusTariffBRCharacteristic(conn);
				AssertExistRefCusTariffBRCharacteristicValue(conn);
				AssertExistRefCusTariffBRCharacteristicAttribute(conn);

				#endregion

				#region AssertVersion324Modifications

				// Restore and Pre-condition Check
				var refDocOrgCusCode = new RefDocOrgCusCode();
				restoreSb.Clear();
				var restoreOldConstarintSql = @"ALTER TABLE [dbo].[RefDocOrgCusCode] DROP CONSTRAINT [CK_RefDocOrgCusCode_DOC_DocumentType];

ALTER TABLE [dbo].[RefDocOrgCusCode]  WITH CHECK ADD  CONSTRAINT [CK_RefDocOrgCusCode_DOC_DocumentType] CHECK  (([DOC_DocumentType]='HAW' OR [DOC_DocumentType]='AWB' OR [DOC_DocumentType]='ESI'));";
				TestDBHelper.ExecuteNonQuery(conn, restoreOldConstarintSql);

				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refDocOrgCusCode.TableName, "CK_RefDocOrgCusCode_DOC_DocumentType", "([DOC_DocumentType]=''HAW'' OR [DOC_DocumentType]=''AWB'' OR [DOC_DocumentType]=''ESI'')"));
				//Verify Version 324 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(324).UpgradeScript);

				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refDocOrgCusCode.TableName, "CK_RefDocOrgCusCode_DOC_DocumentType", "([DOC_DocumentType]=''HAW'' OR [DOC_DocumentType]=''AWB'' OR [DOC_DocumentType]=''ESI'')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refDocOrgCusCode.TableName, "CK_RefDocOrgCusCode_DOC_DocumentType", "([DOC_DocumentType]=''HAW'' OR [DOC_DocumentType]=''AWB'' OR [DOC_DocumentType]=''ESI'' OR [DOC_DocumentType]=''HBL'')"));
				#endregion

				#region AssertVersion325Modifications

				// Restore and Pre-condition Check
				var refCusConfiguration = new RefCusConfiguration();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refCusConfiguration.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConfigurationTableView_V1"));
				//Verify Version 325 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(325).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refCusConfiguration.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refCusConfiguration.TableName, "IX_RefCusConfiguration_ZZJ_RN_NKCustomsCountry_ZZJ_StartDate"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConfigurationTableView_V1"));
				#endregion

				#region AssertVersion326Modifications

				// Restore and Pre-condition Check
				var refCusApplicability = new RefCusApplicability();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_ZZA_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZA_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZX1_Conditions_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZZ2_Rate",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZX1_Conditions_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZZ2_Rate ON RefCusApplicability (ZZT_ZX1_Conditions, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZ2_Rate)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZX1_Conditions",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZX1_Conditions ON RefCusApplicability (ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZX1_Conditions)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup  ON RefCusApplicability (ZZT_ZY2_AdditionalCode, ZZT_StartDate, ZZT_ZZA_TradeGroup) WHERE ZZT_ZY2_AdditionalCode IS NOT NULL"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusApplicability.TableName, "FK_RefCusApplicability_RefCusTradeGroup2"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusApplicability.TableName, "ZZT_ZZA_SecondTradeGroup"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZX1_Conditions_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZZ2_Rate"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZX1_Conditions"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_ZZA_SecondTradeGroup"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusApplicability.TableName, "ZZT_ZZA_SecondTradeGroup"));

				//Verify Version 326 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(326).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refCusApplicability.TableName));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusApplicability.TableName, "ZZT_ZZA_SecondTradeGroup"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusApplicability.TableName, "FK_RefCusApplicability_RefCusTradeGroup2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusApplicabilityTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusApplicabilityTableView_V2").Contains("ZZT_ZZA_SecondTradeGroup"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZX1_Conditions_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZZ2_Rate"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZX1_Conditions"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_ZZA_SecondTradeGroup"));

				#endregion

				#region AssertVersion327Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZA_SecondTradeGroup"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZA_SecondTradeGroup"));

				//Verify Version 327 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(327).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refCusApplicability.TableName));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusApplicability.TableName, "ZZT_ZZA_SecondTradeGroup"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZA_SecondTradeGroup"));

				#endregion

				#region AssertVersion328Modifications

				// Restore and Pre-condition Check
				var refStlScript_V2 = new RefStlScript();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript_V2.TableName, "DF_RefStlScript_STL_DateType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refStlScript_V2.TableName, "CK_RefStlScript_STL_DateType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refStlScript_V2.TableName, "STL_DateType"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ColumnExists(conn, refStlScript_V2.TableName, "STL_DateType"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refStlScript_V2.TableName, "STL_DateType"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefStlScriptTableView_V2"));
				//Verify Version 328 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(328).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refStlScript_V2.TableName, "STL_DateType"));
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, refStlScript_V2.TableName, "STL_DateType"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefStlScriptTableView_V2"));
				#endregion

				#region AssertVersion329Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refStlScript_V2.TableName, "CK_RefStlScript_STL_DateType"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refStlScript_V2.TableName, "CK_RefStlScript_STL_DateType"));
				//Verify Version 329 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(329).UpgradeScript);
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refStlScript_V2.TableName, "CK_RefStlScript_STL_DateType"));
				#endregion

				#region AssertVersion330Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefSysConfigTableView_V1", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				restoreSb.Clear();
				restoreSb.AppendLine(@"CREATE VIEW RefSysConfigTableView_V1 AS
SELECT [ZRC_PK],
[ZRC_ZRT_NKConfigCode],
[ZRC_DecimalValue],
[ZRC_StringValue],
[ZRC_BitValue],
[ZRC_StartDate],
[ZRC_EndDate]
FROM RefSysConfig");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefSysConfigTableView_V1").Contains("ZRC_BinaryValue"));
				//Verify Version 330 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(330).UpgradeScript);
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefSysConfigTableView_V1").Contains("ZRC_BinaryValue"));
				#endregion

				#region AssertVersion331Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity", "[STL_DataGranularity]='TRN' OR [STL_DataGranularity]='MAH' OR [STL_DataGranularity]='MCO'"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V3", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity", "([STL_DataGranularity]=''TRN'' OR [STL_DataGranularity]=''MAH'' OR [STL_DataGranularity]=''MCO'' OR [STL_DataGranularity]=''DAY'')"));

				//Verify Version 331 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(331).UpgradeScript);
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity", "([STL_DataGranularity]=''TRN'' OR [STL_DataGranularity]=''MAH'' OR [STL_DataGranularity]=''MCO'' OR [STL_DataGranularity]=''DAY'')"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V1").Contains("WHERE STL_DataGranularity <> 'DAY'"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V2").Contains("WHERE STL_DataGranularity <> 'DAY'"));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V3").Contains("WHERE STL_DataGranularity <> 'DAY'"));
				#endregion

				#region AssertVersion332Modifications

				// Restore and Pre-condition Check
				var refExchangeRateZZ = new RefExchangeRateZZ();
				restoreSb.Clear();
				restoreOldConstarintSql = @"ALTER TABLE [dbo].[RefExchangeRateZZ] DROP CONSTRAINT [CK_RefExchangeRateZZ_ZZN_ExRateType];

ALTER TABLE [dbo].[RefExchangeRateZZ]  WITH CHECK ADD  CONSTRAINT [CK_RefExchangeRateZZ_ZZN_ExRateType] CHECK  (([ZZN_ExRateType]='CUE' OR [ZZN_ExRateType]='CUS' OR [ZZN_ExRateType]='CUD'));";
				TestDBHelper.ExecuteNonQuery(conn, restoreOldConstarintSql);

				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'')"));
				//Verify Version 332 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(332).UpgradeScript);

				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'' OR [ZZN_ExRateType]=''IAT'')"));
				#endregion

				#region AssertVersion333Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn", "CREATE UNIQUE NONCLUSTERED INDEX [IX_RefStlScript_STL_FeatureCode_STL_ActiveOn] ON [RefStlScript] ([STL_FeatureCode], [STL_ActiveOn])"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version"));
				Assert.True(TestDBHelper.IndexExists(conn, refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn"));

				//Verify Version 333 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(333).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version"));
				Assert.False(TestDBHelper.IndexExists(conn, refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn"));

				#endregion

				#region AssertVersion334Modifications
				//Verify Version 334 Upgrade Script
				Assert.AreEqual("SELECT 1 FROM RefStlScript WHERE 1 = 0", provider.GetUpgradeWrapperByVersion(334).UpgradeScript);

				#endregion

				#region AssertVersion335Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusConditionLanguage.TableName, "FK_RefCusConditionLanguage_RefCusCondition"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				restoreOldConstarintSql = @"ALTER TABLE [dbo].[RefCusConditionLanguage] DROP CONSTRAINT [CK_ZXJ_ZX6_NKLanguageNotEmpty];
ALTER TABLE [dbo].[RefCusConditionLanguage] WITH CHECK ADD CONSTRAINT [Constraint_ZXJ_ZX6_NKLanguageNotEmpty] CHECK (([ZXJ_ZX6_NKLanguage] <> ''));
ALTER TABLE [dbo].[RefCusConditionLanguage] DROP CONSTRAINT [CK_ZXJ_Comment_OR_ZXJ_SourceNotEmpty];
ALTER TABLE [dbo].[RefCusConditionLanguage] WITH CHECK ADD CONSTRAINT [Constraint_ZXJ_Comment_OR_ZXJ_SourceNotEmpty] CHECK (([ZXJ_Comment] <> '' OR [ZXJ_Source] <> ''));";
				TestDBHelper.ExecuteNonQuery(conn, restoreOldConstarintSql);
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusConditionLanguage.TableName, "Constraint_ZXJ_ZX6_NKLanguageNotEmpty", "([ZXJ_ZX6_NKLanguage]<>'''')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusConditionLanguage.TableName, "Constraint_ZXJ_Comment_OR_ZXJ_SourceNotEmpty", "([ZXJ_Comment]<>'''' OR [ZXJ_Source]<>'''')"));

				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusConditionLanguage.TableName, "FK_RefCusConditionLanguage_RefCusCondition"));
				//Verify Version 335 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(335).UpgradeScript);

				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusConditionLanguage.TableName, "FK_RefCusConditionLanguage_RefCusCondition"));
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusConditionLanguage.TableName, "Constraint_ZXJ_ZX6_NKLanguageNotEmpty", "([ZXJ_ZX6_NKLanguage]<>'''')"));
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusConditionLanguage.TableName, "Constraint_ZXJ_Comment_OR_ZXJ_SourceNotEmpty", "([ZXJ_Comment]<>'''' OR [ZXJ_Source]<>'''')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusConditionLanguage.TableName, "CK_ZXJ_ZX6_NKLanguageNotEmpty", "([ZXJ_ZX6_NKLanguage]<>'''')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusConditionLanguage.TableName, "CK_ZXJ_Comment_OR_ZXJ_SourceNotEmpty", "([ZXJ_Comment]<>'''' OR [ZXJ_Source]<>'''')"));
				#endregion

				#region AssertVersion336Modifications

				// Restore and Pre-condition Check
				var refAirlineProductCode = new RefAirlineProductCode();
				var refAirlineProductCodeCommodityCodePivot = new RefAirlineProductCodeCommodityCodePivot();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refAirlineProductCode.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefAirlineProductCodeTableView_V1"));

				Assert.False(TestDBHelper.TableExists(conn, refAirlineProductCodeCommodityCodePivot.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefAirlineProductCodeCommodityCodePivotTableView_V1"));
				//Verify Version 336 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(336).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refAirlineProductCode.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refAirlineProductCode.TableName, "IX_RefAirlineProductCode_RAR_AirlineID_RAR_Code"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefAirlineProductCodeTableView_V1"));

				Assert.True(TestDBHelper.TableExists(conn, refAirlineProductCodeCommodityCodePivot.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refAirlineProductCodeCommodityCodePivot.TableName, "IX_RefAirlineProductCodeCommodityCodePivot_RPC_AirlineID_RPC_RAR_RPC_RAC"));
				Assert.True(TestDBHelper.IndexExists(conn, refAirlineProductCodeCommodityCodePivot.TableName, "IX_RefAirlineProductCodeCommodityCodePivot_RPC_RAR"));
				Assert.True(TestDBHelper.IndexExists(conn, refAirlineProductCodeCommodityCodePivot.TableName, "IX_RefAirlineProductCodeCommodityCodePivot_RPC_RAC"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefAirlineProductCodeCommodityCodePivotTableView_V1"));
				#endregion

				#region AssertVersion337Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refAirlineProductCode.TableName, "CK_RefAirlineProductCode_RAR_CodeNotEmpty"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refAirlineProductCode.TableName, "CK_RefAirlineProductCodeg_RAR_CodeNotEmpty", "[RAR_Code] <> ''"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refAirlineProductCode.TableName, "CK_RefAirlineProductCodeg_RAR_CodeNotEmpty"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refAirlineProductCode.TableName, "CK_RefAirlineProductCode_RAR_CodeNotEmpty"));

				//Verify Version 337 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(337).UpgradeScript);

				Assert.False(TestDBHelper.CheckConstraintExists(conn, refAirlineProductCode.TableName, "CK_RefAirlineProductCodeg_RAR_CodeNotEmpty"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refAirlineProductCode.TableName, "CK_RefAirlineProductCode_RAR_CodeNotEmpty"));
				#endregion

				#region AssertVersion338Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "VATApplicabilityView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_StartDateUnique"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_EndDateUnique"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));

				restoreSb.AppendLine("ALTER TABLE RefCusTariffNationalCode ALTER COLUMN ZZW_ZZF_NKTaxOrFeeCode VARCHAR(3) NOT NULL");
				restoreSb.AppendLine("ALTER TABLE RefCusVATApplicability ALTER COLUMN ZX5_ZZF_NKTaxOrFeeCode VARCHAR(3) NOT NULL");
				restoreSb.AppendLine("ALTER TABLE RefCusTariff ALTER COLUMN ZZ1_ZZF_NKTaxOrFeeCode VARCHAR(3) NOT NULL");

				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode", "CREATE UNIQUE INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode ON RefCusTariffNationalCode (ZZW_ZZZ_NKDataGrouping, ZZW_ZZ1_Tariff, ZZW_ZZF_NKTaxOrFeeCode, ZZW_NationalCode)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate",
					"CREATE NONCLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate ON RefCusTariffNationalCode (ZZW_ZZZ_NKDataGrouping, ZZW_ZZ1_Tariff, ZZW_StartDate ASC)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusTariffNationalCode", false, new[] { new DbColumn("ZZW_ZZF_NKTaxOrFeeCode", "varchar", 3, false, "") }), "ZZW_ZZF_NKTaxOrFeeCode existed with length = 3");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusVATApplicability", false, new[] { new DbColumn("ZX5_ZZF_NKTaxOrFeeCode", "varchar", 3, false, "('')") }), "ZX5_ZZF_NKTaxOrFeeCode existed with length = 3");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusTariff", false, new[] { new DbColumn("ZZ1_ZZF_NKTaxOrFeeCode", "varchar", 3, false, "('')") }), "ZZ1_ZZF_NKTaxOrFeeCode existed with length = 3");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "VATApplicabilityView_V1"));

				//Verify Version 338 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(338).UpgradeScript);

				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusTariffNationalCode", false, new[] { new DbColumn("ZZW_ZZF_NKTaxOrFeeCode", "varchar", 4, false, "") }), "ZZW_ZZF_NKTaxOrFeeCode existed with length = 4");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusVATApplicability", false, new[] { new DbColumn("ZX5_ZZF_NKTaxOrFeeCode", "varchar", 4, false, "('')") }), "ZX5_ZZF_NKTaxOrFeeCode existed with length = 4");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusTariff", false, new[] { new DbColumn("ZZ1_ZZF_NKTaxOrFeeCode", "varchar", 4, false, "('')") }), "ZZ1_ZZF_NKTaxOrFeeCode existed with length = 4");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "VATApplicabilityView_V1", false, new[] { new DbColumn("ZX5_ZZF_NKTaxOrFeeCode", "varchar", 4, false, "") }), "ZX5_ZZF_NKTaxOrFeeCode existed with length = 4");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "TariffView_V1", false, new[] { new DbColumn("ZZ1_ZZF_NKTaxOrFeeCode", "varchar", 4, false, "") }), "ZZ1_ZZF_NKTaxOrFeeCode existed with length = 4");
				Assert.True(TestDBHelper.IndexExists(conn, "RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA"));
				Assert.True(TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate"));
				Assert.False(TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA"));

				#endregion

				#region AssertVersion339Modifications

				// Restore and Pre-condition Check
				var refCusQuota = new RefCusQuota();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refCusQuota.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusQuotaTableView_V1"));
				//Verify Version 339 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(339).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, refCusQuota.TableName));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusQuotaTableView_V1"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusQuota.TableName, "IX_RefCusQuota_ZXQ_ZZZ_NKDataGrouping_ZXQ_OrderNumber_ZXQ_StartDate"));
				#endregion

				#region AssertVersion340Modifications

				// Restore and Pre-condition Check
				var refDataGrouping = new RefDataGrouping();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefDataGrouping", "IX_RefDataGrouping_ZZZ_ZZZ_Grouping"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, refDataGrouping.TableName, "IX_RefDataGrouping_ZZZ_ZZZ_Grouping"));

				//Verify Version 340 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(340).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, refDataGrouping.TableName, "IX_RefDataGrouping_ZZZ_ZZZ_Grouping"));

				#endregion

				#region AssertVersion341Modifications

				// Restore and Pre-condition Check
				var refCusConditionValueTypeLanguage = new RefCusConditionValueTypeLanguage();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusConditionValueTypeLanguage.TableName, "DF_RefCusConditionValueTypeLanguage_ZXX_Description"));
				restoreSb.AppendLine("ALTER TABLE RefCusConditionValueTypeLanguage ALTER COLUMN ZXX_Description NVARCHAR(500)");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusConditionValueTypeLanguage.TableName, false, new[] { new DbColumn("ZXX_Description", "nvarchar", 500, true, "") }), "ZXX_Description is NULL");

				//Verify Version 341 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(341).UpgradeScript);
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusConditionValueTypeLanguage.TableName, false, new[] { new DbColumn("ZXX_Description", "nvarchar", 500, false, "") }), "ZXX_Description is not NULL");

				#endregion

				#region AssertVersion342Modifications

				// Restore and Pre-condition Check
				var refCusTariffNationalCode = new RefCusTariffNationalCode();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffNationalCode.TableName, "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffNationalCode.TableName, "DF_RefCusTariffNationalCode_ZZW_Description"));
				restoreSb.AppendLine("ALTER TABLE RefCusTariffNationalCode ALTER COLUMN ZZW_NationalCode VARCHAR(3) NOT NULL");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffView_V1"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusTariffNationalCode.TableName, false, new[] { new DbColumn("ZZW_NationalCode", "varchar", 3, false, "('')") }), "ZZW_NationalCode existed with length = 3");
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffNationalCode.TableName, "DF_RefCusTariffNationalCode_ZZW_Description"));

				//Verify Version 342 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(342).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffView_V1"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusTariffNationalCode.TableName, false, new[] { new DbColumn("ZZW_NationalCode", "varchar", 10, false, "('')") }), "ZZW_NationalCode existed with length = 10");
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffNationalCode.TableName, "CK_RefCusTariffNationalCode_ZZW_NationalCode", "([ZZW_NationalCode]<>'''')"), "CK_RefCusTariffNationalCode_ZZW_NationalCode still existed");
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffNationalCode.TableName, "DF_RefCusTariffNationalCode_ZZW_Description"));
				#endregion

				#region AssertVersion343Modifications

				// Restore and Pre-condition Check
				var refCusTariffUOM = new RefCusTariffUOM();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffUOM.TableName, "CK_RefCusTariffUOM_ZZ8_Type"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refCusTariffUOM.TableName, "CK_RefCusTariffUOM_ZZ8_Type"));

				//Verify Version 343 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(343).UpgradeScript);
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffUOM.TableName, "CK_RefCusTariffUOM_ZZ8_Type", "([ZZ8_Type]=''CU5'' OR [ZZ8_Type]=''CU4'' OR [ZZ8_Type]=''CU3'' OR [ZZ8_Type]=''CU2'' OR [ZZ8_Type]=''AD1'' OR [ZZ8_Type]=''RU1'' OR [ZZ8_Type]=''CU1'')"));

				#endregion

				#region AssertVersion344Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusApplicability.TableName, "DF_RefCusApplicability_ZZT_AdditionalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusApplicability.TableName, "DF_RefCusApplicability_ZZT_OrderNumber"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_AdditionalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_OrderNumber"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("IF", "GetRatesBySingleCriteriaSet_V1", "FUNCTION"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusApplicability.TableName, "CK_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_AdditionalCode_ZZT_OrderNumber"));
				restoreSb.AppendLine("ALTER TABLE RefCusApplicability ALTER COLUMN ZZT_AdditionalCode NVARCHAR(15)");
				restoreSb.AppendLine("ALTER TABLE RefCusApplicability ALTER COLUMN ZZT_OrderNumber NVARCHAR(15)");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusApplicability.TableName, false, new[] { new DbColumn("ZZT_AdditionalCode", "nvarchar", 15, true, "") }), "ZZT_AdditionalCode is NULL");
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusApplicability.TableName, false, new[] { new DbColumn("ZZT_OrderNumber", "nvarchar", 15, true, "") }), "ZZT_OrderNumber is NULL");

				//Verify Version 344 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(344).UpgradeScript);

				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusApplicability.TableName, false, new[] { new DbColumn("ZZT_AdditionalCode", "nvarchar", 15, false, "('')") }), "ZZT_AdditionalCode is not NULL and default '' value");
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusApplicability.TableName, false, new[] { new DbColumn("ZZT_OrderNumber", "nvarchar", 15, false, "('')") }), "ZZT_OrderNumber is not NULL and default '' value");
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_AdditionalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusApplicability.TableName, "CK_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_AdditionalCode_ZZT_OrderNumber", "([ZZT_ZY2_AdditionalCode] IS NULL OR [ZZT_ZY2_AdditionalCode] IS NOT NULL AND [ZZT_AdditionalCode]='''' AND [ZZT_OrderNumber]='''')"));
				#endregion

				#region AssertVersion345Modifications

				// Restore and Pre-condition Check
				var refCusTariff = new RefCusTariff();
				var refCusRateType = new RefCusRateType();
				var refCusTradeGroupLanguage = new RefCusTradeGroupLanguage();
				var refCusTariffAdditionalCodeLanguage = new RefCusTariffAdditionalCodeLanguage();
				var refCusTariffAdditionalCodeCategory = new RefCusTariffAdditionalCodeCategory();
				var refCusRateCodeLanguage = new RefCusRateCodeLanguage();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariff.TableName, "DF_RefCusTariff_ZZ1_ZZF_NKTaxOrFeeCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusRateType.TableName, "DF_RefCusRateType_ZZR_CustomsValueFormula"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTradeGroupLanguage.TableName, "DF_RefCusTradeGroupLanguage_ZXD_Description"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffAdditionalCodeLanguage.TableName, "DF_RefCusTariffAdditionalCodeLanguage_ZY4_Description"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffAdditionalCodeCategory.TableName, "DF_RefCusTariffAdditionalCodeCategory_ZY3_Category"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffAdditionalCodeCategory.TableName, "DF_RefCusTariffAdditionalCodeCategory_ZY3_Description"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusRateCodeLanguage.TableName, "DF_RefCusRateCodeLanguage_ZXC_Description"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariff.TableName, "DF_RefCusTariff_ZZ1_ZZF_NKTaxOrFeeCode"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusRateType.TableName, "DF_RefCusRateType_ZZR_CustomsValueFormula"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTradeGroupLanguage.TableName, "DF_RefCusTradeGroupLanguage_ZXD_Description"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffAdditionalCodeLanguage.TableName, "DF_RefCusTariffAdditionalCodeLanguage_ZY4_Description"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffAdditionalCodeCategory.TableName, "DF_RefCusTariffAdditionalCodeCategory_ZY3_Category"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffAdditionalCodeCategory.TableName, "DF_RefCusTariffAdditionalCodeCategory_ZY3_Description"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusRateCodeLanguage.TableName, "DF_RefCusRateCodeLanguage_ZXC_Description"));

				//Verify Version 345 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(345).UpgradeScript);

				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariff.TableName, "DF_RefCusTariff_ZZ1_ZZF_NKTaxOrFeeCode"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusRateType.TableName, "DF_RefCusRateType_ZZR_CustomsValueFormula"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTradeGroupLanguage.TableName, "DF_RefCusTradeGroupLanguage_ZXD_Description"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffAdditionalCodeLanguage.TableName, "DF_RefCusTariffAdditionalCodeLanguage_ZY4_Description"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffAdditionalCodeCategory.TableName, "DF_RefCusTariffAdditionalCodeCategory_ZY3_Category"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffAdditionalCodeCategory.TableName, "DF_RefCusTariffAdditionalCodeCategory_ZY3_Description"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusRateCodeLanguage.TableName, "DF_RefCusRateCodeLanguage_ZXC_Description"));

				#endregion

				#region AssertVersion346Modifications

				// Restore and Pre-condition Check
				var refCarrierCodeLanguage = new RefCarrierCodeLanguage();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refCarrierCodeLanguage.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCarrierCodeLanguageTableView_V1"));
				//Verify Version 346 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(346).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refCarrierCodeLanguage.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refCarrierCodeLanguage.TableName, "IX_RefCarrierCodeLanguage_ZCL_ZX6_NKLanguage_ZCL_ZZ4_CarrierCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCarrierCodeLanguage.TableName, "IX_RefCarrierCodeLanguage_ZCL_ZZ4_CarrierCode"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCarrierCodeLanguageTableView_V1"));
				#endregion

				#region AssertVersion347Modifications

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(347).UpgradeScript);

				#endregion

				#region AssertVersion348Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffNationalCodeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusVATApplicabilityTableView_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffTableView_V2"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffNationalCodeTableView_V2"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusVATApplicabilityTableView_V2"));

				//Verify Version 348 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(348).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffTableView_V2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffNationalCodeTableView_V2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusVATApplicabilityTableView_V2"));

				#endregion

				#region AssertVersion349Modifications

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(349).UpgradeScript);
				AssertDefaultValue(conn, false);

				#endregion

				#region AssertVersion350Modifications

				// Restore and Pre-condition Check
				var refStlFieldMapping = new RefStlFieldMapping();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refStlFieldMapping.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefStlFieldMappingTableView_V1"));
				//Verify Version 350 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(350).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, refStlFieldMapping.TableName));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefStlFieldMappingTableView_V1"));
				Assert.True(TestDBHelper.IndexExists(conn, refStlFieldMapping.TableName, "IX_RefStlFieldMapping_SFM_FeatureCode"));
				#endregion

				#region AssertVersion351Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffNationalCodeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffNationalCodeTableView_V3", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffNationalCodeTableView_V2"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffNationalCodeTableView_V3"));

				//Verify Version 351 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(351).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffNationalCodeTableView_V2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffNationalCodeTableView_V3"));
				#endregion

				#region AssertVersion352Modifications

				// Restore and Pre-condition Check
				var refStlFieldMapping_V2 = new RefStlFieldMapping();
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefStlFieldMappingTableView_V2"));
				//Verify Version 352 Upgrade Script

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(352).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, refStlFieldMapping_V2.TableName));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefStlFieldMappingTableView_V2"));
				Assert.True(TestDBHelper.ColumnExists(conn, refStlFieldMapping_V2.TableName, "SFM_Reference5"));
				Assert.True(TestDBHelper.ColumnExists(conn, refStlFieldMapping_V2.TableName, "SFM_Category"));
				Assert.True(TestDBHelper.ColumnExists(conn, refStlFieldMapping_V2.TableName, "SFM_PriceItemCode"));
				Assert.True(TestDBHelper.ColumnExists(conn, refStlFieldMapping_V2.TableName, "SFM_ServiceOccuredUTC"));
				Assert.True(TestDBHelper.ColumnExists(conn, refStlFieldMapping_V2.TableName, "SFM_ClientStaffCode"));
				#endregion

				#region AssertVersion353Modifications

				// Restore and Pre-condition Check
				var refCusRateCode = new RefCusRateCode();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusRateCodeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusRateCode.TableName, "IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusRateCode.TableName, "FK_RefCusRateCode_RefDataGrouping"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusRateCode.TableName, "ZY1_ZZZ_NKDataGrouping"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusRateCode.TableName, "ZY1_ZZZ_NKDataGrouping"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusRateCode.TableName, "FK_RefCusRateCode_RefDataGrouping"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusRateCodeTableView_V2"));

				//Verify Version 353 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(353).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refCusRateCode.TableName, "ZY1_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusRateCode.TableName, "FK_RefCusRateCode_RefDataGrouping"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusRateCodeTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusRateCodeTableView_V2").Contains("ZY1_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusRateCodeTableView_V1"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusRateCode.TableName, "IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode"));

				#endregion

				#region AssertVersion354Modifications

				// Restore and Pre-condition Check
				var refCusConditionType = new RefCusConditionType();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusRateCode.TableName, "IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusConditionType.TableName, "CK_RefCusConditionType_ZX2_ConditionClass"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, refCusRateCode.TableName, "IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode"));
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusConditionType.TableName, "CK_RefCusConditionType_ZX2_ConditionClass", "([ZX2_ConditionClass]=''CLASS'' OR [ZX2_ConditionClass]=''RATE'' OR [ZX2_ConditionClass]=''CTRL'' OR [ZX2_ConditionClass]=''VAT'' OR [ZX2_ConditionClass]=''RISK'')"));

				//Verify Version 354 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(354).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refCusRateCode.TableName, "IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusConditionType.TableName, "CK_RefCusConditionType_ZX2_ConditionClass", "([ZX2_ConditionClass]=''CLASS'' OR [ZX2_ConditionClass]=''RATE'' OR [ZX2_ConditionClass]=''CTRL'' OR [ZX2_ConditionClass]=''VAT'' OR [ZX2_ConditionClass]=''RISK'')"));

				#endregion

				#region AssertVersion355Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(355).UpgradeScript);

				#endregion

				#region AssertVersion356Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate ON RefExchangeRateZZ(ZZN_RN_NKCountry ASC, ZZN_ExRateType ASC, ZZN_RX_NKExCurrency ASC, ZZN_StartDate ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.IndexExists(conn, refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate"));
				Assert.False(TestDBHelper.IndexExists(conn, refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(356).UpgradeScript);

				Assert.False(TestDBHelper.IndexExists(conn, refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));
				#endregion

				#region AssertVersion358Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusConditionTypeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusConditionTypeTableView_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V2"));

				//Verify Version 358 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(358).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V2"));
				#endregion

				#region AssertVersion359Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate ON RefExchangeRateZZ(ZZN_RN_NKCountry ASC, ZZN_ExRateType ASC, ZZN_RX_NKExCurrency ASC, ZZN_StartDate DESC, ZZN_EndDate ASC) INCLUDE (ZZN_Rate)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));

				//Verify Version 359 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(359).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refExchangeRateZZ.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));

				#endregion

				#region AssertVersion360Modifications

				// Restore and Pre-condition Check
				var refAirlineCommodityCodeRAC_SpecialHandling = new RefAirlineCommodityCode();

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefAirlineCommodityCodeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refAirlineCommodityCodeRAC_SpecialHandling.TableName, "DF_RefAirlineCommodityCode_RAC_SpecialHandlingCodes"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refAirlineCommodityCodeRAC_SpecialHandling.TableName, "RAC_SpecialHandlingCodes"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refAirlineCommodityCodeRAC_SpecialHandling.TableName, "RAC_SpecialHandlingCodes"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refAirlineCommodityCodeRAC_SpecialHandling.TableName, "DF_RefAirlineCommodityCode_RAC_SpecialHandlingCodes"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefAirlineCommodityCodeTableView_V2"));

				//Verify Version 360 Upgrade Script

				string script = string.Format(CultureInfo.InvariantCulture, @"
	 INSERT INTO {0}
		   ([RAC_PK]
		   ,[RAC_AirlineID]
		   ,[RAC_Code]
		   ,[RAC_Description])
	 VALUES
		   (NEWID()
		   ,'176'
		   ,'0308'
		   ,'FISH (SMOKED)') ",
				refAirlineCommodityCodeRAC_SpecialHandling.TableName);

				TestDBHelper.ExecuteNonQuery(conn, script);

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(360).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refAirlineCommodityCodeRAC_SpecialHandling.TableName, "RAC_SpecialHandlingCodes"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefAirlineCommodityCodeTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefAirlineCommodityCodeTableView_V2").Contains("RAC_SpecialHandlingCodes"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefAirlineCommodityCodeTableView_V1"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refAirlineCommodityCodeRAC_SpecialHandling.TableName, "DF_RefAirlineCommodityCode_RAC_SpecialHandlingCodes"));

				#endregion

				#region AssertVersion361Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				var testSql_V361 = @"ALTER TABLE RefAccTaxRate ADD ZAT_V361 VARCHAR(10) CONSTRAINT DF_RefAccTaxRate DEFAULT ('') NOT NULL;";
				TestDBHelper.ExecuteNonQuery(conn, testSql_V361);
				Assert.True(TestDBHelper.ColumnExists(conn, nameof(RefAccTaxRate), "ZAT_V361"));
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, nameof(RefAccTaxRate), "ZAT_V361"));
				Assert.False(TestDBHelper.ObjectExists(conn, "D", "DF_RefAccTaxRate_ZAT_V361"));

				//Verify Version 361 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(361).UpgradeScript);
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, nameof(RefAccTaxRate), "ZAT_V361"));
				Assert.True(TestDBHelper.ObjectExists(conn, "D", "DF_RefAccTaxRate_ZAT_V361"));

				testSql_V361 = @"alter table RefAccTaxRate drop constraint DF_RefAccTaxRate_ZAT_V361;
alter table RefAccTaxRate drop column ZAT_V361;";
				TestDBHelper.ExecuteNonQuery(conn, testSql_V361);
				#endregion

				#region AssertVersion362Modifications

				// Restore and Pre-condition Check
				var refUNLOCO = new RefUNLOCO();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refUNLOCO.TableName));
				Assert.False(TestDBHelper.IndexExists(conn, refUNLOCO.TableName, "IX_RefUNLOCO_RL_Code"));

				//Verify Version 362 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(362).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refUNLOCO.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCO.TableName, "IX_RefUNLOCO_RL_Code"));
				#endregion

				#region AssertVersion363Modifications

				// Restore and Pre-condition Check
				var refUNLOCOUtcOffset = new RefUNLOCOUtcOffset();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refUNLOCOUtcOffset.TableName, "FK_RefUNLOCOUtcOffset_RefUNLOCO"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.TableExists(conn, refUNLOCOUtcOffset.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefUNLOCOUtcOffsetTableView_V1"));

				//Verify Version 363 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(363).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refUNLOCOUtcOffset.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_RL_NKCode"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefUNLOCOUtcOffsetTableView_V1"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refUNLOCOUtcOffset.TableName, "DF_RefUNLOCOUtcOffset_RLO_RL_NKCode"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refUNLOCOUtcOffset.TableName, "DF_RefUNLOCOUtcOffset_RLO_OffsetMinutesFromUtc"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_EndTimeUtc"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_RL_NKCode"));
				#endregion

				#region AssertVersion364Modifications

				// Restore and Pre-condition Check
				var refCusTariffBRCharacteristic = new RefCusTariffBRCharacteristic();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ1_Tariff"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ5_Nomenclature"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature_ZB1_ZZ1_Tariff"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusTariffBRCharacteristic.TableName, "FK_RefCusTariffBRCharacteristic_RefCusNomenclatureGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffBRCharacteristic.TableName, "DF_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffBRCharacteristic.TableName, "ZB1_ZZ5_Nomenclature"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffBRCharacteristic.TableName, "ZB1_ZZ5_Nomenclature"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffBRCharacteristic.TableName, "DF_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature"));
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature_ZB1_ZZ1_Tariff", "(ZB1_ZZ1_Tariff IS NOT NULL OR ZB1_ZZ5_Nomenclature IS NOT NULL)"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ5_Nomenclature"));

				//Verify Version 364 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(364).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffBRCharacteristic.TableName, "ZB1_ZZ5_Nomenclature"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffBRCharacteristic.TableName, "FK_RefCusTariffBRCharacteristic_RefCusNomenclatureGroup"));
				#endregion

				#region AssertVersion365Modifications

				// Restore and Pre-condition Check
				refCusTariffBRCharacteristic = new RefCusTariffBRCharacteristic();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusTariffBRCharacteristic.TableName, "ZB1_ZZ1_Tariff", "UNIQUEIDENTIFIER NOT NULL"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ1_Tariff"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ5_Nomenclature"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature_ZB1_ZZ1_Tariff"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.TableColumnsExist(conn, "RefCusTariffBRCharacteristic", false, new[] { new DbColumn("ZB1_ZZ1_Tariff", "uniqueidentifier", -1, true, "") }), "RefCusTariffBRCharacteristic ZB1_ZZ1_Tariff column existed with correct type and length and default value");
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffBRCharacteristic.TableName, "ZB1_ZZ5_Nomenclature"));
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature_ZB1_ZZ1_Tariff", "(ZB1_ZZ1_Tariff IS NULL AND ZB1_ZZ5_Nomenclature IS NOT NULL) OR (ZB1_ZZ1_Tariff IS NOT NULL AND ZB1_ZZ5_Nomenclature IS NULL)"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ5_Nomenclature"));

				//Verify Version 365 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(365).UpgradeScript);
				AssertExistRefCusTariffBRCharacteristicVersion365(conn);
				#endregion

				#region AssertVersion366Modifications

				// Restore and Pre-Condition Check
				var refCusTariffAttributeName = new RefCusTariffAttributeName();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refCusTariffAttributeName.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAttributeNameTableView_V1"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffAttributeName.TableName, "IX_RefCusTariffAttributeName_ZY6_ZZZ_NKDataGrouping_ZY6_ZZI_NKTariffType_ZY6_Name"));

				// Verify Version 366 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(366).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refCusTariffAttributeName.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffAttributeName.TableName, "IX_RefCusTariffAttributeName_ZY6_ZZZ_NKDataGrouping_ZY6_ZZI_NKTariffType_ZY6_Name"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffAttributeName.TableName, "DF_RefCusTariffAttributeName_ZY6_PK"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusTariffAttributeName.TableName, "DF_RefCusTariffAttributeName_ZY6_ColumnCaption"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffAttributeName.TableName, "CK_RefCusTariffAttributeName_ZY6_Description", "([ZY6_Description]<>'''')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffAttributeName.TableName, "CK_RefCusTariffAttributeName_ZY6_Name", "([ZY6_Name]<>'''')"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffAttributeName.TableName, "FK_RefCusTariffAttributeName_RefDataGrouping"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffAttributeName.TableName, "FK_RefCusTariffAttributeName_RefCusTariffType"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAttributeNameTableView_V1"));
				#endregion

				#region AssertVersion368Modifications

				// Restore and Pre-condition Check
				refCusTariffBRCharacteristic = new RefCusTariffBRCharacteristic();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_Style"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType", "(ZB1_CharacteristicType='NVE' OR ZB1_CharacteristicType='NCM' OR ZB1_CharacteristicType='NCMTE')"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_Style", "(ZB1_Style='BOOLEAN' OR ZB1_Style='STRING' OR ZB1_Style='NUMBER' OR ZB1_Style='LIST')"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 368 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(368).UpgradeScript);
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType", "([ZB1_CharacteristicType]=''NVE'' OR [ZB1_CharacteristicType]=''NCM'' OR [ZB1_CharacteristicType]=''NCMTE''"));
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_Style", "([ZB1_Style]=''BOOLEAN'' OR [ZB1_Style]=''STRING'' OR [ZB1_Style]=''NUMBER'' OR [ZB1_Style]=''LIST'')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType", "([ZB1_CharacteristicType]=''NVE'' OR [ZB1_CharacteristicType]=''NCM'' OR [ZB1_CharacteristicType]=''NCMTE'' OR [ZB1_CharacteristicType]=''LPC'' OR [ZB1_CharacteristicType]=''LPCT'')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_Style", "([ZB1_Style]=''BOOLEAN'' OR [ZB1_Style]=''STRING'' OR [ZB1_Style]=''NUMBER'' OR [ZB1_Style]=''LIST'' OR [ZB1_Style]=''COMPOSED'' OR [ZB1_Style]=''DATE'')"));

				#endregion

				#region AssertVersion369Modifications

				// Restore and Pre-Condition Check
				var refHarbourRate_369 = new RefHarbourRate();

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refHarbourRate_369.TableName, "DF_RefHarbourRate_ZXF_PortTaxType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refHarbourRate_369.TableName, "CK_RefHarbourRate_ZXF_PortTaxType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refHarbourRate_369.TableName, "ZXF_PortTaxType"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refHarbourRate_369.TableName, "ZXF_PortTaxType"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refHarbourRate_369.TableName, "ZXF_PortTaxType"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefHarbourRateTableView_V2"));

				//Verify Version 369 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(369).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refHarbourRate_369.TableName, "ZXF_PortTaxType"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refHarbourRate_369.TableName, "DF_RefHarbourRate_ZXF_PortTaxType"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefHarbourRateTableView_V2"));

				#endregion

				#region AssertVersion370Modifications

				// Restore and Pre-Condition Check
				var refHarbourRate_370 = new RefHarbourRate();

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refHarbourRate_370.TableName, "CK_RefHarbourRate_ZXF_PortTaxType"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.CheckConstraintExists(conn, refHarbourRate_370.TableName, "CK_RefHarbourRate_ZXF_PortTaxType"));

				//Verify Version 370 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(370).UpgradeScript);
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refHarbourRate_370.TableName, "CK_RefHarbourRate_ZXF_PortTaxType", "(len([ZXF_PortTaxType])=(0) OR len([ZXF_PortTaxType])=(3))"));

				#endregion

				#region AssertVersion371Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(371).UpgradeScript);

				#endregion

				#region AssertVersion372Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(372).UpgradeScript);

				#endregion

				#region AssertVersion373Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(373).UpgradeScript);

				#endregion

				#region AssertVersion374Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(374).UpgradeScript);

				#endregion

				#region AssertVersion375Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(375).UpgradeScript);

				#endregion

				#region AssertVersion376Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(376).UpgradeScript);

				#endregion

				#region AssertVersion377Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(377).UpgradeScript);

				#endregion

				#region AssertVersion378Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(378).UpgradeScript);

				#endregion

				#region AssertVersion379Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(379).UpgradeScript);

				#endregion

				#region AssertVersion380Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(380).UpgradeScript);

				#endregion

				#region AssertVersion381Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(381).UpgradeScript);

				#endregion

				#region AssertVersion382Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(382).UpgradeScript);

				#endregion

				#region AssertVersion383Modifications

				// Restore and Pre-condition Check
				var refCusRateType_383 = new RefCusRateType();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusRateTypeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusRateType_383.TableName, "DF_RefCusRateType_ZZR_IsExport"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusRateType_383.TableName, "ZZR_IsExport"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refCusRateType_383.TableName, "ZZR_IsExport"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusRateType_383.TableName, "ZZR_IsExport"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusRateTypeTableView_V2"));

				//Verify Version 383 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(383).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refCusRateType_383.TableName, "ZZR_IsExport"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusRateType_383.TableName, "DF_RefCusRateType_ZZR_IsExport"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusRateTypeTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusRateTypeTableView_V2").Contains("ZZR_IsExport"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusRateTypeTableView_V1"));

				#endregion

				#region AssertVersion384Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_RL_NKCode_RLO_StartTimeUtc"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_PK", "CREATE UNIQUE NONCLUSTERED INDEX [IX_RefUNLOCOUtcOffset_RLO_PK] ON [RefUNLOCOUtcOffset] ([RLO_PK] ASC)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refUNLOCOUtcOffset.TableName, "FK_RefUNLOCOUtcOffset_RefUNLOCO"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_EndTimeUtc"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refUNLOCOUtcOffset.TableName, "CK_RefUNLOCOUtcOffset_RLO_EndTimeUtc_RLO_StartTimeUtc", "([RLO_EndTimeUtc]>[RLO_StartTimeUtc])"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_RL_NKCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refUNLOCOUtcOffset.TableName, "CK_RefUNLOCOUtcOffset_RLO_RL_NKCode", "(LEN([RLO_RL_NKCode])=(5))"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "PK_RefUNLOCOUtcOffset"));
				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_PK"));
				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_RL_NKCode"));
				Assert.False(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_RL_NKCode_RLO_StartTimeUtc"));

				Assert.True(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "CK_RefUNLOCOUtcOffset_RLO_EndTimeUtc_RLO_StartTimeUtc"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "CK_RefUNLOCOUtcOffset_RLO_RL_NKCode"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_EndTimeUtc"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_RL_NKCode"));

				Assert.False(TestDBHelper.ForeignKeyExists(conn, refUNLOCOUtcOffset.TableName, "FK_RefUNLOCOUtcOffset_RefUNLOCO"));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(384).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "PK_RefUNLOCOUtcOffset"));
				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_RL_NKCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_RL_NKCode_RLO_StartTimeUtc"));
				Assert.False(TestDBHelper.IndexExists(conn, refUNLOCOUtcOffset.TableName, "IX_RefUNLOCOUtcOffset_RLO_PK"));

				Assert.True(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_EndTimeUtc"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_RL_NKCode"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "CK_RefUNLOCOUtcOffset_RLO_EndTimeUtc_RLO_StartTimeUtc"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refUNLOCOUtcOffset.TableName, "CK_RefUNLOCOUtcOffset_RLO_RL_NKCode"));

				Assert.True(TestDBHelper.ForeignKeyExists(conn, refUNLOCOUtcOffset.TableName, "FK_RefUNLOCOUtcOffset_RefUNLOCO"));

				#endregion

				#region AssertVersion385Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(385).UpgradeScript);

				#endregion

				#region AssertVersion386Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(386).UpgradeScript);

				#endregion

				#region AssertVersion387Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(387).UpgradeScript);

				#endregion

				#region AssertVersion388Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(388).UpgradeScript);

				#endregion

				#region AssertVersion389Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(389).UpgradeScript);

				#endregion

				#region AssertVersion390Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(390).UpgradeScript);

				#endregion

				#region AssertVersion391Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(391).UpgradeScript);

				#endregion

				#region AssertVersion392Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(392).UpgradeScript);

				#endregion

				#region AssertVersion393Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(393).UpgradeScript);

				#endregion

				#region AssertVersion394Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_StartDateUnique"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_EndDateUnique"));
				// Add old indexes to assert they are removed.
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA",
					"CREATE UNIQUE INDEX IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA ON RefCusVATApplicability(ZX5_ZZ1_Tariff ASC, ZX5_ZZW_TariffNationalCode ASC, ZX5_ZZF_NKTaxOrFeeCode ASC, ZX5_AdditionalCode ASC, ZX5_StartDate ASC, ZX5_ZZA_TradeGroup)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA ON RefCusVATApplicability (ZX5_ZZ1_Tariff ASC, ZX5_ZZW_TariffNationalCode ASC, ZX5_ZZF_NKTaxOrFeeCode ASC, ZX5_AdditionalCode ASC, ZX5_EndDate ASC, ZX5_ZZA_TradeGroup)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_StartDateUnique"));
				Assert.False(TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_EndDateUnique"));

				// Run upgrade script for specific version
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(394).UpgradeScript);

				// New indexes added
				Assert.True(
					TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_StartDateUnique"),
					"IX_RefCusVATApplicability_StartDateUnique existis?");
				Assert.True(
					TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_EndDateUnique"),
					"IX_RefCusVATApplicability_EndDateUnique existis?");

				// Old indexes removed
				Assert.False(
					TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA"),
					"IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA existis?");
				Assert.False(
					TestDBHelper.IndexExists(conn, "RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA"),
					"IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA existis?");

				#endregion

				#region AssertVersion395Modifications

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusRateTypeTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusRateType.TableName, "DF_RefCusRateType_ZZR_RX_NKFormulaCurrency"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusRateType.TableName, "ZZR_RX_NKFormulaCurrency"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refCusRateType.TableName, "ZZR_RX_NKFormulaCurrency"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusRateType.TableName, "ZZR_RX_NKFormulaCurrency"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusRateTypeTableView_V3"));

				// Verify Version 395 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(395).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refCusRateType.TableName, "ZZR_RX_NKFormulaCurrency"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusRateType.TableName, "DF_RefCusRateType_ZZR_RX_NKFormulaCurrency"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusRateTypeTableView_V3"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusRateTypeTableView_V3").Contains("ZZR_RX_NKFormulaCurrency"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusRateTypeTableView_V2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusRateTypeTableView_V1"));

				#endregion

				#region AssertVersion396Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(396).UpgradeScript);

				#endregion

				#region AssertVersion397Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(397).UpgradeScript);

				#endregion

				#region AssertVersion398Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(398).UpgradeScript);

				#endregion

				#region AssertVersion399Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(399).UpgradeScript);

				#endregion

				#region AssertVersion400Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(400).UpgradeScript);

				#endregion

				#region AssertVersion401Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(401).UpgradeScript);

				#endregion

				#region AssertVersion402Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(402).UpgradeScript);

				#endregion

				#region AssertVersion403Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusCodeListTransportModeView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusCodeListAttributeTransportModeView_V1"));

				//Verify Version 403 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(403).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusCodeListTransportModeView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusCodeListAttributeTransportModeView_V1"));
				Assert.True(TestDBHelper.IndexExists(conn, "RefCusCodeListTransportModeView_V1", "IX_RefCusCodeListTransportModeView_V1_ZZU_ZZD_CodeList"));
				Assert.True(TestDBHelper.IndexExists(conn, "RefCusCodeListAttributeTransportModeView_V1", "IX_RefCusCodeListAttributeTransportModeView_V1_ZZU_ZZE_Attribute"));

				#endregion

				#region AssertVersion404Modifications

				// Restore and Pre-condition Check
				var refCusProcedure = new RefCusProcedure();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProcedure.TableName, "DF_RefCusProcedure_ZZ6_IntoVATWarehouse"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProcedure.TableName, "ZZ6_IntoVATWarehouse"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProcedure.TableName, "DF_RefCusProcedure_ZZ6_OutOfVATWarehouse"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProcedure.TableName, "ZZ6_OutOfVATWarehouse"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusProcedure.TableName, "ZZ6_IntoVATWarehouse"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refCusProcedure.TableName, "ZZ6_IntoVATWarehouse"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProcedure.TableName, "ZZ6_OutOfVATWarehouse"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, refCusProcedure.TableName, "ZZ6_OutOfVATWarehouse"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProcedureTableView_V2"));

				//Verify Version 404 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(404).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refCusProcedure.TableName, "ZZ6_IntoVATWarehouse"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusProcedure.TableName, "DF_RefCusProcedure_ZZ6_IntoVATWarehouse"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProcedure.TableName, "ZZ6_OutOfVATWarehouse"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusProcedure.TableName, "DF_RefCusProcedure_ZZ6_OutOfVATWarehouse"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProcedureTableView_V2"));
				#endregion

				#region AssertVersion405Modifications

				// Restore and Pre-condition Check
				var refStlScript_405 = new RefStlScript();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V4", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refStlScript_405.TableName, "CK_RefStlScript_STL_CollectionStartDateUtc"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refStlScript_405.TableName, "STL_CollectionStartDateUtc"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refStlScript_405.TableName, "STL_CollectionStartDateUtc"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefStlScriptTableView_V4"));

				//Verify Version 405 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(405).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refStlScript_405.TableName, "STL_CollectionStartDateUtc"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefStlScriptTableView_V4"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V4").Contains("STL_CollectionStartDateUtc"));

				#endregion

				#region AssertVersion406Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(406).UpgradeScript);

				#endregion

				#region AssertVersion407Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(407).UpgradeScript);

				#endregion

				#region AssertVersion408Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(408).UpgradeScript);

				#endregion

				#region AssertVersion409Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(409).UpgradeScript);

				#endregion

				#region AssertVersion410Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(410).UpgradeScript);

				#endregion

				#region AssertVersion411Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(411).UpgradeScript);

				#endregion

				#region AssertVersion412Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(412).UpgradeScript);

				#endregion

				#region AssertVersion413Modifications

				string[] tableNames = {
					"RefCusTariff", "RefCusTariffLanguage", "RefCusTariffNationalCode", "RefCusTariffAttribute", "RefCusTariffRelationship",
					"RefCusTariffUOM", "RefCusRate", "RefCusRateUOM", "RefCusCondition", "RefCusConditionValue", "RefCusConditionLanguage",
					"RefCusTariffAdditionalCode", "RefCusApplicability", "RefCusExcludedTradeGroup", "RefCusVATApplicability", "RefCusTariffAdditionalCodeLanguage",
					"RefCusTariffBRCharacteristic", "RefCusTariffBRCharacteristicAttribute", "RefCusTariffBRCharacteristicValue" };

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				Array.ForEach(tableNames, x => restoreSb.Append(SharedDbSchemaChange.GetSetLockEscalationScript(x, "TABLE")));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Array.ForEach(tableNames, x => Assert.False(TestDBHelper.LockEscalationDisabled(conn, x)));

				//Verify Version 413 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(413).UpgradeScript);
				Array.ForEach(tableNames, x => Assert.True(TestDBHelper.LockEscalationDisabled(conn, x)));

				#endregion

				#region AssertVersion414Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity", "[STL_DataGranularity]='TRN' OR [STL_DataGranularity]='MAH' OR [STL_DataGranularity]='MCO' OR [STL_DataGranularity]='DAY'"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V4", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V5", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity", "([STL_DataGranularity]=''TRN'' OR [STL_DataGranularity]=''MAH'' OR [STL_DataGranularity]=''MCO'' OR [STL_DataGranularity]=''DAY'' OR [STL_DataGranularity]=''SPS'')"));

				//Verify Version 414 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(414).UpgradeScript);
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity", "([STL_DataGranularity]=''TRN'' OR [STL_DataGranularity]=''MAH'' OR [STL_DataGranularity]=''MCO'' OR [STL_DataGranularity]=''DAY'' OR [STL_DataGranularity]=''SPS'')"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V1").Contains("STL_DataGranularity <> 'SPS'"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V2").Contains("STL_DataGranularity <> 'SPS'"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V3").Contains("STL_DataGranularity <> 'SPS'"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V4").Contains("STL_DataGranularity <> 'SPS'"));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefStlScriptTableView_V5").Contains("STL_DataGranularity <> 'SPS'"));
				#endregion

				#region AssertVersion415Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(415).UpgradeScript);

				#endregion

				#region AssertVersion416Modifications

				// Restore and Pre-Condition Check
				var refCusConditionCode = "RefCusConditionCode";
				restoreSb.Clear();

				Assert.False(TestDBHelper.TableExists(conn, refCusConditionCode));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionCodeTableView_V1"));

				//Verify Version 416 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(416).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, refCusConditionCode));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionCodeTableView_V1"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusConditionCode, "IX_RefCusConditionCode_ZY7_ConditionCode_ZY7_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusConditionCode, "IX_RefCusConditionCode_ZY7_ConditionCode"));

				#endregion

				#region AssertVersion417Modifications

				// Restore and Pre-Condition Check
				var refCusConditionCodeLanguage = "RefCusConditionCodeLanguage";
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refCusConditionCodeLanguage));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionCodeLanguageTableView_V1"));

				//Verify Version 417 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(417).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, refCusConditionCodeLanguage));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionCodeLanguageTableView_V1"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusConditionCodeLanguage, "IX_RefCusConditionCodeLanguage_ZY8_ZX1_ConditionCode_ZY8_ZX6_NKLanguage"));

				#endregion

				#region AssertVersion418Modifications

				// Restore and Pre-Condition Check
				var refCusCondition = "RefCusCondition";
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusCondition, "DF_RefCusCondition_ZX1_AdditionalComment"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCondition, "ZX1_AdditionalComment"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCondition, "ZX1_ZY7_NKConditionCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCondition_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusCondition, "ZX1_AdditionalComment"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTableView_V2"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusCondition, "FK_RefCusCondition_RefCusConditionCode"));


				//Verify Version 418 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(418).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refCusCondition, "ZX1_AdditionalComment"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusCondition, "ZX1_ZY7_NKConditionCode"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusCondition, "FK_RefCusCondition_RefCusConditionCode"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusConditionTableView_V2").Contains("ZX1_AdditionalComment"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusConditionTableView_V2").Contains("ZX1_ZY7_NKConditionCode"));

				#endregion

				#region AssertVersion419Modifications

				// Restore and Pre-Condition Check
				var refCusConditionLanguage_419 = "RefCusConditionLanguage";
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusConditionLanguage_419, "DF_RefCusConditionLanguage_ZXJ_AdditionalComment"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusConditionLanguage_419, "ZXJ_AdditionalComment"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusConditionLanguage_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusConditionLanguage_419, "ZXJ_AdditionalComment"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionLanguageTableView_V2"));

				//Verify Version 419 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(419).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refCusConditionLanguage_419, "ZXJ_AdditionalComment"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionLanguageTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusConditionLanguageTableView_V2").Contains("ZXJ_AdditionalComment"));

				#endregion

				#region AssertVersion420Modifications

				AssertExistRefMessagingBussPackageInfo(conn);

				#endregion

				#region AssertVersion421Modifications

				AssertExistRefMessagingBussPackageVersion(conn);

				#endregion

				#region AssertVersion422Modifications

				AssertExistRefMessagingBussCarrierInfo(conn);

				#endregion

				#region AssertVersion423Modifications

				// Restore and Pre-condition Check
				var undgSubstanceCFR = new UNDGSubstanceCFR();
				restoreOldConstarintSql = @"ALTER TABLE [dbo].[UNDGSubstanceCFR] DROP CONSTRAINT [CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType];
ALTER TABLE [dbo].[UNDGSubstanceCFR]  WITH CHECK ADD  CONSTRAINT [CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType] CHECK  (([CFR_PAXAirRailLimitType]='' OR [CFR_PAXAirRailLimitType]='FOB' OR [CFR_PAXAirRailLimitType]='NLM'));";
				TestDBHelper.ExecuteNonQuery(conn, restoreOldConstarintSql);
				restoreOldConstarintSql = @"ALTER TABLE [dbo].[UNDGSubstanceCFR] DROP CONSTRAINT [CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType];
ALTER TABLE [dbo].[UNDGSubstanceCFR]  WITH CHECK ADD  CONSTRAINT [CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType] CHECK  (([CFR_CargoAirRailLimitType]='' OR [CFR_CargoAirRailLimitType]='FOB' OR [CFR_CargoAirRailLimitType]='NLM'));";
				TestDBHelper.ExecuteNonQuery(conn, restoreOldConstarintSql);
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, undgSubstanceCFR.TableName, "CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType",
					"([CFR_PAXAirRailLimitType]='''' OR [CFR_PAXAirRailLimitType]=''FOB'' OR [CFR_PAXAirRailLimitType]=''NLM'')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, undgSubstanceCFR.TableName, "CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType",
					"([CFR_CargoAirRailLimitType]='''' OR [CFR_CargoAirRailLimitType]=''FOB'' OR [CFR_CargoAirRailLimitType]=''NLM'')"));

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceCFRTableView_V1", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 423 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(423).UpgradeScript);
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, undgSubstanceCFR.TableName, "CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType",
					"([CFR_PAXAirRailLimitType]='''' OR [CFR_PAXAirRailLimitType]=''FOB'' OR [CFR_PAXAirRailLimitType]=''NLM'' OR [CFR_PAXAirRailLimitType]=''NLT'' OR [CFR_PAXAirRailLimitType]=''GLM'')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, undgSubstanceCFR.TableName, "CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType",
					"([CFR_CargoAirRailLimitType]='''' OR [CFR_CargoAirRailLimitType]=''FOB'' OR [CFR_CargoAirRailLimitType]=''NLM'' OR [CFR_CargoAirRailLimitType]=''NLT'' OR [CFR_CargoAirRailLimitType]=''GLM'')"));

				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceCFRTableView_V1").Contains("CFR_PAXAirRailLimitType IN('', 'NLM', 'FOB')\r\nAND CFR_CargoAirRailLimitType IN('', 'NLM', 'FOB')"));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceCFRTableView_V2").Contains("CFR_PAXAirRailLimitType IN('', 'NLM', 'FOB')\r\nAND CFR_CargoAirRailLimitType IN('', 'NLM', 'FOB')"));

				#endregion

				#region AssertVersion424Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffUOMView_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V2"));

				//Verify Version 424 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(424).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V2"));
				#endregion

				#region AssertVersion426Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(426).UpgradeScript);

				#endregion

				#region AssertVersion428Modifications

				// Restore and Pre-Condition Check
				var refClient = new RefClient();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, refClient.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefClientTableView_V1"));

				//Verify Version 428 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(428).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, refClient.TableName));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefClientTableView_V1"));
				Assert.True(TestDBHelper.IndexExists(conn, refClient.TableName, "IX_RCT_RefClient_RCT_ClientID"));

				#endregion

				#region AssertVersion429Modifications

				// Restore and Pre-condition Check
				var refUNLOCORelatedPort_429 = new RefUNLOCORelatedPort();
				restoreSb.Clear();

				Assert.False(TestDBHelper.TableExists(conn, refUNLOCORelatedPort_429.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefUNLOCORelatedPortTableView_V1"));

				//Verify Version 429 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(429).UpgradeScript);

				Assert.True(TestDBHelper.TableExists(conn, refUNLOCORelatedPort_429.TableName));
				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCORelatedPort_429.TableName, "IX_RefUNLOCORelatedPort_RLR_GroupNumber_RLR_RL_NKRelatedPort"));
				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCORelatedPort_429.TableName, "IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefUNLOCORelatedPortTableView_V1"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refUNLOCORelatedPort_429.TableName, "FK_RefUNLOCORelatedPort_RefUNLOCO"));

				#endregion

				#region AssertVersion430Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(430).UpgradeScript);

				#endregion

				#region AssertVersion431Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine("DROP TABLE IF EXISTS RefClient");
				restoreSb.AppendLine(@"
CREATE TABLE [dbo].[RefClient](
	[RCT_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefClient_RCT_PK] DEFAULT NEWID(),
	[RCT_ClientID] [VARCHAR] NOT NULL UNIQUE,
	[RCT_Certificate] [VARBINARY] NOT NULL,
	[RCT_LegacyCertificate] [VARBINARY],
	[RCT_Signature] [VARBINARY] NOT NULL,
	CONSTRAINT [PK_RefClient] PRIMARY KEY CLUSTERED ([RCT_PK] ASC),
)
CREATE UNIQUE NONCLUSTERED INDEX IX_RCT_RefClient_RCT_ClientID ON RefClient ( RCT_ClientID )
");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.AreEqual(TestDBHelper.GetColumnLength(conn, refClient.TableName, "RCT_ClientID"), 1);
				Assert.AreEqual(TestDBHelper.GetColumnLength(conn, refClient.TableName, "RCT_Certificate"), 1);
				Assert.AreEqual(TestDBHelper.GetColumnLength(conn, refClient.TableName, "RCT_LegacyCertificate"), 1);
				Assert.AreEqual(TestDBHelper.GetColumnLength(conn, refClient.TableName, "RCT_Signature"), 1);

				//Verify Version 431 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(431).UpgradeScript);

				Assert.AreEqual(TestDBHelper.GetColumnLength(conn, refClient.TableName, "RCT_ClientID"), 255);
				Assert.AreEqual(TestDBHelper.GetColumnLength(conn, refClient.TableName, "RCT_Certificate"), -1);
				Assert.AreEqual(TestDBHelper.GetColumnLength(conn, refClient.TableName, "RCT_LegacyCertificate"), -1);
				Assert.AreEqual(TestDBHelper.GetColumnLength(conn, refClient.TableName, "RCT_Signature"), -1);
				#endregion

				#region AssertVersion432Modifications

				// Restore and Pre-Condition Check
				var undgSubstanceCFR_432 = new UNDGSubstanceCFR();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceCFRTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceCFR_432.TableName, "DF_UNDGSubstanceCFR_CFR_SecondaryPAXAirRailLimit"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimit"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceCFR_432.TableName, "DF_UNDGSubstanceCFR_CFR_SecondaryPAXAirRailLimitUnit"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimitUnit"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceCFR_432.TableName, "DF_UNDGSubstanceCFR_CFR_SecondaryCargoAirRailLimit"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimit"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceCFR_432.TableName, "DF_UNDGSubstanceCFR_CFR_SecondaryCargoAirRailLimitUnit"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimitUnit"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimit"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimit"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimitUnit"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimitUnit"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimit"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimit"));
				Assert.False(TestDBHelper.ConstraintFromColumnNameExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimitUnit"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimitUnit"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceCFRTableView_V3"));

				//Verify Version 432 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(432).UpgradeScript);

				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimit"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimit"));
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimitUnit"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryPAXAirRailLimitUnit"));
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimit"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimit"));
				Assert.True(TestDBHelper.ConstraintFromColumnNameExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimitUnit"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_432.TableName, "CFR_SecondaryCargoAirRailLimitUnit"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceCFRTableView_V3"));

				#endregion

				#region AssertVersion433Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(433).UpgradeScript);

				#endregion

				#region AssertVersion434Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(434).UpgradeScript);

				#endregion

				#region AssertVersion435Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(435).UpgradeScript);

				#endregion

				#region AssertVersion436Modifications

				// Restore and Pre-Condition Check
				var tariffReferencedTables = new[] {
					( nameof(RefCusRate), "ZZ2" ),
					( nameof(RefCusCondition), "ZX1" ),
					( nameof(RefCusVATApplicability), "ZX5" ),
					( nameof(RefCusTariffAttribute), "ZZ3" ),
					( nameof(RefCusTariffRelationship), "ZZH" ),
					( nameof(RefCusTariffLanguage), "ZX7" ),
					( nameof(RefCusTariffUOM), "ZZ8" ),
					( nameof(RefCusTariffBRCharacteristic), "ZB1" ),
					( nameof(RefCusTariffNationalCode), "ZZW" ),
					( nameof(RefCusTariffAdditionalCode), "ZY2" )
				};
				restoreSb.Clear();
				Array.ForEach(tariffReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariff")));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariff), "IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_PK"));
				restoreSb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusTariff), $"PK_{nameof(RefCusTariff)}"));
				restoreSb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusTariff), $"PK_{nameof(RefCusTariff)}", "ZZ1_PK"));
				Array.ForEach(tariffReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariff", $"{x.Item2}_ZZ1_Tariff", $"{nameof(RefCusTariff)}(ZZ1_PK)")));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariff), "IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_PK"));

				//Verify Version 436 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(436).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariff), "IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_PK"));

				#endregion

				#region AssertVersion437Modifications

				// Restore and Pre-Condition Check
				var rateReferencedTables = new[] {
					( nameof(RefCusApplicability), "ZZT" ),
					( nameof(RefCusRateUOM), "ZXG" )
				};
				restoreSb.Clear();
				Array.ForEach(rateReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusRate")));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_PK"));
				restoreSb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusRate), $"PK_{nameof(RefCusRate)}"));
				restoreSb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusRate), $"PK_{nameof(RefCusRate)}", "ZZ2_PK"));
				Array.ForEach(rateReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusRate", $"{x.Item2}_ZZ2_Rate", $"{nameof(RefCusRate)}(ZZ2_PK)")));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_PK"));

				//Verify Version 437 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(437).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_PK"));

				#endregion

				#region AssertVersion438Modifications

				// Restore and Pre-Condition Check
				var conditionReferencedTables = new[] {
					( nameof(RefCusApplicability), "ZZT" ),
					( nameof(RefCusConditionValue), "ZX3" ),
					( nameof(RefCusConditionLanguage), "ZXJ" )
				};
				restoreSb.Clear();
				Array.ForEach(conditionReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusCondition")));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_PK"));
				restoreSb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusCondition), $"PK_{nameof(RefCusCondition)}"));
				restoreSb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusCondition), $"PK_{nameof(RefCusCondition)}", "ZX1_PK"));
				Array.ForEach(conditionReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusCondition", $"{x.Item2}_ZX1_{(x.Item2 == "ZZT" ? "Conditions" : "Condition")}", $"{nameof(RefCusCondition)}(ZX1_PK)")));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_PK"));

				//Verify Version 438 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(438).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_PK"));

				#endregion

				#region AssertVersion439Modifications

				// Restore and Pre-Condition Check
				var additionalCodeReferencedTables = new[] {
					( nameof(RefCusApplicability), "ZZT" ),
					( nameof(RefCusTariffAdditionalCodeLanguage), "ZY4" )
				};
				restoreSb.Clear();
				Array.ForEach(additionalCodeReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariffAdditionalCode")));
				restoreSb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(nameof(RefCusTariffAdditionalCode), "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffAdditionalCode), "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_PK"));
				restoreSb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusTariffAdditionalCode), $"PK_{nameof(RefCusTariffAdditionalCode)}"));
				restoreSb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusTariffAdditionalCode), $"PK_{nameof(RefCusTariffAdditionalCode)}", "ZY2_PK"));
				Array.ForEach(additionalCodeReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariffAdditionalCode", $"{x.Item2}_ZY2_{(x.Item2 == "ZY4" ? "TariffAdditionalCode" : "AdditionalCode")}", $"{nameof(RefCusTariffAdditionalCode)}(ZY2_PK)")));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffAdditionalCode), "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_PK"));

				//Verify Version 439 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(439).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffAdditionalCode), "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_PK"));

				#endregion

				#region AssertVersion440Modifications

				// Restore and Pre-Condition Check
				var nationalCodeReferencedTables = new[] {
					( nameof(RefCusRate), "ZZ2" ),
					( nameof(RefCusVATApplicability), "ZX5" ),
					( nameof(RefCusTariffAttribute), "ZZ3" ),
					( nameof(RefCusTariffUOM), "ZZ8" ),
					( nameof(RefCusTariffAdditionalCode), "ZY2" )
				};
				restoreSb.Clear();
				Array.ForEach(nationalCodeReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariffNationalCode")));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffNationalCode), "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_PK"));
				restoreSb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusTariffNationalCode), $"PK_{nameof(RefCusTariffNationalCode)}"));
				restoreSb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusTariffNationalCode), $"PK_{nameof(RefCusTariffNationalCode)}", "ZZW_PK"));
				Array.ForEach(nationalCodeReferencedTables, x => restoreSb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariffNationalCode", $"{x.Item2}_ZZW_{(x.Item2 == "ZY2" ? "NationalCode" : "TariffNationalCode")}", $"{nameof(RefCusTariffNationalCode)}(ZZW_PK)")));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffNationalCode), "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_PK"));

				//Verify Version 440 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(440).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffNationalCode), "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_PK"));

				#endregion

				#region AssertVersion441Modifications

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_PK"));
				restoreSb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusTariffUOM), $"PK_{nameof(RefCusTariffUOM)}"));
				restoreSb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusTariffUOM), $"PK_{nameof(RefCusTariffUOM)}", "ZZ8_PK"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_PK"));

				//Verify Version 441 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(441).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_PK"));

				#endregion

				#region AssertVersion442Modifications

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK"));
				restoreSb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusVATApplicability), $"PK_{nameof(RefCusVATApplicability)}"));
				restoreSb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusVATApplicability), $"PK_{nameof(RefCusVATApplicability)}", "ZX5_PK", "NONCLUSTERED"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK"));

				//Verify Version 442 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(442).UpgradeScript);
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK"));

				#endregion

				#region AssertVersion443Modifications

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping",
					"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping ON RefCusRate(ZZ2_ZZ1_Tariff ASC, ZZ2_ZZW_TariffNationalCode , ZZ2_ZY1_RateCode ASC, ZZ2_StartDate, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping) INCLUDE (ZZ2_EndDate)"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode",
					"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode ON RefCusRate(ZZ2_ZZ1_Tariff ASC, ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZW_TariffNationalCode ASC)"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff",
					"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff ON RefCusRate(ZZ2_ZZW_TariffNationalCode ASC, ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZ1_Tariff ASC)"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping",
					"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping ON RefCusRate(ZZ2_ZZW_TariffNationalCode, ZZ2_ZY1_RateCode ASC, ZZ2_StartDate, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping) INCLUDE (ZZ2_EndDate)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));

				//Verify Version 443 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(443).UpgradeScript);
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));

				#endregion

				#region AssertVersion444Modifications

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping",
					"CREATE NONCLUSTERED INDEX IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping ON RefCusCondition (ZX1_ZX2_ConditionType ASC, ZX1_ZZ1_Tariff ASC, ZX1_ZZ5_Nomenclature ASC, ZX1_StartDate ASC, ZX1_ZZZ_NKDataGrouping ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate"));

				//Verify Version 444 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(444).UpgradeScript);
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate"));

				#endregion

				#region AssertVersion445Modifications

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode ON RefCusTariffUOM(ZZ8_ZZ1_Tariff ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZW_TariffNationalCode ASC)"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff ON RefCusTariffUOM(ZZ8_ZZW_TariffNationalCode ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZ1_Tariff ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));

				//Verify Version 445 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(445).UpgradeScript);
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));

				#endregion

				#region AssertVersion446Modifications

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference",
					"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference ON RefCusRate(ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZ1_Tariff ASC, ZZ2_ZZW_TariffNationalCode , ZZ2_ZY1_RateCode ASC, ZZ2_StartDate, ZZ2_ZZS_Preference) INCLUDE (ZZ2_EndDate)"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode",
					"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode ON RefCusRate(ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZ1_Tariff ASC, ZZ2_ZZW_TariffNationalCode ASC)"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff",
					"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff ON RefCusRate(ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZW_TariffNationalCode ASC, ZZ2_ZZ1_Tariff ASC)"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference",
					"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference ON RefCusRate(ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZW_TariffNationalCode, ZZ2_ZY1_RateCode ASC, ZZ2_StartDate, ZZ2_ZZS_Preference) INCLUDE (ZZ2_EndDate)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));

				//Verify Version 446 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(446).UpgradeScript);
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));

				#endregion

				#region AssertVersion447Modifications

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate",
					"CREATE NONCLUSTERED INDEX IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZZ_NKDataGrouping_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate ON RefCusCondition (ZX1_ZZZ_NKDataGrouping ASC, ZX1_ZX2_ConditionType ASC, ZX1_ZZ1_Tariff ASC, ZX1_ZZ5_Nomenclature ASC, ZX1_StartDate ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping"));

				//Verify Version 447 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(447).UpgradeScript);
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping"));

				#endregion

				#region AssertVersion448Modifications

				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode ON RefCusTariffUOM(ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZ1_Tariff ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZW_TariffNationalCode ASC)"));
				restoreSb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				restoreSb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff ON RefCusTariffUOM(ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZW_TariffNationalCode ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZ1_Tariff ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));

				//Verify Version 448 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(448).UpgradeScript);
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));

				#endregion

				#region AssertVersion449Modifications
				// Restore and Pre-Condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusVATApplicabilityTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "VATApplicabilityView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZZ1_ParentTariffOrNationalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(nameof(RefCusVATApplicability), "ZX5_ZZ1_ParentTariffOrNationalCode"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, nameof(RefCusVATApplicability), "ZX5_ZZ1_ParentTariffOrNationalCode"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusVATApplicabilityTableView_V3"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "VATApplicabilityView_V2"));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZZ1_ParentTariffOrNationalCode"));

				//Verify Version 449 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(449).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, nameof(RefCusVATApplicability), "ZX5_ZZ1_ParentTariffOrNationalCode"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusVATApplicabilityTableView_V3"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "VATApplicabilityView_V2"));
				Assert.True(TestDBHelper.IndexExists(conn, nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZZ1_ParentTariffOrNationalCode"));
				#endregion

				#region AssertVersion450Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(450).UpgradeScript);

				#endregion

				#region AssertVersion451Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup"));
				// Add old indexes to assert they are removed.
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup",
					"CREATE UNIQUE INDEX IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup ON RefCusApplicability (ZZT_ZX1_Conditions, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZ2_Rate, ZZT_ZZA_SecondTradeGroup) INCLUDE ([ZZT_EndDate])"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup ON RefCusApplicability (ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZX1_Conditions, ZZT_ZZA_SecondTradeGroup)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup"));

				// Run upgrade script for specific version
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(451).UpgradeScript);

				//Verify Version 451 Upgrade Script
				// New indexes added
				Assert.True(
					TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup"),
					"IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup exists?");
				Assert.True(
					TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup"),
					"IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup exists?");

				// Old indexes removed
				Assert.False(
					TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup"),
					"IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup exists?");
				Assert.False(
					TestDBHelper.IndexExists(conn, refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup"),
					"IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup exists?");
				#endregion

				#region AssertVersion452Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(452).UpgradeScript);

				#endregion

				#region AssertVersion453Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(453).UpgradeScript);

				#endregion

				#region AssertVersion454Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(454).UpgradeScript);

				#endregion

				#region AssertVersion455Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(455).UpgradeScript);

				#endregion

				#region AssertVersion456Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(456).UpgradeScript);

				#endregion

				#region AssertVersion457Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(457).UpgradeScript);

				#endregion

				#region AssertVersion458Modifications

				Assert.AreEqual("select 1", provider.GetUpgradeWrapperByVersion(458).UpgradeScript);

				#endregion

				#region AssertVersion459Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffView_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffView_V2"));

				//Verify Version 459 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(459).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffView_V2"));
				#endregion

				#region AssertVersion459Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffView_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffView_V2"));

				//Verify Version 459 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(459).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffView_V2"));
				#endregion

				#region AssertVersion460Modifications

				// Restore and Pre-condition Check
				var vat = new RefCusVATApplicability();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(vat.TableName, "ZX5_DataSetId"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(vat.TableName, "ZX5_DataSetId"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusVATApplicability_ZX5_DataSetId", "D" } }));
				Assert.False(TestDBHelper.ColumnExists(conn, vat.TableName, "ZX5_DataSetId"));

				//Verify Version 460 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(460).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, vat.TableName, "ZX5_DataSetId"));
				#endregion

				#region AssertVersion461Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(@"INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','GB','GB'),
('EF2242EA-6479-4DED-BA14-B441C18648E8','EUN','EUN');

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','EUN');

INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping, ZZA_Description)
VALUES ('879DD842-4D80-46AB-9EFA-73DC72C5D5A8','2005','EUN', 'GSP (R 12/978) - Annex IV');

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description,ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique,ZZ1_StartDate,ZZ1_EndDate,ZZ1_CompositeKeyOnZZ5,ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','EUN','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01'),
('76A71373-0C4C-422B-8842-3BC4D42D9E78', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','GB','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01');

INSERT INTO RefCusTariffNationalCode (ZZW_PK,ZZW_ZZ1_Tariff,ZZW_NationalCode,ZZW_Description,ZZW_ZZF_NKTaxOrFeeCode,ZZW_StartDate,ZZW_EndDate,ZZW_ZZZ_NKDataGrouping,ZZW_PublishedDate)
VALUES ('8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'AAAAA', 'AAAA','', '1900-01-01','2079-06-06', 'EUN', '1900-01-01'),
('D86FCCDF-A43D-4852-9217-54AD30A264B7', '76A71373-0C4C-422B-8842-3BC4D42D9E78', 'AAAAA', 'AAAA','', '1900-01-01','2079-06-06', 'GB', '1900-01-01');

INSERT INTO RefCusVATApplicability (ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_ZZA_TradeGroup,ZX5_VATCategory)
VALUES ('7558A41B-F5AE-46A0-AB6E-510117207DFF', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, '', '1900-01-01','2079-06-06', 'A', 'A1', 'EUN', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', ''),
('7558A41B-F5AE-46A0-AB6E-510117207DF1', '76A71373-0C4C-422B-8842-3BC4D42D9E78', NULL, '', '1900-01-01','2079-06-06', 'A', 'A2', 'EUN', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', ''),
('78AC55ED-0093-46B7-B3A5-7760A2F247BB', NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '', '1900-01-01','2079-06-06', 'A', 'A2', 'EUN', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', ''),
('70CF6AD1-6FF3-4940-8D9D-473B2E126E9D', NULL, 'D86FCCDF-A43D-4852-9217-54AD30A264B7', '', '1900-01-01','2079-06-06', 'A', 'A2', 'EUN', '879DD842-4D80-46AB-9EFA-73DC72C5D5A8', '');

INSERT INTO RefDbVersionControl (RVC_PK, RVC_DataSet, RVC_UpdaterVersion)
VALUES ('77150201-F2F0-4037-ABE7-D1E09B9083C8', 'RefCusTariff', 9),
('13E4D103-1B3E-46B1-B334-7AEBB1EB6B1F', 'GBCustomsTariffs', 9);
");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.AreEqual(4, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusVATApplicability WHERE ZX5_DataSetId=0;"));
				Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet IN ('RefCusTariff', 'GBCustomsTariffs') AND RVC_UpdaterVersion=9;"));

				//Verify Version 461 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(461).UpgradeScript);
				Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusVATApplicability WHERE ZX5_DataSetId=0;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusVATApplicability WHERE ZX5_PK='7558A41B-F5AE-46A0-AB6E-510117207DF1' AND ZX5_DataSetId=201;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusVATApplicability WHERE ZX5_PK='70CF6AD1-6FF3-4940-8D9D-473B2E126E9D' AND ZX5_DataSetId=201;"));
				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet IN ('RefCusTariff', 'GBCustomsTariffs') AND RVC_UpdaterVersion=9;"));
				Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet IN ('RefCusTariff', 'GBCustomsTariffs') AND RVC_UpdaterVersion=10;"));
				#endregion

				#region AssertVersion462Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(vat.TableName, "IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(vat.TableName, "IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK",
					"CREATE CLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping ASC, ZX5_PK ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, vat.TableName, "IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK"));
				Assert.False(TestDBHelper.IndexExists(conn, vat.TableName, "IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK"));

				//Verify Version 462 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(462).UpgradeScript);
				Assert.False(TestDBHelper.IndexExists(conn, vat.TableName, "IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK"));
				Assert.True(TestDBHelper.IndexExists(conn, vat.TableName, "IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK"));
				#endregion

				#region AssertVersion463Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(463).UpgradeScript);

				#endregion

				#region AssertVersion464Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusConditionTypeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusConditionTypeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusConditionTypeTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusConditionType), "IX_RefCusConditionType_ZX2_ConditionType_ZX2_ZZZ_NKDataGrouping"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(nameof(RefCusConditionType), "ZX2_ConditionType", "VARCHAR(5) NOT NULL"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.That(TestDBHelper.GetColumnLength(conn, nameof(RefCusConditionType), "ZX2_ConditionType"), Is.EqualTo(5));
				Assert.That(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V1"), Is.False);
				Assert.That(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V2"), Is.False);
				Assert.That(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V3"), Is.False);

				//Verify Version 464 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(464).UpgradeScript);
				Assert.That(TestDBHelper.GetColumnLength(conn, nameof(RefCusConditionType), "ZX2_ConditionType"), Is.EqualTo(6));
				Assert.That(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V1"), Is.True);
				Assert.That(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V2"), Is.True);
				Assert.That(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTypeTableView_V3"), Is.True);

				#endregion

				#region AssertVersion465Modifications

				AssertVersion465Modifications(conn);

				#endregion

				#region AssertVersion466Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileAttributeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileAttribute", "TABLE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileQuestionTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileQuestion", "TABLE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfile", "TABLE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTypeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileType", "TABLE"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileType"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTypeTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileQuestion"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfile"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileAttribute"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileAttributeTableView_V1"));

				//Verify Version 466 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(466).UpgradeScript);
				AssertExistRefCusProfileType(conn);
				AssertExistRefCusProfileQuestion(conn);
				AssertExistRefCusProfile(conn);
				AssertExistRefCusProfileAttribute(conn);

				#endregion

				#region AssertVersion467Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(467).UpgradeScript);

				#endregion

				#region AssertVersion468Modifications
				Assert.AreEqual("\r\nIF EXISTS(\r\n\tSELECT * FROM sys.tables tab \r\n\tINNER JOIN sys.columns col ON tab.object_id = col.object_id\r\n\tWHERE tab.name = 'UNDGSubstanceCFR' AND col.name = 'CFR_Variation'\r\n)\r\nBEGIN\r\n\tALTER TABLE UNDGSubstanceCFR ALTER COLUMN CFR_Variation VARCHAR(150) NOT NULL\r\nEND", provider.GetUpgradeWrapperByVersion(468).UpgradeScript);
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(468).UpgradeScript);
				#endregion

				#region AssertVersion469Modifications
				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(469).UpgradeScript);
				#endregion

				#region AssertVersion470Modifications

				Assert.True(TestDBHelper.IndexExists(conn, "RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, "RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate"));
				Assert.False(TestDBHelper.IndexExists(conn, "RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(470).UpgradeScript);

				Assert.False(TestDBHelper.IndexExists(conn, "RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, "RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, "RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));

				#endregion

				#region AssertVersion471Modifications

				undgSubstanceCFR = new UNDGSubstanceCFR();
				var cfrTableViewPrefix = $"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.Clear();
				restoreSb.AppendLine(CultureInfo.InvariantCulture, $"ALTER TABLE {undgSubstanceCFR.TableName} ALTER COLUMN [CFR_Variation] VARCHAR(80) NOT NULL");
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{cfrTableViewPrefix}1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{cfrTableViewPrefix}2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{cfrTableViewPrefix}3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{cfrTableViewPrefix}4", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.AreEqual(80, TestDBHelper.GetColumnLength(conn, $"{undgSubstanceCFR.TableName}", "CFR_Variation"));
				for (var i = 1; i <= 4; i++)
				{
					Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{cfrTableViewPrefix}{i}"));
				}

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(471).UpgradeScript);
				var cfr_Variation = TestDBHelper.GetExtendedTableColumns(conn, "UNDGSubstanceCFR").First(x => x.ColumnName == "CFR_Variation");
				Assert.AreEqual(150, cfr_Variation.CharacterMaximumLength);
				Assert.False(cfr_Variation.IsNullable);
				for (var i = 1; i <= 4; i++)
				{
					Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{cfrTableViewPrefix}{i}"));
					var viewDefinition = TestDBHelper.GetViewDefinition(conn, $"{cfrTableViewPrefix}{i}");
					if (i < 4)
					{
						Assert.True(viewDefinition.Contains("LEN(CFR_Variation) <= 80"));
					}
					else
					{
						Assert.False(viewDefinition.Contains("LEN(CFR_Variation) <= 80"));
					}
				}

				#endregion

				#region AssertVersion472Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(472).UpgradeScript);

				#endregion

				#region AssertVersion473Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(nameof(RefExchangeRateZZ), "ZZN_AsPublished", "VARCHAR(10) NOT NULL"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.That(TestDBHelper.GetColumnLength(conn, nameof(RefExchangeRateZZ), "ZZN_AsPublished"), Is.EqualTo(10));

				//Verify Version 473 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(473).UpgradeScript);
				Assert.That(TestDBHelper.GetColumnLength(conn, nameof(RefExchangeRateZZ), "ZZN_AsPublished"), Is.EqualTo(35));

				#endregion

				#region AssertVersion474Modifications

				// Restore and Pre-condition Check
				var refDocOrgCusCode_474 = new RefDocOrgCusCode();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefDocOrgCusCodeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refDocOrgCusCode_474.TableName, "DF_RefDocOrgCusCode_DOC_Direction"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refDocOrgCusCode_474.TableName, "DOC_Direction"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refDocOrgCusCode_474.TableName, "DOC_Direction"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefDocOrgCusCodeTableView_V2"));

				//Verify Version 474 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(474).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refDocOrgCusCode_474.TableName, "DOC_Direction"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefDocOrgCusCodeTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefDocOrgCusCodeTableView_V2").Contains("DOC_Direction"));

				#endregion

				#region AssertVersion475Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(475).UpgradeScript);

				#endregion

				#region AssertVersion476Modifications

				// Restore and Pre-condition Check
				var refCusProfileType = new RefCusProfileType();
				var refCusProfile = new RefCusProfile();
				var refCusProfileAttribute = new RefCusProfileAttribute();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusProfileType.TableName, "FK_RefCusProfileType_RefCusTariffType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusProfileType.TableName, "FK_RefCusProfileType_RefCusTariffType", "XXX_ZZI_TariffType", "RefCusTariffType (ZZI_PK) ON DELETE CASCADE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusProfile.TableName, "FK_RefCusProfile_XX0_XXX_ProfileType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusProfile.TableName, "FK_RefCusProfile_XX0_XXX_ProfileType", "XX0_XXX_ProfileType", "RefCusProfileType (XXX_PK) ON DELETE CASCADE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusProfileAttribute.TableName, "FK_RefCusProfileAttribute_RefCusProfile"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusProfileAttribute.TableName, "FK_RefCusProfileAttribute_RefCusProfile", "XXY_XX0_Profile", "RefCusProfile (XX0_PK) ON DELETE CASCADE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusProfile.TableName, "FK_RefCusProfile_XX0_XQ2_QuestionCode"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfile.TableName, "XX0_TariffCode", "VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusProfile_XX0_TariffCode DEFAULT('')"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate_XX0_EndDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate_XX0_EndDate ON RefCusProfile (XX0_ZZZ_NKDataGrouping ASC, XX0_XXX_ProfileType ASC, XX0_TariffCode ASC, XX0_StartDate ASC, XX0_EndDate ASC)"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileQuestionTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileQuestion", "TABLE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileQuestionAnswerListTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileQuestionAnswerList", "TABLE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileQuestionAttributeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileQuestionAttribute", "TABLE"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.ForeignKeyExistsWithDeleteAction(conn, refCusProfileType.TableName, "FK_RefCusProfileType_RefCusTariffType", true));
				Assert.True(TestDBHelper.ForeignKeyExistsWithDeleteAction(conn, refCusProfile.TableName, "FK_RefCusProfile_XX0_XXX_ProfileType", true));
				Assert.True(TestDBHelper.ForeignKeyExistsWithDeleteAction(conn, refCusProfileAttribute.TableName, "FK_RefCusProfileAttribute_RefCusProfile", true));
				Assert.False(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate_XX0_EndDate"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileQuestion"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileQuestionAnswerList"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionAnswerListTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileQuestionAttribute"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionAttributeTableView_V1"));

				//Verify Version 476 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(476).UpgradeScript);
				Assert.True(TestDBHelper.ForeignKeyExistsWithDeleteAction(conn, refCusProfileType.TableName, "FK_RefCusProfileType_RefCusTariffType", false));
				Assert.True(TestDBHelper.ForeignKeyExistsWithDeleteAction(conn, refCusProfile.TableName, "FK_RefCusProfile_XX0_XXX_ProfileType", false));
				Assert.True(TestDBHelper.ForeignKeyExistsWithDeleteAction(conn, refCusProfileAttribute.TableName, "FK_RefCusProfileAttribute_RefCusProfile", false));
				Assert.False(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate_XX0_EndDate"));
				AssertExistRefCusProfileQuestion(conn);
				AssertExistRefCusProfileQuestionAnswerList(conn);
				AssertExistRefCusProfileQuestionAttribute(conn);

				#endregion

				#region AssertVersion477Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreOldConstarintSql = @"ALTER TABLE [dbo].[RefExchangeRateZZ] DROP CONSTRAINT [CK_RefExchangeRateZZ_ZZN_ExRateType];
ALTER TABLE [dbo].[RefExchangeRateZZ]  WITH CHECK ADD  CONSTRAINT [CK_RefExchangeRateZZ_ZZN_ExRateType] CHECK  (([ZZN_ExRateType]='CUE' OR [ZZN_ExRateType]='CUS' OR [ZZN_ExRateType]='CUD' OR [ZZN_ExRateType]='IAT'));";
				TestDBHelper.ExecuteNonQuery(conn, restoreOldConstarintSql);

				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'' OR [ZZN_ExRateType]=''IAT'')"));
				//Verify Version 477 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(477).UpgradeScript);

				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'' OR [ZZN_ExRateType]=''IAT'')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'' OR [ZZN_ExRateType]=''IAT'' OR [ZZN_ExRateType]=''BNB'' OR [ZZN_ExRateType]=''BNS'')"));
				#endregion

				#region AssertVersion478Modifications

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript("RefAccTaxRate", "CK_RefAccTaxRate_ZAT_RN_NKCountry"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefAccTaxRate", "IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefAccTaxRate", "IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefAccTaxRate", "ZAT_RN_NKCountry", "VARCHAR(2) NOT NULL"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.AreEqual("varchar", TestDBHelper.GetExtendedTableColumns(conn, "RefAccTaxRate").First(x => x.ColumnName == "ZAT_RN_NKCountry").DataType);

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(478).UpgradeScript);
				Assert.AreEqual("char", TestDBHelper.GetExtendedTableColumns(conn, "RefAccTaxRate").First(x => x.ColumnName == "ZAT_RN_NKCountry").DataType);
				Assert.True(TestDBHelper.ObjectExists(conn, "C", "CK_RefAccTaxRate_ZAT_RN_NKCountry"));
				Assert.True(TestDBHelper.IndexesExist(conn, "RefAccTaxRate", new[] { "IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate", "IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate_EndDate" }));

				#endregion

				#region AssertVersion479Modifications

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefCusConditionTypeLanguage", "ZXW_Description", "NVARCHAR(500)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.GetExtendedTableColumns(conn, "RefCusConditionTypeLanguage").First(x => x.ColumnName == "ZXW_Description").IsNullable);

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(479).UpgradeScript);
				Assert.False(TestDBHelper.GetExtendedTableColumns(conn, "RefCusConditionTypeLanguage").First(x => x.ColumnName == "ZXW_Description").IsNullable);

				#endregion

				#region AssertVersion480Modifications

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", "RefCusTariffLanguage", "DF_RefCusTariffLanguage_ZX7_Description"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefCusTariffLanguage", "ZX7_Description", "VARCHAR(4000) NOT NULL"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.AreEqual(4000, TestDBHelper.GetColumnLength(conn, "RefCusTariffLanguage", "ZX7_Description"));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(480).UpgradeScript);
				Assert.AreEqual(-1, TestDBHelper.GetColumnLength(conn, "RefCusTariffLanguage", "ZX7_Description"));
				Assert.True(TestDBHelper.ObjectExists(conn, "D", "DF_RefCusTariffLanguage_ZX7_Description"));

				#endregion

				#region AssertVersion481Modifications

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript("RefCusExcludedTradeGroup", "PK_RefCusExcludedTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript("RefCusExcludedTradeGroup", "[[PK_RefCusExcludedTradeGroup]", "ZZC_PK"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				var excludedTradeGroupPK = "[PK_RefCusExcludedTradeGroup";
				Assert.True(TestDBHelper.ObjectExists(conn, "PK", excludedTradeGroupPK));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(481).UpgradeScript);
				excludedTradeGroupPK = "PK_RefCusExcludedTradeGroup";
				Assert.True(TestDBHelper.ObjectExists(conn, "PK", excludedTradeGroupPK));

				#endregion

				#region AssertVersion482Modifications

				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffNationalCodeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusVATApplicabilityTableView_V1", "VIEW"));

				var viewSql = @"CREATE VIEW RefCusTariffTableView_V1 AS
SELECT ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_IAMUnique,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_PublishedDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CompositeKeyOnZZ5
FROM RefCusTariff";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(viewSql, "V", "RefCusTariffTableView_V1"));
				viewSql = @"CREATE VIEW RefCusTariffNationalCodeTableView_V1 AS
SELECT ZZW_PK, ZZW_ZZ1_Tariff, ZZW_NationalCode, ZZW_Description, ZZW_ZZF_NKTaxOrFeeCode, ZZW_StartDate, ZZW_EndDate, ZZW_PublishedDate, ZZW_ZZZ_NKDataGrouping
FROM RefCusTariffNationalCode";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(viewSql, "V", "RefCusTariffNationalCodeTableView_V1"));
				viewSql = @"CREATE VIEW RefCusVATApplicabilityTableView_V1 AS
SELECT ZX5_PK,ZX5_ZZ1_Tariff,ZX5_ZZW_TariffNationalCode,ZX5_ZZF_NKTaxOrFeeCode,ZX5_StartDate,ZX5_EndDate,ZX5_AdditionalCode,ZX5_Description,ZX5_ZZZ_NKDataGrouping,ZX5_ZZA_TradeGroup,ZX5_VATCategory
FROM RefCusVATApplicability";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(viewSql, "V", "RefCusVATApplicabilityTableView_V1"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusTariffTableView_V1").Contains("SUBSTRING", StringComparison.OrdinalIgnoreCase));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusTariffNationalCodeTableView_V1").Contains("SUBSTRING", StringComparison.OrdinalIgnoreCase));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusVATApplicabilityTableView_V1").Contains("SUBSTRING", StringComparison.OrdinalIgnoreCase));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(482).UpgradeScript);
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusTariffTableView_V1").Contains("SUBSTRING", StringComparison.OrdinalIgnoreCase));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusTariffNationalCodeTableView_V1").Contains("SUBSTRING", StringComparison.OrdinalIgnoreCase));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusVATApplicabilityTableView_V1").Contains("SUBSTRING", StringComparison.OrdinalIgnoreCase));

				#endregion

				#region AssertVersion483Modifications

				var undgSubstanceJTT = new UNDGSubstanceJTT();
				var jttTableViewPrefix = $"{undgSubstanceJTT.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.Clear();
				restoreSb.AppendLine(CultureInfo.InvariantCulture, $"ALTER TABLE {undgSubstanceJTT.TableName} ALTER COLUMN [JTT_PSN] VARCHAR(200) NOT NULL");
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{jttTableViewPrefix}1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{jttTableViewPrefix}2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.AreEqual(200, TestDBHelper.GetColumnLength(conn, $"{undgSubstanceJTT.TableName}", "JTT_PSN"));
				for (var i = 1; i <= 2; i++)
				{
					Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{jttTableViewPrefix}{i}"));
				}

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(483).UpgradeScript);
				var jtt_PSN = TestDBHelper.GetExtendedTableColumns(conn, "UNDGSubstanceJTT").First(x => x.ColumnName == "JTT_PSN");
				Assert.AreEqual(300, jtt_PSN.CharacterMaximumLength);
				Assert.False(jtt_PSN.IsNullable);
				#endregion

				#region AssertVersion484And485Modifications

				// Restore and Pre-condition Check
				var refCusTariffUOM_V2 = new RefCusTariffUOM().TableName;
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_Tariff_Type_UOM_NKDataGrouping_TradeGroup_SecondTradeGroup_StartDate_TariffNationalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_TariffNationalCode_Type_UOM_NKDataGrouping_TradeGroup_SecondTradeGroup_StartDate_Tariff"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffUOM_V2, "CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusTariffUOM_V2, "ZZ8_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusTariffUOM_V2, "ZZ8_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusTariffUOM.TableName, "FK_RefCusTariffUOM_RefCusTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusTariffUOM.TableName, "FK_RefCusTariffUOM_RefCusTradeGroup_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusTariffUOM.TableName, "FK_RefCusTariffUOM_RefCusTariffNationalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffUOM_V2, "ZZ8_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffUOM_V2, "ZZ8_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffUOM_V2, "ZZ8_ZZA_SecondTradeGroup"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffUOMView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffUOMView_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_EndDate"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_StartDate"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_ZZA_SecondTradeGroup"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffUOMTableView_V2"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V2"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refCusTariffUOM_V2, "CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusTariffUOM_V2, "FK_RefCusTariffUOM_RefCusTradeGroup"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusTariffUOM_V2, "FK_RefCusTariffUOM_RefCusTariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffUOM_V2, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffUOM_V2, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffUOM_V2, "IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup"));

				//Verify Version 484 + 485 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(484).UpgradeScript);
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(485).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_EndDate"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_StartDate"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_ZZA_SecondTradeGroup"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_ZZA_TradeGroup"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffUOM_V2, "ZZ8_ZZA_SecondTradeGroup"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", FormattableString.Invariant($"{refCusTariffUOM_V2}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
				Assert.True(TestDBHelper.GetViewDefinition(conn, FormattableString.Invariant($"{refCusTariffUOM_V2}{SharedDbSchemaChange.TableViewVersionSuffix}2")).Contains("ZZ8_EndDate"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, FormattableString.Invariant($"{refCusTariffUOM_V2}{SharedDbSchemaChange.TableViewVersionSuffix}2")).Contains("ZZ8_StartDate"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, FormattableString.Invariant($"{refCusTariffUOM_V2}{SharedDbSchemaChange.TableViewVersionSuffix}2")).Contains("ZZ8_ZZA_SecondTradeGroup"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refCusTariffUOM_V2, "CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffUOM_V2, "FK_RefCusTariffUOM_RefCusTradeGroup"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffUOM_V2, "FK_RefCusTariffUOM_RefCusTariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffUOM_V2, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffUOM_V2, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffUOM_V2, "IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup"));

				#endregion

				#region AssertVersion486Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileQuestionLanguageTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileQuestionLanguage", "TABLE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileQuestionAnswerListLanguageTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileQuestionAnswerListLanguage", "TABLE"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileQuestionPathwayTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefCusProfileQuestionPathway", "TABLE"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileQuestionLanguage"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionLanguageTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileQuestionAnswerListLanguage"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionAnswerListLanguageTableView_V1"));
				Assert.False(TestDBHelper.TableExists(conn, "RefCusProfileQuestionPathway"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionPathwayTableView_V1"));

				//Verify Version 486 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(486).UpgradeScript);
				AssertExistRefCusProfileQuestionLanguage(conn);
				AssertExistRefCusProfileQuestionAnswerListLanguage(conn);
				AssertExistRefCusProfileQuestionPathway(conn);

				#endregion

				#region AssertVersion487Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(487).UpgradeScript);

				#endregion

				#region AssertVersion488Modification

				restoreSb.Clear();

				sql = @"CREATE TABLE [CMRInstrumentTariffGroup]
				(
					[IG_PK] UNIQUEIDENTIFIER NOT NULL,
					[IG_InstrumentType] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRInstrumentTariffGroup_IG_InstrumentType] DEFAULT '',
					[IG_InstrumentNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRInstrumentTariffGroup_IG_InstrumentNumber] DEFAULT '',
					[IG_TariffGroupItem] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRInstrumentTariffGroup_IG_TariffGroupItem] DEFAULT '',
					CONSTRAINT [IG_PK] PRIMARY KEY CLUSTERED
					(
						[IG_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrumentTariffGroup On CMRInstrumentTariffGroup(IG_InstrumentType, IG_InstrumentNumber, IG_TariffGroupItem)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrumentTariffGroup"));
				sql = @"CREATE VIEW CMRInstrumentTariffGroupTableView_V1 AS
				SELECT IG_PK,
				IG_InstrumentType,
				IG_InstrumentNumber,
				IG_TariffGroupItem
				FROM CMRInstrumentTariffGroup";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentTariffGroupTableView_V1"));

				sql = @"CREATE TABLE [CMRInstrument]
				(
					[IN_PK] UNIQUEIDENTIFIER NOT NULL,
					[IN_Type] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_Type] DEFAULT '',
					[IN_Number] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_Number] DEFAULT '',
					[IN_Status] VARCHAR NOT NULL CONSTRAINT [DF_CMRInstrument_IN_Status] DEFAULT '',
					[IN_ConcessionalItemNumber] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_ConcessionalItemNumber] DEFAULT '',
					[IN_StartDate] DATETIME NULL,
					[IN_EndDate] DATETIME NULL,
					[IN_DeclarationDate] DATETIME NULL,
					[IN_RevocationDate] DATETIME NULL,
					[IN_PreferenceSchemeValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_PreferenceSchemeValidationType] DEFAULT '',
					[IN_CountryValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_CountryValidationType] DEFAULT '',
					[IN_TariffValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_TariffValidationType] DEFAULT '',
					[IN_LinkedType] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_LinkedType] DEFAULT '',
					[IN_LinkedNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_LinkedNumber] DEFAULT '',
					[IN_InstrumentCategoryCode] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRInstrument_IN_InstrumentCategoryCode] DEFAULT '',
					CONSTRAINT [IN_PK] PRIMARY KEY CLUSTERED
					(
						[IN_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrument On CMRInstrument(IN_Type, IN_Number)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrument"));
				sql = @"CREATE VIEW CMRInstrumentTableView_V1 AS
				SELECT IN_PK,
				IN_Type,
				IN_Number,
				IN_Status,
				IN_ConcessionalItemNumber,
				IN_StartDate,
				IN_EndDate,
				IN_DeclarationDate,
				IN_RevocationDate,
				IN_PreferenceSchemeValidationType,
				IN_CountryValidationType,
				IN_TariffValidationType,
				IN_LinkedType,
				IN_LinkedNumber,
				IN_InstrumentCategoryCode
				FROM CMRInstrument";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentTableView_V1"));

				sql = @"CREATE TABLE [CMRLodgementQuestion]
				(
					[CQ_PK] UNIQUEIDENTIFIER NOT NULL,
					[CQ_Identifier] INT NOT NULL CONSTRAINT [DF_CMRLodgementQuestion_CQ_Identifier]  DEFAULT ((0)),
					[CQ_StartDate] DATETIME NULL,
					[CQ_EndDate] DATETIME NULL,
					[CQ_Indicator] CHAR(1) NOT NULL CONSTRAINT [DF_CMRLodgementQuestion_CQ_Indicator]  DEFAULT ('N'),
					[CQ_LodgementQuestionType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRLodgementQuestion_CQ_LodgementQuestionType]  DEFAULT (''),
					[CQ_ValidationResponseType] VARCHAR(1) NOT NULL CONSTRAINT [DF_CMRLodgementQuestion_CQ_ValidationResponseType]  DEFAULT (''),
					[CQ_Name] VARCHAR(30) NOT NULL CONSTRAINT [DF_CMRLodgementQuestion_CQ_Name]  DEFAULT (''),
					[CQ_Text] VARCHAR(1000) NOT NULL CONSTRAINT [DF_CMRLodgementQuestion_CQ_Text]  DEFAULT (''),
					CONSTRAINT CQ_PK PRIMARY KEY CLUSTERED ( [CQ_PK] ASC )
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRLodgementQuestion] ON [CMRLodgementQuestion]
				(
					[CQ_Identifier] ASC,
					[CQ_StartDate] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRLodgementQuestion"));
				sql = @"CREATE VIEW CMRLodgementQuestionTableView_V1 AS
				SELECT CQ_PK,
				CQ_Identifier,
				CQ_StartDate,
				CQ_EndDate,
				CQ_Indicator,
				CQ_LodgementQuestionType,
				CQ_ValidationResponseType,
				CQ_Name,
				CQ_Text
				FROM CMRLodgementQuestion";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRLodgementQuestionTableView_V1"));

				sql = @"CREATE TABLE [CMRCommunityProtectionRisk]
				(
					[CK_PK] UNIQUEIDENTIFIER NOT NULL,
					[CK_Identifier] INT NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_Identifier]  DEFAULT ((0)),
					[CK_CreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_CreationTimestamp]  DEFAULT (''),
					[CK_StartDate] DATETIME NULL,
					[CK_EndDate] DATETIME NULL,
					[CK_Type] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_Type]  DEFAULT (''),
					[CK_PermitApplicationIndicator] CHAR(1) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_PermitApplicationIndicator]  DEFAULT ('N'),
					[CK_PermitType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_PermitType]  DEFAULT (''),
					[CK_CQ_CMRLodgementQuestion] UNIQUEIDENTIFIER NOT NULL,
					[CK_HighRiskAnswerType] VARCHAR(1) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_HighRiskAnswerType]  DEFAULT (''),
					[CK_AQISInspectionCategory] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_AQISInspectionCategory]  DEFAULT (''),
					[CK_SelectionType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_SelectionType]  DEFAULT (''),
					[CK_AQISProducerRequiredIndicator] CHAR(1) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_AQISProducerRequiredIndicator]  DEFAULT ('N'),
					[CK_Description] VARCHAR(250) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRisk_CK_Description]  DEFAULT (''),
					CONSTRAINT CK_PK PRIMARY KEY CLUSTERED ( CK_PK ASC),
					CONSTRAINT [FK_CMRCommunityProtectionRisk_CMRLodgementQuestion] FOREIGN KEY([CK_CQ_CMRLodgementQuestion]) REFERENCES [CMRLodgementQuestion] ([CQ_PK])
				)
				CREATE NONCLUSTERED INDEX [NR_UX__CMRCommunityProtectionRisk] ON [CMRCommunityProtectionRisk]
				(
					[CK_Identifier] ASC,
					[CK_CreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRCommunityProtectionRisk"));
				sql = @"CREATE VIEW CMRCommunityProtectionRiskTableView_V1 AS
				SELECT CK_PK,
				CK_Identifier,
				CK_CreationTimestamp,
				CK_StartDate,
				CK_EndDate,
				CK_Type,
				CK_PermitApplicationIndicator,
				CK_PermitType,
				CK_CQ_CMRLodgementQuestion,
				CK_HighRiskAnswerType,
				CK_AQISInspectionCategory,
				CK_SelectionType,
				CK_AQISProducerRequiredIndicator,
				CK_Description
				FROM CMRCommunityProtectionRisk";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRCommunityProtectionRiskTableView_V1"));

				sql = @"CREATE TABLE [CMRCommunityProtectionProfile]
				(
					[CP_PK] UNIQUEIDENTIFIER NOT NULL,
					[CP_TariffClassificationNumberfield] VARCHAR(12) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionProfile_CP_TariffClassificationNumberfield]  DEFAULT (''),
					[CP_StatisticalClassificationCodefield] VARCHAR(6) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionProfile_CP_StatisticalClassificationCodefield]  DEFAULT (''),
					[CP_OriginCountryCodefield] VARCHAR(6) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionProfile_CP_OriginCountryCodefield]  DEFAULT (''),
					[CP_LineNatureTypefield] VARCHAR(7) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionProfile_CP_LineNatureTypefield]  DEFAULT (''),
					[CP_ModeofTransportfield] VARCHAR(14) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionProfile_CP_ModeofTransportfield]  DEFAULT (''),
					[CP_CK_CMRCommunityProtectionRisk] UNIQUEIDENTIFIER NOT NULL,
					CONSTRAINT [CP_PK] PRIMARY KEY CLUSTERED ( [CP_PK] ASC ),
					CONSTRAINT [FK_CMRCommunityProtectionProfile_CMRCommunityProtectionRisk] FOREIGN KEY([CP_CK_CMRCommunityProtectionRisk]) REFERENCES [CMRCommunityProtectionRisk] ([CK_PK])
				)
				CREATE NONCLUSTERED INDEX [NR_UX__CMRCommunityProtectionProfile] ON [dbo].[CMRCommunityProtectionProfile]
				(
					[CP_TariffClassificationNumberfield] ASC,
					[CP_StatisticalClassificationCodefield] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRCommunityProtectionProfile"));
				sql = @"CREATE VIEW CMRCommunityProtectionProfileTableView_V1 AS
				SELECT CP_PK,
				CP_TariffClassificationNumberfield,
				CP_StatisticalClassificationCodefield,
				CP_OriginCountryCodefield,
				CP_LineNatureTypefield,
				CP_ModeofTransportfield,
				CP_CK_CMRCommunityProtectionRisk
				FROM CMRCommunityProtectionProfile";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRCommunityProtectionProfileTableView_V1"));

				sql = @"CREATE TABLE [CMRCommunityProtectionRiskMessageAdvice]
				(
					[CM_PK] UNIQUEIDENTIFIER NOT NULL,
					[CM_CommunityProtectionRiskIdentifier] INT NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRiskMessageAdvice_CM_CommunityProtectionRiskIdentifier]  DEFAULT ((0)),
					[CM_CommunityProtectionRiskCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRiskMessageAdvice_CM_CommunityProtectionRiskCreationTimestamp]  DEFAULT (''),
					[CM_MessageAdviceIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRCommunityProtectionRiskMessageAdvice_CM_MessageAdviceIdentifier]  DEFAULT ((0)),
					CONSTRAINT CM_PK PRIMARY KEY CLUSTERED
					(
						CM_PK ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRCommunityProtectionRiskMessageAdvice] ON [CMRCommunityProtectionRiskMessageAdvice]
				(
					[CM_CommunityProtectionRiskIdentifier] ASC,
					[CM_CommunityProtectionRiskCreationTimestamp] ASC,
					[CM_MessageAdviceIdentifier] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRCommunityProtectionRiskMessageAdvice"));
				sql = @"CREATE VIEW CMRCommunityProtectionRiskMessageAdviceTableView_V1 AS
				SELECT CM_PK,
				CM_CommunityProtectionRiskIdentifier,
				CM_CommunityProtectionRiskCreationTimestamp,
				CM_MessageAdviceIdentifier
				FROM CMRCommunityProtectionRiskMessageAdvice";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRCommunityProtectionRiskMessageAdviceTableView_V1"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentTariffGroupTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRLodgementQuestionTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRCommunityProtectionRiskTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRCommunityProtectionProfileTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRCommunityProtectionRiskMessageAdviceTableView_V1"));

				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrumentTariffGroup"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrument"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRLodgementQuestion"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRCommunityProtectionRisk"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRCommunityProtectionProfile"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRCommunityProtectionRiskMessageAdvice"));

				//Verify Version 488 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(488).UpgradeScript);

				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentTariffGroupTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRLodgementQuestionTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRCommunityProtectionRiskTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRCommunityProtectionProfileTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRCommunityProtectionRiskMessageAdviceTableView_V1"));

				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrumentTariffGroup"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrument"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRLodgementQuestion"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRCommunityProtectionRisk"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRCommunityProtectionProfile"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRCommunityProtectionRiskMessageAdvice"));

				#endregion

				#region AssertVersion489Modification

				restoreSb.Clear();

				sql = @"CREATE TABLE [CMRPreferenceSchemePeriodSnapshot]
				(
					[PF_PK] UNIQUEIDENTIFIER NOT NULL,
					[PF_SchemeType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceSchemePeriodSnapshot_PF_SchemeType]  DEFAULT (''),
					[PF_PeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceSchemePeriodSnapshot_PF_PeriodIdentifier]  DEFAULT ((0)),
					[PF_CreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRPreferenceSchemePeriodSnapshot_PF_CreationTimestamp]  DEFAULT (''),
					[PF_StartDate] DATETIME NULL,
					[PF_EndDate] DATETIME NULL,
					[PF_Sequence] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceSchemePeriodSnapshot_PF_Sequence]  DEFAULT ((0)),
					[PF_Description] VARCHAR(250) NOT NULL CONSTRAINT [DF_CMRPreferenceSchemePeriodSnapshot_PF_Description]  DEFAULT (''),
					CONSTRAINT [PF_PK] PRIMARY KEY CLUSTERED
					(
						[PF_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRPreferenceSchemePeriodSnapshot] ON [CMRPreferenceSchemePeriodSnapshot]
				(
					[PF_SchemeType] ASC,
					[PF_PeriodIdentifier] ASC,
					[PF_CreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPreferenceSchemePeriodSnapshot"));
				sql = @"CREATE VIEW CMRPreferenceSchemePeriodSnapshotTableView_V1 AS
				SELECT PF_PK,
				PF_SchemeType,
				PF_PeriodIdentifier,
				PF_CreationTimestamp,
				PF_StartDate,
				PF_EndDate,
				PF_Sequence,
				PF_Description
				FROM CMRPreferenceSchemePeriodSnapshot";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPreferenceSchemePeriodSnapshotTableView_V1"));

				sql = @"CREATE TABLE [CMRStatisticalClassificationPeriodSnapshot]
				(
					[SC_PK] UNIQUEIDENTIFIER NOT NULL,
					[SC_TariffClassificationNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_TariffClassificationNumber]  DEFAULT (''),
					[SC_StatisticalClassificationCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_StatisticalClassificationCode]  DEFAULT (''),
					[SC_PeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_PeriodIdentifier]  DEFAULT ((0)),
					[SC_CreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_CreationTimestamp]  DEFAULT (''),
					[SC_StartDate] DATETIME NULL,
					[SC_EndDate] DATETIME NULL,
					[SC_Sequence] SMALLINT NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_Sequence]  DEFAULT ((0)),
					[SC_QuantityUnit] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_QuantityUnit]  DEFAULT (''),
					[SC_SecondQuantityUnit] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_SecondQuantityUnit]  DEFAULT (''),
					[SC_LowUnitValueAmount] DECIMAL(13, 4) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_LowUnitValueAmount]  DEFAULT ((0)),
					[SC_HighUnitValueAmount] DECIMAL(13, 4) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_HighUnitValueAmount]  DEFAULT ((0)),
					[SC_SecondLowUnitValueAmount] DECIMAL(13, 4) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_SecondLowUnitValueAmount]  DEFAULT ((0)),
					[SC_SecondHighUnitValueAmount] DECIMAL(13, 4) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodSnapshot_SC_SecondHighUnitValueAmount]  DEFAULT ((0)),
					CONSTRAINT [SC_PK] PRIMARY KEY CLUSTERED
					(
						[SC_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRStatisticalClassificationPeriodSnapshot] ON [CMRStatisticalClassificationPeriodSnapshot]
				(
					[SC_TariffClassificationNumber] ASC,
					[SC_StatisticalClassificationCode] ASC,
					[SC_PeriodIdentifier] ASC,
					[SC_CreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRStatisticalClassificationPeriodSnapshot"));
				sql = @"CREATE VIEW CMRStatisticalClassificationPeriodSnapshotTableView_V1 AS
				SELECT SC_PK,
				SC_TariffClassificationNumber,
				SC_StatisticalClassificationCode,
				SC_PeriodIdentifier,
				SC_CreationTimestamp,
				SC_StartDate,
				SC_EndDate,
				SC_Sequence,
				SC_QuantityUnit,
				SC_SecondQuantityUnit,
				SC_LowUnitValueAmount,
				SC_HighUnitValueAmount,
				SC_SecondLowUnitValueAmount,
				SC_SecondHighUnitValueAmount
				FROM CMRStatisticalClassificationPeriodSnapshot";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRStatisticalClassificationPeriodSnapshotTableView_V1"));

				sql = @"CREATE TABLE [CMRStatisticalClassificationPeriodCharacteristic]
				(
					[SH_PK] UNIQUEIDENTIFIER NOT NULL,
					[SH_CharacteristicCode] SMALLINT NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodCharacteristic_SH_CharacteristicCode] DEFAULT ((0)),
					[SH_SC_CMRStatisticalClassificationPeriodSnapshot] UNIQUEIDENTIFIER NOT NULL,
					CONSTRAINT [SH_PK] PRIMARY KEY CLUSTERED ([SH_PK] ASC),
					CONSTRAINT [FK_CMRStatisticalClassificationPeriodCharacteristic_CMRStatisticalClassificationPeriodSnapshot] FOREIGN KEY ([SH_SC_CMRStatisticalClassificationPeriodSnapshot]) REFERENCES [CMRStatisticalClassificationPeriodSnapshot] ([SC_PK]) ON DELETE CASCADE
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRStatisticalClassificationPeriodCharacteristic] ON [CMRStatisticalClassificationPeriodCharacteristic]
				(
					[SH_CharacteristicCode] ASC,
					[SH_SC_CMRStatisticalClassificationPeriodSnapshot] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRStatisticalClassificationPeriodCharacteristic"));
				sql = @"CREATE VIEW CMRStatisticalClassificationPeriodCharacteristicTableView_V1 AS
				SELECT SH_PK,
				SH_CharacteristicCode,
				SH_SC_CMRStatisticalClassificationPeriodSnapshot
				FROM CMRStatisticalClassificationPeriodCharacteristic";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRStatisticalClassificationPeriodCharacteristicTableView_V1"));

				sql = @"CREATE TABLE [CMRPermitRequirement]
				(
					[PQ_PK] UNIQUEIDENTIFIER NOT NULL,
					[PQ_PermitRequirementAHECCCode] INT CONSTRAINT [DF_CMRPermitRequirement_PQ_PermitRequirementAHECCCode] DEFAULT ((0)) NOT NULL,
					[PQ_PermitRequirementPIAPrefixCode] VARCHAR(5) CONSTRAINT [DF_CMRPermitRequirement_PQ_PermitRequirementPIAPrefixCode] DEFAULT ('') NOT NULL,
					[PQ_PermitRequirementStartDate] DATETIME,
					[PQ_PermitRequirementEndDate] DATETIME,
					[PQ_PermitRequirementMandatoryIndicator] CHAR(1) CONSTRAINT [DF_CMRPermitRequirement_PQ_PermitRequirementMandatoryIndicator] DEFAULT ('N') NOT NULL,
					CONSTRAINT [PQ_PK] PRIMARY KEY CLUSTERED
					(
						[PQ_PK] ASC
					)
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPermitRequirement"));
				sql = @"CREATE VIEW CMRPermitRequirementTableView_V1 AS
				SELECT PQ_PK,
				PQ_PermitRequirementAHECCCode,
				PQ_PermitRequirementPIAPrefixCode,
				PQ_PermitRequirementStartDate,
				PQ_PermitRequirementEndDate,
				PQ_PermitRequirementMandatoryIndicator
				FROM CMRPermitRequirement";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPermitRequirementTableView_V1"));

				sql = @"CREATE TABLE [CMRPermitRequirementExclusions]
				(
					[PE_PK] UNIQUEIDENTIFIER NOT NULL,
					[PE_PermitRequirementExclusionAHECCCode] INT CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionAHECCCode] DEFAULT ((0)) NOT NULL,
					[PE_PermitRequirementExclusionPIAPrefixCode] VARCHAR(5) CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionPIAPrefixCode] DEFAULT ('') NOT NULL,
					[PE_PermitRequirementExclusionSequenceNumber] SMALLINT CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionSequenceNumber] DEFAULT ((0)) NOT NULL,
					[PE_PermitRequirementExclusionDestinationCountryCode] VARCHAR(2) CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionDestinationCountryCode] DEFAULT ('') NOT NULL,
					[PE_PermitRequirementExclusionGoodsOriginCode] VARCHAR(5) CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionGoodsOriginCode] DEFAULT ('') NOT NULL,
					[PE_PermitRequirementExclusionFOBAmountOperatorCode] VARCHAR(2) CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionFOBAmountOperatorCode] DEFAULT ('') NOT NULL,
					[PE_PermitRequirementExclusionFOBAmount] INT CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionFOBAmount] DEFAULT ((0)) NOT NULL,
					[PE_PermitRequirementExclusionNettQuantityOperatorCode] VARCHAR(2) CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionNettQuantityOperatorCode] DEFAULT ('') NOT NULL,
					[PE_PermitRequirementExclusionNettQuantityValue] DECIMAL(15,5) CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionNettQuantityValue] DEFAULT ((0)) NOT NULL,
					[PE_PermitRequirementExclusionExcludingPIAPrefixCode] VARCHAR(5) CONSTRAINT [DF_CMRPermitRequirementExclusions_PE_PermitRequirementExclusionExcludingPIAPrefixCode] DEFAULT ('') NOT NULL,
					CONSTRAINT [PE_PK] PRIMARY KEY CLUSTERED
					(
						[PE_PK] ASC
					)
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPermitRequirementExclusions"));
				sql = @"CREATE VIEW CMRPermitRequirementExclusionsTableView_V1 AS
				SELECT PE_PK,
				PE_PermitRequirementExclusionAHECCCode,
				PE_PermitRequirementExclusionDestinationCountryCode,
				PE_PermitRequirementExclusionExcludingPIAPrefixCode,
				PE_PermitRequirementExclusionFOBAmount,
				PE_PermitRequirementExclusionFOBAmountOperatorCode,
				PE_PermitRequirementExclusionGoodsOriginCode,
				PE_PermitRequirementExclusionNettQuantityOperatorCode,
				PE_PermitRequirementExclusionNettQuantityValue,
				PE_PermitRequirementExclusionPIAPrefixCode,
				PE_PermitRequirementExclusionSequenceNumber
				FROM CMRPermitRequirementExclusions";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPermitRequirementExclusionsTableView_V1"));

				sql = @"CREATE TABLE [CMRPreferenceSchemePeriodCountry]
				(
					[PC_PK] UNIQUEIDENTIFIER NOT NULL,
					[PC_PF_CMRPreferenceSchemePeriodSnapshot] UNIQUEIDENTIFIER NOT NULL,
					[PC_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
					CONSTRAINT [PC_PK] PRIMARY KEY CLUSTERED ( [PC_PK] ASC ),
					CONSTRAINT [DF_CMRPreferenceSchemePeriodCountry_PC_ZZZ_NKDataGrouping] CHECK ([PC_ZZZ_NKDataGrouping] <> ''),
					CONSTRAINT FK_CMRPreferenceSchemePeriodCountry_CMRPreferenceSchemePeriodSnapshot FOREIGN KEY(PC_PF_CMRPreferenceSchemePeriodSnapshot) REFERENCES CMRPreferenceSchemePeriodSnapshot (PF_PK) ON DELETE CASCADE,
					CONSTRAINT [FK_CMRPreferenceSchemePeriodCountry_RefDataGrouping] FOREIGN KEY ([PC_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping])
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRPreferenceSchemePeriodCountry] ON [CMRPreferenceSchemePeriodCountry]
				(
					[PC_PF_CMRPreferenceSchemePeriodSnapshot] ASC,
					[PC_ZZZ_NKDataGrouping] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPreferenceSchemePeriodCountry"));
				sql = @"CREATE VIEW CMRPreferenceSchemePeriodCountryTableView_V1 AS
				SELECT PC_PK,
				PC_PF_CMRPreferenceSchemePeriodSnapshot,
				PC_ZZZ_NKDataGrouping
				FROM CMRPreferenceSchemePeriodCountry";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPreferenceSchemePeriodCountryTableView_V1"));

				sql = @"CREATE TABLE [CMRPreferenceSchemeRule]
				(
					[PR_PK] UNIQUEIDENTIFIER NOT NULL,
					[PR_PF_CMRPreferenceSchemePeriodSnapshot] UNIQUEIDENTIFIER NOT NULL,
					[PR_RuleType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceSchemeRule_PR_RuleType]  DEFAULT (''),
					CONSTRAINT [PR_PK] PRIMARY KEY CLUSTERED ([PR_PK] ASC),
					CONSTRAINT [FK_CMRPreferenceSchemeRule_CMRPreferenceSchemePeriodSnapshot] FOREIGN KEY ([PR_PF_CMRPreferenceSchemePeriodSnapshot]) REFERENCES [CMRPreferenceSchemePeriodSnapshot] ([PF_PK]) ON DELETE CASCADE
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRPreferenceSchemeRule] ON [CMRPreferenceSchemeRule]
				(
					[PR_PF_CMRPreferenceSchemePeriodSnapshot] ASC,
					[PR_RuleType] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPreferenceSchemeRule"));
				sql = @"CREATE VIEW CMRPreferenceSchemeRuleTableView_V1 AS
				SELECT PR_PK,
				PR_PF_CMRPreferenceSchemePeriodSnapshot,
				PR_RuleType
				FROM CMRPreferenceSchemeRule";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPreferenceSchemeRuleTableView_V1"));

				sql = @"CREATE TABLE [CMRPreferenceRulePeriodTariffGroup]
				(
					[PT_PK] UNIQUEIDENTIFIER NOT NULL,
					[PT_PreferenceRulePeriodSnapshotRuleType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodTariffGroup_PT_PreferenceRulePeriodSnapshotRuleType]  DEFAULT (''),
					[PT_PreferenceRulePeriodSnapshotPeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodTariffGroup_PT_PreferenceRulePeriodSnapshotPeriodIdentifier]  DEFAULT ((0)),
					[PT_PreferenceRulePeriodSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodTariffGroup_PT_PreferenceRulePeriodSnapshotCreationTimestamp]  DEFAULT (''),
					[PT_Item] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodTariffGroup_PT_Item]  DEFAULT (''),
					CONSTRAINT [PT_PK] PRIMARY KEY CLUSTERED
					(
						[PT_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRPreferenceRulePeriodTariffGroup] ON [CMRPreferenceRulePeriodTariffGroup]
				(
					[PT_PreferenceRulePeriodSnapshotRuleType] ASC,
					[PT_PreferenceRulePeriodSnapshotPeriodIdentifier] ASC,
					[PT_PreferenceRulePeriodSnapshotCreationTimestamp] ASC,
					[PT_Item] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPreferenceRulePeriodTariffGroup"));
				sql = @"CREATE VIEW CMRPreferenceRulePeriodTariffGroupTableView_V1 AS
				SELECT PT_PK,
				PT_PreferenceRulePeriodSnapshotRuleType,
				PT_PreferenceRulePeriodSnapshotPeriodIdentifier,
				PT_PreferenceRulePeriodSnapshotCreationTimestamp,
				PT_Item
				FROM CMRPreferenceRulePeriodTariffGroup";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPreferenceRulePeriodTariffGroupTableView_V1"));

				sql = @"CREATE TABLE [CMRPreferenceRulePeriodCharacteristic]
				(
					[PI_PK] UNIQUEIDENTIFIER NOT NULL,
					[PI_PreferenceRulePeriodSnapshotRuleType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodCharacteristic_PI_PreferenceRulePeriodSnapshotRuleType]  DEFAULT (''),
					[PI_PreferenceRulePeriodSnapshotPeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodCharacteristic_PI_PreferenceRulePeriodSnapshotPeriodIdentifier]  DEFAULT ((0)),
					[PI_PreferenceRulePeriodSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodCharacteristic_PI_PreferenceRulePeriodSnapshotCreationTimestamp]  DEFAULT (''),
					[PI_CharacteristicCode] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodCharacteristic_PI_CharacteristicCode]  DEFAULT ((0)),
					CONSTRAINT [PI_PK] PRIMARY KEY CLUSTERED
					(
						[PI_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRPreferenceRulePeriodCharacteristic] ON [CMRPreferenceRulePeriodCharacteristic]
				(
					[PI_PreferenceRulePeriodSnapshotRuleType] ASC,
					[PI_PreferenceRulePeriodSnapshotPeriodIdentifier] ASC,
					[PI_PreferenceRulePeriodSnapshotCreationTimestamp] ASC,
					[PI_CharacteristicCode] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPreferenceRulePeriodCharacteristic"));
				sql = @"CREATE VIEW CMRPreferenceRulePeriodCharacteristicTableView_V1 AS
				SELECT PI_PK,
				PI_PreferenceRulePeriodSnapshotRuleType,
				PI_PreferenceRulePeriodSnapshotPeriodIdentifier,
				PI_PreferenceRulePeriodSnapshotCreationTimestamp,
				PI_CharacteristicCode
				FROM CMRPreferenceRulePeriodCharacteristic";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPreferenceRulePeriodCharacteristicTableView_V1"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceSchemePeriodSnapshotTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRStatisticalClassificationPeriodSnapshotTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRStatisticalClassificationPeriodCharacteristicTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPermitRequirementTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPermitRequirementExclusionsTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceSchemePeriodCountryTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceSchemeRuleTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodTariffGroupTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodCharacteristicTableView_V1"));

				Assert.True(TestDBHelper.TableExists(conn, "CMRPreferenceSchemePeriodSnapshot"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRStatisticalClassificationPeriodSnapshot"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRStatisticalClassificationPeriodCharacteristic"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPermitRequirement"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPermitRequirementExclusions"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPreferenceSchemePeriodCountry"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPreferenceSchemeRule"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPreferenceRulePeriodTariffGroup"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPreferenceRulePeriodCharacteristic"));

				//Verify Version 489 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(489).UpgradeScript);

				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceSchemePeriodSnapshotTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRStatisticalClassificationPeriodSnapshotTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRStatisticalClassificationPeriodCharacteristicTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPermitRequirementTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPermitRequirementExclusionsTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceSchemePeriodCountryTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceSchemeRuleTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodTariffGroupTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodCharacteristicTableView_V1"));

				Assert.False(TestDBHelper.TableExists(conn, "CMRPreferenceSchemePeriodSnapshot"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRStatisticalClassificationPeriodSnapshot"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRStatisticalClassificationPeriodCharacteristic"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPermitRequirement"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPermitRequirementExclusions"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPreferenceSchemePeriodCountry"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPreferenceSchemeRule"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPreferenceRulePeriodTariffGroup"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPreferenceRulePeriodCharacteristic"));

				#endregion

				#region AssertVersion490Modification

				restoreSb.Clear();

				sql = @"CREATE TABLE [CMRTreatmentRatePeriodSnapshot]
				(
					[TP_PK] UNIQUEIDENTIFIER NOT NULL,
					[TP_Code] VARCHAR(3) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_Code] DEFAULT ('') NOT NULL,
					[TP_RateNumber] VARCHAR(3) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_RateNumber] DEFAULT ('') NOT NULL,
					[TP_PreferenceSchemeType] VARCHAR(4) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_PreferenceSchemeType] DEFAULT ('') NOT NULL,
					[TP_PeriodIdentifier] SMALLINT CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_PeriodIdentifier] DEFAULT ((0)) NOT NULL,
					[TP_CreationTimestamp] VARCHAR(20) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_CreationTimestamp] DEFAULT ('') NOT NULL,
					[TP_CalculationType] VARCHAR(10) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_CalculationType] DEFAULT ('') NOT NULL,
					[TP_CustomsValueRate] DECIMAL(12,5) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_CustomsValueRate] DEFAULT ((0)) NOT NULL,
					[TP_QuantityRate] DECIMAL(12,5) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_QuantityRate] DEFAULT ((0)) NOT NULL,
					[TP_QuantityUnit] VARCHAR(2) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_QuantityUnit] DEFAULT ('') NOT NULL,
					[TP_SecondQuantityRate] DECIMAL(12,5) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_SecondQuantityRate] DEFAULT ((0)) NOT NULL,
					[TP_SecondQuantityUnit] VARCHAR(2) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_SecondQuantityUnit] DEFAULT ('') NOT NULL,
					[TP_OtherDutyFactorRate] DECIMAL(12,5) CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_OtherDutyFactorRate] DEFAULT ((0)) NOT NULL,
					[TP_StartDate] DATETIME,
					[TP_EndDate] DATETIME,
					[TP_Sequence] SMALLINT CONSTRAINT [DF_CMRTreatmentRatePeriodSnapshot_TP_Sequence] DEFAULT ((0)) NOT NULL,
					CONSTRAINT [TP_PK] PRIMARY KEY CLUSTERED
					(
						[TP_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRTreatmentRatePeriodSnapshot On [CMRTreatmentRatePeriodSnapshot]
				(
					[TP_Code],
					[TP_RateNumber],
					[TP_PreferenceSchemeType],
					[TP_PeriodIdentifier],
					[TP_CreationTimestamp]
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTreatmentRatePeriodSnapshot"));
				sql = @"CREATE VIEW CMRTreatmentRatePeriodSnapshotTableView_V1 AS
				SELECT TP_PK,
				TP_Code,
				TP_RateNumber,
				TP_PreferenceSchemeType,
				TP_PeriodIdentifier,
				TP_CreationTimestamp,
				TP_CalculationType,
				TP_CustomsValueRate,
				TP_QuantityRate,
				TP_QuantityUnit,
				TP_SecondQuantityRate,
				TP_SecondQuantityUnit,
				TP_OtherDutyFactorRate,
				TP_StartDate,
				TP_EndDate,
				TP_Sequence
				FROM CMRTreatmentRatePeriodSnapshot";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTreatmentRatePeriodSnapshotTableView_V1"));

				sql = @"CREATE TABLE [CMRTariffRatePeriodSnapshot]
				(
					[TT_PK] UNIQUEIDENTIFIER NOT NULL,
					[TT_TariffClassificationNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_TariffClassificationNumber]  DEFAULT (''),
					[TT_RateNumber] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_RateNumber]  DEFAULT (''),
					[TT_PreferenceSchemeType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_PreferenceSchemeType]  DEFAULT (''),
					[TT_PeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_PeriodIdentifier]  DEFAULT ((0)),
					[TT_CreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_CreationTimestamp]  DEFAULT (''),
					[TT_CalculationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_CalculationType]  DEFAULT (''),
					[TT_CustomsValueRate] DECIMAL(12, 5) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_CustomsValueRate]  DEFAULT ((0)),
					[TT_QuantityRate] DECIMAL(12, 5) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_QuantityRate]  DEFAULT ((0)),
					[TT_QuantityUnit] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_QuantityUnit]  DEFAULT (''),
					[TT_SecondQuantityRate] DECIMAL(12, 5) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_SecondQuantityRate]  DEFAULT ((0)),
					[TT_SecondQuantityUnit] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_SecondQuantityUnit]  DEFAULT (''),
					[TT_OtherDutyFactorRate] DECIMAL(12, 5) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_OtherDutyFactorRate]  DEFAULT ((0)),
					[TT_StartDate] DATETIME NULL,
					[TT_EndDate] DATETIME NULL,
					[TT_Sequence] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodSnapshot_TT_Sequence]  DEFAULT ((0)),
					CONSTRAINT [TT_PK] PRIMARY KEY CLUSTERED
					(
						[TT_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTariffRatePeriodSnapshot] ON [CMRTariffRatePeriodSnapshot]
				(
					[TT_TariffClassificationNumber] ASC,
					[TT_RateNumber] ASC,
					[TT_PreferenceSchemeType] ASC,
					[TT_PeriodIdentifier] ASC,
					[TT_CreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTariffRatePeriodSnapshot"));
				sql = @"CREATE VIEW CMRTariffRatePeriodSnapshotTableView_V1 AS
				SELECT TT_PK,
				TT_TariffClassificationNumber,
				TT_RateNumber,
				TT_PreferenceSchemeType,
				TT_PeriodIdentifier,
				TT_CreationTimestamp,
				TT_CalculationType,
				TT_CustomsValueRate,
				TT_QuantityRate,
				TT_QuantityUnit,
				TT_SecondQuantityRate,
				TT_SecondQuantityUnit,
				TT_OtherDutyFactorRate,
				TT_StartDate,
				TT_EndDate,
				TT_Sequence
				FROM CMRTariffRatePeriodSnapshot";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTariffRatePeriodSnapshotTableView_V1"));

				sql = @"CREATE TABLE [CMRTreatmentSnapshot]
				(
					[TE_PK] UNIQUEIDENTIFIER NOT NULL,
					[TE_Code] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshot_TE_Code]  DEFAULT (''),
					[TE_CreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshot_TE_CreationTimestamp]  DEFAULT (''),
					[TE_ConcessionalItemNumber] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshot_TE_ConcessionalItemNumber]  DEFAULT (''),
					[TE_TariffValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshot_TE_TariffValidationType]  DEFAULT (''),
					[TE_CountryValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshot_TE_CountryValidationType]  DEFAULT (''),
					[TE_Sequence] SMALLINT NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshot_TE_Sequence]  DEFAULT ((0)),
					CONSTRAINT [TE_PK] PRIMARY KEY CLUSTERED
					(
						[TE_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTreatmentSnapshot] ON [dbo].[CMRTreatmentSnapshot]
				(
					[TE_Code] ASC,
					[TE_CreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTreatmentSnapshot"));
				sql = @"CREATE VIEW CMRTreatmentSnapshotTableView_V1 AS
				SELECT TE_PK,
				TE_Code,
				TE_CreationTimestamp,
				TE_ConcessionalItemNumber,
				TE_TariffValidationType,
				TE_CountryValidationType,
				TE_Sequence
				FROM CMRTreatmentSnapshot";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTreatmentSnapshotTableView_V1"));

				sql = @"CREATE TABLE [CMRTreatmentRatePeriodAdditionalDutyCalculation]
				(
					[TD_PK] UNIQUEIDENTIFIER NOT NULL,
					[TD_Identifier] SMALLINT CONSTRAINT [DF_CMRTreatmentRatePeriodAdditionalDutyCalculation_TD_Identifier] DEFAULT ((0)) NOT NULL,
					[TD_TP_CMRTreatmentRatePeriodSnapshot] UNIQUEIDENTIFIER NOT NULL,
					[TD_CustomsValueRate] DECIMAL(12,5) CONSTRAINT [DF_CMRTreatmentRatePeriodAdditionalDutyCalculation_TD_CustomsValueRate] DEFAULT ((0)) NOT NULL,
					[TD_QuantityRate] DECIMAL(12,5) CONSTRAINT [DF_CMRTreatmentRatePeriodAdditionalDutyCalculation_TD_QuantityRate] DEFAULT ((0)) NOT NULL,
					[TD_SecondQuantityRate] DECIMAL(12,5) CONSTRAINT [DF_CMRTreatmentRatePeriodAdditionalDutyCalculation_TD_SecondQuantityRate] DEFAULT ((0)) NOT NULL,
					[TD_OtherDutyFactorRate] DECIMAL(12,5) CONSTRAINT [DF_CMRTreatmentRatePeriodAdditionalDutyCalculation_TD_OtherDutyFactorRate] DEFAULT ((0)) NOT NULL,
					CONSTRAINT [TD_PK] PRIMARY KEY CLUSTERED ([TD_PK] ASC),
					CONSTRAINT [FK_CMRTreatmentRatePeriodAdditionalDutyCalculation_CMRTreatmentRatePeriodSnapshot] FOREIGN KEY ([TD_TP_CMRTreatmentRatePeriodSnapshot]) REFERENCES [CMRTreatmentRatePeriodSnapshot] ([TP_PK]) ON DELETE CASCADE
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRTreatmentRatePeriodAdditionalDutyCalculation On [CMRTreatmentRatePeriodAdditionalDutyCalculation]
				(
					[TD_Identifier],
					[TD_TP_CMRTreatmentRatePeriodSnapshot]
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTreatmentRatePeriodAdditionalDutyCalculation"));
				sql = @"CREATE VIEW CMRTreatmentRatePeriodAdditionalDutyCalculationTableView_V1 AS
				SELECT TD_PK,
				TD_Identifier,
				TD_TP_CMRTreatmentRatePeriodSnapshot,
				TD_CustomsValueRate,
				TD_QuantityRate,
				TD_SecondQuantityRate,
				TD_OtherDutyFactorRate
				FROM CMRTreatmentRatePeriodAdditionalDutyCalculation";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTreatmentRatePeriodAdditionalDutyCalculationTableView_V1"));

				sql = @"CREATE TABLE [CMRTreatmentRatePeriodCharacteristic]
				(
					[TR_PK] UNIQUEIDENTIFIER NOT NULL,
					[TR_CharacteristicCode] SMALLINT CONSTRAINT [DF_CMRTreatmentRatePeriodCharacteristic_TR_CharacteristicCode] DEFAULT ((0)) NOT NULL,
					[TR_TP_CMRTreatmentRatePeriodSnapshot] UNIQUEIDENTIFIER NOT NULL,
					CONSTRAINT [TR_PK] PRIMARY KEY CLUSTERED ([TR_PK] ASC),
					CONSTRAINT [FK_CMRTreatmentRatePeriodCharacteristic_CMRTreatmentRatePeriodSnapshot] FOREIGN KEY ([TR_TP_CMRTreatmentRatePeriodSnapshot]) REFERENCES [CMRTreatmentRatePeriodSnapshot] ([TP_PK]) ON DELETE CASCADE
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRTreatmentRatePeriodCharacteristic On CMRTreatmentRatePeriodCharacteristic
				(
					[TR_CharacteristicCode],
					[TR_TP_CMRTreatmentRatePeriodSnapshot]
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTreatmentRatePeriodCharacteristic"));
				sql = @"CREATE VIEW CMRTreatmentRatePeriodCharacteristicTableView_V1 AS
				SELECT TR_PK,
				TR_CharacteristicCode,
				TR_TP_CMRTreatmentRatePeriodSnapshot
				FROM CMRTreatmentRatePeriodCharacteristic";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTreatmentRatePeriodCharacteristicTableView_V1"));

				sql = @"CREATE TABLE [CMRTariffRatePeriodAdditionalDutyCalculation]
				(
					[TA_PK] UNIQUEIDENTIFIER NOT NULL,
					[TA_TT_CMRTariffRatePeriodSnapshot] UNIQUEIDENTIFIER NOT NULL,
					[TA_Identifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodAdditionalDutyCalculation_TA_Identifier]  DEFAULT ((0)),
					[TA_CustomsValueRate] DECIMAL(12, 5) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodAdditionalDutyCalculation_TA_CustomsValueRate]  DEFAULT ((0)),
					[TA_QuantityRate] DECIMAL(12, 5) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodAdditionalDutyCalculation_TA_QuantityRate]  DEFAULT ((0)),
					[TA_SecondQuantityRate] DECIMAL(12, 5) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodAdditionalDutyCalculation_TA_SecondQuantityRate]  DEFAULT ((0)),
					[TA_OtherDutyFactorRate] DECIMAL(12, 5) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodAdditionalDutyCalculation_TA_OtherDutyFactorRate]  DEFAULT ((0)),

					CONSTRAINT [TA_PK] PRIMARY KEY CLUSTERED ( [TA_PK] ASC ),
					CONSTRAINT FK_CMRTariffRatePeriodAdditionalDutyCalculation_CMRTariffRatePeriodSnapshot FOREIGN KEY(TA_TT_CMRTariffRatePeriodSnapshot) REFERENCES CMRTariffRatePeriodSnapshot (TT_PK) ON DELETE CASCADE,
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTariffRatePeriodAdditionalDutyCalculation] ON [CMRTariffRatePeriodAdditionalDutyCalculation]
				(
					[TA_Identifier] ASC,
					[TA_TT_CMRTariffRatePeriodSnapshot] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTariffRatePeriodAdditionalDutyCalculation"));
				sql = @"CREATE VIEW CMRTariffRatePeriodAdditionalDutyCalculationTableView_V1 AS
				SELECT TA_PK,
				TA_TT_CMRTariffRatePeriodSnapshot,
				TA_Identifier,
				TA_CustomsValueRate,
				TA_QuantityRate,
				TA_SecondQuantityRate,
				TA_OtherDutyFactorRate
				FROM CMRTariffRatePeriodAdditionalDutyCalculation";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTariffRatePeriodAdditionalDutyCalculationTableView_V1"));

				sql = @"CREATE TABLE [CMRTariffRatePeriodCharacteristic]
				(
					[TH_PK] UNIQUEIDENTIFIER NOT NULL,
					[TH_TT_CMRTariffRatePeriodSnapshot] UNIQUEIDENTIFIER NOT NULL,
					[TH_CharacteristicCode] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodCharacteristic_TH_CharacteristicCode]  DEFAULT ((0)),

					CONSTRAINT [TH_PK] PRIMARY KEY CLUSTERED ( [TH_PK] ASC ),
					CONSTRAINT FK_CMRTariffRatePeriodCharacteristic_CMRTariffRatePeriodSnapshot FOREIGN KEY(TH_TT_CMRTariffRatePeriodSnapshot) REFERENCES CMRTariffRatePeriodSnapshot (TT_PK) ON DELETE CASCADE,
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTariffRatePeriodCharacteristic] ON [CMRTariffRatePeriodCharacteristic]
				(
					[TH_CharacteristicCode] ASC,
					[TH_TT_CMRTariffRatePeriodSnapshot] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTariffRatePeriodCharacteristic"));
				sql = @"CREATE VIEW CMRTariffRatePeriodCharacteristicTableView_V1 AS
				SELECT TH_PK,
				TH_TT_CMRTariffRatePeriodSnapshot,
				TH_CharacteristicCode
				FROM CMRTariffRatePeriodCharacteristic";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTariffRatePeriodCharacteristicTableView_V1"));

				sql = @"CREATE TABLE [CMRTreatmentSnapshotCountry]
				(
					[TY_PK] UNIQUEIDENTIFIER NOT NULL,
					[TY_TE_CMRTreatmentSnapshot] UNIQUEIDENTIFIER NOT NULL,
					[TY_TreatmentSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshotCountry_TY_TreatmentSnapshotCreationTimestamp]  DEFAULT (''),
					[TY_CountryCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshotCountry_TY_CountryCode]  DEFAULT (''),
					CONSTRAINT [TY_PK] PRIMARY KEY CLUSTERED ([TY_PK] ASC),
					CONSTRAINT [FK_CMRTreatmentSnapshotCountry_CMRTreatmentSnapshot] FOREIGN KEY ([TY_TE_CMRTreatmentSnapshot]) REFERENCES CMRTreatmentSnapshot ([TE_PK]) ON DELETE CASCADE
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTreatmentSnapshotCountry] ON [CMRTreatmentSnapshotCountry]
				(
					[TY_TE_CMRTreatmentSnapshot] ASC,
					[TY_TreatmentSnapshotCreationTimestamp] ASC,
					[TY_CountryCode] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTreatmentSnapshotCountry"));
				sql = @"CREATE VIEW CMRTreatmentSnapshotCountryTableView_V1 AS
				SELECT TY_PK,
				TY_TE_CMRTreatmentSnapshot,
				TY_TreatmentSnapshotCreationTimestamp,
				TY_CountryCode
				FROM CMRTreatmentSnapshotCountry";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTreatmentSnapshotCountryTableView_V1"));

				sql = @"CREATE TABLE [CMRTreatmentSnapshotTariffGroup]
				(
					[TG_PK] UNIQUEIDENTIFIER NOT NULL,
					[TG_TE_CMRTreatmentSnapshot] UNIQUEIDENTIFIER NOT NULL,
					[TG_TreatmentSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshotTariffGroup_TG_TreatmentSnapshotCreationTimestamp]  DEFAULT (''),
					[TG_TariffGroupItem] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRTreatmentSnapshotTariffGroup_TG_TariffGroupItem]  DEFAULT (''),
					CONSTRAINT [TG_PK] PRIMARY KEY CLUSTERED ([TG_PK] ASC),
					CONSTRAINT [FK_CMRTreatmentSnapshotTariffGroup_CMRTreatmentSnapshot] FOREIGN KEY ([TG_TE_CMRTreatmentSnapshot]) REFERENCES CMRTreatmentSnapshot ([TE_PK]) ON DELETE CASCADE
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTreatmentSnapshotTariffGroup] ON [CMRTreatmentSnapshotTariffGroup]
				(
					[TG_TE_CMRTreatmentSnapshot] ASC,
					[TG_TreatmentSnapshotCreationTimestamp] ASC,
					[TG_TariffGroupItem] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTreatmentSnapshotTariffGroup"));
				sql = @"CREATE VIEW CMRTreatmentSnapshotTariffGroupTableView_V1 AS
				SELECT TG_PK,
				TG_TE_CMRTreatmentSnapshot,
				TG_TreatmentSnapshotCreationTimestamp,
				TG_TariffGroupItem
				FROM CMRTreatmentSnapshotTariffGroup";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTreatmentSnapshotTariffGroupTableView_V1"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentRatePeriodSnapshotTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTariffRatePeriodSnapshotTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentSnapshotTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentRatePeriodAdditionalDutyCalculationTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentRatePeriodCharacteristicTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTariffRatePeriodAdditionalDutyCalculationTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTariffRatePeriodCharacteristicTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentSnapshotCountryTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentSnapshotTariffGroupTableView_V1"));

				Assert.True(TestDBHelper.TableExists(conn, "CMRTreatmentRatePeriodSnapshot"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTariffRatePeriodSnapshot"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTreatmentSnapshot"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTreatmentRatePeriodAdditionalDutyCalculation"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTreatmentRatePeriodCharacteristic"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTariffRatePeriodAdditionalDutyCalculation"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTariffRatePeriodCharacteristic"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTreatmentSnapshotCountry"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTreatmentSnapshotTariffGroup"));

				//Verify Version 490 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(490).UpgradeScript);

				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentRatePeriodSnapshotTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTariffRatePeriodSnapshotTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentSnapshotTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentRatePeriodAdditionalDutyCalculationTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentRatePeriodCharacteristicTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTariffRatePeriodAdditionalDutyCalculationTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTariffRatePeriodCharacteristicTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentSnapshotCountryTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentSnapshotTariffGroupTableView_V1"));

				Assert.False(TestDBHelper.TableExists(conn, "CMRTreatmentRatePeriodSnapshot"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTariffRatePeriodSnapshot"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTreatmentSnapshot"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTreatmentRatePeriodAdditionalDutyCalculation"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTreatmentRatePeriodCharacteristic"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTariffRatePeriodAdditionalDutyCalculation"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTariffRatePeriodCharacteristic"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTreatmentSnapshotCountry"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTreatmentSnapshotTariffGroup"));

				#endregion

				#region AssertVersion491Modification

				restoreSb.Clear();

				sql = @"CREATE TABLE [CMRInstrumentCategory]
				(
					[IC_PK] UNIQUEIDENTIFIER NOT NULL,
					[IC_Code] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRInstrumentCategory_IC_Code] DEFAULT '',
					[IC_QuantityUnit] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRInstrumentCategory_IC_QuantityUnit] DEFAULT '',
					[IC_CountryValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRInstrumentCategory_IC_CountryValidationType] DEFAULT '',
					[IC_TariffValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRInstrumentCategory_IC_TariffValidationType] DEFAULT '',
					[IC_StartDate] DATETIME NULL,
					[IC_EndDate] DATETIME NULL,
					[IC_Description] VARCHAR(250) NOT NULL CONSTRAINT [DF_CMRInstrumentCategory_IC_Description] DEFAULT ''
					CONSTRAINT [IC_PK] PRIMARY KEY CLUSTERED
					(
						[IC_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrumentCategory On CMRInstrumentCategory(IC_Code)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrumentCategory"));
				sql = @"CREATE VIEW CMRInstrumentCategoryTableView_V1 AS
				SELECT IC_PK,
				IC_Code,
				IC_QuantityUnit,
				IC_CountryValidationType,
				IC_TariffValidationType,
				IC_StartDate,
				IC_EndDate,
				IC_Description
				FROM CMRInstrumentCategory";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentCategoryTableView_V1"));

				sql = @"CREATE TABLE [CMRInstrumentCategoryCharacteristic]
				(
					[IR_PK] UNIQUEIDENTIFIER NOT NULL,
					[IR_InstrumentCategoryCode] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRInstrumentCategoryCharacteristic_IR_InstrumentCategoryCode] DEFAULT '',
					[IR_CharacteristicCode] SMALLINT NOT NULL CONSTRAINT [DF_CMRInstrumentCategoryCharacteristic_IR_CharacteristicCode] DEFAULT 0,
					CONSTRAINT [IR_PK] PRIMARY KEY CLUSTERED
					(
						[IR_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrumentCategoryCharacteristic On CMRInstrumentCategoryCharacteristic(IR_InstrumentCategoryCode, IR_CharacteristicCode)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrumentCategoryCharacteristic"));
				sql = @"CREATE VIEW CMRInstrumentCategoryCharacteristicTableView_V1 AS
				SELECT IR_PK,
				IR_InstrumentCategoryCode,
				IR_CharacteristicCode
				FROM CMRInstrumentCategoryCharacteristic";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentCategoryCharacteristicTableView_V1"));

				sql = @"CREATE TABLE [CMRInstrumentCategoryCountry]
				(
					[IU_PK] UNIQUEIDENTIFIER NOT NULL,
					[IU_InstrumentCategoryCode] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRInstrumentCategoryCountry_IU_InstrumentCategoryCode] DEFAULT '',
					[IU_CountryCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRInstrumentCategoryCountry_IU_CountryCode]  DEFAULT '',
					CONSTRAINT [IU_PK] PRIMARY KEY CLUSTERED
					(
						[IU_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrumentCategoryCountry On CMRInstrumentCategoryCountry(IU_InstrumentCategoryCode, IU_CountryCode)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrumentCategoryCountry"));
				sql = @"CREATE VIEW CMRInstrumentCategoryCountryTableView_V1 AS
				SELECT IU_PK,
				IU_InstrumentCategoryCode,
				IU_CountryCode
				FROM CMRInstrumentCategoryCountry";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentCategoryCountryTableView_V1"));

				sql = @"CREATE TABLE [CMRInstrumentCategoryTariffGroup]
				(
					[IT_PK] UNIQUEIDENTIFIER NOT NULL,
					[IT_InstrumentCategoryCode] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRInstrumentCategoryTariffGroup_IT_InstrumentCategoryCode] DEFAULT '',
					[IT_TariffGroupItem] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRInstrumentCategoryTariffGroup_IT_TariffGroupItem] DEFAULT '',
					[IT_TreatmentRateNumber] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRInstrumentCategoryTariffGroup_IT_TreatmentRateNumber] DEFAULT '',
					[IT_StartDate] DATETIME NULL,
					[IT_EndDate] DATETIME NULL,
					CONSTRAINT [IT_PK] PRIMARY KEY CLUSTERED
					(
						[IT_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrumentCategoryTariffGroup On CMRInstrumentCategoryTariffGroup(IT_InstrumentCategoryCode, IT_TariffGroupItem, IT_StartDate, IT_EndDate)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrumentCategoryTariffGroup"));
				sql = @"CREATE VIEW CMRInstrumentCategoryTariffGroupTableView_V1 AS
				SELECT IT_PK,
				IT_InstrumentCategoryCode,
				IT_TariffGroupItem,
				IT_TreatmentRateNumber,
				IT_StartDate,
				IT_EndDate
				FROM CMRInstrumentCategoryTariffGroup";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentCategoryTariffGroupTableView_V1"));

				sql = @"CREATE TABLE [CMRInstrumentMessageAdvice]
				(
					[IM_PK] UNIQUEIDENTIFIER NOT NULL,
					[IM_InstrumentType] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRInstrumentMessageAdvice_IM_InstrumentType] DEFAULT '',
					[IM_InstrumentNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRInstrumentMessageAdvice_IM_InstrumentNumber] DEFAULT '',
					[IM_MessageAdviceID] SMALLINT NOT NULL CONSTRAINT [DF_CMRInstrumentMessageAdvice_IM_MessageAdviceID] DEFAULT 0,
					CONSTRAINT [IM_PK] PRIMARY KEY CLUSTERED
					(
						[IM_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrumentMessageAdvice On CMRInstrumentMessageAdvice(IM_InstrumentType, IM_InstrumentNumber, IM_MessageAdviceID)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrumentMessageAdvice"));
				sql = @"CREATE VIEW CMRInstrumentMessageAdviceTableView_V1 AS
				SELECT IM_PK,
				IM_InstrumentType,
				IM_InstrumentNumber,
				IM_MessageAdviceID
				FROM CMRInstrumentMessageAdvice";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentMessageAdviceTableView_V1"));

				sql = @"CREATE TABLE [CMRInstrumentCharacteristic]
				(
					[IK_PK] UNIQUEIDENTIFIER NOT NULL,
					[IK_InstrumentType] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRInstrumentCharacteristic_IK_InstrumentType] DEFAULT '',
					[IK_InstrumentNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRInstrumentCharacteristic_IK_InstrumentNumber] DEFAULT '',
					[IK_CharacteristicCode] SMALLINT NOT NULL CONSTRAINT [DF_CMRInstrumentCharacteristic_IK_CharacteristicCode] DEFAULT 0,
					CONSTRAINT [IK_PK] PRIMARY KEY CLUSTERED
					(
						[IK_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrumentCharacteristic On CMRInstrumentCharacteristic(IK_InstrumentType, IK_InstrumentNumber, IK_CharacteristicCode)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrumentCharacteristic"));
				sql = @"CREATE VIEW CMRInstrumentCharacteristicTableView_V1 AS
				SELECT IK_PK,
				IK_InstrumentType,
				IK_InstrumentNumber,
				IK_CharacteristicCode
				FROM CMRInstrumentCharacteristic";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentCharacteristicTableView_V1"));

				sql = @"CREATE TABLE [CMRInstrumentCountry]
				(
					[IY_PK] UNIQUEIDENTIFIER NOT NULL,
					[IY_InstrumentType] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRInstrumentCountry_IY_InstrumentType] DEFAULT '',
					[IY_InstrumentNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRInstrumentCountry_IY_InstrumentNumber] DEFAULT '',
					[IY_CountryCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRInstrumentCountry_IY_CountryCode] DEFAULT '',
					CONSTRAINT [IY_PK] PRIMARY KEY CLUSTERED
					(
						[IY_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRInstrumentCountry On CMRInstrumentCountry(IY_InstrumentType, IY_InstrumentNumber, IY_CountryCode)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRInstrumentCountry"));
				sql = @"CREATE VIEW CMRInstrumentCountryTableView_V1 AS
				SELECT IY_PK,
				IY_InstrumentType,
				IY_InstrumentNumber,
				IY_CountryCode
				FROM CMRInstrumentCountry";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRInstrumentCountryTableView_V1"));

				sql = @"CREATE TABLE [CMRTreatmentRatePeriodMessageAdvice]
				(
					[TS_PK] UNIQUEIDENTIFIER NOT NULL,
					[TS_MessageAdviceIdentifier] SMALLINT CONSTRAINT [DF_CMRTreatmentRatePeriodMessageAdvice_TS_MessageAdviceIdentifier] DEFAULT ((0)) NOT NULL,
					[TS_TreatmentRatePeriodSnapshotCode] VARCHAR(3) CONSTRAINT [DF_CMRTreatmentRatePeriodMessageAdvice_TS_TreatmentRatePeriodSnapshotCode] DEFAULT ('') NOT NULL,
					[TS_TreatmentRatePeriodSnapshotRateNumber] VARCHAR(3) CONSTRAINT [DF_CMRTreatmentRatePeriodMessageAdvice_TS_TreatmentRatePeriodSnapshotRateNumber] DEFAULT ('') NOT NULL,
					[TS_TreatmentRatePeriodSnapshotPreferenceSchemeType] VARCHAR(4) CONSTRAINT [DF_CMRTreatmentRatePeriodMessageAdvice_TS_TreatmentRatePeriodSnapshotPreferenceSchemeType] DEFAULT ('') NOT NULL,
					[TS_TreatmentRatePeriodSnapshotPeriodIdentifier] SMALLINT CONSTRAINT [DF_CMRTreatmentRatePeriodMessageAdvice_TS_TreatmentRatePeriodSnapshotPeriodIdentifier] DEFAULT ((0)) NOT NULL,
					[TS_TreatmentRatePeriodSnapshotCreationTimestamp] VARCHAR(20) CONSTRAINT [DF_CMRTreatmentRatePeriodMessageAdvice_TS_TreatmentRatePeriodSnapshotCreationTimestamp] DEFAULT ('') NOT NULL,
					CONSTRAINT [TS_PK] PRIMARY KEY CLUSTERED
					(
						[TS_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX NR_UX__CMRTreatmentRatePeriodMessageAdvice On [CMRTreatmentRatePeriodMessageAdvice]
				(
					[TS_MessageAdviceIdentifier],
					[TS_TreatmentRatePeriodSnapshotCode],
					[TS_TreatmentRatePeriodSnapshotRateNumber],
					[TS_TreatmentRatePeriodSnapshotPreferenceSchemeType],
					[TS_TreatmentRatePeriodSnapshotPeriodIdentifier],
					[TS_TreatmentRatePeriodSnapshotCreationTimestamp]
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTreatmentRatePeriodMessageAdvice"));
				sql = @"CREATE VIEW CMRTreatmentRatePeriodMessageAdviceTableView_V1 AS
				SELECT TS_PK,
				TS_MessageAdviceIdentifier,
				TS_TreatmentRatePeriodSnapshotCode,
				TS_TreatmentRatePeriodSnapshotRateNumber,
				TS_TreatmentRatePeriodSnapshotPreferenceSchemeType,
				TS_TreatmentRatePeriodSnapshotPeriodIdentifier,
				TS_TreatmentRatePeriodSnapshotCreationTimestamp
				FROM CMRTreatmentRatePeriodMessageAdvice";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTreatmentRatePeriodMessageAdviceTableView_V1"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCategoryTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCategoryCharacteristicTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCategoryCountryTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCategoryTariffGroupTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentMessageAdviceTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCharacteristicTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCountryTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentRatePeriodMessageAdviceTableView_V1"));

				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrumentCategory"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrumentCategoryCharacteristic"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrumentCategoryCountry"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrumentCategoryTariffGroup"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrumentMessageAdvice"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrumentCharacteristic"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRInstrumentCountry"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTreatmentRatePeriodMessageAdvice"));

				//Verify Version 491 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(491).UpgradeScript);

				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCategoryTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCategoryCharacteristicTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCategoryCountryTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCategoryTariffGroupTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentMessageAdviceTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCharacteristicTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRInstrumentCountryTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTreatmentRatePeriodMessageAdviceTableView_V1"));

				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrumentCategory"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrumentCategoryCharacteristic"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrumentCategoryCountry"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrumentCategoryTariffGroup"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrumentMessageAdvice"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrumentCharacteristic"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRInstrumentCountry"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTreatmentRatePeriodMessageAdvice"));

				#endregion

				#region AssertVersion492Modification

				restoreSb.Clear();

				sql = @"CREATE TABLE [CMRTariffClassificationSnapshot]
				(
					[TF_PK] UNIQUEIDENTIFIER NOT NULL,
					[TF_TariffClassificationNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRTariffClassificationSnapshot_TF_TariffClassificationNumber]  DEFAULT (''),
					[TF_CreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTariffClassificationSnapshot_TF_CreationTimestamp]  DEFAULT (''),
					[TF_ConcessionalItemNumber] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRTariffClassificationSnapshot_TF_ConcessionalItemNumber]  DEFAULT (''),
					[TF_Sequence] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffClassificationSnapshot_TF_Sequence]  DEFAULT ((0)),
					CONSTRAINT [TF_PK] PRIMARY KEY CLUSTERED
					(
						[TF_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTariffClassificationSnapshot] ON [CMRTariffClassificationSnapshot]
				(
					[TF_TariffClassificationNumber] ASC,
					[TF_CreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTariffClassificationSnapshot"));
				sql = @"CREATE VIEW CMRTariffClassificationSnapshotTableView_V1 AS
				SELECT TF_PK,
				TF_TariffClassificationNumber,
				TF_CreationTimestamp,
				TF_ConcessionalItemNumber,
				TF_Sequence
				FROM CMRTariffClassificationSnapshot";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTariffClassificationSnapshotTableView_V1"));

				sql = @"CREATE TABLE [CMRTariffClassificationCharacteristic]
				(
					[TC_PK] UNIQUEIDENTIFIER NOT NULL,
					[TC_CharacteristicCode] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffClassificationCharacteristic_TC_CharacteristicCode]  DEFAULT ((0)),
					[TC_TariffClassificationSnapshotTariffClassificationNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRTariffClassificationCharacteristic_TC_TariffClassificationSnapshotTariffClassificationNumber]  DEFAULT (''),
					[TC_TariffClassificationSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTariffClassificationCharacteristic_TC_TariffClassificationSnapshotCreationTimestamp]  DEFAULT (''),
					CONSTRAINT [TC_PK] PRIMARY KEY CLUSTERED
					(
						[TC_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTariffClassificationCharacteristic] ON [CMRTariffClassificationCharacteristic]
				(
					[TC_CharacteristicCode] ASC,
					[TC_TariffClassificationSnapshotTariffClassificationNumber] ASC,
					[TC_TariffClassificationSnapshotCreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTariffClassificationCharacteristic"));
				sql = @"CREATE VIEW CMRTariffClassificationCharacteristicTableView_V1 AS
				SELECT TC_PK,
				TC_CharacteristicCode,
				TC_TariffClassificationSnapshotTariffClassificationNumber,
				TC_TariffClassificationSnapshotCreationTimestamp
				FROM CMRTariffClassificationCharacteristic";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTariffClassificationCharacteristicTableView_V1"));

				sql = @"CREATE TABLE [CMRTariffRatePeriodMessageAdvice]
				(
					[TV_PK] UNIQUEIDENTIFIER NOT NULL,
					[TV_MessageAdviceIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodMessageAdvice_TV_MessageAdviceIdentifier]  DEFAULT ((0)),
					[TV_TariffRatePeriodSnapshotTariffClassificationNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodMessageAdvice_TV_TariffRatePeriodSnapshotTariffClassificationNumber]  DEFAULT (''),
					[TV_TariffRatePeriodSnapshotRateNumber] VARCHAR(3) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodMessageAdvice_TV_TariffRatePeriodSnapshotRateNumber]  DEFAULT (''),
					[TV_TariffRatePeriodSnapshotPreferenceSchemeType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodMessageAdvice_TV_TariffRatePeriodSnapshotPreferenceSchemeType]  DEFAULT (''),
					[TV_TariffRatePeriodSnapshotPeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodMessageAdvice_TV_TariffRatePeriodSnapshotPeriodIdentifier]  DEFAULT ((0)),
					[TV_TariffRatePeriodSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTariffRatePeriodMessageAdvice_TV_TariffRatePeriodSnapshotCreationTimestamp]  DEFAULT (''),
					CONSTRAINT [TV_PK] PRIMARY KEY CLUSTERED
					(
						[TV_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTariffRatePeriodMessageAdvice] ON [CMRTariffRatePeriodMessageAdvice]
				(
					[TV_MessageAdviceIdentifier] ASC,
					[TV_TariffRatePeriodSnapshotTariffClassificationNumber] ASC,
					[TV_TariffRatePeriodSnapshotRateNumber] ASC,
					[TV_TariffRatePeriodSnapshotPreferenceSchemeType] ASC,
					[TV_TariffRatePeriodSnapshotPeriodIdentifier] ASC,
					[TV_TariffRatePeriodSnapshotCreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTariffRatePeriodMessageAdvice"));
				sql = @"CREATE VIEW CMRTariffRatePeriodMessageAdviceTableView_V1 AS
				SELECT TV_PK,
				TV_MessageAdviceIdentifier,
				TV_TariffRatePeriodSnapshotTariffClassificationNumber,
				TV_TariffRatePeriodSnapshotRateNumber,
				TV_TariffRatePeriodSnapshotPreferenceSchemeType,
				TV_TariffRatePeriodSnapshotPeriodIdentifier,
				TV_TariffRatePeriodSnapshotCreationTimestamp
				FROM CMRTariffRatePeriodMessageAdvice";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTariffRatePeriodMessageAdviceTableView_V1"));

				sql = @"CREATE TABLE [CMRTariffClassificationMessageAdvice]
				(
					[TM_PK] UNIQUEIDENTIFIER NOT NULL,
					[TM_MessageAdviceIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRTariffClassificationMessageAdvice_TM_MessageAdviceIdentifier]  DEFAULT ((0)),
					[TM_TariffClassificationSnapshotTariffClassificationNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRTariffClassificationMessageAdvice_TM_TariffClassificationSnapshotTariffClassificationNumber]  DEFAULT (''),
					[TM_TariffClassificationSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTariffClassificationMessageAdvice_TM_TariffClassificationSnapshotCreationTimestamp]  DEFAULT (''),
					CONSTRAINT [TM_PK] PRIMARY KEY CLUSTERED
					(
						[TM_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTariffClassificationMessageAdvice] ON [CMRTariffClassificationMessageAdvice]
				(
					[TM_MessageAdviceIdentifier] ASC,
					[TM_TariffClassificationSnapshotTariffClassificationNumber] ASC,
					[TM_TariffClassificationSnapshotCreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTariffClassificationMessageAdvice"));
				sql = @"CREATE VIEW CMRTariffClassificationMessageAdviceTableView_V1 AS
				SELECT TM_PK,
				TM_MessageAdviceIdentifier,
				TM_TariffClassificationSnapshotTariffClassificationNumber,
				TM_TariffClassificationSnapshotCreationTimestamp
				FROM CMRTariffClassificationMessageAdvice";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTariffClassificationMessageAdviceTableView_V1"));

				sql = @"CREATE TABLE [CMRTariffClassificationConcordance]
				(
					[TX_PK] UNIQUEIDENTIFIER NOT NULL,
					[TX_TariffClassificationNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRTariffClassificationConcordance_TX_TariffClassificationNumber]  DEFAULT (''),
					[TX_TariffClassificationSnapshotTariffClassificationNumber] VARCHAR(8) NOT NULL CONSTRAINT [DF_CMRTariffClassificationConcordance_TX_TariffClassificationSnapshotTariffClassificationNumber]  DEFAULT (''),
					[TX_TariffClassificationSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRTariffClassificationConcordance_TX_TariffClassificationSnapshotCreationTimestamp]  DEFAULT (''),
					CONSTRAINT [TX_PK] PRIMARY KEY CLUSTERED
					(
						[TX_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRTariffClassificationConcordance] ON [CMRTariffClassificationConcordance]
				(
					[TX_TariffClassificationNumber] ASC,
					[TX_TariffClassificationSnapshotTariffClassificationNumber] ASC,
					[TX_TariffClassificationSnapshotCreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRTariffClassificationConcordance"));
				sql = @"CREATE VIEW CMRTariffClassificationConcordanceTableView_V1 AS
				SELECT TX_PK,
				TX_TariffClassificationNumber,
				TX_TariffClassificationSnapshotTariffClassificationNumber,
				TX_TariffClassificationSnapshotCreationTimestamp
				FROM CMRTariffClassificationConcordance
				";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRTariffClassificationConcordanceTableView_V1"));

				sql = @"CREATE TABLE [CMRPreferenceSchemeRuleMessageAdvice]
				(
					[PM_PK] UNIQUEIDENTIFIER NOT NULL,
					[PM_MessageAdviceIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceSchemeRuleMessageAdvice_PM_MessageAdviceIdentifier]  DEFAULT ((0)),
					[PM_PreferenceSchemePeriodSnapshotSchemeType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceSchemeRuleMessageAdvice_PM_PreferenceSchemePeriodSnapshotSchemeType]  DEFAULT (''),
					[PM_PreferenceSchemePeriodSnapshotPeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceSchemeRuleMessageAdvice_PM_PreferenceSchemePeriodSnapshotPeriodIdentifier]  DEFAULT ((0)),
					[PM_PreferenceSchemePeriodSnapshotCreationTimestamp] VARCHAR(20) NOT NULL  CONSTRAINT [DF_CMRPreferenceSchemeRuleMessageAdvice_PM_PreferenceSchemePeriodSnapshotCreationTimestamp]  DEFAULT (''),
					[PM_PreferenceSchemeRuleRuleType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceSchemeRuleMessageAdvice_PM_PreferenceSchemeRuleRuleType]  DEFAULT (''),
					CONSTRAINT [PM_PK] PRIMARY KEY CLUSTERED
					(
						[PM_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRPreferenceSchemeRuleMessageAdvice] ON [CMRPreferenceSchemeRuleMessageAdvice]
				(
					[PM_MessageAdviceIdentifier] ASC,
					[PM_PreferenceSchemePeriodSnapshotSchemeType] ASC,
					[PM_PreferenceSchemePeriodSnapshotPeriodIdentifier] ASC,
					[PM_PreferenceSchemePeriodSnapshotCreationTimestamp] ASC,
					[PM_PreferenceSchemeRuleRuleType] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPreferenceSchemeRuleMessageAdvice"));
				sql = @"CREATE VIEW CMRPreferenceSchemeRuleMessageAdviceTableView_V1 AS
				SELECT PM_PK,
				PM_MessageAdviceIdentifier,
				PM_PreferenceSchemePeriodSnapshotSchemeType,
				PM_PreferenceSchemePeriodSnapshotPeriodIdentifier,
				PM_PreferenceSchemePeriodSnapshotCreationTimestamp,
				PM_PreferenceSchemeRuleRuleType
				FROM CMRPreferenceSchemeRuleMessageAdvice";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPreferenceSchemeRuleMessageAdviceTableView_V1"));

				sql = @"CREATE TABLE [CMRPreferenceRulePeriodSnapshot]
				(
					[PU_PK] UNIQUEIDENTIFIER NOT NULL,
					[PU_RuleType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_RuleType]  DEFAULT (''),
					[PU_PeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_PeriodIdentifier]  DEFAULT ((0)),
					[PU_CreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_CreationTimestamp]  DEFAULT (''),
					[PU_ConcessionalItemNumber] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_ConcessionalItemNumber]  DEFAULT (''),
					[PU_StartDate] DATETIME NULL,
					[PU_EndDate] DATETIME NULL,
					[PU_Sequence] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_Sequence]  DEFAULT ((0)),
					[PU_LowerLocalContentPercentage] DECIMAL(5, 2) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_LowerLocalContentPercentage]  DEFAULT ((0)),
					[PU_HigherLocalContentPercentage] DECIMAL(5, 2) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_HigherLocalContentPercentage]  DEFAULT ((0)),
					[PU_CountryValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_CountryValidationType]  DEFAULT (''),
					[PU_TariffValidationType] VARCHAR(10) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_TariffValidationType]  DEFAULT (''),
					[PU_Description] VARCHAR(250) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodSnapshot_PU_Description]  DEFAULT (''),
					CONSTRAINT [PU_PK] PRIMARY KEY CLUSTERED
					(
						[PU_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRPreferenceRulePeriodSnapshot] ON [CMRPreferenceRulePeriodSnapshot]
				(
					[PU_RuleType] ASC,
					[PU_PeriodIdentifier] ASC,
					[PU_CreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPreferenceRulePeriodSnapshot"));
				sql = @"CREATE VIEW CMRPreferenceRulePeriodSnapshotTableView_V1 AS
				SELECT PU_PK,
				PU_RuleType, PU_PeriodIdentifier,
				PU_CreationTimestamp,
				PU_ConcessionalItemNumber,
				PU_StartDate, PU_EndDate,
				PU_Sequence,
				PU_LowerLocalContentPercentage,
				PU_HigherLocalContentPercentage,
				PU_CountryValidationType,
				PU_TariffValidationType,
				PU_Description
				FROM CMRPreferenceRulePeriodSnapshot";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPreferenceRulePeriodSnapshotTableView_V1"));

				sql = @"CREATE TABLE [CMRPreferenceRulePeriodCountry]
				(
					[PY_PK] UNIQUEIDENTIFIER NOT NULL,
					[PY_PreferenceRulePeriodSnapshotRuleType] VARCHAR(4) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodCountry_PY_PreferenceRulePeriodSnapshotRuleType]  DEFAULT (''),
					[PY_PreferenceRulePeriodSnapshotPeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodCountry_PY_PreferenceRulePeriodSnapshotPeriodIdentifier]  DEFAULT ((0)),
					[PY_PreferenceRulePeriodSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodCountry_PY_PreferenceRulePeriodSnapshotCreationTimestamp]  DEFAULT (''),
					[PY_CountryCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRPreferenceRulePeriodCountry_PY_CountryCode]  DEFAULT (''),
					CONSTRAINT [PY_PK] PRIMARY KEY CLUSTERED
					(
						[PY_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRPreferenceRulePeriodCountry] ON [CMRPreferenceRulePeriodCountry]
				(
					[PY_PreferenceRulePeriodSnapshotRuleType] ASC,
					[PY_PreferenceRulePeriodSnapshotPeriodIdentifier] ASC,
					[PY_PreferenceRulePeriodSnapshotCreationTimestamp] ASC,
					[PY_CountryCode] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRPreferenceRulePeriodCountry"));
				sql = @"CREATE VIEW CMRPreferenceRulePeriodCountryTableView_V1 AS
				SELECT [PY_PK],
				[PY_PreferenceRulePeriodSnapshotRuleType],
				[PY_PreferenceRulePeriodSnapshotPeriodIdentifier],
				[PY_PreferenceRulePeriodSnapshotCreationTimestamp],
				[PY_CountryCode]
				FROM [CMRPreferenceRulePeriodCountry]";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRPreferenceRulePeriodCountryTableView_V1"));

				sql = @"CREATE TABLE [CMRStatisticalClassificationPeriodMessageAdvice]
				(
					[SM_PK] UNIQUEIDENTIFIER NOT NULL,
					[SM_MessageAdviceIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodMessageAdvice_SM_MessageAdviceIdentifier]  DEFAULT ((0)),
					[SM_StatisticalClassificationPeriodSnapshotTariffClassificationNumber] VARCHAR(8) NOT NULL  CONSTRAINT [DF_CMRStatisticalClassificationPeriodMessageAdvice_SM_StatisticalClassificationPeriodSnapshotTariffClassificationNumber]  DEFAULT (''),
					[SM_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode] VARCHAR(2) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodMessageAdvice_SM_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode]  DEFAULT (''),
					[SM_StatisticalClassificationPeriodSnapshotPeriodIdentifier] SMALLINT NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodMessageAdvice_SM_StatisticalClassificationPeriodSnapshotPeriodIdentifier]  DEFAULT ((0)),
					[SM_StatisticalClassificationPeriodSnapshotCreationTimestamp] VARCHAR(20) NOT NULL CONSTRAINT [DF_CMRStatisticalClassificationPeriodMessageAdvice_SM_StatisticalClassificationPeriodSnapshotCreationTimestamp] DEFAULT (''),
					CONSTRAINT [SM_PK] PRIMARY KEY CLUSTERED
					(
						[SM_PK] ASC
					)
				)
				CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CMRStatisticalClassificationPeriodMessageAdvice] ON [CMRStatisticalClassificationPeriodMessageAdvice]
				(
					[SM_MessageAdviceIdentifier] ASC,
					[SM_StatisticalClassificationPeriodSnapshotTariffClassificationNumber] ASC,
					[SM_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode] ASC,
					[SM_StatisticalClassificationPeriodSnapshotPeriodIdentifier] ASC,
					[SM_StatisticalClassificationPeriodSnapshotCreationTimestamp] ASC
				)";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "U", "CMRStatisticalClassificationPeriodMessageAdvice"));
				sql = @"CREATE VIEW CMRStatisticalClassificationPeriodMessageAdviceTableView_V1 AS
				SELECT SM_PK,
				SM_MessageAdviceIdentifier,
				SM_StatisticalClassificationPeriodSnapshotTariffClassificationNumber,
				SM_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode,
				SM_StatisticalClassificationPeriodSnapshotPeriodIdentifier,
				SM_StatisticalClassificationPeriodSnapshotCreationTimestamp
				FROM  CMRStatisticalClassificationPeriodMessageAdvice";
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sql, "V", "CMRStatisticalClassificationPeriodMessageAdviceTableView_V1"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTariffClassificationSnapshotTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTariffClassificationCharacteristicTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTariffRatePeriodMessageAdviceTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTariffClassificationMessageAdviceTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRTariffClassificationConcordanceTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceSchemeRuleMessageAdviceTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodSnapshotTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodCountryTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "CMRStatisticalClassificationPeriodMessageAdviceTableView_V1"));

				Assert.True(TestDBHelper.TableExists(conn, "CMRTariffClassificationSnapshot"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTariffClassificationCharacteristic"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTariffRatePeriodMessageAdvice"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTariffClassificationMessageAdvice"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRTariffClassificationConcordance"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPreferenceSchemeRuleMessageAdvice"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPreferenceRulePeriodSnapshot"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRPreferenceRulePeriodCountry"));
				Assert.True(TestDBHelper.TableExists(conn, "CMRStatisticalClassificationPeriodMessageAdvice"));

				//Verify Version 492 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(492).UpgradeScript);

				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTariffClassificationSnapshotTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTariffClassificationCharacteristicTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTariffRatePeriodMessageAdviceTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTariffClassificationMessageAdviceTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRTariffClassificationConcordanceTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceSchemeRuleMessageAdviceTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodSnapshotTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodCountryTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRStatisticalClassificationPeriodMessageAdviceTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPermitRequirementTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPreferenceRulePeriodCharacteristicTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "CMRPermitRequirementExclusionsTableView_V1"));

				Assert.False(TestDBHelper.TableExists(conn, "CMRTariffClassificationSnapshot"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTariffClassificationCharacteristic"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTariffRatePeriodMessageAdvice"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTariffClassificationMessageAdvice"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRTariffClassificationConcordance"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPreferenceSchemeRuleMessageAdvice"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPreferenceRulePeriodSnapshot"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRPreferenceRulePeriodCountry"));
				Assert.False(TestDBHelper.TableExists(conn, "CMRStatisticalClassificationPeriodMessageAdvice"));

				#endregion

				#region AssertVersion493Modification

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(493).UpgradeScript);

				#endregion

				#region AssertVersion494Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefAccessorialTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefAccessorial", "TABLE"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 494 Upgrade Script
				AssertExistRefAccessorial(conn);

				#endregion

				#region AssertVersion495Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddUniqueConstraintIfNotExistsScript("RefClient", "RCT_ClientID"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.CheckUniqueConstraintExists(conn, "RefClient"));

				//Verify Version 495 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(495).UpgradeScript);
				Assert.False(TestDBHelper.CheckUniqueConstraintExists(conn, "RefClient"));

				#endregion

				#region AssertVersion496Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceJTTTableView_V1", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				restoreSb.Clear();
				restoreSb.AppendLine(@"CREATE VIEW UNDGSubstanceJTTTableView_V1 AS
SELECT JTT_PK,
JTT_IsActive,
JTT_UNNO,
JTT_Variant,
LEFT(JTT_PSN, 260) AS JTT_PSN,
JTT_Class,
JTT_ClassificationCode,
JTT_PG,
JTT_Labels,
JTT_SpecialProvisions,
JTT_LQMaxAmt,
JTT_LQMaxAmtUQ,
JTT_LQ2MaxAmt,
JTT_LQ2MaxAmtUQ,
JTT_ExceptedQuantityCode,
JTT_PackIns,
JTT_PackProv,
JTT_MixedPackingProv,
JTT_BulkTankIns,
JTT_BulkTankSpecProv,
JTT_TankCode,
JTT_TankSpecProv,
JTT_TankVehicle,
JTT_TransportCategory,
JTT_PackingSpecialProv,
JTT_BulkSpecialProv,
JTT_LoadingSpecialProv,
JTT_OperationSpecialProv,
JTT_HazardIDNumber
FROM UNDGSubstanceJTT
");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				var jtt_psn = TestDBHelper.GetExtendedTableColumns(conn, "UNDGSubstanceJTTTableView_V1").First(x => x.ColumnName == "JTT_PSN");
				Assert.True(jtt_psn.IsNullable);
				//Verify Version 496 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(496).UpgradeScript);
				jtt_psn = TestDBHelper.GetExtendedTableColumns(conn, "UNDGSubstanceJTTTableView_V1").First(x => x.ColumnName == "JTT_PSN");
				Assert.False(jtt_psn.IsNullable);
				#endregion

				#region AssertVersion497Modifications

				// Restore and Pre-condition Check
				var refUNLOCORelatedPort_497 = new RefUNLOCORelatedPort();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refUNLOCORelatedPort_497.TableName, "IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refUNLOCORelatedPort_497.TableName, "IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort",
					"CREATE UNIQUE NONCLUSTERED INDEX [IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort] ON [RefUNLOCORelatedPort] ([RLR_RL_NKRelatedPort] ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.UniqueIndexExists(conn, refUNLOCORelatedPort_497.TableName, "IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort"));

				//Verify Version 497 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(497).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refUNLOCORelatedPort_497.TableName, "IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort"));
				Assert.False(TestDBHelper.UniqueIndexExists(conn, refUNLOCORelatedPort_497.TableName, "IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort"));

				#endregion

				#region AssertVersion498Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(refCusProfile.TableName, "FK_RefCusProfile_XX0_XQ2_QuestionCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_XQ2_QuestionCode_XX0_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfile.TableName, "XX0_XQ2_QuestionCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfile.TableName, "XX0_XX2_NKQuestionCode", "VARCHAR(50) NOT NULL CONSTRAINT DF_RefCusProfile_XX0_XX2_NKQuestionCode DEFAULT('')"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate ON RefCusProfile (XX0_ZZZ_NKDataGrouping ASC, XX0_XXX_ProfileType ASC, XX0_TariffCode ASC, XX0_StartDate ASC)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_XQ2_QuestionCode_XX0_StartDate"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_XQ2_QuestionCode"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusProfile.TableName, "FK_RefCusProfile_XX0_XQ2_QuestionCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_XX2_NKQuestionCode"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusProfile.TableName, "DF_RefCusProfile_XX0_XX2_NKQuestionCode"));

				//Verify Version 498 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(498).UpgradeScript);

				Assert.False(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_XX2_NKQuestionCode"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusProfile.TableName, "DF_RefCusProfile_XX0_XX2_NKQuestionCode"));

				#endregion

				#region AssertVersion499Modifications

				// Restore and Pre-condition Check
				var refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V1"));

				//Verify Version 499 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(499).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V1"));

				#endregion

				#region AssertVersion500Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(500).UpgradeScript);

				#endregion

				#region AssertVersion501Modifications

				// Restore and Pre-condition Check
				var refCusCondition_501 = new RefCusCondition();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusCondition_501.TableName, "CK_RefCusCondition_ZX1_Severity"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusConditionTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCondition_501.TableName, "ZX1_Severity"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusCondition_501.TableName, "ZX1_Severity"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTableView_V3"));

				//Verify Version 501 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(501).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refCusCondition_501.TableName, "ZX1_Severity"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusConditionTableView_V3"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusConditionTableView_V3").Contains("ZX1_Severity"));

				#endregion

				#region AssertVersion502Modifications

				// Restore and Pre-condition Check
				var refCusCondition_502 = new RefCusCondition();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusCondition_502.TableName, "CK_RefCusCondition_ZX1_Severity"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.CheckConstraintExists(conn, refCusCondition_502.TableName, "CK_RefCusCondition_ZX1_Severity"));

				//Verify Version 502 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(502).UpgradeScript);
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refCusCondition_502.TableName, "CK_RefCusCondition_ZX1_Severity"));

				#endregion

				#region AssertVsersion503Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", "RefCarrierCodeAttribute", "CK_RefCarrierCodeAttribute_ZZG_Value"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefCarrierCodeAttribute", "ZZG_Value", "VARCHAR(100) NOT NULL"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.AreEqual(100, TestDBHelper.GetColumnLength(conn, "RefCarrierCodeAttribute", "ZZG_Value"));
				Assert.AreEqual("varchar", TestDBHelper.GetExtendedTableColumns(conn, "RefCarrierCodeAttribute").First(x => x.ColumnName == "ZZG_Value").DataType);

				//Verify Version 503 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(503).UpgradeScript);
				Assert.AreEqual(100 * 2, TestDBHelper.GetColumnLength(conn, "RefCarrierCodeAttribute", "ZZG_Value"));
				Assert.AreEqual("nvarchar", TestDBHelper.GetExtendedTableColumns(conn, "RefCarrierCodeAttribute").First(x => x.ColumnName == "ZZG_Value").DataType);
				Assert.True(TestDBHelper.ObjectExists(conn, "C", "CK_RefCarrierCodeAttribute_ZZG_Value"));

				#endregion

				#region AssertVersion504Modifications

				var refAccessorial = new RefAccessorial();

				// Restore and Pre-condition Check
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refAccessorial.TableName, "DF_RefAccessorial_ASI_PK"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refAccessorial.TableName, "DF_RefAccessorial_ASI_Code"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refAccessorial.TableName, "DF_RefAccessorial_ASI_Description"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refAccessorial.TableName, "CK_RefAccessorial_ASI_Code"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refAccessorial.TableName, "CK_RefAccessorial_ASI_Description"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(refAccessorial.TableName, $"PK_{refAccessorial.TableName}"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refAccessorial.TableName, "IX_RefAccessorial_ASI_Code"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refAccessorial.TableName, "ASI_PK", "ACS_PK"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refAccessorial.TableName, "ASI_Code", "ACS_Code"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refAccessorial.TableName, "ASI_Description", "ACS_Description"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(refAccessorial.TableName, $"PK_{refAccessorial.TableName}", "ACS_PK", "NONCLUSTERED"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refAccessorial.TableName, "IX_RefAccessorial_ACS_Code", "CREATE UNIQUE CLUSTERED INDEX [IX_RefAccessorial_ACS_Code] ON [dbo].[RefAccessorial] ([ACS_Code])"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refAccessorial.TableName, "DF_RefAccessorial_ACS_PK", "NEWID()", "ACS_PK"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refAccessorial.TableName, "DF_RefAccessorial_ACS_Code", "''", "ACS_Code"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refAccessorial.TableName, "DF_RefAccessorial_ACS_Description", "''", "ACS_Description"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refAccessorial.TableName, "CK_RefAccessorial_ACS_Code", "(LEN([ACS_Code])=(3))"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refAccessorial.TableName, "CK_RefAccessorial_ACS_Description", "([ACS_Description]<>'')"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refAccessorial.TableViewScriptDictionary[1], "V", $"{refAccessorial.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ACS_PK"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ACS_Code"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ACS_Description"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refAccessorial.TableName, "CK_RefAccessorial_ACS_Code"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refAccessorial.TableName, "CK_RefAccessorial_ACS_Description"));
				Assert.True(TestDBHelper.IndexExists(conn, refAccessorial.TableName, "IX_RefAccessorial_ACS_Code"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefAccessorialTableView_V1"));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(504).UpgradeScript);

				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ACS_PK"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ACS_Code"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ACS_Description"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refAccessorial.TableName, "CK_RefAccessorial_ACS_Code"));
				Assert.False(TestDBHelper.CheckConstraintExists(conn, refAccessorial.TableName, "CK_RefAccessorial_ACS_Description"));
				Assert.False(TestDBHelper.IndexExists(conn, refAccessorial.TableName, "IX_RefAccessorial_ACS_Code"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefAccessorialTableView_V1"));

				#endregion

				#region AssertVersion505Modifications

				Assert.True(TestDBHelper.ColumnExists(conn, refAccessorial.TableName, "ACS_PK"));
				Assert.True(TestDBHelper.ColumnExists(conn, refAccessorial.TableName, "ACS_Code"));
				Assert.True(TestDBHelper.ColumnExists(conn, refAccessorial.TableName, "ACS_Description"));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(505).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refAccessorial.TableName, "ASI_PK"));
				Assert.True(TestDBHelper.ColumnExists(conn, refAccessorial.TableName, "ASI_Code"));
				Assert.True(TestDBHelper.ColumnExists(conn, refAccessorial.TableName, "ASI_Description"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefAccessorialTableView_V1"));

				#endregion

				#region AssertVersion506Modifications

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(506).UpgradeScript);

				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ASI_PK"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ASI_Code"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refAccessorial.TableName, "DF_RefAccessorial_ASI_Description"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refAccessorial.TableName, "CK_RefAccessorial_ASI_Code"));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refAccessorial.TableName, "CK_RefAccessorial_ASI_Description"));
				Assert.True(TestDBHelper.IndexExists(conn, refAccessorial.TableName, "IX_RefAccessorial_ASI_Code"));

				#endregion

				#region AssertVersion507Modifications

				// Restore and Pre-condition Check
				var refCusCodeListAttributeName_507 = new RefCusCodeListAttributeName();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusCodeListAttributeName_507.TableName, "DF_RefCusCodeListAttributeName_ZXE_IsDateRangeUsed"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeListAttributeName_507.TableName, "ZXE_IsDateRangeUsed"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeListAttributeNameTableView_V2", "VIEW"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusCodeListAttributeName_507.TableName, "ZXE_IsDateRangeUsed"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusCodeListAttributeName_507.TableName, "DF_RefCusCodeListAttributeName_ZXE_IsDateRangeUsed"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusCodeListAttributeNameTableView_V2"));

				//Verify Version 507 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(507).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refCusCodeListAttributeName_507.TableName, "ZXE_IsDateRangeUsed"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusCodeListAttributeName_507.TableName, "DF_RefCusCodeListAttributeName_ZXE_IsDateRangeUsed"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusCodeListAttributeNameTableView_V2"));

				#endregion

				#region AssertVersion508Modifications

				// Restore and Pre-condition Check
				var refCusCodeListAttribute_508 = new RefCusCodeListAttribute();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusCodeListAttribute_508.TableName, "CK_RefCusCodeListAttribute_ZZE_StartDate_ZZE_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttribute_508.TableName, "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_StartDate_ZZE_Value"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttribute_508.TableName, "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_Value",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_Value ON RefCusCodeListAttribute(ZZE_ZZD_CodeList ASC, ZZE_ZXE_NKName ASC, ZZE_Value ASC)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeListAttribute_508.TableName, "ZZE_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeListAttribute_508.TableName, "ZZE_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeListAttributeTableView_V2", "VIEW"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.UniqueIndexExists(conn, refCusCodeListAttribute_508.TableName, "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_Value"));
				Assert.False(TestDBHelper.UniqueIndexExists(conn, refCusCodeListAttribute_508.TableName, "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_StartDate_ZZE_Value"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusCodeListAttribute_508.TableName, "ZZE_StartDate"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusCodeListAttribute_508.TableName, "ZZE_EndDate"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusCodeListAttributeTableView_V2"));

				//Verify Version 508 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(508).UpgradeScript);

				Assert.False(TestDBHelper.UniqueIndexExists(conn, refCusCodeListAttribute_508.TableName, "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_Value"));
				Assert.True(TestDBHelper.UniqueIndexExists(conn, refCusCodeListAttribute_508.TableName, "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_StartDate_ZZE_Value"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusCodeListAttribute_508.TableName, "ZZE_StartDate"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusCodeListAttribute_508.TableName, "ZZE_EndDate"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusCodeListAttributeTableView_V2"));

				#endregion

				#region AssertVersion509Modifications

				// Restore and Pre-condition Check
				var refCusCodeListAttribute_509 = new RefCusCodeListAttribute();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusCodeListAttribute_509.TableName, "CK_RefCusCodeListAttribute_ZZE_StartDate_ZZE_EndDate"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.CheckConstraintExists(conn, refCusCodeListAttribute_509.TableName, "CK_RefCusCodeListAttribute_ZZE_StartDate_ZZE_EndDate"));

				//Verify Version 509 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(509).UpgradeScript);

				Assert.True(TestDBHelper.CheckConstraintExists(conn, refCusCodeListAttribute_509.TableName, "CK_RefCusCodeListAttribute_ZZE_StartDate_ZZE_EndDate"));

				#endregion

				#region AssertVersion510Modifications

				// Restore and Pre-condition Check
				var refCusCodeListAttributeName_510 = new RefCusCodeListAttributeName();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropTriggerIfExistsScript(refCusCodeListAttributeName_510.TableName, "TG_RefCusCodeListAttributeName_INS_UPD_Dates"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttributeName_INS_UPD_Dates"));

				//Verify Version 510 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(510).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttributeName_INS_UPD_Dates"));

				#endregion

				#region AssertVersion511Modifications

				// Restore and Pre-condition Check
				var refCusCodeListAttribute_511 = new RefCusCodeListAttribute();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropTriggerIfExistsScript(refCusCodeListAttribute_511.TableName, "TG_RefCusCodeListAttribute_INS_UPD_Dates"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttribute_INS_UPD_Dates"));

				//Verify Version 511 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(511).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttribute_INS_UPD_Dates"));

				#endregion

				#region AssertVersion512Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_QuestionCode_XX0_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProfile.TableName, "DF_RefCusProfile_XX0_QuestionCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfile.TableName, "XX0_QuestionCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfile.TableName, "XX0_XQ2_QuestionCode", "UNIQUEIDENTIFIER NULL"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusProfile.TableName, "FK_RefCusProfile_XX0_XQ2_QuestionCode", "XX0_XQ2_QuestionCode", "RefCusProfileQuestion (XQ2_PK)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_XQ2_QuestionCode_XX0_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_XQ2_QuestionCode_XX0_StartDate ON RefCusProfile (XX0_ZZZ_NKDataGrouping ASC, XX0_XXX_ProfileType ASC, XX0_TariffCode ASC, XX0_XQ2_QuestionCode ASC, XX0_StartDate ASC)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_QuestionCode"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, refCusProfile.TableName, "DF_RefCusProfile_XX0_QuestionCode"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_QuestionCode_XX0_StartDate"));

				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_XQ2_QuestionCode"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusProfile.TableName, "FK_RefCusProfile_XX0_XQ2_QuestionCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_XQ2_QuestionCode_XX0_StartDate"));

				//Verify Version 512 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(512).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_QuestionCode"));
				Assert.True(TestDBHelper.CheckDefaultConstraintExists(conn, refCusProfile.TableName, "DF_RefCusProfile_XX0_QuestionCode"));

				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_XQ2_QuestionCode"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusProfile.TableName, "FK_RefCusProfile_XX0_XQ2_QuestionCode"));

				AssertExistRefCusProfile_512(conn);
				AssertExistRefCusProfileQuestion(conn);
				AssertExistRefCusProfileQuestionAnswerList(conn);
				AssertExistRefCusProfileQuestionAttribute(conn);

				#endregion

				#region AssertVersion513Modifications

				//check constraint
				refExchangeRateZZ = new RefExchangeRateZZ();
				restoreSb.Clear();
				restoreOldConstarintSql = @"ALTER TABLE [dbo].[RefExchangeRateZZ] DROP CONSTRAINT [CK_RefExchangeRateZZ_ZZN_ExRateType];

ALTER TABLE [dbo].[RefExchangeRateZZ]  WITH CHECK ADD  CONSTRAINT [CK_RefExchangeRateZZ_ZZN_ExRateType] CHECK (([ZZN_ExRateType]='CUE' OR [ZZN_ExRateType]='CUS' OR [ZZN_ExRateType]='CUD' OR [ZZN_ExRateType]='IAT' OR [ZZN_ExRateType]='BNB' OR [ZZN_ExRateType]='BNS'));";
				TestDBHelper.ExecuteNonQuery(conn, restoreOldConstarintSql);
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'' OR [ZZN_ExRateType]=''IAT'' OR [ZZN_ExRateType]=''BNB'' OR [ZZN_ExRateType]=''BNS'')"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefExchangeRateZZTableView_V4", "VIEW"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefExchangeRateZZTableView_V4"));

				//Verify Version 513 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(513).UpgradeScript);

				//check constraint verify
				Assert.False(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'' OR [ZZN_ExRateType]=''IAT'' OR [ZZN_ExRateType]=''BNB'' OR [ZZN_ExRateType]=''BNS'')"));
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refExchangeRateZZ.TableName, "CK_RefExchangeRateZZ_ZZN_ExRateType", "([ZZN_ExRateType]=''CUE'' OR [ZZN_ExRateType]=''CUS'' OR [ZZN_ExRateType]=''CUD'' OR [ZZN_ExRateType]=''IAT'' OR [ZZN_ExRateType]=''BNB'' OR [ZZN_ExRateType]=''BNS'' OR [ZZN_ExRateType]=''BUY'' OR [ZZN_ExRateType]=''SEL'')"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefExchangeRateZZTableView_V4"));

				#endregion

				#region AssertVersion514Modifications

				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(514).UpgradeScript);

				#endregion

				#region AssertVersion515Modifications

				restoreSb.Clear();
				var profileQuestionIndexName = "IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate";
				var profileQuestionPathwayIndexName = "IX_RefCusProfileQuestionPathway_XQP_XQ2_QuestionParent_XQP_XQ2_QuestionChild_XQP_StartDate";
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusProfileQuestion), profileQuestionIndexName));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusProfileQuestionPathway), profileQuestionPathwayIndexName));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusProfileQuestionPathway), profileQuestionPathwayIndexName, "CREATE NONCLUSTERED INDEX IX_RefCusProfileQuestionPathway_XQP_XQ2_QuestionParent_XQP_XQ2_QuestionChild_XQP_StartDate ON RefCusProfileQuestionPathway (XQP_XQ2_QuestionParent ASC, XQP_XQ2_QuestionChild ASC, XQP_StartDate ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.UniqueIndexExists(conn, nameof(RefCusProfileQuestion), profileQuestionIndexName));
				Assert.False(TestDBHelper.UniqueIndexExists(conn, nameof(RefCusProfileQuestionPathway), profileQuestionPathwayIndexName));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(515).UpgradeScript);
				Assert.True(TestDBHelper.UniqueIndexExists(conn, nameof(RefCusProfileQuestion), profileQuestionIndexName));
				Assert.True(TestDBHelper.UniqueIndexExists(conn, nameof(RefCusProfileQuestionPathway), profileQuestionPathwayIndexName));

				#endregion

				#region AssertVersion516Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefAccElectronicProcessingFeeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefAccElectronicProcessingFee", "TABLE"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.TableExists(conn, "RefAccElectronicProcessingFee"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefAccElectronicProcessingFeeTableView_V1"));

				//Verify Version 516 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(516).UpgradeScript);
				AssertExistRefAccElectronicProcessingFeeVersion516(conn);

				#endregion

				#region AssertVersion517Modifications

				// Restore and Pre-condition Check
				var refCusTariffUOM_V3 = new RefCusTariffUOM().TableName;
				restoreSb.Clear();
				//1.Alter field ZZ8_ZZA_SecondTradeGroup type
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffUOM_V3, "ZZ8_ZZA_SecondTradeGroup", "VARCHAR(35) SPARSE NULL"));
				//2. Add foreign key
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(refCusTariffUOM_V3, "FK_RefCusTariffUOM_RefCusTradeGroup_SecondTradeGroup"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//1.Alter field ZZ8_ZZA_SecondTradeGroup type
				Assert.True(TestDBHelper.ColumnTypeMatchingLowercase(conn, refCusTariffUOM_V3, "ZZ8_ZZA_SecondTradeGroup", "varchar"));
				//2. Add foreign key
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusTariffUOM_V3, "FK_RefCusTariffUOM_RefCusTradeGroup_SecondTradeGroup"));

				//Verify Version 517 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(517).UpgradeScript);


				//1.Alter field ZZ8_ZZA_SecondTradeGroup type
				Assert.True(TestDBHelper.ColumnTypeMatchingLowercase(conn, refCusTariffUOM_V3, "ZZ8_ZZA_SecondTradeGroup", "uniqueidentifier"));
				//2. Add foreign key
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffUOM_V3, "FK_RefCusTariffUOM_RefCusTradeGroup_SecondTradeGroup"));

				#endregion

				#region AssertVersion518Modifications

				// Restore and Pre-condition Check
				var refCusTariffUOM_V4 = new RefCusTariffUOM().TableName;
				restoreSb.Clear();
				//1.Alter CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffUOM_V4, "CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate", "([ZZ8_StartDate]<=[ZZ8_EndDate])"));
				//2.Alter Index *Tariff
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffUOM_V4, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode ON RefCusTariffUOM(ZZ8_ZZ1_Tariff ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZW_TariffNationalCode ASC)"));
				//3.Alter Index *NationalCode
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffUOM_V4, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff ON RefCusTariffUOM(ZZ8_ZZW_TariffNationalCode ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZ1_Tariff ASC)"));
				//4.Drop Index IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffUOM_V4, "IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup", "CREATE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup ON RefCusTariffUOM(ZZ8_ZZA_TradeGroup ASC)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//1.Alter CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate
				Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusTariffUOM_V4, "CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate", "([ZZ8_StartDate]<=[ZZ8_EndDate])"));
				//2.Alter Index *Tariff
				Assert.True(TestDBHelper.UniqueIndexExists(conn, refCusTariffUOM_V4, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				//3.Alter Index *NationalCode
				Assert.True(TestDBHelper.UniqueIndexExists(conn, refCusTariffUOM_V4, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));

				//Verify Version 518 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(518).UpgradeScript);

				//1.Alter CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate
				TestDBHelper.AssertCheckConstraintsAndDefinitions(conn, refCusTariffUOM_V4, new[] { ("CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate", "([ZZ8_StartDate] IS NULL AND [ZZ8_EndDate] IS NULL OR [ZZ8_StartDate] IS NOT NULL AND [ZZ8_EndDate] IS NOT NULL AND [ZZ8_StartDate]<=[ZZ8_EndDate])") });
				//2.Alter Index *Tariff
				Assert.False(TestDBHelper.UniqueIndexExists(conn, refCusTariffUOM_V4, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
				Assert.True(TestDBHelper.UniqueIndexExists(conn, refCusTariffUOM_V4, "IX_RefCusTariffUOM_Tariff_Type_UOM_NKDataGrouping_TradeGroup_SecondTradeGroup_StartDate_TariffNationalCode"));
				//3.Alter Index *NationalCode
				Assert.False(TestDBHelper.UniqueIndexExists(conn, refCusTariffUOM_V4, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffUOM_V4, "IX_RefCusTariffUOM_TariffNationalCode"));
				//4.Drop Index IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup
				Assert.False(TestDBHelper.UniqueIndexExists(conn, refCusTariffUOM_V4, "IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup"));

				#endregion

				#region AssertVersion519Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(@"
INSERT INTO RefDbVersionControl (RVC_PK, RVC_DataSet, RVC_UpdaterVersion)
VALUES (newid(), 'RefCusCodeList', 1)
");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 519 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(519).UpgradeScript);
				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusCodeList' AND RVC_UpdaterVersion=1;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusCodeList' AND RVC_UpdaterVersion=2;"));
				#endregion

				#region AssertVersion520Modifications

				// Restore and Pre-condition Check
				var refAccElectronicProcessingFee_520 = new RefAccElectronicProcessingFee();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refAccElectronicProcessingFee_520.TableName, "IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_CountryCode_JobDirection_Currency_ValidFrom"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refAccElectronicProcessingFee_520.TableName, "IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_Currency_ValidFrom", "CREATE UNIQUE CLUSTERED INDEX IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_Currency_ValidFrom ON RefAccElectronicProcessingFee(EPF_SystemCode, EPF_Category, EPF_Code, EPF_Currency, EPF_ValidFrom)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefAccElectronicProcessingFeeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refAccElectronicProcessingFee_520.TableName, "EPF_CountryCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refAccElectronicProcessingFee_520.TableName, "EPF_JobDirection"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refAccElectronicProcessingFee_520.TableName, "EPF_CountryCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refAccElectronicProcessingFee_520.TableName, "EPF_JobDirection"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refAccElectronicProcessingFee_520.TableName, "EPF_CountryCode"));
				Assert.False(TestDBHelper.ColumnExists(conn, refAccElectronicProcessingFee_520.TableName, "EPF_JobDirection"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefAccElectronicProcessingFeeTableView_V2"));
				Assert.True(TestDBHelper.IndexExists(conn, refAccElectronicProcessingFee_520.TableName, "IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_Currency_ValidFrom"));
				Assert.False(TestDBHelper.IndexExists(conn, refAccElectronicProcessingFee_520.TableName, "IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_CountryCode_JobDirection_Currency_ValidFrom"));

				//Verify Version 520 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(520).UpgradeScript);
				AssertExistRefAccElectronicProcessingFeeVersion520(conn);

				#endregion

				#region AssertVersion521Modifications
				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(521).UpgradeScript);
				#endregion

				#region AssertVersion522Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(@"
INSERT INTO RefDbVersionControl (RVC_PK, RVC_DataSet, RVC_ClientID, RVC_UpdaterVersion)
VALUES (newid(), 'RefCusTariff', 'V522', 10),
(newid(), 'GBCustomsTariffs', 'V522', 10),
(newid(), 'RefCusPreference', 'V522', 4),
(newid(), 'RefCusNomenclatureGroup','V522', 5)
");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 522 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(522).UpgradeScript);
				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTariff' AND RVC_ClientID = 'V522' AND RVC_UpdaterVersion=10;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTariff' AND RVC_ClientID = 'V522' AND RVC_UpdaterVersion=11;"));

				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_ClientID = 'V522' AND RVC_UpdaterVersion=10;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_ClientID = 'V522' AND RVC_UpdaterVersion=11;"));

				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusPreference' AND RVC_ClientID = 'V522' AND RVC_UpdaterVersion=4;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusPreference' AND RVC_ClientID = 'V522' AND RVC_UpdaterVersion=5;"));

				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusNomenclatureGroup' AND RVC_ClientID = 'V522' AND RVC_UpdaterVersion=5;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusNomenclatureGroup' AND RVC_ClientID = 'V522' AND RVC_UpdaterVersion=6;"));
				#endregion

				#region AssertVersion523Modifications
				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(523).UpgradeScript);
				#endregion

				#region AssertVersion524Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGVersionTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "UNDGVersion", "TABLE"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.TableExists(conn, "UNDGVersion"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "UNDGVersionTableView_V1"));

				//Verify Version 524 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(524).UpgradeScript);

				var expectedColumns = new[]
				{
					new DbColumn("DV_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("DV_Name", "varchar", 20, false,"('')"),
					new DbColumn("DV_Standard", "varchar", 3, false,"('')"),
					new DbColumn("DV_IsActive", "bit", -1, false, "((1))"),
				};

				Assert.True(TestDBHelper.TableExists(conn, "UNDGVersion"), "UNDGVersion existed");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "UNDGVersion", false, expectedColumns), "UNDGVersion columns existed with correct type and length and default value");

				Assert.True(TestDBHelper.ObjectExists(conn, "V", "UNDGVersionTableView_V1"), "UNDGVersionTableView_V1 existed");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "UNDGVersionTableView_V1", true, expectedColumns), "UNDGVersionTableView_V1 columns existed with correct type and length and default value");

				var expectedIndexes = new[]
				{
					"IX_UNDGVersion_DV_Name"
				};
				Assert.True(TestDBHelper.IndexesExist(conn, "UNDGVersion", expectedIndexes), $"Should create all indexes");

				#endregion

				#region AssertVersion525Modifications

				restoreSb.Clear();
				var indexName1 = "IX_RefCusTariffBRCharacteristicAttribute_ZB3_ZB1_Characteristic";
				var indexName2 = "IX_RefCusTariffBRCharacteristicValue_ZB2_ZB1_Characteristic";
				var indexName3 = "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff";
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffBRCharacteristicAttribute), indexName1));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffBRCharacteristicValue), indexName2));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusVATApplicability), indexName3));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffBRCharacteristicAttribute), indexName1));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusTariffBRCharacteristicValue), indexName2));
				Assert.False(TestDBHelper.IndexExists(conn, nameof(RefCusVATApplicability), indexName3));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(525).UpgradeScript);
				Assert.That(TestDBHelper.IndexExists(conn, nameof(RefCusTariffBRCharacteristicAttribute), indexName1));
				Assert.That(TestDBHelper.IndexExists(conn, nameof(RefCusTariffBRCharacteristicValue), indexName2));
				Assert.That(TestDBHelper.IndexExists(conn, nameof(RefCusVATApplicability), indexName3));
				#endregion

				#region AssertVersion527Modifications
				var refCusCodeListAttribute_527 = new RefCusCodeListAttribute();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropTriggerIfExistsScript(refCusCodeListAttribute_527.TableName, "TG_RefCusCodeListAttribute_INS_UPD"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttribute_INS_UPD"));

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(527).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttribute_INS_UPD"));

				#endregion

				#region AssertVersion528Modifications

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(528).UpgradeScript);

				Assert.False(TestDBHelper.ObjectExists(conn, "P", "GetApplicableRatesWithoutDataGrouping_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "P", "GetApplicableRates_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "P", "GetApplicableConditions_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "P", "GetApplicableConditionsBySingleAdditionalCode_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "IF", "GetRatesBySingleCriteriaSet_V1"));

				#endregion

				#region AssertVersion529Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(@"
INSERT INTO RefDbVersionControl (RVC_PK, RVC_DataSet, RVC_ClientID, RVC_UpdaterVersion)
VALUES (newid(), 'RefCusTariff', 'V529', 11),
(newid(), 'GBCustomsTariffs', 'V529', 11),
(newid(), 'RefCusTradeGroup', 'V529', 3),
(newid(), 'RefDataGrouping', 'V529', 10);
");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 529 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(529).UpgradeScript);
				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTariff' AND RVC_ClientID = 'V529' AND RVC_UpdaterVersion=11;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTariff' AND RVC_ClientID = 'V529' AND RVC_UpdaterVersion=12;"));

				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_ClientID = 'V529' AND RVC_UpdaterVersion=11;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_ClientID = 'V529' AND RVC_UpdaterVersion=12;"));

				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTradeGroup' AND RVC_ClientID = 'V529' AND RVC_UpdaterVersion=3;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTradeGroup' AND RVC_ClientID = 'V529' AND RVC_UpdaterVersion=4;"));

				Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefDataGrouping' AND RVC_ClientID = 'V529' AND RVC_UpdaterVersion=10;"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefDataGrouping' AND RVC_ClientID = 'V529' AND RVC_UpdaterVersion=11;"));
				#endregion

				#region AssertVersion530Modifications

				// Restore and Pre-condition Check
				refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode", "UNIQUEIDENTIFIER"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode", "ZY2_ZY2_TariffAdditionalCode", "RefCusTariffAdditionalCode (ZY2_PK)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode", "CREATE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping, ZY2_ZY2_TariffAdditionalCode)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping, ZY2_ZY3_NKCategory, ZY2_AdditionalCode, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode_ZY2_ZY3_NKParentCategory"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffAdditionalCode.TableName, "DF_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffAdditionalCode.TableName, "DF_RefCusTariffAdditionalCode_ZY2_ZY3_NKParentCategory"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ParentAdditionalCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY3_NKParentCategory"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffAdditionalCodeTableView_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode_ZY2_ZY3_NKParentCategory"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
				Assert.False(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode", "D" } }));
				Assert.False(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusTariffAdditionalCode_ZY2_ZY3_NKParentCategory", "D" } }));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ParentAdditionalCode"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ZY3_NKParentCategory"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V1"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V2"));

				//Verify Version 530 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(530).UpgradeScript);

				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode_ZY2_ZY3_NKParentCategory"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
				Assert.True(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode", "D" } }));
				Assert.True(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusTariffAdditionalCode_ZY2_ZY3_NKParentCategory", "D" } }));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ParentAdditionalCode"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ZY3_NKParentCategory"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V2"));
				#endregion

				#region AssertVersion531Modifications
				// Restore and Pre-condition Check
				refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropTriggerIfExistsScript(refCusTariffAdditionalCode.TableName, "TG_RefCusTariffAdditionalCode_INS_UPD_Parents"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusTariffAdditionalCode_INS_UPD_Parents"));

				//Verify Version 531 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(531).UpgradeScript);

				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusTariffAdditionalCode_INS_UPD_Parents"));
				#endregion

				#region AssertVersion532Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefVesselArrivalTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefVesselArrival", "TABLE"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.TableExists(conn, "RefVesselArrival"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefVesselArrivalTableView_V1"));

				//Verify Version 532 Upgrade Sript
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(532).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, "RefVesselArrival"), "RefVesselArrival existed");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefVesselArrivalTableView_V1"), "RefVesselArrivalTableView_V1 existed");
				expectedIndexes = new[]
				{
					"IX_RefVesselArrival_ZYA_ZZO_Vessel_ZYA_VoyageNumber_ZYA_ArrivalDate"
				};
				Assert.True(TestDBHelper.IndexesExist(conn, "RefVesselArrival", expectedIndexes), $"Should create all indexes");

				#endregion

				#region AssertVersion533Modifications
				var refExchangeRateZZ_533 = new RefExchangeRateZZ();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refExchangeRateZZ_533.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));
				restoreSb.AppendLine("CREATE NONCLUSTERED INDEX IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate ON RefExchangeRateZZ(ZZN_RN_NKCountry ASC, ZZN_ExRateType ASC, ZZN_RX_NKExCurrency ASC, ZZN_StartDate DESC, ZZN_EndDate ASC) INCLUDE (ZZN_Rate)");

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, refExchangeRateZZ_533.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));
				Assert.False(TestDBHelper.IndexIncludesColumn(conn, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate", "ZZN_AsPublished"));

				//Verify Version 533 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(533).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refExchangeRateZZ_533.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));
				Assert.True(TestDBHelper.IndexIncludesColumn(conn, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate", "ZZN_AsPublished"));
				#endregion

				#region AssertVersion534Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefGlbReleaseNoteTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "RefGlbReleaseNote", "TABLE"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.TableExists(conn, "RefGlbReleaseNote"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefGlbReleaseNoteTableView_V1"));

				//Verify Version 534 Upgrade Sript
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(534).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, "RefGlbReleaseNote"), "RefGlbReleaseNote existed");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefGlbReleaseNoteTableView_V1"), "RefGlbReleaseNoteTableView_V1 existed");
				expectedIndexes = new[]
				{
					"IX_RefGlbReleaseNote_ZGF_QuickStartPK",
					"IX_RefGlbReleaseNote_ZGF_Section_ZGF_RN_NKCountryForReleaseNote"
				};
				Assert.True(TestDBHelper.IndexesExist(conn, "RefGlbReleaseNote", expectedIndexes), $"Should create all indexes");

				#endregion

				#region AssertVersion535Modifications

				var refCusProfileQuestionPathway = new RefCusProfileQuestionPathway();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileQuestionPathwayTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProfile.TableName, "DF_RefCusProfile_XX0_AllowMultipleAnswers"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProfile.TableName, "DF_RefCusProfile_XX0_IsAnswerMandatory"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfile.TableName, "XX0_AllowMultipleAnswers"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfile.TableName, "XX0_IsAnswerMandatory"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProfileQuestionPathway.TableName, "DF_RefCusProfileQuestionPathway_XQP_AllowMultipleAnswers"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProfileQuestionPathway.TableName, "DF_RefCusProfileQuestionPathway_XQP_IsAnswerMandatory"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfileQuestionPathway.TableName, "XQP_AllowMultipleAnswers"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfileQuestionPathway.TableName, "XQP_IsAnswerMandatory"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusProfile.TableName, "XX0_AppliesToCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfile.TableName, "XX0_AppliesToCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfile.TableName, "XX0_TariffCode", "VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusProfile_XX0_TariffCode DEFAULT('')"));


				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_AllowMultipleAnswers"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_IsAnswerMandatory"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfileQuestionPathway.TableName, "XQP_AllowMultipleAnswers"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfileQuestionPathway.TableName, "XQP_IsAnswerMandatory"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTableView_V2"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionPathwayTableView_V2"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_AppliesToCode"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_TariffCode"));

				//Verify Version 535 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(535).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_AllowMultipleAnswers"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_IsAnswerMandatory"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfileQuestionPathway.TableName, "XQP_AllowMultipleAnswers"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfileQuestionPathway.TableName, "XQP_IsAnswerMandatory"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTableView_V2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileQuestionPathwayTableView_V2"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_AppliesToCode"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_TariffCode"));

				#endregion

				#region AssertVersion536Modifications
				{
					var refCusApplicability_V3 = new RefCusApplicability().TableName;
					const string columnName = "ZZT_ZZH_TariffRelationship";
					const string fkContraintName = "FK_RefCusApplicability_RefCusTariffRelationship";
					const string newContraintName = "CK_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_ZX1_Conditions_ZZT_ZY2_AdditionalCode_ZZT_ZZH_TariffRelationship";
					const string indexName = "IX_RefCusApplicability_ZZT_ZZH_TariffRelationship_ZZT_StartDate_ZZT_ZZA_TradeGroup";

					// Restore and Pre-condition Check
					restoreSb.Clear();
					restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability_V3, indexName));
					restoreSb.AppendLine(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(refCusApplicability_V3, fkContraintName));
					restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusApplicability_V3, newContraintName));
					restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusApplicability_V3, columnName));
					TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

					// Verify Version 536 Upgrade Script
					TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(536).UpgradeScript);

					Assert.True(TestDBHelper.ColumnExists(conn, refCusApplicability_V3, columnName));
					Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusApplicability_V3, fkContraintName));
				}
				#endregion

				#region AssertVersion537Modifications
				{
					var refCusApplicability_V3 = new RefCusApplicability().TableName;
					const string oldContraintName = "CK_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_ZX1_Conditions_ZZT_ZY2_AdditionalCode";
					const string newContraintName = "CK_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_ZX1_Conditions_ZZT_ZY2_AdditionalCode_ZZT_ZZH_TariffRelationship";
					const string indexName = "IX_RefCusApplicability_ZZT_ZZH_TariffRelationship_ZZT_StartDate_ZZT_ZZA_TradeGroup";

					// Restore and Pre-condition Check
					restoreSb.Clear();
					restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability_V3, indexName));
					restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusApplicability_V3, oldContraintName, "1=1"));
					TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

					// Verify Version 537 Upgrade Script
					TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(537).UpgradeScript);

					Assert.True(TestDBHelper.CheckConstraintAndDefinitionExists(conn, refCusApplicability_V3, newContraintName, "([ZZT_ZX1_Conditions] IS NOT NULL AND [ZZT_ZZ2_Rate] IS NULL AND [ZZT_ZY2_AdditionalCode] IS NULL AND [ZZT_ZZH_TariffRelationship] IS NULL OR [ZZT_ZZ2_Rate] IS NOT NULL AND [ZZT_ZX1_Conditions] IS NULL AND [ZZT_ZY2_AdditionalCode] IS NULL AND [ZZT_ZZH_TariffRelationship] IS NULL OR [ZZT_ZY2_AdditionalCode] IS NOT NULL AND [ZZT_ZX1_Conditions] IS NULL AND [ZZT_ZZ2_Rate] IS NULL AND [ZZT_ZZH_TariffRelationship] IS NULL OR [ZZT_ZZH_TariffRelationship] IS NOT NULL AND [ZZT_ZX1_Conditions] IS NULL AND [ZZT_ZZ2_Rate] IS NULL AND [ZZT_ZY2_AdditionalCode] IS NULL)"));
					Assert.False(TestDBHelper.CheckConstraintExists(conn, refCusApplicability_V3, oldContraintName));
					Assert.True(TestDBHelper.UniqueIndexExists(conn, refCusApplicability_V3, indexName));
				}
				#endregion

				#region AssertVersion538Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(nameof(RefCusTariffType), "CK_RefCusTariffType_ZZI_Description"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(nameof(RefCusTariffType), "ZZI_Description", "NVARCHAR(50) NOT NULL"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.That(TestDBHelper.GetColumnLength(conn, nameof(RefCusTariffType), "ZZI_Description"), Is.EqualTo(100));

				//Verify Version 538 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(538).UpgradeScript);
				Assert.That(TestDBHelper.GetColumnLength(conn, nameof(RefCusTariffType), "ZZI_Description"), Is.EqualTo(200));
				Assert.True(TestDBHelper.CheckConstraintExists(conn, nameof(RefCusTariffType), "CK_RefCusTariffType_ZZI_Description"));
				#endregion

				#region AssertVersion539Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(@"
INSERT INTO RefDbVersionControl (RVC_PK, RVC_DataSet, RVC_ClientID, RVC_UpdaterVersion)
VALUES (newid(), 'RefCusTariff', 'V539', 12),
(newid(), 'GBCustomsTariffs', 'V539', 12);
");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 539 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(539).UpgradeScript);
				Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTariff' AND RVC_ClientID = 'V539' AND RVC_UpdaterVersion=12;"), Is.EqualTo(0));
				Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTariff' AND RVC_ClientID = 'V539' AND RVC_UpdaterVersion=13;"), Is.EqualTo(1));

				Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_ClientID = 'V539' AND RVC_UpdaterVersion=12;"), Is.EqualTo(0));
				Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_ClientID = 'V539' AND RVC_UpdaterVersion=13;"), Is.EqualTo(1));
				#endregion

				#region AssertVersion540Modifications
				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(540).UpgradeScript);
				#endregion

				#region AssertVersion541Modifications
				AssertExistRefMessagingBussAttributeInfo(conn);
				#endregion

				#region AssertVersion542Modifications
				AssertExistRefMessagingBussCarrierInfoAttribute(conn);
				#endregion

				#region AssertVersion543Modifications
				AssertExistRefMessagingBussPackageInfoAttribute(conn);
				#endregion

				#region AssertVersion544Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefGlbReleaseNoteTableView_V1", "VIEW"));

				var viewSQL = @"CREATE VIEW RefGlbReleaseNoteTableView_V1 AS
SELECT ZGF_PK
ZGF_IsValid,
ZGF_Category,
ZGF_RN_NKCountryForReleaseNote,
ZGF_Summary,
ZGF_URL,
ZGF_ReleaseNoteDate,
ZGF_Section,
ZGF_MinVersion,
ZGF_QuickStartPK
FROM RefGlbReleaseNote";
				var createViewSQL = $@"
IF NOT EXISTS(
SELECT * FROM sys.objects o
WHERE o.type = 'V' AND o.name = 'RefGlbReleaseNoteTableView_V1'
)
EXEC dbo.sp_executesql @statement = N'{viewSQL}'";
				restoreSb.AppendLine(createViewSQL);
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefGlbReleaseNoteTableView_V1").Contains("SELECT ZGF_PK,"));

				//Verify Version 544 Upgrade Sript
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(544).UpgradeScript);
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefGlbReleaseNoteTableView_V1").Contains("SELECT ZGF_PK,"));

				#endregion

				#region AssertVersion545Modifications
				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(545).UpgradeScript);
				#endregion

				#region AssertVersion546Modifications

				refCusProfileType = new RefCusProfileType();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTypeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfileType.TableName, "IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ProfileType_XXX_ZZI_TariffType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusProfileType.TableName, "XXX_ZZI_TariffType", "UNIQUEIDENTIFIER NULL"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				restoreSb.Clear();
				restoreSb.AppendLine(@"CREATE VIEW RefCusProfileTypeTableView_V1
WITH SCHEMABINDING AS
SELECT XXX_PK,
XXX_ProfileType,
XXX_ZZI_TariffType = ISNULL(PT.XXX_ZZI_TariffType, '00000000-0000-0000-0000-000000000000'),
XXX_Description,
XXX_ZZZ_NKDataGrouping
FROM dbo.RefCusProfileType AS PT");

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(546).UpgradeScript);

				Assert.False(TestDBHelper.IndexExists(conn, refCusProfileType.TableName, "IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ZZI_TariffType_XXX_ProfileType"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusProfileType.TableName, "IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ProfileType_XXX_ZZI_TariffType"));

				var expectedTableColumns = new[]
				{
					new DbColumn("XXX_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("XXX_ProfileType", "varchar", 10, false, ""),
					new DbColumn("XXX_ZZI_TariffType", "uniqueidentifier", -1, false, ""),
					new DbColumn("XXX_Description", "nvarchar", 500, false, ""),
					new DbColumn("XXX_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
				};

				Assert.True(TestDBHelper.TableExists(conn, "RefCusProfileType"), "RefCusProfileType existed");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusProfileType", false, expectedTableColumns), "RefCusProfileType columns existed with correct type and length and default value");

				var expectedRefCusProfileTypeTableView_V1Columns = new[]
				{
					new DbColumn("XXX_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("XXX_ProfileType", "varchar", 10, false, ""),
					new DbColumn("XXX_ZZI_TariffType", "uniqueidentifier", -1, false, ""),
					new DbColumn("XXX_Description", "nvarchar", 500, false, ""),
					new DbColumn("XXX_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
				};
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTypeTableView_V1"), "RefCusProfileTypeTableView_V1 existed");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusProfileTypeTableView_V1", true, expectedRefCusProfileTypeTableView_V1Columns), "RefCusProfileTypeTableView_V1 columns existed with correct type and length and default value");

				#endregion

				#region AssertVersion547Modifications

				refCusProfile = new RefCusProfile();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProfile.TableName, "DF_RefCusProfile_XX0_AppliesToCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfile.TableName, "XX0_AppliesToCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfile.TableName, "XX0_TariffCode", "VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusProfile_XX0_TariffCode DEFAULT('')"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_QuestionCode_XX0_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_QuestionCode_XX0_StartDate ON RefCusProfile (XX0_ZZZ_NKDataGrouping ASC, XX0_XXX_ProfileType ASC, XX0_TariffCode ASC, XX0_QuestionCode ASC, XX0_StartDate ASC)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(547).UpgradeScript);

				Assert.False(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_TariffCode"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_QuestionCode_XX0_StartDate"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusProfile.TableName, "XX0_AppliesToCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTableView_V1"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTableView_V2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTableView_V3"));

				AssertExistRefCusProfile_547(conn);
				#endregion

				#region AssertVersion548Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffAdditionalCodeView_V2", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffAdditionalCodeView_V2"));

				//Verify Version 548 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(548).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffAdditionalCodeView_V2"));
				#endregion

				#region AssertVersion549Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode", "UNIQUEIDENTIFIER"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffAdditionalCodeTableView_V2", "VIEW"));
				var additionalCodeViewSQL = @"CREATE VIEW RefCusTariffAdditionalCodeTableView_V2 AS
SELECT ZY2_PK,
ZY2_ZZ1_Tariff,
ZY2_ZZW_NationalCode,
ZY2_AdditionalCode,
ZY2_Description,
ZY2_ZY2_TariffAdditionalCode,
ZY2_ZY3_NKCategory,
ZY2_IsMandatory,
ZY2_ZZZ_NKDataGrouping
FROM RefCusTariffAdditionalCode";
				var createAdditionalCodeViewSQL = $@"
IF NOT EXISTS(
SELECT * FROM sys.objects o
WHERE o.type = 'V' AND o.name = 'RefCusTariffAdditionalCodeTableView_V2'
)
EXEC dbo.sp_executesql @statement = N'{additionalCodeViewSQL}'";
				restoreSb.AppendLine(createAdditionalCodeViewSQL);
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusTariffAdditionalCodeTableView_V2").Contains("ZY2_ZY2_TariffAdditionalCode,"));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusTariffAdditionalCodeTableView_V2").Contains("ZY2_ParentAdditionalCode,"));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusTariffAdditionalCodeTableView_V2").Contains("ZY2_ZY3_NKParentCategory,"));

				//Verify Version 549 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(549).UpgradeScript);
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusTariffAdditionalCodeTableView_V2").Contains("ZY2_ZY2_TariffAdditionalCode,"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusTariffAdditionalCodeTableView_V2").Contains("ZY2_ParentAdditionalCode,"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusTariffAdditionalCodeTableView_V2").Contains("ZY2_ZY3_NKParentCategory,"));
				#endregion

				#region AssertVersion550Modifications
				Assert.AreEqual("SELECT 1", provider.GetUpgradeWrapperByVersion(550).UpgradeScript);
				#endregion

				#region AssertVersion551Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V4", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V5", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_CompanyCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_BranchCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_CreatingUserCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_BillingReference1"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_BillingReference2"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_BillingReference3"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_BillingReference4"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_AdditionalRefs"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_PreparationScript"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_WhereClause"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refStlScript.TableName, "DF_RefStlScript_STL_MaxCW1Version"));
				var prepareTestDataSql = @"
ALTER TABLE RefStlScript ALTER COLUMN STL_CompanyCode NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BranchCode NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_CreatingUserCode NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference1 NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference2 NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference3 NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference4 NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_AdditionalRefs NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_PreparationScript NVARCHAR(MAX) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_WhereClause NVARCHAR(MAX) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_MinCW1Version NVARCHAR(1000) NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_MaxCW1Version NVARCHAR(1000) NULL;

INSERT INTO [dbo].[RefStlScript]([STL_PK],[STL_FeatureCode],[STL_RoleName],[STL_ModuleName],[STL_FunctionName],[STL_FeatureName],
	[STL_DataGranularity],[STL_CompanyCode],[STL_BranchCode],[STL_TransactionDateUtc],[STL_CreatingUserCode],[STL_GuidReference],
	[STL_BillingReference1],[STL_BillingReference2],[STL_BillingReference3],[STL_BillingReference4],[STL_AdditionalRefs],
	[STL_TransactionCount],[STL_PreparationScript],[STL_FromClause],[STL_WhereClause],[STL_WithOptionRecompile],[STL_UsedInBilling],
	[STL_ActiveOn],[STL_MinCW1Version],[STL_MaxCW1Version],[STL_DateType],[STL_CollectionStartDateUtc])
VALUES (NEWID(),'WIN','LS','WareHouse','Funcs','Contact','TRN',NULL,NULL,'wd.WD_SystemCreateTimeUtc',NULL,'wd.WD_PK',
		NULL,NULL,NULL,NULL,NULL,1,NULL,'WhsDocket wd',NULL,0,1,'ALL',NULL,NULL,'DTE',NULL),
	(NEWID(),'WIN','LS','WareHouse','Funcs','Contact','TRN',NULL,NULL,'wd.WD_SystemCreateTimeUtc',NULL,'wd.WD_PK',
		NULL,NULL,NULL,NULL,NULL,1,NULL,'WhsDocket wd',NULL,0,1,'ALL','',NULL,'DTE',NULL);
";
				restoreSb.AppendLine(prepareTestDataSql);
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				var querySql = @"
select count(1) from RefStlScript
where STL_CompanyCode='' and STL_BranchCode='' and STL_CreatingUserCode='' and STL_BillingReference1='' and STL_BillingReference2='' and STL_BillingReference3='' and STL_BillingReference4=''
	and STL_AdditionalRefs='' and STL_PreparationScript='' and STL_WhereClause='' and STL_MinCW1Version='' and STL_MaxCW1Version='';
";
				Assert.That(TestDBHelper.ExecuteScalar(conn, querySql), Is.EqualTo(0));

				//Verify Version 551 Upgrade Sript
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(551).UpgradeScript);
				Assert.That(TestDBHelper.ExecuteScalar(conn, querySql), Is.EqualTo(1));
				#endregion

				#region AssertVersion552Modifications
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_CompanyCode"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BranchCode"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_CreatingUserCode"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BillingReference1"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BillingReference1"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BillingReference1"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BillingReference1"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_AdditionalRefs"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_PreparationScript"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_WhereClause"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_MinCW1Version"));
				Assert.False(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_MaxCW1Version"));

				//Verify Version 552 Upgrade Sript
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(552).UpgradeScript);
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_CompanyCode"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BranchCode"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_CreatingUserCode"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BillingReference1"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BillingReference1"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BillingReference1"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_BillingReference1"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_AdditionalRefs"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_PreparationScript"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_WhereClause"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_MinCW1Version"));
				Assert.That(TestDBHelper.CheckDefaultConstraintExists(conn, nameof(RefStlScript), "DF_RefStlScript_STL_MaxCW1Version"));
				#endregion

				#region AssertVersion553Modifications

				Assert.That(provider.GetUpgradeWrapperByVersion(553).UpgradeScript, Is.EqualTo("SELECT 1"));

				#endregion

				#region AssertVersion554Modifications
				var refCusCodeListAttribute_554 = new RefCusCodeListAttribute();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropTriggerIfExistsScript(refCusCodeListAttribute_554.TableName, "TG_RefCusCodeListAttribute_INS_UPD"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttribute_INS_UPD"));

				//Verify Version 554 Upgrade Sript
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(554).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "TR", "TG_RefCusCodeListAttribute_INS_UPD"));
				#endregion

				#region AssertVersion555Modifications
				// Restore and Pre-condition Check
				var undgSubstanceADN_555 = new UNDGSubstanceADN();
				var undgSubstanceADR_555 = new UNDGSubstanceADR();
				var undgSubstanceCFR_555 = new UNDGSubstanceCFR();
				var undgSubstanceJTT_555 = new UNDGSubstanceJTT();
				var undgSubstanceRID_555 = new UNDGSubstanceRID();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceADNTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceADRTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceCFRTableView_V5", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceJTTTableView_V3", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceRIDTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceADN_555.TableName, "DF_UNDGSubstanceADN_ADN_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceADN_555.TableName, "DF_UNDGSubstanceADN_ADN_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceADR_555.TableName, "DF_UNDGSubstanceADR_ADR_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceADR_555.TableName, "DF_UNDGSubstanceADR_ADR_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceCFR_555.TableName, "DF_UNDGSubstanceCFR_CFR_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceCFR_555.TableName, "DF_UNDGSubstanceCFR_CFR_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceJTT_555.TableName, "DF_UNDGSubstanceJTT_JTT_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceJTT_555.TableName, "DF_UNDGSubstanceJTT_JTT_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceRID_555.TableName, "DF_UNDGSubstanceRID_RID_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", undgSubstanceRID_555.TableName, "DF_UNDGSubstanceRID_RID_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceADN_555.TableName, "ADN_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceADN_555.TableName, "ADN_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceADR_555.TableName, "ADR_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceADR_555.TableName, "ADR_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceCFR_555.TableName, "CFR_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceCFR_555.TableName, "CFR_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceJTT_555.TableName, "JTT_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceJTT_555.TableName, "JTT_MaxCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceRID_555.TableName, "RID_MinCW1Version"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(undgSubstanceRID_555.TableName, "RID_MaxCW1Version"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceADN_555.TableName, "ADN_MinCW1Version"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceADN_555.TableName, "ADN_MaxCW1Version"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceADNTableView_V2"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceADR_555.TableName, "ADR_MinCW1Version"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceADR_555.TableName, "ADR_MaxCW1Version"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceADRTableView_V2"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_555.TableName, "CFR_MinCW1Version"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_555.TableName, "CFR_MaxCW1Version"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceCFRTableView_V5"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceJTT_555.TableName, "JTT_MinCW1Version"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceJTT_555.TableName, "JTT_MaxCW1Version"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceJTTTableView_V3"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceRID_555.TableName, "RID_MinCW1Version"));
				Assert.False(TestDBHelper.ColumnExists(conn, undgSubstanceRID_555.TableName, "RID_MaxCW1Version"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceRIDTableView_V2"));

				//Verify Version 555 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(555).UpgradeScript);
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceADN_555.TableName, "ADN_MinCW1Version"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceADN_555.TableName, "ADN_MaxCW1Version"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceADNTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceADNTableView_V2").Contains("ADN_MinCW1Version"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceADNTableView_V2").Contains("ADN_MaxCW1Version"));

				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceADR_555.TableName, "ADR_MinCW1Version"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceADR_555.TableName, "ADR_MaxCW1Version"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceADRTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceADRTableView_V2").Contains("ADR_MinCW1Version"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceADRTableView_V2").Contains("ADR_MaxCW1Version"));

				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_555.TableName, "CFR_MinCW1Version"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceCFR_555.TableName, "CFR_MaxCW1Version"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceCFRTableView_V5"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceCFRTableView_V5").Contains("CFR_MinCW1Version"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceCFRTableView_V5").Contains("CFR_MaxCW1Version"));

				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceJTT_555.TableName, "JTT_MinCW1Version"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceJTT_555.TableName, "JTT_MaxCW1Version"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceJTTTableView_V3"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceJTTTableView_V3").Contains("JTT_MinCW1Version"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceJTTTableView_V3").Contains("JTT_MaxCW1Version"));

				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceRID_555.TableName, "RID_MinCW1Version"));
				Assert.True(TestDBHelper.ColumnExists(conn, undgSubstanceRID_555.TableName, "RID_MaxCW1Version"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceRIDTableView_V2"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceRIDTableView_V2").Contains("RID_MinCW1Version"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "UNDGSubstanceRIDTableView_V2").Contains("RID_MaxCW1Version"));

				#endregion

				#region AssertVersion556Modifications

				// Restore and Pre-condition Check
				var refStlScript_556 = new RefStlScript();
				var refStlScriptTableViewPrefix = $"{refStlScript_556.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript_556.TableName, "STL_AdditionalRefs"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refStlScript_556.TableName, "STL_AdditionalRefs", "NVARCHAR(1000) NOT NULL"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript_556.TableName, "DF_RefStlScript_STL_AdditionalRefs", "''", "STL_AdditionalRefs"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.TableColumnsExist(conn, refStlScript_556.TableName, false, [new DbColumn("STL_AdditionalRefs", "nvarchar", 1000, false, "('')")]), "STL_AdditionalRefs has field length of NVARCHAR(1000)");

				// Verify version 556 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(556).UpgradeScript);
				Assert.True(TestDBHelper.TableColumnsExist(conn, refStlScript_556.TableName, false, [new DbColumn("STL_AdditionalRefs", "nvarchar", -1, false, "('')")]), "STL_AdditionalRefs has field length of NVARCHAR(MAX)");

				#endregion

				#region AssertVersion557Modifications

				// Restore and Pre-condition Check
				refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode", "UNIQUEIDENTIFIER"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode", "ZY2_ZY2_TariffAdditionalCode", "RefCusTariffAdditionalCode (ZY2_PK)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode", "CREATE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping, ZY2_ZY2_TariffAdditionalCode)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping, ZY2_ZY3_NKCategory, ZY2_AdditionalCode, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory", "CREATE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping, ZY2_ZY3_NKCategory, ZY2_AdditionalCode, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory)"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode"));
				Assert.True(TestDBHelper.ForeignKeyExists(conn, refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
				Assert.False(TestDBHelper.UniqueIndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory"));

				// Verify version 557 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(557).UpgradeScript);
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode"));
				Assert.False(TestDBHelper.ForeignKeyExists(conn, refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
				Assert.True(TestDBHelper.UniqueIndexExists(conn, refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory"));

				#endregion

				#region AssertVersion558Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffUOMView_V3", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V3"));

				//Verify Version 424 Upgrade 558
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(558).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffUOMView_V3"));

				#endregion

				#region AssertVersion559Modifications

				// Restore and Pre-condition Check
				var refCusCodeListAttributeName = new RefCusCodeListAttributeName();

				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeType_ZXE_ZZZ_NKDataGrouping"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeTypeComputed"));

				var refCusCodeListAttributeNameViewPrefix = $"{refCusCodeListAttributeName.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListAttributeNameViewPrefix}1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListAttributeNameViewPrefix}2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListAttributeNameViewPrefix}3", "VIEW"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeType", "VARCHAR(5) NOT NULL"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeTypeForValueList", "VARCHAR(5)"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeType_ZXE_ZZZ_NKDataGrouping", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeType_ZXE_ZZZ_NKDataGrouping ON RefCusCodeListAttributeName(ZXE_Name,ZXE_ZZK_NKCodeType,ZXE_ZZZ_NKDataGrouping)"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption", "CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption] ON [dbo].[RefCusCodeListAttributeName] (ZXE_ZZZ_NKDataGrouping, ZXE_ZZK_NKCodeType, ZXE_ColumnCaption) WHERE ZXE_ColumnCaption <> ''"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeListAttributeName.TableName, false, [new DbColumn("ZXE_ZZK_NKCodeType", "varchar", 5, false, ""),]), "ZXE_ZZK_NKCodeType max length is 5");
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeListAttributeName.TableName, false, [new DbColumn("ZXE_ZZK_NKCodeTypeForValueList", "varchar", 5, true, "")]), "ZXE_ZZK_NKCodeTypeForValueList max length is 5");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListAttributeNameViewPrefix}1"), $"View {0} should not exist", $"{refCusCodeListAttributeNameViewPrefix}1");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListAttributeNameViewPrefix}2"), $"View {0} should not exist", $"{refCusCodeListAttributeNameViewPrefix}2");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListAttributeNameViewPrefix}3"), $"View {0} should not exist", $"{refCusCodeListAttributeNameViewPrefix}3");

				// Verify version 559 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(559).UpgradeScript);
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeListAttributeName.TableName, false, [new DbColumn("ZXE_ZZK_NKCodeType", "varchar", 10, false, ""),]), "ZXE_ZZK_NKCodeType max length is 10");
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeListAttributeName.TableName, false, [new DbColumn("ZXE_ZZK_NKCodeTypeForValueList", "varchar", 10, true, "")]), "ZXE_ZZK_NKCodeTypeForValueList max length is 10");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListAttributeNameViewPrefix}1"), $"View {0} should exist", $"{refCusCodeListAttributeNameViewPrefix}1");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListAttributeNameViewPrefix}2"), $"View {0} should exist", $"{refCusCodeListAttributeNameViewPrefix}2");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListAttributeNameViewPrefix}3"), $"View {0} should exist", $"{refCusCodeListAttributeNameViewPrefix}3");
				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeListAttributeName.TableName, false, [new DbColumn("ZXE_ZZK_NKCodeTypeComputed", "varchar", 5, false, ""),]), "ZXE_ZZK_NKCodeTypeComputed max length is 5");

				#endregion

				#region AssertVersion560Modifications

				// Restore and Pre-condition Check
				var refCusCodeList = new RefCusCodeList();

				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeType_ZZD_StartDate_ZZD_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeType_ZZD_Code"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeTypeComputed"));

				var refCusCodeListViewPrefix = $"{refCusCodeList.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListViewPrefix}1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListViewPrefix}2", "VIEW"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeType", "VARCHAR(5) NOT NULL"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeType_ZZD_Code", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeType_ZZD_Code ON RefCusCodeList ( ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeType, ZZD_Code )"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeType_ZZD_StartDate_ZZD_EndDate", "CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZK_NKCodeType_ZZD_StartDate_ZZD_EndDate ON RefCusCodeList(ZZD_ZZK_NKCodeType,ZZD_StartDate,ZZD_EndDate) INCLUDE (ZZD_PK,ZZD_Code,ZZD_Description,ZZD_ZZZ_NKDataGrouping)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeList.TableName, false, [new DbColumn("ZZD_ZZK_NKCodeType", "varchar", 5, false, string.Empty)]), "ZZD_ZZK_NKCodeType max length is 5");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListViewPrefix}1"), $"View {0} should not exist", $"{refCusCodeListViewPrefix}1");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListViewPrefix}2"), $"View {0} should not exist", $"{refCusCodeListViewPrefix}2");

				// Verify version 560 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(560).UpgradeScript);
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeList.TableName, false, [new DbColumn("ZZD_ZZK_NKCodeType", "varchar", 10, false, string.Empty)]), "ZZD_ZZK_NKCodeType max length is 10");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListViewPrefix}1"), $"View {0} should exist", $"{refCusCodeListViewPrefix}1");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListViewPrefix}2"), $"View {0} should exist", $"{refCusCodeListViewPrefix}2");
				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeList.TableName, false, [new DbColumn("ZZD_ZZK_NKCodeTypeComputed", "varchar", 5, false, ""),]), "ZZD_ZZK_NKCodeTypeComputed max length is 5");

				#endregion

				#region AssertVersion561Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeType"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeType.TableName, "ZZK_CodeTypeComputed"));

				var refCusCodeTypeViewPrefix = $"{refCusCodeType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeTypeViewPrefix}1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeTypeViewPrefix}2", "VIEW"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusCodeType.TableName, "ZZK_CodeType", "VARCHAR(5) NOT NULL"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeType", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeType ON RefCusCodeType (ZZK_ZZZ_NKDataGrouping ASC, ZZK_CodeType ASC)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeType.TableName, false, [new DbColumn("ZZK_CodeType", "varchar", 5, false, string.Empty)]), "ZZK_CodeType max length is 5");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeTypeViewPrefix}1"), $"View {0} should not exist", $"{refCusCodeTypeViewPrefix}1");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeTypeViewPrefix}2"), $"View {0} should not exist", $"{refCusCodeTypeViewPrefix}2");

				// Verify version 561 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(561).UpgradeScript);
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeType.TableName, false, [new DbColumn("ZZK_CodeType", "varchar", 10, false, string.Empty)]), "ZZK_CodeType max length is 10");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeTypeViewPrefix}1"), $"View {0} should exist", $"{refCusCodeTypeViewPrefix}1");
				Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeTypeViewPrefix}2"), $"View {0} should exist", $"{refCusCodeTypeViewPrefix}2");
				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeType.TableName, false, [new DbColumn("ZZK_CodeTypeComputed", "varchar", 5, false, ""),]), "ZZK_CodeTypeComputed max length is 5");

				#endregion

				#region AssertVersion562Modifications

				Assert.That(provider.GetUpgradeWrapperByVersion(562).UpgradeScript, Is.EqualTo("SELECT 1"));

				#endregion

				#region AssertVersion563Modifications

				// Restore and Pre-condition Check
				var uNDGSubstanceTDG = new UNDGSubstanceTDG();
				restoreSb.Clear();
				Assert.False(TestDBHelper.TableExists(conn, uNDGSubstanceTDG.TableName));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceTDGTableView_V1"));

				// Verify version 563 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(563).UpgradeScript);
				Assert.True(TestDBHelper.TableExists(conn, uNDGSubstanceTDG.TableName));
				Assert.True(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "PK_UNDGSubstanceTDG", "PK" } }));
				Assert.True(TestDBHelper.IndexExists(conn, uNDGSubstanceTDG.TableName, "IX_UNDGSubstanceTDG_TDG_UNNO_TDG_Variant"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "UNDGSubstanceTDGTableView_V1"));

				#endregion

				#region AssertVersion564Modifications

				Assert.That(provider.GetUpgradeWrapperByVersion(564).UpgradeScript, Is.EqualTo("SELECT 1"));

				#endregion

				#region AssertVersion565Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeTypeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeListTableView_V1", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				restoreSb.Clear();
				restoreSb.AppendLine(@"CREATE VIEW RefCusCodeListTableView_V1 AS
SELECT ZZD_PK,
ISNULL(LEFT(ZZD_ZZK_NKCodeType, 5), '') AS ZZD_ZZK_NKCodeType,
ZZD_Code,
ZZD_Description,
ZZD_StartDate,
ZZD_EndDate,
ZZD_ZZZ_NKDataGrouping
FROM RefCusCodeList");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				restoreSb.Clear();
				restoreSb.AppendLine(@"CREATE VIEW RefCusCodeTypeTableView_V1 AS
SELECT ZZK_PK,
ISNULL(LEFT(ZZK_CodeType, 5), '') AS ZZK_CodeType,
ZZK_Description,
ZZK_IsReadonly,
ZZK_MaxLength,
ZZK_ZZZ_NKDataGrouping
FROM RefCusCodeType");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusCodeListTableView_V1").Contains("ISNULL(LEFT(ZZD_ZZK_NKCodeType, 5), '')"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusCodeTypeTableView_V1").Contains("ISNULL(LEFT(ZZK_CodeType, 5), '')"));

				//Verify Version 565 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(565).UpgradeScript);
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusCodeListTableView_V1").Contains("ISNULL(LEFT(ZZD_ZZK_NKCodeType, 5), '')"));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusCodeTypeTableView_V1").Contains("ISNULL(LEFT(ZZK_CodeType, 5), '')"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeList.TableName, false, [new DbColumn("ZZD_ZZK_NKCodeTypeComputed", "varchar", 5, false, ""),]), "ZZD_ZZK_NKCodeTypeComputed max length is 5");
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeType.TableName, false, [new DbColumn("ZZK_CodeTypeComputed", "varchar", 5, false, ""),]), "ZZK_CodeTypeComputed max length is 5");

				#endregion

				#region AssertVersion566Modifications

				var refCusCodeTypeAttribute = new DbTable("RefCusCodeTypeAttribute",
					"RefCusCodeTypeAttributeTableView_V1",
					[
						new DbColumn("ZKE_PK", "uniqueidentifier", -1, false, "(newid())"),
						new DbColumn("ZKE_ZZK_CodeType", "uniqueidentifier", -1, false, ""),
						new DbColumn("ZKE_ZKA_NKName", "varchar", 32, false, ""),
						new DbColumn("ZKE_Value", "nvarchar", 255, false, ""),
						new DbColumn("ZKE_StartDate", "smalldatetime", -1, false, "(getutcdate())"),
						new DbColumn("ZKE_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')")
					],
					new Dictionary<string, string>
					{
						{ "DF_RefCusCodeTypeAttribute_ZKE_PK", "D" },
						{ "DF_RefCusCodeTypeAttribute_ZKE_StartDate", "D" },
						{ "DF_RefCusCodeTypeAttribute_ZKE_EndDate", "D" },
						{ "PK_RefCusCodeTypeAttribute", "PK" },
						{ "FK_RefCusCodeTypeAttribute_RefCusCodeType", "F" },
					},
					[
						("CK_RefCusCodeTypeAttribute_ZKE_ZKA_NKName", "([ZKE_ZKA_NKName]<>'')"),
						("CK_RefCusCodeTypeAttribute_ZKE_StartDate_ZKE_EndDate", "([ZKE_StartDate]<=[ZKE_EndDate])")
					],
					[
						"IX_RefCusCodeTypeAttribute_ZKE_ZZK_CodeType_ZKE_ZKA_NKName_ZKE_StartDate_ZKE_Value",
						"IX_RefCusCodeTypeAttribute_ZKE_ZKA_NKName"
					]);
				var refCusCodeTypeAttributeName = new DbTable(
					"RefCusCodeTypeAttributeName",
					"RefCusCodeTypeAttributeNameTableView_V1",
					[
						new DbColumn("ZKA_PK", "uniqueidentifier", -1, false, "(newid())"),
						new DbColumn("ZKA_Name", "varchar", 32, false, ""),
						new DbColumn("ZKA_Description", "varchar", 500, false, ""),
						new DbColumn("ZKA_ZZK_NKCodeType", "varchar", 10, false, ""),
						new DbColumn("ZKA_ZZZ_NKDataGrouping", "varchar", 3, false, "")
					],
					new Dictionary<string, string>
					{
						{ "DF_RefCusCodeTypeAttributeName_ZKA_PK", "D" },
						{ "PK_RefCusCodeTypeAttributeName", "PK" },
						{ "FK_RefCusCodeTypeAttributeName_RefDataGrouping", "F" },
						{ "FK_RefCusCodeTypeAttributeName_RefCusCodeType", "F" },
					},
					[
						("CK_RefCusCodeTypeAttributeName_ZKA_Description", "([ZKA_Description]<>'')"),
						("CK_RefCusCodeTypeAttributeName_ZKA_Name", "([ZKA_Name]<>'')")
					],
					[
						"IX_RefCusCodeTypeAttributeName_ZKA_ZZZ_NKDataGrouping_ZKA_ZZK_NKCodeType_ZKA_Name"
					]);

				TestDBHelper.AssertSchemaObjects(conn, provider, 566, refCusCodeTypeAttribute, refCusCodeTypeAttributeName);

				#endregion

				#region AssertVersion567Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeTypeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeListTableView_V1", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				restoreSb.Clear();
				restoreSb.AppendLine(@"CREATE VIEW RefCusCodeListTableView_V1 AS
SELECT ZZD_PK,
ZZD_ZZK_NKCodeType,
ZZD_Code,
ZZD_Description,
ZZD_StartDate,
ZZD_EndDate,
ZZD_ZZZ_NKDataGrouping
FROM RefCusCodeList");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				restoreSb.Clear();
				restoreSb.AppendLine(@"CREATE VIEW RefCusCodeTypeTableView_V1 AS
SELECT ZZK_PK,
ZZK_CodeType,
ZZK_Description,
ZZK_IsReadonly,
ZZK_MaxLength,
ZZK_ZZZ_NKDataGrouping
FROM RefCusCodeType");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusCodeListTableView_V1").Contains("ISNULL(LEFT(ZZD_ZZK_NKCodeType, 5), '')"));
				Assert.False(TestDBHelper.GetViewDefinition(conn, "RefCusCodeTypeTableView_V1").Contains("ISNULL(LEFT(ZZK_CodeType, 5), '')"));

				//Verify Version 567 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(567).UpgradeScript);
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusCodeListTableView_V1").Contains("ZZD_ZZK_NKCodeTypeComputed"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, "RefCusCodeTypeTableView_V1").Contains("ZZK_CodeTypeComputed"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeList.TableName, false, [new DbColumn("ZZD_ZZK_NKCodeTypeComputed", "varchar", 5, false, ""),]), "ZZD_ZZK_NKCodeTypeComputed max length is 5");
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeType.TableName, false, [new DbColumn("ZZK_CodeTypeComputed", "varchar", 5, false, ""),]), "ZZK_CodeTypeComputed max length is 5");

				#endregion

				#region AssertVersion568Modifications

				// Restore and Pre-condition Check
				refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusTariffAdditionalCode.TableName, "CK_RefCusTariffAdditionalCode_ZY2_StartDate_ZY2_EndDate"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffAdditionalCode.TableName, "DF_RefCusTariffAdditionalCode_ZY2_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusTariffAdditionalCode.TableName, "DF_RefCusTariffAdditionalCode_ZY2_EndDate"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_StartDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_EndDate"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffAdditionalCodeTableView_V3", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_StartDate"));
				Assert.False(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_EndDate"));
				Assert.False(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusTariffAdditionalCode_ZY2_StartDate", "D" } }));
				Assert.False(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusTariffAdditionalCode_ZY2_EndDate", "D" } }));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V2"));
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V3"));

				//Verify Version 568 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(568).UpgradeScript);

				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_StartDate"));
				Assert.True(TestDBHelper.ColumnExists(conn, refCusTariffAdditionalCode.TableName, "ZY2_EndDate"));
				Assert.True(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusTariffAdditionalCode_ZY2_StartDate", "D" } }));
				Assert.True(TestDBHelper.ConstraintsExist(conn, new Dictionary<string, string> { { "DF_RefCusTariffAdditionalCode_ZY2_EndDate", "D" } }));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V2"));
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusTariffAdditionalCodeTableView_V3"));
				#endregion

				#region AssertVersion569Modifications

				// Restore and Pre-condition Check
				refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusTariffAdditionalCode.TableName, "CK_RefCusTariffAdditionalCode_ZY2_StartDate_ZY2_EndDate"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.CheckConstraintExists(conn, refCusTariffAdditionalCode.TableName, "CK_RefCusTariffAdditionalCode_ZY2_StartDate_ZY2_EndDate"));

				//Verify Version 569 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(569).UpgradeScript);

				Assert.True(TestDBHelper.CheckConstraintExists(conn, refCusTariffAdditionalCode.TableName, "CK_RefCusTariffAdditionalCode_ZY2_StartDate_ZY2_EndDate"));

				#endregion

				#region AssertVersion570Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				refCusCodeListAttributeName = new RefCusCodeListAttributeName();
				refCusCodeListAttributeNameViewPrefix = $"{refCusCodeListAttributeName.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListAttributeNameViewPrefix}1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListAttributeNameViewPrefix}2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeTypeComputed"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption"));
				Assert.False(TestDBHelper.TableColumnsExist(conn, refCusCodeListAttributeName.TableName, false, [new DbColumn("ZXE_ZZK_NKCodeTypeComputed", "varchar", 5, false, ""),]), "ZXE_ZZK_NKCodeTypeComputed not exist");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListAttributeNameViewPrefix}1"), $"View {0} should not exist", $"{refCusCodeListAttributeNameViewPrefix}1");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListAttributeNameViewPrefix}2"), $"View {0} should not exist", $"{refCusCodeListAttributeNameViewPrefix}2");

				// Verify version 570 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(570).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeListAttributeName.TableName, false, [new DbColumn("ZXE_ZZK_NKCodeTypeComputed", "varchar", 5, false, ""),]), "ZXE_ZZK_NKCodeTypeComputed max length is 5");
				Assert.True(TestDBHelper.GetViewDefinition(conn, $"{refCusCodeListAttributeNameViewPrefix}1").Contains("ZXE_ZZK_NKCodeTypeComputed"));
				Assert.True(TestDBHelper.GetViewDefinition(conn, $"{refCusCodeListAttributeNameViewPrefix}2").Contains("ZXE_ZZK_NKCodeTypeComputed"));

				#endregion

				#region AssertVersion571Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				refCusCodeList = new RefCusCodeList();
				refCusCodeListViewPrefix = $"{refCusCodeList.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListViewPrefix}1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeTypeComputed"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate"));
				Assert.False(TestDBHelper.IndexExists(conn, refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code"));
				Assert.False(TestDBHelper.TableColumnsExist(conn, refCusCodeList.TableName, false, [new DbColumn("ZZD_ZZK_NKCodeTypeComputed", "varchar", 5, false, ""),]), "ZZD_ZZK_NKCodeTypeComputed not exist");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeListViewPrefix}1"), $"View {0} should not exist", $"{refCusCodeListViewPrefix}1");

				// Verify version 571 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(571).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeList.TableName, false, [new DbColumn("ZZD_ZZK_NKCodeTypeComputed", "varchar", 5, false, ""),]), "ZZD_ZZK_NKCodeTypeComputed max length is 5");
				Assert.True(TestDBHelper.GetViewDefinition(conn, $"{refCusCodeListViewPrefix}1").Contains("ZZD_ZZK_NKCodeTypeComputed"));

				#endregion

				#region AssertVersion572Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();

				refCusCodeType = new RefCusCodeType();
				refCusCodeTypeViewPrefix = $"{refCusCodeType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeTypeViewPrefix}1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeType.TableName, "ZZK_CodeTypeComputed"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.False(TestDBHelper.IndexExists(conn, refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed"));
				Assert.False(TestDBHelper.TableColumnsExist(conn, refCusCodeType.TableName, false, [new DbColumn("ZZK_CodeTypeComputed", "varchar", 5, false, ""),]), "ZZK_CodeTypeComputed not exist");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusCodeTypeViewPrefix}1"), $"View {0} should not exist", $"{refCusCodeTypeViewPrefix}1");

				// Verify version 572 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(572).UpgradeScript);

				Assert.True(TestDBHelper.IndexExists(conn, refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed"));
				Assert.True(TestDBHelper.TableColumnsExist(conn, refCusCodeType.TableName, false, [new DbColumn("ZZK_CodeTypeComputed", "varchar", 5, false, ""),]), "ZZK_CodeTypeComputed max length is 5");
				Assert.True(TestDBHelper.GetViewDefinition(conn, $"{refCusCodeTypeViewPrefix}1").Contains("ZZK_CodeTypeComputed"));

				#endregion

				#region AssertVersion573Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				var refCusProfileQuestion = new RefCusProfileQuestion();
				var refCusProfileQuestionTableViewPrefix = $"{refCusProfileQuestion.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";

				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusProfileQuestionTableViewPrefix}2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusProfileQuestion.TableName, "CK_RefCusProfileQuestion_XQ2_QuestionCode"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfileQuestion.TableName, "IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate"));

				restoreSb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refCusProfileQuestion.TableName, "XQ2_QuestionCode", "XQ2_Code"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				restoreSb.Clear();

				restoreSb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusProfileQuestion.TableName, "CK_RefCusProfileQuestion_XQ2_Code", "(XQ2_Code <> '')"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfileQuestion.TableName,
					"IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate",
					"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate ON RefCusProfileQuestion (XQ2_Code ASC, XQ2_XXX_ProfileType ASC, XQ2_ZZZ_NKDataGrouping ASC, XQ2_StartDate ASC)"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.True(TestDBHelper.ObjectExists(conn, "V", $"{refCusProfileQuestionTableViewPrefix}1"), $"View {0} should exist", $"{refCusProfileQuestionTableViewPrefix}1");
				Assert.False(TestDBHelper.ObjectExists(conn, "V", $"{refCusProfileQuestionTableViewPrefix}2"), $"View {0} should not exist", $"{refCusProfileQuestionTableViewPrefix}2");
				Assert.True(TestDBHelper.CheckConstraintExists(conn, refCusProfileQuestion.TableName, "CK_RefCusProfileQuestion_XQ2_Code"));
				Assert.True(TestDBHelper.IndexExists(conn, refCusProfileQuestion.TableName, "IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate"));

				// Verify version 573 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(573).UpgradeScript);
				AssertExistRefCusProfileQuestion_573(conn);
				#endregion

				#region AssertVersion574Modifications
				// Verify version 574 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(574).UpgradeScript);
				AssertExistRefCusProfileQuestion_574(conn);
				#endregion

				#region AssertVersion575Modifications

				// Restore and Pre-condition Check
				refStlScript = new RefStlScript();
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refStlScript.TableName, "CK_RefStlScript_STL_CollectionStartDateUtc"));

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				Assert.That(TestDBHelper.CheckConstraintExists(conn, refStlScript.TableName, "CK_RefStlScript_STL_CollectionStartDateUtc"), Is.False);

				//Verify Version 575 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(575).UpgradeScript);

				Assert.That(TestDBHelper.CheckConstraintExists(conn, refStlScript.TableName, "CK_RefStlScript_STL_CollectionStartDateUtc"), Is.True);

				#endregion

				#region AssertVersion576Modifications
				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffAdditionalCodeView_V3", "VIEW"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());
				Assert.False(TestDBHelper.ObjectExists(conn, "V", "TariffAdditionalCodeView_V3"));

				//Verify Version 576 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(576).UpgradeScript);
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "TariffAdditionalCodeView_V3"));
				#endregion

				#region AssertVersion578Modifications

				// Restore and Pre-condition Check
				restoreSb.Clear();
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTypeTableView_V1", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTypeTableView_V2", "VIEW"));
				restoreSb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusProfileType.TableName, "XXX_ZZI_TariffType", "UNIQUEIDENTIFIER NOT NULL"));
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				restoreSb.Clear();
				restoreSb.AppendLine(@"CREATE VIEW RefCusProfileTypeTableView_V1 AS
SELECT XXX_PK,
XXX_ProfileType,
XXX_ZZI_TariffType,
XXX_Description,
XXX_ZZZ_NKDataGrouping
FROM RefCusProfileType");

				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				var expectedRefCusProfileTypeTableView_V1PrereqColumns = new[]
{
					new DbColumn("XXX_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("XXX_ProfileType", "varchar", 10, false, ""),
					new DbColumn("XXX_ZZI_TariffType", "uniqueidentifier", -1, false, ""),
					new DbColumn("XXX_Description", "nvarchar", 500, false, ""),
					new DbColumn("XXX_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
				};
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTypeTableView_V1"), "PREREQ: RefCusProfileTypeTableView_V1 existed");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusProfileTypeTableView_V1", true, expectedRefCusProfileTypeTableView_V1PrereqColumns), "PREREQ: RefCusProfileTypeTableView_V1 columns existed with correct type and length and default value");

				//Verify Version 578 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(578).UpgradeScript);

				var expectedRefCusProfileTypeTableColumns_578 = new[]
				{
					new DbColumn("XXX_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("XXX_ProfileType", "varchar", 10, false, ""),
					new DbColumn("XXX_ZZI_TariffType", "uniqueidentifier", -1, true, ""),
					new DbColumn("XXX_Description", "nvarchar", 500, false, ""),
					new DbColumn("XXX_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
				};

				Assert.True(TestDBHelper.TableExists(conn, "RefCusProfileType"), "RefCusProfileType existed");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusProfileType", false, expectedRefCusProfileTypeTableColumns_578), "RefCusProfileType columns existed with correct type and length and default value");

				var expectedRefCusProfileTypeTableView_V1RecreatedColumns_578 = new[]
				{
					new DbColumn("XXX_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("XXX_ProfileType", "varchar", 10, false, ""),
					new DbColumn("XXX_ZZI_TariffType", "uniqueidentifier", -1, false, ""),
					new DbColumn("XXX_Description", "nvarchar", 500, false, ""),
					new DbColumn("XXX_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
				};
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTypeTableView_V1"), "RefCusProfileTypeTableView_V1 existed");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusProfileTypeTableView_V1", true, expectedRefCusProfileTypeTableView_V1RecreatedColumns_578), "RefCusProfileTypeTableView_V1 columns existed with correct type and length and default value");

				var expectedRefCusProfileTypeTableView_V2Columns_578 = new[]
				{
					new DbColumn("XXX_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("XXX_ProfileType", "varchar", 10, false, ""),
					new DbColumn("XXX_ZZI_TariffType", "uniqueidentifier", -1, true, ""),
					new DbColumn("XXX_Description", "nvarchar", 500, false, ""),
					new DbColumn("XXX_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
				};
				Assert.True(TestDBHelper.ObjectExists(conn, "V", "RefCusProfileTypeTableView_V2"), "RefCusProfileTypeTableView_V2 existed");
				Assert.True(TestDBHelper.TableColumnsExist(conn, "RefCusProfileTypeTableView_V2", true, expectedRefCusProfileTypeTableView_V2Columns_578), "RefCusProfileTypeTableView_V2 columns existed with correct type and length and default value");
				#endregion

				#region AssertVersion579Modifications

				restoreSb.Clear();
				restoreSb.AppendLine(@"
INSERT INTO RefDbVersionControl (RVC_PK, RVC_DataSet, RVC_LastUpdatedUtc, RVC_UpdaterVersion)
VALUES (newid(), 'RefExchangeRateZZ', '2025-07-02 00:00:00', 4),
(newid(), 'RefCusQuota', '2025-07-01 00:00:00', 1),
(newid(), 'RefCusTaxOrFeeType', '2025-06-26 00:00:00', 1);
");
				TestDBHelper.ExecuteNonQuery(conn, restoreSb.ToString());

				//Verify Version 579 Upgrade Script
				TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(579).UpgradeScript);

				Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefExchangeRateZZ' AND RVC_LastUpdatedUtc = '2025-04-02 00:00:00'"), Is.EqualTo(1));
				Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusQuota' AND RVC_LastUpdatedUtc='2025-04-02 00:00:00'"), Is.EqualTo(1));
				Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefDbVersionControl WHERE RVC_DataSet = 'RefCusTaxOrFeeType' AND RVC_LastUpdatedUtc='2025-04-02 00:00:00'"), Is.EqualTo(1));

				#endregion
			}
		}
#pragma warning restore CA1505
#pragma warning restore CA1506

		void AssertDefaultValue(SqlConnection conn, bool shouldBe)
		{
			foreach (var valueTuple in UpgradeScriptProvider.GetColumnsToAddDefaultValue())
			{
				foreach (var column in valueTuple.Columns)
				{
					Assert.AreEqual(shouldBe, TestDBHelper.CheckDefaultConstraintExists(conn, valueTuple.TableScript.TableName,
						$"DF_{valueTuple.TableScript.TableName}_{column.Key}"));
				}
			}
		}

		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void TestAllTablesHavePKAndPrefix(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);

			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				var result = TestDBHelper.ExecuteScalar(conn, @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE cols
WHERE NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
AND (COLUMN_NAME LIKE '[A-Za-z0-9][A-Za-z0-9][A-Za-z0-9]_PK' OR COLUMN_NAME LIKE '[A-Za-z0-9][A-Za-z0-9]_PK')
AND cols.TABLE_NAME = TABLE_NAME)
				");
				Assert.AreEqual(0, result, $"There are {result} tables which do not have primary key or their primary keys do not have format of tablePrefix_PK");
			}
		}

		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void TestTablesShouldHaveNoUniqueConstraint(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);

			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				var result = TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM sys.key_constraints WHERE type='UQ'");
				Assert.AreEqual(0, result, $"There are {result} unique constraints in RemoteDB");
			}
		}

		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void TestRefCusApplicabilityViews(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);

			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();

				TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5', 'EUN', 'EUN')

INSERT INTO RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_ZZZ_NKDataGrouping)
VALUES ('2FD42F2D-BA9F-4FC3-843C-C6B97676C88D', 'FRG', 'BLA', 'EUN')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','EUN')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique, ZZ1_StartDate, ZZ1_EndDate, ZZ1_CompositeKeyOnZZ5, ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','EUN','Test','T','', 1,'1900-01-01','2079-06-06','A','1900-01-01')

INSERT RefCusTariffAdditionalCodeCategory (ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'A', 'AAA', 'EUN')

INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES ('7558A41B-F5AE-46A0-AB6E-510117207DFF', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, 'AAA', 'A1', 'A', 1, 'EUN')

INSERT INTO RefCusApplicability (ZZT_PK, ZZT_ZY2_AdditionalCode, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup, ZZT_ZZA_SecondTradeGroup)
VALUES (NEWID(), '7558A41B-F5AE-46A0-AB6E-510117207DFF', '2017-11-17 00:00:00', '2079-06-06 23:59:00', '2FD42F2D-BA9F-4FC3-843C-C6B97676C88D', null)

INSERT INTO RefCusApplicability (ZZT_PK, ZZT_ZY2_AdditionalCode, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup, ZZT_ZZA_SecondTradeGroup)
VALUES (NEWID(), '7558A41B-F5AE-46A0-AB6E-510117207DFF', '2017-11-17 00:00:00', '2079-06-06 23:59:00', '2FD42F2D-BA9F-4FC3-843C-C6B97676C88D', '2FD42F2D-BA9F-4FC3-843C-C6B97676C88D')
				");
				var refCusApplicability = new RefCusApplicability();
				TestDBHelper.ExecuteNonQuery(conn, SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusApplicability.TableViewScriptDictionary[1], "V", "RefCusApplicabilityTableView_V1"));
				TestDBHelper.ExecuteNonQuery(conn, SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusApplicability.TableViewScriptDictionary[2], "V", "RefCusApplicabilityTableView_V2"));
				Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicabilityTableView_V1"));
				Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusApplicabilityTableView_V2"));
			}
		}

		void AssertExistRefAccElectronicProcessingFeeVersion516(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("EPF_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("EPF_SystemCode", "varchar", 3, false,""),
				new DbColumn("EPF_Category", "varchar", 3, false, ""),
				new DbColumn("EPF_Code", "varchar", 3, false, ""),
				new DbColumn("EPF_Description", "nvarchar", 250, false, ""),
				new DbColumn("EPF_Currency", "varchar", 3, false, ""),
				new DbColumn("EPF_Price", "decimal", -1, false, "((0))"),
				new DbColumn("EPF_ValidFrom", "smalldatetime", -1, false, "('1900-01-01')"),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefAccElectronicProcessingFee"), "RefAccElectronicProcessingFee existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefAccElectronicProcessingFee", false, expectedColumns), "RefAccElectronicProcessingFee columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefAccElectronicProcessingFeeTableView_V1"), "RefAccElectronicProcessingFeeTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefAccElectronicProcessingFeeTableView_V1", true, expectedColumns), "RefAccElectronicProcessingFeeTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "CK_RefAccElectronicProcessingFee_EPF_SystemCode", "C" },
				{ "CK_RefAccElectronicProcessingFee_EPF_Category", "C" },
				{ "CK_RefAccElectronicProcessingFee_EPF_Code", "C" },
				{ "CK_RefAccElectronicProcessingFee_EPF_Description", "C" },
				{ "CK_RefAccElectronicProcessingFee_EPF_Currency", "C" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");
		}

		void AssertExistRefAccElectronicProcessingFeeVersion520(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("EPF_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("EPF_SystemCode", "varchar", 3, false,""),
				new DbColumn("EPF_Category", "varchar", 3, false, ""),
				new DbColumn("EPF_Code", "varchar", 3, false, ""),
				new DbColumn("EPF_Description", "nvarchar", 250, false, ""),
				new DbColumn("EPF_Currency", "varchar", 3, false, ""),
				new DbColumn("EPF_Price", "decimal", -1, false, "((0))"),
				new DbColumn("EPF_ValidFrom", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("EPF_CountryCode", "char", 2, false, "('')"),
				new DbColumn("EPF_JobDirection", "varchar", 3, false, "('')"),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefAccElectronicProcessingFee"), "RefAccElectronicProcessingFee existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefAccElectronicProcessingFee", false, expectedColumns), "RefAccElectronicProcessingFee columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefAccElectronicProcessingFeeTableView_V2"), "RefAccElectronicProcessingFeeTableView_V2 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefAccElectronicProcessingFeeTableView_V2", true, expectedColumns), "RefAccElectronicProcessingFeeTableView_V2 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "CK_RefAccElectronicProcessingFee_EPF_SystemCode", "C" },
				{ "CK_RefAccElectronicProcessingFee_EPF_Category", "C" },
				{ "CK_RefAccElectronicProcessingFee_EPF_Code", "C" },
				{ "CK_RefAccElectronicProcessingFee_EPF_Description", "C" },
				{ "CK_RefAccElectronicProcessingFee_EPF_Currency", "C" },
				{ "DF_RefAccElectronicProcessingFee_EPF_PK", "D" },
				{ "DF_RefAccElectronicProcessingFee_EPF_Price", "D" },
				{ "DF_RefAccElectronicProcessingFee_EPF_ValidFrom", "D" },
				{ "DF_RefAccElectronicProcessingFee_EPF_CountryCode", "D" },
				{ "DF_RefAccElectronicProcessingFee_EPF_JobDirection", "D" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_CountryCode_JobDirection_Currency_ValidFrom"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefAccElectronicProcessingFee", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusTariffBRCharacteristic(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("ZB1_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("ZB1_CharacteristicType", "varchar", 5, false, "('')"),
				new DbColumn("ZB1_ZZ1_Tariff", "uniqueidentifier", -1, true, ""),
				new DbColumn("ZB1_Style", "varchar", 10, false, "('')"),
				new DbColumn("ZB1_MaxLength", "smallint", -1, false, "((0))"),
				new DbColumn("ZB1_DecimalPlaces", "smallint", -1, false, "((0))"),
				new DbColumn("ZB1_Code", "nvarchar", 35, false, "('')"),
				new DbColumn("ZB1_Text", "nvarchar", 1000, false, "('')"),
				new DbColumn("ZB1_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("ZB1_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("ZB1_IsImport", "bit", -1, false, "((0))"),
				new DbColumn("ZB1_IsExport", "bit", -1, false, "((0))"),
				new DbColumn("ZB1_IsMandatory", "bit", -1, false, "((0))"),
				new DbColumn("ZB1_IsConditioningAttribute", "bit", -1, false, "((0))"),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusTariffBRCharacteristic"), "RefCusTariffBRCharacteristic existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusTariffBRCharacteristic", false, expectedColumns), "RefCusTariffBRCharacteristic columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusTariffBRCharacteristicTableView_V1"), "RefCusTariffBRCharacteristicTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusTariffBRCharacteristicTableView_V1", true, expectedColumns), "RefCusTariffBRCharacteristicTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusTariffBRCharacteristic", "PK" },
				{ "FK_RefCusTariffBRCharacteristic_RefCusTariff", "F" },
				{ "CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType", "C" },
				{ "CK_RefCusTariffBRCharacteristic_ZB1_Style", "C" },
				{ "CK_RefCusTariffBRCharacteristic_ZB1_StartDate_ZB1_EndDate", "C" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");
		}

		void AssertExistRefCusTariffBRCharacteristicVersion365(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("ZB1_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("ZB1_CharacteristicType", "varchar", 5, false, "('')"),
				new DbColumn("ZB1_ZZ1_Tariff", "uniqueidentifier", -1, true, ""),
				new DbColumn("ZB1_Style", "varchar", 10, false, "('')"),
				new DbColumn("ZB1_MaxLength", "smallint", -1, false, "((0))"),
				new DbColumn("ZB1_DecimalPlaces", "smallint", -1, false, "((0))"),
				new DbColumn("ZB1_Code", "nvarchar", 35, false, "('')"),
				new DbColumn("ZB1_Text", "nvarchar", 1000, false, "('')"),
				new DbColumn("ZB1_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("ZB1_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("ZB1_IsImport", "bit", -1, false, "((0))"),
				new DbColumn("ZB1_IsExport", "bit", -1, false, "((0))"),
				new DbColumn("ZB1_IsMandatory", "bit", -1, false, "((0))"),
				new DbColumn("ZB1_IsConditioningAttribute", "bit", -1, false, "((0))"),
				new DbColumn("ZB1_ZZ5_Nomenclature", "uniqueidentifier", -1, true, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusTariffBRCharacteristic"), "RefCusTariffBRCharacteristic existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusTariffBRCharacteristic", false, expectedColumns), "RefCusTariffBRCharacteristic columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusTariffBRCharacteristicTableView_V2"), "RefCusTariffBRCharacteristicTableView_V2 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusTariffBRCharacteristicTableView_V2", true, expectedColumns), "RefCusTariffBRCharacteristicTableView_V2 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusTariffBRCharacteristic", "PK" },
				{ "FK_RefCusTariffBRCharacteristic_RefCusTariff", "F" },
				{ "CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType", "C" },
				{ "CK_RefCusTariffBRCharacteristic_ZB1_Style", "C" },
				{ "CK_RefCusTariffBRCharacteristic_ZB1_StartDate_ZB1_EndDate", "C" },
				{ "FK_RefCusTariffBRCharacteristic_RefCusNomenclatureGroup", "F" },
				{ "CK_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature_ZB1_ZZ1_Tariff", "C" }
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");
		}

		void AssertExistRefCusTariffBRCharacteristicValue(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("ZB2_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("ZB2_ZB1_Characteristic", "uniqueidentifier", -1, false, "('')"),
				new DbColumn("ZB2_Value", "nvarchar", 100, false, "('')"),
				new DbColumn("ZB2_Description", "nvarchar", 500, false, "('')"),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusTariffBRCharacteristicValue"), "RefCusTariffBRCharacteristicValue existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusTariffBRCharacteristicValue", false, expectedColumns), "RefCusTariffBRCharacteristicValue columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusTariffBRCharacteristicValueTableView_V1"), "RefCusTariffBRCharacteristicValueTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusTariffBRCharacteristicValueTableView_V1", true, expectedColumns), "RefCusTariffBRCharacteristicValueTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
				{
					{ "PK_RefCusTariffBRCharacteristicValue", "PK" },
					{ "FK_RefCusTariffBRCharacteristicValue_RefCusTariffBRCharacteristic", "F" },
					{ "CK_RefCusTariffBRCharacteristicValue_ZB2_Value", "C" },
					{ "CK_RefCusTariffBRCharacteristicValue_ZB2_Description", "C" }
				};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");
		}

		void AssertExistRefCusTariffBRCharacteristicAttribute(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("ZB3_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("ZB3_ZB1_Characteristic", "uniqueidentifier", -1, false, "('')"),
				new DbColumn("ZB3_Name", "varchar", 35, false, "('')"),
				new DbColumn("ZB3_Code", "nvarchar", 10, false, "('')"),
				new DbColumn("ZB3_Value", "nvarchar", -1, false, "('')"),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusTariffBRCharacteristicAttribute"), "RefCusTariffBRCharacteristicAttribute existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusTariffBRCharacteristicAttribute", false, expectedColumns), "RefCusTariffBRCharacteristicAttribute columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusTariffBRCharacteristicAttributeTableView_V1"), "RefCusTariffBRCharacteristicAttributeTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusTariffBRCharacteristicAttributeTableView_V1", true, expectedColumns), "RefCusTariffBRCharacteristicAttributeTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
				{
					{ "PK_RefCusTariffBRCharacteristicAttribute", "PK" },
					{ "FK_RefCusTariffBRCharacteristicAttribute_RefCusTariffBRCharacteristic", "F" }
				};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");
		}

		void AssertVersion465Modifications(SqlConnection conn)
		{
			// Restore and Pre-condition Check
			const int version = 465;
			var refCusCodeListAttributeName = new RefCusCodeListAttributeName();
			var indexName =
				"IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption";

			TestDBHelper.ExecuteNonQuery(conn, SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, indexName));
			Assert.False(TestDBHelper.IndexExists(conn, refCusCodeListAttributeName.TableName, indexName));

			// Verify Version 465 Upgrade Script
			TestDBHelper.ExecuteNonQuery(conn, provider.GetUpgradeWrapperByVersion(version).UpgradeScript);
			Assert.True(TestDBHelper.IndexExists(conn, refCusCodeListAttributeName.TableName, indexName));
		}

		void AssertExistRefCusProfileType(SqlConnection refDbConn)
		{
			var expectedTableColumns = new[]
			{
				new DbColumn("XXX_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XXX_ProfileType", "varchar", 10, false, ""),
				new DbColumn("XXX_ZZI_TariffType", "uniqueidentifier", -1, true, ""),
				new DbColumn("XXX_Description", "nvarchar", 500, false, ""),
				new DbColumn("XXX_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileType"), "RefCusProfileType existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileType", false, expectedTableColumns), "RefCusProfileType columns existed with correct type and length and default value");

			var expectedViewColumns = new[]
			{
				new DbColumn("XXX_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XXX_ProfileType", "varchar", 10, false, ""),
				new DbColumn("XXX_ZZI_TariffType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XXX_Description", "nvarchar", 500, false, ""),
				new DbColumn("XXX_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};
			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileTypeTableView_V1"), "RefCusProfileTypeTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileTypeTableView_V1", true, expectedViewColumns), "RefCusProfileTypeTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfileType", "PK" },
				{ "CK_RefCusProfileType_XXX_ProfileType", "C" },
				{ "CK_RefCusProfileType_XXX_Description", "C" },
				{ "FK_RefCusProfileType_RefCusTariffType", "F" },
				{ "FK_RefCusProfileType_RefDataGrouping", "F" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ProfileType_XXX_ZZI_TariffType"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileType", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfile(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XX0_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XX0_XXX_ProfileType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XX0_AppliesToCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_QuestionCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XX0_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XX0_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfile"), "RefCusProfile existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfile", false, expectedColumns), "RefCusProfile columns existed with correct type and length and default value");

			var expectedColumnsV1 = new[]
{
				new DbColumn("XX0_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XX0_XXX_ProfileType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XX0_TariffCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_QuestionCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XX0_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XX0_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};
			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileTableView_V1"), "RefCusProfileTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileTableView_V1", true, expectedColumnsV1), "RefCusProfileTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfile", "PK" },
				{ "FK_RefCusProfile_XX0_XXX_ProfileType", "F" },
				{ "CK_RefCusProfile_XX0_StartDate_XX0_EndDate", "C" },
				{ "FK_RefCusProfile_RefDataGrouping", "F" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfile", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfile_512(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XX0_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XX0_XXX_ProfileType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XX0_AppliesToCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_QuestionCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XX0_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XX0_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfile"), "RefCusProfile existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfile", false, expectedColumns), "RefCusProfile columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfile", "PK" },
				{ "FK_RefCusProfile_XX0_XXX_ProfileType", "F" },
				{ "CK_RefCusProfile_XX0_StartDate_XX0_EndDate", "C" },
				{ "FK_RefCusProfile_RefDataGrouping", "F" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");
		}

		void AssertExistRefCusProfile_547(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XX0_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XX0_XXX_ProfileType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XX0_AppliesToCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_QuestionCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XX0_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XX0_AllowMultipleAnswers", "bit", -1, false, "((0))"),
				new DbColumn("XX0_IsAnswerMandatory", "bit", -1, false, "((0))"),
				new DbColumn("XX0_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfile"), "RefCusProfile existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfile", false, expectedColumns), "RefCusProfile columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileTableView_V3"), "RefCusProfileTableView_V3 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileTableView_V3", true, expectedColumns), "RefCusProfileTableView_V3 columns existed with correct type and length and default value");

			var expectedColumnsV1 = new[]
{
				new DbColumn("XX0_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XX0_XXX_ProfileType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XX0_TariffCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_QuestionCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XX0_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XX0_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};
			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileTableView_V1"), "RefCusProfileTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileTableView_V1", true, expectedColumnsV1), "RefCusProfileTableView_V1 columns existed with correct type and length and default value");

			var expectedColumnsV2 = new[]
			{
				new DbColumn("XX0_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XX0_XXX_ProfileType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XX0_TariffCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_QuestionCode", "varchar", 35, false, "('')"),
				new DbColumn("XX0_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XX0_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XX0_AllowMultipleAnswers", "bit", -1, false, "((0))"),
				new DbColumn("XX0_IsAnswerMandatory", "bit", -1, false, "((0))"),
				new DbColumn("XX0_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};
			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileTableView_V2"), "RefCusProfileTableView_V2 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileTableView_V2", true, expectedColumnsV2), "RefCusProfileTableView_V2 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfile", "PK" },
				{ "FK_RefCusProfile_XX0_XXX_ProfileType", "F" },
				{ "CK_RefCusProfile_XX0_StartDate_XX0_EndDate", "C" },
				{ "FK_RefCusProfile_RefDataGrouping", "F" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfile", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfileAttribute(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XXY_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XXY_XX0_Profile", "uniqueidentifier", -1, false, ""),
				new DbColumn("XXY_Name", "varchar", 35, false, ""),
				new DbColumn("XXY_Value", "nvarchar", -1, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileAttribute"), "RefCusProfileAttribute existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileAttribute", false, expectedColumns), "RefCusProfileAttribute columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileAttributeTableView_V1"), "RefCusProfileAttributeTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileAttributeTableView_V1", true, expectedColumns), "RefCusProfileAttributeTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfileAttribute", "PK" },
				{ "FK_RefCusProfileAttribute_RefCusProfile", "F" },
				{ "CK_RefCusProfileAttribute_XXY_Name", "C" },
				{ "CK_RefCusProfileAttribute_XXY_Value", "C" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileAttribute_XXY_XX0_Profile"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileAttribute", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfileQuestion(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XQ2_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XQ2_XXX_ProfileType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XQ2_QuestionCode", "varchar", 35, false, ""),
				new DbColumn("XQ2_AnswerDataType", "varchar", 35, false, ""),
				new DbColumn("XQ2_AnswerMaxLength", "smallint", -1, false, "((0))"),
				new DbColumn("XQ2_AnswerDecimalPlaces", "smallint", -1, false, "((0))"),
				new DbColumn("XQ2_AnswerMask", "varchar", 50, false, "('')"),
				new DbColumn("XQ2_AllowMultipleAnswers", "bit", -1, false, "((0))"),
				new DbColumn("XQ2_Name", "nvarchar", 200, false, ""),
				new DbColumn("XQ2_Text", "nvarchar", 1000, false, ""),
				new DbColumn("XQ2_Note", "nvarchar", 2000, false, "('')"),
				new DbColumn("XQ2_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XQ2_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XQ2_IsAnswerMandatory", "bit", -1, false, "((0))"),
				new DbColumn("XQ2_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileQuestion"), "RefCusProfileQuestion existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestion", false, expectedColumns), "RefCusProfileQuestion columns existed with correct type and length and default value");

			var expectedColumnsTableView1 = new[]
			{
				new DbColumn("XQ2_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XQ2_XXX_ProfileType", "uniqueidentifier", -1, false, ""),
				new DbColumn("XQ2_Code", "varchar", 35, false, ""),
				new DbColumn("XQ2_AnswerDataType", "varchar", 35, false, ""),
				new DbColumn("XQ2_AnswerMaxLength", "smallint", -1, false, "((0))"),
				new DbColumn("XQ2_AnswerDecimalPlaces", "smallint", -1, false, "((0))"),
				new DbColumn("XQ2_AnswerMask", "varchar", 50, false, "('')"),
				new DbColumn("XQ2_AllowMultipleAnswers", "bit", -1, false, "((0))"),
				new DbColumn("XQ2_Name", "nvarchar", 200, false, ""),
				new DbColumn("XQ2_Text", "nvarchar", 1000, false, ""),
				new DbColumn("XQ2_Note", "nvarchar", 2000, false, "('')"),
				new DbColumn("XQ2_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XQ2_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XQ2_IsAnswerMandatory", "bit", -1, false, "((0))"),
				new DbColumn("XQ2_ZZZ_NKDataGrouping", "varchar", 3, false, ""),
			};

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionTableView_V1"), "RefCusProfileQuestionTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionTableView_V1", true, expectedColumnsTableView1), "RefCusProfileQuestionTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfileQuestion", "PK" },
				{ "FK_RefCusProfileQuestion_XQ2_XXX_ProfileType", "F" },
				{ "CK_RefCusProfileQuestion_XQ2_AnswerDataType", "C" },
				{ "CK_RefCusProfileQuestion_XQ2_AnswerMaxLength", "C" },
				{ "CK_RefCusProfileQuestion_XQ2_AnswerDecimalPlaces", "C" },
				{ "CK_RefCusProfileQuestion_XQ2_Name", "C" },
				{ "CK_RefCusProfileQuestion_XQ2_Text", "C" },
				{ "CK_RefCusProfileQuestion_XQ2_QuestionCode", "C" },
				{ "CK_RefCusProfileQuestion_XQ2_StartDate_XQ2_EndDate", "C" },
				{ "FK_RefCusProfileQuestion_RefDataGrouping", "F" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileQuestion", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfileQuestion_573(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XQ2_QuestionCode", "varchar", 35, false, ""),
			};

			Assert.False(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionTableView_V1"), "RefCusProfileQuestionTableView_V1 existed");
			Assert.False(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionTableView_V2"), "RefCusProfileQuestionTableView_V2 existed");
			Assert.False(TestDBHelper.CheckConstraintExists(refDbConn, "RefCusProfileQuestion", "CK_RefCusProfileQuestion_XQ2_Code"));
			Assert.False(TestDBHelper.IndexExists(refDbConn, "RefCusProfileQuestion", "IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate"));
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestion", false, expectedColumns), "RefCusProfileQuestion columns existed with correct type and length and default value");
		}

		void AssertExistRefCusProfileQuestion_574(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XQ2_QuestionCode", "varchar", 35, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileQuestion"), "RefCusProfileQuestion existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestion", false, expectedColumns), "RefCusProfileQuestion columns existed with correct type and length and default value");

			var expectedColumnsTableView1 = new[]
			{
				new DbColumn("XQ2_Code", "varchar", 35, false, ""),
			};

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionTableView_V1"), "RefCusProfileQuestionTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionTableView_V1", true, expectedColumnsTableView1), "RefCusProfileQuestionTableView_V1 columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionTableView_V2"), "RefCusProfileQuestionTableView_V2 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionTableView_V2", true, expectedColumns), "RefCusProfileQuestionTableView_V2 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "CK_RefCusProfileQuestion_XQ2_QuestionCode", "C" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileQuestion", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfileQuestionAnswerList(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XQ4_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XQ4_XQ2_Question", "uniqueidentifier", -1, false, ""),
				new DbColumn("XQ4_Value", "nvarchar", 100, false, ""),
				new DbColumn("XQ4_Description", "nvarchar", 500, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileQuestionAnswerList"), "RefCusProfileQuestionAnswerList existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionAnswerList", false, expectedColumns), "RefCusProfileQuestionAnswerList columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionAnswerListTableView_V1"), "RefCusProfileQuestionAnswerListTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionAnswerListTableView_V1", true, expectedColumns), "RefCusProfileQuestionAnswerListTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfileQuestionAnswerList", "PK" },
				{ "FK_RefCusProfileQuestionAnswerList_RefCusProfileQuestion", "F" },
				{ "CK_RefCusProfileQuestionAnswerList_XQ4_Value", "C" },
				{ "CK_RefCusProfileQuestionAnswerList_XQ4_Description", "C" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileQuestionAnswerList_XQ4_XQ2_Question_XQ4_Value"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileQuestionAnswerList", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfileQuestionAttribute(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XQ3_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XQ3_XQ2_Question", "uniqueidentifier", -1, false, ""),
				new DbColumn("XQ3_Name", "varchar", 35, false, ""),
				new DbColumn("XQ3_Value", "nvarchar", -1, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileQuestionAttribute"), "RefCusProfileQuestionAttribute existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionAttribute", false, expectedColumns), "RefCusProfileQuestionAttribute columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionAttributeTableView_V1"), "RefCusProfileQuestionAttributeTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionAttributeTableView_V1", true, expectedColumns), "RefCusProfileQuestionAttributeTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfileQuestionAttribute", "PK" },
				{ "FK_RefCusProfileQuestionAttribute_RefCusProfileQuestion", "F" },
				{ "CK_RefCusProfileQuestionAttribute_XQ3_Name", "C" },
				{ "CK_RefCusProfileQuestionAttribute_XQ3_Value", "C" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileQuestionAttribute_XQ3_XQ2_Question_XQ3_Name"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileQuestionAttribute", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfileQuestionLanguage(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XQL_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XQL_XQ2_Question", "uniqueidentifier", -1, false, ""),
				new DbColumn("XQL_Name", "nvarchar", 500, false, ""),
				new DbColumn("XQL_Text", "nvarchar", 1000, false, ""),
				new DbColumn("XQL_Note", "nvarchar", 2000, false, "('')"),
				new DbColumn("XQL_ZX6_NKLanguage", "varchar", 3, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileQuestionLanguage"), "RefCusProfileQuestionLanguage existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionLanguage", false, expectedColumns), "RefCusProfileQuestionLanguage columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionLanguageTableView_V1"), "RefCusProfileQuestionLanguageTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionLanguageTableView_V1", true, expectedColumns), "RefCusProfileQuestionLanguageTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfileQuestionLanguage", "PK" },
				{ "FK_RefCusProfileQuestionLanguage_XQL_XQ2_Question", "F" },
				{ "CK_RefCusProfileQuestionLanguage_XQL_Name", "C" },
				{ "CK_RefCusProfileQuestionLanguage_XQL_Text", "C" },
				{ "FK_RefCusProfileQuestionLanguage_XQL_ZX6_NKLanguage", "F" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileQuestionLanguage_XQL_XQ2_Question_XQL_Name_XQL_ZX6_NKLanguage"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileQuestionLanguage", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfileQuestionAnswerListLanguage(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XAL_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XAL_XQ4_QuestionAnswer", "uniqueidentifier", -1, false, ""),
				new DbColumn("XAL_Description", "nvarchar", 500, false, ""),
				new DbColumn("XAL_ZX6_NKLanguage", "varchar", 3, false, ""),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileQuestionAnswerListLanguage"), "RefCusProfileQuestionAnswerListLanguage existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionAnswerListLanguage", false, expectedColumns), "RefCusProfileQuestionAnswerListLanguage columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionAnswerListLanguageTableView_V1"), "RefCusProfileQuestionAnswerListLanguageTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionAnswerListLanguageTableView_V1", true, expectedColumns), "RefCusProfileQuestionAnswerListLanguageTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfileQuestionAnswerListLanguage", "PK" },
				{ "FK_RefCusProfileQuestionAnswerListLanguage_XAL_XQ4_QuestionAnswer", "F" },
				{ "CK_RefCusProfileQuestionAnswerListLanguage_XAL_Description", "C" },
				{ "FK_RefCusProfileQuestionAnswerListLanguage_XAL_ZX6_NKLanguage", "F" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileQuestionAnswerListLanguage_XAL_XQ4_QuestionAnswer_XAL_ZX6_NKLanguage"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileQuestionAnswerListLanguage", expectedIndexes), $"Should create all indexes");
		}

		void AssertExistRefCusProfileQuestionPathway(SqlConnection refDbConn)
		{
			var expectedColumns = new[]
			{
				new DbColumn("XQP_PK", "uniqueidentifier", -1, false, "(newid())"),
				new DbColumn("XQP_XQ2_QuestionParent", "uniqueidentifier", -1, false, ""),
				new DbColumn("XQP_XQ2_QuestionChild", "uniqueidentifier", -1, false, ""),
				new DbColumn("XQP_Description", "nvarchar", 200, false, "('')"),
				new DbColumn("XQP_StartDate", "smalldatetime", -1, false, "('1900-01-01')"),
				new DbColumn("XQP_EndDate", "smalldatetime", -1, false, "('2079-06-06 23:59')"),
				new DbColumn("XQP_ConditionToProceedFormula", "nvarchar", 500, false, "('')"),
			};

			Assert.True(TestDBHelper.TableExists(refDbConn, "RefCusProfileQuestionPathway"), "RefCusProfileQuestionPathway existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionPathway", false, expectedColumns), "RefCusProfileQuestionPathway columns existed with correct type and length and default value");

			Assert.True(TestDBHelper.ObjectExists(refDbConn, "V", "RefCusProfileQuestionPathwayTableView_V1"), "RefCusProfileQuestionPathwayTableView_V1 existed");
			Assert.True(TestDBHelper.TableColumnsExist(refDbConn, "RefCusProfileQuestionPathwayTableView_V1", true, expectedColumns), "RefCusProfileQuestionPathwayTableView_V1 columns existed with correct type and length and default value");

			var expectedConstraints = new Dictionary<string, string>
			{
				{ "PK_RefCusProfileQuestionPathway", "PK" },
				{ "FK_RefCusProfileQuestionPathway_XQP_XQ2_QuestionParent", "F" },
				{ "FK_RefCusProfileQuestionPathway_XQP_XQ2_QuestionChild", "F" },
				{ "CK_RefCusProfileQuestionPathway_XQP_StartDate_XQP_EndDate", "C" },
			};
			Assert.True(TestDBHelper.ConstraintsExist(refDbConn, expectedConstraints), "Should create all constraints");

			var expectedIndexes = new[]
			{
				"IX_RefCusProfileQuestionPathway_XQP_XQ2_QuestionParent_XQP_XQ2_QuestionChild_XQP_StartDate"
			};
			Assert.True(TestDBHelper.IndexesExist(refDbConn, "RefCusProfileQuestionPathway", expectedIndexes), $"Should create all indexes");
		}

		#region Carrier Messaging Buss

		void AssertExistRefMessagingBussPackageInfo(SqlConnection refDbConn)
		{
			TestDBHelper.AssertSchemaObjects(refDbConn, provider, 420,
				"RefMessagingBussPackageInfo",
				"RefMessagingBussPackageInfoTableView_V1",
				new[]
				{
					new DbColumn("ZMP_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("ZMP_PackageName", "varchar", 200, false, "('')")
				},
				new Dictionary<string, string>
				{
					{ "PK_RefMessagingBussPackageInfo", "PK" },
					{ "DF_RefMessagingBussPackageInfo_ZMP_PK", "D" },
					{ "DF_RefMessagingBussPackageInfo_ZMP_PackageName", "D" }
				},
				new[]
				{
					("CK_RefMessagingBussPackageInfo_ZMP_PackageName", "([ZMP_PackageName]<>'')")
				},
				new[]
				{
					"IX_RefMessagingBussPackageInfo_ZMP_PackageName"
				});
		}

		void AssertExistRefMessagingBussPackageVersion(SqlConnection refDbConn)
		{
			TestDBHelper.AssertSchemaObjects(refDbConn, provider, 421,
				"RefMessagingBussPackageVersion",
				"RefMessagingBussPackageVersionTableView_V1",
				new[]
				{
					new DbColumn("ZMV_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("ZMV_ZMP_PackageInfo", "uniqueidentifier", -1, false, ""),
					new DbColumn("ZMV_Version", "varchar", 50, false, "('')")
				},
				new Dictionary<string, string>
				{
					{ "PK_RefMessagingBussPackageVersion", "PK" },
					{ "FK_RefMessagingBussPackageVersion_RefMessagingBussPackageInfo", "F" },
					{ "DF_RefMessagingBussPackageVersion_ZMV_PK", "D" },
					{ "DF_RefMessagingBussPackageVersion_ZMV_Version", "D" }
				},
				new[]
				{
					("CK_RefMessagingBussPackageVersion_ZMV_Version", "([ZMV_Version]<>'')")
				},
				new[]
				{
					"IX_RefMessagingBussPackageVersion_ZMV_ZMP_PackageInfo_ZMV_Version"
				});
		}

		void AssertExistRefMessagingBussCarrierInfo(SqlConnection refDbConn)
		{
			TestDBHelper.AssertSchemaObjects(refDbConn, provider, 422,
				"RefMessagingBussCarrierInfo",
				"RefMessagingBussCarrierInfoTableView_V1",
				new[]
				{
					new DbColumn("ZMC_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("ZMC_ZMP_PackageInfo", "uniqueidentifier", -1, false, ""),
					new DbColumn("ZMC_CarrierCode", "char", 5, false, "('')"),
					new DbColumn("ZMC_CarrierName", "varchar", 200, false, "('')"),
					new DbColumn("ZMC_CountryCode", "varchar", 2, true, "")
				},
				new Dictionary<string, string>
				{
					{ "PK_RefMessagingBussCarrierInfo", "PK" },
					{ "FK_RefMessagingBussCarrierInfo_RefMessagingBussPackageInfo", "F" },
					{ "DF_RefMessagingBussCarrierInfo_ZMC_PK", "D" },
					{ "DF_RefMessagingBussCarrierInfo_ZMC_CarrierCode", "D" },
					{ "DF_RefMessagingBussCarrierInfo_ZMC_CarrierName", "D" }
				},
				new[]
				{
					("CK_RefMessagingBussCarrierInfo_ZMC_CarrierCode", "(len([ZMC_CarrierCode])=(5))"),
					("CK_RefMessagingBussCarrierInfo_ZMC_CarrierName", "([ZMC_CarrierName]<>'')"),
					("CK_RefMessagingBussCarrierInfo_ZMC_CountryCode", "([ZMC_CountryCode] IS NULL OR len([ZMC_CountryCode])=(2))")
				},
				new[]
				{
					"IX_RefMessagingBussCarrierInfo_ZMC_CarrierCode",
					"IX_RefMessagingBussCarrierInfo_ZMC_ZMP_PackageInfo"
				});
		}

		void AssertExistRefAccessorial(SqlConnection refDbConn)
		{
			TestDBHelper.AssertSchemaObjects(refDbConn, provider, 494,
				"RefAccessorial",
				"RefAccessorialTableView_V1",
				new[]
				{
					new DbColumn("ASI_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("ASI_Code", "char", 3, false, "('')"),
					new DbColumn("ASI_Description", "varchar", 50, false, "('')")
				},
				new Dictionary<string, string>
				{
					{ "PK_RefAccessorial", "PK" }
				},
				new[]
				{
					("CK_RefAccessorial_ASI_Code", "(len([ASI_Code])=(3))"),
					("CK_RefAccessorial_ASI_Description", "([ASI_Description]<>'')")
				},
				new[]
				{
					"IX_RefAccessorial_ASI_Code"
				});
		}

		void AssertExistRefMessagingBussAttributeInfo(SqlConnection refDbConn)
		{
			TestDBHelper.AssertSchemaObjects(refDbConn, provider, 541,
				"RefMessagingBussAttributeInfo",
				"RefMessagingBussAttributeInfoTableView_V1",
				new[]
				{
					new DbColumn("ZAI_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("ZAI_AttributeName", "varchar", 200, false, "('')")
				},
				new Dictionary<string, string>
				{
					{ "PK_RefMessagingBussAttributeInfo", "PK" },
					{ "DF_RefMessagingBussAttributeInfo_ZAI_PK", "D" },
					{ "DF_RefMessagingBussAttributeInfo_ZAI_AttributeName", "D" }
				},
				new[]
				{
					("CK_RefMessagingBussAttributeInfo_ZAI_AttributeName", "([ZAI_AttributeName]<>'')")
				},
				new[]
				{
					"IX_RefMessagingBussAttributeInfo_ZAI_AttributeName"
				});
		}

		void AssertExistRefMessagingBussCarrierInfoAttribute(SqlConnection refDbConn)
		{
			TestDBHelper.AssertSchemaObjects(refDbConn, provider, 542,
				"RefMessagingBussCarrierInfoAttribute",
				"RefMessagingBussCarrierInfoAttributeTableView_V1",
				new[]
				{
					new DbColumn("ZCA_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("ZCA_ZMC_CarrierInfo", "uniqueidentifier", -1, false, ""),
					new DbColumn("ZCA_ZAI_AttributeInfo", "uniqueidentifier", -1, false, ""),
					new DbColumn("ZCA_AttributeValue", "varchar", 200, false, "('')")
				},
				new Dictionary<string, string>
				{
					{ "PK_RefMessagingBussCarrierInfoAttribute", "PK" },
					{ "FK_RefMessagingBussCarrierInfoAttribute_RefMessagingBussAttributeInfo", "F"},
					{ "FK_RefMessagingBussCarrierInfoAttribute_RefMessagingBussCarrierInfo", "F"},
					{ "DF_RefMessagingBussCarrierInfoAttribute_ZCA_AttributeValue", "D"},
					{ "DF_RefMessagingBussCarrierInfoAttribute_ZCA_PK", "D"}
				},
				new[]
				{
					("CK_RefMessagingBussCarrierInfoAttribute_ZCA_AttributeValue", "([ZCA_AttributeValue]<>'')")
				},
				new[]
				{
					"IX_RefMessagingBussCarrierInfoAttribute_ZCA_ZMC_CarrierInfo_ZCA_ZAI_AttributeInfo_ZCA_AttributeValue",
				});
		}

		void AssertExistRefMessagingBussPackageInfoAttribute(SqlConnection refDbConn)
		{
			TestDBHelper.AssertSchemaObjects(refDbConn, provider, 543,
				"RefMessagingBussPackageInfoAttribute",
				"RefMessagingBussPackageInfoAttributeTableView_V1",
				new[]
				{
					new DbColumn("ZPA_PK", "uniqueidentifier", -1, false, "(newid())"),
					new DbColumn("ZPA_ZMP_PackageInfo", "uniqueidentifier", -1, false, ""),
					new DbColumn("ZPA_ZAI_AttributeInfo", "uniqueidentifier", -1, false, ""),
					new DbColumn("ZPA_AttributeValue", "varchar", 200, false, "('')")
				},
				new Dictionary<string, string>
				{
					{ "PK_RefMessagingBussPackageInfoAttribute", "PK" },
					{ "FK_RefMessagingBussPackageInfoAttribute_RefMessagingBussAttributeInfo", "F"},
					{ "FK_RefMessagingBussPackageInfoAttribute_RefMessagingBussPackageInfo", "F"},
					{ "DF_RefMessagingBussPackageInfoAttribute_ZPA_PK", "D"},
					{ "DF_RefMessagingBussPackageInfoAttribute_ZPA_AttributeValue", "D"}
				},
				new[]
				{
					("CK_RefMessagingBussPackageInfoAttribute_ZPA_AttributeValue", "([ZPA_AttributeValue]<>'')")
				},
				new[]
				{
					"IX_RefMessagingBussPackageInfoAttribute_ZPA_ZMP_PackageInfo_ZPA_ZAI_AttributeInfo_ZPA_AttributeValue",
				});
		}

		#endregion

		[Test]
		public void TestUpgradeScript_AddNotNullColumnsScriptShouldSpecifyDefault()
		{
			foreach (var version in provider.GetAvailableVersionsAfterVersion(1))
			{
				var t = provider.GetUpgradeWrapperByVersion(version).UpgradeScript;

				foreach (var line in t.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
				{
					var res = new Regex(@"(?>ALTER TABLE.*?ADD\s+(?!CONSTRAINT))(?>(?>([^,]*?(?=NOT\s+NULL)[^,]+)|(?>[^,]*?(?!NOT)[^,]+))+(?>[,]?\s*)?)+", RegexOptions.IgnoreCase).Match(line);
					if (res.Success)
					{
						var invalidColumnDefinitions = res.Groups[1].Captures.Cast<Capture>().Where(s => s.Value.IndexOf("DEFAULT", StringComparison.OrdinalIgnoreCase) < 0);
						Assert.IsEmpty(invalidColumnDefinitions, $"Version {version}: {line}\r\n\tALTER TABLE only allows columns to be added that can contain nulls, or have a DEFAULT definition specified");
					}
				}
			}
		}
		UpgradeScriptProvider provider = new UpgradeScriptProvider();
	}
}
