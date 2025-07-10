//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeTaxOverrideValidation
//
//    This class should be used for overriding validation in AutoAccChargeTaxOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeTaxOverrideValidation : AutoAccChargeTaxOverrideValidation
	{
		public AccChargeTaxOverrideValidation(AutoAccChargeTaxOverride parent)
			: base(parent)
		{
		}

		protected new AccChargeTaxOverride Parent
		{
			get { return (AccChargeTaxOverride)base.Parent; }
		}

		bool IsParentChargeTypeComment
		{
			get { return ParentChargeCode != null && ParentChargeCode.AC_ChargeType == Core.Constants.ChargeType.Comment; }
		}

		AccChargeCode ParentChargeCode
		{
			get
			{
				AccChargeTaxOverride taxOverride = Parent;
				return taxOverride == null ? null : taxOverride.ChargeCode;
			}
		}

		ZString countryCode
		{
			get { return Parent.CurrentCountryCode; }
		}
		#region AO_AT

		protected override void CheckAO_AT()
		{
			if (!Parent.AO_ATInfo.ReadOnly)
			{
				var pivots = Parent.TaxOverrideGroup?.TaxOverrideGroupTaxConfigurationPivots;
				if (!IsParentChargeTypeComment)
				{
					if (pivots == null || pivots.Count == 0 || pivots[0].AXP_AT_TaxID.IsEmpty)
					{
						base.CheckAO_AT();

						MandatoryValidation.CheckEntered(Parent.AO_ATInfo);
						ListValidation.ErrorIfInvalidPK(Parent.AO_ATInfo);
					}
					else if (!Parent.AO_AT.IsEmpty)
					{
						Parent.AO_ATInfo.AddError(Res.GetString("0b79a89c-55b1-4fdc-9d77-3d22e8cd039c", "Tax ID must be empty here as the Tax ID is already been entered in the Tax Framework Configuration grid."));
					}

					if (Parent.TaxRate != null && !Parent.AO_ATInfo.HasErrors() && Parent.AO_DefaultingRule == Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount && !Parent.TaxRate.IsZeroTaxRate)
					{
						Parent.AO_ATInfo.AddError(Res.GetString("4FFC459E-6C47-4824-ADE4-2F0ABCFA8D53", "Tax ID's Tax Rate must equal 0 when Defaulting Rule is 'SUM'."));
					}
				}
				else if (!Parent.AO_AT.IsEmpty)
				{
					Parent.AO_ATInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
				}

				if (pivots?.Count > 1 && !Parent.AO_AT.IsEmpty)
				{
					Parent.AO_ATInfo.AddError(Res.GetString("dc9bf7af-5c49-4193-bc53-8d9332fdec5c", "Tax ID must be empty here since more than one Tax Configuration has been added to the Tax Framework Configuration grid."));
				}
			}
		}

		protected override void CheckAO_ATIsValidZGuid()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_ATIsValidZGuid();
			}
		}

		#endregion

		#region AO_A9_DefaultVATClass

		protected override void CheckAO_A9_DefaultVATClass()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_A9_DefaultVATClass();
				ListValidation.ErrorIfInvalidPK(Parent.AO_A9_DefaultVATClassInfo);
				if (!Parent.AO_A9_DefaultVATClass.IsEmpty && Parent.AO_AT.IsEmpty && Parent.AO_DefaultingRule != Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount)
				{
					Parent.AO_A9_DefaultVATClassInfo.AddError(ErrorMessageCannotHaveTaxMessageIfTaxIDIsMissing);
				}
			}
			else if (!Parent.AO_A9_DefaultVATClass.IsEmpty)
			{
				Parent.AO_A9_DefaultVATClassInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}

			if (!Parent.AO_A9_DefaultVATClassInfo.HasErrors() && !Parent.IsTaxFrameworkRelated)
			{
				CheckTaxIdAndTaxMessageMapping();
			}
		}

		protected override void CheckAO_A9_DefaultVATClassIsValidZGuid()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_A9_DefaultVATClassIsValidZGuid();
			}
		}

		#endregion

		#region AO_CostSellAll

		protected override void CheckAO_CostSellAll()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_CostSellAll();
				MandatoryValidation.CheckEntered(Parent.AO_CostSellAllInfo);
				if (!Parent.AO_CostSellAll.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_CostSellAllInfo);
				}
			}
			else if (!Parent.AO_CostSellAll.IsEmpty)
			{
				Parent.AO_CostSellAllInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_Direction

		protected override void CheckAO_Direction()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_Direction();
				MandatoryValidation.CheckEntered(Parent.AO_DirectionInfo);

				Parent.ClearRowNotifications();
				if (Parent.AO_Direction != AccChargeTaxOverride.ALL
					&& !(Parent.JobType?.IsDirectionSupported ?? false))
				{
					Parent.AddRowError(Res.GetString("4b85447d-29e1-4074-9f54-4678f0bf9288", "The '{0}' Job Type does not support '{1}' Direction. Please, delete an invalid tax override.", Parent.JobType?.Code, Parent.AO_Direction));
				}

				if (!Parent.AO_Direction.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_DirectionInfo);
				}
			}
			else if (!Parent.AO_Direction.IsEmpty)
			{
				Parent.AO_DirectionInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_TransportMode

		protected override void CheckAO_TransportMode()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_TransportMode();
				MandatoryValidation.CheckEntered(Parent.AO_TransportModeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AO_TransportModeInfo);
			}
			else if (!Parent.AO_TransportMode.IsEmpty)
			{
				Parent.AO_TransportModeInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_IncoTerm

		protected override void CheckAO_IncoTerm()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_IncoTerm();
				MandatoryValidation.CheckEntered(Parent.AO_IncoTermInfo);
				if (!Parent.AO_IncoTerm.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_IncoTermInfo);
				}
			}
			else if (!Parent.AO_IncoTerm.IsEmpty)
			{
				Parent.AO_IncoTermInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_JobType

		protected override void CheckAO_JobType()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_JobType();
				MandatoryValidation.CheckEntered(Parent.AO_JobTypeInfo);
				if (!Parent.AO_JobType.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_JobTypeInfo);
				}
			}
			else if (!Parent.AO_JobType.IsEmpty)
			{
				Parent.AO_JobTypeInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_OrganisationCategory
		protected override void CheckAO_OrganisationCategory()
		{
			base.CheckAO_OrganisationCategory();
			MandatoryValidation.CheckEntered(Parent.AO_OrganisationCategoryInfo);
			if (!Parent.AO_OrganisationCategory.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.AO_OrganisationCategoryInfo);
			}
		}

		#endregion

		#region AO_SplitPaymentVATOrganisation
		protected override void CheckAO_SplitPaymentVATOrganisation()
		{
			base.CheckAO_SplitPaymentVATOrganisation();
			if (Parent.AO_SplitPaymentVATOrganisation && Parent.AO_CostSellAll != AccChargeTaxOverrideLookups.Revenue)
			{
				Parent.AO_SplitPaymentVATOrganisationInfo.AddError(ErrorMessageSplitPaymentVATOrganisationIsOnlyForSellCharge);
			}
		}

		#endregion

		#region AO_Origin

		protected override void CheckAO_Origin()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_Origin();
				MandatoryValidation.CheckEntered(Parent.AO_OriginInfo);

				if (!Parent.AO_Origin.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_OriginInfo);

					if (Parent.AO_Direction == OrgConstants.ServiceDirection.Code.Export && Parent.AO_Origin != countryCode)
					{
						Parent.AO_OriginInfo.AddError(ErrorMessageOriginMustBeLocalForExport);
					}
					else if (Parent.AO_Direction == OrgConstants.ServiceDirection.Code.Import && Parent.AO_Origin == countryCode)
					{
						Parent.AO_OriginInfo.AddError(ErrorMessageOriginCannotBeLocalForImport);
					}
					else if (Parent.AO_Direction == OrgConstants.ServiceDirection.Code.Domestic && Parent.AO_Origin != countryCode)
					{
						Parent.AO_OriginInfo.AddError(ErrorMessageOriginMustBeLocalForDomestic);
					}
					else if (Parent.AO_Direction == AccChargeTaxOverride.DirectionType_Other && Parent.AO_Origin == countryCode)
					{
						Parent.AO_OriginInfo.AddError(ErrorMessageOriginCannotBeLocalForOther);
					}
				}
			}
			else if (!Parent.AO_Origin.IsEmpty)
			{
				Parent.AO_OriginInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_Destination

		protected override void CheckAO_Destination()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_Destination();
				MandatoryValidation.CheckEntered(Parent.AO_DestinationInfo);

				if (!Parent.AO_Destination.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_DestinationInfo);

					if (Parent.AO_Direction == OrgConstants.ServiceDirection.Code.Export && Parent.AO_Destination == countryCode)
					{
						Parent.AO_DestinationInfo.AddError(ErrorMessageDestinationCannotBeLocalForExport);
					}
					else if (Parent.AO_Direction == OrgConstants.ServiceDirection.Code.Import && Parent.AO_Destination != countryCode)
					{
						Parent.AO_DestinationInfo.AddError(ErrorMessageDestinationMustBeLocalForImport);
					}
					else if (Parent.AO_Direction == OrgConstants.ServiceDirection.Code.Domestic && Parent.AO_Destination != countryCode)
					{
						Parent.AO_DestinationInfo.AddError(ErrorMessageDestinationMustBeLocalForDomestic);
					}
					else if (Parent.AO_Direction == AccChargeTaxOverride.DirectionType_Other && Parent.AO_Destination == countryCode)
					{
						Parent.AO_DestinationInfo.AddError(ErrorMessageDestinationCannotBeLocalForOther);
					}
				}
			}
			else if (!Parent.AO_Destination.IsEmpty)
			{
				Parent.AO_DestinationInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_TaxRegCntry

		protected override void CheckAO_TaxRegCntryOrGroup()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_TaxRegCntryOrGroup();

				MandatoryValidation.CheckEntered(Parent.AO_TaxRegCntryOrGroupInfo);
				if (!Parent.AO_TaxRegCntryOrGroup.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_TaxRegCntryOrGroupInfo);
				}
			}
			else if (!Parent.AO_TaxRegCntryOrGroup.IsEmpty)
			{
				Parent.AO_TaxRegCntryOrGroupInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_Direction

		protected override void CheckAO_CustomsStatus()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_CustomsStatus();
				if (!Parent.AO_CustomsStatus.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_CustomsStatusInfo);
				}
			}
			else if (!Parent.AO_CustomsStatus.IsEmpty)
			{
				Parent.AO_CustomsStatusInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region AO_DefaultingRule

		protected override void CheckAO_DefaultingRule()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_DefaultingRule();
				MandatoryValidation.CheckEntered(Parent.AO_DefaultingRuleInfo);
				if (!Parent.AO_DefaultingRule.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_DefaultingRuleInfo);
				}
			}
			else if (!Parent.AO_DefaultingRule.IsEmpty)
			{
				Parent.AO_DefaultingRuleInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}

			if (!Parent.AO_DefaultingRuleInfo.HasErrors() &&
				Parent.AO_TransactionContext == Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport &&
				Parent.AO_DefaultingRule != Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount &&
				Parent.AO_DefaultingRule != Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount
				)
			{
				Parent.AO_DefaultingRuleInfo.AddError(Res.GetString("649F5F3D-AEF4-4DFC-B9AF-30F9F166E722", "Defaulting Rule must be 'ART' or 'SUM' when Transaction Context is 'INT'."));
			}
		}

		#endregion

		#region AO_TransactionContext

		bool UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport => AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected override void CheckAO_TransactionContext()
		{
			if (!IsParentChargeTypeComment)
			{
				base.CheckAO_TransactionContext();
				MandatoryValidation.CheckEntered(Parent.AO_TransactionContextInfo);
				if (!Parent.AO_TransactionContext.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_TransactionContextInfo);
				}
			}
			else if (!Parent.AO_TransactionContext.IsEmpty)
			{
				Parent.AO_TransactionContextInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}

			if (!Parent.AO_TransactionContextInfo.HasErrors() && Parent.AO_TransactionContext == Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport)
			{
				if (Parent.IsTaxFrameworkRelated)
				{
					Parent.AO_TransactionContextInfo.AddError(Res.GetString("7573d17c-533c-458b-abb8-b9b62513cc16", "'{0}' option cannot be selected for Tax Configuration Override Group", Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport));
				}
				else if (!UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport)
				{
					Parent.AO_TransactionContextInfo.AddError(Res.GetString("A1C69349-8E3B-418E-9245-6870E09B9A22", "'INT' option can only be selected when registry '{0}' is set to 'Yes'.", AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.Location()));
				}
			}
		}

		#endregion

		#region AO_DefaultingRule

		protected override void CheckAO_HomeCountryOrZone()
		{
			base.CheckAO_HomeCountryOrZone();

			ListValidation.ErrorIfInvalidCode(Parent.AO_HomeCountryOrZoneInfo);

			if (!Parent.AO_HomeCountryOrZoneInfo.HasErrors() &&
				Parent.AO_DefaultingRule == Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount &&
				Parent.AO_HomeCountryOrZone != GlbCompany.CurrentCompany.GC_RN_NKCountryCode
				)
			{
				Parent.AO_HomeCountryOrZoneInfo.AddError(Res.GetString("FA4F39CA-6906-424F-968C-C4263D29E0D3", "'FPOS/AR/AP Location' must be in the same country/region as the current login company when Defaulting Rule is 'ART'."));
			}
		}

		#endregion

		#region AO_SupplyType

		protected override void CheckAO_SupplyType()
		{
			base.CheckAO_SupplyType();

			ListValidation.ErrorIfInvalidCode(Parent.AO_SupplyTypeInfo);
		}

		#endregion

		#region AO_DebtorRole

		protected override void CheckAO_DebtorRole()
		{
			base.CheckAO_DebtorRole();

			if (!Parent.AO_DebtorRole.IsEmpty)
			{
				if (Parent.TaxOverrideGroup != null && Parent.TaxOverrideGroup.Factory.HasContext(AccTaxOverrideGroup.BusinessContext.TaxFramework))
				{
					Parent.AO_DebtorRoleInfo.AddError(ErrorMessageDebtorRoleMustBeEmpty);
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(Parent.AO_DebtorRoleInfo);
				}
			}
		}

		#endregion

		void CheckTaxIdAndTaxMessageMapping()
		{
			string[] lineTypes = null;
			switch (Parent.AO_CostSellAll)
			{
				case AccChargeTaxOverrideLookups.ALL:
					lineTypes = new[] { TransactionLineTypes.Cost, TransactionLineTypes.Revenue };
					break;
				case AccChargeTaxOverrideLookups.Cost:
					lineTypes = new[] { TransactionLineTypes.Cost };
					break;
				case AccChargeTaxOverrideLookups.Revenue:
					lineTypes = new[] { TransactionLineTypes.Revenue };
					break;
			}

			var taxIDAndTaxMessageMappingHelper = ObjectFactory.Get<ITaxIdAndTaxMessageMappingHelper>();
			var errorMessage = taxIDAndTaxMessageMappingHelper?.ValidateMappingForTaxOverride(lineTypes, Parent);
			if (errorMessage != null)
			{
				Parent.AO_A9_DefaultVATClassInfo.AddError(errorMessage);
			}
		}

		#region Implementation

		//OrgConstants.ServiceDirection.Code.Import 
		//OrgConstants.ServiceDirection.Code.Export
		//OrgConstants.ServiceDirection.Code.Domestic
		//AccChargeTaxOverride.DirectionType_Other

		public static string ErrorMessageDestinationCannotBeLocalForExport
		{
			get { return Res.GetString("827497ee-b3e1-46f3-b8d8-af9ef04bd502", "The Destination cannot be your country/region for an Export override"); }
		}
		public static string ErrorMessageDestinationMustBeLocalForImport
		{
			get { return Res.GetString("6763f6a0-d072-4ed0-b3fe-9698640a96d1", "The Destination must be your country/region for an Import override"); }
		}
		public static string ErrorMessageDestinationMustBeLocalForDomestic
		{
			get { return Res.GetString("008016b1-2032-4f40-beb4-1dd6bab34f68", "The Destination must be your country/region for a Domestic override"); }
		}
		public static string ErrorMessageDestinationCannotBeLocalForOther
		{
			get { return Res.GetString("bec9a029-cec7-4312-bfb4-70ed2a6ad6a8", "The Destination cannot be your country/region for an 'Other' override"); }
		}

		public static string ErrorMessageOriginMustBeLocalForExport
		{
			get { return Res.GetString("b5ffc540-3e29-43f5-a76d-49814bedc8cd", "The Origin must be your country/region for an Export override"); }
		}
		public static string ErrorMessageOriginCannotBeLocalForImport
		{
			get { return Res.GetString("6653b02f-34f6-4813-bb59-53af48aea316", "The Origin cannot be your country/region for an Import override"); }
		}
		public static string ErrorMessageOriginMustBeLocalForDomestic
		{
			get { return Res.GetString("a746b107-80f2-48df-b500-5ac43f45c631", "The Origin must be your country/region for a Domestic override"); }
		}
		public static string ErrorMessageOriginCannotBeLocalForOther
		{
			get { return Res.GetString("eb22ee49-c619-4b76-b97b-d0639ce9d39c", "The Origin cannot be your country/region for an 'Other' override"); }
		}

		public static string ErrorMessageCannotHaveTaxMessageIfTaxIDIsMissing
		{
			get { return Res.GetString("b889cc36-344c-4df9-9f22-e2659bff3138", "You must have a Tax ID before you can choose a Tax Message"); }
		}

		public static string ErrorMessageSplitPaymentVATOrganisationIsOnlyForSellCharge
		{
			get { return Res.GetString("e1512815-7a50-4e2e-98ad-f7c37d945912", "Split Payment VAT Organization can only be ticked when Cost/Sell is 'REV'"); }
		}
		public static string ErrorMessageDebtorRoleMustBeEmpty
		{
			get { return Res.GetString("42730DB1-7C63-47B5-82C0-59EA2CDDDA5B", "The Debtor Role must be empty on Tax Configuration Override Groups. Please delete and recreate this record."); }
		}
		#endregion
	}
}
