// Please do CH0/CHS/CHP, if want to restore the tests.

//using System;
//using System.IO;
//using System.Reflection;
//using System.Text;
//using Moq;
//using NUnit.Framework;
//using Xtrade.Core.Objects;

//namespace XT.Internal.API.Tests
//{
//	[TestFixture]
//	public class Tests
//	{
//		readonly MockRepository repository = new MockRepository(MockBehavior.Default);
//		XtProxy proxy;
//		Mock<IXtDataAdapter> adapter;
//		XtradeServer server;
//		Mock<IXtDataStream> stream;

//		const string serverName = "TestServer";
//		const int streamId = 1;
//		readonly DateTime now = DateTime.Now;

//		[SetUp]
//		public void SetUp()
//		{
//			adapter = repository.Create<IXtDataAdapter>();
//			stream = repository.Create<IXtDataStream>();

//			server = new XtradeServer()
//			{
//				Name = serverName,
//				StreamId = streamId
//			};
//			adapter.Setup(x => x.ListServers()).Returns(new[] { server });
//			adapter.Setup(x => x.GetStream(streamId)).Returns(stream.Object);
//			stream.SetupGet(x => x.StreamId).Returns(streamId);
//			stream.SetupGet(x => x.RootFolder).Returns(new XtObject() { Id = Guid.Empty });

//			proxy = new XtProxy((owner) => null, (intercom) => adapter.Object, () => now, serverName);
//		}

//		[Test]
//		public void Test_Import_EmptyStream_SingleXview_Success()
//		{
//			var dataBeingImported = ReadStreamToEndAndDispose(ReadEmbeddedResource("Files.1.json"));
//			var clonedStream = repository.Create<IXtDataStream>();
//			var clonedStreamName = $"Clone {now}";
//			var clonedStreamId = 2;
//			clonedStream.SetupGet(x => x.StreamId).Returns(clonedStreamId);
//			clonedStream.SetupGet(x => x.RootFolder).Returns(new XtObject() { Id = Guid.Empty });

//			stream.Setup(x => x.GetObjects(It.IsAny<QSettings>(), It.IsAny<LSettings>())).Returns(new XtObjectCollection());
//			adapter.SetupSequence(x => x.GetStreamsInfo())
//				.Returns(new[] { 
//					new XtradeStream()
//					{
//						Id = streamId,
//						Name = "default"
//					}
//				})
//				.Returns(new[] { 
//					new XtradeStream()
//					{
//						Id = streamId,
//						Name = "default"
//					},
//					new XtradeStream()
//					{
//						Id = clonedStreamId,
//						Name = clonedStreamName
//					}
//				});
//			adapter.Setup(x => x.CreateStream(-1L, clonedStreamName, "Deployment")).Returns(clonedStreamId);
//			adapter.Setup(x => x.GetStream(clonedStreamId)).Returns(clonedStream.Object);
//			var contents = "Let's imagine this is what would be exported from an empty stream :)";
//			stream.Setup(x => x.ExportConfiguration(Array.Empty<Guid>(), It.IsAny<ExportOptions>())).Returns(contents);
//			clonedStream.Setup(x => x.ImportConfiguration(contents, Guid.Empty, It.IsAny<ImportOptions>()));
//			adapter.Setup(x => x.SaveServer(server, false));
//			stream.Setup(x => x.ImportConfiguration(dataBeingImported, Guid.Empty, It.IsAny<ImportOptions>()));

//			var report = new StringBuilder();
//			proxy.ImportConfiguration(dataBeingImported, report, out var relations);
//			Assert.That(server.StreamId, Is.EqualTo(streamId));
//			Assert.That(relations.Count, Is.EqualTo(0));
//			Assert.That(report.ToString().Contains($"Stream [Clone {now}] was cloned from [default] stream."));
//		}

//		public static Stream ReadEmbeddedResource(string resourceName)
//		{
//			var targetAssembly = Assembly.GetCallingAssembly();
//			var fullResourceName = targetAssembly.GetName().Name + '.' + resourceName;
//			var resource = targetAssembly.GetManifestResourceStream(fullResourceName);
//			if (resource == null)
//			{
//				throw new Exception($"Could not locate embedded resource '{fullResourceName}'");
//			}
//			return resource;
//		}

//		public static string ReadStreamToEndAndDispose(Stream stream)
//		{
//			stream.Seek(0, SeekOrigin.Begin);
//			using (var reader = new StreamReader(stream))
//			{
//				return reader.ReadToEnd();
//			}
//		}
//	}
//}
