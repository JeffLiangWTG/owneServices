using System;
using System.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Common.Logging;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using CargoWise.eHub.Common.Extensions;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.SymmetricalMessaging.Helpers
{
    public class OrchestrationHelper
    {
        internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();

        private const string shippingInstructionClientId = "SHIPPING_INSTRUCTION";

        public static TransformationSet GetTransformationToInternal(string senderId, string messageType, ILog logger)
        {
            var transformationSet = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
            {
                using (var context = GetContext())
                {
                    var dbTransformationSet = context.eHubTransformationSets.Where(x =>
                            x.eHubClient_Sender.CC_ID == senderId &&
                            x.eHubClient_Recipient.CC_ID == shippingInstructionClientId &&
                            x.eHubMessageType.DT_Code == messageType)
                        .FirstOrDefault();
                    if (dbTransformationSet == null)
                    {
                        return new TransformationSet(null, null, "", "", "Not Found", null, "");
                    }
                    else
                    {
                        return new TransformationSet(dbTransformationSet.eHubClient_Sender.CC_PK, dbTransformationSet.eHubClient_Recipient.CC_PK,
                                dbTransformationSet.eHubTransformationMappings.OrderBy(x => x.TM_Order).First().eHubTransformationType.eHubMessageType_Source.DT_Code,
                                dbTransformationSet.eHubTransformationMappings.OrderBy(x => x.TM_Order).Last().eHubTransformationType.eHubMessageType_Target.DT_Code,
                                dbTransformationSet.TS_Name,
                                new List<Tuple<int, string>>(dbTransformationSet.eHubTransformationMappings.Select(x => new Tuple<int, string>(x.TM_Order, x.eHubTransformationType.TT_TransformationType))),
                                "");
                    }

                }
            }, logger);
            return transformationSet;
        }

        public static TransformationSet GetTransformationFromInternal(string recipientId, string messageType, ILog logger)
        {
            var transformationSet = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
            {
                using (var context = GetContext())
                {
                    var dbTransformationSet = context.eHubTransformationSets.Where(x =>
                            x.eHubClient_Sender.CC_ID == shippingInstructionClientId &&
                            x.eHubClient_Recipient.CC_ID == recipientId &&
                            x.eHubMessageType.DT_Code == messageType)
                        .FirstOrDefault();
                    if (dbTransformationSet == null)
                    {
                        return new TransformationSet(null, null, "", "", "Not Found", null, "");
                    }
                    else
                    {
                        return new TransformationSet(dbTransformationSet.eHubClient_Sender.CC_PK, dbTransformationSet.eHubClient_Recipient.CC_PK,
                                dbTransformationSet.eHubTransformationMappings.OrderBy(x => x.TM_Order).First().eHubTransformationType.eHubMessageType_Source.DT_Code,
                                dbTransformationSet.eHubTransformationMappings.OrderBy(x => x.TM_Order).Last().eHubTransformationType.eHubMessageType_Target.DT_Code,
                                dbTransformationSet.TS_Name,
                                new List<Tuple<int, string>>(dbTransformationSet.eHubTransformationMappings.Select(x => new Tuple<int, string>(x.TM_Order, x.eHubTransformationType.TT_TransformationType))),
                                "");
                    }
                }
            }, logger);
            return transformationSet;
        }

        public static TransformationSet GetTransformationSet(string clientId, string messageType, int direction, ILog logger)
        {
            var sourceMessageTypes = new List<string>();
            var destinationMessageTypes = new List<string>();
            TransformationSet transformationSet;
            if (direction == 0)
            {
                transformationSet = GetTransformationToInternal(clientId, messageType, logger);
            }
            else
            {
                transformationSet = GetTransformationFromInternal(clientId, messageType, logger);
            }
            if (transformationSet.TransformationSetName == "Not Found")
            {
                transformationSet.Message = "Unable to find transformation set for the following combination. Client : " + clientId + "; Message Type : " + messageType + "; Direction : " + direction;
            }
            return transformationSet;
        }

        public static RoutingRulesResult GetRoutingRulesResult(string senderId, string messageType, XmlDocument content, ILog logger)
        {
            var recipients = new Queue<string>();
            //ToDo: Use the routing rules to find list of recipients
            return new RoutingRulesResult(recipients);
        }

        public static string DecodeAndDecompress(string document)
        {
            var source = new MemoryStream(System.Text.Encoding.Default.GetBytes(document));
            return source.DecodeAndDecompress().ReadToEnd();
        }

        [Serializable]
        public struct TransformationSet
        {
            public TransformationSet(Guid? senderPK, Guid? recipientPK, string sourceMessageType, string destinationMessageType, string transformationSetName, List<Tuple<int, string>> transformations, string message)
                : this()
            {
                SenderPK = senderPK.ToString();
                RecipientPK = recipientPK.ToString();
                SourceMessageType = sourceMessageType;
                DestinationMessageType = destinationMessageType;
                TransformationSetName = transformationSetName;
                Transformations = transformations;
                Message = message;
            }
            public string SenderPK { get; private set; }
            public string RecipientPK { get; private set; }
            public string SourceMessageType { get; private set; }
            public string DestinationMessageType { get; private set; }
            public string TransformationSetName { get; private set; }
            public List<Tuple<int,string>> Transformations { get; private set; }
            public string Message { get; set; }

            public string TransformationsString
            {
                get
                {
                    if (Transformations != null)
                    {
                        return string.Join("; ", Transformations.Select(x => x.Item1.ToString() + "->" + x.Item2));
                    }
                    else
                    {
                        return "";
                    }
                }
            }

            public string GetTransformationType(int index)
            {
                if (Transformations != null &&  Transformations.Count > index)
                {
                    return Transformations[index].Item2;
                }
                else
                {
                    return "";
                }
            }

        }

        [Serializable]
        public struct RoutingRulesResult
        {
            public RoutingRulesResult(Queue<string> clients)
                : this()
            {
                Clients = clients;
            }
            public Queue<string> Clients { get; private set; }
            
            public string ClientsString
            {
                get
                {
                    if (Clients != null)
                    {
                        return string.Join("; ", Clients.Select(x => x));
                    }
                    else
                    {
                        return "";
                    }
                }
            }

            public string GetClientID()
            {
                if (Clients != null && Clients.Count > 0)
                {
                    return Clients.Peek();
                }
                else
                {
                    return "";
                }
            }

            public void AddClient(string id)
            {
                Clients.Enqueue(id);
            }
        }
    }
}
