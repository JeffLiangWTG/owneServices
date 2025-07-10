using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	[TopLevel(typeof(ERFF108))]
	public class ExchangeRateProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			foreach (ERFF108 f108 in messageBlocks)
			{
				if (f108.Indicator1 == "Q" || f108.Indicator1 == "D")
				{
					RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, f108.ISOCurrencyCode);
					if (currency == null)
					{
						currency = Factory.New<RefCurrency>();
						currency.RX_Code = f108.ISOCurrencyCode;
					}

					foreach (GlbCompany company in USCompanies)
					{
						UpdateExchangeRateForCompany(f108, currency, company.PK);
					}
				}
			}

			if (!IsReferenceRequestedByServiceTask)
			{
				var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
				GenerateHtmlEmailAndSendToOriginalOrGroup("", "Exchange Rate Request", "Exchange Rate Request", "Your query for currency exchange rates was successful. Please check your reference files for updated information.", false, branch, null);
			}
		}

		void UpdateExchangeRateForCompany(ERFF108 f108, RefCurrency currency, ZGuid companyPK)
		{
			ZQuery query = new ZQuery(RefExchangeRateSchema.RE_StartDate, f108.ExchangeRateDate);
			query.AddToFilter(RefExchangeRateSchema.RE_ExRateType, "CUS");
			query.AddToFilter(RefExchangeRateSchema.RE_GC, companyPK);
			query.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, currency.RX_Code);

			RefExchangeRate[] rates = Factory.Load<RefExchangeRate>(query);
			RefExchangeRate rate = rates.Length >= 1 ? rates[0] : Factory.New<RefExchangeRate>();
			rate.RE_GC = companyPK;
			rate.RE_RX_NKExCurrency = currency.RX_Code;
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = f108.ExchangeRateDate;
			rate.RE_ExpiryDate = f108.ExchangeRateDate;
			rate.RE_SellRate = f108.ExchangeRate;
		}

		protected List<GlbCompany> USCompanies
		{
			get
			{
				if (uSCompaniesCached == null)
				{
					uSCompaniesCached = new List<GlbCompany>();
					List<GlbCompany> allCompanies = new List<GlbCompany>();
					allCompanies.AddRange(Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Common.US.USCustomsJurisdiction.Countries)));

					foreach (GlbCompany company in allCompanies)
					{
						ZString entryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;

						if (!entryFilerCode.IsEmpty)
						{
							uSCompaniesCached.Add(company);
						}
					}
				}
				return uSCompaniesCached;
			}
		}
		List<GlbCompany> uSCompaniesCached;
	}
}
