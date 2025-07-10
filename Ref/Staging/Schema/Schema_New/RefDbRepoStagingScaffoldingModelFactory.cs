using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[SuppressMessage("Design", "EF1001:Internal EF Core API usage.", Justification = "Known internal API usage.")]
	public class RefDbRepoStagingScaffoldingModelFactory : RelationalScaffoldingModelFactory
	{
		List<string> DataSets;

		[SuppressMessage("Design", "EF1001:Internal EF Core API usage.", Justification = "Known internal API usage.")]
		public RefDbRepoStagingScaffoldingModelFactory(
			IOperationReporter reporter,
			ICandidateNamingService candidateNamingService,
			IPluralizer pluralizer,
			ICSharpUtilities cSharpUtilities,
			IScaffoldingTypeMapper scaffoldingTypeMapper,
			IModelRuntimeInitializer modelRuntimeInitializer
		) : base(reporter, candidateNamingService, pluralizer, cSharpUtilities, scaffoldingTypeMapper,
			modelRuntimeInitializer)
		{
		}

		[SuppressMessage("Design", "EF1001:Internal EF Core API usage.", Justification = "Known internal API usage.")]
		protected override PropertyBuilder VisitColumn(EntityTypeBuilder builder, DatabaseColumn column)
		{
			var property = base.VisitColumn(builder, column);

			if (column.StoreType != null && getDataSets().Contains(column.Table.Name))
			{
				property.HasColumnType(column.StoreType);
			}

			return property;
		}

		List<string> getDataSets()
		{
			if (DataSets == null)
			{
				var structuredDataSets = DataSetStructureProvider.StructuredDataSets;
				DataSets = structuredDataSets.SelectMany(dataSet => dataSet).ToList();
			}

			return DataSets;
		}
	}
}