using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Environment.Business
{
	public class NotificationManager
	{
		public NotificationManager()
		{
			fStack = new Stack();
			Push(new NotificationBuffer());
		}

		public INotifications Peek
		{
			get { return (INotifications)fStack.Peek(); }
		}

		public void Push(INotifications iNotification)
		{
			Argument.NotNull(iNotification, nameof(iNotification));
			fStack.Push(iNotification);
		}

		public INotifications Pop()
		{
			if (fStack.Count > 1)
			{
				fLastPopped = (INotifications)fStack.Pop();
				return fLastPopped;
			}
			else
			{
				throw new InvalidOperationException("NotificationManager must not be emptied");
			}
		}

		// used for testing
		public INotifications LastPopped
		{
			get { return fLastPopped; }
		}

		INotifications fLastPopped;
		readonly Stack fStack;
	}
}
