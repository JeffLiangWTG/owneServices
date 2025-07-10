using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry
{
	public class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		protected const int ChildLinePriceLimit = 24;

		public EntryCreationStrategy(LineMerger lineMerger)
			: base(lineMerger.Declaration)
		{
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var parentLine = invoiceLine.JI_LinePrice <= ChildLinePriceLimit ? invoiceLine.ParentLine : null;
			if (parentLine != null)
			{
				return GetKeyForLine(parentLine);
			}

			var declaration = invoiceLine.Declaration;
			bool isExport = declaration?.IsExport ?? false;
			var originCountry = invoiceLine.JI_RN_NKEffectiveCountryOfOrigin;

			invoiceLine.PermitCodes.Sort(CodeDataPair.Schema.ZO_Code, System.ComponentModel.ListSortDirection.Ascending);
			invoiceLine.ProhibitedCodes.Sort(CodeDataPair.Schema.ZO_Code, System.ComponentModel.ListSortDirection.Ascending);
			invoiceLine.OtherInfos.Sort(CodeDataPair.Schema.ZO_Code, System.ComponentModel.ListSortDirection.Ascending);

			ZString otherInfo = invoiceLine.PermitCodes.ToString() + invoiceLine.ProhibitedCodes.ToString() + invoiceLine.OtherInfos.ToString();

			var invoiceCurrencyCode = ZString.Empty;
			var supplierCode = ZString.Empty;
			var exchangeRateIndicator = ZString.Empty;
			var relationshipIndicator = ZString.Empty;
			var invoiceGstNumber = ZString.Empty;
			var invoiceGstPrepaid = ZString.Empty;
			var invoice = invoiceLine.InvoiceHeader;
			if (invoice != null)
			{
				invoiceCurrencyCode = invoice.JZ_RX_NKInvoice_Currency;
				supplierCode = invoice.Supplier?.OH_Code ?? ZString.Empty;
				exchangeRateIndicator = invoice.JZ_ExchangeRateIndicator;
				relationshipIndicator = invoice.JZ_RelationshipIndicator;
				invoiceGstNumber = invoice.JZ_SupplierGSTNumber;
				invoiceGstPrepaid = invoice.JZ_IsGSTPrePaid;
			}

			var result = base.GetKeyForLine(baseInvoiceLine);
			var mergeBy = declaration?.JE_MergeBy ?? ZString.Empty;
			if (mergeBy != OrgConstants.MergeInvoiceLines.NotMerge)
			{
				if (isExport)
				{
					result.Add(invoiceCurrencyCode);
					result.Add(originCountry);
					result.Add(otherInfo);
					result.Add(exchangeRateIndicator);
				}
				else
				{
					var concessionCode = invoiceLine.JI_ConcessionCode;
					var exportCountry = invoiceLine.JI_RN_NKEffectiveCountryOfExport;
					var preference = invoiceLine.JI_EffectiveQualifiesForPreferentialDuty;
					var prefGroup = invoiceLine.JI_EffectivePreferentialCountryGroup;
					var partsOfClassification = invoiceLine.JI_PartsOfClassification;
					var geneticallyModified = invoiceLine.JI_GeneticallyModified;
					var isZeroRatedDuty = invoiceLine.EffectiveIsZeroRatedDuty;
					var isZeroRatedExcise = invoiceLine.EffectiveIsZeroRatedExcise;
					var isZeroRatedLevies = invoiceLine.EffectiveIsZeroRatedLevies;
					var isZeroRatedGST = invoiceLine.EffectiveIsZeroRatedGST;
					result.Add(concessionCode);
					result.Add(exportCountry);
					result.Add(originCountry);
					result.Add(supplierCode);
					result.Add(relationshipIndicator);
					result.Add(invoiceCurrencyCode);
					result.Add(preference);
					result.Add(prefGroup);
					result.Add(otherInfo);
					result.Add(partsOfClassification);
					result.Add(isZeroRatedDuty);
					result.Add(isZeroRatedExcise);
					result.Add(isZeroRatedGST);
					result.Add(isZeroRatedLevies);
					result.Add(invoiceGstNumber);
					result.Add(invoiceGstPrepaid);
					if (declaration?.IsTSWDeclaration ?? false)
					{
						var brandName = invoiceLine.JI_BrandName;
						var commonName = invoiceLine.JI_CommonName;
						var registeredName = invoiceLine.JI_RegisteredName;
						var tradeName = invoiceLine.JI_TradeName;
						var usedGoods = invoiceLine.JI_UsedGoods;
						result.Add(brandName);
						result.Add(commonName);
						result.Add(registeredName);
						result.Add(tradeName);
						result.Add(usedGoods);
						result.Add(geneticallyModified);
					}
				}
			}

			return result;
		}

		protected override Customs.Business.MergeKey GetKeyForHeaderCore(Customs.Business.BaseJobComInvoiceLine invoiceLine) => new Customs.Business.MergeKey(0);

		protected override Customs.Business.CusEntryHeader GetExistingEntryHeader(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			var declaration = (JobDeclaration)invoiceLine.Declaration;
			return declaration.CusEntryHeader;
		}

		bool HeaderIsValidForMerge(Customs.Business.CusEntryHeader entryHeader1)
		{
			var entryHeader = entryHeader1 as CusEntryHeader;
			var declaration = entryHeader != null ? entryHeader.Declaration : null;
			return entryHeader != null && declaration != null && entryHeader.IsActive && (entryHeader.PK == declaration.CusEntryHeader.PK);
		}

		protected override bool IsEntryHeaderValidToBeReused(Customs.Business.CusEntryHeader entry, Customs.Business.BaseJobComInvoiceLine invoiceLine) => base.IsEntryHeaderValidToBeReused(entry, invoiceLine) && HeaderIsValidForMerge(entry);
	}
}
