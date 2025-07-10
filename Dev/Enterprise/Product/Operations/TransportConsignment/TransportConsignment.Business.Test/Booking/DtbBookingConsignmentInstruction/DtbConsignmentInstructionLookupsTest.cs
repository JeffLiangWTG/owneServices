using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbConsignmentInstructionLookupsTest : DtbTransportInstructionLookupsTest
	{
		#region Implementation

		protected override DtbTransportInstruction GetNewInstruction()
		{
			var consignment = Helper.CreateBookingConsignment();
			return helper.CreateInstruction(consignment, InstructionTypes.Codes.Multi);
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
