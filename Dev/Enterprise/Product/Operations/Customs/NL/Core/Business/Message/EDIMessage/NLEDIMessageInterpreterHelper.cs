using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public static class NLEDIMessageInterpreterHelper
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Styling")]
	public const string TableHeaderIndentation = "padding-left:1em;";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Styling")]
	public const string TableHeaderIndentation2 = "padding-left:2em;";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Styling")]
	public const string TableHeaderIndentation3 = "padding-left:3em;";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Styling")]
	public const string FontAndStyle = "<font size='2' face='Courier New' ><table style='margin-left: 10pt'>";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Styling")]
	public const string EndTable = "</table></font>";

	public static void AppendTableRow(ZStringBuilder stringBuilder, ZString headingText, ZString valueText, string tableHeaderStyle = "")
	{
		stringBuilder.Append((NoResString)"<tr><td");
		if (!string.IsNullOrEmpty(tableHeaderStyle))
		{
			stringBuilder.Append(FormattableString.Invariant($" style='{tableHeaderStyle}'"));
		}
		stringBuilder.Append((NoResString)"><b>");
		if (!headingText.IsEmpty)
		{
			stringBuilder.Append(FormattableString.Invariant($"{headingText}:"));
		}
		stringBuilder.Append((NoResString)"</b></td><td>");
		if (!valueText.IsEmpty)
		{
			stringBuilder.Append(FormattableString.Invariant($"<i>{valueText}</i>"));
		}
		stringBuilder.Append((NoResString)"</td></tr>");
	}

	public static ZString FormatMessageContent(string eventType, DateTime? expiryDate, string statementType, string statementDescription, string description, DateTime? registrationDateTime)
	{
		var stringBuilder = new ZStringBuilder();

		stringBuilder.Append(FontAndStyle);
		AppendTableRow(stringBuilder, ResStrings.EventType, eventType);
		if (expiryDate != null)
		{
			AppendTableRow(stringBuilder, ResStrings.ExpiryDate, DMSResponseMessageHelper.GetFormattedShortDateString(expiryDate.Value));
		}
		AppendTableRow(stringBuilder, ResStrings.StatementType, statementType);
		AppendTableRow(stringBuilder, ResStrings.StatementDescriptionType, statementDescription);
		AppendTableRow(stringBuilder, ResStrings.CustomsRemark, description);
		if (registrationDateTime != null)
		{
			AppendTableRow(stringBuilder, ResStrings.RegistrationDate, DMSResponseMessageHelper.GetFormattedLocalLongTimeString(registrationDateTime.Value));
		}
		stringBuilder.Append(EndTable);

		return stringBuilder.ToString();
	}

	public static class ResStrings
	{
		public static ZString EventType => Res.GetString("5FBA779B-A3DB-4C42-9BFC-396ADCB9D8BF", "Event Type");
		public static ZString ExpiryDate => Res.GetString("90304E8B-3253-4E61-B78C-A05D9729F678", "Expiry Date");
		public static ZString StatementType => Res.GetString("B9A3105E-3CC4-41B8-9E16-D521C597FE20", "Statement Type");
		public static ZString StatementDescriptionType => Res.GetString("5FFB4E4C-20D0-418C-97B0-DCFDFCCB7BA5", "Statement Description Type");
		public static ZString CustomsRemark => Res.GetString("2965BEA5-0A8B-41D4-8C0E-226ED61AA9EF", "Customs Remark");
		public static ZString MRN => Res.GetString("0B4572B5-8010-469F-B3E8-ABDA672BB192", "MRN");
		public static ZString FunctionalReferenceID => Res.GetString("AF61EBB1-7E64-486B-BDFE-0D212CA289C7", "Functional Reference ID");
		public static ZString ControlDate => Res.GetString("C5949F90-0C28-4E7A-B0FE-3BCD682F4FC7", "Control Date");
		public static ZString ControlType => Res.GetString("1CE4EE9E-E093-43C8-8E1C-37F3680A251C", "Control Type");
		public static ZString AppliesToEntryLine => Res.GetString("4BCCBFBB-DAE9-43C4-9734-BAAA4BA401FA", "Applies to Entry Line");
		public static ZString ControlStatement => Res.GetString("17CE3B1D-1F4D-479E-8CE0-DB6BD63E207B", "Control Statement");
		public static ZString ControlRemarks => Res.GetString("8EDD97E5-3D53-4FA7-B537-39E7AF55D271", "Control Remarks");
		public static ZString RegistrationDate => Res.GetString("FC8346D3-B1A4-4153-B67F-667D944DFB1E", "Registration date");
		public static ZString ControlStatementTitle => Res.GetString("8751C414-BC9D-44DB-9B1D-B37BDF9C4E78", "Control Statement");
		public static ZString StatementDescription => Res.GetString("8C4931A7-762D-4D4D-A8FE-9E9304879CCD", "Statement Description");
		public static ZString ControlDetailsTitle => Res.GetString("5E373DB5-0085-48FA-8ADE-E674A82A0154", "Control Details");
		public static ZString RequestedDocuments => Res.GetString("614D59B5-849F-4E4A-9964-D037923D5092", "Requested Documents");
		public static ZString RequestedDocument => Res.GetString("48C1F3FC-D151-4931-8AFD-541631B5F766", "Requested Document");
		public static ZString AppliesTo => Res.GetString("D69C250C-3B68-48A8-A397-E31128F632CC", "Applies To");
		public static ZString AmendmentDate => Res.GetString("C1517D18-999A-44CB-8B36-D4488C1DDEBB", "Amendment date:");
		public static ZString ErrorInformationTitle => Res.GetString("5A59AA63-6DDA-4395-B7F0-B62319F28E92", "Error Information");
		public static ZString ErrorDetailsTitle => Res.GetString("9C06AEA2-9C8B-4108-A8B6-7C54B6949E53", "Errors reported by customs");
		public static ZString ErrorValidation => Res.GetString("827E403B-094A-4B52-B9DB-CE994FDEB368", "Error Validation");
		public static ZString ErrorDescription => Res.GetString("A212C7F7-E14D-46C3-B379-9F8F11935715", "Error Description");
		public static ZString ValueThatIsRejected => Res.GetString("46965963-52A4-4E5E-A72A-9E4F84B2A9DE", "Value that is rejected");
		public static ZString CorrectedValue => Res.GetString("467AC6AA-E182-4DF5-B37E-400E247252C2", "Corrected Value");
		public static ZString NamePath => Res.GetString("74182A5E-74D9-4803-B8E6-56A70021A851", "Name Path");
		public static ZString InvalidationDate => Res.GetString("6231130E-766E-4DF7-AA22-6BBADD098ABD", "Invalidation Date");
		public static ZString CancellationDate => Res.GetString("718ED06D-5A2F-4A1A-B000-5941B33BFF5A", "Cancellation Date");
		public static ZString Remark => Res.GetString("F5F48728-88BA-492A-BD22-5171B568ACD1", "Remark");
		public static ZString CancelledDueToNoFollowUpOnOutstandingRequests => Res.GetString("C0520FC6-0F0F-4009-84A4-DA1E9A40728D", "Canceled due to not following up outstanding requests");
		public static ZString CancelledByCustoms => Res.GetString("A448A849-1FA9-4D75-ABD3-383EFB181831", "Canceled by Customs");
		public static ZString CustomsReminder => Res.GetString("26751D8B-3CE9-4F5D-8E83-A9744A9650C1", "Customs Reminder");
		public static ZString ExitDate => Res.GetString("5EC4A40A-EC34-4A9F-A3A1-DE71FE638C8B", "Exit Date");
		public static ZString ControlResults => Res.GetString("310FED27-A4B2-45A3-B9E9-FF8CB0F6C5FB", "Control Results");
		public static ZString ErrorText => Res.GetString("5FC339EE-2534-4737-9CBF-1059C9585BCC", "Error Text");
		public static ZString OriginalValue => Res.GetString("E7111491-E797-4499-9106-BEDE676CCCAF", "Original Value");
		public static ZString Position => Res.GetString("D6F45036-5859-4805-B02D-FE863578E7A9", "Position");
	}
}
