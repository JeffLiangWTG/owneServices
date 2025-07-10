using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class RateEntryPanel : ZUserControl
	{
		public RateEntryPanel()
		{
			InitializeComponent();
			InitializeSplitContainerWithLocations();
		}

		#region Overridable Controls

		public virtual ZGrid RateEntryGrid
		{
			get { return null; }
		}

		public virtual RateLinesAndItemsControl RateLinesAndItemsControl
		{
			get { return null; }
		}

		public virtual CostingRateLineAndItemsControl CostingRateLineAndItemsControl
		{
			get { return null; }
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!firstBound && dataSource != null)
			{
				RateEntryGrid.ColourDeciding += EntriesGrid_ColourDeciding;
				AddColumnsToEntryGrid(dataSource, dataMember);
				firstBound = true;

				RateEntryGrid.AfterBind += RateEntryGridOnAfterBind;
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		void RateEntryGridOnAfterBind(object sender, EventArgs eventArgs)
		{
			currentEntryRelatedControlsContainer.SecurityMessageLabel.BindTo = RateEntryGrid.BindTo + (NoResString)".SecurityMessage";
			RateEntryGrid.ListManager.CurrentChanged += OnRateEntryChanged;
			OnRateEntryChanged(sender, eventArgs);
		}

		void OnRateEntryChanged(object sender, EventArgs eventArgs)
		{
			if (RateEntryGrid.CurrentRowIndex >= 0 && RateEntryGrid.List[RateEntryGrid.CurrentRowIndex] is RateEntry rateEntry)
			{
				currentEntryRelatedControlsContainer.OnParentEntryChanged(rateEntry);
			}
		}

		protected bool firstBound;

		void AddColumnsToEntryGrid(object dataSource, string dataMember)
		{
			var bindingMember = BindingContext[dataSource, dataMember].GetCurrent();
			var dataSourceType = bindingMember.GetType();
			var ratingHeader = bindingMember as IRatingHeader;
			var info = RateEntryCollectionGUIInfo.GetInfo(Category, dataSourceType, ratingHeader.IsGlobal(), ratingHeader.IsStandardCostRate());

			foreach (var column in info.Columns)
			{
				RateEntryGrid.ColumnStyles.Add(column);
			}
		}

		#endregion

		#region Category

		[Browsable(true)]
		public string Category
		{
			get { return fCategory; }
			set
			{
				if (Category != value)
				{
					fCategory = value;

					if (RateEntryLocationGridVisibility = CategoriesWithLocations.Contains(fCategory))
					{
						InitializeRateEntryLocationsGrid();
					}

					SetCategory(this);
				}
			}
		}

		string fCategory = "";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded header constant")]
		void SetCategory(Control control)
		{
			foreach (Control subControl in control.Controls)
			{
				var rateLinesAndItemsControl = subControl as BaseRateLinesAndItemsControl;
				if (rateLinesAndItemsControl != null)
				{
					rateLinesAndItemsControl.Category = Category;
					rateLinesAndItemsControl.BindTo = Category + "RateEntriesForBinding" + rateLinesAndItemsControl.BindTo.Substring(rateLinesAndItemsControl.BindTo.IndexOf('.'));

					return;
				}

				var gridControl = subControl as ZGrid;
				if (gridControl != null)
				{
					gridControl.BindTo = gridControl is MatchingLocationsGrid
						? Category + "RateEntriesForBinding.RateEntryLocations"
						: Category + "RateEntriesForBinding";

					if (gridControl.LayoutKey.StartsWith("Template", StringComparison.OrdinalIgnoreCase))
					{
						gridControl.LayoutKey = Category + gridControl.LayoutKey.Substring("Template".Length);
						gridControl.GridId = gridControl.LayoutKey;
					}

					return;
				}

				if (subControl is ZUserControl || subControl is ZPanel)
				{
					SetCategory(subControl);

					return;
				}

				var splitContainerControl = subControl as SplitContainer;
				if (splitContainerControl != null)
				{
					SetCategory(splitContainerControl.Panel1);
					SetCategory(splitContainerControl.Panel2);

					return;
				}

				var entryRelatedControlsContainerControl = subControl as CurrentEntryRelatedControlsContainer;
				if (entryRelatedControlsContainerControl != null)
				{
					SetCategory(entryRelatedControlsContainerControl.Panel);
				}

				var groupBoxControl = subControl as ZGroupBox;
				if (groupBoxControl != null)
				{
					SetCategory(groupBoxControl);
				}
			}
		}

		#endregion

		#region EntriesGrid_ColourDeciding

		void EntriesGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			RateEntry entry = e.ObjectAtRow as RateEntry;
			if (entry != null && !entry.IsDeleted)
			{
				if (entry.IsClientRate() && ClientRateForm != null && entry.IsAccepting && !entry.IsInDatabase)
				{
					e.Colour = Color.LightGreen;
				}
				else if (entry.IsExpired())
				{
					e.Colour = Color.PaleGoldenrod;
				}
				else if (entry.IsPublished)
				{
					e.Colour = Color.Lavender;
				}
			}
		}

		ActiveRatesForm ClientRateForm
		{
			get
			{
				if (fClientRateForm == null)
				{
					Form form = FindForm();
					fClientRateForm = form as ActiveRatesForm;
				}

				return fClientRateForm;
			}
		}

		ActiveRatesForm fClientRateForm;

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				RateEntryGrid.ColourDeciding -= EntriesGrid_ColourDeciding;
				RateEntryGrid.AfterBind -= RateEntryGridOnAfterBind;

				if (RateEntryGrid.ListManager != null)
				{
					RateEntryGrid.ListManager.CurrentChanged -= OnRateEntryChanged;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region RateEntryLocations Grid

		protected virtual IEnumerable<string> CategoriesWithLocations =>
			Enumerable.Empty<string>();

		protected bool RateEntryLocationGridVisibility
		{
			get => !splitContainerWithLocations.Panel2Collapsed;
			set => splitContainerWithLocations.Panel2Collapsed = !value;
		}

		void InitializeRateEntryLocationsGrid()
		{
			if (rateEntryLocationGrid != null)
			{
				return;
			}

			var matchingLocationsGroupBox = new ZGroupBox();
			matchingLocationsGroupBox.SuspendLayout();
			matchingLocationsGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("matchingLocationsGroupBox", "Matching Locations");
			matchingLocationsGroupBox.Dock = DockStyle.Fill;
			matchingLocationsGroupBox.Name = nameof(matchingLocationsGroupBox);
			matchingLocationsGroupBox.TabStop = false;

			rateEntryLocationGrid = new MatchingLocationsGrid();
			matchingLocationsGroupBox.Controls.Add(rateEntryLocationGrid);

			splitContainerWithLocations.SplitterDistance = splitContainerWithLocations.Width - ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			splitContainerWithLocations.Panel2.Controls.Add(matchingLocationsGroupBox);

			matchingLocationsGroupBox.ResumeLayout(false);
			matchingLocationsGroupBox.PerformLayout();
		}

		void InitializeSplitContainerWithLocations()
		{
			splitContainerWithLocations = new KSplitContainer();
			((ISupportInitialize)splitContainerWithLocations).BeginInit();
			splitContainerWithLocations.Panel1.SuspendLayout();
			splitContainerWithLocations.Panel2.SuspendLayout();
			splitContainerWithLocations.SuspendLayout();

			splitContainerWithLocations.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			splitContainerWithLocations.Name = "splitContainerMatchingLocations";
			splitContainerWithLocations.Dock = DockStyle.Fill;
			splitContainerWithLocations.FixedPanel = FixedPanel.Panel2;
			splitContainerWithLocations.TabIndex = 0;
			splitContainerWithLocations.Panel1MinSize = ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			splitContainerWithLocations.Panel1Collapsed = false;
			splitContainerWithLocations.Panel2MinSize = ControlDpiScalingHelper.ScaleToCurrentDpiY(130);
			splitContainerWithLocations.Panel2Collapsed = true;
			splitContainer.Panel1.Controls.Add(splitContainerWithLocations);

			((ISplitterLayoutSaveProvider)splitContainerWithLocations).IsLayoutRestored = false;

			splitContainerWithLocations.Panel1.ResumeLayout(false);
			splitContainerWithLocations.Panel2.ResumeLayout(false);
			((ISupportInitialize)splitContainerWithLocations).EndInit();
			splitContainerWithLocations.ResumeLayout(false);
			splitContainerWithLocations.PerformLayout();
		}

		KSplitContainer splitContainerWithLocations;
		MatchingLocationsGrid rateEntryLocationGrid;

		#endregion

		#region Component Designer generated code

		readonly Container components;

		KSplitContainer splitContainer;
		CurrentEntryRelatedControlsContainer currentEntryRelatedControlsContainer;

		protected Panel EntryGridPanel =>
			splitContainerWithLocations.Panel1;

		protected Panel CurrentEntryRelatedControlsPanel =>
			currentEntryRelatedControlsContainer.Panel;

		protected int CurrentEntryRelatedControlsPanelMininmum
		{
			get => splitContainer.Panel2MinSize;
			set => splitContainer.Panel2MinSize = value;
		}

		protected int SplitterDistance
		{
			get => ((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition;
			set => ((ISplitterLayoutSaveProvider)splitContainer).SplitterPosition = value;
		}

		void InitializeComponent()
		{
			this.splitContainer = new KSplitContainer();
			this.currentEntryRelatedControlsContainer = new CurrentEntryRelatedControlsContainer();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.currentEntryRelatedControlsContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.currentEntryRelatedControlsContainer);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.splitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(344);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(264);
			this.splitContainer.TabIndex = 0;
			// 
			// currentEntryRelatedControlsContainer
			// 
			this.currentEntryRelatedControlsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.currentEntryRelatedControlsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.currentEntryRelatedControlsContainer.Name = "currentEntryRelatedControlsContainer";
			this.currentEntryRelatedControlsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 344, true);
			this.currentEntryRelatedControlsContainer.TabIndex = 0;
			// 
			// RateEntryPanel
			// 
			this.Controls.Add(this.splitContainer);
			this.Name = "RateEntryPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer.Panel2.ResumeLayout(false);
			((ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			this.currentEntryRelatedControlsContainer.ResumeLayout(false);
			this.currentEntryRelatedControlsContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

#if DEBUG
		public void EntriesGrid_ColourDeciding_Exposed(ColourDecidingEventArgs e)
		{
			EntriesGrid_ColourDeciding(null, e);
		}

		public bool IsSecurityMessageLabelVisible
		{
			get { return currentEntryRelatedControlsContainer.SecurityMessageLabel.Visible; }
		}
#endif

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		class CurrentEntryRelatedControlsContainer : ContainerControl
		{
			public CurrentEntryRelatedControlsContainer()
			{
				InitializeComponent();
			}

			public ZLabel SecurityMessageLabel;
			public ZPanel Panel;

			protected override void OnLayout(LayoutEventArgs e)
			{
				AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
				AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
				base.OnLayout(e);
			}

			public void OnParentEntryChanged(RateEntry entry)
			{
				if (entry.IsInDatabase && entry.RateLinesAccessDenied)
				{
					SecurityMessageLabel.Visible = true;
					Panel.Visible = false;
				}
				else
				{
					SecurityMessageLabel.Visible = false;
					Panel.Visible = true;
				}
			}

			void InitializeComponent()
			{
				this.Panel = new ZPanel();
				this.SecurityMessageLabel = new ZLabel();
				this.SuspendLayout();

				this.Controls.Add(this.Panel);
				this.Panel.Name = "Panel";
				this.Panel.Visible = true;
				this.Panel.Dock = DockStyle.Fill;
				this.Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
				this.Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 446, true);

				this.Controls.Add(this.SecurityMessageLabel);
				this.SecurityMessageLabel.Name = "SecurityMessageLabel";
				this.SecurityMessageLabel.Visible = false;
				this.SecurityMessageLabel.Dock = DockStyle.Fill;
				this.SecurityMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
				this.SecurityMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 446, true);
				this.SecurityMessageLabel.TextAlign = ContentAlignment.MiddleCenter;

				this.Name = "CurrentEntryRelatedControlsContainer";
				this.ResumeLayout(false);
			}
		}
	}
}

