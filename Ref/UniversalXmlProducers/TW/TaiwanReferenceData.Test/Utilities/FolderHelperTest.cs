using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	sealed class FolderHelperTest
	{
		[Test]
		public void TestGetBinFolder()
		{
			Assert.AreEqual(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FolderHelper.GetBinFolder());
		}
	}
}
