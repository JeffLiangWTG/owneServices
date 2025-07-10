using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AutoRateDateByChargeGroupControl))]
	sealed class AutoRateDateByChargeGroupControlTest : ChargeGroupSettingControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new AutoRateDateByChargeGroupCollection();
			collection.AddNew();

			return collection;
		}
	}
}
