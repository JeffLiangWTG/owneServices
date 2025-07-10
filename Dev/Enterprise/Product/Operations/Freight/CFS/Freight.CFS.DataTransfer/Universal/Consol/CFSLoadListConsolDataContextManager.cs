using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	public class CFSLoadListConsolDataContextManager : ConsolDataContextManager<CFSLoadListConsol, CFSShipment, CFSContainer>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.CFSLoadListConsol; }
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.CFSLoadListConsolDirectory.Value; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new CFSLoadListConsolDataObjectWriter(writeManager, true);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new CFSLoadListConsolDataObjectReader(universalShipment, logger, factory);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(o => o.Code == RecipientRoleType.ACF || o.Code == RecipientRoleType.DCF)
				&& dataSources != null
				&& dataSources.Any(s =>
				{
					var type = s.Type.GetValueOrDefault();
					return type.EqualsIgnoringCase(nameof(DataContextType.CFSLoadListConsol))
						|| type.EqualsIgnoringCase(nameof(DataContextType.ForwardingConsol));
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
	}
}
