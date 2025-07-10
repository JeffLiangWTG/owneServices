using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class OrgSupplierPartFormCustomsControlAbstractTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (var control = GetUserControl())
			{
				AssertEquals(UserControlName, control.Name);
			}
		}

		public void TestPivotGridColumns()
		{
			var part = Factory.NewWithValidTestData<Business.OrgSupplierPart>();
			part.PivotsForBinding.AddNew();
			Factory.Save();

			using (var form = new ZForm(part))
			using (var control = GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var pivotGrid = control.FindSingle<ZGrid>("PivotGrid");
				AssertNotNull("Last Audited Date", pivotGrid.GetColumnStyle("CI_LastAuditedDate"));
				AssertNotNull("Last Audited User", pivotGrid.GetColumnStyle("LastAuditedUserFullName"));
				AssertNotNull("Classification Description", pivotGrid.GetColumnStyle("CI_Description"));
			}
		}

		public void TestAuditClassificationContextMenu()
		{
			var part = Factory.NewWithValidTestData<Business.OrgSupplierPart>();
			var cusClassPivot = part.PivotsForBinding.AddNew();
			Factory.Save();

			using (var form = new ZForm(part))
			using (var control = GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(UserControlName, control.Name);

				var pivotGrid = control.FindSingle<ZGrid>("PivotGrid");
				var gridContextMenu = pivotGrid.ContextMenu;
				var auditClassificationMenuItem = gridContextMenu.MenuItems.FindByText(OrgSupplierPartFormCustomsControlGlobal.AuditClassificationLinesMenuItemCaption, false);
				AssertNotNull(auditClassificationMenuItem);

				var classificationType = cusClassPivot.GetClassificationTypeProvider();

				pivotGrid.SelectAllElements();

				cusClassPivot.CI_ChildType = classificationType.HTICode;
				Factory.Save();
				gridContextMenu.DoPopup();
				AssertEquals(true, auditClassificationMenuItem.Enabled);

				cusClassPivot.CI_ChildType = classificationType.HTECode;
				Factory.Save();
				gridContextMenu.DoPopup();
				AssertEquals(true, auditClassificationMenuItem.Enabled);

				cusClassPivot.CI_ChildType = classificationType.HTBCode;
				if (!cusClassPivot.CI_ChildType.IsEmpty)
				{
					Factory.Save();
					gridContextMenu.DoPopup();
					AssertEquals(true, auditClassificationMenuItem.Enabled);
				}
			}
		}

		protected abstract ZUserControl GetUserControl();

		protected abstract string UserControlName { get; }
	}
}
