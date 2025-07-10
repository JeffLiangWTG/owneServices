namespace Enterprise.ContractManagement.Module.Common
{
	public partial class AllocationContainerWeightLimitWithTypeFilterControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent(ZArchitecture.GUI.ZBindingSource bindingSource)
		{
			FromCalcEdit = new ZArchitecture.ZCalcEdit();
			ToCalcEdit = new ZArchitecture.ZCalcEdit();
			TypeEdit = new ZArchitecture.GUI.ZDropEdit();
			UnitControl = new ZArchitecture.GUI.ZDropEdit();
			//
			// From number value
			//
			FromCalcEdit.CaptionResourceString = Res.GetData("RatingContractAllocationLineFilterControl|b757e174-7bc4-451e-aebb-79ebaff97c6b", "From");
			FromCalcEdit.Decimals = 0;
			FromCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0);
			FromCalcEdit.BindTo = "Property1";
			FromCalcEdit.TabIndex = 1;
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(
				ref FromCalcEdit,
				 ZArchitecture.GUI.Internal.ZFilterStripDateEdit.DefaultWidth - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(ZArchitecture.GUI.Internal.ZFilterStripDateEdit.CalendarButtonWidthZ),
				false
			);
			bindingSource.SetBindingMember(FromCalcEdit, FromCalcEdit.BindTo);
			//
			// To number value
			//
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(ref ToCalcEdit, ZArchitecture.GUI.Internal.ZFilterStripDateEdit.DefaultWidth, isOnStandardDpi: false);
			ToCalcEdit.CaptionResourceString = Res.GetData("RatingContractAllocationLineFilterControl|4bd82b53-799b-470d-9173-424fc79d297a", "To");
			ToCalcEdit.Decimals = 0;
			ToCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 0);
			ToCalcEdit.BindTo = "Property2";
			ToCalcEdit.TabIndex = 2;
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(
				ref ToCalcEdit,
				ToCalcEdit.Width - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(ZArchitecture.GUI.ZDateEdit.CalendarButtonWidthZ),
				false
			);
			bindingSource.SetBindingMember(ToCalcEdit, ToCalcEdit.BindTo);
			//
			// Unit dropdown
			//
			UnitControl.TabIndex = 3;
			UnitControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 0);
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(UnitControl.CodeBox, 18, isOnStandardDpi: true);
			UnitControl.ShowDescriptionBox = false;
			UnitControl.BindTo = "Property";
			UnitControl.BindToList = "List";
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(ref UnitControl, 18, isOnStandardDpi: true);
			bindingSource.SetBindingMember(UnitControl, UnitControl.BindTo);
			//
			// Type dropdown
			//
			TypeEdit.MaxLength = 3;
			TypeEdit.ShowDescriptionBox = true;
			TypeEdit.DescriptionBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(54);
			TypeEdit.DescriptionBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, TypeEdit.DescriptionBox.Height);
			TypeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 0);
			TypeEdit.BindTo = "LimitType";
			TypeEdit.TabIndex = 4;
			bindingSource.SetBindingMember(TypeEdit, TypeEdit.BindTo);
		}

		private ZArchitecture.ZCalcEdit FromCalcEdit;
		private ZArchitecture.ZCalcEdit ToCalcEdit;
		private ZArchitecture.GUI.ZDropEdit TypeEdit;
		private ZArchitecture.GUI.ZDropEdit UnitControl;
	}
}
