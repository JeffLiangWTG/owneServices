using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SqlSystemConfigurationsForm : ZChildForm
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			FormClosing += SqlSystemConfigurationsForm_FormClosing;
			tabControl.SelectedIndexChanging += TabControl_SelectedIndexChanging;
			tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
			TabControl_SelectedIndexChanged(tabControl, null);
		}

		ZTabPage lastSelectedTab;
		void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			lastSelectedTab = tabControl.SelectedTab;
		}

		void TabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			var lastTabPageControl = lastSelectedTab?.Controls[0];
			if (lastTabPageControl == null)
			{
				return;
			}

			GetTabPageControls()
				.First(x => ReferenceEquals(x, lastTabPageControl))
				.OnParentTabControlSwitchingToOtherTab();
		}

		void SqlSystemConfigurationsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			foreach (var control in GetTabPageControls())
			{
				control.OnParentFormClosing(this, e);
				if (e.Cancel)
				{
					return;
				}
			}
		}

		IEnumerable<ITabPageContentHolder> GetTabPageControls()
		{
			return tabControl.Controls
				.Cast<ZTabPage>()
				.Select(x => x.Controls[0])
				.Cast<ITabPageContentHolder>();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			DisplayMode = ODisplayMode.ReadOnly;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			return ContinueWithSave.Yes;
		}

		public override string FormVerb => null;
		public override string FormCaption => Res.GetString("E0824F0C-033F-4A9F-9F87-43B8878AA4EB", "SQL Server System Configurations");

		protected virtual SqlSystemConfigurationsUserControl CreateSqlSystemConfigurationsUserControl()
		{
			return new SqlSystemConfigurationsUserControl();
		}
	}
}
