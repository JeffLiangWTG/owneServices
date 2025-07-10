using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentForACASWrapperCollection : NonPersistentBusinessObjectCollection<HVLVConsignmentForACASWrapper>
	{
		public HVLVConsignmentForACASWrapperCollection(IEnumerable<HVLVConsignment> consignments)
			: base()
		{
			foreach (var consignment in consignments)
			{
				Add(new HVLVConsignmentForACASWrapper(consignment));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new System.NotSupportedException("HVLVConsignmentForACASWrapperCollection is not allowed to add new elements");

		protected override bool AllowNewCore => false;
	}
}
