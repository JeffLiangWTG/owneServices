using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class OutturnDataContextManager : ShipmentDataContextManager<CusOutturn>
	{
		public override DataContextType DataContextType => DataContextType.Outturn;

		public override ZString DataContextKey => "";

		public override string DefaultOutputDirectory => "";

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();
			var parent = ParentBO;

			if (parent != null)
			{
				contextValues.AddIfNotEmpty(Event.ContextTypes.MBOLNumber, parent.C5_MasterBill);
				contextValues.AddIfNotEmpty(Event.ContextTypes.HBOLNumber, parent.C5_HouseBill);
				contextValues.AddIfNotEmpty(Event.ContextTypes.ContainerNumber, parent.C5_ContainerNumber);
			}

			return contextValues;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new OutturnDataEventParentFinder(factory, this, logger);

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => null;

		public override bool ManagesShipments => false;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory) => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => null;
	}
}
