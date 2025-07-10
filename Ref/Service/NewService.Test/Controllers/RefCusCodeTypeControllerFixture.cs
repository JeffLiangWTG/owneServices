using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	class RefCusCodeTypeControllerFixture
	{
		[Test]
		public async Task GetDataStreamVersioningWithCache()
		{
			var wrapper = new Mock<IDataBlockCacheHelper>();
			var cacheWrapper = new Mock<ICacheWrapper>();
			var dataAdaptor = new Mock<IDataAdaptor>();
			var readonlyRepo = new Mock<IReadOnlyReferenceDataRepository>().Object;
			dataAdaptor.Setup(x => x.ParseVersion("0_25_9")).Returns(new Tuple<int, int, int>(0, 25, 9));
			dataAdaptor.Setup(x => x.ParseVersion("0_26_9")).Returns(new Tuple<int, int, int>(0, 26, 9));
			dataAdaptor.Setup(x => x.ParseVersion("0_27_9")).Returns(new Tuple<int, int, int>(0, 27, 9));
			var logHelper = new Mock<ILogHelper>();

			using (var factory = DataSetControllerBaseFixture.GetTestServerFactory(dataAdaptor.Object, null, logHelper.Object, null, cacheWrapper.Object, wrapper.Object, null, new RefCusCodeTypeService(readonlyRepo)))
			using (var server = factory.Server)
			{
				var dataSetGet = new DataSetGet(null, DateTime.UtcNow, "0_25_9", "RefCusCodeType");
				var context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/RefCusCodeType/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(dataSetGet));
				});
				Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);

				dataSetGet = new DataSetGet(null, DateTime.UtcNow, "0_26_9", "RefCusCodeType");
				context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/RefCusCodeType/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(dataSetGet));
				});
				Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);

				dataSetGet = new DataSetGet(null, DateTime.UtcNow, "0_27_9", "RefCusCodeType");
				context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/RefCusCodeType/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(dataSetGet));
				});
				Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
			}
		}
	}
}
