using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackingListDocumentSupporter))]
	sealed class CusPackingListDocumentSupporterTestForDeclaration : Customs.Business.Testing.CusPackingListDocumentSupporterTest
	{
		protected override Customs.Business.CusPackingListDocumentSupporter GetDocumentSupporterToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var packingList = (CusPackingList)declaration.CreateCusPackingList(Factory);
			return new CusPackingListDocumentSupporter(packingList);
		}

		public override void TestGetBODocDataProviders()
		{
			GlbCompany.CurrentCompany.SetCountry(TestCountryCode);
			var mainNameSpace = GetMainNameSpace();

			var dataContextValue = new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusPackingList);
			var bODocDataProviders = documentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("Document wrapper for data context of CusPackingList is of type DocCusPackingList", "Enterprise.Customs.TW.Business.DocCusPackingList", bODocDataProviders[0].GetType().ToString());
		}
	}

	[TestedType(typeof(CusPackingListDocumentSupporter))]
	sealed class CusPackingListDocumentSupporterTestForInvoice : Customs.Business.Testing.CusPackingListDocumentSupporterTest
	{
		protected override Customs.Business.CusPackingListDocumentSupporter GetDocumentSupporterToTest()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var packingList = invoice.CreateCusPackingList(Factory);
			return new CusPackingListDocumentSupporter(packingList);
		}

		[ExpectNoExceptions]
		public override void TestGetBODocDataProviders()
		{
			GlbCompany.CurrentCompany.SetCountry(TestCountryCode);
			var mainNameSpace = GetMainNameSpace();

			var dataContextValue = new DataContextValueForTesting(Core.Constants.DataContext.CusPackingList);
			var bODocDataProviders = documentSupporter.GetBODocDataProviders(dataContextValue, null);
			NUnit.Framework.Assert.That(bODocDataProviders[0].GetType().ToString(), NUnit.Framework.Is.EqualTo("Enterprise.Customs.TW.Business.InvoiceDocCusPackingList"), "Document wrapper for data context of CusPackingList is of type DocCusPackingList");
		}
	}
}
