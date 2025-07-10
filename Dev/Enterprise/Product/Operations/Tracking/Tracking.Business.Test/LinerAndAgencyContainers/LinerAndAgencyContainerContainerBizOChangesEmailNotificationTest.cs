using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyContainer))]
	sealed class LinerAndAgencyContainerContainerBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<LinerAndAgencyContainer>
	{
		protected override LinerAndAgencyContainer GetNewBizOForNotification()
		{
			var linerAndAgencyContainer = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			linerAndAgencyContainer.HasChanges = true;
			return linerAndAgencyContainer;
		}
	}
}
