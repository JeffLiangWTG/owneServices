using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobDeclarationCartageIntegrationPostCMRWithLegacyData : BaseJobDeclarationCartageIntegrationTest
	{
		protected override ZString GetCMROrLegacy()
		{
			return "CMR";
		}
	}
}
