using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class GlbReleaseNoteTranslatableDataFieldAttribute : TranslatableDataFieldAttribute,
		ICustomizableDataCaptionSource
	{
		public GlbReleaseNoteTranslatableDataFieldAttribute(string tableName, string columnName)
			: base(tableName, columnName)
		{
			Type = typeof(GlbReleaseNote);
		}

		public override ZQuery Filter => new ZQuery(GlbReleaseNoteSchema.GF_ReleaseNoteDate, SQLComparisonOperator.GreaterThan, new ZDateTime(2016, 10, 25));
	}
}
