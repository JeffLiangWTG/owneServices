using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class UpdateExpiryNotificationPeriodControlTestCase : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestUpdateExpiryNotificationPeriodControl()
		{
			using (var form = new ZForm())
			{
				var control = new UpdateExpiryNotificationPeriodControl();
				form.Controls.Add(control);
				form.Show();

				AssertNotNull(form.FindSingle<ZGuidFindBox>("ClientFindBox"));
				AssertNotNull(form.FindSingle<ZGuidFindBox>("WarehouseFindBox"));
				AssertNotNull(form.FindSingle<ZCalcEdit>("ExpiryNotificationPeriodCalcEdit"));
				AssertNotNull(form.FindSingle<ZCheckBox>("OverrideNonZeroExpiryNotificationPeriodCheckBox"));
			}
		}
	}
}

