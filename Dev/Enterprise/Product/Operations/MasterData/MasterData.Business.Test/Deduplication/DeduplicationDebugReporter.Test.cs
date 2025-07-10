using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(DeduplicationDebugReporter))]
	class DeDuplicationDebugReporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDetailsText()
		{
			var details = "Scoring Result: ConfidenceRating: Low";
			var reporter = new DeduplicationDebugReporter(details);

			AssertEquals("Scoring Result: ConfidenceRating: Low", reporter.DetailsText);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeduplicationDebugReporter(ZString.Empty);
		}
		#endregion
	}
}
