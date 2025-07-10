using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	sealed class ECCNCodeListProcessManagerTest
	{
		[Test]
		public void TestGetXlsFile()
		{
			var assembly = Assembly.GetExecutingAssembly();

			var codeListProcessManager = new Mock<ECCNCodeListProcessManager>();
			codeListProcessManager.Setup(x => x.contentFolder).Returns(Path.Combine(Path.GetDirectoryName(assembly.Location), @"\TestFiles\ECCNCodes\Input\"));

			var xlsfile = codeListProcessManager.Object.GetXlsFile();

			Assert.IsNotNull(xlsfile);
		}

		[Test]
		public void TestRunBuilder()
		{
			var errorCollector = new StringBuilder();
			var processManager = new ECCNCodeListProcessManager(errorCollector);

			processManager.RunBuilder();

			Assert.That(errorCollector.ToString().Replace("\r\n", ""), Is.EqualTo("No XLS or XLSX file was found in folder " + Path.Combine(ApplicationConfig.ServiceDir, "ECCNCodeListData")));
		}
	}
}
