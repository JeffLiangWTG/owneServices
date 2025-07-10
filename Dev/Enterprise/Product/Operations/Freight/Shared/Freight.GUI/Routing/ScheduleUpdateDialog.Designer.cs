using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.GUI
{
	partial class ScheduleUpdateDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleUpdateDialog));
			this.textLabel = new Enterprise.ZArchitecture.ZLabel();
			this.keepScheduleDateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.updateScheduleDateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.createNewScheduleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// textLabel
			// 
			this.textLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.textLabel.Name = "textLabel";
			this.textLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 78, true);
			this.textLabel.TabIndex = 0;			
			this.textLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// keepScheduleDateButton
			// 
			this.keepScheduleDateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.keepScheduleDateButton.Name = "keepScheduleDateButton";
			this.keepScheduleDateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.keepScheduleDateButton.TabIndex = 1;
			this.keepScheduleDateButton.CaptionResourceString = Res.GetData("ScheduleUpdateDialog|KeepScheduleData", "Keep Schedule Date");
			this.keepScheduleDateButton.UseVisualStyleBackColor = true;
			this.keepScheduleDateButton.Click += new System.EventHandler(this.keepScheduleDateButton_Click);
			// 
			// updateScheduleDateButton
			// 
			this.updateScheduleDateButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.updateScheduleDateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 0, true);
			this.updateScheduleDateButton.Name = "updateScheduleDateButton";
			this.updateScheduleDateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.updateScheduleDateButton.TabIndex = 2;
			this.updateScheduleDateButton.CaptionResourceString = Res.GetData("ScheduleUpdateDialog|UpdateScheduleDate", "Update Schedule Date");
			this.updateScheduleDateButton.UseVisualStyleBackColor = true;
			this.updateScheduleDateButton.Click += new System.EventHandler(this.updateScheduleDateButton_Click);
			// 
			// createNewScheduleButton
			// 
			this.createNewScheduleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.createNewScheduleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 0, true);
			this.createNewScheduleButton.Name = "createNewScheduleButton";
			this.createNewScheduleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.createNewScheduleButton.TabIndex = 3;
			this.createNewScheduleButton.CaptionResourceString = Res.GetData("ScheduleUpdateDialog|CreateNewSchedule", "Create New Schedule");
			this.createNewScheduleButton.UseVisualStyleBackColor = true;
			this.createNewScheduleButton.Click += new System.EventHandler(this.createNewScheduleButton_Click);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.keepScheduleDateButton);
			this.bottomPanel.Controls.Add(this.createNewScheduleButton);
			this.bottomPanel.Controls.Add(this.updateScheduleDateButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 78, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 28, true);
			this.bottomPanel.TabIndex = 4;
			// 
			// ScheduleUpdateDialog
			// 
			this.AcceptButton = this.keepScheduleDateButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 106, true);
			this.Controls.Add(this.textLabel);
			this.Controls.Add(this.bottomPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ScheduleUpdateDialog";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel textLabel;
		private Enterprise.ZArchitecture.GUI.ZButton keepScheduleDateButton;
		private Enterprise.ZArchitecture.GUI.ZButton updateScheduleDateButton;
		private Enterprise.ZArchitecture.GUI.ZButton createNewScheduleButton;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
	}
}