using CargoWise.Types;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class JobDeclarationFormPerformance_ImportTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}
}
