using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common.TestClasses;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common
{
	[TestFixture]
	class MessageHandlerTests
	{
		[Test]
		public void GetMessages_Empty()
		{
			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, SupportedMessageTypes.Prodat))
			{
				SetupMockRepo(mockRepository, new SourceData[] { }.AsQueryable());

				var results = handler.GetMessages().ToList();
				Assert.That(results, Is.Not.Null.And.Empty);
			}
		}

		[Test]
		public void GetMessages_Ordered()
		{
			SetupMockRepo(mockRepository, new SourceData[]
			{
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Msg 1" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 3, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 2, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Msg 2" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 3, 15, 16, 17), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Msg 3" },
			}.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);

			CreateFiles(inputFolder);

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, SupportedMessageTypes.Prodat))
			{
				var results = handler.GetMessages().ToList();
				Assert.That(results, Is.Not.Null);
				Assert.That(results.Count, Is.EqualTo(4));
				Assert.That(results[0].Content, Is.EqualTo("Msg 2"));
				Assert.That(results[1].Content, Is.EqualTo("Msg 1"));
				Assert.That(results[2].Content, Is.EqualTo("Msg 3"));
				Assert.That(results[3].Filename, Contains.Substring("ValidProdat.txt"));
			}
		}

		[Test]
		public void GetMessages_WithValidInputFile()
		{
			SetupMockRepo(mockRepository, new SourceData[]{}.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);
			var inputFileFolder = Path.Combine(tempInputFile, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFileFolder);
			var inputFile = Path.Combine(inputFileFolder, "ValidInputFile.txt");

			CreateFiles(inputFolder);
			CreateFile(inputFile, "UNB+UNOB:4+SARSINF+COMPUCLEARING::EEEEEEEEEEEEEEBB:COMAS2+20150702:1502+8++PRODAT+++COMPUCLEARING'UNH+1+PRODAT:D:96B:UN:ZZZ01'BGM+6+456+4'");

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, inputFile, SupportedMessageTypes.Prodat))
			{
				var results = handler.GetMessages().ToList();
				Assert.That(results, Is.Not.Null);
				Assert.That(results.Count, Is.EqualTo(2));
				Assert.That(results[0].Filename, Contains.Substring("ValidInputFile.txt"));
				Assert.That(results[1].Filename, Contains.Substring("ValidProdat.txt"));
			}
		}

		[Test]
		public void GetMessages_WithInvalidInputFile()
		{
			SetupMockRepo(mockRepository, new SourceData[] { }.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);
			var inputFileFolder = Path.Combine(tempInputFile, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFileFolder);
			var inputFile = Path.Combine(inputFileFolder, "ValidInputFile.txt");

			CreateFiles(inputFolder);
			CreateFile(inputFile, "Not a valid file");

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, inputFile, SupportedMessageTypes.Prodat))
			{
				var results = handler.GetMessages().ToList();
				Assert.That(results, Is.Not.Null);
				Assert.That(results.Count, Is.EqualTo(1));
				Assert.That(results[0].Filename, Contains.Substring("ValidProdat.txt"));
			}
		}

		[Test]
		public void GetMessages_WithDuplicatedInputFile()
		{
			SetupMockRepo(mockRepository, new SourceData[] { }.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);
			var inputFile = Path.Combine(inputFolder, "ValidProdat.txt");

			CreateFiles(inputFolder);

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, inputFile, SupportedMessageTypes.Prodat))
			{
				var results = handler.GetMessages().ToList();
				Assert.That(results, Is.Not.Null);
				Assert.That(results.Count, Is.EqualTo(1));
				Assert.That(results[0].Filename, Contains.Substring("ValidProdat.txt"));
			}
		}

		[Test]
		public void RecalculateCreatedDate()
		{
			var dt = new DateTime(2023, 11, 20, 13, 14, 15);
			var dt1 = dt;
			var dt2 = dt.AddMilliseconds(2);
			var dt3 = dt.AddSeconds(1).AddMilliseconds(3);
			var dt4 = dt.AddSeconds(5).AddMilliseconds(3);

			SetupMockRepo(mockRepository, new SourceData[]
			{
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = dt1, SDA_SourceTime = dt, SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Msg 1" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = dt2, SDA_SourceTime = dt, SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Msg 2" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = dt3, SDA_SourceTime = dt, SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Msg 3" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = dt4, SDA_SourceTime = dt, SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Msg 4" },
			}.AsQueryable());

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, SupportedMessageTypes.Prodat))
			{
				var results = handler.GetMessages().ToList();
				Assert.That(results, Is.Not.Null);
				Assert.That(results.Count, Is.EqualTo(4));
				Assert.That(results[0].CreatedDate, Is.EqualTo(dt));
				Assert.That(results[1].CreatedDate, Is.EqualTo(dt.AddSeconds(1)));
				Assert.That(results[2].CreatedDate, Is.EqualTo(dt.AddSeconds(2)));
				Assert.That(results[3].CreatedDate, Is.EqualTo(dt.AddSeconds(6)));
			}
		}

		[TestCase(SupportedMessageTypes.Prodat, "ValidProdat.txt", "ValidProdat")]
		[TestCase(SupportedMessageTypes.Gesmes, "ValidGesmes.txt", "ValidGesmes")]
		public void GetMessages_Filtered(SupportedMessageTypes messageType, string expectedFile, string expectedContentText)
		{
			SetupMockRepo(mockRepository, new SourceData[]
			{
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "ValidProdat" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 3, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 2, 1, 12, 13, 14), SDA_Source = "XXX", SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Incorrect Source" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = "XXX", SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Incorrect Content Type" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = "XXX", SDA_ContentText = "Incorrect Status" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_Gesmes, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "ValidGesmes" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 3, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 2, 1, 12, 13, 14), SDA_Source = "XXX", SDA_ContentType = DataSourceConstants.ContentType.ZA_Gesmes, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Incorrect Source" },
				new SourceData { SDA_PK = Guid.NewGuid(), SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_Gesmes, SDA_Status = "XXX", SDA_ContentText = "Incorrect Status" },
			}.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);

			CreateFiles(inputFolder);

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, messageType))
			{
				var results = handler.GetMessages().ToList();
				Assert.That(results, Is.Not.Null);
				Assert.That(results.Count, Is.EqualTo(2));
				Assert.That(results[0].Content, Is.EqualTo(expectedContentText));
				Assert.That(results[1].Filename, Contains.Substring(expectedFile));
			}
		}

		[Test]
		public void GetMessages_Exception()
		{
			mockRepository.Setup(x => x.Get<SourceData>()).Throws(new Exception("The Safe Repo you have dialled is not available at present"));

			using (var handler = new MessageHandlerForTest(mockRepository.Object, SupportedMessageTypes.Prodat))
			{
				var results = ((IMessageHandler)handler).GetMessages().ToList();
				Assert.That(results, Is.Not.Null.And.Empty);
				Assert.That(handler.Logger.ErrorString, Is.EqualTo("Failed to retrieve messages from SourceData: The Safe Repo you have dialled is not available at present\r\n"));
			}
		}

		[Test]
		public void MessageContent()
		{
			var expectedGuid = Guid.NewGuid();
			SetupMockRepo(mockRepository, new SourceData[]
			{
				new SourceData { SDA_PK = expectedGuid, SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_SourceTime = new DateTime(2022, 3, 1, 12, 13, 14), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Prodat Message Content" },
				new SourceData { SDA_PK = expectedGuid, SDA_CreatedTime = new DateTime(2022, 3, 2, 13, 14, 15), SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "NoSourceTime" },
			}.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);

			CreateFiles(inputFolder);

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, SupportedMessageTypes.Prodat))
			{
				var results = handler.GetMessages().ToList();
				Assert.That(results, Is.Not.Null);
				Assert.That(results.Count, Is.EqualTo(3));
				var msg = results[0];

				Assert.That(msg.ID, Is.EqualTo(expectedGuid));
				Assert.That(msg.Content, Is.EqualTo("Prodat Message Content"));
				Assert.That(msg.CreatedDate, Is.EqualTo(new DateTime(2022, 3, 2, 13, 14, 15)));
				Assert.That(msg.PublishDate, Is.EqualTo(new DateTime(2022, 3, 1, 12, 13, 14)));
				Assert.That(msg.Status, Is.EqualTo("QUE"));
				Assert.That(msg.Filename, Is.EqualTo(string.Empty));

				Assert.That(results[1].PublishDate, Is.EqualTo(new DateTime(2022, 3, 2, 13, 14, 15)));

				Assert.That(results[2].ID, Is.EqualTo(Guid.Empty));
				Assert.That(results[2].Content, Contains.Substring("UNB+UNOB:4+SARSINF"));
				Assert.That(results[2].CreatedDate, Is.EqualTo(new DateTime(2022, 6, 6, 6, 6, 6)));
				Assert.That(results[2].PublishDate, Is.EqualTo(new DateTime(2022, 5, 28, 13, 14, 15)));
				Assert.That(results[2].Status, Is.EqualTo("QUE"));
				Assert.That(results[2].Filename, Contains.Substring("ValidProdat.txt"));
			}
		}

		[Test]
		public void NewMessageStatus()
		{
			using (var msgHandler = new MessageHandlerForTest(mockRepository.Object, SupportedMessageTypes.Prodat))
			{
				var result = msgHandler.GetNewStatus_Exposed(true);
				Assert.That(result, Is.EqualTo(StatusProvider.GetMERStatus()));

				result = msgHandler.GetNewStatus_Exposed(false);
				Assert.That(result, Is.EqualTo(StatusProvider.GetERRStatus()));
			}
		}

		[Test]
		public void UpdateMessageStatus_ValidPK()
		{
			var expectedGuid = Guid.NewGuid();

			SetupMockRepo(mockRepository, new SourceData[]
			{
				new SourceData { SDA_PK = expectedGuid, SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Prodat Message Content" },
			}.AsQueryable());

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, SupportedMessageTypes.Prodat))
			{
				var msg = new SourceDataMessage { ID = expectedGuid };
				Assert.DoesNotThrow(() => handler.UpdateStatus(msg, true));
			}
		}

		[Test]
		public void UpdateMessageStatus_InvalidPK()
		{
			var expectedGuid = Guid.NewGuid();
			var randomGuid = Guid.NewGuid();
			SetupMockRepo(mockRepository, new SourceData[]
			{
				new SourceData { SDA_PK = expectedGuid, SDA_Source = DataSourceConstants.Source.eHubZACustomsRepositoryQueue, SDA_ContentType = DataSourceConstants.ContentType.ZA_ProDat, SDA_Status = StatusProvider.GetQUEStatus(), SDA_ContentText = "Prodat Message Content" },
			}.AsQueryable());

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, SupportedMessageTypes.Prodat))
			{
				var msg = new SourceDataMessage { ID = randomGuid };

				Assert.That(expectedGuid, Is.Not.EqualTo(randomGuid));
				var ex = Assert.Throws<ReferenceDataException>(() => handler.UpdateStatus(msg, true));
				Assert.That(ex.Message, Is.EqualTo($"Could not find message in SourceData with SDA_PK = '{randomGuid}' to update the status to 'MER'."));
			}
		}

		[TestCase(SupportedMessageTypes.Prodat, "UNB+UNOB:UNH+1+PRODAT:D:96B")]
		[TestCase(SupportedMessageTypes.Gesmes, "UNB+UNOB:UNH+1+GESMES:D:96B")]
		public void UpdateMessageStatus_FileMovedToProcessed(SupportedMessageTypes messageType, string msgContent)
		{
			SetupMockRepo(mockRepository, new SourceData[] { }.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);
			var inputFileFolder = Path.Combine(tempInputFile, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFileFolder);
			var inputFile = Path.Combine(inputFileFolder, "ValidInputFile.txt");

			CreateFiles(inputFolder);
			CreateFile(inputFile, msgContent);

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, inputFile, messageType))
			{
				var msgs = handler.GetMessages().ToList();

				Assert.That(msgs.Count, Is.EqualTo(2));

				Assert.That(File.Exists(inputFile), Is.EqualTo(true));
				Assert.That(File.Exists(Path.Combine(inputFolder, msgs[1].Filename)), Is.EqualTo(true));

				handler.UpdateStatus(msgs[0], true);
				handler.UpdateStatus(msgs[1], true);

				Assert.That(File.Exists(inputFile), Is.EqualTo(false));
				Assert.That(File.Exists(Path.Combine(inputFolder, "Processed", msgs[0].Filename)), Is.EqualTo(true));
				Assert.That(File.Exists(Path.Combine(inputFolder, msgs[1].Filename)), Is.EqualTo(false));
				Assert.That(File.Exists(Path.Combine(inputFolder, "Processed", msgs[1].Filename)), Is.EqualTo(true));
			}
		}

		[TestCase(SupportedMessageTypes.Prodat, "UNB+UNOB:UNH+1+PRODAT:D:96B")]
		[TestCase(SupportedMessageTypes.Gesmes, "UNB+UNOB:UNH+1+GESMES:D:96B")]
		public void UpdateMessageStatus_FileMovedToFailed(SupportedMessageTypes messageType, string msgContent)
		{
			SetupMockRepo(mockRepository, new SourceData[] { }.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);
			var inputFileFolder = Path.Combine(tempInputFile, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFileFolder);
			var inputFile = Path.Combine(inputFileFolder, "ValidInputFile.txt");

			CreateFiles(inputFolder);
			CreateFile(inputFile, msgContent);

			using (IMessageHandler handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, inputFile, messageType))
			{
				var msgs = handler.GetMessages().ToList();

				Assert.That(msgs.Count, Is.EqualTo(2));

				Assert.That(File.Exists(inputFile), Is.EqualTo(true));
				Assert.That(File.Exists(Path.Combine(inputFolder, msgs[1].Filename)), Is.EqualTo(true));

				handler.UpdateStatus(msgs[0], false);
				handler.UpdateStatus(msgs[1], false);

				Assert.That(File.Exists(inputFile), Is.EqualTo(false));
				Assert.That(File.Exists(Path.Combine(inputFolder, "Failed", msgs[0].Filename)), Is.EqualTo(true));
				Assert.That(File.Exists(Path.Combine(inputFolder, msgs[1].Filename)), Is.EqualTo(false));
				Assert.That(File.Exists(Path.Combine(inputFolder, "Failed", msgs[1].Filename)), Is.EqualTo(true));
			}
		}

		[Test]
		public void IncrementFilenameForDuplicates()
		{
			SetupMockRepo(mockRepository, new SourceData[] { }.AsQueryable());

			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);

			CreateFiles(inputFolder);

			using (var handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, SupportedMessageTypes.Prodat))
			{
				var filename = "TestFile.txt";
				var filename2 = "TestFile[1].txt";
				var newFile = handler.GetDestinationFilename_Exposed(inputFolder, filename);
				Assert.That(newFile, Is.EqualTo(Path.Combine(inputFolder, filename)));

				File.WriteAllText(Path.Combine(inputFolder, filename), "TestFile1");

				newFile = handler.GetDestinationFilename_Exposed(inputFolder, filename);
				Assert.That(newFile, Is.EqualTo(Path.Combine(inputFolder, filename2)));
			}
		}

		[Test]
		public void IsValidInputFolder()
		{
			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			using (var handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, SupportedMessageTypes.Prodat))
			{
				var isValid = handler.IsValidInputFolder_Exposed(inputFolder);
				Assert.That(isValid, Is.EqualTo(false));

				Directory.CreateDirectory(inputFolder);

				isValid = handler.IsValidInputFolder_Exposed(inputFolder);
				Assert.That(isValid, Is.EqualTo(true));
			}
		}

		[TestCase(SupportedMessageTypes.Prodat)]
		[TestCase(SupportedMessageTypes.Gesmes)]
		public void IsValidFile_Prodat(SupportedMessageTypes messageType)
		{
			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			using (var handler = new MessageHandlerForTest(mockRepository.Object, inputFolder, messageType))
			{
				Directory.CreateDirectory(inputFolder);

				CreateFiles(inputFolder);

				var isProdatFile = handler.IsValidFile_Exposed(Path.Combine(inputFolder, $"Valid{messageType}.txt"));
				Assert.That(isProdatFile, Is.EqualTo(true));

				isProdatFile = handler.IsValidFile_Exposed(Path.Combine(inputFolder, "NotAValidFile.txt"));
				Assert.That(isProdatFile, Is.EqualTo(false));
			}
		}

		[TestCase(SupportedMessageTypes.Prodat)]
		[TestCase(SupportedMessageTypes.Gesmes)]
		public void InformationLogging(SupportedMessageTypes messageType)
		{
			var logger = new TestLogger();
			var inputFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
			var inputFile = Path.Combine(tempFolder, Path.GetRandomFileName());
			using (var handler = new MessageHandlerForTest(inputFolder, inputFile, logger, messageType))
			{
				Directory.CreateDirectory(inputFolder);

				CreateFiles(inputFolder);

				var msgs = ((IMessageHandler)handler).GetMessages();

				Assert.That(msgs.Count, Is.EqualTo(3));
			}

			var info = logger.CombinedString;
			Assert.That(info, Contains.Substring("Loading messages from SourceData"));
			Assert.That(info, Contains.Substring($"Loading messages from Input Folder: '{inputFolder}'"));
			Assert.That(info, Contains.Substring($"Checking input file: '{inputFile}'"));
		}

		void SetupMockRepo<T>(Mock<IStagingRepository> mock, IQueryable<T> queryable) where T : class
		{
			mock.Setup(x => x.Get<T>()).Returns(queryable);
			mock.Setup(x => x.SaveChanges()).Returns(1);
		}

		void CreateFiles(string inputFolder)
		{
			Directory.CreateDirectory(inputFolder);

			CreateFile(Path.Combine(inputFolder, "ValidProdat.txt"), "UNB+UNOB:4+SARSINF+COMPUCLEARING::EEEEEEEEEEEEEEBB:COMAS2+20150702:1502+8++PRODAT+++COMPUCLEARING'UNH+1+PRODAT:D:96B:UN:ZZZ01'BGM+6+456+4'");
			CreateFile(Path.Combine(inputFolder, "ValidGesmes.txt"), "UNB+UNOB:4+SARSINF+COMPUCLEARING::EEEEEEEEEEEEEEBB:COMAS2+20160401:1103+352++GESMES+++COMPUCLEARING'UNH+1+GESMES:D:96B:UN:ZZZ01'BGM+190++9'");
			CreateFile(Path.Combine(inputFolder, "NotAValidFile.txt"), "This is some random file");
		}

		void CreateFile(string name, string content)
		{
			File.WriteAllText(name, content);
			var fi = new FileInfo(name);

			// Simulate Copy of file to input folder
			fi.CreationTimeUtc = new DateTime(2022, 6, 6, 6, 6, 6);
			fi.LastWriteTimeUtc = new DateTime(2022, 5, 28, 13, 14, 15);
		}

		[SetUp]
		public void Setup()
		{
			mockRepository = new Mock<IStagingRepository>();
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			tempInputFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		string tempFolder;
		string tempInputFile;
		Mock<IStagingRepository> mockRepository;
	}
}
