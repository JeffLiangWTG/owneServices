using System.Xml;
using Enterprise.Environment;
using Enterprise.Semaphores.Common;
using Enterprise.Semaphores.Common.Testing;
using Enterprise.ZArchitecture.Web.GlobalBase;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.Warehouse.Web
{
	public class GlobalTest : ZGlobalTest
	{
		protected override string WebConfigPath
		{
			get { return GetSupplementaryContentPath("Enterprise", "Product", "Operations", "Warehouse", "Web", "Warehouse.Web", "Web.config"); }
		}

		protected override int NumberOfLocations
		{
			get { return 0; }
		}

		public override void AssertLocations(XmlNodeList locations)
		{
		}

		public void TestSemaphore()
		{
			var global1 = new Global();
			var global2 = new Global();
			var global3 = new Global();
			var global4 = new Global();

			using (var provider1 = global1.WebEnvProvider_Exposed)
			using (var provider2 = global2.WebEnvProvider_Exposed)
			using (var provider3 = global3.WebEnvProvider_Exposed)
			using (var provider4 = global4.WebEnvProvider_Exposed)
			{
				Assert(provider1 is WebEnvProvider);
				Assert(provider1.Instance is WebEnvironment);
				Assert(provider2 is WebEnvProvider);
				Assert(provider2.Instance is WebEnvironment);
				Assert(provider3 is WebEnvProvider);
				Assert(provider3.Instance is WebEnvironment);
				Assert(provider4 is WebEnvProvider);
				Assert(provider4.Instance is WebEnvironment);

				var environment1 = provider1.Instance as IEnvironment;
				var environment2 = provider2.Instance as IEnvironment;
				var environment3 = provider3.Instance as IEnvironment;
				var environment4 = provider4.Instance as IEnvironment;

				AssertNotNull(environment1);
				Assert(environment1.SemaphoreProvider is WebSemaphoreProvider);
				AssertNotNull(environment2);
				Assert(environment2.SemaphoreProvider is WebSemaphoreProvider);
				AssertNotNull(environment3);
				Assert(environment3.SemaphoreProvider is WebSemaphoreProvider);
				AssertNotNull(environment4);
				Assert(environment4.SemaphoreProvider is WebSemaphoreProvider);

				var semaphoreProvider1 = environment1.SemaphoreProvider;
				var semaphoreProvider2 = environment2.SemaphoreProvider;
				var semaphoreProvider3 = environment3.SemaphoreProvider;
				var semaphoreProvider4 = environment4.SemaphoreProvider;

				var semaphoreType = new SemaphoreForTesting();

				using (var handle1 = semaphoreProvider1.CreateSemaphoreHandle(semaphoreType))
				using (var handle2 = semaphoreProvider2.CreateSemaphoreHandle(semaphoreType))
				using (var handle3 = semaphoreProvider3.CreateSemaphoreHandle(semaphoreType))
				using (var handle4 = semaphoreProvider4.CreateSemaphoreHandle(semaphoreType))
				{
					Assert("handle1", handle1.Success);
					Assert("handle2", handle2.Success);
					Assert("handle3", handle3.Success);
					Assert("handle4", !handle4.Success);
					Assert("SqlException should be thrown", handle4.CreateException.InnerException is SqlException);
					Assert(
						"Exception should be wrapped as a SemaphoreReachedMaxAllowedHandlesException but was " +
						handle4.CreateException.GetType().FullName,
						handle4.CreateException is SemaphoreReachedMaxAllowedHandlesException);
				}
			}
		}
	}
}
