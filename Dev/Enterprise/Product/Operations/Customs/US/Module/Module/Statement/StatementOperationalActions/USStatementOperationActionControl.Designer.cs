namespace Enterprise.Customs.US.Module
{
	partial class USStatementOperationActionControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MoveDetailContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MoveDetailContainersGrid)).BeginInit();
			this.MoveDetailContainersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Module.StatementActionMethodApplicator);
			// 
			// MoveDetailContainersGrid
			// 
			this.MoveDetailContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MoveDetailContainersGrid, "StatementPaymentActions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Module.StatementActionMethodApplicator)(null)).StatementPaymentActions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementPaymentAction)(((System.Collections.IList)(((Enterprise.Customs.US.Module.StatementActionMethodApplicator)(null)).StatementPaymentActions)).SyncRoot)).StatementNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementPaymentAction)(((System.Collections.IList)(((Enterprise.Customs.US.Module.StatementActionMethodApplicator)(null)).StatementPaymentActions)).SyncRoot)).PaymentParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.StatementPaymentAction)(((System.Collections.IList)(((Enterprise.Customs.US.Module.StatementActionMethodApplicator)(null)).StatementPaymentActions)).SyncRoot)).TotalAmountPayable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementPaymentAction)(((System.Collections.IList)(((Enterprise.Customs.US.Module.StatementActionMethodApplicator)(null)).StatementPaymentActions)).SyncRoot)).ACHPaymentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementPaymentAction)(((System.Collections.IList)(((Enterprise.Customs.US.Module.StatementActionMethodApplicator)(null)).StatementPaymentActions)).SyncRoot)).PayerUnitNo)));
			this.MoveDetailContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("a2607fc1-040f-4241-8a7e-1d752e61207a", "Statement Number ");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "StatementNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("7ad2a4f8-2b3f-45cc-8ddb-559536216aa2", "Payment Party");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "PaymentParty";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("3ac5106c-c1a2-47a6-9cd6-6c76a63bf6b7", "Total Amount Payable");
			zCalcEditColumnStyleInfo1.ColumnName = "TotalAmountPayable";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("75aae96d-16a3-4e7f-bb6e-3c7c6ff72bf7", "ACH Debit or Credit");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ACHPaymentType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("2a856252-c1d8-4863-be73-b08951d9158c", "Payer Unit No.");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "PayerUnitNo";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.MoveDetailContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MoveDetailContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MoveDetailContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MoveDetailContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MoveDetailContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MoveDetailContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoveDetailContainersGrid.GridId = "ba46386c-25bc-4bc7-bd7f-717a11082a8b";
			this.MoveDetailContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MoveDetailContainersGrid.LayoutKey = "BillAdditionalReferencesGrid";
			this.MoveDetailContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MoveDetailContainersGrid.Name = "MoveDetailContainersGrid";
			this.MoveDetailContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 379, true);
			this.MoveDetailContainersGrid.TabIndex = 1;
			// 
			// USStatementOperationActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MoveDetailContainersGrid);
			this.Name = "USStatementOperationActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 379, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MoveDetailContainersGrid)).EndInit();
			this.MoveDetailContainersGrid.ResumeLayout(false);
			this.MoveDetailContainersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid MoveDetailContainersGrid;
	}
}
