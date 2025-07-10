using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	[TestedType(typeof(FZEventAction))]
	sealed class FZEventActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetReasonsCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._01;
			AssertNull(action.GetReasonsCaption());
		}

		public void TestUpdatedMB_ConcurrenceQtyWhenActionCodeA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 20m;
			var action = new FZEventAction(declaration, FZEventType.Concur);

			action.US_ActionCode = FTZActionCodeList.Codes.A;
			var sendingObjs = action.GetMessageSendingObjectsNeedSending();
			AssertEquals(1, sendingObjs.Length);
			AssertEquals(true, sendingObjs[0].MB_Send);
			AssertEquals(FTZActionCodeList.Codes.A, sendingObjs[0].MB_US_ActionCode);
			AssertEquals("FTZConcurrenceQty is 20", 20m, sendingObjs[0].MB_ConcurrenceQty);

			var clonedDeclaration = (JobDeclaration)declaration.TemplateCopy();
			var action2 = new FZEventAction(clonedDeclaration, FZEventType.Concur);
			action2.US_ActionCode = FTZActionCodeList.Codes.A;
			var sendingObjs2 = action2.GetMessageSendingObjectsNeedSending();
			AssertEquals(1, sendingObjs2.Length);
			AssertEquals(true, sendingObjs2[0].MB_Send);
			AssertEquals(FTZActionCodeList.Codes.A, sendingObjs2[0].MB_US_ActionCode);
			AssertEquals("FTZConcurrenceQty is 0", 0m, sendingObjs2[0].MB_ConcurrenceQty);

			var bill2 = clonedDeclaration.Bills.AddNew();
			bill2.CU_BillNum = "BB3322";
			bill2.CU_NoOfPacks = 30m;

			clonedDeclaration.Factory.Save();

			var action3 = new FZEventAction(clonedDeclaration, FZEventType.Concur);
			action3.US_ActionCode = FTZActionCodeList.Codes.A;
			var sendingObjs3 = action3.GetMessageSendingObjectsNeedSending();

			AssertEquals(1, sendingObjs3.Length);
			AssertEquals(true, sendingObjs3[0].MB_Send);
			AssertEquals(FTZActionCodeList.Codes.A, sendingObjs3[0].MB_US_ActionCode);
			AssertEquals("FTZConcurrenceQty is 0", 30m, sendingObjs3[0].MB_ConcurrenceQty);
		}

		public void TestGetMessageSendingObjectsNeedSending()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
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
			itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V1006003";

			var action = new FZEventAction(declaration, FZEventType.Concur);

			action.US_ActionCode = FTZActionCodeList.Codes.A;
			var sendingObjs = action.GetMessageSendingObjectsNeedSending();
			AssertEquals(1, sendingObjs.Length);
			AssertEquals(true, sendingObjs[0].MB_Send);
			AssertEquals(FTZActionCodeList.Codes.A, sendingObjs[0].MB_US_ActionCode);
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.A)
				{
					obj.MB_Send = false;
				}
			}
			sendingObjs = action.GetMessageSendingObjectsNeedSending();
			AssertEquals(0, sendingObjs.Length);

			action.US_ActionCode = FTZActionCodeList.Codes.B;
			sendingObjs = action.GetMessageSendingObjectsNeedSending();
			AssertEquals(2, sendingObjs.Length);
			AssertEquals(true, sendingObjs[0].MB_Send);
			AssertEquals(true, sendingObjs[1].MB_Send);
			AssertEquals(FTZActionCodeList.Codes.B, sendingObjs[0].MB_US_ActionCode);
			AssertEquals(FTZActionCodeList.Codes.B, sendingObjs[1].MB_US_ActionCode);
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.B && obj.MB_Identifier == "222256789")
				{
					obj.MB_Send = false;
				}
			}
			sendingObjs = action.GetMessageSendingObjectsNeedSending();
			AssertEquals(1, sendingObjs.Length);
			AssertEquals(true, sendingObjs[0].MB_Send);
			AssertEquals(FTZActionCodeList.Codes.B, sendingObjs[0].MB_US_ActionCode);
			AssertEquals("123456789", sendingObjs[0].MB_Identifier);

			action.US_ActionCode = FTZActionCodeList.Codes.C;
			sendingObjs = action.GetMessageSendingObjectsNeedSending();
			AssertEquals(2, sendingObjs.Length);
			AssertEquals(true, sendingObjs[0].MB_Send);
			AssertEquals(true, sendingObjs[1].MB_Send);
			AssertEquals(FTZActionCodeList.Codes.C, sendingObjs[0].MB_US_ActionCode);
			AssertEquals(FTZActionCodeList.Codes.C, sendingObjs[1].MB_US_ActionCode);
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.C && obj.MB_Identifier == "V1006003")
				{
					obj.MB_Send = false;
				}
			}
			sendingObjs = action.GetMessageSendingObjectsNeedSending();
			AssertEquals(1, sendingObjs.Length);
			AssertEquals(true, sendingObjs[0].MB_Send);
			AssertEquals(FTZActionCodeList.Codes.C, sendingObjs[0].MB_US_ActionCode);
			AssertEquals("6005006", sendingObjs[0].MB_Identifier);
		}

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
			bill.CU_NoOfPacks = 1m;

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "222256789";
			bill2.CU_NoOfPacks = 2m;
			var itNo1bill = bill.ITAndSplitDetails.AddNew();
			itNo1bill.US_ITNumber = "6005006";
			itNo1bill.US_NoOfPacks = 4;
			var itNo2bill = bill.ITAndSplitDetails.AddNew();
			itNo2bill.US_ITNumber = "V1006003";
			itNo2bill.US_NoOfPacks = 5;
			var itNo1bill2 = bill2.ITAndSplitDetails.AddNew();
			itNo1bill2.US_ITNumber = "V2006003";
			itNo1bill2.US_NoOfPacks = 6;

			var action = new FZEventAction(declaration, FZEventType.Concur);
			var sendingObjs = action.MessageSendingObjects.Cast<FZConcurrenceMessageSendingObject>().ToList();
			CombineAssertions("Action A", () =>
			{
				sendingObjs.ForEach(x =>
				{
					x.MB_ConcurrenceQty = x.MB_ConcurrenceQty * 10;
				});
				action.US_ActionCode = FTZActionCodeList.Codes.A;
				var obj = action.GetMessageSendingObjectsNeedSending().Single();
				AssertEquals(30m, obj.MB_ConcurrenceQty);
				obj.MB_Send = false;
				action.UpdateFTZConcurrenceQty();
				AssertEquals("declaration.US_FTZConcurrenceQty", ZDecimal.Zero, declaration.US_FTZConcurrenceQty);
				AssertEquals("bill.US_FTZConcurrenceQty", ZDecimal.Zero, bill.US_FTZConcurrenceQty);
				AssertEquals("bill2.US_FTZConcurrenceQty", ZDecimal.Zero, bill2.US_FTZConcurrenceQty);
				AssertEquals("itNo1bill.US_FTZConcurrenceQty", ZDecimal.Zero, itNo1bill.US_FTZConcurrenceQty);
				AssertEquals("itNo2bill.US_FTZConcurrenceQty", ZDecimal.Zero, itNo2bill.US_FTZConcurrenceQty);
				AssertEquals("itNo1bill2.US_FTZConcurrenceQty", ZDecimal.Zero, itNo1bill2.US_FTZConcurrenceQty);
				obj.MB_Send = true;
				action.UpdateFTZConcurrenceQty();
				AssertEquals("declaration.US_FTZConcurrenceQty", 30m, declaration.US_FTZConcurrenceQty);
				AssertEquals("bill.US_FTZConcurrenceQty", ZDecimal.Zero, bill.US_FTZConcurrenceQty);
				AssertEquals("bill2.US_FTZConcurrenceQty", ZDecimal.Zero, bill2.US_FTZConcurrenceQty);
				AssertEquals("itNo1bill.US_FTZConcurrenceQty", ZDecimal.Zero, itNo1bill.US_FTZConcurrenceQty);
				AssertEquals("itNo2bill.US_FTZConcurrenceQty", ZDecimal.Zero, itNo2bill.US_FTZConcurrenceQty);
				AssertEquals("itNo1bill2.US_FTZConcurrenceQty", ZDecimal.Zero, itNo1bill2.US_FTZConcurrenceQty);
			});
			CombineAssertions("Action B", () =>
			{
				sendingObjs.ForEach(x =>
				{
					x.MB_ConcurrenceQty = x.MB_ConcurrenceQty * 10;
				});
				action.US_ActionCode = FTZActionCodeList.Codes.B;
				var objs = action.GetMessageSendingObjectsNeedSending();
				AssertEquals(2, objs.Length);
				var obj1 = objs[0];
				AssertEquals("obj1.MB_ConcurrenceQty", 100m, obj1.MB_ConcurrenceQty);
				var obj2 = objs[1];
				AssertEquals("obj2.MB_ConcurrenceQty", 200m, obj2.MB_ConcurrenceQty);
				obj1.MB_Send = false;
				action.UpdateFTZConcurrenceQty();
				AssertEquals("declaration.US_FTZConcurrenceQty", 30m, declaration.US_FTZConcurrenceQty);
				AssertEquals("bill.US_FTZConcurrenceQty", ZDecimal.Zero, bill.US_FTZConcurrenceQty);
				AssertEquals("bill2.US_FTZConcurrenceQty", 200m, bill2.US_FTZConcurrenceQty);
				AssertEquals("itNo1bill.US_FTZConcurrenceQty", ZDecimal.Zero, itNo1bill.US_FTZConcurrenceQty);
				AssertEquals("itNo2bill.US_FTZConcurrenceQty", ZDecimal.Zero, itNo2bill.US_FTZConcurrenceQty);
				AssertEquals("itNo1bill2.US_FTZConcurrenceQty", ZDecimal.Zero, itNo1bill2.US_FTZConcurrenceQty);
			});
			CombineAssertions("Action C", () =>
			{
				sendingObjs.ForEach(x =>
				{
					x.MB_ConcurrenceQty = x.MB_ConcurrenceQty * 10;
				});
				action.US_ActionCode = FTZActionCodeList.Codes.C;
				var objs = action.GetMessageSendingObjectsNeedSending();
				AssertEquals(3, objs.Length);
				var obj1 = objs[0];
				AssertEquals("obj1.MB_ConcurrenceQty", 6000m, obj1.MB_ConcurrenceQty);
				var obj2 = objs[1];
				AssertEquals("obj2.MB_ConcurrenceQty", 4000m, obj2.MB_ConcurrenceQty);
				var obj3 = objs[2];
				AssertEquals("obj3.MB_ConcurrenceQty", 5000m, obj3.MB_ConcurrenceQty);
				obj2.MB_Send = false;
				action.UpdateFTZConcurrenceQty();
				AssertEquals("declaration.US_FTZConcurrenceQty", 30m, declaration.US_FTZConcurrenceQty);
				AssertEquals("bill.US_FTZConcurrenceQty", ZDecimal.Zero, bill.US_FTZConcurrenceQty);
				AssertEquals("bill2.US_FTZConcurrenceQty", 200m, bill2.US_FTZConcurrenceQty);
				AssertEquals("itNo1bill.US_FTZConcurrenceQty", ZDecimal.Zero, itNo1bill.US_FTZConcurrenceQty);
				AssertEquals("itNo2bill.US_FTZConcurrenceQty", 5000m, itNo2bill.US_FTZConcurrenceQty);
				AssertEquals("itNo1bill2.US_FTZConcurrenceQty", 6000m, itNo1bill2.US_FTZConcurrenceQty);
			});

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._02;
			AssertEquals("Replacement In Bond Number", action.GetReasonsCaption().Caption);

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._03;
			AssertEquals("Replacement FTZ Admission Number", action.GetReasonsCaption().Caption);

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._04;
			AssertEquals("Replacement Entry Number", action.GetReasonsCaption().Caption);

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._05;
			AssertEquals("Replacement Entry Number", action.GetReasonsCaption().Caption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			return new FZEventAction(declaration, FZEventType.Concur);
		}
	}
}
