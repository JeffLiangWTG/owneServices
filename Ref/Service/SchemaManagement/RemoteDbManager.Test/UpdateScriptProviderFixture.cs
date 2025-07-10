using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class UpdateScriptProviderFixture
	{
		[Test]
		public void GetSupportVersion()
		{
			var updater1 = new Mock<IUpdaterScriptInfo>();
			updater1.Setup(x => x.DataSetName).Returns("RefAccTaxRate");
			var updater2 = new Mock<IUpdaterScriptInfo>();
			updater2.Setup(x => x.DataSetName).Returns("RefAccTaxRate");
			var updater3 = new Mock<IUpdaterScriptInfo>();
			updater3.Setup(x => x.DataSetName).Returns("RefCusCodeList");
			var updater4 = new Mock<IUpdaterScriptInfo>();
			updater4.Setup(x => x.DataSetName).Returns("RefCusCodeList");
			var list = new[] { (updater1.Object, 1), (updater2.Object, 10), (updater3.Object, 2), (updater4.Object, 11) };
			var result = UpdateScriptProvider.GetScripts(list, 1).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(updater1.Object, result[0]);
			Assert.AreEqual(updater3.Object, result[1]);
			result = UpdateScriptProvider.GetScripts(list, 2).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(updater2.Object, result[0]);
			Assert.AreEqual(updater3.Object, result[1]);
			result = UpdateScriptProvider.GetScripts(list, 10).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(updater2.Object, result[0]);
			Assert.AreEqual(updater4.Object, result[1]);
			result = UpdateScriptProvider.GetScripts(list, 11).ToArray();
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(updater4.Object, result[0]);
		}

		[Test]
		public void TestScriptInfos()
		{
			var provider = new UpdateScriptProvider();
			Assert.Contains(UpdateScriptProvider.SRDbLatestVersion, provider.UpdaterScriptInfos.Select(x => x.Item2).ToArray(), "ScriptInfos should contain at least one Updater Info with SRDbLatestVersion");

			var infosInAssembly = typeof(IUpdaterScriptInfo).Assembly.GetTypes().Where(x => typeof(IUpdaterScriptInfo).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToArray();
			var infosInProviderClass = provider.UpdaterScriptInfos.Select(x => x.Item1).Select(x => x.GetType()).ToArray();
			foreach (var info in infosInAssembly)
			{
				Assert.Contains(info, infosInProviderClass, "Assembly UpdaterInfo should exist in class");
			}
			foreach (var info in infosInProviderClass)
			{
				Assert.Contains(info, infosInAssembly, "Class UpdaterInfo should exist in assembly");
			}
		}
	}
}
