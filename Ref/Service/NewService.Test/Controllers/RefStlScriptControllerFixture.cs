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
	public class RefStlScriptControllerFixture
	{
		[Test]
		public void GetAvailableDataSetTimestamps()
		{
			var wrapper = new Mock<IDataBlockCacheHelper>();
			var cacheWrapper = new Mock<ICacheWrapper>();
			var tuples = new List<Tuple<string, IEnumerable<DataSetVersion>>>();
			var serverDataVersions = new DataSetVersion[] {
				new DataSetVersion("RefStlScript", DateTime.Now, 0)
			};
			var service = new Mock<IReferenceDataService<Models.RefStlScript>>();
			service.Setup(x => x.GetAvailableDataSetTimestamps(tuples)).Returns(serverDataVersions);
			var dataAdaptor = new Mock<IDataAdaptor>();
			dataAdaptor.Setup(x => x.ParseVersion("0_165_9")).Returns(new Tuple<int, int, int>(0, 165, 9));
			var logHelper = new Mock<ILogHelper>();

			var controller = new RefStlScriptController(service.Object, dataAdaptor.Object, wrapper.Object,
				new Mock<IClientRecord>().Object, cacheWrapper.Object, logHelper.Object);

			var controllerResponse = controller.GetAvailableDataSetTimestamps();
			service.Verify(x => x.GetAvailableDataSetTimestamps(tuples), Times.Never);
			Assert.IsNotNull(controllerResponse);

			var dataSetVersions = controllerResponse.ToList();
			Assert.AreEqual(1, dataSetVersions.Count);
			Assert.AreEqual("RefStlScript", dataSetVersions[0].Name);
			Assert.AreEqual(0, dataSetVersions[0].Order);
			Assert.AreEqual(DateTime.MinValue, dataSetVersions[0].Timestamp);

			controllerResponse = controller.GetAvailableDataSetTimestamps("0_165_9").ToList();
			service.Verify(x => x.GetAvailableDataSetTimestamps(tuples), Times.Once);
			Assert.IsNotNull(controllerResponse);

			dataSetVersions = controllerResponse.ToList();
			Assert.AreEqual(1, dataSetVersions.Count);
			Assert.AreEqual("RefStlScript", dataSetVersions[0].Name);
			Assert.AreEqual(0, dataSetVersions[0].Order);
			Assert.AreEqual(serverDataVersions[0].Timestamp, dataSetVersions[0].Timestamp);
		}
	}
}
