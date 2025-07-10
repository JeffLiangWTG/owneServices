using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ConcurrenceDeliveryMessageSenderTest : FTZRelatedMessageSenderTest
	{
		public void TestUpdateFTZConcurrenceQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "222256789";
			bill2.CU_NoOfPacks = 14m;
			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "6005006";
			itNo.US_NoOfPacks = 15;
			var itNo2 = bill.ITAndSplitDetails.AddNew();
			itNo2.US_ITNumber = "V1006003";
			itNo2.US_NoOfPacks = 16;

			declaration.US_FTZConcurrenceQty = 55m;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			declaration.Factory.Save();

			var sender = new ConcurrenceDeliveryMessageSender(declaration, FZEventType.Delivery);
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ if (shouldSave) { declaration.Factory.Save(); } });
			sender.OnPrepare += (FZEventAction action) =>
			{
				action.US_ActionCode = FTZActionCodeList.Codes.A;
				action.GetMessageSendingObjectsNeedSending().Single().MB_ConcurrenceQty = 40m;
				return true;
			};
			if (shouldAllowNotifications)
			{
				sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
				{ return shouldReturnTrueOnAllowNotifications; });
			}
			Assert(sender.SendMessage());
			AssertEquals("declaration.US_FTZConcurrenceQty", 55m, declaration.US_FTZConcurrenceQty);

			bill.US_FTZConcurrenceQty = 66m;
			sender = new ConcurrenceDeliveryMessageSender(declaration, FZEventType.Delivery);
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ if (shouldSave) { declaration.Factory.Save(); } });
			sender.OnPrepare += (FZEventAction action) =>
			{
				action.US_ActionCode = FTZActionCodeList.Codes.B;
				action.GetMessageSendingObjectsNeedSending()[0].MB_ConcurrenceQty = 40m;
				return true;
			};
			if (shouldAllowNotifications)
			{
				sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
				{ return shouldReturnTrueOnAllowNotifications; });
			}
			Assert(sender.SendMessage());
			AssertEquals("declaration.US_FTZConcurrenceQty", 66m, bill.US_FTZConcurrenceQty);

			itNo.US_FTZConcurrenceQty = 77m;
			sender = new ConcurrenceDeliveryMessageSender(declaration, FZEventType.Delivery);
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ if (shouldSave) { declaration.Factory.Save(); } });
			sender.OnPrepare += (FZEventAction action) =>
			{
				action.US_ActionCode = FTZActionCodeList.Codes.C;
				action.GetMessageSendingObjectsNeedSending()[0].MB_ConcurrenceQty = 40m;
				return true;
			};
			if (shouldAllowNotifications)
			{
				sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
				{ return shouldReturnTrueOnAllowNotifications; });
			}
			Assert(sender.SendMessage());
			AssertEquals("declaration.US_FTZConcurrenceQty", 77m, itNo.US_FTZConcurrenceQty);

			//FZEventType.Concur type 
			sender = new ConcurrenceDeliveryMessageSender(declaration, FZEventType.Concur);
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ if (shouldSave) { declaration.Factory.Save(); } });
			sender.OnPrepare += (FZEventAction action) =>
			{
				action.US_ActionCode = FTZActionCodeList.Codes.A;
				action.GetMessageSendingObjectsNeedSending().Single().MB_ConcurrenceQty = 40m;
				return true;
			};
			if (shouldAllowNotifications)
			{
				sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
				{ return shouldReturnTrueOnAllowNotifications; });
			}
			Assert(sender.SendMessage());
			AssertEquals("declaration.US_FTZConcurrenceQty", 40m, declaration.US_FTZConcurrenceQty);

			bill.US_FTZConcurrenceQty = 66m;
			sender = new ConcurrenceDeliveryMessageSender(declaration, FZEventType.Concur);
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ if (shouldSave) { declaration.Factory.Save(); } });
			sender.OnPrepare += (FZEventAction action) =>
			{
				action.US_ActionCode = FTZActionCodeList.Codes.B;
				action.GetMessageSendingObjectsNeedSending()[0].MB_ConcurrenceQty = 40m;
				return true;
			};
			if (shouldAllowNotifications)
			{
				sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
				{ return shouldReturnTrueOnAllowNotifications; });
			}
			Assert(sender.SendMessage());
			AssertEquals("declaration.US_FTZConcurrenceQty", 40m, bill.US_FTZConcurrenceQty);

			itNo.US_FTZConcurrenceQty = 77m;
			sender = new ConcurrenceDeliveryMessageSender(declaration, FZEventType.Concur);
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ if (shouldSave) { declaration.Factory.Save(); } });
			sender.OnPrepare += (FZEventAction action) =>
			{
				action.US_ActionCode = FTZActionCodeList.Codes.C;
				action.GetMessageSendingObjectsNeedSending()[0].MB_ConcurrenceQty = 40m;
				return true;
			};
			if (shouldAllowNotifications)
			{
				sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
				{ return shouldReturnTrueOnAllowNotifications; });
			}
			Assert(sender.SendMessage());
			AssertEquals("declaration.US_FTZConcurrenceQty", 40m, itNo.US_FTZConcurrenceQty);
		}

		public void TestDoesSendWithMessageErrors()
		{
			Factory.Save();
			((SendsMessagesToCustoms)Declaration.MessageInitiator).ReturnTrueOnYesNoQuery = false;
			Assert(!Sender.SendMessage());
			AssertCollectionContains("Concurrence message should not be sent because the admission data has not been accepted by Customs. Do you want to override and send anyway?", ((SendsMessagesToCustoms)Declaration.MessageInitiator).LastMessages);

			Assert(!UnconcurrenceSender.SendMessage());
			AssertCollectionContains("Post Admission Correction message should not be sent because Concurrence has not been accepted by Customs. Do you want to override and send anyway?", ((SendsMessagesToCustoms)Declaration.MessageInitiator).LastMessages);

			Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			((SendsMessagesToCustoms)Declaration.MessageInitiator).ReturnTrueOnShowUserConfirmation = false;
			Assert(!Sender.SendMessage());
			AssertCollectionContains("Concurrence message should not be sent because the admission data has not been accepted by Customs. You do not have the security rights to override this error and send.", ((SendsMessagesToCustoms)Declaration.MessageInitiator).LastMessages);

			Assert(!UnconcurrenceSender.SendMessage());
			AssertCollectionContains("Post Admission Correction message should not be sent because Concurrence has not been accepted by Customs. You do not have the security rights to override this error and send.", ((SendsMessagesToCustoms)Declaration.MessageInitiator).LastMessages);
		}

		public override void TestSendMessageIfAllOK()
		{
			AssertConcurrenceMessage();
			AssertDeliveryMessage();
			AssertUnconcurrenceMessage();
		}

		void AssertConcurrenceMessage()
		{
			SetHeader();
			bool sentSuccessfuly = Sender.SendMessage();
			Assert(sentSuccessfuly);
			CargoWise.Common.ErrorReporter.Clear();
			AssertEquals(FTZMessageStatusList.Codes.AwaitingConcurrence, Declaration.FTZConcurrenceStatus);
		}

		void AssertDeliveryMessage()
		{
			SetHeader();
			bool sentSuccessfuly = DeliverySender.SendMessage();
			Assert(sentSuccessfuly);
			CargoWise.Common.ErrorReporter.Clear();
			AssertEquals(FTZMessageStatusList.Codes.AwaitingDeliveryOfGoods, Declaration.FTZDeliveryOfGoodsStatus);
		}

		void AssertUnconcurrenceMessage()
		{
			bool sentSuccessfuly = UnconcurrenceSender.SendMessage();
			Assert(sentSuccessfuly);
			AssertEquals(FTZMessageStatusList.Codes.AwaitingUnconcurrence, Declaration.FTZUnconcurrenceStatus);
		}

		void SetHeader()
		{
			Declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
		}

		#region implementation

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new ConcurrenceDeliveryMessageSender(Declaration, FZEventType.Concur);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });

					if (shouldAllowNotifications)
					{
						sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
						{ return shouldReturnTrueOnAllowNotifications; });
					}
				}
				return sender;
			}
		}

		protected MessageSender DeliverySender
		{
			get
			{
				if (deliverySender == null)
				{
					deliverySender = new ConcurrenceDeliveryMessageSender(Declaration, FZEventType.Delivery);
					deliverySender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });

					if (shouldAllowNotifications)
					{
						deliverySender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
						{ return shouldReturnTrueOnAllowNotifications; });
					}
				}
				return deliverySender;
			}
		}
		ConcurrenceDeliveryMessageSender deliverySender;
		protected MessageSender UnconcurrenceSender
		{
			get
			{
				if (unconcurrenceSender == null)
				{
					unconcurrenceSender = new ConcurrenceDeliveryMessageSender(Declaration, FZEventType.Unconcur);
					unconcurrenceSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });

					if (shouldAllowNotifications)
					{
						unconcurrenceSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
						{ return shouldReturnTrueOnAllowNotifications; });
					}
				}
				return unconcurrenceSender;
			}
		}
		ConcurrenceDeliveryMessageSender unconcurrenceSender;

		#endregion
	}
}
