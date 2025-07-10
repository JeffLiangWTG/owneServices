using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationTaxRateFileImportLineCollection : NonPersistentBusinessObjectCollection<OrganisationTaxRateFileImportLine>
	{
		public OrganisationTaxRateFileImportLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrganisationTaxRateFileImportLine(Factory);
		}

		protected override bool AllowNewCore => false;
	}
}
