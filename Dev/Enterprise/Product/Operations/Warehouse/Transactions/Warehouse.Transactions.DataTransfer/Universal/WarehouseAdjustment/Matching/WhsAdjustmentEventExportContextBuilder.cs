using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsAdjustmentEventExportContextBuilder
	{
		internal WhsAdjustmentEventExportContextBuilder(WhsAdjustment adjustment)
		{
			Adjustment = Argument.NotNull(adjustment, nameof(WhsAdjustment) + " adjustment");
		}

		readonly WhsAdjustment Adjustment;

		internal void AddWhsAdjustmentContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.AdjustmentReference, Adjustment.WD_ExternalReference);
		}
	}
}
