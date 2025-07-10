using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class BillOfLadingDataContextManager : AgencyShipmentDataContextManager<BillOfLading>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.BillOfLading; }
		}

		#region Implementation

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new BillOfLadingDataObjectReader(dataObject, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new BillOfLadingDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return (base.RecipientRoleTargettedToThisModule(recipientRoles, dataSources, importSessionLogger) || recipientRoles.Any(o => o.Code == RecipientRoleType.SPM))
				&& !recipientRoles.Any(o => o.ServiceCode == ServiceCodeType.BRQ)
				&& AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(recipientRoles, importSessionLogger);
		}

		#endregion
	}
}
