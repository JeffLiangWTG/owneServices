using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using ValidationConstants = Enterprise.Customs.ASYCUDA.Business.ValidationConstants;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected override void CheckAMA_VoyageCore()
		{
			var parent = Parent;
			if (parent.IsAir)
			{
				var propertyInfo = Parent.AMA_VoyageInfo;
				var value = parent.AMA_Voyage;
				if (!RefAirline.IsValidAirline2LetterCode(parent.Factory, value.Left(2)))
				{
					propertyInfo.AddMessageError(ValidationConstants.FlightNumberDoesNotStartWithAValidIATAAirCode);
				}
				else
				{
					CommonHelper.CheckVoyageFlightNoFormat(propertyInfo, value);
				}
			}
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_CustomsOfficeInfo);
		}

		protected override void CheckAMA_VehicleRegistration()
		{
			base.CheckAMA_VehicleRegistration();
			var parent = Parent;
			if (parent.IsSea)
			{
				var vehicleRegistration = parent.AMA_VehicleRegistration;
				if (!vehicleRegistration.IsEmpty && vehicleRegistration.Length != 6)
				{
					var targetInfo = parent.AMA_VehicleRegistrationInfo;
					targetInfo.AddMessageError(Res.GetString("95AE1720-DA34-418E-8426-53B031C89F89", "{0} is exactly 6 characters long.", targetInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckAMA_OA_DeconsolidateAddress()
		{
			base.CheckAMA_OA_DeconsolidateAddress();
			var parent = (AsycudaManifestHeader)Parent;
			if (parent.DeconsolidateAddress != null && parent.AMA_DeconsolidateVAT.IsEmpty)
			{
				parent.AMA_OA_DeconsolidateAddressInfo.AddMessageError(Res.GetString("01b94ef8-a534-4f37-a1f3-282d7e9c8b19", "The selected Deconsolidator does not have a TW-VAT number."));
			}
		}

		protected override void CheckAMA_CarrierCode()
		{
			base.CheckAMA_CarrierCode();
			var parent = Parent;
			if (!parent.AMA_CarrierCode.IsEmpty)
			{
				var factory = parent.Factory;
				if (parent.IsAir)
				{
					var airline = RefAirline.LoadFromAirlinePrefix(factory, parent.AMA_CarrierCode);
					if (airline == null)
					{
						parent.AMA_CarrierCodeInfo.AddMessageError(Res.GetString("222cec08-22c1-4530-a8fd-9eb8a26421de", "The entered Airline Code does not exist. To create one, visit Maintain > Reference Files > Airlines."));
					}
					else if (!airline.RM_MembershipFlagIATA)
					{
						parent.AMA_CarrierCodeInfo.AddMessageError(Res.GetString("0622c82c-a7fd-41da-bcf3-fde27b53a028", "The entered Airline Code exists but is not an IATA Airline Code. To mark the Airline Code as an IATA Airline Code, visit Maintain > Reference Files > Airline, select the airline record, and check the IATA Member checkbox."));
					}
				}
				else if (parent.IsSea)
				{
					var carrier = new ZZRefCarrierCombinedCollection(factory, Core.Constants.CountryCodes.Taiwan).FirstOrDefault(x => x.ZZ4_Code == parent.AMA_CarrierCode);
					if (carrier == null)
					{
						parent.AMA_CarrierCodeInfo.AddMessageError(Res.GetString("ed26e1ef-f655-4434-b88d-106b7fb45b9d", "The entered value is not a valid Taiwan Carrier or Shipping Agency Code. To create one, visit Maintain > Customs > Global Carriers."));
					}
				}
			}
		}

		protected override void CheckAMA_MasterBill()
		{
			base.CheckAMA_MasterBill();
			var parent = Parent;
			var voyage = parent.AMA_Voyage;
			if (parent.IsAir && voyage.Length >= 2)
			{
				var airline = RefAirline.LoadFromAirline2LetterCode(parent.Factory, voyage.Left(2));
				if (airline != null && airline.RM_EagleAddedAirlinePrefixOrAccountingCode != parent.AMA_MasterBill.Left(3))
				{
					parent.AMA_MasterBillInfo.AddWarning(Res.GetString("59640F6E-BF53-4CB6-8D89-7DADF56E24B6", "The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number."));
				}
			}
		}

		protected override bool ShouldValidateConveyanceCountry => false;
	}
}
