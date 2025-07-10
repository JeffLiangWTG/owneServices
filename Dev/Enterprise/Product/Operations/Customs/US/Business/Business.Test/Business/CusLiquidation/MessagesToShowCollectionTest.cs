using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MessagesToShowCollection))]
	sealed class MessagesToShowCollectionTest : ActiveBusinessObjectCollectionTestCase<MessagesToShowCollection>
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
			var messages = new MessagesToShowCollection(newFactory, new ZGuid[] { declaration.PK }, MessagesToShowCollection.MessagesStatus.ActiveOnly, new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, ApplicationIdentifierCodeList.Codes.BIRDTransaction));
			AssertEquals(4, messages.Count);
			AssertNotNull("USAMS matched", messages.FirstOrDefault(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USAMS));
			AssertNotNull("USCustomsExport matched", messages.FirstOrDefault(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USCustomsExport));
			AssertNotNull("USCustomsImport matched", messages.FirstOrDefault(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USCustomsImport));
			AssertNotNull("USeBond matched", messages.FirstOrDefault(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USeBond));
		}

		public void TestMessagesToShowCollectionForBO()
		{
			AddAndAssertNewElements();
		}

		public void TestMessagesToShowCollectionForLiquidations()
		{
			CusLiquidation liquidation = Factory.New<CusLiquidation>();
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkUniqueID = liquidation.PK;
			CusLiquidation liquidation2 = Factory.New<CusLiquidation>();
			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_LinkUniqueID = liquidation2.PK;
			Declaration.Liquidations.Add(liquidation);
			Declaration.Liquidations.Add(liquidation2);
			IMessageAttacheeInDeclaration liquidations = new LiquidationWithMessagesToShow(Declaration.Liquidations);
			MessagesToShowCollection coll = new MessagesToShowCollection(Factory, liquidations.ParentPKsOfMessages, MessagesToShowCollection.MessagesStatus.ActiveOnly, new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, ApplicationIdentifierCodeList.Codes.BIRDTransaction));
			AssertEquals(2, coll.Count);
		}

		public void TestMessagesToShowCollectionForBIRD()
		{
			var liquidation = Factory.New<CusLiquidation>();
			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkUniqueID = liquidation.PK;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
			var liquidation2 = Factory.New<CusLiquidation>();
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
			message2.EM_LinkUniqueID = liquidation2.PK;
			Declaration.Liquidations.Add(liquidation);
			Declaration.Liquidations.Add(liquidation2);
			IMessageAttacheeInDeclaration liquidations = new LiquidationWithMessagesToShow(Declaration.Liquidations);
			var coll = new MessagesToShowCollection(Factory, liquidations.ParentPKsOfMessages, MessagesToShowCollection.MessagesStatus.ActiveOnly, new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.BIRDTransaction));
			AssertEquals(2, coll.Count);
		}

		public new void TestAdd()
		{
			AddAndAssertNewElements();
		}

		public new void TestAddNew()
		{
			AddAndAssertNewElements();
		}

		public new void TestDelete()
		{
			var header = Declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var message = header.Messages.AddNew(typeof(EDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			var coll = new MessagesToShowCollection(Factory, new ZGuid[] { header.PK }, MessagesToShowCollection.MessagesStatus.ActiveOnly, new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, ApplicationIdentifierCodeList.Codes.BIRDTransaction));
			AssertEquals(1, coll.Count);
			header.Messages.RemoveAndDeleteAll();
			AssertEquals(0, coll.Count);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<EDIMessage>();

		protected override MessagesToShowCollection GetCollectionToTest()
			=> new MessagesToShowCollection(Declaration.Factory, new ZGuid[] { Declaration.PK }, MessagesToShowCollection.MessagesStatus.ActiveOnly, new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, ApplicationIdentifierCodeList.Codes.BIRDTransaction));

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		void AddAndAssertNewElements()
		{
			var header = Declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var message = header.Messages.AddNew(typeof(EDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			message = header.Messages.AddNew(typeof(EDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			message = header.Messages.AddNew(typeof(EDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			var coll = new MessagesToShowCollection(Factory, new ZGuid[] { header.PK }, MessagesToShowCollection.MessagesStatus.ActiveOnly, new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, ApplicationIdentifierCodeList.Codes.BIRDTransaction));
			AssertEquals(3, coll.Count);
		}
	}
}
