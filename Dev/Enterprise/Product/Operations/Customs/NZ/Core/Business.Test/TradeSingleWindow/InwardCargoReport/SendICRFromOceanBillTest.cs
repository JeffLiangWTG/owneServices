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
	class SendICRFromOceanBillTest : SendICRBaseTest
	{
		public void TestSendMessage()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("QW67OP");

			oceanBill.CB_MessageReference = "X00003299";
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_GoodsValue = 10m;
			houseBill.CA_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.NewZealand;
			houseBill.CA_IsSAC = true;
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "94839912H"))
			{
				var icrSender = GetNewSender(TSWTransactionTypes.Original);
				icrSender.SendMessage();

				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, oceanBill.CB_MessageStatus);
				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, oceanBill.CB_CustomsStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, houseBill.CA_MessageStatus);
				Assert(houseBill.CA_ShipmentStatus.IsEmpty);

				AssertEquals(1, oceanBill.Messages.Count);
				var sentMessage = oceanBill.Messages[0];
				AssertEquals("X00003299", sentMessage.EM_ApplicationReference);
				AssertEquals("zQw23S2Bv9Xs7/KPqwd0", sentMessage.EM_MessageOwner);

				var messageSentEvent = oceanBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).First();
				AssertEquals("SND", messageSentEvent.SL_Reference);
			}
		}

		public void TestSendCancelMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "10987347N"))
			{
				var icrSender = GetNewSender(TSWTransactionTypes.Cancel);
				icrSender.SendMessage();

				var messageSentEvent = oceanBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageWithdrawCancelRequestCode)).First();
				AssertEquals("CAN", messageSentEvent.SL_Reference);
			}
		}

		public void TestErrors()
		{
			var icrSender = GetNewSender(TSWTransactionTypes.Original);
			var errors = icrSender.Errors;
			AssertContains("Master Bill: You have not entered a Master Bill.", errors);

			oceanBill.CB_OceanBill = "OCEANBILL1";
			icrSender = GetNewSender(TSWTransactionTypes.Original);
			errors = icrSender.Errors;
			AssertNotContains("Master Bill: You have not entered a Master Bill.", errors);
		}

		public void TestHawbErrors()
		{
			var expectedError = "Consignee Name: You have not entered a Consignee Name.";
			var houseBill = oceanBill.HouseBills.AddNew();

			var icrSender = GetNewSender(TSWTransactionTypes.Original);
			AssertContains(expectedError, icrSender.Errors);
			houseBill.CA_ConsigneeName = "CONSIGNEE";
			icrSender = new SendICRFromOceanBill(oceanBill, null, TSWTransactionTypes.Original);
			AssertNotContains(expectedError, icrSender.Errors);
		}

		public void TestStatusTransactionScopeRollback()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("QW67OP");

			oceanBill.CB_MessageReference = "X00003299";
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_GoodsValue = 10m;
			houseBill.CA_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.NewZealand;
			houseBill.CA_IsSAC = true;
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "94839912H"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var icrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => icrSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals("CB_MessageStatus", "", oceanBill.CB_MessageStatus);
					AssertEquals("CB_CustomsStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, oceanBill.CB_CustomsStatus);
					AssertEquals("CA_MessageStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, houseBill.CA_MessageStatus);
					AssertEquals("Events.MessageSentCode", 0, oceanBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).Length);
					AssertEquals("Messages.Count", 0, oceanBill.Messages.Count);
				});
			}
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			return new SendICRFromOceanBill(oceanBill, null, transactionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();
		}

		CusSCAOceanBill oceanBill;
	}
}
