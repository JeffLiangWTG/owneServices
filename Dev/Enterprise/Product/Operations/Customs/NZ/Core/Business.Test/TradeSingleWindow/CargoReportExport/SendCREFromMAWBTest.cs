using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System;
	using System.Linq;
	using Enterprise.Customs.NZ.Business;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core.Encryption;
	using ZArchitecture.Schema;

	public class SendCREFromMAWBTest : SendCREBaseTest
	{
		public void TestSendMessage()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");

			mawb.CM_MessageReference = "MB0020093";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_IsSelfAssessedClearance = true;
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var creSender = GetNewSender(TSWTransactionTypes.Original);
				creSender.SendMessage();

				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, mawb.CM_CustomsStatus);
				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, hawb.CS_CustomsStatus);

				AssertEquals("Message Created", 1, mawb.Messages.Count);
				var sentMessage = mawb.Messages[0];
				AssertEquals("MB0020093", sentMessage.EM_ApplicationReference);
				AssertEquals("EM_MessageOwner - encrypted Declarant Pin is required to be stored here to enable the generation of MAC in Interchange Provider for eHub inclusion in SOAP manifest string.", "Ix1yvDKoBSDh5nWjOgrt", sentMessage.EM_MessageOwner);

				var messageSentEvent = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).First();
				AssertEquals("SND", messageSentEvent.SL_Reference);
			}
		}

		public void TestSendCancelMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var creSender = GetNewSender(TSWTransactionTypes.Cancel);
				creSender.SendMessage();

				var messageSentEvent = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageWithdrawCancelRequestCode)).First();
				AssertEquals("CAN", messageSentEvent.SL_Reference);
			}
		}

		public void TestErrors()
		{
			var creSender = GetNewSender(TSWTransactionTypes.Original);
			var errors = creSender.Errors;
			AssertContains("Flight No: Please enter a valid Flight Number, or 'COURIER' if the Flight Number is not obtainable.", errors);
			AssertNotContains("Flight number is not in the list of valid Flights supported by NZ Customs.", errors);

			mawb.CM_FlightNo = "AA220";
			creSender = GetNewSender(TSWTransactionTypes.Original);
			errors = creSender.Errors;
			AssertNotContains("Flight No: Please enter a valid Flight Number, or 'COURIER' if the Flight Number is not obtainable.", errors);
			AssertContains("Flight number is not in the list of valid Flights supported by NZ Customs.", errors);
		}

		public void TestSendMessageIncludeHawbErrors()
		{
			var expectedError = "Consignee Name: You have not entered a Consignee Name.";
			var hawb1 = mawb.ChildBills.AddNew();

			var creSender = GetNewSender(TSWTransactionTypes.Original);
			AssertContains(expectedError, creSender.Errors);
			hawb1.CS_ConsigneeName = "CONSIGNEE";
			creSender = GetNewSender(TSWTransactionTypes.Original);
			AssertNotContains(expectedError, creSender.Errors);
		}

		public void TestStatusTransactionScopeRollback()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");

			mawb.CM_MessageReference = "MB0020093";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_IsSelfAssessedClearance = true;
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var creSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => creSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals("CM_CustomsStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, mawb.CM_CustomsStatus);
					AssertEquals("CS_CustomsStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, hawb.CS_CustomsStatus);
					AssertEquals("CS_MsgStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, hawb.CS_MsgStatus);
					AssertEquals("Events.MessageSentCode", 0, mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).Length);
					AssertEquals("Messages.Count", 0, mawb.Messages.Count);
				});
			}
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			return new SendCREFromMAWB(mawb, null, transactionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mawb = Factory.New<CusMAWB>();
		}

		CusMAWB mawb;
	}
}
