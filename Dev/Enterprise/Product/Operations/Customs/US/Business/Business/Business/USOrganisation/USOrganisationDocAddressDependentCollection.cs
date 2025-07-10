using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USOrganisationDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public USOrganisationDocAddressDependentCollection(JobComInvoiceHeader invoice)
			: base(invoice)
		{
		}

		public new USOrganisationDocAddress this[int index]
		{
			get { return (USOrganisationDocAddress)base[index]; }
		}

		public new USOrganisationDocAddress AddNew()
		{
			return (USOrganisationDocAddress)base.AddNew();
		}

		public new USOrganisationDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (USOrganisationDocAddress)base.FindOrCreateWithRequirement(requirement);
		}
	}
}
