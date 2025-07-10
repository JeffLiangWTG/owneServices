using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderDocumentSupporter))]
	sealed class CusInBondHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContexts()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals("DataContext.CusInBondHeader is Supported", true, header.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CusInBondHeader)));
			AssertEquals("DataContext.Notes is Supported", true, header.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Notes)));
		}

		public void TestShowReasonForNotPrinting()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals(true, header.DocumentSupporter.ShowReasonForNotPrinting(DataContext.CusInBondHeader, null));
			AssertEquals(true, header.DocumentSupporter.ShowReasonForNotPrinting(DataContext.Notes, null));
			AssertEquals(false, header.DocumentSupporter.ShowReasonForNotPrinting(DataContext.None, null));
		}

		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals("TW In-bond Header cannot be found.", header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusInBondHeader), null));
			AssertEquals("TW In-bond Header cannot be found.", header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.Notes), null));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals(Env.Security.TWTranshipmentCustomiseDocuments, header.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetBODocDataProviders()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusInBondHeaderDocumentSupporter.N5301Declaration), null);
			AssertEquals(1, providers.Length);
			AssertEquals(typeof(N5301EDIMessageDocumentWrapper), providers[0].ParentBusinessObject.GetType());
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			return header;
		}
	}
}
