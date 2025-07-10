using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class UCCImportCodeListProcessManagerTest
	{
		[Test]
		public void TestGetXlsFile()
		{
			var assembly = Assembly.GetExecutingAssembly();

			var codeListProcessManager = new Mock<UCCImportCodeListProcessManager>();
			codeListProcessManager.Setup(x => x.contentFolder).Returns(Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\Import\TestFiles\Input\"));

			var xlsfile = codeListProcessManager.Object.GetXlsFile();

			Assert.IsNotNull(xlsfile);
		}

		[Test]
		public void TestRunProducers()
		{
			var errorCollector = new StringBuilder();
			var processManager = new UCCImportCodeListProcessManager(errorCollector);

			processManager.RunProducers();

			Assert.That(errorCollector.ToString().Replace("\r\n", ""), Is.EqualTo("No XLS or XLSX file was found in folder " + Path.Combine(ApplicationConfig.ServiceDir, "CodeListData")));
		}
	}
}
