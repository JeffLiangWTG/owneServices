using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal;

public class CYDPickupDataContextManager : EventDataContextManager<CYDPickup>
{
	public override DataContextType DataContextType => DataContextType.CYDPickup;

	public override ZString DataContextKey => ParentBO.YPL_PickupID;

	public override string DefaultOutputDirectory => null;

	protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
	{
		return new ZQuery(CYDPickupSchema.YPL_PickupID, matchingValues.Key);
	}

	protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => [];

	protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

	protected override IEnumerable<IUniversalJobLink> GetEventDataTargetCore(RecipientRoleType recipientRole, IOrgHeader recipientOrganisation)
	{
		if (recipientRole == RecipientRoleType.GDM)
		{
			return UniversalJobLinkHelper.GetMatchingJobLinks(ParentBO, DataContextType.GateMovementBooking, null);
		}

		return base.GetEventDataTargetCore(recipientRole, recipientOrganisation);
	}
}
