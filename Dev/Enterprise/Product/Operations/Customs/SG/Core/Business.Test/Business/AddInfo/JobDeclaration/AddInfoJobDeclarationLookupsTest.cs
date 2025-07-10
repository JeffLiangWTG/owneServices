using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobDeclarationLookupsTest : SGAddInfoLookupsTest
	{
		public void TestVessels()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertNotNull(declaration.Lookups.Vessels);
			AssertType<RefVesselCollection>("Vessels", declaration.Lookups.Vessels);
		}
	}
}
