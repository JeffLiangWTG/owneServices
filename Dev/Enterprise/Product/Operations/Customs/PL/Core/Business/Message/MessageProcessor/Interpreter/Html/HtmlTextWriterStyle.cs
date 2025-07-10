using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.PL.Business;

[SuppressMessage("CargoWiseOne", "CW1161", Justification = "Resource strings are not used by HTML builder.")]
public static class HtmlTextWriterStyle
{
	public const string Color = "color";
	public const string MarginTop = "margin-top";
	public const string MarginBottom = "margin-bottom";
}
