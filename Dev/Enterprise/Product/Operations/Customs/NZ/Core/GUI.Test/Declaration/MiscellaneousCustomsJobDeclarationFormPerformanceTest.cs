using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	sealed class MiscellaneousCustomsJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.MiscellaneousCustoms;
		protected override Dictionary<string, int> NZUniversalXMLExportExpectedHits => new Dictionary<string, int> { { JobHeaderSchema.Constants.TableName, 5 } };
	}
}
