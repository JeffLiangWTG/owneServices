using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.T4Runner.Test
{
	[TestFixture]
	public class TTFileRunnerFixture
	{
		[Test]
		public void IsExecuting()
		{
			_iTTFileConfigMock.Setup(o => o.CallProcessor("CargoWise.RefDbRepo.T4Runner.Test.T4TestResources", It.IsAny<string>())).Verifiable();
			TTFileRunner.Run(new[] { _iTTFileConfigMock.Object });
			_iTTFileConfigMock.VerifyAll();
		}

		[Test]
		public void ThrowingExceptionFromProcessor()
		{
			_iTTFileConfigMock.Setup(o => o.CallProcessor("CargoWise.RefDbRepo.T4Runner.Test.T4TestResources", It.IsAny<string>())).Throws(new Exception("error"));
			Assert.Throws<Exception>(() => TTFileRunner.Run(new[] { _iTTFileConfigMock.Object }));
		}

		Mock<ITTFileConfig> _iTTFileConfigMock;

		[SetUp]
		public void SetUp()
		{
			var directory = DirectoryLookup.First();
			var ttFile = TTSearch.GetTTFiles(directory).First();

			_iTTFileConfigMock = new Mock<ITTFileConfig>();

			_iTTFileConfigMock.SetupGet(x => x.NamespaceName).Returns("CargoWise.RefDbRepo.T4Runner.Test.T4TestResources");
			_iTTFileConfigMock.SetupGet(x => x.FileName).Returns(ttFile);
		}

		IEnumerable<string> DirectoryLookup => new string[] { Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "T4TestResources") };
	}
}
