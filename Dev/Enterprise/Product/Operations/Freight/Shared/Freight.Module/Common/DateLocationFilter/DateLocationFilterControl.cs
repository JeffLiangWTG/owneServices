using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Module
{
	public partial class DateLocationFilterControl : ZDateRangeControl
	{
		public DateLocationFilterControl()
		{
			InitializeComponent();
		}

		protected void InitializeControl()
		{
			ControlDpiScalingHelper.SetTop(ref locationFindBox, ParentStrip.FilterControlTop + ControlDpiScalingHelper.ScaleToCurrentDpiY(26), false);
		}

		public DateLocationFilterControl(ZFilterStrip parentStrip)
			: base(parentStrip)
		{
			InitializeComponent();
			InitializeControl();
		}

		protected new DateLocationFilter Filter
		{
			get { return DataSource as DateLocationFilter; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (Filter != null)
			{
				UpdateLayout();
			}
		}

		void UpdateLayout()
		{
			if (Filter.LocationType == DateLocationFilter.LocationTypes.Load)
			{
				this.locationFindBox.CaptionResourceString = Res.GetData("21edb403-6ab2-4e66-ad60-602ec757be09", "Load Port");
			}

			if (Filter.LocationType == DateLocationFilter.LocationTypes.Discharge)
			{
				this.locationFindBox.CaptionResourceString = Res.GetData("b367e787-458e-409f-a825-0a29c888b87f", "Discharge Port");
			}
		}
	}
}
