using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.US.DataTransfer
{
	public class StatementDataContextManager : ShipmentDataContextManager<CusStatementHeader>
	{
		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment, IXmlImportLogger logger, UniversalDataBuss.DataObjects.Core.UniversalObjectFactory factory)
		{
			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public override bool ManagesShipments
		{
			get { return false; }
		}

		public override bool ManagesEvents
		{
			get { return false; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();
			result.AddIfNotEmpty(UniversalDataBuss.DataObjects.Universal.Event.ContextTypes.ClientReference, ParentBO.B2_StatementNumber);
			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new StatementEventParentFinder(factory, this, logger);
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.B2_StatementNumber; }
		}

		public override DataContextType DataContextType
		{
			get { return UniversalDataBuss.Integration.DataContextType.USStatement; }
		}

		public override string DefaultOutputDirectory
		{
			get { return ""; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null; // "ManagesShipments is false, this is not needed"
		}

		class StatementEventParentFinder : EventParentFinder
		{
			internal StatementEventParentFinder(BusinessObjectFactory factory, StatementDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalDataBuss.DataObjects.Universal.Event xmlEvent)
			{
				return null;
			}
		}
	}
}
