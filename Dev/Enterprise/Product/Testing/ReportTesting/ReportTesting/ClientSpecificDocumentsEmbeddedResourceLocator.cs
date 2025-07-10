using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.IO;
using Enterprise.ZArchitecture.Modules;
using WTG.DevTools.FileManagement;

namespace Enterprise.ReportTesting
{
	public class ClientSpecificDocumentsEmbeddedResourceLocator : IDisposable
	{
		public string ExtractClientDocumentXmlEmbeddedResourceToTempFile(string clientCode)
		{
			var xmlFileName = $"{clientCode}Documents.xml";

			var assemblyFiles = ClientHookLoader.GetFilesFromBin($"ZClient{clientCode}*Test*.dll");
			foreach (var assemblyFile in assemblyFiles)
			{
				var assembly = Assembly.LoadFile(assemblyFile);
				var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(xmlFileName));
				if (resourceName is not null)
				{
					using var retriever = new EmbeddedResourceRetriever(assembly);
					var fileName = Path.Combine(tempDirectoryHelper.Path, xmlFileName);
					return retriever.SaveResourceToFile(resourceName, fileName);
				}
			}

			throw new InvalidOperationException($"Cannot find an embedded resource named {xmlFileName} in any client-specific assembly. Checked: {string.Join(",", assemblyFiles)}. To resolve, please include this file as an embedded resource in the relevant test assembly.");
		}

		public void Dispose()
		{
			tempDirectoryHelper.Dispose();
		}

		readonly TempDirectoryHelper tempDirectoryHelper = new();
	}
}
