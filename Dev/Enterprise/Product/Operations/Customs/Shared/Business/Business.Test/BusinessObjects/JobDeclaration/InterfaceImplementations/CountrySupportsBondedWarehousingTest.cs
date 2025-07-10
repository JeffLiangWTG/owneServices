using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.InterfaceImplementations.Testing
{
	public class CountrySupportsBondedWarehousingTest : TestCaseWithFactory
	{
		public void TestIsSupportsBondedWareshousing()
		{
			var supporter = new CountrySupportsBondedWarehousing();
			CombineAssertions(() =>
			{
				AssertEquals("Australia overrides to true", true, supporter.IsSupportsBondedWarehousing(Factory, Core.Constants.CountryCodes.Australia));
				AssertEquals("FUNCS not set", false, supporter.IsSupportsBondedWarehousing(Factory, Core.Constants.CountryCodes.Eritrea));
				using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("FUNCS set", true, supporter.IsSupportsBondedWarehousing(Factory, Core.Constants.CountryCodes.Eritrea));
				}
			});
		}
	}
}
