namespace Enterprise.Customs.GUI
{
	using System.Windows.Forms;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;

	public interface ICustomsUserNotification : Business.MessageManagers.IUserNotification
	{
		void ShowError(string message);

		DialogResult Show(Form form);

		DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult);

		DialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, MessageBoxIcon icon);
	}

	public class UserNotification : ICustomsUserNotification
	{
		#region ICustomsUserNotification

		public string ShowQuestion(string message, string caption, int answerLength, CodeDescriptionPairList answerList, string defaultAnswer = null)
		{
			var args = new UserResponseArgument
			{
				Message = message,
				Caption = caption,
				MinimumResponseLength = answerLength,
				MaximumResponseLength = answerLength,
				AnswerList = answerList,
				Icon = ZMessageBoxIcon.Question,
				DefaultAnswer = defaultAnswer,
			};
			return Globals.Message.QueryUserResponse(args);
		}

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return Globals.Message.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, MessageBoxIcon.Warning) == DialogResult.OK;
		}

		public DialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, MessageBoxIcon icon)
		{
			return Globals.Message.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, icon);
		}

		public bool ShowConfirmation(string message, string caption, bool warning = false)
		{
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, warning ? MessageBoxIcon.Warning : MessageBoxIcon.Question) == DialogResult.Yes;
		}

		public void ShowWarning(string message, string caption) => ShowWarningCore(message, caption);

		protected virtual void ShowWarningCore(string message, string caption)
		{
			Globals.Message.ShowWarning(message, caption);
		}

		public void ShowError(string message) => ShowErrorCore(message);

		protected virtual void ShowErrorCore(string message)
		{
			Globals.Message.ShowError(message);
		}

		public void ShowError(string message, string caption) => ShowErrorCore(message, caption);

		protected virtual void ShowErrorCore(string message, string caption)
		{
			Globals.Message.ShowError(message, caption);
		}

		public void ShowInformation(string message, string caption) => ShowInformationCore(message, caption);

		protected virtual void ShowInformationCore(string message, string caption)
		{
			Globals.Message.ShowInformation(message, caption);
		}

		public DialogResult Show(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult) => ShowCore(message, caption, buttons, icon, defaultResult);

		protected virtual DialogResult ShowCore(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult)
		{
			return Globals.Message.Show(message, caption, buttons, icon, defaultResult);
		}

		public DialogResult Show(Form form)
		{
			return ShowCore(form);
		}

		protected virtual DialogResult ShowCore(Form form)
		{
			return ZFormModaliser.ShowDialogAndDispose(form);
		}

		#endregion
	}
}
