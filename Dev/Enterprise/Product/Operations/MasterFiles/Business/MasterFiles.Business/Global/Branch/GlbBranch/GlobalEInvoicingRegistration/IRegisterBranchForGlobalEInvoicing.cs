using System;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration
{
	public interface IRegisterBranchForGlobalEInvoicing
	{
		void Register();

		event EventHandler RegistrationSuccessful;

		event EventHandler RegistrationFailed;
	}
}
