using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Web;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost
{
	public class MessageAuthenticationInspector : IDispatchMessageInspector
	{
		public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
		{
			var errorMessage = (string)HttpContext.Current.Items[eHubUserNamePasswordValidator.ErrorMessageKeyInContextItems];
			if (errorMessage != null)
			{
				WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Unauthorized;
				throw new FaultException(errorMessage);
			}
			return null;
		}

		public void BeforeSendReply(ref Message reply, object correlationState)
		{
		}
	}

	public class MessageAuthenticationBehavior : IServiceBehavior
	{
		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (var chDisp in serviceHostBase.ChannelDispatchers.Cast<ChannelDispatcher>())
			{
				foreach (var epDisp in chDisp.Endpoints)
				{
					epDisp.DispatchRuntime.MessageInspectors.Add(new MessageAuthenticationInspector());
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}

	[CodeAlive("Used in web.config to specify the message inspector for the service behavior")]
	public class MessageAuthenticationInspectorExtensionElement : BehaviorExtensionElement
	{
		public override Type BehaviorType
		{
			get { return typeof(MessageAuthenticationBehavior); }
		}

		protected override object CreateBehavior()
		{
			return new MessageAuthenticationBehavior();
		}
	}
}
