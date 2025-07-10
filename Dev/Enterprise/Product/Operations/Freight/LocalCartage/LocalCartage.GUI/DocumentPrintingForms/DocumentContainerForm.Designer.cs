using CargoWise.Types;
namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class DocumentContainerForm
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

		Enterprise.ZArchitecture.ZLabel SelectContainersLabel;
		Enterprise.ZArchitecture.ZGrid zGrid1;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		System.ComponentModel.Container components = null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SelectContainersLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 317, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.DocumentContainerOptions);
			// 
			// SelectContainersLabel
			// 
			this.SelectContainersLabel.AutoSize = true;
			this.SelectContainersLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentContainerForm|ffe89f15-964c-4524-a72d-4c013949caf1", "Select containers to print");
			this.SelectContainersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.SelectContainersLabel.Name = "SelectContainersLabel";
			this.SelectContainersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 13, true);
			this.SelectContainersLabel.TabIndex = 1;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentContainerOptions)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentContainer)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentContainerOptions)(null)).Containers)).SyncRoot)).Container.JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentContainer)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentContainerOptions)(null)).Containers)).SyncRoot)).Container.JC_SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentContainer)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentContainerOptions)(null)).Containers)).SyncRoot)).Container.JC_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.DocumentContainer)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentContainerOptions)(null)).Containers)).SyncRoot)).PrintContainer)));
			this.zGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentContainerForm|bb2f6753-84ce-4a06-bfd1-fa53efa16ae5", "Container #");
			zTextBoxColumnStyleInfo1.ColumnName = "Container+JC_ContainerNum";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentContainerForm|567fbf57-25d8-4e2b-94c5-12b4e07a64d6", "Seal #");
			zTextBoxColumnStyleInfo2.ColumnName = "Container+JC_SealNum";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentContainerForm|01be172c-540e-4932-a9d7-e00099e650c3", "Mode");
			zTextBoxColumnStyleInfo3.ColumnName = "Container+JC_ContainerMode";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentContainerForm|1d57a122-f4aa-498b-b00f-531d2b173ec3", "Print");
			zCheckBoxColumnStyleInfo1.ColumnName = "PrintContainer";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zGrid1.GridId = "400ec2ef-6e2e-4644-88d4-283245575ed0";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 30, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 256, true);
			this.zGrid1.TabIndex = 2;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentContainerForm|92adfad6-ab27-4da3-a1be-314470e9302a", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 290, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 4;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentContainerForm|145a8d05-759e-47fd-8c10-3fac30372acc", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 290, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 3;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// DocumentContainerForm
			// 
			this.AcceptButton = this.PrintButton;
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 339, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentContainerForm|3b8870bd-00de-42bf-a59f-369b56b1b2ca", "Select Containers to Print");
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.SelectContainersLabel);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.DocumentContainerOptions);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentContainerOptions";
			this.Name = "DocumentContainerForm";
			this.Controls.SetChildIndex(this.SelectContainersLabel, 0);
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
