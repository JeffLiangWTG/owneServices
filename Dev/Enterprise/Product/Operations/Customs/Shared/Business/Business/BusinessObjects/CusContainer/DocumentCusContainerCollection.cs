using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Summary description for DocumentCusContainerCollection.
	/// </summary>
	public class DocumentCusContainerCollection : NonPersistentBusinessObjectCollection<DocumentCusContainer>
	{
		public DocumentCusContainerCollection(BusinessObjectFactory factory, ICusContainerCollection<BaseCusContainer> baseCusContainerCollection) : base(factory)
		{
			foreach (BaseCusContainer baseCusContainer in baseCusContainerCollection)
			{
				AddNew(baseCusContainer);
			}
		}
		public DocumentCusContainer AddNew(BaseCusContainer container)
		{
			DocumentCusContainer result = base.AddNew();
			result.Container = container;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			DocumentCusContainer result = new DocumentCusContainer(Factory);
			return result;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
