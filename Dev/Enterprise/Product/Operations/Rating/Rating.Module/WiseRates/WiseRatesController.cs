using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class WiseRatesController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (!DataRegistryRating.Instance.IsLicenseForRatesServiceValid(out string reason))
			{
				Globals.Message.Show(reason);
				return null;
			}

			if (!DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(out reason))
			{
				Globals.Message.Show(reason);
				return null;
			}

			var logger = new ElementaryLogger();
			var wiseRatesProvider = new WiseRatesProvider(Factory, ObjectFactory.Get<IWiseRatesClientFactory>(), logger);
			return new WiseRatesForm(new WiseRatingHeaderView(wiseRatesProvider, Factory, logger));
		}

		public override ControllerID ID => ControllerIDs.WiseRates;
		public override ModuleIdentifier ModuleID => null;
		public override Type TypeOfTopLevelBusinessObject => typeof(WiseRatingHeaderView);
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WiseRatesSearch;

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}
	}
}