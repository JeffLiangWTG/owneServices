using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class OrgProfitShareDetailsValidation : AutoOrgProfitShareDetailsValidation
	{
		public OrgProfitShareDetailsValidation(AutoOrgProfitShareDetails parent)
			: base(parent)
		{
		}

		new OrgProfitShareDetails Parent
		{
			get { return (OrgProfitShareDetails)base.Parent; }
		}

		protected override void CheckO4_SendingPortOrCountry()
		{
			base.CheckO4_SendingPortOrCountry();
			if (!Parent.O4_SendingPortOrCountry.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.O4_SendingPortOrCountryInfo);
			}
		}

		protected override void CheckO4_ReceivingPortOrCountry()
		{
			base.CheckO4_ReceivingPortOrCountry();
			if (!Parent.O4_ReceivingPortOrCountry.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.O4_ReceivingPortOrCountryInfo);
			}
		}

		protected override void CheckO4_AgreementType()
		{
			base.CheckO4_AgreementType();
			MandatoryValidation.CheckEntered(Parent.O4_AgreementTypeInfo);
			if (!Parent.O4_AgreementType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.O4_AgreementTypeInfo);
			}
		}

		protected override void CheckO4_FreightMode()
		{
			base.CheckO4_FreightMode();
			MandatoryValidation.CheckEntered(Parent.O4_FreightModeInfo);
			if (!Parent.O4_FreightMode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.O4_FreightModeInfo);
			}
		}

		protected override void CheckO4_OrgOverrideType()
		{
			base.CheckO4_OrgOverrideType();

			MandatoryValidation.CheckEntered(Parent.O4_OrgOverrideTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.O4_OrgOverrideTypeInfo);
		}

		protected override void CheckO4_StartDate()
		{
			base.CheckO4_StartDate();
			MandatoryValidation.CheckEntered(Parent.O4_StartDateInfo);
			CompareValidation.CheckDateIsBeforeAnotherDate(Parent.O4_StartDateInfo, Parent.O4_EndDateInfo);
		}

		protected override void CheckO4_EndDate()
		{
			base.CheckO4_EndDate();
			MandatoryValidation.CheckEntered(Parent.O4_EndDateInfo);
			CompareValidation.CheckDateIsAfterAnotherDate(Parent.O4_EndDateInfo, Parent.O4_StartDateInfo);
		}

		protected override void CheckO4_OH_OrgOverride()
		{
			//base.CheckO4_OH_OrgOverride();

			if (Parent.HasContext(BusinessContext.ClientSpecificProfitShareDetails))
			{
				MandatoryValidation.CheckEntered(Parent.O4_OH_OrgOverrideInfo);
			}
		}

		protected override void CheckO4_JobType()
		{
			base.CheckO4_JobType();
			ListValidation.ErrorIfInvalidCode(Parent.O4_JobTypeInfo);
		}

		protected override void CheckO4_GatewayAgentType()
		{
			base.CheckO4_GatewayAgentType();
			ListValidation.ErrorIfInvalidCode(Parent.O4_GatewayAgentTypeInfo);
		}

		protected override void CheckO4_GatewayProfitApportionmentMethod()
		{
			base.CheckO4_GatewayProfitApportionmentMethod();
			ListValidation.ErrorIfInvalidCode(Parent.O4_GatewayProfitApportionmentMethodInfo);
			if (!Parent.O4_GatewayProfitApportionmentMethodInfo.HasErrors())
			{
				ValidateProfitRedistributionDetails();
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAgreementTypeDescription();
		}

		public void ValidateAgreementTypeDescription()
		{
			ValidateCalculatedProperty(Parent.AgreementTypeDescriptionInfo);
		}

		void ValidateProfitRedistributionDetails()
		{
			var gatewayProfitApportionmentMethod = Parent.O4_GatewayProfitApportionmentMethod;
			if (gatewayProfitApportionmentMethod.IsEmpty)
			{
				return;
			}

			var parties = Parent.PartyDetailsForGatewayProfitShareRedistribution.OfType<OrgProfitShareParty>();
			if (parties.Any())
			{
				return;
			}

			Parent.O4_GatewayProfitApportionmentMethodInfo.AddError(
				Res.GetString(
					"92447916-6b72-4b0d-b248-7f18ac57dfa4",
					"'{0}' Gateway Profit Apportionment Method requires setup in 'G/W Consol Profit Redistribution' tab.",
					gatewayProfitApportionmentMethod));
		}

		protected void CheckAgreementTypeDescription()
		{
			if (Parent.O4_AgreementType == OrgProfitShareDetailsLookups.AgreementTypeUserDefined)
			{
				MandatoryValidation.CheckEntered(Parent.AgreementTypeDescriptionInfo, Res.GetString("8c7c5f82-832b-4f27-8228-d2e6788b4176", "Charge Code"));

				if (!string.IsNullOrEmpty(Parent.AgreementTypeDescription))
				{
					var invalidCodes = OrgProfitShareDetails.SplitChargeCodesString(Parent.AgreementTypeDescription).Where(x => !Parent.UserChargeCodesDictionary.Values.Contains(x));
					if (invalidCodes.Any())
					{
						Parent.AgreementTypeDescriptionInfo.AddError(Res.GetString("11ad9030-a468-44dd-afe8-d4c13df554c6", "The following Charge Codes are invalid: {0}", OrgProfitShareDetails.JoinChargeCodesList(invalidCodes)));
					}
				}
			}
		}
	}
}
