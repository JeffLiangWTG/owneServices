using System.Collections.Generic;
using System.Linq;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public static class InstructionExtensions
	{
		#region GetConfirmations

		public static IEnumerable<Confirmation> GetConfirmations(this Instruction instruction)
		{
			var containerConfirmations = instruction.InstructionContainerLinkCollection != null
				? instruction.InstructionContainerLinkCollection.SelectMany(cl => cl.ConfirmationCollection ?? Enumerable.Empty<Confirmation>())
				: Enumerable.Empty<Confirmation>();

			var packageConfirmations = instruction.InstructionPackingLineLinkCollection != null
				? instruction.InstructionPackingLineLinkCollection.SelectMany(pl => pl.ConfirmationCollection ?? Enumerable.Empty<Confirmation>())
				: Enumerable.Empty<Confirmation>();

			var allConfirmationsIncludingDupes = containerConfirmations.Union(packageConfirmations);
			return allConfirmationsIncludingDupes.Distinct(new ConfirmationComparer()).ToArray();
		}

		#endregion

		#region GetConfirmationsThatApplyToAllPackagesOnly

		public static IEnumerable<Confirmation> GetConfirmationsThatApplyToAllPackagesOnly(this Instruction instruction, PkgPackageJobDataObjectReader packageJobReader)
		{
			return new InstructionsHelper(instruction, packageJobReader).GetCommonConfirmationsInContainerAndPackingLineDivots();
		}

		class InstructionsHelper
		{
			public InstructionsHelper(Instruction instruction, PkgPackageJobDataObjectReader packageJobReader)
			{
				Instruction = instruction;
				PackageJobReader = packageJobReader;
			}

			readonly Instruction Instruction;
			readonly PkgPackageJobDataObjectReader PackageJobReader;

			internal IEnumerable<Confirmation> GetCommonConfirmationsInContainerAndPackingLineDivots()
			{
				var confirmationsInAllContainerDivots = FindCommonConfirmationsInContainerDivots(Instruction.InstructionContainerLinkCollection);
				return FindCommonConfirmationsInPackingLineDivots(confirmationsInAllContainerDivots, Instruction.InstructionPackingLineLinkCollection);
			}

			IEnumerable<Confirmation> FindCommonConfirmationsInContainerDivots(IEnumerable<InstructionContainerLink> containerLinks)
			{
				IEnumerable<Confirmation> result = null;

				if (containerLinks != null)
				{
					var validContainerLinks = containerLinks.Where(IsValidContainerLink);
					if (validContainerLinks.Any())
					{
						var firstValidContainerLink = validContainerLinks.First();
						result = FindCommonConfirmationsOnAllDivots(firstValidContainerLink.ConfirmationCollection.Where(c => c.Quantity == firstValidContainerLink.Quantity), validContainerLinks.Skip(1).ToArray());
					}
				}

				return result ?? System.Array.Empty<Confirmation>();
			}

			IEnumerable<Confirmation> FindCommonConfirmationsOnAllDivots(IEnumerable<Confirmation> commonConfirmations, IConfirmationParentDivot[] packageDivots)
			{
				var result = new List<Confirmation>(commonConfirmations);

				foreach (var divot in packageDivots)
				{
					if (result.Count == 0)
					{
						return null;
					}

					result.RemoveAll(confirmationToMatch => !divot.ConfirmationCollection.Where(c => c.Quantity == divot.Quantity).Contains(confirmationToMatch, new ConfirmationComparer()));
				}

				return result;
			}

			IEnumerable<Confirmation> FindCommonConfirmationsInPackingLineDivots(IEnumerable<Confirmation> commonContainerConfirmations, IEnumerable<InstructionPackingLineLink> packingLineLinks)
			{
				IEnumerable<Confirmation> result = commonContainerConfirmations;

				if (commonContainerConfirmations != null && packingLineLinks != null && packingLineLinks.Any())
				{
					// if there is only one packing line link it might be the fake one so keep it in the list.
					var validPackingLineLinks = packingLineLinks.Count() > 1
						? packingLineLinks.Where(IsValidPackingLink).ToArray()
						: packingLineLinks.Where(l => l.ConfirmationCollection != null && l.ConfirmationCollection.Any()).ToArray();

					if (validPackingLineLinks.Any())
					{
						var firstValidPackingLineLink = validPackingLineLinks.First();
						var commonConfirmations = result != null && result.Any()
							? result.ToList()
							: new List<Confirmation>(firstValidPackingLineLink.ConfirmationCollection.Where(l => l.Quantity == firstValidPackingLineLink.Quantity));

						var packageDivots = result != null && result.Any()
							? validPackingLineLinks.ToArray()
							: validPackingLineLinks.Where(d => d != firstValidPackingLineLink).ToArray();

						result = FindCommonConfirmationsOnAllDivots(commonConfirmations, packageDivots);
					}
				}

				return result;
			}

			bool IsValidPackingLink(InstructionPackingLineLink packingLineLink)
			{
				return packingLineLink.ConfirmationCollection != null
					&& packingLineLink.ConfirmationCollection.Any()
					&& packingLineLink.PackingLineLink.HasValue
					&& PackageJobReader.PackageLinks.ContainsKey(packingLineLink.PackingLineLink.Value);
			}

			bool IsValidContainerLink(InstructionContainerLink containerDivot)
			{
				// Containers will not contain the fake dummy divot when there are no divots on the instruction, so ignore any divots with no valid Link.
				return containerDivot.ConfirmationCollection != null
					&& containerDivot.ConfirmationCollection.Any()
					&& containerDivot.ContainerLink.HasValue
					&& PackageJobReader.PackageContainerLinks.ContainsKey(containerDivot.ContainerLink.Value);
			}
		}

		#endregion
	}
}
