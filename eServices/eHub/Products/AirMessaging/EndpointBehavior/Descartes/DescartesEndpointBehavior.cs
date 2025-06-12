using System;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services;
using System.Web.Services.Description;
using WsdlDescription = System.Web.Services.Description.ServiceDescription;

namespace CargoWise.eHub.Products.AirMessaging.EndpointBehavior.Descartes
{
	public class DescartesEndpointBehavior : BehaviorExtensionElement, IEndpointBehavior
	{
		public override Type BehaviorType
		{
			get { return typeof(DescartesEndpointBehavior); }
		}

		protected override object CreateBehavior()
		{
			var result = new DescartesEndpointBehavior();
			return result;
		}


		#region IEndpointBehavior Members

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			DescartesMessageInspector headerInspector = new DescartesMessageInspector();
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(headerInspector);
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}

		#endregion
	}
}
