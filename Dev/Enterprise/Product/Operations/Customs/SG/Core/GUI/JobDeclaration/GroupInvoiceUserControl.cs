using System;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class GroupInvoiceUserControl : BaseInvoiceGroupingUserControl
	{
		public GroupInvoiceUserControl()
		{
			InitializeComponent();
			GroupChargeGrid.AfterBind += new EventHandler(GroupChargeGrid_AfterBind);
		}

		void GroupChargeGrid_AfterBind(object sender, EventArgs e)
		{
			GroupChargeGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.Constants.J7_IsDutiable);
			GroupChargeGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable);

			GroupChargeGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_Percentage, Enterprise.Customs.SG.V4.GUI.Res.GetString("A5D6811E-F2B5-43BE-9F81-7939079A8118", "%"));
		}
	}
}
