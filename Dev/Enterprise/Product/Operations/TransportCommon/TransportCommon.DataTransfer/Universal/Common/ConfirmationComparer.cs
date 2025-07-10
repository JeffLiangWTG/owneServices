using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public class ConfirmationComparer : IEqualityComparer<Confirmation>
	{
		#region Equals

		bool IEqualityComparer<Confirmation>.Equals(Confirmation c1, Confirmation c2)
		{
			return (c1.Demurrage == c2.Demurrage && c1.Distance == c2.Distance
				&& c1.DistanceUnit.GetNullableCodeAsUpperCase() == c2.DistanceUnit.GetNullableCodeAsUpperCase()
				&& c1.IsEmptyContainer == c2.IsEmptyContainer && c1.LegLink == c2.LegLink
				&& c1.ReceivedBy == c2.ReceivedBy && CompareDates(c1, c2)
				&& CompareReferencesAndDescriptions(c1, c2));
		}

		bool CompareDates(Confirmation c1, Confirmation c2)
		{
			return c1.ActualDate == c2.ActualDate && c1.ActualOutDate == c2.ActualOutDate
				&& c1.EstimatedDate == c2.EstimatedDate && c1.EstimatedOutDate == c2.EstimatedOutDate
				&& c1.RequiredFromDate == c2.RequiredFromDate && c1.RequiredToDate == c2.RequiredToDate;
		}

		bool CompareReferencesAndDescriptions(Confirmation c1, Confirmation c2)
		{
			return c1.DateDescription == c2.DateDescription
					&& c1.Reference == c2.Reference
					&& c1.ServiceInstruction == c2.ServiceInstruction;
		}

		#endregion

		int IEqualityComparer<Confirmation>.GetHashCode(Confirmation confirmation)
		{
			unchecked
			{
				return
					confirmation.ActualDate.GetHashCodeForNullableDataTypes() + confirmation.ActualOutDate.GetHashCodeForNullableDataTypes() +
					confirmation.DateDescription.GetHashCodeForNullableDataTypes() + confirmation.Demurrage.GetHashCodeForNullableDataTypes() +
					confirmation.Distance.GetHashCodeForNullableDataTypes() + confirmation.DistanceUnit.GetNullableCodeAsUpperCase().GetHashCodeForNullableDataTypes() +
					confirmation.EstimatedDate.GetHashCodeForNullableDataTypes() + confirmation.EstimatedOutDate.GetHashCodeForNullableDataTypes() +
					confirmation.IsEmptyContainer.GetHashCodeForNullableDataTypes() + confirmation.LegLink.GetHashCodeForNullableDataTypes() +
					confirmation.ReceivedBy.GetHashCodeForNullableDataTypes() + confirmation.Reference.GetHashCodeForNullableDataTypes() +
					confirmation.RequiredFromDate.GetHashCodeForNullableDataTypes() + confirmation.RequiredToDate.GetHashCodeForNullableDataTypes() +
					confirmation.ServiceInstruction.GetHashCodeForNullableDataTypes();
			}
		}
	}
}
