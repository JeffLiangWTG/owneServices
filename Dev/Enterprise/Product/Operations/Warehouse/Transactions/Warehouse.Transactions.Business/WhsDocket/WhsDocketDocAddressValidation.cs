using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	// Normally subclasses of ZValidation are auto-generated.
	// However we have manually created this class to avoid rerunning validation methods as this class is only used as piggyback validation.
	public class WhsDocketDocAddressValidation : ZValidation
	{
		public WhsDocketDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(WhsDocketDocAddressValidation); }
		}

		// This method is needed so CheckOrganisationPK can be accessed in unittests.
		// Trying to replace this function with reflection or a subclass will fail due to a reflection sanity check in AddError
		public void ValidateOrganisationPK()
		{
			ValidateCalculatedProperty(Parent.OrganisationPKInfo);
		}

		protected virtual void CheckOrganisationPK()
		{
			var organisation = Parent.Organisation;
			if (organisation != null && !organisation.OH_IsActive)
			{
				Parent.OrganisationPKInfo.AddError(Res.GetString("f7aa4f6f-9ada-4541-90fd-b914afc76e45", "This Organization is not active."));
			}
		}

		public override void ValidateAll()
		{
			throw new InvalidOperationException("Should not be calling ValidateAll() on PiggyBacked Validation.");
		}

		protected JobDocAddress Parent => (JobDocAddress)ParentFilter;

		protected WhsDocket Docket
		{
			get { return (WhsDocket)Parent.Parent; }
		}
	}
}
