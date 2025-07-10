
namespace Enterprise.Workflow.GUI
{
	partial class EDIMessageContentFilterLineUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zTextBox2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dataGridView1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zTextBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.dataGridView1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.EDIMessageContentFilterSpec);
			//// 
			//// zTextBox2
			//// 
			this.zTextBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zTextBox2, "FilterType");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.EDIMessageContentFilterSpec)(null)).FilterType)));
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.ShouldResizeByMaxLength = true;
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zTextBox2.TabIndex = 2;
			//// 
			//// dataGridView1
			//// 
			this.dataGridView1.AllowNavigation = false;
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
					 | System.Windows.Forms.AnchorStyles.Left)
					 | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.dataGridView1, "Lines");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Workflow.Business.EDIMessageContentFilterSpec)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessageContentFilterLine)(((System.Collections.IList)(((Enterprise.Workflow.Business.EDIMessageContentFilterSpec)(null)).Lines)).SyncRoot)).SchemaElement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessageContentFilterLine)(((System.Collections.IList)(((Enterprise.Workflow.Business.EDIMessageContentFilterSpec)(null)).Lines)).SyncRoot)).DataContext)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessageContentFilterLine)(((System.Collections.IList)(((Enterprise.Workflow.Business.EDIMessageContentFilterSpec)(null)).Lines)).SyncRoot)).ElementType)));
			this.dataGridView1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "SchemaElement";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zDropEditColumnStyleInfo2.ColumnName = "DataContext";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo1.ColumnName = "ElementType";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.dataGridView1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.dataGridView1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.dataGridView1.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.dataGridView1.GridId = "e3d1eb67-fb24-4661-8be3-223cbd8804fe";
			this.dataGridView1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGridView1.LayoutKey = "dataGridView1";
			this.dataGridView1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
			this.dataGridView1.TabIndex = 3;
			// 
			// EDIMessageContentFilterLineUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.dataGridView1);
			this.Name = "EDIMessageContentFilterLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 325, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zTextBox2.ResumeLayout(true);
			this.zTextBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.dataGridView1.ResumeLayout(false);
			this.dataGridView1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private ZArchitecture.ZGrid dataGridView1;
		private ZArchitecture.GUI.ZDropEdit zTextBox2;
	}
}
