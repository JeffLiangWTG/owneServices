namespace Enterprise.Warehouse.Integration.Warehouse
{
	public interface IWhsNonExposedFeature
	{
		bool Enabled(WhsNonExposedFeatureName featureName);
	}

	// Add value to this enum when you need to disable feature in progress.
	// Modify class WhsNonExposedFeature to implement check if that feature enabled.
	// Use Stubber class to mock that check in tests.
	public enum WhsNonExposedFeatureName
	{
		None,
		SchemaRedesignChanges
	}
}
