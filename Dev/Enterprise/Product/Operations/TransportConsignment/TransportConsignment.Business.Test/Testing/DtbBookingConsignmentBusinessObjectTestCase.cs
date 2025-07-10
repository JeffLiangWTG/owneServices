using Enterprise.TransportCommon.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(EnterpriseBusinessObject))]
	public abstract class DtbBookingConsignmentBusinessObjectTestCase : DtbTransportBusinessObjectTestCase
	{
		#region Helper

		protected new TransportBookingConsignmentTestHelper Helper
		{
			get { return (TransportBookingConsignmentTestHelper)base.Helper; }
		}

		protected override TransportCommonTestHelper GetNewTestHelper()
		{
			return new TransportBookingConsignmentTestHelper(Factory);
		}

		#endregion
	}
}
