using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QuotaInformation))]
	sealed class QuotaInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestQuotaInformationMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E141333022090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"E4TA0Q01                                  10025       KG 8          KG          " +
"E4TA1Q02                                  99          G  10011      MG          " +
"E4TA2Q03                                  7841225213  OT 89         OT          " +
"E4TA3Q04                                  18          MC 87777723   TL          " +
"E4TA4Q05                                  125         LT 78945614783T           " +
"Y  8888XJ5UC00003";

			var quotaInformation = new QuotaInformation(message.MessageBlock.MessageBlocks.OfType<AESSE4>().FirstOrDefault(x => x.LineItemIdentifier == "TA0"));

			AssertNotNull(quotaInformation);
			AssertEquals(quotaInformation.LineItemIdentifier, "TA0");
			AssertEquals(quotaInformation.QuotaLineStatusCode, "Q01");
			AssertEquals(quotaInformation.QuotaLineStatusDescription, "Quota Processed / Accepted");
			AssertEquals(quotaInformation.ReservedQuotaQuantity, 0.08m);
			AssertEquals(quotaInformation.ReservedQuotaQuantityUQ, "KG");
			AssertEquals(quotaInformation.RequestedQuotaQuantity, 100.25m);
			AssertEquals(quotaInformation.RequestedQuotaQuantityUQ, "KG");

			quotaInformation = new QuotaInformation(message.MessageBlock.MessageBlocks.OfType<AESSE4>().FirstOrDefault(x => x.LineItemIdentifier == "TA3"));

			AssertNotNull(quotaInformation);
			AssertEquals(quotaInformation.LineItemIdentifier, "TA3");
			AssertEquals(quotaInformation.QuotaLineStatusCode, "Q04");
			AssertEquals(quotaInformation.QuotaLineStatusDescription, "Quota Filled or Expired");
			AssertEquals(quotaInformation.ReservedQuotaQuantity, 877777.23m);
			AssertEquals(quotaInformation.ReservedQuotaQuantityUQ, "TL");
			AssertEquals(quotaInformation.RequestedQuotaQuantity, 0.18m);
			AssertEquals(quotaInformation.RequestedQuotaQuantityUQ, "MC");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new QuotaInformation();
		}
	}
}
