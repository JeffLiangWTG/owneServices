using System;
using System.Collections.Generic;
using Hawking.eHub.Model.DataAccess.Integration;

namespace Hawking.eHub.Model.Services
{
    public interface IMappingConfigService
    {
        List<TransformType> SelectTransformsByPartiesMessage(string senderID, string recipientID, string sourceMessageType, out Guid? bestMatchingSetID);
    }
}
