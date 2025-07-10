using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaManifestHeaderValidation : ManifestBase.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAMA_MasterBill();
			ValidateGateInOutMessageType();
			ValidateUnpackedDate();
			ValidateGateInOutDate();
			ValidateOutturnProvider();
			ValidateBookingNumber();
			ValidateFullyLoadedUnloadedDate();
			ValidateExcessIndicator();
			ValidateParentBill();
		}

		public void ValidateAMA_MasterBill() => ValidateCalculatedProperty(Parent.AMA_MasterBillInfo);

		protected void CheckAMA_MasterBill()
		{
			if (ParentHasType && (Parent.IsDCI || Parent.IsBGI || Parent.IsDOR || Parent.IsBBB || Parent.IsAOR || Parent.IsALD))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_MasterBillInfo);
			}
		}

		public void ValidateGateInOutMessageType() => ValidateCalculatedProperty(Parent.GateInOutMessageTypeInfo);

		protected void CheckGateInOutMessageType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.GateInOutMessageTypeInfo);
		}

		public void ValidateUnpackedDate() => ValidateCalculatedProperty(Parent.UnpackedDateInfo);

		protected void CheckUnpackedDate()
		{
			if (ParentHasType)
			{
				if (Parent.IsCOSTCO && Parent.AMA_ContainerMode != Core.Constants.ContainerModes.Containerised)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.UnpackedDateInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.UnpackedDateInfo);
				}
			}
		}

		public void ValidateGateInOutDate() => ValidateCalculatedProperty(Parent.GateInOutDateInfo);

		protected void CheckGateInOutDate()
		{
			if (ParentHasType)
			{
				if (Parent.IsCOSTCO)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.GateInOutDateInfo);
				}
				else if (Parent.IsGOVGIO)
				{
					if (Parent.Containers.Count == 0)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.GateInOutDateInfo);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.GateInOutDateInfo);
					}
				}
			}
		}

		public void ValidateExcessIndicator() => ValidateCalculatedProperty(Parent.ExcessIndicatorInfo);

		protected void CheckExcessIndicator()
		{
			if (ParentHasType)
			{
				if (Parent.IsVOR || Parent.IsEOR)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ExcessIndicatorInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ExcessIndicatorInfo);
				}
			}
		}

		public void ValidateOutturnProvider() => ValidateCalculatedProperty(Parent.OutturnProviderInfo);

		protected void CheckOutturnProvider()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.OutturnProviderInfo);
		}

		public void ValidateFullyLoadedUnloadedDate() => ValidateCalculatedProperty(Parent.FullyLoadedUnloadedDateInfo);

		protected void CheckFullyLoadedUnloadedDate()
		{
			if (ParentHasType && !Parent.IsVOR)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.FullyLoadedUnloadedDateInfo);
			}
		}

		public void ValidateParentBill() => ValidateCalculatedProperty(Parent.ParentBillInfo);

		protected void CheckParentBill()
		{
			if (ParentHasType && (Parent.IsGOVGIO || Parent.IsVOR || Parent.IsEOR || Parent.IsALD))
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.ParentBillInfo);
			}
		}

		public void ValidateBookingNumber() => ValidateCalculatedProperty(Parent.BookingNumberInfo);

		protected void CheckBookingNumber()
		{
			if (ParentHasType)
			{
				if (Parent.IsTGO || Parent.IsTGI)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BookingNumberInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.BookingNumberInfo);
				}
			}
		}

		public void ValidateRegistrationDate() => ValidateCalculatedProperty(Parent.RegistrationDateInfo);

		protected void CheckRegistrationDate()
		{
			var registrationEntryNumber = Parent.RegistrationEntryNumber;
			if (registrationEntryNumber != null)
			{
				registrationEntryNumber.Validation.ValidateCE_IssueDate();
				Parent.RegistrationDateInfo.AddAllNotificationsFrom(registrationEntryNumber.CE_IssueDateInfo);
			}
		}

		protected override void CheckAMA_ManifestType()
		{
			base.CheckAMA_ManifestType();
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_ManifestTypeInfo);
		}

		protected override void CheckAMA_VesselName()
		{
			base.CheckAMA_VesselName();
			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_VesselNameInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.AMA_VesselNameInfo);

				if (Parent.Vessel != null && Parent.Vessel.RV_RadioCallSign.IsEmpty)
				{
					Parent.AMA_VesselNameInfo.AddMessageError(Res.GetString("B1F33848-45EB-4F7B-9D55-98525E0A2C07", "Vessel must have a radio call sign."));
				}
			}
			else if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.AMA_VesselNameInfo);
			}
		}

		protected override void CheckAMA_TransportMode()
		{
			base.CheckAMA_TransportMode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_TransportModeInfo);
		}

		protected override void CheckAMA_ContainerMode()
		{
			base.CheckAMA_ContainerMode();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_ContainerModeInfo);

			if (Parent.IsGOVGIO && Parent.IsCNT && Parent.Containers.Count == 0)
			{
				Parent.AMA_ContainerModeInfo.AddMessageError(Res.GetString("B8FCE148-3422-4609-B650-9C5CBAAD0C8F", "Containerized cargo requires at least one container."));
			}
		}

		protected override void CheckAMA_AgentType()
		{
			base.CheckAMA_AgentType();

			if (ParentHasType)
			{
				if (Parent.IsCOSTCO && Parent.IsAOR)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_AgentTypeInfo);
				}
				else if (Parent.IsGOVGIO)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.AMA_AgentTypeInfo);
				}
			}
		}

		protected override void CheckAMA_Voyage()
		{
			base.CheckAMA_Voyage();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_VoyageInfo);
		}

		protected override void CheckAMA_OA_Carrier()
		{
			base.CheckAMA_OA_Carrier();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AMA_OA_CarrierInfo);

			var carrierCode = Parent.CarrierCode;
			if (carrierCode.IsEmpty)
			{
				Parent.AMA_OA_CarrierInfo.AddMessageError(Res.GetString("0697E6A3-04D2-4CED-B51B-7CD3E456DEBE", "No carrier code can be determined. There is no related ZA Customs reference file for the vessel, and there is no record selected in the Carrier field where a 'CCC' code is present. Please supply a carrier."));
			}
		}

		protected override void CheckAMA_OA_DeconsolidateAddress()
		{
			base.CheckAMA_OA_DeconsolidateAddress();

			if (ParentHasType)
			{
				if (Parent.IsGOVGIO)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_DeconsolidateAddressInfo);
				}
				else if (Parent.IsBBB || Parent.IsALD)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_DeconsolidateAddressInfo);

					var deconsolidateAddress = Parent.DeconsolidateAddress;
					if (deconsolidateAddress != null)
					{
						var depotCode = deconsolidateAddress.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DepotControlledPremisesID, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;
						if (depotCode.IsEmpty)
						{
							Parent.AMA_OA_DeconsolidateAddressInfo.AddMessageError(Res.GetString("f140a859-9100-40fa-8013-fa4a6251532b", "Organization must have a code of type 'CPD' loaded."));
						}
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.AMA_OA_DeconsolidateAddressInfo);
				}
			}
		}

		protected override void CheckAMA_OA_DischargeTerminalAddress()
		{
			base.CheckAMA_OA_DischargeTerminalAddress();

			if (ParentHasType)
			{
				if (Parent.IsBBB || Parent.IsALD || Parent.IsTGO || Parent.IsTGI || Parent.IsATI || Parent.IsBGI)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_DischargeTerminalAddressInfo);

					if (Parent.IsCOSTCO)
					{
						var depotCode = Parent.TerminalBerth;
						if (depotCode.IsEmpty)
						{
							Parent.AMA_OA_DischargeTerminalAddressInfo.AddMessageError(Res.GetString("e68036a2-c6b9-4996-8039-54dec149cbef", "Organization must have a code of type 'CPT' loaded."));
						}
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.AMA_OA_DischargeTerminalAddressInfo);
				}
			}
		}

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();

			if (ParentHasType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_NatureInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.AMA_NatureInfo);
			}
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		bool ParentHasType => Parent.IsCOSTCO || Parent.IsGOVGIO;
	}
}
