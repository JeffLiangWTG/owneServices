using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ARAccountDetailsDependentCollection : DependentBusinessObjectCollection<AccARAccountDetails, OrgCompanyData>
	{
		public ARAccountDetailsDependentCollection(OrgCompanyData parent)
			: base(parent, new ZQuery(AccAPAccountDetailsSchema.A1_PaymentMethod, new string[] { AccARAccountDetails.ARBankAccPayment, AccARAccountDetails.ARCollectionRequest, AccARAccountDetails.ARNettingBankAccount }))
		{ }
	}
}
