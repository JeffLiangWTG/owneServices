using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingInvoicingSupporter : JobInvoicingSupporter,
		IJobInvoicingSupporterWithChargeableFactorSource
	{
		public DtbBookingInvoicingSupporter(DtbBooking booking)
			: base(booking)
		{
			Booking = booking;
		}

		readonly DtbBooking Booking;

		public override JobInvoicingConsumerType ConsumerType => Booking.KM_IsAgentBooking ? JobInvoicingConsumerTypes.TransportBookingWithAgent : JobInvoicingConsumerTypes.TransportBooking;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.DtbBookingJobAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.DtbBookingJobInvoicing;
		}

		public override ZString ContainerMode
		{
			get { return Booking.IsContainerisedOnly ? Constants.ContainerModes.FCL : Constants.ContainerModes.LCL; }
		}

		public override ZDecimal ActualVolume
		{
			get { return Booking.IsLooseOnly ? Booking.GetTotalLoosePackageVolume().Amount : Booking.GetTotalContainerisedVolume().Amount; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return Booking.IsLooseOnly ? Booking.GetTotalLoosePackageVolume().Unit : Booking.GetTotalContainerisedVolume().Unit; }
		}

		public override ZDecimal ActualWeight
		{
			get { return Booking.IsLooseOnly ? Booking.GetTotalLoosePackageWeight().Amount : Booking.GetTotalContainerisedWeight().Amount; }
		}

		public override ZString ActualWeightUnit
		{
			get { return Booking.IsLooseOnly ? Booking.GetTotalLoosePackageWeight().Unit : Booking.GetTotalContainerisedWeight().Unit; }
		}

		public override int ContainerCount
		{
			get { return Booking.Containers.Sum(c => c.Package.KP_PackageQty); }
		}

		public override ZDecimal TEUCount
		{
			get { return Booking.Containers.Sum(c => c.Package.Container.ContainerType.RC_TEU); }
		}

		public override int OuterPackTotal
		{
			get { return Booking.LoosePackages.Sum(p => p.Package.KP_PackageQty); }
		}

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Booking.IsInDatabaseIncludingChildren; }
		}

		public override ZString TransportMode
		{
			get { return Constants.TransportModes.Road; }
		}

		public override OrgHeader Consignor
		{
			get { return null; } // bill to party?
		}

		public override ZGuid OverriddenDepartmentPK
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.TransportBookingJobsDefaultDept.Value;
			}
		}

		public override OrgHeader OverriddenDefaultLocalClient
		{
			get
			{
				var bookingOrSendingPartyOrgPK = Booking.BookedByOrganisationPK;
				return !bookingOrSendingPartyOrgPK.IsEmpty ? Booking.Factory.Load<OrgHeader>(bookingOrSendingPartyOrgPK) : null;
			}
		}

		public override ZString OperationalJobRef
		{
			get { return Booking.KM_TransportReference.IsEmpty ? Booking.KM_JobID : Booking.KM_TransportReference; }
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			var transportCompanyDocAddress = Booking.Address;
			return transportCompanyDocAddress != null ? transportCompanyDocAddress.Organisation : null;
		}

		public override ZString HouseBillNumber
		{
			get { return Booking.KM_TransportReference; } // or WayBill on parent?
		}

		public override GlbBranch OperationsBranch
		{
			get
			{
				GlbBranch result = null;

				if (UseRegistryFallbackForBranch)
				{
					var orderRulesForDefaulting = AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.Value;
					if (orderRulesForDefaulting.DefaultToBranchRelatedToPortOrWarehouseBranch > 0 && Booking.Instructions.Count > 0)
					{
						var instructions = Booking.Instructions.Cast<DtbBookingInstruction>();
						var pickupInstruction = instructions.FirstOrDefault(i => i.IsPickUp);
						if (pickupInstruction != null)
						{
							result = pickupInstruction.DepotForAddress;
						}
					}
				}
				else
				{
					result = GlbBranch.CurrentBranch;
				}

				return result;
			}
		}

		bool UseRegistryFallbackForBranch
		{
			get { return Booking.ParentID.IsEmpty; }
		}

		protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments)
		{
			return !Booking.IsCancelled;
		}

		public override bool IsExport
		{
			get { return Booking.IsPickupDirection; }
		}

		public override bool IsImport
		{
			get { return Booking.IsDeliveryDirection; }
		}

		public override bool IsDomestic
		{
			get { return Booking.IsLocalDirection; }
		}

		public override RefUNLOCO Origin
		{
			get { return IsExport || IsDomestic ? GlbBranch.CurrentBranch.HomePort : base.Origin; }
		}

		public override RefUNLOCO Destination
		{
			get { return IsImport || IsDomestic ? GlbBranch.CurrentBranch.HomePort : base.Destination; }
		}

		ChargeableFactorSource IJobInvoicingSupporterWithChargeableFactorSource.ChargeableFactorSource => ChargeableFactorSource.TransportBooking;
	}
}
