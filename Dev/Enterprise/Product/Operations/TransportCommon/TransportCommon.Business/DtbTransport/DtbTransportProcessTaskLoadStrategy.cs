using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public class DtbTransportProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		#region GetTypeForLoad

		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			var transport = parentID.IsValid ? factory.Load<IDtbTransport>(parentID) : null;
			return MapTransportToProcessTaskType(transport);
		}

		#endregion

		#region AddAdditionalParentFilters

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			var transportType = (typeof(IDtbBookingConsignment).IsAssignableFrom(workflowDescriptor.WorkflowProviderType)) ? new[] { TransportConsolidationJobTypes.Codes.Consignment }
				: new[] { TransportConsolidationJobTypes.Codes.Booking, TransportConsolidationJobTypes.Codes.HighVolumeLowValue };
			var subQueryForTransportType = GetSubQueryForTransportType(transportType);
			subQuery.AddSubQuery(subQueryForTransportType, JoinCondition.And);
		}

		#endregion

		#region GetSubQueryForTransportType

		ZDBOnlySubQuery GetSubQueryForTransportType(string[] transportTypeConstant)
		{
			var jobTypeSubQuery = new ZDBOnlySubQuery(typeof(DtbTransportConsolidation), DtbBookingSchema.KM_KB_Booking);
			jobTypeSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, transportTypeConstant);
			return jobTypeSubQuery;
		}

		#endregion

		#region GetSubQueryForTransportType

		Type MapTransportToProcessTaskType(IDtbTransport transport)
		{
			return transport != null && ObjectFactory.GetType<IDtbBookingConsignment>().IsAssignableFrom(transport.GetType()) ? ObjectFactory.GetType<IDtbBookingConsignmentProcessTask>() : ObjectFactory.GetType<IDtbBookingProcessTask>();
		}

		#endregion
	}
}
