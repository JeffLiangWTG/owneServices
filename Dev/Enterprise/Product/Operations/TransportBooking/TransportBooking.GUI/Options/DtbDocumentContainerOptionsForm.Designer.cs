namespace Enterprise.TransportBookings.GUI.Options
{
	partial class DtbDocumentContainerOptionsForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            this.DeliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
            this.SelectContainersLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 225, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions);
            // 
            // DeliverButton
            // 
            this.DeliverButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DeliverButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("3e7d6462-0b1f-443c-af9e-c357f98d388f", "Deliver");
            this.DeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 198, true);
            this.DeliverButton.Name = "DeliverButton";
            this.DeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
            this.DeliverButton.TabIndex = 7;
            this.DeliverButton.Click += new System.EventHandler(this.DeliverButton_Click);
            // 
            // CancelPrintButton
            // 
            this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelPrintButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("604450dc-1203-443d-8f88-ce88b61975c0", "Cancel");
            this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 198, true);
            this.CancelPrintButton.Name = "CancelPrintButton";
            this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
            this.CancelPrintButton.TabIndex = 8;
            this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
            // 
            // zGrid1
            // 
            this.zGrid1.AllowNavigation = false;
            this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.zGrid1, "Containers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions)(null)).Containers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.Options.DtbDocumentContainerOption)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions)(null)).Containers)).SyncRoot)).ContainerNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.Options.DtbDocumentContainerOption)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions)(null)).Containers)).SyncRoot)).ContainerType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.Options.DtbDocumentContainerOption)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions)(null)).Containers)).SyncRoot)).Seal)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.Options.DtbDocumentContainerOption)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions)(null)).Containers)).SyncRoot)).DeliverContainer)));
            this.zGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.zGrid1.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "ContainerNumber";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo2.ColumnName = "ContainerType";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zTextBoxColumnStyleInfo3.ColumnName = "Seal";
            zCheckBoxColumnStyleInfo1.ColumnName = "DeliverContainer";
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.zGrid1.CopySelectedRowsAllowed = true;
            this.zGrid1.GridId = "400ec2ef-6e2e-4644-88d4-283245575ed0";
            this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGrid1.LayoutKey = "zGrid1";
            this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 23, true);
            this.zGrid1.Name = "zGrid1";
            this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 169, true);
            this.zGrid1.TabIndex = 6;
            // 
            // SelectContainersLabel
            // 
            this.SelectContainersLabel.AutoSize = true;
            this.SelectContainersLabel.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("e89cf6bf-4d68-4d3d-9293-2599fca4f479", "Select Containers to Deliver");
            this.SelectContainersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 7, true);
            this.SelectContainersLabel.Name = "SelectContainersLabel";
            this.SelectContainersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 13, true);
            this.SelectContainersLabel.TabIndex = 5;
            // 
            // DtbDocumentContainerOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("6816dba9-9ff3-419c-ae5c-a876ca5dd29e", "Deliver Containers?");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 249, true);
            this.Controls.Add(this.DeliverButton);
            this.Controls.Add(this.CancelPrintButton);
            this.Controls.Add(this.zGrid1);
            this.Controls.Add(this.SelectContainersLabel);
            this.DataSourceType = typeof(Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions);
            this.Name = "DtbDocumentContainerOptionsForm";
            this.Text = "DtbDocumentContainerOptionsForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.SelectContainersLabel, 0);
            this.Controls.SetChildIndex(this.zGrid1, 0);
            this.Controls.SetChildIndex(this.CancelPrintButton, 0);
            this.Controls.SetChildIndex(this.DeliverButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#endregion
		private ZArchitecture.GUI.ZButton DeliverButton;
		private ZArchitecture.GUI.ZButton CancelPrintButton;
		private ZArchitecture.ZGrid zGrid1;
		private ZArchitecture.ZLabel SelectContainersLabel;
	}
}