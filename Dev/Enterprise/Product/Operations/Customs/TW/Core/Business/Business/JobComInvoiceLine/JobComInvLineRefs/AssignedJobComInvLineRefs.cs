using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	[DependentBusinessObject(typeof(JobComInvoiceLine), "AssignedJobComInvLineRefsCollection")]
	public class AssignedJobComInvLineRefs : JobComInvLineRefs
	{
		public AssignedJobComInvLineRefs(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.AssignedJobComInvLineRefs.JG_ReferenceNumber", Caption = "Assigned Number", FullDescription = "The assigned number by the authority for the specific goods.")]
		public override ZString JG_ReferenceNumber { get => base.JG_ReferenceNumber; set => base.JG_ReferenceNumber = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JG_ReferenceType = JobComInvLineRefsType.Codes.AssignedNumber;
		}

		protected override JobComInvLineRefsValidation GetNewValidation()
		{
			return new AssignedJobComInvLineRefsValidation(this);
		}

		public new AssignedJobComInvLineRefsValidation Validation => (AssignedJobComInvLineRefsValidation)base.Validation;

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected override bool SupportsCloneCore() => true;
	}
}
