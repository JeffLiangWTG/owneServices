using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eServices.USCustoms.InboundService;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass()]
	public class FileSystemMessageRetrieverTest : TestBase
	{
		[TestMethod()]
		public void FileSystemMessageRetriever_ConstructorTest()
		{
			var mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			var mockLogger = MockRepository.GenerateMock<ILog>();

			var testData = new[] { 
				new[] { "INPUTDIR",					"INPUTDIR",					String.Empty },
				new[] { @"INPUTDIR\SECONDDIR",		@"INPUTDIR\SECONDDIR",		String.Empty },
				new[] { @"A:\INPUTDIR\SECONDDIR\",	@"A:\INPUTDIR\SECONDDIR\",	String.Empty },
				new[] { "*INPUT*.TXT",				".",						"*INPUT*.TXT" },
				new[] { @"INPUTDIR\*.*",			@"INPUTDIR\",				"*.*" },
				new[] { @"..\INPUTDIR\*.*",			@"..\INPUTDIR\",			"*.*" },
			}.Select(t => new { inputPath = t[0], expectedFolderPath = t[1], expectedFileMask = t[2] });

			foreach (var test in testData)
			{
				var actualRetriever = new FileSystemMessageRetriever(test.inputPath, mockConfig, mockLogger);
				Assert.AreEqual(test.expectedFolderPath, actualRetriever.inputDirectory.ToString());
				Assert.AreEqual(test.expectedFileMask, actualRetriever.fileMask);
			}
		}

		[TestMethod]
		public void FileSystemMessageRetriever_RetrieveFiles()
		{
			var mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockDirectoryInfo = MockRepository.GenerateMock<FileSystemMessageRetriever.DirectoryInfoWrapper>();
			var mockFileInfo1 = MockRepository.GenerateMock<FileSystemMessageRetriever.FileInfoWrapper>();
			var mockFileInfo2 = MockRepository.GenerateMock<FileSystemMessageRetriever.FileInfoWrapper>();
			var mockFileList = new List<FileSystemMessageRetriever.FileInfoWrapper> { mockFileInfo1, mockFileInfo2 };
			mockDirectoryInfo.Stub(x => x.GetFiles("*")).Do(new Func<string, FileSystemMessageRetriever.FileInfoWrapper[]>(s => { return mockFileList.ToArray(); }));
			mockFileInfo1.Stub(x => x.FullName).Return("MESSAGE1");
			mockFileInfo1.Stub(x => x.OpenRead()).Return(new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1")));
			mockFileInfo1.Stub(x => x.Delete()).Do(new Action(() => mockFileList.Remove(mockFileInfo1)));
			mockFileInfo2.Stub(x => x.FullName).Return("MESSAGE2");
			mockFileInfo2.Stub(x => x.OpenRead()).Return(new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2")));
			mockFileInfo2.Stub(x => x.Delete()).Do(new Action(() => mockFileList.Remove(mockFileInfo2)));
			mockConfig.Stub(x => x.MessageType).Return("ABI");
			var actualLog = new List<string>();
			InitialiseLoggerStubs(mockLogger, actualLog);
			string actualMessage1, actualMessage2;

			var testRetriever = new FileSystemMessageRetriever(@"DIR\*", mockConfig, mockLogger);
			testRetriever.inputDirectory = mockDirectoryInfo;

			testRetriever.BeginTransaction();
			using (var messageStream = testRetriever.Retrieve())
			using (var sr = new StreamReader(messageStream))
				actualMessage1 = sr.ReadToEnd();
			testRetriever.CommitTransaction();
			testRetriever.BeginTransaction();
			using (var messageStream = testRetriever.Retrieve())
			using (var sr = new StreamReader(messageStream))
				actualMessage2 = sr.ReadToEnd();
			testRetriever.CommitTransaction();
			testRetriever.BeginTransaction();
			var actualNoMoreMessages = testRetriever.Retrieve();
			testRetriever.RollbackTransaction();

			Assert.AreEqual("<Message IsProduction=\"false\" MessageType=\"ABI\"><![CDATA[H4sIAAAAAAAEAPN1DQ52dHc1BACugo2+CAAAAA==]]></Message>", actualMessage1);
			Assert.AreEqual("<Message IsProduction=\"false\" MessageType=\"ABI\"><![CDATA[H4sIAAAAAAAEAPN1DQ52dHc1AgAU04QnCAAAAA==]]></Message>", actualMessage2);
			Assert.IsNull(actualNoMoreMessages);
			mockDirectoryInfo.AssertWasCalled(x => x.GetFiles("*"), x => x.Repeat.Times(3));
			mockFileInfo1.AssertWasCalled(x => x.OpenRead(), x => x.Repeat.Once());
			mockFileInfo1.AssertWasCalled(x => x.Delete(), x => x.Repeat.Once());
			mockFileInfo2.AssertWasCalled(x => x.OpenRead(), x => x.Repeat.Once());
			mockFileInfo2.AssertWasCalled(x => x.Delete(), x => x.Repeat.Once());
			Assert.AreEqual(0, mockFileList.Count);
			CollectionAssert.AreEqual(new[] { 
				"[INF] Initialised file system message retriever. Directory: 'DIR\\' FileMask: '*'",
				"[DBG] Receiving file: MESSAGE1",
				"[DBG] Deleting file: MESSAGE1",
				"[DBG] Receiving file: MESSAGE2",
				"[DBG] Deleting file: MESSAGE2"
			}, actualLog);
		}

		[TestMethod]
		public void FileSystemMessageRetriever_Rollback()
		{
			var mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockDirectoryInfo = MockRepository.GenerateMock<FileSystemMessageRetriever.DirectoryInfoWrapper>();
			var mockFileInfo = MockRepository.GenerateMock<FileSystemMessageRetriever.FileInfoWrapper>();
			mockDirectoryInfo.Stub(x => x.GetFiles(String.Empty)).Return(new[] { mockFileInfo });
			mockFileInfo.Stub(x => x.FullName).Return("MESSAGE1");
			mockFileInfo.Stub(x => x.OpenRead()).Return(new MemoryStream());
			mockConfig.Stub(x => x.MessageType).Return("ABI");
			var actualLog = new List<string>();
			InitialiseLoggerStubs(mockLogger, actualLog);

			var testRetriever = new FileSystemMessageRetriever(String.Empty, mockConfig, mockLogger);
			testRetriever.inputDirectory = mockDirectoryInfo;

			testRetriever.BeginTransaction();
			testRetriever.Retrieve();
			testRetriever.RollbackTransaction();

			mockFileInfo.AssertWasCalled(x => x.OpenRead(), x => x.Repeat.Once());
			mockFileInfo.AssertWasNotCalled(x => x.Delete());
			CollectionAssert.AreEqual(new[] { 
				"[INF] Initialised file system message retriever. Directory: '.' FileMask: ''",
				"[DBG] Receiving file: MESSAGE1",
				"[DBG] File not deleted: MESSAGE1"
			}, actualLog);
		}

		[TestMethod]
		public void FileSystemMessageRetriever_Exceptions()
		{
			var mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			var mockLogger = MockRepository.GenerateMock<ILog>();

			AssertException<ArgumentNullException>(() => new FileSystemMessageRetriever(null, mockConfig, mockLogger), e => e.ParamName == "inputPath");
			AssertException<ArgumentNullException>(() => new FileSystemMessageRetriever(String.Empty, null, mockLogger), e => e.ParamName == "config");
			AssertException<ArgumentNullException>(() => new FileSystemMessageRetriever(String.Empty, mockConfig, null), e => e.ParamName == "logger");
			AssertException<NotImplementedException>(() => new FileSystemMessageRetriever(String.Empty, mockConfig, mockLogger).RetrieveSafe());

			var testRetriever = new FileSystemMessageRetriever(String.Empty, mockConfig, mockLogger);
			AssertException<InvalidOperationException>(() => { testRetriever.Retrieve(); }, e => e.Message == "No active transaction.");
			AssertException<InvalidOperationException>(() => { testRetriever.CommitTransaction(); }, e => e.Message == "No active transaction.");
			AssertException<InvalidOperationException>(() => { testRetriever.BeginTransaction(); testRetriever.BeginTransaction(); }, e => e.Message == "Parallel transactions are not supported.");
		}
	}
}
