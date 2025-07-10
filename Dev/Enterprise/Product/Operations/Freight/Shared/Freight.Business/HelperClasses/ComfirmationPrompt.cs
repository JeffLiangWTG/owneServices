using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.Freight.Business.HelperClasses
{
	// A ComfirmationPrompt instance can be used to delegate the decision on whether to ask a user, or not.
	public class ConfirmationPrompt
	{
		public bool PromptYesOrNo(string message, string caption)
		{
			var dialogContext = new DialogDefaultContext(
				new ZGuid("f16b1014-f491-485a-88eb-95c656d3feac"),
				caption,
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Question,
				null,
				showCheckboxOnly: true);

			var dialogResult = Globals.Message.ShowOrDefault(dialogContext, message);
			return dialogResult == ZDialogResult.Yes;
		}
	}
}
