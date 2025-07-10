using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefFacilityLocalCodeControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestControl()
		{
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			var refFacilityLocalCode = Factory.NewWithValidTestData<RefFacilityLocalCode>();

			refFacilityLocalCode.RFL_Code = "123";
			refFacilityLocalCode.RFL_RN_NKCountryCode = "US";
			refFacilityLocalCode.RFL_Usage = "123";
			refFacilityLocalCode.RFL_RFT_NKFacilityCode = refFacility.RFT_Code;
			Factory.Save();

			using (var form = new RefFacilityForm(refFacility))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("FacilityCodeTabControl", true)[0];
				var tabPage = (ZTabPage)tabControl.TabPages["RefFacilityLocalCodesTabPage"];
				AssertEquals(true, tabPage.TabVisible);

				tabControl.SelectedIndex = 0;

				var refFacilityLocalCodeControl = tabPage.Controls.Find("RefFacilityLocalCodeControl", true)[0];
				var requirementsGrid = (ZGrid)refFacilityLocalCodeControl.Controls["LocalCodesGrid"];

				AssertEquals(3, requirementsGrid.Columns.Count);

				CombineAssertions(() =>
				{
					AssertNotNull(requirementsGrid.Columns["RFL_RN_NKCountryCode"]);
					AssertNotNull(requirementsGrid.Columns["RFL_Usage"]);
					AssertNotNull(requirementsGrid.Columns["RFL_Code"]);
				});
			}
		}
	}
}
