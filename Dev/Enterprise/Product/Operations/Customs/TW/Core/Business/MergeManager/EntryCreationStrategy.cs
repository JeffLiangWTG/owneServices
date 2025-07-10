using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, declaration == null ? ZString.Empty : declaration.JE_MessageType)
		{
		}

		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader entryHeader, BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(entryHeader, baseInvoiceLine);
			entryHeader.CH_CEI_Instruction = baseInvoiceLine.JI_CEI;
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var result = base.GetKeyForHeaderCore(baseInvoiceLine);
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			result.Add(invoiceLine.JI_CEI);
			return result;
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var result = new MergeKey();
			var invoiceLinePK = invoiceLine.PK;
			if (ShouldInvoiceLineBeSingleLineEntry(invoiceLine))
			{
				result.Add(invoiceLinePK);
			}
			else
			{
				SetDefaultMergeKey(invoiceLine, result);

				var mergeBy = Declaration?.JE_MergeBy ?? ZString.Empty;
				switch (mergeBy)
				{
					case OrgConstants.MergeInvoiceLines.NotMerge:
						result.Add(invoiceLinePK);
						break;
					case OrgConstants.MergeInvoiceLines.PartNumber:
						result.Add(invoiceLine.JI_PartNo);
						break;
				}

				if (mergeBy != MergeByCodeList.Codes.CondensedDeclaration)
				{
					result.Add(invoiceLine.JI_EnteredUnitPrice);
					result.Add(invoiceLine.JI_BrandName);
					result.Add(invoiceLine.JI_Model);
					result.Add(invoiceLine.JI_TariffAdditionalCode);
					result.Add(invoiceLine.JI_OA_ManufacturerAddress);
					result.Add(invoiceLine.JI_DeclarationGoodsDescription);
				}
				else if (!invoiceLine.JI_TariffAdditionalCode.IsEmpty)
				{
					result.Add(invoiceLinePK);
				}
			}
			return GetKeyForHeader(baseInvoiceLine) + result;
		}

		bool ShouldInvoiceLineBeSingleLineEntry(JobComInvoiceLine invoiceLine)
		{
			return ShouldInvoiceLineBeSingleLineEntryBase(invoiceLine) || invoiceLine.IsImport && ShouldInvoiceLineBeSingleLineEntryImport(invoiceLine);
		}

		bool ShouldInvoiceLineBeSingleLineEntryBase(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.PermitCusSupportingCollection.Cast<PermitCusSupporting>().Any(x => !x.CSI_ReferenceNumber.IsEmpty || !x.CSI_LineNo.IsEmpty)
		   || invoiceLine.AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().Any(x => !x.JG_ReferenceNumber.IsEmpty)
		   || !invoiceLine.IsCarRelatedDataEmpty
		   || !invoiceLine.CitesPermit.IsEmpty
		   || !invoiceLine.CertificateOfOriginNumber.IsEmpty
		   || !invoiceLine.CertificateOfOriginNumberItemNumber.IsEmpty
		   || !invoiceLine.HighTechLicense.IsEmpty
		   || !invoiceLine.JI_PreviousEntryNumber.IsEmpty
		   || !invoiceLine.JI_PreviousEntryLineNumber.IsEmpty
		   || !invoiceLine.JI_CustomsOwnerPartNo.IsEmpty
		   || !invoiceLine.JI_CustomsSupplierPartNo.IsEmpty
		   || !invoiceLine.JI_Compositions.IsEmpty
		   || !invoiceLine.PreviousPermitNo.IsEmpty
		   || !invoiceLine.JI_BondedGoodsCode.IsEmpty
		   || !invoiceLine.JI_Calc_EnvironmentalProtectionCode.IsEmpty
		   || invoiceLine.IsRAPOrROR
		   || invoiceLine.HasLinkedCMHeader
		   || !invoiceLine.TrademarkStorageDocsGuid.IsEmpty;
		}

		bool ShouldInvoiceLineBeSingleLineEntryImport(JobComInvoiceLine invoiceLine)
		{
			return !invoiceLine.JI_AntiDumpingDutyRate.IsEmpty
			  || !invoiceLine.JI_CountervailingDutyRate.IsEmpty
			  || !invoiceLine.JI_AdditionalDutyRate.IsEmpty
			  || !invoiceLine.JI_RetaliatoryDutyRate.IsEmpty;
		}

		void SetDefaultMergeKey(JobComInvoiceLine invoiceLine, MergeKey result)
		{
			result.Add(invoiceLine.JI_Tariff);
			result.Add(invoiceLine.JI_Procedure);
			if (invoiceLine.IsImport)
			{
				result.Add(invoiceLine.JI_CountryOfOrigin);
				result.Add(invoiceLine.JI_DtyPymntMthd);
				result.Add(invoiceLine.JI_VatPymntMthd);
				result.Add(invoiceLine.JI_TpfPymntMthd);
			}
			result.Add(GetAdditionalTariffHashCode(invoiceLine));
			result.Add(invoiceLine.JI_AlcoholPercentage);
			result.Add(invoiceLine.JI_PrimaryPreference);
			result.Add(invoiceLine.JI_CusValueConvRatio);
			result.Add(invoiceLine.JI_ConcessionOrder);
		}

		internal ZInt GetAdditionalTariffHashCode(JobComInvoiceLine invoiceLine)
		{
			var keys = invoiceLine.Taxes.Cast<JobComInvoiceLineTax>().
				Where(x => !x.JLT_Type.IsEmpty || !x.JLT_Tariff.IsEmpty).
				Select(x => x.JLT_Type + "|" + x.JLT_Tariff).Distinct().ToList();
			keys.Sort();
			var hashCode = keys.Count.GetHashCode();
			foreach (object obj in keys)
			{
				hashCode = hashCode ^ obj.GetHashCode();
			}
			return hashCode;
		}
	}
}
