using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.BeneficiaryRequest;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccEPaymentBeneficiaryRequestValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABR_GC_Company()
		{
			request.ABR_GC_Company = ZGuid.Empty;
			AssertHasError(request.ABR_GC_CompanyInfo, "Please enter a value.");
			request.ABR_GC_Company = ZGuid.NewZGuid();
			AssertHasError(request.ABR_GC_CompanyInfo, "Beneficiary request must specify a valid Company.");
			var company = Factory.New<GlbCompany>();
			request.ABR_GC_Company = company.PK;
			AssertNoErrors(request.ABR_GC_CompanyInfo);
		}

		public void TestCheckABR_InternalReference()
		{
			request.ABR_InternalReference = ZString.Empty;
			AssertHasError(request.ABR_InternalReferenceInfo, "Please enter a value.");
			request.ABR_InternalReference = "00001002";
			AssertNoErrors(request.ABR_InternalReferenceInfo);
		}

		public void TestCheckABR_ProviderCode()
		{
			request.ABR_ProviderCode = ZString.Empty;
			AssertHasError(request.ABR_ProviderCodeInfo, "Please enter a value.");
			request.ABR_ProviderCode = "AAA";
			AssertHasError(request.ABR_ProviderCodeInfo, "Enter a valid selection.");
			request.ABR_ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertNoErrors(request.ABR_ProviderCodeInfo);
		}

		public void TestCheckABR_Status()
		{
			request.ABR_Status = ZString.Empty;
			AssertHasError(request.ABR_StatusInfo, "Please enter a value.");
			request.ABR_Status = "AAA";
			AssertHasError(request.ABR_StatusInfo, "Enter a valid selection.");

			request.ABR_Status = StatusCodes.Queued;
			AssertNoErrors(request.ABR_StatusInfo);
		}

		public void TestCheckABR_ErrorDescription()
		{
			foreach (var code in request.Lookups.StatusCodeList.GetAllCodes())
			{
				request.ABR_Status = code;
				request.ABR_ErrorDescription = ZString.Empty;
				AssertNoErrors(request.ABR_ErrorDescriptionInfo);

				request.ABR_ErrorDescription = "Heyo an error happened";
				if (code == StatusCodes.Error)
				{
					AssertNoErrors(request.ABR_ErrorDescriptionInfo);
				}
				else
				{
					AssertHasError(request.ABR_ErrorDescriptionInfo, "Error Description should only be recorded if the status is ERR.");
				}
			}
		}

		#region Implementation

		AccEPaymentBeneficiaryRequest request;

		protected override void SetUp()
		{
			base.SetUp();
			request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
		}

		#endregion
	}
}
