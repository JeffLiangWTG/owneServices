
namespace BizTalk.Utilities.GroupAdmin.Options
{
    internal enum PortTrackingOption
    {
        None,
        RequestMessageBodyBeforePortProcessing,
        RequestMessageBodyAfterPortProcessing,
        RequestMessagePropsBeforePortProcessing,
        RequestMessagePropsAfterPortProcessing,
        ResponseMessageBodyBeforePortProcessing,
        ResponseMessageBodyAfterPortProcessing,
        ResponseMessagePropsBeforePortProcessing,
        ResponseMessagePropsAfterPortProcessing
    }
}
