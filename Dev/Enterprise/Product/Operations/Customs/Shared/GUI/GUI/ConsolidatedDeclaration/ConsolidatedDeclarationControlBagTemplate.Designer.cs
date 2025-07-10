namespace Enterprise.Customs.GUI
{
	partial class ConsolidatedDeclarationControlBagTemplate
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.SuspendLayout();
			Enterprise.ZArchitecture.ZTextBox JobNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			Enterprise.ZArchitecture.ZTextBox EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			Enterprise.ZArchitecture.ZTextBox EntryStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			Enterprise.ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			Enterprise.ZArchitecture.GUI.ZDropEdit MessageSubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			Enterprise.ZArchitecture.GUI.ZDropEdit TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			Enterprise.ZArchitecture.GUI.ZGuidFindBox ImporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			Enterprise.ZArchitecture.ZTextBox VoyageFlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			Enterprise.ZArchitecture.GUI.ZDateEdit DischargeETADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			Enterprise.ZArchitecture.GUI.ZDateEdit EntryPeriodDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();

			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.ConsolidatedDeclaration);

			this.BindingSource.SetBindingMember(JobNumberTextBox, "CRD_JobReferenceNumber");
			JobNumberTextBox.Name = "JobNumberTextBox";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).CRD_JobReferenceNumber)));

			this.BindingSource.SetBindingMember(EntryNumberTextBox, "LeadDeclaration.DeclarationNumber");
			EntryNumberTextBox.Name = "EntryNumberTextBox";
			EntryNumberTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|c1eeb997-7fb0-4d2f-8d48-f8bb76786d88", "Entry Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DeclarationNumber)));

			this.BindingSource.SetBindingMember(EntryStatusTextBox, "LeadDeclaration.JE_EntryStatusDescription");
			EntryStatusTextBox.Name = "EntryStatusTextBox";
			EntryStatusTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("AEEE6BEF-DB4D-451A-BE83-9D5DC80D31CA", "Customs Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_EntryStatusDescription)));

			this.BindingSource.SetBindingMember(MessageTypeDropEdit, "LeadDeclaration.JE_MessageType");
			MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			MessageTypeDropEdit.ReadOnly = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MessageType)));

			this.BindingSource.SetBindingMember(MessageSubTypeDropEdit, "LeadDeclaration.JE_MessageSubType");
			MessageSubTypeDropEdit.Name = "MessageSubTypeDropEdit";
			MessageSubTypeDropEdit.ReadOnly = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MessageSubType)));

			this.BindingSource.SetBindingMember(TransportModeDropEdit, "LeadDeclaration.JE_TransportMode");
			TransportModeDropEdit.Name = "TransportModeDropEdit";
			TransportModeDropEdit.ReadOnly = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportMode)));

			this.BindingSource.SetBindingMember(ImporterGuidFindBox, "LeadDeclaration.JE_OH_Importer");
			ImporterGuidFindBox.Name = "ImporterGuidFindBox";
			ImporterGuidFindBox.Enabled = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OH_Importer)));

			this.BindingSource.SetBindingMember(VesselCodeFindBox, "LeadDeclaration.JE_VesselName");
			VesselCodeFindBox.Name = "VesselCodeFindBox";
			VesselCodeFindBox.ReadOnly = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VesselName)));

			this.BindingSource.SetBindingMember(VoyageFlightNoTextBox, "LeadDeclaration.JE_VoyageFlightNo");
			VoyageFlightNoTextBox.Name = "VoyageFlightNoTextBox";
			VoyageFlightNoTextBox.ReadOnly = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VoyageFlightNo)));

			this.BindingSource.SetBindingMember(DischargeETADateEdit, "LeadDeclaration.JE_DateOfArrival");
			DischargeETADateEdit.Name = nameof(DischargeETADateEdit);
			DischargeETADateEdit.ReadOnly = true;
			DischargeETADateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|DischargeETA", "Discharge ETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DateOfArrival)));

			this.BindingSource.SetBindingMember(PortOfLoadingCodeFindBox, "LeadDeclaration.JE_RL_NKPortOfLoading");
			PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			PortOfLoadingCodeFindBox.ReadOnly = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKPortOfLoading)));

			this.BindingSource.SetBindingMember(PortOfDischargeCodeFindBox, "LeadDeclaration.JE_RL_NKPortOfArrival");
			PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			PortOfDischargeCodeFindBox.ReadOnly = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKPortOfArrival)));

			this.BindingSource.SetBindingMember(EntryPeriodDateEdit, "CRD_PeriodTo");
			EntryPeriodDateEdit.Name = "EntryPeriodDateEdit";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).CRD_PeriodTo)));

			this.Controls.Add(JobNumberTextBox);
			this.Controls.Add(EntryNumberTextBox);
			this.Controls.Add(EntryStatusTextBox);
			this.Controls.Add(MessageTypeDropEdit);
			this.Controls.Add(MessageSubTypeDropEdit);
			this.Controls.Add(TransportModeDropEdit);
			this.Controls.Add(ImporterGuidFindBox);
			this.Controls.Add(VesselCodeFindBox);
			this.Controls.Add(VoyageFlightNoTextBox);
			this.Controls.Add(DischargeETADateEdit);
			this.Controls.Add(PortOfLoadingCodeFindBox);
			this.Controls.Add(PortOfDischargeCodeFindBox);
			this.Controls.Add(EntryPeriodDateEdit);
		}

		#endregion
	}
}
