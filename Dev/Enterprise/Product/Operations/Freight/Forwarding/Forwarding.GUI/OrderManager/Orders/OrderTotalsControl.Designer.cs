using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Freight.Forwarding.GUI
{
	partial class OrderTotalsControl : ZUserControl
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
			this.JD_Calc_TotalQuantityRemainingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_Calc_TotalQuantityReceivedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_Calc_TotalQuantityInvoicedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_Calc_TotalQuantityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_Calc_CountOuterPacksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_Calc_InnerPacksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_Calc_LineCountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.Order);
			// 
			// JD_Calc_TotalQuantityRemainingTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_Calc_TotalQuantityRemainingTextBox, "JD_Calc_TotalQuantityRemaining");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Calc_TotalQuantityRemaining)));
			this.JD_Calc_TotalQuantityRemainingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(776, 3, true);
			this.JD_Calc_TotalQuantityRemainingTextBox.Name = "JD_Calc_TotalQuantityRemainingTextBox";
			this.JD_Calc_TotalQuantityRemainingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.JD_Calc_TotalQuantityRemainingTextBox.TabIndex = 7;
			// 
			// JD_Calc_TotalQuantityReceivedTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_Calc_TotalQuantityReceivedTextBox, "JD_Calc_TotalQuantityReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Calc_TotalQuantityReceived)));
			this.JD_Calc_TotalQuantityReceivedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(654, 3, true);
			this.JD_Calc_TotalQuantityReceivedTextBox.Name = "JD_Calc_TotalQuantityReceivedTextBox";
			this.JD_Calc_TotalQuantityReceivedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.JD_Calc_TotalQuantityReceivedTextBox.TabIndex = 6;
			// 
			// JD_Calc_TotalQuantityInvoicedTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_Calc_TotalQuantityInvoicedTextBox, "JD_Calc_TotalQuantityInvoiced");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Calc_TotalQuantityInvoiced)));
			this.JD_Calc_TotalQuantityInvoicedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 3, true);
			this.JD_Calc_TotalQuantityInvoicedTextBox.Name = "JD_Calc_TotalQuantityInvoicedTextBox";
			this.JD_Calc_TotalQuantityInvoicedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.JD_Calc_TotalQuantityInvoicedTextBox.TabIndex = 5;
			// 
			// JD_Calc_TotalQuantityTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_Calc_TotalQuantityTextBox, "JD_Calc_TotalQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Calc_TotalQuantity)));
			this.JD_Calc_TotalQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 3, true);
			this.JD_Calc_TotalQuantityTextBox.Name = "JD_Calc_TotalQuantityTextBox";
			this.JD_Calc_TotalQuantityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.JD_Calc_TotalQuantityTextBox.TabIndex = 4;
			// 
			// JD_Calc_CountOuterPacksTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_Calc_CountOuterPacksTextBox, "JD_Calc_OuterPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Calc_OuterPacks)));
			this.JD_Calc_CountOuterPacksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 3, true);
			this.JD_Calc_CountOuterPacksTextBox.Name = "JD_Calc_CountOuterPacksTextBox";
			this.JD_Calc_CountOuterPacksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.JD_Calc_CountOuterPacksTextBox.TabIndex = 3;
			// 
			// JD_Calc_InnerPacksTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_Calc_InnerPacksTextBox, "JD_Calc_InnerPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Calc_InnerPacks)));
			this.JD_Calc_InnerPacksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 3, true);
			this.JD_Calc_InnerPacksTextBox.Name = "JD_Calc_InnerPacksTextBox";
			this.JD_Calc_InnerPacksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.JD_Calc_InnerPacksTextBox.TabIndex = 2;
			// 
			// JD_Calc_LineCountTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_Calc_LineCountTextBox, "JD_Calc_LineCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Calc_LineCount)));
			this.JD_Calc_LineCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 3, true);
			this.JD_Calc_LineCountTextBox.Name = "JD_Calc_LineCountTextBox";
			this.JD_Calc_LineCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.JD_Calc_LineCountTextBox.TabIndex = 1;
			// 
			// OrderTotalsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.JD_Calc_TotalQuantityRemainingTextBox);
			this.Controls.Add(this.JD_Calc_TotalQuantityReceivedTextBox);
			this.Controls.Add(this.JD_Calc_TotalQuantityInvoicedTextBox);
			this.Controls.Add(this.JD_Calc_LineCountTextBox);
			this.Controls.Add(this.JD_Calc_TotalQuantityTextBox);
			this.Controls.Add(this.JD_Calc_InnerPacksTextBox);
			this.Controls.Add(this.JD_Calc_CountOuterPacksTextBox);
			this.Name = "OrderTotalsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox JD_Calc_TotalQuantityRemainingTextBox;
		private ZArchitecture.ZTextBox JD_Calc_TotalQuantityReceivedTextBox;
		private ZArchitecture.ZTextBox JD_Calc_TotalQuantityInvoicedTextBox;
		private ZArchitecture.ZTextBox JD_Calc_TotalQuantityTextBox;
		private ZArchitecture.ZTextBox JD_Calc_CountOuterPacksTextBox;
		private ZArchitecture.ZTextBox JD_Calc_InnerPacksTextBox;
		private ZArchitecture.ZTextBox JD_Calc_LineCountTextBox;
	}
}
