using System;
using System.Text;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.AlertService.Helpers
{
	public class ExceptionHelper
	{
		public const string IssueMessageDelimiter = "\r\n----------BIZTALK PROPERTIES----------\r\n";

		public static string BuildExceptionDescription(IBaseMessage message, Exception e)
		{
			var description = new StringBuilder(e.ToString()).Append(IssueMessageDelimiter);
			string name, ns;
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				object value = message.Context.ReadAt(i, out name, out ns);
				description.AppendFormat("{0}#{1} = {2}", ns, name, value).AppendLine();
			}

			return description.ToString();
		}
	}
}
