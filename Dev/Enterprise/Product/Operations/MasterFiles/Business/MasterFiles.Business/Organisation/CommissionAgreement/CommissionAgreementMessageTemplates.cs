
namespace Enterprise.MasterFiles.Business
{
	public static class CommissionAgreementMessageTemplates
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public static string GetCannotMergeDraftCommisionMessage(string agreementId)
		{
			return Res.GetString(
				"a348e28e-8c51-4354-bc36-c805c676528f",
				"Commission transactions lines have been processed under the previously approved agreement conditions." +
				"\r\nThe commission transactions do not match your new agreement conditions." +
				"\r\nOnly entitlement percentages and wolf pack members can be altered." +
				"\r\nPlease reverse the existing agreement '{0}' and create another agreement to enforce the new agreement conditions." +
				"\r\nAn agreement can become incompatible with existing transactions" +
				"\r\n - if you delete entitlement line;" +
				"\r\n - if you change entitlement period (different selection);" +
				"\r\n - if you delete primary sales person;" +
				"\r\n - if you alter the module and/or change the origin/destination/mode to something no longer applicable for sales person.",
				agreementId
			);
		}
	}
}
