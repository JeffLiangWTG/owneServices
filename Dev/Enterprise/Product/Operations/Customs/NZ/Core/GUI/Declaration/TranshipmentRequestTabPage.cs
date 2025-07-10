using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class TranshipmentRequestTabPage : ZTabPage
	{
		public TranshipmentRequestTabPage(ZTabControl tabControl)
		{
			this.tabControl = tabControl;
			tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
		}

		void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (Controls.Count == 0 && TabVisible)
				{
					control = new TranshipmentRequestUserControl();
					control.Dock = DockStyle.Fill;
					Controls.Add(control);
				}

				if (tabControl.SelectedTab == this)
				{
					control?.RefreshBinding();
				}
			}
		}

		TranshipmentRequestUserControl control;

		protected override void Dispose(bool disposing)
		{
			if (disposing && tabControl != null)
			{
				tabControl.SelectedIndexChanged -= TabControl_SelectedIndexChanged;
			}

			base.Dispose(disposing);
		}

		readonly ZTabControl tabControl;
	}
}
