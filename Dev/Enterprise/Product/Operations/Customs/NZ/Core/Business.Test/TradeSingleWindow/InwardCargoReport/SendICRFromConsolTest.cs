using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System;
	using Enterprise.Customs.NZ.Business;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core.Encryption;

	public class SendICRFromConsolTest : SendICRBaseTest
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
				AssertEquals(LowValueManifestStatusList.Descriptions.NotSentToCustoms, manifestStatus.E2_MessageStatus);
				AssertEquals(0, consol.Messages.Count);

				var icrSender = GetNewSender(TSWTransactionTypes.Original);
				icrSender.SendMessage();

				AssertEquals(LowValueManifestStatusList.Descriptions.SentToCustoms, manifestStatus.E2_MessageStatus);
				AssertEquals(1, consol.Messages.Count);
				var sentMessage = consol.Messages[0];
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Original, sentMessage.EM_MessageSubType);
				AssertEquals("C002003289", sentMessage.EM_ApplicationReference);
				AssertEquals("Ix1yvDKoBSDh5nWjOgrt", sentMessage.EM_MessageOwner);
			}
		}

		public void TestSendCancelMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var icrSender = GetNewSender(TSWTransactionTypes.Cancel);
				icrSender.SendMessage();

				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation, consol.Messages[0].EM_MessageSubType);
			}
		}

		public void TestSendReplaceMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var icrSender = GetNewSender(TSWTransactionTypes.Replace);
				icrSender.SendMessage();

				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Replacement, consol.Messages[0].EM_MessageSubType);
			}
		}

		public void TestErrors()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var icrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertEquals(3, icrSender.ErrorCount);
				var errorsExpected = @"Carrier is not valid. Please enter a valid Carrier for this consol.
ICR can be sent only for Air or Sea.
Voyage or flight no is empty for this consol.
";
				AssertEquals(errorsExpected, icrSender.Errors);

				consol.JK_TransportMode = "AIR";
				var consolTransport = consol.Transports[0];
				consolTransport.JW_VoyageFlight = "QF108";
				icrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertEquals("Error Count", 1, icrSender.ErrorCount);
				errorsExpected = @"Carrier is not valid. Please enter a valid Carrier for this consol.
";
				AssertEquals(errorsExpected, icrSender.Errors);

				var shippingLine = Factory.New<OrgHeader>();
				consolTransport.JW_OA_CarrierAddress = shippingLine.MainAddress.PK;
				icrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertEquals(0, icrSender.ErrorCount);
			}
		}

		public void TestStatusTransactionScopeRollback()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");

			consol.JK_UniqueConsignRef = "C002003289";
			consol.Shipments.AddNew();
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var icrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => icrSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals(LowValueManifestStatusList.Descriptions.NotSentToCustoms, manifestStatus.E2_MessageStatus);
					AssertEquals("Messages.Count", 0, consol.Messages.Count);
				});
			}
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			return new SendICRFromConsol(consol, null, manifestStatus, transactionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			manifestStatus = new ICRManifestStatus(consol);
		}

		ForwardingConsol consol;
		ICRManifestStatus manifestStatus;
	}
}
