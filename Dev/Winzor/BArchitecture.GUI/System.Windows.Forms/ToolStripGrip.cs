namespace System.Windows.Forms;

public partial class ToolStripGrip : ToolStripButton
{
	internal int GripThickness { get; private set; } = 4;
	internal string GripBase64Image { get; private set; } = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAsAAAAYCAYAAAAs7gcTAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABISURBVDhPY/zw4cN/BiIBE5QGAy4uLjDGBVAUg8Dz588ZTpw4AeWhAhTF3759Y7h+/TqUhwlG3YwMRt2MDEaWmwkBWilmYAAArlJMdRfDnMMAAAAASUVORK5CYII=";
}
