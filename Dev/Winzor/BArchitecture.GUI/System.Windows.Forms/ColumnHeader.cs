using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Forms;

public class ColumnHeader
{
	[AllowNull]
	public string Text
	{
		get => text ?? nameof(ColumnHeader);
		set => text = value ?? string.Empty;
	}
	string? text;

	public int Width { get; set; } = 60;

	public HorizontalAlignment TextAlign { get; set; }

	public string StyleString
	{
		get
		{
			return $"width:{Width}px;";
		}
	}

	public ColumnHeader()
	{
	}
}
