using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetRatingAdapter : LinehaulAndRunSheetRatingAdapter<DtbConsignmentRunSheet>
	{
		public DtbConsignmentRunSheetRatingAdapter(DtbConsignmentRunSheet runsheet)
			: base(runsheet)
		{
		}

		#region AdapterType

		public override AdapterType AdapterType => AdapterType.RunSheet;

		#endregion

		#region JobDatesProvider

		public override IJobDatesProvider JobDatesProvider => new DtbConsignmentRunSheetJobDatesProvider(Parent);

		#endregion

		#region IAutoRatingFreightInfo_FreightMode

		public override FreightMode FreightMode
		{
			get { return FreightRatingHelper.CalculateFreightMode(Parent.KG_TransportMode, ContainerMode); }
		}

		public override ZString ContainerMode
		{
			get { return Parent.KG_ContainerMode; }
		}

		#endregion

		#region Origin

		// Used as Location
		public override ILocation Origin
		{
			get
			{
				var addresses = Parent.HasNewConsignments ? GetAddressesFromConsignments() : GetAddressesFromBookingConsignments();
				var firstAddress = addresses.FirstOrDefault();

				ILocation result = null;
				if (firstAddress != null)
				{
					if (firstAddress.E2_AddressOverride)
					{
						result = firstAddress.Country;
					}
					else
					{
						var address = firstAddress.Address;
						if (address != null)
						{
							var relatedPort = address.EffectiveRelatedPortCode;
							if (relatedPort != null)
							{
								result = relatedPort;
							}
						}
					}
				}

				return result;
			}
		}

		IEnumerable<JobDocAddress> GetAddressesFromBookingConsignments()
		{
			var confirmations = Parent.RunSheetInstructions.SelectMany(i => i.Confirmations);
			var consignmentInstructions = confirmations.Select(c => c.Instruction);
			var addresses = consignmentInstructions.Where(i => i.Address.Address != null || i.Address.E2_AddressOverride).Select(i => i.Address);
			return addresses;
		}

		IEnumerable<JobDocAddress> GetAddressesFromConsignments()
		{
			var actions = Parent.RunSheetInstructions.SelectMany(i => i.Actions);
			var consignmentAddresses = actions.Select(c => c.ConsignmentAddress);
			var addresses = consignmentAddresses.Where(i => i.Address.Address != null || i.Address.E2_AddressOverride).Select(i => i.Address);
			return addresses;
		}

		#endregion

		#region StatusInformation

		public override AutoRatingStatusInfo StatusInformation
			=> Parent.RunSheetInstructions.Any()
				? new AutoRatingStatusInfo(true)
				: new AutoRatingStatusInfo
				(
					false,
					Res.GetString
					(
						"DtbConsignmentRunSheet|IAutoRating.StatusInformation",
						"{0} doesn't have at least 2 {1}. Cannot continue.",
						Parent.HumanReadableName,
						Parent.HasNewConsignments
							? Res.GetString("82ad008b-7961-4ae4-a92a-01a36d44b566", "Actions")
							: Res.GetString("4ff51961-d8de-485f-b81a-fa14d729eb50", "Instructions")
					)
				);

		#endregion

		#region Services

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = new JobServicesCollection();
				result.AddRange(GetServiceInfosFromJobServices(Parent.Services, ChargeCodeGroupList.Codes.TransportBooking));

				return result;
			}
		}

		#endregion

		#region Creditors

		public override Creditors Creditors => Creditors.New(OrgWithSource.NewFrom<OrgHeader>(Parent.KG_OH_TransportCoInfo));

		#endregion

		#region Carrier

		public override OrgHeader Carrier => Parent.TransportCo;

		#endregion

		#region CarrierServiceLevel

		protected override ZString CarrierServiceLevel => Parent.KG_PL_NKCarrierServiceLevel;

		#endregion

		#region Vehicle

		protected override RefEquipment Vehicle => Parent.Truck;

		#endregion

		#region Packages

		protected override IEnumerable<PkgPackage> Packages => GetPackages();

		IEnumerable<PkgPackage> GetPackages()
		{
			if (Parent.HasNewConsignments)
			{
				var consignments = Parent.RunSheetInstructions.SelectMany(i => i.Actions).Select(c => c.Consignment).Distinct();
				return consignments.SelectMany(c => c.LoosePackages);
			}
			else
			{
				var consignments = Parent.RunSheetInstructions.SelectMany(i => i.Confirmations).Select(c => c.Instruction.Booking).Distinct();
				return consignments.SelectMany(c => c.LoosePackages.Select(p => p.Package));
			}
		}

		#endregion

		#region DtbConsignmentRunSheetJobDatesProvider

		internal class DtbConsignmentRunSheetJobDatesProvider : JobDatesProvider<DtbConsignmentRunSheet>
		{
			public DtbConsignmentRunSheetJobDatesProvider(DtbConsignmentRunSheet runSheet)
				: base(runSheet) { }

			protected override ZDateTime GetDepartureDateCore()
			{
				return Parent.KG_StartTime.ToLocalZDateTime();
			}

			protected override ZDateTime GetArrivalDateCore()
			{
				return Parent.KG_EndTime.ToLocalZDateTime();
			}
		}

		#endregion

		#region Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var measures = (RateableMeasureSet)base.RateableMeasures;
				var timeSpan = Parent.KG_Duration.IsValid ? Parent.KG_Duration.ToTimeSpan() : TimeSpan.Zero;
				measures.Time = new TimeInfo(timeSpan);
				return measures;
			}
		}

		#endregion
	}
}
