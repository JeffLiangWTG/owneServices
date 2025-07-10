namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class BrokerAccountsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BankAccountGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BankAccountGrid)).BeginInit();
			this.BankAccountGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.BrokersAccountCollection);
			// 
			// BankAccountGrid
			// 
			this.BankAccountGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BankAccountGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.ManagedAccount)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.ManagedAccount)(null)).PayerUnitNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.DataRegistry.Business.ManagedAccount)(null)).BankAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.ManagedAccount)(null)).BankAccountList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.ManagedAccount)(null)).ClientBranchDesignation)));
			this.BankAccountGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0eab9acc-8b09-47fd-9fdd-8a5c9b0dee10", "Payer\'s Unit No");
			zTextBoxColumnStyleInfo1.ColumnName = "PayerUnitNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "BankAccountList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eaed4bc1-fa02-4f0c-9fd8-c8c328e8ce49", "Bank Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BankAccount";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8351668a-9730-46ca-a8d8-4c953b19a3e0", "Branch Design.", "Client Branch Designation", "");
			zTextBoxColumnStyleInfo2.ColumnName = "ClientBranchDesignation";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.BankAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BankAccountGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BankAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BankAccountGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BankAccountGrid.GridId = "00969ce2-4c73-47ff-b1da-09682bd79afb";
			this.BankAccountGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BankAccountGrid.LayoutKey = "BankAccountBasedOnCurrencyGrid";
			this.BankAccountGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BankAccountGrid.Name = "BankAccountGrid";
			this.BankAccountGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 239, true);
			this.BankAccountGrid.TabIndex = 1;
			// 
			// BrokerAccountsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BankAccountGrid);
			this.Name = "BrokerAccountsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 239, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BankAccountGrid)).EndInit();
			this.BankAccountGrid.ResumeLayout(false);
			this.BankAccountGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid BankAccountGrid;
	}
}
