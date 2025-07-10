using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentInstructionDocAddressValidation : JobDocAddressValidation
	{
		public DtbConsignmentInstructionDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			AdditionalValidationForAddressProperties(Parent.E2_OA_AddressInfo);
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.LocalCartageExporter:
				case DocAddressTypes.Codes.LocalCartageImporter:
					ValidateConsignorVsConsignee();
					break;
			}

			AddErrorIfAddressesAreTheSame(Parent.OrganisationPKInfo);

			if (Parent.E2_OA_AddressInfo.HasChanges)
			{
				AddErrorIfRunSheetExists(Parent.OrganisationPKInfo);
			}
		}

		void ValidateConsignorVsConsignee()
		{
			if (!HasAddress(DocAddressType.LocalCartageExporter) &&
				!HasAddress(DocAddressType.LocalCartageImporter))
			{
				var message = Res.GetString("ACA462AE-2322-4C22-9086-9018EC557023", "This Organization has no address entered");

				if (Instruction.IsPickUp)
				{
					Parent.OrganisationPKInfo.AddError(message);
				}
				else
				{
					Parent.OrganisationPKInfo.AddWarning(message);
				}
			}
		}

		bool HasAddress(DocAddressType addressType)
		{
			IDocAddresses parent = Parent.Parent;
			JobDocAddress address = parent.DocAddresses.FindByDocAddressType(addressType);
			return address != null && address.IsValidAddress;
		}

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			AdditionalValidationForAddressProperties(Parent.E2_Address1Info);
		}

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();
			AdditionalValidationForAddressProperties(Parent.E2_Address2Info);
		}

		protected override void CheckE2_City()
		{
			base.CheckE2_City();
			AdditionalValidationForAddressProperties(Parent.E2_CityInfo);
		}

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			AdditionalValidationForAddressProperties(Parent.E2_CompanyNameInfo);
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();
			AdditionalValidationForAddressProperties(Parent.E2_PostcodeInfo);
		}

		protected override void CheckE2_State()
		{
			base.CheckE2_State();
			AdditionalValidationForAddressProperties(Parent.E2_StateInfo);
		}

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			AdditionalValidationForAddressProperties(Parent.E2_RN_NKCountryCodeInfo);
		}

		#region Implementation

		void AdditionalValidationForAddressProperties(ZPropertyInfo info)
		{
			AddErrorIfAddressesAreTheSame(info);

			if (Parent.E2_AddressOverride && info.HasChanges)
			{
				AddErrorIfRunSheetExists(info);
			}
		}

		void AddErrorIfRunSheetExists(ZPropertyInfo info)
		{
			var runSheet = RunSheet;
			if (runSheet != null)
			{
				var msg = Res.GetString("8734856e-c10f-4757-8ad0-a236d4c00a75", "Address cannot be changed because it is already allocated to Run Sheet {0}.", runSheet.KG_RunSheetNumber);
				info.AddError(msg);
			}
		}

		void AddErrorIfAddressesAreTheSame(ZPropertyInfo info)
		{
			if ((Instruction.IsDelivery || Instruction.IsPickUp) && !IsAddressEmpty)
			{
				var booking = Instruction.Booking;
				if (booking != null)
				{
					var isPickup = Instruction.IsPickUp;
					var otherInstruction = isPickup ? booking.DeliveryInstruction : booking.PickupInstruction;
					if (otherInstruction != null && Instruction.Address.GetAddressUniqueKey() == otherInstruction.Address.GetAddressUniqueKey())
					{
						var pickupText = Res.GetString("20d6e3d6-050d-4205-b8fa-7b114805a0a0", "Pickup");
						var deliveryText = Res.GetString("041e2bf2-36fd-42f2-9ebc-71ab72c72836", "Delivery");
						info.AddError(Res.GetString("97bc0ccd-202f-4b87-854a-58be3b214648", "{0} Address cannot be the same as {1} Address.",
							isPickup ? pickupText : deliveryText,
							isPickup ? deliveryText : pickupText));
					}
				}
			}
		}

		bool IsAddressEmpty
		{
			get { return !Instruction.Address.E2_AddressOverride && Instruction.Address.E2_OA_Address.IsEmpty; }
		}

		DtbConsignmentInstruction Instruction
		{
			get { return (DtbConsignmentInstruction)Parent.Parent; }
		}

		DtbConsignmentRunSheet RunSheet
		{
			get
			{
				var instruction = Instruction;
				var confirmation = instruction.IsPickUp ? instruction.PickupConfirmation : instruction.DeliveryConfirmation;
				var rsi = confirmation.KK_K1_RunSheetInstruction.IsValid ? confirmation.RunSheetInstruction : null;

				return (rsi != null) ? rsi.RunSheet : null;
			}
		}

		#endregion
	}
}
