using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class JobDeclarationFormPerformance_ImportCancelledTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override bool DeclarationIsCancelled => true;

		protected override Dictionary<string, int> ZADeleteExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 8 },
			{ StmNoteSchema.Constants.TableName, 5 }
		};
	}
}
