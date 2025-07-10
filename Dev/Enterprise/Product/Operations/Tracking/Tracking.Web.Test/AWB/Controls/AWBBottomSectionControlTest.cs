using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBBottomSectionControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBBottomSectionControl();
		}

		protected override List<Type> GetControlTypesToIgnore()
		{
			var result = base.GetControlTypesToIgnore();
			result.Add(typeof(AWBPrepaidCollectControl));
			result.Add(typeof(AWBShipperSignatureControl));
			result.Add(typeof(AWBCarrierSignatureControl));
			return result;
		}
	}
}
