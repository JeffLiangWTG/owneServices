using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingContainerCollection : ActiveBusinessObjectCollection<BillOfLadingContainer>
	{
		public BillOfLadingContainerCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public BillOfLadingContainerCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship) { }
	}
}
