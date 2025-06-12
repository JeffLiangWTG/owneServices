using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

[assembly: Timeout(600000)]

namespace CargoWise.eHub.Shared.ProjectTests
{
	public class SourceFileHelpers
	{
		public static readonly Lazy<DirectoryInfo> HubSourceDirectory = new Lazy<DirectoryInfo>(() =>
		{
			var sourcePath = Environment.GetEnvironmentVariable("Nunit3TestSourcePath");
			if (string.IsNullOrEmpty(sourcePath))
			{
				sourcePath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, ".."));
			}
			return new DirectoryInfo(sourcePath);
		});

		public static XmlDocument LoadBuildXML(DirectoryInfo directoryInfo)
		{
			var buildXmlFiles = directoryInfo.GetFiles("Build.xml", SearchOption.TopDirectoryOnly);
			if (buildXmlFiles.Length <= 0)
			{
				Assert.Fail(string.Format("Cannot find build.xml in directory {0}.", directoryInfo.FullName));
			}
			var buildXmlFileInfo = buildXmlFiles[0];
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(buildXmlFileInfo.FullName);
			return xmlDoc;
		}

		public static bool TryFindChildDependencyEntry(XmlElement parent, string assemblyFileName, out XmlNode matchingChild)
		{
			var assemblyFileNameToLower = assemblyFileName.ToLower();
			return TryFindChild(parent,
								(c, afn) =>
								{
									foreach (XmlNode childNode in c.ChildNodes)
									{
										foreach (var attributeObj in childNode.Attributes)
										{
											var attribute = (XmlNode)attributeObj;
											if (attribute.Value.ToLower().Contains(assemblyFileNameToLower))
											{
												return true;
											}
										}
									}
									return false;
								},
								assemblyFileName,
								out matchingChild);
		}

		public static bool TryFindChildCopyToBinEntry(XmlElement parent, string assemblyFileName, out XmlNode matchingChild) => TryFindChild(parent, (c, afn) => c.ChildNodes[0].Value.Equals(afn, StringComparison.InvariantCultureIgnoreCase), assemblyFileName, out matchingChild);
		public static bool TryFindChildByName(XmlElement parent, string childName, out XmlNode matchingChild) => TryFindChild(parent, (c, m) => c.Name.Equals(m, StringComparison.InvariantCultureIgnoreCase), childName, out matchingChild);

		public static bool TryFindChildByAttributeThatContainsValue(XmlElement parent, string attributeName, string attributeValue, out XmlNode matchingChild)
		{
			var attributeValueToLower = attributeValue.ToLower();
			return TryFindChild(parent,
								(c, m) =>
								{
									foreach (var attributeObj in c.Attributes)
									{
										var attribute = (XmlNode)attributeObj;
										if (attribute.Name.Equals(m, StringComparison.InvariantCultureIgnoreCase) && (attributeValueToLower.Contains(attribute.Value.ToLower())))
										{
											return true;
										}
									}
									return false;
								},
								attributeName,
								out matchingChild);
		}

		public static bool TryFindChild(XmlElement parent, Func<XmlNode, string, bool> matchFunc, string matchInput, out XmlNode matchingChild)
		{
			foreach (var child in parent.ChildNodes)
			{
				var childNode = (XmlNode)child;
				if (matchFunc(childNode, matchInput))
				{
					matchingChild = childNode;
					return true;
				}
			}

			matchingChild = null;
			return false;

		}

		internal static IEnumerable<string> GetFiles(DirectoryInfo sourceDirectory, string fileSearchPattern)
		{
			var files = new ConcurrentQueue<string>();
			GetFiles(files, sourceDirectory, fileSearchPattern);
			return files;
		}

		public static IEnumerable<string> GetCSAndBTProjects(DirectoryInfo directory) => GetFiles(directory, "*.*proj").Where(p => p.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".btproj", StringComparison.InvariantCultureIgnoreCase));
		public static IEnumerable<string> GetSolutions(DirectoryInfo directory) => GetFiles(directory, "*.sln");
		public static IEnumerable<DirectoryInfo> TopLevelDirectories => topLevelDirectories.Value;

		static void GetFiles(ConcurrentQueue<string> files, DirectoryInfo directory, string fileSearchPattern)
		{
			foreach (var file in directory.GetFiles(fileSearchPattern))
			{
				files.Enqueue(file.FullName);
			}

			Parallel.ForEach(directory.GetDirectories(), (d) => GetFiles(files, d, fileSearchPattern));
		}

		static IEnumerable<DirectoryInfo> GetTopLevelDirectories(DirectoryInfo directory)
		{
			var directories = directory.GetDirectories();

			foreach (var subDirectory in directories)
			{
				if (NonSourceDirectories.Contains(subDirectory.Name, StringComparer.InvariantCultureIgnoreCase))
				{
					continue;
				}

				yield return subDirectory;
			}
		}

		static readonly List<string> NonSourceDirectories = new List<string>(new string[] { "bin", ".paket", "packages", "paket-files" });
		static readonly Lazy<IEnumerable<DirectoryInfo>> topLevelDirectories = new Lazy<IEnumerable<DirectoryInfo>>(() => GetTopLevelDirectories(HubSourceDirectory.Value));
	}
}
