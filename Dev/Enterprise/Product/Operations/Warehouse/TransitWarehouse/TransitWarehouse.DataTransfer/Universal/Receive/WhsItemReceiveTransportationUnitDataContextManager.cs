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
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsItemReceiveTransportationUnitDataContextManager : WhsTransitHeaderDataContextManager<WhsItemReceiveTransportationUnit>
	{
		#region Context

		public override ZString DataContextKey => ParentBO.WRH_ReferenceNumber;

		public override DataContextType DataContextType => DataContextType.TransitReceiveHeader;

		#endregion

		#region Shipments

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsTransitReceiveTransportationUnitDataObjectWriter(writeManager);
		}

		#endregion

		#region Event

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber, matchingValues.Key);
		}

		protected override IEnumerable<IUniversalJobLink> GetEventDataTargetCore(RecipientRoleType recipientRoleType, IOrgHeader recipientOrganisation)
		{
			DataContextType? dataContextType;

			if (recipientRoleType == RecipientRoleType.FOR)
			{
				dataContextType = DataContextType.ForwardingConsol;
				var links = ParentBO.ReceiveASNs.SelectMany(asn => UniversalJobLinkHelper.GetMatchingJobLinks(asn, dataContextType.Value, recipientOrganisation));

				return links.Any() ? links : base.GetEventDataTargetCore(recipientRoleType, recipientOrganisation);
			}

			return base.GetEventDataTargetCore(recipientRoleType, recipientOrganisation);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new WhsTransitReceiveTransportationUnitEventParentFinder(this, factory, logger);
		}

		#endregion
	}
}
