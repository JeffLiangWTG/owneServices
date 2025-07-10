using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WorkflowModuleFilterQueryBuilderTest : TestCaseWithFactory
	{
		#region TestBuildQuery

		public void TestBuildQuery()
		{
			var subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
			subQuery.AddToFilter(ProcessTasksSchema.P9_Condition1, SQLComparisonOperator.Equal, "blah");

			var bizoType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var adoText = WorkflowModuleFilterQueryBuilder.BuildQuery(bizoType, subQuery, null).LiteralTextADO;
			AssertEquals("Correct column containing workflow parent", true, adoText.StartsWith("JS_PK"));

			bizoType = ObjectFactory.GetType<Freight.Integration.QuotedBooking.IViewQuotedBooking>();
			adoText = WorkflowModuleFilterQueryBuilder.BuildQuery(bizoType, subQuery, null).LiteralTextADO;
			AssertEquals("Correct column containing workflow parent", true, adoText.StartsWith("VB_PK"));
		}

		#endregion

		#region TestBuildQueryWithFieldNameOverride

		public void TestBuildQueryWithFieldNameOverride()
		{
			var milestonQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
			milestonQuery.AddToFilter(ProcessTasksSchema.P9_Condition1, SQLComparisonOperator.Equal, "abc");

			var relatedSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				var adoText = WorkflowModuleFilterQueryBuilder.BuildQuery(typeof(JobDocAddress), milestonQuery, new[] { relatedSubQuery }, OrgAddressSchema.OA_OH).LiteralTextADO;
				AssertEquals("E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH IN (SELECT P9_ParentID FROM dbo.ProcessTasks WHERE P9_ParentID IS NOT NULL AND P9_Condition1 = 'abc'))", adoText);
			}
		}

		#endregion
	}
}
