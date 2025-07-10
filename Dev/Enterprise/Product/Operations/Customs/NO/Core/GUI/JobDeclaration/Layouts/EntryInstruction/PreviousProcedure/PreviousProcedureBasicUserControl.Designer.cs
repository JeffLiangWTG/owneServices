using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

partial class PreviousProcedureBasicUserControl
{
	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.PreviousProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.ImportFromTemporaryStorageRegisterButton = new Enterprise.ZArchitecture.GUI.ZButton();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.PreviousProcedureDropEdit.SuspendLayout();
		this.ImportFromTemporaryStorageRegisterButton.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.PreviousDocumentMaster);
		// 
		// PreviousProcedureDropEdit
		// 
		this.PreviousProcedureDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.PreviousProcedureDropEdit, "CSI_Procedure");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.PreviousDocumentMaster)(null)).CSI_Procedure)));
		this.PreviousProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 13, true);
		this.PreviousProcedureDropEdit.Name = "PreviousProcedureDropEdit";
		this.PreviousProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 18, true);
		this.PreviousProcedureDropEdit.TabIndex = 0;
		//
		// ImportFromTemporaryStorageRegisterButton
		//
		this.ImportFromTemporaryStorageRegisterButton.AllowDrop = true;
		this.ImportFromTemporaryStorageRegisterButton.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("BFAEEFF0-DBC3-66A9-41AD-48B6DC534191", "Import from TS Register", "Import from Temporary Storage Register");
		this.ImportFromTemporaryStorageRegisterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 15, true);
		this.ImportFromTemporaryStorageRegisterButton.Name = "ImportFromTemporaryStorageRegisterButton";
		this.ImportFromTemporaryStorageRegisterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 22, true);
		this.ImportFromTemporaryStorageRegisterButton.TabIndex = 1;
		this.ImportFromTemporaryStorageRegisterButton.ToolTipCaption = ResString.GetMultilingualString("81179E02-109A-17A7-4375-DF7D2BC14ACD", "Import from Temporary Storage Register");
		this.ImportFromTemporaryStorageRegisterButton.Click += new System.EventHandler(this.ImportFromTemporaryStorageRegisterButton_Click);
		// 
		// PreviousProcedureBasicUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Controls.Add(this.PreviousProcedureDropEdit);
		this.Controls.Add(this.ImportFromTemporaryStorageRegisterButton);
		this.Name = "PreviousProcedureBasicUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 64, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.PreviousProcedureDropEdit.ResumeLayout(true);
		this.PreviousProcedureDropEdit.PerformLayout();
		this.ImportFromTemporaryStorageRegisterButton.ResumeLayout(true);
		this.ImportFromTemporaryStorageRegisterButton.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal Enterprise.ZArchitecture.GUI.ZDropEdit PreviousProcedureDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZButton ImportFromTemporaryStorageRegisterButton;
}
