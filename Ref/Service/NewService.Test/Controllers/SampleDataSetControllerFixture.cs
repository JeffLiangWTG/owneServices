using System;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.NewService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	class SampleDataSetControllerFixture
	{
		[Test]
		public void GetServerResponse()
		{
			var controller = new SampleDataSetController();
			var dataSetGet = new DataSetGet(null, new DateTime(2023, 8, 18), "0_9_9", "DataSetForTest");
			var response = (OkObjectResult)controller.GetServerResponse(dataSetGet);
			Assert.NotNull(response);
			Assert.AreEqual(JsonConvert.SerializeObject(dataSetGet), response.Value.ToString());
		}
	}
}
