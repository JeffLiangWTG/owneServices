using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgMiscServValidation : AutoOrgMiscServValidation
	{
		public OrgMiscServValidation(AutoOrgMiscServ parent)
			: base(parent)
		{
		}

		public new OrgMiscServ Parent
		{
			get { return (OrgMiscServ)base.Parent; }
		}

		#region Warehouse

		#region ABCAnalysis

		#region CheckOM_WhsABCAnalysisMethod

		protected override void CheckOM_WhsABCAnalysisMethod()
		{
			MandatoryValidation.CheckEntered(Parent.OM_WhsABCAnalysisMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OM_WhsABCAnalysisMethodInfo);
		}

		#endregion

		#region CheckOM_WhsABCAnalysisPeriod

		protected override void CheckOM_WhsABCAnalysisPeriod()
		{
			MandatoryValidation.CheckEntered(Parent.OM_WhsABCAnalysisPeriodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OM_WhsABCAnalysisPeriodInfo);
		}

		#endregion

		#endregion

		#region CheckOM_IMDefaultWarehousePickOption

		protected override void CheckOM_IMDefaultWarehousePickOption()
		{
			base.CheckOM_IMDefaultWarehousePickOption();

			ListValidation.ErrorIfInvalidCode(Parent.OM_IMDefaultWarehousePickOptionInfo);
			if (!Parent.OM_IMDefaultWarehousePickOptionInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.OM_IMDefaultWarehousePickOptionInfo);
			}
		}

		#endregion

		#region CheckOM_WhsOrderFulfillmentRule

		protected override void CheckOM_WhsOrderFulfillmentRule()
		{
			base.CheckOM_WhsOrderFulfillmentRule();

			ListValidation.ErrorIfInvalidCode(Parent.OM_WhsOrderFulfillmentRuleInfo);
			if (!Parent.OM_WhsOrderFulfillmentRuleInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.OM_WhsOrderFulfillmentRuleInfo);
			}
		}

		#endregion

		#region CheckOM_WhsDefaultWarehousePickMode

		protected override void CheckOM_WhsDefaultWarehousePickMode()
		{
			base.CheckOM_WhsDefaultWarehousePickMode();

			ListValidation.ErrorIfInvalidCode(Parent.OM_WhsDefaultWarehousePickModeInfo);
			if (!Parent.OM_WhsDefaultWarehousePickModeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.OM_WhsDefaultWarehousePickModeInfo);
			}
		}

		#endregion

		#region CheckOM_WhsOrderDefaultPickPriority

		protected override void CheckOM_WhsOrderDefaultPickPriority()
		{
			base.CheckOM_WhsOrderDefaultPickPriority();

			if (!Parent.OM_WhsOrderDefaultPickPriorityInfo.HasErrors())
			{
				var priority = Parent.OM_WhsOrderDefaultPickPriority;

				if (priority < 0 || priority > 20)
				{
					Parent.OM_WhsOrderDefaultPickPriorityInfo.AddError(Res.GetString("CD029DA2-F973-4EC4-B147-9E09726BE403", "Pick Priority must be between 0 and 20."));
				}
			}
		}

		#endregion

		#region Warehouse Part Attribute Rules

		#region Check Attribute Names

		protected override void CheckOM_IMPartAttrib1Name()
		{
			base.CheckOM_IMPartAttrib1Name();
			PartAttributeValidation.CheckAttributeNameDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib1NameInfo, Parent.OM_IMPartAttrib1TypeInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
			PartAttributeValidation.CheckAttributeNameForDuplication(Parent.OM_IMPartAttrib1NameInfo, Parent.OM_IMPartAttrib2NameInfo);
			PartAttributeValidation.CheckAttributeNameForDuplication(Parent.OM_IMPartAttrib1NameInfo, Parent.OM_IMPartAttrib3NameInfo);
			TranslatableDataFieldAttribute.Validate(Parent.OM_IMPartAttrib1NameInfo);
		}

		protected override void CheckOM_IMPartAttrib2Name()
		{
			base.CheckOM_IMPartAttrib2Name();
			PartAttributeValidation.CheckAttributeNameDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib2NameInfo, Parent.OM_IMPartAttrib2TypeInfo, OrgPartRelationSchema.OU_UsePartAttrib2);
			PartAttributeValidation.CheckAttributeNameForDuplication(Parent.OM_IMPartAttrib2NameInfo, Parent.OM_IMPartAttrib1NameInfo);
			PartAttributeValidation.CheckAttributeNameForDuplication(Parent.OM_IMPartAttrib2NameInfo, Parent.OM_IMPartAttrib3NameInfo);
			TranslatableDataFieldAttribute.Validate(Parent.OM_IMPartAttrib2NameInfo);
		}

		protected override void CheckOM_IMPartAttrib3Name()
		{
			base.CheckOM_IMPartAttrib3Name();
			PartAttributeValidation.CheckAttributeNameDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib3NameInfo, Parent.OM_IMPartAttrib3TypeInfo, OrgPartRelationSchema.OU_UsePartAttrib3);
			PartAttributeValidation.CheckAttributeNameForDuplication(Parent.OM_IMPartAttrib3NameInfo, Parent.OM_IMPartAttrib1NameInfo);
			PartAttributeValidation.CheckAttributeNameForDuplication(Parent.OM_IMPartAttrib3NameInfo, Parent.OM_IMPartAttrib2NameInfo);
			TranslatableDataFieldAttribute.Validate(Parent.OM_IMPartAttrib3NameInfo);
		}

		#endregion

		#region Check Attribute Types

		protected override void CheckOM_IMPartAttrib1Type()
		{
			base.CheckOM_IMPartAttrib1Type();
			PartAttributeValidation.CheckAttributeTypeDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib1TypeInfo, Parent.OM_IMPartAttrib1NameInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
			PartAttributeValidation.CheckAttributeTypeIsNotUsedByExistingStock(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib1TypeInfo, OrgPartRelationSchema.OU_UsePartAttrib1);
			DuplicateBatchNumberAttributeCheck(Parent.OM_IMPartAttrib1TypeInfo, Parent.OM_IMPartAttrib2Type, Parent.OM_IMPartAttrib3Type);
		}

		protected override void CheckOM_IMPartAttrib2Type()
		{
			base.CheckOM_IMPartAttrib2Type();
			PartAttributeValidation.CheckAttributeTypeDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib2TypeInfo, Parent.OM_IMPartAttrib2NameInfo, OrgPartRelationSchema.OU_UsePartAttrib2);
			PartAttributeValidation.CheckAttributeTypeIsNotUsedByExistingStock(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib2TypeInfo, OrgPartRelationSchema.OU_UsePartAttrib2);
			DuplicateBatchNumberAttributeCheck(Parent.OM_IMPartAttrib2TypeInfo, Parent.OM_IMPartAttrib1Type, Parent.OM_IMPartAttrib3Type);
		}

		protected override void CheckOM_IMPartAttrib3Type()
		{
			base.CheckOM_IMPartAttrib3Type();
			PartAttributeValidation.CheckAttributeTypeDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib3TypeInfo, Parent.OM_IMPartAttrib3NameInfo, OrgPartRelationSchema.OU_UsePartAttrib3);
			PartAttributeValidation.CheckAttributeTypeIsNotUsedByExistingStock(Parent.Factory, Parent.Header, Parent.OM_IMPartAttrib3TypeInfo, OrgPartRelationSchema.OU_UsePartAttrib3);
			DuplicateBatchNumberAttributeCheck(Parent.OM_IMPartAttrib3TypeInfo, Parent.OM_IMPartAttrib1Type, Parent.OM_IMPartAttrib2Type);
		}

		void DuplicateBatchNumberAttributeCheck(ZPropertyInfo attributeTypeInfo, ZString attributeTypeValue1, ZString attributeTypeValue2)
		{
			var attributeType = (ZString)attributeTypeInfo.Value;
			if (attributeType == PartAttributeTypeList.Codes.JulianBatchNumber
				&& (attributeType == attributeTypeValue1 || attributeType == attributeTypeValue2))
			{
				attributeTypeInfo.AddError(ResString.GetMultilingualString("e93b8712-5bfa-4be3-83f5-0e4fde678216", "Julian Batch number Attribute type should not be duplicated."));
			}
		}

		#endregion

		#region CheckOM_IMUseExpiryDate

		protected override void CheckOM_IMUseExpiryDate()
		{
			base.CheckOM_IMUseExpiryDate();
			PartAttributeValidation.CheckExpiryDateDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMUseExpiryDateInfo);
			CheckOM_IMUseExpiryDate_IsUsedIfJulianBatchNumberIsUsed();
		}

		void CheckOM_IMUseExpiryDate_IsUsedIfJulianBatchNumberIsUsed()
		{
			if (!Parent.OM_IMUseExpiryDateInfo.HasErrors() && !Parent.OM_IMUseExpiryDate && PartAttributeManager != null &&
				(PartAttributeManager.IsPartAttributeAJulianBatchNumber(1) ||
				PartAttributeManager.IsPartAttributeAJulianBatchNumber(2) ||
				PartAttributeManager.IsPartAttributeAJulianBatchNumber(3)))
			{
				Parent.OM_IMUseExpiryDateInfo.AddError(Res.GetString("3feadae6-d42a-4a80-b86e-d80240603f1e", "'Use Expiry Date' must be enabled in conjunction with Julian Batch Numbers."));
			}
		}

		#endregion

		#region CheckOM_IMUsePackingDate

		protected override void CheckOM_IMUsePackingDate()
		{
			base.CheckOM_IMUsePackingDate();
			PartAttributeValidation.CheckPackingDateDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMUsePackingDateInfo);
		}

		#endregion

		#region CheckOM_IMUseSerialNumber

		protected override void CheckOM_IMUseSerialNumber()
		{
			base.CheckOM_IMUseSerialNumber();

			PartAttributeValidation.CheckSerialNumberDefinitionForOrganisation(Parent.Factory, Parent.Header, Parent.OM_IMUseSerialNumberInfo);
		}

		#endregion

		#region PartAttributeManager

		PartAttributeManager PartAttributeManager
		{
			get
			{
				if (partAttributeManager == null)
				{
					var header = Parent.Header;
					if (header != null)
					{
						partAttributeManager = new PartAttributeManager(Parent.Header);
					}
				}
				return partAttributeManager;
			}
		}

		PartAttributeManager partAttributeManager;

		#endregion

		#region PartAttributeValidation

		PartAttributeValidation PartAttributeValidation
		{
			get { return partAttributeValidation ?? (partAttributeValidation = new PartAttributeValidation()); }
		}

		PartAttributeValidation partAttributeValidation;

		#endregion

		#endregion

		#region CheckOM_MinimumShelfLifeAccepted

		protected override void CheckOM_MinimumShelfLifeAccepted()
		{
			base.CheckOM_MinimumShelfLifeAccepted();

			MandatoryValidation.CheckNotNegative(Parent.OM_MinimumShelfLifeAcceptedInfo);
			CheckOM_MinimumShelfLifeAccepted_CannotBeModifiedIfAnyUnfinalisedPicksWithProductWithExpiryDateExist();
			CheckOM_MinimumShelfLifeAccepted_WarningIfLessThan30Days();
		}

		#region CheckOM_MinimumShelfLifeAccepted_CannotBeModifiedIfAnyUnfinalisedPicksWithProductWithExpiryDateExist

		void CheckOM_MinimumShelfLifeAccepted_CannotBeModifiedIfAnyUnfinalisedPicksWithProductWithExpiryDateExist()
		{
			if (!Parent.OM_MinimumShelfLifeAcceptedInfo.HasErrors())
			{
				bool minimumShelfLifeIncreased = (ZShort)Parent.OM_MinimumShelfLifeAcceptedInfo.Value > (ZShort)Parent.OM_MinimumShelfLifeAcceptedInfo.OriginalValue;
				if (minimumShelfLifeIncreased && IsAnyUnfinalisedPicksWithProductWithExpiryDateExist())
				{
					Parent.OM_MinimumShelfLifeAcceptedInfo.AddError(Res.GetString("2084dea1-1937-4684-ab7f-3527cb4e704a",
					 "Minimum Shelf Life cannot be increased because there are un-finalized Picks for this Consignee that have Products with expiry dates."));
				}
			}
		}

		#region IsAnyUnfinalisedPicksWithProductWithExpiryDateExist

		bool IsAnyUnfinalisedPicksWithProductWithExpiryDateExist()
		{
			var rawSQL = @"
select TOP(1) 
	RowExists = convert(bit, NULL) 
from
	dbo.WhsPick
	join dbo.WhsDocket on WD_WP = WP_PK
	join dbo.WhsDocketLine on WE_WD = WD_PK
	join dbo.JobDocAddress on E2_ParentID = WD_PK and E2_AddressType = @ConsigneeType
	join dbo.OrgAddress on OA_PK = E2_OA_Address
	join dbo.OrgMiscServ as ConsigneeMiscServ on ConsigneeMiscServ.OM_OH = OA_OH
	join dbo.OrgPartRelation on OU_OP = WE_OP and OU_OH = WD_OH_Client and (OU_Relationship = @OwnerRelationship or OU_Relationship = @BothRelationship)
where
	ConsigneeMiscServ.OM_OH = @ConsigneePK and
	WP_PickStatus != 'FIN' and
	WP_PickStatus != 'CAN' and
	OU_UseExpiryDate = 1
";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@OwnerRelationship", OrgPartRelation.RelationshipTypes.Owner, OrgPartRelationSchema.OU_Relationship);
			sqlParams.Add("@BothRelationship", OrgPartRelation.RelationshipTypes.Both, OrgPartRelationSchema.OU_Relationship);
			sqlParams.Add("@ConsigneeType", DocAddressTypes.Codes.ConsigneeAddress, JobDocAddressSchema.E2_AddressType);
			sqlParams.Add("@ConsigneePK", Parent.OM_OH, OrgMiscServSchema.OM_OH);

			var collection = new DynamicBusinessObjectCollection(Parent.Factory);
			collection.Load(rawSQL, sqlParams);
			return collection.Any();

			#endregion

		}

		#endregion

		#region CheckOM_MinimumShelfLifeAccepted_WarningIfLessThan30Days

		void CheckOM_MinimumShelfLifeAccepted_WarningIfLessThan30Days()
		{
			var info = Parent.OM_MinimumShelfLifeAcceptedInfo;
			if (!info.HasErrors())
			{
				CheckMinimumShelfLifeRangeWarning(info);
			}
		}

		public static void CheckMinimumShelfLifeRangeWarning(ZPropertyInfo info)
		{
			var value = (ZShort)info.Value;
			if (value > 0 && value < MinimumShelfLifeWarningRange)
			{
				info.AddWarning(Res.GetString("aaa23655-094c-41f6-a0da-bae189b61cfe", "Minimum Shelf Life is less than 30 days."));
			}
		}

		const short MinimumShelfLifeWarningRange = 30;

		#endregion

		#endregion

		#region CheckOM_WCG_CartonGroup

		protected override void CheckOM_WCG_CartonGroup()
		{
			base.CheckOM_WCG_CartonGroup();
			ListValidation.ErrorIfInvalidPK(Parent.OM_WCG_CartonGroupInfo);
		}

		#endregion

		#region CheckOM_WhsPackageWeightTolerance

		protected override void CheckOM_WhsPackageWeightTolerancePercent()
		{
			base.CheckOM_WhsPackageWeightTolerancePercent();

			if (!Parent.OM_WhsPackageWeightTolerancePercentInfo.HasErrors())
			{
				var packageWeightTolerance = Parent.OM_WhsPackageWeightTolerancePercent;
				if (packageWeightTolerance < 0 || packageWeightTolerance > 100)
				{
					Parent.OM_WhsPackageWeightTolerancePercentInfo.AddError(Res.GetString("B3931BA7-C7BF-42E4-BC82-ADA5099E2842", "Please enter a valid package weight tolerance level value. Value should be from 0-100."));
				}
			}
		}

		#endregion

		#endregion

		#region Check_ContainerFillingPercentage

		#region CheckOM_ORDMinimumContainerFillingPercentage

		protected override void CheckOM_ORDMinimumContainerFillingPercentage()
		{
			base.CheckOM_ORDMinimumContainerFillingPercentage();

			if (!Parent.OM_ORDMinimumContainerFillingPercentageInfo.HasErrors())
			{
				var minimumContainerFillingPercentage = Parent.OM_ORDMinimumContainerFillingPercentage;

				if (minimumContainerFillingPercentage < 0 || minimumContainerFillingPercentage > 100)
				{
					Parent.OM_ORDMinimumContainerFillingPercentageInfo.AddError(Res.GetString(
						"49C7A1A2-A6AE-46C1-9869-C62071F38F94",
						"Please enter a valid minimum container filling percentage. Value should be from 0-100.")
					);
				}
			}
		}

		#endregion

		#region CheckOM_ORDMaximumContainerFillingPercentage

		protected override void CheckOM_ORDMaximumContainerFillingPercentage()
		{
			base.CheckOM_ORDMaximumContainerFillingPercentage();

			if (!Parent.OM_ORDMaximumContainerFillingPercentageInfo.HasErrors())
			{
				var minimumContainerFillingPercentage = Parent.OM_ORDMinimumContainerFillingPercentage;
				var maximumContainerFillingPercentage = Parent.OM_ORDMaximumContainerFillingPercentage;

				if (maximumContainerFillingPercentage < 0 || maximumContainerFillingPercentage > 100)
				{
					Parent.OM_ORDMaximumContainerFillingPercentageInfo.AddError(Res.GetString(
						"DEFE4571-FF21-4FAC-B5FA-3F10E12E80D5",
						"Please enter a valid maximum container filling percentage. Value should be from 0-100.")
					);
				}
				else if (!Parent.OM_ORDMinimumContainerFillingPercentageInfo.HasErrors() &&
					maximumContainerFillingPercentage <= minimumContainerFillingPercentage)
				{
					Parent.OM_ORDMaximumContainerFillingPercentageInfo.AddError(Res.GetString(
						"B3941CD4-DB5B-4C5D-A803-D3478D96E867",
						"Please enter a valid maximum container filling percentage. Value should be strictly greater than the minimum container filling percentage ({0}%).",
						minimumContainerFillingPercentage)
					);
				}
			}
		}

		#endregion

		#endregion

		#region Calculated Properties

		public void ValidateVoyageRecyclingPeriodCode()
		{
			ValidateCalculatedProperty(Parent.VoyageRecyclingPeriodCodeInfo);
		}

		protected virtual void CheckVoyageRecyclingPeriodCode()
		{
			MandatoryValidation.CheckEntered(Parent.VoyageRecyclingPeriodCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.VoyageRecyclingPeriodCodeInfo);
		}

		#endregion

		#region Global Credit Limit

		protected override void CheckOM_OH_ARGlobalCreditGroup()
		{
			base.CheckOM_OH_ARGlobalCreditGroup();
			if (Parent.OM_OH == Parent.OM_OH_ARGlobalCreditGroup)
			{
				Parent.OM_OH_ARGlobalCreditGroupInfo.AddError(Res.GetString("aa1fe9e5-163a-4f3e-ba65-6a6934df016b", "If this organization is the Global Credit Group, please do not specify a value."));
			}
			else if (Parent.OM_OH_ARGlobalCreditGroup.IsValid)
			{
				var globalCreditGroupParent = Parent.Factory.Load<OrgHeader>(Parent.OM_OH_ARGlobalCreditGroup);
				if (globalCreditGroupParent != null)
				{
					if (!globalCreditGroupParent.CompanyData.OB_IsDebtor)
					{
						Parent.OM_OH_ARGlobalCreditGroupInfo.AddError(Res.GetString("c2a71462-c6df-4823-a8c5-5693f650b3cd", "The selected organization is not a receivable organization."));
					}
					if (globalCreditGroupParent.MiscServ.IsMemberOfGlobalCreditGroup)
					{
						Parent.OM_OH_ARGlobalCreditGroupInfo.AddError(Res.GetString("706d8228-fac3-43ad-840a-b85f3c82a2ab", "The current organization is a Global Credit Group. Please leave this value as empty."));
					}
				}
			}
		}

		protected override void CheckOM_RX_NKARGlobalCreditCurrency()
		{
			base.CheckOM_RX_NKARGlobalCreditCurrency();
			if (Parent.OM_ARGlobalCreditLimit > 0)
			{
				MandatoryValidation.CheckEntered(Parent.OM_RX_NKARGlobalCreditCurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OM_RX_NKARGlobalCreditCurrencyInfo);
			}
		}

		protected override void CheckOM_ARGlobalCreditLimit()
		{
			base.CheckOM_ARGlobalCreditLimit();

			if (Parent.OM_ARGlobalCreditLimit < 0)
			{
				Parent.OM_ARGlobalCreditLimitInfo.AddError(Res.GetString("81238e29-f7eb-4739-85e0-e57b51c5c03e", "The Global Credit Limit must be greater than zero."));
			}
			else
			{
				var header = Parent.Header;
				var companyData = header.CompanyData;

				if (companyData.OB_IsDebtor && Parent.ARGlobalCreditApproved)
				{
					var globalCreditLimit = Parent.ARGlobalCreditLimit;
					var globalCreditCurrency = Parent.GlobalCreditCurrency;

					if (globalCreditLimit > 0 && globalCreditCurrency != null)
					{
						if (Parent.IsInvalidGlobalCreditCurrencyOrMissingExRate)
						{
							Parent.OM_ARGlobalCreditLimitInfo.AddWarning(header.InvalidGlobalCreditCurrencyOrMissingExRateMessage);
						}
						else
						{
							var localCreditLimitTotal = Parent.LocalCreditLimitTotal;
							if (localCreditLimitTotal > globalCreditLimit)
							{
								Parent.OM_ARGlobalCreditLimitInfo.AddError(Res.GetString("80142bd4-9792-483f-90e4-51d8d59b0927", @"The organization {0} Global Credit Limit must be greater than the sum of all Local Credit Limit for this organization.
	- Global Credit Limit - {1} {2}
	- Sum of all local credit limit converted to {1} = {1} {3}", header.OH_Code, globalCreditCurrency.RX_Code,
									Utilities.FormatNumberWithGroupSeparators(Math.Abs(globalCreditLimit), globalCreditCurrency.Decimals, CultureInfo.InvariantCulture),
									Utilities.FormatNumberWithGroupSeparators(Math.Abs(localCreditLimitTotal), globalCreditCurrency.Decimals, CultureInfo.InvariantCulture)));
							}
						}
					}
				}
			}
		}

		#endregion

		#region CheckOM_RS_NKEXDefaultServiceLevel

		protected override void CheckOM_RS_NKEXDefaultServiceLevel()
		{
			base.CheckOM_RS_NKEXDefaultServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.OM_RS_NKEXDefaultServiceLevelInfo);
		}

		#endregion

		#region CheckOM_RS_NKIMDefaultServiceLevel

		protected override void CheckOM_RS_NKIMDefaultServiceLevel()
		{
			base.CheckOM_RS_NKIMDefaultServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.OM_RS_NKIMDefaultServiceLevelInfo);
		}

		#endregion

		#region CheckOM_ARGlobalOnCreditHold

		protected override void CheckOM_ARGlobalOnCreditHold()
		{
			base.CheckOM_ARGlobalOnCreditHold();
			CreditOnHoldChecker.ValidateGlobalOnCreditHold(Parent);
		}

		#endregion

		#region CheckOM_ConsigneeAuthorityToLeave

		protected override void CheckOM_ConsigneeAuthorityToLeave()
		{
			base.CheckOM_ConsigneeAuthorityToLeave();
			var info = Parent.OM_ConsigneeAuthorityToLeaveInfo;
			ListValidation.ErrorIfInvalidCode(info);
			MandatoryValidation.CheckEntered(info);
		}

		#endregion

		#region CheckOM_ConsignorAuthorityToLeave

		protected override void CheckOM_ConsignorAuthorityToLeave()
		{
			base.CheckOM_ConsignorAuthorityToLeave();
			var info = Parent.OM_ConsignorAuthorityToLeaveInfo;
			ListValidation.ErrorIfInvalidCode(info);
			MandatoryValidation.CheckEntered(info);
		}

		#endregion

		#region CheckOM_IMAdvanceCargoReportingSelfFiler

		protected override void CheckOM_IMAdvanceCargoReportingSelfFiler()
		{
			base.CheckOM_IMAdvanceCargoReportingSelfFiler();

			if (Parent.OM_IMAdvanceCargoReportingSelfFilerInfo.HasChanges && !Parent.OM_IMAdvanceCargoReportingSelfFiler && IsThisOrgHasBeenSetupAsRelatedPartyUnderAnyOtherOrg(Parent.Factory, Parent.OM_OH, RelatedPartyTypeList.Codes.SelfFilerForICS2))
			{
				Parent.OM_IMAdvanceCargoReportingSelfFilerInfo.AddError(Res.GetString("E3998A01-05FB-4DEF-B803-CC20CBBD6B04", @"This organization is currently setup as the Self-Filer for other Organizations. Please verify the related party setup before taking further action.
To proceed, you can remove parties related to this self-filer by navigating to: Organization > Details > Related Parties > Parties Related To This Organization."));
			}
		}

		#endregion

		#region CheckOM_IMAirCargoReportDefaultConsignee

		protected override void CheckOM_IMAirCargoReportDefaultConsignee()
		{
			base.CheckOM_IMAirCargoReportDefaultConsignee();
			var propertyInfo = Parent.OM_IMAirCargoReportDefaultConsigneeInfo;
			ListValidation.ErrorIfInvalidCode(propertyInfo);
			MandatoryValidation.CheckEntered(propertyInfo);
		}

		#endregion

		#region CheckOM_IMSeaCargoReportDefaultConsignee

		protected override void CheckOM_IMSeaCargoReportDefaultConsignee()
		{
			base.CheckOM_IMSeaCargoReportDefaultConsignee();
			var propertyInfo = Parent.OM_IMSeaCargoReportDefaultConsigneeInfo;
			ListValidation.ErrorIfInvalidCode(propertyInfo);
			MandatoryValidation.CheckEntered(propertyInfo);
		}

		#endregion

		#region CheckOM_RH_NKCMMainImportCmdty

		protected override void CheckOM_RH_NKCMMainImportCmdty()
		{
			base.CheckOM_RH_NKCMMainImportCmdty();
			ListValidation.ErrorIfInvalidCode(Parent.OM_RH_NKCMMainImportCmdtyInfo);
		}

		#endregion

		#region CheckOM_RH_NKCMMainExportCmdty

		protected override void CheckOM_RH_NKCMMainExportCmdty()
		{
			base.CheckOM_RH_NKCMMainExportCmdty();
			ListValidation.ErrorIfInvalidCode(Parent.OM_RH_NKCMMainExportCmdtyInfo);
		}

		#endregion

		#region CheckOM_FWAdvanceCargoReportingSelfFiler

		protected override void CheckOM_FWAdvanceCargoReportingSelfFiler()
		{
			base.CheckOM_FWAdvanceCargoReportingSelfFiler();

			if (Parent.OM_FWAdvanceCargoReportingSelfFilerInfo.HasChanges && !Parent.OM_FWAdvanceCargoReportingSelfFiler && IsThisOrgHasBeenSetupAsRelatedPartyUnderAnyOtherOrg(Parent.Factory, Parent.OM_OH, RelatedPartyTypeList.Codes.SelfFilerForICS2))
			{
				Parent.OM_FWAdvanceCargoReportingSelfFilerInfo.AddError(Res.GetString("E3998A01-05FB-4DEF-B803-CC20CBBD6B04", @"This organization is currently setup as the Self-Filer for other Organizations. Please verify the related party setup before taking further action.
To proceed, you can remove parties related to this self-filer by navigating to: Organization > Details > Related Parties > Parties Related To This Organization."));
			}
		}

		bool IsThisOrgHasBeenSetupAsRelatedPartyUnderAnyOtherOrg(BusinessObjectFactory factory, ZGuid orgPK, string partyType)
		{
			var query = new ZDBOnlyQuery(typeof(OrgRelatedParty));
			query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, partyType);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, orgPK);

			return factory.LoadTop1<OrgRelatedParty>(query) != null;
		}

		#endregion

		#region CheckOM_FWAsAgentOption

		protected override void CheckOM_FWAsAgentOption()
		{
			base.CheckOM_FWAsAgentOption();
			ListValidation.ErrorIfInvalidCode(Parent.OM_FWAsAgentOptionInfo);

			if (Parent.OM_FWAsAgentOption.IsEmpty && !Parent.OM_FWAsAgentName.IsEmpty)
			{
				Parent.OM_FWAsAgentOptionInfo.AddError(Res.GetString("7DE590B9-C69E-4339-8CBC-33DA07DE5C26", "As Agent Option must have a value to save As Agent Details. Please select a value in As Agent Option."));
			}
		}

		#endregion

		#region CheckOM_FWAsAgentName

		protected override void CheckOM_FWAsAgentName()
		{
			base.CheckOM_FWAsAgentName();

			if (!Parent.OM_FWAsAgentOption.IsEmpty && Parent.OM_FWAsAgentName.IsEmpty && !Parent.OM_FWAsAgentOptionInfo.HasErrors())
			{
				Parent.OM_FWAsAgentNameInfo.AddWarning(Res.GetString("3E95D7DE-7925-4EDA-9D7F-1E9D296002CD", "Enter As Agent Details value if you would like Shipper Name on MBL, Carrier Booking/SI/etc. to read {0} {1} plus value entered in this field.", Parent.Header?.OH_FullName, Parent.Lookups.AsAgentOptions.GetDescriptionFromCode(Parent.OM_FWAsAgentOption)));
			}
		}

		#endregion

		#region OH_IsConsignee = true

		#region ValidateOM_IMDefaultINCOTerm

		protected override void CheckOM_IMDefaultINCOTerm()
		{
			base.CheckOM_IMDefaultINCOTerm();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				IncotermValidation.Instance.WarningIfExpired(Parent.OM_IMDefaultINCOTermInfo);
			}
		}

		#endregion

		#region ValidateOM_IMMinEFTAmount

		protected override void CheckOM_IMMinEFTAmount()
		{
			base.CheckOM_IMMinEFTAmount();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				if (Parent.OM_IMMinEFTAmount != 0)
				{
					CompareValidation.CheckNumberNotNegative(Parent.OM_IMMinEFTAmountInfo);
					CompareValidation.CheckNumberLessThanOtherNumber(Parent.OM_IMMinEFTAmountInfo, Parent.OM_IMMaxEFTAmountInfo);
				}
			}
		}

		#endregion

		#region ValidateOM_IMMaxEFTAmount

		protected override void CheckOM_IMMaxEFTAmount()
		{
			base.CheckOM_IMMaxEFTAmount();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				if (Parent.OM_IMMaxEFTAmount != 0)
				{
					CompareValidation.CheckNumberNotNegative(Parent.OM_IMMaxEFTAmountInfo);
					CompareValidation.CheckNumberGreaterThanOtherNumber(Parent.OM_IMMaxEFTAmountInfo, Parent.OM_IMMinEFTAmountInfo);
				}
			}
		}

		#endregion

		#region ValidateOM_IMEstDaysDeliveryAir

		protected override void CheckOM_IMEstDaysDeliveryAir()
		{
			base.CheckOM_IMEstDaysDeliveryAir();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_IMEstDaysDeliveryAirInfo);
			}
		}

		#endregion

		#region ValidateOM_IMEstDaysDeliveryLCL

		protected override void CheckOM_IMEstDaysDeliveryLCL()
		{
			base.CheckOM_IMEstDaysDeliveryLCL();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_IMEstDaysDeliveryLCLInfo);
			}
		}

		#endregion

		#region ValidateOM_IMEstDaysDeliveryFCL

		protected override void CheckOM_IMEstDaysDeliveryFCL()
		{
			base.CheckOM_IMEstDaysDeliveryFCL();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_IMEstDaysDeliveryFCLInfo);
			}
		}

		#endregion

		#region ValidateOM_IMMergeCustomsInvoiceLinesBy

		protected override void CheckOM_IMMergeCustomsInvoiceLinesBy()
		{
			base.CheckOM_IMMergeCustomsInvoiceLinesBy();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				MandatoryValidation.CheckEntered(Parent.OM_IMMergeCustomsInvoiceLinesByInfo);

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada ||
					Parent.OM_IMMergeCustomsInvoiceLinesBy != OrgConstants.MergeInvoiceLines.TariffAndMultiInvoices)
				{
					ListValidation.ErrorIfInvalidCode(Parent.OM_IMMergeCustomsInvoiceLinesByInfo);
				}
			}
		}

		#endregion

		#region ValidateOM_IMSendImportDocsTo

		protected override void CheckOM_IMSendImportDocsTo()
		{
			base.CheckOM_IMSendImportDocsTo();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_IMSendImportDocsToInfo);
			}
		}

		#endregion

		#region ValidateOM_IMSendSeaImportDocsTo

		protected override void CheckOM_IMSendSeaImportDocsTo()
		{
			base.CheckOM_IMSendSeaImportDocsTo();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_IMSendSeaImportDocsToInfo);
			}
		}

		#endregion

		#region ValidateOM_IMImporterCategory

		protected override void CheckOM_IMImporterCategory()
		{
			base.CheckOM_IMImporterCategory();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_IMImporterCategoryInfo);
			}
		}

		#endregion

		#region ValidateOM_IMAutoPopulateOwnerRefWithOrderNums

		protected override void CheckOM_IMAutoPopulateOwnerRefWithOrderNums()
		{
			base.CheckOM_IMAutoPopulateOwnerRefWithOrderNums();
			if (Parent.Header != null && Parent.Header.OH_IsConsignee)
			{
				MandatoryValidation.CheckEntered(Parent.OM_IMAutoPopulateOwnerRefWithOrderNumsInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OM_IMAutoPopulateOwnerRefWithOrderNumsInfo);
			}
		}

		#endregion

		#endregion

		#region OH_IsConsignor = true

		#region ValidateOM_EXDefaultIncoTerm

		protected override void CheckOM_EXDefaultIncoTerm()
		{
			base.CheckOM_EXDefaultIncoTerm();
			if (Parent.Header != null && Parent.Header.OH_IsConsignor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_EXDefaultIncoTermInfo);
				IncotermValidation.Instance.WarningIfExpired(Parent.OM_EXDefaultIncoTermInfo);
			}
		}

		#endregion

		#region ValidateOM_EXPreAllocPrefix

		protected override void CheckOM_EXPreAllocPrefix()
		{
			base.CheckOM_EXPreAllocPrefix();
			if (Parent.Header != null && Parent.Header.OH_IsConsignor && !Parent.OM_EXPreAllocPrefix.IsEmpty)
			{
				if (Parent.OM_EXPreAllocPrefix.Length != 3)
				{
					Parent.OM_EXPreAllocPrefixInfo.AddError(Res.GetString("8f6c1bdb-cd42-4da4-8610-485ea65d97de", "Pre-Allocation Prefix needs to be 3 characters."));
				}
				else if (!Parent.OM_EXPreAllocPrefix.IsLettersOnlyOrEmpty)
				{
					Parent.OM_EXPreAllocPrefixInfo.AddError(Res.GetString("6b399311-cdba-4c5e-b3af-903c40830fcb", "Pre-Allocation Prefix can only contain Letters."));
				}
				else if (!Parent.OM_EXPreAllocPrefixInfo.OriginalValue.IsEmpty && (ZString)Parent.OM_EXPreAllocPrefixInfo.OriginalValue != Parent.OM_EXPreAllocPrefix)
				{
					ZString prefix = "PPH" + Parent.OM_EXPreAllocPrefixInfo.OriginalValue;
					ZQuery houseBillQuery = new ZQuery(JobShipmentSchema.JS_HouseBill, SQLComparisonOperator.StartsWith, prefix);
					houseBillQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false);
					houseBillQuery.AddToFilter(JobShipmentSchema.JS_IsBooking, true);
					bool alreadyExists = Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>()), houseBillQuery);
					if (alreadyExists)
					{
						Parent.OM_EXPreAllocPrefixInfo.AddError(Res.GetString("697530b7-e275-4cee-af41-f8adb96030af", "Cannot change Pre-Allocation Prefix. Pre-Allocated Bookings still exist with this prefix."));
					}
				}
				else if (!Parent.OM_EXPreAllocPrefix.IsEmpty)
				{
					ZQuery omQuery = new ZQuery(OrgMiscServSchema.OM_EXPreAllocPrefix, Parent.OM_EXPreAllocPrefix);
					omQuery.AddToFilter(JoinCondition.And, OrgMiscServSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					bool alreadyExists = Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(OrgMiscServ)), omQuery);
					if (alreadyExists)
					{
						Parent.OM_EXPreAllocPrefixInfo.AddError(Res.GetString("d7c268ba-01a7-4725-bc70-f76722f7ce34", "The Pre-Allocation Prefix entered is not unique."));
					}
				}
			}
		}

		#endregion

		#endregion

		#region OH_IsShippingProvider = true

		protected override void CheckOM_CRCarrierCategory()
		{
			base.CheckOM_CRCarrierCategory();
			if (Parent.Header != null && Parent.Header.OH_IsShippingProvider)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CRCarrierCategoryInfo);
			}
		}

		protected override void CheckOM_RM_Airline()
		{
			base.CheckOM_RM_Airline();
			if (Parent.Header != null && Parent.IsAirline)
			{
				MandatoryValidation.CheckEntered(Parent.OM_RM_AirlineInfo);

				if (Parent.OM_RM_Airline.IsValid)
				{
					EnsureAirlineIsUniqueInThisCountry();
				}
			}
		}

		void EnsureAirlineIsUniqueInThisCountry()
		{
			if (Parent.Header != null && Parent.Header.ClosestPort != null)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.OM_OH);

				ZDBOnlySubQuery miscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				miscServSubQuery.AddToFilter(OrgMiscServSchema.OM_RM_Airline, Parent.OM_RM_Airline);
				query.AddSubQuery(miscServSubQuery, JoinCondition.And);

				ZDBOnlySubQuery portSubQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code);
				portSubQuery.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, Parent.Header.ClosestPort.RL_RN_NKCountryCode);
				query.AddSubQuery(OrgHeaderSchema.OH_RL_NKClosestPort, portSubQuery, JoinCondition.And);

				OrgHeader org = (OrgHeader)Parent.Header.ReadOnlyFactory.LoadTop1(typeof(OrgHeader), query);
				if (org != null)
				{
					Parent.OM_RM_AirlineInfo.AddError(Res.GetString("be85c3a7-2c73-46bf-9e6b-3d0273365928", "This master bill prefix has already been used on organization {0}.", org.OH_Code));
				}
			}
		}

		#endregion

		#region OH_IsMiscFreightServices = true

		protected override void CheckOM_SVServicesCategory()
		{
			base.CheckOM_SVServicesCategory();
			if (Parent.Header != null && Parent.Header.OH_IsMiscFreightServices)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_SVServicesCategoryInfo);
			}
		}

		#endregion

		#region OH_IsSalesLead = true

		protected override void CheckOM_CMOverallClientRelation()
		{
			base.CheckOM_CMOverallClientRelation();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckWithinRange(Parent.OM_CMOverallClientRelationInfo, 0, 10);
			}
		}

		protected override void CheckOM_CMClientsDesireToRemain()
		{
			base.CheckOM_CMClientsDesireToRemain();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckWithinRange(Parent.OM_CMClientsDesireToRemainInfo, 0, 10);
			}
		}

		protected override void CheckOM_CMEaseClientCanBePoached()
		{
			base.CheckOM_CMEaseClientCanBePoached();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckWithinRange(Parent.OM_CMEaseClientCanBePoachedInfo, 0, 10);
			}
		}

		protected override void CheckOM_CMAmountOfElectronicIntegration()
		{
			base.CheckOM_CMAmountOfElectronicIntegration();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckWithinRange(Parent.OM_CMAmountOfElectronicIntegrationInfo, 0, 10);
			}
		}

		protected override void CheckOM_CMWarehouseRevenue()
		{
			base.CheckOM_CMWarehouseRevenue();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CMWarehouseRevenueInfo);
			}
		}

		protected override void CheckOM_CMEstimatedProfit()
		{
			base.CheckOM_CMEstimatedProfit();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CMEstimatedProfitInfo);
			}
		}

		protected override void CheckOM_CMAcheivableClientRevenue()
		{
			base.CheckOM_CMAcheivableClientRevenue();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CMAcheivableClientRevenueInfo);
			}
		}

		protected override void CheckOM_CMPercentage()
		{
			base.CheckOM_CMPercentage();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CMPercentageInfo);
			}
		}

		protected override void CheckOM_CMTotalClientRevenue()
		{
			base.CheckOM_CMTotalClientRevenue();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CMTotalClientRevenueInfo);
			}
		}

		protected override void CheckOM_CMConsultingRevenue()
		{
			base.CheckOM_CMConsultingRevenue();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CMConsultingRevenueInfo);
			}
		}

		protected override void CheckOM_CMPaidUpCapital()
		{
			base.CheckOM_CMPaidUpCapital();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CMPaidUpCapitalInfo);
			}
		}

		protected override void CheckOM_CMNoOfEmployees()
		{
			base.CheckOM_CMNoOfEmployees();
			if (Parent.IsSalesLead)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CMNoOfEmployeesInfo);
			}
		}

		protected override void CheckOM_CMCompetitorActivity()
		{
			base.CheckOM_CMCompetitorActivity();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMCompetitorActivityInfo);
			}
		}

		protected override void CheckOM_CMGrowthOutlook()
		{
			base.CheckOM_CMGrowthOutlook();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMGrowthOutlookInfo);
			}
		}

		protected override void CheckOM_CMClientSize()
		{
			base.CheckOM_CMClientSize();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMClientSizeInfo);
			}
		}

		protected override void CheckOM_CMOverallEffectOfClientOnAirfreightCosts()
		{
			base.CheckOM_CMOverallEffectOfClientOnAirfreightCosts();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMOverallEffectOfClientOnAirfreightCostsInfo);
			}
		}

		protected override void CheckOM_CMOverallEffectOfClientOnLCLCosts()
		{
			base.CheckOM_CMOverallEffectOfClientOnLCLCosts();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMOverallEffectOfClientOnLCLCostsInfo);
			}
		}

		protected override void CheckOM_CMOverallEffectOfClientOnOtherCosts()
		{
			base.CheckOM_CMOverallEffectOfClientOnOtherCosts();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMOverallEffectOfClientOnOtherCostsInfo);
			}
		}

		protected override void CheckOM_CMOverallEffectOfClientOnTEUCosts()
		{
			base.CheckOM_CMOverallEffectOfClientOnTEUCosts();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMOverallEffectOfClientOnTEUCostsInfo);
			}
		}

		protected override void CheckOM_CMOverallEffectOfClientOnWarehousingCosts()
		{
			base.CheckOM_CMOverallEffectOfClientOnWarehousingCosts();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMOverallEffectOfClientOnWarehousingCostsInfo);
			}
		}

		protected override void CheckOM_CMIndustryVertical()
		{
			base.CheckOM_CMIndustryVertical();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.OM_CMIndustryVerticalInfo, Parent.OM_CMIndustryVertical_List, Parent.OM_CMIndustryVertical_ActiveList);
			}
		}

		protected override void CheckOM_CMPeriodOfActivity()
		{
			base.CheckOM_CMPeriodOfActivity();
			if (Parent.IsSalesLead)
			{
				ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.OM_CMPeriodOfActivityInfo, Parent.OM_CMPeriodOfActivity_List, Parent.OM_CMPeriodOfActivity_ActiveList);
			}
		}

		protected override void CheckOM_GC_CMPreferredPaymentCompany()
		{
			base.CheckOM_GC_CMPreferredPaymentCompany();
			if (Parent.IsSalesLead && !Parent.UseTransactionCompanyAsPreferredPayment)
			{
				MandatoryValidation.CheckEntered(Parent.OM_GC_CMPreferredPaymentCompanyInfo);
			}
		}

		#endregion

		#region OH_IsCompetitor = true

		protected override void CheckOM_CICapitalEmployed()
		{
			base.CheckOM_CICapitalEmployed();
			if (Parent.Header != null && Parent.Header.OH_IsCompetitor)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CICapitalEmployedInfo);
			}
		}

		protected override void CheckOM_CIProfit()
		{
			base.CheckOM_CIProfit();
			if (Parent.Header != null && Parent.Header.OH_IsCompetitor)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CIProfitInfo);
			}
		}

		protected override void CheckOM_CITurnover()
		{
			base.CheckOM_CITurnover();
			if (Parent.Header != null && Parent.Header.OH_IsCompetitor)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CITurnoverInfo);
			}
		}

		protected override void CheckOM_CIEstimatedStaffThisCountry()
		{
			base.CheckOM_CIEstimatedStaffThisCountry();
			if (Parent.Header != null && Parent.Header.OH_IsCompetitor)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CIEstimatedStaffThisCountryInfo);
			}
		}

		protected override void CheckOM_CIEstimatedStaffThisLocation()
		{
			base.CheckOM_CIEstimatedStaffThisLocation();
			if (Parent.Header != null && Parent.Header.OH_IsCompetitor)
			{
				CompareValidation.CheckNumberNotNegative(Parent.OM_CIEstimatedStaffThisLocationInfo);
			}
		}

		protected override void CheckOM_CITypeOfService()
		{
			base.CheckOM_CITypeOfService();
			if (Parent.Header != null && Parent.Header.OH_IsCompetitor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CITypeOfServiceInfo);
			}
		}

		protected override void CheckOM_CISellingStyle()
		{
			base.CheckOM_CISellingStyle();
			if (Parent.Header != null && Parent.Header.OH_IsCompetitor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_CISellingStyleInfo);
			}
		}

		#endregion

		#region OH_IsForwarder = true

		#region ValidateOM_FWIATACode

		protected override void CheckOM_FWIATACode()
		{
			base.CheckOM_FWIATACode();
			if (Parent.Header != null && Parent.Header.OH_IsForwarder)
			{
				if (!Parent.OM_FWIATACode.IsEmpty && !Regex.IsMatch(Parent.OM_FWIATACode, @"[0-9]{2}-[0-9] [0-9]{4}/[0-9]{4}"))
				{
					ZString message = Res.GetString("121b6127-ea57-43d5-b2af-a4e07ea0933e", @"The IATA Code has the incorrect format.
For CASS regions, the number should be 7 digits, followed by the 3 digit CASS code, followed by a single check digit.
(e.g: 12-3 4567/1234)
For non-CASS regions, the number should be 7 digits.
(e.g: 12-3 4567)");
					Parent.OM_FWIATACodeInfo.AddWarning(message);
				}
			}
		}

		#endregion

		#endregion

		#region OM_WhsDefaultExpiryNotificationPeriodInDays >= 0

		#region ValidateOM_WhsDefaultExpiryNotificationPeriodInDays

		protected override void CheckOM_WhsDefaultExpiryNotificationPeriodInDays()
		{
			base.CheckOM_WhsDefaultExpiryNotificationPeriodInDays();
			CompareValidation.CheckNumberNotNegative(Parent.OM_WhsDefaultExpiryNotificationPeriodInDaysInfo);
		}

		#endregion

		#endregion

		#region Custom Label Mandatory Validation

		protected CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}
		CustomLabelPropertyValidation customLabelPropertyValidation;

		protected override void CheckOM_CustomAttrib1()
		{
			base.CheckOM_CustomAttrib1();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomAttrib1Info);
		}

		protected override void CheckOM_CustomAttrib2()
		{
			base.CheckOM_CustomAttrib2();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomAttrib2Info);
		}

		protected override void CheckOM_CustomAttrib3()
		{
			base.CheckOM_CustomAttrib3();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomAttrib3Info);
		}

		protected override void CheckOM_CustomDecimal1()
		{
			base.CheckOM_CustomDecimal1();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomDecimal1Info);
		}

		protected override void CheckOM_CustomDecimal2()
		{
			base.CheckOM_CustomDecimal2();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomDecimal2Info);
		}

		protected override void CheckOM_CustomDecimal3()
		{
			base.CheckOM_CustomDecimal3();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomDecimal3Info);
		}

		protected override void CheckOM_CustomDate1()
		{
			base.CheckOM_CustomDate1();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomDate1Info);
		}

		protected override void CheckOM_CustomDate2()
		{
			base.CheckOM_CustomDate2();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomDate2Info);
		}

		protected override void CheckOM_CustomDate3()
		{
			base.CheckOM_CustomDate3();
			CustomLabelPropertyValidation.Validate(Parent.Header, Parent.OM_CustomDate3Info);
		}

		#endregion

		#region CheckOM_RX_NKEXDefCurrency

		protected override void CheckOM_RX_NKEXDefCurrency()
		{
			base.CheckOM_RX_NKEXDefCurrency();
			if (Parent.Header != null && Parent.Header.OH_IsConsignor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_RX_NKEXDefCurrencyInfo);
			}
		}

		#endregion

		#region CheckOM_RN_NKEXDefaultCntryOfOrigin

		protected override void CheckOM_RN_NKEXDefaultCntryOfOrigin()
		{
			base.CheckOM_RN_NKEXDefaultCntryOfOrigin();
			if (Parent.Header != null && Parent.Header.OH_IsConsignor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_RN_NKEXDefaultCntryOfOriginInfo);
			}
		}

		#endregion

		#region CheckOM_RX_NKFWDefCurrency

		protected override void CheckOM_RX_NKFWDefCurrency()
		{
			base.CheckOM_RX_NKFWDefCurrency();
			if (Parent.Header != null && Parent.Header.OH_IsForwarder)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_RX_NKFWDefCurrencyInfo);
			}
		}

		#endregion

		#region ValidateOM_EXDefaultInvoicePriceFromProductLastCost

		protected override void CheckOM_EXDefaultInvoicePriceFromProductLastCost()
		{
			base.CheckOM_EXDefaultInvoicePriceFromProductLastCost();
			if (Parent.Header != null && Parent.Header.OH_IsConsignor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OM_EXDefaultInvoicePriceFromProductLastCostInfo);
			}
		}

		#endregion

		#region UNDGPhoneType

		protected override void CheckOM_EXDefaultDGContactPhoneUsed()
		{
			base.CheckOM_EXDefaultDGContactPhoneUsed();
			ListValidation.ErrorIfInvalidCode(Parent.OM_EXDefaultDGContactPhoneUsedInfo, new PhoneTypeList());
			if (!Parent.OM_OC_EXDefaultDGContact.IsEmpty && Parent.OM_OC_EXDefaultDGContact.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.OM_EXDefaultDGContactPhoneUsedInfo);
				if (Parent.OM_EXDefaultDGContactPhoneUsedInfo.GetErrors().Count() == 0)
				{
					MandatoryValidation.CheckEntered(Parent.OM_EXDefaultDGContactPhoneUsedInfo, Res.GetString("7278151a-b434-46d6-a1d5-b6cebb050b29", "number on Contacts tab"));
				}
			}
			else
			{
				if (!Parent.OM_EXDefaultDGContactPhoneUsed.IsEmpty)
				{ Parent.OM_EXDefaultDGContactPhoneUsedInfo.AddError(Res.GetString("897f609c-a27d-40f0-9087-52f92270885f", "Please select a valid Contact.")); }
			}
		}

		#endregion

		#region DistanceCalculation

		protected override void CheckOM_CMDistanceCalculationProvider()
		{
			base.CheckOM_CMDistanceCalculationProvider();
			MandatoryValidation.CheckEntered(Parent.OM_CMDistanceCalculationProviderInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OM_CMDistanceCalculationProviderInfo);
		}

		protected override void CheckOM_CMDistanceCalculationVersion()
		{
			base.CheckOM_CMDistanceCalculationVersion();
			if (Parent.OM_CMDistanceCalculationProvider == DistanceCalculationConstants.Providers.PCMiler)
			{
				MandatoryValidation.CheckEntered(Parent.OM_CMDistanceCalculationVersionInfo);
			}
		}

		protected override void CheckOM_CMDistanceCalculationMethod()
		{
			base.CheckOM_CMDistanceCalculationMethod();
			if (Parent.OM_CMDistanceCalculationProvider == DistanceCalculationConstants.Providers.PCMiler)
			{
				MandatoryValidation.CheckEntered(Parent.OM_CMDistanceCalculationMethodInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OM_CMDistanceCalculationMethodInfo);
			}
		}

		#endregion

		#region OM_CMEstablishedDate

		/// <summary>
		/// Established date can be > 10 years and cannot be in the future so we don't want base validation
		/// </summary>
		protected override void CheckOM_CMEstablishedDateIsValidZDateTimeRange()
		{
			if (Parent.OM_CMEstablishedDate > ZDateTime.Today)
			{
				Parent.OM_CMEstablishedDateInfo.AddError(Res.GetString("2e4572aa-6310-41a3-9cb9-b1a7c707f908", "Cannot set to a date in the future."));
			}
		}

		#endregion

		#region OM_CIFinancialDetailsApplicableFromDate

		/// <summary>
		/// Applicable From Date can be more than one year old (i.e the data can be more than one year old), so we don't want the base validation.
		/// </summary>
		protected override void CheckOM_CIFinancialDetailsApplicableFromDateIsValidZDateRange()
		{
			if (Parent.OM_CIFinancialDetailsApplicableFromDate > ZDate.Today.AddYears(-1).AddDays(1))
			{
				Parent.OM_CIFinancialDetailsApplicableFromDateInfo.AddError(Res.GetString("ADED9479-C6CF-4328-9A6A-C1C1BD239A4F", "Applicable From Date should be greater than twelve months old."));
			}
		}

		#endregion

		#region OM_CarrierPackageGrouping

		protected override void CheckOM_CarrierPackageGrouping()
		{
			base.CheckOM_CarrierPackageGrouping();

			MandatoryValidation.CheckEntered(Parent.OM_CarrierPackageGroupingInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OM_CarrierPackageGroupingInfo);
		}

		#endregion

		#region OM_FWAgentPackageGrouping

		protected override void CheckOM_FWAgentPackageGrouping()
		{
			base.CheckOM_FWAgentPackageGrouping();

			MandatoryValidation.CheckEntered(Parent.OM_FWAgentPackageGroupingInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OM_FWAgentPackageGroupingInfo);
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateVoyageRecyclingPeriodCode();
		}

		#endregion
	}
}
