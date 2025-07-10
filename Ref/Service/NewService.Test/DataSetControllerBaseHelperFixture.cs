using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class DataSetControllerBaseHelperFixture
	{
		[Test]
		public void DoesCacheVersionMatter()
		{
			Assert.True(DataSetControllerBaseHelper.DoesCacheVersionMatter("RefCusCodeType"));
			Assert.True(DataSetControllerBaseHelper.DoesCacheVersionMatter("RefVesselZZ"));
			Assert.True(DataSetControllerBaseHelper.DoesCacheVersionMatter("RefCarrierCode"));

			Assert.False(DataSetControllerBaseHelper.DoesCacheVersionMatter("RefCusTariff"));
			Assert.False(DataSetControllerBaseHelper.DoesCacheVersionMatter("RefCusProcedure"));
		}

		[Test]
		public void GetVersionForCacheString()
		{
			var datasetName = "RefCusCodeType";
			var version = "0_25_9";
			Assert.AreEqual("0_26_9", DataSetControllerBaseHelper.GetVersionForCacheString(datasetName, version));

			version = "0_26_9";
			Assert.AreEqual("0_26_9", DataSetControllerBaseHelper.GetVersionForCacheString(datasetName, version));

			version = "0_27_9";
			Assert.AreEqual("", DataSetControllerBaseHelper.GetVersionForCacheString(datasetName, version));
		}
	}
}
