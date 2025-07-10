namespace Enterprise.eManifest.GUI
{
	partial class ELoadListFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("3eb17e68-7c2d-4359-ae57-77811205778a", "ID", "eLoadList ID", "eLoadList ID", "");
			zTextBoxColumnStyleInfo1.ColumnName = "DO_UniqueReference";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("1d53c639-fd64-40d8-8591-736f4e775e6a", "Transport", "Transport", "Transport Mode", "");
			zDropEditColumnStyleInfo1.ColumnName = "DO_TransportMode";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("28728eab-c5af-4be0-ae5c-606ad148f261", "Bill", "Bill Number", "Master Bill Number", "");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "DO_MasterBillNumber";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("a2af81e3-f2ea-46b4-ae90-19b5a0932835", "House", "House Number", "Master House Number", "");
			zTextBoxColumnStyleInfo3.ColumnName = "DO_MasterHouseNumber";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("2c183dec-0f8b-4252-8941-5bdbf5a84d18", "Vessel", "Vessel", "Vessel", "");
			zTextBoxColumnStyleInfo4.ColumnName = "DO_RV_NKVessel";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("0359e99d-51a2-4a77-ad1e-d6565415ce12", "Voyage", "Voyage", "Voyage Flight", "");
			zTextBoxColumnStyleInfo5.ColumnName = "DO_VoyageFlight";
			zDateEditColumnStyleInfo13.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("5a25177f-e0e5-4243-8175-4cd7d7ca9b0f", "ETA", "ETA", "Estimated Time of Arrival", "");
			zDateEditColumnStyleInfo13.ColumnName = "DO_E_ARV";
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("ba7d9cf4-8bfd-4ac1-97b3-56777067ca6f", "ETD", "ETD", "Estimated Time of Departure", "");
			zDateEditColumnStyleInfo3.ColumnName = "DO_E_DEP";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("5924edc6-1530-4d19-bba3-5e74a7a823bc", "Container", "Container", "Container Number", "");
			zTextBoxColumnStyleInfo7.ColumnName = "DO_ContainerNumber";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("db8713ed-3c1d-422a-a62c-e9b624dc24c3", "Status", "Status", "Status", "");
			zTextBoxColumnStyleInfo10.ColumnName = "DO_Status";
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.eManifest.GUI.Res.GetData("38f7ecc7-1ab2-415d-96d2-501b1dfcbb00", "Status", "Status", "Status", "");
			zTextBoxColumnStyleInfo11.ColumnName = "OriginDepot+Header+OH_Code";
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("5616698e-7015-4431-a749-faa8b3642ab9", "Origin", "Origin", "Origin Depot", "");
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.eManifest.GUI.Res.GetData("746b6a6d-0211-41ed-8ea4-184dc6e052ff", "Destination", "Destination", "Destination Depot", "");
			zTextBoxColumnStyleInfo12.ColumnName = "DestinationDepot+Header+OH_Code";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			// 
			// ELoadListFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ELoadListFilterControl";
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
