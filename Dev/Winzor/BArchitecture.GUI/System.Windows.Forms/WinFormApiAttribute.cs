namespace System.Windows.Forms;
public class WinFormApiAttribute : Attribute
{
	public string? SourceUrl { get; init; }
	public WinFormApiAttribute(string? sourceUrl = null)
	{
		SourceUrl = sourceUrl;
	}
}
