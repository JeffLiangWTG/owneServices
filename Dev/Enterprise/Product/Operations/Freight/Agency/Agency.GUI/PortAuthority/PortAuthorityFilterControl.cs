using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class PortAuthorityFilterControl : ZUserControl
	{
		const string VisiblePropertyName = "ShowThirdPartyOptionsForBinding"; // Property name.

		#region State

		public enum State
		{
			None,
			Locked,
			CollectingData,
			BuildingMessage,
			Saving,
		}

		#endregion

		public PortAuthorityFilterControl()
		{
			InitializeComponent();
			AddContextMenuOptions();
		}

		public event EventHandler SendClicked
		{
			add { sendButton.Click += value; }
			remove { sendButton.Click -= value; }
		}

		public event EventHandler CancelClicked
		{
			add { cancelButton.Click += value; }
			remove { cancelButton.Click -= value; }
		}

		public void PerformClickSend()
		{
			sendButton.PerformClick();
		}

		public void PerformClickCancel()
		{
			cancelButton.PerformClick();
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void SetState(State state)
		{
			PortAuthority filter = CurrentDataItem as PortAuthority;

			if (filter != null)
			{
				stateLabel.Text = GetStateText(state);

				bool disable = (state != State.None);

				sendButton.Enabled = !disable;
				cancelButton.Enabled = !disable;
				filter.SetReadOnlyIncludingChildren(disable);

				Application.DoEvents();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ZBool ShowThirdPartyOptionsForBinding
		{
			get { return thirdPartyPanel.Visible; }
			set
			{
				thirdPartyPanel.Visible = value;
				ControlDpiScalingHelper.SetHeight(ref topPanel, value ? thirdPartyPanel.Bottom : thirdPartyPanel.Top, false);
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public event EventHandler ShowThirdPartyOptionsForBindingChanged
		{
			add { }
			remove { }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public PortMessageIssue CurrentIssue
		{
			get
			{
				CurrencyManager manager = issuesGrid.ListManager;
				return manager == null || manager.Position < 0 ? null : (PortMessageIssue)manager.GetCurrent();
			}
		}

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<PortAuthorityFilterControl>()
				.Property<ZBool>(VisiblePropertyName)
				.Result;
		}

		#region Implementation

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				DataBindings.RemoveBinding(VisiblePropertyName);
			}

			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				DataBindings.Add(new KBinding(VisiblePropertyName, CurrentDataItem, PortAuthority.Schema.DeliverTo3rdParty));
			}
		}

		void AddContextMenuOptions()
		{
			Menu.MenuItemCollection menu = issuesGrid.ContextMenu.MenuItems;

			MenuItem menuItemEdit = new ZMenuItem(ResString.GetMultilingualString("PortAuthorityFilterDialog|issuesGrid|MenuItems|Edit", "Edit"), editTarget);
			menu.Add(0, menuItemEdit);

			menu.Add(1, new ZMenuItem("-"));
		}

		string GetStateText(State state)
		{
			switch (state)
			{
				case State.None:
				case State.Locked:
					return "";

				case State.CollectingData:
					return Res.GetString("PortAuthorityFilterDialog|State|CollectingData", "Collecting Data ...");

				case State.BuildingMessage:
					return Res.GetString("PortAuthorityFilterDialog|State|BuildingMessage", "Building Message ...");

				case State.Saving:
					return Res.GetString("PortAuthorityFilterDialog|State|Saving", "Saving ...");

				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown State");
			}
		}

		void OpenFilterIssue(PortMessageIssue issue)
		{
			Form form = FindForm();

			if (form != null && !issue.TargetPK.IsEmpty)
			{
				ZController controller = GetController(issue.TargetCode);
				SetLastControllerForTesting(controller);

				if (controller != null)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					controller.SetFormsModalTo(form);
					controller.ShowEditForm(factory.Load(controller.TypeOfTopLevelBusinessObject, issue.TargetPK));
				}
			}
		}

		ZController GetController(ZString targetCode)
		{
			switch (targetCode)
			{
				case JobShipmentSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.AgencyBillOfLading);

				case RefVesselSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.RefVessel);

				default:
					return null;
			}
		}

		void OpenFilterIssue(int row)
		{
			if (row > -1)
			{
				PortMessageIssue issue = (PortMessageIssue)issuesGrid.ListManager.List[row];
				OpenFilterIssue(issue);
			}
		}

		partial void SetLastControllerForTesting(ZController controller);

		void Grid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				OpenFilterIssue(issuesGrid.HitTest(e.X, e.Y).Row);
			}
		}

		void senderNotificationsGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				e.Handled = true;
				OpenFilterIssue(issuesGrid.ListManager.Position);
			}
		}

		void senderNotificationsGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			e.Colour = Color.White;
		}

		void editTarget(object sender, EventArgs e)
		{
			OpenFilterIssue(issuesGrid.ListManager.Position);
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Members

namespace Enterprise.Freight.Agency.GUI
{
	partial class PortAuthorityFilterControl
	{
		public void OpenFilterIssueForTesting(PortMessageIssue issue)
		{
			int row = issuesGrid.ListManager.List.IndexOf(issue);

			Rectangle rec = issuesGrid.GetCellBounds(row, 0);

			MouseEventArgs args = new MouseEventArgs(
				MouseButtons.Left, 2,
				(rec.Left + rec.Right) >> 1,
				(rec.Top + rec.Bottom) >> 1,
				0);

			issuesGrid.PerformMouseDownForTest(args, row: row);
		}

		partial void SetLastControllerForTesting(ZController controller)
		{
			LastControllerForTesting = controller;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ZController LastControllerForTesting { get; set; }
	}
}

#endregion


#endif
#endregion
