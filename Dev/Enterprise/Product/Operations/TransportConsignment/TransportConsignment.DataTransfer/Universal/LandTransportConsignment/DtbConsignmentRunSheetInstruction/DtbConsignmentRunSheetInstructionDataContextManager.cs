using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbConsignmentRunSheetInstructionDataContextManager : ShipmentDataContextManager<DtbConsignmentRunSheetInstruction>
	{
		#region DataContextKey

		public override ZString DataContextKey
		{
			get { return string.Join("-", ParentBO.RunSheet?.KG_RunSheetNumber, ParentBO.AddressAsSingleLine); }
		}

		#endregion

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportConsignmentRunSheetInstruction; }
		}

		#endregion

		#region DefaultOutputDirectory

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region ManagesShipments

		public override bool ManagesShipments
		{
			get { return true; }
		}

		#endregion

		#region ShipmentDataObjectWriter

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DtbConsignmentRunSheetInstructionDataObjectWriter(writeManager);
		}

		#endregion

		#region GetDataContextKeyMatchingQuery

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		#endregion

		#region GetEventContextValues

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		#endregion

		#region GetEventParentFinder

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DtbConsignmentRunSheetInstructionEventParentFinder(this, factory, logger);
		}

		#endregion

		#region GetShipmentDataObjectReader

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		#endregion

		#region RecipientRoleTargettedToThisModule

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion
	}
}


