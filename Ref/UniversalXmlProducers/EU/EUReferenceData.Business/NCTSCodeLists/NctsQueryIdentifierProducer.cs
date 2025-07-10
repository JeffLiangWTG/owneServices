using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsQueryIdentifierProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsQueryIdentifierType();

		protected override Dependency[] GetDependencies(DateTime? dependencyDateTime)
		{
			return new Dependency[]
			{
				new Dependency($"{CodeListDetail.DataSource} Code Type", dependencyDateTime.Value, DependencyType.Preferred)
			};
		}

		protected override RefCusCodeTypeProducer RefCusCodeTypeProducer => new CL054_RefCusCodeTypeProducer();
	}
}

