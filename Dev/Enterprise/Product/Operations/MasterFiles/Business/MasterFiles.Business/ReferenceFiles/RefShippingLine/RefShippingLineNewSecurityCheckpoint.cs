using System;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefShippingLineNewSecurityCheckpoint : SecurityCheckpoint
	{
		public RefShippingLineNewSecurityCheckpoint()
			: base("ShippingLineNew", (NoResString)"Cannot create new Shipping Line", null, null, false)
		{
		}

		public override bool IsAllowed => false;

		public override void AddChild(SecurityCheckpoint child)
		{
			throw new NotSupportedException("AddChild() is not supported by RefShippingLineNewSecurityCheckpoint.");
		}

		public override void ShowError()
		{
			throw new NotSupportedException("ShowError() is not supported by RefShippingLineNewSecurityCheckpoint.");
		}

		public override MultilingualString ErrorMessageForNotAllowed
		{
			get { return ResString.GetMultilingualString("E6C69121-806C-42BD-8681-138B439BDAF1", "To register a new Carrier with CargoWise, please raise a CR8 Compliance, Ocean Carrier Integration request. Once this request has been processed the new record will be available for selection in your system."); }
		}
	}
}
