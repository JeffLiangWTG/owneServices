using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageJobServiceDependentCollection : JobServiceDependentCollection
	{
		public CartageJobServiceDependentCollection(CommonBookedCtgMove bookedCtgMove, BusinessObjectFactory factory)
			: base(bookedCtgMove, factory)
		{
		}

		public new CartageJobService this[int index]
		{
			get { return (CartageJobService)base[index]; }
		}

		public new CartageJobService AddNew()
		{
			return (CartageJobService)base.AddNew();
		}
	}
}
