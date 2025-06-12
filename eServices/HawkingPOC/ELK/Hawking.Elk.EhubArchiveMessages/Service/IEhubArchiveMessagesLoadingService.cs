using System;
using System.Collections.Generic;
using Hawking.Elk.EhubArchiveMessages.Model;

namespace Hawking.Elk.EhubArchiveMessages.Service
{
    public interface IEhubArchiveMessagesLoadingService
    {
        IEnumerable<EhubArchiveMessage> LoadEhubArchiveMessages(DateTime fromArchivedUtc, int batchSize);
    }
}
