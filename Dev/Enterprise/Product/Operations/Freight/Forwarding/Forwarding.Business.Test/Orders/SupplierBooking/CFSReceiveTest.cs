using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(CFSReceive))]
	public class CFSReceiveTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.JobSupplierBooking);
			}
		}
	}
}
