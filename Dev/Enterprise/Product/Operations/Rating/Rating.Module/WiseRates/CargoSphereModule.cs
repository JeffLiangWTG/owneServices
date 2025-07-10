namespace Enterprise.Rating.Module
{
	using Enterprise.Environment;
	using Enterprise.Licensing;
	using Enterprise.Rating.GUI;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Modules;

	public class CargoSphereModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WiseRatesCargoSphere;

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				var checkpointRateSearch = Env.Security.WiseRatesCargoSphereRateSearch;
				if (checkpointRateSearch.IsAllowed)
				{
					return checkpointRateSearch;
				}

				var checkpointSUDS = Env.Security.WiseRatesCargoSphereContractManagement;
				if (checkpointSUDS.IsAllowed)
				{
					return checkpointSUDS;
				}

				return checkpointRateSearch;
			}
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override System.Uri Url => null;

		public override void Show()
		{
			var rateProviderAccess = new RateProviderAccessAndMenu(null);

			if (SecurityCheckpoint.Equals(Env.Security.WiseRatesCargoSphereRateSearch))
			{
				rateProviderAccess.CargoSphereRateSearch();
			}
			else
			{
				rateProviderAccess.CargoSphereContractManagement();
			}
		}
	}
}

