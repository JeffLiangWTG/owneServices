using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class InBondNumberController : NumberController
	{
		public override ControllerID ID => ControllerIDs.Customs.US.InBondNumber;

		public override ModuleIdentifier ModuleID => null;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			IZForm result = null;
			InBondNumberSetting setting = null;

			if (InBondNumberGenerator.IsInBondNumberRangeByCompany(GlbCompany.CurrentCompany.PK))
			{
				setting = InBondNumberSetting.New(GlbCompany.CurrentCompany);
			}
			else
			{
				setting = InBondNumberSetting.New(GlbBranch.CurrentBranch);
			}

			if (setting != null)
			{
				result = new InBondNumberSettingForBranchSpecificForm(setting);
			}

			return result;
		}

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.InBondNumber;
	}
}
