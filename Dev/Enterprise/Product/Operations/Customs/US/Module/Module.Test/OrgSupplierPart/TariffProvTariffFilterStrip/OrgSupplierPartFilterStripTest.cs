using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class OrgSupplierPartFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (var filterStrip = new OrgSupplierPartFilterStripForTest())
			{
				filterStrip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				using (var result = filterStrip.GetCurrentFilterControlsForTest(new TariffProvTariffModuleFilter(OrgSupplierPartFilterConstants.Tariff.TariffProvTariff, DummyFuncForTest)))
				{
					AssertEquals(typeof(TariffProvTariffFilterControl), result.GetType());
				}

				using (var result = filterStrip.GetCurrentFilterControlsForTest(new TariffInvalidModuleFilter(OrgSupplierPartFilterConstants.Tariff.TariffInvalid, (expiredDate) => new ZQuery())))
				{
					AssertEquals(typeof(TariffInvalidFilterControl), result.GetType());
				}
			}
		}

		ZQuery DummyFuncForTest(ZString property, ZString provTariff) => new ZQuery();

		sealed class OrgSupplierPartFilterStripForTest : OrgSupplierPartFilterStrip
		{
			public Control GetCurrentFilterControlsForTest(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter)[0];
		}
	}
}
