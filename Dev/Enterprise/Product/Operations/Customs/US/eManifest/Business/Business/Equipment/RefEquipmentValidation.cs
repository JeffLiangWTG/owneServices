using System;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	class RefEquipmentValidation : RelatedObjectValidation
	{
		RefEquipmentValidation(Equipment equipment)
			: base(equipment.BJ_RQ_EquipmentInfo)
		{
			this.equipment = equipment;
			refEquipment = equipment.RefEquipment;
		}

		internal static void Validate(Equipment equipment)
		{
			var validation = new RefEquipmentValidation(equipment);
			if (equipment.BJ_IsConveyance)
			{
				validation.ValidateConveyance();
			}
			else
			{
				validation.ValidateEquipment();
			}
		}

		#region Implementation

		void ValidateConveyance()
		{
			if (refEquipment != null && !notificationInfo.HasNotifications())
			{
				var errorBuilder = new ErrorStringBuilder(notificationInfo, equipment.HumanReadableName);
				var aceId = equipment.BJ_ACEID;
				MaxLengthValidation(aceId, 10, errorBuilder, ConveyanceACEIDCaption);
				MaxLengthValidation(equipment.GetReferenceNumber(ConveyanceReferences.Codes.CarrierId), 23, errorBuilder, ConveyanceIDCaption);
				ValidateEquipmentType(errorBuilder, ConveyanceTypeCaption, () => equipment.Lookups.ConveyanceTypes);

				if (aceId.IsEmpty)
				{
					MessageErrorIfNotEntered(refEquipment.RQ_VINInfo, errorBuilder, VINCaption);
					ValidateLicensePlateDetails(errorBuilder);
				}
				errorBuilder.FillErrorMessages();
			}
		}

		void ValidateEquipment()
		{
			if (refEquipment != null && !notificationInfo.HasNotifications())
			{
				var errorBuilder = new ErrorStringBuilder(notificationInfo, equipment.HumanReadableName);
				var aceId = equipment.BJ_ACEID;
				MaxLengthValidation(aceId, 10, errorBuilder, EquipmentACEIDCaption);
				var equipmentType = ValidateEquipmentType(errorBuilder, EquipmentTypeCaption, () => equipment.Lookups.EquipmentTypes);

				if (aceId.IsEmpty)
				{
					if (equipment.Lookups.EquipmentTypes.ContainsCode(equipmentType))
					{
						if (EquipmentTypes.IsLicensePlateRequired(refEquipment.Factory, equipmentType))
						{
							ValidateLicensePlateDetails(errorBuilder);
						}
						else
						{
							MessageErrorIfNotEntered(refEquipment.RQ_RegistrationInfo, errorBuilder, EquipmentNumberCaption);
						}
					}
				}
				errorBuilder.FillErrorMessages();
			}
		}

		ZString ValidateEquipmentType(ErrorStringBuilder errorBuilder, string caption, Func<ICodeDescriptionPairList> getList)
		{
			var equipmentType = ZString.Empty;
			MessageErrorIfNotEntered(refEquipment.RQ_RC_RoadContainerTypeInfo, errorBuilder, caption);
			var refContainer = refEquipment.RoadContainerType;
			if (refContainer != null)
			{
				if (refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates).IsEmpty)
				{
					errorBuilder.AddMessageError(GetMissingEquipmentType(caption, refContainer.RC_Code));
				}
				else
				{
					if (!getList().ContainsCode(refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates)))
					{
						errorBuilder.AddMessageError(GetInvalidEquipmentType(caption, refContainer.RC_Code).ToString());
					}
					equipmentType = refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				}
			}
			return equipmentType;
		}

		void ValidateLicensePlateDetails(ErrorStringBuilder errorBuilder)
		{
			MessageErrorIfNotEntered(refEquipment.RQ_RegistrationInfo, errorBuilder, LicensePlateNumberCaption);
			MessageErrorIfNotEntered(refEquipment.RQ_RegStateInfo, errorBuilder, StateProvinceOfRegistrationCaption);

			var regCountry = refEquipment.RQ_RN_NKRegistrationCountry;
			MaxLengthValidation(regCountry, 2, errorBuilder, CountryOfRegistrationCaption, true);
		}

		#endregion

		#region Strings

		internal static string ConveyanceACEIDCaption
		{
			get { return Res.GetString("fa748589-5f26-4a6f-8461-27f20090b900", "Conveyance ACE ID"); }
		}

		internal static string ConveyanceIDCaption
		{
			get { return Res.GetString("74b32688-3b7a-47f9-9b0e-9fd1aa2fb63d", "Conveyance ID"); }
		}

		internal static string ConveyanceTypeCaption
		{
			get { return Res.GetString("d19aa2ec-93f9-4b18-a2a3-586bfba5bd12", "Conveyance Type"); }
		}

		internal static string VINCaption
		{
			get { return Res.GetString("139b61c4-2dbe-4d92-8fe5-03e296028ab9", "Vehicle Identification Number (VIN)"); }
		}

		internal static string LicensePlateNumberCaption
		{
			get { return Res.GetString("c4c73482-c0f0-41a9-85ce-05c9687152c0", "License Plate Number"); }
		}

		internal static string StateProvinceOfRegistrationCaption
		{
			get { return Res.GetString("1e77c804-fcf5-48c2-9857-29ea140e6bc7", "State/Province of Registration"); }
		}

		internal static string CountryOfRegistrationCaption
		{
			get { return Res.GetString("79a361e6-56e3-4aab-9e6b-c31abb55f6e5", "Country of Registration"); }
		}

		internal static string EquipmentACEIDCaption
		{
			get { return Res.GetString("1a36b1aa-7dfc-4c02-9e17-fe4c82c654fc", "Equipment ACE ID"); }
		}

		internal static string EquipmentTypeCaption
		{
			get { return Res.GetString("27fb78aa-8348-4e0b-8b7b-179b8826324b", "Equipment Type"); }
		}

		internal static string EquipmentNumberCaption
		{
			get { return Res.GetString("073519d4-257d-4385-8bde-a09b0e1a62b7", "Equipment Number"); }
		}

		internal static IMultilingualString GetInvalidEquipmentType(string caption, string containerCode)
		{
			return ResString.GetMultilingualString("caa5e3ca-431e-4cf8-9f0f-3047d318f59f", @"The equipment you have selected has an invalid e-Manifest {0}.{1}", caption, GetSpecifyUSCodeMessage(containerCode));
		}

		internal static string GetMissingEquipmentType(string caption, string containerCode)
		{
			return Res.GetString("dd726dc9-67a9-4390-be71-df15f2b3a113", @"The equipment you have selected is missing e-Manifest {0}.{1}", caption, GetSpecifyUSCodeMessage(containerCode));
		}

		static string GetSpecifyUSCodeMessage(string containerCode)
		{
			return "\r\n" + Res.GetString("5636b9f6-d8c9-470c-9635-dff923bdc2f9", "Please go to Reference Files -> Containers and specify proper US Container Code against container with code '{0}'.", containerCode);
		}

		#endregion

		readonly RefEquipment refEquipment;
		readonly Equipment equipment;
	}
}
