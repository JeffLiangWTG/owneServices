using System.Drawing;

#nullable disable

namespace System.Windows.Forms;

public class DataGridViewCellStyle
{
	public string Format { get; set; }

	public DataGridViewTriState WrapMode { get; set; }

	public DataGridViewContentAlignment Alignment { get; set; }

	public Color BackColor { get; set; }

	public Color SelectionBackColor { get; set; }

	public Color SelectionForeColor { get; set; }
}
