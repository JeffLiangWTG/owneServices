using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Hawking.eHub.Model.eHubTransactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hawking.eHub.Mapping.Config.eHubTransactions
{
    public class eHubMappingJsonDataContext : IeHubMappingDataContext
    {
        const string DataPath = "Data";
        const string FileExtension = "json";
        static Assembly ExecutingAssembly;
        string baseDir;

        #region Constructor

        public eHubMappingJsonDataContext()
        {
            baseDir = typeof(eHubMappingJsonDataContext).Namespace + "." + DataPath;
            ExecutingAssembly = Assembly.GetExecutingAssembly();

            ResourceFileNames = new List<string>();
            ResourceFileNames.AddRange(
                ExecutingAssembly
                .GetManifestResourceNames()
                .Where(x => x.EndsWith(FileExtension, StringComparison.InvariantCultureIgnoreCase)));
        }

        #endregion

        public List<string> ResourceFileNames { get; }

        public IEnumerable<eHubClient> eHubClients => GetEmbeddedData<eHubClient>();
        public IEnumerable<eHubClientRegistration> eHubClientRegistrations => GetEmbeddedData<eHubClientRegistration>();
        public IEnumerable<eHubClientSystem> eHubClientSystems => GetEmbeddedData<eHubClientSystem>();
        public IEnumerable<eHubClientSystemRegistration> eHubClientSystemRegistrations => GetEmbeddedData<eHubClientSystemRegistration>();
        public IEnumerable<eHubCodeMapKey> eHubCodeMapKeys => GetEmbeddedData<eHubCodeMapKey>();
        public IEnumerable<eHubCodeMapValue> eHubCodeMapValues => GetEmbeddedData<eHubCodeMapValue>();
        public IEnumerable<eHubCodeSet> eHubCodeSets => GetEmbeddedData<eHubCodeSet>();
        public IEnumerable<eHubCodeSetResult> eHubCodeSetResults => GetEmbeddedData<eHubCodeSetResult>();

        public IEnumerable<eHubMessageType> eHubMessageTypes => GetEmbeddedData<eHubMessageType>();
        public IEnumerable<eHubTransformationMapping> eHubTransformationMappings => GetEmbeddedData<eHubTransformationMapping>();
        public IEnumerable<eHubTransformationSet> eHubTransformationSets => GetEmbeddedData<eHubTransformationSet>();
        public IEnumerable<eHubTransformationType> eHubTransformationTypes => GetEmbeddedData<eHubTransformationType>();

        #region Helpers

        Stream GetEmbeddedResource(string tableName)
        {
            var filePath = $"{baseDir}.{tableName}.{FileExtension}";
            if (!ResourceFileNames.Contains(filePath))
            {
                throw new FileNotFoundException("No embedded resource found.", tableName);
            }

            return ExecutingAssembly.GetManifestResourceStream(typeof(eHubMappingJsonDataContext), $"{DataPath}.{tableName}.{FileExtension}");
        }

        IEnumerable<T> GetEmbeddedData<T>()
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Include;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.PreserveReferencesHandling = PreserveReferencesHandling.None;
            serializer.Formatting = Formatting.Indented;

            using (var streamReader = new StreamReader(GetEmbeddedResource(typeof(T).Name)))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<List<T>>(jsonTextReader);
            }
        }

        #endregion
    }
}
