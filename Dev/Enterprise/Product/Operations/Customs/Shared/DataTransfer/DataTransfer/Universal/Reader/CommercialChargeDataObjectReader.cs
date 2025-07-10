using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CommercialChargeDataObjectReader<T> : DataObjectReader<CommercialCharge, T>
		where T : CommonNonApportionedCharge
	{
		public static void FillCommercialInfo(IEnumerable<CommercialCharge> commercialInvoiceChargeCollection, ICommonNonApportionedChargeProvider<T> provider, IXmlImportLogger logger, UniversalObjectFactory factory,
			Func<CommercialCharge, IXmlImportLogger, ICommonNonApportionedChargeProvider<T>, UniversalObjectFactory, CommercialChargeDataObjectReader<T>> createNewCommercialChargeDataObjectReader = null)
		{
			if (commercialInvoiceChargeCollection != null)
			{
				foreach (var charge in commercialInvoiceChargeCollection)
				{
					if (!charge.IsApportionedCharge.GetValueOrDefault())
					{
						(createNewCommercialChargeDataObjectReader?.Invoke(charge, logger, provider, factory) ?? new CommercialChargeDataObjectReader<T>(charge, logger, provider, factory)).ReadIntoBusinessObject();
					}
				}
			}
		}

		public CommercialChargeDataObjectReader(CommercialCharge containerDataObject, IXmlImportLogger logger, ICommonNonApportionedChargeProvider<T> provider, UniversalObjectFactory factory)
			: base(containerDataObject, logger, factory)
		{
			this.provider = Argument.NotNull(provider, string.Format("ICommonNonApportionedChargeProvider<{0}> provider", typeof(T).Name));
			if (containerDataObject.IsApportionedCharge.GetValueOrDefault())
			{
				throw new NotSupportedException("CommercialCharge where the IsApportionedCharge is ZBool.True is not supported.");
			}
		}

		readonly ICommonNonApportionedChargeProvider<T> provider;

		protected override T GetExistingBusinessObject()
		{
			return provider.GetChargeWithZeroAmount(dataObject.ChargeType.GetCodeAsUpperCase()); // should match to empty one that were added by setting the Inco Term
		}

		protected override T GetNewBusinessObject()
		{
			return provider.CreateNew();
		}

		protected override void PopulateBusinessObject(T charge)
		{
			var chargeRow = GetColumnIndexer(charge);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_ChargeType, dataObject.ChargeType);
			SetPercentageOfLinePrice(chargeRow);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_RX_NKCurrency, dataObject.Currency);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_Amount, dataObject.Amount);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_DistributeBy, dataObject.DistributeBy);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_ExchangeRateType, dataObject.ExchangeRateType);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_FullOrPartialApportionment, dataObject.ApportionmentType);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_IsDutiable, dataObject.IsDutiable);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_IsGSTApplicable, dataObject.IsGSTApplicable);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_IsIncludedInITOT, dataObject.IsIncludedInITOT);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_IsNotIncludedInInvoice, dataObject.IsNotIncludedInInvoice);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_PrepaidCollect, dataObject.PrepaidCollect);
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_IsStatisticalValueApplicable, dataObject.IsStatisticalValueApplicable);
			if (dataObject.AgreedExchangeRate.HasValue && charge.IsJ7_ExchangeRateUserEnterable)
			{
				SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_ExchangeRate, dataObject.AgreedExchangeRate.Value);
			}
		}

		protected virtual void SetPercentageOfLinePrice(IColumnIndexer chargeRow)
		{
			SetValue(chargeRow, JobComInvHeaderChargeSchema.J7_Percentage, dataObject.PercentageOfLinePrice);
		}
	}
}
