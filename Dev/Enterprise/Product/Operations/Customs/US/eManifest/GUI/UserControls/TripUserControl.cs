namespace Enterprise.Customs.US.eManifest.GUI
{
	using System;
	using Enterprise.Customs.US.eManifest.Business;
	using Enterprise.ZArchitecture.GUI;

	public partial class TripUserControl : ZUserControl
	{
		public TripUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ChangeUNLOCOPortsComponentVisibility();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			var trip = (Trip)CurrentDataItem;
			if (trip != null)
			{
				trip.BH_RL_NKPortUnladingInfo.ValueChanged -= ChangeUNLOCOPortsComponentType;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var trip = (Trip)CurrentDataItem;
			if (trip != null)
			{
				trip.BH_RL_NKPortUnladingInfo.ValueChanged += ChangeUNLOCOPortsComponentType;
			}
		}

		void ChangeUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeUNLOCOPortsComponentVisibility();
		}

		void ChangeUNLOCOPortsComponentVisibility()
		{
			var trip = (Trip)CurrentDataItem;
			if (trip != null && !trip.IsDeleted)
			{
				FirstExpectedPortOfArrivalDDropEdit.Visible = trip.PortUnladingDCodeIsDropEdit;
				FirstExpectedPortOfArrivalDCodeFindBox.Visible = !trip.PortUnladingDCodeIsDropEdit;
			}
		}
	}
}
