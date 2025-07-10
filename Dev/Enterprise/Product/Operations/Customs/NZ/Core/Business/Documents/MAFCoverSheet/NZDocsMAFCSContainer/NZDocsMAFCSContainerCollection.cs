using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet
{
	public class NZDocsMAFCSContainerCollection : NonPersistentBusinessObjectCollection<NZDocsMAFCSContainer>
	{
		public NZDocsMAFCSContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NZDocsMAFCSContainer(Factory);
		}
	}
}
