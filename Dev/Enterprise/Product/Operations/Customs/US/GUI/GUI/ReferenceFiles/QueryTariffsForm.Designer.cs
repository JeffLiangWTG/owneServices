
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	partial class QueryTariffsForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffQueryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TariffGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TariffQueryGrid)).BeginInit();
			this.TariffGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 278, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.QueryTariffOption);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 249, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 249, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 1;
			this.SendButton.Text = "Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// TariffQueryGrid
			// 
			this.TariffQueryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TariffQueryGrid, "TariffsToQuery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryTariffOption)(null)).TariffsToQuery)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QueryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryTariffOption)(null)).TariffsToQuery)).SyncRoot)).FromTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryTariffOption)(null)).TariffsToQuery)).SyncRoot)).Tariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QueryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryTariffOption)(null)).TariffsToQuery)).SyncRoot)).ToTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryTariffOption)(null)).TariffsToQuery)).SyncRoot)).Tariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.QueryTariff)(((System.Collections.IList)(((Enterprise.Customs.US.Business.QueryTariffOption)(null)).TariffsToQuery)).SyncRoot)).AsOfDate)));
			this.TariffQueryGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Tariffs";
			zCodeFindBoxColumnStyleInfo1.Caption = "From Tariff";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "FromTariff";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Tariffs";
			zCodeFindBoxColumnStyleInfo2.Caption = "To Tariff";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ToTariff";
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo1.Caption = "As of Date";
			zDateEditColumnStyleInfo1.ColumnName = "AsOfDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.TariffQueryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TariffQueryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TariffQueryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TariffQueryGrid.GridId = "3b8f4408-b6b2-48cf-a992-0d60e20d08bc";
			this.TariffQueryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TariffQueryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TariffQueryGrid.LayoutKey = "TariffQueryGrid";
			this.TariffQueryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TariffQueryGrid.Name = "TariffQueryGrid";
			this.TariffQueryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 223, true);
			this.TariffQueryGrid.TabIndex = 0;
			// 
			// TariffGroupBox
			// 
			this.TariffGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.TariffGroupBox.Controls.Add(this.TariffQueryGrid);
			this.TariffGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.TariffGroupBox.Name = "TariffGroupBox";
			this.TariffGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 242, true);
			this.TariffGroupBox.TabIndex = 4;
			this.TariffGroupBox.TabStop = false;
			this.TariffGroupBox.Text = "Tariff or Tariff Range";
			// 
			// QueryTariffsForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 302, true);
			this.Controls.Add(this.TariffGroupBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SendButton);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.QueryTariffOption);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 336, true);
			this.Name = "QueryTariffsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.TariffGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TariffQueryGrid)).EndInit();
			this.TariffGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private new ZButton CancelButton;
		public ZButton SendButton;
		private ZGrid TariffQueryGrid;
		private ZGroupBox TariffGroupBox;

	}
}
