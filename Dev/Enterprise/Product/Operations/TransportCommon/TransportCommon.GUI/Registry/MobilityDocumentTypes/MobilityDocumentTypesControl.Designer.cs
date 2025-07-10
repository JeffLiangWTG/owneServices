namespace Enterprise.TransportCommon.GUI.Registry
{
	partial class MobilityDocumentTypesControl
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
            this.DocTypesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DocTypesGrid)).BeginInit();
            this.DocTypesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes.MobilityDocumentTypeCollection);
            // 
            // DocTypesGrid
            // 
            this.DocTypesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.DocTypesGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes.MobilityDocumentType)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes.MobilityDocumentType)(null)).RT_PK)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes.MobilityDocumentType)(null)).DocTypeDescription)));
            this.DocTypesGrid.CaptionVisible = false;
            zGuidFindBoxColumnStyleInfo1.ColumnName = "RT_PK";
            zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
            zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefDocType;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("MobilityDocumentTypesControl|42cae981-3ec2-41af-a5ac-12285798cc25", "Description");
            zTextBoxColumnStyleInfo1.ColumnName = "DocTypeDescription";
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            this.DocTypesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.DocTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.DocTypesGrid.GridId = "98ea7a9d-458a-487b-a6f9-bff6a21fb350";
            this.DocTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.DocTypesGrid.LayoutKey = "DocTypesGrid";
            this.DocTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DocTypesGrid.Name = "DocTypesGrid";
            this.DocTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 229, true);
            this.DocTypesGrid.TabIndex = 0;
            this.DocTypesGrid.Navigate += new System.Windows.Forms.NavigateEventHandler(this.DocTypesGrid_Navigate);
            // 
            // MobilityDocumentTypesControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.DocTypesGrid);
            this.Name = "MobilityDocumentTypesControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 229, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DocTypesGrid)).EndInit();
            this.DocTypesGrid.ResumeLayout(false);
            this.DocTypesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid DocTypesGrid;

		public bool DocTypesGridReadOnly { get => DocTypesGrid.ReadOnly; }

	}
}
