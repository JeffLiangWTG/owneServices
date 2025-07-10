using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SupplyTypeConfigurationByChargeGroupControl))]
	sealed class SupplyTypeConfigurationByChargeGroupControlTest : ChargeGroupSettingControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new SupplyTypeConfigurationByChargeGroupCollection();
			collection.AddNew();

			return collection;
		}
	}
}
