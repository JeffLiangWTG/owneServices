using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.NewService.Controllers;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Moq;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	public class RefTimeZoneSetControllerFixture
	{
		[Test]
		public void GetAvailableDataSetTimestamps()
		{
			var wrapper = new Mock<IDataBlockCacheHelper>();
			var cacheWrapper = new Mock<ICacheWrapper>();
			var tuples = new List<Tuple<string, IEnumerable<DataSetVersion>>>();
			cacheWrapper.Setup(x => x.Get("AvailableDataSetTimestampsCacheKey")).Returns(tuples);
			var serverDataVersions = new DataSetVersion[] {
				new DataSetVersion("RefTimeZoneSet", DateTime.Now, 0)
			};
			var service = new Mock<IReferenceDataService<Models.RefTimeZoneSet>>();
			service.Setup(x => x.GetAvailableDataSetTimestamps(tuples)).Returns(serverDataVersions);
			var dataAdaptor = new Mock<IDataAdaptor>();
			dataAdaptor.Setup(x => x.ParseVersion("0_16_9")).Returns(new Tuple<int, int, int>(0, 16, 9));
			var logHelper = new Mock<ILogHelper>();

			var controller = new RefTimeZoneSetController(service.Object, dataAdaptor.Object, wrapper.Object,
				new Mock<IClientRecord>().Object, cacheWrapper.Object, logHelper.Object);

			var controllerResponse = controller.GetAvailableDataSetTimestamps();
			service.Verify(x => x.GetAvailableDataSetTimestamps(tuples), Times.Never);
			service.Verify(x => x.GetServerTimestamp(), Times.Never);
			Assert.IsNotNull(controllerResponse);

			var dataSetVersions = controllerResponse.ToList();
			Assert.AreEqual(1, dataSetVersions.Count);
			Assert.AreEqual("RefTimeZoneSet", dataSetVersions[0].Name);
			Assert.AreEqual(DateTime.MinValue, dataSetVersions[0].Timestamp);

			controllerResponse = controller.GetAvailableDataSetTimestamps("0_16_9").ToList();
			service.Verify(x => x.GetAvailableDataSetTimestamps(tuples), Times.Once);
			Assert.IsNotNull(controllerResponse);

			dataSetVersions = controllerResponse.ToList();
			Assert.AreEqual(1, dataSetVersions.Count);
			Assert.AreEqual("RefTimeZoneSet", dataSetVersions[0].Name);
			Assert.AreEqual(serverDataVersions[0].Timestamp, dataSetVersions[0].Timestamp);
		}
	}
}
