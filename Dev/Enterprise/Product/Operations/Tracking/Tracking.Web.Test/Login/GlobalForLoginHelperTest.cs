namespace Enterprise.Tracking.Web.Testing
{
	sealed class GlobalForLoginHelperTest : TestGlobal
	{
		public string DefaultPageForTesting { get; set; }

		public override string DefaultPage => !string.IsNullOrEmpty(DefaultPageForTesting) ? DefaultPageForTesting : base.DefaultPage;
	}
}
