using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer.Core
{
	[TestClass]
	public class ReceiveHandlerTests : TestBase
	{
		[TestMethod]
		public void TransferrerCore_ReceiveHandler_OpenClose()
		{
			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Folder></Folder>
  <FileMask></FileMask>
  <FlagFile></FlagFile>
  <SortOrder></SortOrder>
  <EmptyFileOption></EmptyFileOption>
  <RenameBeforeDownload></RenameBeforeDownload>
  <MoveBeforeDownload></MoveBeforeDownload>
  <RenameAfterDownload></RenameAfterDownload>
  <MoveAfterDownload></MoveAfterDownload>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(ReceiveHandlerTests).Name);

			var stubTransferrer = MockRepository.GenerateStub<ITransferrer>();
			stubTransferrer.Stub(x => x.OpenAsync(configXml, log, CancellationToken.None)).Return(Task.Delay(0));
			stubTransferrer.Stub(x => x.CloseAsync(log, CancellationToken.None)).Return(Task.Delay(0));

			using (var receiveHandler = new ReceiveHandler(() => stubTransferrer, configXml))
			{
				receiveHandler.OpenAsync(configXml, log, CancellationToken.None).Wait();
				receiveHandler.CloseAsync(log, CancellationToken.None).Wait();
			}

			stubTransferrer.AssertWasCalled(x => x.OpenAsync(configXml, log, CancellationToken.None));
			stubTransferrer.AssertWasCalled(x => x.CloseAsync(log, CancellationToken.None));
			stubTransferrer.AssertWasCalled(x => x.Dispose());
		}

		[TestMethod]
		public void TransferrerCore_ReceiveHandler_ListServerFiles()
		{
			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Folder></Folder>
  <FileMask></FileMask>
  <FlagFile></FlagFile>
  <SortOrder></SortOrder>
  <EmptyFileOption>Ignore</EmptyFileOption>
  <RenameBeforeDownload></RenameBeforeDownload>
  <MoveBeforeDownload></MoveBeforeDownload>
  <RenameAfterDownload></RenameAfterDownload>
  <MoveAfterDownload></MoveAfterDownload>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(ReceiveHandlerTests).Name);

			var serverFiles = new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "FILE1.001", Timestamp = DateTime.Parse("2000-01-01 00:00"), Size = 0 },
				new TransferrerFileInfo { Folder = "", Name = "FILE2.002", Timestamp = DateTime.Parse("1999-12-31 00:00"), Size = 500 },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE3.003", Timestamp = DateTime.Parse("2002-01-01 00:00"), Size = 1000 },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE4.000", Timestamp = DateTime.Parse("2001-01-01 00:00"), Size = 100 },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE5.000", Timestamp = DateTime.Parse("2010-01-01 00:00"), Size = 200 },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE5.000.OK", Timestamp = DateTime.Parse("2001-01-01 00:00"), Size = 0 },
				new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE6.000", Timestamp = DateTime.Parse("2011-01-01 00:00"), Size = 100 },
				new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE7.000", Timestamp = DateTime.Parse("2011-01-01 00:00"), Size = 0 },
			};
			List<TransferrerFileInfo> result;

			result = TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(configXml, serverFiles);
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "FILE2.002" },
			}, result);

			configXml["Config"]["Folder"].InnerText = "FOLDER";
			configXml["Config"]["EmptyFileOption"].InnerText = "Discard";
			result = TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(configXml, serverFiles);
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE3.003" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE4.000" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE5.000" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE6.000" },
			}, result);

			configXml["Config"]["SortOrder"].InnerText = "Timestamp";
			result = TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(configXml, serverFiles);
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE4.000" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE3.003" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE5.000" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE6.000" },
			}, result);

			configXml["Config"]["SortOrder"].InnerText = "Name";
			result = TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(configXml, serverFiles);
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE6.000" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE3.003" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE4.000" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE5.000" },
			}, result);

			configXml["Config"]["FileMask"].InnerText = "*.000";
			configXml["Config"]["SortOrder"].InnerText = "";
			result = TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(configXml, serverFiles);
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE4.000" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE5.000" },
				new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE6.000" },
			}, result);

			configXml["Config"]["FlagFile"].InnerText = "{f}.OK";
			result = TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(configXml, serverFiles);
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE5.000" },
			}, result);

			configXml["Config"]["FlagFile"].InnerText = "";
			configXml["Config"]["RenameBeforeDownload"].InnerText = ".{f}";
			result = TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(configXml, serverFiles);
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
					new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE4.000" },
					new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE5.000" },
					new TransferrerFileInfo { Folder = "FOLDER", Name = ".FILE6.000" },
			}, result);

			configXml["Config"]["RenameBeforeDownload"].InnerText = "";
			configXml["Config"]["MoveBeforeDownload"].InnerText = "MOVED";
			result = TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(configXml, serverFiles);
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
					new TransferrerFileInfo { Folder = "MOVED", Name = "FILE4.000" },
					new TransferrerFileInfo { Folder = "MOVED", Name = "FILE5.000" },
					new TransferrerFileInfo { Folder = "MOVED", Name = ".FILE6.000" },
			}, result);
		}

		static List<TransferrerFileInfo> TransferrerCore_ReceiveHandler_ListServerFiles_ArrangeAndAct(XmlDocument configXml, List<TransferrerFileInfo> serverFiles)
		{
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(ReceiveHandlerTests).Name);

			var stubTransferrer = MockRepository.GenerateStub<ITransferrer>();
			stubTransferrer.Stub(x => x.OpenAsync(configXml, log, CancellationToken.None)).Return(Task.Delay(0));
			stubTransferrer.Stub(x => x.ListFilesAsync(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg.Is(log), Arg.Is(CancellationToken.None)))
				.Do(new Func<string, string, ILog, CancellationToken, Task<List<TransferrerFileInfo>>>(
					(f, m, l, c) => Task.FromResult(serverFiles.Where(s => s.Folder == f && Regex.IsMatch(s.Name, TransferrerHelpers.ConvertFileMaskToRegexPattern(m), RegexOptions.IgnoreCase)).ToList())));
			stubTransferrer.Stub(x => x.RenameFileAsync(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg.Is(log), Arg.Is(CancellationToken.None)))
				.Do(new Func<string, string, ILog, CancellationToken, Task>(
					(p, d, l, c) => 
					{
						string fromName = Path.GetFileName(p);
						string fromFolder = p.Remove(p.Length - fromName.Length).TrimEnd('/');
						string destName = Path.GetFileName(d);
						string destFolder = d.Remove(d.Length - destName.Length).TrimEnd('/');
						serverFiles = serverFiles.Select(s => { if (s.Folder == fromFolder && s.Name == fromName) { s.Folder = destFolder; s.Name = destName; } return s; }).ToList();
						return Task.Delay(0);
					}));

			using (var receiveHandler = new ReceiveHandler(() => stubTransferrer, configXml))
			{
				receiveHandler.OpenAsync(configXml, log, CancellationToken.None).Wait();
				return receiveHandler.ListServerFilesAsync(log, CancellationToken.None).Result;
			}
		}

		[TestMethod]
		public void TransferrerCore_ReceiveHandler_Download()
		{
			var configXml = new XmlDocument();
			configXml.LoadXml(@"<Config><Folder/></Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(ReceiveHandlerTests).Name);
			var fileInfo = new TransferrerFileInfo { Folder = "FOLDER", Name = "FILE1.000" };
			var fileStream = new MemoryStream();
			Stream resultStream = null;

			var stubTransferrer = MockRepository.GenerateStub<ITransferrer>();
			stubTransferrer.Stub(x => x.GetFileAsync("FOLDER/FILE1.000", log, CancellationToken.None)).Return(Task.Factory.StartNew<Stream>(() => fileStream));

			using (var receiveHandler = new ReceiveHandler(() => stubTransferrer, configXml))
			{
				resultStream = receiveHandler.DownloadAsync(fileInfo, log, CancellationToken.None).Result;
			}

			Assert.AreSame(fileStream, resultStream);
		}

		[TestMethod]
		public void TransferrerCore_ReceiveHandler_PostDownloadProcessing()
		{
			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Folder></Folder>
  <FileMask></FileMask>
  <FlagFile></FlagFile>
  <SortOrder></SortOrder>
  <EmptyFileOption>Ignore</EmptyFileOption>
  <RenameBeforeDownload></RenameBeforeDownload>
  <MoveBeforeDownload></MoveBeforeDownload>
  <RenameAfterDownload></RenameAfterDownload>
  <MoveAfterDownload></MoveAfterDownload>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(ReceiveHandlerTests).Name);

			var serverFiles = new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "FILE1.001" },
				new TransferrerFileInfo { Folder = "", Name = "FILE2.002" },
				new TransferrerFileInfo { Folder = "", Name = "FILE2.002.OK" },
				new TransferrerFileInfo { Folder = "", Name = "FILE3.003" },
				new TransferrerFileInfo { Folder = "", Name = "FILE4.004" },
			};

			serverFiles = TransferrerCore_ReceiveHandler_PostDownloadProcessing_ArrangeAndAct(configXml, serverFiles, new TransferrerFileInfo { Folder = "", Name = "FILE1.001" });
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "FILE2.002" },
				new TransferrerFileInfo { Folder = "", Name = "FILE2.002.OK" },
				new TransferrerFileInfo { Folder = "", Name = "FILE3.003" },
				new TransferrerFileInfo { Folder = "", Name = "FILE4.004" },
			}, serverFiles);

			configXml["Config"]["FlagFile"].InnerText = "{f}.OK";
			serverFiles = TransferrerCore_ReceiveHandler_PostDownloadProcessing_ArrangeAndAct(configXml, serverFiles, new TransferrerFileInfo { Folder = "", Name = "FILE2.002" });
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "FILE3.003" },
				new TransferrerFileInfo { Folder = "", Name = "FILE4.004" },
			}, serverFiles);

			configXml["Config"]["FlagFile"].InnerText = "";
			configXml["Config"]["RenameAfterDownload"].InnerText = "{f}.done";
			serverFiles = TransferrerCore_ReceiveHandler_PostDownloadProcessing_ArrangeAndAct(configXml, serverFiles, new TransferrerFileInfo { Folder = "", Name = "FILE3.003" });
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "FILE3.003.done" },
				new TransferrerFileInfo { Folder = "", Name = "FILE4.004" },
			}, serverFiles);

			configXml["Config"]["RenameAfterDownload"].InnerText = "";
			configXml["Config"]["MoveAfterDownload"].InnerText = "MOVED";
			serverFiles = TransferrerCore_ReceiveHandler_PostDownloadProcessing_ArrangeAndAct(configXml, serverFiles, new TransferrerFileInfo { Folder = "", Name = "FILE4.004" });
			CollectionAssert.AreEqual(new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "FILE3.003.done" },
				new TransferrerFileInfo { Folder = "MOVED", Name = "FILE4.004" },
			}, serverFiles);
		}

		static List<TransferrerFileInfo> TransferrerCore_ReceiveHandler_PostDownloadProcessing_ArrangeAndAct(XmlDocument configXml, List<TransferrerFileInfo> serverFiles, TransferrerFileInfo fileInfo)
		{
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(ReceiveHandlerTests).Name);

			var stubTransferrer = MockRepository.GenerateStub<ITransferrer>();
			stubTransferrer.Stub(x => x.OpenAsync(configXml, log, CancellationToken.None)).Return(Task.Delay(0));
			stubTransferrer.Stub(x => x.DeleteFileAsync(Arg<string>.Is.Anything, Arg.Is(log), Arg.Is(CancellationToken.None)))
				.Do(new Func<string, ILog, CancellationToken, Task>(
					(p, l, c) =>
					{
						string name = Path.GetFileName(p);
						string folder = p.Remove(p.Length - name.Length).TrimEnd('/');
						var file = new TransferrerFileInfo { Folder = folder, Name = name };
						serverFiles.Remove(file);
						return Task.Delay(0);
					}));
			stubTransferrer.Stub(x => x.RenameFileAsync(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg.Is(log), Arg.Is(CancellationToken.None)))
				.Do(new Func<string, string, ILog, CancellationToken, Task>(
					(p, d, l, c) =>
					{
						string fromName = Path.GetFileName(p);
						string fromFolder = p.Remove(p.Length - fromName.Length).TrimEnd('/');
						string destName = Path.GetFileName(d);
						string destFolder = d.Remove(d.Length - destName.Length).TrimEnd('/');
						serverFiles = serverFiles.Select(s => { if (s.Folder == fromFolder && s.Name == fromName) { s.Folder = destFolder; s.Name = destName; } return s; }).ToList();
						return Task.Delay(0);
					}));

			using (var receiveHandler = new ReceiveHandler(() => stubTransferrer, configXml))
			{
				receiveHandler.OpenAsync(configXml, log, CancellationToken.None).Wait();
				receiveHandler.PostDownloadProcessingAsync(fileInfo, log).Wait();
				return serverFiles;
			}
		}
	}
}
