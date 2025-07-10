using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobComInvCharge : Customs.Common.JobComInvCharge
		, Enterprise.Integration.Freight.IJobComInvHeaderCharge
		, ICurrencyConverterDataProviderWithFixedExRates
	{
		public JobComInvCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void RegisterParentType(List<Type> parentTypes)
		{
			base.RegisterParentType(parentTypes);

			parentTypes.Add(typeof(Order));
			parentTypes.Add(typeof(OrderLine));
		}

		protected override bool GetIncludedInITOTReadOnly()
		{
			return base.GetIncludedInITOTReadOnly() || (IsIncludedInInvoiceAmountFixed && J7_IsNotIncludedInInvoice);
		}

		protected override ZBool IsJ7_ExchangeRateUserEnterableCore
		{
			get { return !GetIsJ7_ExchangeRateUserEnterableReadOnly(); }
			set
			{
				IsJ7_ExchangeRateUserEnterableInfo.RefreshBinding();
			}
		}

		public override ZString J7_RX_NKCurrency
		{
			get { return base.J7_RX_NKCurrency; }
			set
			{
				base.J7_RX_NKCurrency = value;

				if (!J7_IsApportionedCharge && J7_ExchangeRate == 0m)
				{
					Order order = Parent as Order;

					if (order != null && J7_RX_NKCurrency == order.JD_RX_NKOrderCurrency)
					{
						J7_ExchangeRate = order.JD_EstimatedExchangeRate;
					}
				}
			}
		}

		#region ICurrencyConverterDataProvider Members

		ZArchitecture.Core.ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return Enterprise.ZArchitecture.Core.ExchangeRateType.All; }
		}

		#endregion
	}
}
