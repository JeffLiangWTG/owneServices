using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class OneStopContainerEventDataVendor : ContainerEventDataVendor, Integration.SailingDataVendor.IOneStopContainerEventDataVendor
	{
		protected override void NotifyContainerCreatedCore(CommonContainer container)
		{
			base.NotifyContainerCreatedCore(container);
			NotifyContainerStatusChange(container);
		}

		protected override void NotifyContainerNumberChangedCore(CommonContainer container)
		{
			base.NotifyContainerNumberChangedCore(container);
			NotifyContainerStatusChange(container);
		}

		protected void NotifyContainerStatusChange(CommonContainer container)
		{
			if (CanNotifyStatusChange)
			{
				OneStopContainerEventRequestList containerRequestList = GetContainerEventRequestList(container.Factory);
				containerRequestList.FindOrCreateRequest(container);

				container.Factory.Saving -= OnFactorySaving_CreateEDIMessagesForEventRequests;
				container.Factory.Saving += OnFactorySaving_CreateEDIMessagesForEventRequests;
				container.Factory.Saved -= OnFactorySaved_PerformCleanup;
				container.Factory.Saved += OnFactorySaved_PerformCleanup;
			}
		}

		protected virtual bool CanNotifyStatusChange
		{
			get
			{
				return
					(FreightDataRegistry.OneStopAUContainerIntegrationIsEnabled || FreightDataRegistry.OneStopNZContainerIntegrationIsEnabled) &&
					FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived > ZDateTime.Now.AddDays(-7);
			}
		}

		#region Factory Saving/Saved Events

		static void OnFactorySaving_CreateEDIMessagesForEventRequests(BusinessObjectFactory factory)
		{
			OneStopContainerEventRequestList requestList = GetContainerEventRequestList(factory);
			requestList.RemoveUnwantedRequests();
			DeleteMessagesIfSaveFails(requestList.CreateMessages());
		}

		static void DeleteMessagesIfSaveFails(IEnumerable<EDIMessage> messages)
		{
			void DeleteMessageIfSaveFails(EDIMessage message, bool saveSucceeded)
			{
				if (!saveSucceeded)
				{
					message.Delete();
				}
				message.Saved -= DeleteMessageIfSaveFails;
			}
			foreach (var message in messages)
			{
				message.Saved += DeleteMessageIfSaveFails;
			}
		}

		static void OnFactorySaved_PerformCleanup(BusinessObjectFactory factory, bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				DeleteUnsavedMessages(factory);
			}
			else
			{
				factory.Saving -= OnFactorySaving_CreateEDIMessagesForEventRequests;
				factory.Saved -= OnFactorySaved_PerformCleanup;
				factory.ServiceContainer.RemoveService<OneStopContainerEventRequestList>();
			}
		}

		static void DeleteUnsavedMessages(BusinessObjectFactory factory)
		{
			foreach (OneStopContainerEventRequest request in GetContainerEventRequestList(factory))
			{
				for (int i = request.Container.ComTracMessages.Count - 1; i >= 0; i--)
				{
					if (!request.Container.ComTracMessages[i].IsInDatabase)
					{
						request.Container.ComTracMessages[i].Delete();
					}
				}
			}
		}

		#endregion

		#region Implementation

		static OneStopContainerEventRequestList GetContainerEventRequestList(BusinessObjectFactory factory)
		{
			OneStopContainerEventRequestList result = factory.ServiceContainer.GetService<OneStopContainerEventRequestList>();
			if (result == null)
			{
				result = OneStopContainerEventRequestList.New();
				factory.ServiceContainer.AddService(result);
			}
			return result;
		}

		#endregion
	}
}
