using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI
{
	internal sealed partial class CreateBulkContainerDetentionControl : ZUserControl
	{
		public CreateBulkContainerDetentionControl()
		{
			InitializeComponent();

			if (!AgencyRegistry.Instance.AllowInterCountryDetentionJobCreation.Value)
			{
				countryCode.Visible = false;
				ControlDpiScalingHelper.SetHeight(ref topPanel, countryCode.Top, false);
			}
		}

		public event EventHandler FindClicked
		{
			add { findButton.Click += value; }
			remove { findButton.Click -= value; }
		}

		public void PerformFindClicked()
		{
			findButton.PerformClick();
		}

		void OpenDetentionJob(ZGuid detentionPK)
		{
			if (!detentionPK.IsEmpty)
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyContainerDetention);
				RecordControllerForTest(controller);

				ContainerDetention detention = controller.Factory.Load<ContainerDetention>(detentionPK);

				if (detention != null)
				{
					controller.ShowEditForm(detention);
				}
			}
		}

		void childGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && e.Clicks == 2)
			{
				DataGrid.HitTestInfo info = childGrid.HitTest(e.Location);
				CurrencyManager manager;
				IList list;

				if (info.Row >= 0 && (manager = childGrid.ListManager) != null && (list = manager.List) != null && info.Row < list.Count)
				{
					OpenDetentionJob(((BulkDetentionChild)list[info.Row]).DetentionPK);
				}
			}
		}

		partial void RecordControllerForTest(ZController controller);
	}
}

#region Test
#if DEBUG

#region Testing Methods

namespace Enterprise.Freight.Agency.GUI
{
	partial class CreateBulkContainerDetentionControl
	{
		public ZController LastUsedControllerForTest { get; set; }

		partial void RecordControllerForTest(ZController controller)
		{
			LastUsedControllerForTest = controller;
		}
	}
}

#endregion


#endif
#endregion
