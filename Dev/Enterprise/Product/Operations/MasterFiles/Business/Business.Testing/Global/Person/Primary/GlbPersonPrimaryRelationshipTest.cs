using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPersonPrimaryRelationship))]
	sealed class GlbPersonPrimaryRelationshipTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = person.PK;
			var primaryRelationship = Factory.New<GlbPersonPrimaryRelationship>();
			primaryRelationship.PPR_PER = person.PK;
			primaryRelationship.Primary = staff;

			return primaryRelationship;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion

		public void TestPPR_PrimaryId_ShouldUseIndex()
		{
			var bizObj = GetNewBusinessObjectForDeleteTest(Factory);
			Factory.Save();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var sql = $"SELECT TOP 1 PPR_PrimaryId FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{ZGuid.NewZGuid().ToGuid()}'";
				using (var reader = TestConnection.Command(sql).ExecuteReader())
				{ while (reader.Read()) { } }

				var plans = TestConnection.ExecutedCommandsAndQueryPlans.Single(t => t.Item1.IndexOf("PPR_PrimaryId", StringComparison.CurrentCultureIgnoreCase) != -1);
				var planalyzer = new QueryPlanalyzer(plans.Item2.Single());
				AssertEquals(false, planalyzer.TableScans.Any());
				AssertEquals(true, planalyzer.IndexSeeks.Any(x => x.IndexName == "NR_RX__PPR_PrimaryId"));
			}
		}
	}
}
