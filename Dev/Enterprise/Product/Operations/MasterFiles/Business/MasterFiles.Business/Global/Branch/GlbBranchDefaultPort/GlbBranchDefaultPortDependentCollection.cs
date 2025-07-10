using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchDefaultPortDependentCollection : DependentBusinessObjectCollection<GlbBranchDefaultPort, GlbBranch>
	{
		public GlbBranchDefaultPortDependentCollection(GlbBranch parent) : base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return GlbBranchDefaultPortSchema.GBP_GB_Branch; }
		}

		public bool ContainsDuplicatedCombination(GlbBranchDefaultPort defaultPort)
		{
			return defaultPort != null && this.Cast<GlbBranchDefaultPort>().Any(defaultTo => defaultTo != defaultPort && defaultTo.EqualsDefaultCombination(defaultPort));
		}

		public GlbBranchDefaultPort GetDefaultPort(ZString defaultTo, ZString transportMode, ZString containerMode)
		{
			if (string.IsNullOrEmpty(defaultTo) || string.IsNullOrEmpty(transportMode))
			{
				return null;
			}

			ColumnValueRanker ranker = new ColumnValueRanker();
			ranker.Add(GlbBranchDefaultPortSchema.GBP_DefaultTo, defaultTo);
			ranker.Add(GlbBranchDefaultPortSchema.GBP_TransportMode, transportMode, (ZString)Core.Constants.TransportModes.All);
			ranker.Add(GlbBranchDefaultPortSchema.GBP_ContainerMode, containerMode, (ZString)"ALL");

			return ranker.GetBestMatch<GlbBranchDefaultPort>(Factory, CreateRelationshipFilter());
		}
	}
}
