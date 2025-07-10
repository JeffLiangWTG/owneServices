using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	public class SendICRFromMAWBTest : SendICRBaseTest
	{
		public void TestSendMessage()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");

			mawb.CM_MessageReference = "MB0020093";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			hawb.CS_GoodsValue = 50m;
			hawb.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.NewZealand;
			hawb.CS_IsSelfAssessedClearance = true;
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				var icrSender = GetNewSender(TSWTransactionTypes.Original);
				icrSender.SendMessage();

				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, mawb.CM_CustomsStatus);
				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, hawb.CS_CustomsStatus);

				AssertEquals(1, mawb.Messages.Count);
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
				var icrSender = GetNewSender(TSWTransactionTypes.Cancel);
				icrSender.SendMessage();

				var messageSentEvent = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageWithdrawCancelRequestCode)).First();
				AssertEquals("CAN", messageSentEvent.SL_Reference);
			}
		}

		public void TestErrors()
		{
			var icrSender = GetNewSender(TSWTransactionTypes.Original);
			var errors = icrSender.Errors;
			AssertContains("Flight No: Please enter a valid Flight Number, or 'COURIER' if the Flight Number is not obtainable.", errors);
			AssertNotContains("Flight number is not in the list of valid Flights supported by NZ Customs.", errors);

			mawb.CM_FlightNo = "ZZ69";
			icrSender = GetNewSender(TSWTransactionTypes.Original);
			errors = icrSender.Errors;
			AssertNotContains("Flight No: Please enter a valid Flight Number, or 'COURIER' if the Flight Number is not obtainable.", errors);
			AssertContains("Flight number is not in the list of valid Flights supported by NZ Customs.", errors);
		}

		public void TestSendMessageIncludeHawbErrors()
		{
			var expectedError = "Consignee Name: You have not entered a Consignee Name.";
			var hawb = mawb.ChildBills.AddNew();

			var icrSender = GetNewSender(TSWTransactionTypes.Original);
			AssertContains(expectedError, icrSender.Errors);
			hawb.CS_ConsigneeName = "SCOTTS PLACE";
			icrSender = GetNewSender(TSWTransactionTypes.Original);
			AssertNotContains(expectedError, icrSender.Errors);
		}

		public void TestStatusTransactionScopeRollback()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("NN12WW");

			mawb.CM_MessageReference = "MB0020093";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			hawb.CS_GoodsValue = 50m;
			hawb.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.NewZealand;
			hawb.CS_IsSelfAssessedClearance = true;
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var icrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => icrSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals("CM_CustomsStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, mawb.CM_CustomsStatus);
					AssertEquals("CS_CustomsStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, hawb.CS_CustomsStatus);
					AssertEquals("CS_MsgStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, hawb.CS_MsgStatus);
					AssertEquals("Events.MessageSentCode", 0, mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).Length);
					AssertEquals("Messages.Count", 0, mawb.Messages.Count);
				});
			}
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			return new SendICRFromMAWB(mawb, null, transactionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mawb = Factory.New<CusMAWB>();
		}

		CusMAWB mawb;
	}
}
