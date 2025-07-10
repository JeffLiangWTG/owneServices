using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.TransportConsignment.Business
{
	/// Tested via DtbAddressPoint and DtbConsignmentRunSheetInstruction.
	public static class DtbConsignmentConfirmationTotalsHelper
	{
		// flags

		#region GetSpecialInstructionsExist

		public static ZBool GetSpecialInstructionsExist(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return confirmations != null && confirmations.Any(c => c.Instruction.SpecialInstructionExists);
		}

		#endregion

		#region GetServicesExist

		public static ZBool GetServicesExist(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return confirmations != null && confirmations.Any(c => c.Instruction.ServiceExists);
		}

		#endregion

		#region GetIsHazardous

		public static ZBool GetIsHazardous(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return confirmations != null && confirmations.Any(c => c.IsHazardous);
		}

		#endregion

		#region GetRequiresRefrigeration

		public static ZBool GetRequiresRefrigeration(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return confirmations != null && confirmations.Any(c => c.RequiresRefrigeration);
		}

		#endregion

		// totals

		#region GetCompletePackageSummary

		public static ZString GetCompleteBookedPickupPackageSummary(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return (confirmations == null) ? ZString.Empty : PackageTotals.GetCompletePackageSummary(confirmations.Where(c => c.IsPickUp).Select(c => c.BookedPickupPackageList));
		}

		public static ZString GetCompletePickupPackageSummary(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return confirmations.GetCompletePackageSummary(c => c.IsPickUp);
		}

		public static ZString GetCompleteDeliveryPackageSummary(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return confirmations.GetCompletePackageSummary(c => c.IsDelivery);
		}

		static ZString GetCompletePackageSummary(this IEnumerable<DtbConsignmentConfirmation> confirmations, Func<DtbConsignmentConfirmation, bool> isPickupOrDelivery)
		{
			return (confirmations == null) ? ZString.Empty : PackageTotals.GetCompletePackageSummary(confirmations.Where(isPickupOrDelivery).SelectMany(c => c.PackageSummaryList));
		}

		#endregion

		#region GetTotalWeight

		public static ZDecimal GetTotalPickupWeight(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return GetTotalWeight(confirmations, c => c.IsPickUp);
		}

		public static ZDecimal GetTotalDeliveryWeight(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return GetTotalWeight(confirmations, c => c.IsDelivery);
		}

		static ZDecimal GetTotalWeight(IEnumerable<DtbConsignmentConfirmation> confirmations, Func<DtbConsignmentConfirmation, bool> isPickupOrDelivery)
		{
			return (confirmations == null) ? 0m : confirmations.Where(isPickupOrDelivery).Sum(c => c.TotalWeight);
		}

		#endregion

		#region GetTotalVolume

		public static ZDecimal GetTotalPickupVolume(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return GetTotalVolume(confirmations, c => c.IsPickUp);
		}

		public static ZDecimal GetTotalDeliveryVolume(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return GetTotalVolume(confirmations, c => c.IsDelivery);
		}

		static ZDecimal GetTotalVolume(IEnumerable<DtbConsignmentConfirmation> confirmations, Func<DtbConsignmentConfirmation, bool> isPickupOrDelivery)
		{
			return (confirmations == null) ? 0m : confirmations.Where(isPickupOrDelivery).Sum(c => c.TotalVolume);
		}

		#endregion

		#region GetTotalPackages

		public static ZInt GetTotalPickupPackages(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return GetTotalPackages(confirmations, c => c.IsPickUp);
		}

		public static ZInt GetTotalDeliveryPackages(this IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return GetTotalPackages(confirmations, c => c.IsDelivery);
		}

		static ZInt GetTotalPackages(IEnumerable<DtbConsignmentConfirmation> confirmations, Func<DtbConsignmentConfirmation, bool> isPickupOrDelivery)
		{
			return (confirmations == null) ? 0 : confirmations.Where(isPickupOrDelivery).Sum(c => c.TotalPackages);
		}

		#endregion
	}
}
