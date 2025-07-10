using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlProcessor;
using CargoWise.RefDbRepo.UniversalXmlProcessor.Configuration;
using Unity;
using Unity.Resolution;

namespace Cargowise.RefDbRepo.UniversalXmlProcessor
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Argument.NotNull(args, nameof(args));
			Argument.GreaterThanZero(args.Length, "args.Length");
			Argument.NotNullOrEmpty(args[0], "args[0]");

			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			try
			{
				VerifyApplicationArgs(args);
				var filePath = args[0];
				var connectionString = args.Length > 1 ? args[1] : ApplicationConfig.RefDbRepoStagingConnString;
				Application.ConfigEnvironment(filePath, connectionString);

				Task.Run(async () =>
				{
					var isForceParsingRequired = args.Length >= 2 &&
												string.Equals(args[1], "force", StringComparison.OrdinalIgnoreCase);

					var sourceDataWriter = Application.UnityContainer.Resolve<ISourceDataWriter>(new ParameterOverride("sourceData", CreateSourceData(args[0])));
					var parser = Application.UnityContainer.Resolve<IUniversalXmlParser>(new ParameterOverride("universalXmlSourceDataWriter", sourceDataWriter));

					using (var stream = File.Open(args[0], FileMode.Open))
					{
						await parser.ParseAsync(stream, isForceParsingRequired);
					}
				})?.GetAwaiter().GetResult();
			}
			finally
			{
				Console.WriteLine("End of UniversalXmlProcessor");

				var stagingRepository = Application.UnityContainer.Resolve<IStagingRepository>();
				stagingRepository?.Dispose();
				Application.UnityContainer?.Dispose();
			}
		}

		#region Helpers

		static void VerifyApplicationArgs(string[] args)
		{
			if ((args != null) && (args.Length > 0))
			{
				return;
			}

			Console.WriteLine("Please provide a file path to the universal xml file.");
			Environment.Exit(0);
		}

		static SourceData CreateSourceData(string fileName)
		{
			return new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Content = null,
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filename = fileName,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_ContentText = string.Empty,
				SDA_Status = StatusProvider.GetQUEStatus(),
				SDA_SourceTime = DateTime.Now,
				SDA_SubSource = string.Empty
			};
		}

		#endregion
	}
}
