using System;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Validation;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	public class NoEffectiveDateRangeCheckerFixture
	{
		[Test]
		public void ValidateNewestObjectFromSafeDb_NoEffectiveDateRange()
		{
			var cond1 = new RefCusCondition() { ZX1_StartDate = new DateTime(2010, 1, 1), ZX1_EndDate = new DateTime(2011, 1, 1), ZX1_Source = "A" };
			var app = new RefCusApplicability() { ZZT_StartDate = new DateTime(2000, 1, 1), ZZT_EndDate = new DateTime(2001, 1, 1) };
			var mockRepo = new Mock<ISafeRepository>().Object;
			var condApp = new RefCusConditionApplicability(cond1, app, mockRepo);

			//incorrect record.
			Assert.That(() => new NoEffectiveDateRangeChecker().IsValid(condApp),
				Throws.TypeOf<RefDataProcessingException>().With.Message.Contain("Incorrect Safe Date Range: no effective date range for the following safeObject."));
		}
	}
}
