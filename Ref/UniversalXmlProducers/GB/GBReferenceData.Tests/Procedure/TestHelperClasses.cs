using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Business.Procedure;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;
using Moq;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure
{
	internal static class TestHelperClasses
	{
		public class ProcessManagerTester : ProcessManager
		{
			public Mock<IConfigProvider> TestConfigProviderMock { get; set; } = new Mock<IConfigProvider>();
			public override IConfigProvider GetConfigProvider() => TestConfigProviderMock.Object;

			public Mock<IWebClientWrapper> TestWebClientMock { get; set; } = new Mock<IWebClientWrapper>();
			public override IWebClientWrapper GetWebClientWrapper() => TestWebClientMock.Object;

			public IProcedureBuilder TestProcedureBuilder { get; set; } = new VirtualProcedureBuilder();
			public override IProcedureBuilder GetProcedureBuilder() => TestProcedureBuilder;
		}

		public class VirtualProcedureBuilder : IProcedureBuilder
		{
			public int ProcedureCodeDataCount { get; set; }
			public int CategoryProcedureMappingDataCount { get; set; }

			public void BuildXml(DateTime publicationDate, IEnumerable<ProcedureCodeData> data, IEnumerable<CategoryProcedureMapping> categoryProcedureMapping, string outputPath)
			{
				ProcedureCodeDataCount = data.Count();
				CategoryProcedureMappingDataCount = categoryProcedureMapping.Count();
			}
		}
	}
}
