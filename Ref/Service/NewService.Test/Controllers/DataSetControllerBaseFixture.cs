using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.NewService.Controllers;
using CargoWise.RefDbRepo.NewService.Logging;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Common.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	class DataSetControllerBaseFixture
	{
		[Test]
		public void GetDataStream_TotalRecordsDelivered()
		{
			var response = new Mock<HttpResponse>();
			response.SetupGet(x => x.Body).Returns(new MemoryStream());
			var httpContext = new Mock<HttpContext>();
			httpContext.SetupGet(a => a.Response).Returns(response.Object);
			var controller = new DummyDataSetController(service.Object, adaptor.Object, cacheHelper.Object, clientRecord.Object, cacheWrapper.Object, logHelper.Object);
			controller.ControllerContext.HttpContext = httpContext.Object;
			controller.GetDataStream(new DataSetGet(null, new DateTime(2023, 8, 18), "0_39_9", "RefDataSet"));
			logHelper.Verify(x => x.LogInfo(null, @"No. of records delivered: 0. Key: {""LowerTimestamp"":null,""UpperTimestamp"":""2023-08-18T00:00:00"",""Checkpoint"":null,""DataSet"":""RefDataSet"",""Version"":2}", null));
		}


		[Test]
		public void GetDataStreamV2()
		{
			adaptor.Setup(x => x.ParseVersion("0_39_9")).Returns(Tuple.Create(0, 39, 9));
			adaptor.Setup(x => x.RequireTransform("0_39_9", typeof(RefDataSet))).Returns(false);
			string[] filePaths = null;
			filePaths = new[] { Path.GetTempFileName() };
			cacheHelper.Setup(x => x.GetCachePaths(It.IsAny<IDataBlockKey<RefDataSet>>(), It.IsAny<long>()))
			.Returns(filePaths.Select((x, i) => (x, i.ToString(CultureInfo.InvariantCulture))));
			var key = new Mock<DataBlockKey<RefDataSet>>();
			var logHelper = new Mock<ILogHelper>();

			var controller = new DummyDataSetController(service.Object, adaptor.Object, cacheHelper.Object, clientRecord.Object, cacheWrapper.Object, logHelper.Object);
			using (var fs = File.Open(filePaths[0], FileMode.Open, FileAccess.Write))
			{
				Assert.ThrowsAsync<IOException>(async () => await controller.GetRawResponse(It.IsAny<DataBlockKey<RefDataSet>>()));
				try
				{
					var exception = controller.GetDataStreamV2(new DataSetGet(null, new DateTime(2023, 8, 18), "0_39_9", "RefDataSet"));
				}
				catch (Exception ex)
				{
					Assert.AreNotEqual(-2147024864, ex.HResult);
				}
				logHelper.Verify(x => x.LogInfo(null, @"Cache does not exist. Key: {""LowerTimestamp"":null,""UpperTimestamp"":""2023-08-18T00:00:00"",""Checkpoint"":null,""DataSet"":""RefDataSet"",""Version"":2}", null));
				logHelper.Verify(x => x.LogInfo(null, "Getting data from RefDataSet service.", null));
			}

			if (filePaths != null)
			{
				Array.ForEach(filePaths, x => File.Delete(x));
			}
		}

		[Test]
		public async Task GetDataStream_RawResponse()
		{
			adaptor.Setup(x => x.ParseVersion("0_39_9")).Returns(Tuple.Create(0, 39, 9));
			adaptor.Setup(x => x.RequireTransform("0_39_9", typeof(RefDataSet))).Returns(false);
			string[] filePaths = null;

			try
			{
				filePaths = new[] { Path.GetTempFileName(), Path.GetTempFileName() };
				File.WriteAllText(filePaths[0], "A");
				File.WriteAllText(filePaths[1], "B");
				cacheHelper.Setup(x => x.GetCachePaths(It.IsAny<IDataBlockKey<RefDataSet>>(), It.IsAny<long>()))
				.Returns(filePaths.Select((x, i) => (x, i.ToString(CultureInfo.InvariantCulture))));

				using (var factory = GetTestServerFactory<RefDataSet>(adaptor.Object, logWrapper.Object, logHelper.Object, null, null, cacheHelper.Object, null, null))
				using (var server = factory.Server)
				{
					var context = await server.SendAsync(c =>
						{
							c.Request.Method = HttpMethods.Post;
							c.Request.Path = new PathString("/DummyDataSet/GetDataStreamV2");
							c.Request.ContentType = "application/json";
							c.Request.SetBody(JsonConvert.SerializeObject(new DataSetGet(null, new DateTime(2023, 8, 18), "0_39_9", "RefDataSet")));
						});
					var response = context.Response;
					Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
					Assert.That(response.ContentType.StartsWith("multipart/mixed"));
					var content = response.GetBody();
					Assert.False(string.IsNullOrEmpty(content));
					Assert.That(content.Contains($"Content-Disposition: attachment; name=0; filename={Path.GetFileName(filePaths[0])}\r\nContent-Type: application/zip\r\n\r\nA"));
					Assert.That(content.Contains($"Content-Disposition: attachment; name=1; filename={Path.GetFileName(filePaths[1])}\r\nContent-Type: application/zip\r\n\r\nB"));
					Assert.That(content.Contains($"Content-Type: text/plain; charset=utf-8\r\nContent-Disposition: attachment; name=Checkpoint\r\n\r\n1"));

					logHelper.Verify(x => x.LogInfo("XX", @"Cache exists. Key: {""LowerTimestamp"":null,""UpperTimestamp"":""2023-08-18T00:00:00"",""Checkpoint"":null,""DataSet"":""RefDataSet"",""Version"":2}", "/DummyDataSet/GetDataStreamV2"));
					logHelper.Verify(x => x.LogInfo("XX", "Getting data from RefDataSet service.", "/DummyDataSet/GetDataStreamV2"), Times.Never);
				}
			}
			finally
			{
				if (filePaths != null)
				{
					Array.ForEach(filePaths, x => File.Delete(x));
				}
			}
		}

		[Test]
		public async Task GetDataStream_RawResponse_NoData()
		{
			adaptor.Setup(x => x.RequireTransform("0_39_9", typeof(RefDataSet))).Returns(true);
			cacheHelper.Setup(x => x.GetCachePaths(It.IsAny<IDataBlockKey<RefDataSet>>(), It.IsAny<long>()))
			.Returns(Enumerable.Empty<(string, string)>());

			using (var factory = GetTestServerFactory<RefDataSet>(adaptor.Object, logWrapper.Object, logHelper.Object, null, null, cacheHelper.Object, null, null))
			using (var server = factory.Server)
			{
				var context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/DummyDataSet/GetDataStreamV2");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(new DataSetGet(null, new DateTime(2023, 8, 18), "0_39_9", "RefDataSet")));
				});
				var response = context.Response;
				Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
				Assert.That(string.IsNullOrEmpty(response.GetBody()));
				logHelper.Verify(x => x.LogInfo("XX", @"Cache does not exist. Key: {""LowerTimestamp"":null,""UpperTimestamp"":""2023-08-18T00:00:00"",""Checkpoint"":null,""DataSet"":""RefDataSet"",""Version"":2}", "/DummyDataSet/GetDataStreamV2"));
				logHelper.Verify(x => x.LogInfo("XX", "Getting data from RefDataSet service.", "/DummyDataSet/GetDataStreamV2"));
			}
		}

		[Test]
		public async Task LastCheckpoint()
		{
			adaptor.Setup(x => x.ParseVersion(It.IsAny<string>())).Returns(Tuple.Create(0, 1, 9));

			using (var factory = GetTestServerFactory(adaptor.Object, null, logHelper.Object, null, cacheWrapper.Object, cacheHelper.Object, null, service.Object))
			using (var server = factory.Server)
			{
				var checkpointPK = Guid.NewGuid();
				var checkpoint = CheckpointHelper.Create(checkpointPK);
				var context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/DummyDataSet/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(new DataSetGet(null, new DateTime(2016, 10, 31), "1", "RefDataSet") { Checkpoint = checkpoint.ToString() }));
				});
				var response = context.Response;
				Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
				service.Verify(x => x.GetData(It.IsAny<DateTime?>(), It.IsAny<DateTime>(), It.Is<ICheckpoint>(y => y.ToString() == checkpoint.ToString()), It.IsAny<int?>(), It.IsAny<short>()));
				logHelper.Verify(x => x.LogInfo("XX", $@"Cache does not exist. Key: {{""LowerTimestamp"":null,""UpperTimestamp"":""2016-10-31T00:00:00"",""Checkpoint"":""{{_""DataSetPK_"":_""{checkpointPK}_""}}"",""DataSet"":""RefDataSet"",""Version"":2}}", "/DummyDataSet/GetDataStream"));
				logHelper.Verify(x => x.LogInfo("XX", "Getting data from RefDataSet service.", "/DummyDataSet/GetDataStream"));
			}
		}

		[Test]
		public async Task RecordClient()
		{
			var controller = new DummyDataSetController(service.Object, adaptor.Object, cacheHelper.Object, clientRecord.Object, cacheWrapper.Object, logHelper.Object);
			var now = DateTime.UtcNow;
			await controller.Report("XX", JsonConvert.SerializeObject(now), null, null, "PRO");
			clientRecord.Verify(x => x.RecordAsync("RefDataSet", "XX", "PRO", now, null, true));
		}

		[Test]
		public async Task GetCacheDataFirst()
		{
			var dummy1 = new RefDataSet();
			var dummy2 = new RefDataSet();
			var key = new Mock<IDataBlockKey<RefDataSet>>();
			var checkpointPK1 = Guid.NewGuid();
			dummy2.Checkpoint = CheckpointHelper.Create(checkpointPK1).ToString();
			var dummy3 = new RefDataSet();
			var dummy4 = new RefDataSet();
			var checkpointPK2 = Guid.NewGuid();
			dummy4.Checkpoint = CheckpointHelper.Create(checkpointPK2).ToString();
			cacheHelper.Setup(x => x.GetSequentialKeys(It.IsAny<IDataBlockKey<RefDataSet>>())).Returns(new[] { key.Object }.AsEnumerable());
			cacheHelper.Setup(x => x.GetData(It.IsAny<IDataBlockKey<RefDataSet>>())).Returns(new[] { dummy1, dummy2 }.AsEnumerable());
			service.Setup(x => x.GetData(It.IsAny<DateTime?>(), It.IsAny<DateTime>(), It.IsAny<ICheckpoint>(), It.IsAny<int?>(), It.IsAny<short>())).Returns(new[] { dummy3, dummy4 }.AsEnumerable());
			adaptor.Setup(x => x.ParseVersion(It.IsAny<string>())).Returns(Tuple.Create(0, 1, 9));

			using (var factory = GetTestServerFactory(adaptor.Object, null, logHelper.Object, null, cacheWrapper.Object, cacheHelper.Object, null, service.Object))
			using (var server = factory.Server)
			{
				var context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/DummyDataSet/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(new DataSetGet(null, new DateTime(2016, 10, 31), "1", "RefDataSet")));
				});
				var response = context.Response;
				Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
				service.Verify(x => x.GetData(null, new DateTime(2016, 10, 31), It.Is<ICheckpoint>(y => y.ToString() == dummy2.Checkpoint), It.IsAny<int>(), It.IsAny<short>()), Times.AtLeast(2));
				cacheHelper.Verify(x => x.WriteToCache(new[] { dummy3, dummy4 }, It.IsAny<DataBlockKey<RefDataSet>>(), It.IsAny<Func<RefDataSet, bool>>()));
			}
		}

		[Test]
		public async Task CompressionTest()
		{
			var key = new Mock<IDataBlockKey<RefDataSet>>();
			var checkpoint = CheckpointHelper.Create(Guid.NewGuid()).ToString();

			var dummy1 = new RefDataSet { Checkpoint = checkpoint };
			var dummy2 = new RefDataSet { Checkpoint = checkpoint };
			var dummy3 = new RefDataSet { Checkpoint = checkpoint };
			var dummy4 = new RefDataSet { Checkpoint = checkpoint };
			var dummy5 = new RefDataSet { Checkpoint = checkpoint };

			cacheHelper.Setup(x => x.GetSequentialKeys(It.IsAny<IDataBlockKey<RefDataSet>>()))
				.Returns(new[] { key.Object }.AsEnumerable());
			cacheHelper.Setup(x => x.GetData(It.IsAny<IDataBlockKey<RefDataSet>>()))
				.Returns(new[] { dummy1, dummy2, dummy3, dummy4, dummy5 }.AsEnumerable());

			service.Setup(x => x.GetData(It.IsAny<DateTime?>(), It.IsAny<DateTime>(), It.IsAny<ICheckpoint>(), It.IsAny<int?>(), It.IsAny<short>()))
				.Returns(new[] { dummy1, dummy2, dummy3, dummy4 }.AsEnumerable());
			adaptor.Setup(x => x.ParseVersion(It.IsAny<string>())).Returns(Tuple.Create(0, 1, 9));

			using (var factory = GetTestServerFactory(adaptor.Object, null, logHelper.Object, null, cacheWrapper.Object, cacheHelper.Object, null, service.Object))
			using (var server = factory.Server)
			{
				var context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/DummyDataSet/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(new DataSetGet(null, new DateTime(2016, 10, 31), "1", "RefDataSet")));
				});
				var response = context.Response;
				Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
				var contentBuffer = new byte[100];
				var originalDataLength = await response.Body.ReadAsync(contentBuffer.AsMemory(0, 100));

				context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/DummyDataSet/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.Headers.AcceptEncoding = "gzip, deflate, br";
					c.Request.SetBody(JsonConvert.SerializeObject(new DataSetGet(null, new DateTime(2016, 10, 31), "1", "RefDataSet")));
				});
				response = context.Response;
				Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
				Assert.AreEqual(CompressionType.GZip, response.GetCompressionType());
				var compressedDataLength = await response.Body.ReadAsync(contentBuffer.AsMemory(0, 100));
				Assert.That(compressedDataLength < originalDataLength);
			}
		}

		[Test]
		public async Task CompressionTest_ContentReadable()
		{
			var key = new Mock<IDataBlockKey<RefDataSet>>();
			var checkpoint = CheckpointHelper.Create(Guid.NewGuid()).ToString();

			var dummy1 = new RefDataSet { Checkpoint = checkpoint };
			var dummy2 = new RefDataSet { Checkpoint = checkpoint };
			var dummy3 = new RefDataSet { Checkpoint = checkpoint };
			var dummy4 = new RefDataSet { Checkpoint = checkpoint };
			var dummy5 = new RefDataSet { Checkpoint = checkpoint };

			cacheHelper.Setup(x => x.GetSequentialKeys(It.IsAny<IDataBlockKey<RefDataSet>>()))
				.Returns(new[] { key.Object }.AsEnumerable());
			cacheHelper.Setup(x => x.GetData(It.IsAny<IDataBlockKey<RefDataSet>>()))
				.Returns(new[] { dummy1, dummy2, dummy3, dummy4, dummy5 }.AsEnumerable());

			service.Setup(x => x.GetData(It.IsAny<DateTime?>(), It.IsAny<DateTime>(), It.IsAny<ICheckpoint>(), It.IsAny<int?>(), It.IsAny<short>()))
				.Returns(new[] { dummy1, dummy2, dummy3, dummy4 }.AsEnumerable());
			adaptor.Setup(x => x.ParseVersion(It.IsAny<string>())).Returns(Tuple.Create(0, 1, 9));

			using (var factory = GetTestServerFactory<RefDataSet>(adaptor.Object, logWrapper.Object, logHelper.Object, null, cacheWrapper.Object, cacheHelper.Object, null, service.Object))
			using (var server = factory.Server)
			{
				var context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/DummyDataSet/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(new DataSetGet(null, new DateTime(2016, 10, 31), "1", "RefDataSet")));
				});
				var response = context.Response;
				Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
				var originalData = response.GetBody();

				context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/DummyDataSet/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.Headers.AcceptEncoding = "gzip, deflate, br";
					c.Request.SetBody(JsonConvert.SerializeObject(new DataSetGet(null, new DateTime(2016, 10, 31), "1", "RefDataSet")));
				});
				response = context.Response;
				Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
				Assert.AreEqual(CompressionType.GZip, response.GetCompressionType());
				var compressedData = response.GetBody();
				Assert.AreEqual(originalData, compressedData);
			}
		}

		[Test]
		public void GetAvailableDataSetTimestamps()
		{
			adaptor.Setup(x => x.ParseVersion("0_16_9")).Returns(new Tuple<int, int, int>(0, 16, 9));
			var controller = new DummyDataSetController(service.Object, adaptor.Object, cacheHelper.Object, clientRecord.Object, cacheWrapper.Object, logHelper.Object);

			var timestamps = controller.GetAvailableDataSetTimestamps().ToList();
			var tuples = new List<Tuple<string, IEnumerable<DataSetVersion>>>();
			service.Setup(x => x.GetAllDataSetTimestamps()).Returns(tuples);
			service.Verify(x => x.GetAllDataSetTimestamps(), Times.Once);
			service.Verify(x => x.GetAvailableDataSetTimestamps(tuples), Times.Once);

			service.Invocations.Clear();
			cacheWrapper.Setup(x => x.Get("AvailableDataSetTimestampsCacheKey")).Returns(tuples);
			timestamps = controller.GetAvailableDataSetTimestamps().ToList();
			service.Verify(x => x.GetAllDataSetTimestamps(), Times.Never);
			service.Verify(x => x.GetAvailableDataSetTimestamps(tuples), Times.Once);
		}

		[Test]
		public void ControllersNeedingAdditionalCacheVersionsMapping()
		{
			var baseType = typeof(DataSetControllerBase<RefDataSet>);
			var allTypes = baseType.Assembly.GetTypes().Where(x => x.Name.EndsWith("Controller"));
			Assert.Multiple(() =>
			{
				foreach (var type in allTypes)
				{
					var parentType = type.BaseType;
					if (parentType != null && parentType.Name == baseType.Name)
					{
						var methodInfo = type.GetMethod("GetData", BindingFlags.Instance | BindingFlags.NonPublic);
						if (methodInfo != null && methodInfo.DeclaringType.Name != baseType.Name)
						{
							var dataSetName = type.Name.Replace("Controller", string.Empty);
							Assert.True(DataSetControllerBaseHelper.DoesCacheVersionMatter(dataSetName), $"{type.Name} Needs Additional Cache Versions Mapping.");
						}
					}
				}
			});
		}

		public static WebApplicationFactory<Program> GetTestServerFactory<T>(IDataAdaptor dataAdaptor, ILogWrapper logWrapper, ILogHelper logHelper, IFileCacheWrapper fileCacheWrapper, ICacheWrapper cacheWrapper, IDataBlockCacheHelper dataBlockCacheHelper, IClientRecord clientRecord, IReferenceDataService<T> dummyService, ErrorReportingClientWrapper errorReportWrapper = null) where T : RefDataSet
		{
			dataAdaptor = dataAdaptor ?? new Mock<IDataAdaptor>().Object;
			if (logWrapper == null)
			{
				var log = new Mock<ILog>();
				var logWrapperMock = new Mock<ILogWrapper>();
				logWrapperMock.Setup(x => x.GetLog<ApiLogMiddleWare>()).Returns(log.Object);
				logWrapperMock.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
				logWrapper = logWrapperMock.Object;
			}
			logHelper = logHelper ?? new Mock<ILogHelper>().Object;
			fileCacheWrapper = fileCacheWrapper ?? new Mock<IFileCacheWrapper>().Object;
			cacheWrapper = cacheWrapper ?? new Mock<ICacheWrapper>().Object;
			dataBlockCacheHelper = dataBlockCacheHelper ?? new Mock<IDataBlockCacheHelper>().Object;
			clientRecord = clientRecord ?? new Mock<IClientRecord>().Object;
			dummyService = dummyService ?? new Mock<IReferenceDataService<T>>().Object;
			errorReportWrapper = errorReportWrapper ?? new Mock<ErrorReportingClientWrapper>(null).Object;

			var providerMock = new Mock<IAuthenticationHandlerProvider>();
			var handlerMock = new Mock<IAuthenticationHandler>();
			var principal = new ClaimsPrincipal(new GenericIdentity("XX", AuthType.BasicAuth));
			var authenticateTicket = new AuthenticationTicket(principal, AuthType.BasicAuth);
			var result = AuthenticateResult.Success(authenticateTicket);
			handlerMock.Setup(x => x.AuthenticateAsync()).ReturnsAsync(result);
			handlerMock.Setup(x => x.InitializeAsync(It.IsAny<AuthenticationScheme>(), It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
			providerMock.Setup(x => x.GetHandlerAsync(It.IsAny<HttpContext>(), It.IsAny<string>())).ReturnsAsync(handlerMock.Object);

			var factory = WebApplicationFactoryHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddScoped(x => dataAdaptor)
					.AddSingleton(logWrapper)
					.AddSingleton(errorReportWrapper)
					.AddSingleton(logHelper)
					.AddSingleton(cacheWrapper)
					.AddSingleton(providerMock.Object)
					.AddScoped(x => fileCacheWrapper)
					.AddScoped(x => dataBlockCacheHelper)
					.AddScoped(x => clientRecord)
					.AddScoped(x => dummyService);

					services.Configure<TestServerOptions>(options =>
					{
						options.AllowSynchronousIO = true;
						options.BaseAddress = new Uri("http://localhost/");
					});
				});
			});
			return factory;
		}

		[SetUp]
		public void TestSetup()
		{
			service = new Mock<IReferenceDataService<RefDataSet>>();
			adaptor = new Mock<IDataAdaptor>();
			cacheHelper = new Mock<IDataBlockCacheHelper>();
			clientRecord = new Mock<IClientRecord>();
			cacheWrapper = new Mock<ICacheWrapper>();
			logHelper = new Mock<ILogHelper>();

			var log = new Mock<ILog>();
			logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ApiLogMiddleWare>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
		}

		Mock<IReferenceDataService<RefDataSet>> service;
		Mock<IDataAdaptor> adaptor;
		Mock<IDataBlockCacheHelper> cacheHelper;
		Mock<IClientRecord> clientRecord;
		Mock<ICacheWrapper> cacheWrapper;
		Mock<ILogHelper> logHelper;
		Mock<ILogWrapper> logWrapper;
	}
}
