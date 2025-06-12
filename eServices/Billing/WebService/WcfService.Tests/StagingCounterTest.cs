using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.eServices.Billing.DataAccess;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingTransaction;
using Castle.Windsor;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class StagingCounterTest
	{
		[SetUp]
		public void Setup()
		{
			configurationProviderMock = new Mock<IConfigurationProvider>();
			configurationProviderMock.Setup(x => x.CacheTimeInMinutesOfStagingCounter).Returns(5);
			billingRepositoryMock = new Mock<IBillingRepository>();
		}

		[Test]
		public void TestConstructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => new StagingCounter(null, configurationProviderMock.Object));
			Assert.AreEqual("repository", ex.ParamName);
		}

		[Test]
		public void TestConstructor_ShouldThrowArgumentNullException_WhenConfigurationProviderIsNull()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => new StagingCounter(billingRepositoryMock.Object, null));
			Assert.AreEqual("configurationProvider", ex.ParamName);
		}

		[Test]
		public void TestCount_ShouldReturnCachedValue_IfWithinCacheTime()
		{
			billingRepositoryMock.Setup(r => r.CountStaging()).Returns(23);

			var stagingCounter = new StagingCounter(billingRepositoryMock.Object, configurationProviderMock.Object);
			var result1 = stagingCounter.Count;
			var result2 = stagingCounter.Count;

			Assert.AreEqual(23, result1);
			Assert.AreEqual(23, result2);
			billingRepositoryMock.Verify(r => r.CountStaging(), Times.Once);
		}

		[Test]
		public void TestCount_ShouldRefresh_WhenCacheTimeExceeded()
		{
			billingRepositoryMock.SetupSequence(r => r.CountStaging())
						  .Returns(23)
						  .Returns(56);

			var stagingCounterMock = new Mock<StagingCounter>(billingRepositoryMock.Object, configurationProviderMock.Object);
			var now = DateTime.Now;
			stagingCounterMock.SetupSequence(_ => _.Now())
							.Returns(now)
							.Returns(now.AddMinutes(4))
							.Returns(now.AddMinutes(6));

			var result1 = stagingCounterMock.Object.Count;
			var result2 = stagingCounterMock.Object.Count;
			var result3 = stagingCounterMock.Object.Count;

			Assert.AreEqual(23, result1);
			Assert.AreEqual(23, result2);
			Assert.AreEqual(56, result3);
			billingRepositoryMock.Verify(r => r.CountStaging(), Times.Exactly(2));
		}

		[Test]
		public void TestCount_ShouldBeThreadSafe()
		{
			var repositoryMock = new Mock<IBillingRepository>();
			repositoryMock.Setup(r => r.CountStaging()).Returns(23);

			var stagingCounter = new StagingCounter(repositoryMock.Object, configurationProviderMock.Object);
			var size = 10;
			int[] results = new int[size];

			Parallel.For(0, size, i =>
			{
				results[i] = stagingCounter.Count;
			});


			var firstResult = results[0];
			Assert.AreEqual(23, firstResult);
			Assert.AreEqual(size, results.Count());
			Assert.AreEqual(1, results.Distinct().Count(), "All items should have the same value.");
			repositoryMock.Verify(r => r.CountStaging(), Times.Once);
		}

		[Test]
		public void TestSingleton_ShouldBeThreadSafe()
		{
			var containerMock = new Mock<IWindsorContainer>();
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(configurationProviderMock.Object);
			Global.WindsorContainer = containerMock.Object;

			var size = 10;
			StagingCounter[] results = new StagingCounter[size];

			Parallel.For(0, size, i =>
			{
				results[i] = StagingCounter.Singleton;
			});

			var firstResult = results[0];
			Assert.IsInstanceOf<StagingCounter>(firstResult);
			Assert.AreSame(firstResult, results[1]);
			Assert.AreEqual(size, results.Count());
			Assert.AreEqual(1, results.Distinct().Count(), "All items should have the same value.");
		}

		private Mock<IConfigurationProvider> configurationProviderMock;
		private Mock<IBillingRepository> billingRepositoryMock;
	}
}
