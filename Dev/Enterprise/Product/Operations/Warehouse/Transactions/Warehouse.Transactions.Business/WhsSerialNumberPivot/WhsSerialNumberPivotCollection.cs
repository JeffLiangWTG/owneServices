using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsSerialNumberPivotCollection : ActiveBusinessObjectCollection<WhsSerialNumberPivot>
	{
		#region WhsSerialNumberPivotCollection

		public WhsSerialNumberPivotCollection(ISerialNumberParent master)
			: base(master.Factory, (BusinessObject)master, Filter, WhsSerialNumberPivotSchema.WSV_ParentID)
		{
		}

		static ZQuery Filter => WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value ? null : ZQuery.NoResultQuery;

		ISerialNumberParent Parent => (ISerialNumberParent)Relationship.Master;

		#endregion

		#region SetDefaultsForNewElement

		protected override void SetDefaultsForNewElementCore(WhsSerialNumberPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.WSV_ParentTableCode = Parent.TablePrefix;
			newElement.WSV_ParentID = Parent.PK;
		}

		#endregion

		#region AllowNew

		protected override bool AllowNew => base.AllowNew && !Parent.SerialNumberReadOnly;

		#endregion

		#region WhsSerialNumberPivotCollectionFetchStrategy

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new WhsSerialNumberPivotCollectionFetchStrategy(this);

		#endregion
	}
}
