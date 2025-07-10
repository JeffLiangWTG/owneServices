using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackShipmentDependentCollection : CFSShipmentDependentCollection
	{
		public PackUnpackShipmentDependentCollection(BusinessObjectFactory factory, CFSContainer parent)
			: base(factory, parent)
		{
		}

		#region BusinessObjectCollection Overrides

		public new PackUnpackShipment this[int index]
		{
			get { return (PackUnpackShipment)(Elements[index]); }
		}

		public new PackUnpackShipment AddNew()
		{
			return (PackUnpackShipment)base.AddNew();
		}

		public new PackUnpackShipment AddNew(Type bizObjType)
		{
			return (PackUnpackShipment)base.AddNew(bizObjType);
		}

		public override void Add(BusinessObject businessObject)
		{
			PackUnpackShipment newShipment = (PackUnpackShipment)businessObject;
			base.Add(newShipment);
			newShipment.ParentContainerRegistration = fParent;
		}

		#region OnAdded

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			PackUnpackShipment shipment = (PackUnpackShipment)bizOAdded;
			shipment.OuterPackLines.AttemptedToDeleteSavedPack += new EventHandler(NotifyAttemptToDeleteSavedPackline);
			base.OnAdded(bizOAdded);
		}

		#endregion

		#region OnRemoved

		protected override void OnRemoved(BusinessObject bizO)
		{
			PackUnpackShipment shipment = (PackUnpackShipment)bizO;
			shipment.OuterPackLines.AttemptedToDeleteSavedPack -= new EventHandler(NotifyAttemptToDeleteSavedPackline);
			base.OnRemoved(bizO);
		}

		#endregion

		#region RemoveAndDelete

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (elementToDelete.IsInDatabase)
			{
				NotifyAttemptToDeleteSavedShipment();
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		#endregion

		#endregion

		#region Events

		public event EventHandler AttemptedToDeleteSavedShipment;
		public void NotifyAttemptToDeleteSavedShipment()
		{
			if (AttemptedToDeleteSavedShipment != null)
			{
				AttemptedToDeleteSavedShipment(this, EventArgs.Empty);
			}
		}

		public event EventHandler AttemptedToDeleteSavedPack;
		void NotifyAttemptToDeleteSavedPackline(object sender, EventArgs args)
		{
			if (AttemptedToDeleteSavedPack != null)
			{
				AttemptedToDeleteSavedPack(sender, args);
			}
		}

		#endregion

	}
}
