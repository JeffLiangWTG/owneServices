using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.USCustoms.eBond.EndpointBehavior
{
    public class EBondHttpHeadersEndpointBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        public override Type BehaviorType
        {
            get { return typeof(EBondHttpHeadersEndpointBehavior); }
        }

        protected override object CreateBehavior()
        {
            var result = new EBondHttpHeadersEndpointBehavior();
            return result;
        }

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            AddHttpHeaderInspector headerInspector = new AddHttpHeaderInspector();
            clientRuntime.MessageInspectors.Add(headerInspector);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}
