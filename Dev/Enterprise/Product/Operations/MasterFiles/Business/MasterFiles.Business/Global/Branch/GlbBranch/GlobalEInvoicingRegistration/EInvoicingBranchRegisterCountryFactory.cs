using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration
{
	public static class EInvoicingBranchRegisterCountryFactory
	{
		public static IRegisterBranchForGlobalEInvoicing RegistrationRequestor(EInvoicingBranchRegister branchRegister, ILogger logger)
		{
			switch (branchRegister.Branch.BaseCountry.Code)
			{
				case CountryCodes.SaudiArabia:
					var requestor = new BranchRegistrationForSAInvoicing(branchRegister.Branch, branchRegister.Debtor, new CSRGenerator(branchRegister.Branch), new HttpClientProvider(), logger);
					requestor.OTP = branchRegister.OTP;
					return requestor;
				default:
					return null;
			}
		}
	}
}
