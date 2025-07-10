using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class ReferenceUserControl
	{
		#region Windows Forms Designer Generated
		protected ZGroupBox RefsGroupBox;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZGrid refsGrid;
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RefsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			refsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(refsGrid)).BeginInit();
			this.RefsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// RefsGrid
			// 
			refsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(refsGrid, "Invoices.InvoiceHeaderRefs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).InvoiceHeaderRefs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.JobComInvoiceHeaderRefs)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).InvoiceHeaderRefs)).SyncRoot)).J2_ReferenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.JobComInvoiceHeaderRefs)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).InvoiceHeaderRefs)).SyncRoot)).Lookups.ReferenceTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.JobComInvoiceHeaderRefs)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).InvoiceHeaderRefs)).SyncRoot)).J2_ReferenceNumber)));
			refsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ReferenceTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ReferenceUserControl|2B896378-8E88-4998-853A-FBA1E17AE2BA", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "J2_ReferenceType";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ReferenceUserControl|0640C1A6-17E0-4525-A4D1-562C5F99041D", "Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "J2_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			refsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			refsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			refsGrid.GridId = "9f825c23-99ae-4543-81f8-c2b50637d131";
			refsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			refsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			refsGrid.LayoutKey = "RefsGrid";
			refsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			refsGrid.Name = "RefsGrid";
			refsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 217, true);
			refsGrid.TabIndex = 0;
			// 
			// RefsGroupBox
			// 
			this.RefsGroupBox.Controls.Add(refsGrid);
			this.RefsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RefsGroupBox.Name = "RefsGroupBox";
			this.RefsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 236, true);
			this.RefsGroupBox.TabIndex = 4;
			this.RefsGroupBox.TabStop = false;
			this.RefsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("59AA4235-91F6-4842-8EBC-5A9D6E59E19C", "Reference");
			// 
			// ReferenceUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RefsGroupBox);
			this.Name = "ReferenceUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(refsGrid)).EndInit();
			this.RefsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}
