namespace System.Windows.Forms;

public class LinkArea
{
	public LinkArea(int start, int length)
	{
		Start = start;
		Length = length;
	}

	public bool IsEmpty => Length == 0 && Start == 0;

	public int Start;

	public int Length;
}
