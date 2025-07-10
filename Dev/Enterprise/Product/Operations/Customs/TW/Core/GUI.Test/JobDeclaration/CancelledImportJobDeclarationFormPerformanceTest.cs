using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class CancelledImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
		protected override bool DeclarationIsCancelled => true;
		protected override Dictionary<string, int> TWDeleteExpectedHits => new Dictionary<string, int> { { StmNoteSchema.Constants.TableName, 10 } };
		protected override Dictionary<string, int> TWUniversalXMLExportExpectedHits => new Dictionary<string, int> { };
	}
}
