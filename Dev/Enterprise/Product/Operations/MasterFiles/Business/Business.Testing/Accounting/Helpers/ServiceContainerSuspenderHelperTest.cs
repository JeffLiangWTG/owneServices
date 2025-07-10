using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ServiceContainerSuspenderHelperTest : TestCaseWithFactory
	{
		public void TestDocManagerInfoNewFactorySuspend()
		{
			AssertEquals(false, ServiceContainerSuspenderHelper.IsDocManagerInfoNewFactorySuspended(Factory));
			using (ServiceContainerSuspenderHelper.GetDocManagerInfoNewFactorySuspender(Factory))
			{
				using (ServiceContainerSuspenderHelper.GetDocManagerInfoNewFactorySuspender(Factory))
				{
					AssertEquals(true, ServiceContainerSuspenderHelper.IsDocManagerInfoNewFactorySuspended(Factory));
				}
				AssertEquals(true, ServiceContainerSuspenderHelper.IsDocManagerInfoNewFactorySuspended(Factory));
			}
			AssertEquals(false, ServiceContainerSuspenderHelper.IsDocManagerInfoNewFactorySuspended(Factory));
		}

		public void TestApplyRevenueRecognitionDateSuspenderService()
		{
			AssertEquals("Precondition: IsApplyRevenueRecognitionDateSuspended should be false", false, ServiceContainerSuspenderHelper.IsApplyRevenueRecognitionDateSuspended(Factory));
			using (ServiceContainerSuspenderHelper.GetApplyRevenueRecognitionDateSuspender(Factory))
			{
				using (ServiceContainerSuspenderHelper.GetApplyRevenueRecognitionDateSuspender(Factory))
				{
					AssertEquals("Inside the using clause, IsApplyRevenueRecognitionDateSuspended should be true", true, ServiceContainerSuspenderHelper.IsApplyRevenueRecognitionDateSuspended(Factory));
				}
				AssertEquals("Inside the using clause, IsApplyRevenueRecognitionDateSuspended should be true", true, ServiceContainerSuspenderHelper.IsApplyRevenueRecognitionDateSuspended(Factory));
			}
			AssertEquals("Postcondition: IsApplyRevenueRecognitionDateSuspended should be false", false, ServiceContainerSuspenderHelper.IsApplyRevenueRecognitionDateSuspended(Factory));
		}

		public void TestGUIRelatedActionsSuspend()
		{
			AssertEquals("Precondition: GUIRelatedActionsSuspend.IsSuspended should be false", false, ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.IsSuspended(Factory));
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.GetSuspender(Factory))
			{
				using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.GetSuspender(Factory))
				{
					AssertEquals("Inside the using clause, GUIRelatedActionsSuspend.IsSuspended should be true", true, ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.IsSuspended(Factory));
				}
				AssertEquals("Inside the using clause, GUIRelatedActionsSuspend.IsSuspended should be true", true, ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.IsSuspended(Factory));
			}
			AssertEquals("Postcondition: GUIRelatedActionsSuspend.IsSuspended should be false", false, ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.IsSuspended(Factory));
		}

		public void TestPostDateCriticalValidationSuspenderService()
		{
			AssertEquals("Precondition: PostDateCriticalValidationSuspenderService.IsSuspended should be false", false, ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.IsSuspended(Factory));
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
			{
				using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
				{
					AssertEquals("Inside the using clause, PostDateCriticalValidationSuspenderService.IsSuspended should be true", true, ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.IsSuspended(Factory));
				}
				AssertEquals("Inside the using clause, PostDateCriticalValidationSuspenderService.IsSuspended should be true", true, ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.IsSuspended(Factory));
			}
			AssertEquals("Postcondition: PostDateCriticalValidationSuspenderService.IsSuspended should be false", false, ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.IsSuspended(Factory));
		}

		public void TestAccTransactionHeaderWithLinesCriticalValidationStaticActivatorService()
		{
			AssertEquals("Precondition: AccTransactionHeaderWithLinesCriticalValidationActivatorService.IsActivated should be false", false, ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.IsActivated);
			using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
			{
				using (ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.GetActivator())
				{
					AssertEquals("Inside the using clause, AccTransactionHeaderWithLinesCriticalValidationActivatorService.IsActivated should be true", true, ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.IsActivated);
				}
				AssertEquals("Inside the using clause, AccTransactionHeaderWithLinesCriticalValidationActivatorService.IsActivated should be true", true, ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.IsActivated);
			}
			AssertEquals("Postcondition: AccTransactionHeaderWithLinesCriticalValidationActivatorService.IsActivated should be false", false, ServiceContainerSuspenderHelper.AccTransactionHeaderWithLinesCriticalValidationActivatorService.Instance.IsActivated);
		}
	}
}
