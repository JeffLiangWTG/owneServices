using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PickLinePair : IPickLinePair
	{
		PickLinePair(WhsPickLine pickLineOnOrder)
		{
			PickLineOnOrder = Argument.NotNull(pickLineOnOrder, nameof(pickLineOnOrder));
		}

		PickLinePair(WhsPickLine pickLineOnOrder, WhsPickLine originalPickLine)
			: this(pickLineOnOrder)
		{
			OriginalPickLine = Argument.NotNull(originalPickLine, nameof(originalPickLine));
		}

		WhsPickLine OriginalPickLine { get; }

		public WhsPickLine PickLineOnOrder { get; }
		public WhsPickLine PickLineForPickingDetails => OriginalPickLine ?? PickLineOnOrder;

		public GlbStaff AssignedTo => PickLineForPickingDetails.AssignedTo;
		public ZString AssignedToCode
		{
			get => PickLineForPickingDetails.WZ_GS_NKAssignedTo;
			set => PickLineForPickingDetails.WZ_GS_NKAssignedTo = value;
		}

		public ZDateTimeOffset PickedDateTime
		{
			get => PickLineForPickingDetails.WZ_PickedDateTime;
			set
			{
				if (value.IsValid)
				{
					var warehouse = PickLineForPickingDetails.InventoryLine.Warehouse;
					PickLineForPickingDetails.WZ_PickedDateTime = warehouse.GetWarehouseBranchDateTimeOffset(value.ToUtcDateTime());
				}
				else
				{
					PickLineForPickingDetails.WZ_PickedDateTime = value;
				}
			}
		}

		public static PickLinePair New(WhsPickLine pickLine)
		{
			Argument.NotNull(pickLine, nameof(pickLine));

			PickLinePair result;

			if (pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid)
			{
				var originalPickLine = GetOriginalPickLine(pickLine, pickLine.WZ_WE_OriginalPickedInventoryLine);
				result = new PickLinePair(pickLine, originalPickLine);
			}
			else
			{
				result = new PickLinePair(pickLine);
			}

			return result;
		}

		static WhsPickLine GetOriginalPickLine(WhsPickLine pickLine, ZGuid originalInventoryLinePK)
		{
			var result = pickLine;

			while (result.WZ_WE_InventoryLine != originalInventoryLinePK)
			{
				result = result.InventoryLine.PickLines.Single();
			}

			return result;
		}
	}
}
