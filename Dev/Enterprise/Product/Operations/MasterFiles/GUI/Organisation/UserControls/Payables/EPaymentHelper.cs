using System;
using System.Linq;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public static class EPaymentHelper
	{
		internal static bool AllowEmptyDefaultPaymentReason(OrgHeader organisation)
		{
			var result = true;
			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).IsEPaymentEnabledForAnyProvider)
			{
				var accountDetails = organisation?.CompanyData?.AccountDetailsCollection;
				if (accountDetails != null && accountDetails.Cast<AccAPAccountDetails>().Any(x => !x.IsInDatabase && x.A1_PaymentMethod == EPaymentMethods.EPaymentViaOFX && x.A1_EPaymentReasonCode.IsEmpty))
				{
					result = GetUserConfirmationToContinueWithEmptyPaymentReason();
				}
			}
			return result;
		}

		public static bool GetUserConfirmationToContinueWithEmptyPaymentReason()
		{
			var message = Res.GetString("e033ad30-334a-471b-aa21-b1e13cdfc287", "You have not selected a Default Payment Method. If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider. Do you wish to continue without adding a Default Payment Method?");
			var caption = Res.GetString("005fc414-c473-415e-8d09-e62c4820b94c", "Default Payment Method for FX Provider");
			var answer = Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning);
			return answer == ZDialogResult.Yes;
		}
	}
}
