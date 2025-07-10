using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusRulingController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.Universal.ZZRefCusRuling;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCusRuling;

		public override Type TypeOfTopLevelBusinessObject => typeof(ZZRefCusRulingCombined);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalCodesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalCodesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalCodesEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalCodesDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCusRulingForm((ZZRefCusRulingCombined)businessEntity);
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return base.AlreadyDeletedOrIrreversiblyChangedMessageCore + System.Environment.NewLine + Res.GetString("{B836499B-19AE-400B-B9D6-D718EB4682EE}", "If you have just created a user-editable copy of a system-generated record, please re-run your search query and try again.");
			}
		}
	}
}
