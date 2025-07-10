using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;

namespace WinzorTestFramework;

public static class CursorCases
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
	public static IEnumerable<TestCaseData> TestCursors
	{
		get
		{
			yield return new TestCaseData(null, string.Empty);
			yield return new TestCaseData(Cursors.AppStarting, "progress");
			yield return new TestCaseData(Cursors.Cross, "crosshair");
			yield return new TestCaseData(Cursors.Default, "default");
			yield return new TestCaseData(Cursors.Hand, "pointer");
			yield return new TestCaseData(Cursors.Help, "help");
			yield return new TestCaseData(Cursors.HSplit, "row-resize");
			yield return new TestCaseData(Cursors.IBeam, "text");
			yield return new TestCaseData(Cursors.No, "not-allowed");
			yield return new TestCaseData(Cursors.SizeWE, "ew-resize");
			yield return new TestCaseData(Cursors.SizeNWSE, "nwse-resize");
			yield return new TestCaseData(Cursors.SizeNESW, "nesw-resize");
			yield return new TestCaseData(Cursors.SizeNS, "ns-resize");
			yield return new TestCaseData(Cursors.SizeAll, "all-scroll");
			yield return new TestCaseData(Cursors.VSplit, "col-resize");
			yield return new TestCaseData(Cursors.WaitCursor, "wait");
			yield return new TestCaseData(Cursors.NoMoveVert, "url(/_content/WinzorFramework/images/cursors/nomovev.cur), default");
			yield return new TestCaseData(Cursors.NoMoveHoriz, "url(/_content/WinzorFramework/images/cursors/nomoveh.cur), default");
			yield return new TestCaseData(Cursors.NoMove2D, "url(/_content/WinzorFramework/images/cursors/nomove2d.cur), default");
			yield return new TestCaseData(Cursors.PanSW, "url(/_content/WinzorFramework/images/cursors/sw.cur), default");
			yield return new TestCaseData(Cursors.PanSouth, "url(/_content/WinzorFramework/images/cursors/south.cur), default");
			yield return new TestCaseData(Cursors.PanSE, "url(/_content/WinzorFramework/images/cursors/se.cur), default");
			yield return new TestCaseData(Cursors.PanNW, "url(/_content/WinzorFramework/images/cursors/nw.cur), default");
			yield return new TestCaseData(Cursors.PanNorth, "url(/_content/WinzorFramework/images/cursors/north.cur), default");
			yield return new TestCaseData(Cursors.PanNE, "url(/_content/WinzorFramework/images/cursors/ne.cur), default");
			yield return new TestCaseData(Cursors.PanEast, "url(/_content/WinzorFramework/images/cursors/east.cur), default");
			yield return new TestCaseData(Cursors.PanWest, "url(/_content/WinzorFramework/images/cursors/west.cur), default");
			yield return new TestCaseData(Cursors.UpArrow, "url(/_content/WinzorFramework/images/cursors/uparrow.cur), default");
		}
	}
}
