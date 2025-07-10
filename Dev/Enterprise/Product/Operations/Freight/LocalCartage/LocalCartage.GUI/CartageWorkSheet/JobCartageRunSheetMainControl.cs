using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Freight.GUI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.GUI
{
	[SuppressBindingMemberBashingTest] // for ShowRunSheetDetailsCheckBox
	public partial class JobCartageRunSheetMainControl : ZUserControl
	{
		public JobCartageRunSheetMainControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				new UNDGDataItemFormManager(WorkSheetModuleButtonGrid.InnerGrid, "BookedCtgMove").Initialize(CartageLegsControlWithDetails.DGLinkLabel, CartageLegsControlWithDetails.DGSubstanceGuidFindBox, CartageLegsControlWithDetails.FlashPointCalcEdit, CartageLegsControlWithDetails.DGContactGuidFindBox);
				FreightCalculateDistanceMenuHelper.SetupCalculateDistanceContextMenu(WorkSheetModuleButtonGrid.InnerGrid);
			}

			this.SizeChanged += new EventHandler(JobCartageRunSheetMainControl_SizeChanged);
			StatusDescriptionTextBox.BackColorChanged += new EventHandler(StatusDescriptionTextBox_BackColorChanged);

			WorkSheetModuleButtonGrid.InnerGrid.ColorContextKey = LegGridColourScheme.LegColourKey;
			WorkSheetModuleButtonGrid.InnerGrid.ShareActiveColorScheme = true;

			AddGPSEvent.Visible = Env.CurrentUser.IsSupportUser;
			OptimizeButton.Visible = TransportCommon.Registry.TransportRegistry.Instance.ShowSequence.Value;

			SplitContainer.AllowOverlap(ShowGPSEventsCheckBox);
			SplitContainer.AllowOverlap(ShowRunSheetDetailsCheckBox);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetGPSControls();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.SizeChanged -= new EventHandler(JobCartageRunSheetMainControl_SizeChanged);
				StatusDescriptionTextBox.BackColorChanged -= new EventHandler(StatusDescriptionTextBox_BackColorChanged);

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		CommonWorkSheet WorkSheet
		{
			get { return ((CommonWorkSheet)BindingSource.DataSource); }
		}

		void SetGPSControls()
		{
			ShowGPSEventsCheckBox.Visible = true;

			SetColourLegend();
			SplitContainer.Panel1Collapsed = true;

			GPSEventsGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(GPSEventsGrid_ColourDeciding);

			WorkSheetModuleButtonGrid.InnerGrid.FontDeciding += new EventHandler<FontDecidingEventArgs>(GPSLegsGrid_FontDeciding);
			GPSEventsGrid.FontDeciding += new EventHandler<FontDecidingEventArgs>(GPSEventsGrid_FontDeciding);

			if (WorkSheetModuleButtonGrid.InnerGrid.ListManager != null)
			{
				GPSEventsGrid.ListManager.CurrentChanged += delegate
				{ WorkSheetModuleButtonGrid.InnerGrid.Refresh(); };
				WorkSheetModuleButtonGrid.InnerGrid.ListManager.CurrentChanged += delegate
				{ GPSEventsGrid.Refresh(); };
			}
		}

		void GPSEventsGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			e.Colour = GPSGUIHelper.GetEventColour(e.ObjectAtRow as GPSEvent);
		}

		void GPSEventsGrid_FontDeciding(object sender, FontDecidingEventArgs e)
		{
			if (!populatingGrid)
			{
				GPSGUIHelper.GetEventFont(e, GPSEventsGrid.Font, WorkSheetModuleButtonGrid.InnerGrid, ShowGPSEventsCheckBox.Checked);
			}
		}

		void GPSLegsGrid_FontDeciding(object sender, FontDecidingEventArgs e)
		{
			if (!populatingGrid)
			{
				GPSGUIHelper.GetLegFont(e, WorkSheetModuleButtonGrid.InnerGrid.Font, GPSEventsGrid, ShowGPSEventsCheckBox.Checked);
			}
		}

		void SetColourLegend()
		{
			ColourLegendUsedGPSEventColour.BackColor = GPSGUIHelper.ColourForUsedGPSEvent;
			ColourLegendUsedGPSEventText.Text = Res.GetString("3cf01acb-5f16-49c5-8170-e9a4dd3ebe63", "Used");
			ColourLegendUnusedGPSEventColour.BackColor = GPSGUIHelper.ColourForUnusedGPSEvent;
			ColourLegendUnusedGPSEventText.Text = Res.GetString("2fd4d042-acb2-4e96-a82e-f2d458d6208c", "Unused");
			ColourLegendUsedForOtherGPSEventColour.BackColor = GPSGUIHelper.ColourForOverwrittenGPSEvent;
			ColourLegendUsedForOtherGPSEventText.Text = Res.GetString("59ef1256-323c-44fc-a142-25c144a39f3d", "Superseded / Overridden");
		}

		void ShowGPSEventsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			SplitContainer.Panel1Collapsed = !ShowGPSEventsCheckBox.Checked;

			populatingGrid = true;
			try
			{
				if (ShowGPSEventsCheckBox.Checked)
				{
					new GPSEventsHelper(WorkSheet).PopulateEventLeg(WorkSheet);
				}
			}
			finally
			{
				populatingGrid = false;
			}

			WorkSheetModuleButtonGrid.InnerGrid.Refresh();
			GPSEventsGrid.Refresh();
		}

		bool populatingGrid;

		GPS.GPSEventsGUIHelper GPSGUIHelper
		{
			get
			{
				if (fGPSGUIHelper == null)
				{
					fGPSGUIHelper = new GPS.GPSEventsGUIHelper(WorkSheet);
				}
				return fGPSGUIHelper;
			}
		}
		GPS.GPSEventsGUIHelper fGPSGUIHelper;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember parameter not supported", nameof(dataMember));
			}

			if (dataSource == null && WorkSheet != null)
			{
				WorkSheet.StatusInfo.ValueChanged -= new EventHandler(StatusInfo_ValueChanged);
			}

			base.SetDataBinding(dataSource, "");

			if (dataSource != null)
			{
				WorkSheet.StatusInfo.ValueChanged += new EventHandler(StatusInfo_ValueChanged);
			}

			ShowCartageLegDetails = true;
			CartageLegsControlWithDetails.CartageLegPanel.RemoveQuickAllocationBinding();

			if (WorkSheet != null)
			{
				RefreshStatusColor();
			}
		}

		void StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshStatusColor();
		}

		void StatusDescriptionTextBox_BackColorChanged(object sender, EventArgs e)
		{
			RefreshStatusColor();
		}

		void RefreshStatusColor()
		{
			StatusDescriptionTextBox.BackColor = GetColour(WorkSheet.Status, -3);
			StatusDescriptionTextBox.ForeColor = Color.White;
		}

		/// <summary>
		/// Get Color
		/// </summary>
		/// <param name="status">CommonWorkSheet.RunSheetStatuses</param>
		/// <param name="lightDark">Dark: Negative, Light: Positive</param>
		/// <returns></returns>
		Color GetColour(string status, int lightDark)
		{
			Color middle;
			int step = 25;

			if (status == nameof(CommonWorkSheet.RunSheetStatuses.None))
			{
				middle = Color.Silver;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.OK))
			{
				middle = Color.Blue;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.NearCompletion))
			{
				middle = Color.Aqua;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.Completed))
			{
				middle = Color.Lime;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.Warning))
			{
				middle = Color.FromArgb(255, 172, 0); //Orange
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.LegError))
			{
				middle = Color.Red;
			}
			else if (status == nameof(CommonWorkSheet.RunSheetStatuses.RunSheetError))
			{
				middle = Color.Red;
			}
			else
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "GetColour: Leg Status {0} not supported", status));
			}

			int red = step * lightDark + middle.R;
			red = red > 255 ? 255 : red < 0 ? 0 : red;

			int green = step * lightDark + middle.G;
			green = green > 255 ? 255 : green < 0 ? 0 : green;

			int blue = step * lightDark + middle.B;
			blue = blue > 255 ? 255 : blue < 0 ? 0 : blue;

			return Color.FromArgb(red, green, blue);
		}

		void ShowRunSheetDetailsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			ShowCartageLegDetails = ShowRunSheetDetailsCheckBox.Checked;
		}

		public bool ShowCartageLegDetails
		{
			get { return showCartageLegDetails; }
			set
			{
				if (showCartageLegDetails != value)
				{
					showCartageLegDetails = value;
					CartageLegsControlWithDetails.Visible = value;

					if (value && !DoCartageLegDetailsFit)
					{
						int widthDif = CartageLegsControlWithDetails.MinimumSize.Width - CartageLegsControlWithDetails.Parent.ClientRectangle.Width;
						int heightDif = WorkSheetModuleButtonGrid.MinimumSize.Height + TopPanel.MinimumSize.Height + CartageLegsControlWithDetails.MinimumSize.Height - CartageLegsControlWithDetails.Parent.ClientRectangle.Height;

						if (widthDif > 0)
						{
							ControlDpiScalingHelper.SetWidth(ParentForm, ParentForm.Width + widthDif, false);
						}
						if (heightDif > 0)
						{
							ControlDpiScalingHelper.SetHeight(ParentForm, ParentForm.Height + heightDif, false);
						}
					}
				}

				if (ShowRunSheetDetailsCheckBox.Checked != value)
				{
					ShowRunSheetDetailsCheckBox.Checked = value;
				}
			}
		}
		bool showCartageLegDetails;

		void JobCartageRunSheetMainControl_SizeChanged(object sender, EventArgs e)
		{
			if (!DoCartageLegDetailsFit)
			{
				ShowCartageLegDetails = false;
			}
		}

		bool DoCartageLegDetailsFit
		{
			get
			{
				return CartageLegsControlWithDetails.Parent.ClientRectangle.Width >= CartageLegsControlWithDetails.MinimumSize.Width &&
					CartageLegsControlWithDetails.Parent.ClientRectangle.Height >= WorkSheetModuleButtonGrid.MinimumSize.Height + TopPanel.MinimumSize.Height + CartageLegsControlWithDetails.MinimumSize.Height;
			}
		}

		void WorkSheetModuleButtonGrid_DragDrop(object sender, DragEventArgs e)
		{
			CommonCartageLeg[] cartageLegs = (CommonCartageLeg[])((ArrayList)e.Data.GetData(typeof(ArrayList))).ToArray(typeof(CommonCartageLeg));

			if (WorkSheet.HasRunSheetError)
			{
				Globals.Message.ShowError(Res.GetString("92ad1934-a87b-4414-86f3-f75a3c7038d7", "Cannot attach Port Transport Legs to a Run Sheet with Status: {0}", WorkSheet.StatusDescription));
			}
			else if (cartageLegs.Length > 0)
			{
				bool anyHaveChanges = false;
				foreach (CommonCartageLeg leg in cartageLegs)
				{
					if (leg.HasChanges || !leg.IsInDatabase)
					{
						anyHaveChanges = true;
					}
				}

				if (anyHaveChanges)
				{
					Globals.Message.ShowError(Res.GetString("38708f9d-6e5a-4c93-9419-e0818a0d2452", "Cannot attach Port Transport Legs to a Run Sheet if they have changes. Please Save the Port Transport Legs before using drag/drop."));
				}
				else
				{
					BusinessObjectFactory originFactory = cartageLegs.Length > 0 ? cartageLegs[0].Factory : null;

					if (originFactory != WorkSheet.Factory)
					{
						CommonCartageLeg[] cartageLegsInThisFactory = WorkSheet.Factory.Load<CommonCartageLeg>(new ZQuery(JobContainerLegsSchema.PK, Array.ConvertAll(cartageLegs, a => a.PK)));

						List<CommonCartageLeg> addedLegs = new List<CommonCartageLeg>();

						foreach (CommonCartageLeg leg in cartageLegsInThisFactory)
						{
							ZString capacityWarning = CapacityChecker.CheckTruckCapacity(leg);

							if (capacityWarning.IsEmpty)
							{
								addedLegs.Add(leg);
							}
							else
							{
								capacityWarning = Res.GetString("b4a12f13-3ad4-40b6-8740-7f5f4f213dc5", "Adding this Port Transport Leg to this Run Sheet exceeds the allocated Truck's capacity. Are you sure? \r\n\r\n{0}", capacityWarning);
								DialogResult response = Globals.Message.Show(capacityWarning, Res.GetString("7bbcc4e4-0176-4156-9af0-abc703ff61e5", "Truck Capacity Exceeded"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

								if (response == DialogResult.Yes)
								{
									addedLegs.Add(leg);
								}
							}
						}

						foreach (CommonCartageLeg cartageLeg in addedLegs)
						{
							cartageLeg.JU_EY_RunSheet = WorkSheet.PK;
						}

						WorkSheet.CartageLegs.AddRange(addedLegs);
					}
				}
			}
		}

		WorkSheetTruckCapacityChecker CapacityChecker
		{
			get
			{
				if (fCapacityChecker == null)
				{
					fCapacityChecker = new WorkSheetTruckCapacityChecker(WorkSheet);
				}

				return fCapacityChecker;
			}
		}
		WorkSheetTruckCapacityChecker fCapacityChecker;

		void WorkSheetModuleButtonGrid_DragEnter(object sender, DragEventArgs e)
		{
			CommonCartageLeg[] cartageLegs = (CommonCartageLeg[])((ArrayList)e.Data.GetData(typeof(ArrayList))).ToArray(typeof(CommonCartageLeg));
			e.Effect = (cartageLegs.Length > 0 && ((BusinessObject)CurrentDataItem).Factory != cartageLegs[0].Factory) ? DragDropEffects.Move : DragDropEffects.None;
		}

		void GroupButton_Click(object sender, EventArgs e)
		{
			CommonCartageLeg[] cartageLegs = WorkSheetModuleButtonGrid.InnerGrid.GetSelectedElements<CommonCartageLeg>();
			if (cartageLegs.Length > 0)
			{
				new RunSheetSequenceHelper(cartageLegs[0]).Group(cartageLegs);
				SelectCartageLegsInGrid(cartageLegs);
				WorkSheet.Validation.ValidateEY_RQ_Truck();
			}
		}

		void UnGroupButton_Click(object sender, EventArgs e)
		{
			CommonCartageLeg[] cartageLegs = WorkSheetModuleButtonGrid.InnerGrid.GetSelectedElements<CommonCartageLeg>();
			if (cartageLegs.Length > 0)
			{
				var errors = new RunSheetSequenceHelper(cartageLegs[0]).Split(cartageLegs);
				if (errors != null)
				{
					var caption = Res.GetString("6fc7091b-40a6-413c-b070-a13e16d792fc", "Splitting Legs");
					Globals.Message.ShowInformation(errors, caption);
				}
				SelectCartageLegsInGrid(cartageLegs);
			}
		}

		void MoveUpButton_Click(object sender, EventArgs e)
		{
			MoveLegs(sequenceHelper => sequenceHelper.MoveUp());
		}

		void MoveDownButton_Click(object sender, EventArgs e)
		{
			MoveLegs(sequenceHelper => sequenceHelper.MoveDown());
		}

		void MoveLegs(Func<RunSheetSequenceHelper, CommonCartageLeg[]> moveAction)
		{
			var currentLeg = CurrentCartageLeg;
			if (currentLeg != null && currentLeg.WorkSheet != null)
			{
				var sameGroupLegs = moveAction(new RunSheetSequenceHelper(currentLeg));
				SelectCartageLegsInGrid(sameGroupLegs);
			}
		}

		void OrderButton_Click(object sender, EventArgs e)
		{
			CommonCartageLeg[] selectedCartageLegs = WorkSheetModuleButtonGrid.InnerGrid.GetSelectedElements<CommonCartageLeg>();

			CommonCartageLeg[] runSheetLegs = RunSheetSequenceHelper.GetRunSheetLegs(WorkSheet, JobContainerLegsSchema.JU_PlannedPickupTime);
			RunSheetSequenceHelper.SetSequenceNumberBasedOnCurrentOrder(runSheetLegs);

			WorkSheet.CartageLegs.ApplySort(WorkSheet.CartageLegs.SortComparer);

			if (selectedCartageLegs.Length > 0)
			{
				SelectCartageLegsInGrid(selectedCartageLegs);
			}
		}

		CommonCartageLeg CurrentCartageLeg
		{
			get
			{
				CommonCartageLeg commonCartageLeg = null;
				CurrencyManager listManager = WorkSheetModuleButtonGrid.InnerGrid.ListManager;
				if (listManager != null && listManager.Position >= 0)
				{
					commonCartageLeg = (CommonCartageLeg)listManager.GetCurrent();
				}
				return commonCartageLeg;
			}
		}

		public void SelectCartageLegsInGrid(CommonCartageLeg[] legs)
		{
			WorkSheet.CartageLegs.ApplySort(WorkSheet.CartageLegs.SortComparer);

			IBindingList list = WorkSheetModuleButtonGrid.InnerGrid.List;
			for (int i = 0; i < list.Count; i++)
			{
				if (Contained(legs, (CommonCartageLeg)list[i]))
				{
					WorkSheetModuleButtonGrid.InnerGrid.ListManager.Position = i;
					WorkSheetModuleButtonGrid.InnerGrid.Select(i);
				}
				else
				{
					WorkSheetModuleButtonGrid.InnerGrid.UnSelect(i);
				}
			}
		}

		bool Contained(CommonCartageLeg[] legs, CommonCartageLeg findLeg)
		{
			foreach (CommonCartageLeg leg in legs)
			{
				if (findLeg.PK == leg.PK)
				{
					return true;
				}
			}
			return false;
		}

		void AddGPSEvent_Click(object sender, EventArgs e)
		{
			if (WorkSheet != null)
			{
				GPSClientActivityTestDataEntryForm.ShowDialog(WorkSheet);
				new GPSEventsHelper(WorkSheet).PopulateEventLeg(WorkSheet);
				GPSEventsGrid.Refresh();
			}
		}

		void UpdatePickInButton_Click(object sender, EventArgs e)
		{
			SetLegTimeFromSelectedEvent(JobContainerLegsSchema.JU_PickupTimeIn.Name);
		}

		void UpdatePickOutButton_Click(object sender, EventArgs e)
		{
			SetLegTimeFromSelectedEvent(JobContainerLegsSchema.JU_PickupTimeOut.Name);
		}

		void UpdateWaitPointInButton_Click(object sender, EventArgs e)
		{
			SetLegTimeFromSelectedEvent(JobContainerLegsSchema.JU_WaitPointTimeIn.Name);
		}

		void UpdateWaitPointOutButton_Click(object sender, EventArgs e)
		{
			SetLegTimeFromSelectedEvent(JobContainerLegsSchema.JU_WaitPointTimeOut.Name);
		}

		void UpdateDeliverInButton_Click(object sender, EventArgs e)
		{
			SetLegTimeFromSelectedEvent(JobContainerLegsSchema.JU_DeliverTimeIn.Name);
		}

		void UpdateDeliverOutButton_Click(object sender, EventArgs e)
		{
			SetLegTimeFromSelectedEvent(JobContainerLegsSchema.JU_DeliverTimeOut.Name);
		}

		void SetLegTimeFromSelectedEvent(ZString propertyName)
		{
			CommonCartageLeg[] cartageLegs = WorkSheetModuleButtonGrid.InnerGrid.GetSelectedElements<CommonCartageLeg>();
			GPSEvent[] events = GPSEventsGrid.GetSelectedElements<GPSEvent>();
			if (events.Length == 1 && cartageLegs.Length == 1)
			{
				cartageLegs[0][propertyName] = events[0].EventTime;
				var activity = cartageLegs[0].Factory.Load<GPSSupporterActivity>(events[0].EventPK);
				activity.EN_JU = cartageLegs[0].PK;
				new GPSEventsHelper(WorkSheet).PopulateEventLeg(WorkSheet);
				WorkSheetModuleButtonGrid.InnerGrid.Refresh();
				GPSEventsGrid.Refresh();
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("23217474-6cc8-470a-a8db-e5cd68396d51", "You need to select only one event and one leg to update. All grouped legs will be updated together even if you select only one of them."));
			}
		}

		void OptimizeButton_Click(object sender, EventArgs e)
		{
		}
	}
}
