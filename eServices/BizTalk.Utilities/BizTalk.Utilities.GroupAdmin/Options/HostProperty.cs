
namespace BizTalk.Utilities.GroupAdmin.Options
{
    /// <summary>
    /// Keep these in sync with the WMI property names in BTSWMISchema.mof
    /// </summary>
    internal enum HostProperty
    {
        AuthTrusted,
        IsHost32BitOnly,
        HostTracking,
        // Advanced Settings
        ThreadPoolSize,
        // Advanced Throttle Settings
        DeliveryQueueSize,
        DBSessionThreshold,
        ThreadThreshold,
        InflightMessageThreshold,
        DBQueueSizeThreshold,
        GlobalMemoryThreshold,
        ProcessMemoryThreshold,
        // Advanced Message Publishing Throttle Settings
        MessagePublishSampleSpaceSize,
        MessagePublishSampleSpaceWindow,
        MessagePublishOverdriveFactor,
        MessagePublishMaximumDelay,
        // Advanced Message Delivery Throttle Settings
        MessageDeliverySampleSpaceSize,
        MessageDeliverySampleSpaceWindow,
        MessageDeliveryOverdriveFactor,
        MessageDeliveryMaximumDelay
    }
}
