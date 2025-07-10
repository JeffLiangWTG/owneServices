using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USPSTAddInfoValidation : AutoUSPSTAddInfoValidation
	{
		public USPSTAddInfoValidation(AutoUSPSTAddInfo parent)
			: base(parent)
		{
		}

		new USPSTAddInfo Parent
		{
			get { return (USPSTAddInfo)base.Parent; }
		}

		bool IsPGAValidationOn
		{
			get
			{
				var pesticide = Parent.Parent;
				var invoiceLine = pesticide != null ? pesticide.InvoiceLine : null;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		protected override void CheckUS_UnregReasonRemarks()
		{
			base.CheckUS_UnregReasonRemarks();
			if (IsPGAValidationOn)
			{
				if (Parent.US_ProductType == PSTProductTypeList.Codes.PS3)
				{
					if (Parent.US_UnregReasonRemarks.IsEmpty && Parent.US_UnregReasonCode.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UnregReasonRemarksInfo, "General Remarks");
					}
					else if (!Parent.US_UnregReasonRemarks.IsEmpty && !Parent.US_UnregReasonCode.IsEmpty)
					{
						Parent.US_UnregReasonRemarksInfo.AddMessageError(NowAllowBothUnregReasonCodeAndUnregReasonRemarksEntered);
					}
				}
			}
			ValidateUS_UnregReasonCode();
		}
		internal const string NowAllowBothUnregReasonCodeAndUnregReasonRemarksEntered = "You can enter either General Remarks or an Unregistered Reason Code, not both.";

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_IntendedUseCodeInfo, Parent.Lookups.IntendedUseCodeList);
		}

		protected override void CheckUS_ProductType()
		{
			base.CheckUS_ProductType();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductTypeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductTypeInfo, Parent.Lookups.ProductTypeList);

			var pesticide = Parent.Parent;
			if (Parent.US_ProductType == PSTProductTypeList.Codes.PS2 && pesticide != null && pesticide.PesticideLines.Count > 0)
			{
				Parent.US_ProductTypeInfo.AddMessageError(NoLinesAllowedForPS2);
			}
			else if ((Parent.US_ProductType == PSTProductTypeList.Codes.PS1 || Parent.US_ProductType == PSTProductTypeList.Codes.PS3) &&
				pesticide != null && pesticide.PesticideLines.Count == 0)
			{
				Parent.US_ProductTypeInfo.AddMessageError(LinesMissing);
			}
			ValidateUS_UnregReasonCode();
			ValidateUS_UnregReasonRemarks();
		}
		internal const string NoLinesAllowedForPS2 = "No detail lines are allowed for Pesticide Details when Product Type is PS2.";
		internal const string LinesMissing = "At least one Pesticide Details Line is required if product type is 'PS1 - Registered Pesticides' or 'PS3 - Pesticides Other'.";

		protected override void CheckUS_UnregReasonCode()
		{
			base.CheckUS_UnregReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UnregReasonCodeInfo, Parent.Lookups.ReasonCodeList);
			if (IsPGAValidationOn && !Parent.US_ProductType.IsEmpty)
			{
				if (Parent.US_ProductType == PSTProductTypeList.Codes.PS3)
				{
					if (Parent.US_UnregReasonCode.IsEmpty && Parent.US_UnregReasonRemarks.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UnregReasonCodeInfo, "Unregistered Reason Code");
					}
					else if (!Parent.US_UnregReasonCode.IsEmpty && !Parent.US_UnregReasonRemarks.IsEmpty)
					{
						Parent.US_UnregReasonCodeInfo.AddMessageError(NowAllowBothUnregReasonCodeAndUnregReasonRemarksEntered);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.US_UnregReasonCodeInfo, "Unregistered Reason Code");
				}
			}
			ValidateUS_UnregReasonRemarks();
		}

		protected override void CheckUS_BrandName()
		{
			base.CheckUS_BrandName();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_BrandNameInfo);
			}
		}

		protected override void CheckUS_LPCONumber()
		{
			base.CheckUS_LPCONumber();
			if (Parent.US_ProductType == PSTProductTypeList.Codes.PS1 && IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_LPCONumberInfo);
			}
		}

		protected override void CheckUS_ProducerEstNo()
		{
			base.CheckUS_ProducerEstNo();
			if (Parent.US_ProductType == PSTProductTypeList.Codes.PS3 &&
				(Parent.US_UnregReasonCode == PSTRemarksCodeList.Codes.TR1 || Parent.US_UnregReasonCode == PSTRemarksCodeList.Codes.TR2)
				&& IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProducerEstNoInfo, "Producer Est. No");
			}
			if (!Parent.US_ProducerEstNo.IsEmpty && (Parent.US_ProducerEstNo.Length != 12 || !Regex.IsMatch(Parent.US_ProducerEstNo, @"^[0-9]{6}[A-Z]{3}[0-9]{3}$", RegexOptions.IgnoreCase)))
			{
				Parent.US_ProducerEstNoInfo.AddMessageError(ProducerEstNoFormat);
			}
		}
		internal const string ProducerEstNoFormat = "Producer Establishment Number(Domestic) should be 12 characters in length and format 'NNNNNNAAANNN'.";

		protected override void CheckUS_ProducerEstNoForeign()
		{
			base.CheckUS_ProducerEstNoForeign();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProducerEstNoForeignInfo);
			}
			if (!Parent.US_ProducerEstNoForeign.IsEmpty && (Parent.US_ProducerEstNoForeign.Length != 12 || !Regex.IsMatch(Parent.US_ProducerEstNoForeign, @"^[0-9]{6}[A-Z]{3}[0-9]{3}$", RegexOptions.IgnoreCase)))
			{
				Parent.US_ProducerEstNoForeignInfo.AddMessageError(ProducerEstNoForeignFormat);
			}
		}
		internal const string ProducerEstNoForeignFormat = "Producer Establishment Number(Foreign) should be 12 characters in length and format 'NNNNNNAAANNN'.";

		protected override void CheckUS_OA_ExaminationLocation()
		{
			base.CheckUS_OA_ExaminationLocation();
			var pesticide = Parent.Parent;
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_ExaminationLocationInfo);

				if (pesticide != null)
				{
					var wrapper = pesticide.ExaminationLocationWrapper;
					if (wrapper != null)
					{
						OrganisationValidation.ValidatePGAContact(Parent.US_OA_ExaminationLocationInfo, wrapper);

						var companyAddress = ((IPGAContactDetails)wrapper).CompanyAddress;
						if (companyAddress != null && companyAddress.Country != Core.Constants.CountryCodes.UnitedStates)
						{
							Parent.US_OA_ExaminationLocationInfo.AddMessageError(ExaminationLocationShouldBeUSAddress);
						}
						OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ExaminationLocationInfo, pesticide.ExaminationLocationAddress);
					}

					OrganisationValidation.ValidateCharactorsForAddressDescription(pesticide.US_OA_ExaminationLocationInfo, pesticide.ExaminationLocationAddress);
				}
			}
		}

		internal const string ExaminationLocationShouldBeUSAddress = "Examination location should be an US address.";

		void ApplyPSTQtyValidation(ZDecimal value, ZString unit, ZPropertyInfo info)
		{
			if (value.IsEmpty && !unit.IsEmpty)
			{
				info.AddMessageError(ValueIsRequired);
			}
			if (value < 0m)
			{
				info.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		internal const string ValueIsRequired = "A unit value is required if a unit quantity is entered.";

		protected override void CheckUS_NoOfUnit1()
		{
			base.CheckUS_NoOfUnit1();

			if (Parent.US_NoOfUnit1.IsEmpty)
			{
				Parent.US_NoOfUnit1Info.AddMessageError("The largest package quantity is required.");
			}
			ApplyPSTQtyValidation(Parent.US_NoOfUnit1, Parent.US_UQ1, Parent.US_NoOfUnit1Info);
		}

		protected override void CheckUS_NoOfUnit2()
		{
			base.CheckUS_NoOfUnit2();
			if (Parent.US_NoOfUnit2.IsEmpty)
			{
				Parent.US_NoOfUnit2Info.AddMessageError("The second largest package quantity is required.");
			}
			ApplyPSTQtyValidation(Parent.US_NoOfUnit2, Parent.US_UQ2, Parent.US_NoOfUnit2Info);
		}

		protected override void CheckUS_NoOfUnit3()
		{
			base.CheckUS_NoOfUnit3();
			ApplyPSTQtyValidation(Parent.US_NoOfUnit3, Parent.US_UQ3, Parent.US_NoOfUnit3Info);
		}

		protected override void CheckUS_NoOfUnit4()
		{
			base.CheckUS_NoOfUnit4();
			ApplyPSTQtyValidation(Parent.US_NoOfUnit4, Parent.US_UQ4, Parent.US_NoOfUnit4Info);
		}

		protected override void CheckUS_NoOfUnit5()
		{
			base.CheckUS_NoOfUnit5();
			ApplyPSTQtyValidation(Parent.US_NoOfUnit5, Parent.US_UQ5, Parent.US_NoOfUnit5Info);
		}

		protected override void CheckUS_NoOfUnit6()
		{
			base.CheckUS_NoOfUnit6();
			ApplyPSTQtyValidation(Parent.US_NoOfUnit6, Parent.US_UQ6, Parent.US_NoOfUnit6Info);
		}

		protected override void CheckUS_UQ1()
		{
			base.CheckUS_UQ1();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ1Info, Parent.Lookups.OuterPackingTypes);
			if (Parent.US_NoOfUnit1.IsEmpty && !Parent.US_UQ1.IsEmpty)
			{
				Parent.US_UQ1Info.AddMessageError(ValueIsRequired);
			}

			if (!Parent.US_NoOfUnit1.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UQ1Info);
			}
		}

		protected override void CheckUS_UQ2()
		{
			base.CheckUS_UQ2();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ2Info, Parent.Lookups.InnerPackingTypes);
			if (Parent.US_NoOfUnit2.IsEmpty && !Parent.US_UQ2.IsEmpty)
			{
				Parent.US_UQ2Info.AddMessageError(ValueIsRequired);
			}

			if (!Parent.US_NoOfUnit2.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UQ2Info);
			}
		}

		protected override void CheckUS_UQ3()
		{
			base.CheckUS_UQ3();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ3Info, Parent.Lookups.InnerPackingTypes);
			if (Parent.US_NoOfUnit3.IsEmpty && !Parent.US_UQ3.IsEmpty)
			{
				Parent.US_UQ3Info.AddMessageError(ValueIsRequired);
			}

			if (!Parent.US_NoOfUnit3.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UQ3Info);
			}
		}

		protected override void CheckUS_UQ4()
		{
			base.CheckUS_UQ4();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ4Info, Parent.Lookups.InnerPackingTypes);
			if (Parent.US_NoOfUnit4.IsEmpty && !Parent.US_UQ4.IsEmpty)
			{
				Parent.US_UQ4Info.AddMessageError(ValueIsRequired);
			}

			if (!Parent.US_NoOfUnit4.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UQ4Info);
			}
		}

		protected override void CheckUS_UQ5()
		{
			base.CheckUS_UQ5();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ5Info, Parent.Lookups.InnerPackingTypes);
			if (Parent.US_NoOfUnit5.IsEmpty && !Parent.US_UQ5.IsEmpty)
			{
				Parent.US_UQ5Info.AddMessageError(ValueIsRequired);
			}

			if (!Parent.US_NoOfUnit5.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UQ5Info);
			}
		}

		protected override void CheckUS_UQ6()
		{
			base.CheckUS_UQ6();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ6Info, Parent.Lookups.InnerPackingTypes);
			if (Parent.US_NoOfUnit6.IsEmpty && !Parent.US_UQ6.IsEmpty)
			{
				Parent.US_UQ6Info.AddMessageError(ValueIsRequired);
			}

			if (!Parent.US_NoOfUnit6.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UQ6Info);
			}
		}

		protected override void CheckUS_NetWeight()
		{
			base.CheckUS_NetWeight();
			if (Parent.US_NetWeight == 0m)
			{
				Parent.US_NetWeightInfo.AddMessageError(NetWeightCannotBeBlank);
			}
			else if (Parent.US_NetWeight < 0m)
			{
				Parent.US_NetWeightInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_WeightUQ()
		{
			base.CheckUS_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_WeightUQInfo, Parent.Lookups.PSTWeightUQList);

			if (!Parent.US_NetWeight.IsEmpty && IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_WeightUQInfo);
			}
		}

		internal const string NetWeightCannotBeBlank = "Net weight cannot be blank.";
		internal const string EnterNumberGreaterThanZero = "Please enter a number greater than 0.";

		protected override void CheckUS_CertifyingIndividual()
		{
			base.CheckUS_CertifyingIndividual();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CertifyingIndividualInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CertifyingIndividualInfo, Parent.Lookups.PSTCertifyingIndividualList);
		}

		protected override void CheckUS_NotifyParty()
		{
			base.CheckUS_NotifyParty();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NotifyPartyInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NotifyPartyInfo, Parent.Lookups.NotifyPartyList);
		}

		protected override void CheckUS_CBIIndicator()
		{
			base.CheckUS_CBIIndicator();
			ValidateUS_ConfidentialityRemarks();
		}

		protected override void CheckUS_OA_ShipperAddress()
		{
			base.CheckUS_OA_ShipperAddress();

			var pesticide = Parent.Parent;
			if (pesticide != null && IsPGAValidationOn)
			{
				var shipperAddress = pesticide.ShipperAddress;
				if (shipperAddress == null)
				{
					Parent.US_OA_ShipperAddressInfo.AddMessageError(ShipperRequired);
				}
				else
				{
					var organisation = shipperAddress.Header;
					OrganisationValidation.ValidatePGAContact(Parent.US_OA_ShipperAddressInfo, OrgHeaderWrapper.New(organisation));
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ShipperAddressInfo, shipperAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(pesticide.US_OA_ShipperAddressInfo, shipperAddress);
				}
			}
		}
		internal const string ShipperRequired = "Shipper is required for PST reporting.";

		protected override void CheckUS_PSTLabelsSent()
		{
			base.CheckUS_PSTLabelsSent();
			if (!Parent.US_PSTLabelsSent)
			{
				Parent.US_PSTLabelsSentInfo.AddMessageError(LabelRequired);
			}
		}
		internal const string LabelRequired = "This indicator is mandatory, please confirm Electronic Image Submitted.";
	}
}
