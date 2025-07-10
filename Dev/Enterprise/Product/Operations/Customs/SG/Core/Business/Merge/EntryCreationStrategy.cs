namespace Enterprise.Customs.SG.V4.Business
{
	public class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, declaration.JE_MessageType)
		{
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			Customs.Business.MergeKey result = base.GetKeyForLine(baseInvoiceLine);

			JobComInvoiceLine invoiceLine = baseInvoiceLine as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				result.Add(invoiceLine.SG_CategoryCode);
				result.Add(invoiceLine.SG_DutyPercentageRate);
				result.Add(invoiceLine.SG_DutyUnitRate);
				result.Add(invoiceLine.SG_ExciseUnitRate);
				result.Add(invoiceLine.SG_ExcisePercentageRate);
				result.Add(invoiceLine.SG_OtherTaxUnitRate);
				result.Add(invoiceLine.SG_OtherTaxPercentageRate);
				result.Add(invoiceLine.SG_PercAlcohol);
				result.Add(invoiceLine.SG_UnitDutiableWGTVOLQTY);
				result.Add(invoiceLine.JI_BrandName);
				result.Add(invoiceLine.JI_CountryOfOrigin);
				result.Add(invoiceLine.SG_LotNo);
				result.Add(invoiceLine.JI_HazMatCodeQualifier);
				result.Add(invoiceLine.SG_ESNDPIndicator);
				result.Add(invoiceLine.SG_EndUseCode1);
				result.Add(invoiceLine.SG_EndUseCode2);
				result.Add(invoiceLine.SG_EndUseCode3);
				result.Add(invoiceLine.SG_EndUseDescription);
				result.Add(invoiceLine.JI_Description);
				result.Add(invoiceLine.JI_Tariff);
				result.Add(invoiceLine.JI_CustomsUnitQty);
				result.Add(invoiceLine.JI_Model);
				result.Add(invoiceLine.JI_InvoiceUQ);
				result.Add(invoiceLine.MarksAndNumbers);
				result.Add(invoiceLine.SG_OuterPackQuantityUnit);
				result.Add(invoiceLine.SG_InPackQuantityUnit);
				result.Add(invoiceLine.SG_InnerPackQuantityUnit);
				result.Add(invoiceLine.SG_InmostPackQuantityUnit);
				result.Add(invoiceLine.JI_PrimaryPreference);
				result.Add(invoiceLine.SG_PreviousLotNo);
				result.Add(invoiceLine.SG_TotalDutiableWGTVOLQTYUnit);
				result.Add(invoiceLine.SG_UnitDutiableWGTVOLQTYUnit);
				result.Add(invoiceLine.SG_ManufacturingCostStatementDate);
				result.Add(invoiceLine.SG_CertItemValue);
				result.Add(invoiceLine.SG_PercContent);
				result.Add(invoiceLine.SG_TextileQuotaQuantity);
				result.Add(invoiceLine.CertItemDescription);
				result.Add(invoiceLine.SG_StrategicGoodsProductCodeQuantityUnit);
				result.Add(invoiceLine.SG_CertItemQuantityUnit);
				result.Add(invoiceLine.SG_CertOriginCriterion1);
				result.Add(invoiceLine.SG_CertOriginCriterion2);
				result.Add(invoiceLine.SG_CertOriginCriterion3);
				result.Add(invoiceLine.SG_TextileCatCode);
				result.Add(invoiceLine.SG_TextileQuotaQuantityUnit);
				result.Add(invoiceLine.SG_InwardHAWB);
				result.Add(invoiceLine.SG_InwardMAWB);
				result.Add(invoiceLine.SG_IsStrategic);
				result.Add(invoiceLine.SG_OutwardHAWB);
				result.Add(invoiceLine.JI_OutwardMAWB);
				result.Add(invoiceLine.SG_TobaccoMultiplier);
				result.Add(invoiceLine.SG_TariffCommodityType);
				result.Add(invoiceLine.InvoiceHeader.PK);

				if (invoiceLine.Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT || invoiceLine.Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP)
				{
					result.Add(invoiceLine.UnitPrice);
					result.Add(invoiceLine.SG_LastSellingPrice);
				}

				foreach (CusLineTariffDetail productCode in invoiceLine.ProductCodes)
				{
					result.Add(productCode.PK);
				}

				foreach (CASCCode1 cascCode in invoiceLine.CASCCode1s)
				{
					result.Add(cascCode.PK);
				}

				foreach (CASCCode2 cascCode in invoiceLine.CASCCode2s)
				{
					result.Add(cascCode.PK);
				}

				foreach (CASCCode3 cascCode in invoiceLine.CASCCode3s)
				{
					result.Add(cascCode.PK);
				}
			}

			return result;
		}

		protected override Customs.Business.MergeKey GetKeyForHeaderCore(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			return new Customs.Business.MergeKey();
		}
	}
}
