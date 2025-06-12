using System;
using Microsoft.BizTalk.Message.Interop;
namespace Microsoft.Samples.BizTalk.Adapter.Common
{
    public interface ISyncReceiveSubmitBatch : IDisposable
    {
        void SubmitMessage(IBaseMessage message);
        void Done();
        bool Wait();
    }
}
