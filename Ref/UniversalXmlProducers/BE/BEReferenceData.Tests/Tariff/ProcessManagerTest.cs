using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class ProcessManagerTest
	{
		[Test]
		public void GetFileManager()
		{
			var processManager = new ProcessManagerForTest(new IProcessor[] { }, new ILoader[] { });
			var fileManager = processManager.GetFileManagerExposed();
			Assert.That(fileManager.GetType(), Is.EqualTo(typeof(Services.WebDriverFileManager)));
		}

		[Test]
		public void GetProcessors()
		{
			var processManager = new ProcessManagerForTest(new IProcessor[] { }, new ILoader[] { });
			var processors = processManager.GetProcessorsExposed();
			Assert.That(processors.Length, Is.EqualTo(0));

			var p1 = new Mock<IProcessor>();
			var p2 = new Mock<IProcessor>();

			processManager = new ProcessManagerForTest(new IProcessor[] { p1.Object, p2.Object }, new ILoader[] { });
			processors = processManager.GetProcessorsExposed();
			Assert.That(processors.Length, Is.EqualTo(2));
			Assert.That(processors[0], Is.SameAs(p1.Object));
		}

		[Test]
		public void GetLoaders()
		{
			var processManager = new ProcessManagerForTest(new IProcessor[] { }, new ILoader[] { });
			var loaders = processManager.GetLoadersExposed();
			Assert.That(loaders.Length, Is.EqualTo(0));

			var l1 = new Mock<ILoader>();
			var l2 = new Mock<ILoader>();

			processManager = new ProcessManagerForTest(new IProcessor[] { }, new ILoader[] { l1.Object, l2.Object });
			loaders = processManager.GetLoadersExposed();
			Assert.That(loaders.Length, Is.EqualTo(2));
			Assert.That(loaders[0], Is.SameAs(l1.Object));
		}

		[Test]
		public void SourceDataProvider()
		{
			var processManager = new ProcessManagerForTest(new IProcessor[] { }, new ILoader[] { });
			Assert.That(processManager.SourceDataProvider, Is.EqualTo("TARBEL"));
		}

		[Test]
		public void Team()
		{
			var processManager = new ProcessManagerForTest(new IProcessor[] { }, new ILoader[] { });
			Assert.That(processManager.Team, Is.EqualTo("CBE"));
		}
	}
}

