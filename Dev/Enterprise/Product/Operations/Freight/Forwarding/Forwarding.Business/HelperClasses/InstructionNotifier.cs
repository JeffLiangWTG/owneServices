using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class InstructionNotifier : INotifications, IDisposable
	{
		protected InstructionNotifier(INotifications parent)
		{
			this.parent = parent;
		}

		readonly INotifications parent;

		public void Add(INotification notification)
		{
			notifications.Add(notification);
		}

		public IReadOnlyList<INotification> Notifications => notifications;

		protected List<INotification> notifications = new List<INotification>();

		public bool IsSuccess => notifications.Any() && notifications.Sum(n => n.Type.Severity) == 0;

		#region IDisposable

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (IsSuccess)
					{
						var instructionMessage = BuildInstructionMessage();

						if (notifications.Any(n => n is ErrorNotification))
						{
							parent.Add(CargoWise.ComponentModel.NotificationType.Error, instructionMessage);
						}
						else if (notifications.Any(n => n is WarningNotification))
						{
							parent.Add(CargoWise.ComponentModel.NotificationType.Warning, instructionMessage);
						}
						else
						{
							parent.Add(CargoWise.ComponentModel.NotificationType.Information, instructionMessage);
						}
					}
					else
					{
						parent.Add(CargoWise.ComponentModel.NotificationType.Error, new ZStringBuilder(notifications.Select(n => n.Message)).ToStringWithNewLineBetweenAppends());
					}
				}

				disposedValue = true;
			}
		}

		protected abstract string BuildInstructionMessage();

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		bool disposedValue;

		#endregion
	}
}
