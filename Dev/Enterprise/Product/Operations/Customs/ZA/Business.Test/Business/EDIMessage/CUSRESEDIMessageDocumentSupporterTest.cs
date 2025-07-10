using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CUSRESEDIMessageDocumentSupporter))]
	sealed class CUSRESEDIMessageDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessageReturnOneReason()
		{
			var message = Factory.New<CUSRESEDIMessage>();
			var supporter = message.DocumentSupporter;
			AssertEquals(false, supporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
			AssertEquals("The selected message is not linked to a proper Entry Header.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null));
			AssertEquals("", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null));
			AssertEquals("", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null));
			AssertEquals("", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCRefundDocument), null));
		}

		public void TestGetBODocDataProviders()
		{
			CombineAssertions("Message without EntryHeader", () =>
			{
				var message = Factory.New<CUSRESEDIMessage>();
				var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null);
				AssertNull("Provider for CUSDECCUSRESMessagePair", providers);
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null);
				AssertNull("Provider for SADDocumentPack", providers);
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null);
				AssertNull("Provider for VOCDocumentPack", providers);
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CUSRESEDIMessageDocumentSupporter.CUSCAR_CUSRES), null);
				AssertNull("Provider for CUSCAR_CUSRES", providers);
			});
			CombineAssertions("Message with EntryHeader", () =>
			{
				var message = GetDocumentSupportableBusinessObject();
				var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null);
				AssertEquals("Provider for CUSDECCUSRESMessagePair", 1, providers.Length);
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null);
				AssertNull("Provider for SADDocumentPack", providers);
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null);
				AssertNull("Provider for VOCDocumentPack", providers);
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CUSRESEDIMessageDocumentSupporter.CUSCAR_CUSRES), null);
				AssertNull("Provider for CUSCAR_CUSRES", providers);
			});
		}

		public void TestGetBODocDataProviders_CUSCAR_CUSRES()
		{
			var message = Factory.New<CUSRESEDIMessage>();
			message.EM_LinkedObject = Factory.New<ManifestBase.AsycudaManifestHeader>();
			var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CUSRESEDIMessageDocumentSupporter.CUSCAR_CUSRES), null);
			AssertNotNull(providers);
			AssertEquals(1, providers.Length);
		}

		public void TestGetBODocDataProviders_CUSCAR_CUSRESWhenLinkedObjectIsAnAsycudaBill()
		{
			var message = Factory.New<CUSRESEDIMessage>();
			var header = Factory.New<ManifestBase.AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			message.EM_LinkedObject = bill;
			var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CUSRESEDIMessageDocumentSupporter.CUSCAR_CUSRES), null);
			AssertNotNull(providers);
			AssertEquals(1, providers.Length);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var testMessage = Factory.NewWithValidTestData<CUSRESEDIMessage>();
			entryHeader.Messages.Add(testMessage);
			return testMessage;
		}
	}
}
