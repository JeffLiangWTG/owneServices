namespace System.Windows.Forms
{
	public class SplitterEventArgs : EventArgs
	{
		//
		// Summary:
		//     Initializes an instance of the System.Windows.Forms.SplitterEventArgs class with
		//     the specified coordinates of the mouse pointer and the coordinates of the upper-left
		//     corner of the System.Windows.Forms.Splitter control.
		//
		// Parameters:
		//   x:
		//     The x-coordinate of the mouse pointer (in client coordinates).
		//
		//   y:
		//     The y-coordinate of the mouse pointer (in client coordinates).
		//
		//   splitX:
		//     The x-coordinate of the upper-left corner of the System.Windows.Forms.Splitter
		//     (in client coordinates).
		//
		//   splitY:
		//     The y-coordinate of the upper-left corner of the System.Windows.Forms.Splitter
		//     (in client coordinates).
		public SplitterEventArgs(int x, int y, int splitX, int splitY)
		{
			X = x;
			Y = y;
			SplitX = splitX;
			SplitY = splitY;
		}

		//
		// Summary:
		//     Gets the x-coordinate of the mouse pointer (in client coordinates).
		//
		// Returns:
		//     The x-coordinate of the mouse pointer.
		public int X { get; }
		//
		// Summary:
		//     Gets the y-coordinate of the mouse pointer (in client coordinates).
		//
		// Returns:
		//     The y-coordinate of the mouse pointer.
		public int Y { get; }
		//
		// Summary:
		//     Gets or sets the x-coordinate of the upper-left corner of the System.Windows.Forms.Splitter
		//     (in client coordinates).
		//
		// Returns:
		//     The x-coordinate of the upper-left corner of the control.
		public int SplitX { get; set; }
		//
		// Summary:
		//     Gets or sets the y-coordinate of the upper-left corner of the System.Windows.Forms.Splitter
		//     (in client coordinates).
		//
		// Returns:
		//     The y-coordinate of the upper-left corner of the control.
		public int SplitY { get; set; }
	}
}
