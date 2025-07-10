using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class CARMGSTCodeProducerTest : TariffProducerTest
	{
		Mock<IWebServiceCaller> callerMock;
		string WorkingDirectory;

		protected override string FunctionCode => Constants.ProgramFunctions.CAGSTCode;

		protected override string NothingNewPublishedMessage => $"CARM GST Codes: Nothing new published since last process. Skip processing this time.";

		[Test]
		public void TestFolderClearedFinally()
		{
			var checker = new PreProcessChecker(FunctionCode);
			checker.MarkAsProcessRequired();
			Directory.CreateDirectory(WorkingDirectory);
			var testFile = TestHelper.CreateTempFile(WorkingDirectory, "Test.txt", "TEST");
			Assert.IsTrue(File.Exists(testFile));
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.IsFalse(File.Exists(testFile));
			Assert.IsFalse(Directory.Exists(WorkingDirectory));
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			WorkingDirectory = Path.Combine(Path.GetTempPath(), FunctionCode);

		}

		protected override IProducer CreateNewProducer()
		{
			return new CARMGSTCodeProducer(callerMock.Object, LogBuilder);
		}

		protected override void CreateDownloaderMockData()
		{
			callerMock = new Mock<IWebServiceCaller>();
			callerMock.Setup(x => x.GetLatestUpdateOnDate(CARMAPIQueryTypes.GSTCodesQueryType)).Returns(new System.DateTime(2024, 12, 17));
			callerMock.Setup(x => x.QueryAndDownloadXmlFiles(WorkingDirectory, "", new string[] { }, new string[] { }, null, null)).Verifiable();
		}
	}
}
