using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(PkgPalletTransactionConfirmationDependentCollection))]
	sealed class PkgPalletTransactionConfirmationDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<PkgPalletTransactionConfirmationDependentCollection>
	{
		public void TestQuery_ParentProvidesPallets()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var parent = runSheet.RunSheetInstructions.AddNew();
			var consignment = Helper.CreateBookingConsignmentWithTemplate();

			var otherConsignment = Helper.CreateBookingConsignmentWithTemplate();
			var otherConfirmation = otherConsignment.PickupInstruction.PickupConfirmation;

			parent.Confirmations.Add(consignment.PickupInstruction.PickupConfirmation);

			var palletTrx1 = Factory.New<PkgPalletTransaction>();
			palletTrx1.RelatedJob = parent;

			var palletTrx2 = Factory.New<PkgPalletTransaction>();
			palletTrx2.RelatedJob = consignment.PickupInstruction.PickupConfirmation;

			var palletTrx3 = Factory.New<PkgPalletTransaction>();
			palletTrx3.RelatedJob = otherConfirmation;

			var collection = new PkgPalletTransactionConfirmationDependentCollection(parent);
			AssertCollectionContains(palletTrx1, collection);
			AssertCollectionContains(palletTrx2, collection);
			AssertCollectionNotContains(palletTrx3, collection);
		}

		public void TestQuery_ParentDoesNotProvidePallets()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();

			var otherConsignment = Helper.CreateBookingConsignmentWithTemplate();
			var otherConfirmation = otherConsignment.PickupInstruction.PickupConfirmation;

			var palletTrx1 = Factory.New<PkgPalletTransaction>();
			palletTrx1.RelatedJob = consignment.PickupInstruction.PickupConfirmation;

			var palletTrx2 = Factory.New<PkgPalletTransaction>();
			palletTrx2.RelatedJob = otherConfirmation;

			var collection = new PkgPalletTransactionConfirmationDependentCollection(consignment);
			AssertCollectionContains(palletTrx1, collection);
			AssertCollectionNotContains(palletTrx2, collection);
		}

		public void TestAddNew_ParentProvidesPallets()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var parent = runSheet.RunSheetInstructions.AddNew();
			var collection = new PkgPalletTransactionConfirmationDependentCollection(parent);
			var palletTrx = collection.AddNew();
			AssertEquals(parent.PK, palletTrx.KTR_ParentID);
			AssertEquals(parent.TablePrefix, palletTrx.KTR_ParentTableCode);
		}

		public void TestAddNew_ParentDoesNotProvidePallets()
		{
			var parent = Factory.New<DtbBookingConsignment>();
			var collection = new PkgPalletTransactionConfirmationDependentCollection(parent);
			var palletTrx = collection.AddNew();
			AssertEquals(ZGuid.Empty, palletTrx.KTR_ParentID);
			AssertEquals(ZString.Empty, palletTrx.KTR_ParentTableCode);
		}

		public void TestLoaded_ParentProvidesPallets()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var parent = runSheet.RunSheetInstructions.AddNew();
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var confirmation = consignment.PickupInstruction.PickupConfirmation;
			parent.Confirmations.Add(confirmation);

			var collection = new PkgPalletTransactionConfirmationDependentCollection(parent);
			var palletTrx = collection.AddNew();
			AssertEquals(2, palletTrx.PossibleParents.Count());
			AssertCollectionContains(parent, palletTrx.PossibleParents);
			AssertCollectionContains(confirmation, palletTrx.PossibleParents);
		}

		public void TestLoaded_ParentDoesNotProvidePallets()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var collection = new PkgPalletTransactionConfirmationDependentCollection(consignment);
			var palletTrx = collection.AddNew();
			AssertEquals(2, palletTrx.PossibleParents.Count());
			AssertCollectionContains(consignment.PickupInstruction.PickupConfirmation, palletTrx.PossibleParents);
			AssertCollectionContains(consignment.DeliveryInstruction.DeliveryConfirmation, palletTrx.PossibleParents);
		}

		public void TestOnAdded_ParentIsPalletProvider()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var parent = runSheet.RunSheetInstructions.AddNew();
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var confirmation = consignment.PickupInstruction.PickupConfirmation;
			parent.Confirmations.Add(confirmation);

			var collection = new PkgPalletTransactionConfirmationDependentCollection(parent);
			var newItem = collection.AddNew();
			AssertEquals(parent, newItem.RelatedJob);
		}

		public void TestOnAdded_ParentIsNotPalletProvider()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var collection = new PkgPalletTransactionConfirmationDependentCollection(consignment);
			var newItem = collection.AddNew();
			AssertEquals(consignment.PickupInstruction.PickupConfirmation, newItem.RelatedJob);
		}

		public void TestOnAdded_ParentIsNotPalletProvider_NoConfirmations()
		{
			var parent = Factory.New<DtbBookingConsignment>();

			var collection = new PkgPalletTransactionConfirmationDependentCollection(parent);
			var newItem = collection.AddNew();
			AssertEquals(null, newItem.RelatedJob);
		}

		public override void TestAddNew()
		{
			var initialCount = Collection.Count;

			var bizo1 = GetNewElementToAddToTheCollection();
			var bizo2 = GetNewElementToAddToTheCollection();

			AssertEquals("Collection count", initialCount + 2, Collection.Count);
			Assert("Contains new elements", Collection.Contains(bizo1));
			Assert("Contains new elements", Collection.Contains(bizo2));
		}

		protected override PkgPalletTransactionConfirmationDependentCollection GetCollectionToTest()
		{
			return new PkgPalletTransactionConfirmationDependentCollection(Factory.New<DtbBookingConsignment>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var confirmation = consignment.PickupInstruction.PickupConfirmation;

			var newElement = (PkgPalletTransaction)base.GetNewElementToAddToTheCollection();
			newElement.RelatedJob = confirmation;
			return newElement;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(DtbBookingConfirmationSchema.Constants.TableName);
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
