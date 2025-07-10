
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	partial class OrgTransactionsFilterControl : ZDateRangeControl
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
			this.NumberOfTransactionsTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgTransactionsModuleFilter);
			// 
			// NumberOfTransactionsTextBox
			// 
			this.BindingSource.SetBindingMember(this.NumberOfTransactionsTextBox, "NumberOfTransactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Module.OrgTransactionsModuleFilter)(null)).NumberOfTransactions)));
			this.NumberOfTransactionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 1, true);
			this.NumberOfTransactionsTextBox.Name = "NumberOfTransactionsTextBox";
			this.NumberOfTransactionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.NumberOfTransactionsTextBox.TabIndex = 0;
			this.NumberOfTransactionsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OrgTransactionsFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NumberOfTransactionsTextBox);
			this.Name = "OrgTransactionsFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			this.Controls.SetChildIndex(this.NumberOfTransactionsTextBox, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.PropertySearchDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
		private ZCalcEdit NumberOfTransactionsTextBox;

        #endregion
    }
}
