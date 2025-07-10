using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.PL.Business;

public abstract class MessageHtmlInterpreterBase<TLinkedObject, TDataProvider>(TLinkedObject attachedObject)
	: MessageInterpreterBase<TLinkedObject, TDataProvider>(attachedObject)
	where TLinkedObject : EnterpriseBusinessObject
	where TDataProvider : class
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "HTML style strings")]
	protected virtual ZString GlobalStyle =>
"table, th, td { " +
	"border: 1px solid black; " +
	"border-collapse: collapse; " +
"} " +
"th, td { " +
	"padding: 5px; " +
	"text-align: left; " +
"}";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "HTML style strings")]
	const string ExtendedGlobalStyle =
		"body { font-family: Arial, sans-serif; text-align: left; } " +
		"caption { text-align: left; } " +
		"h2 { color:steelblue; font-weight: normal; } " +
		"table {margin-top: 2em; margin-bottom: 2em;} " +
		"table, caption, th, td { border: 1px solid black; border-collapse: collapse; padding: 3px; font-weight: normal; text-align: left; } " +
		".bold-font th { font-weight:bold; } " +
		".no-border, .no-border * { border: none; } " +
		".fixed-table { width: 100 %; } " +
		".fixed-table h3 { margin-top: 0; margin-bottom: 0em; } " +
		".fixed-table th { width: 300px; }";

	protected virtual bool UseExtendedGlobalStyle => false;

	protected sealed override ZString InterpretCore(TDataProvider dataProvider)
	{
		using var htmlBlockWriter = new HtmlBlockWriter();
		htmlBlockWriter.WriteStyle(UseExtendedGlobalStyle ? ExtendedGlobalStyle : GlobalStyle);

		InterpretCore(dataProvider, htmlBlockWriter);

		return htmlBlockWriter.ToString();
	}

	protected abstract void InterpretCore(TDataProvider dataProvider, HtmlBlockWriter htmlWriter);
}
