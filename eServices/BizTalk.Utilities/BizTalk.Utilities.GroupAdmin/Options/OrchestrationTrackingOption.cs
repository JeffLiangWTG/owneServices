
namespace BizTalk.Utilities.GroupAdmin.Options
{
    internal enum OrchestrationTrackingOption
    {
        None,
        ServiceStartEnd,
        MessageSendReceive,
        InboundMessageBody,
        OutboundMessageBody,
        OrchestrationEvents,
        TrackPropertiesForIncomingMessages,
        TrackPropertiesForOutgoingMessages
    }
}
