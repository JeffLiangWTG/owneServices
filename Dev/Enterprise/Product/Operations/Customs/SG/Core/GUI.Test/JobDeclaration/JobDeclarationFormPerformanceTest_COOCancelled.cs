using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class JobDeclarationFormPerformanceTest_COOCancelled : JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZString MessageTypeForFormBashing => MessageTypeCodeList.Codes.COO;
		protected override bool DeclarationIsCancelled => true;
	}
}
