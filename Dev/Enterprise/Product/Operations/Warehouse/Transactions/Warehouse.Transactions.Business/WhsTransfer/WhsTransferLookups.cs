using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferLookups : WhsDocketLookups
	{
		public WhsTransferLookups(WhsTransfer parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList SubTypesCore => new TransferType();

		public CodeDescriptionPairList SubTypesWithPutawayType
		{
			get
			{
				var subTypes = SubTypes;

				if (Parent.WD_IsPutawayTransfer)
				{
					var putawayTypes = new CodeDescriptionPairList();
					putawayTypes.AddPair(NonPersistentTransferType.Codes.Putaway, NonPersistentTransferType.Descriptions.Putaway);

					subTypes = putawayTypes;
				}
				else if (Parent.IsTransferringForOrder)
				{
					var outboundDockDoorTransferTypes = new CodeDescriptionPairList();
					outboundDockDoorTransferTypes.AddPair(NonPersistentTransferType.Codes.OutboundDockDoor, NonPersistentTransferType.Descriptions.OutboundDockDoor);

					subTypes = outboundDockDoorTransferTypes;
				}
				else if (Parent.WD_IsPickFaceReplenishment)
				{
					var autoCreatedReplenishmentTransferTypes = new CodeDescriptionPairList();
					autoCreatedReplenishmentTransferTypes.AddPair(NonPersistentTransferType.Codes.AutoCreatedReplenishment, NonPersistentTransferType.Descriptions.AutoCreatedReplenishment);

					subTypes = autoCreatedReplenishmentTransferTypes;
				}

				return subTypes;
			}
		}

		protected override WhsPickCollectionForTransfers PicksForReplenishmentCore
			=> Factory.GetCachedValue("WhsTransferLookups|PicksForReplenishment|" + Parent.WD_WW_Whs, () => new WhsPickCollectionForTransfers(Factory, Parent.WD_WW_Whs));

		protected new WhsTransfer Parent
		{
			get { return (WhsTransfer)base.Parent; }
		}
	}
}
