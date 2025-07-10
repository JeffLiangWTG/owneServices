namespace Enterprise.Freight.Agency.GUI
{
	using System.Diagnostics.CodeAnalysis;
	using CargoWise.Windows.UI;
	using Enterprise.Core.Forms;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.GUI;

	internal class PacksSplitGrid : SplitGrid
	{
		public PacksSplitGrid()
		{
			base.DataSourceType = typeof(AgencyShipmentPackLine);
		}

		protected override string CollectionName
		{
			get { return AgencyBooking.Schema.OuterPackLines; }
		}

		protected override string[] Columns
		{
			get
			{
				return new string[]
				{
					AgencyShipmentPackLine.Schema.JL_F3_NKPackType,
					AgencyShipmentPackLine.Schema.JL_PackageCount,
					AgencyShipmentPackLine.Schema.JL_JC,
					AgencyShipmentPackLine.Schema.JL_Description,
					AgencyShipmentPackLine.Schema.JL_Length,
					AgencyShipmentPackLine.Schema.JL_Width,
					AgencyShipmentPackLine.Schema.JL_Height,
					AgencyShipmentPackLine.Schema.JL_UnitOfDimension,
					AgencyShipmentPackLine.Schema.JL_ActualVolume,
					AgencyShipmentPackLine.Schema.JL_ActualVolumeUQ,
					AgencyShipmentPackLine.Schema.JL_ActualWeight,
					AgencyShipmentPackLine.Schema.JL_ActualWeightUQ
				};
			}
		}

		protected override string ItemsName
		{
			get { return Res.GetString("SplitGrid|e0f51308-d8b1-4591-84fa-5c16ce5b8d86", "Pack Lines"); }
		}

		protected override void PerformMove(object item, SplitBookingsHeader.MoveDirection direction)
		{
			Header.MovePackline((AgencyBookingPackLine)item, direction);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override ZGridColumnInfo GetColumnControl(string columnName)
		{
			ZGridColumnInfo control;

			switch (columnName)
			{
				case AgencyShipmentPackLine.Schema.JL_F3_NKPackType:
					control = new ZDropEditColumnStyleInfo();
					ControlDpiScalingHelper.SetWidth(control, 60, true);
					break;

				case AgencyShipmentPackLine.Schema.JL_PackageCount:
					control = new ZCalcEditColumnStyleInfo
					{
						Decimals = 0,
						BindToDecimalPlaces = null
					};
					ControlDpiScalingHelper.SetWidth(control, 40, true);
					break;

				case AgencyShipmentPackLine.Schema.JL_JC:
					control = new ZGuidDropEditColumnStyleInfo();
					break;

				case AgencyShipmentPackLine.Schema.JL_Description:
					control = new ZTextBoxColumnStyleInfo();
					break;

				case AgencyShipmentPackLine.Schema.JL_Length:
				case AgencyShipmentPackLine.Schema.JL_Width:
				case AgencyShipmentPackLine.Schema.JL_Height:
					control = new ZCalcEditColumnStyleInfo
					{
						Decimals = 0,
						BindToDecimalPlaces = null,
						GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("PacksSplitGrid|b9ccb4ee-90dc-4111-8fe4-60c033b548c2", "Dimension")
					};
					ControlDpiScalingHelper.SetWidth(control, 60, true);
					break;

				case AgencyShipmentPackLine.Schema.JL_UnitOfDimension:
					control = new ZTextBoxColumnStyleInfo
					{
						GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("PacksSplitGrid|b9ccb4ee-90dc-4111-8fe4-60c033b548c2", "Dimension")
					};
					ControlDpiScalingHelper.SetWidth(control, 40, true);
					break;

				case AgencyShipmentPackLine.Schema.JL_ActualVolume:
					control = new ZCalcEditColumnStyleInfo
					{
						Decimals = 0,
						BindToDecimalPlaces = null,
						GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("PacksSplitGrid|0b993f18-d0f2-4ce3-978e-8eda04772201", "Volume")
					};
					ControlDpiScalingHelper.SetWidth(control, 60, true);
					break;

				case AgencyShipmentPackLine.Schema.JL_ActualVolumeUQ:
					control = new ZTextBoxColumnStyleInfo
					{
						GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("PacksSplitGrid|0b993f18-d0f2-4ce3-978e-8eda04772201", "Volume")
					};
					ControlDpiScalingHelper.SetWidth(control, 30, true);
					break;

				case AgencyShipmentPackLine.Schema.JL_ActualWeight:
					control = new ZCalcEditColumnStyleInfo
					{
						Decimals = 0,
						BindToDecimalPlaces = null,
						GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("PacksSplitGrid|9f6e7bb2-f77f-4a84-8c59-991eab3708ed", "Weight")
					};
					ControlDpiScalingHelper.SetWidth(control, 60, true);
					break;

				case AgencyShipmentPackLine.Schema.JL_ActualWeightUQ:
					control = new ZTextBoxColumnStyleInfo
					{
						GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("PacksSplitGrid|9f6e7bb2-f77f-4a84-8c59-991eab3708ed", "Weight")
					};
					ControlDpiScalingHelper.SetWidth(control, 30, true);
					break;

				default:
					control = base.GetColumnControl(columnName);
					break;
			}

			return control;
		}
	}
}
