using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI.OnlineSailingSchedules;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class OnlineSailingSchedulesController : ZSingletonController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OnlineSchedulesForm(new OnlineSchedules(Factory, new RoutesProvider(Factory)));
		}
		public override ControllerID ID => ControllerIDs.OnlineSailingSchedules;
		public override ModuleIdentifier ModuleID => null;
		public override Type TypeOfTopLevelBusinessObject => typeof(OnlineSchedules);
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.OnlineSailingSchedules;
		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}
		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}
	}
}
