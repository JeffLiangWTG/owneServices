using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class WhsOrderLineWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public WhsOrderLineWrapper(IWhsDocketLine orderLine, BusinessObjectFactory factory) : base(factory)
		{
			OrderLine = orderLine;
			Order = Argument.NotNull(Factory.Load<IWhsDocket>(OrderLine.WE_WD), "orderLine.WE_WD");
		}

		public IWhsDocketLine OrderLine { get; }
		public IWhsDocket Order { get; }

		public ZBool IsAllocatedToInventories
		{
			get
			{
				return AllocatedInventories.Any();
			}
		}

		public IWhsPickLine[] PickLines
		{
			get
			{
				if (pickLines is null)
				{
					pickLines = Factory.Load<IWhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, OrderLine.PK));
				}
				return pickLines;
			}
		}
		IWhsPickLine[] pickLines;

		public IReadOnlyDictionary<IWhsInventoryView, IWhsPickLine> InventoryPickLineDict
		{
			get
			{
				if (pickLineInventoryDict is null)
				{
					pickLineInventoryDict = PickLines.Zip(PickLines.Select(pickLine => Factory.Load<IWhsInventoryView>(pickLine.WZ_WE_InventoryLine)), (pickLine, inventory) => new { pickLine, inventory }).ToDictionary(pair => pair.inventory, pair => pair.pickLine);
				}
				return pickLineInventoryDict;
			}
		}
		IReadOnlyDictionary<IWhsInventoryView, IWhsPickLine> pickLineInventoryDict;

		public IEnumerable<IWhsInventoryView> AllocatedInventories => InventoryPickLineDict.Keys;
	}
}
