using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	public class AgencyShipmentContainerDataContextManager : ShipmentDataContextManager<AgencyShipmentContainer>
	{
		#region DataContext

		public override DataContextType DataContextType
		{
			get { return DataContextType.AgencyShipmentContainer; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.JC_ContainerJobID; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(JobContainerSchema.JC_ContainerJobID, matchingValues.Key);
		}

		protected override AgencyShipmentContainer[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery query)
		{
			var agencyContainers = factory.GetCachedReadOnlyFactory().Load<AgencyShipmentContainer>(query);

			var containers = new List<AgencyShipmentContainer>();

			foreach (var agencyContainer in agencyContainers)
			{
				if (agencyContainer.Booking == null)
				{
					continue;
				}

				Type containerType = agencyContainer.Booking.IsBillOfLadingStage
					? typeof(BillOfLadingContainer)
					: typeof(AgencyBookingContainer);

				var containersInTargetFactory = factory.GetBizOsForPK(agencyContainer.PK.ToGuid());

				if (containersInTargetFactory.Length == 0)
				{
					containers.Add((AgencyShipmentContainer)factory.ImportFromAnotherFactory(agencyContainer, containerType));
				}
				else
				{
					var containerInTargetFactory = containersInTargetFactory.FirstOrDefault(containerType.IsInstanceOfType) ??
						factory.Load(containerType, agencyContainer.PK);

					containers.Add((AgencyShipmentContainer)containerInTargetFactory);
				}
			}

			return containers.ToArray();
		}

		#endregion

		#region Event Management

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			List<KeyValuePair<TypeWithDescription, IZType>> result = null;

			if (ParentBO != null)
			{
				var references = new AgencyShipmentContainerReferences(ParentBO);
				result = references.GetEventContextValues();
			}

			return result == null || !result.Any() ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AgencyShipmentContainerEventParentFinder(factory, this, logger);
		}

		#endregion

		#region Shipment Management

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new AgencyShipmentContainerDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new AgencyShipmentContainerDataObjectWriter(writeManager);
		}

		#endregion

		#region Implementation

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.ShipmentExportDirectory.Value; }
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion
	}
}


