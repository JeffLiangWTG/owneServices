using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.PL.Business;

[SuppressMessage("CargoWiseOne", "CW1161", Justification = "Resource strings are not used by HTML builder.")]
public static class HtmlTextWriterTag
{
	public const string Style = "style";

	public const string Br = "br";
	public const string Hr = "hr";
	public const string P = "p";

	public const string H1 = "h1";
	public const string H2 = "h2";
	public const string H3 = "h3";
	public const string H4 = "h4";
	public const string H5 = "h5";

	public const string Table = "table";
	public const string Caption = "caption";
	public const string Th = "th";
	public const string Tbody = "tbody";
	public const string Tr = "tr";
	public const string Td = "td";
}
