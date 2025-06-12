using System;

namespace CargoWise.eHub.DataModel.Business.Semantics
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class eHubPortalSemanticsProviderAttribute : Attribute
    {
        public eHubPortalSemanticsProviderAttribute(string product)
        {
            Product = product;
        }

        public string Product;
    }
}
