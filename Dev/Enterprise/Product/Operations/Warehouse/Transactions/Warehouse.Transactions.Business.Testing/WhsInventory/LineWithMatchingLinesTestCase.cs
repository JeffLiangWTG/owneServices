using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestsSubclassesOf(typeof(ILineWithMatchingLines<>))]
	abstract class LineWithMatchingLinesTestCase<T> : LineWithCommittedPickLinesTestCase
			where T : BusinessObject, ILineWithMatchingLines<T>
	{
		#region TestMatchingLines

		public void TestMatchingLines()
		{
			var line = Factory.New<T>();
			AssertNotNull(line.MatchingLines);

			var matchingLine = Factory.New<T>();
			matchingLine.MatchingLinePK = line.PK;
			AssertContainsExactElementsInAnyOrder(new[] { matchingLine }, line.MatchingLines);
		}

		#endregion
	}
}
