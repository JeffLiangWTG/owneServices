using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TrackingStatusChartRowUserControl : ZUserControl
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracking status")]
		public TrackingStatusChartRowUserControl(TrackingStatusChartData data)
		{
			InitializeComponent();

			Color backColor = Color.Black;

			labelItemText.Text = data.MultilingualTrackingStatus?.ToString();
			labelEmailsCount.Text = data.EmailsCount.ToString("#,#0", CultureInfo.CurrentCulture);
			labelClientsCount.Text = data.ClientsCount.ToString("#,#0", CultureInfo.CurrentCulture);

			switch (data.Status)
			{
				case TrackingStatusCodes.Codes.NDR:
					backColor = Color.Orange;
					break;

				case TrackingStatusCodes.Codes.VER:
					backColor = Color.LightSkyBlue;
					break;

				case TrackingStatusCodes.Codes.UNV:
					backColor = Color.Silver;
					break;

				case TrackingStatusCodes.Codes.QUE:
					backColor = Color.YellowGreen;
					break;

				case TrackingStatusCodes.Codes.OPC:
					backColor = Color.LimeGreen;
					break;

				case TrackingStatusCodes.Codes.OPQ:
					backColor = Color.IndianRed;
					break;

				case "NEW":
					backColor = Color.WhiteSmoke;
					break;

				case "UNS":
					backColor = Color.DarkSalmon;
					break;

				case "Non Existent":
					backColor = Color.Black;
					break;
			}

			labelRectangle.BackColor = backColor;
		}
	}
}
