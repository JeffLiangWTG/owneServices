using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Tools.Common;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public static class EdmxGenerator
	{
		public static void GenerateEDMX(IGeneratorConfiguration generatorConfiguration, ISSDLProducer ssdlProducer)
		{
			Argument.NotNull(generatorConfiguration, nameof(generatorConfiguration));
			Argument.NotNull(ssdlProducer, nameof(ssdlProducer));

			Argument.NotNullOrEmpty(generatorConfiguration.TempFilePath, nameof(generatorConfiguration.TempFilePath));
			Argument.NotNull(generatorConfiguration.CurrentEDMX, nameof(generatorConfiguration.CurrentEDMX));

			var tempFilesPath = generatorConfiguration.TempFilePath;
			var currentEdmx = generatorConfiguration.CurrentEDMX;

			if (!Directory.Exists(tempFilesPath))
			{
				Directory.CreateDirectory(tempFilesPath);
			}

			LoadUpdatedFilesFromDatabase(generatorConfiguration);

			var directoryFiles = Directory.GetFiles(tempFilesPath);
			if (directoryFiles.Any())
			{
				ssdlProducer.SetSSDLFile(directoryFiles);
				var ssdlDocument = ssdlProducer.LoadDocument();
				ssdlProducer.AddEntitiesToEdmx(currentEdmx, ssdlDocument, ElementNodeType.StorageModels);
				var pathToSave = Path.Combine(tempFilesPath, ssdlProducer.SSDLFilePath);
				ssdlDocument.Save(pathToSave);

				var edmgenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), Constants.EdmGenPath);
				var procStartInfo = new ProcessStartInfo(edmgenPath, $"/mode:FromSSDLGeneration /inssdl:\"{ssdlProducer.SSDLFilePath}\" /project:Test /namespace:{generatorConfiguration.Namespace} /entitycontainer:{generatorConfiguration.EntityContainer} /targetversion:4.5 {string.Join(" ", generatorConfiguration.ExtraParameters)}")
				{
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					WorkingDirectory = tempFilesPath
				};
				WinProcessor.RunProcess(procStartInfo);

				var csdlDocument = XDocument.Load(Path.Combine(tempFilesPath, $"Test{Constants.Extensions.ConceptualModels}"));
				ssdlProducer.AddEntitiesToEdmx(currentEdmx, csdlDocument, ElementNodeType.ConceptualModels);

				var mappingsDocument = XDocument.Load(Path.Combine(tempFilesPath, $"Test{Constants.Extensions.Mapping}"));
				ssdlProducer.AddEntitiesToEdmx(currentEdmx, mappingsDocument, ElementNodeType.Mappings);
			}
		}

		static void LoadUpdatedFilesFromDatabase(IGeneratorConfiguration generatorConfiguration)
		{
			Argument.NotNull(generatorConfiguration, nameof(generatorConfiguration));

			var edmgenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), Constants.EdmGenPath);
			var procStartInfo = new ProcessStartInfo(edmgenPath, $"/mode:fullgeneration /c:\"{generatorConfiguration.ConnectionString}\" /project:Test /namespace:{generatorConfiguration.Namespace} /language:csharp /targetversion:4.5 /entitycontainer:{generatorConfiguration.EntityContainer}")
			{
				WorkingDirectory = generatorConfiguration.TempFilePath,
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden
			};

			WinProcessor.RunProcess(procStartInfo);
		}
	}
}
