namespace System.Windows.Forms;

public sealed class Cursors
{
	#region default styled cursors

	public static Cursor AppStarting { get; } = new Cursor("progress", CursorType.System);

	public static Cursor Arrow { get; } = new Cursor("default", CursorType.System);

	public static Cursor Cross { get; } = new Cursor("crosshair", CursorType.System);

	public static Cursor Default { get; } = new Cursor("default", CursorType.System);

	public static Cursor Hand { get; } = new Cursor("pointer", CursorType.System);

	public static Cursor Help { get; } = new Cursor("help", CursorType.System);

	public static Cursor HSplit { get; } = new Cursor("row-resize", CursorType.System);

	public static Cursor IBeam { get; } = new Cursor("text", CursorType.System);

	public static Cursor No { get; } = new Cursor("not-allowed", CursorType.System);

	public static Cursor SizeWE { get; } = new Cursor("ew-resize", CursorType.System);

	public static Cursor SizeNWSE { get; } = new Cursor("nwse-resize", CursorType.System);

	public static Cursor SizeNESW { get; } = new Cursor("nesw-resize", CursorType.System);

	public static Cursor SizeNS { get; } = new Cursor("ns-resize", CursorType.System);

	public static Cursor SizeAll { get; } = new Cursor("all-scroll", CursorType.System);

	public static Cursor VSplit { get; } = new Cursor("col-resize", CursorType.System);

	public static Cursor WaitCursor { get; } = new Cursor("wait", CursorType.System);

	#endregion

	#region winform file cursors

	public static Cursor NoMoveVert { get; } = new Cursor("nomovev.cur", CursorType.SystemCursor);

	public static Cursor NoMoveHoriz { get; } = new Cursor("nomoveh.cur", CursorType.SystemCursor);

	public static Cursor NoMove2D { get; } = new Cursor("nomove2d.cur", CursorType.SystemCursor);

	public static Cursor PanSW { get; } = new Cursor("sw.cur", CursorType.SystemCursor);

	public static Cursor PanSouth { get; } = new Cursor("south.cur", CursorType.SystemCursor);

	public static Cursor PanSE { get; } = new Cursor("se.cur", CursorType.SystemCursor);

	public static Cursor PanNW { get; } = new Cursor("nw.cur", CursorType.SystemCursor);

	public static Cursor PanNorth { get; } = new Cursor("north.cur", CursorType.SystemCursor);

	public static Cursor PanNE { get; } = new Cursor("ne.cur", CursorType.SystemCursor);

	public static Cursor PanEast { get; } = new Cursor("east.cur", CursorType.SystemCursor);

	public static Cursor PanWest { get; } = new Cursor("west.cur", CursorType.SystemCursor);

	public static Cursor UpArrow { get; } = new Cursor("uparrow.cur", CursorType.SystemCursor);

	#endregion
}
