using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class ReleaseImportOrderActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		protected ReleaseImportOrderSettings Settings
		{
			get { return settings ?? (settings = new ReleaseImportOrderSettings()); }
		}

		ReleaseImportOrderSettings settings;
	}
}
