using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyContainerProcessTask : ContainerProcessTask, Integration.Agency.IAgencyContainerProcessTask
	{
		public AgencyContainerProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get
			{
				if (parentType == null)
				{
					parentType = GetTypeFromLoadedContainer() ?? GetTypeFromSavedContainer();
				}

				return parentType;
			}
		}

		Type parentType;

		Type GetTypeFromLoadedContainer()
		{
			var agencyContainer = Factory.GetBizOsForPK(P9_ParentID.ToGuid())
				.OfType<AgencyShipmentContainer>()
				.FirstOrDefault();

			return agencyContainer != null ? agencyContainer.GetType() : null;
		}

		Type GetTypeFromSavedContainer()
		{
			var query = new ZDBOnlyQuery(typeof(BillOfLadingContainer));
			query.AddToFilter(JobContainerSchema.PK, P9_ParentID);

			var shipmentQuery = new ZDBOnlySubQuery(typeof(BillOfLading), JobShipmentSchema.PK);
			shipmentQuery.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());

			query.AddSubQuery(JobContainerSchema.JC_JS_FCLBookingOnlyLink, shipmentQuery, JoinCondition.And);

			var billOfLadingContainers = Factory.Load<BillOfLadingContainer>(query);

			return billOfLadingContainers.Length > 0 ? typeof(BillOfLadingContainer) : typeof(AgencyBookingContainer);
		}

		public new AgencyShipmentContainer Parent
		{
			get { return (AgencyShipmentContainer)base.Parent; }
		}
	}
}
