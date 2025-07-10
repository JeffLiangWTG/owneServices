using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsReceiveEventExportContextBuilder : WhsOrderAndReceiveEventExportContextBuilder
	{
		internal WhsReceiveEventExportContextBuilder(WhsReceive receive)
			: base(receive)
		{
		}

		protected override void AddSpecificContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ReceiveReference, Docket.WD_ExternalReference);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ReceiveReferenceSplit, Docket.WD_ExternalReferenceSplit);
		}
	}
}
