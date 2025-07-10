using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ENSMergeStrategy : ImportEntryCreationStrategy
	{
		public ENSMergeStrategy(JobDeclaration declaration)
			: base(declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary)
		{
		}

		protected override bool IsActiveCore
		{
			get
			{
				var declaration = (JobDeclaration)Declaration;
				return declaration.IsFormalImport && declaration.US_EnableENS;
			}
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
		{
			MergeKey result = base.GetKeyForLine(invoiceLine);
			JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;

			new ENSLineKeyGenerator().AddKey(result, line);

			return result;
		}

		protected override IComparer<BaseJobComInvoiceLine> GetInvoiceLineComparerForMerge(ReadOnlyBusinessObjectFactory cleanFactory)
		{
			return new InvoiceLineParentChildComparer();
		}
	}

	class ENSLineKeyGenerator : ILineKeyGenerator
	{
		public void AddKey(MergeKey result, JobComInvoiceLine line)
		{
			JobComInvoiceHeader invoice = line.InvoiceHeader;

			if (invoice != null)
			{
				result.Add(invoice.PK);
			}

			if (line != null &&
				line.Declaration != null &&
				line.Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMerge &&
				line.Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription)
			{
				ZGuid additionalTariffDetailsKey = ImportEntryCreationStrategy.ShouldNotMergeThisLineWithOtherLines(line) ? line.PK : ZGuid.Empty;

				result.Add(line.JI_Tariff);
				result.Add(line.US_SupTariff);
				result.Add(line.US_SupAdditionalTariff1);
				result.Add(line.US_SupAdditionalTariff2);
				result.Add(line.US_SupAdditionalTariff3);
				result.Add(line.US_SupAdditionalTariff4);
				result.Add(line.US_SupAdditionalTariff5);
				result.Add(line.US_UC_NKCountryOfOrigin);
				result.Add(line.US_ZoneStatus);
				result.Add(line.US_PrivilegedStatusDate);
				result.Add(line.US_IsNAFTANet);
				result.Add(line.US_PIRPRulingNo);
				result.Add(line.US_SPI);
				result.Add(line.US_UC_NKCountryOfExport);
				result.Add(line.US_DateOfExport);
				result.Add(line.US_SecondarySPI);
				result.Add(line.US_DateOfExportFromCountryOfOrigin);
				result.Add(line.US_VisaNo);
				result.Add(line.US_TextileCategoryNo);
				result.Add(line.US_VisaUQ);
				result.Add(line.US_AgricultureLicNo);
				result.Add(line.US_CottonCertificateNo);
				result.Add(line.US_CottonFeeExempt);
				result.Add(line.US_SWPMIndicator);
				result.Add(line.US_CAExportCertificate);
				result.Add(line.US_WoolLicenceNo);
				result.Add(line.US_CBTPACertificateNo);
				result.Add(line.US_MiscPermitNo);
				result.Add(line.US_CVDCaseNo);
				result.Add(line.US_CVDDepositRateIndicator);
				result.Add(line.US_ADDCaseNo);
				result.Add(line.US_ADDDepositRateIndicator);
				result.Add(line.ManufacturerFallBackToSupplierNumber);
				result.Add(line.US_IsBondedCVD);
				result.Add(line.US_IsBondedADD);
				result.Add(line.US_FDAIndicator);
				result.Add(line.US_FCCIndicator);
				result.Add(line.US_DOTIndicator);
				result.Add(line.US_SelectedRateType);
				result.Add(line.US_DestinationState);
				result.Add(line.InvoiceHeader != null ? line.InvoiceHeader.US_TransactionsRelated : ZString.Empty);
				result.Add(additionalTariffDetailsKey);
				result.Add(line.US_OverrideDuty);
				result.Add(line.US_OverrideSupDuty);
				result.Add(line.US_OverrideSupAdditionalTariff1Duty);
				result.Add(line.US_OverrideSupAdditionalTariff2Duty);
				result.Add(line.US_OverrideSupAdditionalTariff3Duty);
				result.Add(line.US_OverrideSupAdditionalTariff4Duty);
				result.Add(line.US_OverrideSupAdditionalTariff5Duty);
				result.Add(line.US_TaxApply);
				result.Add(line.US_TaxCode);
				result.Add(line.US_TaxRate);
				result.Add(((IFeeCalculationDataProvider)line).TaxComputationCode);
				result.Add(((IFeeCalculationDataProvider)line).OverriddenTaxRateUQ);
				result.Add(line.US_TaxRateT);
				result.Add(line.US_TransactionsRelated);
				result.Add(line.US_SchDLoading);
				result.Add(line.US_ADD_Cert);

				result.Add(line.US_ExclusionNumber);
				result.Add(line.US_ProductExclusion);

				result.Add(GetOverridenOrRateTypeFeesInOrder(line));

				result.Add(line.US_DisclaimSanctions);

				if (invoice.JobDeclaration.IsACE)
				{
					result.Add(line.US_ADCVDStat);
					result.Add(line.US_ADDDecID);
					result.Add(line.US_AMSDisclaimProgram);
					result.Add(line.US_AMSDisclaimReason);
					result.Add(line.US_AMSInd);
					result.Add(line.US_NOPInd);
					result.Add(line.US_NOPDisclaimReason);
					result.Add(line.US_APHISInd);
					result.Add(line.US_FDADisclaimReason);
					result.Add(line.US_FSISDisclaimReason);
					result.Add(line.US_FSISInd);
					result.Add(line.US_LaceyDisclaimReason);
					result.Add(line.US_LaceyIndicator);
					result.Add(line.US_NHTDisclaimReason);
					result.Add(line.US_NHTSAIndicator);
					result.Add(line.US_NMFS370Ind);
					result.Add(line.US_NMFS370DisclaimReason);
					result.Add(line.US_NMFSAMRInd);
					result.Add(line.US_NMFSAMRDisclaimReason);
					result.Add(line.US_NMFSHMSInd);
					result.Add(line.US_NMFSHMSDisclaimReason);
					result.Add(line.US_NMFSSIMPInd);
					result.Add(line.JI_OA_ExporterAddress);
					result.Add(line.JI_OA_Seller);
					result.Add(line.JI_OA_ManufacturerAddress);
					result.Add(line.US_ODSDisclaimReason);
					result.Add(line.US_ODSInd);
					result.Add(line.JI_OA_SoldToPartyAddress);
					result.Add(line.JI_OA_ConsigneeAddress);
					result.Add(line.US_PSTDisclaimReason);
					result.Add(line.US_PSTIndicator);
					result.Add(line.US_SetInd);
					result.Add(line.US_OMCInd);
					result.Add(line.US_OMCDisclaimReason);
					result.Add(line.US_TSCACertification);
					result.Add(line.US_TSCADisclaimReason);
					result.Add(line.US_TSCAInd);
					result.Add(line.US_TTBDisclaimReason);
					result.Add(line.US_TTBInd);
					result.Add(line.US_VNEDisclaimReason);
					result.Add(line.US_VNEInd);
					result.Add(line.US_FWSDisclaimReason);
					result.Add(line.US_FWSInd);
					result.Add(line.US_ATFInd);
					result.Add(line.JI_OA_ShipToPartyAddress);
					result.Add(line.US_CPSCInd);
					result.Add(line.US_CPSCDisclaimReason);
					result.Add(line.US_DEAInd);
					result.Add(line.US_DEADisclaimReason);
					result.Add(line.US_HFCInd);
					result.Add(line.US_HFCDisclaimReason);
					if (line.IsCBMAProductClaim)
					{
						result.Add(line.US_FlavorContentCreditInd);
						if (line.IsCBMA23Effective)
						{
							result.Add(ZString.Empty);
							result.Add(ZString.Empty);
							result.Add(line.US_TTBRateDesignationCode);
						}
						else
						{
							result.Add(line.US_ControlledGroupName);
							result.Add(line.US_TaxRateS);
							result.Add(ZString.Empty);
						}
					}
					else
					{
						result.Add(ZBool.False);
						result.Add(ZString.Empty);
						result.Add(ZString.Empty);
						result.Add(ZString.Empty);
					}

					result.Add(GetCensusWarningOverridesInOrder(line));
					result.Add(GetLicenceAndPermitsInOrder(line));
					result.Add(line.US_Prim_NA);
					result.Add(line.US_RN_NKPrimCtry);
					result.Add(line.US_Sec_NA);
					result.Add(line.US_RN_NKSecCtry);
					result.Add(line.US_RN_NKCastCtry);
					result.Add(line.US_RN_NKCertOrigin);
					result.Add(line.US_RN_NKMeltCtry);
				}
			}
		}

		ZString GetOverridenOrRateTypeFeesInOrder(JobComInvoiceLine invoiceLine)
		{
			List<FeeCusCodeData> fees = new List<FeeCusCodeData>();
			fees.AddRange(new TypedEnumerable<FeeCusCodeData>(invoiceLine.FeeCusCodes));

			fees.Sort(new System.Comparison<FeeCusCodeData>((FeeCusCodeData fee1, FeeCusCodeData fee2) => fee1.CY_Code.CompareTo(fee2.CY_Code)));

			ZStringBuilder result = new ZStringBuilder();

			foreach (FeeCusCodeData feeData in fees)
			{
				if (feeData.CY_IsOverridden || !feeData.CY_SelectedRateType.IsEmpty)
				{
					result.Append(feeData.CY_Code + feeData.CY_IsOverridden + feeData.CY_SelectedRateType);
				}
			}

			return result.ToString();
		}

		ZString GetLicenceAndPermitsInOrder(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.LicenceAndPermits.GetStringRepresentationOfElementsInOrder();
		}

		ZString GetCensusWarningOverridesInOrder(JobComInvoiceLine invoiceLine)
		{
			var result = new ZStringBuilder();

			var sortedList = new List<CensusWarningOverride>(invoiceLine.CensusWarningOverrides.ToArray<CensusWarningOverride>());
			sortedList.Sort(new CensusWarningOverride.Comparer());

			foreach (CensusWarningOverride cwo in sortedList)
			{
				result.Append(cwo.CY_Code + ":" + cwo.CY_Data);
			}

			return result.ToString();
		}
	}
}
