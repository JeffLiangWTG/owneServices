using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.SG;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	class CMDDataValueWrapperValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePermitOrExemptionType()
		{
			EntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertMandatoryValidationError(EntryWrapper.PermitOrExemptionTypeInfo, true);
			EntryWrapper.PermitOrExemptionType = "123";
			EntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertMandatoryValidationError(EntryWrapper.PermitOrExemptionTypeInfo, false);
			AssertListValidationInvalidCodeError(EntryWrapper.PermitOrExemptionTypeInfo, true);
			EntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Certificate;
			EntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertNoErrors(EntryWrapper.PermitOrExemptionTypeInfo);
		}

		public void TestValidatePermitOrExemptionType_DuplicatePermit()
		{
			EntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Certificate;
			EntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertNoErrors(EntryWrapper.PermitOrExemptionTypeInfo);
			CMDDataValueWrapper newEntryWrapper = ShipmentWrapper.CMDDataValuesForBinding.AddNew();
			newEntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
			newEntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertNoErrors(newEntryWrapper.PermitOrExemptionTypeInfo);
			newEntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Certificate;
			newEntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertHasError(newEntryWrapper.PermitOrExemptionTypeInfo, "A Certificate has already been entered, cannot add another one.");
			EntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
			newEntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
			newEntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertHasWarning(newEntryWrapper.PermitOrExemptionTypeInfo, "A Permit has already been entered.");
			newEntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Certificate;
			newEntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertNoErrors(newEntryWrapper.PermitOrExemptionTypeInfo);
			AssertNoWarnings(newEntryWrapper.PermitOrExemptionTypeInfo);
		}

		public void TestValidatePermitValue_DuplicatePermit()
		{
			EntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
			EntryWrapper.PermitNumberOrExemptionRemarks = "PERMIT1";
			EntryWrapper.Validation.ValidatePermitNumberOrExemptionRemarks();
			AssertNoMessageErrors(EntryWrapper.PermitNumberOrExemptionRemarksInfo);
			var newEntryWrapper = ShipmentWrapper.CMDDataValuesForBinding.AddNew();
			newEntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
			newEntryWrapper.PermitNumberOrExemptionRemarks = "PERMIT2";
			newEntryWrapper.Validation.ValidatePermitNumberOrExemptionRemarks();
			AssertNoMessageErrors(newEntryWrapper.PermitNumberOrExemptionRemarksInfo);
			var newEntryWrapper2 = ShipmentWrapper.CMDDataValuesForBinding.AddNew();
			newEntryWrapper2.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
			newEntryWrapper2.PermitNumberOrExemptionRemarks = "PERMIT1";
			newEntryWrapper2.Validation.ValidatePermitNumberOrExemptionRemarks();
			AssertHasMessageError(newEntryWrapper2.PermitNumberOrExemptionRemarksInfo, "This permit number value has already been entered.");
			newEntryWrapper2.PermitNumberOrExemptionRemarks = "PERMIT3";
			newEntryWrapper2.Validation.ValidatePermitNumberOrExemptionRemarks();
			AssertNoMessageErrors(newEntryWrapper2.PermitNumberOrExemptionRemarksInfo);
		}

		public void TestValidatePermitOrExemptionType_OnlyOneTDBExemptionIsAllowed()
		{
			EntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.SGExemption.Codes.DP;
			EntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertNoErrors(EntryWrapper.PermitOrExemptionTypeInfo);
			CMDDataValueWrapper newEntryWrapper = ShipmentWrapper.CMDDataValuesForBinding.AddNew();
			newEntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.SGExemption.Codes.HT;
			newEntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertHasError(newEntryWrapper.PermitOrExemptionTypeInfo, "An Exemption already exists, cannot add additional exemptions.");
			newEntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
			newEntryWrapper.Validation.ValidatePermitOrExemptionType();
			AssertNoErrors(newEntryWrapper.PermitOrExemptionTypeInfo);
		}

		public void TestValidatePermitNumberOrExemptionRemarks()
		{
			EntryWrapper.Validation.ValidatePermitNumberOrExemptionRemarks();
			AssertMandatoryValidationError(EntryWrapper.PermitNumberOrExemptionRemarksInfo, true);
			EntryWrapper.PermitNumberOrExemptionRemarks = "BLAH";
			AssertMandatoryValidationError(EntryWrapper.PermitNumberOrExemptionRemarksInfo, false);
		}

		public void TestValidateAll()
		{
			EntryWrapper.Validation.ValidateAll();
			AssertMandatoryValidationError(EntryWrapper.PermitOrExemptionTypeInfo, true);
			AssertMandatoryValidationError(EntryWrapper.PermitNumberOrExemptionRemarksInfo, true);
		}

		#region Implementation
		CMDDataValueWrapper EntryWrapper
		{
			get
			{
				if (fEntryWrapper == null)
				{
					fEntryWrapper = ShipmentWrapper.CMDDataValuesForBinding.AddNew();
				}

				return fEntryWrapper;
			}
		}

		CMDShipmentWrapper ShipmentWrapper
		{
			get
			{
				if (fShipmentWrapper == null)
				{
					ForwardingShipment shipment = Factory.New<ForwardingShipment>();
					fShipmentWrapper = new CMDShipmentWrapper(shipment);
				}

				return fShipmentWrapper;
			}
		}

		CMDDataValueWrapper fEntryWrapper;
		CMDShipmentWrapper fShipmentWrapper;
		#endregion
	}
}
