using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.MasterFiles.DataTransfer.Native
{
	public class ProductDataContextManager : ShipmentDataContextManager<OrgSupplierPart>
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
			result.AddIfNotEmpty(UniversalDataBuss.DataObjects.Universal.Event.ContextTypes.ClientReference, ParentBO.OP_PartNum);
			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ProductEventParentFinder(factory, this, logger);
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.OP_PartNum; }
		}

		public override DataContextType DataContextType
		{
			get { return UniversalDataBuss.Integration.DataContextType.Product; }
		}

		public override string DefaultOutputDirectory
		{
			get { return ""; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;  // Products do not have a single key field.  The combination of product number, related orgs and org relations is the key. A single key field cannot be used to match. 
		}

		class ProductEventParentFinder : EventParentFinder
		{
			internal ProductEventParentFinder(BusinessObjectFactory factory, ProductDataContextManager manager, IXmlImportLogger logger)
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
