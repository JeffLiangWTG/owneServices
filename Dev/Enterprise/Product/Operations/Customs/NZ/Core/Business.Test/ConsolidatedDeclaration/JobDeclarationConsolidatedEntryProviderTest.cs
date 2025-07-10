using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business.Test
{
	class JobDeclarationConsolidatedEntryProviderTest : TestCaseWithFactory
	{
		public void TestConsolidationStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			declaration.ConsolidatedEntryProvider.QueueForConsolidation(out var queueMessage);
			AssertEquals($"Entry status after queue. Queue result: {queueMessage}", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, declaration.JE_EntryStatus);
			Factory.Save();
			declaration.ConsolidatedEntryProvider.DequeueOrRemoveFromConsolidation(out var dequeueMessage);
			AssertEquals($"Entry status after dequeue. Dequeue result: {dequeueMessage}", FormalEntryStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);
		}
	}
}
