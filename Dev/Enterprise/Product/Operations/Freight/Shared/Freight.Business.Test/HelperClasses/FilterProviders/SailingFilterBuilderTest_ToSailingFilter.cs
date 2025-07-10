using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingFilterBuilderTest_ToSailingFilter : SailingFilterBuilderTest<JobSailing>
	{
		#region Implementation

		protected override JobSailing[] CreateBusinessObjects(JobSailing sailing)
		{
			return new JobSailing[] { sailing };
		}

		protected override ZQuery GetFilter(SailingFilterBuilder builder)
		{
			return builder.ToSailingFilter();
		}

		protected override bool IsLinked(JobSailing bo)
		{
			return true;
		}

		#endregion
	}
}
