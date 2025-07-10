using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterAdditionalConfigurationLookups : ZLookups
	{
		public EDIMessageContentFilterAdditionalConfigurationLookups(EDIMessageContentFilterAdditionalConfiguration parent)
			: base(parent)
		{
			Parent = parent;
		}

		public new EDIMessageContentFilterAdditionalConfiguration Parent { get; }

		public CodeDescriptionPairList PrimaryDataSource => Factory.GetCachedValue<EDIMessageContentPrimaryDataSource>();
	}
}
