namespace CargoWise.RefDbRepo.EdmxGen
{
	public static class Constants
	{
		public const string EdmGenPath = @"Microsoft.NET\Framework\v4.0.30319\edmgen.exe";

		public static class EdmxHeaders
		{
			public const string StorageModels = "StorageModels";
			public const string ConceptualModels = "ConceptualModels";
			public const string Mappings = "Mappings";
		}

		public static class Elements
		{
			public const string Mapping = "Mapping";
			public const string Schema = "Schema";
			public const string EntityType = "EntityType";
			public const string Property = "Property";
			public const string Association = "Association";
			public const string AssociationSet = "AssociationSet";
			public const string NavigationProperty = "NavigationProperty";
			public const string EntitySet = "EntitySet";
			public const string End = "End";
			public const string EntityTypeMapping = "EntityTypeMapping";
			public const string FunctionImport = "FunctionImport";
			public const string ReferentialConstraint = "ReferentialConstraint";
			public const string Principal = "Principal";
			public const string Dependent = "Dependent";
			public const string PropertyRef = "PropertyRef";
		}

		public static class Extensions
		{
			public const string Storage = ".ssdl";
			public const string Mapping = ".msl";
			public const string ConceptualModels = ".csdl";
		}

		public static class YamlConfig
		{
			public const string TablesConfig = "tables_config.yaml";
			public const string LogicalRelationships = "logical_relationships.yaml";
		}
	}

	public enum ElementNodeType
	{
		StorageModels,
		ConceptualModels,
		Mappings
	}
}
