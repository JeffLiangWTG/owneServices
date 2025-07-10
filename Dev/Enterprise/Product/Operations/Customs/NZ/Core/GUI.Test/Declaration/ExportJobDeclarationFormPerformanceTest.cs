using CargoWise.Types;
using Enterprise.Customs.NZ.Business;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	sealed class ExportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;
	}
}
