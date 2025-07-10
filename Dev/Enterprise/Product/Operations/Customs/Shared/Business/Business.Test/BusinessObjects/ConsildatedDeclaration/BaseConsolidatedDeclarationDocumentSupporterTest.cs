using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseConsolidatedDeclarationDocumentSupporter))]
	public class BaseConsolidatedDeclarationDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			AssertEquals("ConsolidatedDeclaration has a business context of ConsolidatedEntry", BusinessContext.ConsolidatedEntry, consolidatedDeclaration.DocumentSupporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			AssertEquals("Should have CustomsDeclarationCustomiseDocument Security check point", Env.Security.CustomsDeclarationCustomiseDocument, consolidatedDeclaration.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContexts()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			AssertEquals("Enterprise.Core.Constants.DataContext.CusEntryHeader is Supported", true, consolidatedDeclaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusEntryHeader)));
		}

		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			AssertEquals("Entry Header cannot be found.", consolidatedDeclaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusEntryHeader), null));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
		}
	}
}
