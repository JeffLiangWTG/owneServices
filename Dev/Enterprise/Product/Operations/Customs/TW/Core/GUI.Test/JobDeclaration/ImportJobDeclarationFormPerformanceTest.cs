using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
		Dictionary<string, int> TWImportUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ RefUNLOCOSchema.Constants.TableName, 6 },
			{ StmNoteSchema.Constants.TableName, 10 }
		};
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(TWImportUniversalXMLImportUpdateExpectedHits, TWUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> TWUniversalXMLExportExpectedHits => new Dictionary<string, int>();
	}
}
