using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class SupplyTypeConfigurationByChargeGroupControl : ChargeGroupSettingControl
	{
		public SupplyTypeConfigurationByChargeGroupControl()
		{
			AddNewColumns();
		}

		void AddNewColumns()
		{
			var incoterm = new ZDropEditColumnStyleInfo();
			incoterm.CharacterCasing = CharacterCasing.Upper;
			incoterm.CaptionResourceString = Res.GetData("SupplyTypeConfigurationByChargeGroupControl|9C05EAFC-0BEE-404F-A63F-4B5BC026B09D", "Incoterm");
			incoterm.ColumnName = "Incoterm";
			ControlDpiScalingHelper.SetWidth(ref incoterm, 50, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(incoterm);

			var lineDepartmentPK = new ZGuidFindBoxColumnStyleInfo();
			lineDepartmentPK.CaptionResourceString = Res.GetData("SupplyTypeConfigurationByChargeGroupControl|E3D22710-B27A-46BE-833E-5C94A58140B1", "Line Department");
			lineDepartmentPK.ColumnName = "LineDepartmentPK";
			ControlDpiScalingHelper.SetWidth(ref lineDepartmentPK, 50, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(lineDepartmentPK);

			var supplyType = new ZDropEditColumnStyleInfo();
			supplyType.CharacterCasing = CharacterCasing.Upper;
			supplyType.CaptionResourceString = Res.GetData("SupplyTypeConfigurationByChargeGroupControl|72FF8D6D-79FB-4240-9479-D97D813C7AB6", "Supply Type");
			supplyType.ColumnName = "SupplyType";
			ControlDpiScalingHelper.SetWidth(ref supplyType, 130, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(supplyType);
		}
	}
}
