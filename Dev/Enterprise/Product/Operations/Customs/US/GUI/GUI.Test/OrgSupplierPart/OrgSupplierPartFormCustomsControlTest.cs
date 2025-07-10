using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsControlTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlAbstractTest
	{
		public void TestCanShowTariffModuleSearch()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = "10";
			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = string.Empty;
			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				form.SelectMainTabPageForTest();
				var grid = form.FindSingle<ZGrid>("PivotGrid");
				grid.CurrentCell = new DataGridCell(0, 3);
				var columnNumber = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals(CusClassPartPivot.Schema.CI_FormattedTariffNum, columnNumber.ColumnName);
				var findBox = (ZArchitecture.GUI.Internal.ZPopupFindBox)(((ZCodeFindBoxColumnStyle)columnNumber.ColumnStyle).EditControl);
				findBox.PopupButton.PerformClick();
				AssertEquals(typeof(ZArchitecture.GUI.Internal.EmbeddedModulePopup), ((IFindBox)findBox).PopupForm.GetType());
			}

			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient = false;
			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				form.SelectMainTabPageForTest();
				var grid = form.FindSingle<ZGrid>("PivotGrid");
				grid.CurrentCell = new DataGridCell(0, 3);
				var columnNumber = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals(CusClassPartPivot.Schema.CI_FormattedTariffNum, columnNumber.ColumnName);
				var findBox = (ZArchitecture.GUI.Internal.ZPopupFindBox)(((ZCodeFindBoxColumnStyle)columnNumber.ColumnStyle).EditControl);
				findBox.PopupButton.PerformClick();
				AssertEquals(typeof(Common.GUI.FindBoxWrapperForBorderWise), ((IFindBox)findBox).PopupForm.GetType());
			}
		}

		protected override ZUserControl GetUserControl() => new OrgSupplierPartFormCustomsControl();

		protected override string UserControlName => "OrgSupplierPartFormCustomsControl";
	}
}
