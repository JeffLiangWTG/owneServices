using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	public abstract class RunSheetMenuItem : ZToolStripMenuItem
	{
		protected RunSheetMenuItem(ResourceStringData caption, EventHandler clickEventHandler)
			: base(caption, clickEventHandler)
		{
			ImageScaling = ToolStripItemImageScaling.None; // fix the look of the checked state
		}
	}
}
