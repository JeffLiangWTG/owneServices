using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public class ADDCVDAppliesApplicator : OperationalActionMethodApplicator
	{
		public ADDCVDAppliesApplicator()
			: base("ADDCVDAppliesApplicator")
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var questionMessage = Res.GetString("e4f296c1-7f62-4715-b618-c9b205c9aa50", "This will untick ADD N/A and CVD N/A for all selected Consignments.");
			var confirmationMessage = Res.GetString("0d3cacd3-8c49-4a03-9641-2313671fd4e8", "ADD N/A and CVD N/A have been unticked for all selected Consignments.");
			var actionCaption = Res.GetString("c056e99e-c9ce-4fa3-ba28-00cb9eb1f0f9", "ADD/CVD Applies");

			var dialogResult = Globals.Message.Show(questionMessage, actionCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (dialogResult == DialogResult.No)
			{
				return;
			}

			var consignments = targets.OfType<USConsignmentCombined>()
				.Where(target => target.IsConsignment)
				.Select(target => target.Consignment);

			foreach (var consignment in consignments)
			{
				foreach (CusUSLVItem item in consignment.CusUSLVItems)
				{
					item.ULI_AntiDumping = false;
					item.ULI_Countervailing = false;
				}
			}

			Globals.Message.ShowInformation(confirmationMessage, actionCaption);
		}
	}
}
