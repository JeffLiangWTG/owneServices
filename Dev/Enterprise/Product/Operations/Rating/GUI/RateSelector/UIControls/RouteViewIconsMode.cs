using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class RouteViewIconsMode : ZUserControl
	{
		public RouteViewIconsMode()
		{
			InitializeComponent();

			BindingSource.DataSourceChanged += BindingSource_DataSourceChanged;
		}

		BookingEngineRateViewModel CurrentViewModel => (BookingEngineRateViewModel)BindingSource.Current;

		void BindingSource_DataSourceChanged(object sender, EventArgs e)
		{
			if (CurrentViewModel is null)
			{
				return;
			}

			var pics = CurrentViewModel.TransportLegs.Select(NewTransportIcon).ToArray();

			kTableLayoutPanel1.Controls.Clear();

			var fl = new KFlowLayoutPanel();
			fl.Name = "transportIconsFlowLayout";
			fl.FlowDirection = FlowDirection.LeftToRight;
			fl.Size = ControlDpiScalingHelper.NewScaledSize(0, kTableLayoutPanel1.Size.Height, isInStandardDpi: false);
			fl.AutoSize = true;
			fl.MaximumSize = ControlDpiScalingHelper.NewScaledSize(kTableLayoutPanel1.Size.Width, kTableLayoutPanel1.Size.Height, isInStandardDpi: false);
			fl.Margin = ControlDpiScalingHelper.NewScaledPadding(0);
			fl.Anchor = AnchorStyles.Top; // Center horizontally

			kTableLayoutPanel1.Controls.Add(fl);

			fl.Controls.AddRange(pics);
		}

		static ZPictureBox NewTransportIcon(TransportLegViewModel vm)
		{
			var pb = new ZPictureBox();
			pb.Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0);
			switch (vm.TransportMode)
			{
				case TransportMode.Sea:
					pb.Image = Properties.Resources.ShipDrawing;
					break;
				case TransportMode.Air:
					pb.Image = Properties.Resources.PlaneDrawing;
					break;
				default:
					// Empty space shown for trucking legs
					break;
			}

			pb.SizeMode = PictureBoxSizeMode.StretchImage;
			pb.Size = ControlDpiScalingHelper.NewScaledSize(25, 20);

			pb.SetTooltip(vm.VoyageNumber);
			return pb;
		}

#if DEBUG
		public IEnumerable<ZPictureBox> TransportIconsForTest => kTableLayoutPanel1.Controls.OfType<KFlowLayoutPanel>().Single().Controls.Cast<ZPictureBox>();
#endif
	}
}
