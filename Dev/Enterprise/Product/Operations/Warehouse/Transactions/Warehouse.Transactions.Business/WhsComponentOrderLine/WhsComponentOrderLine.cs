using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsComponentOrderLine : WhsPickableDocketLine
	{
		protected WhsComponentOrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region CanCalculateShortfallForAllLinesFromDBCore

		// Component Orders do not calculate shortfall in one hit
		protected override bool CanCalculateShortfallForAllLinesFromDBCore => false;

		#endregion

		#region WE_CrossDockQuantity

		protected override ZDecimal WE_CrossDockQuantityCore => ZDecimal.Zero;

		protected override bool CanCrossDockInventoryCore => false;

		#endregion

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZString WE_AllocationKey
		{
			get => base.WE_AllocationKey;
			set => base.WE_AllocationKey = value;
		}

		#region IsComponentLineOnSalesOrder

		protected override bool IsComponentLineOnSalesOrderCore => false;

		#endregion

		#region GetPickLinesForRelease

		protected override WhsPickLineCollection GetPickLinesForRelease() => new WhsPickLineCollection(Factory);

		#endregion

		#region IsUpdateForOrderPickingStatusAllowed

		protected override bool IsUpdateForOrderPickingStatusAllowedCore() => !IsFinalised;

		#endregion

		#region Cloning

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				WhsDocketLineSchema.Constants.WE_LineNo,
				WhsDocketLineSchema.Constants.WE_SubLineNo,
			};

			return result;
		}

		#endregion
	}
}
