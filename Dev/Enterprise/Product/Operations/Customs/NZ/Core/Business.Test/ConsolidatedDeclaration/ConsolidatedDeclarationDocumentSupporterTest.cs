using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineIntegration;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Test
{
	[TestedType(typeof(ConsolidatedDeclarationDocumentSupporter))]
	sealed class ConsolidatedDeclarationDocumentSupporterTest : BaseConsolidatedDeclarationDocumentSupporterTest
	{
		public void TestGetSupportedDataContexts()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var supporter = new ConsolidatedDeclarationDocumentSupporter(consolidatedDeclaration);
			Assert("Core.Constants.DataContext.Declaration is Supported", supporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.Declaration)));
		}

		public void TestGetFilterValueMSGBKRCTY()
		{
			var consolidatedDeclaration = (ConsolidatedDeclaration)GetDocumentSupportableBusinessObject();
			Assertion.AssertEquals("For filter 'MSGBKRCTY' result is 'IMPNZ'", "IMPNZ", consolidatedDeclaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
		}

		public void TestGetDocBusinessObjects()
		{
			var consolidatedDeclaration = (ConsolidatedDeclaration)GetDocumentSupportableBusinessObject();
			var result = consolidatedDeclaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertEquals("Docwrapper count = customs entry header count", 1, result.Length);
			var warpper = result[0];
			AssertEquals("Docwrapper for data context Declaration is DocDocDeclaration", "Enterprise.DocumentWrappers.Customs.NZ.DocDeclaration", warpper.GetType().ToString());
			AssertEquals("Docwrapper.BusinessObjectForPrintJob should implement ISourceIdentifierProvider and SourceIdentifier equal to ConsolidatedDeclaration's pk",
				consolidatedDeclaration.PK,
				(warpper.BusinessObjectForPrintJob as ISourceIdentifierProvider)?.SourceIdentifier);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryHeader = leadDeclaration.CusEntryHeader;
			entryHeader.EntryNumber = "AAA";
			Factory.Save();
			return consolidatedDeclaration;
		}
	}
}
