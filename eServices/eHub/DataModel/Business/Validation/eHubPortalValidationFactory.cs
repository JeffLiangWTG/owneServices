using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CargoWise.eHub.DataModel.Business.Validation
{
    public class eHubPortalValidationFactory
    {
        static Dictionary<string, Type> Cache = new Dictionary<string, Type>();

        static Type GetValidator(string product, Type targetType)
        {
            if (Cache.ContainsKey(product + targetType.Name)) return Cache[product + targetType.Name];
            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes().Where(t => t.IsClass))
            {
                var attributes = type.GetCustomAttributes(typeof(eHubPortalValidationProviderAttribute), false);

                foreach (eHubPortalValidationProviderAttribute attribute in attributes)
                {
                    if (attribute.Product == product && attribute.Model.Name == targetType.Name)
                    {
                        Cache.Add(product + targetType.Name, type);
                        return type;
                    }
                }
            }
            return null;
        }

        static TypeDescriptionProvider currentProvider;

        public static bool ApplyValidation(string product, Type targetModel)
        {
            if (currentProvider != null)
            {
                TypeDescriptor.RemoveProvider(currentProvider, targetModel);
            }
            var validator = GetValidator(product, targetModel);
            if (validator != null)
            {
                currentProvider = new AssociatedMetadataTypeTypeDescriptionProvider(targetModel, validator);
                TypeDescriptor.AddProvider(currentProvider, targetModel);
                return true;
            }
            return false;
        }
    }
}
