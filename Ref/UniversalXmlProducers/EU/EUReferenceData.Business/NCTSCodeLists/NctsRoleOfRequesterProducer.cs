using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsRoleOfRequesterProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsRoleOfRequesterType();

		protected override Dependency[] GetDependencies(DateTime? dependencyDateTime)
		{
			return new Dependency[]
			{
				new Dependency($"{CodeListDetail.DataSource} Code Type", dependencyDateTime.Value, DependencyType.Preferred)
			};
		}

		protected override RefCusCodeTypeProducer RefCusCodeTypeProducer => new CL156_RefCusCodeTypeProducer();
	}
}

