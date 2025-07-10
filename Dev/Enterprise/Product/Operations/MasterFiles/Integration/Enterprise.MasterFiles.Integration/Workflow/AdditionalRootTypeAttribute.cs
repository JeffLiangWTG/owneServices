using System;

namespace Enterprise.MasterFiles.Integration
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class AdditionalRootTypeAttribute : Attribute
	{
		public AdditionalRootTypeAttribute(string dataSourceTypeName)
		{
			DataSourceTypeName = dataSourceTypeName;
		}

		public string DataSourceTypeName { get; }
	}
}
