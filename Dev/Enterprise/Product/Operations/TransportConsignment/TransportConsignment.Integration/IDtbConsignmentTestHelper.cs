using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportConsignment.Integration
{
	public interface IDtbConsignmentTestHelper
	{
		BusinessObject CreateConsignment();
		BusinessObject CreateInstruction(BusinessObject consignment, ZString instructionType);
		BusinessObject CreateConfirmation(BusinessObject instruction, ZString confirmationType);
		BusinessObject CreateRunSheet(string runSheetNumber);
		BusinessObject CreateRunSheetInstruction(BusinessObject confirmation);
	}
}
