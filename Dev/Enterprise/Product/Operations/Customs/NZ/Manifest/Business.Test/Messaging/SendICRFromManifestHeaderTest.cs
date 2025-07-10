using System;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class SendICRFromManifestHeaderTest : NZ.Business.TradeSingleWindow.Testing.SendICRBaseTest
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new SendICRFromManifestHeader(null, null, TSWTransactionTypes.Original);
		}

		public void TestApplicationReference()
		{
			var icrSender = new SendICRFromManifestHeader(manifestHeader, null, TSWTransactionTypes.Original);
			Assert(icrSender.ApplicationReference.IsEmpty);
			manifestHeader.AMA_JobReference = "MAN2003289";
			AssertEquals("MAN2003289", icrSender.ApplicationReference);
		}

		public void TestCheckErrorsBeforeGeneratingMessage()
		{
			var icrSenderForTest = GetNewSender(TSWTransactionTypes.Original);
			AssertContains("Transport Mode: You have not entered a Transport Mode.", icrSenderForTest.Errors);
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			icrSenderForTest = GetNewSender(TSWTransactionTypes.Original);
			AssertNotContains("Transport Mode: You have not entered a Transport Mode.", icrSenderForTest.Errors);
		}

		public void TestSendMessage()
		{
			AssertMessageSent("MAN2003289", (AsycudaManifestHeader manifestHeader) =>
			{
				AssertEquals(1, manifestHeader.Messages.Count);
				AssertEquals(NZMessageStatusList.Codes.Sent, manifestHeader.AMA_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, manifestHeader.Bills[0].ABL_MessageStatus);
				var sentMessage = manifestHeader.Messages[0];
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Original, sentMessage.EM_MessageSubType);
				AssertEquals("MAN2003289", sentMessage.EM_ApplicationReference);
				Assert(sentMessage.EM_MessageOwner.IsEmpty);
			});
		}

		public void TestCancelMessage()
		{
			AssertMessageSent("MAN2003289", (AsycudaManifestHeader manifestHeader) =>
			{
				AssertEquals(1, manifestHeader.Messages.Count);
				AssertEquals(NZMessageStatusList.Codes.Sent, manifestHeader.AMA_MessageStatus);
				CreateResponseMessage(manifestHeader);
				GetNewSender(TSWTransactionTypes.Cancel).SendMessage();
				AssertEquals(3, manifestHeader.Messages.Count);
				var cancelMessage = manifestHeader.Messages[2];
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation, cancelMessage.EM_MessageSubType);
				AssertEquals("MAN2003289", cancelMessage.EM_ApplicationReference);
				Assert(cancelMessage.EM_MessageOwner.IsEmpty);
			});
		}

		public void TestModifyMessage()
		{
			AssertMessageSent("MAN2003289", (AsycudaManifestHeader manifestHeader) =>
			{
				AssertEquals(1, manifestHeader.Messages.Count);
				AssertEquals(NZMessageStatusList.Codes.Sent, manifestHeader.AMA_MessageStatus);
				CreateResponseMessage(manifestHeader);
				GetNewSender(TSWTransactionTypes.Replace).SendMessage();
				AssertEquals(3, manifestHeader.Messages.Count);
				var modifyMessage = manifestHeader.Messages[2];
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Replacement, modifyMessage.EM_MessageSubType);
				AssertEquals("MAN2003289", modifyMessage.EM_ApplicationReference);
				Assert(modifyMessage.EM_MessageOwner.IsEmpty);
			});
		}

		public void TestStatusTransactionScopeRollback()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = "abc";

			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN2003289";
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = manifestHeader.Bills.AddNew();
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var icrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => icrSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals("AMA_MessageStatus", "", manifestHeader.AMA_MessageStatus);
					AssertEquals("ABL_MessageStatus", "", bill.ABL_MessageStatus);
					AssertEquals("Messages.Count", 0, manifestHeader.Messages.Count);
				});
			}
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType) => new SendICRFromManifestHeader(manifestHeader, null, transactionType);

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.New<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader manifestHeader;

		void CreateResponseMessage(AsycudaManifestHeader manifestHeader)
		{
			var responseMessage = Factory.New<TSWMessage>();
			manifestHeader.Messages.Add(responseMessage);
			manifestHeader.RegistrationNumber = "123456789";
			Factory.Save();
		}

		void AssertMessageSent(ZString jobReference, Action<AsycudaManifestHeader> testFunc)
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = "abc";
			Factory.Save();
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "94839912H"))
			{
				manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifestHeader.AMA_JobReference = jobReference;
				manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				var bill = manifestHeader.Bills.AddNew();
				GetNewSender(TSWTransactionTypes.Original).SendMessage();
				testFunc(manifestHeader);
			}
		}
	}
}
