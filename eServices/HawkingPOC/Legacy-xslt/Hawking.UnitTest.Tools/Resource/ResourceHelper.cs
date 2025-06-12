using System;
using System.IO;
using System.Reflection;

namespace Hawking.UnitTest.Tools.Resource
{
    public static class ResourceHelper
    {
        /// <summary>
        /// Extracts an embedded resource out of a given assembly.  The root namespace of the resource
        /// is inferred using the assembly name
        /// </summary>
        /// <param name="resourceHost">Assembly containing the embedded resource</param>
        /// <param name="resourceName">The extension name of the resource to extract</param>
        /// <returns>A stream containing the resource data</returns>
        public static Stream GetEmbeddedResource(Assembly resourceHost, string resourceName)
        {
            return GetEmbeddedResource(resourceHost, resourceName, true);
        }

        /// <summary>
        /// Extracts an embedded resource out of a given assembly
        /// </summary>
        /// <param name="resourceHost">Assembly containing the embedded resource</param>
        /// <param name="resourceName">The fully qualified or extension name of the resource to extract</param>
        /// <param name="inferRootNamespace">Set to false if <paramref name="resourceName"/> is fully qualified</param>
        /// <returns>A stream containing the resource data</returns>
        public static Stream GetEmbeddedResource(Assembly resourceHost, string resourceName, bool inferRootNamespace)
        {
            var fullResourceName = (inferRootNamespace ? resourceHost.GetName().Name + '.' + resourceName : resourceName);
            var resource = resourceHost.GetManifestResourceStream(fullResourceName);
            if (resource == null)
            {
                throw new Exception(string.Format("Could not locate embedded resource '{0}'", fullResourceName));
            }

            return resource;
        }
    }
}
