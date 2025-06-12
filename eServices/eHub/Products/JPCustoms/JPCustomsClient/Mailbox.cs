using System;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public class Mailbox
	{
		int currentIndex;

		public Mailbox(int messageCount)
		{
			MessageCount = messageCount;
			currentIndex = 0;
		}

		public int PullMessageIndex()
		{
			if (currentIndex > MessageCount) throw new IndexOutOfRangeException("Check MessageCount propety first");
			currentIndex++;
			return currentIndex;
		}

		public int CurrentIndex
		{
			get
			{
				if (currentIndex == 0) throw new IndexOutOfRangeException("Call PullMessageIndex first.");
				return currentIndex;
			}
		}

		public int MessageCount { get; private set; }

		public void Clear()
		{
			MessageCount = 0;
			currentIndex = 0;
		}
	}
}
