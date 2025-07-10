using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FunctionalitySuspenderTest : TestCase
	{
		public void TestSuspenderWithoutAction()
		{
			var hostSuspender = new FunctionalitySuspender();
			var wrappedSuspender = GetWrappedSuspender(hostSuspender);
			AssertEquals("IsSuspended by default", false, hostSuspender.IsSuspended);

			using (wrappedSuspender.GetSuspender())
			{
				AssertEquals("IsSuspended after GetSuspender called", true, hostSuspender.IsSuspended);

				using (wrappedSuspender.GetSuspender())
				{
					AssertEquals("IsSuspended after GetSuspender called the second time", true, hostSuspender.IsSuspended);
				}
				AssertEquals("IsSuspended after Dispose called", true, hostSuspender.IsSuspended);
			}
			AssertEquals("IsSuspended after Dispose called the second time", false, hostSuspender.IsSuspended);
		}

		public void TestSuspenderWithAction()
		{
			int someNumber = 32;
			var hostSuspender = new FunctionalitySuspender(() => someNumber /= 2);
			var wrappedSuspender = GetWrappedSuspender(hostSuspender);
			AssertEquals("IsSuspended by default.", false, hostSuspender.IsSuspended);

			using (wrappedSuspender.GetSuspender())
			{
				AssertEquals("IsSuspended after GetSuspender called.", true, hostSuspender.IsSuspended);

				using (wrappedSuspender.GetSuspender())
				{
					AssertEquals("IsSuspended after GetSuspender called the second time.", true, hostSuspender.IsSuspended);
					AssertEquals("Resume action shouldn't be called.", 32, someNumber);
				}
				AssertEquals("IsSuspended after Dispose called.", true, hostSuspender.IsSuspended);
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 32, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspender.IsSuspended);
			AssertEquals("Resume action should be called after resuming a functionality.", 16, someNumber);
		}

		public void TestSuspenderWithSuspendAction()
		{
			int someNumber = 100;
			var hostSuspender = new FunctionalitySuspender(() => someNumber /= 2, false, () => someNumber++);
			var wrappedSuspender = GetWrappedSuspender(hostSuspender);
			AssertEquals("IsSuspended by default.", false, hostSuspender.IsSuspended);

			using (wrappedSuspender.GetSuspender())
			{
				AssertEquals("Suspend action should be called.", 101, someNumber);
				AssertEquals("IsSuspended after GetSuspender called.", true, hostSuspender.IsSuspended);

				using (wrappedSuspender.GetSuspender())
				{
					AssertEquals("Suspend action shouldn't be called.", 101, someNumber);
					AssertEquals("IsSuspended after GetSuspender called the second time.", true, hostSuspender.IsSuspended);
					AssertEquals("Resume action shouldn't be called.", 101, someNumber);
				}
				AssertEquals("IsSuspended after Dispose called.", true, hostSuspender.IsSuspended);
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 101, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspender.IsSuspended);
			AssertEquals("Resume action should be called after resuming a functionality.", 50, someNumber);
		}

		public void TestSuspenderWithActionAndDoActionOnlyIfRequested()
		{
			int someNumber = 32;
			var hostSuspenderWithActionOnlyIfRequested = new FunctionalitySuspender(() => someNumber /= 2, true);
			var wrappedSuspender = GetWrappedSuspender(hostSuspenderWithActionOnlyIfRequested);
			AssertEquals("IsSuspended by default.", false, hostSuspenderWithActionOnlyIfRequested.IsSuspended);

			using (wrappedSuspender.GetSuspender())
			{
				using (wrappedSuspender.GetSuspender())
				{
					AssertEquals("Resume action shouldn't be called.", 32, someNumber);
				}
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 32, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspenderWithActionOnlyIfRequested.IsSuspended);
			AssertEquals("Resume action shouldn't be called as functionality was not requested as it was suspended.", 32, someNumber);

			using (wrappedSuspender.GetSuspender())
			{
				using (wrappedSuspender.GetSuspender())
				{
					AssertEquals("Resume action shouldn't be called.", 32, someNumber);
				}
				AssertEquals("IsSuspended call makes an Action requested.", true, hostSuspenderWithActionOnlyIfRequested.IsSuspended);
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 32, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspenderWithActionOnlyIfRequested.IsSuspended);
			AssertEquals("Resume action should be called as functionality was requested as it was suspended.", 16, someNumber);

			using (wrappedSuspender.GetSuspender())
			{
				using (wrappedSuspender.GetSuspender())
				{
					AssertEquals("IsSuspended call makes an Action requested.", true, hostSuspenderWithActionOnlyIfRequested.IsSuspended);
					AssertEquals("Resume action shouldn't be called.", 16, someNumber);
				}
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 16, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspenderWithActionOnlyIfRequested.IsSuspended);
			AssertEquals("Resume action should be called as functionality was requested as it was suspended.", 8, someNumber);

			using (wrappedSuspender.GetSuspender())
			{
				AssertEquals("IsSuspended call makes an Action requested.", true, hostSuspenderWithActionOnlyIfRequested.IsSuspended);
				using (wrappedSuspender.GetSuspender())
				{
					AssertEquals("Resume action shouldn't be called.", 8, someNumber);
				}
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 8, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspenderWithActionOnlyIfRequested.IsSuspended);
			AssertEquals("Resume action should be called as functionality was requested as it was suspended.", 4, someNumber);

			using (wrappedSuspender.GetSuspender())
			{
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 4, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspenderWithActionOnlyIfRequested.IsSuspended);
			AssertEquals("Resume action shouldn't be called as functionality was not requested as it was suspended.", 4, someNumber);

			var hostSuspenderWithActionAlways = new FunctionalitySuspender(() => someNumber /= 2, false);
			using (hostSuspenderWithActionAlways.GetSuspender())
			{
				using (hostSuspenderWithActionOnlyIfRequested.GetSuspender())
				{
					AssertEquals("Resume action shouldn't be called.", 4, someNumber);
				}
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 4, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspenderWithActionAlways.IsSuspended);
			AssertEquals("Resume action should be called as functionality is not suspended and even it not requesed.", 2, someNumber);

			using (hostSuspenderWithActionAlways.GetSuspender())
			{
				using (wrappedSuspender.GetSuspender())
				{
					AssertEquals("IsSuspended call makes an Action requested.", true, hostSuspenderWithActionAlways.IsSuspended);
					AssertEquals("Resume action shouldn't be called.", 2, someNumber);
				}
				AssertEquals("Resume action shouldn't be called if a functionality is still suspended.", 2, someNumber);
			}
			AssertEquals("IsSuspended after Dispose called the second time.", false, hostSuspenderWithActionAlways.IsSuspended);
			AssertEquals("Resume action should be called as functionality is not suspended.", 1, someNumber);
		}

		#region SuspendCollection

		public void TestSuspendCollection()
		{
			List<SuspenderParent> suspenderParents = new List<SuspenderParent>
				{
					new SuspenderParent(),
					new SuspenderParent(),
					new SuspenderParent()
				};

			Action<bool> assertIsSuspended = isSuspended =>
			{
				foreach (var suspenderParent in suspenderParents)
				{
					AssertEquals("IsSuspended", isSuspended, suspenderParent.Suspender.IsSuspended);
				}
			};

			assertIsSuspended(false);
			var suspenders = FunctionalitySuspender.SuspendCollection(suspenderParents, parent => parent.Suspender.GetSuspender());
			assertIsSuspended(true);
			FunctionalitySuspender.ResumeCollection(suspenders);
			assertIsSuspended(false);
		}

		class SuspenderParent
		{
			public FunctionalitySuspender Suspender
			{
				get { return suspender ?? (suspender = new FunctionalitySuspender()); }
			}
			FunctionalitySuspender suspender;
		}

		#endregion

		public FunctionalitySuspender hostSuspenderWithActionAlways { get; set; }

		protected virtual IFunctionalitySuspender GetWrappedSuspender(FunctionalitySuspender suspenderToWrap)
		{
			return suspenderToWrap;
		}
	}
}
