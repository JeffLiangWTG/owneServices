using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class NotificationSubscriberGuiHelper : INotificationSubscriberQueryUserDataImport
	{
		public virtual void QueryUser(IQueryUserEventArgs e)
		{
			switch (e)
			{
				case QueryUserYesNoEventArgs args:
					var yesNoResult = ShowMessageBox(args.Message, args.Caption, MessageBoxButtons.YesNo, args.Response ? DialogResult.Yes : DialogResult.No);
					args.Response = (yesNoResult == DialogResult.Yes);
					break;
				case QueryUserOkCancelEventArgs args:
					var okCancelResult = ShowMessageBox(args.Message, args.Caption, MessageBoxButtons.OKCancel, args.Response ? DialogResult.OK : DialogResult.Cancel);
					args.Response = (okCancelResult == DialogResult.OK);
					break;
				case QueryUserRetryCancelEventArgs args:
					var retryCanceResult = ShowMessageBox(args.Message, args.Caption, MessageBoxButtons.RetryCancel, args.Response ? DialogResult.Retry : DialogResult.Cancel);
					args.Response = (retryCanceResult == DialogResult.Retry);
					break;
				case QueryUserYesNoCancelEventArgs args:
					var yesNoCancelResult = ShowMessageBox(args.Message, args.Caption, MessageBoxButtons.YesNoCancel, args.Response ? DialogResult.Yes : DialogResult.Cancel);
					args.Response = (yesNoCancelResult == DialogResult.Yes);
					args.Cancel = (yesNoCancelResult == DialogResult.Cancel);
					break;
				case QueryUserYesNoYesAllNoAllEventArgs args:
					HandleQueryUserYesNoYesAllNoAllEventArgs(args);
					break;
				case DefaultableQueryUserEventArgs args:
					var result = Globals.Message.ShowOrDefault(args.Context, args.Message);
					args.Response = result == ZDialogResult.Yes;
					break;
				case QueryUserOrganisationMatchEventArgs _:
					throw new NotSupportedException("coming soon..");
			}
		}

		void HandleQueryUserYesNoYesAllNoAllEventArgs(QueryUserYesNoYesAllNoAllEventArgs args)
		{
			if (NeverUpdateDuringImport)
			{
				args.Response = false;
			}
			else if (AlwaysUpdateDuringImport)
			{
				args.Response = true;
			}
			else
			{
				var result = ShowYesNoAllMessageBox(args.Message, args.Caption);

				switch (result)
				{
					case YesNoYesAllNoAllMessageBoxResult.Yes:
						args.Response = true;
						break;

					case YesNoYesAllNoAllMessageBoxResult.No:
						args.Response = false;
						break;

					case YesNoYesAllNoAllMessageBoxResult.YesToAll:
						args.Response = true;
						AlwaysUpdateDuringImport = true;
						break;

					case YesNoYesAllNoAllMessageBoxResult.NoToAll:
						args.Response = false;
						NeverUpdateDuringImport = true;
						break;
				}
			}
		}

		protected virtual YesNoYesAllNoAllMessageBoxResult ShowYesNoAllMessageBox(string message, string caption)
		{
			return YesNoAllYesAllNoAllMessageBox.Show(message, caption);
		}

		protected virtual DialogResult ShowMessageBox(string message, string caption, MessageBoxButtons buttons, DialogResult defaultResult)
		{
			return Globals.Message.Show(message, caption, buttons, defaultResult);
		}

		bool AlwaysUpdateDuringImport
		{
			get { return fAlwaysUpdateDuringImport; }
			set
			{
				fAlwaysUpdateDuringImport = value;
				fNeverUpdateDuringImport = !value;
			}
		}
		bool fAlwaysUpdateDuringImport;

		public void ResetUpdateDuringImportFlags()
		{
			fAlwaysUpdateDuringImport = false;
			fNeverUpdateDuringImport = false;
		}

		bool NeverUpdateDuringImport
		{
			get { return fNeverUpdateDuringImport; }
			set
			{
				fNeverUpdateDuringImport = value;
				fAlwaysUpdateDuringImport = !value;
			}
		}
		bool fNeverUpdateDuringImport;
	}
}
