using System;

namespace Enterprise.Freight.Integration
{
	[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
	public sealed class ResponseImporterAttribute : Attribute
	{
		public string ImporterTypeName { get; }

		public ResponseImporterAttribute(string importerTypeName)
		{
			ImporterTypeName = importerTypeName;
		}
	}
}
