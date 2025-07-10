using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Packing.Business
{
	class PkgPackageProcessHandlingInfo : ProcessHandlingInfo
	{
		public PkgPackageProcessHandlingInfo(PkgPackage package)
			: base(package)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			if (Package.IsDeleted || !Package.IsOuter) // only propagate from Outers
			{
				yield break;
			}

			var packageJob = Package.PackageJob;
			var stmALogParent = (packageJob != null) ? packageJob.ParentJob as IStmALogParent : null;
			if (stmALogParent == null)
			{
				yield break;
			}

			yield return new PropagationLink(stmALogParent, packageJob.Packages.Where(p => !p.IsContainer), "Outer Packages");
		}

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			var result = Enumerable.Empty<IBaseTrigger>();
			var parentJob = Package.PackageJob?.ParentJob;
			if (parentJob != null)
			{
				result = TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(parentJob, logBeingAdded, TriggerLineTypes.Codes.PkgPackage).Select(p => p.trigger);
			}
			return result;
		}

		PkgPackage Package
		{
			get { return (PkgPackage)LogParent; }
		}
	}
}

