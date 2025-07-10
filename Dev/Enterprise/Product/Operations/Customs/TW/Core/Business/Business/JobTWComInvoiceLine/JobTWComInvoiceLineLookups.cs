using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class JobTWComInvoiceLineLookups : AutoJobTWComInvoiceLineLookups
	{
		public JobTWComInvoiceLineLookups(AutoJobTWComInvoiceLine parent) : base(parent)
		{
		}

		protected new JobTWComInvoiceLine Parent => (JobTWComInvoiceLine)base.Parent;

		IRefCusPackListProvider CachedCusPackListProvider => Factory.GetCachedValue<RefCusPackListProvider>();

		public ICodeDescriptionPairList CustomsUQList => CachedCusPackListProvider.GetCIPCustomsPackList(Factory, string.Empty);

		public ICodeDescriptionPairList CategoryCodesOfCAAAircraftPartsList => TWRefCusCodeListTypes.GetCategoryCodesOfCAAAircraftParts(Factory);

		public ICodeDescriptionPairList CAAAircraftPartsCodesList => TWRefCusCodeListTypes.GetCAAAircraftPartsCodes(Factory, Parent.TWL_AircraftPartsCategory);
	}
}
