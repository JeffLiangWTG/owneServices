using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsItemDispatchTransportationUnitDataContextManager : WhsTransitHeaderDataContextManager<WhsItemDispatchTransportationUnit>
	{
		#region Context

		public override ZString DataContextKey => ParentBO.WDH_ReferenceNumber;

		public override DataContextType DataContextType => DataContextType.TransitDispatchHeader;

		#endregion

		#region GetDataContextKeyMatchingQuery

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery();
		}

		#endregion

		#region Shipments

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public override bool ManagesShipments => false;

		#endregion

		#region Event

		protected override IEnumerable<IUniversalJobLink> GetEventDataTargetCore(RecipientRoleType recipientRoleType, IOrgHeader recipientOrganisation)
		{
			DataContextType? dataContextType;

			if (recipientRoleType == RecipientRoleType.FOR)
			{
				dataContextType = DataContextType.ForwardingConsol;
				var links = ParentBO.DispatchLoadLists.SelectMany(dll => UniversalJobLinkHelper.GetMatchingJobLinks(dll, dataContextType.Value, recipientOrganisation));

				return links.Any() ? links : base.GetEventDataTargetCore(recipientRoleType, recipientOrganisation);
			}

			return base.GetEventDataTargetCore(recipientRoleType, recipientOrganisation);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new WhsTransitDispatchTransportationUnitEventParentFinder(this, factory, logger);
		}

		#endregion
	}
}
