using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using Moq;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common.TestClasses
{
	internal class MessageHandlerForTest : MessageHandler
	{
		public TestLogger Logger { get; set; }
		public Dictionary<Guid, string> StatusUpdates = new Dictionary<Guid, string>();

		public MessageHandlerForTest(string inputFolder, string inputFile, TestLogger logger, SupportedMessageTypes messageType) : this(SetupMockRepo(), inputFolder, inputFile, logger, messageType) { }
		public MessageHandlerForTest(IStagingRepository stagingRepo, SupportedMessageTypes messageType) : this(stagingRepo, string.Empty, string.Empty, messageType) { }
		public MessageHandlerForTest(IStagingRepository stagingRepo, string inputFolder, string inputFile, SupportedMessageTypes messageType) : this(stagingRepo, inputFolder, inputFile, CreateLogger(), messageType) { }
		public MessageHandlerForTest(IStagingRepository stagingRepo, string inputFolder, SupportedMessageTypes messageType) : this(stagingRepo, inputFolder, string.Empty, CreateLogger(), messageType) { }
		public MessageHandlerForTest(IStagingRepository stagingRepo, string inputFolder, string inputFile, TestLogger logger, SupportedMessageTypes messageType) : base(stagingRepo, inputFolder, inputFile, logger, messageType)
		{
			Logger = logger;
		}

		static TestLogger CreateLogger() => new TestLogger();

		protected override void UpdateStatus(SourceDataMessage prodatMsg, string newStatus)
		{
			StatusUpdates[prodatMsg.ID] = newStatus;

			base.UpdateStatus(prodatMsg, newStatus);
		}

		public string GetNewStatus_Exposed(bool success) => GetNewStatus(success);

		public string GetDestinationFilename_Exposed(string folder, string filename) => GetDestinationFilename(folder, filename);

		public bool IsValidFile_Exposed(string filename) => IsValidFile(filename);

		public bool IsValidInputFolder_Exposed(string folder) => IsValidInputFolder(folder);

		static IStagingRepository SetupMockRepo()
		{
			var mockRepo = new Mock<IStagingRepository>();

			mockRepo.Setup(x => x.Get<SourceData>()).Returns(MockSourceData.AsQueryable);
			mockRepo.Setup(x => x.SaveChanges()).Returns(1);
			mockRepo.Setup(x => x.Dispose()).Callback(() => { });

			return mockRepo.Object;
		}

		static SourceData[] MockSourceData => new SourceData[]
		{
			new SourceData { SDA_PK = new Guid("12300000000000000000000000000001"), SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Valid_01.txt") },
			new SourceData { SDA_PK = new Guid("12300000000000000000000000000002"), SDA_CreatedTime = new DateTime(2022, 3, 3, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 2, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Invalid.txt") },
			new SourceData { SDA_PK = new Guid("22300000000000000000000000000003"), SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_Gesmes, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates.TestFiles.Input.Gesmes_Valid_01.txt") },
			new SourceData { SDA_PK = new Guid("22300000000000000000000000000004"), SDA_CreatedTime = new DateTime(2022, 3, 3, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 2, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_Gesmes, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates.TestFiles.Input.Gesmes_Invalid.txt") },
		};
	}
}
