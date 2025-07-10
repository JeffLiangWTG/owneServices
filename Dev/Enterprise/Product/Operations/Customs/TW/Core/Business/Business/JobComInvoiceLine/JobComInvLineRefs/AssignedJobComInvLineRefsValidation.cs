using System.Linq;

namespace Enterprise.Customs.TW.Business
{
	public class AssignedJobComInvLineRefsValidation : Customs.Business.JobComInvLineRefsValidation
	{
		public AssignedJobComInvLineRefsValidation(AssignedJobComInvLineRefs parent)
			: base(parent)
		{
		}

		public new AssignedJobComInvLineRefs Parent => (AssignedJobComInvLineRefs)base.Parent;

		protected override void CheckJG_ReferenceNumber()
		{
			base.CheckJG_ReferenceNumber();
			var invoiceLine = Parent.InvoiceLine;
			if (!Parent.JG_ReferenceNumber.IsEmpty)
			{
				if (invoiceLine.AssignedJobComInvLineRefsCollection?.Cast<AssignedJobComInvLineRefs>().Any(x => x.JG_ReferenceNumber == Parent.JG_ReferenceNumber && x.PK != Parent.PK) ?? false)
				{
					Parent.JG_ReferenceNumberInfo.AddMessageError(Res.GetString("312a9f1c-0876-490a-ae47-a07981bc7592", "Assigned Number cannot be duplicated."));
				}
			}
			invoiceLine.Validation?.CheckAssignedJobComInvLineRefs();
		}
	}
}
