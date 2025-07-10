using System.Linq;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class UpdaterDependencyProviderTest : TestCase
	{
		public void TestGetAllRoots()
		{
			var sqlBuilder = new SqlServerSQLBuilder();
			provider = new UpdaterDependencyProvider<IDataSetUpdater>(undgSubstanceGroup, mainDbHelper.GetReferencedForeignKeys().ToArray());
			provider.Initialize();
			AssertContainsExactElementsInAnyOrder(
				new IDataSetUpdater[] { undgCommonDataUpdater, undgSubstanceUpdater },
				provider.GetAllRoots()
			);
		}

		public void TestGetAllLeaves()
		{
			var sqlBuilder = new SqlServerSQLBuilder();
			provider = new UpdaterDependencyProvider<IDataSetUpdater>(undgSubstanceGroup, mainDbHelper.GetReferencedForeignKeys().ToArray());
			provider.Initialize();
			AssertContainsExactElementsInAnyOrder(
				new IDataSetUpdater[] { undgSubstanceUpdater, undgCommonDataUpdater },
				provider.GetAllLeaves()
			);
		}

		public void TestGetParents()
		{
			var sqlBuilder = new SqlServerSQLBuilder();
			provider = new UpdaterDependencyProvider<IDataSetUpdater>(undgSubstanceGroup, mainDbHelper.GetReferencedForeignKeys().ToArray());
			provider.Initialize();
			AssertEquals(0, provider.GetParents(undgCommonDataUpdater).Count());
			AssertEquals(0, provider.GetParents(undgSubstanceUpdater).Count());
		}

		public void TestGetChildren()
		{
			var sqlBuilder = new SqlServerSQLBuilder();
			provider = new UpdaterDependencyProvider<IDataSetUpdater>(undgSubstanceGroup, mainDbHelper.GetReferencedForeignKeys().ToArray());
			provider.Initialize();
			AssertEquals(0, provider.GetChildren(undgSubstanceUpdater).Count());
			AssertEquals(0, provider.GetChildren(undgCommonDataUpdater).Count());
		}

		#region Helpers

		protected override void SetUp()
		{
			base.SetUp();

			var mainDbConnection = Db.NewAdminConnection();
			var mainDbAdoConnection = ((IDbConnectionInternals)mainDbConnection).ADOConnection;
			mainDbHelper = new DBHelper(mainDbAdoConnection);

			undgSubstanceGroup = new IDataSetUpdater[] {
				undgCommonDataUpdater = new OneTableDataSetUpdater<UNDGCommonData, IUNDGCommonData>(proxy, mainDbHelper, versionControlManager),
				undgSubstanceUpdater = new UNDGSubstanceUpdater(proxy, mainDbHelper, versionControlManager)
			};
		}

		UpdaterDependencyProvider<IDataSetUpdater> provider;
		IDBHelper mainDbHelper;
		readonly IServerProxy proxy = new Mock<IServerProxy>().Object;
		readonly IRefVersionControlManager versionControlManager = new Mock<IRefVersionControlManager>().Object;

		IDataSetUpdater[] undgSubstanceGroup;

		IDataSetUpdater undgCommonDataUpdater, undgSubstanceUpdater;

		#endregion
	}
}
