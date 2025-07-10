using System;

namespace Enterprise.Freight.CFS.Business.Testing
{
	internal class EventWatcher
	{
		int fCount;
		public readonly EventHandler Handler;

		public EventWatcher()
		{
			Handler = new EventHandler(HandlerMethod);
		}

		public int Count
		{
			get { return fCount; }
		}

		public void Clear()
		{
			fCount = 0;
		}

		void HandlerMethod(object sender, EventArgs e)
		{
			fCount++;
		}
	}
}
