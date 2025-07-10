using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsDocketLineCollectionAdHoc<TDocketLine> : ActiveBusinessObjectCollection<TDocketLine>
		where TDocketLine : WhsDocketLine
	{
		protected WhsDocketLineCollectionAdHoc(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(TDocketLine)))
		{
		}

		#region AllowNew

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}
