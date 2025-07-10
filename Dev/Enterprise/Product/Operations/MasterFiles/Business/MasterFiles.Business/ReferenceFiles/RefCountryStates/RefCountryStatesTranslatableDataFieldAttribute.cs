using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	public sealed class RefCountryStatesTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public RefCountryStatesTranslatableDataFieldAttribute(string tableName, string columnName, string dataXmlFilePaths)
			: base(tableName, columnName, dataXmlFilePaths)
		{
		}

		protected override string GetRuntimeCaptionsSQL(object context)
		{
			return GetRuntimeCaptionsSQL(context, RefCountryStatesSchema.Constants.RW_RN_NKCountryCode);
		}
	}
}
