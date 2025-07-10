using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ConsolDashboardController : ZSingletonController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID => ControllerIDs.ConsolPlanningBoard;

		public override ModuleIdentifier ModuleID => null;

		public override Type TypeOfTopLevelBusinessObject => typeof(ConsolDashboard);

		protected override BusinessObjectFactory GetNewFactory()
		{
			return new ReadOnlyBusinessObjectFactory() { NameForDebugging = "ConsolDashboard Factory" };
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ConsolDashboardForm(new ConsolDashboard(Factory));
		}

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ConsolPlanningBoard;

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
