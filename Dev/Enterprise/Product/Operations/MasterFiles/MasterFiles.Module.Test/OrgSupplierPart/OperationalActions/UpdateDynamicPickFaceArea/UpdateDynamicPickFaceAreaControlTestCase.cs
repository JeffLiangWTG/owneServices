using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class UpdateDynamicPickFaceAreaControlTestCase : TestCaseWithFactory
	{
		public void TestUpdateDynamicPickFaceAreaControl()
		{
			using (var form = new ZForm())
			{
				var control = new UpdateDynamicPickFaceAreaControl();
				form.Controls.Add(control);
				form.Show();

				AssertNotNull(form.FindSingle<ZGuidFindBox>("ClientFindBox"));
				AssertNotNull(form.FindSingle<ZGuidFindBox>("WarehouseFindBox"));
				AssertNotNull(form.FindSingle<ZGuidFindBox>("DynamicPickAreaFindBox"));
				AssertNotNull(form.FindSingle<ZCheckBox>("OverrideNonEmptyDynamicPickAreaCheckBox"));
			}
		}
	}
}

