using CargoWise.RefDbRepo.USReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class LocalFileStorageTest
	{
		[Test]
		public void TestAnythingNeedToProcess()
		{
			var storage = new LocalFileStorage("TST");
			storage.ClearData();
			Assert.Null(storage.Load());

			var data = "test";
			storage.Save(data);
			Assert.AreEqual(data, storage.Load());
		}
	}
}
