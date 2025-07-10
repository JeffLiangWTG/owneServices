using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.T4Runner
{
	public static class TTHelper
	{
		public static ITTFileConfig[] GetTTConfigurationFromProjectContentDefinition(string dir, string[] ttFiles)
		{
			Argument.NotNullOrEmpty(dir, nameof(dir));
			Argument.NotNull(ttFiles, nameof(ttFiles));

			var result = new List<TTFileConfig>();
			var csProjFile = LookForTheCsProj(dir);

			if (!string.IsNullOrEmpty(csProjFile))
			{
				foreach (var tt in ttFiles)
				{
					result.Add(new TTFileConfig { DirectoryPath = dir, FileName = tt, NamespaceName = GetNamespaceFromCsProj(csProjFile, tt) });
				}
			}
			else
			{
				throw new FileNotFoundException($"{dir} and its parents have no csproj file configured.");
			}

			return result.ToArray();
		}

		static string GetNamespaceFromCsProj(string csProjFile, string ttFile)
		{
			Argument.NotNullOrEmpty(csProjFile, nameof(csProjFile));
			Argument.NotNullOrEmpty(ttFile, nameof(ttFile));

			var xDoc = XDocument.Load(csProjFile);
			var allElements = xDoc.Elements().FirstOrDefault()?.Elements();
			if (allElements != null && allElements.Any())
			{
				var contentElement = allElements.Elements().FirstOrDefault(x => x.Name.LocalName == "Content" && ttFile.Contains(x.FirstAttribute.Value));
				if (contentElement != null)
				{
					var customToolNamespace = contentElement.Descendants().FirstOrDefault(o => o.Name.LocalName == "CustomToolNamespace")?.Value;
					if (customToolNamespace == null)
					{
						throw new NotSupportedException($"{ttFile} is missing CustomToolNamespace configuration, please modify the file properties and add a namespace to it.");
					}
					return customToolNamespace;
				}
				throw new FileNotFoundException($"{ttFile} is missing in {csProjFile} and the program is unable to run it.");
			}
			throw new NotSupportedException($"{csProjFile} is not a valid csproj file, the program could not understand its structure.");
		}

		static string LookForTheCsProj(string pathToLook)
		{
			Argument.NotNullOrEmpty(pathToLook, nameof(pathToLook));

			var file = Directory.GetFiles(pathToLook, "*.csproj").FirstOrDefault();
			if (file != null)
			{
				return file;
			}
			else
			{
				var parentDirectory = Directory.GetParent(pathToLook);
				if (parentDirectory == null)
				{
					return null;
				}
				return LookForTheCsProj(parentDirectory.FullName);
			}
		}
	}
}
