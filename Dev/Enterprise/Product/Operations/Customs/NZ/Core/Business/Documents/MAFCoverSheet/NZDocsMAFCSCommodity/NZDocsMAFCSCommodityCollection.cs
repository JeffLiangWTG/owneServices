using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet
{
	public class NZDocsMAFCSCommodityCollection : NonPersistentBusinessObjectCollection<NZDocsMAFCSCommodity>
	{
		public NZDocsMAFCSCommodityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NZDocsMAFCSCommodity(Factory);
		}
	}
}
