namespace Enterprise.MasterFiles.Integration
{
	public interface IFormCustomisationSettings
	{
		bool? IsElementVisible(string elementName, ElementType elementType);
		TabPlacement GetTabPlacement(string elementName);
		string[] CustomisableTabPageNames { get; }
		bool AllFieldsAreOnDefaultTabs();
	}
}
