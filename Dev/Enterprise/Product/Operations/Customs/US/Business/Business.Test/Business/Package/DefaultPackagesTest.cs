using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DefaultPackagesTest : Customs.Business.Testing.DefaultPackagesTest
	{
		protected override BaseJobDeclaration GetDeclarationPackageRelevant()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			return declaration;
		}
	}
}
