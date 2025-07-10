using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbAddressPoint : NonPersistentBusinessObject
	{
		#region Schema

		/// <summary>
		/// This is required for Query Analyzer to work properly
		/// </summary>
		public static class Schema
		{
			public const string TableName = DtbBookingConfirmationSchema.Constants.TableName;
		}

		#endregion

		#region Construction

		public DtbAddressPoint(PickupAndDeliveryPair pickupAndDeliveryPair)
			: this(pickupAndDeliveryPair.PickupAddress)
		{
			this.directDeliveryAddress = Argument.NotNull(pickupAndDeliveryPair.DeliveryAddress, "Delivery Address");
		}

		public DtbAddressPoint(IDocAddress address)
			: this(GetFactoryWithAddressNullCheck((BusinessObject)address))
		{
			this.address = address;
		}

		DtbAddressPoint(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		internal static DtbAddressPoint GetNewFakeDtbAddressPointForBinding(BusinessObjectFactory factory)
		{
			return new DtbAddressPoint(factory);
		}

		static BusinessObjectFactory GetFactoryWithAddressNullCheck(BusinessObject address)
		{
			return Argument.NotNull(address, "address").Factory;
		}

		public IDocAddress Address
		{
			get { return address; }
		}

		public IDocAddress DirectDeliveryAddress
		{
			get { return directDeliveryAddress; }
		}

		readonly IDocAddress address;
		readonly IDocAddress directDeliveryAddress;

		#endregion

		#region Related Entities

		#region Confirmations

		public DtbConsignmentConfirmationCollectionAdHoc Confirmations
		{
			get
			{
				if (confirmations == null)
				{
					confirmations = new DtbConsignmentConfirmationCollectionAdHoc(Factory);

					// update the GUI to reflect new totals (weight, volume etc.)
					confirmations.CollectionCountChange += (sender, e) =>
					{
						if (!ConfirmationsCountChangedSemaphore.IsSuspended)
						{
							consignmentID = null;
							ClearCaches();
							this.RefreshBinding();
						}
					};
					RegisterEditableChildObject(confirmations);
				}

				return confirmations;
			}
		}

		DtbConsignmentConfirmationCollectionAdHoc confirmations;

		Semaphore ConfirmationsCountChangedSemaphore
		{
			get { return confirmationsCountChangedSemaphore ?? (confirmationsCountChangedSemaphore = new Semaphore()); }
		}

		Semaphore confirmationsCountChangedSemaphore;

		/// <summary>
		/// This collection is used for the Confirmations user control in the RoutePlanner in direct mode.
		/// </summary>
		public DtbConsignmentConfirmationCollectionAdHoc PickupConfirmations_ForBinding
		{
			get
			{
				if (pickupConfirmations == null)
				{
					pickupConfirmations = (DtbConsignmentConfirmationCollectionAdHoc)Confirmations.Clone(); // this clones the CollectionCountChanged event as well
					pickupConfirmations.AdditionalFilter = new ZQuery(DtbBookingConfirmationSchema.KK_ConfirmationType, ConfirmationTypes.Codes.PickUp);
				}

				return pickupConfirmations;
			}
		}

		DtbConsignmentConfirmationCollectionAdHoc pickupConfirmations;

		#endregion

		#endregion

		#region SuspendConfirmationsCountChanged

		public IDisposable SuspendConfirmationsCountChanged()
		{
			return new SemaphoreManager(ConfirmationsCountChangedSemaphore);
		}

		#endregion

		#region Properties

		// calculated

		#region AddressPK

		[List("Lookups.Addresses")]
		[ResourceStringData("DtbAddressPoint|AddressPK", Caption = "Address")]
		public ZGuid AddressPK
		{
			get { return ((BusinessObject)Address).PK; }
		}

		#endregion

		#region AddressCompanyName

		[ResourceStringData("DtbAddressPoint|AddressCompanyName", ShortCaption = "Co. Name", Caption = "Company Name")]
		public ZString AddressCompanyName
		{
			get { return Address.E2_CompanyNameTruncated; }
		}

		#endregion

		#region AddressLine1

		[ResourceStringData("DtbAddressPoint|AddressLine1", ShortCaption = "Addr.", MediumCaption = "Address 1", Caption = "Address Line 1")]
		public ZString AddressLine1
		{
			get { return Address.E2_Address1; }
		}

		#endregion

		#region AddressCity

		[ResourceStringData("DtbAddressPoint|AddressCity", Caption = "City")]
		public ZString AddressCity
		{
			get { return Address.E2_City; }
		}

		#endregion

		#region AddressState

		[ResourceStringData("DtbAddressPoint|AddressState", Caption = "State")]
		public ZString AddressState
		{
			get { return Address.E2_State; }
		}

		#endregion

		#region AddressPostCode

		[ResourceStringData("DtbAddressPoint|AddressPostCode", ShortCaption = "P/C", Caption = "Postcode")]
		public ZString AddressPostCode
		{
			get { return Address.E2_Postcode; }
		}

		#endregion

		public ZString ZoneDescription
		{
			get
			{
				var firstConfirmation = Confirmations.FirstOrDefault();
				return firstConfirmation != null && firstConfirmation.Instruction != null && firstConfirmation.Instruction.Zone != null ? firstConfirmation.Instruction.Zone.TZ_ZoneName : ZString.Empty;
			}
		}

		public JobDocAddress DepotAddress
		{
			get
			{
				if (depotAddress == null)
				{
					var firstConfirmation = Confirmations.FirstOrDefault();
					var nextConfirmation = firstConfirmation.GetRelatedConfirmation();
					depotAddress = nextConfirmation != null ? nextConfirmation.Instruction.Address : null;
				}

				return depotAddress;
			}
		}

		JobDocAddress depotAddress;

		#region ConsignmentID

		[ResourceStringData("DtbAddressPoint|ConsignmentID", Caption = "Consignment ID")]
		public ZString ConsignmentID
		{
			get { return consignmentID ?? (consignmentID = GetConsignmentID()); }
		}

		string GetConsignmentID()
		{
			var firstIDFound = Confirmations.Any() ? Confirmations.First(c => !c.ConsignmentID.IsEmpty).ConsignmentID : ZString.Empty;
			return Confirmations.Any(c => c.ConsignmentID != firstIDFound) ? (ZString)Res.GetString("f0252c82-5e49-4244-8c22-44e0ee4a2481", "Many") : firstIDFound;
		}

		string consignmentID;

		#endregion

		#region DirectDeliveryAddressCompanyName

		[ResourceStringData("DtbAddressPoint|DirectDeliveryAddressCompanyName", ShortCaption = "Dlv. Co.", MediumCaption = "Dlv. Co. Name", Caption = "Delivery Company Name")]
		public ZString DirectDeliveryAddressCompanyName
		{
			get { return DirectDeliveryAddress != null ? DirectDeliveryAddress.E2_CompanyNameTruncated : ZString.Empty; }
		}

		#endregion

		#region DirectDeliveryAddressLine1

		[ResourceStringData("DtbAddressPoint|DirectDeliveryAddressLine1", ShortCaption = "Dlv. Addr.", MediumCaption = "Dlv. Address 1", Caption = "Delivery Address Line 1")]
		public ZString DirectDeliveryAddressLine1
		{
			get { return DirectDeliveryAddress != null ? DirectDeliveryAddress.E2_Address1 : ZString.Empty; }
		}

		#endregion

		#region DirectDeliveryAddressCity

		[ResourceStringData("DtbAddressPoint|DirectDeliveryAddressCity", MediumCaption = "Dlv. City", Caption = "Delivery City")]
		public ZString DirectDeliveryAddressCity
		{
			get { return DirectDeliveryAddress != null ? DirectDeliveryAddress.E2_City : ZString.Empty; }
		}

		#endregion

		#region DirectDeliveryAddressState

		[ResourceStringData("DtbAddressPoint|DirectDeliveryAddressState", MediumCaption = "Dlv. State", Caption = "Delivery State")]
		public ZString DirectDeliveryAddressState
		{
			get { return DirectDeliveryAddress != null ? DirectDeliveryAddress.E2_State : ZString.Empty; }
		}

		#endregion

		#region DirectDeliveryAddressPostCode

		[ResourceStringData("DtbAddressPoint|DirectDeliveryAddressPostCode", ShortCaption = "Dlv. P/C", Caption = "Delivery Postcode")]
		public ZString DirectDeliveryAddressPostCode
		{
			get { return DirectDeliveryAddress != null ? DirectDeliveryAddress.E2_Postcode : ZString.Empty; }
		}

		#endregion

		#region IsHazardous

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|IsHazardous", Caption = "Hazardous", ShortCaption = "Haz.")]
		public ZBool IsHazardous
		{
			get { return Confirmations.GetIsHazardous(); }
		}

		#endregion

		#region NumberOfDeliveries

		[ResourceStringData("DtbAddressPoint|NumberOfDeliveries", Caption = "Deliveries", ShortCaption = "DLV")]
		public ZInt NumberOfDeliveries
		{
			get { return Confirmations.Count(c => c.IsDelivery); }
		}

		#endregion

		#region NumberOfPickups

		[ResourceStringData("DtbAddressPoint|NumberOfPickups", Caption = "Pickups", ShortCaption = "PIC")]
		public ZInt NumberOfPickups
		{
			get { return Confirmations.Count(c => c.IsPickUp); }
		}

		#endregion

		#region RequiresRefrigeration

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|RequiresRefrigeration", Caption = "Refrigeration", ShortCaption = "Re-frig.")]
		public ZBool RequiresRefrigeration
		{
			get { return Confirmations.GetRequiresRefrigeration(); }
		}

		#endregion

		#region SpecialInstructionsExist

		[ResourceStringData("DtbAddressPoint|SpecialInstructionsExist", Caption = "Special Instructions", MediumCaption = "Instructions", ShortCaption = "Sp. Instr.")]
		public ZBool SpecialInstructionsExist
		{
			get { return Confirmations.GetSpecialInstructionsExist(); }
		}

		#endregion

		#region ServicesExist

		[ResourceStringData("DtbAddressPoint|ServicesExist", Caption = "Services Exist", MediumCaption = "Services Exist", ShortCaption = "Services")]
		public ZBool ServicesExist
		{
			get { return Confirmations.GetServicesExist(); }
		}

		#endregion

		// calculated -- dates

		#region DirectPickupEarliestEstimated

		[ResourceStringData("DtbAddressPoint|DirectPickupEarliestEstimated", Caption = "Earliest Pickup Estimated", ShortCaption = "Pic. Estimated")]
		public ZDateTime DirectPickupEarliestEstimated
		{
			get { return directPickupEarliestEstimated ?? (directPickupEarliestEstimated = GetEarliestValidDateTime(Confirmations.Where(c => c.IsPickUp), c => c.KK_Estimated)).Value; }
		}

		ZDateTime? directPickupEarliestEstimated;

		#endregion

		#region DirectPickupEarliestRequiredFrom

		[ResourceStringData("DtbAddressPoint|DirectPickupEarliestReqFrom", Caption = "Earliest Pickup Requested From", ShortCaption = "Pic. Req. From")]
		public ZDateTime DirectPickupEarliestRequiredFrom
		{
			get { return directPickupEarliestRequiredFrom ?? (directPickupEarliestRequiredFrom = GetEarliestValidDateTime(Confirmations.Where(c => c.IsPickUp), c => c.KK_RequiredFrom)).Value; }
		}

		ZDateTime? directPickupEarliestRequiredFrom;

		#endregion

		#region DirectPickupEarliestReqTo

		[ResourceStringData("DtbAddressPoint|DirectPickupEarliestRequiredTo", Caption = "Earliest Pickup Requested To", ShortCaption = "Pic. Req. To")]
		public ZDateTime DirectPickupEarliestRequiredTo
		{
			get { return directPickupEarliestRequiredTo ?? (directPickupEarliestRequiredTo = GetEarliestValidDateTime(Confirmations.Where(c => c.IsPickUp), c => c.KK_RequiredTo)).Value; }
		}

		ZDateTime? directPickupEarliestRequiredTo;

		#endregion

		#region DirectDeliveryEarliestEstimated

		[ResourceStringData("DtbAddressPoint|DirectDeliveryEarliestEstimated", Caption = "Earliest Delivery Estimated", ShortCaption = "Dlv. Estimated")]
		public ZDateTime DirectDeliveryEarliestEstimated
		{
			get { return directDeliveryEarliestEstimated ?? (directDeliveryEarliestEstimated = GetEarliestValidDateTime(Confirmations.Where(c => c.IsDelivery), c => c.KK_Estimated)).Value; }
		}

		ZDateTime? directDeliveryEarliestEstimated;

		#endregion

		#region DirectDeliveryEarliestRequiredFrom

		[ResourceStringData("DtbAddressPoint|DirectDeliveryEarliestRequiredFrom", Caption = "Earliest Delivery Requested From", ShortCaption = "Dlv. Req. From")]
		public ZDateTime DirectDeliveryEarliestRequiredFrom
		{
			get { return directDeliveryEarliestRequiredFrom ?? (directDeliveryEarliestRequiredFrom = GetEarliestValidDateTime(Confirmations.Where(c => c.IsDelivery), c => c.KK_RequiredFrom)).Value; }
		}

		ZDateTime? directDeliveryEarliestRequiredFrom;

		#endregion

		#region DirectDeliveryEarliestRequiredTo

		[ResourceStringData("DtbAddressPoint|EarliestReqTo", Caption = "Earliest Delivery Requested To", ShortCaption = "Dlv. Req. To")]
		public ZDateTime DirectDeliveryEarliestRequiredTo
		{
			get { return directDeliveryEarliestRequiredTo ?? (directDeliveryEarliestRequiredTo = GetEarliestValidDateTime(Confirmations.Where(c => c.IsDelivery), c => c.KK_RequiredTo)).Value; }
		}

		ZDateTime? directDeliveryEarliestRequiredTo;

		#endregion

		#region EarliestEstimated

		[ResourceStringData("DtbAddressPoint|EarliestEstimated", Caption = "Earliest Estimated", ShortCaption = "Estimated")]
		public ZDateTime EarliestEstimated
		{
			get { return earliestEstimated ?? (earliestEstimated = GetEarliestValidDateTime(Confirmations, c => c.KK_Estimated)).Value; }
		}

		ZDateTime? earliestEstimated;

		#endregion

		#region EarliestRequiredFrom

		[ResourceStringData("DtbAddressPoint|EarliestRequiredFrom", Caption = "Earliest Requested From", ShortCaption = "Req. From")]
		public ZDateTime EarliestRequiredFrom
		{
			get { return earliestRequiredFrom ?? (earliestRequiredFrom = GetEarliestValidDateTime(Confirmations, c => c.KK_RequiredFrom)).Value; }
		}

		ZDateTime? earliestRequiredFrom;

		#endregion

		#region EarliestRequiredTo

		[ResourceStringData("DtbAddressPoint|EarliestRequiredTo", Caption = "Earliest Requested To", ShortCaption = "Req. To")]
		public ZDateTime EarliestRequiredTo
		{
			get { return earliestRequiredTo ?? (earliestRequiredTo = GetEarliestValidDateTime(Confirmations, c => c.KK_RequiredTo)).Value; }
		}

		ZDateTime? earliestRequiredTo;

		#endregion

		#region GetEarliestValidDate

		ZDateTime GetEarliestValidDateTime(IEnumerable<DtbConsignmentConfirmation> consignmentConfirmations, Func<DtbConsignmentConfirmation, ZDateTime> getDateTime)
		{
			var dates = consignmentConfirmations.Select(getDateTime).Where(dt => dt.IsValid).ToArray();
			return dates.Any() ? dates.Min() : ZDateTime.Empty;
		}

		#endregion

		// calculated -- totals

		#region CompleteBookedPickupPackageSummary

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|CompleteBookedPickupPackageSummary", Caption = "Booked Pickup", ShortCaption = "Booked PIC")]
		public ZString CompleteBookedPickupPackageSummary
		{
			get { return completeBookedPickupPackageSummary ?? (completeBookedPickupPackageSummary = Confirmations.GetCompleteBookedPickupPackageSummary()).Value; }
		}

		ZString? completeBookedPickupPackageSummary;

		#endregion

		#region CompleteDeliveryPackageSummary

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|CompleteDeliveryPackageSummary", Caption = "Delivery Summary", ShortCaption = "DLV Summary")]
		public ZString CompleteDeliveryPackageSummary
		{
			get { return completeDeliveryPackageSummary ?? (completeDeliveryPackageSummary = Confirmations.GetCompleteDeliveryPackageSummary()).Value; }
		}

		ZString? completeDeliveryPackageSummary;

		#endregion

		#region CompletePickupPackageSummary

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|CompletePickupPackageSummary", Caption = "Pickup Summary", ShortCaption = "PIC Summary")]
		public ZString CompletePickupPackageSummary
		{
			get { return completePickupPackageSummary ?? (completePickupPackageSummary = Confirmations.GetCompletePickupPackageSummary()).Value; }
		}

		ZString? completePickupPackageSummary;

		#endregion

		#region TotalDeliveryPackages

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalDeliveryPackages", Caption = "Delivery Packs", ShortCaption = "DLV Packs")]
		public ZInt TotalDeliveryPackages
		{
			get { return totalDeliveryPackages ?? (totalDeliveryPackages = Confirmations.GetTotalDeliveryPackages()).Value; }
		}

		ZInt? totalDeliveryPackages;

		#endregion

		#region TotalDeliveryWeight

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalDeliveryWeight", Caption = "Delivery Weight", ShortCaption = "DLV Wgt.")]
		public ZDecimal TotalDeliveryWeight
		{
			get { return totalDeliveryWeight ?? (totalDeliveryWeight = Confirmations.GetTotalDeliveryWeight()).Value; }
		}

		ZDecimal? totalDeliveryWeight;

		#endregion

		#region TotalDeliveryVolume

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalDeliveryVolume", Caption = "Delivery Volume", ShortCaption = "DLV Vol.")]
		public ZDecimal TotalDeliveryVolume
		{
			get { return totalDeliveryVolume ?? (totalDeliveryVolume = Confirmations.GetTotalDeliveryVolume()).Value; }
		}

		ZDecimal? totalDeliveryVolume;

		#endregion

		#region TotalPickupPackages

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalPickupPackages", Caption = "Pickup Packs", ShortCaption = "PIC Packs")]
		public ZInt TotalPickupPackages
		{
			get { return totalPickupPackages ?? (totalPickupPackages = Confirmations.GetTotalPickupPackages()).Value; }
		}

		ZInt? totalPickupPackages;

		#endregion

		#region TotalPickupWeight

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalPickupWeight", Caption = "Pickup Weight", ShortCaption = "PIC Wgt.")]
		public ZDecimal TotalPickupWeight
		{
			get { return totalPickupWeight ?? (totalPickupWeight = Confirmations.GetTotalPickupWeight()).Value; }
		}

		ZDecimal? totalPickupWeight;

		#endregion

		#region TotalPickupVolume

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalPickupVolume", Caption = "Pickup Volume", ShortCaption = "PIC Vol.")]
		public ZDecimal TotalPickupVolume
		{
			get { return totalPickupVolume ?? (totalPickupVolume = Confirmations.GetTotalPickupVolume()).Value; }
		}

		ZDecimal? totalPickupVolume;

		#endregion

		#region TotalWeightUnit

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalWeightUnit", Caption = "Weight Unit", MediumCaption = "Unit", ShortCaption = "UQ")]
		public ZString TotalWeightUnit
		{
			get { return DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		#endregion

		#region TotalVolumeUnit

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalVolumeUnit", Caption = "Volume Unit", MediumCaption = "Unit", ShortCaption = "UQ")]
		public ZString TotalVolumeUnit
		{
			get { return DtbTransportTotalsHelper.TotalVolumeUnit; }
		}

		#endregion

		#region ClearCaches

		void ClearCaches()
		{
			completeDeliveryPackageSummary = null;
			totalDeliveryPackages = null;
			totalDeliveryWeight = null;
			totalDeliveryVolume = null;

			completeBookedPickupPackageSummary = null;
			completePickupPackageSummary = null;
			totalPickupPackages = null;
			totalPickupWeight = null;
			totalPickupVolume = null;

			directPickupEarliestEstimated = null;
			directPickupEarliestRequiredFrom = null;
			directPickupEarliestRequiredTo = null;
			directDeliveryEarliestEstimated = null;
			directDeliveryEarliestRequiredFrom = null;
			directDeliveryEarliestRequiredTo = null;
			earliestEstimated = null;
			earliestRequiredFrom = null;
			earliestRequiredTo = null;
		}

		#endregion

		#endregion

		#region Lookups

		public DtbAddressPointLookups Lookups
		{
			get { return GetNewLookups(); }
		}

		DtbAddressPointLookups GetNewLookups()
		{
			return new DtbAddressPointLookups(this);
		}

		#endregion
	}
}
