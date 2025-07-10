using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class TaxFrameworkAccTaxOverrideGroupController : AccTaxOverrideGroupControllerBase
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.TaxFrameworkAccTaxOverrideGroup; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TaxFrameworkAccTaxOverrideGroupForm((AccTaxOverrideGroup)businessEntity);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			return factory;
		}
	}
}
