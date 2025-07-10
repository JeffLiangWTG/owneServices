using System.Collections.Generic;
using CargoWise.RefDbRepo.NewService.Controllers;
using CargoWise.RefDbRepo.RemoteDbManager;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	class DataSetScriptsControllerFixture
	{
		[Test]
		public void TestGetAllDataSetScripts()
		{
			var updateScriptProvider = new Mock<IUpdateScriptProvider>();
			updateScriptProvider.Setup(x => x.GetScriptInfos(It.IsAny<int>())).Returns(GetScriptInfos());
			var scriptsController = new DataSetScriptsController(updateScriptProvider.Object);
			var scripts = scriptsController.GetAllDataSetScripts(300);
			Assert.AreEqual("0_37_9", scripts.DataVersion);
			Assert.AreEqual(2, scripts.Scripts.Length);
			Assert.AreEqual("DataSetA", scripts.Scripts[0].DataSetType);
			Assert.AreEqual(1, scripts.Scripts[0].UpdaterVersion);
			Assert.AreEqual("DataSetB", scripts.Scripts[1].DataSetType);
			Assert.AreEqual(2, scripts.Scripts[1].UpdaterVersion);
		}

		IEnumerable<IUpdaterScriptInfo> GetScriptInfos()
		{
			var scriptInfo1 = new Mock<IUpdaterScriptInfo>();
			scriptInfo1.Setup(x => x.DataSetName).Returns("DataSetA");
			scriptInfo1.Setup(x => x.UpdaterVersion).Returns(1);
			var scriptInfo2 = new Mock<IUpdaterScriptInfo>();
			scriptInfo2.Setup(x => x.DataSetName).Returns("DataSetB");
			scriptInfo2.Setup(x => x.UpdaterVersion).Returns(2);

			return new[] { scriptInfo1.Object, scriptInfo2.Object };
		}
	}
}
