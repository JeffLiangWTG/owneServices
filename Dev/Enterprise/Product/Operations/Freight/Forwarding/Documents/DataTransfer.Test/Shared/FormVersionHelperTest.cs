using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class FormVersionHelperTest : TestCaseWithFactory
	{
		public void TestGetOCMFormVersion()
		{
			using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var version = FormVersionHelper.GetOCMFormVersion();
				AssertEquals("Version should be 2.5.0", "2.5.0", version);

				using (FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					version = FormVersionHelper.GetOCMFormVersion();
					AssertEquals("Version should be 2.5.0", "2.5.0", version);
				}

				using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					version = FormVersionHelper.GetOCMFormVersion();
					AssertEquals("Version should be 3.0.0", "3.0.0", version);
				}

				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					version = FormVersionHelper.GetOCMFormVersion();
					AssertEquals("Version should be 4.0.0", "4.0.0", version);
				}
			}
		}
	}
}
