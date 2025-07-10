using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonPickupDeliveryConfirmValidation : JobPickupDeliveryConfirmValidation
	{
		public CommonPickupDeliveryConfirmValidation(CommonPickupDeliveryConfirm parent)
			: base(parent)
		{
		}

		CommonPickupDeliveryConfirm Confirm
		{
			get { return (CommonPickupDeliveryConfirm)Parent; }
		}

		public static string TruckDetailsError
		{
			get { return Res.GetString("bc8626a9-1f57-4225-a168-4a15ec5ede3d", "Enter the Vehicle Registration or select a Company Vehicle."); }
		}
		public static string TransportCompanyDetailsError
		{
			get { return Res.GetString("6aea1318-7073-442b-aa84-587630f75191", "Enter a Transport Company or Transport Company Name."); }
		}
		public static string StaffDetailsError
		{
			get { return Res.GetString("2671dc85-f6af-4431-b17b-934669e08977", "Enter the Drivers Name and Drivers License or select a Staff Driver."); }
		}

		#region CheckEU_DriversName

		protected override void CheckEU_DriversName()
		{
			base.CheckEU_DriversName();

			if (IsMandatoryForGatePass)
			{
				if (Confirm.EU_DriversName.IsEmpty)
				{
					Confirm.EU_DriversNameInfo.AddError(StaffDetailsError);
				}
			}
		}

		#endregion

		#region CheckEU_DriversLicence

		protected override void CheckEU_DriversLicence()
		{
			base.CheckEU_DriversLicence();

			if (IsMandatoryForGatePass)
			{
				RefUNLOCO location = Confirm.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, GlbBranch.CurrentBranch.GB_RL_NKHomePort);

				if (location != null && location.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore)
				{
					if (Confirm.EU_DriversLicence.IsEmpty)
					{
						Confirm.EU_DriversLicenceInfo.AddError(StaffDetailsError);
					}
				}
			}

			if (!Confirm.EU_DriversLicenceInfo.HasErrors() && IsPartEnteredAndMandatoryForCFSConfirmations)
			{
				MandatoryValidation.CheckEntered(Confirm.EU_DriversLicenceInfo);
			}
		}

		#endregion

		#region CheckEU_VehicleRegistration

		protected override void CheckEU_VehicleRegistration()
		{
			base.CheckEU_VehicleRegistration();

			if (IsMandatoryForGatePass && Confirm.EU_VehicleRegistration.IsEmpty)
			{
				Confirm.EU_VehicleRegistrationInfo.AddError(TruckDetailsError);
			}

			if (!Confirm.EU_VehicleRegistrationInfo.HasErrors() && IsPartEnteredAndMandatoryForCFSConfirmations)
			{
				MandatoryValidation.CheckEntered(Confirm.EU_VehicleRegistrationInfo);
			}
		}

		#endregion

		#region CheckEU_TransportCoName

		protected override void CheckEU_TransportCoName()
		{
			base.CheckEU_TransportCoName();
			if (IsMandatoryForGatePass && Confirm.EU_TransportCoName.IsEmpty && !Confirm.EU_OA_TransportProvider.IsValid)
			{
				Confirm.EU_TransportCoNameInfo.AddError(TransportCompanyDetailsError);
			}
		}

		#endregion

		#region CheckEU_OA_TransportProvider

		protected override void CheckEU_OA_TransportProvider()
		{
			base.CheckEU_OA_TransportProvider();
			if (IsMandatoryForGatePass && Confirm.EU_TransportCoName.IsEmpty && !Confirm.EU_OA_TransportProvider.IsValid)
			{
				Confirm.EU_OA_TransportProviderInfo.AddError(TransportCompanyDetailsError);
			}

			if (!Confirm.EU_OA_TransportProviderInfo.HasErrors() && IsPartEnteredAndMandatoryForCFSConfirmations)
			{
				MandatoryValidation.CheckEntered(Confirm.EU_OA_TransportProviderInfo);
			}
		}

		#endregion

		#region CheckEU_PickupDeliveryTime

		protected override void CheckEU_PickupDeliveryTime()
		{
			base.CheckEU_PickupDeliveryTime();

			if (IsMandatoryForGatePass)
			{
				MandatoryValidation.CheckEntered(Confirm.EU_PickupDeliveryTimeInfo);
			}

			if (!Confirm.EU_PickupDeliveryTimeInfo.HasErrors() && IsPartEnteredAndMandatoryForCFSConfirmations)
			{
				MandatoryValidation.CheckEntered(Confirm.EU_PickupDeliveryTimeInfo);
			}

			if (!Confirm.EU_PickupDeliveryTimeInfo.HasErrors() && !Confirm.EU_PickupDeliveryTime.IsEmpty && Confirm.Container != null)
			{
				if (Confirm.EU_PickupDeliveryType == Core.Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture &&
					Confirm.EU_PickupDeliveryTime < Confirm.Container.FindConfirmOfType(Core.Constants.PickupDeliveryConfirmTypes.OriginCFSArrival)?.EU_PickupDeliveryTime ||
					Confirm.EU_PickupDeliveryType == Core.Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture &&
					Confirm.EU_PickupDeliveryTime < Confirm.Container.FindConfirmOfType(Core.Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival)?.EU_PickupDeliveryTime)
				{
					Confirm.EU_PickupDeliveryTimeInfo.AddError(Res.GetString("ed007215-4166-4efb-ae4e-8240631fae20", "Container Departure Time cannot be set to a time before Container Arrival Time."));
				}
			}
		}

		#endregion

		#region EU_DistanceUnit

		protected override void CheckEU_DistanceUnit()
		{
			base.CheckEU_DistanceUnit();
			ListValidation.ErrorIfInvalidCode(Parent.EU_DistanceUnitInfo, Confirm.BindToLists.DistanceUnits);

			if (Parent.EU_Distance > 0m)
			{
				MandatoryValidation.CheckEntered(Parent.EU_DistanceUnitInfo);
			}
		}

		#endregion

		#region Implementation

		ZBool IsMandatoryForGatePass
		{
			get { return Confirm.IsGatePass && !Confirm.ReadOnly && !Confirm.IsInDatabase; }
		}

		ZBool IsPartEnteredAndMandatoryForCFSConfirmations
		{
			get
			{
				return Confirm.IsContainerised
					&& Confirm.IsCFSArrivalOrDeparture
					&& CFSDataRegistry.Instance.RequireTransportDetails.Value
					&& (!Confirm.EU_VehicleRegistration.IsEmpty
						|| !Confirm.EU_DriversLicence.IsEmpty
						|| !Confirm.EU_PickupDeliveryTime.IsEmpty
						|| Confirm.EU_OA_TransportProvider.IsValid);
			}
		}

		#endregion
	}
}
