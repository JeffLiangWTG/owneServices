
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(ConfirmationCollection_PackageView))]
	public class ConfirmationCollection_PackageViewTest : NonPersistentBusinessObjectCollectionTestCase<ConfirmationCollection_PackageView>
	{
		public void TestAddRemove()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "PIC");
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);
			var collection = new ConfirmationCollection_PackageView(packageView); //packageView.Confirmations_View.Count;
			collection.Initialise();
			AssertEquals(0, collection.Count);

			var divot1 = Helper.CreatePackageDivot(instruction, package, 1);
			AssertEquals(0, collection.Count);

			var instructionConfirmation = Helper.CreateConfirmation(instruction, "PIC");
			AssertEquals(1, collection.Count);

			var divotConfirmation = Helper.CreateConfirmation(divot1, "CON");
			AssertEquals(2, collection.Count);

			divotConfirmation.Delete();
			AssertEquals(1, collection.Count);

			instructionConfirmation.Delete();
			AssertEquals(0, collection.Count);
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(true, collection.AllowNew);
		}

		public void TestCreateNonPersistentBusinessObject()
		{
			var booking = Factory.New<DtbBooking>();
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);
			var collection = new ConfirmationCollection_PackageView(packageView);
			var confirmationView = collection.AddNew();
			AssertNotNull(confirmationView);
			AssertNotNull(confirmationView.Confirmation);
			AssertEquals(package, confirmationView.Package);
		}

		protected override ConfirmationCollection_PackageView GetCollectionToTest()
		{
			var booking = Factory.New<DtbBooking>();
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);

			return new ConfirmationCollection_PackageView(packageView);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var booking = Factory.New<DtbBooking>();
			var confirmation = Factory.New<DtbBookingConfirmation>();
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);

			return new Confirmation_PackageView(confirmation, packageView);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
