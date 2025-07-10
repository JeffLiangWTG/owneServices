namespace Enterprise.Freight.GUI
{
	public partial class DocumentOptionsForm
	{

		#region Windows Form Designer generated code

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OptionsGroupBox;
		System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 344, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.DocumentShipment);
			//
			// CancelPrintButton
			//
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentOptionsForm|00289bb2-5134-4989-9f12-d410daf00831", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 312, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelPrintButton.TabIndex = 2;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			//
			// PrintButton
			//
			this.PrintButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentOptionsForm|accf2299-d2c9-4f08-be0d-1f34a17fdad0", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 312, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PrintButton.TabIndex = 1;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			//
			// OptionsGroupBox
			//
			this.OptionsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentOptionsForm|d656c756-9c29-43dd-ae9e-9cb121d35f55", "Options");
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 296, true);
			this.OptionsGroupBox.TabIndex = 0;
			this.OptionsGroupBox.TabStop = false;
			//
			// DocumentOptionsForm
			//
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 368, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentOptionsForm|9bead88e-c48d-4ed5-b2dd-878b870013f9", "Document Options");
			this.Controls.Add(this.OptionsGroupBox);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.PrintButton);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.DocumentShipment);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentShipment";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "DocumentOptionsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
