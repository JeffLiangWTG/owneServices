using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ConsolCostApportionmentHelper
	{
		/// <summary>
		/// Returns values that are possibly broken. See MatchByContainerCode
		///
		/// This function used to check the basis and the job and return true
		/// if there was a container in the job that is not present in the basis.
		///
		/// Instead, now it returns:
		/// True:  if the job matched rate does not have a specific container in the
		///        RateEntry (ie: LCL)
		/// False: In most other cases, it first checks for job containers
		///        that match the basis based on container code; and then out of those
		///        it then checks if any remain that have a different Freight-Rate class.
		///        But why would two containers - with the SAME RC_Code have a
		///        different Freight-Rate class?
		///        
		/// </summary>
		public static bool ContainersSetupHasChanged(IEnumerable<IJobPaymentBasis> paymentBases, ForwardingConsol consol)
		{
			if (paymentBases != null && consol != null)
			{
				var basisContainers = GetContainerInfosFromBasis(paymentBases);

				var containersMatchedByContainerCode =
					consol.Containers
						.Cast<ForwardingContainer>()
						.Where(consolContainer => basisContainers.Any(basisContainer => MatchByContainerCode(consolContainer, basisContainer)))
						.Select(c => new ContainerInfo(c.RefContainer.RC_Code, c.JC_ContainerCount, c.JC_ContainerNum));

				return containersMatchedByContainerCode.Any(cc => !basisContainers.Any(cr => cr.MatchByFreightRateClass(cc)));
			}

			return false;
		}

		static bool MatchByContainerCode(ForwardingContainer forwardingContainer, ContainerInfo basisContainer)
		{
			return
				forwardingContainer.RefContainer != null &&
				(
					// below RefContainer null check was to fix as NRE WI00203347
					// it possibly is incorrect as it means the warning is shown in all jobs
					// where the matched rate does not have a specific container in the RateEntry
					// and the user cant do anything to make the warning go away
					// the second warning in CheckCapacityPerContainerApportionmentIfRequired
					basisContainer.RefContainer == null ||
					// below checks introduced in WI00189226_12 and possibly broke ContainersSetupHasChanged
					// since it no longer accurately detects a container type changing between basis and
					// job because it only looks at those which are the same, and then later looks for those
					// that are different. Resulting in the second warning in
					// CheckCapacityPerContainerApportionmentIfRequired rarely being visible
					basisContainer.RefContainer.RC_Code == forwardingContainer.RefContainer.RC_Code
				);
		}

		static IEnumerable<ContainerInfo> GetContainerInfosFromBasis(IEnumerable<IJobPaymentBasis> paymentBases)
			=> GetContainersFromBasis(paymentBases, (containerBasis, containerCode) =>
			{
				return
					new ContainerInfo
					(
						containerTypeCode: containerCode,
						containerCount: containerBasis.PBS_ChargeableAmount.ToZInt(),
						containerNumber: containerBasis.PBS_ChargeableDescription
					);
			});

		internal static IEnumerable<ContainerResult> GetContainerResultsFromBasis(IEnumerable<IJobPaymentBasis> paymentBases)
			=> GetContainersFromBasis(paymentBases, (containerBasis, containerCode) =>
			{
				return
					new ContainerResult
					(
						containerTypeCode: containerCode,
						containerNumber: containerBasis.PBS_ChargeableDescription,
						result: containerBasis.PBS_ChargeableAmount * containerBasis.PBS_PerUnitRate
					);
			});

		static IEnumerable<T> GetContainersFromBasis<T>(IEnumerable<IJobPaymentBasis> paymentBases, Func<JobPaymentBasis, string, T> extractDataFunction)
		{
			if (paymentBases == null)
			{
				return null;
			}

			var containersUsedForRating = new List<T>();
			var containerBasisGroups =
				paymentBases
					.Cast<JobPaymentBasis>()
					.Where(pb => pb.PBS_RateUnit == QuantityUnit.CN)
					.GroupBy(pb => pb.PBS_ChargeableUnit);

			foreach (var containerBasisGroup in containerBasisGroups)
			{
				// The key comes from PBS_ChargeableUnit which can be either
				// 'CN' or a container code. Need to keep only the container
				// codes
				var containerCode =
					containerBasisGroup.Key == QuantityUnit.CN
					? ZString.Empty
					: containerBasisGroup.Key;

				foreach (var containerBasis in containerBasisGroup)
				{
					containersUsedForRating.Add(extractDataFunction(containerBasis, containerCode));
				}
			}

			return containersUsedForRating;
		}
	}
}
