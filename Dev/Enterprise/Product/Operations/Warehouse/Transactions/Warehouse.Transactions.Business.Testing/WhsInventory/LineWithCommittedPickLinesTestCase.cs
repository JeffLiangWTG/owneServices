using System;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestsSubclassesOf(typeof(ILineWithCommittedPickLines))]
	abstract class LineWithCommittedPickLinesTestCase : LineWithInventoryTestCase
	{
		#region TestCollectionsNotNull

		protected override void TestCollectionsNotNullCore()
		{
			base.TestCollectionsNotNullCore();

			var line = (ILineWithCommittedPickLines)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			AssertNotNull(line.PickLines);
		}

		#endregion

		#region TestParentDocketPK_MatchesParentDocket

		public void TestParentDocketPK_MatchesParentDocket()
		{
			var docket = (WhsDocket)Factory.New(DocketType);
			var line = (ILineWithCommittedPickLines)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			line.ParentDocketPK = docket.PK;
			AssertEquals(docket, line.ParentDocket);
		}

		protected abstract Type DocketType { get; }

		#endregion

		#region TestPropertiesAreNotNull

		public void TestPropertiesAreNotNull()
		{
			var line = (ILineWithCommittedPickLines)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			AssertEquals("Noun should return a value.", false, string.IsNullOrWhiteSpace(line.Noun));
			AssertEquals("Verb should return a value.", false, string.IsNullOrWhiteSpace(line.Verb));
		}

		#endregion
	}
}
