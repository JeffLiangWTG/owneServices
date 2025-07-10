using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE40Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();

			var asese40 = new ASESE40()
			{
				CountryOfOrigin = "CN",
				CommercialInvoiceDescription = "THIS IS APPLE"
			};

			((IACEBIRDLineRecord)asese40).Update(invoiceLine, notifications);
			AssertEquals("CN", invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("THIS IS APPLE", invoiceLine.JI_Description);

			asese40 = new ASESE40()
			{
				CountryOfOrigin = "US",
			};

			((IACEBIRDLineRecord)asese40).Update(invoiceLine, notifications);
			AssertEquals("US", invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("THIS IS APPLE", invoiceLine.JI_Description);
		}
	}
}
