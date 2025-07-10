using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RevenueRecognitionByChargeGroupControl))]
	sealed class RevenueRecognitionByChargeGroupControlTest : ChargeGroupSettingControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new RevenueRecognitionByChargeGroupCollection();
			collection.AddNew();

			return collection;
		}
	}
}
