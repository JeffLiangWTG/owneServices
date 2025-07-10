using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickDockDoorAssignmentService
	{
		string AddPackageToHandlingUnit(ZGuid packagePK, ZGuid handlingUnitPackagePK);

		string GeneratePickDockDoorAssignment(ZGuid packagePK, DockDoorAssignmentLinkType linkType, ZGuid parentJobPK, BusinessObjectFactory inputFactory);

		string RemovePackageFromHandlingUnit(ZGuid packagePK);

		string RemovePickDockDoorAssignment(ZGuid packagePK, BusinessObjectFactory inputFactory);

		string GetIsDockDoorOverrideAllowedForHandlingUnit(ZGuid packagePK);

		string GetIsDockDoorOverrideAllowedForHandlingUnit(ZGuid huPackagePK, BusinessObjectFactory inputFactory);

		string GetIsDockDoorOverrideAllowedForPicks(IEnumerable<IWhsPick> picks, BusinessObjectFactory inputFactory);

		void OverrideDockDoorLocationsOfPicks(IEnumerable<IWhsPick> picks, ZGuid dockDoorPK, BusinessObjectFactory inputFactory);

		string TryCreateAndPutawayDockDoorAssignmentForPicks(IEnumerable<IWhsPick> picks, BusinessObjectFactory inputFactory);
	}
}
