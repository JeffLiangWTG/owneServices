namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	partial class MAFeBACCaInvoiceLineUserControl
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
			this.GoodsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NewGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MeasurementValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MeasurementUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EffectiveMAF_MeasurementUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EffectiveMAF_MeasurementValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.JobDeclaration);
			// 
			// GoodsTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "FilteredInvoiceLines.JI_MAF_GoodsType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MAF_GoodsType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).EffectiveMAF_GoodsTypeDescription)));
			this.GoodsTypeDropEdit.BindToForDescription = "FilteredInvoiceLines.EffectiveMAF_GoodsTypeDescription";
			this.GoodsTypeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaInvoiceLineUserControl|7b549a4e-8132-4753-8fa7-4d9af43b0995", "Goods Type", "MPI Goods Type.");
			this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 15, true);
			this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
			this.GoodsTypeDropEdit.PreBoundMaxLength = 3;
			this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.GoodsTypeDropEdit.TabIndex = 0;
			// 
			// NewGoodsDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NewGoodsDropEdit, "FilteredInvoiceLines.JI_MAF_NewGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MAF_NewGoods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).EffectiveMAF_NewGoodsDescription)));
			this.NewGoodsDropEdit.BindToForDescription = "FilteredInvoiceLines.EffectiveMAF_NewGoodsDescription";
			this.NewGoodsDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaInvoiceLineUserControl|5bfd68db-32a1-47bb-b1c4-d5b3e092bfe1", "Is New Goods", "Is New Goods for MPI Quarantine purposes.");
			this.NewGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 41, true);
			this.NewGoodsDropEdit.Name = "NewGoodsDropEdit";
			this.NewGoodsDropEdit.PreBoundMaxLength = 1;
			this.NewGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.NewGoodsDropEdit.TabIndex = 1;
			// 
			// MeasurementValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MeasurementValueCalcEdit, "FilteredInvoiceLines.JI_MAF_MeasurementValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MAF_MeasurementValue)));
			this.MeasurementValueCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaInvoiceLineUserControl|39e15e5f-916e-4a95-8eb3-77b8aa933495", "Measurement", "Measurement of packages.");
			this.MeasurementValueCalcEdit.DecimalPlaces = 0;
			this.MeasurementValueCalcEdit.Decimals = 0;
			this.MeasurementValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 67, true);
			this.MeasurementValueCalcEdit.Name = "MeasurementValueCalcEdit";
			this.MeasurementValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.MeasurementValueCalcEdit.TabIndex = 2;
			this.MeasurementValueCalcEdit.Text = "0";
			this.MeasurementValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MeasurementUQDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MeasurementUQDropEdit, "FilteredInvoiceLines.JI_MAF_MeasurementUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MAF_MeasurementUQ)));
			this.MeasurementUQDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaInvoiceLineUserControl|4c334487-a6c4-42af-bc6b-1308939b6fc8", "", "Measurement of packages.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MeasurementUQDropEdit, false);
			this.MeasurementUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 67, true);
			this.MeasurementUQDropEdit.Name = "MeasurementUQDropEdit";
			this.MeasurementUQDropEdit.PreBoundMaxLength = 3;
			this.MeasurementUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.MeasurementUQDropEdit.TabIndex = 3;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.EffectiveMAF_MeasurementUQDropEdit);
			this.DetailsGroupBox.Controls.Add(this.EffectiveMAF_MeasurementValueCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.NewGoodsDropEdit);
			this.DetailsGroupBox.Controls.Add(this.MeasurementUQDropEdit);
			this.DetailsGroupBox.Controls.Add(this.GoodsTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.MeasurementValueCalcEdit);
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("13ED41C7-7D37-458C-9C0E-71212B8D12C0", "Details");
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 118, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// EffectiveMAF_MeasurementUQDropEdit
			// 
			this.BindingSource.SetBindingMember(this.EffectiveMAF_MeasurementUQDropEdit, "FilteredInvoiceLines.EffectiveMAF_MeasurementUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).EffectiveMAF_MeasurementUQ)));
			this.EffectiveMAF_MeasurementUQDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaInvoiceLineUserControl|56157b99-ed8f-41a3-9a35-1951add99a64", "", "The Measurement that is sent to MPI - Is defaulted from the Invoice Line unless you enter a value.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EffectiveMAF_MeasurementUQDropEdit, false);
			this.EffectiveMAF_MeasurementUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 93, true);
			this.EffectiveMAF_MeasurementUQDropEdit.Name = "EffectiveMAF_MeasurementUQDropEdit";
			this.EffectiveMAF_MeasurementUQDropEdit.PreBoundMaxLength = 3;
			this.EffectiveMAF_MeasurementUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.EffectiveMAF_MeasurementUQDropEdit.TabIndex = 5;
			// 
			// EffectiveMAF_MeasurementValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EffectiveMAF_MeasurementValueCalcEdit, "FilteredInvoiceLines.EffectiveMAF_MeasurementValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).EffectiveMAF_MeasurementValue)));
			this.EffectiveMAF_MeasurementValueCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaInvoiceLineUserControl|2efac997-3991-4207-89dc-d72be2cfc1b7", "Measurement Sent", "The Measurement that is sent to MPI - Is defaulted from the Invoice Line unless you enter a value.");
			this.EffectiveMAF_MeasurementValueCalcEdit.DecimalPlaces = 0;
			this.EffectiveMAF_MeasurementValueCalcEdit.Decimals = 0;
			this.EffectiveMAF_MeasurementValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 93, true);
			this.EffectiveMAF_MeasurementValueCalcEdit.Name = "EffectiveMAF_MeasurementValueCalcEdit";
			this.EffectiveMAF_MeasurementValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.EffectiveMAF_MeasurementValueCalcEdit.TabIndex = 4;
			this.EffectiveMAF_MeasurementValueCalcEdit.Text = "0";
			this.EffectiveMAF_MeasurementValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MAFeBACCaInvoiceLineUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "MAFeBACCaInvoiceLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 375, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit GoodsTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit NewGoodsDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit MeasurementValueCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit MeasurementUQDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit EffectiveMAF_MeasurementUQDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit EffectiveMAF_MeasurementValueCalcEdit;

	}
}
