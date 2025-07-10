using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	sealed class GlbDepartmentTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public GlbDepartmentTranslatableDataFieldAttribute(string columnName)
			: base(GlbDepartment.Schema.TableName, columnName, DataXmlFilePaths.GlbDepartment)
		{
			Type = typeof(GlbDepartment);
		}
	}
}
