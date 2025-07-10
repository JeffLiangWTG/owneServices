using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingJobCostSupporter : GenericJobCostSupporter
	{
		public DtbBookingJobCostSupporter(DtbBooking booking)
		{
			Booking = booking ?? throw new ArgumentNullException(nameof(booking));
		}

		DtbBooking Booking { get; }

		public bool HasConsolParent => Booking.ConsolidationSingleJob.KB_ParentTableCode == JobConsolSchema.Constants.Prefix;

		ConsolShipmentCollection ParentShipmentCollection => HasConsolParent ? ConsolShipmentProvider.Shipments : null;
		ConsolShipmentProvider ConsolShipmentProvider => consolShipmentProvider ?? new ConsolShipmentProvider(Booking.ConsolidationSingleJob.ParentBO);
		readonly ConsolShipmentProvider consolShipmentProvider;

		IJobInvoicingPlugIn[] ParentShipments => HasConsolParent ? ParentShipmentCollection.Cast<IJobInvoicingPlugIn>().ToArray() : Array.Empty<IJobInvoicingPlugIn>();

		public override ZGuid PK => HasConsolParent ? Booking.PK : ZGuid.Empty;

		public override ZString Type => DtbBookingSchema.Constants.Prefix;

		public override ZGuid[] ShipmentsListPKs => ParentShipments.Select(s => s.PK).ToArray();

		public override IJobInvoicingPlugIn[] ShipmentsList => ParentShipments;

		public override bool HasChanges => Booking.HasChanges;

		public override bool IsInDatabase => Booking.IsInDatabase;

		public override DocumentSupporter DocumentSupporter => ((IDocumentSupportable)Booking).DocumentSupporter;

		public override ZString TotalChargeableUnit => DtbTransportTotalsHelper.TotalWeightUnit;

		public override ManyToManyBusinessObjectCollection Shipments => ParentShipmentCollection;

		public override IEnumerable<ZString> ExcludedApportionmentMethods => new ZString[] { AllocationMethod.FreeSpaceContribution };

		public IEnumerable<ZString> DefaultChargeGroups => new ZString[] { ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Codes.TransportBooking };

		public override ZGuid GetCreditorPK(ZString chargeGroup, ZGuid rateProviderOrgPK) => DefaultChargeGroups.Contains(chargeGroup) ? Booking.Address.OrganisationPK : ZGuid.Empty;
	}
}
