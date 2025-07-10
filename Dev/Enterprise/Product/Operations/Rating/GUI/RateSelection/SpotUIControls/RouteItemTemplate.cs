using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Views;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using TransportMode = Enterprise.Rating.GUI.RateSelector.Models.TransportMode;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class RouteItemTemplate : TemplateBasedControl, IItemTemplateControl
	{
		public RouteItemTemplate()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				AutoSize = true;
				AutoSizeMode = AutoSizeMode.GrowAndShrink;
			}
			lblTitle.Text = ResStrings.Deadlines;
			lblCode.Text = ResStrings.Code;
			lblName.Text = ResStrings.Name;
			lblType.Text = ResStrings.Type;
			lblDate.Text = ResStrings.Date;

#if DEBUG
			TypeDescriptor.AddAttributes(lblCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDate, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(estimatedArrivalLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(estimatedDepartureLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblFlagCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblPortOfDischargeCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(portOfDischargeNameLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblPortOfLoadingCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(portOfLoadingNameLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTitle, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTransitTime, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblType, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblVoyageNumber, new SuppressFormsLocalizedTestAttribute());

			// Suppresses errors for "Missing resource string caption for control"
			var suppressTextBasher = new SuppressControlRequiresTextBasherAttribute();
			TypeDescriptor.AddAttributes(lblFlagCode, suppressTextBasher);
			TypeDescriptor.AddAttributes(portOfLoadingNameLabel, suppressTextBasher);
			TypeDescriptor.AddAttributes(portOfDischargeNameLabel, suppressTextBasher);
			TypeDescriptor.AddAttributes(estimatedArrivalLabel, suppressTextBasher);
			TypeDescriptor.AddAttributes(estimatedDepartureLabel, suppressTextBasher);
			TypeDescriptor.AddAttributes(lblTransitTime, suppressTextBasher);
#endif
		}

		protected override Panel GetContainerPanel()
		{
			return pnlDeadlineContainer;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new DeadlineItemTemplate();
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return Data?.DateInfos;
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (Data == null)
			{
				return;
			}

			cpnlDeadlines.Visible = Data.ShowDateInfos;
			cpnlDeadlines.IsCollapsed = true;
			pbAirplane.Visible = Data.TransportMode == TransportMode.Air;
			pbShip.Visible = Data.TransportMode == TransportMode.Sea;
			pbAirplane.SetTooltip(Data.VesselName);
			pbShip.SetTooltip(Data.VesselName);

			lblTransitTime.Text = Data.TransitTime?.ToString((NoResString)"dd\\d\\ hh\\h") ?? string.Empty;

			portOfLoadingNameLabel.Text = Data.PortOfLoadingName;
			estimatedDepartureLabel.Text = Data.EstimatedDeparture.ToString("yyyy-MMM-dd HH:mm");
			portOfDischargeNameLabel.Text = Data.PortOfDischargeName;
			estimatedArrivalLabel.Text = Data.EstimatedArrival.ToString("yyyy-MMM-dd HH:mm");

			if (string.IsNullOrWhiteSpace(portOfLoadingNameLabel.Text))
			{
				// move up ETD
				portOfLoadingNameLabel.Text = estimatedDepartureLabel.Text;
				estimatedDepartureLabel.Text = null;
			}

			if (string.IsNullOrWhiteSpace(portOfDischargeNameLabel.Text))
			{
				// move up ETA
				portOfDischargeNameLabel.Text = estimatedArrivalLabel.Text;
				estimatedArrivalLabel.Text = null;
			}
		}

		#region IItemTemplateControl
		public void DataBind(object data)
		{
			BindingSource.DataSource = data;
		}

		public bool IsSelected { get; set; }
		public EventHandler SelectionChanged { get; set; }
		public void ClearSelection()
		{
		}
		#endregion

		#region Tooltips
		void lblTransitTime_MouseHover(object sender, EventArgs e)
		{
			lblTransitTime.SetTooltip(ResStrings.TransitTime);
		}
		#endregion

		TransportLegViewModel Data => CurrentDataItem as TransportLegViewModel;
	}
}
