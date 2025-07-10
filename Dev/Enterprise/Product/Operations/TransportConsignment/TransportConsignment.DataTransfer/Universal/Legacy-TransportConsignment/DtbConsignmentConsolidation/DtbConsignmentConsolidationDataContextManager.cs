using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbConsignmentConsolidationDataContextManager : DtbTransportConsolidationDataContextManager<DtbConsignmentConsolidation>
	{
		#region Context

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportConsignmentConsolidation; }
		}

		protected override string[] GetConsolidationJobTypes()
		{
			return new[] { TransportConsolidationJobTypes.Codes.Consignment };
		}

		#endregion

		#region Events

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DtbConsignmentConsolidationEventParentFinder(this, factory, logger);
		}

		#endregion

		#region Shipments

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DtbConsignmentConsolidationDataObjectWriter(writeManager);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new DtbConsignmentConsolidationDataObjectReader(universalShipment, logger, factory);
		}

		#endregion

		#region RecipientRoles

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion
	}
}


