using System;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.GUI
{
	public class RunSheetMenuItemForDay : RunSheetMenuItem
	{
		public RunSheetMenuItemForDay(RunSheetDay day, EventHandler clickEventHandler)
			: base(day.GetDescription(), clickEventHandler)
		{
			Day = day;
			Name = day.ToString(); // key
		}

		public RunSheetDay Day
		{
			get;
			private set;
		}
	}
}
