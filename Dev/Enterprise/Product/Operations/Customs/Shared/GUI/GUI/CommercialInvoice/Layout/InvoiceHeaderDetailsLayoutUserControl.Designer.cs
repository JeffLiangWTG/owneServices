namespace Enterprise.Customs.GUI.CommercialInvoice
{
	partial class InvoiceHeaderDetailsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InvoiceNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InvoiceAmountCalcFindBox = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.InvoiceCurrExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IncotermAndIncotermPlaceUserControl = new Enterprise.Customs.GUI.CommercialInvoice.IncotermAndIncotermPlaceUserControl();
			this.ValuationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceCurrLandedCostExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NoOfPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceDateEdit.SuspendLayout();
			this.InvoiceAmountCalcFindBox.SuspendLayout();
			this.IncotermAndIncotermPlaceUserControl.SuspendLayout();
			this.ValuationCodeDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.NoOfPacksCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceHeader);
			// 
			// InvoiceNumberBoundTextBox
			// 
			this.InvoiceNumberBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.InvoiceNumberBoundTextBox, "JZ_InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_InvoiceNumber)));
			this.InvoiceNumberBoundTextBox.CaptionResourceString = null;
			this.InvoiceNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceNumberBoundTextBox.Name = "InvoiceNumberBoundTextBox";
			this.InvoiceNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 25, true);
			this.InvoiceNumberBoundTextBox.TabIndex = 0;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "JZ_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_InvoiceDate)));
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 1;
			// 
			// InvoiceAmountCalcFindBox
			// 
			this.InvoiceAmountCalcFindBox.AllowDrop = true;
			this.InvoiceAmountCalcFindBox.BindToAmount = "JZ_InvoiceAmount";
			this.InvoiceAmountCalcFindBox.BindToList = "Lookups.CurrencyList";
			this.InvoiceAmountCalcFindBox.BindToUnit = "JZ_RX_NKInvoice_Currency";
			this.InvoiceAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.InvoiceAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceAmountCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.InvoiceAmountCalcFindBox.Name = "InvoiceAmountCalcFindBox";
			this.InvoiceAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.InvoiceAmountCalcFindBox.TabIndex = 2;
			// 
			// InvoiceCurrExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceCurrExRateCalcEdit, "JZ_InvoiceCurrExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_InvoiceCurrExRate)));
			this.InvoiceCurrExRateCalcEdit.CaptionResourceString = null;
			this.InvoiceCurrExRateCalcEdit.DecimalPlaces = 2;
			this.InvoiceCurrExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceCurrExRateCalcEdit.Name = "InvoiceCurrExRateCalcEdit";
			this.InvoiceCurrExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 25, true);
			this.InvoiceCurrExRateCalcEdit.TabIndex = 3;
			this.InvoiceCurrExRateCalcEdit.Text = "0.000000";
			this.InvoiceCurrExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IncotermAndIncotermPlaceUserControl
			// 
			this.IncotermAndIncotermPlaceUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncotermAndIncotermPlaceUserControl, ".");
			this.IncotermAndIncotermPlaceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncotermAndIncotermPlaceUserControl.Name = "IncotermAndIncotermPlaceUserControl";
			this.IncotermAndIncotermPlaceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 25, true);
			this.IncotermAndIncotermPlaceUserControl.TabIndex = 3;
			// 
			// ValuationCodeDropEdit
			// 
			this.ValuationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationCodeDropEdit, "JZ_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_ValuationCode)));
			this.ValuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValuationCodeDropEdit.Name = "ValuationCodeDropEdit";
			this.ValuationCodeDropEdit.PreBoundMaxLength = 2;
			this.ValuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 25, true);
			this.ValuationCodeDropEdit.TabIndex = 8;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).Lookups.JZ_WeightUQ_List)));
			this.GrossWeightCalcDropEdit.BindToAmount = "JZ_Weight";
			this.GrossWeightCalcDropEdit.BindToList = "Lookups.JZ_WeightUQ_List";
			this.GrossWeightCalcDropEdit.BindToUnit = "JZ_WeightUQ";
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 202, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 25, true);
			this.GrossWeightCalcDropEdit.TabIndex = 9;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).Lookups.JZ_WeightUQ_List)));
			this.NetWeightCalcDropEdit.BindToAmount = "JZ_NetWeight";
			this.NetWeightCalcDropEdit.BindToList = "Lookups.JZ_WeightUQ_List";
			this.NetWeightCalcDropEdit.BindToUnit = "JZ_NetWeightUQ";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 226, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 25, true);
			this.NetWeightCalcDropEdit.TabIndex = 10;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// InvoiceCurrLandedCostExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceCurrLandedCostExRateCalcEdit, "JZ_InvoiceCurrLandedCostExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_InvoiceCurrLandedCostExRate)));
			this.InvoiceCurrLandedCostExRateCalcEdit.CaptionResourceString = null;
			this.InvoiceCurrLandedCostExRateCalcEdit.DecimalPlaces = 2;
			this.InvoiceCurrLandedCostExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 250, true);
			this.InvoiceCurrLandedCostExRateCalcEdit.Name = "InvoiceCurrLandedCostExRateCalcEdit";
			this.InvoiceCurrLandedCostExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 25, true);
			this.InvoiceCurrLandedCostExRateCalcEdit.TabIndex = 11;
			this.InvoiceCurrLandedCostExRateCalcEdit.Text = "0.000000";
			this.InvoiceCurrLandedCostExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NoOfPacksCalcDropEdit
			// 
			this.NoOfPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_NoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).NoOfPacksPackType)));
			this.NoOfPacksCalcDropEdit.BindToAmount = "JZ_NoOfPacks";
			this.NoOfPacksCalcDropEdit.BindToUnit = "NoOfPacksPackType";
			this.NoOfPacksCalcDropEdit.Decimals = 3;
			this.NoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 274, true);
			this.NoOfPacksCalcDropEdit.Name = "NoOfPacksCalcDropEdit";
			this.NoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 25, true);
			this.NoOfPacksCalcDropEdit.TabIndex = 12;
			this.NoOfPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// InvoiceHeaderDetailsLayoutUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.Controls.Add(this.InvoiceNumberBoundTextBox);
			this.Controls.Add(this.InvoiceDateEdit);
			this.Controls.Add(this.InvoiceAmountCalcFindBox);
			this.Controls.Add(this.InvoiceCurrExRateCalcEdit);
			this.Controls.Add(this.IncotermAndIncotermPlaceUserControl);
			this.Controls.Add(this.ValuationCodeDropEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.InvoiceCurrLandedCostExRateCalcEdit);
			this.Controls.Add(this.NoOfPacksCalcDropEdit);
			this.Name = "InvoiceHeaderDetailsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 381, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.InvoiceAmountCalcFindBox.ResumeLayout(true);
			this.InvoiceAmountCalcFindBox.PerformLayout();
			this.IncotermAndIncotermPlaceUserControl.ResumeLayout(true);
			this.IncotermAndIncotermPlaceUserControl.PerformLayout();
			this.ValuationCodeDropEdit.ResumeLayout(true);
			this.ValuationCodeDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.NoOfPacksCalcDropEdit.ResumeLayout(true);
			this.NoOfPacksCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZTextBox InvoiceNumberBoundTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit InvoiceDateEdit;
		internal Customs.GUI.ConvertToLocalCurrencyControl InvoiceAmountCalcFindBox;
		internal Enterprise.ZArchitecture.ZCalcEdit InvoiceCurrExRateCalcEdit;
		internal Customs.GUI.CommercialInvoice.IncotermAndIncotermPlaceUserControl IncotermAndIncotermPlaceUserControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ValuationCodeDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		internal ZArchitecture.ZCalcEdit InvoiceCurrLandedCostExRateCalcEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit NoOfPacksCalcDropEdit;

		#endregion
	}
}
