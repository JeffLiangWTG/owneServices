using System.Net;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;

namespace WinzorFramework.Extensions;

public static class MnemonicTextExtension
{
	public static MarkupString ProcessMnemonicToHtml(this string text, bool displayMnemonic)
	{
		var mnemonicIndex = GetMnemonicIndex(text);

		if (!displayMnemonic || mnemonicIndex == -1)
		{
			var textEncoded = WebUtility.HtmlEncode(text.WithoutMnemonic());
			return new MarkupString(textEncoded);
		}

		var textBeforeEncoded = WebUtility.HtmlEncode(text.WithoutMnemonic().Substring(0, mnemonicIndex));
		var textAfterEncoded = WebUtility.HtmlEncode(text.WithoutMnemonic().Substring(mnemonicIndex + 1));
		var mnemonicChar = text[mnemonicIndex + 1];
		return new MarkupString($"{textBeforeEncoded}<span class=\"mnemonickey\">{mnemonicChar}</span>{textAfterEncoded}");
	}

	public static string WithoutMnemonic(this string text) => WindowsFormsUtils.TextWithoutMnemonics(text);

	static int GetMnemonicIndex(string text)
	{
		//Adapted from WindowsFormsUtils.GetMnemonic
		var index = -1;
		if (text is not null)
		{
			int len = text.Length;
			for (int i = 0; i < len - 1; i++)
			{
				if (text[i] == '&')
				{
					if (text[i + 1] == '&')
					{
						// we have an escaped &, so we need to skip it.
						i++;
						continue;
					}

					index = i;
					break;
				}
			}
		}
		return index;
	}
}
