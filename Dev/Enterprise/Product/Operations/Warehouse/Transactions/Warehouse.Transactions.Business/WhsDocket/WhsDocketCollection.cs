using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsDocketCollection : ActiveBusinessObjectCollection<WhsDocket>
	{
		protected WhsDocketCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected WhsDocketCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected WhsDocketCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected WhsDocketCollection(BusinessObject master, SchemaColumn relationshipColumn)
			: base(master.Factory, master, null, relationshipColumn)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(WhsDocketSchema.WD_DocketType, DocketTypes);
		}

		protected abstract IEnumerable<string> DocketTypes { get; }

		#region AllowNew

		public void SetAllowNew(bool value)
		{
			allowNew = value;
		}

		protected override sealed bool AllowNew
		{
			get { return allowNew ?? AllowNewDocket; }
		}

		protected virtual bool AllowNewDocket
		{
			get { return true; }
		}

		bool? allowNew;

		#endregion
	}
}
