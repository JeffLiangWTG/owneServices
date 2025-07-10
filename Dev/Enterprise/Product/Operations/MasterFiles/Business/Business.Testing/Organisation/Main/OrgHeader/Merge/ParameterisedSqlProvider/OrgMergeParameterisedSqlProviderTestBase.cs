using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(OrgMergeParameterisedSqlProvider))]
	public abstract class OrgMergeParameterisedSqlProviderTestBase : TestCaseWithFactory
	{
		public abstract string ExpectedFullSqlText { get; }

		public abstract OrgMergeParameterisedSqlProvider Provider { get; }

		protected void RunProviderSql(OrgHeader oldOrg, OrgHeader newOrg)
		{
			using (var cmd = TestConnection.Command(Provider.FullSqlText))
			{
				cmd.AddParameter(OrgMergeParameterisedSqlProvider.OldPkParameter, SqlDbType.UniqueIdentifier, oldOrg.PK.ToGuid());
				cmd.AddParameter(OrgMergeParameterisedSqlProvider.NewPkParameter, SqlDbType.UniqueIdentifier, newOrg.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		public void TestActionIsNotNull()
		{
			AssertNotNull(Provider);
		}

		public void TestDescriptionIsNotNull()
		{
			var firstLine = Provider.FullSqlText.Split(new[] { '\r', '\n' }).FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertNotNull("Please provide some description for this sql.", firstLine);
				Assert("Please enter at least 10 characters", firstLine.Substring(3).TrimEnd().Length > 10);
			});
		}

		public void TestFullSqlText()
		{
			AssertEquals(ExpectedFullSqlText, Provider.FullSqlText);
		}
	}
}
