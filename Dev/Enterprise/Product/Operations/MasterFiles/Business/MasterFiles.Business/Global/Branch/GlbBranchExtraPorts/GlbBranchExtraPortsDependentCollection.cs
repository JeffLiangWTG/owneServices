using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchExtraPortsDependentCollection : DependentBusinessObjectCollection<GlbBranchExtraPorts, GlbBranch>
	{
		public GlbBranchExtraPortsDependentCollection(GlbBranch parent)
			: base(parent)
		{
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			Master.DefaultPorts.MarkAsNeedingValidation();
			base.OnRemoved(bizO);
		}
	}
}
