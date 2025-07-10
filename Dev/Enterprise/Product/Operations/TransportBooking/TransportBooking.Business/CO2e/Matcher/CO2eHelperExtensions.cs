using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.TransportBookings.Business
{
	public static class CO2eHelperExtensions
	{
		public static bool IsEmptyContainer(this DtbBookingInstructionPkgDivot divot, string type = default)
		{
			if (!divot.IsContainerised)
			{
				return false;
			}
			if (!divot.Confirmations.Any())
			{
				return divot.Instruction.IsEmptyYard;
			}

			if (divot.Instruction.IsMulti)
			{
				return divot.Confirmations.Any(c => c.KK_ConfirmationType == type && c.IsEmptyContainer);
			}
			else
			{
				return divot.Confirmations.Any(c => c.IsEmptyContainer);
			}
		}

		public static List<ICO2eMatchAction> GetActionsIncludingInner(this CO2eMatchResult result)
		{
			var actions = new List<ICO2eMatchAction>();
			foreach (var action in result.Actions)
			{
				actions.Add(action);
				actions.AddRange(action.InnerPackagesActions);
			}
			return actions;
		}

		public static bool HasPackageBeenLoadedAlready(this CO2eMatchResult result, PkgPackage package, DtbBookingInstruction currentInstruction)
		{
			return result.GetActionsIncludingInner().Any(action => action.IsPickup && action.GetDivot().Instruction.PKEquals(currentInstruction) && action.Package.PKEquals(package));
		}

		public static bool IsPackageLoadedAsInner(this CO2eMatchResult result, PkgPackage package)
		{
			var loadActions = result.Actions.SelectMany(action => action.InnerPackagesActions).Where(action => action.IsPickup && action.Package.PKEquals(package));
			var latestInnerLoad = loadActions.LastOrDefault();
			if (latestInnerLoad == null)
			{
				return false;
			}

			var allActions = result.GetActionsIncludingInner();
			var lastInnerLoadIndex = allActions.IndexOf(latestInnerLoad);
			for (var idx = lastInnerLoadIndex + 1; idx < allActions.Count; idx++)
			{
				var action = allActions[idx];
				if (!action.IsPickup && action.Package.PKEquals(package))
				{
					return false;
				}
			}
			return true;
		}

		public static DtbBookingInstructionPkgDivot GetDivot(this ICO2eMatchAction matchedInstruction)
		{
			return matchedInstruction switch
			{
				CO2eLoadAction loadAction => loadAction.LoadBy,
				CO2eUnLoadAction unLoadAction => unLoadAction.UnLoadBy,
				_ => null
			};
		}
	}
}
