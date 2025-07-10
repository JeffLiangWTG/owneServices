using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class NonPersistentObjectExtensions
	{
		public static void BuildNonPersistentObjects(this object obj, ISafeRepository safeRepo = null)
		{
			Argument.NotNull(obj, nameof(obj));
			if (obj.GetEntityType() == typeof(Safe.RefCusTariff))
			{
				Argument.NotNull(safeRepo, nameof(safeRepo));
				(obj as Safe.RefCusTariff).BuildNonPersistentObjects(safeRepo);
			}
			else if (obj.GetEntityType() == typeof(Safe.RefCusTariffNationalCode))
			{
				Argument.NotNull(safeRepo, nameof(safeRepo));
				(obj as Safe.RefCusTariffNationalCode).BuildNonPersistentObjects(safeRepo);
			}
			else if (obj.GetEntityType() == typeof(Safe.RefCusNomenclatureGroup))
			{
				Argument.NotNull(safeRepo, nameof(safeRepo));
				(obj as Safe.RefCusNomenclatureGroup).BuildNonPersistentObjects(safeRepo);
			}
			else if (obj.GetEntityType() == typeof(Stage.RefCusTariff))
			{
				(obj as Stage.RefCusTariff).BuildNonPersistentObjects();
			}
			else if (obj.GetEntityType() == typeof(Stage.RefCusTariffNationalCode))
			{
				(obj as Stage.RefCusTariffNationalCode).BuildNonPersistentObjects();
			}
			else if (obj.GetEntityType() == typeof(Stage.RefCusNomenclatureGroup))
			{
				(obj as Stage.RefCusNomenclatureGroup).BuildNonPersistentObjects();
			}
		}
	}
}
