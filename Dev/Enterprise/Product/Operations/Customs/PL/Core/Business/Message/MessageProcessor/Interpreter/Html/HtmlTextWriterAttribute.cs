using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.PL.Business;

[SuppressMessage("CargoWiseOne", "CW1161", Justification = "Resource strings are not used by HTML builder.")]
public static class HtmlTextWriterAttribute
{
	public const string Class = "class";
	public const string Style = "style";
}
