using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public abstract class EntrySummary7501Print : NonPersistentBusinessObject, IObsoleteValidation, IDocumentDeliveredLogSupporter, IParentDocManagerSupport, IVisualizerNoteSupporter
	{
		protected EntrySummary7501Print(CusEntryHeader entry)
			: base(entry.Factory)
		{
			this.entry = entry;
			DetermineInvoiceAdjustmentsSectionPrintingFlag();
		}

		public readonly CusEntryHeader entry;
		public JobDeclaration Declaration
		{
			get { return entry.Declaration; }
		}

		#region PrintingFlags

		protected ZBool entryHasADDCVDLines;
		protected ZBool entryHasSecondaryTariffLines;
		protected ZBool entryHasAdditionalTariffLines;
		protected ZBool entryHasProRatedCalculation;
		protected ZBool entryHasAdValoremConversionCalculation;
		protected ZBool entryHasInvoiceAdjustmentLines;

		void DetermineInvoiceAdjustmentsSectionPrintingFlag()
		{
			foreach (var invHeader in entry.InvoiceHeaders)
			{
				var charges = invHeader.Charges.Cast<BaseJobComInvHeaderCharge>();
				charges = charges.Concat(invHeader.GroupCharges.Cast<BaseJobComInvHeaderCharge>());

				foreach (BaseJobComInvHeaderCharge charge in charges)
				{
					if (charge.IsAdjustmentChargeToInvoiceAmount())
					{
						entryHasInvoiceAdjustmentLines = true;
						break;
					}
				}

				if (!entryHasInvoiceAdjustmentLines)
				{
					foreach (JobComInvoiceLine invoiceLine in invHeader.JobComInvoiceLines)
					{
						if (invoiceLine.US_AMMVPerUnit > 0 || invoiceLine.US_AMMVPercentage > 0)
						{
							foreach (InvoiceLineApportionCharge charge in invoiceLine.ApportionedCharges)
							{
								if (charge.IsAMMV())
								{
									entryHasInvoiceAdjustmentLines = true;
									break;
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Document Properties

		public ZString Block24ReferenceNumber
		{
			get
			{
				var referenceNumber = Block24ReferenceNumberCore;
				if (SocialSecurityNumberValidator.IsValidSSN(referenceNumber) && !Declaration.PrintSocialSecurityNumberOnDocument)
				{
					referenceNumber = ZString.Empty;
				}

				return referenceNumber;
			}
		}

		protected abstract ZString Block24ReferenceNumberCore { get; }

		public abstract ZString BondType { get; }
		public abstract ZString FormattedEntryNumber { get; }
		public abstract ZDateTime EntrySummaryFiledDate { get; }
		public abstract ZString EntryType { get; }
		public abstract ZString EntryTypeCode { get; }
		public abstract ZString SchDEntry { get; }
		public abstract ZString USTransportMode { get; }
		public abstract ZString UniqueCountryOfOrigin { get; }
		public abstract ZString ManufacturerID { get; }
		public abstract ZString UniqueCountryOfExport { get; }
		public abstract ZString FirstBillITNO { get; }
		public abstract ZString MissingDoc1 { get; }
		public abstract ZString MissingDoc2 { get; }
		public abstract ZString UniquePortOfLading { get; }
		public abstract ZString SchDArrival { get; }
		public ZString EffectiveUltimateConsigneeCustomsRegNo
		{
			get
			{
				var ultimateConsigneeNo = EffectiveUltimateConsigneeCustomsRegNoCore;
				if (!Declaration.PrintSocialSecurityNumberOnDocument && (SocialSecurityNumberValidator.IsValidSSN(ultimateConsigneeNo) || ultimateConsigneeNo == USConstants.Same && ImporterOfRecordCustomsRegNo.IsEmpty))
				{
					ultimateConsigneeNo = ZString.Empty;
				}

				return ultimateConsigneeNo;
			}
		}

		protected abstract ZString EffectiveUltimateConsigneeCustomsRegNoCore { get; }

		public ZString ImporterOfRecordCustomsRegNo
		{
			get
			{
				var importerOfRecordNumber = ImporterOfRecordCustomsRegNoCore;
				if (SocialSecurityNumberValidator.IsValidSSN(importerOfRecordNumber) && !Declaration.PrintSocialSecurityNumberOnDocument)
				{
					importerOfRecordNumber = ZString.Empty;
				}

				return importerOfRecordNumber;
			}
		}

		protected abstract ZString ImporterOfRecordCustomsRegNoCore { get; }

		public abstract ZString EffectiveUltimateConsigneeCompanyName { get; }
		public abstract ZString EffectiveUltimateConsigneeAddressLine1 { get; }
		public abstract ZString EffectiveUltimateConsigneeAddressLine2 { get; }
		public abstract ZString EffectiveUltimateConsigneeCity { get; }
		public abstract ZString EffectiveUltimateConsigneeState { get; }
		public abstract ZString EffectiveUltimateConsigneePostCode { get; }
		public abstract ZString UltimateState { get; }
		protected abstract ZString WarehouseEntryNoForWarehouseEntryWithdrawal { get; }
		public abstract ZString ImporterCompanyName { get; }
		public abstract ZString ImporterAddressLine1 { get; }
		public abstract ZString ImporterAddressLine2 { get; }
		public abstract ZString ImporterCity { get; }
		public abstract ZString ImporterState { get; }
		public abstract ZString ImporterPostCode { get; }
		public abstract List<IFee> Fees { get; }
		public abstract ZString EntryFilerCode { get; }
		public abstract ZString EntryNo { get; }
		public abstract ZString SCACAndMBillNumber { get; }
		public abstract ZString SummaryBlockOverflow { get; }
		public abstract ZBool DecFinalWithdrawal { get; }
		public abstract ZBool TaxToBeDeferred { get; }
		public abstract ZBool DeferredTaxToBePaidByEFT { get; }
		public abstract ZDateTime FirstBillITDate { get; }
		public abstract ZString SuretyCode { get; }
		public abstract ZDecimal TotalAntidumpingDutyAmountPayable { get; }
		public abstract ZDecimal TotalCountervailingDutyPayable { get; }
		/// <summary>
		/// Payable Fees + AD/CVD Duty if applicable
		/// </summary>
		public abstract ZDecimal TotalOther { get; }
		public abstract ZDecimal TotalDutyAmt { get; }
		public abstract ZDecimal TotalEstTax { get; }
		public abstract ZDateTime DateOfFirstArrival { get; }
		public abstract ZDecimal TotalEnteredValue { get; }
		public abstract EntrySummary7501BillCollection EntryPrintBills { get; }
		public abstract EntrySummary7501LineCollection EntryPrintLines { get; }
		protected abstract ZDecimal TotalLineLevelHMFs { get; }
		protected abstract ZDecimal EntryHMF { get; }
		public abstract ZString USTeamNo { get; }

		public ZBool BulkLiquorTaxDeferred
		{
			get { return Declaration != null && Declaration.IsBulkLiquorTaxDeferred && DeferredTaxIndicator.IsEmpty; }
		}
		protected abstract ZString DeferredTaxIndicator { get; }

		public ZString ExpirationDate
		{
			get { return RefSysConfigLoader.GetStringValue(UniversalReferenceConstants.RefSysConfig.Codes.CBP7501ED); }
		}

		public ZString RevisionDate
		{
			get { return RefSysConfigLoader.GetStringValue(UniversalReferenceConstants.RefSysConfig.Codes.CBP7501RD); }
		}

		public ZString ApprovalNumber
		{
			get { return RefSysConfigLoader.GetStringValue(UniversalReferenceConstants.RefSysConfig.Codes.CBP7501AN); }
		}

		RefSysConfig.Loader RefSysConfigLoader
		{
			get { return refSysConfigLoader ?? (refSysConfigLoader = new RefSysConfig.Loader(entry.Factory)); }
		}
		RefSysConfig.Loader refSysConfigLoader;

		public virtual ZDateTime EstimatedEntryDate
		{
			get { return Declaration.IsExWarehouseEntryType ? Declaration.US_EstimatedEntryDate : Declaration.EntryDate; }
		}

		public virtual ZDecimal AdditionalTotalPrintingDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZString WarehouseEntryNo
		{
			get
			{
				return WarehouseEntryNoForReWarehouse.IsEmpty ? WarehouseEntryNoForWarehouseEntryWithdrawal : WarehouseEntryNoForReWarehouse;
			}
		}

		protected ZString WarehouseEntryNoForReWarehouse
		{
			get
			{
				var result = ZString.Empty;

				if (Declaration.US_EntryType == EntryTypeList.Codes.ReWarehouse && !Declaration.US_WHSEntryFilerCode.IsEmpty && !Declaration.US_WHSEntryNumber.IsEmpty)
				{
					result = Declaration.US_WHSEntryFilerCode + "-" + Declaration.US_WHSEntryNumber.SubstringSafe(0, 7) + "-" + Declaration.US_WHSEntryNumber.SubstringSafe(7, 1);
				}

				return result;
			}
		}

		public ZString ExportDate
		{
			get { return IsConsumptionFTZ ? string.Empty : (HasMultiExportDates ? USConstants.MultipleValueIndicator : ExportDateFromFirstLine.ToString("MM/dd/yyyy")); }
		}

		protected abstract bool HasMultiExportDates { get; }
		protected abstract ZDate ExportDateFromFirstLine { get; }

		public ZString LocationOfGoodsAndName
		{
			get { return LocationOfGoodsAndNameCore; }
		}

		protected virtual ZString LocationOfGoodsAndNameCore
		{
			get
			{
				return CachedValueHelper.GetValue(
					ref cachedLocationOfGoodsAndName,
					() =>
					{
						var result = ZString.Empty;
						var declaration = Declaration;

						if (!declaration.US_GeneralOrderNo.IsEmpty)
						{
							result = FormattedGeneralOrderNo(declaration.US_GeneralOrderNo);
						}
						else if (declaration.IsWarehouseEntryType)
						{
							var warehouseAddress = declaration.WarehouseAddress;
							if (warehouseAddress != null)
							{
								var warehouseFirmsCode = warehouseAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);

								if (!warehouseFirmsCode.IsEmpty)
								{
									var firms = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, warehouseFirmsCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);
									result = firms != null ? warehouseFirmsCode + "/" + firms.ZZD_Description
										: warehouseFirmsCode + "/" + (warehouseAddress.OA_CompanyNameOverride.IsEmpty ? warehouseAddress.Header.OH_FullNameTruncated : warehouseAddress.OA_CompanyNameOverrideTruncated);
								}
							}
						}
						return result;
					});
			}
		}

		CachedValue<ZString> cachedLocationOfGoodsAndName;

		protected ZString FormattedGeneralOrderNo(ZString unformattedGO)
		{
			ZString gONumber = unformattedGO.KeepNumericCharacters();
			ZString formattedGONumber = "G.O. " + gONumber;
			if (gONumber.Length == 13)
			{
				formattedGONumber = "GO-" + gONumber.SubstringSafe(0, 4) + "-" + gONumber.SubstringSafe(4, 4) + "-" + gONumber.SubstringSafe(8).Trim();
			}

			return formattedGONumber;
		}

		/// <summary>
		/// To cater for current declarations in the system prior to use of db field, allow for calculation of SummaryStatus for a period of time.
		/// </summary>
		public abstract ZString SummaryStatus { get; }

		internal List<IFee> fees;

		internal void AddFee(ZString code, ZDecimal amount)
		{
			IFee fee = new Fee();
			fee.Code = code;
			fee.Amount = amount;
			fees.Add(fee);
		}

		public ZDate EFTPaymentDate
		{
			get { return Declaration.US_DeferredTaxDueDate.Date; }
		}

		public ZString IORAlcoholLicence
		{
			get { return OrgHeaderWrapper.GetCustomsCode(entry.ImporterOfRecord, OrgCusCode.USACodeTypes.AlcoholImportLicence); }
		}

		public ZString DeclarationCheckBox1
		{
			get { return IsImporterDeclaration ? "X" : ""; }
		}

		public ZString DeclarationCheckBox2
		{
			get { return IsImporterDeclaration ? "" : "X"; }
		}

		public ZString ImportingCarrier
		{
			get
			{
				var carrierDetails = ZString.Empty;
				if (IsConsumptionFTZ)
				{
					carrierDetails = FTZNumber.SubstringSafe(0, 3) + " " + FTZNumber.SubstringSafe(3);
				}
				else if (IsSeaTransportMode)
				{
					carrierDetails = VesselName + (!CarrierCode.IsEmpty ? " (" + CarrierCode + ")" : "");
				}
				else if (IsAirOrHandCarry)
				{
					carrierDetails = CarrierCode;
				}
				else if (Declaration.IsFixedTransportInstallations)
				{
					carrierDetails = PipelineName + (!CarrierCode.IsEmpty ? " (" + CarrierCode + ")" : "");
				}
				return carrierDetails;
			}
		}

		protected abstract bool IsSeaTransportMode { get; }
		protected abstract bool IsAirOrHandCarry { get; }
		protected abstract ZString VesselName { get; }
		protected abstract ZString FTZNumber { get; }
		protected abstract ZString CarrierCode { get; }
		protected abstract ZString PipelineName { get; }

		internal ZBool IsConsumptionFTZ
		{
			get { return EntryType == EntryTypeList.Codes.ConsumptionFTZ; }
		}

		protected bool IsImporterDeclaration
		{
			get { return !Declaration.US_7501Agent; }
		}

		public ZString DeclarationCheckBox3
		{
			get { return IsPurchased == YesNoDefaultList.Codes.Yes ? "X" : ""; }
		}

		public ZString DeclarationCheckBox4
		{
			get { return IsPurchased == YesNoDefaultList.Codes.No ? "X" : ""; }
		}

		protected ZString IsPurchased
		{
			get { return Declaration.US_7501Purchased; }
		}

		public ZString DeclarantName
		{
			get { return ContactNameHelper.GetFormattedName(entry.DeclarantName, true); }
		}

		public Image BrokerSignatureImage
		{
			get { return entry.BrokerSignature3461; }
		}

		public ZBool ShouldPrintBrokerSignature
		{
			get { return entry.ShouldPrintBrokerSignature; }
		}

		public ZString DeclarantTitle
		{
			get
			{
				ZString result = ZString.Empty;

				if (entry.Declaration.IsAttorneyInFact)
				{
					result = "ATTY-IN-FACT";
				}
				else
				{
					var signatory = entry.Signatory;
					result = signatory != null ? signatory.GS_Title : ZString.Empty;
				}

				return result;
			}
		}

		Guid RegistryCompanyPK
		{
			get { return entry.RegistryCompanyPK; }
		}

		Guid RegistryBranchPK
		{
			get { return entry.RegistryBranchPK; }
		}

		public ZString BrokerFileNo
		{
			get
			{
				ZString result = Declaration.JE_DeclarationReference;

				if (USCustomsDataRegistry.Instance.DefaultOwnerRefOn7501.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
				{
					if (!Declaration.JE_OwnerRef.IsEmpty)
					{
						result = result + " / Ref: " + Declaration.JE_OwnerRef;
					}
				}

				return result;
			}
		}

		public ZString BoxNumber
		{
			get { return entry.IsRemoteLocationFiling || entry.BoxNumber.IsEmpty ? "" : "BOX " + entry.BoxNumber; }
		}

		public ZString ReconStatement
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsNAFTAReconIndicator)
				{
					result = "Flagged For Recon - FTA   ";
				}

				if (!ENSOtherIssueCode.IsEmpty) // && ENSOtherIssueCode != ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.NotApplicable))
				{
					result += "Flagged For Recon - "
						+ ENSOtherIssueCode + " - " + ENSOtherIssueCodeDescription;
				}

				return result;
			}
		}

		protected abstract bool IsNAFTAReconIndicator { get; }
		protected abstract ZString ENSOtherIssueCode { get; }
		protected abstract ZString ENSOtherIssueCodeDescription { get; }

		ZString SummaryFeeDesc(int summaryFee)
		{
			return Fees.Count > summaryFee ? GetSummaryFeeDescription(Fees[summaryFee].Code) : ZString.Empty;
		}

		ZDecimal SummaryFee(int summaryFee)
		{
			return Fees.Count > summaryFee ? Fees[summaryFee].Amount : ZDecimal.Zero;
		}

		public ZString SummaryFeeDesc1
		{
			get { return SummaryFeeDesc(0); }
		}

		public ZDecimal SummaryFee1
		{
			get { return SummaryFee(0); }
		}

		public ZString SummaryFeeDesc2
		{
			get { return SummaryFeeDesc(1); }
		}

		public ZDecimal SummaryFee2
		{
			get { return SummaryFee(1); }
		}

		public ZString SummaryFeeDesc3
		{
			get { return SummaryFeeDesc(2); }
		}

		public ZDecimal SummaryFee3
		{
			get { return SummaryFee(2); }
		}

		public ZString SummaryFeeDesc4
		{
			get { return SummaryFeeDesc(3); }
		}

		public ZDecimal SummaryFee4
		{
			get { return SummaryFee(3); }
		}

		ZString ExcessSummaryFeeDesc(int excessFee)
		{
			return EntryPrintExcessFees.Count > excessFee ? EntryPrintExcessFees[excessFee].SummaryFeeDesc : ZString.Empty;
		}

		ZDecimal ExcessSummaryFee(int excessFee)
		{
			return EntryPrintExcessFees.Count > excessFee ? EntryPrintExcessFees[excessFee].SummaryFee : ZDecimal.Zero;
		}

		public ZString SummaryFeeDesc5
		{
			get { return ExcessSummaryFeeDesc(0); }
		}

		public ZDecimal SummaryFee5
		{
			get { return ExcessSummaryFee(0); }
		}

		public ZString SummaryFeeDesc6
		{
			get { return ExcessSummaryFeeDesc(1); }
		}

		public ZDecimal SummaryFee6
		{
			get { return ExcessSummaryFee(1); }
		}

		public ZString SummaryFeeDesc7
		{
			get { return ExcessSummaryFeeDesc(2); }
		}

		public ZDecimal SummaryFee7
		{
			get { return ExcessSummaryFee(2); }
		}

		public ZString SummaryFeeDesc8
		{
			get { return ExcessSummaryFeeDesc(3); }
		}

		public ZDecimal SummaryFee8
		{
			get { return ExcessSummaryFee(3); }
		}

		public ZString SummaryFeeDesc9
		{
			get { return ExcessSummaryFeeDesc(4); }
		}

		public ZDecimal SummaryFee9
		{
			get { return ExcessSummaryFee(4); }
		}

		public ZString SummaryFeeDesc10
		{
			get { return ExcessSummaryFeeDesc(5); }
		}

		public ZDecimal SummaryFee10
		{
			get { return ExcessSummaryFee(5); }
		}

		public ZString SummaryFeeDesc11
		{
			get { return ExcessSummaryFeeDesc(6); }
		}

		public ZDecimal SummaryFee11
		{
			get { return ExcessSummaryFee(6); }
		}

		public ZString SummaryFeeDesc12
		{
			get { return ExcessSummaryFeeDesc(7); }
		}

		public ZDecimal SummaryFee12
		{
			get { return ExcessSummaryFee(7); }
		}

		public ZString SummaryFeeDesc13
		{
			get { return ExcessSummaryFeeDesc(8); }
		}

		public ZDecimal SummaryFee13
		{
			get { return ExcessSummaryFee(8); }
		}

		public ZString SummaryFeeDesc14
		{
			get { return ExcessSummaryFeeDesc(9); }
		}

		public ZDecimal SummaryFee14
		{
			get { return ExcessSummaryFee(9); }
		}

		public ZString SummaryFeeDesc15
		{
			get { return ExcessSummaryFeeDesc(10); }
		}

		public ZDecimal SummaryFee15
		{
			get { return ExcessSummaryFee(10); }
		}

		public ZString SummaryFeeDesc16
		{
			get { return ExcessSummaryFeeDesc(11); }
		}

		public ZDecimal SummaryFee16
		{
			get { return ExcessSummaryFee(11); }
		}

		public ZString SummaryFeeDesc17
		{
			get { return ExcessSummaryFeeDesc(12); }
		}

		public ZDecimal SummaryFee17
		{
			get { return ExcessSummaryFee(12); }
		}

		public ZString SummaryFeeDesc18
		{
			get { return ExcessSummaryFeeDesc(13); }
		}

		public ZDecimal SummaryFee18
		{
			get { return ExcessSummaryFee(13); }
		}

		public ZString SummaryFeeDesc19
		{
			get { return ExcessSummaryFeeDesc(14); }
		}

		public ZDecimal SummaryFee19
		{
			get { return ExcessSummaryFee(14); }
		}

		public ZString SummaryFeeDesc20
		{
			get { return ExcessSummaryFeeDesc(15); }
		}

		public ZDecimal SummaryFee20
		{
			get { return ExcessSummaryFee(15); }
		}

		public ZString SummaryFeeDesc21
		{
			get { return ExcessSummaryFeeDesc(16); }
		}

		public ZDecimal SummaryFee21
		{
			get { return ExcessSummaryFee(16); }
		}

		public ZString SummaryFeeDesc22
		{
			get { return ExcessSummaryFeeDesc(17); }
		}

		public ZDecimal SummaryFee22
		{
			get { return ExcessSummaryFee(17); }
		}

		public ZString SummaryFeeDesc23
		{
			get { return ExcessSummaryFeeDesc(18); }
		}

		public ZDecimal SummaryFee23
		{
			get { return ExcessSummaryFee(18); }
		}

		public ZString SummaryFeeDesc24
		{
			get { return ExcessSummaryFeeDesc(19); }
		}

		public ZDecimal SummaryFee24
		{
			get { return ExcessSummaryFee(19); }
		}

		public ZString BondedAmountWithdrawalStatement
		{
			get
			{
				ZString result = ZString.Empty;

				if (Declaration.IsExWarehouse)
				{
					result = "BONDED AMOUNT " + Declaration.US_QtyInWHBeforeWithdrawal.ToStringTrimZeros() + "    WITHDRAWAL " + Declaration.US_QtyBeingWithdrawn.ToStringTrimZeros() + "    BALANCE " + Declaration.US_QtyInWHAfterWithdrawal.ToStringTrimZeros();
				}

				return result;
			}
		}

		public ZBool EntryHasADDCVDLines
		{
			get
			{
				var result = entryHasADDCVDLines;
				if (IsTIBEntry)
				{
					foreach (JobComInvoiceLine invoiceLine in entry.InvoiceLines)
					{
						result = !invoiceLine.US_ADDCaseNo.IsEmpty || !invoiceLine.US_CVDCaseNo.IsEmpty;
						if (result)
						{
							break;
						}
					}
				}

				return result;
			}
		}

		public ZBool EntryHasSecondaryTariffLines
		{
			get { return entryHasSecondaryTariffLines; }
		}

		public ZBool EntryHasAdditionalTariffLines
		{
			get { return entryHasAdditionalTariffLines; }
		}

		public ZBool EntryHasProRatedCalculation
		{
			get { return entryHasProRatedCalculation; }
		}

		public ZBool EntryHasAdValoremConversionCalculation
		{
			get { return entryHasAdValoremConversionCalculation; }
		}

		public ZBool EntryHasInvoiceAdjustmentLines
		{
			get { return entryHasInvoiceAdjustmentLines; }
		}

		public ZDateTime DeclarationDate
		{
			get { return ZDateTime.Now; }
		}

		public ZDecimal TotalOtherFees
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (!EntryTypeList.IsNothingPayable(EntryType))
				{
					if (HMFDeMinimis || EntryTypeList.IsOnlyHMFPayable(EntryType))
					{
						result = TotalLineLevelHMFs;
					}
					else
					{
						result = TotalOther;
					}
				}
				return result;
			}
		}

		protected bool HMFDeMinimis
		{
			get
			{
				bool result = false;

				if (EntryHMF == 0m)
				{
					if (IsACE)
					{
						result = entry.Declaration.US_IsHMFApplicable == YesNoDefaultList.Codes.Yes;
					}
					else
					{
						var totalLineLevelHMFs = this.TotalLineLevelHMFs;
						result = totalLineLevelHMFs > 0 && totalLineLevelHMFs <= new FeeCalculationHelper(Factory, ((IDutyDataLineHeader)entry).DateForFeeCalculation).HMFThresholdAmount;
					}
				}

				return result;
			}
		}

		protected abstract bool IsACE { get; }

		public ZDecimal Block40Total
		{
			get
			{
				ZDecimal result = TotalDutyAmt + TotalEstTax + TotalOther - AdditionalTotalPrintingDuty;
				if (TaxToBeDeferred || DeferredTaxToBePaidByEFT || EntryTypeList.IsOnlyHMFPayable(EntryType) || EntryTypeList.IsNothingPayable(EntryType))
				{
					result = result - TotalEstTax;
				}

				return result;
			}
		}

		public ZDecimal TIBTotalDuty
		{
			get { return IsTIBEntry ? TIBCalcs.TIBDuty : ZDecimal.Zero; }
		}

		public ZDecimal TIBTotalADDCVD
		{
			get { return IsTIBEntry ? (ZDecimal)(TIBCalcs.TIBADDAmount + TIBCalcs.TIBCVDAmount) : ZDecimal.Zero; }
		}

		public ZDecimal TIBTotalCharges
		{
			get { return IsTIBEntry ? TIBCalcs.TIBCharges : ZDecimal.Zero; }
		}

		public ZString TIBTotalChargesDesc
		{
			get { return IsTIBEntry ? TIBCalcs.TIBChargesDesc : ZString.Empty; }
		}

		public ZDecimal TIBTotal
		{
			get { return TIBTotalDuty + TIBTotalADDCVD + TIBTotalCharges; }
		}

		public ZDecimal TIBBondChg
		{
			get
			{
				var result = ZDecimal.Zero;

				if (IsTIBEntry)
				{
					if (Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond || Declaration.US_BondCalcCode == SEBCalculationList.Codes.MAN)
					{
						result = Declaration.US_BondAmount;
					}
					else if (Declaration.US_BondType == BondTypeList.Codes.ContinuousBond && Declaration.US_TIBMotorVehicles == YesNoList.Codes.Yes && Declaration.US_TIBMVNonConforming)
					{
						result = new BondDetailsDefaulter().DetermineSEBBondAmount(entry, SEBCalculationList.Codes.MSC);
					}
					else
					{
						// calculate as if entry had been for a single transaction bond to determine bond amount for TIB
						new BondDetailsDefaulter().DetermineSEBBondAmount(entry, SEBCalculationList.Codes.TIB);
						result = TIBCalcs.TIBBondCharge.Round(2);
					}
				}

				return result;
			}
		}

		public ZString TIBStatement
		{
			get
			{
				var result = ZString.Empty;

				if (IsTIBEntry)
				{
					if (Declaration.US_TIBMotorVehicles == YesNoList.Codes.Yes)
					{
						result = USCustomsDataRegistry.Instance.TIBStatementForMV.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
					}
					else
					{
						result = USCustomsDataRegistry.Instance.TIBStatement.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
					}
				}

				return result;
			}
		}

		public ZString TIBPurpose
		{
			get { return IsTIBEntry ? Declaration.US_TIBPurpose : ZString.Empty; }
		}

		protected bool IsTIBEntry
		{
			get { return EntryType == EntryTypeList.Codes.TemporaryImportationBond; }
		}

		BondDetailsDefaulter.TIBCalculations TIBCalcs
		{
			get { return fTIBCalcs ?? (fTIBCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entry)); }
		}
		BondDetailsDefaulter.TIBCalculations fTIBCalcs;

		public ZString MessageToPrintOn7501
		{
			get
			{
				ZString result = ZString.Empty;

				if (entry.MessageToPrintOn7501.Count > 0)
				{
					result = entry.MessageToPrintOn7501[0].ST_NoteText;
				}

				return result;
			}
		}

		public ZString BranchName
		{
			get { return entry.BranchName; }
		}

		public ZString BranchAddress
		{
			get { return entry.BranchAddress; }
		}

		public ZString BranchPhone
		{
			get { return entry.BranchPhone; }
		}

		public CusEntryHeaderChargesCollection NonExcisableCharges
		{
			get
			{
				if (nonExcisableCharges == null)
				{
					CusEntryHeaderChargesCollection standardCharges = new CusEntryHeaderChargesCollection(entry);

					foreach (CusEntryHeaderCharges charge in entry.Charges)
					{
						if (!CusFeeCodeConstants.IsExciseTax(charge.C1_ChargeType) && charge.C1_ChargeAmount > 0)
						{
							standardCharges.Add(charge);
						}
					}

					nonExcisableCharges = standardCharges;
				}

				return nonExcisableCharges;
			}
		}
		CusEntryHeaderChargesCollection nonExcisableCharges;

		public ZBool WarehouseWithdrawal
		{
			get { return EntryTypeList.IsExWarehouseType(EntryType); }
		}

		#endregion

		#region IDocumentDeliveredLogSupporter Members

		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst
		{
			get { return entry.GetType(); }
		}

		ZGuid IDocumentDeliveredLogSupporter.Identifier
		{
			get { return entry.PK; }
		}

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get { return entry.PK; }
		}

		ZString IParentDocManagerSupport.ParentTableName
		{
			get { return CusEntryHeaderSchema.Constants.TableName; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsEntry)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => entry.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;

		#endregion

		#region Fees

		protected List<string> FeesPrintedOnBlock39
		{
			get
			{
				if (feesPrintedOnBlock39 == null)
				{
					feesPrintedOnBlock39 = new List<string>();

					ZString feeCode = GetSummaryFeeCode(1);
					IFee feeToCheck = Fees.Find(x => x.Code == feeCode);
					if (Fees.Contains(feeToCheck))
					{
						feesPrintedOnBlock39.Add(feeCode);
					}

					feeCode = GetSummaryFeeCode(2);
					feeToCheck = Fees.Find(x => x.Code == feeCode);
					if (Fees.Contains(feeToCheck))
					{
						feesPrintedOnBlock39.Add(feeCode);
					}

					feeCode = GetSummaryFeeCode(3);
					feeToCheck = Fees.Find(x => x.Code == feeCode);
					if (Fees.Contains(feeToCheck))
					{
						feesPrintedOnBlock39.Add(feeCode);
					}

					feeCode = GetSummaryFeeCode(4);
					feeToCheck = Fees.Find(x => x.Code == feeCode);
					if (Fees.Contains(feeToCheck))
					{
						feesPrintedOnBlock39.Add(feeCode);
					}
				}
				return feesPrintedOnBlock39;
			}
		}
		List<string> feesPrintedOnBlock39;

		public EntrySummary7501ExcessFeeCollection EntryPrintExcessFees
		{
			get
			{
				if (entryPrintExcessFees == null)
				{
					entryPrintExcessFees = new EntrySummary7501ExcessFeeCollection(Factory);

					foreach (IFee fee in Fees)
					{
						if (!FeesPrintedOnBlock39.Contains(fee.Code))
						{
							EntrySummary7501ExcessFee excessFee = new EntryHeader7501ExcessFee(entry, fee);
							entryPrintExcessFees.Add(excessFee);
						}
					}
				}

				return entryPrintExcessFees;
			}
		}
		EntrySummary7501ExcessFeeCollection entryPrintExcessFees;

		protected ZString GetSummaryFeeDescription(ZString code)
		{
			ZString result = ZString.Empty;

			switch (code)
			{
				case Core.Constants.USCustoms.FeeCodes.HMF:
					result = HMFDeMinimis ? "501 HMF (De Minimus)         $0.00" : code + " " + FeeCodeList.GetDescriptionFromCode(code);
					break;

				case "012":
					result = "012 AD";
					break;

				case "013":
					result = "013 CVD";
					break;

				default:
					if (!code.IsEmpty)
					{
						result = code + " " + FeeCodeList.GetDescriptionFromCode(code);
					}
					break;
			}
			return result;
		}

		protected ZString GetSummaryFeeCode(int summaryFeeNumber)
		{
			int localNumber = 1;
			ZString result = ZString.Empty;

			if (HMFDeMinimis && summaryFeeNumber == 1)
			{
				localNumber++;
				result = Core.Constants.USCustoms.FeeCodes.HMF;
			}

			if (localNumber <= summaryFeeNumber)
			{
				if (TotalAntidumpingDutyAmountPayable > 0)
				{
					localNumber++;
					result = "012";
				}
			}

			if (localNumber <= summaryFeeNumber)
			{
				if (TotalCountervailingDutyPayable > 0)
				{
					localNumber++;
					result = "013";
				}
			}

			if (localNumber <= summaryFeeNumber)
			{
				int collIndex = summaryFeeNumber - 1;

				if (Fees.Count > collIndex)
				{
					result = Fees[collIndex].Code;
				}
			}

			return result;
		}

		protected CodeDescriptionPairList FeeCodeList => CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory);

		#endregion
	}
}
