using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbBranchCredential : GlbExternalPasswordWithPasswordType
	{
		public GlbBranchCredential(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_GB = GlbBranch.CurrentBranch.PK;
			GP_GS = ZGuid.Empty;
		}
	}
}
