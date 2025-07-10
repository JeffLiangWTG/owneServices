using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketLineCollectionTestCase<T> : WhsActiveBusinessObjectCollectionTestCaseWithHelper<T> where T : WhsDocketLineCollection
	{
		#region TestAllowRemove

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			var bindingList = (IBindingList)collection;
			var docket = collection.Docket;

			docket.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals("Status New - AllowRemove", true, bindingList.AllowRemove);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Status Entered - AllowRemove", true, bindingList.AllowRemove);

			docket.WD_DocketStatus = DocketStatus.Codes.Held;
			AssertEquals("Status Held - AllowRemove", true, bindingList.AllowRemove);

			docket.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertEquals("Status Picking", ExpectedAllowAddOrRemoveForPickedOrder, bindingList.AllowRemove);

			docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertEquals("Status AttachedToPick", ExpectedAllowAddOrRemoveForPickedOrder, bindingList.AllowRemove);

			docket.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals("Status Putaway", ExpectedAllowAddOrRemoveForPutawayDocket, bindingList.AllowRemove);

			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals("Status Finalised - Dont AllowRemove", false, bindingList.AllowRemove);

			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals("Status Cancelled - AllowRemove", false, bindingList.AllowRemove);

			docket.Delete();
			AssertEquals("Docket Deleted - AllowRemove", true, bindingList.AllowRemove);
		}

		protected virtual bool ExpectedAllowAddOrRemoveForPickedOrder => false;

		protected virtual bool ExpectedAllowAddOrRemoveForPutawayDocket => false;

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			var docket = collection.Docket;
			docket.WD_OH_Client = ZGuid.Empty;
			docket.WD_WW_Whs = ZGuid.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.New;

			var bindingList = (IBindingList)collection;
			AssertEquals("Client invalid, Warehouse invalid, docket status valid, so should not allow new", false, bindingList.AllowNew);

			docket.WD_OH_Client = ZGuid.NewZGuid();
			AssertEquals("Client valid, Warehouse invalid, docket status valid, so should not allow new", false, bindingList.AllowNew);

			docket.WD_WW_Whs = ZGuid.NewZGuid();
			AssertEquals("Client valid, Warehouse valid, docket status valid, so should allow new", true, bindingList.AllowNew);

			docket.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals("Client valid, Warehouse valid, docket status valid, so should allow new", true, bindingList.AllowNew);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Client valid, Warehouse valid, docket status valid, so should allow new", true, bindingList.AllowNew);

			docket.WD_DocketStatus = DocketStatus.Codes.Held;
			AssertEquals("Client valid, Warehouse valid, docket status valid, so should allow new", true, bindingList.AllowNew);

			docket.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertEquals("Status Picking", ExpectedAllowAddOrRemoveForPickedOrder, bindingList.AllowNew);

			docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertEquals("Status AttachedToPick", ExpectedAllowAddOrRemoveForPickedOrder, bindingList.AllowNew);

			docket.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals("Client valid, Warehouse valid, docket status valid, so should allow new", ExpectedAllowAddOrRemoveForPutawayDocket, bindingList.AllowNew);

			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals("Client valid, Warehouse valid, docket status invalid, so should not allow new", false, bindingList.AllowNew);
		}

		#endregion

		#region TestSetDefaultsForNewChild

		public virtual void TestSetDefaultsForNewChild() => Assert(true);

		#endregion

		#region TestOnAdded

		public void TestOnAdded()
		{
			var collection = GetCollectionToTest();
			collection.AddNew();
			AssertEquals((ZShort)1, collection[0].WE_LineNo);
			collection.AddNew();
			AssertEquals((ZShort)2, collection[1].WE_LineNo);
			collection.RemoveFromRelationship(collection[0]);

			collection.AddNew();
			AssertEquals((ZShort)3, collection[1].WE_LineNo);
			collection.RemoveFromRelationship(collection[1]);

			var docket = collection.Docket;
			var line1 = collection.AddNew();
			line1.WE_WD = docket.PK;
			line1.WE_LineNo = 45;

			var addedLine = collection.AddNew();
			AssertEquals((ZShort)46, addedLine.WE_LineNo);

			var line2 = (WhsDocketLine)GetNewElementToAddToTheCollection();
			line2.WE_LineNo = 23;
			collection.Add(line2);
			AssertEquals((ZShort)23, line2.WE_LineNo);
		}

		#endregion

		#region TestOnAdded_ArithmeticOverflow

		public void TestOnAdded_ArithmeticOverflow()
		{
			var collection = GetCollectionToTest();
			var line1 = collection.AddNew();
			line1.WE_LineNo = short.MaxValue;

			AssertNoExceptionThrown("Line number should not go over maximum value.", () =>
			{
				var line2 = collection.AddNew();
				AssertEquals("When line number cannot be assigned it should be left as 0.", ZShort.Zero, line2.WE_LineNo);
			});
		}

		#endregion

		#region TestLineNoCalculation

		public virtual void TestLineNoCalculation()
		{
			var collection = GetCollectionToTest();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.ApplySort(WhsDocketLineSchema.Constants.WE_LineNo, ListSortDirection.Descending);

			collection.AddNew();
			AssertEquals((ZShort)1, collection[3].WE_LineNo);
		}

		#endregion

		#region TestDocket

		public void TestDocket() => TestDocketCore();

		protected virtual void TestDocketCore()
		{
			var collection = GetCollectionToTest();
			Assert("Docket == Master in base", object.ReferenceEquals(collection.Relationship.Master, collection.Docket));
		}

		#endregion

		#region TestLocationStringSortedProperly

		public void TestLocationStringSortedProperly()
		{
			TestLocationSortedProperlyCore(nameof(WhsDocketLine.LocationString));
		}

		public void TestWE_WLSortedProperly()
		{
			TestLocationSortedProperlyCore(nameof(WhsDocketLine.WE_WL));
		}

		protected abstract void TestLocationSortedProperlyCore(string locationPropertyToSort);

		public void TestTransferFromLocationStringSortedProperly()
		{
			TestTransferFromLocationStringSortedProperlyCore(nameof(WhsTransferLine.TransferFromLocationString));
		}

		public void TestWE_WL_TransferFromSortedProperly()
		{
			TestTransferFromLocationStringSortedProperlyCore(nameof(WhsDocketLine.WE_WL_TransferFrom));
		}

		protected abstract void TestTransferFromLocationStringSortedProperlyCore(string locationPropertyToSort);

		#endregion
	}
}
