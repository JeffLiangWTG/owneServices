using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	partial class UXMLMatchedResultUserControl
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

			if (disposing)
			{
				detailsLinkLabel?.Dispose();
				detailsLinkLabel = null;
				layoutZpanel?.Dispose();
				layoutZpanel = null;

				if (components != null)
				{
					components.Dispose();
				}
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
			this.components = new System.ComponentModel.Container();
			this.layoutZpanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.detailsLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();

			//components.Add(layoutZpanel);
			//components.Add(detailsLinkLabel);

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.layoutZpanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			// this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.GUI.UXMLMatchingDiagnosticModel);
			// 
			// layoutZpanel
			// 
			this.layoutZpanel.ColumnCount = 2;
			this.layoutZpanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.layoutZpanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.layoutZpanel.RowCount = 0;
			this.layoutZpanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.layoutZpanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutZpanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.layoutZpanel.Name = "layoutZpanel";
			this.layoutZpanel.AutoSize = true;
			this.layoutZpanel.TabIndex = 0;
			// 
			// detailsLinkLabel
			// 
			this.detailsLinkLabel.AutoSize = true;
			this.detailsLinkLabel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("b66391a9-ece1-4b1b-b92c-85561ea94cf0", "Details +");
			this.detailsLinkLabel.IsFontBold = false;
			this.detailsLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 9);
			this.detailsLinkLabel.LinkColor = System.Drawing.Color.Blue;
			this.detailsLinkLabel.Name = "detailsLinkLabel";
			this.detailsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 16, true);
			this.detailsLinkLabel.TabIndex = 4;
			this.detailsLinkLabel.TabStop = false;
			this.detailsLinkLabel.UseCompatibleTextRendering = true;
			this.detailsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.detailsLinkLabel_LinkClicked);
			// 
			// UXMLMatchedResultUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.layoutZpanel);
			this.Name = "UXMLMatchedResultUserControl";
			this.AutoSize = true;
			// this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 115, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		}

		#endregion

		private KTableLayoutPanel layoutZpanel;
		private ZLinkLabel detailsLinkLabel;
	}
}
