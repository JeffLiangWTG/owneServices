using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageLegPlannerController : ZPopupController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public CartageLegPlannerController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CartageLegPlanner; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CartageLegPlannerForm(Factory);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.LocalTransportLegPlanner; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			BusinessObjectFactory moduleFactory = base.GetNewFactory();
			//DocumentSupport
			CommonCartageBehaviorStrategyProvider.SetProvider(moduleFactory, new CartageBehaviorStrategyProvider()); //DocumentSupport
			return moduleFactory;
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}
	}
}
