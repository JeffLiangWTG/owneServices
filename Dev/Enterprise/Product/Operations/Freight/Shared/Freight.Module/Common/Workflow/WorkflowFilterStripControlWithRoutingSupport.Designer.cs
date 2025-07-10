using System.ComponentModel;
using System.Drawing;
using CargoWise.ComponentModel.Design;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module
{
	public partial class WorkflowFilterStripControlWithRoutingSupport
	{
		#region Designer Generated code

		ZCodeFindBox OriginFindbox;
		ZCodeFindBox DestinationFindBox;

		void InitializeComponent()
		{
			this.OriginFindbox = new ZCodeFindBox();
			this.DestinationFindBox = new ZCodeFindBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MilestoneTypeDropEdit
			// 
			// 
			// PropertySearchDropEdit
			// 
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WorkflowModuleFilterWithRoutingSupport);
			// 
			// OriginFindbox
			// 
			this.BindingSource.SetBindingMember(this.OriginFindbox, "Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((WorkflowModuleFilterWithRoutingSupport)(null)).Origin)));
			this.OriginFindbox.CaptionResourceString = Res.GetData("WorkflowFilterStripControlWithRoutingSupport|c0798825-b642-42ad-bd03-716fad2a32e9", "Origin");
			this.OriginFindbox.Location = ControlDpiScalingHelper.NewScaledPoint(332, 24, true);
			this.OriginFindbox.Name = "OriginFindbox";
			this.OriginFindbox.ShowDescriptionBox = false;
			this.OriginFindbox.Size = ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OriginFindbox.TabIndex = 8;
			// 
			// DestinationFindBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationFindBox, "Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((WorkflowModuleFilterWithRoutingSupport)(null)).Destination)));
			this.DestinationFindBox.CaptionResourceString = Res.GetData("WorkflowFilterStripControlWithRoutingSupport|a2ee0295-b9e5-4b09-b928-2c7bdca1c180", "Destination");
			this.DestinationFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(522, 24, true);
			this.DestinationFindBox.Name = "DestinationFindBox";
			this.DestinationFindBox.ShowDescriptionBox = false;
			this.DestinationFindBox.Size = ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.DestinationFindBox.TabIndex = 9;
			// 
			// WorkflowFilterStripControlWithRoutingSupport
			// 
			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.Controls.Add(this.DestinationFindBox);
			this.Controls.Add(this.OriginFindbox);
			this.Name = "WorkflowFilterStripControlWithRoutingSupport";
			this.Size = ControlDpiScalingHelper.NewScaledSize(667, 68, true);
			this.Controls.SetChildIndex(this.OriginFindbox, 0);
			this.Controls.SetChildIndex(this.MilestoneTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.PropertySearchDropEdit, 0);
			this.Controls.SetChildIndex(this.DestinationFindBox, 0);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
