using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class JobDeclarationFormPerformanceGeneralCountryTest : JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;
	}
}
