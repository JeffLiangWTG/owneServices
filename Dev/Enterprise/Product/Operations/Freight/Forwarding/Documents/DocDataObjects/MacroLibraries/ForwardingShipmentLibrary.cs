using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ForwardingShipmentLibrary : MacroLibrary
	{
		public ForwardingShipmentLibrary(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, "shipment");
		}

		readonly ForwardingShipment shipment;

		#region SuppressResourceStringsCheckRegion

		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<string>>(
					"GetChargesDisplayOption",
					"Get charges display option code.",
					() => shipment.JS_HBLAWBChargesDisplay);
			}
		}

		#endregion
	}
}
