using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	public class CFSShipmentDataContextManager : BaseShipmentDataContextManager<CFSShipment>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.CFSShipment; }
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.ShipmentExportDirectory.Value; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new CFSShipmentDataObjectWriter(writeManager, true, true);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (universalShipment.GetMatchingDataTarget(DataContextType.CFSLoadListConsol) != null)
			{
				return null;
			}

			return new CFSShipmentDataObjectReader(universalShipment, logger, factory, null, null);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(o => o.Code == RecipientRoleType.ACF || o.Code == RecipientRoleType.DCF)
				&& dataSources != null
				&& !dataSources.Any(s =>
				{
					var type = s.Type.GetValueOrDefault();
					return type.EqualsIgnoringCase(nameof(DataContextType.ForwardingConsol))
						|| type.EqualsIgnoringCase(nameof(DataContextType.CFSLoadListConsol));
				});
		}

		protected override IUniversalXmlSchema GetSchemaOverride()
		{
			return null;
		}

		protected override IUniversalFreightHelper GetNewHelper()
		{
			return new UniversalCFSHelper();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new CFSShipmentEventParentFinder(factory, this, logger);
		}
	}
}
