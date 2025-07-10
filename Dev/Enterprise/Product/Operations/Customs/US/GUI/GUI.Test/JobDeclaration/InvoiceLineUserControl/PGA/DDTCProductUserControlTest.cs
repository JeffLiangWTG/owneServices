using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class DDTCProductUserControlTest : TestCaseWithFactory
	{
		public void TestDDTCProductUserVisibility()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			using (var form = new TestForm(pivot))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertNotNull(userControl);
				AssertEquals(true, userControl.Visible);
				AssertEquals(false, userControl.DDTCTabPage.TabVisible);
				pivot.Details.CD_DDTCIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals(true, pivot.HasImportDDTCData);
				userControl.ImportTabControl.SelectedTab = userControl.DDTCTabPage;
				AssertNotNull(userControl.ddtcUserControl);
			}
		}

		sealed class TestForm : ZForm
		{
			public TestForm(CusClassPartPivot pivot) : base(pivot)
			{
			}

			public ImportClassificationUserControl UserControl;

			protected override void InitializeComponent()
			{
				UserControl = new ImportClassificationUserControl(null);
				Controls.Add(UserControl);
				DataSourceAssemblyName = "Enterprise.Customs.US.Business";
				DataSourceTypeName = "Enterprise.Customs.US.Business.Pivot";
			}
		}
	}
}
