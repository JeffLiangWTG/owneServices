namespace Enterprise.Customs.NZ.GUI
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
			Enterprise.ZArchitecture.GUI.ZDropEdit EntryStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			Enterprise.ZArchitecture.ZTextBox VoyageFlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			ConsolidatedDeclarationDetailsUserControl ConsolidatedDeclarationDetailsUserControl = new ConsolidatedDeclarationDetailsUserControl();

			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.ConsolidatedDeclaration);

			this.BindingSource.SetBindingMember(EntryStyleDropEdit, "EntryStyle");
			EntryStyleDropEdit.Name = "EntryStyleDropEdit";
			EntryStyleDropEdit.ReadOnly = true;
			EntryStyleDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("61347F8B-E024-4E33-88F6-68ACC1CD8420", "Entry Style");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ConsolidatedDeclaration)(null)).EntryStyle)));

			this.BindingSource.SetBindingMember(VesselCodeFindBox, "VesselName");
			VesselCodeFindBox.Name = "VesselCodeFindBox";
			VesselCodeFindBox.ReadOnly = true;
			VesselCodeFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8E4A42E9-B829-4E6E-98E4-0887EEF9DCAF", "Vessel");
			VesselCodeFindBox.CodeBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ConsolidatedDeclaration)(null)).VesselName)));

			this.BindingSource.SetBindingMember(VoyageFlightNoTextBox, "VoyageFlightNo");
			VoyageFlightNoTextBox.Name = "VoyageFlightNoTextBox";
			VoyageFlightNoTextBox.ReadOnly = true;
			VoyageFlightNoTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("72E36571-8C87-4C89-A9A0-B98220021DFD", "Arrival Flight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ConsolidatedDeclaration)(null)).VoyageFlightNo)));

			this.BindingSource.SetBindingMember(ConsolidatedDeclarationDetailsUserControl, ".");
			ConsolidatedDeclarationDetailsUserControl.Name = "ConsolidatedDeclarationDetailsUserControl";

			this.Controls.Add(EntryStyleDropEdit);
			this.Controls.Add(VesselCodeFindBox);
			this.Controls.Add(VoyageFlightNoTextBox);
			this.Controls.Add(ConsolidatedDeclarationDetailsUserControl);
		}

		#endregion
	}
}
