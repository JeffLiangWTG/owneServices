using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class SRDbUpdaterDependencyProviderTest : TestCase
	{
		public void TestAllMethods()
		{
			var updater1 = new Mock<ISRDbDataSetUpdater>();
			updater1.SetupGet(x => x.UpdaterName).Returns("A");
			var updater2 = new Mock<ISRDbDataSetUpdater>();
			updater2.SetupGet(x => x.UpdaterName).Returns("B");
			var updater3 = new Mock<ISRDbDataSetUpdater>();
			updater3.SetupGet(x => x.UpdaterName).Returns("C");
			updater3.SetupGet(x => x.Prerequisites).Returns(new[] { "A", "B" });
			var updater4 = new Mock<ISRDbDataSetUpdater>();
			updater4.SetupGet(x => x.UpdaterName).Returns("D");
			updater4.SetupGet(x => x.Prerequisites).Returns(new[] { "A" });
			var provider = new SRDbUpdaterDependencyProvider(new[] { updater1.Object, updater2.Object, updater3.Object, updater4.Object });
			var result = provider.GetAllLeaves().ToArray();
			Assert(result.Contains(updater3.Object));
			Assert(result.Contains(updater4.Object));
			result = provider.GetAllRoots().ToArray();
			Assert(result.Contains(updater1.Object));
			Assert(result.Contains(updater2.Object));
			result = provider.GetAllNodes().ToArray();
			Assert(result.Contains(updater3.Object));
			Assert(result.Contains(updater4.Object));
			Assert(result.Contains(updater1.Object));
			Assert(result.Contains(updater2.Object));
			result = provider.GetChildren(updater2.Object).ToArray();
			Assert(result.Contains(updater3.Object));
			result = provider.GetParents(updater4.Object).ToArray();
			Assert(result.Contains(updater1.Object));
		}
	}
}
