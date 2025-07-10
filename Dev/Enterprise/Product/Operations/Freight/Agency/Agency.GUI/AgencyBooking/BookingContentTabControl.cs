using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public class BookingContentTabControl : ZTemplateTabControl
	{
		public BookingContentTabControl()
		{
			InitialiseTabs();
		}

		#region SetPackingMode

		public void UpdateVisibleTabsForPackingMode(string packingMode)
		{
			SuspendLayout();
			try
			{
				var tabPagesToShow = new List<ZTabPage>();
				switch (packingMode)
				{
					case Constants.ContainerModes.FCL:
						tabPagesToShow.Add(ContainersTab);
						tabPagesToShow.Add(PackLinesTab);
						break;

					case Constants.ContainerModes.RollOnRollOff:
						tabPagesToShow.Add(VehiclesTab);
						break;

					default:
						tabPagesToShow.Add(TopLevelPacksTab);
						break;
				}
				tabPagesToShow.Add(CustomFieldsTab);

				var allTabPages = new ZTabPage[]
					{
						ContainersTab,
						PackLinesTab,
						TopLevelPacksTab,
						VehiclesTab,
						CustomFieldsTab
					};

				foreach (ZTabPage tabPage in allTabPages)
				{
					if (tabPagesToShow.Contains(tabPage))
					{
						tabPage.TabVisible = true;

						ZBindingTabPage desiredBindingTab = tabPage as ZBindingTabPage;
						if (desiredBindingTab != null && !desiredBindingTab.IsBound)
						{
							desiredBindingTab.Bind();
						}
					}
					else
					{
						tabPage.TabVisible = false;
					}
				}

				SelectedTab = tabPagesToShow.FirstOrDefault();
			}
			finally
			{
				ResumeLayout();
			}
		}

		#endregion

		#region Initialising the Tabs

		void InitialiseTabs()
		{
			ContainersTab = new ZTabPage();
			PackLinesTab = new ZTabPage();
			TopLevelPacksTab = new ZTabPage();
			VehiclesTab = new ZTabPage();
			CustomFieldsTab = new ZTabPage();

			ContainersControl = new ContainersUserControl();
			PackLinesControl = new FCLPackLinesControl();
			TopLevelPacksControl = new TopLevelPacksControl();
			TopLevelPacksTotalsControl = new TopLevelPacksTotalsControl();
			VehiclesControl = new VehiclesUserControl();
			CustomFieldsControl = new ProcessTemplateCustomFieldsControl();

			SuspendLayout();

			ContainersTab.SuspendLayout();
			PackLinesTab.SuspendLayout();
			TopLevelPacksTab.SuspendLayout();
			VehiclesTab.SuspendLayout();
			CustomFieldsTab.SuspendLayout();

			ContainersControl.SuspendLayout();
			PackLinesControl.SuspendLayout();
			TopLevelPacksControl.SuspendLayout();
			VehiclesControl.SuspendLayout();
			CustomFieldsControl.SuspendLayout();

			try
			{
				CustomFieldsControl.AllowDrop = true;
				CustomFieldsControl.Dock = DockStyle.Fill;
				CustomFieldsControl.Name = "CustomFieldsControl";
				CustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("df7f0573-5d82-4da9-976d-d1de7b0b897a", "To make use of this tab, please setup Booking custom fields for L&&A Booking in Workflow Manager.");
				CustomFieldsControl.TabIndex = 0;

				CustomFieldsTab.Controls.Add(CustomFieldsControl);
				CustomFieldsTab.Name = "CustomFieldsTab";
				CustomFieldsTab.Padding = ControlDpiScalingHelper.NewScaledPadding(8, isInStandardDpi: true);
				CustomFieldsTab.Text = Res.GetString("29f7fd40-a7ca-47ef-acb7-7f67433ba85e", "Custom Fields");

				ContainersControl.Name = "ContainersControl";
				ContainersControl.Dock = DockStyle.Fill;

				ContainersTab.Name = "ContainersTab";
				ContainersTab.Text = Res.GetString("680c7b94-bfbb-491e-920d-5b53459aa577", "Containers");
				ContainersTab.Controls.Add(ContainersControl);

				PackLinesControl.Name = "PackLinesControl";
				PackLinesControl.Dock = DockStyle.Fill;

				PackLinesTab.Name = "PackLinesTab";
				PackLinesTab.Text = Res.GetString("e9d44560-f589-448d-b3e3-e36175c54a9d", "Packs");
				PackLinesTab.Controls.Add(PackLinesControl);

				TopLevelPacksControl.Name = "TopLevelPacksControl";
				TopLevelPacksControl.Dock = DockStyle.Fill;
				TopLevelPacksControl.RemoveColumnsWhichAreUnavailableOnBookings();

				TopLevelPacksTotalsControl.Name = "TopLevelPacksTotalsControl";
				TopLevelPacksTotalsControl.Dock = DockStyle.Bottom;

				TopLevelPacksTab.Name = "TopLevelPacksTab";
				TopLevelPacksTab.Text = Res.GetString("e9d44560-f589-448d-b3e3-e36175c54a9d", "Packs");
				TopLevelPacksTab.Controls.Add(TopLevelPacksControl);
				TopLevelPacksTab.Controls.Add(TopLevelPacksTotalsControl);

				VehiclesControl.Name = "VehiclesControl";
				VehiclesControl.Dock = DockStyle.Fill;

				VehiclesTab.Name = "VehiclesTab";
				VehiclesTab.Text = Res.GetString("fd18f2cc-8590-468e-b8c8-5ea91b582ecf", "Vehicles");
				VehiclesTab.Controls.Add(VehiclesControl);

				Controls.Add(ContainersTab);
				Controls.Add(VehiclesTab);
				Controls.Add(TopLevelPacksTab);
				Controls.Add(PackLinesTab);
				Controls.Add(CustomFieldsTab);
			}
			finally
			{
				ContainersControl.ResumeLayout();
				PackLinesControl.ResumeLayout();
				TopLevelPacksControl.ResumeLayout();
				VehiclesControl.ResumeLayout();
				CustomFieldsControl.ResumeLayout();

				CustomFieldsTab.ResumeLayout();
				ContainersTab.ResumeLayout();
				PackLinesTab.ResumeLayout();
				TopLevelPacksTab.ResumeLayout();
				VehiclesTab.ResumeLayout();

				ResumeLayout();
			}
		}

		#endregion

		ZTabPage ContainersTab;
		ZTabPage PackLinesTab;
		ZTabPage TopLevelPacksTab;
		ZTabPage VehiclesTab;
		ZTabPage CustomFieldsTab;

		ContainersUserControl ContainersControl;
		FCLPackLinesControl PackLinesControl;
		TopLevelPacksControl TopLevelPacksControl;
		TopLevelPacksTotalsControl TopLevelPacksTotalsControl;
		VehiclesUserControl VehiclesControl;
		ProcessTemplateCustomFieldsControl CustomFieldsControl;
	}
}


