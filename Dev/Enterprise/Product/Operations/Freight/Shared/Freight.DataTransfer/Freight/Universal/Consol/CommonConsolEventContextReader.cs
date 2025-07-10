using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CommonConsolEventContextReader
	{
		internal CommonConsolEventContextReader(CommonConsol consol)
		{
			this.consol = Argument.NotNull(consol, "CommonConsol consol");
		}

		readonly CommonConsol consol;

		internal void AddConsolContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			var helper = new EventContextValuesHelper(consol.JK_TransportMode == Constants.TransportModes.Air, contextValues);
			helper.AddMasterBillNumberAndPortCodes(consol.JK_MasterBillNum, consol.LoadPort, consol.DischargePort);

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CarriersBookingReference, consol.JK_BookingReference);

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.AgentsReference, consol.JK_AgentsReference);

			if (consol.ShippingLine != null)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CarrierCode, consol.ShippingLine.SCACCode);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CarrierC1CCode, consol.ShippingLine.C1CCode);
			}

			consol.Numbers.AddAdditionalReferences(contextValues);
		}
	}
}
