using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ZA.ServiceTasks.Testing
{
	[TestedType(typeof(TotalLiabilityAmountUpdaterService))]
	sealed class TotalLiabilityAmountUpdaterServiceTest : ServiceTaskTestCase<TotalLiabilityAmountUpdaterService>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
