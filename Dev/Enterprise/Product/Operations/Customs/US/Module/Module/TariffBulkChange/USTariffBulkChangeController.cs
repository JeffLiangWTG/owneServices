using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USTariffBulkChangeController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var factory = new BusinessObjectFactory();
			var tariffBulkChange = new USTariffBulkChange(factory);
			return new USTariffBulkChangeForm(tariffBulkChange);
		}

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.TariffBulkChange;

		public override ControllerID ID => ControllerIDs.Customs.US.USTariffBulkChange;

		public override ModuleIdentifier ModuleID => null;

		public override Type TypeOfTopLevelBusinessObject => typeof(USTariffBulkChange);

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => new USTariffBulkChange(Factory);
	}
}
