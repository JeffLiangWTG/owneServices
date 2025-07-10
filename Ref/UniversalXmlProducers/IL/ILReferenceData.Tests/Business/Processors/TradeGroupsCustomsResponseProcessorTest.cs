using System;
using System.IO;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using Moq;
using NUnit.Framework;
using IDateTimeProvider = CargoWise.RefDbRepo.Common.Utils.IDateTimeProvider;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	public class TradeGroupsCustomsResponseProcessorTest : CustomsResponseProcessorTestBase<TradeGroupsCustomsResponseProcessor>
	{
		protected override bool ExpectedSupportDataSetProcessing => true;

		protected override bool ExpectedSupportTableDataProcessing => false;

		protected override string OutputFolderPath => ApplicationConfig.Instance.DownloadsDirectory;

		protected override string ExpectedOutputFileName => $"{base.ExpectedOutputFileName}_20250511045010";

		protected override string DataSource => "TradeGroupsResponse";

		protected override DateTime PublicationDateForTest => DateTime.Now;

		protected override string TableNameForTest => "Test";

		protected override TradeGroupsCustomsResponseProcessor CreateProcessor() => new TradeGroupsCustomsResponseProcessor(dateTimeProvider, new Logger());

		[SetUp]
		public void SetUp()
		{
			var mock = new Mock<IDateTimeProvider>();
			mock.Setup(i => i.GetUTCNow()).Returns(new DateTime(2025, 05, 11, 04, 50, 10));
			dateTimeProvider = mock.Object;

			string tempFolderName = Path.GetRandomFileName();
			string tempFolderPath = Path.Combine(Path.GetTempPath(), tempFolderName);
			ApplicationConfig.Instance.DownloadsDirectory = tempFolderPath;
		}

		[TearDown]
		public void TearDown()
		{
			string tempFolderPath = ApplicationConfig.Instance.DownloadsDirectory;
			if (Directory.Exists(tempFolderPath))
			{
				Directory.Delete(tempFolderPath, true);
			}
		}

		IDateTimeProvider dateTimeProvider;
	}
}
