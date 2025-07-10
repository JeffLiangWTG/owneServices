using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public abstract class RelatedItemsSupportableFormBase : ZTemplateForm
	{
		protected RelatedItemsSupportableFormBase()
		{
		}

		protected RelatedItemsSupportableFormBase(IBusiness businessEntity)
			: base(businessEntity)
		{
		}

		protected void SetUpRelatedTabPage()
		{
			MainTabControl.Selecting += MainTabControl_Selecting;

			RelatedItemsTabPage.CaptionResourceString = Res.GetData("e7c20b4f-f99f-4e83-b619-b7e3cdd48609", "Related Items");
			RelatedItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			RelatedItemsTabPage.Name = "RelatedItemsTabPage";
			RelatedItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 603, true);
		}

		void MainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPage == RelatedItemsTabPage && RelatedItemsTabPage.Controls.Count == 0)
			{
				InitializeRelatedItemsTabPage(RelatedItemsTabPage);
			}

			OnMainTabControlSelecting(e.TabPageIndex);
		}

		protected virtual void OnMainTabControlSelecting(int tabPageIndex)
		{
		}

		protected virtual void InitializeRelatedItemsTabPage(ZTabPage relatedItemsTabPage)
		{
			var control = new WorkTaskRelatedItemUserControl(IsViewOrDeleteMode);
			control.Dock = DockStyle.Fill;
			relatedItemsTabPage.Controls.Add(control);
		}

		protected ZTabPage RelatedItemsTabPage { get; set; }
	}
}
