using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public partial class HourlyFreightNotificationEmailSender : IProcessor
	{
		public void SendEmailsIfRequired(INotifications notifications, CancellationToken token)
		{
			ZDateTime dateFrom = FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.Value;

			ZDateTime dateTo = ZDateTime.UtcNow;
			dateTo = dateTo.AddMilliseconds(-dateTo.Millisecond);

			ZDateTime maximumAllowedBacklog = dateTo.AddMonths(-1);
			if (!dateFrom.IsValid || dateFrom < maximumAllowedBacklog)
			{
				dateFrom = maximumAllowedBacklog;
			}

			SendScheduleChangeEmail(Constants.TransportModes.Sea, dateFrom, dateTo, notifications, token);
			SendScheduleChangeEmail(Constants.TransportModes.Air, dateFrom, dateTo, notifications, token);
			SendScheduleChangeEmail(Constants.TransportModes.Road, dateFrom, dateTo, notifications, token);
			SendScheduleChangeEmail(Constants.TransportModes.Rail, dateFrom, dateTo, notifications, token);

			// We will get here only when all schedule changes were processed w/o errors
			FreightDataRegistry.Instance.HourlyFreightNotificationEmailsLastSent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateTo.ToDateTime());
		}

#if DEBUG

		public void SendEmailsIfRequired(INotifications notifications)
		{
			SendEmailsIfRequired(notifications, CancellationToken.None);
		}

#endif

		#region IProcessor Members

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			SendEmailsIfRequired(notifications, token);
		}

		#endregion

		#region Implementation

		void SendScheduleChangeEmail(ZString transportMode, ZDateTime from, ZDateTime to, INotifications notifications, CancellationToken token)
		{
			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();
			factoryProvider.Current.RefreshEnabled = false;

			int factorySaveRetries = 3;
			bool allScheduleChangesWereProcessed = false;

			while (!allScheduleChangesWereProcessed)
			{
				token.ThrowIfCancellationRequested();
				ScheduleChangeEmailSender emailSender = GetScheduleChangeEmailSender(transportMode);

				int batchSize = 100;
				JobScheduleChange[] processedScheduleChanges = emailSender.SendEmailIfRequired(factoryProvider.Current, from, to, notifications, batchSize);

				allScheduleChangesWereProcessed = processedScheduleChanges.Length < batchSize;
				foreach (JobScheduleChange scheduleChange in processedScheduleChanges)
				{
					scheduleChange.Delete();
				}

				FirePreSaveHookForTesting(factoryProvider.Current);

				try
				{
					factoryProvider.SaveCurrentAndCreateNew();
					factorySaveRetries = 3;
				}
				catch (ZSaveConcurrencyException)
				{
					if (--factorySaveRetries == 0)
					{
						notifications.AddError((NoResString)"Concurrency error during sending schedule change email.");
						break;
					}

					factoryProvider.CreateNewWithoutSave();
					allScheduleChangesWereProcessed = false;
				}
			}
		}

		partial void FirePreSaveHookForTesting(BusinessObjectFactory factory);

#if DEBUG
		internal virtual
#endif
 ScheduleChangeEmailSender GetScheduleChangeEmailSender(string transportMode)
		{
			return ScheduleChangeEmailSender.New(transportMode);
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Testing Members

namespace Enterprise.Freight.Business
{
	partial class HourlyFreightNotificationEmailSender
	{
		partial void FirePreSaveHookForTesting(BusinessObjectFactory factory)
		{
			if (PreSaveHookForTesting != null)
			{
				PreSaveHookForTesting(factory);
			}
		}

		public Action<BusinessObjectFactory> PreSaveHookForTesting { get; set; }
	}
}

#endregion

#endif
#endregion
