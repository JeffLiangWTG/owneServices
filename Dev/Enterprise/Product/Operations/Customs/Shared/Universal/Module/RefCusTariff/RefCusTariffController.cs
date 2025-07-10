using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class RefCusTariffController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.Universal.RefCusTariff;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.RefCusTariff;

		public override Type TypeOfTopLevelBusinessObject => typeof(TariffView);

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalTariffsDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalTariffsEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalTariffsNew;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalTariffsView;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCusTariffForm((TariffView)businessEntity);
		}
		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var result = (TariffView)base.LoadBusinessEntity(factory, sourceEntityPK);
			if (ParentModule is RefCusTariffModule module)
			{
				result.SetupDefaultFilterDataIfNeeded(module.TariffViewFilterData);
			}
			return result;
		}
	}
}
