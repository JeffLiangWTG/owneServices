using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.STLBillingCollector.Business
{
	public class STLBillingCollectorHelper
	{
		public STLBillingCollectorHelper(IHttpClientHelper httpClientHelper, StringBuilder errorBuilder)
		{
			this.httpClientHelper = httpClientHelper;
			this.errorBuilder = errorBuilder;
		}
		readonly IHttpClientHelper httpClientHelper;
		readonly StringBuilder errorBuilder;

		public string ProcessStlBillingCollectors(string sourceNamespace, string outputDirectory = null)
		{
			ProcessAndExportBillingEntities(sourceNamespace, outputDirectory, GetBillingAssemblyFromNuget(GetLatestVersion(), DownloadDir));
			return errorBuilder?.ToString() ?? string.Empty;
		}
		public void ProcessAndExportBillingEntities(string sourceNamespace, string outputDirectory, Assembly billingAssembly)
		{
			try
			{
				Type interfaceType = billingAssembly.GetType(sourceNamespace + Constants.ProgramFunctions.CollectorInterfaceName);

				if (interfaceType == null)
				{
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Interface type '{sourceNamespace}.{Constants.ProgramFunctions.CollectorInterfaceName}' not found in assembly '{billingAssembly.FullName}'.");
					return;
				}
				var types = billingAssembly.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract);
				var entities = types.Select(type => Activator.CreateInstance(type)).Select(instance => MapToStlScriptEntity(instance)).ToList();
				ExportToXmlFile(outputDirectory, GetStlCollectorCodeListConfiguration(), PublicationDateTime, entities.OrderBy(x => x.STL_FeatureCode));
			}
			finally
			{
				Cleanup(billingAssembly);
			}
		}

		private void Cleanup(Assembly billingAssembly)
		{
			if (billingAssembly != null && AssemblyLoadContext.GetLoadContext(billingAssembly) is CollectorLoadContext alc)
			{
				alc.Unload();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
			if (Directory.Exists(DownloadDir))
			{
				Directory.Delete(DownloadDir, recursive: true);
			}
		}
		private Assembly GetBillingAssemblyFromNuget(string version, string tempFolder)
		{
			var packageUrl = $"{AppConfig.Nuget.PackageUrl}{version}";
			tempFolder = Path.Combine(tempFolder ?? DownloadDir, Constants.Dll.DllExtractOutputDir);
			var dllFolder = Path.Combine(tempFolder, Constants.Dll.DllLocation, AppConfig.Collectors.TargetFramework);

			DownloadAndExtractNuGetPackage(packageUrl, tempFolder);
			string dllPath = Path.Combine(dllFolder, Constants.Dll.DllName);
			try
			{
				var alc = new CollectorLoadContext();
				using var fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
				var billingAssembly = alc.LoadFromStream(fs);
				return billingAssembly;
			}
			catch (FileNotFoundException ex)
			{
				errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Could not find the assembly file at path: {dllPath}. Error: {ex.Message}");

			}
			catch (BadImageFormatException ex)
			{
				errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Invalid assembly format at path: {dllPath}. Error: {ex.Message}");
			}
			return null;
		}
		private string GetLatestVersion()
		{
			var response = httpClientHelper.GetWebPageAsync(AppConfig.Nuget.FeedUrl).GetAwaiter().GetResult();
			if (string.IsNullOrEmpty(response))
			{
				errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Failed to retrieve content from URL: {AppConfig.Nuget.FeedUrl}. The response is empty or null.");
				return null;
			}
			var match = Regex.Match(response, @"/CargoWise\.Billing\.Collectors/(\d+\.\d+\.\d+)");
			if (!match.Success)
			{
				errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Failed to find a valid version number in the response from URL: {AppConfig.Nuget.FeedUrl}.");
				return null;
			}
			return match.Groups[1].Value;
		}

		private void DownloadAndExtractNuGetPackage(string packageUrl, string outputFolder)
		{
			if (Directory.Exists(outputFolder))
			{
				Directory.Delete(outputFolder, true);
			}
			Directory.CreateDirectory(outputFolder);
			string packageFile = Path.Combine(outputFolder, "package.nupkg");
			using (var responseStream = httpClientHelper.GetAsync(packageUrl).GetAwaiter().GetResult())
			{
				if (responseStream == null || responseStream.Length == 0)
				{
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"The response stream from URL: {packageUrl} is empty or invalid.");
				}
				else
				{
					using (var memoryStream = new MemoryStream())
					{
						responseStream.CopyTo(memoryStream);
						var packageData = memoryStream.ToArray();
						File.WriteAllBytes(packageFile, packageData);
					}
					ZipFile.ExtractToDirectory(packageFile, outputFolder);
				}
			}
		}
		private static void ExportToXmlFile<T>(string outputFolder, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList)
		{
			var outputPath = Path.Combine(outputFolder, AppConfig.Collectors.OutputFileName);
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource("RefStlScript");
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(UpdateType.Full);

			var dirName = Path.GetDirectoryName(outputPath);
			Directory.CreateDirectory(dirName);
			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputPath);
		}
		private RefStlScript MapToStlScriptEntity(object source)
		{
			var result = new RefStlScript();
			var sourceProperties = source.GetType().GetProperties();
			var targetProperties = typeof(RefStlScript).GetProperties().Where(prop => prop.CanWrite);

			foreach (var targetProperty in targetProperties)
			{
				var sourcePropertyName = targetProperty.Name.Substring(4); 
				var sourceProperty = sourceProperties.FirstOrDefault(prop => prop.Name.Equals(sourcePropertyName, StringComparison.OrdinalIgnoreCase));

				if (sourceProperty != null)
				{
					var sourceValue = sourceProperty.GetValue(source);
					object targetValue = null;

					if (targetProperty.PropertyType == typeof(bool) && sourceValue != null)
					{
						targetValue = Convert.ToBoolean(sourceValue, CultureInfo.InvariantCulture);
					}
					else if (targetProperty.PropertyType == typeof(string))
					{
						targetValue = Convert.ToString(sourceValue, CultureInfo.InvariantCulture);
					}
					else if (targetProperty.PropertyType == typeof(DateTime?) && sourceValue != null)
					{
						var dateValue = (DateTime)sourceValue;
						targetValue = dateValue == default(DateTime) ? PublicationDateTime : dateValue;
					}
					else
					{
						targetValue = sourceValue;
					}
					targetProperty.SetValue(result, targetValue);
				}
			}
			return result;
		}

		private static XmlWriterConfiguration GetStlCollectorCodeListConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var stlBillingFields = new EntityTypeConfiguration<RefStlScript>(true);

			var properties = typeof(RefStlScript).GetProperties()
				.Where(prop => prop.Name.StartsWith("STL_", StringComparison.OrdinalIgnoreCase));
			var keyProperties = new HashSet<string> { "STL_FeatureCode", "STL_ActiveOn", "STL_MinCW1Version", "STL_MaxCW1Version" };
			foreach (var property in properties)
			{
				bool isKey = keyProperties.Contains(property.Name);
				stlBillingFields.IncludeColumn(property.Name, isKeyColumn: isKey);
			}
			writerConfiguration.IncludeEntityTypeConfiguration(stlBillingFields);

			return writerConfiguration;
		}
		protected  virtual DateTime PublicationDateTime => DateTime.UtcNow;
		protected virtual string DownloadDir => Path.GetFullPath(Path.Combine(Path.GetTempPath(), "RefStlData"));
	}
}
