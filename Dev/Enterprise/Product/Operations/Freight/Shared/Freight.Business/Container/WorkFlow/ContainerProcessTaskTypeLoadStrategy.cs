using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ContainerProcessTaskTypeLoadStrategy : IProcessTaskLoadStrategy
	{
		Type IProcessTaskLoadStrategy.GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			Type result = null;

			if (parentID.IsValid)
			{
				var container = GetLoadedContainer(parentID, factory) ?? GetContainerFromDatabase(parentID, factory);

				if (container != null && !container.IsDeleted && container.Booking != null && container.Booking.JS_IsShipping)
				{
					result = ObjectFactory.GetType<Integration.Agency.IAgencyContainerProcessTask>();
				}
			}

			return result ?? typeof(ContainerProcessTask);
		}

		CommonContainer GetLoadedContainer(ZGuid parentID, BusinessObjectFactory factory)
		{
			return (CommonContainer)factory.GetBizOsForPK(parentID.ToGuid())
				.FirstOrDefault();
		}

		CommonContainer GetContainerFromDatabase(ZGuid parentID, BusinessObjectFactory factory)
		{
			return factory.GetCachedReadOnlyFactory().Load<CommonContainer>(parentID);
		}

		void IProcessTaskLoadStrategy.AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			if (typeof(Integration.Agency.IAgencyShipmentContainer).IsAssignableFrom(workflowDescriptor.WorkflowProviderType))
			{
				var parentShipmentQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				parentShipmentQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				subQuery.AddSubQuery(parentShipmentQuery, JoinCondition.And);
			}
			else
			{
				var noBookingQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.PK);
				noBookingQuery.AddToFilter(JobContainerSchema.JC_JS_FCLBookingOnlyLink, null);

				var parentShipmentQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				parentShipmentQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, true);

				noBookingQuery.AddSubQuery(parentShipmentQuery, JoinCondition.Or);

				subQuery.AddSubQuery(noBookingQuery, JoinCondition.And);
			}
		}
	}
}
