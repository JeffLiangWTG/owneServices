using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RelatableActivityTypeDefinitionTest : TestCaseWithFactory
	{
		public void TestGetNewCollection()
		{
			var info = new RelatableActivityTypeDefinition((NoResString)"", ModuleIDs.Communication, ControllerIDs.Communication, OrgSalesCallSchema.Constants.Prefix);
			AssertNotNull(info.GetNewCollection());
		}

		public void TestGetNewCollection_WithFilterBizOjWithErrors()
		{
			var definition = new RelatableActivityTypeDefinitionForTest((NoResString)"", ModuleIDs.Communication, ControllerIDs.Communication, OrgSalesCallSchema.Constants.Prefix);
			definition.GetNewCollection();
			AssertEquals("Error trying to get Filter for filterBizObj:FilterBusinessObjectWithProblems", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
