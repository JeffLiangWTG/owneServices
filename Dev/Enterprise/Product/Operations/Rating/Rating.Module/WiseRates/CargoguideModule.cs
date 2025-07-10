namespace Enterprise.Rating.Module
{
	using Enterprise.Environment;
	using Enterprise.Licensing;
	using Enterprise.Rating.GUI;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Modules;

	public class CargoguideModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WiseRatesCargoguide;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WiseRatesCargoguideRateSearch;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override System.Uri Url => null;

		public override void Show()
		{
			var rateProviderAccess = new RateProviderAccessAndMenu(null);
			rateProviderAccess.CargoguideRateSearch();
		}
	}
}
