using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider
{
	public class ServiceProviderFactory
	{
		public static CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider.ServiceProvider Create(ILog logger, string senderId, string recepientId, string message)
		{
			Type type;

			if (Handlers.TryGetValue(recepientId, out type))
			{
				return (CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider.ServiceProvider)Activator.CreateInstance(type, logger, senderId, recepientId, message);
			}
			else
			{
				throw new Exception(string.Format("Service provider {0} not supported.", recepientId));
			}
		}

		static Dictionary<string, Type> Handlers
		{
			get
			{
				if (handlers == null)
				{
					InitialiseHandlers();
				}

				return handlers;
			}
		}

		[ThreadStatic]
		static Dictionary<string, Type> handlers;

		static void InitialiseHandlers()
		{
			handlers = new Dictionary<string, Type>();
			var assembly = Assembly.GetExecutingAssembly();

			foreach (Type type in assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider.ServiceProvider)))
				{
					var attributes = type.GetCustomAttributes(typeof(SupportedRecipientAttribute), false);

					foreach (var attr in attributes)
					{
						string fieldValue = ((SupportedRecipientAttribute)attr).Recipient;
						handlers.Add(fieldValue, type);
					}
				}
			}
		}
	}
}