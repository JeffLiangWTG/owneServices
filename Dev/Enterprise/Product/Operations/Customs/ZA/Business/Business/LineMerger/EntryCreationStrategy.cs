using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, declaration == null ? ZString.Empty : declaration.JE_MessageType)
		{
		}

		#region Overrides

		public override bool LineIsValidForMerge(BaseJobComInvoiceLine invoiceLine)
		{
			return invoiceLine.JI_CEI.IsValid && !BusinessObject.IsNullOrDeleted(invoiceLine.EntryInstruction);
		}

		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader entryHeader, BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(entryHeader, baseInvoiceLine);
			entryHeader.CH_CEI_Instruction = baseInvoiceLine.JI_CEI;
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine baseInvoiceLine)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			MergeKey result = new MergeKey();

			var invoiceHeader = invoiceLine.InvoiceHeader;
			if (invoiceHeader != null)
			{
				var isExport = Declaration?.IsExport ?? false;
				if (!isExport)
				{
					result.Add(invoiceHeader.JZ_VDN);
					result.Add(invoiceHeader.JZ_RelatedIndicator);
					result.Add(invoiceHeader.JZ_ValuationCode);
				}
			}
			result.Add(invoiceLine.JI_CEI);
			return result;
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var result = base.GetKeyForLine(invoiceLine);
			result.Add(invoiceLine.JI_CEI);
			result.Add(invoiceLine.JI_Procedure);
			result.Add(invoiceLine.JI_PreviousEntryNumber);
			result.Add(invoiceLine.JI_PreviousEntryLineNumber);
			result.Add(invoiceLine.JI_CountryOfOrigin);
			result.Add(invoiceLine.JI_PrimaryPreference);
			if (GetMergeBy() != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff)
			{
				result.Add(invoiceLine.JI_Description);
			}
			result.Add(invoiceLine.JI_BondedWhsUnitQty);
			result.Add(invoiceLine.JI_NewUsed);
			result.Add(invoiceLine.JI_ZZF_NKTaxType);
			result.Add(invoiceLine.JI_ROOCert);
			result.Add(invoiceLine.JI_TargetEntryLineNumber);
			result.Add(invoiceLine.JI_DiamondBeneficiaryLicense);
			result.Add(invoiceLine.JI_DiamondDealerLicense);
			result.Add(invoiceLine.JI_TemporaryExportExemption);
			result.Add(invoiceLine.JI_DiamondProducerRegistration);
			result.Add(invoiceLine.JI_DiamondProducerExemption);
			result.Add(invoiceLine.JI_ElectionsExemptionsLevy);
			result.Add(invoiceLine.JI_KimberleyCertificate);
			result.Add(invoiceLine.JI_TemporaryBuyersPermit);
			result.Add(invoiceLine.JI_PermitNumber);
			result.Add(invoiceLine.JI_AdvancePaymentNo);
			result.Add(invoiceLine.JI_ConversionFactor);
			if (ShouldInvoiceLineBeSingleEntryLine(invoiceLine))
			{
				result.Add(invoiceLine.PK);
			}

			return result;
		}

		static bool ShouldInvoiceLineBeSingleEntryLine(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.Charges.Count != 0
				|| invoiceLine.CusLineTariffDetails.Count != 0
				|| !invoiceLine.JI_ValuationMarkup.IsEmpty
				|| invoiceLine.JI_DiamondLevyValue != 0
				|| !invoiceLine.JI_VIN.IsEmpty
				|| !invoiceLine.JI_EngineNumber.IsEmpty
				|| !invoiceLine.JI_Make.IsEmpty
				|| !invoiceLine.JI_Model.IsEmpty
				|| !invoiceLine.JI_VehicleFormat.IsEmpty
				|| !invoiceLine.JI_VehicleType.IsEmpty
				|| !invoiceLine.JI_Colour.IsEmpty
				|| !invoiceLine.JI_YearOfManufacture.IsEmpty
				|| invoiceLine.CustomsValueOverrideMoney != null
				|| invoiceLine.JI_CustomsValue != invoiceLine.JI_Calc_ActualPrice;
		}

		#endregion

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
			{
				if (entry.CH_MessageType == CH_MessageTypeToNewEntryHeader)
				{
					yield return entry;
				}
			}
		}

		protected override bool IsEntryHeaderValidToBeReused(Customs.Business.CusEntryHeader entry, BaseJobComInvoiceLine invoiceLine)
		{
			return base.IsEntryHeaderValidToBeReused(entry, invoiceLine) && !ChangedCEIOnAnEntryHeaderHasResponses(entry as CusEntryHeader, invoiceLine);
		}

		protected override bool IsEntryLineValidToBeReused(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
		{
			return base.IsEntryLineValidToBeReused(entryLine, invoiceLine) && !ChangedCEIOnAnEntryHeaderHasResponses(entryLine.Header as CusEntryHeader, invoiceLine);
		}

		bool ChangedCEIOnAnEntryHeaderHasResponses(CusEntryHeader entry, BaseJobComInvoiceLine invoiceLine)
		{
			return entry == null || (entry.CH_CEI_Instruction != invoiceLine.JI_CEI && entry.HasResponses);
		}
	}
}
