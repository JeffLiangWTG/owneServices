using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVConsignmentForMessagingCollection : NonPersistentBusinessObjectCollection<CusUSLVConsignmentForMessaging>
	{
		public CusUSLVConsignmentForMessagingCollection(CusUSLVClearance clearance)
			: this(clearance.Factory, clearance.CusUSLVConsignments.OfType<CusUSLVConsignment>())
		{
		}

		public CusUSLVConsignmentForMessagingCollection(CusUSLVConsignment consignment)
			: this(consignment.Shipment.Factory, new[] { consignment })
		{
		}

		public CusUSLVConsignmentForMessagingCollection(BusinessObjectFactory factory, IEnumerable<CusUSLVConsignment> consignments)
			: base(factory)
		{
			BuildCollectionForMessaging(consignments);
		}

		void BuildCollectionForMessaging(IEnumerable<CusUSLVConsignment> consignments)
		{
			consignments.Where(consignment => consignment.ULB_IsActive && MessageSendingHelper.HasValidStatusOnConsignment(consignment))
				.ForEach(consignment => Add(new CusUSLVConsignmentForMessaging(consignment)));
		}

		protected override bool AllowRemoveCore => false;

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Collection does not support direct creation of Non-Persistent BizOs, please use a CusUSLVConsignmentForMessaging constructor");
		}
	}
}
