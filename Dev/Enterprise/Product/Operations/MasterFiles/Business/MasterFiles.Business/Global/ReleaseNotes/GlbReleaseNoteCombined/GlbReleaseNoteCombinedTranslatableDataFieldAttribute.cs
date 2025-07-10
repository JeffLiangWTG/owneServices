using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business;

[AttributeUsage(AttributeTargets.Property)]
public sealed class GlbReleaseNoteCombinedTranslatableDataFieldAttribute : TranslatableDataFieldAttribute
{
	public GlbReleaseNoteCombinedTranslatableDataFieldAttribute(string tableName, string columnName)
		: base(tableName, columnName)
	{
		Type = typeof(GlbReleaseNoteCombined);
	}

	public override ZQuery Filter => new (GlbReleaseNoteCombinedSchema.GF_ReleaseNoteDate, SQLComparisonOperator.GreaterThan, new ZDateTime(2016, 10, 25));
}
