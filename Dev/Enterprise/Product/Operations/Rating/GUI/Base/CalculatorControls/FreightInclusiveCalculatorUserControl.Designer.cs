using CargoWiseOne.ResourceStrings;

namespace Enterprise.Rating.GUI
{
	public partial class FreightInclusiveCalculatorUserControl
	{
		System.ComponentModel.Container components = null;

		void InitializeComponent()
		{
			this.descriptionLabel = new ZArchitecture.ZLabel();
			this.FreightInclusiveCalculatorTypeDropDown = new ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.PreCarriageOnCarriageChargeFindBox = new ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FreightInclusiveCalculatorTypeDropDown.SuspendLayout();
			this.PreCarriageOnCarriageChargeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// descriptionLabel
			// 
			this.descriptionLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif;
			this.descriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.descriptionLabel.Name = "descriptionLabel";
			this.descriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 46, true);
			this.descriptionLabel.TabIndex = 0;
			this.descriptionLabel.Text = Res.GetString("a625f540-f239-426d-9ed2-e9764707f5ea", "A separate Job Charge will not be created when using the Freight Inclusive Calculator. Instead, the Charge Code Description is added to the Calculation Description of the Freight Charge Code.");
			// 
			// FreightInclusiveCalculatorTypeDropDown
			// 
			this.FreightInclusiveCalculatorTypeDropDown.AllowDrop = true;
			this.FreightInclusiveCalculatorTypeDropDown.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("FreightInclusiveCalculatorUserControl|2fc26b10-e0e3-45b3-91d1-74554ea79e8f", "Type");
			this.FreightInclusiveCalculatorTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 48, true);
			this.FreightInclusiveCalculatorTypeDropDown.Name = "FreightInclusiveCalculatorTypeDropDown";
			this.FreightInclusiveCalculatorTypeDropDown.PreBoundMaxLength = 3;
			this.FreightInclusiveCalculatorTypeDropDown.ShouldResizeByMaxLength = true;
			this.FreightInclusiveCalculatorTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
			this.FreightInclusiveCalculatorTypeDropDown.TabIndex = 1;
			// 
			// OtherFreightChargeFindBox
			// 
			this.PreCarriageOnCarriageChargeFindBox.AllowDrop = true;
			this.PreCarriageOnCarriageChargeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("FreightInclusiveCalculatorUserControl|43e33485-3879-4e9d-9bcf-d4723d80efc6", "Pre-Carriage / On-Carriage Freight");
			this.PreCarriageOnCarriageChargeFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.PreCarriageOnCarriageChargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 75, true);
			this.PreCarriageOnCarriageChargeFindBox.Name = "PreCarriageOnCarriageChargeFindBox";
			this.PreCarriageOnCarriageChargeFindBox.ShowDescriptionBox = true;
			this.PreCarriageOnCarriageChargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.PreCarriageOnCarriageChargeFindBox.TabIndex = 2;
			// 
			// FreightInclusiveCalculatorUserControl
			// 
			this.Controls.Add(this.PreCarriageOnCarriageChargeFindBox);
			this.Controls.Add(this.FreightInclusiveCalculatorTypeDropDown);
			this.Controls.Add(this.descriptionLabel);
			this.Name = "FreightInclusiveCalculatorUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FreightInclusiveCalculatorTypeDropDown.ResumeLayout(true);
			this.FreightInclusiveCalculatorTypeDropDown.PerformLayout();
			this.PreCarriageOnCarriageChargeFindBox.ResumeLayout(true);
			this.PreCarriageOnCarriageChargeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
