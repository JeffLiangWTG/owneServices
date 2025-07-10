using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefExchangeRateFilterControl : ZFilterStripControl
	{
		public RefExchangeRateFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetupClientColumn();
		}

		void SetupClientColumn()
		{
			if (GridCollection.Factory.IsLocalClientExchangeRatefieldNeeded())
			{
				grid.ColumnStyles.Add(
					new GUI.ZOrganisationFindBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("c35aa51c-0918-44bb-924c-b7abafaadf34", "Local Client"),
						ColumnName = RefExchangeRate.Schema.RE_OH_Client,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					}
				);
			}
		}
	}
}
