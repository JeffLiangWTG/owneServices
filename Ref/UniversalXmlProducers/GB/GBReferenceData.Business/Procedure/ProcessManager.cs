using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Procedure
{
	public abstract class ProcessManager
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
		public void RunProcess(string outputPath, StringBuilder errorCollector)
		{
			try
			{
				var configProvider = GetConfigProvider();
				var sourceData = ConfigLoader.LoadConfigFile(configProvider);
				var webClient = GetWebClientWrapper();
				var builder = GetProcedureBuilder();
				var codes = new List<ProcedureCodeData>();
				var mappings = new List<CategoryProcedureMapping>();

				foreach (var source in sourceData)
				{
					try
					{
						var result = ProcedureHelper.GetProcedureCodesAndMappings(webClient, source);
						mappings.AddRange(result.Mappings);
						codes.AddRange(result.Codes);
					}
					catch (Exception ex)
					{
						errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for Source '{source.Name}' Exception: {ex.GetBaseException().Message}");
						throw;
					}
				};

				builder.BuildXml(DateTime.Now, codes, mappings, outputPath); 
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Processing failed", ex);
			}
		}

		public abstract IConfigProvider GetConfigProvider();
		public abstract IProcedureBuilder GetProcedureBuilder();
		public abstract IWebClientWrapper GetWebClientWrapper();
	}

	public class WebClientProcessManager : ProcessManager
	{
		public WebClientProcessManager(IProcedureBuilder procedureBuilder)
		{
			ProcedureBuilder = procedureBuilder;
		}

		readonly IProcedureBuilder ProcedureBuilder;
		const string ConfigFileName = "CDSProcedureConfig.xml";

		public override IConfigProvider GetConfigProvider() => new ConfigProvider(ConfigFileName);
		public override IProcedureBuilder GetProcedureBuilder() => ProcedureBuilder;
		public override IWebClientWrapper GetWebClientWrapper() => new WebClientWrapper();
	}
}
