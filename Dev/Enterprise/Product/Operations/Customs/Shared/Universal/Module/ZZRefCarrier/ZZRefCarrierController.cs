using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCarrierController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.Universal.ZZRefCarrier;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCarrier;

		public override Type TypeOfTopLevelBusinessObject => typeof(ZZRefCarrierCombined);

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalCarriersDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalCarriersEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalCarriersNew;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalCarriersView;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCarrierForm((ZZRefCarrierCombined)businessEntity);
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return base.AlreadyDeletedOrIrreversiblyChangedMessageCore + System.Environment.NewLine + Res.GetString("92F9CBCA-E339-46FC-A7AA-FDC1430F0FEF", "If you have just created a user-editable copy of a system-generated record, please re-run your search query and try again.");
			}
		}
	}
}
