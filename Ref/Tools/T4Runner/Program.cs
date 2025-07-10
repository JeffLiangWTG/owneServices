using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.T4Runner.Generator;

namespace CargoWise.RefDbRepo.T4Runner
{
	class Program
	{
		
		const string TypeScriptPath = @"..\..\..\Web\ReferenceDataUpdateService.Web\src\models";
		const string ContractModelPath = @"..\..\..\..\Shared\ContractManagement\Contract_0_9";
		const string UXMLWriterModelPath = @"..\..\..\..\Shared\UniversalXmlWriter\UniversalXmlWriter\EntityType";
		
		static void Main(string[] args)
		{
			GenerateByTTFile();
			GenerateTypeScript();
			GenerateContractModel();
			GenerateUXMLWriterModel();
		}

		static void GenerateByTTFile()
		{
			Application.ConfigEnvironment();
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				Console.WriteLine("Opening self hosted services in the background...");
				Application.InitializeSelfHostedServices();
				Console.WriteLine("All self hosted services are running...");
				var directories = TTSearch.FindTTDirectories();

				var ttConfigs = new List<TTFileConfig>();

				foreach (var dir in directories)
				{
					var ttFiles = TTSearch.GetTTFiles(dir);
					if (!ttFiles.Any())
					{
						continue;
					}

					var ttFileConfigurations = TTHelper.GetTTConfigurationFromProjectContentDefinition(dir, ttFiles);
					ttConfigs.AddRange(ttFileConfigurations.Cast<TTFileConfig>());
				}

				TTFileRunner.Run(ttConfigs.ToArray());

				Console.WriteLine("Finished! You may need to update your source control explorer to see the changes.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error has occurred during the t4 generation: ");
				Console.WriteLine(ex.Message);
			}
#pragma warning restore CA1031 // Do not catch general exception types
			finally
			{
				Application.CloseSafeDataUpdateService();
			}
		}

		static void GenerateTypeScript()
		{
			Console.WriteLine($"Start to generate Type Script"); 
			TypeScriptGenerator.CreateFor(TypeScriptPath).Generate();
			Console.WriteLine($"Generate Type Script success");
		}
		
		static void GenerateContractModel()
		{
			Console.WriteLine($"Start to generate contract model"); 
			ContractModelGenerator.CreateFor(ContractModelPath).Generate();
			Console.WriteLine($"Generate contract model success");
		}
		
		static void GenerateUXMLWriterModel()
		{
			Console.WriteLine($"Start to generate UXML Writer model");
			UniversalXMLModelGenerator.CreateFor(UXMLWriterModelPath).Generate();
			Console.WriteLine($"Generate UXML Writer model success");
		}
	}
}
