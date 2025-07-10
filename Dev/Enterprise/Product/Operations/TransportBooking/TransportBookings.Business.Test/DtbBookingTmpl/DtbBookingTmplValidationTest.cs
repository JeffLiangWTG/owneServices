using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingTmplValidationTest : DtbTransportTmplValidationTest
	{
		public void TestKT_Code()
		{
			var template = Helper.CreateTransportBookingTemplate("", "FCL Import", Constants.CartageDirection.Import);
			AssertHasError(template.KT_CodeInfo, "Please enter a Code.");

			template.KT_Code = "I";
			AssertHasError(template.KT_CodeInfo, "Transport Booking Template Code needs to be 4 characters");

			template.KT_Code = "IFCL";
			AssertNoErrors(template.KT_CodeInfo);

			Factory.Save();

			var newTemplateWithSameCode = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			AssertHasError(newTemplateWithSameCode.KT_CodeInfo, "Another Transport Booking Template is already using the Code 'IFCL'");
		}

		public void TestKT_Description()
		{
			var template = Helper.CreateTransportBookingTemplate("IMP", "", Constants.CartageDirection.Import);
			AssertHasError(template.KT_DescriptionInfo, "Please enter a Description.");

			template.KT_Description = "Desc";
			AssertNoErrors(template.KT_DescriptionInfo);
		}

		public void TestKT_Direction()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", "");
			AssertHasError(template.KT_DirectionInfo, "Please enter a Direction.");

			template.KT_Direction = "XXX";
			AssertHasError(template.KT_DirectionInfo, "Enter a valid Direction.");

			template.KT_Direction = Constants.CartageDirection.Import;
			AssertNoErrors(template.KT_DirectionInfo);
		}

		public void TestKT_IsActive()
		{
			const string errorMsg = "This Template is set as a default template in Registry setting 'Job Template Defaults' and therefore cannot be made Inactive.";

			var registryBookingTmpl = Factory.LoadTop1<DtbBookingTmpl>(new ZQuery(DtbBookingTmplSchema.KT_Code, SQLComparisonOperator.Equal, TransportRegistry.Instance.JobTemplateDefault.Value[0].BookingTemplate));
			registryBookingTmpl.KT_IsActive = false;
			AssertHasError(registryBookingTmpl.KT_IsActiveInfo, errorMsg);

			var bookingTmpl = Factory.NewWithValidTestData<DtbBookingTmpl>();
			bookingTmpl.KT_Code = "ABCD";
			bookingTmpl.KT_IsActive = true;
			AssertNoError(bookingTmpl.KT_IsActiveInfo, errorMsg);

			bookingTmpl.KT_IsActive = false;
			AssertNoError(bookingTmpl.KT_IsActiveInfo, errorMsg);
		}

		public void TestValidateInstructions()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", "");
			template.Validation.ValidateAll();
			AssertHasRowError(template, "Transport Booking Template requires at least 2 instructions.");

			template.Instructions.AddNew();
			template.Validation.ValidateAll();
			AssertHasRowError(template, "Transport Booking Template requires at least 2 instructions.");

			template.Instructions.AddNew();
			template.Validation.ValidateAll();
			AssertNoRowError(template, "Transport Booking Template requires at least 2 instructions.");
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
