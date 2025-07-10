using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SendScheduleUserControl : ZUserControl
	{
		internal SendScheduleModule FilterItemModule;
		internal SendScheduleFilterControl FilterStripControl;

		bool filterGridAdded;

		public SendScheduleUserControl()
		{
			InitializeComponent();
		}

		public new GlbCompanyCampaignItemSchedule CurrentDataItem
		{
			get { return (GlbCompanyCampaignItemSchedule)base.CurrentDataItem; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!filterGridAdded)
			{
				GlbCompanyCampaign campaign = dataSource as GlbCompanyCampaign;

				if (campaign != null)
				{
					var scheduledItems = campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Where(item => !item.G8_ScheduleTimeUtc.IsEmpty);
					campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(scheduledItems);
					AddFilterGrid(campaign);

					filterGridAdded = true;
				}

				base.SetDataBinding(dataSource, dataMember);
			}
		}

		void AddFilterGrid(GlbCompanyCampaign campaign)
		{
			FilterItemModule = (SendScheduleModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbCompanyCampaignItemSchedule);
			FilterItemModule.Campaign = campaign;

			FilterStripControl = (SendScheduleFilterControl)FilterItemModule.EmbeddedControl;
			FilterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			FilterStripControl.Size = ControlDpiScalingHelper.NewScaledSize(Width, Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(30), false);
			FilterStripControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

			Controls.Add(FilterStripControl);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				SetupMenuItems();
			}
		}

		void SetupMenuItems()
		{
			if (FilterItemModule != null)
			{
				foreach (var menuItem in FilterItemModule.FormActionMenu)
				{
					ToolStripItem item = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem, FilterItemModule);
					ToolStrip.Items.Add(item);
				}
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (FilterItemModule != null)
			{
				FilterItemModule.Dispose();
				FilterItemModule = null;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
