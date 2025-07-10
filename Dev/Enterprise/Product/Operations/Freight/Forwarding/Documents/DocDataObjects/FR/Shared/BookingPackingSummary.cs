using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class BookingPackingSummary : DocDataObject
	{
		public BookingPackingSummary(object identifier = default)
			: base(identifier)
		{
		}

		#region ShipmentID

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentIDInfo, ref shipmentNumber, value))
				{
					Validate(ShipmentIDInfo);
				}
			}
		}

		ZString shipmentNumber;

		public ZPropertyInfo ShipmentIDInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region ICVReference

		public ZString ICVReference
		{
			get => icvReference;
			set
			{
				if (SetNonPersistentPropertyValue(ICVReferenceInfo, ref icvReference, value))
				{
					Validate(ICVReferenceInfo);
				}
			}
		}
		ZString icvReference;

		public ZPropertyInfo ICVReferenceInfo => GetZPropertyInfo(nameof(ICVReference));

		#endregion

		#region TotalPackages

		public ZInt TotalPackages
		{
			get => totalPackages;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPackagesInfo, ref totalPackages, value))
				{
					Validate(TotalPackagesInfo);
				}
			}
		}
		ZInt totalPackages;

		public ZPropertyInfo TotalPackagesInfo => GetZPropertyInfo(nameof(TotalPackages));

		#endregion

		#region TotalPackagesUnit

		public ZString TotalPackagesUnit
		{
			get => totalPackagesUnit;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPackagesUnitInfo, ref totalPackagesUnit, value))
				{
					Validate(TotalPackagesUnitInfo);
				}
			}
		}
		ZString totalPackagesUnit;

		public ZPropertyInfo TotalPackagesUnitInfo => GetZPropertyInfo(nameof(TotalPackagesUnit));

		#endregion

		#region TotalWeight

		public Measurement TotalWeight
		{
			get => totalWeight;
			set => totalWeight = SetChild(totalWeight, value);
		}

		Measurement totalWeight;

		#endregion

		#region TotalVolume

		public Measurement TotalVolume
		{
			get => totalVolume;
			set => totalVolume = SetChild(totalVolume, value);
		}

		Measurement totalVolume;

		#endregion

		#region TotalOutturnedPackages

		public ZInt TotalOutturnedPackages
		{
			get => totalOutturnedPackages;
			set
			{
				if (SetNonPersistentPropertyValue(TotalOutturnedPackagesInfo, ref totalOutturnedPackages, value))
				{
					Validate(TotalOutturnedPackagesInfo);
				}
			}
		}
		ZInt totalOutturnedPackages;

		public ZPropertyInfo TotalOutturnedPackagesInfo => GetZPropertyInfo(nameof(TotalOutturnedPackages));

		#endregion

		#region TotalOutturnedWeight

		public Measurement TotalOutturnedWeight
		{
			get => totalOutturnedWeight;
			set => totalOutturnedWeight = SetChild(totalOutturnedWeight, value);
		}

		Measurement totalOutturnedWeight;

		#endregion

		#region TotalOutturnedVolume

		public Measurement TotalOutturnedVolume
		{
			get => totalOutturnedVolume;
			set => totalOutturnedVolume = SetChild(totalOutturnedVolume, value);
		}

		Measurement totalOutturnedVolume;

		#endregion

		#region UnpackedReference

		public ZString UnpackedReference
		{
			get => unpackedReference;
			set
			{
				if (SetNonPersistentPropertyValue(UnpackedReferenceInfo, ref unpackedReference, value))
				{
					Validate(UnpackedReferenceInfo);
				}
			}
		}
		ZString unpackedReference;

		public ZPropertyInfo UnpackedReferenceInfo => GetZPropertyInfo(nameof(UnpackedReference));

		#endregion

		#region SurplusIndicator

		public ZBool SurplusIndicator
		{
			get => surplusIndicator;
			set
			{
				if (SetNonPersistentPropertyValue(SurplusIndicatorInfo, ref surplusIndicator, value))
				{
					Validate(SurplusIndicatorInfo);
				}
			}
		}
		ZBool surplusIndicator;

		public ZPropertyInfo SurplusIndicatorInfo => GetZPropertyInfo(nameof(SurplusIndicator));

		#endregion

		#region UnpackingIndicator

		public ZBool UnpackingIndicator
		{
			get => unpackingIndicator;
			set
			{
				if (SetNonPersistentPropertyValue(UnpackingIndicatorInfo, ref unpackingIndicator, value))
				{
					Validate(UnpackingIndicatorInfo);
				}
			}
		}
		ZBool unpackingIndicator;

		public ZPropertyInfo UnpackingIndicatorInfo => GetZPropertyInfo(nameof(UnpackingIndicator));

		#endregion

		#region ReserveIndicator

		public ZBool ReserveIndicator
		{
			get => reserveIndicator;
			set
			{
				if (SetNonPersistentPropertyValue(ReserveIndicatorInfo, ref reserveIndicator, value))
				{
					Validate(ReserveIndicatorInfo);
				}
			}
		}
		ZBool reserveIndicator;

		public ZPropertyInfo ReserveIndicatorInfo => GetZPropertyInfo(nameof(ReserveIndicator));

		#endregion

		#region IsAnyLastKnownTWStatusEmpty 

		public ZBool IsAnyLastKnownTWStatusEmpty
		{
			get => isAnyLastKnownTWStatusEmpty;
			set
			{
				if (SetNonPersistentPropertyValue(IsAnyLastKnownTWStatusEmptyInfo, ref isAnyLastKnownTWStatusEmpty, value))
				{
					Validate(IsAnyLastKnownTWStatusEmptyInfo);
				}
			}
		}
		ZBool isAnyLastKnownTWStatusEmpty;

		public ZPropertyInfo IsAnyLastKnownTWStatusEmptyInfo => GetZPropertyInfo(nameof(IsAnyLastKnownTWStatusEmpty));

		#endregion
	}
}
