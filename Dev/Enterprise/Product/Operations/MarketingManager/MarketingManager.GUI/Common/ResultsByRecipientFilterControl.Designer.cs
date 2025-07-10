namespace Enterprise.MarketingManager.GUI
{
	partial class ResultsByRecipientFilterControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid, "CampaignsItemsSentForDisplayOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignsItemsSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignsItemsSent)).SyncRoot)).ContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignsItemsSent)).SyncRoot)).EmailAddress)));
			this.grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsByRecipientUserControl|bb72bba8-f5b3-4cc4-8c37-4b6d423fb0b1", "Contact Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ContactName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsByRecipientUserControl|815cf761-6a02-44d9-84b0-640dfbec9489", "Contact Email");
			zTextBoxColumnStyleInfo2.ColumnName = "EmailAddress";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.CopySelectedRowsAllowed = true;
			this.grid.GridId = "52c71830-6441-44fd-84f9-ca5aa345dd70";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "zGrid1";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid.Name = "grid";
			this.grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.grid.ShouldSetErrorsOnTabPage = false;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 240, true);
			this.grid.TabIndex = 0;
			this.grid.IsWholeRowSelectedOnClick = true;
			this.grid.ReadOnly = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// ResultsByRecipientFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "ResultsByRecipientFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
