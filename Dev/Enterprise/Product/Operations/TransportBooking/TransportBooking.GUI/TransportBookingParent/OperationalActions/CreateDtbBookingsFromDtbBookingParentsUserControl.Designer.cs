using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.GUI
{
	partial class CreateDtbBookingsFromDtbBookingParentsUserControl
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
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BookingTemplateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ICreateDtbBookingsFromDtbBookingParentsApplicator);
			// 
			// StatusCodeDropEdit
			// 
			this.DirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ICreateDtbBookingsFromDtbBookingParentsApplicator)(null)).Direction)));
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 2, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.DirectionDropEdit.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "Direction";
			// 
			// BookingTemplateDropEdit
			// 
			this.BookingTemplateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BookingTemplateDropEdit, "BookingTemplate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ICreateDtbBookingsFromDtbBookingParentsApplicator)(null)).BookingTemplate)));
			this.BookingTemplateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 32, true);
			this.BookingTemplateDropEdit.Name = "BookingTemplateDropEdit";
			this.BookingTemplateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.BookingTemplateDropEdit.TabIndex = 0;
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.zLabel2.TabIndex = 1;
			this.zLabel2.Text = "Booking Template";
			// 
			// CreateDtbBookingsFromDtbBookingParentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DirectionDropEdit);
			this.Controls.Add(this.BookingTemplateDropEdit);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zLabel2);
			this.Name = "CreateDtbBookingsFromDtbBookingParentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 80, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		private ZArchitecture.GUI.ZDropEdit BookingTemplateDropEdit;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
	}
}
