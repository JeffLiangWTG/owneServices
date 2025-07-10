using System.Threading;
using System.Web;
using Enterprise.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost
{
	class MessagingContext : IMessagingContext
	{
		const string HttpContextItemKey = "Enterprise.Messaging.MessageingContext";

		[ThreadSafe]
		static readonly AsyncLocal<IEDICommunicationPartyConfig> currentConfig = new AsyncLocal<IEDICommunicationPartyConfig>();

		public IEDICommunicationPartyConfig CurrentInboundConfig
		{
			get
			{
				if (HttpContext.Current == null)
				{
					return currentConfig.Value;
				}

				return (IEDICommunicationPartyConfig)HttpContext.Current.Items[HttpContextItemKey];
			}
			set
			{
				if (HttpContext.Current == null)
				{
					currentConfig.Value = value;
				}
				else
				{
					HttpContext.Current.Items[HttpContextItemKey] = value;
				}
			}
		}

		public void ResetCurrentInboundConfig()
		{
			currentConfig.Value = null;
		}
	}
}
