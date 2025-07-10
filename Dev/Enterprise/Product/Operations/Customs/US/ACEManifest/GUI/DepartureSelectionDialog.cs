using System;
using Enterprise.Customs.US.ACEManifest.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public partial class DepartureSelectionDialog : FlightSelectionDialog
	{
		[Obsolete("This constructor is just for the designer")]
		protected DepartureSelectionDialog()
			: base()
		{
		}

		public DepartureSelectionDialog(AdditionalMessageInformation additionalMessageInformation)
				: base(additionalMessageInformation, "Lift Off Time")
		{
			additionalMessageInformation.InitialiseFlightDepartureTime();
			SetupTimeZoneLabel();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void SetupTimeZoneLabel()
		{
			var loadPort = additionalMessageInformation.Header.AMA_RL_NKPortOfLoading;
			if (!loadPort.IsEmpty)
			{
				TimeZoneLabel.Text = ResString.GetMultilingualString("80E6313B-B87D-48E6-B268-E58C3D919593", "in the Time Zone of the load port {0}", loadPort);
			}
		}
	}
}
