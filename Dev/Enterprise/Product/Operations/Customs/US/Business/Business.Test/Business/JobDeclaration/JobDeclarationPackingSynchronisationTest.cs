using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationPackingSynchronisationTest : Customs.Business.Testing.PackingSynchronisationTest
	{
		protected override BaseJobDeclaration GetDeclarationPackingRelevant()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JE_TransportMode = TransportTypeList.Codes.Sea;
			return result;
		}
	}
}
