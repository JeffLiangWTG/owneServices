namespace CargoWise.RefDbRepo.UniversalXmlParser.Resources
{
	public static class Constants
	{
		public static class UniversalXml
		{
			public const string RootElement = "UniversalReferenceData";
			public const string Dependency = "Dependency";
			public const string SchemaElement = "Schema";
		}

		public static class UniversalXmlMetadata
		{
			public const string Metadata = "Metadata";
			public const string DataSource = "DataSource";
			public const string AppName = "AppName";
			public const string PublicationTime = "PublicationTime";
			public const string UpdateType = "UpdateType";
			public const string InclusiveEndDate = "InclusiveEndDate";
		}

		public static class UniversalXmlSchema
		{
			public const string EntityTypeElement = "EntityType";
			public const string EntityTypeNameAttribute = "Name";
		}

		public static class ApplicationSettings
		{
			public const string UniversalXmlParserLogger = "UniversalXmlParser";
			public const string TraceFileExtension = ".~trace";
		}

		public static class StagingSchema
		{
			public const string AssemblyName = "CargoWise.RefDbRepo.Staging.Schema_New";
			public const string NamespaceName = "CargoWise.RefDbRepo.Staging.Schema_New";
		}
	}
}
