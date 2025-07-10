using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.DeniedPartyScreening.Integration
{
	public interface IDeniedPartyScreeningActionsProvider
	{
		void AddEntitiesMenuItem();
		void AddJobsMenuItem();
		ZBool ModuleHasSelectedBusinessObjectsWithShowMessage();
		IZFilterGridModule ParentModuleFilterGrid { get; }
		void AddParentModuleActionsMenuItem(object menuItem);
	}
}
