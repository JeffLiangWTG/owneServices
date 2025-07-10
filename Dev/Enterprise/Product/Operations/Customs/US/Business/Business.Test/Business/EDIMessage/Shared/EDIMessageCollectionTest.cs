using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EDIMessageCollection))]
	sealed class EDIMessageCollectionTest : Enterprise.Messaging.Business.EDIMessageCollectionTest
	{
		public void TestOnlyCBPMessagesAreIncluded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "A!@";
			var messageApplicationCode = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			messageApplicationCode.EM_ApplicationCode = "A!@";
			messageApplicationCode.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageApplicationCode.EM_LinkedObject = declaration;
			var messageUnknown = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			messageUnknown.EM_ApplicationCode = "D!@";
			messageUnknown.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageUnknown.EM_LinkedObject = declaration;
			foreach (ICodeDescription pair in new ApplicationCodeList())
			{
				var message = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
				message.EM_ApplicationCode = pair.Code;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_LinkedObject = declaration;
				message.EM_MessageSubType = pair.Code switch
				{
					ApplicationCodeList.Codes.eNett => eNettMessageSubTypeList.Codes.GetNewPayments,
					ApplicationCodeList.Codes.GlobalElectronicPayment => GEPProviderAPICommandList.Codes.CreateADeal,
					ApplicationCodeList.Codes.GlobalElectronicInvoice => EInvoiceAPICommandList.Codes.GenerateCancellationRequest,
					_ => ZString.Empty
				};
			}

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var declarationInDiffFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			var messages = declarationInDiffFactory.Messages.Cast<Enterprise.Messaging.Business.EDIMessage>().ToArray();
			AssertEquals(3, messages.Length);
			AssertNotNull("USAMS matched", messages.FirstOrDefault(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USAMS));
			AssertNotNull("USCustomsExport matched", messages.FirstOrDefault(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USCustomsExport));
			AssertNotNull("USCustomsImport matched", messages.FirstOrDefault(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USCustomsImport));
		}

		public void TestDiscardAll()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertNotNull(entry);
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			entry.Messages.Add(message);
			entry.Messages.DiscardAll(declaration);
			AssertEquals("No messages remaining", 0, entry.Messages.Count);
			AssertEquals("Linked to the new master", declaration.PK, message.EM_LinkUniqueID);
			AssertEquals("Discarded", EDIMessage.Status.Discarded, message.EM_Status);
		}

		public void TestGetLastMessageWithSpecificMessageBlock()
		{
			var coll = (EDIMessageCollection)GetCollectionToTest();
			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			message1.EM_MessageText = "B011101OHLHT                                               51163                H11101OHL 000011432GBDATA REPLACED AS REQUESTED               2082709Y90001118  Y  1101OHLHT00001";
			coll.Add(message1);
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Today;
			message2.EM_MessageText = "B011101OHLHT                                               51765                H11101OHL 000011432GBDATA REPLACED AS REQUESTED               2083109Y90001118  Y  1101OHLHT00001";
			coll.Add(message2);
			AssertEquals(message2, coll.GetLastMessageWithSpecificMessageBlock(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse, EDIMessage.Direction.Receive, typeof(ENSH1)));
		}

		public void TestTypedSingleParameterAddNew()
		{
			var collection = (EDIMessageCollection)GetCollectionToTest();
			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });
			var bizO2 = (BusinessObject)method.Invoke(collection, new object[] { typeof(EDIMessage) });
			AssertNotNull("AddNew of Type " + method.ReturnType.FullName + " not null", bizO2);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new EDIMessageCollection(entry);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<EDIMessage>();
	}
}
