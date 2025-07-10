using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondHeaderValidation : Customs.Business.CusInBondHeaderValidation
	{
		public CusInBondHeaderValidation(CusInBondHeader parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateAtLeastOneBillExists();
			}
		}

		void ValidateAtLeastOneBillExists()
		{
			if (IsInventoryRecordValidationMode && Parent.Bills.Count == 0)
			{
				Parent.AddRowMessageError(ValidationConstants.Header.AtLeastOneBillExists);
			}
		}

		protected override void CheckBH_ImportTransportMode()
		{
			base.CheckBH_ImportTransportMode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_ImportTransportModeInfo);
		}

		protected override void CheckBH_CarrierSCAC()
		{
			base.CheckBH_CarrierSCAC();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_CarrierSCACInfo);

			if (Parent.IsNVOCCHeader && Parent.SCACInCarrier != Parent.BH_CarrierSCAC)
			{
				Parent.BH_CarrierSCACInfo.AddMessageError(ValidationConstants.Header.SCACOfVesselOperatorShouldBeSubmitted);
			}
		}

		protected override void CheckBH_ImportConveyanceCountry()
		{
			base.CheckBH_ImportConveyanceCountry();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_ImportConveyanceCountryInfo);
		}

		protected override void CheckBH_LloydsNumber()
		{
			base.CheckBH_LloydsNumber();
			ValidateBH_ImportConveyanceName();
		}

		protected override void CheckBH_ImportConveyanceName()
		{
			base.CheckBH_ImportConveyanceName();
			if (Parent.BH_LloydsNumber.IsEmpty && Parent.BH_ImportConveyanceName.IsEmpty)
			{
				Parent.BH_ImportConveyanceNameInfo.AddMessageError(ValidationConstants.Header.ImportingConveyanceRequiresWhenLloydsNumberIsEmpty);
			}
			else if (!Parent.BH_LloydsNumber.IsEmpty && !Parent.BH_ImportConveyanceName.IsEmpty)
			{
				Parent.BH_ImportConveyanceNameInfo.AddWarning(ValidationConstants.Header.VesselNameNotBeSentOnlyLloydsNumberBeSent);
			}
			else if (Parent.BH_LloydsNumber.IsEmpty && Parent.BH_ImportConveyanceName.Length > 23)
			{
				Parent.BH_ImportConveyanceNameInfo.AddWarning(ValidationConstants.Header.VesselNameOnlySendFirst23Characters);
			}
		}

		protected override void CheckBH_VoyageNumber()
		{
			base.CheckBH_VoyageNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_VoyageNumberInfo);

			if (IsInventoryRecordValidationMode && !Parent.BH_VoyageNumber.IsEmpty && Parent.BH_VoyageNumber.Length > Parent.VoyageNumberMaxLength)
			{
				Parent.BH_VoyageNumberInfo.AddWarning(ValidationConstants.Header.VoyageNumberLengthExceeded);
			}
		}

		protected override void CheckBH_RL_NKPortUnlading()
		{
			base.CheckBH_RL_NKPortUnlading();
			ListValidation.WarnIfInvalidCode(Parent.BH_RL_NKPortUnladingInfo);
		}

		protected override void CheckBH_PortUnladingDCode()
		{
			base.CheckBH_PortUnladingDCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_PortUnladingDCodeInfo);
		}

		protected override void CheckBH_ETA()
		{
			base.CheckBH_ETA();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_ETAInfo);
		}

		protected override void CheckBH_FIRMS()
		{
			base.CheckBH_FIRMS();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BH_FIRMSInfo);
			}
		}

		protected CusInBondHeaderLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected new CusInBondHeader Parent
		{
			get { return (CusInBondHeader)base.Parent; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		bool IsInventoryRecordValidationMode
		{
			get { return Parent.IsInventoryRecordValidationMode; }
		}
	}
}
