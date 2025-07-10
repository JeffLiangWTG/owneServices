using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPalletTransactionLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestParents

		public void TestParents()
		{
			var dummy1 = (IPalletTransactionParent)Factory.New<DummyPalletMaster>();
			var dummy2 = (IPalletTransactionParent)Factory.New<DummyPalletMaster>();
			var pallet = Factory.New<PkgPalletTransaction>();

			AssertNull(pallet.PossibleParents);
			AssertEquals(0, pallet.Lookups.ParentList.Count);

			pallet.PossibleParents = new[] { dummy1, dummy2 };
			AssertEquals(2, pallet.PossibleParents.Count());
			AssertEquals(2, pallet.Lookups.ParentList.Count);
			AssertEquals(dummy1.PK, pallet.Lookups.ParentList[0].PK);
			AssertEquals("ZAYRYALELOLA", pallet.Lookups.ParentList[0].Code);

			AssertEquals(dummy2.PK, pallet.Lookups.ParentList[1].PK);
			AssertEquals("ZAYRYALELOLA", pallet.Lookups.ParentList[1].Code);

			pallet.Context = Factory.New<DummyPivot>();
			AssertEquals("ZAYRYALELOLA Pivot", pallet.Lookups.ParentList[1].Code);

			pallet.Context = Factory.New<DummyWithPacking>();
			AssertEquals("ZAYRYALELOLA Packing", pallet.Lookups.ParentList[1].Code);

			pallet.Context = Factory.New<DummyWithValidation>();
			AssertEquals("ZAYRYALELOLA", pallet.Lookups.ParentList[1].Code);
		}

		#endregion

		#region TestTransferTypes

		public void TestTransferTypes()
		{
			var pallet = Factory.New<PkgPalletTransaction>();
			AssertEquals("Precondition", 2, pallet.Lookups.TransactionTypes.Count);
			AssertContainsExactElementsInAnyOrder("Precondition", new PalletTransactionTypeList(), pallet.Lookups.TransactionTypes);

			pallet.KTR_TransferType = PalletTransactionTypeList.Codes.Transfer;
			AssertContainsExactElementsInAnyOrder(new PalletTransferTypeList(), pallet.Lookups.TransferTypes);

			pallet.KTR_TransferType = PalletTransactionTypeList.Codes.Exchange;
			AssertContainsExactElementsInAnyOrder(new PalletExchangeTypeList(), pallet.Lookups.TransferTypes);
		}

		#endregion

		#region TestActionTypes

		public void TestActionTypes_TransferPallet()
		{
			var transferPallet = Factory.New<PkgPalletTransaction>();
			transferPallet.KTR_TransactionType = PalletTransactionTypeList.Codes.Transfer;
			transferPallet.KTR_TransferType = PalletTransferTypeList.Codes.Direct;
			Assert("Precondition", transferPallet.IsTransfer);
			AssertEquals(ActionTypeList.Codes.Direct, transferPallet.Lookups.ActionTypes.CodesAsString);

			var expectedActionTypes = $"{ActionTypeList.Codes.PickUp}, {ActionTypeList.Codes.Deliver}";
			transferPallet.KTR_TransferType = PalletTransferTypeList.Codes.TransferOn;
			AssertEquals(expectedActionTypes, transferPallet.Lookups.ActionTypes.CodesAsString);

			transferPallet.KTR_TransferType = PalletTransferTypeList.Codes.TransferOff;
			AssertEquals(expectedActionTypes, transferPallet.Lookups.ActionTypes.CodesAsString);

			transferPallet.KTR_TransferType = "";
			AssertEquals(expectedActionTypes, transferPallet.Lookups.ActionTypes.CodesAsString);
		}

		public void TestActionTypes_ExchangePallet()
		{
			var transferPallet = Factory.New<PkgPalletTransaction>();
			transferPallet.KTR_TransactionType = PalletTransactionTypeList.Codes.Exchange;
			transferPallet.KTR_TransferType = PalletExchangeTypeList.Codes.DirectExchange;
			Assert("Precondition", !transferPallet.IsTransfer);

			var expectedActionTypes = $"{ActionTypeList.Codes.PickUp}, {ActionTypeList.Codes.Deliver}";
			AssertEquals(expectedActionTypes, transferPallet.Lookups.ActionTypes.CodesAsString);

			transferPallet.KTR_TransferType = PalletExchangeTypeList.Codes.DeferredExchange;
			AssertEquals(expectedActionTypes, transferPallet.Lookups.ActionTypes.CodesAsString);

			transferPallet.KTR_TransferType = "";
			AssertEquals(expectedActionTypes, transferPallet.Lookups.ActionTypes.CodesAsString);
		}

		#endregion
	}
}
