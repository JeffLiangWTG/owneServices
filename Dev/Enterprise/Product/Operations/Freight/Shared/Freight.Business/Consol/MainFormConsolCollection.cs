using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Consol collection used on the main form.
	/// </summary>
	public class MainFormConsolCollection : MainFormGenericConsolCollection<CommonConsol>
	{
		public MainFormConsolCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ManyToManyShipmentCollection GetShipmentFromBusinessObject(CommonConsol businessObject)
		{
			return businessObject != null ? businessObject.Shipments : null;
		}
	}

	public abstract class MainFormGenericConsolCollection<T> : BusinessObjectCollection<T>, IRelationshipAdderForController where T : BusinessObject
	{
		public MainFormGenericConsolCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected abstract ManyToManyShipmentCollection GetShipmentFromBusinessObject(T businessObject);

		#region IRelationshipAdderForController Members

		public void AddRelationshipToNewObject(BusinessObject newBusinessObject)
		{
			if (newBusinessObject is T && ParentShipment != null)
			{
				ManyToManyShipmentCollection shipments = GetShipmentFromBusinessObject((T)newBusinessObject);
				if (shipments != null)
				{
					shipments.IsAddingRelationshipToNewObject = true;
					try
					{
						shipments.AddFromDatabase(ParentShipment.PK);
					}
					finally
					{
						shipments.IsAddingRelationshipToNewObject = false;
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Set when this collection is used in a findbox list from a Shipment.
		/// Used by AddRelationshipToNewObject.
		/// </summary>
		public CommonShipment ParentShipment { get; set; }
	}
}
