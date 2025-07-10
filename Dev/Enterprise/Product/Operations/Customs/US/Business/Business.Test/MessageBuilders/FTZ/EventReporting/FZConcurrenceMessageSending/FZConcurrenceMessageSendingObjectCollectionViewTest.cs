using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	[TestedType(typeof(FZConcurrenceMessageSendingObjectCollectionView))]
	sealed class FZConcurrenceMessageSendingObjectCollectionViewTest : NonPersistentBusinessObjectCollectionViewTestCase<FZConcurrenceMessageSendingObjectCollectionView>
	{
		public void TestFilterBy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "123456789";
			bill1.CU_NoOfPacks = 12m;
			var bill1IT1 = bill1.ITAndSplitDetails.AddNew();
			bill1IT1.US_ITNumber = "6005006";
			var bill1IT2 = bill1.ITAndSplitDetails.AddNew();
			bill1IT2.US_ITNumber = "6005007";
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "223456789";
			bill2.CU_NoOfPacks = 16m;
			var bill2IT1 = bill2.ITAndSplitDetails.AddNew();
			bill2IT1.US_ITNumber = "6005008";
			var bill2IT2 = bill2.ITAndSplitDetails.AddNew();
			bill2IT2.US_ITNumber = "6005009";
			var action = new FZEventAction(declaration, FZEventType.Concur);
			var view = action.MessageSendingObjectsView;
			AssertType<FZConcurrenceMessageSendingObjectCollectionView>(view);
			view.FilterBy = ZString.Empty;
			AssertEquals(0, view.Count);
			view.FilterBy = FTZActionCodeList.Codes.A;
			AssertEquals(1, view.Count);
			view.FilterBy = FTZActionCodeList.Codes.C;
			AssertEquals(4, view.Count);
			view.FilterBy = FTZActionCodeList.Codes.B;
			AssertEquals(2, view.Count);
		}

		protected override FZConcurrenceMessageSendingObjectCollectionView GetCollectionToTest() => new FZConcurrenceMessageSendingObjectCollectionView(EventAction);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new FZConcurrenceMessageSendingObject(new FTZConcurrenceForTesting(Factory));

		FZEventAction EventAction
		{
			get
			{
				if (eventAction == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					eventAction = new FZEventAction(declaration, FZEventType.Concur);
				}
				return eventAction;
			}
		}
		FZEventAction eventAction;
	}
}
