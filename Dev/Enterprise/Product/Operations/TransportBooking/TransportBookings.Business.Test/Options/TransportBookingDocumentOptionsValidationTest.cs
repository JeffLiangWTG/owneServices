using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class TransportBookingDocumentOptionsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTemplate()
		{
			Data.CreateTransportBookingTemplates();

			var options = new TransportBookingDocumentOptions(null, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", false);
			options.Template = "";
			AssertHasError(options.TemplateInfo, "Please enter a Template.");

			options.Template = "NVLD";
			AssertHasError(options.TemplateInfo, "Enter a valid Template.");

			options.Template = Data.ImportFCLTemplate.KT_Code;
			AssertNoErrors(options.TemplateInfo);

			options = new TransportBookingDocumentOptions(Factory, DtbBookingDirection.PIC);
			options.Validation.ValidateTemplate();
			AssertNoErrors(options.TemplateInfo);

			options.Template = "NVLD";
			AssertNoErrors(options.TemplateInfo);

			options.Template = Data.ImportFCLTemplate.KT_Code;
			AssertNoErrors(options.TemplateInfo);
		}

		TransportBookingTestData Data
		{
			get { return data ?? (data = new TransportBookingTestData(Factory)); }
		}

		TransportBookingTestData data;
	}
}
