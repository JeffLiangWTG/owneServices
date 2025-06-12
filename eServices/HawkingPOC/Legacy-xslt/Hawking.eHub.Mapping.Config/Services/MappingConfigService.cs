using System;
using System.Collections.Generic;
using System.Linq;
using Hawking.eHub.Mapping.Config.eHubTransactions;
using Hawking.eHub.Mapping.Config.Services.Model;
using Hawking.eHub.Model.DataAccess.Integration;
using Hawking.eHub.Model.Services;

namespace Hawking.eHub.Mapping.Config.Services
{
    public class MappingConfigService : IMappingConfigService
    {
        const string CC_SystemCategory_Enterprise = "Enterprise";
        readonly IeHubMappingDataContext mappingDataContext;
        public MappingConfigService(IeHubMappingDataContext mappingDataContext)
        {
            this.mappingDataContext = mappingDataContext;
        }

        //public List<TransformSet> SelectTransformsByPartiesMessage(string senderID, string recipientID, string sourceMessageType)
        //{
        //    var transformSets = new List<TransformSet>();

        //    if (!string.IsNullOrWhiteSpace(sourceMessageType))
        //    {
        //        var bestMatchingSetId = new Guid?();
        //        var transformSetsFound = GetTransformTypes(senderID, recipientID, sourceMessageType, out bestMatchingSetId);

        //        var predicatedTransformSets = transformSetsFound.FindAll(delegate (TransformType set) { return !string.IsNullOrEmpty(set.XpathPredicate); });
        //        if (predicatedTransformSets.Count > 0)
        //        {
        //            var settings = new XmlReaderSettings
        //            {
        //                CloseInput = false,
        //                IgnoreWhitespace = true
        //            };

        //            using (var xmlReader = XmlReader.Create(message.BodyPart.GetOriginalDataStream(), settings))
        //            {
        //                var xPaths = new List<string>();
        //                predicatedTransformSets.ForEach(delegate (TransformSet set) { xPaths.Add(set.XpathPredicate); });
        //                var xDoc = new XPathDocument(xmlReader);
        //                var xNavigator = xDoc.CreateNavigator();
        //                for (int i = 0; i < xPaths.Count; i++)
        //                {
        //                    var xNode = xNavigator.SelectSingleNode(xPaths[i]);
        //                    if (xNode != null)
        //                    {
        //                        transformSetsToApply.Add(predicatedTransformSets[i]);
        //                        break;
        //                    }
        //                }
        //            }

        //            message.BodyPart.GetOriginalDataStream().Seek(0, SeekOrigin.Begin);
        //        }
        //        else
        //        {
        //            // Determine if there are any non-predicated transform sets and if so return the first one in the list
        //            var nonPredicatedTransformSets = transformSetsFound.FindAll(delegate (TransformSet set) { return String.IsNullOrEmpty(set.XpathPredicate); });
        //            if (nonPredicatedTransformSets.Count > 0)
        //            {
        //                transformSetsToApply.Add(nonPredicatedTransformSets[0]);
        //            }
        //        }
        //    }

        //    return transformSets;
        //}

        public List<TransformType> SelectTransformsByPartiesMessage(string senderID, string recipientID, string sourceMessageType, out Guid? bestMatchingSetID)
        {
            var transformationTypes = new List<TransformType>();
            bestMatchingSetID = new Guid?();

            var messageTypePk = from messageType in mappingDataContext.eHubMessageTypes.Where(x => x.DT_Code == sourceMessageType) select messageType.DT_PK;
            var senderPK = from client in mappingDataContext.eHubClients.Where(x => x.CC_ID == senderID) select client.CC_PK;
            var recipientPK = from client in mappingDataContext.eHubClients.Where(x => x.CC_ID == recipientID) select client.CC_PK;

            var rankedTransformationSets = new List<RankedTransformSet>();
            var transformationSets =
                mappingDataContext
                    .eHubTransformationSets
                    .Where(
                        x =>
                        x.TS_CC_Sender.Equals(senderPK) ||
                        x.TS_CC_Recipient.Equals(recipientPK) ||
                        x.TS_DT_Source.Equals(sourceMessageType))
                    .OrderBy(x => x.TS_XPathPredicate);

            foreach (var set in transformationSets)
            {
                var rankedSet = new RankedTransformSet
                {
                    SenderPK = set.TS_CC_Sender,
                    RecipientPK = set.TS_CC_Recipient,
                    TransformSetId = set.TS_PK,
                    XPathPredicate = set.TS_XPathPredicate
                };

                if (set.TS_CC_Sender.Equals(senderPK) && set.TS_CC_Recipient.Equals(recipientPK) && set.TS_DT_Source.Equals(messageTypePk))
                {
                    rankedSet.MatchRank = 1;
                }
                else if (set.TS_CC_Sender.Equals(senderPK) && !set.TS_CC_Recipient.HasValue && set.TS_DT_Source.Equals(messageTypePk))
                {
                    rankedSet.MatchRank = 2;
                }
                else if (!set.TS_CC_Sender.HasValue && set.TS_CC_Recipient.Equals(recipientPK) && set.TS_DT_Source.Equals(messageTypePk))
                {
                    rankedSet.MatchRank = 3;
                }
                else if (!set.TS_CC_Sender.HasValue && !set.TS_CC_Recipient.HasValue && set.TS_DT_Source.Equals(messageTypePk))
                {
                    rankedSet.MatchRank = 4;
                }
                else if (set.TS_CC_Sender.Equals(senderPK) && set.TS_CC_Recipient.Equals(recipientPK) && !set.TS_DT_Source.HasValue)
                {
                    rankedSet.MatchRank = 10;
                }
                else if (set.TS_CC_Sender.Equals(senderPK) && !set.TS_CC_Recipient.HasValue && !set.TS_DT_Source.HasValue)
                {
                    rankedSet.MatchRank = 11;
                }
                else if (!set.TS_CC_Sender.HasValue && set.TS_CC_Recipient.Equals(recipientPK) && !set.TS_DT_Source.HasValue)
                {
                    rankedSet.MatchRank = 12;
                }
                else if (set.TS_CC_Sender.Equals(senderPK) && set.TS_CC_Recipient.Equals(recipientPK) && !set.TS_DT_Source.Equals(messageTypePk))
                {
                    rankedSet.MatchRank = 20;
                }
                else
                {
                    rankedSet.MatchRank = 999;
                }

                rankedTransformationSets.Add(rankedSet);
            }

            rankedTransformationSets.OrderBy(x => x.MatchRank);

            if (rankedTransformationSets.Any(x => x.MatchRank < 999))
            {
                var bestMatchingSet = rankedTransformationSets.Where(x => x.MatchRank < 999).OrderBy(x => x.MatchRank).First();

                bestMatchingSetID = bestMatchingSet.TransformSetId;
                var bestMatchRanking = bestMatchingSet.MatchRank;

                foreach (var set in rankedTransformationSets.Where(x => x.MatchRank.Equals(bestMatchingSet)))
                {
                    foreach (var mapping in mappingDataContext.eHubTransformationMappings.Where(x => x.TM_TS_PK.Equals(set.TransformSetId)))
                    {
                        foreach (var transformType in mappingDataContext.eHubTransformationTypes.Where(x => x.TT_PK.Equals(mapping.TM_TT_PK)))
                        {
                            foreach (var messageType in mappingDataContext.eHubMessageTypes.Where(x => x.DT_PK.Equals(transformType.TT_DT_Target)))
                            {
                                transformationTypes.Add(new TransformType
                                {
                                    TransformSetId = set.TransformSetId,
                                    XpathPredicate = set.XPathPredicate,
                                    MapOrder = mapping.TM_Order,
                                    TransformTypeName = transformType.TT_TransformationType,
                                    MapTargetVersion = transformType.TT_Target_Version,
                                });
                            }
                        }
                    }
                }
            }

            // TODO: future implementation to look up in the ediProdCache for enterprise codes
            //if (transformationTypes.Any())
            //{
            //    if (mappingDataContext.eHubClients.Any(x => x.CC_ID == recipientID && x.CC_SystemCategory == CC_SystemCategory_Enterprise))
            //    {
            //        var recipientEnterpriseCode = recipientID.Substring(1, 3);
            //        var recipientCompanyCode = recipientID.Substring(4, 3);
            //        var recipientServerCode = recipientID.Substring(7, 3);

            //        // ...To be continued
            //        // new eHubGateway would expect the enterprise code be passed in along with the message
            //        // therefore, no need to query the ediProd cache database
            //        // refer to the stored proc "[dbo].[SelectTransformsByPartiesMessage]" for details
            //    }
            //}

            return transformationTypes;
        }
    }
}
