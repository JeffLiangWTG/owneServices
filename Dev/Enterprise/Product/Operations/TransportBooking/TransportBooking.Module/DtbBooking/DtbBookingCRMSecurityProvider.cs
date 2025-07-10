using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Module
{
	internal class DtbBookingCRMSecurityProvider : CRMSecurityProvider<DtbBooking>
	{
		public override CRMSecurity CRMSecurity => Env.Security.DtbBookingCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => true;

		protected override void AppendAdditionalJobHeaderQuery(ZDBOnlySubQuery jobHeaderQuery)
		{
			var noLocalChargesAddressQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			noLocalChargesAddressQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			noLocalChargesAddressQuery.AddToFilter(JobHeaderSchema.JH_OA_LocalChargesAddr, DBNull.Value);
			jobHeaderQuery.AddAsUnionQuery(noLocalChargesAddressQuery, true);

			var parentIdQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);
			parentIdQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			var noHeaderJobShipmentQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), JobShipmentSchema.PK);
			noHeaderJobShipmentQuery.AddSubQuery(JobShipmentSchema.PK, parentIdQuery, JoinCondition.And);

			var jobShipmentQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), JobShipmentSchema.PK);
			jobShipmentQuery.AddSubQuery(JobShipmentSchema.PK, jobHeaderQuery, JoinCondition.And);
			jobShipmentQuery.AddSubQuery(noHeaderJobShipmentQuery, JoinCondition.Or);

			var consolidationQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBookingConsolidation>(), DtbBookingConsolidationSchema.PK);
			consolidationQuery.AddToFilter(DtbBookingConsolidationSchema.KB_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			consolidationQuery.AddSubQuery(DtbBookingConsolidationSchema.KB_ParentID, jobShipmentQuery, JoinCondition.And);
			consolidationQuery.AddToFilter(JoinCondition.Or, DtbBookingConsolidationSchema.KB_ParentTableCode, JobConsolSchema.Constants.Prefix);

			var bookingQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.PK);
			bookingQuery.AddSubQuery(DtbBookingSchema.KM_KB_Booking, consolidationQuery, JoinCondition.And);
			jobHeaderQuery.AddAsUnionQuery(bookingQuery, true);

			var parentIdConsolidationQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBookingConsolidation>(), DtbBookingConsolidationSchema.PK);
			parentIdConsolidationQuery.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, DBNull.Value);

			var noHeaderBookingQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.PK);
			noHeaderBookingQuery.AddSubQuery(DtbBookingSchema.KM_KB_Booking, parentIdConsolidationQuery, JoinCondition.And);
			noHeaderBookingQuery.AddSubQuery(DtbBookingSchema.PK, parentIdQuery, JoinCondition.And);
			jobHeaderQuery.AddAsUnionQuery(noHeaderBookingQuery, true);
		}
	}
}
