namespace Hawking.CSI.Tracking.Client
{
    public enum MessageEventType
    {
        Created,
        Received,
        Storing,
        ForwardIn,
        ForwardContinue,
        ForwardOut,
        StoreIn,
        StoreOut,
        Processing,
        Processed,
        Sending,
        Sent,
        SentPendingAck,
        Acknowledged,
        NegativeAcknowledgement,
        Discarded,
        Warning,
        Error
    }
}
