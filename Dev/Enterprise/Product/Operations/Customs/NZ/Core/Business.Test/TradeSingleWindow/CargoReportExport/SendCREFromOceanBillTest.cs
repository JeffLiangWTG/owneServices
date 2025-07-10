using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.NZ.Business;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core.Encryption;
	using Enterprise.ZArchitecture.Schema;

	class SendCREFromCusSCAOceanBillTest : SendCREBaseTest
	{
		public void TestSendMessage()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("XX13ZZ");

			oceanBill.CB_MessageReference = "X00002093";
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_IsSAC = true;
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "10987347N"))
			{
				var creSender = GetNewSender(TSWTransactionTypes.Original);
				creSender.SendMessage();

				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, oceanBill.CB_MessageStatus);
				AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, oceanBill.CB_CustomsStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, houseBill.CA_MessageStatus);
				Assert(houseBill.CA_ShipmentStatus.IsEmpty);

				AssertEquals(1, oceanBill.Messages.Count);
				var sentMessage = oceanBill.Messages[0];
				AssertEquals("X00002093", sentMessage.EM_ApplicationReference);
				AssertEquals("Klnz772P80v2LtrwCOxh", sentMessage.EM_MessageOwner);

				var messageSentEvent = oceanBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode)).First();
				AssertEquals("SND", messageSentEvent.SL_Reference);
			}
		}

		public void TestSendCancelMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "10987347N"))
			{
				var creSender = GetNewSender(TSWTransactionTypes.Cancel);
				creSender.SendMessage();

				var messageSentEvent = oceanBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageWithdrawCancelRequestCode)).First();
				AssertEquals("CAN", messageSentEvent.SL_Reference);
			}
		}

		public void TestErrors()
		{
			var creSender = GetNewSender(TSWTransactionTypes.Original);
			var errors = creSender.Errors;
			AssertContains("Master Bill: You have not entered a Master Bill.", errors);

			oceanBill.CB_OceanBill = "OCEANBILL1";
			creSender = GetNewSender(TSWTransactionTypes.Original);
			errors = creSender.Errors;
			AssertNotContains("Master Bill: You have not entered a Master Bill.", errors);
		}

		public void TestHawbErrors()
		{
			var expectedError = "Consignee Name: You have not entered a Consignee Name.";
			var houseBill = oceanBill.HouseBills.AddNew();

			var creSender = GetNewSender(TSWTransactionTypes.Original);
			AssertContains(expectedError, creSender.Errors);
			houseBill.CA_ConsigneeName = "CONSIGNEE";
			creSender = GetNewSender(TSWTransactionTypes.Original);
			AssertNotContains(expectedError, creSender.Errors);
		}

		public void TestStatusTransactionScopeRollback()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = staff.GetNZWrapper();
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(staff.PK.ToGuid()).Encrypt("XX13ZZ");

			oceanBill.CB_MessageReference = "X00002093";
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_IsSAC = true;
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "10987347N"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var creSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => creSender.SendMessage());

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
			return new SendCREFromOceanBill(oceanBill, null, transactionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();
		}

		CusSCAOceanBill oceanBill;
	}
}
