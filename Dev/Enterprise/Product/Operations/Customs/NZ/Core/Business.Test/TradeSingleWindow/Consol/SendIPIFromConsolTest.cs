using System;
using System.Linq;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	public class SendIPIFromConsolTest : SendTSWBaseTest
	{
		public void TestSendMessage()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");

			consol.JK_UniqueConsignRef = "C002003289";
			consol.Shipments.AddNew();
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var ipiSender = GetNewSender(TSWTransactionTypes.Original);
				ipiSender.SendMessage();

				AssertEquals(MessagingStatusList.Codes.SentPendingAcknowledgement, mafMessaging.ZX_MessagingStatus);
				AssertEquals(1, mafMessaging.TSWMessages.Count);
				var sentMessage = mafMessaging.TSWMessages[0];
				AssertEquals(Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original, sentMessage.EM_MessageSubType);
				AssertEquals("C002003289", sentMessage.EM_ApplicationReference);
			}
		}

		public void TestSendCancelMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var ipiSender = GetNewSender(TSWTransactionTypes.Cancel);
				ipiSender.SendMessage();

				AssertEquals(Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation, mafMessaging.TSWMessages[0].EM_MessageSubType);
			}
		}

		public void TestSendReplaceMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var ipiSender = GetNewSender(TSWTransactionTypes.Replace);
				ipiSender.SendMessage();

				AssertEquals(Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Replacement, mafMessaging.TSWMessages[0].EM_MessageSubType);
			}
		}

		public void TestErrors()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");

			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var ipiSender = GetNewSender(TSWTransactionTypes.Original);
				AssertStartsWith("Errors", @"Cannot send message due the following validation errors:", ipiSender.Errors);
				AssertEquals("Error Count", 1, ipiSender.ErrorCount);

				new TestDataBuilder(consol.Factory).PopulateConsolThatPassesValidation(consol);

				ipiSender = GetNewSender(TSWTransactionTypes.Original);
				AssertEquals("Errors", "", ipiSender.Errors);
				AssertEquals("Error Count", 0, ipiSender.ErrorCount);
			}
		}

		public void TestStatusTransactionScopeRollback()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");

			consol.JK_UniqueConsignRef = "C002003289";
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var ipiSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => ipiSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals(MessagingStatusList.Codes.NotSentToMpi, mafMessaging.ZX_MessagingStatus);
					AssertEquals("Messages.Count", 0, consol.Messages.Count);
					AssertEquals("Unsaved Logs", false, consol.Logs.GetAllLogs().Any(x => !x.IsInDatabase));
				});
			}
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			mafMessaging = MAFeBACCa.Testing.TestDataBuilder.GetMAFMessaging(consol);
			var sender = new SendIPIFromConsol(mafMessaging, new AdditionalMessageInformation(transactionType, Factory), transactionType);
			return sender;
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
		}
		ForwardingConsol consol;
		MAFMessagingBO mafMessaging;
	}
}
