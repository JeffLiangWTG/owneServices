using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBControl();
		}

		protected override List<Type> GetControlTypesToIgnore()
		{
			var result = base.GetControlTypesToIgnore();
			result.Add(typeof(AWBPrepaidCollectControl));
			result.Add(typeof(AWBShipperSignatureControl));
			result.Add(typeof(AWBHeaderControl));
			result.Add(typeof(AWBCarrierSignatureControl));
			result.Add(typeof(AWBAddressControl));
			result.Add(typeof(AWBIssuedControl));
			result.Add(typeof(AWBCarrierControl));
			result.Add(typeof(AWBShippingReferenceControl));
			result.Add(typeof(AWBRoutingControl));
			result.Add(typeof(AWBDeclaredValuesControl));
			result.Add(typeof(AWBDestinationControl));
			result.Add(typeof(AWBRequestedFlightControl));
			result.Add(typeof(AWBHandlingInfoControl));
			result.Add(typeof(AWBRateLinesControl));
			result.Add(typeof(AWBBottomSectionControl));
			return result;
		}
	}
}
