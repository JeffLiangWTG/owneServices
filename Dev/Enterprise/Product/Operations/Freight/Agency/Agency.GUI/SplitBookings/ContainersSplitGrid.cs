
namespace Enterprise.Freight.Agency.GUI
{
	using CargoWise.Windows.UI;
	using Enterprise.Core.Forms;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.GUI;

	internal abstract class ContainersSplitGrid : SplitGrid
	{
		protected ContainersSplitGrid()
		{
			base.DataSourceType = typeof(AgencyBookingContainer);
		}

		protected override ZGridColumnInfo GetColumnControl(string columnName)
		{
			ZGridColumnInfo control;

			switch (columnName)
			{
				case AgencyBookingContainer.Schema.JC_ContainerNum:
					control = new ZTextBoxColumnStyleInfo();
					break;

				case AgencyBookingContainer.Schema.JC_MarksAndNumbers:
					control = new ZTextBoxColumnStyleInfo();
					break;

				case AgencyBookingContainer.Schema.JC_RC:
					control = new ZGuidFindBoxColumnStyleInfo();
					break;

				case AgencyBookingContainer.Schema.JC_ContainerCount:
					control = new ZCalcEditColumnStyleInfo
					{
						Decimals = 0,
						BindToDecimalPlaces = null
					};
					break;

				case AgencyBookingContainer.Schema.JC_Calc_NetWeight:
				case AgencyBookingContainer.Schema.JC_TareWeight:
				case AgencyBookingContainer.Schema.JC_GrossWeight:
					control = new ZCalcEditColumnStyleInfo
					{
						Decimals = 0,
						BindToDecimalPlaces = null,
						GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("ActualContainersSplitGrid|2b8e3108-6150-42ea-a23b-c23bcc92d784", "Weight")
					};
					ControlDpiScalingHelper.SetWidth(control, 60, true);
					break;

				default:
					control = base.GetColumnControl(columnName);
					break;
			}

			return control;
		}
	}
}
