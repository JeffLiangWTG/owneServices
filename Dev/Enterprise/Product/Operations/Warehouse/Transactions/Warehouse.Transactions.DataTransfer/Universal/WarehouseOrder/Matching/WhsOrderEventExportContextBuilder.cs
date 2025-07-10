using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsOrderEventExportContextBuilder : WhsOrderAndReceiveEventExportContextBuilder
	{
		internal WhsOrderEventExportContextBuilder(WhsOrder order)
			: base(order)
		{
		}

		protected override void AddSpecificContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			contextValues.AddOrderNumber(Docket);
			contextValues.AddOrderNumberSplit(Docket);
		}
	}
}
