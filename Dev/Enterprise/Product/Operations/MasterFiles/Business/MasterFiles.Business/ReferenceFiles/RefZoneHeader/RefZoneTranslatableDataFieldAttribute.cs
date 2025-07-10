using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RefZoneTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public RefZoneTranslatableDataFieldAttribute(string columnName) : base(
			RefZoneHeader.Schema.TableName, columnName, DataXmlFilePaths.RefZone)
		{
			this.Type = typeof(RefZoneHeader);
		}

		public override ZQuery Filter
		{
			get
			{
				var query = new ZQuery(RefZoneHeaderSchema.FZ_IsActive, true);
				query.AddToFilter(RefZoneHeaderSchema.FZ_Code, SQLComparisonOperator.NotEqual, "BCTZ");
				return query;
			}
		}
	}
}
