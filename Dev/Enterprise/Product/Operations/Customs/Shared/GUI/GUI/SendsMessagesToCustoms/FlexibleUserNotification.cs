using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class FlexibleUserNotification : UserNotification
	{
		protected override void ShowInformationCore(string message, string caption)
		{
			if (IsFixedSizeMessageBoxRequired(message))
			{
				ShowCore(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
			}
			else
			{
				base.ShowInformationCore(message, caption);
			}
		}

		protected override void ShowErrorCore(string message)
		{
			if (IsFixedSizeMessageBoxRequired(message))
			{
				ShowCore(message, Res.GetString("C77F54E1-5B67-4189-9161-2FC6C134CF64", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
			}
			else
			{
				base.ShowErrorCore(message);
			}
		}

		protected override void ShowErrorCore(string message, string caption)
		{
			if (IsFixedSizeMessageBoxRequired(message))
			{
				ShowCore(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
			}
			else
			{
				base.ShowErrorCore(message, caption);
			}
		}

		protected override void ShowWarningCore(string message, string caption)
		{
			if (IsFixedSizeMessageBoxRequired(message))
			{
				ShowCore(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, DialogResult.OK);
			}
			else
			{
				base.ShowWarningCore(message, caption);
			}
		}

		protected override DialogResult ShowCore(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult)
		{
			DialogResult result;

			if (IsFixedSizeMessageBoxRequired(message))
			{
				result = ShowMessageBoxWithFixedSize(message, caption, buttons, icon, defaultResult);
			}
			else
			{
				result = base.ShowCore(message, caption, buttons, icon, defaultResult);
			}

			return result;
		}

		DialogResult ShowMessageBoxWithFixedSize(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult)
		{
			using (Db.DisposableActionForDbConnection())
			using (var messsageBox = new ZMessageBoxWithFixedSize(message, caption, buttons, icon, ButtonsHelper.DefaultButtonFromDialogResult(buttons, defaultResult)))
			{
				return ZFormModaliser.ShowMessageBoxWithoutDispose(messsageBox);
			}
		}

		MessageBoxButtonsHelper ButtonsHelper => buttonsHelper ?? (buttonsHelper = new MessageBoxButtonsHelper());
		MessageBoxButtonsHelper buttonsHelper;

		bool IsFixedSizeMessageBoxRequired(string message)
		{
			var linesCount = string.IsNullOrWhiteSpace(message) ? 0 : message.Where(c => c == SplitChar).Take(MaxMessageContentLinesCount).Count();
			return linesCount >= MaxMessageContentLinesCount && Globals.Message is Core.Environment.UserNotification;
		}

		const char SplitChar = '\n';
		const int MaxMessageContentLinesCount = 1000;
	}
}
