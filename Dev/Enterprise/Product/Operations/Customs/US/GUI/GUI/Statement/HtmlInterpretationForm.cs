using System;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class HtmlInterpretationForm : ZChildForm
	{
		public HtmlInterpretationForm()
		{
			InitializeComponent();
		}

		public HtmlInterpretationForm(ZString documentText)
			: this()
		{
			this.DocumentText = documentText;
		}

		public override string FormHeading
		{
			get
			{
				return Res.GetString("6021A665-CB61-401D-A2D5-7DDDEBC94EB5", "Accounting Integration");
			}
		}

		public string DocumentText
		{
			get
			{
				return this.HtmlInterpretationBox.FormattedDocumentText;
			}
			set
			{
				this.HtmlInterpretationBox.DocumentText = GetHtmlFormattedText(value);
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		string GetHtmlFormattedText(string textToFormat)
		{
			var builder = new StringBuilder(textToFormat);
			builder.Replace("&", "&amp;");
			builder.Replace("\r\n", "<br />");
			builder.Replace("\n", "<br />");

			return htmlTemplateForAccountingIntegration.Replace("(*HtmlBody*)", builder.ToString());
		}

		readonly string htmlTemplateForAccountingIntegration = @"<html>
<head>
	<style type='text/css'>
		b {
			color: #FF0000;
			font-family: Arial, sans-serif;
			font-size: 20px;
		}

		p {
			color: #666666;
			font-family: Arial, sans-serif;
			font-size: 20px;
		}

		body{
			background-color: #FFFFFF;
			font-family: Arial, sans-serif;
			font-size: 20px;
		}

		th {
			background-color: #005596;
			color: #FFFFFF;
			font-family: Arial, sans-serif;
			font-size: 20px;
		}

		td {
			color: #666666;
			font-family: Arial, sans-serif;
			font-size: 20px;
		}
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>
		<tr>
			<td class='content'>
				(*HtmlBody*)
			</td>
		</tr>
	</table>
</body>
</html>";
	}
}
