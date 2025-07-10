namespace Enterprise.Customs.US.eManifest.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Modules;

	public interface IMultiSelectHandler
	{
		/// <summary>
		/// Module ID of ZFilterModule of objects to select.
		/// </summary>
		ModuleIdentifier FilterModuleId { get; }

		/// <summary>
		/// Additional filter for the module grid.
		/// </summary>
		ZQuery AdditionalFilter { get; }

		/// <summary>
		/// Selection handler.
		/// </summary>
		/// <param name="selectedObjects">Result of selection.</param>
		void HandleSelectedObjects(BusinessObject[] selectedObjects);
	}
}
