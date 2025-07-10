using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;

using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	abstract class WhsOrderAndReceiveEventExportContextBuilder
	{
		protected WhsOrderAndReceiveEventExportContextBuilder(WhsDocket docket)
		{
			this.docket = Argument.NotNull(docket, nameof(WhsDocket) + " docket");
		}

		#region Docket

		protected WhsDocket Docket
		{
			get { return docket; }
		}

		readonly WhsDocket docket;

		#endregion

		#region AddWhsDocketContextValues

		internal void AddWhsDocketContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			AddSpecificContextValues(contextValues);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ClientReference, Docket.WD_CustomerReference);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportReference, Docket.WD_TransportReference);
			contextValues.AddDocketReferences(Docket);
		}

		protected abstract void AddSpecificContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues);

		#endregion
	}
}
