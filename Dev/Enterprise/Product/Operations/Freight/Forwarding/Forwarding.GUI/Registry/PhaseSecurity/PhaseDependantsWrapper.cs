using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class PhaseDependantsWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PhaseDependantsWrapper(PhaseDependantsProvider dependantsProvider, IEnumerable<IPhaseDependant> dependants)
		{
			DependantsProvider = dependantsProvider;
			OriginalDependants = dependants;
		}

		public readonly PhaseDependantsProvider DependantsProvider;
		public readonly IEnumerable<IPhaseDependant> OriginalDependants;

		[BusinessObjectTestExclude]
		public IEnumerable<IPhaseDependant> SelectedDependants { get; set; }

		public bool HasChangesInDependants
		{
			get
			{
				if (OriginalDependants != null && SelectedDependants != null)
				{
					var originalReadOnlyDependants = OriginalDependants.Where(x => x.IsReadOnly);
					var originalMandatoryDependants = OriginalDependants.Where(x => x.IsMandatory);

					var selectedReadOnlyDependants = SelectedDependants.Where(x => x.IsReadOnly);
					var selectedMandatoryDependants = SelectedDependants.Where(x => x.IsMandatory);

					return originalReadOnlyDependants.Except(selectedReadOnlyDependants).Any() || selectedReadOnlyDependants.Except(originalReadOnlyDependants).Any()
						|| originalMandatoryDependants.Except(selectedMandatoryDependants).Any() || selectedMandatoryDependants.Except(originalMandatoryDependants).Any();
				}

				return false;
			}
		}
	}
}
