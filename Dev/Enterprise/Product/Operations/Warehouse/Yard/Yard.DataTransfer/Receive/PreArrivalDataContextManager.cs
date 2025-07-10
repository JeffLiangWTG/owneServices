using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class PreArrivalDataContextManager : ShipmentDataContextManager<CYDReceiveAdvice>
	{
		public override bool ManagesShipments => true;

		public override DataContextType DataContextType => DataContextType.CYDReceiveAdvice;

		public override ZString DataContextKey => ParentBO.YRA_JobNumber;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(CYDReceiveAdviceSchema.YRA_JobNumber, matchingValues.Key);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new PreArrivalDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new PreArrivalDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			foreach (var roleType in SupportedRoleTypes)
			{
				return recipientRoles.Any(o => o.Code == roleType && o.ServiceCode.HasValue && SupportedServiceTypes.Contains(o.ServiceCode.Value));
			}

			return false;
		}

		protected IEnumerable<RecipientRoleType> SupportedRoleTypes => new[] { RecipientRoleType.YIA };

		protected IEnumerable<ServiceCodeType> SupportedServiceTypes => new[] { ServiceCodeType.CPA };

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => new List<KeyValuePair<TypeWithDescription, IZType>>();
	}
}
