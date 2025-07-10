using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.MessageManagers.Testing
{
	public class TestUserNotification : IUserNotification
	{
		public string ShowQuestion(string message, string caption, int answerLength, CodeDescriptionPairList answerList, string defaultAnswer = null)
		{
			LastMessage = message;
			LastCaption = caption;
			return NextTextAnswer;
		}

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			LastMessage = message;
			LastCaption = caption;
			return NextAnswer ?? true;
		}

		public bool ShowConfirmation(string message, string caption, bool warning = false)
		{
			LastMessage = message;
			LastCaption = caption;
			return NextAnswer ?? true;
		}

		public void ShowWarning(string message, string caption)
		{
			LastMessage = message;
			LastCaption = caption;
		}

		public void ShowError(string message, string caption)
		{
			LastMessage = message;
			LastCaption = caption;
		}

		public void ShowInformation(string message, string caption)
		{
			LastMessage = message;
			LastCaption = caption;
		}

		public string NextTextAnswer { get; set; }
		public bool? NextAnswer { get; set; }
		public string LastMessage { get; protected set; }
		public string LastCaption { get; protected set; }
	}
}
