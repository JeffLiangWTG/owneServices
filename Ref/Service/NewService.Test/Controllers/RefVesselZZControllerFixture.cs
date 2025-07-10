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
	class RefVesselZZControllerFixture
	{
		[Test]
		public async Task GetDataStreamVersioningWithCache()
		{
			var wrapper = new Mock<IDataBlockCacheHelper>();
			var cacheWrapper = new Mock<ICacheWrapper>();
			var dataAdaptor = new Mock<IDataAdaptor>();
			var readonlyRepo = new Mock<IReadOnlyReferenceDataRepository>().Object;
			dataAdaptor.Setup(x => x.ParseVersion("0_32_9")).Returns(new Tuple<int, int, int>(0, 32, 9));
			dataAdaptor.Setup(x => x.ParseVersion("0_33_9")).Returns(new Tuple<int, int, int>(0, 33, 9));
			dataAdaptor.Setup(x => x.ParseVersion("0_34_9")).Returns(new Tuple<int, int, int>(0, 34, 9));
			var logHelper = new Mock<ILogHelper>();

			using (var factory = DataSetControllerBaseFixture.GetTestServerFactory(dataAdaptor.Object, null, logHelper.Object, null, cacheWrapper.Object, wrapper.Object, null, new RefVesselZZService(readonlyRepo)))
			using (var server = factory.Server)
			{
				var dataSetGet1 = new DataSetGet(null, DateTime.UtcNow, "0_32_9", "RefVesselZZ");
				var context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/RefVesselZZ/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(dataSetGet1));
				});
				Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);

				dataSetGet1 = new DataSetGet(null, DateTime.UtcNow, "0_33_9", "RefVesselZZ");
				context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/RefVesselZZ/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(dataSetGet1));
				});
				Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);

				dataSetGet1 = new DataSetGet(null, DateTime.UtcNow, "0_34_9", "RefVesselZZ");
				context = await server.SendAsync(c =>
				{
					c.Request.Method = HttpMethods.Post;
					c.Request.Path = new PathString("/RefVesselZZ/GetDataStream");
					c.Request.ContentType = "application/json";
					c.Request.SetBody(JsonConvert.SerializeObject(dataSetGet1));
				});
				Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
			}
		}
	}
}
