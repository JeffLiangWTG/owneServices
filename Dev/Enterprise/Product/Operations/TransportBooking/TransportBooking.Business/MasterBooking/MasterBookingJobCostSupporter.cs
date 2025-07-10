using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Business
{
	public sealed class MasterBookingJobCostSupporter : IGenericJobCostSupporter
	{
		public MasterBookingJobCostSupporter(DtbBooking booking)
		{
			Argument.NotNull(booking, nameof(booking));

			MasterBooking = booking;
		}

#if DEBUG
		public
#endif
		readonly DtbBooking MasterBooking;

		IJobInvoicingPlugIn[] SubParentShipments
		{
			get
			{
				if (subParentShipments == null)
				{
					var subParentShipmentCollection = new List<IJobInvoicingPlugIn>();
					var dictionary = new Dictionary<IJobInvoicingPlugIn, DtbBookingConsolidationJobInvoicingPlugIn>();

					foreach (var booking in MasterBooking.SubBookings)
					{
						var shipment = GetParentShipment(booking);
						if (shipment != null)
						{
							if (!dictionary.TryGetValue(shipment, out var bookingInvoicingPlugIn))
							{
								bookingInvoicingPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(shipment);
								dictionary.Add(shipment, bookingInvoicingPlugIn);
							}
							bookingInvoicingPlugIn.Bookings.Add(booking);
						}
					}
					subParentShipmentCollection.AddRange(dictionary.Values);
					subParentShipments = subParentShipmentCollection.ToArray();
				}

				return subParentShipments;
			}
		}

		IJobInvoicingPlugIn[] subParentShipments;

		ZGuid IGenericJobCostSupporter.PK => MasterBooking.PK;
		ZString IGenericJobCostSupporter.Type => MasterBooking.TablePrefix;
		ZGuid[] IGenericJobCostSupporter.ShipmentsListPKs => SubParentShipments.Select(job => job.PK).ToArray();
		IJobInvoicingPlugIn[] IGenericJobCostSupporter.ShipmentsList => SubParentShipments;
		bool IGenericJobCostSupporter.HasChanges => MasterBooking.HasChanges;
		bool IGenericJobCostSupporter.IsInDatabase => MasterBooking.IsInDatabase;
		DocumentSupporter IGenericJobCostSupporter.DocumentSupporter => ((IDocumentSupportable)MasterBooking).DocumentSupporter;
		ZString IGenericJobCostSupporter.MasterBillNum => "";
		ZString IGenericJobCostSupporter.TransportMode => "";
		ZString IGenericJobCostSupporter.TotalChargeableUnit => DtbTransportTotalsHelper.TotalWeightUnit;
		Directions IGenericJobCostSupporter.Direction => Directions.Unknown;
		bool IGenericJobCostSupporter.IsBuyersConsol => false;
		ZString IGenericJobCostSupporter.PortOfLoading => "";
		ZString IGenericJobCostSupporter.PortOfDischarge => "";
		ZString IGenericJobCostSupporter.ConsolMode => "";
		OrgHeader IGenericJobCostSupporter.SendingForwarder => null;
		OrgHeader IGenericJobCostSupporter.ReceivingForwarder => null;
		ManyToManyBusinessObjectCollection IGenericJobCostSupporter.Shipments => null;
		ZDateTime IGenericJobCostSupporter.ETD => ZDateTime.Empty;
		ZDateTime IGenericJobCostSupporter.ETA => ZDateTime.Empty;
		IEnumerable<ZString> IGenericJobCostSupporter.ExcludedApportionmentMethods { get { yield return AllocationMethod.FreeSpaceContribution; } }
		bool IGenericJobCostSupporter.IsApportionmentFilterEnabled => true;

		ZDecimal IGenericJobCostSupporter.FreeSpace => ZDecimal.Zero;

		SecurityCheckpoint IGenericJobCostSupporter.JobConsolCostingCheckPoint => Env.Security.None;

		IJobInvoicingPlugIn GetParentShipment(DtbBooking booking)
		{
			var consolidation = booking.ConsolidationSingleJob;
			return (consolidation != null && consolidation.Parent != null) ? consolidation.Parent.ParentWithWorkflow as IJobInvoicingPlugIn : null;
		}
		IEnumerable<ZString> DefaultChargeGroups()
		{
			return new ZString[] { ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Codes.TransportBooking };
		}
		ZGuid IGenericJobCostSupporter.GetCreditorPK(ZString chargeCode, ZGuid rateProviderOrgPK)
		{
			return DefaultChargeGroups().Contains(chargeCode) ? MasterBooking.Address.OrganisationPK : ZGuid.Empty;
		}
	}
}
