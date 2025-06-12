using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Ftp;
using Common.Logging.Simple;
using FluentFTP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using TransferrerException = CargoWise.eHub.BizTalkAdapters.Transferrer.Core.TransferrerException;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer.Ftp
{
	[TestClass]
	public class FtpTransferrerTests : TestBase
	{
		[TestMethod]
		public void FtpTransferrer_OpenClose()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <Port>22</Port>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
  <Timeout>-1</Timeout>
  <Mode>Active</Mode>
  <FtpsMode>None</FtpsMode>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();
				ftpTransferrer.CloseAsync(log, CancellationToken.None).Wait();
			}

			stubFtpClient.AssertWasCalled(s => s.Connect());
			stubFtpClient.AssertWasCalled(x => x.Disconnect());
			stubFtpClient.AssertWasCalled(x => x.Dispose());
		}

		[TestMethod]
		public void FtpTransferrer_OpenTimeout()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
  <Timeout>100</Timeout>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			int timeout = 0;
			stubFtpClient.Stub(x => x.Connect()).Do(new Action(() => { Thread.Sleep(timeout); throw new TimeoutException("FtpClient"); }));

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				try
				{
					timeout = 0;
					ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();
				}
				catch (AggregateException ex)
				{
					Assert.IsInstanceOfType(ex.InnerException.InnerException, typeof(TimeoutException));
					Assert.AreEqual("FtpClient", ex.InnerException.InnerException.Message);
				}
				try
				{
					timeout = 200;
					ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();
				}
				catch (AggregateException ex)
				{
					Assert.IsInstanceOfType(ex.InnerException, typeof(TransferrerException));
					Assert.IsTrue(ex.InnerException.Message.ToLowerInvariant().Contains("timeout"));
				}
			}
		}

		[TestMethod]
		public void FtpTransferrer_HostKeyFingerprint()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
  <Timeout>100</Timeout>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();
				ftpTransferrer.CloseAsync(log, CancellationToken.None).Wait();
			}

			stubFtpClient.AssertWasCalled(s => s.Connect());
			stubFtpClient.AssertWasCalled(x => x.Disconnect());
			stubFtpClient.AssertWasCalled(x => x.Dispose());
		}

		[TestMethod]
		public void FtpTransferrer_HostKeyFingerprintMismatch()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
  <Timeout>100</Timeout>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				try
				{
					ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();

				}
				catch (AggregateException ex)
				{
					Assert.IsInstanceOfType(ex.InnerException.InnerException, typeof(TransferrerException));
				}
			}

			stubFtpClient.AssertWasCalled(s => s.Connect());
			stubFtpClient.AssertWasCalled(x => x.Dispose());
		}

		[TestMethod]
		public void FtpTransferrer_ListFiles()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();

				string folder = "PATH";
				string fileMask = "*.txt";
				string path = folder + '/' + fileMask;
				var dir = new FtpListItem { Type = FtpFileSystemObjectType.Directory };
				var file1 = new FtpListItem { Type = FtpFileSystemObjectType.File, Name = "FILE1.TXT.TMP" };
				var file2 = new FtpListItem { Type = FtpFileSystemObjectType.File, Name = "FILE2.TXT", Size = 1000, Modified = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc) };
				stubFtpClient.Stub(x => x.GetListing(Arg.Is(folder), Arg<FtpListOption>.Is.Anything)).Return(new FtpListItem[] { dir, file1, file2 });

				var result = ftpTransferrer.ListFilesAsync(folder, fileMask, log, CancellationToken.None).Result;

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(result[0].Name, file2.Name);
				Assert.AreEqual(result[0].Size, file2.Size);
				Assert.AreEqual(result[0].Timestamp, file2.Modified);

				ftpTransferrer.CloseAsync(log, CancellationToken.None).Wait();
			}
		}

		[TestMethod]
		public void FtpTransferrer_ListFilesNoMask()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();

				string folder = "PATH";
				string fileMask = "";
				string path = folder + '/' + fileMask;
				var dir = new FtpListItem { Type = FtpFileSystemObjectType.Directory };
				var file1 = new FtpListItem { Type = FtpFileSystemObjectType.File, Name = "FILE1.TXT.TMP" };
				var file2 = new FtpListItem { Type = FtpFileSystemObjectType.File, Name = "FILE2.TXT", Size = 1000, Modified = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc) };
				stubFtpClient.Stub(x => x.GetListing(Arg.Is(folder), Arg<FtpListOption>.Is.Anything)).Return(new FtpListItem[] { dir, file1, file2 });

				var result = ftpTransferrer.ListFilesAsync(folder, fileMask, log, CancellationToken.None).Result;

				Assert.AreEqual(2, result.Count);
				Assert.AreEqual(result[0].Name, file1.Name);
				Assert.AreEqual(result[0].Size, file1.Size);
				Assert.AreEqual(result[0].Timestamp, file1.Modified);
				Assert.AreEqual(result[1].Name, file2.Name);
				Assert.AreEqual(result[1].Size, file2.Size);
				Assert.AreEqual(result[1].Timestamp, file2.Modified);

				ftpTransferrer.CloseAsync(log, CancellationToken.None).Wait();
			}
		}

		[TestMethod]
		public void FtpTransferrer_GetFile()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();

				string path = "PATH";
				var source = CreatePaddedStream(500);
				var sourceCopy = new MemoryStream(source.ToArray());
				stubFtpClient.Stub(x => x.OpenRead(Arg.Is(path))).Return(source);

				var result = ftpTransferrer.GetFileAsync(path, log, CancellationToken.None).Result;

				Assert.AreEqual(new StreamReader(sourceCopy).ReadToEnd(), new StreamReader(result).ReadToEnd());

				ftpTransferrer.CloseAsync(log, CancellationToken.None).Wait();
			}
		}

		[TestMethod]
		public void FtpTransferrer_GetFileAbort()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();

				string path = "PATH";
				var source = CreatePaddedStream(5000);
				var stubFtpStream = MockRepository.GenerateStub<Stream>();
				var cancelTokenSource = new CancellationTokenSource();
				stubFtpClient.Stub(x => x.OpenRead(path)).Return(stubFtpStream);
				//stubFtpClient.Stub(x => x.IsConnected).Return(true);
				stubFtpStream.Stub(x => x.ReadAsync(Arg<byte[]>.Is.Anything, Arg.Is(0), Arg<int>.Is.Anything, Arg<CancellationToken>.Is.Anything))
					.Do(new Func<byte[], int, int, CancellationToken, Task<int>>((b, o, l, c) => Task.FromResult(source.Read(b, o, l))))
					.Repeat.Once();
				stubFtpStream.Stub(x => x.ReadAsync(Arg<byte[]>.Is.Anything, Arg.Is(0), Arg<int>.Is.Anything, Arg<CancellationToken>.Is.Anything))
					.Do(new Func<byte[], int, int, CancellationToken, Task<int>>((b, o, l, c) =>
					{
						cancelTokenSource.Cancel();
						return Task.FromResult(source.Read(b, o, l));
					}));
				stubFtpStream.Expect(x => x.Close()).Throw(new Exception());
				Stream result = null;

				try
				{
					result = ftpTransferrer.GetFileAsync(path, log, cancelTokenSource.Token).Result;
				}
				catch (AggregateException ex)
				{
					Assert.IsInstanceOfType(ex.InnerException, typeof(OperationCanceledException));
				}

				stubFtpStream.VerifyAllExpectations();
				Assert.IsNull(result);
			}
		}

		[TestMethod]
		public void FtpTransferrer_PutFile()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();

				string path = "PATH";
				var source = CreatePaddedStream(500);
				var result = new MemoryStream();
				stubFtpClient.Stub(x => x.OpenWrite(Arg.Is(path))).Return(result);

				ftpTransferrer.PutFileAsync(path, source, log, CancellationToken.None).Wait();

				source.Position = 0;
				Assert.AreEqual(new StreamReader(source).ReadToEnd(), Encoding.UTF8.GetString(result.ToArray()));

				ftpTransferrer.CloseAsync(log, CancellationToken.None).Wait();
			}
		}

		[TestMethod]
		public void FtpTransferrer_PutFileAbort()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();

				string path = "PATH";
				var stubSourceStream = MockRepository.GenerateStub<MemoryStream>();
				var stubFtpStream = MockRepository.GenerateStub<Stream>();
				var cancelTokenSource = new CancellationTokenSource();
				stubFtpClient.Stub(x => x.OpenWrite(path)).Return(stubFtpStream);
				stubSourceStream.Stub(x => x.ReadAsync(Arg<byte[]>.Is.Anything, Arg.Is(0), Arg<int>.Is.Anything, Arg<CancellationToken>.Is.Anything))
					.Do(new Func<byte[], int, int, CancellationToken, Task<int>>((b, o, l, c) => { cancelTokenSource.Cancel(); return Task.FromResult(1); }));
				stubFtpStream.Expect(x => x.Close()).Throw(new Exception());

				try
				{
					ftpTransferrer.PutFileAsync(path, stubSourceStream, log, cancelTokenSource.Token).Wait();
				}
				catch (AggregateException ex)
				{
					Assert.IsInstanceOfType(ex.InnerException, typeof(OperationCanceledException));
				}

				stubFtpClient.AssertWasCalled(x => x.OpenWrite(path));
				stubFtpStream.VerifyAllExpectations();
			}
		}

		[TestMethod]
		public void FtpTransferrer_RenameFile()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);
			string path = "PATH";
			string dest = "DEST";

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();
				ftpTransferrer.RenameFileAsync(path, dest, log, CancellationToken.None).Wait();
				ftpTransferrer.CloseAsync(log, CancellationToken.None).Wait();
			}

			stubFtpClient.AssertWasCalled(s => s.Rename(path, dest));
		}

		[TestMethod]
		public void FtpTransferrer_DeleteFile()
		{
			var stubFtpClient = MockRepository.GenerateStub<IFtpClient>();

			var configXml = new XmlDocument();
			configXml.LoadXml(
@"<Config>
  <Server>localhost</Server>
  <User>USERNAME</User>
  <Password>PASSWORD</Password>
</Config>");
			var log = new TraceLoggerFactoryAdapter().GetLogger(typeof(FtpTransferrerTests).Name);
			string path = "PATH";

			using (var ftpTransferrer = new FtpTransferrer(() => stubFtpClient))
			{
				ftpTransferrer.OpenAsync(configXml, log, CancellationToken.None).Wait();
				ftpTransferrer.DeleteFileAsync(path, log, CancellationToken.None).Wait();
				ftpTransferrer.CloseAsync(log, CancellationToken.None).Wait();
			}

			stubFtpClient.AssertWasCalled(s => s.DeleteFile(path));
		}
	}
}
