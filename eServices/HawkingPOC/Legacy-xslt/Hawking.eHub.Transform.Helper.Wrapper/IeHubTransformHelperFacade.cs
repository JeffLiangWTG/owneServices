using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.eHub.Transform.Helper.Wrapper
{
    public interface IeHubTransformHelperFacade
        : ITransformAccessor
        , ICACustomsGroupingHelper
        , IConfigurationAccessor
        , IContextAccessor
        , IDataModelAccessor
        , IJPCustomsDataModelAccessor
        , INEXDOCDataModelAccessor
        , ISGCustomsDataModelAccessor
        , ISGCustomsSubscriptionHelper
        , IMathHelper
        , IStringMapper
        , IUnitConverter
        , IXmlHelper
        , ICodeMapper
        , IDateMapper
    {
    }
}
