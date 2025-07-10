using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRGlbCompanyCampaignValidation : BusinessObjectValidationTestCase
	{
		public void TestContactDataSource()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
			campaign.G0_UseLastEmailSenderAddress = false;

			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.G0_UseLastEmailSenderAddressInfo);

			campaign.G0_UseLastEmailSenderAddress = true;
			AssertHasWarning(campaign.ContactDataSourceInfo, "Last Sender function is not available in HR Campaign Management.");
			AssertHasWarning(campaign.G0_UseLastEmailSenderAddressInfo, "Last Sender function is not available in HR Campaign Management.");

			campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			campaign.G0_UseLastEmailSenderAddress = false;
			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.G0_UseLastEmailSenderAddressInfo);

			campaign.ContactDataSource = HRContactDataSourceList.Codes.CampaignTracking;
			campaign.G0_UseLastEmailSenderAddress = false;
			AssertNoWarnings(campaign.ContactDataSourceInfo);
			AssertNoWarnings(campaign.G0_UseLastEmailSenderAddressInfo);
		}

		public void TestG0_UseLastEmailSenderAddress()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
			campaign.G0_UseLastEmailSenderAddress = true;

			AssertHasWarning(campaign.ContactDataSourceInfo, "Last Sender function is not available in HR Campaign Management.");
			AssertHasWarning(campaign.G0_UseLastEmailSenderAddressInfo, "Last Sender function is not available in HR Campaign Management.");

			campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;

			AssertHasWarning(campaign.ContactDataSourceInfo, "Last Sender function is not available in HR Campaign Management.");
			AssertHasWarning(campaign.G0_UseLastEmailSenderAddressInfo, "Last Sender function is not available in HR Campaign Management.");
		}

		public void TestSimulationContactDataSource()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.SimulationContactDataSource = HRContactDataSourceList.Codes.Staff;

			AssertNoErrors(campaign.SimulationContactDataSourceInfo);

			campaign.SimulationContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			AssertNoErrors(campaign.SimulationContactDataSourceInfo);

			campaign.SimulationContactDataSource = HRContactDataSourceList.Codes.CampaignTracking;
			AssertHasError(campaign.SimulationContactDataSourceInfo, "Enter a valid selection.");

			campaign.SimulationContactDataSource = ZString.Empty;
			AssertHasError(campaign.SimulationContactDataSourceInfo, "Please enter a value.");
		}
	}
}
