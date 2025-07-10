using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AuthorisationRequirementCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompanyDataValidation : AutoOrgCompanyDataValidation
	{
		public OrgCompanyDataValidation(AutoOrgCompanyData parent)
			: base(parent)
		{
			CompanyData = (OrgCompanyData)parent;
		}

		readonly OrgCompanyData CompanyData;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOB_ARCreditCardExpire_Month();
			ValidateOB_ARCreditCardExpire_Year();
		}

		#region Tax Configuration Template

		protected override void CheckOB_OCT_ARTaxTemplate()
		{
			base.CheckOB_OCT_ARTaxTemplate();
			ListValidation.ErrorIfInvalidPK(Parent.OB_OCT_ARTaxTemplateInfo);
		}

		protected override void CheckOB_OCT_APTaxTemplate()
		{
			base.CheckOB_OCT_APTaxTemplate();
			ListValidation.ErrorIfInvalidPK(Parent.OB_OCT_APTaxTemplateInfo);
		}

		#endregion

		#region Payables (OB_IsCreditor)

		protected override void CheckOB_IsCreditor()
		{
			base.CheckOB_IsCreditor();

			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent.Header, Parent.OB_IsCreditorInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent.Header, Parent.OB_IsCreditorInfo);
		}

		protected override void CheckOB_OG_APCreditorGroup()
		{
			if (!Parent.OB_OG_APCreditorGroupInfo.ReadOnly)
			{
				base.CheckOB_OG_APCreditorGroup();
				if (Parent.OB_IsCreditor)
				{
					MandatoryValidation.CheckEntered(Parent.OB_OG_APCreditorGroupInfo);
				}
			}
		}

		protected override void CheckOB_APCreditLimit()
		{
			base.CheckOB_APCreditLimit();
			if (Parent.OB_IsCreditor)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OB_APCreditLimitInfo);
			}
		}

		#region CheckOB_APPaymentTerms

		protected override void CheckOB_APPaymentTerms()
		{
			base.CheckOB_APPaymentTerms();

			if (Parent.OB_IsCreditor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OB_APPaymentTermsInfo);

				if (Parent.OB_APPaymentTerms == OrgCompanyDataLookups.DefaultInvoiceTerm.Code && !CanDefaultAPTermBeSet)
				{
					Parent.OB_APPaymentTermsInfo.AddError(CanDefaultAPTermBeSetError);
				}

				if (Parent.OB_APPaymentTerms == InvoiceTermsList.FromShipmentDate.Code)
				{
					Parent.OB_APPaymentTermsInfo.AddWarning(Res.GetString("16fb10f8-e7ac-4989-afeb-dc114e0d9c82", "This AP Invoice Term is for reference purposes only and can be used to record the invoice term that your supplier has given to you. However this term does not default the due date based on the shipment date and rather defaults based on the user entered invoice date, because it is common to receive AP Invoices that span multiple jobs with different shipment dates."));
				}

				if (Parent.OB_APPaymentTerms == InvoiceTermsList.FromCustomsClearanceDate.Code)
				{
					Parent.OB_APPaymentTermsInfo.AddWarning(Res.GetString("329491bf-47f3-4b35-93d5-10e2d0d2c7fd", "This AP Invoice Term is for reference purposes only and can be used to record the invoice term that your supplier has given to you. However this term does not default the due date based on the customs clearance date and rather defaults based on the user entered invoice date, because it is common to receive AP Invoices that span multiple jobs with different customs clearance dates."));
				}
			}
		}

		public bool CanDefaultAPTermBeSet
		{
			get { return string.IsNullOrEmpty(CanDefaultAPTermBeSetError); }
		}

		string CanDefaultAPTermBeSetError
		{
			get
			{
				string reason = null;
				if (Parent.Header != null)
				{
					ZQuery relatedPartyQuery = new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, Parent.Header.PK);
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.APSettlementGroup);
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyDirectionList.Codes.AP);
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, Parent.Header.PK);

					OrgRelatedParty childSettlementRelatedParty = Parent.Factory.LoadTop1<OrgRelatedParty>(relatedPartyQuery);
					if (childSettlementRelatedParty != null)
					{
						reason = Res.GetString("64e82581-b76c-48bd-a2ec-f90e8b9cf6f2", "this organization is settlement group for another one");
					}
					else if (Parent.Header.APSettlementGroup != null)
					{
						bool isSettlementGroupTheSameAsHeader = Parent.Header.APSettlementGroup.PK == Parent.Header.PK;
						bool isSettelmentGroupHasDefaultInvoiceTerm = Parent.Header.APSettlementGroup.CompanyData.GetAPTermWithoutFallback().Term == OrgCompanyDataLookups.DefaultInvoiceTerm.Code;

						if (isSettlementGroupTheSameAsHeader)
						{
							reason = Res.GetString("3d3ef207-4a9a-46a9-b2d9-c48142a8b59d", "AP Settlement Group is set to itself");
						}
						else if (isSettelmentGroupHasDefaultInvoiceTerm)
						{
							reason = Res.GetString("e80fd41c-6f07-4ef5-aa75-b6b040ee84e7", "settlement organization also has '{0}' term", OrgCompanyDataLookups.DefaultInvoiceTerm.Code);
						}
					}
					else
					{
						reason = Res.GetString("5bb64cc3-e62c-49d7-aebb-f16f37876b22", "AP Settlement Group is empty");
					}
				}

				return string.IsNullOrEmpty(reason) ? string.Empty :
					Res.GetString("f20774b4-6d5b-49b4-88ed-2b53da105984", "'{0}' term can't be set if {1}.", OrgCompanyDataLookups.DefaultInvoiceTerm.Code, reason);
			}
		}

		#endregion

		protected override void CheckOB_APCategory()
		{
			base.CheckOB_APCategory();
			if (Parent.OB_IsCreditor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OB_APCategoryInfo);
			}
		}

		protected override void CheckOB_RX_NKAPDefltCurrency()
		{
			base.CheckOB_RX_NKAPDefltCurrency();
			if (Parent.OB_IsCreditor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OB_RX_NKAPDefltCurrencyInfo);
			}
		}

		protected override void CheckOB_APExternalCreditorCode()
		{
			base.CheckOB_APExternalCreditorCode();
			if (Parent.OB_IsCreditor &&
				OrganisationsDataRegistry.Instance.MakeExternalCreditorCodeMandatory.GetFallBackValueAtAllLevels(Parent.OB_GC.ToGuid(), Guid.Empty, Guid.Empty) &&
				Parent.OB_APExternalCreditorCode.IsEmpty)
			{
				Parent.OB_APExternalCreditorCodeInfo.AddError(Res.GetString("851dfb44-7957-40a1-b967-74e14725fb14", "Please enter an External Creditor Code.\r\nThis field is made mandatory by the following registry item:\r\nRegistry -> Organizations -> Make External Creditor Code Mandatory"));
			}
		}

		protected override void CheckOB_APVATConfig()
		{
			base.CheckOB_APVATConfig();

			if (Parent.OB_IsCreditor)
			{
				MandatoryValidation.CheckEntered(Parent.OB_APVATConfigInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OB_APVATConfigInfo);

				bool isOverrideSet = Parent.OB_APVATConfig != AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code &&
									Parent.OB_APVATConfig != AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				if (isOverrideSet)
				{
					if (!GlbCompany.CurrentCompany.GC_IsGSTCashBasis && isOverrideSet)
					{
						Parent.OB_APVATConfigInfo.AddError(Res.GetString("c89f2f32-0f9d-48f6-85c2-5e124eab225f", "Override is not permitted. All Tax for your company is reported on an Accrual Basis."));
					}
					if (!ObjectFactory.Get<IAccounting>().TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(LedgerTypes.AccountsPayable))
					{
						Parent.OB_APVATConfigInfo.AddError(Res.GetString("4e08a907-77c8-48ac-a843-1adc52727a27", "AP Organization specific Overrides are not Permitted by your Login Company’s Tax Recognition Defaulting Rules registry."));
					}
				}
			}
		}

		protected override void CheckOB_APCreateVATComplianceDocumentOnPosting()
		{
			base.CheckOB_APCreateVATComplianceDocumentOnPosting();

			if (Parent.OB_IsCreditor)
			{
				MandatoryValidation.CheckEntered(Parent.OB_APCreateVATComplianceDocumentOnPostingInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OB_APCreateVATComplianceDocumentOnPostingInfo);

				if (Parent.OB_APVATConfig == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code
					&& Parent.OB_APCreateVATComplianceDocumentOnPosting != AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable.Code)
				{
					Parent.OB_APCreateVATComplianceDocumentOnPostingInfo.AddError(Res.GetString("c8a0a7fe-5913-4c6d-9901-440f01fe0952", "This value must be set 'NON' if Creditor is Not Applicable to Tax."));
				}
			}
		}

		protected override void CheckOB_APCreditAgreedPaymentMethod()
		{
			base.CheckOB_APCreditAgreedPaymentMethod();
			if (CompanyData.OB_IsCreditor && !CompanyData.OB_APCreditAgreedPaymentMethod.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(CompanyData.OB_APCreditAgreedPaymentMethodInfo);
			}
		}

		#endregion

		#region Receivables (OB_IsDebtor)

		protected override void CheckOB_IsDebtor()
		{
			base.CheckOB_IsDebtor();

			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent.Header, Parent.OB_IsDebtorInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent.Header, Parent.OB_IsDebtorInfo);
		}

		protected override void CheckOB_RX_NKARDDefltCurrency()
		{
			base.CheckOB_RX_NKARDDefltCurrency();
			if (Parent.OB_IsDebtor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OB_RX_NKARDDefltCurrencyInfo);
			}
		}

		protected override void CheckOB_ARTreatDisbursementsAsStandardValue()
		{
			base.CheckOB_ARTreatDisbursementsAsStandardValue();
			if (Parent.OB_IsDebtor)
			{
				MandatoryValidation.CheckNotNegative(Parent.OB_ARTreatDisbursementsAsStandardValueInfo);
			}
		}

		protected override void CheckOB_ARCreditRating()
		{
			base.CheckOB_ARCreditRating();
			if (Parent.OB_IsDebtor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OB_ARCreditRatingInfo);
			}
		}

		protected override void CheckOB_ARConsolidatedAccountingCategory()
		{
			base.CheckOB_ARConsolidatedAccountingCategory();
			if (Parent.OB_IsDebtor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OB_ARConsolidatedAccountingCategoryInfo);
			}
		}

		protected override void CheckOB_OJ_ARDebtorGroup()
		{
			if (!Parent.OB_OJ_ARDebtorGroupInfo.ReadOnly)
			{
				base.CheckOB_OJ_ARDebtorGroup();

				if (CompanyData.OB_IsDebtor)
				{
					MandatoryValidation.CheckEntered(CompanyData.OB_OJ_ARDebtorGroupInfo);
				}
			}
		}

		protected override void CheckOB_AB_ARPayToAccount()
		{
			base.CheckOB_AB_ARPayToAccount();

			if (CompanyData.OB_IsDebtor)
			{
				if (CompanyData.OverrideBankAccountFromDebtorGroup)
				{
					MandatoryValidation.CheckEntered(CompanyData.OB_AB_ARPayToAccountInfo);
				}

				ListValidation.ErrorIfInvalidPK(CompanyData.OB_AB_ARPayToAccountInfo);
			}
		}

		protected override void CheckOB_ARWarehouseRatingPeriod()
		{
			base.CheckOB_ARWarehouseRatingPeriod();

			if (CompanyData.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(CompanyData.OB_ARWarehouseRatingPeriodInfo);
				ListValidation.ErrorIfInvalidCode(CompanyData.OB_ARWarehouseRatingPeriodInfo);

				if (CompanyData.IsWhsSplitMonthBilling &&
					CompanyData.GetWarehouseRatingPeriod() != Core.Constants.StorageCalculationPeriods.Daily &&
					CompanyData.GetWarehouseRatingPeriod() != Core.Constants.StorageCalculationPeriods.Weekly &&
					CompanyData.GetWarehouseRatingPeriod() != Core.Constants.StorageCalculationPeriods.Monthly)
				{
					CompanyData.OB_ARWarehouseRatingPeriodInfo.AddError(Res.GetString("cdd8e3a7-b502-4bc2-a393-863b63215490", "For Split Period Billing, you can only specify a rating/billing period of Daily, Weekly or Monthly."));
				}

				ValidateOB_ARWhsStorageCalcMethod();
			}
		}

		protected override void CheckOB_ARWhsStorageCalcMethod()
		{
			base.CheckOB_ARWhsStorageCalcMethod();

			if (CompanyData.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(CompanyData.OB_ARWhsStorageCalcMethodInfo);
				ListValidation.ErrorIfInvalidCode(CompanyData.OB_ARWhsStorageCalcMethodInfo);
				ValidateOB_ARWarehouseRatingPeriod();
			}
		}

		protected override void CheckOB_ARBuyersConsolInvoicingStyle()
		{
			base.CheckOB_ARBuyersConsolInvoicingStyle();
			if (CompanyData.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(Parent.OB_ARBuyersConsolInvoicingStyleInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OB_ARBuyersConsolInvoicingStyleInfo);
			}
		}

		protected override void CheckOB_ARCreditAgreedPaymentMethod()
		{
			base.CheckOB_ARCreditAgreedPaymentMethod();
			if (CompanyData.OB_IsDebtor && !CompanyData.OB_ARCreditAgreedPaymentMethod.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(CompanyData.OB_ARCreditAgreedPaymentMethodInfo);

				if (CompanyData.Organisation != null)
				{
					var warningMessage = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>()
						.GetFeatureInterface<IPreferredPaymentMethod>(CompanyData.Organisation.MainAddress.OA_RN_NKCountryCode)?
						.GetPreferredPaymentMethodWarning(CompanyData.Organisation.OH_Category, CompanyData.OB_ARCreditAgreedPaymentMethod);

					if (!string.IsNullOrEmpty(warningMessage))
					{
						CompanyData.OB_ARCreditAgreedPaymentMethodInfo.AddWarning(warningMessage);
					}
				}
			}
		}

		protected override void CheckOB_ARExternalDebtorCode()
		{
			base.CheckOB_ARExternalDebtorCode();
			if (Parent.OB_IsDebtor &&
				OrganisationsDataRegistry.Instance.MakeExternalDebtorCodeMandatory.GetFallBackValueAtAllLevels(Parent.OB_GC.ToGuid(), Guid.Empty, Guid.Empty) &&
				Parent.OB_ARExternalDebtorCode.IsEmpty)
			{
				Parent.OB_ARExternalDebtorCodeInfo.AddError(Res.GetString("a9e4bbcd-ca76-404c-85f0-eb367602747e", "Please enter an External Debtor Code.\r\nThis field is made mandatory by the following registry item:\r\nRegistry -> Organizations -> Make External Debtor Code Mandatory"));
			}
		}

		protected override void CheckOB_ARUseSettlementGroupCreditLimit()
		{
			base.CheckOB_ARUseSettlementGroupCreditLimit();

			if (CompanyData.OB_IsDebtor && CompanyData.OB_ARUseSettlementGroupCreditLimit && CompanyData.Header.ARSettlementGroupPK.IsEmpty)
			{
				CompanyData.OB_ARUseSettlementGroupCreditLimitInfo.AddError(Res.GetString("33e6377a-61f9-47e2-ab2b-0678224c6583", "Settlement Group Credit Limit must not be ticked if the Settlement Group is empty"));
			}
		}

		protected override void CheckOB_ARVATConfig()
		{
			base.CheckOB_ARVATConfig();

			if (Parent.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(Parent.OB_ARVATConfigInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OB_ARVATConfigInfo);

				bool isOverrideSet = Parent.OB_ARVATConfig != AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code &&
									Parent.OB_ARVATConfig != AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				if (isOverrideSet)
				{
					if (!GlbCompany.CurrentCompany.GC_IsGSTCashBasis && isOverrideSet)
					{
						Parent.OB_ARVATConfigInfo.AddError(Res.GetString("c89f2f32-0f9d-48f6-85c2-5e124eab225f", "Override is not permitted. All Tax for your company is reported on an Accrual Basis."));
					}
					if (!ObjectFactory.Get<IAccounting>().TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(LedgerTypes.AccountsReceivable))
					{
						Parent.OB_ARVATConfigInfo.AddError(Res.GetString("f8a0a7fe-5913-4c6d-9901-440f01fe0952", "AR Organization specific Overrides are not Permitted by your Login Company’s Tax Recognition Defaulting Rules registry."));
					}
				}
			}
		}

		protected override void CheckOB_ARCreateVATComplianceDocumentOnPosting()
		{
			base.CheckOB_ARCreateVATComplianceDocumentOnPosting();

			if (Parent.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(Parent.OB_ARCreateVATComplianceDocumentOnPostingInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OB_ARCreateVATComplianceDocumentOnPostingInfo);

				if (Parent.OB_ARVATConfig == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code
				&& Parent.OB_ARCreateVATComplianceDocumentOnPosting != AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable.Code
				)
				{
					Parent.OB_ARCreateVATComplianceDocumentOnPostingInfo.AddError(Res.GetString("m8a0a7fe-5913-4c6d-9901-440f01fe0952", "This value must be set 'NON' if Debtor is Not Applicable to Tax."));
				}
			}
		}

		protected override void CheckOB_AROnCreditHold()
		{
			base.CheckOB_AROnCreditHold();
			CreditOnHoldChecker.ValidateLocalOnCreditHold(CompanyData);
		}

		#region Credit CardDetails

		protected override void CheckOB_ARCreditCardType()
		{
			base.CheckOB_ARCreditCardType();
			if (Parent.OB_IsDebtor && CreditCardDetailsFieldsNotEmpty)
			{
				MandatoryValidation.CheckEntered(CompanyData.OB_ARCreditCardTypeInfo);
				ListValidation.ErrorIfInvalidCode(CompanyData.OB_ARCreditCardTypeInfo);
				ValidateOB_ARCreditCardNum();
				ValidateOB_ARCreditCardHolder();
				ValidateOB_ARCreditCardExpire_Month();
				ValidateOB_ARCreditCardExpire_Year();
			}
		}

		protected override void CheckOB_ARCreditCardNum()
		{
			base.CheckOB_ARCreditCardNum();
			if (Parent.OB_IsDebtor && CreditCardDetailsFieldsNotEmpty)
			{
				MandatoryValidation.CheckEntered(CompanyData.OB_ARCreditCardNumInfo);
				ZString message = ZString.Empty;
				if (!IsCreditCardNumberValid(CompanyData.OB_ARCreditCardNum, ref message))
				{
					CompanyData.OB_ARCreditCardNumInfo.AddError(message);
				}
				ValidateOB_ARCreditCardType();
				ValidateOB_ARCreditCardHolder();
				ValidateOB_ARCreditCardExpire_Month();
				ValidateOB_ARCreditCardExpire_Year();
			}
		}

		protected override void CheckOB_ARCreditCardHolder()
		{
			base.CheckOB_ARCreditCardHolder();
			if (Parent.OB_IsDebtor && CreditCardDetailsFieldsNotEmpty)
			{
				MandatoryValidation.CheckEntered(CompanyData.OB_ARCreditCardHolderInfo);
				ValidateOB_ARCreditCardType();
				ValidateOB_ARCreditCardNum();
				ValidateOB_ARCreditCardExpire_Month();
				ValidateOB_ARCreditCardExpire_Year();
			}
		}

		public void ValidateOB_ARCreditCardExpire_Month()
		{
			ValidateCalculatedProperty(CompanyData.OB_ARCreditCardExpire_MonthInfo);
		}

		protected void CheckOB_ARCreditCardExpire_Month()
		{
			if (Parent.OB_IsDebtor && CreditCardDetailsFieldsNotEmpty)
			{
				if (CompanyData.OB_ARCreditCardExpire_Month == "__")
				{
					CompanyData.OB_ARCreditCardExpire_MonthInfo.AddError(Res.GetString("7ec35bea-16a5-4f99-94a5-acd8b8fcb0f7", "Please enter a value."));
				}
				ListValidation.ErrorIfInvalidCode(CompanyData.OB_ARCreditCardExpire_MonthInfo, CompanyData.Lookups.OB_ARCreditCardExpire_Month_List);
				ValidateOB_ARCreditCardType();
				ValidateOB_ARCreditCardNum();
				ValidateOB_ARCreditCardHolder();
				ValidateOB_ARCreditCardExpire_Year();
			}
		}

		public void ValidateOB_ARCreditCardExpire_Year()
		{
			ValidateCalculatedProperty(CompanyData.OB_ARCreditCardExpire_YearInfo);
		}

		protected void CheckOB_ARCreditCardExpire_Year()
		{
			if (Parent.OB_IsDebtor && CreditCardDetailsFieldsNotEmpty)
			{
				if (CompanyData.OB_ARCreditCardExpire_Year == "__")
				{
					CompanyData.OB_ARCreditCardExpire_YearInfo.AddError(Res.GetString("7ec35bea-16a5-4f99-94a5-acd8b8fcb0f7", "Please enter a value."));
				}
				ListValidation.ErrorIfInvalidCode(CompanyData.OB_ARCreditCardExpire_YearInfo, CompanyData.Lookups.OB_ARCreditCardExpire_Year_List);
				ValidateOB_ARCreditCardType();
				ValidateOB_ARCreditCardNum();
				ValidateOB_ARCreditCardHolder();
				ValidateOB_ARCreditCardExpire_Month();
			}
		}

		ZBool CreditCardDetailsFieldsNotEmpty
		{
			get
			{
				return !CompanyData.OB_ARCreditCardType.IsEmpty || !CompanyData.OB_ARCreditCardNum.IsEmpty
					   || !CompanyData.OB_ARCreditCardHolder.IsEmpty || CompanyData.OB_ARCreditCardExpire_Month != "__"
					   || CompanyData.OB_ARCreditCardExpire_Year != "__";
			}
		}

		public bool IsCreditCardNumberValid(ZString cardNumber, ref ZString message)
		{
			if (cardNumber.ContainsAnyLetters)
			{
				message = Res.GetString("dac19677-ba4c-4f2c-ae92-4c92b7b4b165", "Credit Card Number should be numerical.");
				return false;
			}

			if (cardNumber.Length < 13 || cardNumber.Length > 19)
			{
				message = Res.GetString("8c227047-4b16-403d-b435-d5578917308d", "Credit Card Number should be 13-19 digits length.");
				return false;
			}

			if ((CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.Bankcard ||
				CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.DinClubUSCanada ||
				CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.Discover ||
				CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.JCB ||
				CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.Mastercard ||
				CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.VisaEl) && (cardNumber.Length != 16))
			{
				message = Res.GetString("bcf50073-a29e-4b78-b7a4-37ac0d3aeaaa", "Credit Card Number should be 16 digits length.");
				return false;
			}

			if ((CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.AmExpress ||
				CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.DinClubenRoute ||
				CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.JCBobsolete) && (cardNumber.Length != 15))
			{
				message = Res.GetString("e77a9e9b-c91f-4f6b-b163-aef1182b242b", "Credit Card Number should be 15 digits length.");
				return false;
			}

			if ((CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.DinClub ||
				CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.DinClubInt) && (cardNumber.Length != 14))
			{
				message = Res.GetString("b6166bf3-ae11-4b62-840e-cd244f671119", "Credit Card Number should be 14 digits length.");
				return false;
			}

			if (CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.Visa && cardNumber.Length != 13 && cardNumber.Length != 16)
			{
				message = Res.GetString("2e615fe3-6d98-4c61-99e9-7fb947c6c83b", "Credit Card Number should be 13 or 16 digits length.");
				return false;
			}

			if (CompanyData.OB_ARCreditCardType == CreditCardTypeList.Codes.ChinaUnionPay && (cardNumber.Length < 16 || cardNumber.Length > 19))
			{
				message = Res.GetString("c2f00dcd-f2b5-4295-ac4d-d9b840f174bc", "Credit Card Number should be 16-19 digits length.");
				return false;
			}

			if (CompanyData.OB_ARCreditCardType != CreditCardTypeList.Codes.ChinaUnionPay && CompanyData.OB_ARCreditCardType != CreditCardTypeList.Codes.DinClubenRoute)
			{
				int digit, sum, total = 0;
				string number = cardNumber.ToString();

				for (int i = number.Length + 1; i <= 16; i++)
				{
					number = "0" + number;
				}

				for (int i = 1; i <= 16; i++)
				{
					int multiplier = 1 + (i % 2);
					digit = ZInt.ParseSafe(number.Substring(i - 1, 1), 0);
					sum = digit * multiplier;
					if (sum > 9)
					{
						sum -= 9;
					}

					total += sum;
				}
				bool result = total % 10 == 0;

				if (!result)
				{
					message = Res.GetString("b0c1093b-590a-430d-bc56-7613742e2c6a", "Credit Card Number invalid.");
				}

				return result;
			}

			return true;
		}

		#endregion

		CreditTemporaryIncreaseAuthorisationSettingsRegistryItem CreditLimitCheckTemporaryCreditLimitIncreaseThreshold
		{
			get { return ObjectFactory.Get<IAccounting>().Registry.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold as CreditTemporaryIncreaseAuthorisationSettingsRegistryItem; }
		}

		ZString SecurityRequiredForTemporaryCreditLimitIncreaseErrorMessage
		{
			get { return Res.GetString("f8549869-7aba-4814-b256-f331dd638ced", @"You do not have the appropriate security rights to set this adjustment amount.

Please ask your administrator to change either your Staff or Group Security Rights to allow the appropriate access via:

{0}", Env.Security.OrgReceivablesTemporaryCreditAdjustment.DisplayTextPathToSecurityRight); }
		}

		protected override void CheckOB_ARTemporaryCreditLimitIncrease()
		{
			base.CheckOB_ARTemporaryCreditLimitIncrease();
			if (CompanyData.OB_IsDebtor)
			{
				MandatoryValidation.CheckNotNegative(Parent.OB_ARTemporaryCreditLimitIncreaseInfo);

				if (!Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasErrors() &&
					(Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasChanges ||
					Parent.OB_ARCreditApprovedInfo.HasChanges ||
					!Parent.IsInDatabase) &&
					Parent.OB_ARTemporaryCreditLimitIncrease != 0)
				{
					if (!Parent.OB_ARCreditApproved)
					{
						Parent.OB_ARTemporaryCreditLimitIncreaseInfo.AddError(Res.GetString("28a5b566-4197-4af4-850a-a0ad0a53a218", "You cannot enter a Temporary Credit Limit Increase because credit is not approved."));
					}
				}

				if (!Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasErrors() &&
					(Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasChanges ||
					Parent.OB_ARCreditLimitInfo.HasChanges ||
					!Parent.IsInDatabase) &&
					Parent.OB_ARTemporaryCreditLimitIncrease != 0)
				{
					if (Parent.OB_ARCreditLimit == 0)
					{
						Parent.OB_ARTemporaryCreditLimitIncreaseInfo.AddError(Res.GetString("04950e78-0fd3-4d50-a1f3-04841f6d960a", "You cannot enter a Temporary Credit Limit Increase because the Credit Limit is not set."));
					}
				}

				if (!Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasErrors() &&
					(Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasChanges || !Parent.IsInDatabase) &&
					Parent.OB_ARTemporaryCreditLimitIncrease != 0)
				{
					if (!Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasErrors() && ObjectFactory.Get<IAccounting>().UseWebServiceForCreditLimit)
					{
						Parent.OB_ARTemporaryCreditLimitIncreaseInfo.AddError(Res.GetString("4578f68e-9a70-420f-b6dc-59350f93a0f1", "This setting cannot be used when the 'Use Web Service for Credit Limit' registry item is turned on."));
					}

					if (!Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasErrors())
					{
						CheckARTemporaryCreditLimitIncreaseAgainstRegistry();
					}
				}

				if (!Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasErrors() &&
					Parent.IsInDatabase &&
					Parent.OB_ARTemporaryCreditLimitIncreaseInfo.HasChanges &&
					(ZDecimal)Parent.OB_ARTemporaryCreditLimitIncreaseInfo.OriginalValue != 0m &&
					(ZDateTime)Parent.OB_ARTemporaryCreditLimitIncreaseExpiryInfo.OriginalValue > ZDateTime.UtcNow)
				{
					CheckARTemporaryCreditLimitForHigherSecurityAccess();
				}
			}
		}

		int GetCreditLimitAdjustmentMaxLevel(SecurityCore security)
		{
			return security.CreditLimitAdjustmentThirdLevel.IsAllowed ? 3 :
						security.CreditLimitAdjustmentSecondLevel.IsAllowed ? 2 :
						security.CreditLimitAdjustmentFirstLevel.IsAllowed ? 1 : 0;
		}

		void CheckARTemporaryCreditLimitForHigherSecurityAccess()
		{
			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModified.Code);
			filter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Like, String.Format("%|{0}={1}", OrgCompanyData.LogParameterKeys.Type, OrgCompanyData.LogTypes.TemporaryCreditLimitAdjustment));
			filter.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";
			var log = Parent.Logs.Find(filter).FirstOrDefault();

			if (log != null)
			{
				int levelRequired = GetCreditLimitAdjustmentMaxLevel(new SecurityCore(null, log.User.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK));
				int levelAuthorised = GetCreditLimitAdjustmentMaxLevel(Env.Security);

				if (levelAuthorised < levelRequired)
				{
					Parent.OB_ARTemporaryCreditLimitIncreaseInfo.AddError(Res.GetString("7d58ccad-de6d-4a77-8548-03e83036edfc", "You do not have the appropriate security rights to change the Temporary Credit Limit Increase. This is because the Temporary Credit Limit Increase has been set by {0} ({1}) with higher approval right.", log.User.GS_FullName, log.User.GS_Code));
				}
			}
		}

		void CheckARTemporaryCreditLimitIncreaseAgainstRegistry()
		{
			TemporaryCreditLimitIncreaseHelper.SettingNotFoundReason settingNotFoundReason;

			var setting = TemporaryCreditLimitIncreaseHelper.Instance.GetSettingForProposedIncrease(Parent.OB_ARCreditLimit, Parent.OB_ARTemporaryCreditLimitIncrease, out settingNotFoundReason);

			if (setting == null)
			{
				switch (settingNotFoundReason)
				{
					case TemporaryCreditLimitIncreaseHelper.SettingNotFoundReason.RegistryNotConfigured:
						Parent.OB_ARTemporaryCreditLimitIncreaseInfo.AddError(
							Res.GetString("62d4cb02-8657-4964-8ab5-e32a3478c0d3", "To enter a temporary credit limit increase you must first configure the registry setting: '{0}'",
							CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.Caption));
						break;
					default:
						Parent.OB_ARTemporaryCreditLimitIncreaseInfo.AddError(SecurityRequiredForTemporaryCreditLimitIncreaseErrorMessage);
						break;
				}
			}
			else
			{
				bool isAuthorised;
				switch (setting.AuthorisationRequirement)
				{
					case AuthorisationRequirementCodes.NoApprovalRequired:
						isAuthorised = true;
						break;
					case AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
						isAuthorised = Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed;
						break;
					case AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
						isAuthorised = Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed;
						break;
					case AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
						isAuthorised = Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed;
						break;
					default:
						isAuthorised = false;
						break;
				}
				if (!isAuthorised)
				{
					Parent.OB_ARTemporaryCreditLimitIncreaseInfo.AddError(SecurityRequiredForTemporaryCreditLimitIncreaseErrorMessage);
				}
			}
		}

		#endregion

		#region Controlling Branch

		protected override void CheckOB_GB_ControllingBranch()
		{
			base.CheckOB_GB_ControllingBranch();

			if (CompanyData.ControllingBranch != null && (CompanyData.ControllingBranch.Company == null || CompanyData.ControllingBranch.Company.PK != Env.CurrentCompany.PK))
			{
				CompanyData.OB_GB_ControllingBranchInfo.AddWarning(Res.GetString("93c96040-0153-41f9-aaa2-637e87834185", "The selected branch does not belong to the company you are currently logged into. In almost all cases this branch should belong to the currently logged in company."));
			}

			// If the header is null, then there is nothing to validate.
			if (CompanyData.Header != null && CompanyData.Header.RequiredFieldsForOrg.RequireBranch && !IsOrgProxyForAnyBranch)
			{
				MandatoryValidation.CheckEntered(CompanyData.OB_GB_ControllingBranchInfo);
			}
		}

		bool IsOrgProxyForAnyBranch
		{
			get { return CompanyData.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(GlbBranch)), new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, CompanyData.Header.PK)); }
		}

		#endregion

		protected override void CheckOB_RateSecurityGroup()
		{
			base.CheckOB_RateSecurityGroup();
			ListValidation.ErrorIfInvalidCode(Parent.OB_RateSecurityGroupInfo);
		}

		protected override void CheckOB_CRIsShipsAgencyPrincipal()
		{
			base.CheckOB_CRIsShipsAgencyPrincipal();
			if (CompanyData.OB_CRIsShipsAgencyPrincipal && !CompanyData.Header.OH_IsShippingLine)
			{
				CompanyData.OB_CRIsShipsAgencyPrincipalInfo.AddError(Res.GetString("1284505a-fde9-481e-b554-7da9451cd059", "To mark an Organization as Principal it must also be marked as Shipping Line."));
			}
		}

		protected override void CheckOB_APTransactionCreationRestriction()
		{
			base.CheckOB_APTransactionCreationRestriction();
			MandatoryValidation.CheckEntered(Parent.OB_APTransactionCreationRestrictionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OB_APTransactionCreationRestrictionInfo);
		}

		protected override void CheckOB_ARTransactionCreationRestriction()
		{
			base.CheckOB_ARTransactionCreationRestriction();
			MandatoryValidation.CheckEntered(Parent.OB_ARTransactionCreationRestrictionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OB_ARTransactionCreationRestrictionInfo);
		}

		protected override void CheckOB_ARGoodsOwnership()
		{
			base.CheckOB_ARGoodsOwnership();
			ListValidation.ErrorIfInvalidCode(Parent.OB_ARGoodsOwnershipInfo);
		}

		#region OB_ARApplicableSurcharges

		AccSurchargeConfigurationCollection accSurchargeConfigurationCollection;
		AccSurchargeConfigurationCollection AccSurchargeConfigurationCollection
		{
			get
			{
				if (accSurchargeConfigurationCollection == null)
				{
					var localAccSurchargeConfigurationCollection = new AccSurchargeConfigurationCollection(Parent.Factory, Parent.OB_GC);
					localAccSurchargeConfigurationCollection.Load();
					accSurchargeConfigurationCollection = localAccSurchargeConfigurationCollection;
				}
				return accSurchargeConfigurationCollection;
			}
		}

		protected override void CheckOB_ARApplicableSurcharges()
		{
			base.CheckOB_ARApplicableSurcharges();
			MandatoryValidation.CheckEntered(Parent.OB_ARApplicableSurchargesInfo);
			CheckOB_ARApplicableSurchargesIsValid();
			CheckOB_ARApplicableSurchargesDuplicate();
		}

		protected void CheckOB_ARApplicableSurchargesIsValid()
		{
			string applicableSurcharges = Parent.OB_ARApplicableSurcharges;

			if (!Parent.OB_ARApplicableSurchargesInfo.HasErrors() && applicableSurcharges != AccountingMasterFilesConstants.ReserveSurchargeCodes.All && applicableSurcharges != AccountingMasterFilesConstants.ReserveSurchargeCodes.Non)
			{
				string[] applicableSurchargeArray = applicableSurcharges.Split(',');
				var invalidSurchargeList = new List<string>();

				for (int i = 0; i < applicableSurchargeArray.Length; i++)
				{
					string surchargeCode = applicableSurchargeArray[i].Trim();
					if (!AccSurchargeConfigurationCollection.OfType<AccSurchargeConfiguration>().Any(o => o.ASC_Code == surchargeCode))
					{
						if (!invalidSurchargeList.Contains(surchargeCode))
						{
							invalidSurchargeList.Add(surchargeCode);
							if (!string.IsNullOrEmpty(surchargeCode))
							{
								Parent.OB_ARApplicableSurchargesInfo.AddError(Res.GetString("821372e0-6bf7-47b0-8f8e-27201d9f9114", "{0} is not a valid Surcharge Code.", surchargeCode));
							}
						}
					}
				}

				if (invalidSurchargeList.Count > 0)
				{
					Parent.OB_ARApplicableSurchargesInfo.AddError(Res.GetString("9ae2ce2f-0f73-4ad1-8da1-830a124dd72d", "Applicable Surcharges must be {0},{1} or a comma separated list of valid Surcharge Codes.", AccountingMasterFilesConstants.ReserveSurchargeCodes.All, AccountingMasterFilesConstants.ReserveSurchargeCodes.Non));
				}
			}
		}

		void CheckOB_ARApplicableSurchargesDuplicate()
		{
			string applicableSurcharges = Parent.OB_ARApplicableSurcharges;

			if (!Parent.OB_ARApplicableSurchargesInfo.HasErrors() && applicableSurcharges != AccountingMasterFilesConstants.ReserveSurchargeCodes.All && applicableSurcharges != AccountingMasterFilesConstants.ReserveSurchargeCodes.Non)
			{
				var applicableSurchargeArray = applicableSurcharges.Split(',').GroupBy(x => x.Trim()).Where(g => g.Count() > 1).Select(y => y.Key);
				if (applicableSurchargeArray.Any())
				{
					var duplicatedSurcharges = applicableSurchargeArray.Aggregate((m, n) => $"{m}, {n}");
					if (duplicatedSurcharges.Length > 0)
					{
						Parent.OB_ARApplicableSurchargesInfo.AddError(Res.GetString("388a110e-2a12-49f9-9607-40a30201aa35", "Please remove duplicate surcharges: {0}.", duplicatedSurcharges));
					}
				}
			}
		}
		#endregion

		#region WhsAutoPeriodicInvoice

		#region CheckOB_WhsAllowCreateInvoiceWithNoTransactionsOrStock

		protected override void CheckOB_WhsAllowCreateInvoiceWithNoTransactionsOrStock()
		{
			if (CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock && !CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice)
			{
				CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo.AddError(Res.GetString("18CCAA55-D3DA-41CB-BF79-56600D3F83A5", "Cannot enable Allow Creation of Periodic Invoice jobs without current transactions or existing Inventory if this Organization does not automatically Create Periodic Invoices."));
			}
		}

		#endregion

		#region CheckOB_WhsAutoPostPeriodicInvoice

		protected override void CheckOB_WhsAutoPostPeriodicInvoice()
		{
			if (CompanyData.OB_WhsAutoPostPeriodicInvoice && !CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice)
			{
				CompanyData.OB_WhsAutoPostPeriodicInvoiceInfo.AddError(Res.GetString("A59DA34F-008F-4015-BFEA-AC92D0D86177", "Cannot enable Auto Post Periodic Invoices if this Organization does not automatically Create Periodic Invoices."));
			}
		}

		#endregion

		#region CheckOB_WhsAutoDeliverPeriodicInvoice

		protected override void CheckOB_WhsAutoDeliverPeriodicInvoice()
		{
			if (CompanyData.OB_WhsAutoDeliverPeriodicInvoice && !CompanyData.OB_WhsAutoPostPeriodicInvoice)
			{
				CompanyData.OB_WhsAutoDeliverPeriodicInvoiceInfo.AddError(Res.GetString("28C9F51B-00E7-474C-AA25-6CC625FE23A7", "Cannot enable Auto Deliver Periodic Invoices if this Organization does not automatically Post Periodic Invoices."));
			}
		}

		#endregion

		#endregion
	}
}
