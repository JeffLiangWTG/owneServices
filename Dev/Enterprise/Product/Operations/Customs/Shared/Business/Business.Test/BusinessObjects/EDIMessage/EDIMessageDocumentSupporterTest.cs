using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(EDIMessageDocumentSupporter))]
	sealed class EDIMessageDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var bo = Factory.New<MockEDIMessage>();
			AssertEquals("", bo.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.None), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			var bo = Factory.New<MockEDIMessage>();
			AssertEquals(false, bo.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<MockEDIMessage>();
		}
	}
}
