using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.Freight.Forwarding.GUI
{
	class SecuredFreightVerificationChecker : ISecuredFreightVerificationChecker
	{
		public static void Register(BusinessObjectFactory factory)
		{
			factory?.SetValue<ISecuredFreightVerificationChecker, SecuredFreightVerificationChecker>();
		}

		bool ISecuredFreightVerificationChecker.FreightIsVerifiedToBeSecure(bool mawbValueMayNotBeSynced)
		{
			var dialogContext = new DialogDefaultContext(
					new ZGuid("ab734261-be60-e8b5-4df7-a5e86b3a6c61"),
					ResString.GetMultilingualString("a36a9858-b782-959c-46d9-e092f534b8d6", "SPX Verification"),
					ZMessageBoxButtons.YesNo,
					ZMessageBoxIcon.Question,
					null,
					showCheckboxOnly: true
				);

			var dialogString = mawbValueMayNotBeSynced
				? ResString.GetMultilingualString("203ae78e-64c7-ddab-431d-37cd35590eeb", @"MAWB has been overridden. Please ensure security status on MAWB matches security status on Consol.
Setting the secured status to 'SPX' requires verification. 
Have you verified that the freight is secure and the accompanying CSC or CSD has been checked and found to be correct?") // Dialog Parameter Constant.
				: ResString.GetMultilingualString("51551606-EC98-4FA6-925B-1636F60AF586", @"Setting the secured status to 'SPX' requires verification.
Have you verified that the freight is secure and the accompanying CSC or CSD has been checked and found to be correct?"); // Dialog Parameter Constant.

			var dialogResult = Globals.Message.ShowOrDefault(dialogContext, dialogString);

			return dialogResult == ZDialogResult.Yes;
		}
	}
}
