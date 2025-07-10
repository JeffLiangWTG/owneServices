using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingQuotedBooking))]
	class TrackingQuotedBookingBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingQuotedBooking>
	{
		protected override TrackingQuotedBooking GetNewBizOForNotification()
		{
			TrackingQuotedBooking bizO = TrackingQuotedBooking.GetNewQuotation(Factory, new TestHelper(Factory).TestSiteUser);

			bizO.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			bizO.ConsigneeDocumentaryAddress.E2_CompanyName = "Company1";
			bizO.ConsigneeDocumentaryAddress.E2_Address1 = "Address1";
			bizO.ConsigneeDocumentaryAddress.E2_Contact = "Contact1";

			bizO.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			bizO.ConsignorDocumentaryAddress.E2_CompanyName = "Company2";
			bizO.ConsignorDocumentaryAddress.E2_Address1 = "Address2";
			bizO.ConsignorDocumentaryAddress.E2_Contact = "Contact3";

			bizO.Origin = "AUSYD";
			bizO.Destination = "AUMEL";

			bizO.Mode = Core.Constants.RateMode.SEA;
			bizO.PaymentTerms = "XXX";
			bizO.ServiceLevel = "D2D";

			bizO.Weight = 10;
			bizO.WeightUnit = "T";
			bizO.Volume = 5;
			bizO.VolumeUnit = "M3";
			bizO.Commodity = "ALUM";

			var container = bizO.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 5;
			container.TC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40FR")).PK;

			var looseCargo = bizO.Quote.CurrentOneOffQuote.LooseCargo.AddNew();
			looseCargo.TPL_PackLineCount = 5;
			looseCargo.TPL_Length = 1;
			looseCargo.TPL_Width = 2;
			looseCargo.TPL_Height = 3;

			return bizO;
		}
	}
}
