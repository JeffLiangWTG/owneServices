using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	sealed class ExciseJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Excise;
		protected override Dictionary<string, int> NZFormMergeExpectedHits => new Dictionary<string, int> { { RefCusTaxOrFeeSchema.Constants.TableName, 62 } };
		protected override Dictionary<string, int> NZUniversalXMLExportExpectedHits => new Dictionary<string, int> { { JobHeaderSchema.Constants.TableName, 5 } };
	}
}
