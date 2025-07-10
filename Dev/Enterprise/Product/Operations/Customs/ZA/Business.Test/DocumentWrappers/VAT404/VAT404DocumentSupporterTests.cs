using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VAT404DocumentSupporter))]
	sealed class VAT404DocumentSupporterTests : DocumentSupporterTest
	{
		public void TestShowReasonForNotPrinting()
		{
			var entry = VAT404TestHelper.SetupEntry(Factory, "LRN1");
			VAT404TestHelper.AddPayInfo(entry, "VAT", 12m, "RCP01", ZDateTime.Today, ZDateTime.Today);
			var tester = new VAT404DocumentInstruction(Factory);
			tester.PerformSearch();
			var document = (VAT404Document)tester.VAT404Documents.FirstOrDefault();
			AssertEquals(false, document.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		public void TestGetContactOrganisation()
		{
			var entry = VAT404TestHelper.SetupEntry(Factory, "LRN1");
			VAT404TestHelper.AddPayInfo(entry, "VAT", 12m, "RCP01", ZDateTime.Today, ZDateTime.Today);
			var tester = new VAT404DocumentInstruction(Factory);
			tester.PerformSearch();
			var document = (VAT404Document)tester.VAT404Documents.FirstOrDefault();
			AssertEquals(entry.Declaration.Importer, document.DocumentSupporter.GetContactOrganisation("", null, DocumentDirection.ARV).OrgHeader);
			AssertEquals(entry.Declaration.Importer, document.DocumentSupporter.GetContactOrganisation("", null, DocumentDirection.DEP).OrgHeader);
		}

		public void TestGetDocumentTitlesForPivot()
		{
			var entry = VAT404TestHelper.SetupEntry(Factory, "LRN1");
			VAT404TestHelper.AddPayInfo(entry, "VAT", 12m, "RCP01", ZDateTime.Today, ZDateTime.Today);
			var tester = new VAT404DocumentInstruction(Factory);
			tester.PerformSearch();
			var document = (VAT404Document)tester.VAT404Documents.FirstOrDefault();
			AssertEquals("VAT 404 Proof of Payment", document.DocumentSupporter.GetDocumentTitlesForPivot("VAT 404 Proof of Payment", null, null).Title);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return VAT404TestHelper.GetVAT404Document(Factory);
		}
	}
}
