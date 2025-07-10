using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCodeValidation : AutoAccChargeCodeValidation
	{
		public AccChargeCodeValidation(AutoAccChargeCode parent) : base(parent)
		{
		}

		new AccChargeCode Parent
		{
			get { return (AccChargeCode)base.Parent; }
		}

		protected override void CheckAC_InputGSTVATRecoverable()
		{
			base.CheckAC_InputGSTVATRecoverable();

			if (Parent.Company != null)
			{
				if (Parent.AC_InputGSTVATRecoverable < 0M || Parent.AC_InputGSTVATRecoverable > 1M)
				{
					Parent.AC_InputGSTVATRecoverableInfo.AddError(Res.GetString("759706ec-33c0-40a8-9c43-8ed0e9f0f942", "{0} Recoverable % must be between 0 and 100.", Parent.Company.Country.ConsumptionTaxDescription));
				}
				else if (Parent.AC_Calc_InputGSTVATRecoverablePercentage != 100m && Parent.AC_ChargeType != Core.Constants.ChargeType.Overhead)
				{
					Parent.AC_InputGSTVATRecoverableInfo.AddError(Res.GetString("4f9cc2d5-4ef0-4f94-ac2b-225885d69249", "{0} Recoverable % must be 100% for all charge codes that are not Overheads.", Parent.Company.Country.ConsumptionTaxDescription));
				}
			}
		}

		#region AC_MarginPercentage

		protected override void CheckAC_MarginPercentage()
		{
			base.CheckAC_MarginPercentage();

			if (Parent.RequiredProperties(Parent.AC_ChargeType).IsValid && Parent.RequiredProperties(Parent.AC_ChargeType).MarginPercentage)
			{
				if (Parent.AC_MarginPercentage < 0.0M || Parent.AC_MarginPercentage > 100M)
				{
					Parent.AC_MarginPercentageInfo.AddError(Res.GetString("adbd6dd8-1ebd-4b75-a573-64c51807706d", "Margin Percentage must be a value between 0.01 and 100."));
				}
			}
			else
			{
				if (Parent.AC_MarginPercentage != 0M)
				{
					Parent.AC_MarginPercentageInfo.AddError(Res.GetString("968054c7-97a7-45d9-ac9b-35b3b781426f", "Margin Percentage value must be 0."));
				}
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_MarginPercentageInfo);
		}

		#endregion

		#region AC_Code

		protected override void CheckAC_Code()
		{
			base.CheckAC_Code();

			MandatoryValidation.CheckEntered(Parent.AC_CodeInfo);

			if (!Parent.AC_CodeInfo.HasErrors())
			{
				var filter = new ZQuery(AccChargeCodeSchema.AC_Code, Parent.AC_Code);
				filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, Parent.AC_GC.IsEmpty ? null : Parent.AC_GC);

				if (Parent.Factory.Exists(typeof(AccChargeCode), filter))
				{
					Parent.AC_CodeInfo.AddError(Parent.IsGlobal
						? Res.GetString("3854af23-c199-41aa-b304-82acbc1daf28", "Code must be unique")
						: Res.GetString("f4d7a536-b34e-4e74-84f0-fab1ae9e5222", "Code must be unique within a single company"));
				}

				if (Parent.AC_CodeInfo.HasChanges && IsUsedForElectronicProcessingChargeCode(isOnlyCheckForLocal: true))
				{
					Parent.AC_CodeInfo.AddError(Res.GetString("677ee422-217d-48b0-bdb3-5baa0372d3a2", "This is a system defined Charge Code used in Electronic Processing Fee management and must equal the Global Charge Code {0} as defined in 'Electronic Processing Charge Code' registry.", Parent.AC_CodeInfo.OriginalValue));
				}
			}

			if (!Parent.AC_CodeInfo.HasErrors() && (!Parent.IsInDatabase || Parent.AC_CodeInfo.HasChanges) && !Parent.IsGlobalVsLocalValidationSupressed)
			{
				if (Parent.IsGlobal)
				{
					if (!Parent.AllowCodeToMatchExisting)
					{
						var filter = new ZQuery(AccChargeCodeSchema.AC_Code, Parent.AC_Code);
						filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
						var chargeCode = Parent.Factory.LoadTop1<AccChargeCode>(filter);

						if (chargeCode != null)
						{
							Parent.AC_CodeInfo.AddError(Res.GetString("c42a11f7-bba4-4cbf-8756-a36bbe119bbc", "There is a charge code in company '{0}' with the Code '{1}'. Please choose another Code.", chargeCode.Company.GC_Name, Parent.AC_Code));
						}
					}
				}
				else
				{
					if (Parent.IsLinkedToGlobalChargeCode)
					{
						if (Env.Security.ChargeCodesLTGNew.IsAllowed)
						{
							Parent.AC_CodeInfo.AddWarning(Res.GetString("fcbc310d-c681-4d05-bf9f-f4f6d8f9bc07", "This change will link this code to the Global Charge Code '{0}'", Parent.AC_Code));
						}
						else
						{
							Parent.AC_CodeInfo.AddError(
								Res.GetString("681817a4-58f6-4a7e-9020-7f1c97f43ffc", @"This change will link this code to the Global Charge Code '{0}'. To do this you require the following security permission:

{1}", Parent.AC_Code, Env.Security.ChargeCodesLTGNew.DisplayTextPathToSecurityRight));
						}
					}

					if (Parent.IsInDatabase)
					{
						var previousGlobalChargeCode = AccChargeCode.GetGlobalChargeCodeByCode(Parent.Factory, (ZString)Parent.AC_CodeInfo.OriginalValue);

						if (previousGlobalChargeCode != null)
						{
							if (Env.Security.ChargeCodesNew.IsAllowed)
							{
								Parent.AC_CodeInfo.AddWarning(Res.GetString("a3b375b2-55ef-49ba-a22d-eabca36d12f4", "This change will remove the link from this code to the Global Charge Code '{0}'", Parent.AC_CodeInfo.OriginalValue));
							}
							else
							{
								Parent.AC_CodeInfo.AddError(
									Res.GetString("4889753a-6a33-43cb-883c-4659a7bf64b5", @"This change will remove the link from this code to the Global Charge Code '{0}'.  To do this you require the following security permission:

{1}", Parent.AC_CodeInfo.OriginalValue, Env.Security.ChargeCodesNew.DisplayTextPathToSecurityRight));
							}
						}
					}
				}
			}

			PortugalValidatorHelper.AddErrorIfChargeCodeFieldChangeIsNotAllowed(Parent.AC_CodeInfo,
					Res.GetString("32c583d3-4555-4074-ba1b-bf7400370f3b", "You cannot edit this code. At least one transaction has been posted in a Portugal Login Company in this database using this Charge Code."));
		}

		#endregion

		#region AC_Desc

		protected override void CheckAC_Desc()
		{
			base.CheckAC_Desc();
			MandatoryValidation.CheckEntered(Parent.AC_DescInfo);
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_DescInfo);
			TranslatableDataFieldAttribute.Validate(Parent.AC_DescInfo);
			PortugalValidatorHelper.AddErrorIfChargeCodeDescriptionIsNonCompliant(Parent.AC_DescInfo);
		}

		#endregion

		#region AC_LocalLanguageDescription

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected override void CheckAC_LocalLanguageDescription()
		{
			base.CheckAC_LocalLanguageDescription();

			if (!Parent.AC_LocalLanguageDescription.IsEmpty && Parent.AC_LocalLanguageDescription.EqualsIgnoringCase(Parent.AC_Desc))
			{
				Parent.AC_LocalLanguageDescriptionInfo.AddError(Res.GetString("02be3ff6-6f4f-4f7a-8b5c-8723886fb42e", "The Local Language Description should only be entered where this is DIFFERENT to the main Description. Please clear this out or enter a value different to the Description."));
			}
		}

		#endregion

		#region AC_ChargeType

		protected override void CheckAC_ChargeType()
		{
			base.CheckAC_ChargeType();

			MandatoryValidation.CheckEntered(Parent.AC_ChargeTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AC_ChargeTypeInfo);

			if (!Parent.AC_ChargeTypeInfo.HasErrors())
			{
				if (RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value == Parent.PK && Parent.AC_ChargeType != Core.Constants.ChargeType.Disbursement)
				{
					Parent.AC_ChargeTypeInfo.AddError(Res.GetString("563b5e55-3d56-4360-8688-c6d8345ad5e7", "This charge code is referenced in the registry as a customs disbursement charge code, and must have type disbursement"));
				}
				if (IsUsedForElectronicProcessingChargeCode() && Parent.AC_ChargeType != Core.Constants.ChargeType.Margin && Parent.AC_ChargeType != Core.Constants.ChargeType.Disbursement)
				{
					Parent.AC_ChargeTypeInfo.AddError(Res.GetString("1ce97fe8-41c2-4472-85ea-302502bbb3c8", "This charge code is used in Electronic Processing Fee Management and can only be set to 'MRG - Margin' or 'DSB - Disbursement'."));
				}
			}

			if (IsEnteredChargeTypeValid(Parent.AC_ChargeType))
			{
				Parent.ShouldUpdateAC_MarginPercentage = true;
				Parent.UpdateChargeTypeDependentReadOnlyInfo();
				Parent.IsPostedTransactionLineReadOnlyInfoUpdated = false;

				if (Parent.IsInDatabase && Parent.AC_ChargeType != Parent.AC_ChargeTypeInfo.OriginalValue.ToString() && (Parent.AC_ChargeType == Core.Constants.ChargeType.Comment || Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.Comment))
				{
					ValidateCommentChargeTypeDependentProperties();
				}
			}

			if (Parent.IsComment)
			{
				ValidateCommentChargeType();
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_ChargeTypeInfo);
		}

		void ValidateCommentChargeType()
		{
			if (Parent.ChargeComplianceDescriptions.Count > 0)
			{
				Parent.AC_ChargeTypeInfo.AddError(Res.GetString("A3593599-2EE7-4BF7-AE3A-FB2BB496AAFB", "Sell Compliance Description rules must be empty on Comment Charge Code."));
			}
		}

		#endregion

		#region AC_RateCalculator

		protected override void CheckAC_RateCalculator()
		{
			base.CheckAC_RateCalculator();
			ListValidation.ErrorIfInvalidCode(Parent.AC_RateCalculatorInfo);

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_RateCalculatorInfo);

			ValidateRateCalculator();
		}

		void ValidateRateCalculator()
		{
			if (Parent.AC_RateCalculator == "EXL")
			{
				Parent.AC_RateCalculatorInfo.AddError(Res.GetString("c0bf5bef-87e3-4e6a-b73a-46f8a6858627", "Exclude from Company Tariffs Calculator can't be used as Rate Calculator as such calculator only applicable to Client Rates setup."));
			}
		}

		#endregion

		#region AC_AT_GSTRate

		protected override void CheckAC_AT_GSTRate()
		{
			base.CheckAC_AT_GSTRate();

			if (!Parent.IsComment)
			{
				if (!Parent.IsGlobal &&
					(Parent.AC_GC.ToGuid() == Env.CurrentCompany.PK ? Env.CurrentCompany.IsGSTRegistered : (bool)Parent.Company.GC_IsGSTRegistered))
				{
					var taxRate = AccTaxRate.Helper.FindTaxRate(Parent.Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, Parent.Company.PK.ToGuid());
					if (taxRate != null)
					{
						MandatoryValidation.CheckEntered(Parent.AC_AT_GSTRateInfo);
					}
				}
			}
			else
			{
				if (!Parent.AC_AT_GSTRate.IsEmpty)
				{
					Parent.AC_AT_GSTRateInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
				}
			}
		}

		#endregion

		#region AC_AW_WitholdingTaxrate

		protected override void CheckAC_AW_WithholdingTaxRate()
		{
			base.CheckAC_AW_WithholdingTaxRate();

			if (Parent.IsComment && !Parent.AC_AW_WithholdingTaxRate.IsEmpty)
			{
				Parent.AC_AW_WithholdingTaxRateInfo.AddError(AccountingMasterFilesConstants.ErrorMessageForInvalidDataIfChargeTypeIsComment);
			}
		}

		#endregion

		#region ClearingAccounts

		protected override void CheckAC_AG_CostClearingAccount()
		{
			base.CheckAC_AG_CostClearingAccount();

			ListValidation.ErrorIfInvalidPK(Parent.AC_AG_CostClearingAccountInfo, clrAccErrMsg);

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AG_CostClearingAccountInfo);
			CheckGLAccounts(Parent.CostClearingAccount, Parent.AC_AG_CostClearingAccountInfo);
		}

		readonly IMultilingualString clrAccErrMsg = ResString.GetMultilingualString("7F7D4B2A-34BE-473A-B64D-D9205631BD32", "Clearing Account must be a Balance Sheet account and must allow Direct Posting.");

		protected override void CheckAC_AG_RevenueClearingAccount()
		{
			base.CheckAC_AG_RevenueClearingAccount();

			ListValidation.ErrorIfInvalidPK(Parent.AC_AG_RevenueClearingAccountInfo, clrAccErrMsg);

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AG_RevenueClearingAccountInfo);
			CheckGLAccounts(Parent.RevenueClearingAccount, Parent.AC_AG_RevenueClearingAccountInfo);
		}

		#endregion

		#region AC_AG_RevenueAccount

		protected override void CheckAC_AG_RevenueAccount()
		{
			base.CheckAC_AG_RevenueAccount();

			if (Parent.RequiredProperties(Parent.HighestChargeType).RevenueAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AC_AG_RevenueAccountInfo);
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AG_RevenueAccountInfo);
			CheckGLAccounts(Parent.RevenueAccount, Parent.AC_AG_RevenueAccountInfo);
		}

		#endregion

		#region AC_AG_CostAccount

		protected override void CheckAC_AG_CostAccount()
		{
			base.CheckAC_AG_CostAccount();

			if (Parent.RequiredProperties(Parent.HighestChargeType).CostAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AC_AG_CostAccountInfo);
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AG_CostAccountInfo);
			CheckGLAccounts(Parent.CostAccount, Parent.AC_AG_CostAccountInfo);
		}

		#endregion

		#region AC_AG_WIPAccount

		protected override void CheckAC_AG_WIPAccount()
		{
			base.CheckAC_AG_WIPAccount();

			if (Parent.RequiredProperties(Parent.HighestChargeType).WIPAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AC_AG_WIPAccountInfo);
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AG_WIPAccountInfo);
			CheckGLAccounts(Parent.WIPAccount, Parent.AC_AG_WIPAccountInfo);
		}

		#endregion

		#region AC_AG_AccrualAccount

		protected override void CheckAC_AG_AccrualAccount()
		{
			base.CheckAC_AG_AccrualAccount();

			if (Parent.RequiredProperties(Parent.HighestChargeType).AccrualAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AC_AG_AccrualAccountInfo);
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AG_AccrualAccountInfo);
			CheckGLAccounts(Parent.AccrualAccount, Parent.AC_AG_AccrualAccountInfo);
		}

		#endregion

		#region AC_AG_DisbursementSurplusAccount

		protected override void CheckAC_AG_DisbursementSurplusAccount()
		{
			base.CheckAC_AG_DisbursementSurplusAccount();
			ListValidation.ErrorIfInvalidPK(Parent.AC_AG_DisbursementSurplusAccountInfo);
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AG_DisbursementSurplusAccountInfo);
			CheckGLAccounts(Parent.DisbursementSurplusAccount, Parent.AC_AG_DisbursementSurplusAccountInfo);
			if (Parent.AC_ChargeType == Core.Constants.ChargeType.Disbursement && !Parent.AC_AG_DisbursementShortfallAccount.IsEmpty && Parent.AC_AG_DisbursementSurplusAccount.IsEmpty)
			{
				Parent.AC_AG_DisbursementSurplusAccountInfo.AddError(DisbursementShortfallAccountAndDisbursementSurplusAccountMandatoryMessage);
			}

			if (Parent.AC_AG_DisbursementSurplusAccountInfo.HasChanges && !Parent.AC_AG_DisbursementSurplusAccountInfo.HasErrors())
			{
				var accounting = ObjectFactory.Get<IAccounting>();
				if (accounting != null && accounting.GetIsExistDsbBatchByCharge(Parent.PK))
				{
					var glAccountNumber = GetOriginalGLAccountNumber(Parent.AC_AG_DisbursementSurplusAccountInfo);
					Parent.AC_AG_DisbursementSurplusAccountInfo.AddError(GetDisbursementSurplusAccountNotAllowEditMessage(glAccountNumber));
				}
			}
		}

		#endregion

		#region AC_AG_DisbursementShortfallAccount

		protected override void CheckAC_AG_DisbursementShortfallAccount()
		{
			base.CheckAC_AG_DisbursementShortfallAccount();
			ListValidation.ErrorIfInvalidPK(Parent.AC_AG_DisbursementShortfallAccountInfo);
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AG_DisbursementShortfallAccountInfo);
			CheckGLAccounts(Parent.DisbursementShortfallAccount, Parent.AC_AG_DisbursementShortfallAccountInfo);
			if (Parent.AC_ChargeType == Core.Constants.ChargeType.Disbursement && !Parent.AC_AG_DisbursementSurplusAccount.IsEmpty && Parent.AC_AG_DisbursementShortfallAccount.IsEmpty)
			{
				Parent.AC_AG_DisbursementShortfallAccountInfo.AddError(DisbursementShortfallAccountAndDisbursementSurplusAccountMandatoryMessage);
			}

			if (Parent.AC_AG_DisbursementShortfallAccountInfo.HasChanges && !Parent.AC_AG_DisbursementShortfallAccountInfo.HasErrors())
			{
				var accounting = ObjectFactory.Get<IAccounting>();
				if (accounting != null && accounting.GetIsExistDsbBatchByCharge(Parent.PK))
				{
					var glAccountNumber = GetOriginalGLAccountNumber(Parent.AC_AG_DisbursementShortfallAccountInfo);
					Parent.AC_AG_DisbursementShortfallAccountInfo.AddError(GetDisbursementShortfallAccountNotAllowEditMessage(glAccountNumber));
				}
			}
		}

		#endregion

		#region AC_ChargeGroup

		protected override void CheckAC_ChargeGroup()
		{
			base.CheckAC_ChargeGroup();

			if (!Parent.AC_ChargeGroupInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.AC_ChargeGroupInfo);
			}

			if (!Parent.AC_ChargeGroupInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.AC_ChargeGroupInfo);
			}

			ValidateAC_IsAdhocServiceCharge();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_ChargeGroupInfo);
		}

		#endregion

		#region AC_ChargeOtherGroups

		protected override void CheckAC_ChargeOtherGroups()
		{
			base.CheckAC_ChargeOtherGroups();
			MandatoryValidation.CheckEntered(Parent.AC_ChargeOtherGroupsInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AC_ChargeOtherGroupsInfo);

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_ChargeOtherGroupsInfo);
		}

		#endregion

		#region AC_ChargeSubGroup

		protected override void CheckAC_ChargeSubGroup()
		{
			base.CheckAC_ChargeSubGroup();
			if (!Parent.AC_ChargeSubGroupInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.AC_ChargeSubGroupInfo);
			}

			if (!Parent.AC_ChargeSubGroupInfo.HasErrors() && Parent.IsGlobal && !Parent.AC_ChargeSubGroup.IsEmpty)
			{
				var allCompanies = Parent.Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode));

				foreach (var company in allCompanies)
				{
					var listForCompany = ChargeCodeSubGroupList.GetList(Parent.AC_ChargeGroup, company.PK);

					if (!listForCompany.ContainsCode(Parent.AC_ChargeSubGroup))
					{
						Parent.AC_ChargeSubGroupInfo.AddError(
							Res.GetString("a88c43c6-bd41-41e1-ac22-de804921bef8", "This Service Type is not valid in company '{0}' ({1})", company.GC_Code, company.GC_Name));
					}
				}
			}

			ValidateAC_IsAdhocServiceCharge();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_ChargeSubGroupInfo);
		}

		#endregion

		#region AC_IsAdhocServiceCharge

		protected override void CheckAC_IsAdhocServiceCharge()
		{
			base.CheckAC_IsAdhocServiceCharge();

			if (Parent.AC_IsAdhocServiceCharge)
			{
				if (Parent.AC_ChargeGroup.IsEmpty || Parent.AC_ChargeSubGroup.IsEmpty)
				{
					Parent.AC_IsAdhocServiceChargeInfo.AddError(Res.GetString("cf452179-4ea3-4948-b534-6cacf3a3876a", "For a charge to be an Ad Hoc Service charge, it must have a Charge Group and Sub Group."));
				}
				else
				{
					var existingAdHocServicesQuery = new ZQuery(AccChargeCodeSchema.AC_IsAdhocServiceCharge, true);
					existingAdHocServicesQuery.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					existingAdHocServicesQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, Parent.AC_ChargeGroup);
					existingAdHocServicesQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeSubGroup, Parent.AC_ChargeSubGroup);
					if (Parent.IsGlobal)
					{
						existingAdHocServicesQuery.AddToFilter(AccChargeCodeSchema.AC_GC, null);
					}
					else
					{
						existingAdHocServicesQuery.AddToFilter(AccChargeCodeSchema.AC_GC, Parent.AC_GC);
					}

					var adHocServiceChargeCodes = Parent.Factory.Load<AccChargeCode>(existingAdHocServicesQuery);
					if (adHocServiceChargeCodes.Any())
					{
						var sb = new ZStringBuilder(System.Environment.NewLine);
						foreach (var chargeCode in adHocServiceChargeCodes)
						{
							var chargeDescription = chargeCode.IsGlobal
								? Res.GetString("9ae8816c-9ffd-4dc6-a764-693e6c49f3f1", "- Global Charge Code '{0}'", chargeCode.AC_Code)
								: Res.GetString("986594cc-963b-4773-9626-60c8e1b560c6", "- Charge Code '{0}' in Company {1}", chargeCode.AC_Code, chargeCode.Company.GC_Code);

							sb.AppendLine(chargeDescription);
						}

						var message = Res.GetString("9e6d46df-f042-44c5-9f8f-fb0ecc2aa1af", "Only one charge code can be selected as the Ad Hoc Service Charge per Company, Charge Group and Service Type combination. Please either clear this tick or all of the following:{0}", sb.ToString());
						Parent.AC_IsAdhocServiceChargeInfo.AddError(message);
					}
				}
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_IsAdhocServiceChargeInfo);
		}

		#endregion

		#region AC_IATA_ChargeCodeMap

		protected override void CheckAC_IATA_ChargeCodeMap()
		{
			base.CheckAC_IATA_ChargeCodeMap();

			ListValidation.ErrorIfInvalidCode(Parent.AC_IATA_ChargeCodeMapInfo);

			if (!Parent.AC_IATA_ChargeCodeMapInfo.HasErrors())
			{
				if (!Parent.AC_IATA_ChargeCodeMap.IsEmpty &&
					Parent.AC_ChargeType != Core.Constants.ChargeType.Disbursement &&
					Parent.AC_ChargeType != Core.Constants.ChargeType.Margin &&
					Parent.AC_ChargeType != Core.Constants.ChargeType.Revenue &&
					Parent.AC_ChargeType != Core.Constants.ChargeType.ManualJobAccrual)
				{
					string message = Res.GetString("c1fcae23-315c-4f60-b828-260bd2f06d40", "Cannot assign IATA code for charge code with charge type '{0}'.", Parent.AC_ChargeType);
					Parent.AC_IATA_ChargeCodeMapInfo.AddError(message);
				}
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_IATA_ChargeCodeMapInfo);
		}

		#endregion

		#region AC_IsActive

		protected override void CheckAC_IsActive()
		{
			base.CheckAC_IsActive();

			if (!Parent.AC_IsActiveInfo.HasErrors() && !Parent.AC_IsActive)
			{
				if (IsUsedForElectronicProcessingChargeCode(isOnlyCheckForLocal: true))
				{
					Parent.AC_IsActiveInfo.AddError(Res.GetString("0b9d35b0-4ecd-4772-9365-23b5747366d1", "This is a system defined Charge Code used in Electronic Processing Fee management and cannot be set to inactive. Please set it to active."));
				}

				bool isPKReferencedByRegistry;
				using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
				{
					isPKReferencedByRegistry = registryDataAccessor.IsPKReferencedByRegistry(Parent.PK.ToGuid());
				}

				if (isPKReferencedByRegistry)
				{
					Parent.AC_IsActiveInfo.AddError(Res.GetString("a6207faa-fb19-4343-a2b1-961bd8ac45e3", "Cannot set charge code to inactive because the following Registry items are referencing it: {0}",
						new RegistryItemSetLocator().GetFormattedListOfRegistryItemsReferencingPK(Parent.PK.ToGuid(), false, ", ")));
				}
			}

			ValidateAC_AG_CostAccount();

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_IsActiveInfo);
		}

		#endregion

		#region AC_DepartmentFilterList

		protected const string AllDepartments = "ALL";

		protected override void CheckAC_DepartmentFilterList()
		{
			base.CheckAC_DepartmentFilterList();

			MandatoryValidation.CheckEntered(Parent.AC_DepartmentFilterListInfo);
			CheckAC_DepartmentFilterListIsValid();
			CheckForWipsAndAccrualsUsingThisChargeCode();
			CheckForDepartmentChargesUsingThisChargeCode();

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_DepartmentFilterListInfo);
		}

		protected void CheckAC_DepartmentFilterListIsValid()
		{
			string departmentList = Parent.AC_DepartmentFilterList.Trim().ToUpper();

			if (!(string.IsNullOrEmpty(departmentList) || departmentList == AllDepartments))
			{
				string[] departmentArray = departmentList.Split(',');
				ArrayList invalidDepartmentList = new ArrayList();

				for (int i = 0; i < departmentArray.Length; i++)
				{
					string departmentCode = departmentArray[i].Trim();

					if (Parent.Factory.LoadFromNaturalKey(typeof(GlbDepartment), GlbDepartmentSchema.GE_Code, departmentCode) == null)
					{
						if (!invalidDepartmentList.Contains(departmentCode))
						{
							invalidDepartmentList.Add(departmentCode);
							if (!string.IsNullOrEmpty(departmentCode))
							{
								Parent.AC_DepartmentFilterListInfo.AddError(Res.GetString("c91acfd1-a925-4cb0-843a-280928d9a8d3", "{0} is not a valid Department Code.", departmentCode));
							}
						}
					}
				}

				if (invalidDepartmentList.Count > 0)
				{
					Parent.AC_DepartmentFilterListInfo.AddError(Res.GetString("c5ac9138-be64-403f-9788-c3bf28547886", "Department Filter List must be either {0} or a comma separated list of valid Department Codes.", AllDepartments));
				}
			}
		}

		protected void CheckForWipsAndAccrualsUsingThisChargeCode()
		{
			if (Parent.AC_DepartmentFilterList != AllDepartments)
			{
				List<ZString> lineTypes = new List<ZString>();
				lineTypes.Add(ZArchitecture.Core.TransactionLineTypes.WIP);
				lineTypes.Add(ZArchitecture.Core.TransactionLineTypes.Accrual);

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_GE);
				subQuery.AddToFilter(AccTransactionLinesSchema.AL_AC, SQLComparisonOperator.Equal, Parent.PK);
				subQuery.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.Equal, null);
				subQuery.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, lineTypes);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbDepartment));
				query.AddSubQuery(subQuery, JoinCondition.And);
				GlbDepartmentCollection usedDepartments = new GlbDepartmentCollection(Parent.ReadOnlyFactory, query);
				ActiveBusinessObjectCollection.RefreshAll(Parent.ReadOnlyFactory);

				ZString usedDepartmentsNotMentionedInFilter = ZString.Empty;

				foreach (GlbDepartment dept in usedDepartments)
				{
					if (!Parent.AC_DepartmentFilterList.Contains(dept.GE_Code))
					{
						if (!usedDepartmentsNotMentionedInFilter.Contains(dept.GE_Code))
						{
							usedDepartmentsNotMentionedInFilter += dept.GE_Code + " ";
						}
					}
				}

				if (!usedDepartmentsNotMentionedInFilter.IsEmpty)
				{
					ZString errorMessage = Res.GetString("8c89dd8c-430c-4a65-8b37-b444bb55ffbd", "Please include the following departments in the filter because there are outstanding WIPs or Accruals that use these Departments:") + "\r\n";
					Parent.AC_DepartmentFilterListInfo.AddError(string.Format(errorMessage + "{0}", usedDepartmentsNotMentionedInFilter));
				}
			}
		}

		protected void CheckForDepartmentChargesUsingThisChargeCode()
		{
			if (Parent.AC_DepartmentFilterList != AllDepartments)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbDeptCharges), GlbDeptChargesSchema.GD_GE);
				subQuery.AddToFilter(GlbDeptChargesSchema.GD_AC, SQLComparisonOperator.Equal, Parent.PK);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbDepartment));
				query.AddSubQuery(subQuery, JoinCondition.And);

				GlbDepartmentCollection usedDepartments = new GlbDepartmentCollection(Parent.ReadOnlyFactory, query);
				ActiveBusinessObjectCollection.RefreshAll(Parent.ReadOnlyFactory);

				ZString usedDepartmentsNotMentionedInFilter = ZString.Empty;

				foreach (GlbDepartment dept in usedDepartments)
				{
					if (!Parent.AC_DepartmentFilterList.Contains(dept.GE_Code))
					{
						if (!usedDepartmentsNotMentionedInFilter.Contains(dept.GE_Code))
						{
							usedDepartmentsNotMentionedInFilter += dept.GE_Code + " ";
						}
					}
				}

				if (!usedDepartmentsNotMentionedInFilter.IsEmpty)
				{
					ZString errorMessage = Res.GetString("b23e52d9-552c-4b75-bf9a-0923a233573d", "Please include the following departments in the filter because they are referenced as Department Charges:") + "\r\n";
					Parent.AC_DepartmentFilterListInfo.AddError(string.Format(errorMessage + "{0}", usedDepartmentsNotMentionedInFilter));
				}
			}
		}

		#endregion

		#region AC_PrintSequence

		protected override void CheckAC_PrintSequence()
		{
			base.CheckAC_PrintSequence();

			if (!Parent.AC_PrintSequenceInfo.HasErrors())
			{
				if (Parent.AC_PrintSequence < 0 || Parent.AC_PrintSequence > 999)
				{
					Parent.AC_PrintSequenceInfo.AddError(Res.GetString("8bc97cbf-c5c5-4eaa-b03d-9b21bcd36f18", "Print Sequence should be between 0 and 999"));
				}
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_PrintSequenceInfo);
		}

		#endregion

		#region AC_AllowDescriptionOvertype

		protected override void CheckAC_AllowDescriptionOvertype()
		{
			base.CheckAC_AllowDescriptionOvertype();

			if (Parent.AC_ChargeType == Core.Constants.ChargeType.Comment && !Parent.AC_AllowDescriptionOvertype)
			{
				Parent.AC_AllowDescriptionOvertypeInfo.AddError(Res.GetString("48d75f5e-8fe6-4d2b-a715-0455d6dd68a3", "You cannot untick this field when the charge type is 'Comment'"));
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AllowDescriptionOvertypeInfo);
		}

		#endregion

		#region AC_AR_ExpenseGroupInfo

		protected override void CheckAC_AR_ExpenseGroup()
		{
			base.CheckAC_AR_ExpenseGroup();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AR_ExpenseGroupInfo);
		}

		#endregion

		#region AC_AR_SalesGroupInfo

		protected override void CheckAC_AR_SalesGroup()
		{
			base.CheckAC_AR_SalesGroup();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_AR_SalesGroupInfo);
		}

		#endregion

		#region AC_ENettChargeCodeMap

		protected override void CheckAC_ENettChargeCodeMap()
		{
			base.CheckAC_ENettChargeCodeMap();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_ENettChargeCodeMapInfo);
		}

		#endregion

		#region AC_GoodsServiceType

		protected override void CheckAC_GoodsServiceType()
		{
			base.CheckAC_GoodsServiceType();
			MandatoryValidation.CheckEntered(Parent.AC_GoodsServiceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AC_GoodsServiceTypeInfo);

			if (!Parent.AC_GoodsServiceTypeInfo.HasErrors())
			{
				CheckChargeCodeVsGlobalChargeCode(c => c.AC_GoodsServiceTypeInfo);
			}
		}

		#endregion

		#region AC_GovtChargeCode

		protected override void CheckAC_GovtChargeCode()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value && !Parent.IsGlobal)
			{
				if (!Parent.IsLinkedToGlobalChargeCode)
				{
					base.CheckAC_GovtChargeCode();
					MandatoryValidation.CheckEntered(Parent.AC_GovtChargeCodeInfo);
				}
				else
				{
					if (Parent.AC_GovtChargeCodeInfo.Value.IsEmpty)
					{
						Parent.AC_GovtChargeCodeInfo.AddWarning(Res.GetString("4C18B80D-C84F-4863-8C04-D496706D40F1"
							, @"In your login company recording Government Charge Code is mandatory. Since '{0}' is linked to Global Charge Code, the mandatory validation is not enforced but you might want to set a Government Charge Code."
							, Parent.AC_Code));
					}
				}
			}
		}

		#endregion

		#region AC_EnergySourceGroup

		protected override void CheckAC_EnergySourceGroup()
		{
			base.CheckAC_EnergySourceGroup();

			if (!Parent.AC_EnergySourceGroupInfo.HasErrors())
			{
				CheckChargeCodeVsGlobalChargeCode(c => c.AC_EnergySourceGroupInfo);
			}
		}

		#endregion

		#region AC_IsCommissionable

		protected override void CheckAC_IsCommissionable()
		{
			base.CheckAC_IsCommissionable();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_IsCommissionableInfo);
		}

		#endregion

		#region AC_DefaultCommissionProduct

		protected override void CheckAC_DefaultCommissionProduct()
		{
			base.CheckAC_DefaultCommissionProduct();
			ListValidation.ErrorIfInvalidCode(Parent.AC_DefaultCommissionProductInfo);
			if (!Parent.AC_DefaultCommissionService.IsEmpty || !Parent.AC_DefaultCommissionSubModule.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AC_DefaultCommissionProductInfo);
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_DefaultCommissionProductInfo);
		}

		#endregion

		#region AC_DefaultCommissionService

		protected override void CheckAC_DefaultCommissionService()
		{
			base.CheckAC_DefaultCommissionService();
			ListValidation.ErrorIfInvalidCode(Parent.AC_DefaultCommissionServiceInfo);
			if (!Parent.AC_DefaultCommissionSubModule.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AC_DefaultCommissionServiceInfo);
			}

			CheckChargeCodeVsGlobalChargeCode(c => c.AC_DefaultCommissionServiceInfo);
		}

		#endregion

		#region AC_DefaultCommissionSubModule

		protected override void CheckAC_DefaultCommissionSubModule()
		{
			base.CheckAC_DefaultCommissionSubModule();
			ListValidation.ErrorIfInvalidCode(Parent.AC_DefaultCommissionSubModuleInfo);
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_DefaultCommissionSubModuleInfo);
		}

		#endregion

		#region AC_IsGroupageCharge

		protected override void CheckAC_IsGroupageCharge()
		{
			base.CheckAC_IsGroupageCharge();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_IsGroupageChargeInfo);
		}

		#endregion

		#region AC_ShowOnQuotation

		protected override void CheckAC_ShowOnQuotation()
		{
			base.CheckAC_ShowOnQuotation();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_ShowOnQuotationInfo);
		}

		#endregion

		#region AC_AC_RevenueChargeCode

		protected override void CheckAC_AC_RevenueChargeCode()
		{
			base.CheckAC_AC_RevenueChargeCode();

			if (!Parent.AC_AC_RevenueChargeCodeInfo.HasErrors())
			{
				CheckChargeCodeVsGlobalChargeCode(c => c.AC_AC_RevenueChargeCodeInfo);
			}
		}

		#endregion

		#region AC_SuppressOnQuoteIfZero

		protected override void CheckAC_SuppressOnQuoteIfZero()
		{
			base.CheckAC_SuppressOnQuoteIfZero();
			CheckChargeCodeVsGlobalChargeCode(c => c.AC_SuppressOnQuoteIfZeroInfo);
		}

		#endregion

		#region AX_TaxOverrideGroup

		protected override void CheckAC_AX_TaxOverrideGroup()
		{
			base.CheckAC_AX_TaxOverrideGroup();

			if (Parent.AC_AX_TaxOverrideGroup.IsValid)
			{
				ZQuery filter = new ZQuery(AccTaxOverrideGroupSchema.PK, SQLComparisonOperator.Equal, Parent.AC_AX_TaxOverrideGroup);
				var taxOverrideGroup = Parent.Factory.LoadTop1<AccTaxOverrideGroup>(filter);
				if (taxOverrideGroup != null && taxOverrideGroup.IsTaxFrameworkRelated)
				{
					Parent.AC_AX_TaxOverrideGroupInfo.AddError(Res.GetString("DBF96228-235E-4233-8E08-6055F90AF19E", "This is an invalid selection. Please select a valid ‘Tax Override Group’ from the list."));
				}
			}
		}
		#endregion

		#region Duplicate Tax Overrides

		public void ValidateDuplicateTaxOverrides()
		{
			bool duplicateFound = false;

			foreach (AccChargeTaxOverride @override in Parent.TaxOverrides)
			{
				foreach (AccChargeTaxOverride override2 in Parent.TaxOverrides)
				{
					override2.ClearRowNotifications();
					if (@override.PK != override2.PK && override2.IsDuplicate(@override))
					{
						override2.AddRowError(Res.GetString("7fa7f0bd-b37c-43db-81e2-f87f0c9e7ffc", "You cannot have identical tax overrides."));
						duplicateFound = true;
						break;
					}
				}
				if (duplicateFound)
				{
					break;
				}
			}
			if (!duplicateFound && Parent.TaxOverrideGroup != null)
			{
				foreach (AccChargeTaxOverride @override in Parent.TaxOverrideGroup.TaxOverrides)
				{
					foreach (AccChargeTaxOverride override2 in Parent.TaxOverrides)
					{
						override2.ClearRowNotifications();
						if (@override.PK != override2.PK && override2.IsDuplicate(@override))
						{
							override2.AddRowError(Res.GetString("A6BC0B52-E131-4f12-99A4-DE01CCBE2FC3", "You cannot have identical tax overrides with tax override group."));
							duplicateFound = true;
							break;
						}
					}
					if (duplicateFound)
					{
						break;
					}
				}
			}
		}

		#endregion

		#region Duplicate Charge Type Overrides

		public void ValidateDuplicateChargeTypeOverrides()
		{
			bool duplicateFound = false;

			foreach (AccChargeTypeOverride @override in Parent.ChargeTypeOverrides)
			{
				foreach (AccChargeTypeOverride override2 in Parent.ChargeTypeOverrides)
				{
					override2.ClearRowNotifications();
					if (@override.PK != override2.PK && override2.IsDuplicate(@override))
					{
						override2.AddRowError(Res.GetString("df7ec1ea-5930-43c5-a0c9-c747bcd65714", "You cannot have identical charge type overrides."));
						duplicateFound = true;
						break;
					}
				}
				if (duplicateFound)
				{
					break;
				}
			}
		}

		#endregion

		#region Duplicate Compliance Descriptions

		void ValidateDuplicateComplianceDescriptions()
		{
			var errorMessage = Res.GetString("A7C0C4C0-6BED-43B5-AFBA-456BBD9AFAD9", "The combination of Job Type, Transport Mode and Sell Supply Type must be unique.");

			ObjectFactory.Get<IJobConfigurationHelperFactory>().GetDuplicateValidationHelper().CheckDuplicates(Parent, errorMessage);
		}

		#endregion

		void CheckGLAccounts(AccGLHeader glHeader, ZPropertyInfo propertyInfo)
		{
			if (Parent.IsGlobal)
			{
				if (glHeader != null && !glHeader.AG_IsGlobal)
				{
					propertyInfo.AddError(Res.GetString("76f51018-1dc7-4946-99d3-b8e0593f8fbe", "This account must be a Global GL Account."));
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDuplicateTaxOverrides();
			ValidateDuplicateChargeTypeOverrides();
			ValidateCollectionsGlobalVsLocal();
			ValidateDuplicateComplianceDescriptions();
		}

		void ValidateCollectionsGlobalVsLocal()
		{
			if (Parent.IsGlobalVsLocalValidationSupressed || !Parent.IsInDatabase)
			{
				return;
			}

			if (Parent.IsLinkedToGlobalChargeCode)
			{
				foreach (var snapshotMap in AccChargeCode.GlobalChargeCodeSnapshotMap)
				{
					var localCurrentSnapshot = snapshotMap(Parent).GetCurrentSnapshot();
					var globalLastSnapshot = snapshotMap(Parent.GlobalChargeCode).GetLastSnapshot();
					if (!BusinessObjectCollectionCopier.AreEqual(localCurrentSnapshot, globalLastSnapshot))
					{
						foreach (BusinessObject collectionItem in snapshotMap(Parent).Collection)
						{
							collectionItem.AddRowWarning(Res.GetString("192e84bf-ecd8-4e80-9db3-98369f32f1de", "These rows are different to those set on the Global Charge Code. These values will not be updated when the Global Charge Code is changed. To rectify this, set the values to be the same as the global value."));
						}
					}
				}
			}

			if (Parent.IsGlobal)
			{
				foreach (var snapshotMap in AccChargeCode.GlobalChargeCodeSnapshotMap)
				{
					var companyNamesWithDifference = new List<ZString>();

					var globalLastSnapshot = snapshotMap(Parent).GetLastSnapshot();
					foreach (AccChargeCode localChargeCode in Parent.ChildChargeCodes)
					{
						var localCurrentSnapshot = snapshotMap(localChargeCode).GetCurrentSnapshot();
						if (!BusinessObjectCollectionCopier.AreEqual(localCurrentSnapshot, globalLastSnapshot))
						{
							companyNamesWithDifference.Add(localChargeCode.Company.GC_Name);
						}
					}

					if (companyNamesWithDifference.Count > 0)
					{
						foreach (BusinessObject collectionItem in snapshotMap(Parent).Collection)
						{
							collectionItem.AddRowWarning(Res.GetString("4dc89e0f-2689-414c-a1b5-ce11b4f3cade", @"The rows for the corresponding Charge Code in these companies will not be updated because they are different to the original values on this Global Charge Code: 

{0}

To rectify this for a company, on that Charge Codes change the rows to match this Global Charge Code.",
									ZString.Join(",", companyNamesWithDifference.ToArray())));
						}
					}
				}
			}
		}

		void ValidateCommentChargeTypeDependentProperties()
		{
			foreach (AccChargeTaxOverride taxOverride in Parent.TaxOverrides)
			{
				taxOverride.Validation.ValidateAll();
			}
			foreach (AccChargeTypeOverride typeOverride in Parent.ChargeTypeOverrides)
			{
				typeOverride.Validation.ValidateAll();
			}

			ValidateAC_AT_GSTRate();
		}

		#region IsEnteredChargeTypeValid

		bool IsEnteredChargeTypeValid(string valueToValidate)
		{
			if (Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.Comment || valueToValidate == Core.Constants.ChargeType.Comment)
			{
				return IsChargeTypeValidIfPreviousOrCurrentTypeIsComment(valueToValidate);
			}
			else if (Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.Revenue && Parent.AC_ChargeTypeInfo.OriginalValue.ToString() != valueToValidate)
			{
				return IsChargeTypeValidIfPreviousTypeIsRevenue(valueToValidate);
			}
			else if (Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.Margin)
			{
				return IsChargeTypeValidIfPreviousTypeIsMargin(valueToValidate);
			}
			else if (Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.Disbursement)
			{
				return IsChargeTypeValidIfPreviousTypeIsDisbursement(valueToValidate);
			}
			else if (Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.NonAccrual)
			{
				return IsChargeTypeValidIfPreviousTypeIsNonAccrual(valueToValidate);
			}
			else if (Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.Overhead)
			{
				return IsChargeTypeValidIfPreviousTypeIsOverhead(valueToValidate);
			}
			else if (Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.ManualJobAccrual)
			{
				return IsChargeTypeValidPreviousTypeIsManualJobAccrual(valueToValidate);
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region IsChargeTypeValidIfPreviousTypeIsRevenue

		public bool IsChargeTypeValidIfPreviousTypeIsRevenue(string valueToValidate)
		{
			if (valueToValidate != Core.Constants.ChargeType.Margin && valueToValidate != Core.Constants.ChargeType.Disbursement && valueToValidate != Core.Constants.ChargeType.ManualJobAccrual)
			{
				ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_AC, Parent.PK);
				filter.AddToFilter(AccTransactionLinesSchema.AL_GC, Parent.AC_GC);
				if (Parent.Factory.Exists(typeof(AccTransactionLines), filter))
				{
					Parent.AC_ChargeTypeInfo.AddError(Res.GetString("ac2831b8-da21-4c65-8a2a-1b701927c37a", "Previous type was REV, new charge type must be MRG, DSB or MJA because there are accrued / actual revenue lines associated with this charge code."));
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region IsChargeTypeValidIfPreviousTypeIsOverhead

		bool IsChargeTypeValidIfPreviousTypeIsOverhead(string valueToValidate)
		{
			if (valueToValidate == Core.Constants.ChargeType.Margin || valueToValidate == Core.Constants.ChargeType.Disbursement ||
				valueToValidate == Core.Constants.ChargeType.NonAccrual || valueToValidate == Core.Constants.ChargeType.Overhead ||
				valueToValidate == Core.Constants.ChargeType.ManualJobAccrual)
			{
				return true;
			}
			else
			{
				if (valueToValidate == Core.Constants.ChargeType.Revenue && IsChargeCodeUsedByLinesAlready)
				{
					Parent.AC_ChargeTypeInfo.AddError(Res.GetString("8873b494-88e4-4805-90fd-28cba840e756", "Previous type was OVR, charge type must be MRG, DSB, NON, OVR or MJA because there are actual costs related to this Charge Code"));
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		#endregion

		#region IsChargeTypeValidIfPreviousTypeIsNonAccrual

		bool IsChargeTypeValidIfPreviousTypeIsNonAccrual(string valueToValidate)
		{
			if (valueToValidate == Core.Constants.ChargeType.Margin || valueToValidate == Core.Constants.ChargeType.Disbursement ||
				valueToValidate == Core.Constants.ChargeType.NonAccrual || valueToValidate == Core.Constants.ChargeType.ManualJobAccrual)
			{
				return true;
			}
			else
			{
				if (IsChargeCodeUsedByLinesAlready)
				{
					if (IsChargeCodeUsedByLinesButThisLineTypeOnly(TransactionLineTypes.Cost))
					{
						if (valueToValidate == Core.Constants.ChargeType.Overhead)
						{
							return true;
						}
						else
						{
							Parent.AC_ChargeTypeInfo.AddError(Res.GetString("459bdc25-1f8c-4631-b0a8-ca3b35932b47", "Previous type was NON, charge type must be MRG, DSB, NON, OVR or MJA because there are actual costs associated with this Charge Code"));
							return false;
						}
					}
					else if (IsChargeCodeUsedByLinesButThisLineTypeOnly(TransactionLineTypes.Revenue))
					{
						if (valueToValidate == Core.Constants.ChargeType.Revenue)
						{
							return true;
						}
						else
						{
							Parent.AC_ChargeTypeInfo.AddError(Res.GetString("642d8b25-35e5-46ed-82ed-6688803ba432", "Previous type was NON, charge type must be MRG, DSB, NON, REV or MJA because there is actual revenue associated with this Charge Code"));
							return false;
						}
					}
					else
					{
						Parent.AC_ChargeTypeInfo.AddError(Res.GetString("19d84416-b8dd-4147-9788-20a06b659e46", "Previous type was NON, charge type must be MRG, DSB, NON or MJA because there are actual costs and revenues associated with this Charge Code"));
						return false;
					}
				}
				else
				{
					return true;
				}
			}
		}

		#endregion

		#region IsChargeTypeValidIfPreviousTypeIsDisbursement

		public bool IsChargeTypeValidIfPreviousTypeIsDisbursement(string valueToValidate)
		{
			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_AC, Parent.PK);
			filter.AddToFilter(AccTransactionLinesSchema.AL_GC, Parent.AC_GC);

			if (valueToValidate != Core.Constants.ChargeType.Disbursement && valueToValidate != Core.Constants.ChargeType.Margin && valueToValidate != Core.Constants.ChargeType.ManualJobAccrual
				&& Parent.Factory.Exists(typeof(AccTransactionLines), filter))
			{
				Parent.AC_ChargeTypeInfo.AddError(Res.GetString("c05558af-7a36-4ae6-a36a-bbd4992831c5", "Previous type was DSB and cannot be changed because there are posted charges relating to operation job(s)"));
				return false;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region IsChargeTypeValidIfPreviousTypeIsMargin

		public bool IsChargeTypeValidIfPreviousTypeIsMargin(string valueToValidate)
		{
			if (valueToValidate == Core.Constants.ChargeType.Margin || valueToValidate == Core.Constants.ChargeType.Disbursement || valueToValidate == Core.Constants.ChargeType.ManualJobAccrual)
			{
				return true;
			}
			else
			{
				if (IsChargeCodeUsedByLinesAlready)
				{
					if (IsChargeCodeUsedByLinesWithJobsAlready)
					{
						if (IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready)
						{
							Parent.AC_ChargeTypeInfo.AddError(Res.GetString("767bd99f-0be3-4d65-94f9-275549292df0", "Previous type was MRG, charge type must be MRG, DSB or MJA because there are accrued/actual costs and revenue associated with this Charge Code"));
							return false;   // restrict to MRG & DSB
						}
						else
						{
							if (valueToValidate == Core.Constants.ChargeType.Revenue)
							{
								return true;
							}
							else
							{
								Parent.AC_ChargeTypeInfo.AddError(Res.GetString("5fb5aa30-9075-4175-92fe-08ab02d7d547", "Previous type was MRG, charge type must be MRG, DSB, REV or MJA because there is actual revenue associated with this Charge Code"));
								return false;
							}
						}
					}
					else    // all lines have AL_JH null 
					{
						if (valueToValidate == Core.Constants.ChargeType.NonAccrual)
						{
							return true;
						}
						else
						{
							if (IsChargeCodeUsedByLinesButThisLineTypeOnly(TransactionLineTypes.Cost)) // all transaction lines are of type cost
							{
								if (valueToValidate == Core.Constants.ChargeType.Overhead)
								{
									return true;
								}
								else
								{
									Parent.AC_ChargeTypeInfo.AddError(Res.GetString("7d83fa97-98d5-4f36-9852-cac31bc078be", "Previous type was MRG, charge type must be MRG, DSB, NON, OVR or MJA because there are actual costs associated with this Charge Code"));
									return false;   // restrict to MRG, DSB, NON
								}
							}
							else
							{
								Parent.AC_ChargeTypeInfo.AddError(Res.GetString("1ec11049-8681-42aa-baa9-99c527d984ff", "Previous type was MRG, type must be MRG, DSB, NON or MJA because there are actual/accrued costs or revenue associated with this Charge Code"));
								return false;   // restrict to MRG, DSB, NON
							}
						}
					}
				}
				else    // there are no lines referencing the current charge code
				{
					return true;
				}
			}
		}

		#endregion

		#region IsChargeTypeValidPreviousTypeIsComment

		public bool IsChargeTypeValidIfPreviousOrCurrentTypeIsComment(string valueToValidate)
		{
			if (valueToValidate == Core.Constants.ChargeType.Comment && Parent.AC_ChargeTypeInfo.OriginalValue.ToString() != Core.Constants.ChargeType.Comment && !string.IsNullOrEmpty(Parent.AC_ChargeTypeInfo.OriginalValue.ToString()))
			{
				Parent.AC_ChargeTypeInfo.AddError(Res.GetString("99a6e6c0-c282-4d6c-9fc0-4d11f82ea145", "You Cannot change existing charge type to Comment Charge Type"));
				return false;
			}
			else if (valueToValidate != Core.Constants.ChargeType.Comment && Parent.AC_ChargeTypeInfo.OriginalValue.ToString() == Core.Constants.ChargeType.Comment)
			{
				Parent.AC_ChargeTypeInfo.AddError(Res.GetString("d4d28393-321a-435f-a0a9-07b9014b9885", "Comment Charge Type cannot be change to other Type"));
				return false;
			}

			return true;
		}

		#endregion

		#region IsChargeTypeValidPreviousTypeIsManualJobAccrual

		public bool IsChargeTypeValidPreviousTypeIsManualJobAccrual(string valueToValidate)
		{
			if (valueToValidate == Core.Constants.ChargeType.Margin || valueToValidate == Core.Constants.ChargeType.Disbursement || valueToValidate == Core.Constants.ChargeType.ManualJobAccrual)
			{
				return true;
			}
			else
			{
				if (IsChargeCodeUsedByLinesAlready)
				{
					if (IsChargeCodeUsedByLinesWithJobsAlready)
					{
						if (IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready)
						{
							Parent.AC_ChargeTypeInfo.AddError(Res.GetString("a44d4c4f-34f4-4fdb-84ca-9ea6faad3eb9", "Previous type was MJA, charge type must be MRG, DSB or MJA because there are accrued/actual costs and revenue associated with this Charge Code"));
							return false;   // restrict to MRG & DSB
						}
						else
						{
							if (valueToValidate == Core.Constants.ChargeType.Revenue)
							{
								return true;
							}
							else
							{
								Parent.AC_ChargeTypeInfo.AddError(Res.GetString("cfbbb5a1-d0f8-4caf-8e99-d8b51fa5e98e", "Previous type was MJA, charge type must be MRG, DSB, REV or MJA because there is actual revenue associated with this Charge Code"));
								return false;
							}
						}
					}
					else    // all lines have AL_JH null 
					{
						if (valueToValidate == Core.Constants.ChargeType.NonAccrual)
						{
							return true;
						}
						else
						{
							if (IsChargeCodeUsedByLinesButThisLineTypeOnly(TransactionLineTypes.Cost)) // all transaction lines are of type cost
							{
								if (valueToValidate == Core.Constants.ChargeType.Overhead)
								{
									return true;
								}
								else
								{
									Parent.AC_ChargeTypeInfo.AddError(Res.GetString("cc0f5c61-d4fc-4520-8fd7-58f463196539", "Previous type was MJA, charge type must be MRG, DSB, NON, OVR or MJA because there are actual costs associated with this Charge Code"));
									return false;   // restrict to MRG, DSB, NON
								}
							}
							else
							{
								Parent.AC_ChargeTypeInfo.AddError(Res.GetString("efc2b11b-dc79-41b4-a811-327e87a9c469", "Previous type was MJA, type must be MRG, DSB, NON or MJA because there are actual/accrued costs or revenue associated with this Charge Code"));
								return false;   // restrict to MRG, DSB, NON
							}
						}
					}
				}
				else    // there are no lines referencing the current charge code
				{
					return true;
				}
			}
		}

		bool IsChargeCodeUsedByLinesAlready
		{
			get
			{
				var filter = new ZQuery(AccTransactionLinesSchema.AL_AC, Parent.PK);
				filter.AddToFilter(AccTransactionLinesSchema.AL_GC, Parent.AC_GC);
				return Parent.Factory.Exists(typeof(AccTransactionLines), filter);
			}
		}

		bool IsChargeCodeUsedByLinesWithJobsAlready
		{
			get
			{
				var jobHeaderFilter = new ZQuery(AccTransactionLinesSchema.AL_AC, Parent.PK);
				jobHeaderFilter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.NotEqual, null);
				jobHeaderFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, Parent.AC_GC);
				return Parent.Factory.Exists(typeof(AccTransactionLines), jobHeaderFilter);
			}
		}

		bool IsChargeCodeUsedByWipCstAcrLinesWithJobsAlready
		{
			get
			{
				var jobHeaderFilter = new ZQuery(AccTransactionLinesSchema.AL_AC, Parent.PK);
				jobHeaderFilter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.NotEqual, null);
				jobHeaderFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, Parent.AC_GC);
				jobHeaderFilter.AddToFilter(AccTransactionLinesSchema.AL_LineType, new string[] { TransactionLineTypes.WIP, TransactionLineTypes.Cost, TransactionLineTypes.Accrual });
				return Parent.Factory.Exists(typeof(AccTransactionLines), jobHeaderFilter);
			}
		}

		bool IsChargeCodeUsedByLinesButThisLineTypeOnly(string lineType)
		{
			if (IsChargeCodeUsedByLinesAlready)
			{
				var filter = new ZQuery(AccTransactionLinesSchema.AL_AC, Parent.PK);
				filter.AddToFilter(AccTransactionLinesSchema.AL_GC, Parent.AC_GC);
				filter.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.NotEqual, lineType);
				return !Parent.Factory.Exists(typeof(AccTransactionLines), filter);
			}
			return false;
		}

		#endregion

		#region CheckDifferentToGlobalChargeCode Helper Method

		AccChargeCodeReadableNameHelper readableNameHelper;

		AccChargeCodeReadableNameHelper ReadableNameHelper
		{
			get { return readableNameHelper ?? (readableNameHelper = new AccChargeCodeReadableNameHelper(Parent.Factory, true)); }
		}

		void CheckChargeCodeVsGlobalChargeCode(Func<AccChargeCode, ZPropertyInfo> fieldToCompare)
		{
			if (!(fieldToCompare(Parent).HasChanges || Parent.ShowDifferenceWarnings) || !Parent.IsInDatabase || Parent.IsGlobalVsLocalValidationSupressed)
			{
				return;
			}

			if (Parent.IsLinkedToGlobalChargeCode)
			{
				var globalChargeCodeInfo = fieldToCompare(Parent.GlobalChargeCode);
				var localChargeCodeFieldInfo = fieldToCompare(Parent);
				if (!localChargeCodeFieldInfo.Value.Equals(globalChargeCodeInfo.OriginalValue))
				{
					localChargeCodeFieldInfo.AddWarning(
						Res.GetString("248c8059-583c-458d-ab79-1d439a8abe39", "This has a different value to that set on the Global Charge Code. This field will not be updated when the Global Charge Code is changed. To rectify this, set the value to be the same as the global value, i.e. '{0}'",
						ReadableNameHelper.GetReadableName(globalChargeCodeInfo, true)));
				}
			}

			if (Parent.IsGlobal)
			{
				var companyNamesWithDifference = new List<ZString>();
				var globalChargeCodeFieldInfo = fieldToCompare(Parent);

				foreach (AccChargeCode localChargeCode in Parent.ChildChargeCodes)
				{
					var localChargeCodeFieldInfo = fieldToCompare(localChargeCode);
					if (!localChargeCodeFieldInfo.Value.Equals(globalChargeCodeFieldInfo.OriginalValue))
					{
						companyNamesWithDifference.Add(localChargeCode.Company.GC_Name);
					}
				}

				if (companyNamesWithDifference.Count > 0)
				{
					globalChargeCodeFieldInfo.AddWarning(Res.GetString("248c8059-583c-458d-ab79-1d439a8abe40", @"The corresponding Charge Code in these companies will not be updated because it's {1} is different to the original value of the Global Charge Code: 

{0}

To rectify this for a company, on the Charge Code change {1} to match this Global Charge Code original value, i.e. '{2}'",
							ZString.Join(",", companyNamesWithDifference.ToArray()),
							globalChargeCodeFieldInfo.HumanReadableName,
							ReadableNameHelper.GetReadableName(globalChargeCodeFieldInfo, true)));
				}
			}
		}

		#endregion

		#region Messages and Constants

		string DisbursementShortfallAccountAndDisbursementSurplusAccountMandatoryMessage => Res.GetString("3e5d4d3e-7ed8-49fb-94d7-5354a76447c8", "This GL Account must be specified if either Disbursement Surplus/Shortfall GL Account has been specified.");
		string GetDisbursementShortfallAccountNotAllowEditMessage(string glHeaderNum) => Res.GetString("9176C030-3CE7-4ba4-82A2-9E5E2D28D7F3", "Cannot edit Disbursement Shortfall GL Account because relative DSB Job Close batch was created. The DSB Shortfall Account must be set to '{0}'.", glHeaderNum);
		string GetDisbursementSurplusAccountNotAllowEditMessage(string glHeaderNum) => Res.GetString("003883E7-CD66-4a99-9726-5CF7582F56DF", "Cannot edit Disbursement Surplus GL Account because relative DSB Job Close batch was created. The DSB Surplus Account must be set to '{0}'.", glHeaderNum);

		#endregion

		string GetOriginalGLAccountNumber(ZPropertyInfo glAccountInfo)
		{
			string originalGLAccountNumber = null;

			var originalGLHeaderPK = glAccountInfo.OriginalValue as ZGuid?;
			if (originalGLHeaderPK.HasValue && !originalGLHeaderPK.Value.IsEmpty)
			{
				originalGLAccountNumber = Parent.Factory.Load<AccGLHeader>(originalGLHeaderPK.Value)?.AccountNum;
			}

			return originalGLAccountNumber;
		}

		bool IsUsedForElectronicProcessingChargeCode(bool isOnlyCheckForLocal = false)
		{
			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			var globalChargeCodePk = accountingRegistryProvider.ElectronicProcessingChargeCode;

			if (globalChargeCodePk != ZGuid.Empty && (isOnlyCheckForLocal || !Parent.IsGlobal))
			{
				var accChargeCode = Parent.Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, globalChargeCodePk));
				return Parent.AC_CodeInfo.OriginalValue?.ToString() == accChargeCode?.AC_Code.ToString();
			}

			return Parent.PK == globalChargeCodePk;
		}

		#region AccChargeCodeReadableNameHelper

		public class AccChargeCodeReadableNameHelper
		{
			readonly BusinessObjectFactory factory;
			readonly bool presentEmptyStringAsWord;

			public AccChargeCodeReadableNameHelper(BusinessObjectFactory factory, bool presentEmptyStringAsWord = false)
			{
				this.factory = factory;
				this.presentEmptyStringAsWord = presentEmptyStringAsWord;
			}

			public string GetReadableName(ZPropertyInfo propertyInfo, bool useOriginalValue)
			{
				var valueToUse = useOriginalValue ? propertyInfo.OriginalValue : propertyInfo.Value;
				var result = "";

				if (propertyInfo.PropertyType == typeof(ZGuid))
				{
					result = GetReadableNameFromGUID((ZGuid)valueToUse, propertyInfo.Name);
				}
				else
				{
					result = valueToUse.ToString();
				}

				if (presentEmptyStringAsWord && string.IsNullOrEmpty(result))
				{
					result = Res.GetString("659aa281-0b38-4f15-a281-6e35234e48fb", "Empty");
				}

				return result;
			}

			string GetReadableNameFromGUID(ZGuid zValue, string propName)
			{
				ZString readableName = ZString.Empty;

				if (propName.Contains(AccGroupsSchema.Constants.Prefix))
				{
					AccGroups group = factory.Load<AccGroups>(zValue);
					readableName = group != null ? group.AR_Code : ZString.Empty;
				}
				else if (propName.Contains(AccGLHeaderSchema.Constants.Prefix))
				{
					AccGLHeader glHeader = factory.Load<AccGLHeader>(zValue);
					readableName = glHeader != null ? (ZString)glHeader.AG_AccountNum.ToString() : ZString.Empty;
				}
				else if (propName.Contains(AccTaxRateSchema.Constants.Prefix))
				{
					AccTaxRate taxRate = factory.Load<AccTaxRate>(zValue);
					readableName = taxRate != null ? taxRate.AT_Code : ZString.Empty;
				}
				else if (propName.Contains(AccTaxOverrideGroupSchema.Constants.Prefix))
				{
					AccTaxOverrideGroup group = factory.Load<AccTaxOverrideGroup>(zValue);
					readableName = group != null ? group.AX_Code : ZString.Empty;
				}
				return readableName;
			}
		}

		#endregion
	}
}
