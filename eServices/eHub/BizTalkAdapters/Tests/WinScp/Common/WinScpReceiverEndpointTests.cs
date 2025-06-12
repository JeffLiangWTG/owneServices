using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using Common.Logging;
using Common.Logging.Simple;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Common
{
	public class WinScpReceiverEndpointTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task EndpointTask_SingleLocation()
		{
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var cancelTokenSource = new CancellationTokenSource();
			var configXml = new XmlDocument();
			configXml.LoadXml("<Config><Server>server</Server><pollingInterval>1</pollingInterval><pollingUnitOfMeasure>Seconds</pollingUnitOfMeasure></Config>");
			var clientFactory = new Mock<IWinScpClientFactory>();
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.ConfigXml).Returns(configXml);
			target.SetupGet(x => x.Logger).Returns(logger);
			target.Setup(x => x.DownloadAndSubmitLocation(It.IsAny<string>(), It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.Callback((string _, WinScpLocation _, CancellationToken _) => cancelTokenSource.Cancel())
				.Returns(Task.CompletedTask);

			await target.Object.EndpointTask(cancelTokenSource.Token);

			target.Verify(x => x.DownloadAndSubmitLocation(It.IsAny<string>(), It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		public static IEnumerable<(string id, WinScpSortOrder sortOrder, (string name, int age)[] serverFiles, string[] expectedSubmits)> DownloadAndSubmitLocation_SortOrder_TestCases => new[]
		{
			( "None0", WinScpSortOrder.None, new(string, int)[] { }, new string[] { }),
			( "None1", WinScpSortOrder.None, new[] { ("FILE", 0) }, new [] { "FILE" }),
			( "Name", WinScpSortOrder.Name, new[] { ("00", 0), ("999", 0), ("aaa", 0), ("AAA", 0), ("ooo", 0), ("ZZ", 0) }, new [] { "00", "999", "AAA", "ZZ", "aaa", "ooo" }),
			( "Timestamp", WinScpSortOrder.Timestamp, new[] { ("100", 100), ("30", 30), ("600", 600), ("10000", 10000), ("120", 120) }, new [] { "10000", "600", "120", "100", "30" }),
		};

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCaseSource(nameof(DownloadAndSubmitLocation_SortOrder_TestCases))]
		public async Task DownloadAndSubmitLocation_SortOrder((string id, WinScpSortOrder sortOrder, (string name, int age)[] serverFiles, string[] expectedSubmits) testCase)
		{
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var location = new WinScpLocation("winscp://user@server:99/*");
			var config = new WinScpReceiveConfiguration { Locations = new List<WinScpLocation> { location }, SortOrder = testCase.sortOrder };
			var msgData = Encoding.ASCII.GetBytes("<Message/>");
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(testCase.serverFiles.Select(file => new WinScpFileInfo($"{file.name}", $"{file.name}", 10, DateTime.Now.AddSeconds(-file.age))));
			client.Setup(x => x.GetFileAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.Returns(() => Task.FromResult<Stream>(new MemoryStream(msgData)));
			var captureDeletes = new List<string>();
			client.Setup(x => x.RemoveFileAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.Callback((WinScpLocation l, CancellationToken c) => captureDeletes.Add(l.FileName)).Returns(Task.CompletedTask);
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger);
			var captureSubmits = new List<string>();
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback((Stream s, string n, string l, string a) => captureSubmits.Add(n)).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None);

			Assert.That(captureSubmits, Is.EqualTo(testCase.expectedSubmits));
			Assert.That(captureDeletes, Is.EqualTo(testCase.expectedSubmits));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCase(1, 10)]
		[TestCase(2, 10)]
		[TestCase(3, 10)]
		[TestCase(4, 10)]
		[TestCase(4, 0)]
		[TestCase(4, 2)]
		[TestCase(20, 40)]
		public async Task DownloadAndSubmitLocation_ConcurrentDownloads(int numberOfClients, int numberOfFiles)
		{
			var serverFiles = Enumerable.Range(0, numberOfFiles).Select(idx =>
			{
				var name = $"{Guid.NewGuid():N}.{idx}";
				return new WinScpFileInfo(name, name, 10, DateTime.Now);
			}).ToList();

			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var location = new WinScpLocation("winscp://user@server:99/*");
			var config = new WinScpReceiveConfiguration { Locations = new List<WinScpLocation> { location }, MaximumConcurrentDownloads = numberOfClients };
			var msgData = Encoding.ASCII.GetBytes("<Message/>");
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
			{
				var client = new Mock<IWinScpClient>();
				client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).ReturnsAsync(serverFiles);
				client.Setup(x => x.GetFileAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
					.Returns(() => Task.FromResult<Stream>(new MemoryStream(msgData)));
				return client.Object;
			});
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger);
			var captureSubmits = new List<string>();
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback((Stream s, string n, string l, string a) => captureSubmits.Add(n)).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None);

			Assert.That(captureSubmits, Is.EqualTo(serverFiles.Select(f => f.FullName).ToList()));
			clientFactory.Verify(x => x.CreateClientAsync(
				It.IsAny<WinScpLocation>(),
				config,
				logger,
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()),
				Times.Exactly(Math.Min(numberOfClients, Math.Max(numberOfFiles, 1))));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task DownloadAndSubmitLocation_FlagFile_MoveAndRename_BeforeAndAfter()
		{
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				FlagFile = "{n}.flg",
				RenameBeforeDownload = "{f}.tmp",
				MoveBeforeDownload = "tmp",
				RenameAfterDownload = "{n}.done{x}",
				MoveAfterDownload = "archive"
			};
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new[]
			{
				new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now),
				new WinScpFileInfo("MSG2.xml", "out/MSG2.xml", 10, DateTime.Now)
			});
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG2.flg"), It.IsAny<CancellationToken>())).ReturnsAsync(false);
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger);
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None);

			client.Verify(x => x.MoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.xml"), It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()), Times.Once);
			client.Verify(x => x.MoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.tmp"), It.Is<WinScpLocation>(l => l.GetPath() == "archive/MSG1.xml.done.tmp"), It.IsAny<CancellationToken>()), Times.Once);
			client.Verify(x => x.RemoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.flg"), It.IsAny<CancellationToken>()), Times.Once);
			target.Verify();
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task DownloadAndSubmitLocation_DuplicateFileRemoved()
		{
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				RenameBeforeDownload = "{f}.tmp",
				MoveBeforeDownload = "tmp",
				RenameAfterDownload = "{n}.done{x}"
			};
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new[]
			{
				new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now)
			});
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger);
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None);

			client.Verify(x => x.MoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.xml"), It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()), Times.Once);
			client.Verify(x => x.RemoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.done.tmp"), It.IsAny<CancellationToken>()), Times.Once);
			target.Verify();
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task DownloadAndSubmitLocation_RetryAndSucceeding_SubmitMessage()
		{
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				FlagFile = "{n}.flg",
				RenameBeforeDownload = "{f}.tmp",
				MoveBeforeDownload = "tmp",
				RenameAfterDownload = "{n}.done{x}",
				TransferErrorsDisablePort = true,
				TransferErrorsRetryCount = 10,
				TransferErrorsRetryInterval = 1
			};
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new[]
			{
				new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now)
			});
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.SetupSequence(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"), It.IsAny<CancellationToken>()))
				.Throws(new Exception("Exception 1"))
				.Throws(new Exception("Exception 2"))
				.Throws(new Exception("Exception 3"))
				.Throws(new Exception("Exception 4"))
				.Throws(new Exception("Exception 5"))
				.ReturnsAsync(false);
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger);
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None);

			client.Verify(x => x.MoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.xml"), It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()), Times.Once);
			client.Verify(x => x.FileExistsAsync(It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.done.tmp"), It.IsAny<CancellationToken>()), Times.Exactly(6));
			client.Verify(x => x.RemoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.flg"), It.IsAny<CancellationToken>()), Times.Once);

			target.Verify();
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DownloadAndSubmitLocation_RetryAndFailing_SubmitMessage()
		{
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				FlagFile = "{n}.flg",
				RenameBeforeDownload = "{f}.tmp",
				MoveBeforeDownload = "tmp",
				RenameAfterDownload = "{n}.done{x}",
				TransferErrorsDisablePort = true,
				TransferErrorsRetryCount = 5,
				TransferErrorsRetryInterval = 1
			};
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new[]
			{
				new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now)
			});
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"), It.IsAny<CancellationToken>()))
				.Throws(new TransferrerException("Exception!"));
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger);
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			Assert.ThrowsAsync(Is.TypeOf<TransferrerException>().And.Message.EqualTo("Exception!"),
				() => target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None));

			client.Verify(x => x.MoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.xml"), It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()), Times.Once);
			client.Verify(x => x.FileExistsAsync(It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.done.tmp"), It.IsAny<CancellationToken>()), Times.Exactly(5));
			client.Verify(x => x.RemoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.flg"), It.IsAny<CancellationToken>()), Times.Never);

			target.Verify();
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCase(true)]
		[TestCase(false)]
		public async Task DownloadAndSubmitLocation_ShouldReportDownloadLimitExceeded(bool includeNonEmptyFile)
		{
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var issueManager = new Mock<IIssueManager>();
			var tokenSource = new CancellationTokenSource();
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				DownloadExcludedFilesLimit = 10,
			};
			var client = new Mock<IWinScpClient>();
			var files = new List<WinScpFileInfo>
			{
				new ("MSG1.xml", "out/MSG1.xml", 0, DateTime.Now),
				new ("MSG2.xml", "out/MSG2.xml", 0, DateTime.Now),
				new ("MSG3.xml", "out/MSG3.xml", 0, DateTime.Now),
				new ("MSG4.xml", "out/MSG4.xml", 0, DateTime.Now),
				new ("MSG5.xml", "out/MSG5.xml", 0, DateTime.Now),
				new ("MSG6.xml", "out/MSG6.xml", 0, DateTime.Now),
				new ("MSG7.xml", "out/MSG7.xml", 0, DateTime.Now),
				new ("MSG8.xml", "out/MSG8.xml", 0, DateTime.Now),
				new ("MSG9.xml", "out/MSG9.xml", 0, DateTime.Now),
				new ("MSG10.xml", "out/MSG10.xml", 0, DateTime.Now),
				new ("MSG11.xml", "out/MSG11.xml", 0, DateTime.Now),
				new ("MSG12.xml", "out/MSG12.xml", 0, DateTime.Now),
			};
			if (includeNonEmptyFile)
			{
				files.Add(new WinScpFileInfo("MSG_Not_Empty.xml", "out/MSG_Not_Empty.xml", 100, DateTime.Now));
			}
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).ReturnsAsync(files);
			client.Setup(x => x.FileExistsAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG_Not_Empty.xml"), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger);
			target.SetupGet(x => x.IssueManager).Returns(issueManager.Object);
			target.SetupGet(x => x.PortName).Returns("GLSHK_Rcv");
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), "out/MSG_Not_Empty.xml", It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, tokenSource.Token);

			issueManager.Verify(x => x.ReportToIssueManagerAsync(
				It.Is<string>(s => s.Equals("T0001")),
				It.Is<string>(s => s.Equals("GLSHK_Rcv Excluded files exceeded the limit")),
				It.Is<Exception>(ex => ex.Message.Contains("These polling states\r\nEmptyFileCount: 12,\r\nSkippedFileCount: 0")),
				logger,
				It.IsAny<CancellationToken>())
				, Times.Once
			);
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task DownloadAndSubmitLocation_ShouldReportProcessingTimeExceeded()
		{
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var issueManager = new Mock<IIssueManager>();
			var tokenSource = new CancellationTokenSource();
			var warningTimeSeconds = 1;
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				ReceiveProcessingTimeWarning = warningTimeSeconds,
			};
			var client = new Mock<IWinScpClient>();
			var files = new List<WinScpFileInfo>
			{
				new ("MSG1.xml", "out/MSG1.xml", 0, DateTime.Now),
			};

			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).Returns(async () =>
			{
				await Task.Delay(1000 * (warningTimeSeconds + 1));
				return files;
			});
			client.Setup(x => x.FileExistsAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG_Not_Empty.xml"), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger);
			target.SetupGet(x => x.IssueManager).Returns(issueManager.Object);
			target.SetupGet(x => x.PortName).Returns("GLSHK_Rcv");
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), "out/MSG_Not_Empty.xml", It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, tokenSource.Token);

			issueManager.Verify(x => x.ReportToIssueManagerAsync(
				It.Is<string>(s => s.Equals("T0001")),
				It.Is<string>(s => s.Equals("GLSHK_Rcv ReceiveProcessingTimeWarning exceeded the limit")),
				It.IsAny<Exception>(),
				logger,
				It.IsAny<CancellationToken>())
				, Times.Once
			);
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task EndpointTask_UpdateLocationsFromClientRegistration()
		{
			var cancelTokenSource = new CancellationTokenSource();
			var logger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpReceiverEndpointTests));
			var issueManager = new Mock<IIssueManager>();
			var location = new WinScpLocation("winscp://user@server:99/*");
			var config = new Mock<WinScpReceiveConfiguration> { CallBase = true };
			config.Object.Locations = new List<WinScpLocation> { location };
			config.Object.PollingIntervalMs = 1000;
			config.Object.RegistrationConnectionStringName = "eHubTransactionsContext";
			config.Object.RegistrationType = "GLSHK";

			var configXml = new XmlDocument();
			configXml.LoadXml("<Config><Server>server</Server><pollingInterval>1</pollingInterval><pollingUnitOfMeasure>Seconds</pollingUnitOfMeasure></Config>");
			var clientFactory = new Mock<IWinScpClientFactory>();
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };

			target.Setup(x => x.GetClientRegistrationAsync(It.IsAny<CancellationToken>()))
				.Returns<CancellationToken>(_ => Task.FromResult(CreateClientRegistrations(new Dictionary<string, string>
				{
					{"winscp://user1@server1.com:21/folder1", "XWnOWZBWm5/dIIqH/SDzrKzZ35YIGubbxQAWNTGeKIGzD1qpwPhIpNqh7M0+td4Lkof0zu1ptNFJCy33tdk/RjeHZf2aSaKXG5wkogiry4DxiRAL4bmt1Q2QDt0Zh0sU43B7XsE98y6pdN2G4Om8M4AdxuTFKdrdIN9BE229jwM="},
					{"winscp://user2@server2.com:2221/folder2", "ElXHmlKeII7s7D2Z76ZjrUgS1YGulIOPtZTSnDl3y6O7U0hWz5DJBqdlzOZxWFafH/44hGWFKFKKeOyO4Fnt66ok3og7FGQ266myGKVUlxXwqzFdMVmRPBwAqFIEAcKFv55GyYIMAPgCfqNFrTbbjuWsTmzVdg0oHyNRveXKr5s="},
					{"winscp://user3@server3.com:21/folder3", "wrong password"}
				})));
			target.SetupGet(x => x.Config).Returns(config.Object);
			target.SetupGet(x => x.IssueManager).Returns(issueManager.Object);
			target.SetupGet(x => x.ConfigXml).Returns(configXml);
			target.SetupGet(x => x.Logger).Returns(logger);
			target.SetupGet(x => x.PortName).Returns("ReceiveLocation");
			var count = 0;
			target.Setup(x => x.DownloadAndSubmitLocation(
				It.IsAny<string>(),
				It.IsAny<WinScpLocation>(),
				It.IsAny<CancellationToken>()))
				.Callback<string, WinScpLocation, CancellationToken>((x, y, z) =>
				{
					if (++count == config.Object.Locations.Count)
					{
						cancelTokenSource.Cancel();
					}
				})
				.Returns(Task.CompletedTask);

			await target.Object.EndpointTask(cancelTokenSource.Token);

			Assert.That(config.Object.Locations, Has.Exactly(1).Matches<WinScpLocation>(x => x.GetIdentity(true).Equals("user1:abcd@server1.com:21")));
			Assert.That(config.Object.Locations, Has.Exactly(1).Matches<WinScpLocation>(x => x.GetIdentity(true).Equals("user2:1234@server2.com:2221")));
			issueManager.Verify(x => x.ReportToIssueManagerAsync(
				"00000001",
				"ReceiveLocation GLSHK winscp://user3@server3.com:21/folder3 Invalid length for a Base-64 char array or string.",
				It.Is<Exception>(x => x.Message.StartsWith("Invalid length for a Base-64 char array or string")),
				logger,
				cancelTokenSource.Token), Times.AtLeastOnce);
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task EndpointTask_UpdateLocationsDatabaseError()
		{
			var cancelTokenSource = new CancellationTokenSource();
			var logger = new Mock<ILog>();
			logger.Setup(x => x.IsErrorEnabled).Returns(true);
			var issueManager = new Mock<IIssueManager>();
			var location = new WinScpLocation("winscp://user@server:99/*");
			var config = new Mock<WinScpReceiveConfiguration> { CallBase = true };
			var exception = new Exception("Network error");
			config.Object.Locations = new List<WinScpLocation> { location };
			config.Object.PollingIntervalMs = 1000;
			config.Object.RegistrationConnectionStringName = "eHubTransactionsContext";
			config.Object.RegistrationType = "GLSHK";

			var configXml = new XmlDocument();
			configXml.LoadXml("<Config><Server>server</Server><pollingInterval>1</pollingInterval><pollingUnitOfMeasure>Seconds</pollingUnitOfMeasure></Config>");
			var clientFactory = new Mock<IWinScpClientFactory>();
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };

			target.Setup(x => x.GetClientRegistrationAsync(It.IsAny<CancellationToken>()))
				.Callback<CancellationToken>(_ => cancelTokenSource.Cancel())
				.Throws(exception);
			target.SetupGet(x => x.Config).Returns(config.Object);
			target.SetupGet(x => x.IssueManager).Returns(issueManager.Object);
			target.SetupGet(x => x.ConfigXml).Returns(configXml);
			target.SetupGet(x => x.Logger).Returns(logger.Object);
			target.SetupGet(x => x.PortName).Returns("ReceiveLocation");

			await target.Object.EndpointTask(cancelTokenSource.Token);

			logger.Verify(x => x.Error("(00000001) Encountered error when trying to refresh Locations from client registration", exception));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task EndpointTask_EmptyFileOptionDiscard_ShouldDeleteEmptyFile()
		{
			var logger = new Mock<ILog>();
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var issueManager = new Mock<IIssueManager>();
			var tokenSource = new CancellationTokenSource();
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				DownloadExcludedFilesLimit = 10,
				EmptyFileOption = "Discard"
			};
			var client = new Mock<IWinScpClient>();
			var files = new List<WinScpFileInfo>
			{
				new ("MSG1.xml", "out/MSG1.xml", 0, DateTime.Now),
				new ("MSG2.xml", "out/MSG2.xml", 0, DateTime.Now),
				new ("MSG3.xml", "out/MSG3.xml", 0, DateTime.Now),
				new ("MSG4.xml", "out/MSG4.xml", 1000, DateTime.Now),
			};

			logger.Setup(x => x.IsDebugEnabled).Returns(true);
			logger.Setup(x => x.IsWarnEnabled).Returns(true);
			logger.Setup(x => x.IsInfoEnabled).Returns(true);
			logger.Setup(x => x.IsErrorEnabled).Returns(true);
			logger.Setup(x => x.IsTraceEnabled).Returns(true);
			logger.Setup(x => x.IsFatalEnabled).Returns(true);

			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).ReturnsAsync(files);
			client.Setup(x => x.FileExistsAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x => x.RemoveFileAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG4.xml"), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));

			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger.Object);
			target.SetupGet(x => x.IssueManager).Returns(issueManager.Object);
			target.SetupGet(x => x.PortName).Returns("GLSHK_Rcv");
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, tokenSource.Token);

			client.Verify(x => x.RemoveFileAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()), Times.Exactly(files.Count));
			var emptyFileCount = files.Count(x => x.Length == 0);
			logger.Verify(x => x.InfoFormat("(T0001) {0} empty files discarded from location {1}", null, emptyFileCount, location), Times.Once);
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task DownloadAndSubmitLocation_ZeroByteSteam_NotSubmitToBizTalk()
		{
			var logger = new Mock<ILog>();
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var issueManager = new Mock<IIssueManager>();
			var tokenSource = new CancellationTokenSource();
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				DownloadExcludedFilesLimit = 1,
			};
			var client = new Mock<IWinScpClient>();
			var files = new List<WinScpFileInfo>
			{
				new ("MSG1.xml", "out/MSG1.xml", 1000, DateTime.Now),
				new ("MSG2.xml", "out/MSG2.xml", 1000, DateTime.Now),
			};

			logger.Setup(x => x.IsDebugEnabled).Returns(true);
			logger.Setup(x => x.IsWarnEnabled).Returns(true);
			logger.Setup(x => x.IsInfoEnabled).Returns(true);
			logger.Setup(x => x.IsErrorEnabled).Returns(true);
			logger.Setup(x => x.IsTraceEnabled).Returns(true);
			logger.Setup(x => x.IsFatalEnabled).Returns(true);

			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).ReturnsAsync(files);
			client.Setup(x => x.FileExistsAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x => x.RemoveFileAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			client.Setup(x => x.GetFileAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.Returns(() => Task.FromResult((Stream)new MemoryStream(Encoding.UTF8.GetBytes(""))));

			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object, It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target = new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration)) { CallBase = true };
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger.Object);
			target.SetupGet(x => x.IssueManager).Returns(issueManager.Object);
			target.SetupGet(x => x.PortName).Returns("GLSHK_Rcv");
			target.Setup(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, tokenSource.Token);

			logger.Verify(x => x.WarnFormat("(T0001+F0000) Retrieved zero byte stream from WinSCP download file: {0}", null, files.FirstOrDefault().FullName), Times.Once);
			logger.Verify(x => x.WarnFormat("(T0001+F0001) Retrieved zero byte stream from WinSCP download file: {0}", null, files[1].FullName), Times.Once);
			target.Verify(x => x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			issueManager.Verify(x => x.ReportToIssueManagerAsync("T0001", "GLSHK_Rcv Excluded files exceeded the limit", It.IsAny<Exception>(), logger.Object, It.IsAny<CancellationToken>()), Times.Once);
		}

		private IEnumerable<DataRow> CreateClientRegistrations(IDictionary<string, string> clientRegistrations)
		{
			var list = new List<DataRow>();
			foreach (var entry in clientRegistrations)
			{
				var row = WinScpReceiverEndpoint.CustomDataTable.Value.NewRow();
				row["URI"] = entry.Key;
				row["Password"] = entry.Value;
				list.Add(row);
			}

			return list;
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task DownloadAndSubmitLocation_RetryAndSucceeding_SubmitMessage_RetryConnect()
		{
			var logger = new Mock<ILog>();
			logger.Setup(x => x.IsDebugEnabled).Returns(true);
			logger.Setup(x => x.IsWarnEnabled).Returns(true);
			logger.Setup(x => x.IsInfoEnabled).Returns(true);
			logger.Setup(x => x.IsErrorEnabled).Returns(true);
			logger.Setup(x => x.IsTraceEnabled).Returns(true);
			logger.Setup(x => x.IsFatalEnabled).Returns(true);

			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				FlagFile = "{n}.flg",
				RenameBeforeDownload = "{f}.tmp",
				MoveBeforeDownload = "tmp",
				RenameAfterDownload = "{n}.done{x}",
				TransferErrorsDisablePort = true,
				TransferErrorsRetryCount = 10,
				TransferErrorsRetryInterval = 1
			};
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new[] { new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now) });
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"),
				It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x =>
					x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"),
						It.IsAny<CancellationToken>()))
				.Throws(new TransferrerException("TransferrerException"));
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));

			var client1 = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new[] { new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now) });
			client1.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"),
				It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client1.Setup(x =>
					x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"),
						It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);
			client1.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));
			client1.Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);

			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x =>
					x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object, "T0001+C0001",
						It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object,
					"T0001+C0002",
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(client1.Object);
			var target =
				new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration))
				{
					CallBase = true
				};
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger.Object);
			target.Setup(x =>
				x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>())).Returns(true);

			await target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None);

			client.Verify(
				x => x.MoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.xml"),
					It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()),
				Times.Once);
			client.Verify(
				x => x.FileExistsAsync(It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.done.tmp"),
					It.IsAny<CancellationToken>()), Times.Once);
			client1.Verify(
				x => x.FileExistsAsync(It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.done.tmp"),
					It.IsAny<CancellationToken>()), Times.Once);
			client1.Verify(x => x.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
			logger.Verify(x => x.Warn(It.Is<string>(y => y.Contains("Retrying 0 of")), It.IsAny<Exception>()),
				Times.Once);
			target.Verify();
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCase(true)]
		[TestCase(false)]
		public async Task
			DownloadAndSubmitLocation_RetryAndSucceeding_SubmitMessage_RetryConnectOperationCanceledException(
				bool transferErrorsDisablePort)
		{
			var logger = new Mock<ILog>();
			logger.Setup(x => x.IsDebugEnabled).Returns(true);
			logger.Setup(x => x.IsWarnEnabled).Returns(true);
			logger.Setup(x => x.IsInfoEnabled).Returns(true);
			logger.Setup(x => x.IsErrorEnabled).Returns(true);
			logger.Setup(x => x.IsTraceEnabled).Returns(true);
			logger.Setup(x => x.IsFatalEnabled).Returns(true);
			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				FlagFile = "{n}.flg",
				RenameBeforeDownload = "{f}.tmp",
				MoveBeforeDownload = "tmp",
				RenameAfterDownload = "{n}.done{x}",
				TransferErrorsDisablePort = transferErrorsDisablePort,
				TransferErrorsRetryCount = 10,
				TransferErrorsRetryInterval = 1
			};
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new[] { new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now) });
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"),
				It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x =>
					x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"),
						It.IsAny<CancellationToken>()))
				.Throws(new OperationCanceledException("TransferrerException"));
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));

			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x =>
					x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object, It.IsAny<string>(),
						It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			var target =
				new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration))
				{
					CallBase = true
				};
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger.Object);
			target.Setup(x =>
				x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>())).Returns(true);
			await target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None);

			if (transferErrorsDisablePort)
			{
				logger.Verify(
					x => x.Error(It.Is<string>(y => y.Contains("Starting to disable Biztalk receive location")),
						It.IsAny<Exception>()), Times.Once);
			}
			else
			{
				logger.Verify(
					x => x.Error(It.Is<string>(y => y.Contains("Starting to disable Biztalk receive location")),
						It.IsAny<Exception>()), Times.Never);
			}
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCase(true)]
		[TestCase(false)]
		public void DownloadAndSubmitLocation_RetryAndSucceeding_SubmitMessage_RetryConnectJustOnce(
			bool transferErrorsDisablePort)
		{
			var logger = new Mock<ILog>();
			logger.Setup(x => x.IsDebugEnabled).Returns(true);
			logger.Setup(x => x.IsWarnEnabled).Returns(true);
			logger.Setup(x => x.IsInfoEnabled).Returns(true);
			logger.Setup(x => x.IsErrorEnabled).Returns(true);
			logger.Setup(x => x.IsTraceEnabled).Returns(true);
			logger.Setup(x => x.IsFatalEnabled).Returns(true);

			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				FlagFile = "{n}.flg",
				RenameBeforeDownload = "{f}.tmp",
				MoveBeforeDownload = "tmp",
				RenameAfterDownload = "{n}.done{x}",
				TransferErrorsDisablePort = transferErrorsDisablePort,
				TransferErrorsRetryCount = 1,
				TransferErrorsRetryInterval = 1
			};
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new[] { new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now) });
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"),
				It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x =>
					x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"),
						It.IsAny<CancellationToken>()))
				.Throws(new TransferrerException("TransferrerException"));
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));

			var client1 = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new[] { new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now) });
			client1.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"),
				It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client1.Setup(x =>
					x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"),
						It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);
			client1.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));
			client1.Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);

			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x =>
					x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object, "T0001+C0001",
						It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object,
					"T0001+C0002",
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(client1.Object);
			var target =
				new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration))
				{
					CallBase = true
				};
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger.Object);
			target.Setup(x =>
				x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>())).Returns(true);

			Assert.ThrowsAsync(Is.TypeOf<TransferrerException>(),
				() => target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None));

			client.Verify(
				x => x.MoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.xml"),
					It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()),
				Times.Once);
			client.Verify(
				x => x.FileExistsAsync(It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.done.tmp"),
					It.IsAny<CancellationToken>()), Times.Once);
			client1.Verify(x => x.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
			logger.Verify(x => x.Warn(It.Is<string>(y => y.Contains("Retrying 0 of")), It.IsAny<Exception>()),
				Times.Once);
			logger.Verify(x => x.Error(It.Is<string>(y => y.Contains("Retries Exhausted")), It.IsAny<Exception>()),
				Times.Once);
			if (transferErrorsDisablePort)
			{
				logger.Verify(
					x => x.Error(It.Is<string>(y => y.Contains("Starting to disable Biztalk receive location")),
						It.IsAny<Exception>()), Times.Once);
			}
			else
			{
				logger.Verify(
					x => x.Error(It.Is<string>(y => y.Contains("Starting to disable Biztalk receive location")),
						It.IsAny<Exception>()), Times.Never);
			}

			target.Verify();
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DownloadAndSubmitLocation_RetryAndSucceeding_SubmitMessage_RetryConnectAllFail()
		{
			var logger = new Mock<ILog>();
			logger.Setup(x => x.IsDebugEnabled).Returns(true);
			logger.Setup(x => x.IsWarnEnabled).Returns(true);
			logger.Setup(x => x.IsInfoEnabled).Returns(true);
			logger.Setup(x => x.IsErrorEnabled).Returns(true);
			logger.Setup(x => x.IsTraceEnabled).Returns(true);
			logger.Setup(x => x.IsFatalEnabled).Returns(true);

			var location = new WinScpLocation("winscp://user@server:99/out/*.xml");
			var config = new WinScpReceiveConfiguration
			{
				Locations = new List<WinScpLocation> { location },
				FlagFile = "{n}.flg",
				RenameBeforeDownload = "{f}.tmp",
				MoveBeforeDownload = "tmp",
				RenameAfterDownload = "{n}.done{x}",
				TransferErrorsDisablePort = false,
				TransferErrorsRetryCount = 1,
				TransferErrorsRetryInterval = 1
			};
			var client = new Mock<IWinScpClient>();
			client.Setup(x => x.EnumerateRemoteFilesAsync(It.IsAny<WinScpLocation>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new[] { new WinScpFileInfo("MSG1.xml", "out/MSG1.xml", 10, DateTime.Now) });
			client.Setup(x => x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "out/MSG1.flg"),
				It.IsAny<CancellationToken>())).ReturnsAsync(true);
			client.Setup(x =>
					x.FileExistsAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.done.tmp"),
						It.IsAny<CancellationToken>()))
				.Throws(new TransferrerException("TransferrerException"));
			client.Setup(x => x.GetFileAsync(It.Is<WinScpLocation>(f => f.GetPath() == "tmp/MSG1.xml.tmp"),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(new MemoryStream(Encoding.ASCII.GetBytes("<Message/>")));

			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x =>
					x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object, "T0001+C0001",
						It.IsAny<CancellationToken>()))
				.ReturnsAsync(client.Object);
			clientFactory.Setup(x => x.CreateClientAsync(It.IsAny<WinScpLocation>(), config, logger.Object,
					"T0001+C0002",
					It.IsAny<CancellationToken>()))
				.ThrowsAsync(new TransferrerException("T0001+C0002"));
			var target =
				new Mock<WinScpReceiverEndpoint>(clientFactory.Object, typeof(WinScpReceiveConfiguration))
				{
					CallBase = true
				};
			target.SetupGet(x => x.Config).Returns(config);
			target.SetupGet(x => x.Logger).Returns(logger.Object);
			target.Setup(x =>
				x.SubmitMessageToBizTalk(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>())).Returns(true);

			Assert.ThrowsAsync(Is.TypeOf<TransferrerException>().With.Message.EqualTo("T0001+C0002"),
				() => target.Object.DownloadAndSubmitLocation("T0001", location, CancellationToken.None));

			client.Verify(
				x => x.MoveFileAsync(It.Is<WinScpLocation>(l => l.GetPath() == "out/MSG1.xml"),
					It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.tmp"), It.IsAny<CancellationToken>()),
				Times.Once);
			client.Verify(
				x => x.FileExistsAsync(It.Is<WinScpLocation>(l => l.GetPath() == "tmp/MSG1.xml.done.tmp"),
					It.IsAny<CancellationToken>()), Times.Once);
			logger.Verify(x => x.Warn(It.Is<string>(y => y.Contains("Retrying 0 of")), It.IsAny<Exception>()),
				Times.Once);
			logger.Verify(x => x.Error(It.Is<string>(y => y.Contains("Retries Exhausted")), It.IsAny<Exception>()),
				Times.Once);
			logger.Verify(
				x => x.Error(It.Is<string>(y => y.Contains("Starting to disable Biztalk receive location")),
					It.IsAny<Exception>()), Times.Never);

			target.Verify();
		}
	}
}
