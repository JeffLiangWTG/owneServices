using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBOMInventoryPivot : AutoWhsBOMInventoryPivot, IWhsBOMInventoryPivot
	{
		public WhsBOMInventoryPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public WhsDocketLine ComponentLine => Factory.Load<WhsDocketLine>(WIP_WE_ComponentLine);
		IWhsDocketLine IWhsBOMInventoryPivot.ComponentLine => ComponentLine;
		public WhsDocketLine InventoryLine => Factory.Load<WhsDocketLine>(WIP_WE_InventoryLine);

		[RelatedBusinessObjectTestExclude("RelatedBusinessObject ComponentLine (WhsDocketLine) is an abstract class")]
		[RelatedBusinessObject(nameof(ComponentLine))]
		public override ZGuid WIP_WE_ComponentLine
		{
			get => base.WIP_WE_ComponentLine;
			set => base.WIP_WE_ComponentLine = value;
		}

		[RelatedBusinessObjectTestExclude("RelatedBusinessObject InventoryLine (WhsDocketLine) is an abstract class")]
		[RelatedBusinessObject(nameof(InventoryLine))]
		public override ZGuid WIP_WE_InventoryLine
		{
			get => base.WIP_WE_InventoryLine;
			set => base.WIP_WE_InventoryLine = value;
		}

		#region FillWithValidTestData
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			receiveLine.FillWithValidTestData();

			var order = Factory.NewWithValidTestData<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			orderLine.FillWithValidTestData();

			WIP_WE_ComponentLine = orderLine.PK;
			WIP_WE_InventoryLine = receiveLine.PK;
			WIP_ComponentQuantity = 1m;
		}
#endif
		#endregion
	}
}
