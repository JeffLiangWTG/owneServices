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
	public class ADDCVDDoesNotApplyApplicator : OperationalActionMethodApplicator
	{
		public ADDCVDDoesNotApplyApplicator()
			: base("ADDCVDDoesNotApplyApplicator")
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var questionMessage = Res.GetString("670d8430-5b89-494d-808d-49a2d2e433e0", "This will tick ADD N/A and CVD N/A for all selected Consignments.");
			var confirmationMessage = Res.GetString("bc5953fe-3da1-4276-9f76-44b580cbe149", "ADD N/A and CVD N/A have been ticked for all selected Consignments.");
			var actionCaption = Res.GetString("6c488e30-e55d-4151-a5f2-5a815c09adc6", "ADD/CVD Does not apply");

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
					item.ULI_AntiDumping = true;
					item.ULI_Countervailing = true;
				}
			}

			Globals.Message.ShowInformation(confirmationMessage, actionCaption);
		}
	}
}
