using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	/// <summary>
	/// Summary description for ForwardingConsolCollection.
	/// </summary>
	public class ForwardingConsolCollection : MainFormConsolCollection
	{
		public ForwardingConsolCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new ForwardingConsol this[int index]
		{
			get { return (ForwardingConsol)Elements[index]; }
		}

		public new ForwardingConsol AddNew()
		{
			return (ForwardingConsol)base.AddNew();
		}

		#region IRelationshipAdderForController Members

		/// <summary>
		/// Set when this collection is used in a findbox list from a Shipment.
		/// Used by AddRelationshipToNewObject.
		/// </summary>
		ForwardingShipment fParent;
		public ForwardingShipment Parent
		{
			get { return fParent; }
			set { fParent = value; }
		}

		public new void AddRelationshipToNewObject(BusinessObject newBusinessObject)
		{
			ForwardingConsol newLoadList = newBusinessObject as ForwardingConsol;
			if (newLoadList != null && fParent != null)
			{
				newLoadList.Shipments.IsAddingRelationshipToNewObject = true;
				try
				{
					newLoadList.Shipments.AddFromDatabase(fParent.PK);
				}
				finally
				{
					newLoadList.Shipments.IsAddingRelationshipToNewObject = false;
				}
			}
		}

		#endregion
	}
}
