using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketValidation : AutoWhsDocketValidation
	{
		public WhsDocketValidation(AutoWhsDocket parent)
			: base(parent)
		{
		}

		#region Parent

		public new WhsDocket Parent
		{
			get { return (WhsDocket)base.Parent; }
		}

		#endregion

		#region CheckWD_PL_NKCarrierServiceLevel

		protected override void CheckWD_PL_NKCarrierServiceLevel()
		{
			base.CheckWD_PL_NKCarrierServiceLevel();
			ListValidation.WarnIfInvalidCode(Parent.WD_PL_NKCarrierServiceLevelInfo);
		}

		#endregion

		#region CheckWD_WW_Whs

		protected override void CheckWD_WW_Whs()
		{
			base.CheckWD_WW_Whs();

			MandatoryValidation.CheckEntered(Parent.WD_WW_WhsInfo);
			CheckWarehouseIsActive();
			CheckTransactionType(Parent.WD_WW_WhsInfo);
		}

		void CheckWarehouseIsActive()
		{
			if (!Parent.WD_WW_WhsInfo.HasErrors())
			{
				var warehouse = Parent.Warehouse;
				if (warehouse != null && !warehouse.WW_IsActive)
				{
					Parent.WD_WW_WhsInfo.AddError(CannotSelectInactiveWarehouse);
				}
			}
		}

		#endregion

		#region CannotSelectInactiveWarehouse

		public static string CannotSelectInactiveWarehouse
		{
			get { return Res.GetString("094f30bd-ed87-416d-bbdc-41b080c54002", "This warehouse is set to inactive and cannot be used in transactions"); }
		}

		#endregion

		#region CheckWD_DocketType

		protected override void CheckWD_DocketType()
		{
			base.CheckWD_DocketType();
			MandatoryValidation.CheckEntered(Parent.WD_DocketTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WD_DocketTypeInfo, new DocketType());
		}

		#endregion

		#region CheckWD_DocketStatus

		protected override void CheckWD_DocketStatus()
		{
			base.CheckWD_DocketStatus();
			MandatoryValidation.CheckEntered(Parent.WD_DocketStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WD_DocketStatusInfo, GetValidCodeList());
		}

		protected virtual CodeDescriptionPairList GetValidCodeList()
		{
			return new DocketStatus();
		}

		#endregion

		#region CheckWD_DocketSubType

		protected override void CheckWD_DocketSubType()
		{
			base.CheckWD_DocketSubType();

			CheckWD_DocketSubType_NotEmptyAndCorrectType();
			CheckTransactionType(Parent.WD_DocketSubTypeInfo);
		}

		#region CheckWarehouseSupportsDocketSubTypeAndIsInwardsProcessing

		protected void CheckTransactionType(ZPropertyInfo info)
		{
			if (ShouldCheckTransactionType && !info.HasErrors())
			{
				var warehouse = Parent.Warehouse;
				if (warehouse != null && !Parent.WD_DocketSubType.IsEmpty)
				{
					var isFreeStoreOrExciseWarehouse = new Lazy<bool>(() => warehouse.IsWarehouseFreeStoreEnabled || warehouse.IsWarehouseExciseEnabled);

					if (Parent.IsCustomsTransaction)
					{
						CheckTransactionTypeForCustomsJob(info, warehouse);
					}
					else if (!isFreeStoreOrExciseWarehouse.Value)
					{
						info.AddError(FreeStoreNotEnabledErrorMsg);
					}
				}
			}
		}

		void CheckTransactionTypeForCustomsJob(ZPropertyInfo info, WhsWarehouse warehouse)
		{
			if (!Parent.WD_IsInwardsProcessingJob && !warehouse.IsWarehouseBondEnabled && !warehouse.IsWarehouseExciseEnabled)
			{
				info.AddError(BondNotEnabledErrorMsg);
			}
			else if (Parent.WD_IsInwardsProcessingJob && !warehouse.IsInwardProcessingEnabled)
			{
				info.AddError(InwardProcessingNotEnabledErrorMsg);
			}
			else
			{
				var countryCode = warehouse.CountryCode;
				var isCountrySupportedForBonded = countryCode.IsEmpty || BondedHelper.IsCountrySupportedForBonded(Parent.Factory, countryCode);

				if (!isCountrySupportedForBonded)
				{
					info.AddWarning(BondNotSupportedCountryErrorMsg);
				}
			}
		}

		protected virtual bool ShouldCheckTransactionType => IsDocketSubTypeSupported;

		#endregion

		#region CheckWD_DocketSubType_NotEmptyAndCorrectType

		void CheckWD_DocketSubType_NotEmptyAndCorrectType()
		{
			if (IsDocketSubTypeSupported && !Parent.WD_DocketSubTypeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.WD_DocketSubTypeInfo);

				if (!Parent.WD_DocketSubTypeInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.WD_DocketSubTypeInfo);
				}
			}
		}

		protected virtual bool IsDocketSubTypeSupported => true;

		#endregion

		#endregion

		#region CheckWD_DropMode

		protected override void CheckWD_DropMode()
		{
			base.CheckWD_DropMode();
			ListValidation.ErrorIfInvalidCode(Parent.WD_DropModeInfo);
		}

		#endregion

		#region CheckWD_ExternalReference

		protected override void CheckWD_ExternalReference()
		{
			base.CheckWD_ExternalReference();

			if (Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.WD_ExternalReferenceInfo);
			}

			if (!Parent.IsUniqueExternalReferenceCreatedOnSave)
			{
				CheckWD_ExternalReferenceForDuplicates();
			}
		}

		#endregion

		#region CheckWD_ExternalReferenceForDuplicates

		protected void CheckWD_ExternalReferenceForDuplicates()
		{
			if (ShouldCheckForDuplicateExternalReference() && Parent.CheckExternalReferenceForDuplicates())
			{
				Parent.WD_ExternalReferenceInfo.AddError(DuplicateReferenceErrorMsg());
			}
		}

		protected virtual bool ShouldCheckForDuplicateExternalReference() => true;

		#endregion

		#region AllowDuplicateExternalReferences

		protected virtual bool AllowDuplicateExternalReferences
		{
			get { return false; }
		}

		#endregion

		#region CheckWD_RS_NKServiceLevel

		protected override void CheckWD_RS_NKServiceLevel()
		{
			base.CheckWD_RS_NKServiceLevel();
			ListValidation.WarnIfInvalidCode(Parent.WD_RS_NKServiceLevelInfo);
		}

		#endregion

		#region CheckWD_TotalUnits

		protected override void CheckWD_TotalUnits()
		{
			base.CheckWD_TotalUnits();
			if (Parent.WD_TotalUnits < 0)
			{
				Parent.WD_TotalUnitsInfo.AddError(Res.GetString("002ba739-64d0-422e-b7ea-39c835c7fdda", "Total units must be greater or equal to zero."));
			}
		}

		#endregion

		#region CheckWD_TotalWeight, CheckWD_TotalCubic

		#region CheckWD_TotalWeight

		protected override void CheckWD_TotalWeight()
		{
			base.CheckWD_TotalWeight();

			MandatoryValidation.CheckNotNegative(Parent.WD_TotalWeightInfo);
			CheckWD_TotalWeightVolume_MatchLineLevelWeightVolume(Parent.WD_TotalWeightInfo, Parent.WD_TotalWeightUnitInfo, Parent.GetWeightMeasure());
		}

		#endregion

		#region CheckWD_TotalWeightUnit

		protected override void CheckWD_TotalWeightUnit()
		{
			base.CheckWD_TotalWeightUnit();
			MandatoryValidation.CheckEntered(Parent.WD_TotalWeightUnitInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WD_TotalWeightUnitInfo, Parent.TotalWeightUnits);
		}

		#endregion

		#region CheckWD_TotalCubic

		protected override void CheckWD_TotalCubic()
		{
			base.CheckWD_TotalCubic();

			MandatoryValidation.CheckNotNegative(Parent.WD_TotalCubicInfo);
			CheckWD_TotalWeightVolume_MatchLineLevelWeightVolume(Parent.WD_TotalCubicInfo, Parent.WD_TotalCubicUnitInfo, Parent.GetVolumeMeasure());
		}

		#endregion

		#region CheckWD_TotalCubicUnit

		protected override void CheckWD_TotalCubicUnit()
		{
			base.CheckWD_TotalCubicUnit();
			MandatoryValidation.CheckEntered(Parent.WD_TotalCubicUnitInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WD_TotalCubicUnitInfo, Parent.TotalCubicUnits);
		}

		#endregion

		#region CheckWD_TotalWeightVolume_MatchLineLevelWeightVolume

		void CheckWD_TotalWeightVolume_MatchLineLevelWeightVolume(ZPropertyInfo totalWeightOrVolumeInfo, ZPropertyInfo totalWeightOrVolumeUQInfo, IUnitOfMeasure unitOfMeasure)
		{
			if (!WhsEnvironment.IsRF && !totalWeightOrVolumeInfo.HasErrors() && !totalWeightOrVolumeUQInfo.HasErrors() && Parent.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var totalFromLinesRounded = UnitOfMeasureConverter.GetTotalQuantityFromLines(Parent, unitOfMeasure, GetLinesForWeightAndVolumeCalculation().ToProductAndQuantities());
				var totalFromJobRounded = Utilities.Round((ZDecimal)totalWeightOrVolumeInfo.Value, unitOfMeasure.DecimalPlacesForRounding);

				if (totalFromJobRounded != totalFromLinesRounded)
				{
					totalWeightOrVolumeInfo.AddWarning(Res.GetString("573ed4d4-8aa7-47e1-8dfb-2ee79b33fe55",
						@"Original {0} of product calculated from the product master file was {3} {2}. This has been overridden with {1} {2}.
To fix the {0} go to Maintain > Warehouse > Products and amend the {0} for these products.",
						totalWeightOrVolumeInfo.Description, totalFromJobRounded, totalWeightOrVolumeUQInfo.Value, totalFromLinesRounded));
				}
			}
		}

		protected virtual IEnumerable<WhsDocketLine> GetLinesForWeightAndVolumeCalculation() => Parent.Lines;

		#endregion

		#endregion

		#region CheckWD_TotalPallets

		protected override void CheckWD_TotalPallets()
		{
			base.CheckWD_TotalPallets();
			if (Parent.WD_TotalPallets < 0)
			{
				Parent.WD_TotalPalletsInfo.AddError(Res.GetString("2cd13cff-6057-4c7c-915b-514919baf8a8", "Please do not enter negative pallets."));
			}
		}

		#endregion

		#region CheckWD_PackagesSent

		protected override void CheckWD_PackagesSent()
		{
			base.CheckWD_PackagesSent();
			if (Parent.WD_PackagesSent < 0)
			{
				Parent.WD_PackagesSentInfo.AddError(Res.GetString("dfe4b11d-61c0-40fe-ab30-72fbe1fa3e87", "Please do not enter negative quantity."));
			}
		}

		#endregion

		#region CheckWD_PalletsSent

		protected override void CheckWD_PalletsSent()
		{
			base.CheckWD_PalletsSent();
			if (Parent.WD_PalletsSent < 0)
			{
				Parent.WD_PalletsSentInfo.AddError(Res.GetString("dfe4b11d-61c0-40fe-ab30-72fbe1fa3e87", "Please do not enter negative quantity."));
			}
		}

		#endregion

		#region WD_PickPriority

		protected override void CheckWD_PickPriority()
		{
			base.CheckWD_PickPriority();
			if (Parent.WD_PickPriority > 20)
			{
				Parent.WD_PickPriorityInfo.AddError(Res.GetString("3BAFE33C-6C1A-43B3-B586-D1D7C4835A84", "Pick Priority should be in range 0 to 20."));
			}
		}

		#endregion

		#region CheckWD_UnitsSent

		protected override void CheckWD_UnitsSent()
		{
			base.CheckWD_UnitsSent();
			if (Parent.WD_UnitsSent < 0)
			{
				Parent.WD_UnitsSentInfo.AddError(Res.GetString("dfe4b11d-61c0-40fe-ab30-72fbe1fa3e87", "Please do not enter negative quantity."));
			}

			if (Parent.WD_UnitsSent > new ZDecimal(int.MaxValue))
			{
				Parent.WD_UnitsSentInfo.AddWarning(Res.GetString("4291C665-5A16-49A7-8632-B314720E6F91", "Please do not enter a quantity greater than {0} it will not export.", int.MaxValue));
			}
		}

		#endregion

		#region CheckWD_WeightSentUserEntered

		protected override void CheckWD_WeightSentUserEntered()
		{
			base.CheckWD_WeightSentUserEntered();
			if (Parent.WD_WeightSentUserEntered < 0m)
			{
				Parent.WD_WeightSentUserEnteredInfo.AddError(Res.GetString("684c969d-4437-401c-a41a-2137130762d9", "Please do not enter negative weight."));
			}
		}

		#endregion

		#region CheckWD_CubicSent

		protected override void CheckWD_CubicSent()
		{
			base.CheckWD_CubicSent();
			if (Parent.WD_CubicSent < 0m)
			{
				Parent.WD_CubicSentInfo.AddError(Res.GetString("dfe4b11d-61c0-40fe-ab30-72fbe1fa3e87", "Please do not enter negative quantity."));
			}
		}

		#endregion

		#region CheckWD_OH_Client

		protected override void CheckWD_OH_Client()
		{
			base.CheckWD_OH_Client();
			CheckWD_OH_Client_CreditCheck();
			CheckWD_OH_Client_ReservedStock();
		}

		#region CheckWD_OH_Client_CreditCheck

		protected virtual void CheckWD_OH_Client_CreditCheck()
		{
			var client = Parent.Client;
			if (!Globals.IsWeb && client != null && !Parent.WD_OH_ClientInfo.HasErrors())
			{
				var companyData = client.CompanyData;
				if (companyData != null && companyData.OB_AROnCreditHold)
				{
					Parent.WD_OH_ClientInfo.AddWarning(Res.GetString("28e7893e-e795-4853-abda-774f32ffcd97", "Credit on hold."));
				}

				if (client != null && client.OH_IsDebtor && !client.OH_Code.IsEmpty)
				{
					client.CreditChecker.ValidateIsCreditLimitExceeded(Parent.WD_OH_ClientInfo, LedgerTypes.AccountsReceivable);
				}
			}
		}

		#endregion

		#region CheckWD_OH_Client_ReservedStock

		void CheckWD_OH_Client_ReservedStock()
		{
			if (!Parent.IsFinalised && Parent.IsInDatabase &&
				!Parent.WD_OH_ClientInfo.HasErrors() && !Parent.WD_OH_ClientInfo.OriginalValue.Equals(Parent.WD_OH_Client) &&
				Parent.Lines.Any(l => l.ReservedPickLines.Count > 0))
			{
				Parent.WD_OH_ClientInfo.AddError(Res.GetString("83253974-5511-4048-93F3-3D6CA6D3283D", "Some of the lines are Cross Docked, you cannot change the Client."));
			}
		}

		#endregion

		#endregion

		#region CheckWD_WSH_SalesChannel

		protected override void CheckWD_WSH_SalesChannel()
		{
			base.CheckWD_WSH_SalesChannel();
			CheckWD_WSH_SalesChannel_IsAllowed();
		}

		void CheckWD_WSH_SalesChannel_IsAllowed()
		{
			if (!Parent.WD_WSH_SalesChannelInfo.Value.IsEmpty && !Parent.IsSalesChannelAllowed)
			{
				Parent.WD_WSH_SalesChannelInfo.AddError(Res.GetString("8539353c-1bdd-45cf-acf5-44ddaa0c4eca", "Sales Channels are only allowed on Warehouse Orders."));
			}
		}

		#endregion

		#region Custom Attribs Validation

		protected override void CheckWD_CustomAttrib1()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomAttrib1Info);
		}

		protected override void CheckWD_CustomAttrib2()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomAttrib2Info);
		}

		protected override void CheckWD_CustomAttrib3()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomAttrib3Info);
		}

		protected override void CheckWD_CustomAttrib4()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomAttrib4Info);
		}

		protected override void CheckWD_CustomAttrib5()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomAttrib5Info);
		}

		protected override void CheckWD_CustomDecimal1()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomDecimal1Info);
		}

		protected override void CheckWD_CustomDecimal2()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomDecimal2Info);
		}

		protected override void CheckWD_CustomDecimal3()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomDecimal3Info);
		}

		protected override void CheckWD_CustomDecimal4()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomDecimal4Info);
		}

		protected override void CheckWD_CustomDecimal5()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomDecimal5Info);
		}

		protected override void CheckWD_CustomDate1()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomDate1Info);
		}

		protected override void CheckWD_CustomDate2()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomDate2Info);
		}

		protected override void CheckWD_CustomFlag1()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomFlag1Info);
		}

		protected override void CheckWD_CustomFlag2()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomFlag2Info);
		}

		protected override void CheckWD_CustomFlag3()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomFlag3Info);
		}

		protected override void CheckWD_CustomFlag4()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomFlag4Info);
		}

		protected override void CheckWD_CustomFlag5()
		{
			CustomLabelPropertyValidation.Validate(new WhsDocket.CustomLabelsProvider(Parent), Parent.WD_CustomFlag5Info);
		}

		#endregion

		#region Implementation

		public string DuplicateReferenceErrorMsg()
		{
			return Res.GetString("aa4c673c-17bb-4dd1-a3c8-fe2529fdd0af", "This {0} Reference is already used by another {1} for this client. To save this {2} you must enter a reference that is not already used", TypeInMsg, TypeInMsg, TypeInMsg);
		}

		protected virtual ZString TypeInMsg => throw new NotImplementedException();

		CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}

		CustomLabelPropertyValidation customLabelPropertyValidation;

		public static string BondNotEnabledErrorMsg
		{
			get { return Res.GetString("01267fb2-1078-4219-b9c1-aeeafd4800fa", "You have selected a Bond Transaction Type for a Warehouse that is not enabled for Bond or Excise transactions. You must either enable the Warehouse for Bond or Excise transactions or select a Free Store transaction type. To enable a warehouse for Bond or Excise transactions, create an Area in the warehouse of type Bond or Excise respectively."); }
		}

		public static string InwardProcessingNotEnabledErrorMsg
		{
			get { return Res.GetString("4677d182-edc9-4f2e-8ae9-2c610e847946", "You have selected an Inward Processing Transaction Type for a Warehouse that is not enabled for Inward Processing transactions. You must either enable the Warehouse for Inward Processing transactions or select a different transaction type. To enable a warehouse for Inward Processing transactions, create an Area in the warehouse of type Inward Processing."); }
		}

		public static string FreeStoreNotEnabledErrorMsg
		{
			get { return Res.GetString("7fc117cd-5d6e-46c5-a479-adc6e278d5a1", "You have selected a Free Store Transaction Type for a Warehouse that is not enabled for Free Store or Excise transactions. You must either enable the Warehouse for Free Store or Excise transactions or select a Bond or Excise transaction type. To enable a warehouse for Free Store or Excise transactions, create an Area in the warehouse of type Free Store or Excise respectively."); }
		}

		public string BondNotSupportedCountryErrorMsg => Res.GetString("cb81e282-eb2a-4fe3-b964-0195002c0c68", "This Country/Region does not have Customs Integration.");

		public string BuildUNDGLimitExceededError(string limitsInError)
			=> Res.GetString("7673646e-ffc8-4b5c-983c-b446203b4e6e", "The following UNDG Limits will be exceeded by finalizing this job:\r\n{0}", limitsInError);

		public string BuildUNDGLimitExceededErrorWhenSaving(string limitsInError)
			=> Res.GetString("414f166b-485c-47f7-bb3f-86bcf4979175", "The following UNDG Limits will exceed 100% of warehouse capacity limit by saving this job:\r\n{0}", limitsInError);

		public string BuildUNDGWarningPercentageExceededErrorWhenSaving(decimal warningPercentage, string limitsInError)
			=> Res.GetString("3824badd-fcef-4141-8809-3c00123ab6cf", "The following UNDG Limits will exceed {0}% of warehouse capacity limit by saving this job:\r\n{1}", warningPercentage, limitsInError);

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(
				WhsDocketSchema.Constants.WD_WL_CrossDock,
				WhsDocketSchema.Constants.WD_WL_InboundDockDoor,
				WhsDocketSchema.Constants.WD_WLO_PlannedLoad,
				WhsDocketSchema.Constants.WD_WSH_SalesChannel,
				WhsDocketSchema.Constants.WD_F3_NKTotalPackType,
				WhsDocketSchema.Constants.WD_PL_NKCarrierServiceLevel,
				WhsDocketSchema.Constants.WD_P9_PackingTask);

		#endregion
	}
}
