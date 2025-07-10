
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Business
{
	[ModuleID(ModuleId.LoadListConsol)]
	public class CFSLoadListConsolCollection : MainFormConsolCollection
	{
		public CFSLoadListConsolCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new CFSLoadListConsol this[int index]
		{
			get { return (CFSLoadListConsol)Elements[index]; }
		}

		public new CFSLoadListConsol AddNew()
		{
			return (CFSLoadListConsol)base.AddNew();
		}

		#region IRelationshipAdderForController Members

		/// <summary>
		/// Set when this collection is used in a findbox list from a Shipment.
		/// Used by AddRelationshipToNewObject.
		/// </summary>
		CFSShipment fParent;
		public CFSShipment Parent
		{
			get { return fParent; }
			set { fParent = value; }
		}

		public new void AddRelationshipToNewObject(BusinessObject newBusinessObject)
		{
			CFSLoadListConsol newLoadList = newBusinessObject as CFSLoadListConsol;
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
