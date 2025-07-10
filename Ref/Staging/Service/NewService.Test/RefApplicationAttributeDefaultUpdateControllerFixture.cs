using System.Linq;
using CargoWise.RefDbRepo.Staging.NewService.Controllers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	class RefApplicationAttributeDefaultUpdateControllerFixture
	{
		[Test]
		public void GetApplicationAttributes()
		{
			var controller = new RefApplicationAttributeDefaultUpdateController();
			var attributes = controller.Get();
			Assert.Greater(attributes.Count(), 500);
			Assert.True(attributes.Any(x => x.RAA_ConfigFilePath == "CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.config.json" && x.RAA_AttributeName== "safeUpdateServiceUri"));
		}
	}
}
