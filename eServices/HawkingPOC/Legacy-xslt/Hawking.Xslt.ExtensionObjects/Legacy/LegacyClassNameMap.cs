using System;
using System.Collections.Generic;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.Xslt.ExtensionObjects.Legacy
{
    public static class LegacyClassNameMap
    {
        public static IDictionary<string, Type> ClassNameMap;

        static LegacyClassNameMap()
        {
            ClassNameMap = new Dictionary<string, Type>
            {
                { "CargoWise.eHub.Core.Transforms.Helper.CodeMapper", typeof(ICodeMapper) },
                { "CargoWise.eHub.Core.Transforms.Helper.DateMapper", typeof(IDateMapper) },
                { "CargoWise.eHub.Core.Transforms.Helper.MathHelper", typeof(IMathHelper) },
                { "CargoWise.eHub.Core.Transforms.Helper.StringMapper", typeof(IStringMapper) },
                { "CargoWise.eHub.Core.Transforms.Helper.UnitConverter", typeof(IUnitConverter) },
                { "CargoWise.eHub.Core.Transforms.Helper.XmlHelper", typeof(IXmlHelper) },
                { "CargoWise.eHub.Core.Transforms.Helper.ContextAccessor", typeof(IContextAccessor) },
                { "CargoWise.eHub.Core.Transforms.Helper.DataModelAccessor", typeof(IDataModelAccessor) },
                { "CargoWise.eHub.Products.AUCustomsNEXDOC.Configuration.ConfigurationAccessor", typeof(IConfigurationAccessor) },
                { "CargoWise.eHub.Products.AUCustomsNEXDOC.Transforms.Helper.NEXDOCDataModelAccessor", typeof(INEXDOCDataModelAccessor) },
                { "CargoWise.eHub.Products.CACustoms.Transformations.Helper.CACustomsGroupingHelper", typeof(ICACustomsGroupingHelper) },
                { "CargoWise.eHub.Products.JPCustoms.Transforms.Helper.JPCustomsDataModelAccessor", typeof(IJPCustomsDataModelAccessor) },
                { "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper.SGCustomsDataModelAccessor", typeof(ISGCustomsDataModelAccessor) },
                { "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper.SGCustomsSubscriptionHelper", typeof(ISGCustomsSubscriptionHelper) },
            };
        }
    }
}