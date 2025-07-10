using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used in future WIs")]
	public class WhsDynamicWorkOrderCollection : WhsComponentOrderCollection
	{
		public WhsDynamicWorkOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new WhsDynamicWorkOrder this[int index]
		{
			get { return (WhsDynamicWorkOrder)base[index]; }
		}

		public new WhsDynamicWorkOrder AddNew()
		{
			return (WhsDynamicWorkOrder)base.AddNew();
		}

		protected override IEnumerable<string> DocketTypes
		{
			get { return new[] { DocketType.Codes.DynamicWorkOrder }; }
		}
	}
}
