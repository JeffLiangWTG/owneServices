using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class PackContainerForm : ZForm
	{
		public PackContainerForm(PackContainerHelper sailingHelper)
	: base(sailingHelper)
		{
			this.SailingHelper = sailingHelper;
			this.Sailing = sailingHelper.Sailing;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			AllowDrop = true;
			if (sailingHelper.Containers.Count == 0)
			{
				sailingHelper.AddBookingContainer();
				sailingHelper.Containers.HasChanges = false;
				Sailing.HasChanges = false;
			}
			sailingHelper.Containers.QueryReJoinPackLines += new CommonContainerCollection.QueryReJoinPackLinesEventHandler(OnPackContainerForm_QueryReJoinPackLines);
			SetUpContextMenus();
			SetUpActionMenu();

#if DEBUG
			TypeDescriptor.AddAttributes(ContainerVesselLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		private JobSailing fSailing;
		public JobSailing Sailing
		{
			get { return fSailing; }
			set { fSailing = value; }
		}

		public PackContainerHelper SailingHelper
		{
			get { return fSailingHelper; }
			set { fSailingHelper = value; }
		}
		PackContainerHelper fSailingHelper;

		protected override void InitialiseForm()
		{
			Sailing = BusinessEntity as JobSailing;
			base.InitialiseForm();
			if (!this.IsDesignMode() && Sailing != null &&
				(Sailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Air
				|| Sailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Road))
			{
				for (int i = LoadListGrid.ColumnStyles.Count - 1; i >= 0; i--) //ZGridColumnInfo Column in FilteredGrid.ColumnStyles)
				{
					ZString columnName = ((ZGridColumnInfo)LoadListGrid.ColumnStyles[i]).ColumnName;
					if (columnName == PackLine.Schema.JL_Calc_JV_Vessel)
					{
						LoadListGrid.ColumnStyles.RemoveAt(i);
						break;
					}
				}
				for (int i = ContainerDetailsGrid.ColumnStyles.Count - 1; i >= 0; i--) //ZGridColumnInfo Column in FilteredGrid.ColumnStyles)
				{
					ZString columnName = ((ZGridColumnInfo)ContainerDetailsGrid.ColumnStyles[i]).ColumnName;
					if (columnName == PackLine.Schema.JL_Calc_JV_Vessel)
					{
						ContainerDetailsGrid.ColumnStyles.RemoveAt(i);
						break;
					}
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			string onlyThisJourney = Res.GetString("PackContainerForm|ThisSailingCheckBox|OnlyThisJourney", "Only This Journey");

			if (Sailing != null && Sailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Air)
			{
				VoyageLabel.Text = Res.GetString("PackContainerForm|VoyageLabel|FlightNo", "Flight No.:");
				VoyageNoPanel.Location = VesselPanel.Location;
				VesselPanel.Visible = false;
				ThisSailingCheckBox.Text = Res.GetString("PackContainerForm|ThisSailingCheckBox|OnlyThisFlight", "Only This Flight");
				LoadListGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo].ColumnStyle.HeaderText = Res.GetString("PackContainerForm|LoadListGrid|Flight", "Flight");
				if (ContainerDetailsGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo] != null)
				{
					ContainerDetailsGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo].ColumnStyle.HeaderText = Res.GetString("PackContainerForm|ContainerDetailsGrid|Flight", "Flight");
				}
			}
			else if (Sailing != null && Sailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Road)
			{
				VoyageLabel.Text = Res.GetString("PackContainerForm|VoyageLabel|TruckRef", "Truck Ref.:");
				VoyageNoPanel.Location = VesselPanel.Location;
				VesselPanel.Visible = false;
				ThisSailingCheckBox.Text = onlyThisJourney;
				LoadListGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo].ColumnStyle.HeaderText = Res.GetString("PackContainerForm|LoadListGrid|Truck", "Truck");
				if (ContainerDetailsGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo] != null)
				{
					ContainerDetailsGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo].ColumnStyle.HeaderText = Res.GetString("PackContainerForm|ContainerDetailsGrid|Truck", "Truck");
				}
			}
			else if (Sailing != null && Sailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Rail)
			{
				VoyageLabel.Text = Res.GetString("PackContainerForm|VoyageLabel|JourneyNo", "Journey No.:");
				VesselLabel.Text = Res.GetString("PackContainerForm|VesselLabel|Journey", "Journey:");
				ThisSailingCheckBox.Text = onlyThisJourney;
				LoadListGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo].ColumnStyle.HeaderText = Res.GetString("PackContainerForm|LoadListGrid|Journey", "Journey");
				if (ContainerDetailsGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo] != null)
				{
					ContainerDetailsGrid.Columns[PackLine.Schema.JL_Calc_JV_VoyageNo].ColumnStyle.HeaderText = Res.GetString("PackContainerForm|ContainerDetailsGrid|Journey", "Journey");
				}
			}
			DisableNewAction();
		}

		public override string FormCaption
		{
			get { return Res.GetString("04c0b70b-4fdc-4018-9fde-d697d2f8bb03", "Pack Containers"); }
		}

		protected void SetUpContextMenus()
		{
			MenuItem packMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Context.Pack", "Pack"));
			packMenuItem.Click += new EventHandler(Pack_Click);
			LoadListGrid.ContextMenu.MenuItems.Add(0, packMenuItem);

			MenuItem packAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Context.PackAll", "Pack All"));
			packAllMenuItem.Click += new EventHandler(PackAll_Click);
			LoadListGrid.ContextMenu.MenuItems.Add(1, packAllMenuItem);

			MenuItem splitMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Context.Split", "Split"));
			splitMenuItem.Click += new EventHandler(Split_Click);
			LoadListGrid.ContextMenu.MenuItems.Add(2, splitMenuItem);

			LoadListGrid.ContextMenu.MenuItems.Add(3, new ZMenuItem("-"));

			MenuItem unpackMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Context.Unpack", "Unpack"));
			unpackMenuItem.Click += new EventHandler(Unpack_Click);
			ContainerDetailsGrid.ContextMenu.MenuItems.Add(0, unpackMenuItem);

			MenuItem unpackAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Context.UnpackAll", "Unpack All"));
			unpackAllMenuItem.Click += new EventHandler(UnpackAll_Click);
			ContainerDetailsGrid.ContextMenu.MenuItems.Add(1, unpackAllMenuItem);

			ContainerDetailsGrid.ContextMenu.MenuItems.Add(2, new ZMenuItem("-"));
		}

		protected void SetUpActionMenu()
		{
			ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));

			MenuItem addContainerMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.AddContainer", "Add Container"));
			addContainerMenuItem.Click += new EventHandler(NewContainer_Click);
			ActionsMenuItem.MenuItems.Add(addContainerMenuItem);

			MenuItem deleteContainerMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.DeleteContainer", "Delete Container"));
			deleteContainerMenuItem.Click += new EventHandler(RemoveContainer_Click);
			ActionsMenuItem.MenuItems.Add(deleteContainerMenuItem);

			BookingContainersGrid.RowsDeleting += BookingContainersGrid_RowDeleting;

			ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));

			MenuItem packMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.Pack", "Pack"));
			packMenuItem.Click += new EventHandler(Pack_Click);
			ActionsMenuItem.MenuItems.Add(packMenuItem);

			MenuItem packAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.PackAll", "Pack All"));
			packAllMenuItem.Click += new EventHandler(PackAll_Click);
			ActionsMenuItem.MenuItems.Add(packAllMenuItem);

			MenuItem splitMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.Split", "Split"));
			splitMenuItem.Click += new EventHandler(Split_Click);
			ActionsMenuItem.MenuItems.Add(splitMenuItem);

			ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));

			MenuItem unpackMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.Unpack", "Unpack"));
			unpackMenuItem.Click += new EventHandler(Unpack_Click);
			ActionsMenuItem.MenuItems.Add(unpackMenuItem);

			MenuItem unpackAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.UnpackAll", "Unpack All"));
			unpackAllMenuItem.Click += new EventHandler(UnpackAll_Click);
			ActionsMenuItem.MenuItems.Add(unpackAllMenuItem);

			ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));

			MenuItem buildConsolMenuItem = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Action.BuildConsol", "Build Consol"));
			buildConsolMenuItem.Click += new EventHandler(BuildConsol_Click);
			ActionsMenuItem.MenuItems.Add(buildConsolMenuItem);
		}

		#region Events

		private void NewContainer_Click(object sender, EventArgs e)
		{
			ForwardingContainer container = SailingHelper.Containers.AddNew();
			if (container == null)
			{
				Globals.Message.ShowError(Res.GetString("ac1e82a3-7b74-43a2-9c89-c94b247bc68e", "This container number already exists on the sailing."), Res.GetString("51202e87-7618-4c04-8e2f-ab2cf81d46f3", "Container Error"));
			}
		}

		private void Pack_Click(object sender, EventArgs e)
		{
			ForwardingContainer currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				currentContainer.AddPackLines(LoadListGrid.SelectedElements);
				LoadListGrid.Refresh();
			}
			else
			{
				NoContainerSelected();
			}
		}

		private void PackAll_Click(object sender, EventArgs e)
		{
			ForwardingContainer currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				currentContainer.AddPackLines(Sailing.UnAllocatedPackLines.ToArray());
				LoadListGrid.Refresh();
			}
			else
			{
				NoContainerSelected();
			}
		}

		#region Unpack_Click

		private void Unpack_Click(object sender, EventArgs e)
		{
			ForwardingContainer currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				currentContainer.RemovePackLines(ContainerDetailsGrid.SelectedElements);
				ContainerDetailsGrid.Refresh();
			}
			else
			{
				NoContainerSelected();
			}
		}

		#endregion

		#region UnpackAll_Click

		private void UnpackAll_Click(object sender, EventArgs e)
		{
			ForwardingContainer currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				currentContainer.RemovePackLines(currentContainer.PackLines.ToArray());
			}
			else
			{
				NoContainerSelected();
			}
		}

		#endregion

		#region Remove_Container_Click

		private void RemoveContainer_Click(object sender, EventArgs e)
		{
			ForwardingContainer currentContainer = GetSelectedContainer();
			RemoveContainer(currentContainer);
		}

		void BookingContainersGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			if (!RemoveContainer((ForwardingContainer)e.Objects.ElementAtOrDefault(0)))
			{
				e.Cancel = true;
			}
		}

		public bool RemoveContainer(ForwardingContainer container)
		{
			if (container != null)
			{
				if (container.PackLines.Count != 0)
				{
					DialogResult result = ConfirmDelete();
					if (result == DialogResult.Yes)
					{
						container.RemovePackLines(container.PackLines.ToArray());
						LoadListGrid.Refresh();
						container.Delete();
					}
					else if (result == DialogResult.No)
					{
						CannotRemoveError();
						return false;
					}
				}
				else
				{
					container.Delete();
				}
			}
			else
			{
				NoContainerSelected();
				return false;
			}
			return true;
		}
		#endregion

		#region Split_Click

		private void Split_Click(object sender, EventArgs e)
		{
			ForwardingContainer currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				if (LoadListGrid.SelectedElements.Length == 1)
				{
					PackLine line = (PackLine)LoadListGrid.SelectedElements[0];
					PackLineSplitter splitter = new PackLineSplitter(line, currentContainer);
					SplitForm splitForm = new SplitForm(splitter);
					ZFormModaliser.Show(splitForm, this);
				}
				else
				{
					NoPackLineSelected();
				}
			}
			else
			{
				NoContainerSelected();
			}
		}

		#endregion

		#region QueryReJoinPackLines

		private void OnPackContainerForm_QueryReJoinPackLines(object sender, CommonContainerCollection.QueryReJoinPackLinesEventArgs e)
		{
			string message = e.Line != null && !e.Line.JL_Calc_JS_UniqueConsignRef.IsEmpty
				? Res.GetString("25adaa05-e4a3-412d-b1d5-de47a3acfc84", "Rejoin packlines from booking {0} that were previously split?", e.Line.JL_Calc_JS_UniqueConsignRef)
				: Res.GetString("13cef0e6-f854-4f19-a66c-4cb045bd7c97", "Rejoin packlines that were previously split?");
			DialogResult result = Globals.Message.Show(message, Res.GetString("e142bc1d-cad2-490d-a546-aadd1b8b40f0", "Rejoin split packlines"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		#endregion

		#endregion

		#region Error Messages

		protected void NoContainerSelected()
		{
			Globals.Message.Show(Res.GetString("d49a6a51-f032-48a2-ac80-2e59a710a0d0", "Please select an existing container or create a new container."), Res.GetString("5d118413-78f1-4bd0-8527-d7f9441c1e99", "Select A Container"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		protected void NoPackLineSelected()
		{
			Globals.Message.Show(Res.GetString("ee00006f-b133-444a-928c-166a51e5dabe", "Please select a packline."), Res.GetString("3c424075-c1ab-4c54-b471-3de3fa5be131", "Select A Packline"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		protected DialogResult ConfirmDelete()
		{
			return Globals.Message.Show(Res.GetString("cb61c57e-ce89-411c-8fc9-645c01e20e5c", "All shipments in this container will be moved back to the Load List.\r\nDo you want to delete this container?"), Res.GetString("91f3a1c1-2ead-45fe-a77b-4386c59c71dc", "Remove Container"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
		}

		protected void CannotRemoveError()
		{
			Globals.Message.ShowError(Res.GetString("1f10f005-389b-498c-b356-b12c903aa030", "Cannot delete a container with shipments."), Res.GetString("f5e80183-2c8e-4cf4-89b4-f51222b59aa1", "Remove Container Aborted"));
		}

		protected ForwardingContainer GetSelectedContainer()
		{
			return BookingContainersGrid.SelectedElements.Length > 0 ? (ForwardingContainer)BookingContainersGrid.SelectedElements[0] : null;
		}

		#endregion

		#region Drag/Drop

		private void ContainerDetailsGrid_DragEnter(object sender, DragEventArgs e)
		{
			BusinessObject[] lines = (BusinessObject[])((ArrayList)e.Data.GetData(typeof(ArrayList))).ToArray(typeof(BusinessObject));
			if (lines.Length > 0)
			{
				PackLine firstLine = (PackLine)lines[0];
				if (firstLine.GetContainer(Sailing) == null)
				{
					e.Effect = DragDropEffects.Move;
				}
				else
				{
					e.Effect = DragDropEffects.None;
				}
			}
		}

		private void LoadListGrid_DragEnter(object sender, DragEventArgs e)
		{
			BusinessObject[] lines = (BusinessObject[])((ArrayList)e.Data.GetData(typeof(ArrayList))).ToArray(typeof(BusinessObject));
			if (lines.Length > 0)
			{
				PackLine firstLine = (PackLine)lines[0];
				if (firstLine.GetContainer(Sailing) != null)
				{
					e.Effect = DragDropEffects.Move;
				}
				else
				{
					e.Effect = DragDropEffects.None;
				}
			}
		}

		private void LoadListGrid_DragDrop(object sender, DragEventArgs e)
		{
			BusinessObject[] lines = (BusinessObject[])((ArrayList)e.Data.GetData(typeof(ArrayList))).ToArray(typeof(BusinessObject));
			ForwardingContainer currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				currentContainer.RemovePackLines(lines);
				ContainerDetailsGrid.Refresh();
				LoadListGrid.Refresh();
			}
		}

		private void ContainerDetailsGrid_DragDrop(object sender, DragEventArgs e)
		{
			BusinessObject[] lines = (BusinessObject[])((ArrayList)e.Data.GetData(typeof(ArrayList))).ToArray(typeof(BusinessObject));
			ForwardingContainer currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				currentContainer.AddPackLines(lines);
				ContainerDetailsGrid.Refresh();
				LoadListGrid.Refresh();
			}
		}

		#endregion

		private void BuildConsol_Click(object sender, EventArgs e)
		{
			if (!Sailing.HasChanges)
			{
				PackContainerHelper sailingHelper = new PackContainerHelper(Sailing);
				BuildConsolForm consolBuilderForm = new BuildConsolForm(sailingHelper);
				ZFormModaliser.Show(consolBuilderForm, this);
				consolBuilderForm.Closed += new EventHandler(ConsolBuilderForm_Closed);
			}
			else
			{
				Globals.Message.Show(Res.GetString("bd43e9f8-979d-44c2-b568-6ba795a65ca4", "Please save container packing before building a consol."),
					Res.GetString("3844c823-6086-45f0-9dee-d489d815a09b", "Build Consol"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		void ConsolBuilderForm_Closed(object sender, EventArgs e)
		{
			if (!IsClosed)
			{
				IsClosed = true;
				this.Close();
			}
		}

		bool IsClosed;

		private void JX_JA_RL_NKPortOfLoadingBoundCodeFindBox_Load(object sender, EventArgs e)
		{
		}
	}
}
