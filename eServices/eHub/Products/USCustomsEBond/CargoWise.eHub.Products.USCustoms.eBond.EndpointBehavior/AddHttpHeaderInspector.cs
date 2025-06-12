using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.USCustoms.eBond.EndpointBehavior
{
    class AddHttpHeaderInspector : IClientMessageInspector
    {
        #region IDispatchMessageInspector Members

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            const string httpHeadersKey = "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties#HttpHeaders";
            if (request.Properties.ContainsKey(httpHeadersKey))
            {
                var headers = ((string)request.Properties[httpHeadersKey])
                    .Split(',')
                    .Select(str => str.Split(':').Select(str2 => str2.Trim()).ToArray())
                    .Where(header => header.Length == 2)
                    .Select(header => Tuple.Create(header[0], header[1]));

                HttpRequestMessageProperty httpRequestMessage;
                object httpRequestMessageObject;
                if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
                {
                    httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                }
                else
                {
                    httpRequestMessage = new HttpRequestMessageProperty();
                    request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
                }

                foreach (var header in headers)
                {
                    httpRequestMessage.Headers[header.Item1] = header.Item2;
                }
            }

            return null;
        }

        #endregion
    }
}
