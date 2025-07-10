using System;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.GUI
{
	public class RunSheetMenuItemForView : RunSheetMenuItem
	{
		public RunSheetMenuItemForView(RunSheetView view, EventHandler clickEventHandler)
			: base(view.GetDescription(), clickEventHandler)
		{
			View = view;
			Name = view.ToString(); // key
		}

		public RunSheetView View
		{
			get;
			private set;
		}
	}
}
