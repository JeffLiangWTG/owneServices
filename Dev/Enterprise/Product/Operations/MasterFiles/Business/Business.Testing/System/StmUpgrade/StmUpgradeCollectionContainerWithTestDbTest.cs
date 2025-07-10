using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmUpgradeCollectionContainerWithTestDbTest : TestCase
	{
		public void TestIsCmrManualTestFileUpdateAllowed()
		{
			const string testSharedDb = "CW-RefDb-TestIsCmrManualTestFileUpdateAllowed";
			const string testSharedAGDb = "CW-AG-RefDb-TestIsCmrManualTestFileUpdateAllowed";

			using (var adminConn = Db.NewAdminConnection())
			{
				AssertEquals("IsCmrManualTestFileUpdateAllowed?", true, NewStmUpgradeContainer(adminConn).IsCmrManualTestFileUpdateAllowed);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConn, testSharedDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminConn, testSharedAGDb, Db.DatabaseName);
					adminConn.BeginTransaction();

					(adminConn as IPhysicalRefDbLocation).ClearRefDbNameBuffers();
					RecreateCmrSynonymsToSharedDbForTesting(adminConn, testSharedDb);
					AssertEquals("IsCmrManualTestFileUpdateAllowed?", false, NewStmUpgradeContainer(adminConn).IsCmrManualTestFileUpdateAllowed);

					(adminConn as IPhysicalRefDbLocation).ClearRefDbNameBuffers();
					RecreateCmrSynonymsToSharedDbForTesting(adminConn, testSharedAGDb);
					AssertEquals("IsCmrManualTestFileUpdateAllowed?", false, NewStmUpgradeContainer(adminConn).IsCmrManualTestFileUpdateAllowed);
				}
				finally
				{
					adminConn.RollbackTransaction();
					(adminConn as IPhysicalRefDbLocation).ClearRefDbNameBuffers();
					AdoTestUtils.DropDbIfExists(adminConn, testSharedDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminConn, testSharedAGDb, Db.DatabaseName);
				}

				AssertEquals("IsCmrManualTestFileUpdateAllowed?", true, NewStmUpgradeContainer(adminConn).IsCmrManualTestFileUpdateAllowed);
			}
		}

		void RecreateCmrSynonymsToSharedDbForTesting(DbConnection conn, string sharedDbName)
		{
			string sqlText = String.Format(@"
				DECLARE @Cmd nvarchar(max) = '';
				SELECT
					@Cmd = @Cmd + 'DROP SYNONYM [' + name + '];'
					FROM sys.synonyms WHERE name like 'RefDbCmrAU%'
				EXEC (@Cmd);
				CREATE SYNONYM RefDbCmrAU_NonexistingTable FOR [{0}]..NonexistingTable;",
				sharedDbName);
			conn.ExecuteNonQuery(sqlText);
			(conn as IPhysicalRefDbLocation).ClearRefDbNameBuffers();
		}

		StmUpgradeCollectionContainer NewStmUpgradeContainer(DbConnection testConnection)
		{
			var factory = new BusinessObjectFactory(testConnection);
			return new StmUpgradeCollectionContainer(factory);
		}
	}
}
