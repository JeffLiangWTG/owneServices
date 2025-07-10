using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.Freight.Forwarding.Routing.S8.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Routing.S8.Module
{
	public class RealTimeRoutingController : ZSingletonController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.RoutingRealTimeLookup; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RealTimeRoutingForm((RoutingManager)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RoutingLookups; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RoutingManager); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new RoutingManager(Factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RoutingLookups; }
		}
	}
}
