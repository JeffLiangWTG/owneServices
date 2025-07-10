using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class JobDeclarationFormPerformanceTest_INP : JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZString MessageTypeForFormBashing => MessageTypeCodeList.Codes.INP;
	}
}
