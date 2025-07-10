namespace Enterprise.Freight.GUI
{
	partial class CommunitySystemCodesOfForwarderAndAgentControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CommunitySystemCodesOfForwarderAndAgentGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CommunitySystemCodesOfForwarderAndAgentGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommunitySystemCodesOfForwarderAndAgentCollection);
			// 
			// CommunitySystemCodesOfForwarderAndAgentGrid
			// 
			this.CommunitySystemCodesOfForwarderAndAgentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommunitySystemCodesOfForwarderAndAgentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.CommunitySystemCodesOfForwarderAndAgent)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommunitySystemCodesOfForwarderAndAgent)(null)).Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommunitySystemCodesOfForwarderAndAgent)(null)).PCS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommunitySystemCodesOfForwarderAndAgent)(null)).ForwarderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommunitySystemCodesOfForwarderAndAgent)(null)).AgentCode)));
			this.CommunitySystemCodesOfForwarderAndAgentGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("9862D15D-C109-4155-9250-3AED0EBCEADC", "Port");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Port";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("85D27108-5E04-453A-80D5-9BB842CDB871", "PCS");
			zTextBoxColumnStyleInfo1.ColumnName = "PCS";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("6F9C2FF8-068F-4061-9DC0-FCB5944A2AEA", "Forwarder Code");
			zTextBoxColumnStyleInfo2.ColumnName = "ForwarderCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("10C813D6-EFCA-41E7-BBE6-B38FE527520D", "Agent Code");
			zTextBoxColumnStyleInfo3.ColumnName = "AgentCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CommunitySystemCodesOfForwarderAndAgentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CommunitySystemCodesOfForwarderAndAgentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CommunitySystemCodesOfForwarderAndAgentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CommunitySystemCodesOfForwarderAndAgentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CommunitySystemCodesOfForwarderAndAgentGrid.CopySelectedRowsAllowed = true;
			this.CommunitySystemCodesOfForwarderAndAgentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommunitySystemCodesOfForwarderAndAgentGrid.GridId = "65C89B72-346B-42F3-AFFE-36B2C01BBAE6";
			this.CommunitySystemCodesOfForwarderAndAgentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommunitySystemCodesOfForwarderAndAgentGrid.LayoutKey = "CommunitySystemCodesOfForwarderAndAgentGrid";
			this.CommunitySystemCodesOfForwarderAndAgentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommunitySystemCodesOfForwarderAndAgentGrid.Name = "CommunitySystemCodesOfForwarderAndAgentGrid";
			this.CommunitySystemCodesOfForwarderAndAgentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 210, true);
			this.CommunitySystemCodesOfForwarderAndAgentGrid.TabIndex = 0;
			// 
			// CommunitySystemCodesOfForwarderAndAgentControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommunitySystemCodesOfForwarderAndAgentGrid);
			this.Name = "CommunitySystemCodesOfForwarderAndAgentControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 210, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CommunitySystemCodesOfForwarderAndAgentGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid CommunitySystemCodesOfForwarderAndAgentGrid;
	}
}
