using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public static class ConsignmentRunSheetHelper
	{
		public static bool IsDepotInstruction(DtbConsignmentRunSheetInstruction instruction, ZString actionType)
		{
			var action = GetFirstAction(instruction, actionType);
			return action != null && action.ConsignmentAddress != null && action.ConsignmentAddress.IsDepot;
		}

		public static JobDocAddress GetInstructionAddress(DtbConsignmentRunSheetInstruction instruction, ZString actionType)
		{
			var action = GetFirstAction(instruction, actionType);
			return action?.ConsignmentAddress?.Address;
		}

		public static DtbConsignmentAction GetFirstAction(DtbConsignmentRunSheetInstruction instruction, ZString actionType)
		{
			var query = new ZQuery(DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction, instruction.PK);
			query.AddToFilter(DtbConsignmentActionSchema.LTA_ActionType, actionType);
			return instruction.Factory.LoadTop1<DtbConsignmentAction>(query);
		}

		public static DtbConsignment[] GetAllConsignments(DtbConsignmentRunSheetInstruction instruction, ZString actionType)
		{
			var query = new ZQuery(DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction, instruction.PK);
			query.AddToFilter(DtbConsignmentActionSchema.LTA_ActionType, actionType);
			var actions = instruction.Factory.Load<DtbConsignmentAction>(query);
			return actions.Select(a => a.ConsignmentAddress).Distinct().Select(a => a.Consignment).Distinct().ToArray();
		}

		public static DtbConsignment[] GetConsignmentsFromRunSheet(DtbConsignmentRunSheet runSheet)
		{
			return runSheet.RunSheetInstructions
								.SelectMany(instruction => instruction.Actions
								.Select(action => action.ConsignmentAddress?.Consignment))
								.Distinct()
								.ToArray();
		}
	}
}
