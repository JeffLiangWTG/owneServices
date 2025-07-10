using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	public abstract class HtmlTemplate<T> : IHtmlTemplate
	{
		public string GetHeader()
		{
			var result =
$@"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
  <title>Error found during processing of data</title>
  <style type='text/css'>
	{Css}
</style>
</head>
<body>
  <table>
	<tr>
	  <td class='content'>
".Replace("'", "\"");
			return result;
		}

		public string GetFooter()
		{
			var result =
@"		</td>
	</tr>
	<tr style='padding:15px'>
	  <td><small>These notifications are generated automatically, please do not reply to this e-mail</small></td>
	</tr>
  </table>
</body>
</html>
".Replace("'", "\"");
			return result;
		}

		public string GetTitle(string message)
		{
			Argument.NotNullOrEmpty(message, nameof(message));
			var result = FormattableString.Invariant($"<br /><br /><h2>{message}</h2><br /><br />");
			return result;
		}

		public const string Css = @"
td.content table tr td,
td.content table tr th {
	padding:8px;
}";

		public abstract string GetBody(IEnumerable<T> data);
	}
}
