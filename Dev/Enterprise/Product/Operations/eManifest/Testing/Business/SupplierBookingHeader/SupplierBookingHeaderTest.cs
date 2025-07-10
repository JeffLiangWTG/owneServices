
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.eManifest.Business.Testing
{
	[TestedType(typeof(SupplierBookingHeader))]
	class SupplierBookingHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJobNumber()
		{
			var booking = Factory.New<SupplierBookingHeader>();
			AssertEquals(" / {none}", booking.JobNumber);

			booking.DH_SupplierReference = "SUPREF01";
			AssertEquals("SUPREF01 / {none}", booking.JobNumber);

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "BENTEST012";
			booking.DH_OA_Consignor = supplier.MainAddress.PK;
			AssertEquals("SUPREF01 / BENTEST012", booking.JobNumber);
		}

		public void TestDocumentSupporter()
		{
			var header = Factory.New<SupplierBookingHeader>();
			AssertEquals("Document Supporter should be of type", typeof(SupplierBookingHeaderDocumentSupporter), header.DocumentSupporter.GetType());
		}

		public void TestSetSupplierReference()
		{
			var header1 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			AssertEquals("PRE: no supplier reference", string.Empty, header1.DH_SupplierReference);
			AssertEquals("PRE: ShouldSetSupplierReference is false", false, header1.ShouldSetSupplierReference);
			Factory.Save();

			header1 = Factory.Load<SupplierBookingHeader>(header1.PK);
			AssertEquals("There should not be supplier reference", string.Empty, header1.DH_SupplierReference);

			var header2 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header2.ShouldSetSupplierReference = true;
			AssertEquals("PRE: no supplier reference", string.Empty, header2.DH_SupplierReference);
			Factory.Save();

			header2 = Factory.Load<SupplierBookingHeader>(header2.PK);
			AssertEquals("There should be a supplier reference", "M00000001", header2.DH_SupplierReference);

			var header3 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header3.DH_SupplierReference = "TESTSR";
			header3.ShouldSetSupplierReference = true;
			Factory.Save();

			header3 = Factory.Load<SupplierBookingHeader>(header3.PK);
			AssertEquals("Supplier reference should not be assigned a new one", "TESTSR", header3.DH_SupplierReference);

			header3.DH_SupplierReference = string.Empty;
			Factory.Save();

			header3 = Factory.Load<SupplierBookingHeader>(header3.PK);
			AssertEquals("Supplier reference should not be assigned as header already exists", string.Empty, header3.DH_SupplierReference);
		}

		public void TestUpdateSupplierBookingLineStatus()
		{
			var header1 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			Assert("PRE: DH_IsShipperApproved is false", !header1.DH_IsShipperApproved);

			var line1 = header1.BookingLines.AddNew();
			AssertEquals("PRE: DL_Status is empty", ZString.Empty, line1.DL_Status);

			var line2 = header1.BookingLines.AddNew();
			line2.DL_Status = Constants.SupplierBookingLineStatus.Codes.Incomplete;
			Factory.Save();

			var factory = Factory.CreateNewFactory();

			line1 = factory.Load<SupplierBookingLine>(line1.PK);
			AssertEquals("line1 DL_Status should not be changed", ZString.Empty, line1.DL_Status);

			line2 = factory.Load<SupplierBookingLine>(line2.PK);
			AssertEquals("line2 DL_Status should not be changed", Constants.SupplierBookingLineStatus.Codes.Incomplete, line2.DL_Status);

			header1.DH_IsShipperApproved = true;
			Factory.Save();

			factory = Factory.CreateNewFactory();

			line1 = factory.Load<SupplierBookingLine>(line1.PK);
			AssertEquals(SupplierBookingLineSchema.DL_Status.Name + " should be Consolidated", Constants.SupplierBookingLineStatus.Codes.Confirmed, line1.DL_Status);

			line2 = factory.Load<SupplierBookingLine>(line2.PK);
			AssertEquals(SupplierBookingLineSchema.DL_Status.Name + " should be Consolidated", Constants.SupplierBookingLineStatus.Codes.Confirmed, line2.DL_Status);
		}
	}
}


