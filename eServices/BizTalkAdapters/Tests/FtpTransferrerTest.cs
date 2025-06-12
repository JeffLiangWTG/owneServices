using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using FluentFTP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class FtpTransferrerTest : TestBase
	{
		[TestMethod()]
		public void FtpTransferrer_OpenCloseTest()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				target.ReadLocationConfiguration(new XmlDocument());

				target.Open();

				stubFtpClient.AssertWasCalled(s => s.Connect());

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_ConnectionModeTest()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var modeElem = new XElement("Mode", "");
				var configXml = new XElement("Config", modeElem);
				var configDOM = new XmlDocument();

				var testMode = new Action<string, FtpDataConnectionType>((adapterMode, clientMode) =>
				{
					stubFtpClient.BackToRecord();
					stubFtpClient.Replay();
					modeElem.Value = adapterMode;
					configDOM.LoadXml(configXml.ToString());
					target.ReadLocationConfiguration(configDOM);

					target.Open();
					target.Close();

					stubFtpClient.AssertWasCalled(s => s.DataConnectionType = clientMode);
				});
				testMode("Active", FtpDataConnectionType.AutoActive);
				testMode("Passive", FtpDataConnectionType.AutoPassive);
				testMode("PASV", FtpDataConnectionType.PASV);
				testMode("PASVEX", FtpDataConnectionType.PASVEX);
				testMode("EPSV", FtpDataConnectionType.EPSV);
				testMode("PORT", FtpDataConnectionType.PORT);
				testMode("EPRT", FtpDataConnectionType.EPRT);
			}
		}

		[TestMethod()]
		public void FtpTransferrer_ListFilesWithZeroSizeTest_ClientSideFiltering()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var token = new CancellationTokenSource();
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = "URI://USR@SVR",
				Folder = "FOLDER",
				FileMask = "FILEMASK.*"
			};

			var dir = new FtpListItem()
			{
				Name = "FILEMASK.xml",
				Size = -1,
				Modified = new DateTime(2000, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.Directory
			};
			var file1 = new FtpListItem()
			{
				Name = "FILEMASK.OK",
				Size = 500,
				Modified = new DateTime(2001, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			var file2 = new FtpListItem()
			{
				Name = "FILEMASK.NOTOK",
				Size = 0,
				Modified = new DateTime(2002, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			stubFtpClient.Stub(x => x.GetListing(location.Folder, new FtpListOption())).Return(new FtpListItem[] { dir, file1, file2 });

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configXml = new XElement("Config", new XElement("UseNLST", "false"));
				var configDOM = new XmlDocument();
				configDOM.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDOM);

				target.Open();

				var result = target.ListFiles(location, receive, token).ToList();

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(result[0].Name, file1.Name);
				Assert.AreEqual(result[0].Size, file1.Size);
				Assert.AreEqual(result[0].Timestamp, file1.Modified);

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_RunAsNLST_ListFilesWithUnidentifiedSizeTest_WorstCaseFromDakosyOnly()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var token = new CancellationTokenSource();
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = "URI://USR@SVR",
				Folder = "FOLDER",
				FileMask = "FILEMASK.*"
			};

			var dir = new FtpListItem()
			{
				Name = "FILEMASK.xml",
				Size = -1,
				Modified = new DateTime(2000, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.Directory
			};
			var file1 = new FtpListItem()
			{
				Name = "FILEMASK.OK",
				Size = -1,
				Modified = new DateTime(2001, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			var file2 = new FtpListItem()
			{
				Name = "FILEMASK.NOTOK",
				Size = -1,
				Modified = new DateTime(2002, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			stubFtpClient.Stub(x => x.GetListing(location.Folder, FtpListOption.ForceNameList
																  | FtpListOption.Modify
																  | FtpListOption.Size)).Return(new FtpListItem[] { dir, file1, file2 });

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configXml = new XElement("Config", new XElement("UseNLST", "true"));
				var configDOM = new XmlDocument();
				configDOM.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDOM);

				target.Open();

				var result = target.ListFiles(location, receive, token).ToList();

				Assert.AreEqual(2, result.Count);
				Assert.AreEqual(result[0].Name, file1.Name);
				Assert.AreEqual(result[0].Size, file1.Size);
				Assert.AreEqual(result[0].Timestamp, file1.Modified);
				Assert.AreEqual(result[1].Name, file2.Name);
				Assert.AreEqual(result[1].Size, file2.Size);
				Assert.AreEqual(result[1].Timestamp, file2.Modified);

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_FlagFileTest()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var cancelTokenSource = new CancellationTokenSource();
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var folder = "FOLDER";
			var locationAAA = new TransferrerProperties.Receive.Location
			{
				Uri = uri,
				Folder = folder,
				FileMask = "*.AAA"
			};

			var locationBBB = new TransferrerProperties.Receive.Location
			{
				Uri = uri,
				Folder = folder,
				FileMask = "*.BBB"
			};

			var listing = new FtpListItem[]
			{
				new FtpListItem()
				{
					Name = "AFILE.AAA",
					Size = 500,
					Modified = new DateTime(2003, 1, 1, 12, 0, 0),
					Type = FtpFileSystemObjectType.File
				},
				new FtpListItem()
				{
					Name = "FLAG_AFILE.AAA",
					Size = 500,
					Modified = new DateTime(2001, 1, 1, 12, 0, 0),
					Type = FtpFileSystemObjectType.File
				},
				new FtpListItem()
				{
					Name = "AFILE.BBB",
					Size = 500,
					Modified = new DateTime(2002, 1, 1, 12, 0, 0),
					Type = FtpFileSystemObjectType.File
				}
			};

			stubFtpClient.Stub(x => x.GetListing(Arg<string>.Is.Anything, Arg<FtpListOption>.Is.Anything)).Return(listing);
			stubFtpClient.Stub(x => x.FileExists(Path.Combine(folder, listing[1].Name).Replace('\\', '/'))).Return(true);

			var configReceiveXml = new XElement("Config", new XElement("FlagFile", "FLAG_{f}"));
			var configDom = new XmlDocument();
			configDom.LoadXml(configReceiveXml.ToString());

			receive.ReadLocationConfiguration(configDom, "TestPortName", cancelTokenSource.Token);

			var configXml = new XElement("Config", new XElement("UseNLST", "true"));
			configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				target.ReadLocationConfiguration(configDom);
				target.Open();

				var result = target.ListFiles(locationAAA, receive, cancelTokenSource).ToList();

				Assert.AreEqual(1, result.Count);

				target.Close();
			};

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				target.ReadLocationConfiguration(configDom);
				target.Open();

				var result = target.ListFiles(locationBBB, receive, cancelTokenSource).ToList();

				Assert.AreEqual(0, result.Count);
				target.Close();
			};

		}

		[TestMethod()]
		public void FtpTransferrer_SortedFilesTest()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var cancelTokenSource = new CancellationTokenSource();
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = uri,
				Folder = "FOLDER",
				FileMask = "*.*"
			};

			var listing = new FtpListItem[]
			{
				new FtpListItem()
				{
					Name = "CCC FILEMASK.OK",
					Size = 500,
					Modified = new DateTime(2003, 1, 1, 12, 0, 0),
					Type = FtpFileSystemObjectType.File
				},
				new FtpListItem()
				{
					Name = "BBB FILENAME.OK",
					Size = 500,
					Modified = new DateTime(2001, 1, 1, 12, 0, 0),
					Type = FtpFileSystemObjectType.File
				},
				new FtpListItem()
				{
					Name = "AAA FILENAME.OK",
					Size = 500,
					Modified = new DateTime(2002, 1, 1, 12, 0, 0),
					Type = FtpFileSystemObjectType.File
				},
				new FtpListItem()
				{
					Name = "___ FILENAME.NOTOK",
					Size = 0,
					Modified = new DateTime(1995, 1, 1, 12, 0, 0),
					Type = FtpFileSystemObjectType.File
				}
			};

			stubFtpClient.Stub(x => x.GetListing(Arg<string>.Is.Anything, Arg<FtpListOption>.Is.Anything)).Return(listing);

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configReceiveXml = new XElement("Config", new XElement("SortOrder", "Timestamp")
					, new XElement("EmptyFileOption", "Discard"));
				var configDom = new XmlDocument();
				configDom.LoadXml(configReceiveXml.ToString());
				receive.ReadLocationConfiguration(configDom, "TestPortName", cancelTokenSource.Token);

				var configXml = new XElement("Config", new XElement("UseNLST", "true"));
				configDom = new XmlDocument();
				configDom.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDom);

				target.Open();
				var result = target.ListFiles(location, receive, cancelTokenSource).ToList();

				stubFtpClient.AssertWasCalled(x => x.DeleteFile(Path.Combine(location.Folder, listing[3].Name).Replace('\\', '/')));

				Assert.AreEqual(listing[1].Name, result[0].Name);
				Assert.AreEqual(listing[2].Name, result[1].Name);
				Assert.AreEqual(listing[0].Name, result[2].Name);

				Assert.AreEqual(listing[1].Modified, result[0].Timestamp);
				Assert.AreEqual(listing[2].Modified, result[1].Timestamp);
				Assert.AreEqual(listing[0].Modified, result[2].Timestamp);

				target.Close();
			};

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configReceiveXml = new XElement("Config", new XElement("SortOrder", "Name"));
				var configDom = new XmlDocument();
				configDom.LoadXml(configReceiveXml.ToString());
				receive.ReadLocationConfiguration(configDom, "TestPortName", cancelTokenSource.Token);

				var configXml = new XElement("Config", new XElement("UseNLST", "true"));
				configDom = new XmlDocument();
				configDom.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDom);

				target.Open();
				var result = target.ListFiles(location, receive, cancelTokenSource).ToList();

				Assert.AreEqual(listing[2].Name, result[0].Name);
				Assert.AreEqual(listing[1].Name, result[1].Name);
				Assert.AreEqual(listing[0].Name, result[2].Name);

				Assert.AreEqual(listing[2].Modified, result[0].Timestamp);
				Assert.AreEqual(listing[1].Modified, result[1].Timestamp);
				Assert.AreEqual(listing[0].Modified, result[2].Timestamp);

				target.Close();
			};
		}

		[TestMethod()]
		public void FtpTransferrer_ListFilesTest_ClientSideFiltering()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var token = new CancellationTokenSource();
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = "URI://USR@SVR",
				Folder = "FOLDER",
				FileMask = "FILEMASK.*"
			};

			var dir = new FtpListItem()
			{
				Name = "DIRNAME",
				Size = -1,
				Modified = new DateTime(2000, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.Directory
			};
			var file1 = new FtpListItem()
			{
				Name = "FILEMASK.OK",
				Size = 500,
				Modified = new DateTime(2001, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			var file2 = new FtpListItem()
			{
				Name = "FILENAME.NOTOK",
				Size = 500,
				Modified = new DateTime(2002, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			stubFtpClient.Expect(x => x.GetListing(location.Folder, FtpListOption.ForceNameList
																  | FtpListOption.Modify
																  | FtpListOption.Size)).Return(new FtpListItem[] { dir, file1, file2 });

			var target = MockRepository.GeneratePartialMock<FtpTransferrer>(stubFtpClient);
			target.Server = "SERVER";
			target.Port = 21;
			target.UserName = "USERNAME";
			target.Password = "PASSWORD";
			target.Logger = LogManager.GetLogger(GetType().FullName);

			var configXml = new XElement("Config", new XElement("UseNLST", "true"));
			var configDOM = new XmlDocument();
			configDOM.LoadXml(configXml.ToString());
			target.ReadLocationConfiguration(configDOM);

			target.Open();

			var result = target.ListFiles(location, receive, token).ToList();

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(result[0].Name, file1.Name);
			Assert.AreEqual(result[0].Size, file1.Size);
			Assert.AreEqual(result[0].Timestamp, file1.Modified);
			target.AssertWasCalled(
				x => x.FilterAndSort(
					Arg<TransferrerProperties.Receive.Location>.Is.Anything,
					Arg<TransferrerProperties.Receive>.Is.Anything,
					Arg<CancellationTokenSource>.Is.Anything,
					Arg<String>.Is.Anything,
					// The call to FilterAndSort should have only the files 
					// the server returns
					Arg<IEnumerable<TransferrerFileInfo>>.Matches(y => y.Count() == 2),
					Arg<Regex>.Is.NotNull
				));

			target.Close();

			stubFtpClient.VerifyAllExpectations();
			target.Dispose();
		}

		[TestMethod()]
		public void FtpTransferrer_ListFilesTest_ServerSideFiltering()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var cancellationTokenSource = new CancellationTokenSource();
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var configReceiveXml = new XElement("Config", new XElement("ServerSideFiltering", "true"));
			var configDom = new XmlDocument();
			configDom.LoadXml(configReceiveXml.ToString());
			receive.ReadLocationConfiguration(configDom, "TestPortName", cancellationTokenSource.Token);
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = "URI://USR@SVR",
				Folder = "FOLDER",
				FileMask = "FILEMASK.*"
			};

			var dir = new FtpListItem()
			{
				Name = "DIRNAME",
				Size = -1,
				Modified = new DateTime(2000, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.Directory
			};
			var file1 = new FtpListItem()
			{
				Name = "FILEMASK.OK",
				Size = 500,
				Modified = new DateTime(2001, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			var file2 = new FtpListItem()
			{
				Name = "FILENAME.NOTOK",
				Size = 500,
				Modified = new DateTime(2002, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			stubFtpClient.Expect(x => x.GetListing(location.Folder + "/" + location.FileMask, FtpListOption.ForceNameList
																  | FtpListOption.Modify
																  | FtpListOption.Size)).Return(new FtpListItem[] { file1 });


			var target = MockRepository.GeneratePartialMock<FtpTransferrer>(stubFtpClient);
			target.Server = "SERVER";
			target.Port = 21;
			target.UserName = "USERNAME";
			target.Password = "PASSWORD";
			target.Logger = LogManager.GetLogger(GetType().FullName);

			var configXml = new XElement("Config", new XElement("UseNLST", "true"));
			var configDOM = new XmlDocument();
			configDOM.LoadXml(configXml.ToString());
			target.ReadLocationConfiguration(configDOM);

			target.Open();

			var result = target.ListFiles(location, receive, cancellationTokenSource).ToList();

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(result[0].Name, file1.Name);
			Assert.AreEqual(result[0].Size, file1.Size);
			Assert.AreEqual(result[0].Timestamp, file1.Modified);
			target.AssertWasCalled(
				x => x.FilterAndSort(
					Arg<TransferrerProperties.Receive.Location>.Is.Anything,
					Arg<TransferrerProperties.Receive>.Is.Anything,
					Arg<CancellationTokenSource>.Is.Anything,
					Arg<String>.Is.Anything,
					// The call to FilterAndSort should have only the files 
					// the server returns
					Arg<IEnumerable<TransferrerFileInfo>>.Matches(y => y.Count() == 1),
					Arg<Regex>.Is.Null
				));

			target.Close();

			stubFtpClient.VerifyAllExpectations();
			target.Dispose();
		}

		[TestMethod()]
		public void FtpTransferrer_ListFilesTest_MoveWorkDir()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var token = new CancellationTokenSource();
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = "URI://USR@SVR",
				Folder = "FOLDER",
				FileMask = "FILEMASK.*"
			};

			var dir = new FtpListItem()
			{
				Name = "DIRNAME",
				Size = -1,
				Modified = new DateTime(2000, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.Directory
			};
			var file1 = new FtpListItem()
			{
				Name = "FILEMASK.OK",
				Size = 500,
				Modified = new DateTime(2001, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			var file2 = new FtpListItem()
			{
				Name = "FILENAME.NOTOK",
				Size = 500,
				Modified = new DateTime(2002, 1, 1, 12, 0, 0),
				Type = FtpFileSystemObjectType.File
			};
			stubFtpClient.Stub(x => x.GetListing("", FtpListOption.ForceNameList
													 | FtpListOption.Modify
													 | FtpListOption.Size
													 | FtpListOption.NoPath)).Return(new FtpListItem[]
				{dir, file1, file2});
			stubFtpClient.Stub(s => s.GetWorkingDirectory()).Return("/");

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configXml = new XElement("Config", new XElement("MoveWorkingDirectory", "true"), new XElement("UseNLST", "true"));
				var configDOM = new XmlDocument();
				configDOM.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDOM);

				target.Open();

				var result = target.ListFiles(location, receive, token).ToList();

				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("FOLDER"));
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("/"));
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(result[0].Name, file1.Name);
				Assert.AreEqual(result[0].Size, file1.Size);
				Assert.AreEqual(result[0].Timestamp, file1.Modified);

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_ListFilesTest_BlankFileMask()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var token = new CancellationTokenSource();
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = "URI://USR@SVR",
				Folder = "FOLDER",
				FileMask = String.Empty
			};
			var dir = new FtpListItem() { Name = "DIRNAME", Size = -1, Modified = new DateTime(2000, 1, 1, 12, 0, 0), Type = FtpFileSystemObjectType.Directory };
			var file1 = new FtpListItem() { Name = "FILE1.OK", Size = 500, Modified = new DateTime(2001, 1, 1, 12, 0, 0), Type = FtpFileSystemObjectType.File };
			var file2 = new FtpListItem() { Name = "FILE2.OK", Size = 1500, Modified = new DateTime(2002, 1, 1, 12, 0, 0), Type = FtpFileSystemObjectType.File };
			stubFtpClient.Stub(x => x.GetListing(location.Folder, FtpListOption.ForceNameList
													 | FtpListOption.Modify
													 | FtpListOption.Size)).Return(new FtpListItem[] { dir, file1, file2 });

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configXml = new XElement("Config", new XElement("UseNLST", "true"));
				var configDOM = new XmlDocument();
				configDOM.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDOM);

				target.Open();

				var result = target.ListFiles(location, receive, token).ToList();

				Assert.AreEqual(2, result.Count);
				Assert.AreEqual(result[0].Name, file1.Name);
				Assert.AreEqual(result[0].Size, file1.Size);
				Assert.AreEqual(result[0].Timestamp, file1.Modified);
				Assert.AreEqual(result[1].Name, file2.Name);
				Assert.AreEqual(result[1].Size, file2.Size);
				Assert.AreEqual(result[1].Timestamp, file2.Modified);

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_GetFileTest()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			string path = "PATH";
			var source = CreatePaddedStream(500);
			var sourceCopy = new MemoryStream(source.ToArray());
			var stubSource = MockRepository.GenerateMock<FtpDataStream>();
			stubSource.Stub(x => x.Read(Arg<byte[]>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Do(new Func<byte[], int, int, int>((b, o, c) => { return source.Read(b, o, c); }));
			stubFtpClient.Stub(x => x.OpenRead(path)).Return(stubSource);

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				target.ReadLocationConfiguration(new XmlDocument());

				target.Open();
				var result = target.GetFile(path);

				sourceCopy.Position = 0;
				Assert.AreEqual(new StreamReader(sourceCopy).ReadToEnd(), new StreamReader(result).ReadToEnd());

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_GetFileTest_MoveWorkDir()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			string path = "/DIR/FILE.NAM";
			var source = CreatePaddedStream(500);
			var sourceCopy = new MemoryStream(source.ToArray());
			var stubSource = MockRepository.GenerateMock<FtpDataStream>();
			stubSource.Stub(x => x.Read(Arg<byte[]>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Do(new Func<byte[], int, int, int>((b, o, c) => { return source.Read(b, o, c); }));
			stubFtpClient.Stub(x => x.OpenRead("FILE.NAM")).Return(stubSource);
			stubFtpClient.Stub(x => x.GetWorkingDirectory()).Return("/DIR");

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configXml = new XElement("Config", new XElement("MoveWorkingDirectory", "true"));
				var configDOM = new XmlDocument();
				configDOM.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDOM);

				target.Open();
				var result = target.GetFile(path);

				stubFtpClient.AssertWasNotCalled(x => x.SetWorkingDirectory(Arg<string>.Is.Anything));
				sourceCopy.Position = 0;
				Assert.AreEqual(new StreamReader(sourceCopy).ReadToEnd(), new StreamReader(result).ReadToEnd());

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_GetFileAbortTest()
		{
			string path = "PATH";
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var stubStream = MockRepository.GenerateStub<FtpDataStream>();
			stubFtpClient.Stub(x => x.OpenRead(path)).Return(stubStream);
			stubStream.Expect(x => x.Close()).Throw(new Exception());

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				stubStream.Stub(x => x.Read(new byte[4096], 0, 4096)).Do(new Func<byte[], int, int, int>((b, o, c) => { target.Dispose(); return 1000; }));

				target.ReadLocationConfiguration(new XmlDocument());

				target.Open();
				var result = target.GetFile(path);

				stubStream.VerifyAllExpectations();
				Assert.IsNull(result);
			}
		}

		[TestMethod()]
		public void FtpTransferrer_PutFileTest()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			string path = "PATH";
			var source = CreatePaddedStream(500);
			var result = new MemoryStream();
			var stubSftpStream = MockRepository.GenerateStub<FtpDataStream>();
			stubFtpClient.Stub(x => x.OpenWrite(path)).Return(stubSftpStream);
			stubSftpStream.Stub(x => x.Write(null, 0, 0)).IgnoreArguments().Do(new Action<byte[], int, int>((b, o, l) => result.Write(b, o, l)));

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				target.ReadLocationConfiguration(new XmlDocument());

				target.Open();
				target.PutFile(path, source);

				source.Position = 0;
				result.Position = 0;
				Assert.AreEqual(new StreamReader(source).ReadToEnd(), new StreamReader(result).ReadToEnd());

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_PutFileTest_MoveWorkDir()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			string path = "/PATH/FILE.NAM";
			var source = CreatePaddedStream(500);
			var result = new MemoryStream();
			var stubSftpStream = MockRepository.GenerateStub<FtpDataStream>();
			stubFtpClient.Stub(x => x.OpenWrite("FILE.NAM")).Return(stubSftpStream);
			stubSftpStream.Stub(x => x.Write(null, 0, 0)).IgnoreArguments().Do(new Action<byte[], int, int>((b, o, l) => result.Write(b, o, l)));
			stubFtpClient.Stub(x => x.GetWorkingDirectory()).Return("/DIR");

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configXml = new XElement("Config", new XElement("MoveWorkingDirectory", "true"));
				var configDOM = new XmlDocument();
				configDOM.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDOM);

				target.Open();
				target.PutFile(path, source);

				stubFtpClient.AssertWasCalled(x => x.SetWorkingDirectory("/PATH"));
				stubFtpClient.AssertWasCalled(x => x.SetWorkingDirectory("/DIR"));
				source.Position = 0;
				result.Position = 0;
				Assert.AreEqual(new StreamReader(source).ReadToEnd(), new StreamReader(result).ReadToEnd());

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_PutFileAbortTest()
		{
			string path = "PATH";
			var buffer = new byte[4096];
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			var stubSourceStream = MockRepository.GenerateStub<MemoryStream>();
			var stubTargetStream = MockRepository.GenerateStub<FtpDataStream>();
			stubFtpClient.Stub(x => x.OpenWrite(path)).Return(stubTargetStream);
			stubTargetStream.Expect(x => x.Close()).Throw(new Exception());

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				target.ReadLocationConfiguration(new XmlDocument());

				stubSourceStream.Stub(x => x.Read(buffer, 0, buffer.Length)).Do(new Func<byte[], int, int, int>((b, o, c) => { target.Dispose(); return buffer.Length; }));

				target.Open();
				target.PutFile(path, stubSourceStream);

				stubFtpClient.AssertWasCalled(x => x.OpenWrite(path));
				stubTargetStream.VerifyAllExpectations();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_RenameFileTest()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			string path = "PATH";
			string dest = "DEST";

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				target.ReadLocationConfiguration(new XmlDocument());

				target.Open();
				target.RenameFile(path, dest);

				stubFtpClient.AssertWasCalled(s => s.Rename(path, dest));

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_RenameFileTest_MoveWorkDir()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			stubFtpClient.Stub(x => x.GetWorkingDirectory()).Return("/DIR");

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configXml = new XElement("Config", new XElement("MoveWorkingDirectory", "true"));
				var configDOM = new XmlDocument();
				configDOM.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDOM);

				target.Open();

				target.RenameFile("PATH/FILE.NAM", "PATH/DEST/FILE.NAM");
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("PATH"));
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("/DIR"));
				stubFtpClient.AssertWasCalled(s => s.Rename("FILE.NAM", "DEST/FILE.NAM"));

				target.RenameFile("PATH/SOURCE/FILE.NAM", "PATH/FILE.NAM");
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("PATH/SOURCE"));
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("/DIR"));
				stubFtpClient.AssertWasCalled(s => s.Rename("FILE.NAM", "../FILE.NAM"));

				target.RenameFile("PATH/FILE.NAM", "DEST/FILE.NAM");
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("PATH"));
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("/DIR"));
				stubFtpClient.AssertWasCalled(s => s.Rename("FILE.NAM", "../DEST/FILE.NAM"));

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_DeleteFileTest()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			string path = "PATH";
			stubFtpClient.Stub(x => x.FileExists(path)).Return(true);

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				target.ReadLocationConfiguration(new XmlDocument());

				target.Open();
				target.DeleteFile(path);

				stubFtpClient.AssertWasCalled(s => s.DeleteFile(path));

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrer_DeleteFileTest_MoveWorkDir()
		{
			var stubFtpClient = MockRepository.GenerateMock<IFtpClient, IDisposable>();
			string path = "PATH/TEMP/FILE.NAM";
			stubFtpClient.Stub(x => x.GetWorkingDirectory()).Return("/DIR");
			stubFtpClient.Stub(x => x.FileExists("FILE.NAM")).Return(true);

			using (var target = new FtpTransferrer(stubFtpClient)
			{
				Server = "SERVER",
				Port = 21,
				UserName = "USERNAME",
				Password = "PASSWORD",
				Logger = LogManager.GetLogger(GetType().FullName)
			})
			{
				var configXml = new XElement("Config", new XElement("MoveWorkingDirectory", "true"));
				var configDOM = new XmlDocument();
				configDOM.LoadXml(configXml.ToString());
				target.ReadLocationConfiguration(configDOM);

				target.Open();
				target.DeleteFile(path);

				stubFtpClient.AssertWasCalled(s => s.DeleteFile("FILE.NAM"));
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("PATH/TEMP"));
				stubFtpClient.AssertWasCalled(s => s.SetWorkingDirectory("/DIR"));

				target.Close();
			}
		}

		[TestMethod()]
		public void FtpTransferrerAbstract_IssueManager_DownloadLimitTest()
		{
			/*
			 * Checks that an issue is logged when the DownloadLimit of unprocessed files is exceeded
			 * and that it an issue is NOT logged otherwise.
			 */
			string uri = "URI://USR@SVR";
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = "URI://USR@SVR",
				Folder = "FOLDER",
				FileMask = "*.doc"
			};

			var receive = new TransferrerProperties.Receive(uri);
			var configReceiveXml = new XElement("Config", new XElement("DownloadExcludedFilesLimit", 105));
			var configDom = new XmlDocument();
			var cancellationTokenSource = new CancellationTokenSource();
			configDom.LoadXml(configReceiveXml.ToString());
			receive.ReadLocationConfiguration(configDom, "TestPortName", cancellationTokenSource.Token);

			var files = new List<TransferrerFileInfo>();
			for (int i = 0; i < (receive.DownloadExcludedFilesLimit + 1); i++)
			{
				files.Add(new TransferrerFileInfo()
				{
					Name = "ExampleFilename.txt",
					Size = 124,
					Timestamp = DateTime.Now
				});
			}
			files.Add(new TransferrerFileInfo()
			{
				Name = "ExampleFilename.doc",
				Size = 421,
				Timestamp = DateTime.Now
			});

			var ftpTransferrer = MockRepository.GeneratePartialMock<FtpTransferrerAbstract>();
			// Comment out the next line to actually send an issue to the issue manager
			ftpTransferrer.Stub(x => x.ReportToIssueManager(Arg<String>.Is.Anything, Arg<String>.Is.Anything, Arg<TransferrerProperties.Receive>.Is.Anything, Arg<TransferrerProperties.Receive.Location>.Is.Anything, Arg<Exception>.Is.Anything, Arg<CancellationTokenSource>.Is.Anything));
			ftpTransferrer.Stub(x => x.GetListing(Arg<string>.Is.Anything)).Return(files.GetRange(0, 50)).Repeat.Once();
			ftpTransferrer.Stub(x => x.GetListing(Arg<string>.Is.Anything)).Return(files).Repeat.Once();
			
			// Test that an issue was raised when the download limit was not exceeded
			ftpTransferrer.ListFiles(location, receive, cancellationTokenSource);
			ftpTransferrer.AssertWasNotCalled(x => x.ReportToIssueManager(Arg<String>.Is.Anything, Arg<String>.Is.Anything, Arg<TransferrerProperties.Receive>.Is.Anything, Arg<TransferrerProperties.Receive.Location>.Is.Anything, Arg<Exception>.Is.Anything, Arg<CancellationTokenSource>.Is.Anything));

			//// Test that an issue was raised when the download limit was exceeded
			ftpTransferrer.ListFiles(location, receive, cancellationTokenSource);
			ftpTransferrer.AssertWasCalled(x => x.ReportToIssueManager(Arg<String>.Is.Anything, Arg<String>.Is.Anything, Arg<TransferrerProperties.Receive>.Is.Anything, Arg<TransferrerProperties.Receive.Location>.Is.Anything, Arg<Exception>.Is.NotNull, Arg<CancellationTokenSource>.Is.Anything));
		}


		IEnumerable<TransferrerFileInfo> GetFilesSlowly(TimeSpan delayLength)
		{
			yield return new TransferrerFileInfo()
			{
				Name = "ExampleFilename1.doc",
				Size = 421,
				Timestamp = DateTime.Now
			};


			yield return new TransferrerFileInfo()
			{
				Name = "ExampleFilename2.doc",
				Size = 422,
				Timestamp = DateTime.Now
			};

			Thread.Sleep((int)delayLength.TotalMilliseconds);

			yield return new TransferrerFileInfo()
			{
				Name = "ExampleFilename31.doc",
				Size = 423,
				Timestamp = DateTime.Now
			};


		}





		[TestMethod()]
		public void FtpTransferrerAbstract_IssueManager_SlowWarningTimeTest()
		{
			/*
			 * Checks that an issue is logged when the Listing of files takes too long
			 * and that it an issue is NOT logged otherwise.
			 */
			string uri = "URI://USR@SVR";
			var location = new TransferrerProperties.Receive.Location
			{
				Uri = "URI://USR@SVR",
				Folder = "FOLDER",
				FileMask = "*.doc"
			};

			var receive = new TransferrerProperties.Receive(uri);
			var configReceiveXml = new XElement("Config", new XElement("ReceiveProcessingTimeWarning", 2));
			var configDom = new XmlDocument();
			var cancellationTokenSource = new CancellationTokenSource();
			configDom.LoadXml(configReceiveXml.ToString());
			receive.ReadLocationConfiguration(configDom, "TestPortName", cancellationTokenSource.Token);


			var ftpTransferrer = MockRepository.GeneratePartialMock<FtpTransferrerAbstract>();
			ftpTransferrer.Stub(x => x.GetListing(Arg<string>.Is.Anything)).Return(GetFilesSlowly(TimeSpan.FromSeconds(1))).Repeat.Once();
			ftpTransferrer.Stub(x => x.GetListing(Arg<string>.Is.Anything)).Return(GetFilesSlowly(TimeSpan.FromSeconds(3))).Repeat.Once();
			// Comment out the next line to actually send an issue to the issue manager
			ftpTransferrer.Stub(x => x.ReportToIssueManager(Arg<String>.Is.Anything, Arg<String>.Is.Anything, Arg<TransferrerProperties.Receive>.Is.Anything, Arg<TransferrerProperties.Receive.Location>.Is.Anything, Arg<Exception>.Is.Anything, Arg<CancellationTokenSource>.Is.Anything));

			//// Test that an issue was not raised when the listing does not take too long.
			ftpTransferrer.ListFiles(location, receive, cancellationTokenSource);
			ftpTransferrer.AssertWasNotCalled(x => x.ReportToIssueManager(Arg<String>.Is.Anything, Arg<String>.Is.Anything, Arg<TransferrerProperties.Receive>.Is.Anything, Arg<TransferrerProperties.Receive.Location>.Is.Anything, Arg<Exception>.Is.Anything, Arg<CancellationTokenSource>.Is.Anything));

			//// Test that an issue was raised when the listing takes too long
			ftpTransferrer.ListFiles(location, receive, cancellationTokenSource);
			ftpTransferrer.AssertWasCalled(x => x.ReportToIssueManager(Arg<String>.Is.Anything, Arg<String>.Is.Anything, Arg<TransferrerProperties.Receive>.Is.Anything, Arg<TransferrerProperties.Receive.Location>.Is.Anything, Arg<Exception>.Is.NotNull, Arg<CancellationTokenSource>.Is.Anything));
		}

	}
}
