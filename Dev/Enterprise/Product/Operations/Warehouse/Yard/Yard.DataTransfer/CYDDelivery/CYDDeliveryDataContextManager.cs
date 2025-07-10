using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal;

public class CYDDeliveryDataContextManager : EventDataContextManager<CYDDelivery>
{
	public override DataContextType DataContextType => DataContextType.CYDDelivery;

	public override ZString DataContextKey => ParentBO.YDL_DeliveryID;

	public override string DefaultOutputDirectory => null;

	protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
	{
		return new ZQuery(CYDDeliverySchema.YDL_DeliveryID, matchingValues.Key);
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
