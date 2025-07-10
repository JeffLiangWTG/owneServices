using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ExportCustomsClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestPGATabsVisibility()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			using (var form = new TestForm(pivot))
			{
				form.Show();
				ExportCustomsClassificationUserControl userControl = form.UserControl;
				userControl.CurrentPivot = pivot;
				AssertEquals("AMS/EPA tab should not be visible", false, userControl.AMSTabPage.TabVisible);
				AssertEquals("ATF tab should not be visible", false, userControl.ATFTabPage.TabVisible);
				AssertEquals("DEA tab should not be visible", false, userControl.DEATabPage.TabVisible);
				AssertEquals("FWS tab should not be visible", false, userControl.FWSTabPage.TabVisible);
				AssertEquals("NMFS tab should not be visible", false, userControl.NMFSTabPage.TabVisible);
				AssertEquals("TTB tab should not be visible", false, userControl.TTBTabPage.TabVisible);
				pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("AMS tab should be visible", true, userControl.AMSTabPage.TabVisible);
				AssertEquals("AMS", userControl.AMSTabPage.Text);
				pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("AMS tab should be visible", true, userControl.AMSTabPage.TabVisible);
				AssertEquals("AMS/EPA", userControl.AMSTabPage.Text);
				pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("ATF tab should be visible", true, userControl.ATFTabPage.TabVisible);
				pivot.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("DEA tab should be visible", true, userControl.DEATabPage.TabVisible);
				pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("FWS tab should be visible", true, userControl.FWSTabPage.TabVisible);
				pivot.CD_NMFSHMSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("NMFS tab should be visible", true, userControl.NMFSTabPage.TabVisible);
				pivot.CD_TTBIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("TTB tab should be visible", true, userControl.TTBTabPage.TabVisible);
			}
		}

		public void TestExportDEASwitchByCustomLineChange()
		{
			var product = Factory.New<OrgSupplierPart>();
			using (OrgSupplierPartForm form = new OrgSupplierPartForm(product))
			{
				product.OP_PartNum = "PROPGA";
				product.OP_Desc = "PRODUCT PGA";
				var pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
				pivot.CI_FormattedTariffNum = "1111";
				pivot.CD_DEAIndicator = "D";
				var deaLine = pivot.DEAHeaders.AddNew();
				deaLine.US_DrugCode = "1111";
				var pivot2 = product.PivotsForBinding.AddNew();
				form.Show();
				form.SelectMainTabPageForTest();
				var userControl = form.Controls.Find("exportClassificationUserControl", true)[0] as ExportCustomsClassificationUserControl;
				var exportTabControl = userControl.Controls.Find("ExportTabControl", true)[0] as ZTabControl;
				exportTabControl.SelectedTab = userControl.DEATabPage;
				var grid = form.Controls.Find("pivotGrid", true)[0] as ZArchitecture.ZGrid;
				grid.SelectSingleElement(pivot2);
				pivot2.CI_ChildType = ClassificationTypeList.Codes.SHB;
				pivot2.CI_FormattedTariffNum = "2222";
				pivot2.CD_DEAIndicator = "D";
				var deaLine2 = pivot2.DEAHeaders.AddNew();
				deaLine2.US_DrugCode = "2222";
				grid.SelectSingleElement(pivot);
				var deaGrid = userControl.exportDEAUserControl.Controls.Find("DEAGrid", true)[0] as ZArchitecture.ZGrid;
				AssertEquals("1111", deaGrid[0, 0].ToString());
			}
		}

		public void TestExportTTBSwitchByCustomLineChange()
		{
			var product = Factory.New<OrgSupplierPart>();
			using (OrgSupplierPartForm form = new OrgSupplierPartForm(product))
			{
				product.OP_PartNum = "PROPGA";
				product.OP_Desc = "PRODUCT PGA";
				var pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
				pivot.CI_FormattedTariffNum = "1111";
				pivot.CD_TTBIndicator = "D";
				var ttbLine = pivot.TTBLines.AddNew();
				ttbLine.US_NumberForIRC = "1111";
				var pivot2 = product.PivotsForBinding.AddNew();
				form.Show();
				form.SelectMainTabPageForTest();
				var userControl = form.Controls.Find("exportClassificationUserControl", true)[0] as ExportCustomsClassificationUserControl;
				var exportTabControl = userControl.Controls.Find("ExportTabControl", true)[0] as ZTabControl;
				exportTabControl.SelectedTab = userControl.TTBTabPage;
				var grid = form.Controls.Find("pivotGrid", true)[0] as ZArchitecture.ZGrid;
				grid.SelectSingleElement(pivot2);
				pivot2.CI_ChildType = ClassificationTypeList.Codes.SHB;
				pivot2.CI_FormattedTariffNum = "2222";
				pivot2.CD_TTBIndicator = "D";
				var ttbLine2 = pivot2.TTBLines.AddNew();
				ttbLine2.US_NumberForIRC = "2222";
				grid.SelectSingleElement(pivot);
				var ttbGrid = userControl.exportTTBUserControl.Controls.Find("TTBGrid", true)[0] as ZArchitecture.ZGrid;
				AssertEquals("1111", ttbGrid[0, 0].ToString());
			}
		}

		public void TestExportNMFSSwitchByCustomLineChange()
		{
			var product = Factory.New<OrgSupplierPart>();
			using (OrgSupplierPartForm form = new OrgSupplierPartForm(product))
			{
				product.OP_PartNum = "PROPGA";
				product.OP_Desc = "PRODUCT PGA";
				var pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
				pivot.CI_FormattedTariffNum = "1111";
				pivot.CD_NMFSHMSIndicator = "D";
				var nmfsLine = pivot.NMFSLines.AddNew();
				nmfsLine.US_IFTPPermitNumber = "1111";
				var pivot2 = product.PivotsForBinding.AddNew();
				form.Show();
				form.SelectMainTabPageForTest();
				var userControl = form.Controls.Find("exportClassificationUserControl", true)[0] as ExportCustomsClassificationUserControl;
				var exportTabControl = userControl.Controls.Find("ExportTabControl", true)[0] as ZTabControl;
				exportTabControl.SelectedTab = userControl.NMFSTabPage;
				var grid = form.Controls.Find("pivotGrid", true)[0] as ZArchitecture.ZGrid;
				grid.SelectSingleElement(pivot2);
				pivot2.CI_ChildType = ClassificationTypeList.Codes.SHB;
				pivot2.CI_FormattedTariffNum = "2222";
				pivot2.CD_NMFSHMSIndicator = "D";
				var nmfsLine2 = pivot2.NMFSLines.AddNew();
				nmfsLine2.US_IFTPPermitNumber = "2222";
				grid.SelectSingleElement(pivot);
				var nmfsGrid = userControl.exportNMFSUserControl.Controls.Find("NMFSHeaderGrid", true)[0] as ZArchitecture.ZGrid;
				AssertEquals("1111", nmfsGrid[0, 3].ToString());
			}
		}

		public void TestExportATFSwitchByCustomLineChange()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var pivot2 = product.PivotsForBinding.AddNew();
			using (OrgSupplierPartForm form = new OrgSupplierPartForm(product))
			{
				form.Show();
				form.SelectMainTabPageForTest();
				var userControl = form.Controls.Find("exportClassificationUserControl", true)[0] as ExportCustomsClassificationUserControl;
				var grid = form.Controls.Find("pivotGrid", true)[0] as ZArchitecture.ZGrid;
				AssertEquals("ATF Tab should be invisible as ATF Indicator is empty.", false, userControl.ATFTabPage.TabVisible);
				grid.SelectSingleElement(pivot1);
				pivot1.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("ATF should be visible as ATF Indicator is 'D'.", true, userControl.ATFTabPage.TabVisible);
				userControl.LoadATFUserControls();
				pivot1.CD_ATFIndicator = string.Empty;
				pivot1.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("should bind to the object selected.", pivot1, grid.ListManager.GetCurrent());
				AssertEquals("should bind to the object selected.", pivot1.ExportATF, userControl.exportATFUserControl.CurrentDataItem);
				grid.SelectSingleElement(pivot2);
				pivot2.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("should bind to the object selected.", pivot2, grid.ListManager.GetCurrent());
				AssertEquals("should bind to the object selected.", pivot2.ExportATF, userControl.exportATFUserControl.CurrentDataItem);
			}
		}

		sealed class TestForm : ZForm
		{
			public TestForm(CusClassPartPivot pivot) : base(pivot)
			{
			}

			internal ExportCustomsClassificationUserControl UserControl;

			protected override void InitializeComponent()
			{
				UserControl = new ExportCustomsClassificationUserControl(null);
				Controls.Add(UserControl);
				DataSourceAssemblyName = "Enterprise.Customs.US.Business";
				DataSourceTypeName = "Enterprise.Customs.US.Business.Pivot";
			}
		}
	}
}
