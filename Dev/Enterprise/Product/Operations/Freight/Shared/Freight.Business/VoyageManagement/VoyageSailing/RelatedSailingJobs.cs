using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public partial class RelatedSailingJobs
	{
		#region Consol / Shipment

		public JobSailingRelatedJob[] LoadRelatedConsolJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedConsolJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		public JobSailingRelatedJob[] LoadRelatedConsolJobs(JobSailing sailing, params ZString[] relatedSailingDates)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobConsolTransportSchema.JW_JX, sailing.PK);
			filter.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Consol);

			Transport[] transports = sailing.Factory.Load<Transport>(filter);
			foreach (Transport transport in transports)
			{
				if (sailing.Voyage != null && sailing.Voyage.ParentConsolType != null)
				{
					transport.ParentType = sailing.Voyage.ParentConsolType;
				}

				CommonConsol consol = (CommonConsol)transport.Parent;
				if (consol != null && consol.JK_IsForwarding &&
					IsRelatedBySailingDateChange(sailing, relatedSailingDates, consol.JK_RL_NKLoadPort, consol.JK_RL_NKDischargePort, consol.Factory))
				{
					transport.ParentType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>();
					consol = (CommonConsol)transport.Parent;
					result.Add(new JobSailingRelatedJob(consol, sailing, consol.JK_UniqueConsignRef, ControllerIDs.JobConsol));
				}
			}
			result.Sort((x, y) => x.JobNumber.CompareTo(y.JobNumber));
			return result.ToArray();
		}

		public JobSailingRelatedJob[] LoadRelatedConsolAndShipmentJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedConsolAndShipmentJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		JobSailingRelatedJob[] LoadRelatedConsolAndShipmentJobs(JobSailing sailing, params ZString[] relatedSailingDates)
		{
			var result = new List<JobSailingRelatedJob>();
			var consolJobs = LoadRelatedConsolJobs(sailing, relatedSailingDates);

			foreach (JobSailingRelatedJob consolJob in consolJobs)
			{
				result.Add(consolJob);
				AddShipments(sailing, result, ((CommonConsol)consolJob.Job).Shipments.Cast<CommonShipment>());
			}

			AddShipments(sailing, result, LoadRelatedShipments(sailing, relatedSailingDates));
			return result.ToArray();
		}

		static void AddShipments(JobSailing sailing, List<JobSailingRelatedJob> result, IEnumerable<CommonShipment> relatedShipments)
		{
			var shipments = new List<CommonShipment>(relatedShipments);
			shipments.Sort((x, y) => x.JS_UniqueConsignRef.CompareTo(y.JS_UniqueConsignRef));
			foreach (CommonShipment shipment in shipments)
			{
				result.Add(new JobSailingRelatedJob(shipment, sailing, shipment.JS_UniqueConsignRef, ControllerIDs.JobShipment));
			}
		}

		public IEnumerable<CommonShipment> LoadRelatedShipments(JobSailing sailing, params ZString[] relatedSailingDates)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobConsolTransportSchema.JW_JX, sailing.PK);
			filter.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Shipment);

			Transport[] transports = sailing.Factory.Load<Transport>(filter);
			foreach (Transport transport in transports)
			{
				transport.ParentType = typeof(CommonShipment);
				var shipment = (CommonShipment)transport.Parent;
				if (shipment != null && IsRelatedBySailingDateChange(sailing, relatedSailingDates, shipment.JS_RL_NKOrigin, shipment.JS_RL_NKDestination, shipment.Factory))
				{
					yield return shipment;
				}
			}
		}

		#endregion

		#region Customs Declaration

		public JobSailingRelatedJob[] LoadRelatedDeclarationJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedDeclarationJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		public JobSailingRelatedJob[] LoadRelatedDeclarationJobs(JobSailing sailing, params ZString[] relatedSailingDates)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobConsolTransportSchema.JW_JX, sailing.PK);
			filter.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Declaration);

			Transport[] transports = sailing.Factory.Load<Transport>(filter);
			foreach (Transport transport in transports)
			{
				transport.ParentType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				BusinessObject declaration = (BusinessObject)transport.Parent;
				if (declaration != null && IsDeclarationRelatedBySailingDateChange(declaration, sailing, relatedSailingDates))
				{
					result.Add(new JobSailingRelatedJob(declaration, sailing, (ZString)declaration[JobDeclarationSchema.JE_DeclarationReference], ControllerIDs.Customs.JobDeclaration));
				}
			}
			result.Sort(delegate(JobSailingRelatedJob x, JobSailingRelatedJob y)
			{ return x.JobNumber.CompareTo(y.JobNumber); });

			return result.ToArray();
		}

		bool IsDeclarationRelatedBySailingDateChange(BusinessObject declaration, JobSailing sailing, params ZString[] relatedSailingDates)
		{
			bool isExportWithModifiedDepartureDate =
				new ZBool(declaration["IsExport"]) &&
				IsSailingOriginDateChanged(sailing, relatedSailingDates);
			bool isImportWithModifiedArrivalDate =
				new ZBool(declaration["IsImport"]) &&
				IsSailingDestinationDateChanged(sailing, relatedSailingDates);
			return isExportWithModifiedDepartureDate || isImportWithModifiedArrivalDate;
		}

		#endregion

		#region CFS Load List

		public JobSailingRelatedJob[] LoadRelatedLoadListJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedLoadListJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		JobSailingRelatedJob[] LoadRelatedLoadListJobs(JobSailing sailing, params ZString[] relatedSailingDates)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobConsolTransportSchema.JW_JX, sailing.PK);
			filter.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Consol);

			Transport[] transports = sailing.Factory.Load<Transport>(filter);
			foreach (Transport transport in transports)
			{
				transport.ParentType = ObjectFactory.GetType<CFS.ICFSLoadListConsol>();
				CommonConsol consol = (CommonConsol)transport.Parent;
				if (consol != null && consol.JK_IsCFS && !consol.JK_IsForwarding &&
					IsRelatedBySailingDateChange(sailing, relatedSailingDates, consol.JK_RL_NKLoadPort, consol.JK_RL_NKDischargePort, consol.Factory))
				{
					result.Add(new JobSailingRelatedJob(consol, sailing, consol.JK_UniqueConsignRef, ControllerIDs.LoadListConsol));
				}
			}
			return result.ToArray();
		}

		#endregion

		#region Export Booking

		public JobSailingRelatedJob[] LoadRelatedExportBookingJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedExportBookingJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		JobSailingRelatedJob[] LoadRelatedExportBookingJobs(JobSailing sailing, params ZString[] relatedSailingDates)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			if (IsSailingOriginDateChanged(sailing, relatedSailingDates))
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_JX, sailing.PK);
				query.AddToFilter(JobShipmentSchema.JS_IsBooking, true);
				query.OrderBy = JobShipmentSchema.JS_UniqueConsignRef.Name;

				CommonShipment[] shipments = (CommonShipment[])sailing.Factory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(query);
				foreach (CommonShipment shipment in shipments)
				{
					result.Add(new JobSailingRelatedJob(shipment, sailing, shipment.JS_UniqueConsignRef, ControllerIDs.QuotedBookings));
				}
			}
			return result.ToArray();
		}

		#endregion

		#region Shipping Agency

		public JobSailingRelatedJob[] LoadRelatedAgencyBookingJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedAgencyBookingJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		public JobSailingRelatedJob[] LoadRelatedAgencyDocumentationJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedAgencyDocumentationJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		JobSailingRelatedJob[] LoadRelatedAgencyBookingJobs(JobSailing sailing, ZString[] relatedSailingDates)
		{
			ZQuery statusQuery = new ZQuery(JobShipmentSchema.JS_ShipmentStatus, SQLComparisonOperator.NotEqual, new ZString[]
			{
				ShipmentStatusList.Codes.Confirmed,
				ShipmentStatusList.Codes.WebFwdInstruction,
			});

			return LoadRelatedAgencyJobs(sailing, relatedSailingDates, ObjectFactory.GetType<Agency.IAgencyBooking>(), statusQuery, ControllerIDs.AgencyBooking);
		}

		JobSailingRelatedJob[] LoadRelatedAgencyDocumentationJobs(JobSailing sailing, ZString[] relatedSailingDates)
		{
			ZQuery statusQuery = new ZQuery(JobShipmentSchema.JS_ShipmentStatus, new ZString[]
			{
				ShipmentStatusList.Codes.Confirmed,
				ShipmentStatusList.Codes.WebFwdInstruction,
			});
			return LoadRelatedAgencyJobs(sailing, relatedSailingDates, ObjectFactory.GetType<Agency.IBillOfLading>(), statusQuery, ControllerIDs.AgencyBillOfLading);
		}

		JobSailingRelatedJob[] LoadRelatedAgencyJobs(JobSailing sailing, ZString[] relatedSailingDates, Type businessObjectType, ZQuery statusQuery, ControllerID controllerID)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.JS_JX, sailing.PK);
			query.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_IsShipping, true);
			query.AddToFilter(statusQuery, JoinCondition.And);
			query.OrderBy = JobShipmentSchema.JS_UniqueConsignRef.Name;

			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (CommonShipment shipment in sailing.Factory.Load(businessObjectType, query))
			{
				if ((IsPortHandled(shipment.Factory, shipment.JS_RL_NKOrigin) && IsSailingOriginDateChanged(sailing, relatedSailingDates)) ||
					(IsPortHandled(shipment.Factory, shipment.JS_RL_NKDestination) && IsSailingDestinationDateChanged(sailing, relatedSailingDates)))
				{
					result.Add(new JobSailingRelatedJob(shipment, sailing, shipment.JS_UniqueConsignRef, controllerID));
				}
			}
			return result.ToArray();
		}

		#endregion

		#region Local Cartage

		public JobSailingRelatedJob[] LoadRelatedLocalCartageJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedLocalCartageJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		JobSailingRelatedJob[] LoadRelatedLocalCartageJobs(JobSailing sailing, params ZString[] relatedSailingDates)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();

			foreach (BusinessObject cartage in (BusinessObject[])sailing.Factory.Load<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_JX_Sailing, sailing.PK)))
			{
				if ((((ICommonCartage)cartage).IsExportOrOrigin && IsSailingOriginDateChanged(sailing, relatedSailingDates)) ||
					(((ICommonCartage)cartage).IsImportOrDestination && IsSailingDestinationDateChanged(sailing, relatedSailingDates)))
				{
					result.Add(new JobSailingRelatedJob(cartage, sailing, cartage[JobCartageSchema.JJ_ConsignmentID].ToString(), ControllerIDs.Cartage));
				}
			}

			return result.ToArray();
		}

		#endregion

		#region Orders

		public JobSailingRelatedJob[] LoadRelatedOrderJobs(JobScheduleChange[] scheduleChanges)
		{
			List<JobSailingRelatedJob> result = new List<JobSailingRelatedJob>();
			foreach (JobSailing sailing in GetSailingsFromScheduleChanges(scheduleChanges))
			{
				result.AddRange(LoadRelatedOrderJobs(sailing, GetSailingDateTypesChanged(sailing, scheduleChanges)));
			}
			return result.ToArray();
		}

		JobSailingRelatedJob[] LoadRelatedOrderJobs(JobSailing sailing, params ZString[] relatedSailingDates)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobOrderLineDeliverContainerSchema.J5_RV_NKArrivalVessel, sailing.Voyage.JV_RV_NKVessel);
			query.AddToFilter(JoinCondition.And, JobOrderLineDeliverContainerSchema.J5_Voyage, sailing.Voyage.JV_VoyageFlight);

			Dictionary<BusinessObject, JobSailingRelatedJob> result = new Dictionary<BusinessObject, JobSailingRelatedJob>();
			BusinessObject[] orderContainers = (BusinessObject[])sailing.Factory.Load<Forwarding.IOrderLineDeliverContainer>(query);
			foreach (BusinessObject orderContainer in orderContainers)
			{
				if (IsOrderJobSailingRelatedJobBySailingChange(orderContainer, sailing, relatedSailingDates))
				{
					BusinessObject order = GetOrderFromContainer(orderContainer);
					if (!result.ContainsKey(order))
					{
						JobSailingRelatedJob jobSailingRelatedJob = new JobSailingRelatedJob(order, sailing, (ZString)order[JobOrderHeaderSchema.JD_OrderNumber], ControllerIDs.Orders);
						result.Add(order, jobSailingRelatedJob);
					}
				}
			}
			return new List<JobSailingRelatedJob>(result.Values).ToArray();
		}

		bool IsOrderJobSailingRelatedJobBySailingChange(BusinessObject orderContainer, JobSailing sailing, params ZString[] relatedSailingDates)
		{
			BusinessObject orderLineDelivery = GetOrderLineDeliveryFromContainer(orderContainer);
			ZString loadPort = (ZString)orderContainer[JobOrderLineDeliverContainerSchema.J5_RL_NKLoadPort];
			ZString dischargePort = (ZString)orderLineDelivery[JobOrderLineDeliverySchema.J4_RL_NKDestinationPort];

			bool result = false;
			result |=
				IsPortHandled(orderContainer.Factory, loadPort) &&
				sailing.Origin.JA_RL_NKPortOfLoading == loadPort &&
				IsSailingOriginDateChanged(sailing, relatedSailingDates);
			result |=
				IsPortHandled(orderContainer.Factory, dischargePort) &&
				sailing.Destination.JB_RL_NKPortOfDischarge == dischargePort &&
				IsSailingDestinationDateChanged(sailing, relatedSailingDates);
			return result;
		}

		BusinessObject GetOrderFromContainer(BusinessObject orderContainer)
		{
			PropertyDescriptor orderProperty = TypeDescriptor.GetProperties(orderContainer)["Order"];
			return (BusinessObject)orderProperty.GetValue(orderContainer);
		}

		BusinessObject GetOrderLineDeliveryFromContainer(BusinessObject orderContainer)
		{
			PropertyDescriptor orderLineDeliveryProperty = TypeDescriptor.GetProperties(orderContainer)["OrderLineDelivery"];
			return (BusinessObject)orderLineDeliveryProperty.GetValue(orderContainer);
		}

		#endregion

		#region Implementation

		JobSailing[] GetSailingsFromScheduleChanges(JobScheduleChange[] scheduleChanges)
		{
			if (scheduleChanges.Length == 0)
			{
				return Array.Empty<JobSailing>();
			}
			else
			{
				JobSailing[] sailings = scheduleChanges[0].Factory.Load<JobSailing>(scheduleChanges.GetSailingFilter());
				SortSailingsForTest(sailings);
				return sailings;
			}
		}

		ZString[] GetSailingDateTypesChanged(JobSailing sailing, JobScheduleChange[] scheduleChanges)
		{
			List<ZString> result = new List<ZString>();
			foreach (JobScheduleChange scheduleChange in scheduleChanges)
			{
				IScheduleChangeParent parent = scheduleChange.Parent;
				if (parent == null || !parent.PK.Equals(sailing[parent.SailingRefColumn]))
				{
					continue;
				}

				if (!result.Contains(scheduleChange.E7_DateType))
				{
					result.Add(scheduleChange.E7_DateType);
				}
			}
			return result.ToArray();
		}

		bool IsSailingOriginDateChanged(JobSailing sailing, ZString[] relatedSailingDatesArray)
		{
			IList<ZString> relatedSailingDates = relatedSailingDatesArray;
			return
				relatedSailingDates.Contains(ScheduleDateTypes.Codes.ETD) ||
				relatedSailingDates.Contains(ScheduleDateTypes.Codes.ATD) ||
				relatedSailingDates.Contains(ScheduleDateTypes.Codes.FCLCutOff) ||
				relatedSailingDates.Contains(ScheduleDateTypes.Codes.FCLReceivalCommences);
		}

		bool IsSailingDestinationDateChanged(JobSailing sailing, ZString[] relatedSailingDatesArray)
		{
			IList<ZString> relatedSailingDates = relatedSailingDatesArray;
			return
				relatedSailingDates.Contains(ScheduleDateTypes.Codes.ETA) ||
				relatedSailingDates.Contains(ScheduleDateTypes.Codes.ATA) ||
				relatedSailingDates.Contains(ScheduleDateTypes.Codes.FCLAvailable) ||
				relatedSailingDates.Contains(ScheduleDateTypes.Codes.FCLStorage);
		}

		bool IsRelatedBySailingDateChange(JobSailing sailing, ZString[] relatedSailingDates, ZString loadPort, ZString dischargePort, BusinessObjectFactory factory)
		{
			bool isExportJobWithModifiedDepartureDate = IsPortHandled(factory, loadPort) && IsSailingOriginDateChanged(sailing, relatedSailingDates);
			bool isImportJobWithModifiedArrivalDate = IsPortHandled(factory, dischargePort) && IsSailingDestinationDateChanged(sailing, relatedSailingDates);
			return isExportJobWithModifiedDepartureDate || isImportJobWithModifiedArrivalDate;
		}

		partial void SortSailingsForTest(JobSailing[] sailings);

		bool IsPortHandled(BusinessObjectFactory factory, ZString port)
		{
			GlbBranchExtraPorts[] additionalPorts = factory.Load<GlbBranchExtraPorts>(new ZQuery(GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, SQLComparisonOperator.StartsWith, port.SubstringSafe(0, 2)));

			ZQuery glbBranchQuery = new ZQuery(GlbBranchSchema.GB_IsActive, true);
			ZQuery glbBranchCountryQuery = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, port.SubstringSafe(0, 2));
			if (additionalPorts.Length > 0)
			{
				glbBranchCountryQuery.AddToFilter(JoinCondition.Or, GlbBranchSchema.PK, Array.ConvertAll(additionalPorts, e => e.GY_GB));
			}
			glbBranchQuery.AddToFilter(glbBranchCountryQuery);

			return factory.LoadTop1<GlbBranch>(glbBranchQuery) != null;
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Members

namespace Enterprise.Freight.Business
{
	public partial class RelatedSailingJobs
	{
		partial void SortSailingsForTest(JobSailing[] sailings)
		{
			Array.Sort(sailings, delegate(JobSailing s1, JobSailing s2)
			{
				int diff;

				diff = StringComparer.InvariantCulture.Compare(s1.JX_JV_NKVessel, s2.JX_JV_NKVessel);
				if (diff != 0)
				{
					return diff;
				}

				diff = StringComparer.InvariantCulture.Compare(s1.JX_JV_VoyageFlight, s2.JX_JV_VoyageFlight);
				if (diff != 0)
				{
					return diff;
				}

				diff = StringComparer.InvariantCulture.Compare(s1.JX_JA_RL_NKPortOfLoading, s2.JX_JA_RL_NKPortOfLoading);
				if (diff != 0)
				{
					return diff;
				}

				diff = StringComparer.InvariantCulture.Compare(s1.JX_JB_RL_NKPortOfDischarge, s2.JX_JB_RL_NKPortOfDischarge);
				return diff;
			});
		}
	}
}

#endregion

#endif
#endregion
