using System;

namespace CargoWise.eHub.DataModel.Business.Validation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class eHubPortalValidationProviderAttribute : Attribute
    {
        public eHubPortalValidationProviderAttribute(string product, Type model)
        {
            Product = product;
            Model = model;
        }

        public string Product;
        public Type Model;
    }
}
