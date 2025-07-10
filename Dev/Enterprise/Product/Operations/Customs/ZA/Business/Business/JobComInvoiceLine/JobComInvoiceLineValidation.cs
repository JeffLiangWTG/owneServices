using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using Inst = Enterprise.Customs.ZA.Business.MessageDataProviderInstruction;

namespace Enterprise.Customs.ZA.Business
{
	public partial class JobComInvoiceLineValidation : AutoZAJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected CusEntryInstruction EntryInstruction
		{
			get { return Parent.EntryInstruction; }
		}

		protected JobComInvoiceHeader InvoiceHeader
		{
			get { return Parent.InvoiceHeader; }
		}

		JobComInvoiceLine InvoiceLine => Parent;

		protected BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		protected JobComInvoiceLineLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		CodeDescriptionPairList fAdditionalUnitList;
		protected CodeDescriptionPairList AdditionalUnitList
		{
			get
			{
				if (fAdditionalUnitList == null)
				{
					fAdditionalUnitList = Parent.Lookups.AdditionalUnitCodeList;
				}
				return fAdditionalUnitList;
			}
		}

		MessageDataProviderKeyFactor MessageKeyFactor
		{
			get { return Parent.MessageKeyFactor; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJI_Colour();
			ValidateJI_EngineCapacity();
			ValidateJI_EngineNumber();
			ValidateJI_Make();
			ValidateJI_VehicleFormat();
			ValidateJI_VehicleType();
			ValidateJI_VIN();
			ValidateJI_YearOfManufacture();
			ValidateRefundRebateCode();
			ValidateRefundRebateValue();
		}

		public void ValidateRefundRebateCode()
		{
			ValidateCalculatedProperty(Parent.RefundRebateCodeInfo);
		}

		protected void CheckRefundRebateCode()
		{
			var firstTariffDetailForConcession = Parent.FirstTariffDetailForConcession;
			if (firstTariffDetailForConcession != null)
			{
				Parent.RefundRebateCodeInfo.AddAllNotificationsFrom(firstTariffDetailForConcession.BZ_TariffInfo);
			}
		}

		public void ValidateRefundRebateValue()
		{
			ValidateCalculatedProperty(Parent.RefundRebateValueInfo);
		}

		protected void CheckRefundRebateValue()
		{
			var firstTariffDetailForConcession = Parent.FirstTariffDetailForConcession;
			if (firstTariffDetailForConcession != null)
			{
				Parent.RefundRebateValueInfo.AddAllNotificationsFrom(firstTariffDetailForConcession.FormulaSpecificValueInfo);
			}
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();

			var targetInfo = Parent.JI_CustomsUnitQtyInfo;
			if (Parent.JI_CustomsUnitQty.IsEmpty)
			{
				if (Parent.CustomsUOMAutoPopulated && Parent.customsUnitQtyAutoSet)
				{
					targetInfo.AddMessageError(UoMIsRequired);
				}
			}
			else if (!Parent.JI_CustomsUnitQty.IsEmpty)
			{
				if (Parent.CustomsUOMAutoPopulated)
				{
					if (!Parent.customsUnitQtyAutoSet)
					{
						targetInfo.AddMessageError(UoMIsNotRequired);
					}
					if (!Parent.UniversalTariff?.UnitsOfMeasure.Any(x => x.ZZ8_UOM == Parent.JI_CustomsUnitQty) ?? false)
					{
						targetInfo.AddMessageError(UoMDoesNotMatchUoMRequired);
					}
				}
				ListValidation.MessageErrorIfInvalidCode(targetInfo, AdditionalUnitList, UoMNotInknownUnitsOfMeasureList);
			}
			new AdditionalUnitValidationProvider().ValidateDuplicateAdditionalUnit(targetInfo, Parent.JI_CustomsSecondUnitQtyInfo, Parent.JI_CustomsThirdQuantityInfo);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			var sourceValue = Parent.JI_CustomsQuantity;
			if (sourceValue != 1m && (Parent.UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.VIN) ?? false))
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("1C4CE9FD-E3DD-4CFA-BD89-4439EA7BDDBD", "Customs Qty can only be 1 for vehicles, only 1 vehicle allowed per line."));
			}
			if (ZArchitecture.Core.Utilities.Round(sourceValue, 2) == 0m)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("a1bef252-7070-402b-b7e6-4e83256c8642", "Customs Qty should be greater than zero. Please enter a Customs Qty directly or enter an invoice UQ that can be convertible to Customs UQ '{0}'.", Parent.JI_CustomsUnitQty));
			}
			if (sourceValue != Math.Floor(sourceValue) && (Parent.JI_CustomsUnitQty == QuantityCodeList.Codes.Number || Parent.JI_CustomsUnitQty == QuantityCodeList.Codes.Pairs))
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("A989DD41-80B1-47E8-802A-99AFD93BF622", "Only Integer values allowed for quantity"));
			}
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (!Parent.JI_CustomsSecondUnitQty.IsEmpty && Parent.JI_CustomsSecondQuantity.IsEmpty)
			{
				Parent.JI_CustomsSecondQuantityInfo.AddMessageError("Please enter a quantity in " + AdditionalUnitList.GetDescriptionFromCode(Parent.JI_CustomsSecondUnitQty));
			}
			else if (Parent.JI_CustomsSecondUnitQty.IsEmpty && !Parent.JI_CustomsSecondQuantity.IsEmpty)
			{
				Parent.JI_CustomsSecondQuantityInfo.AddMessageError(UnitOfQuantityIsRequired);
			}
			else if (!Parent.JI_CustomsSecondQuantity.IsEmpty && Parent.JI_CustomsSecondQuantity != Math.Floor(Parent.JI_CustomsSecondQuantity) && (Parent.JI_CustomsSecondUnitQty == QuantityCodeList.Codes.Number || Parent.JI_CustomsSecondUnitQty == QuantityCodeList.Codes.Pairs))
			{
				Parent.JI_CustomsSecondQuantityInfo.AddMessageError(Res.GetString("A989DD41-80B1-47E8-802A-99AFD93BF622", "Only Integer values allowed for quantity"));
			}
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();

			var targetInfo = Parent.JI_CustomsSecondUnitQtyInfo;
			if (Parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				if (Parent.CustomsUOMAutoPopulated && Parent.customsSecondUnitQtyAutoSet)
				{
					targetInfo.AddMessageError(UoMIsRequired);
				}
			}
			else if (!Parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				if (Parent.CustomsUOMAutoPopulated)
				{
					if (!Parent.customsSecondUnitQtyAutoSet)
					{
						targetInfo.AddMessageError(UoMIsNotRequired);
					}

					if (!Parent.DistinctAdditionalUOMsFromAllValidTariffs.Contains(Parent.JI_CustomsSecondUnitQty))
					{
						targetInfo.AddMessageError(UoMDoesNotMatchUoMRequired);
					}
				}
				ListValidation.MessageErrorIfInvalidCode(targetInfo, AdditionalUnitList, UoMNotInknownUnitsOfMeasureList);
			}
			new AdditionalUnitValidationProvider().ValidateDuplicateAdditionalUnit(targetInfo, Parent.JI_CustomsUnitQtyInfo, Parent.JI_CustomsThirdUnitQtyInfo);
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();
			if (!Parent.JI_CustomsThirdUnitQty.IsEmpty && Parent.JI_CustomsThirdQuantity.IsEmpty)
			{
				Parent.JI_CustomsThirdQuantityInfo.AddMessageError("Please enter a quantity in " + AdditionalUnitList.GetDescriptionFromCode(Parent.JI_CustomsThirdUnitQty));
			}
			else if (Parent.JI_CustomsThirdUnitQty.IsEmpty && !Parent.JI_CustomsThirdQuantity.IsEmpty)
			{
				Parent.JI_CustomsThirdQuantityInfo.AddMessageError(ResString.GetMultilingualString("AB78BF6A-588C-4CE0-864A-8DE039757F4D", "You have not entered a unit of quantity."));
			}
			else if (!Parent.JI_CustomsThirdQuantity.IsEmpty && Parent.JI_CustomsThirdQuantity != Math.Floor(Parent.JI_CustomsThirdQuantity) && (Parent.JI_CustomsThirdUnitQty == QuantityCodeList.Codes.Number || Parent.JI_CustomsThirdUnitQty == QuantityCodeList.Codes.Pairs))
			{
				Parent.JI_CustomsThirdQuantityInfo.AddMessageError(Res.GetString("A989DD41-80B1-47E8-802A-99AFD93BF622", "Only Integer values allowed for quantity"));
			}
		}

		protected override void CheckJI_CustomsThirdUnitQty()
		{
			base.CheckJI_CustomsThirdUnitQty();

			var targetInfo = Parent.JI_CustomsThirdUnitQtyInfo;
			if (Parent.JI_CustomsThirdUnitQty.IsEmpty)
			{
				if (Parent.CustomsUOMAutoPopulated && Parent.customsThirdUnitQtyAutoSet)
				{
					targetInfo.AddMessageError(UoMIsRequired);
				}
			}
			else if (!Parent.JI_CustomsThirdUnitQty.IsEmpty)
			{
				if (Parent.CustomsUOMAutoPopulated)
				{
					if (!Parent.customsThirdUnitQtyAutoSet)
					{
						targetInfo.AddMessageError(UoMIsNotRequired);
					}
					if (!Parent.DistinctAdditionalUOMsFromAllValidTariffs.Contains(Parent.JI_CustomsThirdUnitQty))
					{
						targetInfo.AddMessageError(UoMDoesNotMatchUoMRequired);
					}
				}
				ListValidation.MessageErrorIfInvalidCode(targetInfo, AdditionalUnitList, UoMNotInknownUnitsOfMeasureList);
			}
			new AdditionalUnitValidationProvider().ValidateDuplicateAdditionalUnit(targetInfo, Parent.JI_CustomsUnitQtyInfo, Parent.JI_CustomsSecondUnitQtyInfo);
		}

		protected override bool IsBondedWhsQuantityRequired => Inst.ShouldOutputWarehouseCountableQuantity(MessageKeyFactor);

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo);
			var countryOfOrigin = Parent.JI_CountryOfOrigin;
			if (!countryOfOrigin.IsEmpty)
			{
				if (countryOfOrigin.Equals(Core.Constants.CountryCodes.SouthAfrica))
				{
					var procedureCode = Parent.ProcedureCode;
					if (Declaration != null && Declaration.JE_MessageType == ZAJobMessageTypeList.Codes.Export)
					{
						if (procedureCode.Equals(UniversalReferenceConstants.ProcedureCodes._36)
							|| procedureCode.Equals(UniversalReferenceConstants.ProcedureCodes._38)
							|| procedureCode.Equals(UniversalReferenceConstants.ProcedureCodes._62)
							|| procedureCode.Equals(UniversalReferenceConstants.ProcedureCodes._65)
							|| procedureCode.Equals(UniversalReferenceConstants.ProcedureCodes._66)
							|| procedureCode.Equals(UniversalReferenceConstants.ProcedureCodes._83))
						{
							Parent.JI_CountryOfOriginInfo.AddMessageError(ResString.GetMultilingualString("3125E3EF-315B-4A75-8D2F-D48CE2AEC61C", "Goods Origin Cannot be South Africa (ZA) for selected Procedure Code."));
						}
					}
				}
				else
				{
					if (ValidationConstants.InvoiceLine.ProcedureCodesWhereCountryOfOriginMustBeZA.Contains(Parent.JI_ProcedureCode.ToString()))
					{
						Parent.JI_CountryOfOriginInfo.AddMessageError(ValidationConstants.InvoiceLine.CountryOfOriginMustBeZA);
					}
				}
			}
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			if (Parent.JI_LinePrice.IsEmpty && Parent.CustomsValueOverrideMoney == null)
			{
				Parent.JI_LinePriceInfo.AddMessageError(ResString.GetMultilingualString("56624C48-E830-4B6D-88A9-0E18678D8FAE", "You cannot send a message without any export/import value.\r\nPlease specify Price here or provide Customs Value Override if you want to achieve Actual Price Zero"));
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			var tariff = Parent.JI_Tariff;
			var targetInfo = Parent.JI_TariffInfo;
			if (!tariff.IsEmpty)
			{
				var cusTariff = Parent.UniversalTariff;
				if (cusTariff != null)
				{
					if (Parent.IsDiamondProcessingRequired)
					{
						targetInfo.AddWarning(ResString.GetMultilingualString("0B9F1179-B825-437A-B211-A6F5E4499733", "This tariff indicates that Diamond details may be required. Please consider entering the requisite information on the Diamond Processing Tab."));
					}

					if (Parent.DistinctAdditionalUOMsFromAllValidTariffs.Count() > 2)
					{
						targetInfo.AddMessageError(ValidationConstants.CusLineTariffDetail.UOMsExceed);
					}

					CheckTariffForREBPermit(tariff);
				}
			}

			Parent.Validation.ValidateJI_VIN();
			Parent.Validation.ValidateJI_CustomsQuantity();
		}

		protected override ZString UniversalTariffNotExistedError => ValidationConstants.InvoiceLine.Schedule1Part1TariffDoesNotExistsForDate(Parent.JI_Tariff, Parent.EffectiveAssessmentDate);

		protected override IEnumerable<IZZRateSelectionCriteria> RateSelectionCriteriaLists => new List<IZZRateSelectionCriteria>() { Parent.DutyRateSelectionCriteria, Parent.AdValoremExciseRateSelectionCriteria, Parent.ExciseRateSelectionCriteria, Parent.AntiDumpingRateSelectionCriteria, Parent.RebateRateSelectionCriteria };

		protected override void RunCountrySpecificRateValidation(ZPropertyInfo targetInfo, IZZRateSelectionCriteria criteria, IEnumerable<RateView> applicableRates)
		{
			if (Parent.IsStandardTradeAgreement && applicableRates.Any(x => x.PreferenceCode == UniversalReferenceConstants.PrimaryPreference.Standard && x.ZZ2_RateFormula == "0") && (Parent.CusProcedure?.IsIntoWarehouse() ?? false))
			{
				if (ZACustomsRegistry.Instance.DutyFreeGoodsIntoBondedWarehouse.Value)
				{
					targetInfo.AddError(ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
				}
				else
				{
					targetInfo.AddWarning(ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
				}
			}
		}

		void CheckTariffForREBPermit(ZString tariff)
		{
			var parent = Parent;
			var declaration = parent.Declaration;
			if (declaration != null)
			{
				var tariffs = from cusLineTariffDetail in parent.CusLineTariffDetails.Cast<CusLineTariffDetail>()
							  where cusLineTariffDetail.BZ_Type.StartsWith(UniversalReferenceConstants.RefCusTariffTypes.StartsWith3, StringComparison.Ordinal)
							  || cusLineTariffDetail.BZ_Type.StartsWith(UniversalReferenceConstants.RefCusTariffTypes.StartsWith4, StringComparison.Ordinal)
							  select cusLineTariffDetail.BZ_Tariff;
				if (tariffs.Any())
				{
					var query = new ZQuery();
					query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Permit);
					query.AddToFilter(CusPermitHeaderSchema.CPH_Type, PermitTypeList.Codes.REB);
					query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.SouthAfrica);
					query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, declaration.JE_OH_Importer);
					query.AddToFilter(CusPermitHeaderSchema.CPH_Number, tariffs);
					var matchingPermits = Factory.Load<CusPermitHeader>(query);
					if (matchingPermits.Length > 0 && !ValidationHelper.CheckTariffIsInRange(tariff, matchingPermits))
					{
						parent.JI_TariffInfo.AddMessageError(ResString.GetMultilingualString("6BF9A739-D5A8-451D-BD08-E178443E498D", "Tariff Code is not available for this Rebate Code in the REB Permit Rule."));
					}
				}
			}
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();

			var sourceValue = Parent.JI_PrimaryPreference;
			var targetInfo = Parent.JI_PrimaryPreferenceInfo;

			if (Parent.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_PrimaryPreferenceInfo);
				if (sourceValue.IsEmpty && !Parent.JI_ROOCert.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.NoROOTypeEnteredForCert);
				}
			}
			else if (Parent.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				RefCountry refCountry = Declaration?.Origin?.Country;
				if (refCountry != null)
				{
					var countryOfOrigin = Parent.CountryOfOrigin;
					var tradeAgreement = Parent.TradeAgreementCode;
					if ((tradeAgreement == UniversalReferenceConstants.TradeAgreement.EUTRADE || tradeAgreement == UniversalReferenceConstants.TradeAgreement.EFTA)
								&& ((countryOfOrigin != null && refCountry.Factory.IsMemberOfEU(countryOfOrigin.RN_Code)) || (countryOfOrigin?.IsInEFTA ?? false))
								&& !refCountry.Factory.IsMemberOfEU(refCountry.RN_Code))
					{
						targetInfo.AddWarning(TradeAgreementField);
					}
				}
			}
			Parent.Validation.ValidateJI_ROOCert();
		}

		public string TradeAgreementField = Res.GetString("3F09FC1F-E8CA-4BCE-9DC2-3E50A46DFF9B", "Country/Region of Export is not an EU country/region, and EU trade Agreement can only apply if the shipment complies with Article 12 of the EU trade Agreement.");

		protected override void CheckJI_ZZF_NKTaxType()
		{
			if (Parent.IsImport)
			{
				base.CheckJI_ZZF_NKTaxType();
				var parent = Parent;
				if (parent.JI_ZZF_NKTaxType.IsEmpty && !parent.JI_Tariff.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.JI_ZZF_NKTaxTypeInfo);
				}
			}
		}

		protected override bool ShouldWarnVAT => false;

		protected override void CheckJI_PartNo()
		{
			base.CheckJI_PartNo();
			var part = Parent.Part;
			if (part != null)
			{
				var importer = Parent.Importer;
				if (importer != null && part.RelatedOrganisations.FindByOrganisationAndRelationship(importer, OrgPartRelation.RelationshipTypes.Owner) == null)
				{
					var entryInstruction = Parent.EntryInstruction;
					if (entryInstruction != null && Parent.IsChangeOfOwnershipWarehousing && !Parent.IsBondedWarehousingDisabled && Parent.SupportsBondedWarehousing)
					{
						Parent.JI_PartNoInfo.AddMessageError(ValidationConstants.InvoiceLine.ProductMustBelongToOrganisationForChangeOfOwnership(importer.OH_Code));
					}
				}

				Parent.Validation.ValidateJI_PartAttrib1();
				Parent.Validation.ValidateJI_PartAttrib2();
				Parent.Validation.ValidateJI_PartAttrib3();
			}
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure(); // Includes list & empty validation
			Parent.Validation.ValidateJI_PreviousEntryLineNumber();

			var previousProcedures = Parent.EntryInstruction?.PreviousProcedures;
			if (previousProcedures != null
				&& previousProcedures.Count > 1
				&& previousProcedures.Any(previousProcedure => !ValidationConstants.InvoiceLine.MixablePreviousProcedureCodes.Contains(previousProcedure)))
			{
				Parent.JI_ProcedureInfo.AddMessageError(ValidationConstants.InvoiceLine.MixOfPreviousProceduresOnEntryInstruction);
			}

			if (Declaration != null && !Declaration.IsMergeByValidForPreviousProcedureCode)
			{
				Parent.JI_ProcedureInfo.AddMessageError(ValidationConstants.InvoiceLine.UnsupportedMergeByForNonZeroPreviousProcedureCode);
			}

			var entryInstruction = EntryInstruction;
			if (entryInstruction != null)
			{
				if (Inst.ShouldOutputFromWarehouse(MessageKeyFactor))
				{
					if (entryInstruction.Warehouse == null)
					{
						Parent.JI_ProcedureInfo.AddMessageError(ValidationConstants.InvoiceLine.FromWarehouseRequiredForPPC);
					}
				}

				if (entryInstruction.CEI_OH_Carrier.IsEmpty && Inst.RemoverTransporterCodeRequiredForPreviousWarehouseExport(MessageKeyFactor))
				{
					Parent.JI_ProcedureInfo.AddMessageError(ValidationConstants.InvoiceLine.RemoverRequiredForProcedureCode);
				}

				if (entryInstruction.JobDeclaration?.IsWHSUniversalXMLActive ?? false)
				{
					if (entryInstruction.IsIntoWarehouseWarehousing)
					{
						var cusProcedure = Parent.CusProcedure;
						if (cusProcedure == null || !cusProcedure.IsIntoWarehouse())
						{
							Parent.JI_ProcedureInfo.AddError(ValidationConstants.InvoiceLine.PreviousProcedureCodeIsNotAnIntoWarehouse);
						}
					}
					else if (entryInstruction.IsOutOfWarehouseWarehousing)
					{
						var cusProcedure = Parent.CusProcedure;
						if (cusProcedure == null || !cusProcedure.IsOutOfWarehouse())
						{
							Parent.JI_ProcedureInfo.AddError(ValidationConstants.InvoiceLine.PreviousProcedureCodeIsNotAnOutOfWarehouse);
						}
					}
				}
			}

			if (Parent.IsExcise && Parent.Schedule1Part2ATariff == null)
			{
				Parent.JI_ProcedureInfo.AddMessageError(ValidationConstants.InvoiceLine.ThisEntryIsUsingAnExciseCPCButNoExciseDataDeclared);
			}
			Parent.Validation.ValidateJI_NewOwnerPartNo();
			ValidateJI_PartNo();
		}

		protected override IMultilingualString ProcedureCodeInvalid => ValidationConstants.InvoiceLine.PreviousProcedureInvalid;

		protected override void CheckJI_CEI()
		{
			if (!Parent.JI_CEI.IsValid && (Parent.InvoiceHeader?.IsAttachedToPersistentDeclaration ?? false))
			{
				var targetInfo = Parent.JI_CEIInfo;
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
			}
			Parent.Validation.ValidateJI_PreviousEntryNumber();
			Parent.Validation.ValidateJI_CountryOfOrigin();
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);

			ValidationHelper.ValidateBlankLinesForEDIFACT(Parent.JI_DescriptionInfo, Parent.JI_Description);
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_WeightInfo);
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			var isimx = Declaration?.IsImportByExternalBroker ?? false;
			if (!isimx)
			{
				base.CheckJI_PreviousEntryNumber();

				var invoiceLine = Parent;
				if (invoiceLine != null)
				{
					ValidationHelper.ValidateMRNFormat(invoiceLine.JI_PreviousEntryNumberInfo);
					var factor = MessageKeyFactor;

					if (!factor.PPC.IsEmpty)
					{
						if (invoiceLine.JI_PreviousEntryNumber.IsEmpty)
						{
							if (Inst.ShouldOutputPreviousMRN(factor))
							{
								invoiceLine.JI_PreviousEntryNumberInfo.AddMessageError(ValidationConstants.InvoiceLine.PreviousMRNRequiredForPPC);
							}
						}
					}

					if (IsReExportToBLNSCPC(factor.CPC))
					{
						var invoiceLines = invoiceLine.Declaration?.InvoiceLines;
						if (invoiceLines != null)
						{
							var previousMRN = invoiceLine.JI_PreviousEntryNumber;
							if (invoiceLines.OfType<JobComInvoiceLine>().Any(x => x != invoiceLine && IsReExportToBLNSCPCAndIsDifferent(x.ProcedureCode, x.JI_PreviousEntryNumber, previousMRN)))
							{
								invoiceLine.JI_PreviousEntryNumberInfo.AddMessageError(ValidationConstants.InvoiceLine.PreviousMRNSForSameProcedureMustBeSame(factor.CPC));
							}
						}
					}
				}
			}
		}

		bool IsReExportToBLNSCPCAndIsDifferent(ZString otherCPC, ZString otherPreviousMRN, ZString previousMRN)
		{
			return IsReExportToBLNSCPC(otherCPC) && otherPreviousMRN != previousMRN;
		}

		public static bool IsReExportToBLNSCPC(ZString cpc)
		{
			return cpc.Equals(UniversalReferenceConstants.ProcedureCodes._66);
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			base.CheckJI_PreviousEntryLineNumber();

			var line = Parent;
			if (line != null)
			{
				var lineNumber = line.JI_PreviousEntryLineNumber;
				var targetInfo = line.JI_PreviousEntryLineNumberInfo;
				if (lineNumber < 0 || lineNumber > 9999)
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PreviousMRNLineNumberMinMaxValue);
				}

				var declaration = Declaration;
				if (declaration != null && declaration.IsImportByExternalBroker)
				{
					if (lineNumber == ZInt.Zero)
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PreviousMRNLineNumberRequiredForIMX);
					}
					else
					{
						var hasduplicatelines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.PK != line.PK && l.JI_CEI == line.JI_CEI && l.JI_PreviousEntryLineNumber == lineNumber);
						if (hasduplicatelines)
						{
							targetInfo.AddMessageError(ValidationConstants.InvoiceLine.DuplicateMRNLineNumberExistPerEntryInstruction);
						}
					}

					if (line.IsInDatabase && !targetInfo.HasErrors() && !targetInfo.OriginalValue.Equals(targetInfo.Value))
					{
						var entry = Parent.EntryInstruction?.EntryHeader;
						if (entry != null && entry.IsInDatabase && entry.HasWHSTransaction)
						{
							targetInfo.AddError(WarehouseTransactionExistsNeedsCancel);
						}
					}
				}
				else if (lineNumber == ZInt.Zero && Inst.ShouldOutputPreviousMRNLineNumber(MessageKeyFactor))
				{
					targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PreviousMRNLineNumberRequiredForPPC);
				}
			}
		}

		public static string WarehouseTransactionExistsNeedsCancel
		{
			get { return Res.GetString("078451E1-7066-49B9-91A3-0AEF67E1CFA7", "There is an Inventory transaction created against this invoice line.\r\nPlease cancel it before changing this value."); }
		}

		protected override void CheckJI_PartAttrib1()
		{
			base.CheckJI_PartAttrib1();
			CheckJI_PartAttrib(1, Parent.JI_PartAttrib1Info);
		}

		protected override void CheckJI_PartAttrib2()
		{
			base.CheckJI_PartAttrib2();
			CheckJI_PartAttrib(2, Parent.JI_PartAttrib2Info);
		}

		protected override void CheckJI_PartAttrib3()
		{
			base.CheckJI_PartAttrib3();
			CheckJI_PartAttrib(3, Parent.JI_PartAttrib3Info);
		}

		void CheckJI_PartAttrib(int partAttributeIndex, ZPropertyInfo propertyInfo)
		{
			if (Parent.VINPartAttributeIndex == partAttributeIndex)
			{
				var actualLength = Parent.JI_VIN.Length;
				var maxLength = propertyInfo.MaxLength;
				if (actualLength > maxLength)
				{
					propertyInfo.AddWarning(Res.GetString("2D985330-7C4E-4815-B182-CDCD66B6B062", "The length of VIN ({0}) exceeds the maximum allowed ({1}) for {2}. Value not updated.", actualLength, maxLength, propertyInfo.HumanReadableName));
				}
				else
				{
					if ((ZString)propertyInfo.Value != Parent.JI_VIN)
					{
						propertyInfo.AddMessageError(VINAndVINAttributeMustBeTheSame);
					}
				}
			}
		}

		internal static string VINAndVINAttributeMustBeTheSame => Res.GetString("3898a945-e85e-49a9-9446-2bff2513e559", "The VIN number & the product Attribute VIN number must be the same.");

		protected override void CheckJI_ValuationMarkup()
		{
			base.CheckJI_ValuationMarkup();
			var targetInfo = Parent.JI_ValuationMarkupInfo;
			var sourceValue = Parent.JI_ValuationMarkup;
			var invoiceHeader = Parent?.InvoiceHeader;
			if (!sourceValue.IsEmpty)
			{
				MandatoryValidation.CheckNotNegative(targetInfo);
				if (Parent.CustomsValueOverrideMoney != null)
				{
					targetInfo.AddWarning(Res.GetString("E552CCEA-EEAE-4E1D-AC81-39ECE5AFB334", "The Valuation Markup % won't take effect since you have entered an Overridden Customs Value"));
				}
				if (invoiceHeader?.JZ_VDN.IsEmpty ?? true)
				{
					targetInfo.AddMessageError(Res.GetString("4F84DDA9-CE0B-47C6-AB84-B13C87616E0B", "Please make sure the parent Invoice Header has a valid VDN number"));
				}
			}
			else if ((!invoiceHeader?.JZ_ValuationMarkup.IsEmpty ?? false) && Parent.CustomsValueOverrideMoney == null)
			{
				targetInfo.AddWarning(Res.GetString("E270A94D-A7BA-4C5B-9C3B-41260B812125", "You may need to provide a Valuation Markup % since the parent Invoice Header has captured a Valuation Markup % value"));
			}
		}

		protected override void CheckJI_NewOwnerPartNo()
		{
			base.CheckJI_NewOwnerPartNo();
			var invoiceLine = InvoiceLine;
			var entryInstruction = invoiceLine?.EntryInstruction;
			var owner = entryInstruction?.Owner;
			if (owner != null && !invoiceLine.IsBondedWarehousingDisabled && invoiceLine.SupportsBondedWarehousing)
			{
				var part = invoiceLine.NewOwnerProduct;
				if (part == null)
				{
					if (invoiceLine.HasBothOutOfAndIntoRegimeProcedure && (invoiceLine.IsChangeOfOwnershipWarehousing || invoiceLine.IsIntoWarehouseWarehousing))
					{
						Parent.JI_NewOwnerPartNoInfo.AddMessageError(ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
					}
				}

				if (!Parent.JI_NewOwnerPartNo.IsEmpty && invoiceLine.NewOwnerProductSyncManager.Enabled)
				{
					if (part == null)
					{
						if (invoiceLine.NewOwnerProductSyncManager.TotalNumberOfPartsCount == 1)
						{
							Parent.JI_NewOwnerPartNoInfo.AddWarning(ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
						}
						else if (invoiceLine.NewOwnerProductSyncManager.TotalNumberOfPartsCount > 1)
						{
							Parent.JI_NewOwnerPartNoInfo.AddWarning(ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
						}
						else
						{
							Parent.JI_NewOwnerPartNoInfo.AddWarning(JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
						}
					}
					else
					{
						if (invoiceLine.NewOwnerProductSyncManager.TotalMatchCount > 1)
						{
							Parent.JI_NewOwnerPartNoInfo.AddWarning(JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);
						}
					}
				}
			}
			ValidateJI_NewOwnerPartAttrib1();
			ValidateJI_NewOwnerPartAttrib2();
			ValidateJI_NewOwnerPartAttrib3();
		}

		#region Attributes

		protected override void CheckJI_NewOwnerPartAttrib1()
		{
			base.CheckJI_NewOwnerPartAttrib1();
			CheckPartAttribute(Parent.JI_NewOwnerPartAttrib1Info, 1);
		}

		protected override void CheckJI_NewOwnerPartAttrib2()
		{
			base.CheckJI_NewOwnerPartAttrib2();
			CheckPartAttribute(Parent.JI_NewOwnerPartAttrib2Info, 2);
		}

		protected override void CheckJI_NewOwnerPartAttrib3()
		{
			base.CheckJI_NewOwnerPartAttrib3();
			CheckPartAttribute(Parent.JI_NewOwnerPartAttrib3Info, 3);
		}

		void CheckPartAttribute(ZPropertyInfo info, int attribNumber)
		{
			var invoiceLine = this.InvoiceLine;
			var part = invoiceLine?.NewOwnerProduct;
			if (part != null)
			{
				var owner = invoiceLine.EntryInstruction?.Owner;
				if (owner != null)
				{
					PartAttributeValidation.CheckAttribute(owner, part, info, attribNumber);
				}
			}
		}

		#endregion

		public void ValidateJI_Colour()
		{
			ValidateCalculatedProperty(Parent.JI_ColourInfo);
		}

		protected void CheckJI_Colour()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JI_ColourInfo);
		}

		public void ValidateJI_EngineCapacity()
		{
			ValidateCalculatedProperty(Parent.JI_EngineCapacityInfo);
		}

		protected void CheckJI_EngineCapacity()
		{
		}

		public void ValidateJI_Make()
		{
			ValidateCalculatedProperty(Parent.JI_MakeInfo);
		}

		protected void CheckJI_Make()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JI_MakeInfo);
		}

		public void ValidateJI_VehicleFormat()
		{
			ValidateCalculatedProperty(Parent.JI_VehicleFormatInfo);
		}

		protected void CheckJI_VehicleFormat()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JI_VehicleFormatInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_VehicleFormatInfo);
		}

		public void ValidateJI_VehicleType()
		{
			ValidateCalculatedProperty(Parent.JI_VehicleTypeInfo);
		}

		protected void CheckJI_VehicleType()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JI_VehicleTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_VehicleTypeInfo);
		}

		public void ValidateJI_YearOfManufacture()
		{
			ValidateCalculatedProperty(Parent.JI_YearOfManufactureInfo);
		}

		protected virtual void CheckJI_YearOfManufacture()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JI_YearOfManufactureInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_YearOfManufactureInfo);
		}

		protected override void CheckJI_NewUsed()
		{
			base.CheckJI_NewUsed();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_NewUsedInfo);
		}

		public void ValidateJI_EngineNumber()
		{
			ValidateCalculatedProperty(Parent.JI_EngineNumberInfo);
		}

		protected void CheckJI_EngineNumber()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JI_EngineNumberInfo);
			if (InvoiceLine.IsImport && InvoiceLine.JI_EngineNumber.IsEmpty && !InvoiceLine.JI_VIN.IsEmpty)
			{
				Parent.JI_EngineNumberInfo.AddMessageError(EngineNumberAndVINAreRequiredInPair);
			}
		}

		public void ValidateJI_VIN()
		{
			ValidateCalculatedProperty(Parent.JI_VINInfo);
		}

		protected void CheckJI_VIN()
		{
			var targetInfo = Parent.JI_VINInfo;
			var sourceValue = Parent.JI_VIN;

			EnglishCharactersValidation.ErrorIfNotWesternEuropean(targetInfo);

			var hasTariffVINAttribute = InvoiceLine.UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.VIN) ?? false;
			if (sourceValue.IsEmpty)
			{
				if (InvoiceLine.IsImport && !InvoiceLine.JI_EngineNumber.IsEmpty)
				{
					targetInfo.AddMessageError(EngineNumberAndVINAreRequiredInPair);
				}
				if (hasTariffVINAttribute)
				{
					targetInfo.AddWarning(VINMayBeRequired(InvoiceLine.JI_Tariff));
				}
			}
			else
			{
				if (!hasTariffVINAttribute)
				{
					targetInfo.AddWarning(VINMayNotBeRequired(InvoiceLine.JI_Tariff));
				}
				HashSet<ZGuid> pksWithSameVIN = null;
				var foundVIN = InvoiceLine.Declaration?.VINLookup.TryGetValue(sourceValue, out pksWithSameVIN) ?? false;
				if (foundVIN && (pksWithSameVIN.Count > 1 || !pksWithSameVIN.Contains(InvoiceLine.PK)))
				{
					targetInfo.AddMessageError(DuplicatedVINNumber(sourceValue));
				}
				CheckDuplicatedVINNumberOnAnotherDeclaration(sourceValue, targetInfo);
			}

			Parent.Validation.ValidateJI_PartAttrib1();
			Parent.Validation.ValidateJI_PartAttrib2();
			Parent.Validation.ValidateJI_PartAttrib3();
		}

		void CheckDuplicatedVINNumberOnAnotherDeclaration(ZString vinNumber, ZPropertyInfo targetInfo)
		{
			var declaration = Declaration;
			if (!vinNumber.IsEmpty && declaration != null)
			{
				var duplicateVINWarningDictionary = declaration.DuplicateVINWarningDictionary;
				if (declaration.IsInPreSaveValidation && duplicateVINWarningDictionary != null)
				{
					if (duplicateVINWarningDictionary.ContainsKey(vinNumber))
					{
						targetInfo.AddWarning(duplicateVINWarningDictionary[vinNumber].ToStringWithNewLineBetweenAppends());
					}
				}
				else
				{
					var duplicateVINInvoiceLines = ValidationHelper.GetDuplicatedVINInvoiceLines(declaration, new ZString[] { vinNumber });
					if (duplicateVINInvoiceLines.Any())
					{
						var warningMessage = new ZStringBuilder();
						warningMessage.Append(DuplicatedVINNumberOnAnotherDeclaration);
						foreach (DynamicBusinessObject invoiceLineDetail in duplicateVINInvoiceLines)
						{
							warningMessage.Append(ValidationHelper.GetduplicateVINWarningMessage(invoiceLineDetail));
						}
						targetInfo.AddWarning(warningMessage.ToStringWithNewLineBetweenAppends());
					}
				}
			}
		}

		protected override void CheckJI_ROOCert()
		{
			if (!InvoiceLine.JI_PrimaryPreference.IsEmpty && InvoiceLine.JI_ROOCert.IsEmpty)
			{
				if (InvoiceLine.IsExport)
				{
					InvoiceLine.JI_ROOCertInfo.AddMessageError(ValidationConstants.InvoiceLine.NoROOCertEnteredForType);
				}
				if (InvoiceLine.AreRooDetailsIncomplete)
				{
					InvoiceLine.JI_ROOCertInfo.AddWarning(ValidationConstants.InvoiceLine.NoROOCertEnteredForTradeAgreement(InvoiceLine.TradeAgreementCode));
				}
			}
			InvoiceLine.Validation.ValidateJI_PrimaryPreference();
		}

		protected override void CheckJI_TargetEntryLineNumber()
		{
			base.CheckJI_TargetEntryLineNumber();
			var targetInfo = Parent.JI_TargetEntryLineNumberInfo;
			var sourceNumber = Parent.JI_TargetEntryLineNumber;
			if (!(InvoiceLine?.Declaration?.MergeManager?.RequiresMerge ?? true))
			{
				var linkedEntryLineNumber = InvoiceLine?.CusEntryLine?.CL_LineNumber ?? ZShort.Zero;
				if (!linkedEntryLineNumber.IsEmpty && !sourceNumber.IsEmpty && linkedEntryLineNumber != sourceNumber)
				{
					targetInfo.AddMessageError(Res.GetString("39FBAB08-65F6-407D-AD7D-688E59263E5C", "The linked entry line has a Line Number \"{0}\" while the Target Entry Line Number is \"{1}\".\r\nPlease make sure you put the right Target Entry Line Number.", linkedEntryLineNumber, sourceNumber));
				}
			}

			if (!sourceNumber.IsEmpty)
			{
				var entryInstruction = InvoiceLine.EntryInstruction;
				if (entryInstruction != null && !entryInstruction.CEI_DateForDuty.IsEmpty)
				{
					if (sourceNumber > 9999)
					{
						targetInfo.AddMessageError(ValidationConstants.Shared.EntryLineNumberExceedMax);
					}
				}
			}
		}

		protected override void CheckJI_CustomsValueOverride()
		{
			base.CheckJI_CustomsValueOverride();
			if (Parent.JI_CustomsValueOverride.IsEmpty
				&& (InvoiceLine?.CusLineTariffDetails.Cast<CusLineTariffDetail>().Any(x => x.UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.CostOfRepair) ?? false) ?? false))
			{
				Parent.JI_CustomsValueOverrideInfo.AddWarning(Res.GetString("171C9678-3359-44B4-B680-D0EBADAEC7F8", "You may need to manually enter the Customs Value Override to present the Cost of Repair"));
			}
			InvoiceLine?.Validation.ValidateJI_LinePrice();
			InvoiceLine?.Validation.ValidateJI_ValuationMarkup();
		}

		protected override void CheckJI_RX_NKCustomsValueCurrencyOverride()
		{
			base.CheckJI_RX_NKCustomsValueCurrencyOverride();
			var targetInfo = Parent.JI_RX_NKCustomsValueCurrencyOverrideInfo;
			if (!Parent.JI_CustomsValueOverride.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			InvoiceLine?.Validation.ValidateJI_LinePrice();
			InvoiceLine?.Validation.ValidateJI_ValuationMarkup();
		}

		protected override void CheckJI_PermitNumber()
		{
			var targetInfo = Parent.JI_PermitNumberInfo;
			var invoiceLine = InvoiceLine;
			var permitNumber = Parent.JI_PermitNumber;
			if (permitNumber.IsEmpty)
			{
				var isExport = Declaration?.IsExport ?? true;
				var tariff = invoiceLine.UniversalTariff;
				if (tariff != null)
				{
					CheckPermit(tariff, isExport ? TariffAttributes.ExportPermit : TariffAttributes.ImportPermit);
				}

				if (!isExport && (Parent.JI_NewUsed == GoodsTypeList.Codes.S || Parent.JI_NewUsed == GoodsTypeList.Codes.U))
				{
					targetInfo.AddWarning(ValidationConstants.InvoiceLine.PermitForSecondHandGoods);
				}
			}
			else
			{
				var permit = Parent.Lookups.Permits.FirstOrDefault(x => x.CPH_Number == permitNumber);
				if (permit == null)
				{
					var matchingPermits = new CusPermitHeader.Loader(Parent.Factory).LoadByNumber(Core.Constants.CountryCodes.SouthAfrica, permitNumber).OfType<CusPermitHeader>();
					if (!matchingPermits.Any())
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PermitNotFound(permitNumber));
					}
					else
					{
						var expectedPermitHolder = invoiceLine.IsImport ? invoiceLine.Importer_Effective : invoiceLine.Supplier_Effective;
						if (expectedPermitHolder != null)
						{
							matchingPermits = matchingPermits.Where(x => x.CPH_OH_PermitHolder == expectedPermitHolder.PK);
						}
						if (expectedPermitHolder == null || !matchingPermits.Any())
						{
							targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PermitHolderInvalid(permitNumber, expectedPermitHolder?.OH_Code ?? ZString.Empty));
						}
						else
						{
							var expectedPermitType = invoiceLine.IsImport ? PermitTypeList.Codes.IMP : PermitTypeList.Codes.EXP;
							matchingPermits = matchingPermits.Where(x => x.CPH_Type == expectedPermitType);
							if (!matchingPermits.Any())
							{
								targetInfo.AddMessageError(ValidationConstants.InvoiceLine.PermitTypeInvalid(permitNumber, expectedPermitType));
							}
							else
							{
								var expectedTariff = invoiceLine.JI_Tariff;
								if (!ValidationHelper.CheckTariffIsInRange(expectedTariff, matchingPermits))
								{
									targetInfo.AddMessageError(ValidationConstants.InvoiceLine.TariffNotInPermitRange(permitNumber, expectedTariff));
								}
							}
						}
					}
				}
				else
				{
					var expectedUOM = permit.CPH_UnitOfMeasure;
					if (expectedUOM != invoiceLine.JI_CustomsUnitQty && expectedUOM != invoiceLine.JI_CustomsSecondUnitQty && expectedUOM != invoiceLine.JI_CustomsThirdUnitQty)
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLine.UnitOfMeasureDoesNotMatchPermit(permitNumber, expectedUOM));
					}
				}
			}
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();

			if (JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled && Declaration.JE_MessageType != ZAJobMessageTypeList.Codes.ExBond)
			{
				var codeList = (ZZRefCusCodeListCombinedCollection)Lookups.InvoiceUQUNE20CodeList;
				codeList.Load();
				if (!codeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == Parent.JI_InvoiceUQ))
				{
					Parent.JI_InvoiceUQInfo.AddMessageError(JobComInvoiceLineValidation.UoMNotFoundOnUNCodeList20);
				}
			}
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			if (JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled)
			{
				if (Parent.JI_InvoiceQuantity == 0m)
				{
					Parent.JI_InvoiceQuantityInfo.AddMessageError(JobComInvoiceLineValidation.InvoiceQuantityRequired);
				}
			}
			else
			{
				base.CheckJI_InvoiceQuantity();
			}
		}

		void CheckPermit(TariffView tariff, ZString permitType)
		{
			var attr = tariff.GetAttribute(permitType);
			if (attr != null)
			{
				var attributeValue = attr.ZZ3_Value;
				var targetInfo = Parent.JI_PermitNumberInfo;
				if (attributeValue == UniversalReferenceConstants.TariffAttributes.Values.Mandatory)
				{
					MandatoryValidation.WarnIfNotEntered(targetInfo);
				}
				else if (attributeValue == UniversalReferenceConstants.TariffAttributes.Values.Optional)
				{
					targetInfo.AddWarning(ValidationConstants.InvoiceLine.PermitIsOptional);
				}
			}
		}

		#region Checking for DA63 Values

		protected override void CheckJI_ImportCustomsQty2()
		{
			base.CheckJI_ImportCustomsQty2();
			if (InvoiceLine.IsDA63)
			{
				if (!Parent.JI_ImportCustomsQty2UQ.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ImportCustomsQty2Info);
				}
			}
		}

		protected override void CheckJI_ImportCustomsQty2UQ()
		{
			base.CheckJI_ImportCustomsQty2UQ();
			if (InvoiceLine.IsDA63)
			{
				if (!Parent.JI_ImportCustomsQty2.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_ImportCustomsQty2UQInfo, InvoiceLine.Lookups.AdditionalUnitCodeList);
				}
				new AdditionalUnitValidationProvider().ValidateDuplicateAdditionalUnit(Parent.JI_ImportCustomsQty2UQInfo, Parent.JI_ImportCustomsQtyUQInfo, Parent.JI_ImportCustomsQty3UQInfo);
			}
		}

		protected override void CheckJI_ImportCustomsQty3()
		{
			base.CheckJI_ImportCustomsQty3();
			if (InvoiceLine.IsDA63)
			{
				if (!Parent.JI_ImportCustomsQty3UQ.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ImportCustomsQty3Info);
				}
			}
		}

		protected override void CheckJI_ImportCustomsQty3UQ()
		{
			base.CheckJI_ImportCustomsQty3UQ();
			if (InvoiceLine.IsDA63)
			{
				if (!Parent.JI_ImportCustomsQty3.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_ImportCustomsQty3UQInfo, InvoiceLine.Lookups.AdditionalUnitCodeList);
				}
				new AdditionalUnitValidationProvider().ValidateDuplicateAdditionalUnit(Parent.JI_ImportCustomsQty3UQInfo, Parent.JI_ImportCustomsQtyUQInfo, Parent.JI_ImportCustomsQty2UQInfo);
			}
		}

		protected override void CheckJI_ImportTariff()
		{
			base.CheckJI_ImportTariff();
			if (InvoiceLine.IsDA63)
			{
				var targetInfo = Parent.JI_ImportTariffInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

				var sourceValue = Parent.JI_ImportTariff;
				if (!sourceValue.IsEmpty)
				{
					var cusTariff = InvoiceLine.ImportTariff;
					if (cusTariff == null)
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLine.Schedule1Part1TariffDoesNotExists(sourceValue));
					}

					if (InvoiceLine.IsDA63WithOriginalEntry && sourceValue != InvoiceLine.TariffFromRelatedImportBOE)
					{
						targetInfo.AddWarning(Res.GetString("F8FB0A45-5402-46D0-ADA4-164A64CF7481", "Tariff on DA63 line is different to tariff on import entry."));
					}
				}
			}
		}

		protected override void CheckJI_ImportCustomsQty()
		{
			base.CheckJI_ImportCustomsQty();
			if (InvoiceLine.IsDA63WithOriginalEntry)
			{
				if (Parent.JI_ImportCustomsQty > InvoiceLine.CustomsQtyFromRelatedImportBOE)
				{
					Parent.JI_ImportCustomsQtyInfo.AddMessageError(DA63ValueShouldntBeExceedingOriginalValue);
				}
			}
		}

		protected override void CheckJI_ImportCustomsQtyUQ()
		{
			base.CheckJI_ImportCustomsQtyUQ();
			if (InvoiceLine.IsDA63WithOriginalEntry)
			{
				if (Parent.JI_ImportCustomsQtyUQ != InvoiceLine.CustomsQtyUQFromRelatedImportBOE)
				{
					Parent.JI_ImportCustomsQtyUQInfo.AddMessageError(Res.GetString("220526EF-09BC-4485-AF82-E9AC433690B3", "Customs Quantity Unit on DA63 line should be the same as the Unit from original Import Entry."));
				}
			}
		}

		protected override void CheckJI_ImportCustomsValue()
		{
			base.CheckJI_ImportCustomsValue();
			if (InvoiceLine.IsDA63WithOriginalEntry)
			{
				if (Parent.JI_ImportCustomsValue > InvoiceLine.CustomsValueFromRelatedImportBOE)
				{
					Parent.JI_ImportCustomsValueInfo.AddMessageError(DA63ValueShouldntBeExceedingOriginalValue);
				}
			}
		}

		protected override void CheckJI_ImportDutyPaid()
		{
			base.CheckJI_ImportDutyPaid();
			if (InvoiceLine.IsDA63WithOriginalEntry)
			{
				if (Parent.JI_ImportDutyPaid > InvoiceLine.CustomsDutyFromRelatedImportBOE)
				{
					Parent.JI_ImportDutyPaidInfo.AddMessageError(DA63ValueShouldntBeExceedingOriginalValue);
				}
			}
		}

		protected override void CheckJI_ImportVATPaid()
		{
			base.CheckJI_ImportVATPaid();
			if (InvoiceLine.IsDA63WithOriginalEntry)
			{
				if (Parent.JI_ImportVATPaid > InvoiceLine.VATFromRelatedImportBOE)
				{
					Parent.JI_ImportVATPaidInfo.AddMessageError(DA63ValueShouldntBeExceedingOriginalValue);
				}
			}
		}

		#endregion
		#region Notification Message
		internal static IMultilingualString UoMNotInknownUnitsOfMeasureList
		{
			get { return ResString.GetMultilingualString("AB381731-6E8B-4997-9C91-10A2C2D21E37", "This UoM is not on the current list of known Units of Measure as published by Customs"); }
		}

		internal static string UoMIsNotRequired
		{
			get { return ResString.GetMultilingualString("2A744381-6245-4BCD-AB94-0A8F235C15B5", "The current CW1 Tariff Book indicates that this UoM is not required by the selected Tariff and/or Additional Tariff"); }
		}

		internal static string UoMIsRequired
		{
			get { return ResString.GetMultilingualString("9CC8A092-44C8-4199-89F9-2498D7F254C4", "The current CW1 Tariff Book indicates that a UoM is required by the selected Tariff and/or Additional Tariff"); }
		}

		internal static string UoMDoesNotMatchUoMRequired
		{
			get { return ResString.GetMultilingualString("56E252B0-1DDF-42D4-A9B8-A9D7CA9C8A79", "This UoM does not match the UoM required by the current CW1 Tariff Book"); }
		}

		internal static string UoMNotFoundOnUNCodeList20 => ResString.GetMultilingualString("540E0B33-5231-4938-89FA-FA1A73039753", "UoM not found on UN Code List 20");
		internal static string InvoiceQuantityRequired => ResString.GetMultilingualString("6D47EFC9-DE46-4E78-A6F5-7C40D60855E7", "Invoice Quantity is required and cannot be zero");

		internal static string EngineNumberAndVINAreRequiredInPair
		{
			get { return ResString.GetMultilingualString("84681F93-ABF4-419D-B9AF-9571AAC773DE", "Both 'Engine Number' and 'VIN Number' are required."); }
		}

		internal static string VINMayBeRequired(ZString tariffCode) => ResString.GetMultilingualString("F69EAFDB-0F3B-451C-92C8-C2CBACF45B05", "VIN Number may be required for the selected tariff:{0}", tariffCode);

		internal static string VINMayNotBeRequired(ZString tariffCode) => ResString.GetMultilingualString("92791853-0F56-4FCD-9CAA-ACA4C1BEDE6A", "VIN Number may not be required for the selected tariff:{0}", tariffCode);

		internal static string DuplicatedVINNumber(ZString vinNumber) => ResString.GetMultilingualString("ECB21A41-A296-4A7F-89AF-5C343321D6C3", "VIN Number should be unique, but \"{0}\" has been used on another Invoice Line, please double check the the correctness of this VIN Number.", vinNumber);

		internal static string DA63ValueShouldntBeExceedingOriginalValue => Res.GetString("54B05555-63A8-48C9-ABE1-D80B8E9B7C80", "Value on Export entry should not exceed value on Import Entry");

		internal static string DuplicatedVINNumberOnAnotherDeclaration => Res.GetString("f389f3ee-5a0b-44b6-b693-9e4342d75281", "The VIN Number has been used on another declaration:");

		#endregion
	}
}
