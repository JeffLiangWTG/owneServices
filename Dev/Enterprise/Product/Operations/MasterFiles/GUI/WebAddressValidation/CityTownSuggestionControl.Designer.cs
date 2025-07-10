using System.Windows.Forms;
namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	partial class CityTownSuggestionControl
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
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CityTownListView = new ListView();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressSuggestionControl|E07946E2-EB25-410C-86A9-18CE39F022D5", "Suggested Cities");
			this.HeaderLabel.IsFontBold = true;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 22, true);
			this.HeaderLabel.TabIndex = 6;
			// 
			// CityTownListView
			// 
			this.CityTownListView.View = View.List;
			this.CityTownListView.Columns.Add("", this.CityTownListView.ClientSize.Width);
			this.CityTownListView.HeaderStyle = ColumnHeaderStyle.None;
			this.CityTownListView.HideSelection = false;
			this.CityTownListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.CityTownListView.Name = "CityTownListView";
			this.CityTownListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 124, true);
			this.CityTownListView.TabIndex = 1;
			// 
			// CityTownSuggestionControl
			// 
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.CityTownListView);
			this.Controls.Add(this.HeaderLabel);
			this.Name = "CityTownSuggestionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 150, true);
			//this.Leave += new System.EventHandler(this.CityTownSuggestionControl_Leave);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ListView CityTownListView;
		public ZArchitecture.ZLabel HeaderLabel;
	}
}
