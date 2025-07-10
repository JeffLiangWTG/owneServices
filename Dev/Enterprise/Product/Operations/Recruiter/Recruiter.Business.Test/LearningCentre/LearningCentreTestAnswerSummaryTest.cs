using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreTestAnswerSummary))]
	sealed class LearningCentreTestAnswerSummaryTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LearningCentreTestAnswerSummary();
		}

		#endregion
	}
}
