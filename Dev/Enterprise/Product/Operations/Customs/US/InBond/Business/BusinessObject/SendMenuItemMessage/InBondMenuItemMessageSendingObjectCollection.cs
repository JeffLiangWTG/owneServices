using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMenuItemMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<InBondMenuItemMessageSendingObject>
	{
		public InBondMenuItemMessageSendingObjectCollection(InBondMenuItemMessageData inBondMenuItemMessageData)
			: base(inBondMenuItemMessageData.Factory)
		{
			Argument.NotNull(inBondMenuItemMessageData, nameof(inBondMenuItemMessageData));
			parent = inBondMenuItemMessageData;
		}
		readonly InBondMenuItemMessageData parent;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InBondMenuItemMessageSendingObject(parent);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var inBondMenuItemMessageSendingObject = (InBondMenuItemMessageSendingObject)child;
			if (parent.IsExport && inBondMenuItemMessageSendingObject.EntryType.IsEmpty)
			{
				inBondMenuItemMessageSendingObject.EntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			}
		}

		public void Populate(List<CusInBondMoveHeader> cusInBondMoveHeaders)
		{
			foreach (var cusInBondMoveHeader in cusInBondMoveHeaders)
			{
				var inBondMenuItemMessageSendingObject = new InBondMenuItemMessageSendingObject(parent, cusInBondMoveHeader);
				SetDefaultsForNewChild(inBondMenuItemMessageSendingObject);
				Add(inBondMenuItemMessageSendingObject);
			}
		}
	}
}
