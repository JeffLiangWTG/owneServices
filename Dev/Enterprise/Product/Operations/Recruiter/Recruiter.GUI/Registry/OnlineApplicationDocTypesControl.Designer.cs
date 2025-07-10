namespace Enterprise.Recruiter.GUI
{
	partial class OnlineApplicationDocTypesControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.DocTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DocTypesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.OnlineApplicationDocTypeCollection);
			// 
			// DocTypesGrid
			// 
			this.DocTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocTypesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.OnlineApplicationDocType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.OnlineApplicationDocType)(null)).RT_PK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.OnlineApplicationDocType)(null)).DocTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Business.OnlineApplicationDocType)(null)).IsCompulsory)));
			this.DocTypesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RT_PK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefDocType;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("OnlineApplicationDocTypesControl|c2a48725-9914-4ec8-aafc-546e54289c35", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "DocTypeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsCompulsory";
			this.DocTypesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.DocTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DocTypesGrid.GridId = "086b6cc5-4194-44c6-8800-5764545105ac";
			this.DocTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocTypesGrid.LayoutKey = "DocTypesGrid";
			this.DocTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocTypesGrid.Name = "DocTypesGrid";
			this.DocTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 229, true);
			this.DocTypesGrid.TabIndex = 0;
			// 
			// OnlineApplicationDocTypesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DocTypesGrid);
			this.Name = "OnlineApplicationDocTypesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 229, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DocTypesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid DocTypesGrid;
	}
}
