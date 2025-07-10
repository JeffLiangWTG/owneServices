using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingConsolidationJobCostSupporter : IGenericJobCostSupporter
	{
		public DtbBookingConsolidationJobCostSupporter(DtbBookingConsolidation consolidationMultiJob)
		{
			Argument.NotNull(consolidationMultiJob, "consolidationMultiJob");

			ConsolidationMultiJob = consolidationMultiJob;
		}

#if DEBUG
		public
#endif
		readonly DtbBookingConsolidation ConsolidationMultiJob;

		IJobInvoicingPlugIn[] ParentShipments
		{
			get
			{
				if (parentShipments == null)
				{
					var parentShipmentCollection = new List<IJobInvoicingPlugIn>();
					var dictionary = new Dictionary<IJobInvoicingPlugIn, DtbBookingConsolidationJobInvoicingPlugIn>();

					foreach (var booking in ConsolidationMultiJob.Bookings)
					{
						var consolidation = booking.ConsolidationSingleJob;
						var parent = (consolidation != null && consolidation.Parent != null) ? consolidation.Parent.ParentWithWorkflow as IJobInvoicingPlugIn : null;

						if (parent != null)
						{
							if (consolidation.Parent.JobType == TransportParentTypes.Consol)
							{
								var consolShipmentProvider = new ConsolShipmentProvider(booking.ConsolidationSingleJob.ParentBO);
								foreach (var shipment in consolShipmentProvider.Shipments.Cast<IJobInvoicingPlugIn>())
								{
									AddShipmentToBookingInvoicingPlugin(dictionary, shipment, booking, supporterShouldUseParent: true);
								}
							}
							else
							{
								AddShipmentToBookingInvoicingPlugin(dictionary, parent, booking, supporterShouldUseParent: false);
							}
						}
						else
						{
							parentShipmentCollection.Add(booking);
						}
					}

					parentShipmentCollection.AddRange(dictionary.Values);
					parentShipments = parentShipmentCollection.ToArray();
				}

				return parentShipments;
			}
		}

		void AddShipmentToBookingInvoicingPlugin(Dictionary<IJobInvoicingPlugIn, DtbBookingConsolidationJobInvoicingPlugIn> dictionary, IJobInvoicingPlugIn shipment, DtbBooking booking, bool supporterShouldUseParent)
		{
			if (!dictionary.TryGetValue(shipment, out var bookingInvoicingPlugIn))
			{
				bookingInvoicingPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(shipment, supporterShouldUseParent: supporterShouldUseParent);
				dictionary.Add(shipment, bookingInvoicingPlugIn);
			}
			bookingInvoicingPlugIn.Bookings.Add(booking);
		}

		IJobInvoicingPlugIn[] parentShipments;

		ZString IGenericJobCostSupporter.ConsolMode
		{
			get { return ""; }
		}

		DocumentSupporter IGenericJobCostSupporter.DocumentSupporter
		{
			get { return ((IDocumentSupportable)ConsolidationMultiJob).DocumentSupporter; }
		}

		bool IGenericJobCostSupporter.HasChanges
		{
			get { return ConsolidationMultiJob.HasChanges; }
		}

		bool IGenericJobCostSupporter.IsBuyersConsol
		{
			get { return false; }
		}

		Directions IGenericJobCostSupporter.Direction => Directions.Unknown;

		bool IGenericJobCostSupporter.IsInDatabase
		{
			get { return ConsolidationMultiJob.IsInDatabase; }
		}

		ZString IGenericJobCostSupporter.MasterBillNum
		{
			get { return ""; }
		}

		IEnumerable<ZString> DefaultChargeGroups()
		{
			return new ZString[] { ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Codes.TransportBooking };
		}

		ZGuid IGenericJobCostSupporter.GetCreditorPK(ZString chargeCode, ZGuid rateProviderOrgPK)
		{
			return DefaultChargeGroups().Contains(chargeCode) ? ConsolidationMultiJob.Address.OrganisationPK : ZGuid.Empty;
		}

		ZGuid IGenericJobCostSupporter.PK
		{
			get { return ConsolidationMultiJob.PK; }
		}

		ZString IGenericJobCostSupporter.PortOfDischarge
		{
			get { return ""; }
		}

		ZString IGenericJobCostSupporter.PortOfLoading
		{
			get { return ""; }
		}

		OrgHeader IGenericJobCostSupporter.ReceivingForwarder
		{
			get { return null; }
		}

		OrgHeader IGenericJobCostSupporter.SendingForwarder
		{
			get { return null; }
		}

		ManyToManyBusinessObjectCollection IGenericJobCostSupporter.Shipments
		{
			get { return null; }
		}

		IJobInvoicingPlugIn[] IGenericJobCostSupporter.ShipmentsList
		{
			get { return ParentShipments; }
		}

		ZGuid[] IGenericJobCostSupporter.ShipmentsListPKs
		{
			get { return ParentShipments.Select(job => job.PK).ToArray(); }
		}

		ZString IGenericJobCostSupporter.TotalChargeableUnit
		{
			get { return DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		ZString IGenericJobCostSupporter.TransportMode
		{
			get { return ""; }
		}

		ZString IGenericJobCostSupporter.Type
		{
			get { return ConsolidationMultiJob.TablePrefix; }
		}

		ZDateTime IGenericJobCostSupporter.ETD
		{
			get { return ZDateTime.Empty; }
		}

		ZDateTime IGenericJobCostSupporter.ETA
		{
			get { return ZDateTime.Empty; }
		}

		IEnumerable<ZString> IGenericJobCostSupporter.ExcludedApportionmentMethods
		{
			get { yield return AllocationMethod.FreeSpaceContribution; }
		}

		bool IGenericJobCostSupporter.IsApportionmentFilterEnabled
		{
			get { return true; }
		}

		ZDecimal IGenericJobCostSupporter.FreeSpace => ZDecimal.Zero;

		SecurityCheckpoint IGenericJobCostSupporter.JobConsolCostingCheckPoint => Env.Security.None;
	}
}
