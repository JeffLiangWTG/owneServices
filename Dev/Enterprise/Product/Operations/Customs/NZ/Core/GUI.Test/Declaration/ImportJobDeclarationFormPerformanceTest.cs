using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	sealed class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;
		protected override Dictionary<string, int> NZFormMergeExpectedHits => new Dictionary<string, int> { { RefCusTaxOrFeeSchema.Constants.TableName, 64 } };
	}
}
