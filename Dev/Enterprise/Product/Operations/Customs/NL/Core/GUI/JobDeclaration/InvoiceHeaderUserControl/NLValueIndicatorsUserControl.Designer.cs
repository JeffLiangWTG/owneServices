
namespace Enterprise.Customs.NL.GUI
{
	partial class NLValueIndicatorsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PartyRelationShipCheckBox
			// 
			this.PartyRelationShipCheckBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("8CAF3916-B286-43C2-B878-C2604D9763CF", "Party relationship, whether there is price influence");
			this.PartyRelationShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.PartyRelationShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 38, true);
			// 
			// RestrictionsShipCheckBox
			// 
			this.RestrictionsShipCheckBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FD17E666-955A-4FE5-B027-475FD00669C9", "Restrictions as to the disposal or use of the goods by the buyer in accordance with Article 70(3)(a) of the Code");
			this.RestrictionsShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 46, true);
			this.RestrictionsShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 38, true);
			// 
			// SaleConditionsShipCheckBox
			// 
			this.SaleConditionsShipCheckBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("276A71BD-6B4A-44A3-9B38-0EA45674F84D", "Sale or price is subject to some condition or consideration in accordance with Article 70(3)(b) of the Code");
			this.SaleConditionsShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 86, true);
			this.SaleConditionsShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 38, true);
			// 
			// DisposalAccrualShipCheckBox
			// 
			this.DisposalAccrualShipCheckBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("024D9892-5C0B-4476-9E44-751C47604FF0", "The sale is subject to an arrangement under which part of the proceeds of any subsequent resale, disposal or use accrues directly or indirectly to the seller");
			this.DisposalAccrualShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 126, true);
			this.DisposalAccrualShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 38, true);
			// 
			// NLValueIndicatorsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "NLValueIndicatorsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
