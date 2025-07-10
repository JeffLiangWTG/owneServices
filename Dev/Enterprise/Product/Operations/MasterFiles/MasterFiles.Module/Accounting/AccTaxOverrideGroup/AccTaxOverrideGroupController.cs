using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccTaxOverrideGroupController : AccTaxOverrideGroupControllerBase
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.AccTaxOverrideGroup; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccTaxOverrideGroupForm((AccTaxOverrideGroup)businessEntity);
		}
	}
}
